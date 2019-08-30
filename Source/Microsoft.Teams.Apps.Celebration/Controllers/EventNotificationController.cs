// <copyright file="EventNotificationController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.Celebration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Models.Enums;
    using Microsoft.Teams.Apps.Celebration.Utilities;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;

    /// <summary>
    /// Controller to send event notification.
    /// </summary>
    [BotAuthentication]
    public class EventNotificationController : ApiController
    {
        private readonly EventHelper eventHelper;
        private readonly UserManagementHelper userManagementHelper;
        private readonly ILogProvider logProvider;

        private ConnectorServiceHelper connectorServiceHelper;
        private Dictionary<string, List<string>> teamsEventsDictionary;
        private List<EventOccurrence> recurringEvents;
        private List<CelebrationEvent> celebrationEvents;
        private List<User> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventNotificationController"/> class.
        /// </summary>
        /// <param name="eventHelper">EventHelper instance.</param>
        /// <param name="userManagementHelper">UserManagementHelper Instance.</param>
        /// <param name="logProvider">The instance of <see cref="ILogProvider"/></param>
        public EventNotificationController(EventHelper eventHelper, UserManagementHelper userManagementHelper, ILogProvider logProvider)
        {
            this.eventHelper = eventHelper;
            this.userManagementHelper = userManagementHelper;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Process and send event notification in teams
        /// </summary>
        /// /// <param name="currentDateTime">Current dateTime</param>
        /// <returns>A <see cref="Task"/>Representing the asynchronous operation</returns>
        public async Task<HttpResponseMessage> Post(string currentDateTime = "")
        {
            this.logProvider.LogInfo($"Processing events to send the event card in team. CurrentDateTime:{currentDateTime}");
            DateTimeOffset currentDateTimeOffset;
            if (!DateTimeOffset.TryParse(currentDateTime, null, DateTimeStyles.AdjustToUniversal, out currentDateTimeOffset))
            {
                currentDateTimeOffset = DateTimeOffset.UtcNow;
            }

            this.recurringEvents = await (await this.eventHelper.GetRecurringEventsToSendNotificationAsync(currentDateTimeOffset.DateTime)).ToListAsync();
            this.logProvider.LogInfo($"found {this.recurringEvents.Count} to share with team.");
            if (this.recurringEvents.Count > 0)
            {
                List<string> eventIds = this.recurringEvents.Select(x => x.EventId).ToList();
                this.logProvider.LogInfo($"found {this.recurringEvents.Count} to share with team.eventIds:" + string.Join(",", eventIds));

                this.celebrationEvents = await this.eventHelper.GetEventsByEventIdsAsync(eventIds);
                if (this.celebrationEvents.Count > 0)
                {
                    this.users = await this.userManagementHelper.GetUsersByAadObjectIdsAsync(this.celebrationEvents.Select(x => x.OwnerAadObjectId).ToList());
                    this.connectorServiceHelper = new ConnectorServiceHelper(this.CreateConnectorClient(this.users.FirstOrDefault().ServiceUrl), this.logProvider);
                    this.teamsEventsDictionary = new Dictionary<string, List<string>>();

                    foreach (var recurringEvent in this.recurringEvents)
                    {
                        var celebrationEvent = this.celebrationEvents.Where(x => x.Id == recurringEvent.EventId).FirstOrDefault();

                        if (celebrationEvent != null)
                        {
                            foreach (var team in celebrationEvent.Teams)
                            {
                                this.UpdateTeamsEventsDictionary(team.Id, celebrationEvent.Id);
                            }
                        }
                    }

                    await this.ProcessEvents();
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private static void AddMentionedEntities(List<Entity> entities, EventNotificationCardPayload notificationPayload)
        {
            bool ifEntityExits = entities.Any(x => x.GetAs<Mention>().Mentioned.Id == notificationPayload.UserTeamsId);
            if (!ifEntityExits)
            {
                var entity = new Entity();
                entity.SetAs(new Mention()
                {
                    Text = $"<at>{notificationPayload.UserName}</at>",
                    Mentioned = new ChannelAccount()
                    {
                        Name = notificationPayload.UserName,
                        Id = notificationPayload.UserTeamsId,
                        Role = "link",
                    },
                    Type = "mention",
                });
                entities.Add(entity);
            }
        }

        /// <summary>
        /// Create Connector client.
        /// </summary>
        /// <param name="serviceUrl">Service URL to initiate the connection.</param>
        private IConnectorClient CreateConnectorClient(string serviceUrl)
        {
            MicrosoftAppCredentials.TrustServiceUrl(serviceUrl, DateTime.MaxValue);
            return new ConnectorClient(new Uri(serviceUrl));
        }

        /// <summary>
        /// Process and send all the due events in a team.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ProcessEvents()
        {
            foreach (var teamId in this.teamsEventsDictionary.Keys)
            {
                List<string> eventIds = new List<string>();
                this.teamsEventsDictionary.TryGetValue(teamId, out eventIds);
                List<Attachment> cardAttachments = new List<Attachment>();
                List<EventMessage> eventMessages = new List<EventMessage>();
                const int maxEventPerCarousel = 6;
                const int minEventToSendIndividually = 3;
                int counter = 0;
                List<EventNotificationCardPayload> eventNotificationCardPayloadList = new List<EventNotificationCardPayload>();
                List<Entity> entities = new List<Entity>();
                foreach (var eventId in eventIds)
                {
                    counter++;
                    var recurringEvent = this.recurringEvents.Where(x => x.EventId == eventId).FirstOrDefault();
                    var celebrationEvent = this.celebrationEvents.Where(x => x.Id == eventId).FirstOrDefault();

                    // Get event owner information.
                    var user = this.users.Where(x => x.AadObjectId == celebrationEvent.OwnerAadObjectId).FirstOrDefault();

                    // Add an Entry to Messages collection.
                    EventMessage eventMessage = await this.AddEntryToEventMessagesCollection(teamId, recurringEvent, celebrationEvent, user);
                    eventMessages.Add(eventMessage);

                    // Get Hero card for event.
                    HeroCard card = CelebrationCard.GetEventCard(eventMessage.Activity);

                    EventNotificationCardPayload eventNotificationCardPayload = new EventNotificationCardPayload()
                    {
                        UserName = user.UserName,
                        UserTeamsId = user.TeamsId,
                        Message = $"<at>{user.UserName}</at> is celebrating {celebrationEvent.Title}",
                        Attachment = card.ToAttachment(),
                    };

                    eventNotificationCardPayloadList.Add(eventNotificationCardPayload);

                    if (eventIds.Count > minEventToSendIndividually && (counter % maxEventPerCarousel == 0 || counter == eventIds.Count))
                    {
                        eventNotificationCardPayloadList = eventNotificationCardPayloadList.OrderBy(x => x.UserName).ToList();

                        string message = "Stop the presses! Today ";
                        foreach (var notificationPayload in eventNotificationCardPayloadList)
                        {
                            message = message + notificationPayload.Message + ",";
                            cardAttachments.Add(notificationPayload.Attachment);

                            AddMentionedEntities(entities, notificationPayload);
                        }

                        message = message.TrimEnd(',');
                        int position = message.LastIndexOf(',');
                        message = (message.Substring(0, position) + " and " + message.Substring(position + 1)).Replace(",", ", ") + ". That’s a lot of merrymaking for one day—pace yourselves! \n\n";

                        // Do not send separate message in case of 1 event.
                        if (eventNotificationCardPayloadList.Count == 1)
                        {
                            message = string.Empty;
                        }

                        // send event notification in team.
                        this.logProvider.LogInfo("Sending event message in team", new Dictionary<string, string>()
                            {
                                { "EventId", celebrationEvent.Id },
                                { "TeamId", teamId },
                                { "Attachment", cardAttachments.ToString() },
                                { "Message", message },
                            });
                        await this.SendEventCard(message, cardAttachments, teamId, eventMessages, entities);

                        // Reset list
                        cardAttachments = new List<Attachment>();
                        eventMessages = new List<EventMessage>();
                        eventNotificationCardPayloadList = new List<EventNotificationCardPayload>();
                        entities = new List<Entity>();
                    }
                    else if (eventIds.Count <= minEventToSendIndividually)
                    {
                        this.logProvider.LogInfo("Sending event message in team", new Dictionary<string, string>()
                        {
                            { "EventId", celebrationEvent.Id },
                            { "TeamId", teamId },
                            { "Attachment", cardAttachments.ToString() },
                            { "Message", string.Empty },
                        });
                        await this.SendEventCard(string.Empty, new List<Attachment> { eventNotificationCardPayload.Attachment }, teamId, eventMessages);

                        // Reset list
                        cardAttachments = new List<Attachment>();
                        eventMessages = new List<EventMessage>();
                        eventNotificationCardPayloadList = new List<EventNotificationCardPayload>();
                        entities = new List<Entity>();
                    }
                }
            }

            // Delete entry from occurrences collection.
            foreach (var recurringEvent in this.recurringEvents)
            {
                this.logProvider.LogInfo("Deleting recurring event", new Dictionary<string, string>()
                {
                    { "EventId", recurringEvent.EventId },
                    { "RecurringEventId", recurringEvent.Id },
                });
                await this.eventHelper.DeleteRecurringEventAsync(recurringEvent.Id, recurringEvent.EventId);
            }
        }

        /// <summary>
        /// Send event card in Teams.
        /// </summary>
        /// <param name="message">message.</param>
        /// <param name="attachments">Attachments list</param>
        /// <param name="teamId">teamId</param>
        /// <param name="eventMessages">List of EventMessage</param>
        /// <param name="entities">entities</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendEventCard(string message, List<Attachment> attachments, string teamId, List<EventMessage> eventMessages, List<Entity> entities = null)
        {
            int lastAttemptStatusCode = (int)HttpStatusCode.OK;
            string responseBody = string.Empty;
            bool isMessageSentSuccessfully = false;
            Exception exception = null;

            try
            {
                await this.connectorServiceHelper.SendMessageInCarouselFormatAsync(message, attachments, teamId, entities);
                isMessageSentSuccessfully = true;
            }
            catch (HttpException httpException)
            {
                lastAttemptStatusCode = httpException.GetHttpCode();
                responseBody = httpException.GetHtmlErrorMessage();
                exception = httpException;
            }
            catch (ErrorResponseException errorResponseException)
            {
                lastAttemptStatusCode = (int)errorResponseException.Response.StatusCode;
                responseBody = errorResponseException.Response.Content.ToString();
                exception = errorResponseException;
                if (errorResponseException.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    await this.userManagementHelper.DeleteTeamDetailsAsync(teamId);
                    await this.userManagementHelper.DeleteUserTeamMembershipByTeamIdAsync(teamId);
                }
            }
            catch (Exception ex)
            {
                lastAttemptStatusCode = (int)HttpStatusCode.BadRequest;
                responseBody = ex.ToString();
            }
            finally
            {
                if (!isMessageSentSuccessfully)
                {
                    foreach (var eventMessage in eventMessages)
                    {
                        this.logProvider.LogError("Failed to send event card.", exception, new Dictionary<string, string>
                        {
                            { "EventId", eventMessage.EventId },
                            { "OccurrenceId", eventMessage.OccurrenceId },
                            { "eventActivity", eventMessage.Activity.ToString() },
                            { "LastAttemptStatusCode", lastAttemptStatusCode.ToString() },
                            { "LastAttemptTime", DateTime.UtcNow.ToString() },
                            { "TeamId", teamId },
                        });
                    }
                }

                foreach (var eventMessage in eventMessages)
                {
                    MessageSendResult messageSendResult = new MessageSendResult()
                    {
                        LastAttemptTime = DateTime.Now,
                        StatusCode = lastAttemptStatusCode,
                        ResponseBody = responseBody,
                    };

                    await this.eventHelper.UpdateEventMessageAsync(eventMessage.Id, messageSendResult);
                }
            }
        }

        /// <summary>
        /// Add an entry to EventMessages collection.
        /// </summary>
        /// <param name="conversationId">conversationId</param>
        /// <param name="recurringEvent">EventOccurrence instance</param>
        /// <param name="celebrationEvent">CelebrationEvent instance.</param>
        /// <param name="user">User instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<EventMessage> AddEntryToEventMessagesCollection(string conversationId, EventOccurrence recurringEvent, CelebrationEvent celebrationEvent, User user)
        {
            EventMessage eventMessage = new EventMessage
            {
                OccurrenceId = recurringEvent.Id,
                EventId = celebrationEvent.Id,
                Activity = this.GetEventMessageActivity(celebrationEvent, user, conversationId),
                MessageType = MessageType.Event,
                ExpireAt = recurringEvent.Date.AddHours(12),
            };

            // Add new entry to EventMessages collection for reminder.
            await this.eventHelper.AddEventMessageAsync(eventMessage);

            return eventMessage;
        }

        /// <summary>
        /// Update dictionary that holds events to send in a team.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        private void UpdateTeamsEventsDictionary(string key, string value)
        {
            List<string> outvalue = new List<string>();
            if (this.teamsEventsDictionary.TryGetValue(key, out outvalue))
            {
                outvalue.Add(value);
                this.teamsEventsDictionary[key] = outvalue;
            }
            else
            {
                this.teamsEventsDictionary.Add(key, new List<string> { value });
            }
        }

        /// <summary>
        /// Prepare EventMessageActivity object.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent instance</param>
        /// <param name="user">User instance</param>
        /// <param name="conversationId">ConversationId</param>
        /// <returns>EventMessageActivity instance.</returns>
        private EventMessageActivity GetEventMessageActivity(CelebrationEvent celebrationEvent, User user, string conversationId)
        {
            return new EventMessageActivity
            {
                OwnerName = user.UserName,
                OwnerAadObjectId = celebrationEvent.OwnerAadObjectId,
                Id = celebrationEvent.Id,
                ServiceUrl = user.ServiceUrl,
                Title = celebrationEvent.Title,
                Message = celebrationEvent.Message,
                ImageUrl = celebrationEvent.ImageURL,
                ConversationId = conversationId,
            };
        }
    }
}
