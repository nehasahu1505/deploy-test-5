// <copyright file="EventHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Helpers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Teams.Celebration.App.Models;
    using Microsoft.Teams.Celebration.App.Utilities;

    /// <summary>
    /// Helper class for CelebrationEvent.
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
        /// <returns>DocumentQuery for Events.</returns>
        public static IDocumentQuery<CelebrationEvent> GetEventsbyOwnerObjectId(string aadObjectId)
        {
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            return documentClient.CreateDocumentQuery<CelebrationEvent>(documentCollectionUri, option)
                .Where(x => x.OwnerAadObjectId == aadObjectId).AsDocumentQuery();
        }

        /// <summary>
        /// Get CelebrationEvent by eventId.
        /// </summary>
        /// <param name="eventId">event Id.</param>
        /// <returns>CelebrationEvent object.</returns>
        public static async Task<CelebrationEvent> GetTeamEventByEventId(string eventId)
        {
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            return (await documentClient.CreateDocumentQuery<CelebrationEvent>(documentCollectionUri, option).Where(x => x.Id == eventId)
                         .AsDocumentQuery().ToListAsync()).FirstOrDefault();
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