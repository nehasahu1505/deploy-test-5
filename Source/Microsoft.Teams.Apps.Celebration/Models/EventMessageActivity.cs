// <copyright file="EventMessageActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Models
{
    using System;

    /// <summary>
    /// Represents an activity to construct the celebration card.
    /// </summary>
    public class EventMessageActivity
    {
        /// <summary>
        /// Gets or sets id, which is Id(Guid) in events collection
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets user AAD object id.
        /// </summary>
        public string OwnerAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets event's owner name
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets event's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets event's Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets event's Image URL
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets conversation Id that is required to initiate the conversation between user/team and bot.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or Sets event Date.
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Gets or sets service URL,required to instantiate connector service.
        /// </summary>
        public string ServiceUrl { get; set; }
    }
}