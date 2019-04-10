// <copyright file="ApplicationSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Helpers
{
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
            DocumentDbUrl = ConfigurationManager.AppSettings[ConfigurationKeys.DocumentDbUrl];
            DocumentDbKey = ConfigurationManager.AppSettings[ConfigurationKeys.DocumentDbKey];
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
        /// Gets or sets document Db url.
        /// </summary>
        public static string DocumentDbUrl { get; set; }

        /// <summary>
        /// Gets or sets Document Db Key.
        /// </summary>
        public static string DocumentDbKey { get; set; }
    }
}