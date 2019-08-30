// <copyright file="ConfigurationKeys.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration
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
        /// Cosmos DB Endpoint Url key.
        /// </summary>
        public const string CosmosDBEndpointUrl = "CosmosDBEndpointUrl";

        /// <summary>
        /// Cosmos Db Key.
        /// </summary>
        public const string CosmosDBKey = "CosmosDBKey";

        /// <summary>
        /// Key that stores the timespan, to post the celebration in team.
        /// </summary>
        public const string TimeToPostCelebration = "TimeToPostCelebration";

        /// <summary>
        /// Deep link to Tab key.
        /// </summary>
        public const string DeepLinkToTab = "DeepLinkToTab";

        /// <summary>
        /// No. of days in advance to notify for upcoming event key.
        /// </summary>
        public const string NoOfDaysInAdvanceToNotifyForUpcomingEvents = "NoOfDaysInAdvanceToNotifyForUpcomingEvents";

        /// <summary>
        /// ManifestAppId key
        /// </summary>
        public const string ManifestAppId = "ManifestAppId";
    }
}