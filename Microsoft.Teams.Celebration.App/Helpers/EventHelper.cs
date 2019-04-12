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
    /// Helper class for TeamEvents.
    /// </summary>
    public static class EventHelper
    {
        private static Uri documentCollectionUri;
        private static DocumentClient documentClient;

        static EventHelper()
        {
            InitializeDocumentclient();
            InitializeDocumentClientUri();
        }

        /// <summary>
        /// Returns the List of TeamEvents based on user object Id.
        /// </summary>
        /// <param name="aadObjectId">AadUserObjectId.</param>
        /// <returns>List of TeamEvents.</returns>
        public static IDocumentQuery<Events> GetEventsbyOwnerObjectId(string aadObjectId)
        {
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            return documentClient.CreateDocumentQuery<Events>(documentCollectionUri, option)
                .Where(x => x.OwnerAadObjectId == aadObjectId).AsDocumentQuery();
        }

        private static void InitializeDocumentclient()
        {
            var uri = new Uri(ApplicationSettings.DocumentDbUrl);
            var key = ApplicationSettings.DocumentDbKey;
            documentClient = new DocumentClient(uri, key);
        }

        private static void InitializeDocumentClientUri()
        {
            documentCollectionUri = UriFactory.CreateDocumentCollectionUri(Constant.CelebrationBotDb, Constant.CelebrationBotEventCollection);
        }
    }
}