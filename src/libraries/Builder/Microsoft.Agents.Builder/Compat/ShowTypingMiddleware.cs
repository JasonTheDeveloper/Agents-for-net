﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Authentication;
using Microsoft.Agents.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Agents.Builder.Compat
{
    /// <summary>
    /// When added, this middleware will send typing activities back to the user when a Message activity
    /// is received to let them know that the Agent has received the message and is working on the response.
    /// You can specify a delay in milliseconds before the first typing activity is sent and then a frequency,
    /// also in milliseconds which determines how often another typing activity is sent. Typing activities
    /// will continue to be sent until your Agent sends another message back to the user.
    /// </summary>
    public class ShowTypingMiddleware : IMiddleware
    {
        private readonly TimeSpan _delay;
        private readonly TimeSpan _period;
        private readonly ConcurrentDictionary<string, (Task, CancellationTokenSource)> _tasks = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTypingMiddleware"/> class.
        /// </summary>
        /// <param name="delay">Initial delay before sending first typing indicator. Defaults to 500ms.</param>
        /// <param name="period">Rate at which additional typing indicators will be sent. Defaults to every 2000ms.</param>
        public ShowTypingMiddleware(int delay = 500, int period = 2000)
        {
            if (delay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be greater than or equal to zero");
            }

            if (period <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(period), "Repeat period must be greater than zero");
            }

            _delay = TimeSpan.FromMilliseconds(delay);
            _period = TimeSpan.FromMilliseconds(period);
        }

        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the Agent middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Spawns a thread that sends the periodic typing activities until the turn ends.
        /// </remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Activity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
            {
                var containsMessage = activities.Any(e => e.Type == ActivityTypes.Message);
                if (containsMessage)
                {
                    await ProcessTypingAsync(ctx).ConfigureAwait(false);
                }

                return await nextSend().ConfigureAwait(false);
            });

            await ProcessTypingAsync(turnContext).ConfigureAwait(false);

            await next(cancellationToken).ConfigureAwait(false);

            // Ensures there are no Tasks left running.
            await FinishTypingTaskAsync(turnContext).ConfigureAwait(false);
        }

        private static async Task SendTypingAsync(ITurnContext turnContext, TimeSpan delay, TimeSpan period, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await SendTypingActivityAsync(turnContext, cancellationToken).ConfigureAwait(false);
                    }

                    // if we happen to cancel when in the delay we will get a TaskCanceledException
                    await Task.Delay(period, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // do nothing
            }
        }

        private static async Task SendTypingActivityAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // create a TypingActivity, associate it with the conversation and send immediately
            var typingActivity = new Activity
            {
                Type = ActivityTypes.Typing,
                RelatesTo = turnContext.Activity.RelatesTo,
            };

            // sending the Activity directly on the Adapter avoids other Middleware and avoids setting the Responded
            // flag, however, this also requires that the conversation reference details are explicitly added.
            var conversationReference = turnContext.Activity.GetConversationReference();
            typingActivity.ApplyConversationReference(conversationReference);

            // make sure to send the Activity directly on the Adapter rather than via the TurnContext
            await turnContext.Adapter.SendActivitiesAsync(turnContext, [typingActivity], cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts the typing background task for the current conversation.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        private void StartTypingTask(ITurnContext turnContext)
        {
            if (string.IsNullOrEmpty(turnContext?.Activity?.Conversation?.Id) &&
                _tasks.ContainsKey(turnContext.Activity.Conversation.Id))
            {
                return;
            }

            var cts = new CancellationTokenSource();

            // do not await task - we want this to run in the background and we will cancel it when its done
            var typingTask = SendTypingAsync(turnContext, _delay, _period, cts.Token);
            _tasks.TryAdd(turnContext.Activity.Conversation.Id, (typingTask, cts));
        }

        /// <summary>
        /// Finishes the typing background task for the current conversation.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        private async Task FinishTypingTaskAsync(ITurnContext turnContext)
        {
            if (string.IsNullOrEmpty(turnContext?.Activity?.Conversation?.Id) &&
                !_tasks.ContainsKey(turnContext.Activity.Conversation.Id))
            {
                return;
            }

            // Cancel the typing loop.
            _tasks.TryGetValue(turnContext.Activity.Conversation.Id, out var item);
            var (typingTask, cts) = item;
            cts?.Cancel();
            cts?.Dispose();
            if (typingTask != null && !typingTask.IsFaulted)
            {
                await typingTask.ConfigureAwait(false);
                typingTask.Dispose();
            }

            _tasks.TryRemove(turnContext.Activity.Conversation.Id, out _);
        }


        private static bool IsAgent(ITurnContext turnContext)
        {
            return turnContext.Identity as ClaimsIdentity != null && AgentClaims.IsAgentClaim(turnContext.Identity);
        }

        /// <summary>
        /// Start a timer to periodically send the typing activity (Agents running as skills should not send typing activity).
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        private async Task ProcessTypingAsync(ITurnContext turnContext)
        {
            if (!IsAgent(turnContext) && turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Override the typing background task.
                await FinishTypingTaskAsync(turnContext).ConfigureAwait(false);
                StartTypingTask(turnContext);
            }
        }
    }
}
