// <copyright file="EventOccurrence.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Models
{
    using System;
    using System.ComponentModel;
    using Microsoft.Azure.Documents;
    using Microsoft.Teams.Apps.Celebration.Models.Enums;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the upcoming occurrences of a recurring event
    /// </summary>
    public class EventOccurrence : Document
    {
        /// <summary>
        /// Gets or sets id, which is Id(Guid) in events collection.
        /// </summary>
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets UTC time of upcoming occurrence.
        /// </summary>
        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        /// <summary>
        /// Gets or sets event's Status
        /// </summary>
        [JsonProperty("status")]
        [DefaultValue(EventStatus.Default)]
        public EventStatus Status { get; set; }
    }
}