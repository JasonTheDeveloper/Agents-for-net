﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Extensions.Teams.Models;
using Xunit;

namespace Microsoft.Agents.Extensions.Teams.Tests.Model
{
    public class MessageActionsPayloadFromTests
    {
        [Fact]
        public void MessageActionsPayloadFromInits()
        {
            var user = new MessageActionsPayloadUser("aadUser");
            var application = new MessageActionsPayloadApp("bot");
            var conversation = new MessageActionsPayloadConversation("channel");

            var msgActionsPayloadFrom = new MessageActionsPayloadFrom(user, application, conversation);

            Assert.NotNull(msgActionsPayloadFrom);
            Assert.IsType<MessageActionsPayloadFrom>(msgActionsPayloadFrom);
            Assert.Equal(user, msgActionsPayloadFrom.User);
            Assert.Equal(application, msgActionsPayloadFrom.Application);
            Assert.Equal(conversation, msgActionsPayloadFrom.Conversation);
        }

        [Fact]
        public void MessageActionsPayloadFromInitsWithNoArgs()
        {
            var msgActionsPayloadFrom = new MessageActionsPayloadFrom();

            Assert.NotNull(msgActionsPayloadFrom);
            Assert.IsType<MessageActionsPayloadFrom>(msgActionsPayloadFrom);
        }
    }
}
