// <copyright file="EventMessageActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using System;

    /// <summary>
    /// Represents an activity to construct the celebration card.
    /// </summary>
    [Serializable]
    public class EventMessageActivity
    {
        /// <summary>
        /// Gets or sets id, which is Id(Guid) in events collection
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets event's owner name
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets event's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets event's Header.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets event's Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets event's Image Url
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets conversation Id that is required to intiate the conversation between user/team and bot.
        /// </summary>
        public string ConversationId { get; set; }
    }
}