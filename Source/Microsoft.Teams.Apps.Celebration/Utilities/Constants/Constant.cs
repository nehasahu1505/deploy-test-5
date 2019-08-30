// <copyright file="Constant.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration
{
    /// <summary>
    /// Store constants used in project.
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Cosmos DB id to store the celebration bot data.
        /// </summary>
      public const string DatabaseId = "CelebrationBotDb";

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
        /// Document db collection id to store UserTeamMembership.
        /// </summary>
      public const string UserTeamMembershipCollectionId = "UserTeamMembership";

        /// <summary>
        /// Document DB collection id to store the upcoming recurring events.
        /// </summary>
      public const string OccurrencesCollectionId = "Occurrences";

        /// <summary>
        /// Document DB collection id to store the message to be sent in team or user.
        /// </summary>
      public const string EventMessagesCollectionId = "EventMessages";

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