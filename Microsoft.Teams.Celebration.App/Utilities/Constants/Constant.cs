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
        /// Cosmos DB name to store the celebration bot data.
        /// </summary>
        public const string CelebrationBotDb = "celebrationbotdb";

        /// <summary>
        /// Collection to store user events.
        /// </summary>
        public const string CelebrationBotEventCollection = "Events";

        /// <summary>
        /// Default locale for bot.
        /// </summary>
        public const string DefaultLocale = "en-us";
    }
}