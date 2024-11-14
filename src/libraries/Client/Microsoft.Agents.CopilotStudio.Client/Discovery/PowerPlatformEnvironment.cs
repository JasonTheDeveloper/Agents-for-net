﻿using System;

namespace Microsoft.Agents.CopilotStudio.Client.Discovery
{
    internal class PowerPlatformEnvironment
    {
        /// <summary>
        ///  The API version to use when connecting to the Power Platform API.
        /// </summary>
        static readonly string ApiVersion = "2022-03-01-preview";

        /// <summary>
        /// Gets the Power Platform API connection URL for the given settings.
        /// </summary>
        /// <param name="settings">Configuration Settings to use</param>
        /// <param name="conversationId">Optional, Conversation ID to address</param>
        /// <param name="botType">Type of Bot being addressed. <see cref="BotType"/></param>
        /// <param name="cloud">Power Platform Cloud Hosting Bot <see cref="PowerPlatformCloud"/></param>
        /// <param name="cloudBaseAddress">Power Platform API endpoint to use if Cloud is configured as "other". <see cref="PowerPlatformCloud.Other"/> </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Uri GetCopilotStudioConnectionUrl(
                ConnectionSettings settings,
                string? conversationId,
                BotType botType = BotType.Published,
                PowerPlatformCloud cloud = PowerPlatformCloud.Prod, 
                string? cloudBaseAddress = default
            )
        {
            if (cloud == PowerPlatformCloud.Other && string.IsNullOrWhiteSpace(cloudBaseAddress))
            {
                throw new ArgumentException("cloudBaseAddress must be provided when PowerPlatformCloudCategory is Other", nameof(cloudBaseAddress));
            }
            if (string.IsNullOrEmpty(settings.EnvironmentId))
            {
                throw new ArgumentException("EnvironmentId must be provided", nameof(settings.EnvironmentId));
            }
            if (string.IsNullOrEmpty(settings.BotIdentifier))
            {
                throw new ArgumentException("BotIdentifier must be provided", nameof(settings.BotIdentifier));
            }
            if ( settings.Cloud != null && settings.Cloud != PowerPlatformCloud.Unknown)
            {
                cloud = settings.Cloud.Value;
            }
            if (cloud == PowerPlatformCloud.Other)
            {
                if (!string.IsNullOrEmpty(cloudBaseAddress) && Uri.IsWellFormedUriString(cloudBaseAddress, UriKind.Absolute))
                {
                    cloud = PowerPlatformCloud.Other;
                }
                else
                {
                    if (!string.IsNullOrEmpty(settings.CustomPowerPlatformCloud) && Uri.IsWellFormedUriString(settings.CustomPowerPlatformCloud, UriKind.RelativeOrAbsolute))
                    {
                        cloud = PowerPlatformCloud.Other;
                        cloudBaseAddress = settings.CustomPowerPlatformCloud;
                    }
                    else
                    {
                        throw new ArgumentException("Either CustomPowerPlatformCloud or cloudBaseAddress must be provided when PowerPlatformCloudCategory is Other");
                    }
                }
            }
            if ( settings.CopilotBotType != null)
            {
                botType = settings.CopilotBotType.Value;
            }

            cloudBaseAddress ??= "api.unknown.powerplatform.com";

            var host = GetEnvironmentEndpoint(cloud, settings.EnvironmentId, cloudBaseAddress);
            return CreateUri(settings.BotIdentifier, host, botType, conversationId);
        }

