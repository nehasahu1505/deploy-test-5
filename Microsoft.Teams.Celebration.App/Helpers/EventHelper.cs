// <copyright file="EventHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App.Helpers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
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

        /// <summary>
        /// Create new event in document DB.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task CreateNewEventAsync(CelebrationEvent celebrationEvent)
        {
            await documentClient.CreateDocumentAsync(documentCollectionUri, celebrationEvent);
        }

        /// <summary>
        /// Update existing event.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task UpdateEventAsync(CelebrationEvent celebrationEvent)
        {
            var option = new FeedOptions { EnableCrossPartitionQuery = true };
            var document = documentClient.CreateDocumentQuery(documentCollectionUri, option)
                          .Where(x => x.Id == celebrationEvent.Id).AsEnumerable().FirstOrDefault();
            if (document != null)
            {
                Document updated = await documentClient.ReplaceDocumentAsync(document.SelfLink, celebrationEvent);
            }
        }

        /// <summary>
        /// Delete Event.
        /// </summary>
        /// <param name="eventId">Event Id.</param>
        /// <param name="eventType">Event Type.</param>
        /// <returns>Task.</returns>
        public static async Task DeleteEvent(string eventId, string eventType)
        {
            var eventDocument = GetEventbyEventId(eventId);
            await documentClient.DeleteDocumentAsync(eventDocument.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(eventType) });
        }

        private static Document GetEventbyEventId(string eventId)
        {
            return documentClient.CreateDocumentQuery(documentCollectionUri).AsEnumerable().Where(x => x.Id == eventId).FirstOrDefault();
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