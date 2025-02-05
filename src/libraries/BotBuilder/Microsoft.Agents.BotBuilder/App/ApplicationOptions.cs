﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using System;
using Microsoft.Agents.Storage;
using Microsoft.Agents.BotBuilder.State;
using Microsoft.Agents.BotBuilder.App.AdaptiveCards;

namespace Microsoft.Agents.BotBuilder.App
{
    /// <summary>
    /// Options for the <see cref="Application"/> class.
    /// </summary>
    public class ApplicationOptions
    {
        /// <summary>
        /// Optional. Storage provider to use for the application.
        /// </summary>
        public IStorage? Storage { get; set; }

        /// <summary>
        /// Optional. Options used to customize the processing of Adaptive Card requests.
        /// </summary>
        public AdaptiveCardsOptions? AdaptiveCards { get; set; }

        /// <summary>
        /// Optional. Factory used to create a custom turn state instance.
        /// </summary>
        public Func<ITurnState>? TurnStateFactory { get; set; }

        /// <summary>
        /// Optional. Logger factory that will be used in this application.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public ILoggerFactory? LoggerFactory { get; set; }

        /// <summary>
        /// Optional. If true, the bot will automatically remove mentions of the bot's name from incoming
        /// messages. Defaults to true.
        /// </summary>
        public bool RemoveRecipientMention { get; set; } = true;

        /// <summary>
        /// Optional. If true, the bot will automatically start a typing timer when messages are received.
        /// This allows the bot to automatically indicate that it's received the message and is processing
        /// the request. Defaults to true.
        /// </summary>
        public bool StartTypingTimer { get; set; } = true;

        //TODO
        /*
        /// <summary>
        /// Optional. Options used to enable authentication for the application.
        /// </summary>
        public AuthenticationOptions<TState>? Authentication { get; set; }
        */
    }
}