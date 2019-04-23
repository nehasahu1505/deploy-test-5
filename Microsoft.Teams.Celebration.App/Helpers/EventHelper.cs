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
        /// <param name="ownerAadObjectId">AadUserObjectId.</param>
        /// <returns>DocumentQuery for Events.</returns>
        public static IDocumentQuery<CelebrationEvent> GetEventsbyOwnerObjectId(string ownerAadObjectId)
        {
            var option = new FeedOptions { PartitionKey = new PartitionKey(ownerAadObjectId) };
            return documentClient.CreateDocumentQuery<CelebrationEvent>(documentCollectionUri, option)
                .Where(x => x.OwnerAadObjectId == ownerAadObjectId).AsDocumentQuery();
        }

        /// <summary>
        /// Get CelebrationEvent by eventId.
        /// </summary>
        /// <param name="eventId">event Id.</param>
        /// <param name="ownerAadObjectId">AadObjectId of owner.</param>
        /// <returns>CelebrationEvent object.</returns>
        public static async Task<CelebrationEvent> GetTeamEventByEventId(string eventId, string ownerAadObjectId)
        {
            var option = new FeedOptions { PartitionKey = new PartitionKey(ownerAadObjectId) };
            return (await documentClient.CreateDocumentQuery<CelebrationEvent>(documentCollectionUri, option).Where(x => x.Id.ToString() == eventId)
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
            var document = await GetEventbyEventId(celebrationEvent.Id.ToString());

            if (document != null)
            {
                Document updated = await documentClient.ReplaceDocumentAsync(document.SelfLink, celebrationEvent);
            }
        }

        /// <summary>
        /// Delete Event.
        /// </summary>
        /// <param name="eventId">Event Id.</param>
        /// <param name="ownerAadObjectId">Aad object id of owner.</param>
        /// <returns>Task.</returns>
        public static async Task DeleteEvent(string eventId, string ownerAadObjectId)
        {
            var eventDocument = await GetEventbyEventId(eventId);
            await documentClient.DeleteDocumentAsync(eventDocument.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(ownerAadObjectId) });
        }

        private static async Task<CelebrationEvent> GetEventbyEventId(string eventId)
        {
            return (await documentClient.CreateDocumentQuery<CelebrationEvent>(documentCollectionUri).Where(x => x.Id.ToString() == eventId)
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