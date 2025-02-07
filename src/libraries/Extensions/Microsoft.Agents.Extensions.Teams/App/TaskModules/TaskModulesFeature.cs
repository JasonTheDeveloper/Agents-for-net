﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.BotBuilder;
using Microsoft.Agents.BotBuilder.App;
using Microsoft.Agents.BotBuilder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Core.Serialization;
using Microsoft.Agents.Extensions.Teams.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Agents.Extensions.Teams.App.TaskModules
{
    /// <summary>
    /// TaskModules class to enable fluent style registration of handlers related to Task Modules.
    /// </summary>
    public class TaskModulesFeature
    {
        private static readonly string FETCH_INVOKE_NAME = "task/fetch";
        private static readonly string SUBMIT_INVOKE_NAME = "task/submit";

        //TODO
        //private static readonly string DEFAULT_TASK_DATA_FILTER = "verb";

        private readonly Application _app;

        /// <summary>
        /// Creates a new instance of the TaskModules class.
        /// </summary>
        /// <param name="app"> The top level application class to register handlers with.</param>
        public TaskModulesFeature(Application app)
        {
            this._app = app;
        }

        /// <summary>
        ///  Registers a handler to process the initial fetch of the task module.
        /// </summary>
        /// <param name="verb">Name of the verb to register the handler for.</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnFetch(string verb, FetchHandlerAsync handler)
        {
            throw new NotImplementedException();

            //TODO
            /*
            ArgumentNullException.ThrowIfNull(verb);
            ArgumentNullException.ThrowIfNull(handler);
            string filter = _app.Options.TaskModules?.TaskDataFilter ?? DEFAULT_TASK_DATA_FILTER;
            RouteSelectorAsync routeSelector = CreateTaskSelector((string input) => string.Equals(verb, input), filter, FETCH_INVOKE_NAME);
            return OnFetch(routeSelector, handler);
            */
        }

        /// <summary>
        ///  Registers a handler to process the initial fetch of the task module.
        /// </summary>
        /// <param name="verbPattern">Regular expression to match against the verbs to register the handler for.</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnFetch(Regex verbPattern, FetchHandlerAsync handler)
        {
            throw new NotImplementedException();

            //TODO
            /*
            ArgumentNullException.ThrowIfNull(verbPattern);
            ArgumentNullException.ThrowIfNull(handler);
            string filter = _app.Options.TaskModules?.TaskDataFilter ?? DEFAULT_TASK_DATA_FILTER;
            RouteSelectorAsync routeSelector = CreateTaskSelector((string input) => verbPattern.IsMatch(input), filter, FETCH_INVOKE_NAME);
            return OnFetch(routeSelector, handler);
            */
        }

        /// <summary>
        ///  Registers a handler to process the initial fetch of the task module.
        /// </summary>
        /// <param name="routeSelector">Function that's used to select a route. The function returning true triggers the route.</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnFetch(RouteSelectorAsync routeSelector, FetchHandlerAsync handler)
        {
            ArgumentNullException.ThrowIfNull(routeSelector);
            ArgumentNullException.ThrowIfNull(handler);
            RouteHandler routeHandler = async (ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
            {
                TaskModuleAction? taskModuleAction;
                if (!string.Equals(turnContext.Activity.Type, ActivityTypes.Invoke, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(turnContext.Activity.Name, FETCH_INVOKE_NAME)
                    || (taskModuleAction = ProtocolJsonSerializer.ToObject<TaskModuleAction>(turnContext.Activity.Value)) == null)
                {
                    throw new InvalidOperationException($"Unexpected TaskModules.OnFetch() triggered for activity type: {turnContext.Activity.Type}");
                }

                TaskModuleResponse result = await handler(turnContext, turnState, taskModuleAction.Value, cancellationToken);

                // Check to see if an invoke response has already been added
                if (!turnContext.StackState.Has(ChannelAdapter.InvokeResponseKey))
                {
                    Activity activity = ActivityUtilities.CreateInvokeResponseActivity(result);
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            };

            _app.AddRoute(routeSelector, routeHandler, isInvokeRoute: true);
            return _app;
        }

        /// <summary>
        ///  Registers a handler to process the initial fetch of the task module.
        /// </summary>
        /// <param name="routeSelectors">Combination of String, Regex, and RouteSelectorAsync selectors.</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnFetch(MultipleRouteSelector routeSelectors, FetchHandlerAsync handler)
        {
            ArgumentNullException.ThrowIfNull(routeSelectors);
            ArgumentNullException.ThrowIfNull(handler);

            if (routeSelectors.Strings != null)
            {
                foreach (string verb in routeSelectors.Strings)
                {
                    OnFetch(verb, handler);
                }
            }
            if (routeSelectors.Regexes != null)
            {
                foreach (Regex verbPattern in routeSelectors.Regexes)
                {
                    OnFetch(verbPattern, handler);
                }
            }
            if (routeSelectors.RouteSelectors != null)
            {
                foreach (RouteSelectorAsync routeSelector in routeSelectors.RouteSelectors)
                {
                    OnFetch(routeSelector, handler);
                }
            }

            return _app;
        }

        /// <summary>
        /// Registers a handler to process the submission of a task module.
        /// </summary>
        /// <param name="verb">Name of the verb to register the handler for.</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnSubmit(string verb, SubmitHandlerAsync handler)
        {
            throw new NotImplementedException();

            //TODO
            /*
            ArgumentNullException.ThrowIfNull(verb);
            ArgumentNullException.ThrowIfNull(handler);
            string filter = _app.Options.TaskModules?.TaskDataFilter ?? DEFAULT_TASK_DATA_FILTER;
            RouteSelectorAsync routeSelector = CreateTaskSelector((string input) => string.Equals(verb, input), filter, SUBMIT_INVOKE_NAME);
            return OnSubmit(routeSelector, handler);
            */
        }


        /// <summary>
        /// Registers a handler to process the submission of a task module.
        /// </summary>
        /// <param name="verbPattern">Regular expression to match against the verbs to register the handler for</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnSubmit(Regex verbPattern, SubmitHandlerAsync handler)
        {
            throw new NotImplementedException();

            //TODO
            /*
            ArgumentNullException.ThrowIfNull(verbPattern);
            ArgumentNullException.ThrowIfNull(handler);
            string filter = _app.Options.TaskModules?.TaskDataFilter ?? DEFAULT_TASK_DATA_FILTER;
            RouteSelectorAsync routeSelector = CreateTaskSelector((string input) => verbPattern.IsMatch(input), filter, SUBMIT_INVOKE_NAME);
            return OnSubmit(routeSelector, handler);
            */
        }

        /// <summary>
        /// Registers a handler to process the submission of a task module.
        /// </summary>
        /// <param name="routeSelector">Function that's used to select a route. The function returning true triggers the route.</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnSubmit(RouteSelectorAsync routeSelector, SubmitHandlerAsync handler)
        {
            ArgumentNullException.ThrowIfNull(routeSelector);
            ArgumentNullException.ThrowIfNull(handler);
            RouteHandler routeHandler = async (ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken) =>
            {
                TaskModuleAction? taskModuleAction;
                if (!string.Equals(turnContext.Activity.Type, ActivityTypes.Invoke, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(turnContext.Activity.Name, SUBMIT_INVOKE_NAME)
                    || (taskModuleAction = ProtocolJsonSerializer.ToObject<TaskModuleAction>(turnContext.Activity)) == null)
                {
                    throw new InvalidOperationException($"Unexpected TaskModules.OnSubmit() triggered for activity type: {turnContext.Activity.Type}");
                }

                TaskModuleResponse result = await handler(turnContext, turnState, taskModuleAction.Value, cancellationToken);

                // Check to see if an invoke response has already been added
                if (!turnContext.StackState.Has(ChannelAdapter.InvokeResponseKey))
                {
                    Activity activity = ActivityUtilities.CreateInvokeResponseActivity(result);
                    await turnContext.SendActivityAsync(activity, cancellationToken);
                }
            };

            _app.AddRoute(routeSelector, routeHandler, isInvokeRoute: true);
            return _app;
        }

        /// <summary>
        /// Registers a handler to process the submission of a task module.
        /// </summary>
        /// <param name="routeSelectors">Combination of String, Regex, and RouteSelectorAsync verb(s) to register the handler for.</param>
        /// <param name="handler">Function to call when the route is triggered.</param>
        /// <returns>The application instance for chaining purposes.</returns>
        public Application OnSubmit(MultipleRouteSelector routeSelectors, SubmitHandlerAsync handler)
        {
            ArgumentNullException.ThrowIfNull(routeSelectors);
            ArgumentNullException.ThrowIfNull(handler);

            if (routeSelectors.Strings != null)
            {
                foreach (string verb in routeSelectors.Strings)
                {
                    OnSubmit(verb, handler);
                }
            }
            if (routeSelectors.Regexes != null)
            {
                foreach (Regex verbPattern in routeSelectors.Regexes)
                {
                    OnSubmit(verbPattern, handler);
                }
            }
            if (routeSelectors.RouteSelectors != null)
            {
                foreach (RouteSelectorAsync routeSelector in routeSelectors.RouteSelectors)
                {
                    OnSubmit(routeSelector, handler);
                }
            }

            return _app;
        }

        private static RouteSelectorAsync CreateTaskSelector(Func<string, bool> isMatch, string filter, string invokeName)
        {
            RouteSelectorAsync routeSelector = (ITurnContext turnContext, CancellationToken cancellationToken) =>
            {
                bool isInvoke = string.Equals(turnContext.Activity.Type, ActivityTypes.Invoke, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(turnContext.Activity.Name, invokeName);
                if (!isInvoke)
                {
                    return Task.FromResult(false);
                }

                //TODO
                /*
                JObject? obj = turnContext.Activity.Value as JObject;
                if (obj == null)
                {
                    return Task.FromResult(false);
                }

                JObject? data = obj["data"] as JObject;
                if (data == null)
                {
                    return Task.FromResult(false);
                }

                bool isVerbMatch = data.TryGetValue(filter, out JToken? filterField) && filterField != null && filterField.Type == JTokenType.String
                && isMatch(filterField.Value<string>()!);

                return Task.FromResult(isVerbMatch);
                */
            return Task.FromResult(false);
            };
            return routeSelector;
        }
    }
}
