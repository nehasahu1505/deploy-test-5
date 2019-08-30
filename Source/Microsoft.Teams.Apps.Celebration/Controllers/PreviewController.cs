// <copyright file="PreviewController.cs" company="Microsoft">
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
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Models.Enums;
    using Microsoft.Teams.Apps.Celebration.Resources;
    using Microsoft.Teams.Apps.Celebration.Utilities;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;

    /// <summary>
    /// Controller that handles the request to send the reminder for upcoming event.
    /// </summary>
    [BotAuthentication]
    public class PreviewController : ApiController
    {
        private readonly EventHelper eventHelper;
        private readonly UserManagementHelper userManagementHelper;
        private readonly ILogProvider logProvider;
        private ConnectorServiceHelper connectorServiceHelper;
        private List<User> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewController"/> class.
        /// </summary>
        /// <param name="eventHelper">EventHelper instance.</param>
        /// <param name="userManagementHelper">UserManagementHelper instance</param>
        /// <param name="logProvider">The instance of <see cref="ILogProvider"/></param>
        public PreviewController(EventHelper eventHelper, UserManagementHelper userManagementHelper, ILogProvider logProvider)
        {
            this.eventHelper = eventHelper;
            this.userManagementHelper = userManagementHelper;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Process request to send preview card
        /// </summary>
        /// <param name="currentDateTime">Current dateTime</param>
        /// <returns>A <see cref="Task"/>Representing the asynchronous operation</returns>
        public async Task<HttpResponseMessage> Post(string currentDateTime = "")
        {
            this.logProvider.LogInfo($"Processing events to send the reminder to owner. CurrentDateTime:{currentDateTime}");

            DateTimeOffset currentDateTimeOffset;
            if (!DateTimeOffset.TryParse(currentDateTime, null, DateTimeStyles.AdjustToUniversal, out currentDateTimeOffset))
            {
                currentDateTimeOffset = DateTimeOffset.UtcNow;
            }

            var events = await (await this.eventHelper.GetCelebrationEventsAsync(GetEventQuery(currentDateTimeOffset.Date))).ToListAsync();
            this.logProvider.LogInfo($"found {events.Count} which are coming in next 72 hours.");
            if (events.Count > 0)
            {
                var existingRecurringEvents = await (await this.eventHelper.GetRecurringEventsAsync(events.Select(x => x.Id).ToList())).ToListAsync();

                this.logProvider.LogInfo($"Found {existingRecurringEvents.Count} for which reminder has already sent");
                int lastAttemptStatusCode = (int)HttpStatusCode.OK;
                string responseBody = string.Empty;

                // remove events which exist in Occurrences collection
                events.RemoveAll(x => existingRecurringEvents.Any(y => y.EventId == x.Id));

                if (events.Count > 0)
                {
                    this.users = await this.userManagementHelper.GetUsersByAadObjectIdsAsync(events.Select(x => x.OwnerAadObjectId).ToList());
                    this.connectorServiceHelper = new ConnectorServiceHelper(this.CreateConnectorClient(this.users.FirstOrDefault().ServiceUrl), this.logProvider);
                }

                // Loop each event and make entry in Occurrences collection to send preview and event card.
                foreach (var celebrationEvent in events)
                {
                    this.logProvider.LogInfo("Processing event to send reminder", new Dictionary<string, string>() { { "EventId", celebrationEvent.Id }, { "UserObjectId", celebrationEvent.OwnerAadObjectId } });

                    // Get event owner information.
                    var user = this.users.Where(x => x.AadObjectId == celebrationEvent.OwnerAadObjectId).FirstOrDefault();

                    // update conversation id if it is null.
                    await this.ModifyUserDetailsAsync(user);

                    DateTime upcomingEventDate = Common.GetUpcomingEventDate(celebrationEvent.Date, currentDateTimeOffset.Date);
                    var timespan = Array.ConvertAll<string, int>(ApplicationSettings.TimeToPostCelebration.Split(':'), Convert.ToInt32);
                    DateTime upcomingEventDateTime = upcomingEventDate.AddHours(timespan[0]).AddMinutes(timespan[1]);
                    DateTimeOffset upcomingEventDateTimeInUTC = TimeZoneInfo.ConvertTimeToUtc(upcomingEventDateTime, TimeZoneInfo.FindSystemTimeZoneById(celebrationEvent.TimeZoneId));

                    // add an entry to Occurrence collection for all the upcoming event.
                    EventOccurrence eventOccurrence = new EventOccurrence
                    {
                        EventId = celebrationEvent.Id,
                        Date = upcomingEventDateTimeInUTC,
                    };

                    await this.eventHelper.AddRecurringEventAsync(eventOccurrence);

                    // Do not send reminder if event is today.
                    if (upcomingEventDate != currentDateTimeOffset.Date)
                    {
                        // Add new entry to EventMessages collection for reminder.
                        EventMessage eventMessage = new EventMessage
                        {
                            OccurrenceId = eventOccurrence.Id,
                            EventId = celebrationEvent.Id,
                            Activity = this.GetEventMessageActivity(celebrationEvent, user),
                            MessageType = MessageType.Preview,
                            ExpireAt = upcomingEventDate.AddHours(24),
                        };

                        await this.eventHelper.AddEventMessageAsync(eventMessage);

                        bool isMessageSentSuccessfully = false;
                        Exception exception = null;

                        try
                        {
                            HeroCard previewCard = CelebrationCard.GetPreviewCard(eventMessage.Activity);

                            string message = string.Format(Strings.PreviewText, user.UserName);

                            this.logProvider.LogInfo("Sending reminder message to the owner of the event", new Dictionary<string, string>()
                            {
                                { "EventId", celebrationEvent.Id },
                                { "Attachment", Newtonsoft.Json.JsonConvert.SerializeObject(previewCard) },
                                { "Message", message },
                            });

                            // Send reminder of event to owner.
                            await this.connectorServiceHelper.SendPersonalMessageAsync(
                                                                message,
                                                                new List<Attachment> { previewCard.ToAttachment() },
                                                                user.ConversationId);

                            this.logProvider.LogInfo($"Reminder message sent to the owner of the event. EventId: {celebrationEvent.Id}");
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
                                this.logProvider.LogError("Failed to send reminder for upcoming event.", exception, new Dictionary<string, string>
                            {
                                { "EventId", eventMessage.EventId },
                                { "OccurrenceId", eventMessage.OccurrenceId },
                                { "eventActivity", eventMessage.Activity.ToString() },
                                { "LastAttemptStatusCode", lastAttemptStatusCode.ToString() },
                                { "LastAttemptTime", DateTime.UtcNow.ToString() },
                                { "ConversationId", user.ConversationId },
                            });
                            }

                            MessageSendResult messageSendResult = new MessageSendResult()
                            {
                                LastAttemptTime = DateTime.UtcNow,
                                StatusCode = lastAttemptStatusCode,
                                ResponseBody = responseBody,
                            };

                            await this.eventHelper.UpdateEventMessageAsync(eventMessage.Id, messageSendResult);
                        }
                    }
                    else
                    {
                        this.logProvider.LogInfo("Not sending reminder for this event as its upcoming event date is today.");
                    }
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Prepare and return query to events collection to get upcoming events.
        /// </summary>
        /// <returns>SQL query</returns>
        private static string GetEventQuery(DateTime currentDateTime)
        {
            StringBuilder eventQuery = new StringBuilder("select * from Events Where ");

            foreach (var reference in GetReferenceDateCollection(currentDateTime))
            {
                eventQuery = eventQuery.AppendFormat("(Events.eventMonth = {0} and Events.eventDay ={1})", reference.Item2, reference.Item3);
                eventQuery = eventQuery.Append(" or ");
            }

            return eventQuery.Remove(eventQuery.Length - 4, 3).ToString();
        }

        /// <summary>
        /// Reference set to get the upcoming events.
        /// </summary>
        /// <returns>Tuple that contains reference set of month and day part of date.</returns>
        private static List<Tuple<int, int, int>> GetReferenceDateCollection(DateTime currentDateTime)
        {
            DateTimeOffset potentialReferenceDate = currentDateTime;
            List<Tuple<int, int, int>> monthDayReferenceSet = new List<Tuple<int, int, int>>();

            // Add month and day part in reference set for events which are coming in next {NoOfDaysInAdvanceToNotifyForUpcomingEvents} days.
            for (int i = 0; i < ApplicationSettings.NoOfDaysInAdvanceToNotifyForUpcomingEvents; i++)
            {
                potentialReferenceDate = currentDateTime.AddDays(i);
                monthDayReferenceSet.Add(Tuple.Create(i, potentialReferenceDate.Month, potentialReferenceDate.Day));
            }

            // Add 29th Feb in reference set if the year is not leap year.
            if (!DateTime.IsLeapYear(currentDateTime.Year)
                && currentDateTime.Month == 2
                && currentDateTime.Day <= 29
                && 29 - currentDateTime.Day < ApplicationSettings.NoOfDaysInAdvanceToNotifyForUpcomingEvents)
            {
                monthDayReferenceSet.Add(Tuple.Create(ApplicationSettings.NoOfDaysInAdvanceToNotifyForUpcomingEvents, 2, 29));
            }

            return monthDayReferenceSet;
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
        /// Prepare EventMessageActivity object.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent instance</param>
        /// <param name="user">User instance.</param>
        /// <returns>EventMessageActivity instance.</returns>
        private EventMessageActivity GetEventMessageActivity(CelebrationEvent celebrationEvent, User user)
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
                ConversationId = user.ConversationId,
                EventDate = celebrationEvent.Date,
            };
        }

        /// <summary>
        /// Update conversation id of user if it does not exist.
        /// </summary>
        /// <param name="user">User instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task ModifyUserDetailsAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.ConversationId))
            {
                this.logProvider.LogInfo($"Create or get conversationId using connectorclient as conversation id is blank for userId: {user.Id}, Name: {user.UserName}.");
                user.ConversationId = this.connectorServiceHelper.CreateOrGetConversationIdAsync(user.TenantId, user.TeamsId);
                await this.userManagementHelper.SaveUserAsync(user);
            }
        }
    }
}
