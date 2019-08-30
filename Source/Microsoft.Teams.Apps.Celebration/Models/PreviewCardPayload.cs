// <copyright file="PreviewCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.Celebration.Models
{
    using System;

    /// <summary>
    /// Preview card payload.
    /// </summary>
    public class PreviewCardPayload : SubmitActionPayload
    {
        /// <summary>
        /// Gets or sets event id.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets OwnerAadObjectId
        /// </summary>
        public string OwnerAadObjectId { get; set; }

        /// <summary>
        /// Gets or sets OwnerName
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets event title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets upcoming event date.
        /// </summary>
        public DateTime UpcomingEventDate { get; set; }
    }
}