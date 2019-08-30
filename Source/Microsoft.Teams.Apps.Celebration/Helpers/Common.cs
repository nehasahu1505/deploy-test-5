// <copyright file="Common.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Models;

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

        /// <summary>
        /// GetImage URL.
        /// </summary>
        /// <param name="imageName">Image name.</param>
        /// <returns>image path.</returns>
        public static string GetImagePath(string imageName)
        {
            return ApplicationSettings.BaseUrl + imageName.Split(new string[] { "../.." }, StringSplitOptions.None)[1];
        }

        /// <summary>
        /// Get welcome image.
        /// </summary>
        /// <param name="imageName">Image name.</param>
        /// <returns>image path.</returns>
        public static string GetWelcomeImage(string imageName)
        {
            return ApplicationSettings.BaseUrl + "/Content/Images/" + imageName;
        }

        /// <summary>
        /// Create new activity using conversation id.
        /// </summary>
        /// <param name="conversationId">conversation id.</param>
        /// <returns>Activity instance.</returns>
        public static Activity CreateNewActivity(string conversationId)
        {
            return new Activity()
            {
                Type = ActivityTypes.Message,
                Conversation = new ConversationAccount
                {
                    Id = conversationId,
                },
            };
        }

        /// <summary>
        /// Construct deeplink URL from bot to tab.
        /// </summary>
        /// <param name="entityId">entityId.</param>
        /// <param name="relativeWebUrl">relativeWebUrl.</param>
        /// <param name="context">JavaScript array of context.</param>
        /// <returns>Deep link URL.</returns>
        public static string GetDeepLinkUrlToEventsTab(string entityId, string relativeWebUrl, string context = "")
        {
            if (!string.IsNullOrWhiteSpace(context))
            {
                context = "&context=" + HttpUtility.UrlEncode(context);
            }

            return string.Format(
                                        "{0}/{1}/{2}?webUrl={3}{4}",
                                        ApplicationSettings.DeepLinkToTab,
                                        ApplicationSettings.ManifestAppId,
                                        entityId,
                                        HttpUtility.UrlEncode(ApplicationSettings.BaseUrl + "/" + relativeWebUrl),
                                        context);
        }

        /// <summary>
        /// Compute upcoming event date against reference date.
        /// </summary>
        /// <param name="eventDate">event date.</param>
        /// <param name="referenceDate">reference date.</param>
        /// <returns>upcoming event date.</returns>
        public static DateTime GetUpcomingEventDate(DateTime eventDate, DateTime referenceDate)
        {
            int upcomingEventYear = referenceDate.Date.Year - eventDate.Year;

            if (eventDate.AddYears(upcomingEventYear) < referenceDate)
            {
                upcomingEventYear += 1;
            }

            return eventDate.AddYears(upcomingEventYear);
        }
    }
}