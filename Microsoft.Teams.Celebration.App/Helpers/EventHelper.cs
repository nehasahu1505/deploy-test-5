// <copyright file="EventHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
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
        /// Returns all the events.
        /// </summary>
        /// <returns>List of TeamEvent.</returns>
        public static IEnumerable<Events> GetAllEvents()
        {
            return documentClient.CreateDocumentQuery<Events>(documentCollectionUri);
        }

        /// <summary>
        /// Returns the List of TeamEvents based on user object Id.
        /// </summary>
        /// <param name="aadObjectId">AadUserObjectId.</param>
        /// <returns>List of TeamEvents.</returns>
        public static IEnumerable<Events> GetEventsbyOwnerObjectId(string aadObjectId)
        {
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            return documentClient.CreateDocumentQuery<Events>(documentCollectionUri, option)
                .Where(x => x.OwnerAadObjectId == aadObjectId);
        }

        private static Document GetEventbyEventId(string eventId)
        {
            return documentClient.CreateDocumentQuery(documentCollectionUri).AsEnumerable().Where(x => x.Id == eventId).FirstOrDefault();
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