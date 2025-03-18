﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Agents.Client
{
    /// <summary>
    /// Values and constants used for Conversation specific info.
    /// </summary>
    public static class ConversationConstants
    {
        /// <summary>
        /// The name of Http Request Header to add Conversation Id to multi-agent requests.
        /// </summary>
        public const string ConversationIdHttpHeaderName = "x-ms-conversation-id";
    }
}
