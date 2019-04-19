// <copyright file="CelebrationEvent.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Teams.Celebration.App.Models.Enums;
    using Newtonsoft.Json;

    /// <summary>
    /// Represent event data.
    /// </summary>
    public class CelebrationEvent
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
        /// Gets or sets event header.
        /// </summary>
        [JsonProperty("header")]
        public string Header { get; set; }

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
        /// Gets or sets timezone id given by TimeZoneInfo.Id .
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
        /// Gets month part of the event date.
        /// </summary>
        [JsonProperty("eventMonth")]
        public int EventMonth
        {
            get { return this.Date.Month; }
        }

        /// <summary>
        /// Gets day part of the event date.
        /// </summary>
        [JsonProperty("eventDay")]
        public int EventDay
        {
            get { return this.Date.Day; }
        }

        /// <summary>
        /// Gets or sets time to post event in team.
        /// </summary>
        public TimeSpan TimeToPostEvent { get; set; }

        /// <summary>
        /// Gets or sets list of team information where bot is installed.
        /// </summary>
        [JsonProperty("teams")]
        public List<Team> Teams { get; set; }
    }
}