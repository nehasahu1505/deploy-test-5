// <copyright file="Team.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Store the teams metadata.
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Gets or sets team's Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}