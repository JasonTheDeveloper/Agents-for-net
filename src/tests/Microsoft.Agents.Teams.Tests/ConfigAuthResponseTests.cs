﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Teams.Primitives;
using Xunit;

namespace Microsoft.Agents.Teams.Tests
{
    public class ConfigAuthResponseTests
    {
        [Fact]
        public void ConfigAuthResponseInitWithNoArgs()
        {
            var configAuthResponse = new ConfigAuthResponse();

            Assert.NotNull(configAuthResponse);
            Assert.IsType<ConfigAuthResponse>(configAuthResponse);
            Assert.Equal("config", configAuthResponse.ResponseType);
        }
    }
}
