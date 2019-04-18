// <copyright file="ManageEventModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    using System.Collections.Generic;
    using Microsoft.Bot.Connector.Teams.Models;

    /// <summary>
    /// Model for ManageEvent view.
    /// </summary>
    public class ManageEventModel
    {
        /// <summary>
        /// Gets or sets CelebrationEvent.
        /// </summary>
        public CelebrationEvent CelebrationEvent { get; set; }

        /// <summary>
        /// Gets or sets TeamDetails.
        /// </summary>
        public IEnumerable<TeamDetails> TeamDetails { get; set; }

        /// <summary>
        /// Gets or sets list of windows timezones, from TimeZonIfo Api.
        /// </summary>
        public IEnumerable<TimeZoneDisplayInfo> TimeZonelist { get; set; }
    }
}