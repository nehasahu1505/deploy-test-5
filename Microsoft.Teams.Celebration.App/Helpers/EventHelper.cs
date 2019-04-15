// <copyright file="EventHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Helpers
{
    using System;
    using System.Linq;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Teams.Celebration.App.Models;

    /// <summary>
    /// Helper class for Events.
    /// </summary>
    public static class EventHelper
    {
        private static Uri documentCollectionUri;
        private static DocumentClient documentClient;

        static EventHelper()
        {
            InitializeDocumentClient();
            InitializeDocumentCollectionUri();
        }

        /// <summary>
        /// Returns DocumentQuery for Events.
        /// </summary>
        /// <param name="aadObjectId">AadUserObjectId.</param>
        /// <returns>List of TeamEvents.</returns>
        public static IDocumentQuery<CelebrationEvent> GetEventsbyOwnerObjectId(string aadObjectId)
        {
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            return documentClient.CreateDocumentQuery<CelebrationEvent>(documentCollectionUri, option)
                .Where(x => x.OwnerAadObjectId == aadObjectId).AsDocumentQuery();
        }

        private static void InitializeDocumentClient()
        {
            var uri = new Uri(ApplicationSettings.DocumentDbUrl);
            var key = ApplicationSettings.DocumentDbKey;
            documentClient = new DocumentClient(uri, key);
        }

        private static void InitializeDocumentCollectionUri()
        {
            documentCollectionUri = UriFactory.CreateDocumentCollectionUri(Constant.CelebrationBotDb, Constant.CelebrationBotEventCollection);
        }
    }
}