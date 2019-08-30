// <copyright file="ApplicationConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration
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
        /// Cosmos DB endpoint url
        /// </summary>
        CosmosDBEndpointUrl,

        /// <summary>
        /// CosmosDB connection key
        /// </summary>
        CosmosDBKey,
    }
}