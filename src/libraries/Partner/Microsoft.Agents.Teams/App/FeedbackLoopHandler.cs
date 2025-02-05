﻿using Microsoft.Agents.BotBuilder;
using Microsoft.Agents.BotBuilder.State;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Agents.Teams.App
{
    /// <summary>
    /// Function for feedback loop activites
    /// </summary>
    /// <param name="turnContext">A strongly-typed context object for this turn.</param>
    /// <param name="turnState">The turn state object that stores arbitrary data for this turn.</param>
    /// <param name="feedbackLoopData">The feedback loop data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects
    /// or threads to receive notice of cancellation.</param>
    /// <returns>A task that represents the work queued to execute.</returns>
    public delegate Task FeedbackLoopHandler(ITurnContext turnContext, ITurnState turnState, FeedbackLoopData feedbackLoopData, CancellationToken cancellationToken);
}
