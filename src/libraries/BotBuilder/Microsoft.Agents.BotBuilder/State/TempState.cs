﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Agents.BotBuilder.State
{
    public class TempState : IBotState
    {
        public static readonly string ScopeName = "temp";
        private readonly Dictionary<string, object> _state = [];

        public string Name => ScopeName;

        public void ClearState()
        {
            _state.Clear();
        }

        public Task DeleteStateAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            _state.Clear();
            return Task.CompletedTask;
        }

        public bool HasValue(string name)
        {
            return _state.ContainsKey(name);
        }

        public void DeleteValue(string name)
        {
            _state.Remove(name);
        }

        public T GetValue<T>(string name, Func<T> defaultValueFactory = null)
        {
            if (!_state.TryGetValue(name, out var value))
            {
                if (defaultValueFactory != null)
                {
                    value = defaultValueFactory();
                    SetValue(name, value);
                }
            }

            return (T) value;
        }

        public void SetValue<T>(string name, T value)
        {
            _state[name] = value;
        }

        public T GetValue<T>()
        {
            return GetValue<T>(typeof(T).FullName);
        }

        public void SetValue<T>(T value)
        {
            SetValue(typeof(T).FullName, value);
        }

        public bool IsLoaded()
        {
            return true;
        }

        public Task LoadAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(ITurnContext turnContext, bool force = false, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
