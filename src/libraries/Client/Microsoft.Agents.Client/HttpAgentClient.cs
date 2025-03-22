﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.Authentication;
using Microsoft.Agents.Client.Errors;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Core.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Agents.Client
{
    /// <summary>
    /// Sends Activities to a remote Agent.
    /// </summary>
    internal class HttpAgentClient : IAgentClient
    {
        private readonly HttpAgentClientSettings _settings;
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private bool _disposed;

        /// <param name="channelSettings"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="tokenProvider"></param>
        /// <param name="logger"></param>
        public HttpAgentClient(
            HttpAgentClientSettings channelSettings,
            IHttpClientFactory httpClientFactory,
            IAccessTokenProvider tokenProvider,
            ILogger<HttpAgentClient> logger = null)
        {
            _settings = channelSettings ?? throw new ArgumentNullException(nameof(channelSettings));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? NullLogger<HttpAgentClient>.Instance;
        }

        /// <inheritdoc/>
        public string Name => _settings.Name;

        /// <inheritdoc/>
        public async Task SendActivityAsync(string agentConversationId, IActivity activity, IActivity relatesTo = null, CancellationToken cancellationToken = default)
        {
            await SendActivityAsync<object>(agentConversationId, activity, relatesTo, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<InvokeResponse<T>> SendActivityAsync<T>(string agentConversationId, IActivity activity, IActivity relatesTo = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(agentConversationId);
            ArgumentNullException.ThrowIfNull(activity);

            _logger.LogInformation($"SendActivityAsync: '{_settings.ConnectionSettings.ClientId}' at '{_settings.ConnectionSettings.Endpoint.ToString()}'");

            // Clone the activity so we can modify it before sending without impacting the original object.
            var activityClone = CreateSendActivity(agentConversationId, activity, relatesTo);

            // Create the HTTP request from the cloned Activity and send it to the bot.
            using var response = await SendRequest(activityClone, cancellationToken).ConfigureAwait(false);
            var content = response.Content != null ? await response.Content.ReadAsStringAsync().ConfigureAwait(false) : null;

            // On success assuming either JSON that can be deserialized to T or empty.
            return new InvokeResponse<T>
            {
                Status = (int)response.StatusCode,
                Body = content?.Length > 0 ? ProtocolJsonSerializer.ToObject<T>(content) : default
            };
        }

        /// <inheritdoc/>
        public async Task<T> SendActivityStreamedAsync<T>(string agentConversationId, IActivity activity, Action<IActivity> handler, IActivity relatesTo = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(agentConversationId);
            ArgumentNullException.ThrowIfNull(activity);
            ArgumentNullException.ThrowIfNull(handler);

            await foreach (var received in SendActivityStreamedAsync(agentConversationId, activity, relatesTo, cancellationToken))
            {
                if (received is IActivity receivedActivity)
                {
                    if (receivedActivity.Type == ActivityTypes.EndOfConversation)
                    {
                        if (receivedActivity.Code != EndOfConversationCodes.CompletedSuccessfully)
                        {
                            throw new ChannelOperationException($"Unsuccessful EOC from Channel: {receivedActivity.Code}");
                        }

                        return ProtocolJsonSerializer.ToObject<T>(receivedActivity.Value);
                    }

                    handler(receivedActivity);
                }
                else if (received is InvokeResponse invokeResponse)
                {
                    if (invokeResponse.Status >= 200 && invokeResponse.Status <= 299)
                    {
                        throw new ChannelOperationException($"Unsuccessful InvokeResponse from Channel: {invokeResponse.Status}");
                    }

                    if (activity.DeliveryMode == DeliveryModes.ExpectReplies)
                    {
                        var expectedReplies = ProtocolJsonSerializer.ToObject<ExpectedReplies>(invokeResponse.Body);
                        foreach (var reply in expectedReplies.Activities)
                        {
                            handler(reply);
                        }

                        return ProtocolJsonSerializer.ToObject<T>(expectedReplies.Body);
                    }
                    else
                    {
                        return ProtocolJsonSerializer.ToObject<T>(invokeResponse.Body);
                    }
                }
            }

            return default;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<object> SendActivityStreamedAsync(string agentConversationId, IActivity activity, IActivity relatesTo = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var activityClone = CreateSendActivity(agentConversationId, activity, relatesTo);
            activityClone.DeliveryMode = DeliveryModes.Stream;

            // Create the HTTP request from the cloned Activity and send it to the Agent.
            using var response = await SendRequest(activityClone, cancellationToken).ConfigureAwait(false);

            // Read streamed response
            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using StreamReader sr = new(stream);
            string streamType = string.Empty;

            string line;
            while ((line = ReadLineSafe(sr)) != null)
            {
                if (line!.StartsWith("event:", StringComparison.InvariantCulture))
                {
                    streamType = line[7..];
                }
                else if (line.StartsWith("data:", StringComparison.InvariantCulture) && streamType == "activity")
                {
                    string jsonRaw = line[6..];
                    var inActivity = ProtocolJsonSerializer.ToObject<IActivity>(jsonRaw);
                    yield return inActivity;
                }
                else if (line.StartsWith("data:", StringComparison.InvariantCulture) && streamType == "invokeResponse")
                {
                    string jsonRaw = line[6..];
                    yield return ProtocolJsonSerializer.ToObject<InvokeResponse>(jsonRaw);
                }
                else
                {
                    _logger.LogWarning("Channel {ChannelInfoId}: Unexpected stream type {StreamType}, {LineValue}", streamType, Name, line.Trim());
                }
            }
        }

        private static string ReadLineSafe(StreamReader reader)
        {
            try
            {
                return reader.ReadLine();
            }
            catch (Exception)
            {
                // TBD:  Not sure how to resolve this yet.  It is because Readline will throw when the 
                // other end closes the stream.
                // (HttpIoException.HttpRequestError == HttpRequestError.ResponseEnded)
                return null;
            }
        }

        private async Task<HttpResponseMessage> SendRequest(IActivity activity, CancellationToken cancellationToken)
        {
            var jsonContent = new StringContent(activity.ToJson(), Encoding.UTF8, "application/json");
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = _settings.ConnectionSettings.Endpoint,
                Content = jsonContent
            };

            using var httpClient = _httpClientFactory.CreateClient(nameof(HttpAgentClient));

            // Add the auth header to the HTTP request.
            var tokenResult = await _tokenProvider.GetAccessTokenAsync(_settings.ConnectionSettings.ResourceUrl, [$"{_settings.ConnectionSettings.ClientId}/.default"]).ConfigureAwait(false);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult);

            var completionOption = activity.DeliveryMode == DeliveryModes.Stream ? HttpCompletionOption.ResponseHeadersRead : HttpCompletionOption.ResponseContentRead;
            HttpResponseMessage response;

            try
            {
                response = await httpClient.SendAsync(httpRequestMessage, completionOption, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendActivityAsync: Channel request failed to '{ChannelName}' at '{ChannelEndpoint}'", Name, _settings.ConnectionSettings.Endpoint.ToString());
                throw Core.Errors.ExceptionHelper.GenerateException<InvalidOperationException>(ErrorHelper.SendToAgentFailed, ex, Name);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("SendActivityAsync: Channel request unsuccessful to '{ChannelName}' at '{ChannelEndpoint}' returned '{ChannelResponse}'", 
                    Name, _settings.ConnectionSettings.Endpoint.ToString(), (int)response.StatusCode);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw Core.Errors.ExceptionHelper.GenerateException<InvalidOperationException>(ErrorHelper.SendToAgentUnauthorized, null, Name);
                }
                throw Core.Errors.ExceptionHelper.GenerateException<InvalidOperationException>(ErrorHelper.SendToAgentUnsuccessful, null, Name, response.StatusCode.ToString());
            }

            return response;
        }

        private IActivity CreateSendActivity(string agentConversationId, IActivity activity, IActivity relatesTo)
        {
            // Clone the activity so we can modify it before sending without impacting the original object.
            var activityClone = activity.Clone();

            // Apply the appropriate addressing to the newly created Activity.
            if (relatesTo != null)
            {
                activityClone.RelatesTo = new ConversationReference
                {
                    ServiceUrl = relatesTo.ServiceUrl,
                    ActivityId = relatesTo.Id,
                    ChannelId = relatesTo.ChannelId,
                    Locale = relatesTo.Locale,
                    Conversation = new ConversationAccount
                    {
                        Id = relatesTo.Conversation.Id,
                        Name = relatesTo.Conversation.Name,
                        ConversationType = relatesTo.Conversation.ConversationType,
                        AadObjectId = relatesTo.Conversation.AadObjectId,
                        IsGroup = relatesTo.Conversation.IsGroup,
                        Properties = relatesTo.Conversation.Properties,
                        Role = relatesTo.Conversation.Role,
                        TenantId = relatesTo.Conversation.TenantId,
                    }
                };
            }

            activityClone.ServiceUrl = _settings.ConnectionSettings.ServiceUrl;
            activityClone.Recipient ??= new ChannelAccount();
            activityClone.Recipient.Role = RoleTypes.Skill;
            activityClone.From ??= new ChannelAccount();
            activityClone.From.Role = RoleTypes.Skill;

            activityClone.Conversation ??= new ConversationAccount();
            if (!string.IsNullOrEmpty(activityClone.Conversation.Id))
            {
                activityClone.Conversation.Id = agentConversationId;
            }

            return activityClone;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of dispose pattern.
        /// </summary>
        /// <param name="disposing">Indicates where this method is called from.</param>
        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }
    }
}
