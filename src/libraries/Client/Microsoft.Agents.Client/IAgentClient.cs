﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.Core.Models;

namespace Microsoft.Agents.Client
{
    public interface IAgentClient : IDisposable
    {
        string Name { get; }

        /// <summary>
        /// Sends an Activity with DeliveryMode "normal" or "expectReplies".  For `normal`, this would require handling of async replies via IChannelApiHandler via ChannelApiController.
        /// </summary>
        /// <remarks>This is a rather base level of functionality and in most cases <see cref="SendActivityForResultAsync"/> is easier to use.</remarks>
        /// <param name="agentConversationId"></param>
        /// <param name="activity"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="relatesTo"></param>
        /// <returns></returns>
        Task<InvokeResponse<T>> SendActivityAsync<T>(string agentConversationId, IActivity activity, IActivity relatesTo = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends an Activity with DeliveryMode "normal" or "expectReplies". Convenience method when a result is not expected.
        /// </summary>
        /// <param name="agentConversationId"></param>
        /// <param name="activity"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="relatesTo"></param>
        /// <returns></returns>
        Task SendActivityAsync(string agentConversationId, IActivity activity, IActivity relatesTo = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send an Activity with streaming replies.
        /// </summary>
        /// <remarks>
        /// This method will handle EndOfConversation Value and InvokeResponse.Body return values, specified by T.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="agentConversationId"></param>
        /// <param name="activity"></param>
        /// <param name="handler"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="relatesTo"></param>
        /// <returns></returns>
        Task<T> SendActivityStreamedAsync<T>(string agentConversationId, IActivity activity, Action<IActivity> handler, IActivity relatesTo = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agentConversationId"></param>
        /// <param name="activity"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="relatesTo"></param>
        /// <returns></returns>
        IAsyncEnumerable<object> SendActivityStreamedAsync(string agentConversationId, IActivity activity, IActivity relatesTo = null, CancellationToken cancellationToken = default);
    }
}
