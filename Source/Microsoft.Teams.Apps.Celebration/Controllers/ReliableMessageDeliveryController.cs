// <copyright file="ReliableMessageDeliveryController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Controllers
{
    using System;
    using System.Collections.Generic;
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
    /// Controller that implements reliable message delivery.
    /// </summary>
    [BotAuthentication]
    public class ReliableMessageDeliveryController : ApiController
    {
        private const int MaxParallelism = 4;
        private readonly EventHelper eventHelper;
        private readonly UserManagementHelper userManagementHelper;
        private readonly ILogProvider logProvider;
        private ConnectorServiceHelper connectorServiceHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReliableMessageDeliveryController"/> class.
        /// </summary>
        /// <param name="eventHelper">EventHelper instance.</param>
        /// <param name="userManagementHelper">UserManagementHelper instance</param>
        /// <param name="logProvider">The instance of <see cref="ILogProvider"/></param>
        public ReliableMessageDeliveryController(EventHelper eventHelper, UserManagementHelper userManagementHelper, ILogProvider logProvider)
        {
            this.eventHelper = eventHelper;
            this.userManagementHelper = userManagementHelper;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Reliable message delivery.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> Post()
        {
            // Delete all expired messages.
            await this.eventHelper.DeleteExpiredMessagesAsync();

            List<int> statusCodes = new List<int> { 429, 500, 501, 502, 503, 504, 505, 506, 507, 508, 510, 511 };
            var eventMessages = await this.eventHelper.GetEventMessagesByEventStatus(statusCodes);
            this.connectorServiceHelper = new ConnectorServiceHelper(this.CreateConnectorClient(eventMessages.FirstOrDefault().Activity.ServiceUrl), this.logProvider);

            var previewMessages = eventMessages.Where(x => x.MessageType == MessageType.Preview).ToList();
            var eventNotificationMessages = eventMessages.Where(x => x.MessageType == MessageType.Event).ToList();

            // Retry sending event notification
            await this.SendEventCard(eventNotificationMessages);

            // Retry for preview card.
            await this.SendEventCard(previewMessages);

            return new HttpResponseMessage(HttpStatusCode.OK);
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
        /// Send event card
        /// </summary>
        /// <param name="eventMessages">List of EventMessage</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendEventCard(List<EventMessage> eventMessages)
        {
            var tasks = Task.Run(() =>
                {
                    Parallel.ForEach(eventMessages, new ParallelOptions { MaxDegreeOfParallelism = MaxParallelism }, async (eventMessage) =>
                   {
                       var activity = eventMessage.Activity;
                       HeroCard card = null;
                       switch (eventMessage.MessageType)
                       {
                           case MessageType.Event:
                               card = CelebrationCard.GetEventCard(activity);
                               break;

                           case MessageType.Preview:
                               card = CelebrationCard.GetPreviewCard(activity);
                               break;
                       }

                       var task = this.connectorServiceHelper.SendPersonalMessageAsync(string.Empty, new List<Attachment> { card.ToAttachment() }, activity.ConversationId);

                       RetryWithExponentialBackoff retryBackOff = new RetryWithExponentialBackoff();
                       await retryBackOff.RunAsync(task, eventMessage, this.SuccessCallback, this.FailureCallback);
                   });
                });

            await tasks;
        }

        /// <summary>
        /// Failure callback.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="eventMessage">EventMessage instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task FailureCallback(Exception exception, EventMessage eventMessage)
        {
            int statusCode = (int)HttpStatusCode.BadRequest;
            var responsebody = exception.ToString();

            if (exception is HttpException)
            {
                HttpException httpException = (HttpException)exception;
                statusCode = httpException.GetHttpCode();
                responsebody = httpException.GetHtmlErrorMessage();
            }
            else if (exception is ErrorResponseException)
            {
                ErrorResponseException errorResponseException = (ErrorResponseException)exception;
                statusCode = (int)errorResponseException.Response.StatusCode;
                responsebody = errorResponseException.Response.ToString();
            }

            this.logProvider.LogError($"Failed to send {eventMessage.MessageType} card.", exception, new Dictionary<string, string>
                        {
                            { "EventId", eventMessage.EventId },
                            { "OccurrenceId", eventMessage.OccurrenceId },
                            { "eventActivity", eventMessage.Activity.ToString() },
                            { "LastAttemptStatusCode", statusCode.ToString() },
                            { "LastAttemptTime", DateTime.UtcNow.ToString() },
                        });

            await this.UpdateMessageSendResult(eventMessage.Id, statusCode, DateTime.Now, responsebody);
        }

        /// <summary>
        /// Success callback.
        /// </summary>
        /// <param name="eventMessage">EventMessage instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SuccessCallback(EventMessage eventMessage)
        {
            await this.UpdateMessageSendResult(eventMessage.Id, Convert.ToInt32(HttpStatusCode.OK), DateTime.Now, "Successfully sent the message.");
        }

        /// <summary>
        /// Update MessageSendResult in EventMessages Collection.
        /// </summary>
        /// <param name="id">unique id that identifies the record in EventMessages Collection. </param>
        /// <param name="statusCode">HTTP Status code.</param>
        /// <param name="lastAtttemptTime">Last attempt time.</param>
        /// <param name="responseBody">Response body.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task UpdateMessageSendResult(string id, int statusCode, DateTime lastAtttemptTime, string responseBody)
        {
            MessageSendResult messageSendResult = new MessageSendResult()
            {
                LastAttemptTime = lastAtttemptTime,
                StatusCode = statusCode,
                ResponseBody = responseBody,
            };

            await this.eventHelper.UpdateEventMessageAsync(id, messageSendResult);
        }
    }
}
