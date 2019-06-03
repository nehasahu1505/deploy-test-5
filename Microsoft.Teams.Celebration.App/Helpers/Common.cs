// <copyright file="Common.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Teams.Celebration.App.Models;

    /// <summary>
    /// Contains common static methods used in project.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Construct the teams botId.
        /// </summary>
        /// <returns>botId with 28:.</returns>
        public static string GetTeamsBotId()
        {
            return "28:" + ApplicationSettings.MicrosoftAppId;
        }

        /// <summary>
        /// Count no. of files in a directory.
        /// </summary>
        /// <param name="directoryPath">directory path.</param>
        /// <returns>file count.</returns>
        public static int GetCountOfFilesInDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath) ? Directory.GetFiles(directoryPath).Length : 0;
        }

        /// <summary>
        /// Returns IEnumerable of TimeZoneDisplayInfo.
        /// </summary>
        /// <returns>IEnumerable of TimeZoneDisplayInfo.</returns>
        public static IEnumerable<TimeZoneDisplayInfo> GetTimeZoneList()
        {
            var timeZonelist = new List<TimeZoneDisplayInfo>();
            foreach (TimeZoneInfo info in TimeZoneInfo.GetSystemTimeZones())
            {
                timeZonelist.Add(new TimeZoneDisplayInfo { TimeZoneDisplayName = info.DisplayName, TimeZoneId = info.Id });
            }

            return timeZonelist;
        }
    }
}