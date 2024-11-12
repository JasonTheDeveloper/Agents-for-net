﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Agents.Authentication.Msal.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Agents.Authentication.Msal.Model
{
    /// <summary>
    /// Gets and manages connection settings for MSAL Auth
    /// </summary>
    internal class ConnectionSettings : ConnectionSettingsBase, IMSALConnectionSettings
    {
        /*
        public static ConnectionSettings CreateFromConfigurationOptions(IConfigurationSection configurationSection)
        {
            var config = new ConnectionSettings(configurationSection);
            if (configurationSection != null)
            {
                config.CertificateThumbPrint = configurationSection.GetValue<string>("CertThumbprint", string.Empty);
                config.CertificateSubjectName = configurationSection.GetValue<string>("CertSubjectName", string.Empty);
                config.CertificateStoreName = configurationSection.GetValue<string>("CertStoreName", "My");
                config.ValidCertificateOnly = configurationSection.GetValue<bool>("ValidCertificateOnly", true);
                config.SendX5C = configurationSection.GetValue<bool>("SendX5C", false);
                config.ClientSecret = configurationSection.GetValue<string>("ClientSecret", string.Empty);
                config.AuthType = configurationSection.GetValue<AuthTypes>("AuthType", AuthTypes.ClientSecret);
            }

            ValidateConfiguration(config);

            return config;
        }
        */

        public ConnectionSettings(IConfigurationSection configurationSection) : base(configurationSection)
        {
            if (configurationSection != null)
            {
                CertificateThumbPrint = configurationSection.GetValue<string>("CertThumbprint", string.Empty);
                CertificateSubjectName = configurationSection.GetValue<string>("CertSubjectName", string.Empty);
                CertificateStoreName = configurationSection.GetValue<string>("CertStoreName", "My");
                ValidCertificateOnly = configurationSection.GetValue<bool>("ValidCertificateOnly", true);
                SendX5C = configurationSection.GetValue<bool>("SendX5C", false);
                ClientSecret = configurationSection.GetValue<string>("ClientSecret", string.Empty);
                if (string.IsNullOrEmpty(configurationSection.GetValue<string>("AuthType")))
                {
                    AuthType = AuthTypes.ClientSecret;
                }
                else
                {
                    AuthType = configurationSection.GetValue<AuthTypes>("AuthType", AuthTypes.ClientSecret);
                }
            }

            ValidateConfiguration();
        }

        /// <summary>
        /// Auth Type to use for the connection
        /// </summary>
        public AuthTypes AuthType { get; set; }

        /// <summary>
        /// Certificate thumbprint to use for the connection when using a certificate that is resident on the machine
        /// </summary>
        public string CertificateThumbPrint { get; set; }

        /// <summary>
        /// Client Secret to use for the connection when using a client secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Subject name to search a cert for. 
        /// </summary>
        public string CertificateSubjectName { get; set; }

        /// <summary>
        /// Cert store name to use. 
        /// </summary>
        public string CertificateStoreName { get; set; }

        /// <summary>
        /// Only use valid certs.  Defaults to true.
        /// </summary>
        public bool ValidCertificateOnly { get; set; } = true;

        /// <summary>
        /// Use x5c for certs.  Defaults to false.
        /// </summary>
        public bool SendX5C { get; set; } = false;

        /// <summary>
        /// Validates required properties are present in the configuration for the requested authentication type. 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ValidateConfiguration()
        {
            switch (AuthType)
            {
                case AuthTypes.Certificate:
                    if (string.IsNullOrEmpty(ClientId))
                    {
                        throw new ArgumentNullException(nameof(ClientId), "ClientId is required");
                    }
                    if (string.IsNullOrEmpty(CertificateThumbPrint))
                    {
                        throw new ArgumentNullException(nameof(CertificateThumbPrint), "CertificateThumbPrint is required");
                    }
                    if (string.IsNullOrEmpty(Authority) && string.IsNullOrEmpty(TenantId))
                    {
                        throw new ArgumentNullException(nameof(Authority), "TenantId or Authority is required");
                    }
                    break;
                case AuthTypes.CertificateSubjectName:
                    if (string.IsNullOrEmpty(ClientId))
                    {
                        throw new ArgumentNullException(nameof(ClientId), "ClientId is required");
                    }
                    if (string.IsNullOrEmpty(CertificateSubjectName))
                    {
                        throw new ArgumentNullException(nameof(CertificateSubjectName), "CertificateSubjectName is required");
                    }
                    if (string.IsNullOrEmpty(Authority) && string.IsNullOrEmpty(TenantId))
                    {
                        throw new ArgumentNullException(nameof(Authority), "TenantId or Authority is required");
                    }
                    break;
                case AuthTypes.ClientSecret:
                    if (string.IsNullOrEmpty(ClientId))
                    {
                        throw new ArgumentNullException(nameof(ClientId), "ClientId is required");
                    }
                    if (string.IsNullOrEmpty(ClientSecret))
                    {
                        throw new ArgumentNullException(nameof(ClientSecret), "ClientSecret is required");
                    }
                    if (string.IsNullOrEmpty(Authority) && string.IsNullOrEmpty(TenantId))
                    {
                        throw new ArgumentNullException(nameof(Authority), "TenantId or Authority is required");
                    }
                    break;
                case AuthTypes.UserManagedIdentity:
                    if (string.IsNullOrEmpty(ClientId))
                    {
                        throw new ArgumentNullException(nameof(ClientId), "ClientId is required");
                    }
                    break;
                case AuthTypes.SystemManagedIdentity:
                    // No additional validation needed
                    break;
                default:
                    break;
            }
        }

    }
}
