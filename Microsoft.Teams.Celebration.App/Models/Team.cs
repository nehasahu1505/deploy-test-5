// <copyright file="Team.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using Microsoft.Azure.Documents;
    using Newtonsoft.Json;

    /// <summary>
    /// Store the teams meta data.
    /// </summary>
    public class Team : Document
    {
        /// <summary>
        /// Gets or sets team name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}