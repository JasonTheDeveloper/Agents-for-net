﻿using System.Text.Json;
using Microsoft.Agents.MCP.Core.Abstractions;
using Microsoft.Agents.MCP.Core.Payloads;

namespace Microsoft.Agents.MCP.Server.Handlers.Methods.Tools.ToolsCall.Handlers;

public interface IMcpToolExecutor
{
    public abstract Type InputType { get; }
    public abstract string Id { get; }
    public abstract string Description { get; }
    public abstract Task<ToolsCallResult> ExecuteAsync(McpRequest<ToolsCallRequest> payload, IMcpContext context, CancellationToken ct);
}