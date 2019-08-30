// <copyright file="ApplicationSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Helpers
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Store Application settings.
    /// </summary>
    public static class ApplicationSettings
    {
        /// <summary>
        /// Initializes static members of the <see cref="ApplicationSettings"/> class.
        /// </summary>
        static ApplicationSettings()
        {
            BaseUrl = ConfigurationManager.AppSettings[ConfigurationKeys.BaseUrl];
            MicrosoftAppId = ConfigurationManager.AppSettings[ConfigurationKeys.MicrosoftAppId];
            MicrosoftAppPassword = ConfigurationManager.AppSettings[ConfigurationKeys.MicrosoftAppPassword];
            CosmosDBEndpointUrl = ConfigurationManager.AppSettings[ConfigurationKeys.CosmosDBEndpointUrl];
            CosmosDBKey = ConfigurationManager.AppSettings[ConfigurationKeys.CosmosDBKey];
            TimeToPostCelebration = ConfigurationManager.AppSettings[ConfigurationKeys.TimeToPostCelebration];
            DeepLinkToTab = ConfigurationManager.AppSettings[ConfigurationKeys.DeepLinkToTab];
            NoOfDaysInAdvanceToNotifyForUpcomingEvents = Convert.ToInt32(ConfigurationManager.AppSettings[ConfigurationKeys.NoOfDaysInAdvanceToNotifyForUpcomingEvents]);
            ManifestAppId = ConfigurationManager.AppSettings[ConfigurationKeys.ManifestAppId];
        }

        /// <summary>
        /// Gets or sets base url.
        /// </summary>
        public static string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets MicrosoftAppId of bot.
        /// </summary>
        public static string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets MicrosoftAppId of bot.
        /// </summary>
        public static string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Gets or sets Cosmos Db Endpoint url.
        /// </summary>
        public static string CosmosDBEndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets Cosmos Db Key.
        /// </summary>
        public static string CosmosDBKey { get; set; }

        /// <summary>
        /// Gets or sets time to post the celebration in team.
        /// </summary>
        public static string TimeToPostCelebration { get; set; }

        /// <summary>
        /// Gets or sets deep link to Tab.
        /// </summary>
        public static string DeepLinkToTab { get; set; }

        /// <summary>
        /// Gets or sets No. of days in advance to notify for upcoming event.
        /// </summary>
        public static int NoOfDaysInAdvanceToNotifyForUpcomingEvents { get; set; }

        /// <summary>
        /// Gets manifest id which is Guid.
        /// </summary>
        public static string ManifestAppId { get; }
    }
}