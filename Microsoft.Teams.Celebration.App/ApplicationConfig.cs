// <copyright file="ApplicationConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    /// <summary>
    /// Application configuration keys
    /// </summary>
    public enum ApplicationConfig
    {
        /// <summary>
        /// Application base uri, without trailing slash
        /// </summary>
        BaseUri,

        /// <summary>
        /// CosmosDB endpoint
        /// </summary>
        DocumentDbUrl,

        /// <summary>
        /// CosmosDB connection key
        /// </summary>
        DocumentDbKey,
    }
}