// <copyright file="EventHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Models.Enums;
    using Microsoft.Teams.Apps.Celebration.Utilities;

    /// <summary>
    /// Helper class for CelebrationEvent.
    /// </summary>
    public class EventHelper
    {
        // Request the minimum throughput by default
        private const int DefaultRequestThroughput = 400;

        private readonly TelemetryClient telemetryClient;
        private readonly Lazy<Task> initializeTask;
        private DocumentClient documentClient;
        private Database database;

        private DocumentCollection eventsCollection;
        private DocumentCollection occurencesCollection;
        private DocumentCollection eventMessagesCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHelper"/> class.
        /// </summary>
        /// <param name="telemetryClient">TelemetryClient instance</param>
        public EventHelper(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
            this.initializeTask = new Lazy<Task>(() => this.IntializeDatabaseAsync());
        }

        /// <summary>
        /// Returns DocumentQuery for events.
        /// </summary>
        /// <returns>DocumentQuery for events.</returns>
        public async Task<IDocumentQuery<CelebrationEvent>> GetAllCelebrationEventsAsync()
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return this.documentClient.CreateDocumentQuery<CelebrationEvent>(this.eventsCollection.SelfLink, options).AsDocumentQuery();
        }

        /// <summary>
        /// Returns DocumentQuery for Events.
        /// </summary>
        /// <param name="aadObjectId">AadObjectId of owner.</param>
        /// <returns>DocumentQuery for Events.</returns>
        public async Task<IDocumentQuery<CelebrationEvent>> GetEventsByOwnerObjectIdAsync(string aadObjectId)
        {
            await this.EnsureInitializedAsync();

            return this.documentClient.CreateDocumentQuery<CelebrationEvent>(this.eventsCollection.SelfLink)
                .Where(x => x.OwnerAadObjectId == aadObjectId).AsDocumentQuery();
        }

        /// <summary>
        /// Returns DocumentQuery for events.
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns>DocumentQuery for events.</returns>
        public async Task<IDocumentQuery<CelebrationEvent>> GetCelebrationEventsAsync(string query)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return this.documentClient.CreateDocumentQuery<CelebrationEvent>(this.eventsCollection.SelfLink, query, options).AsDocumentQuery();
        }

        /// <summary>
        /// Get CelebrationEvent by eventId.
        /// </summary>
        /// <param name="eventId">event Id.</param>
        /// <param name="ownerAadObjectId">AadObjectId of owner.</param>
        /// <returns>CelebrationEvent object.</returns>
        public async Task<CelebrationEvent> GetEventByEventIdAsync(string eventId, string ownerAadObjectId)
        {
            try
            {
                await this.EnsureInitializedAsync();
                var options = new RequestOptions { PartitionKey = new PartitionKey(ownerAadObjectId) };
                var documentUri = UriFactory.CreateDocumentUri(this.database.Id, this.eventsCollection.Id, eventId);

                return await this.documentClient.ReadDocumentAsync<CelebrationEvent>(documentUri, options);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("Entity with the specified id does not exist in the system."))
                {
                    this.telemetryClient.TrackTrace(
                                                    string.Format("Document with the event id '{0}' does not exist in Events collection.", eventId),
                                                    SeverityLevel.Information);
                }

                return null;
            }
        }

        /// <summary>
        /// Get CelebrationEvents by eventId.
        /// </summary>
        /// <param name="eventIds">List of event id.</param>
        /// <returns>CelebrationEvent object.</returns>
        public async Task<List<CelebrationEvent>> GetEventsByEventIdsAsync(List<string> eventIds)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return await this.documentClient.CreateDocumentQuery<CelebrationEvent>(this.eventsCollection.SelfLink, options).Where(x => eventIds.Contains(x.Id))
                         .AsDocumentQuery().ToListAsync();
        }

        /// <summary>
        /// Save event.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveEventAsync(CelebrationEvent celebrationEvent)
        {
            await this.EnsureInitializedAsync();

            await this.documentClient.UpsertDocumentAsync(this.eventsCollection.SelfLink, celebrationEvent);
        }

        /// <summary>
        /// Update event.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventAsync(CelebrationEvent celebrationEvent)
        {
            await this.EnsureInitializedAsync();

            await this.documentClient.ReplaceDocumentAsync(celebrationEvent.SelfLink, celebrationEvent);
        }

        /// <summary>
        /// Delete Event.
        /// </summary>
        /// <param name="eventId">Event Id.</param>
        /// <param name="ownerAadObjectId">AadObject id of owner.</param>
        /// <returns>Task.</returns>
        public async Task DeleteEventAsync(string eventId, string ownerAadObjectId)
        {
            await this.EnsureInitializedAsync();
            var document = await this.GetEventByEventIdAsync(eventId, ownerAadObjectId);

            if (document != null)
            {
                await this.documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(ownerAadObjectId) });
            }
        }

        /// <summary>
        /// Get recurring events.
        /// </summary>
        /// <param name="eventIds">List of event id.</param>
        /// <returns>EventOccurrence DocumentQuery.</returns>
        public async Task<IDocumentQuery<EventOccurrence>> GetRecurringEventsAsync(List<string> eventIds)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return this.documentClient.CreateDocumentQuery<EventOccurrence>(this.occurencesCollection.SelfLink, options)
                .Where(x => eventIds.Contains(x.EventId)).AsDocumentQuery();
        }

        /// <summary>
        /// Get recurring events
        /// </summary>
        /// <param name="currentDateTime">represents current dateTime instance</param>
        /// <returns>EventOccurrence DocumentQuery.</returns>
        public async Task<IDocumentQuery<EventOccurrence>> GetRecurringEventsToSendNotificationAsync(DateTime currentDateTime)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            var currentDateTimeInUtc = new DateTimeOffset(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, currentDateTime.Minute, 0, new TimeSpan(0, 0, 0));

            return this.documentClient.CreateDocumentQuery<EventOccurrence>(this.occurencesCollection.SelfLink, options)
                .Where(x => x.Status == EventStatus.Default && x.Date == currentDateTimeInUtc).AsDocumentQuery();
        }

        /// <summary>
        /// Add recurring event.
        /// </summary>
        /// <param name="recurringEvent">EventOccurrence instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddRecurringEventAsync(EventOccurrence recurringEvent)
        {
            await this.EnsureInitializedAsync();

            await this.documentClient.CreateDocumentAsync(this.occurencesCollection.SelfLink, recurringEvent);
        }

        /// <summary>
        /// update recurring event.
        /// </summary>
        /// <param name="eventId">eventId.</param>
        /// <param name="eventStatus">event status.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateRecurringEventAsync(string eventId, EventStatus eventStatus)
        {
            await this.EnsureInitializedAsync();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            var document = this.documentClient.CreateDocumentQuery<EventOccurrence>(this.occurencesCollection.SelfLink, options)
                          .Where(x => x.EventId == eventId).AsEnumerable().FirstOrDefault();

            if (document != null)
            {
                EventOccurrence eventOccurrence = (dynamic)document;
                eventOccurrence.Status = eventStatus;
                await this.documentClient.ReplaceDocumentAsync(document.SelfLink, eventOccurrence);
            }
        }

        /// <summary>
        /// Delete recurring event
        /// </summary>
        /// <param name="id">unique id.</param>
        /// <param name="eventId">eventId.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteRecurringEventAsync(string id, string eventId)
        {
            await this.EnsureInitializedAsync();

            var documentUri = UriFactory.CreateDocumentUri(this.database.Id, this.occurencesCollection.Id, id);
            var options = new RequestOptions { PartitionKey = new PartitionKey(eventId) };
            Document document = null;
            try
            {
                document = await this.documentClient.ReadDocumentAsync(documentUri, options);
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("Entity with the specified id does not exist in the system."))
                {
                    this.telemetryClient.TrackTrace(
                                                    string.Format("Document with the event id '{0}' does not exist in Events collection.", eventId),
                                                    SeverityLevel.Error);
                }
            }

            if (document != null)
            {
                await this.documentClient.DeleteDocumentAsync(documentUri, options);
            }
        }

        /// <summary>
        /// Delete Recurring event for given eventId.
        /// </summary>
        /// <param name="eventId">Event Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteRecurringEventAsync(string eventId)
        {
            await this.EnsureInitializedAsync();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            var document = this.documentClient.CreateDocumentQuery<EventOccurrence>(this.occurencesCollection.SelfLink, options)
                          .Where(x => x.EventId == eventId).AsEnumerable().FirstOrDefault();

            if (document != null)
            {
               await this.documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(eventId) });
            }
        }

        /// <summary>
        /// Get EventMessages by StatusCode.
        /// </summary>
        /// <param name="statusCode">HTTP Status code</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<EventMessage>> GetEventMessagesByEventStatus(List<int> statusCode)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return await this.documentClient.CreateDocumentQuery<EventMessage>(this.eventMessagesCollection.SelfLink, options)
                             .Where(x => statusCode.Contains(x.MessageSendResult.StatusCode)).AsDocumentQuery().ToListAsync();
        }

        /// <summary>
        /// Add message to send preview/event card.
        /// </summary>
        /// <param name="eventMessage">EventMessage instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddEventMessageAsync(EventMessage eventMessage)
        {
            await this.EnsureInitializedAsync();

            await this.documentClient.CreateDocumentAsync(this.eventMessagesCollection.SelfLink, eventMessage);
        }

        /// <summary>
        /// update last message send result.
        /// </summary>
        /// <param name="id">Id to uniquely identify the record.</param>
        /// <param name="messageSendResult">MessageSendResult instance</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateEventMessageAsync(string id, MessageSendResult messageSendResult)
        {
            await this.EnsureInitializedAsync();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            var document = this.documentClient.CreateDocumentQuery<EventMessage>(this.eventMessagesCollection.SelfLink, options)
                          .Where(x => x.Id == id).AsEnumerable().FirstOrDefault();

            if (document != null)
            {
                EventMessage eventMessage = (dynamic)document;
                eventMessage.MessageSendResult = messageSendResult;
                await this.documentClient.ReplaceDocumentAsync(document.SelfLink, eventMessage);
            }
        }

        /// <summary>
        /// Delete record from EventMessages collection for given eventId.
        /// </summary>
        /// <param name="eventId">event Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteEventMessagesAsync(string eventId)
        {
            await this.EnsureInitializedAsync();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            var document = this.documentClient.CreateDocumentQuery<EventMessage>(this.eventMessagesCollection.SelfLink, options)
                          .Where(x => x.EventId == eventId).AsEnumerable().FirstOrDefault();

            if (document != null)
            {
              await this.documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(eventId) });
            }
        }

        /// <summary>
        /// Delete Expired messages
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteExpiredMessagesAsync()
        {
            await this.EnsureInitializedAsync();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            var documents = this.documentClient.CreateDocumentQuery<EventMessage>(this.eventMessagesCollection.SelfLink, options)
                          .Where(x => x.ExpireAt < DateTimeOffset.UtcNow).AsEnumerable();

            foreach (var document in documents)
            {
                await this.documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(document.EventId) });
            }
        }

        private async Task IntializeDatabaseAsync()
        {
            this.telemetryClient.TrackTrace("Initializing data store");

            var documentDbEndpointUrl = new Uri(ApplicationSettings.CosmosDBEndpointUrl);
            var documentDbPrimaryKey = ApplicationSettings.CosmosDBKey;
            this.documentClient = new DocumentClient(documentDbEndpointUrl, documentDbPrimaryKey);

            var requestOptions = new RequestOptions { OfferThroughput = DefaultRequestThroughput };
            bool useSharedOffer = true;

            // Create the database if needed
            try
            {
                this.database = await this.documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = Constant.DatabaseId }, requestOptions);
            }
            catch (DocumentClientException ex)
            {
                if (ex.Error?.Message?.Contains("SharedOffer is Disabled") ?? false)
                {
                    this.telemetryClient.TrackTrace("Database shared offer is disabled for the account, will provision throughput at container level", SeverityLevel.Information);
                    useSharedOffer = false;

                    this.database = await this.documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = Constant.DatabaseId });
                }
                else
                {
                    throw;
                }
            }

            // Get a reference to the Events collection, creating it if needed
            var eventsCollectionDefinition = new DocumentCollection
            {
                Id = Constant.EventsCollectionId,
            };

            eventsCollectionDefinition.PartitionKey.Paths.Add("/ownerAadObjectId");
            this.eventsCollection = await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(this.database.SelfLink, eventsCollectionDefinition, useSharedOffer ? null : requestOptions);

            // Get a reference to the Occurrences collection, creating it if needed
            var ocurrencesCollectionDefinition = new DocumentCollection
            {
                Id = Constant.OccurrencesCollectionId,
            };

            ocurrencesCollectionDefinition.PartitionKey.Paths.Add("/eventId");
            this.occurencesCollection = await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(this.database.SelfLink, ocurrencesCollectionDefinition, useSharedOffer ? null : requestOptions);

            // Get a reference to the EventMessages collection, creating it if needed
            var eventMessagesCollectionDefinition = new DocumentCollection
            {
                Id = Constant.EventMessagesCollectionId,
            };

            eventMessagesCollectionDefinition.PartitionKey.Paths.Add("/eventId");
            this.eventMessagesCollection = await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(this.database.SelfLink, eventMessagesCollectionDefinition, useSharedOffer ? null : requestOptions);

            this.telemetryClient.TrackTrace("Data store initialized");
        }

        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }
    }
}