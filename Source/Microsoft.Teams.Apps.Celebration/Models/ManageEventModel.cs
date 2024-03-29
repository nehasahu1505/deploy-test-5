﻿// <copyright file="ManageEventModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Models
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
        public IEnumerable<Team> TeamDetails { get; set; }

        /// <summary>
        /// Gets or sets list of windows timezones, from TimeZonInfo Api.
        /// </summary>
        public IEnumerable<TimeZoneDisplayInfo> TimeZoneList { get; set; }

        /// <summary>
        /// Gets or sets selected time zone id
        /// </summary>
        public string SelectedTimeZoneId { get; set; }
    }
}