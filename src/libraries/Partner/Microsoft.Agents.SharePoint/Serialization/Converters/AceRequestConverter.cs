﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Core.Serialization.Converters;
using Microsoft.Agents.SharePoint.Models;
using System.Text.Json;

namespace Microsoft.Agents.SharePoint.Serialization.Converters
{
    internal class AceRequestConverter : ConnectorConverter<AceRequest>
    {
        protected override bool TryReadGenericProperty(ref Utf8JsonReader reader, AceRequest value, string propertyName, JsonSerializerOptions options)
        {
            if (propertyName.Equals(nameof(value.Properties)))
            {
                SetGenericProperty(ref reader, data => value.Properties = data, options);
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
