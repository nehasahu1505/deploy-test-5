// <copyright file="Teams.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Store the metadata that is used to identify the last interaction of bot with team.
    /// </summary>
    public class Teams
    {
        /// <summary>
        /// Gets or sets team's Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}