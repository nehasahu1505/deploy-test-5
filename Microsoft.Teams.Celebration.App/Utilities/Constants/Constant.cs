// <copyright file="Constant.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    /// <summary>
    /// Store constants used in project.
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Cosmos DB id to store the celebration bot data.
        /// </summary>
        public const string DatabaseId = "celebrationbotdb";

        /// <summary>
        /// Document db collection id to store user events.
        /// </summary>
        public const string EventsCollectionId = "Events";

        /// <summary>
        /// Document db collection Id to store Team details.
        /// </summary>
        public const string TeamsCollectionId = "Teams";

        /// <summary>
        /// Document db collection id to store User details.
        /// </summary>
        public const string UsersCollectionId = "Users";

        /// <summary>
        /// Documnet db collection id to store UserTeamMembership.
        /// </summary>
        public const string UserTeamMembershipCollectionId = "UserTeamMembership";

        /// <summary>
        /// Default locale for bot.
        /// </summary>
        public const string DefaultLocale = "en-us";

        /// <summary>
        /// maximum no. of events per user.
        /// </summary>
        public const int MaxEventCountPerUser = 5;
    }
}