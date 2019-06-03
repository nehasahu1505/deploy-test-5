// <copyright file="EventOccurrence.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using System;
    using System.ComponentModel;
    using Microsoft.Azure.Documents;
    using Microsoft.Teams.Celebration.App.Models.Enums;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the upcoming occurrences of a recurring event
    /// </summary>
    public class EventOccurrence : Document
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventOccurrence"/> class.
        /// </summary>
        public EventOccurrence()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets Guid.
        /// </summary>
        [JsonProperty("Id")]
        public new string Id { get; set; }

        /// <summary>
        /// Gets or sets id, which is Id(Guid) in events collection.
        /// </summary>
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets UTC time of upcoming occurance.
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