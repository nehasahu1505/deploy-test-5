// <copyright file="EventNotificationCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Models
{
    using Microsoft.Bot.Connector;

    /// <summary>
    /// Represents the payload to send event notification card.
    /// </summary>
    public class EventNotificationCardPayload
    {
        /// <summary>
        /// Gets or sets User Name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets teams id of user
        /// </summary>
        public string UserTeamsId { get; set; }

        /// <summary>
        /// Gets or sets Message to send
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets Attachment to send
        /// </summary>
        public Attachment Attachment { get; set; }
    }
}