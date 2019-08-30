// <copyright file="EventMessage.cs" company="Microsoft">
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
    /// Represents a message to be sent to a team or user
    /// </summary>
    public class EventMessage : Document
    {
        /// <summary>
        /// Gets or sets occurrenceId, which is Id(Guid) in Occurrence collection.
        /// </summary>
        [JsonProperty("occurrenceId")]
        public string OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets event id, which is Id(Guid) in events collection.
        /// </summary>
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets activity which requires to construct the celebration card.
        /// </summary>
        [JsonProperty("activity")]
        public EventMessageActivity Activity { get; set; }

        /// <summary>
        /// Gets or sets messageType
        /// </summary>
        [JsonProperty("messageType")]
        [DefaultValue(MessageType.Unknown)]
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets sent message result.
        /// </summary>
        [JsonProperty("messageSendResult")]
        public MessageSendResult MessageSendResult { get; set; }

        /// <summary>
        /// Gets or sets expiration time at which bot should give up retry to send card.
        /// </summary>
        [JsonProperty("expireAt")]
        public DateTimeOffset ExpireAt { get; set; }
    }
}