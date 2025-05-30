﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.Agents.Builder.State
{
    /// <summary>
    /// IPropertyManager defines implementation of a source of named properties.
    /// </summary>
    public interface IPropertyManager
    {
        /// <summary>
        /// Creates a managed state property accessor for a property.
        /// </summary>
        /// <typeparam name="T">The property value type.</typeparam>
        /// <param name="name">The name of the property accessor.</param>
        /// <returns>A state property accessor for the property.</returns>
        [Obsolete("Use AgentState Get/Set methods and call AgentState.LoadAsync or use AutoSaveStateMiddleware.")]
        IStatePropertyAccessor<T> CreateProperty<T>(string name);
    }
}