        /// <summary>
        /// Returns the Power Platform API Audience.
        /// </summary>
        /// <param name="settings">Configuration Settings to use</param>
        /// <param name="cloud">Power Platform Cloud Hosting Bot <see cref="PowerPlatformCloud"/></param>
        /// <param name="cloudBaseAddress">Power Platform API endpoint to use if Cloud is configured as "other". <see cref="PowerPlatformCloud.Other"/> </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetTokenAudience(
            ConnectionSettings? settings,
            PowerPlatformCloud cloud = PowerPlatformCloud.Unknown, 
            string? cloudBaseAddress = default)
        {
            if (cloud == PowerPlatformCloud.Other && string.IsNullOrWhiteSpace(cloudBaseAddress))
            {
                throw new ArgumentException("cloudBaseAddress must be provided when PowerPlatformCloudCategory is Other", nameof(cloudBaseAddress));
            }

            if (settings == null && cloud == PowerPlatformCloud.Unknown)
            {
                throw new ArgumentException("Either settings or cloud must be provided", nameof(settings));
            }

            if (settings != null && settings.Cloud != null && settings.Cloud != PowerPlatformCloud.Unknown)
            {
                cloud = settings.Cloud.Value;
            }

            if (cloud == PowerPlatformCloud.Other)
            {
                if (!string.IsNullOrEmpty(cloudBaseAddress) && Uri.IsWellFormedUriString(cloudBaseAddress, UriKind.Absolute))
                {
                    cloud = PowerPlatformCloud.Other;
                }
                else
                {
                    if (!string.IsNullOrEmpty(settings?.CustomPowerPlatformCloud) && Uri.IsWellFormedUriString(settings.CustomPowerPlatformCloud, UriKind.RelativeOrAbsolute))
                    {
                        cloud = PowerPlatformCloud.Other;
                        cloudBaseAddress = settings.CustomPowerPlatformCloud;
                    }
                    else
                    {
                        throw new ArgumentException("Either CustomPowerPlatformCloud or cloudBaseAddress must be provided when PowerPlatformCloudCategory is Other");
                    }
                }
            }


            cloudBaseAddress ??= "api.unknown.powerplatform.com";
            return $"https://{GetEndpointSuffix(cloud, cloudBaseAddress)}/.default";
        }


        #region Private 

        /// <summary>
        /// Creates the PowerPlatform API connection URL for the given settings.
        /// </summary>
        private static Uri CreateUri(string botIdentifier, string host, BotType botType, string? conversationId)
        {
            string botPathName = "";
            if (BotType.Published == botType)
            {
                botPathName = "dataverse-backed";
            }
            else
            {
                botPathName = "prebuilt";
            }
            var builder = new UriBuilder();
            builder.Scheme = "https";
            builder.Host = host;
            builder.Query = $"api-version={ApiVersion}";
            if (string.IsNullOrEmpty(conversationId))
                builder.Path = $"/copilotstudio/{botPathName}/authenticated/bots/{botIdentifier}/conversations";
            else
                builder.Path = $"/copilotstudio/{botPathName}/authenticated/bots/{botIdentifier}/conversations/{conversationId}";
            return builder.Uri;
        }

        /// <summary>
        /// Gets the environment endpoint for the Power Platform API.
        /// </summary>
        /// <param name="cloud"></param>
        /// <param name="environmentId"></param>
        /// <param name="cloudBaseAddress"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static string GetEnvironmentEndpoint(PowerPlatformCloud cloud, string environmentId, string? cloudBaseAddress = default)
        {
            if (cloud == PowerPlatformCloud.Other && string.IsNullOrWhiteSpace(cloudBaseAddress))
            {
                throw new ArgumentException("cloudBaseAddress must be provided when PowerPlatformCloudCategory is Other", nameof(cloudBaseAddress));
            }
            cloudBaseAddress ??= "api.unknown.powerplatform.com";

            var normalizedResourceId = environmentId.ToLower().Replace("-", "");
            var idSuffixLength = GetIdSuffixLength(cloud);
            var hexPrefix = normalizedResourceId.Substring(0, normalizedResourceId.Length - idSuffixLength);
            var hexSuffix = normalizedResourceId.Substring(normalizedResourceId.Length - idSuffixLength, idSuffixLength);
            return $"{hexPrefix}.{hexSuffix}.environment.{GetEndpointSuffix(cloud, cloudBaseAddress)}";
        }

        /// <summary>
        /// Gets the base Cloud to attach to the Power Platform API end point. 
        /// </summary>
        /// <param name="category"></param>
        /// <param name="cloudBaseAddress"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static string GetEndpointSuffix(PowerPlatformCloud category, string cloudBaseAddress) => category switch
        {
            PowerPlatformCloud.Local => "api.powerplatform.localhost",
            PowerPlatformCloud.Exp => "api.exp.powerplatform.com",
            PowerPlatformCloud.Dev => "api.dev.powerplatform.com",
            PowerPlatformCloud.Prv => "api.prv.powerplatform.com",
            PowerPlatformCloud.Test => "api.test.powerplatform.com",
            PowerPlatformCloud.Preprod => "api.preprod.powerplatform.com",
            PowerPlatformCloud.FirstRelease => "api.powerplatform.com",
            PowerPlatformCloud.Prod => "api.powerplatform.com",
            PowerPlatformCloud.GovFR => "api.gov.powerplatform.microsoft.us",
            PowerPlatformCloud.Gov => "api.gov.powerplatform.microsoft.us",
            PowerPlatformCloud.High => "api.high.powerplatform.microsoft.us",
            PowerPlatformCloud.DoD => "api.appsplatform.us",
            PowerPlatformCloud.Mooncake => "api.powerplatform.partner.microsoftonline.cn",
            PowerPlatformCloud.Ex => "api.powerplatform.eaglex.ic.gov",
            PowerPlatformCloud.Rx => "api.powerplatform.microsoft.scloud",
            PowerPlatformCloud.Other => cloudBaseAddress,
            _ => throw new ArgumentException($"Invalid cluster category value: {category}", nameof(category)),
        };

        /// <summary>
        /// Get Environment ID Suffix Length for the given Power Platform Cloud Category.
        /// </summary>
        /// <param name="cloud"></param>
        /// <returns></returns>
        private static int GetIdSuffixLength(PowerPlatformCloud cloud)
        {
            switch (cloud)
            {
                case PowerPlatformCloud.FirstRelease:
                case PowerPlatformCloud.Prod:
                    return 2;
                default:
                    return 1;
            }
        }

        #endregion
    }
}
