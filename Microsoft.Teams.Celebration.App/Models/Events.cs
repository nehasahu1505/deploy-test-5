// <copyright file="Events.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Celebration.App.Utilities.Enums;
    using Newtonsoft.Json;

    /// <summary>
    /// Represent event data.
    /// </summary>
    public class Events
    {
        /// <summary>
        /// Gets or sets event id that uniquely idetifies the event.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets type of event. Birthday/Anniversary/others.
        /// </summary>
        [JsonProperty("type")]
        public EventTypes Type { get; set; }

        /// <summary>
        /// Gets or sets event title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets message to post.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or Sets event Date.
        /// </summary>
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets timezone id.
        /// </summary>
        [JsonProperty("timeZoneId")]
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Gets or sets owner teamsId of event.
        /// </summary>
        [JsonProperty("OwnerId")]
        public string OwnerTeamsId { get; set; }

        /// <summary>
        /// Gets or sets user AAD object id.
        /// </summary>
        [JsonProperty("ownerAadObjectId")]
        public string OwnerAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets image Url for event.
        /// </summary>
        [JsonProperty("imageURL")]
        public string ImageURL { get; set; }

        /// <summary>
        /// Gets or sets month of event.
        /// </summary>
        [JsonProperty("eventMonth")]
        public int EventMonth { get; set; }

        /// <summary>
        /// Gets or sets day of event.
        /// </summary>
        [JsonProperty("eventDay")]
        public int EventDay { get; set; }

        /// <summary>
        /// Gets or sets list of team id's, where the bot is installed.
        /// </summary>
        [JsonProperty("teams")]
        public List<Teams> Teams { get; set; }
    }
}