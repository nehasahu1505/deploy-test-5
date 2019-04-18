// <copyright file="TimeZoneDisplayInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Models
{
    /// <summary>
    /// Store windows TimeZone list.
    /// </summary>
    public class TimeZoneDisplayInfo
    {
        /// <summary>
        /// Gets or sets timeZone display name.
        /// </summary>
        public string TimeZoneDisplayName { get; set; }

        /// <summary>
        /// Gets or sets timeZoneId.
        /// </summary>
        public string TimeZoneId { get; set; }
    }
}