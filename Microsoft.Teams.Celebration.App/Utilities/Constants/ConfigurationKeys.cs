// <copyright file="ConfigurationKeys.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    /// <summary>
    /// store configuration constants.
    /// </summary>
    public static class ConfigurationKeys
    {
        /// <summary>
        /// Application Base url key.
        /// </summary>
        public const string BaseUrl = "BaseUrl";

        /// <summary>
        /// Microsoft AppId Key.
        /// </summary>
        public const string MicrosoftAppId = "MicrosoftAppId";

        /// <summary>
        /// Microsoft App password key.
        /// </summary>
        public const string MicrosoftAppPassword = "MicrosoftAppPassword";

        /// <summary>
        /// Document Db Url key.
        /// </summary>
        public const string DocumentDbUrl = "DocumentDbUrl";

        /// <summary>
        /// Document Db Key.
        /// </summary>
        public const string DocumentDbKey = "DocumentDbKey";

        /// <summary>
        /// Key that stores the timespan, to post the celebration in team.
        /// </summary>
        public const string TimeToPostCelebration = "TimeToPostCelebration";
    }
}