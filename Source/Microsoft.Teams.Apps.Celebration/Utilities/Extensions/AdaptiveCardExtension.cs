// <copyright file="AdaptiveCardExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Helpers
{
    using AdaptiveCards;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// Store extension method for Adaptive card.
    /// </summary>
    public static class AdaptiveCardExtension
    {
        /// <summary>
        /// AdaptiveCard instance.
        /// </summary>
        /// <param name="card">adaptive card.</param>
        /// <returns>Attachment.</returns>
        public static Attachment ToAttachment(this AdaptiveCard card)
        {
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }
    }
}