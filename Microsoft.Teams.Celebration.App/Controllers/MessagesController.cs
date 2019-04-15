// <copyright file="MessagesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.Common.Telemetry;
    using Microsoft.Teams.Celebration.App.Resources;

    /// <summary>
    /// Messaging Controller.
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly ILogProvider logProvider;
        private IConnectorClient connectorClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="logProvider">The instance of <see cref="ILogProvider"/></param>
        public MessagesController(ILogProvider logProvider)
        {
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Recieves message from user and reply to it.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken cancellationToken)
        {
            UserTelemetryInitializer.SetTelemetryUserId(HttpContext.Current, activity.From.Id);
            this.LogUserActivity(activity);

            if (activity.Type == ActivityTypes.Message)
            {
            }
            else
            {
                await this.HandleSystemMessageAsync(activity, cancellationToken);
            }

            var response = this.Request.CreateResponse(HttpStatusCode.OK);

            return response;
        }

        /// <summary>
        /// Handles actions for rest of activity type except message .
        /// </summary>
        /// <param name="message">activity object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task HandleSystemMessageAsync(Activity message, CancellationToken cancellationToken)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                if (message.MembersAdded?.Count > 0)
                {
                    await this.HandleMemberAddedAction(message, cancellationToken);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.Typing)
            {
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
        }

        /// <summary>
        /// Handles the new member added action.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <param name="cancellationToken">Cacellation token.</param>
        /// <returns>Task.</returns>
        private async Task HandleMemberAddedAction(Activity activity, CancellationToken cancellationToken)
        {
            this.connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (!(activity.Conversation.IsGroup ?? false))
            {
            }
            else
            {
                bool isBotAdded = activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id);

                if (isBotAdded)
                {
                    var reply = activity.CreateReply();
                    reply.Text = Strings.WelcomeMessage;
                    await this.connectorClient.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                }
            }
        }

        // Log information about the received user activity
        private void LogUserActivity(Activity activity)
        {
            // Log the user activity
            var channelData = activity.GetChannelData<TeamsChannelData>();
            var fromTeamsAccount = activity.From.AsTeamsChannelAccount();
            var fromObjectId = fromTeamsAccount.ObjectId ?? activity.From.Properties["aadObjectId"]?.ToString();
            var clientInfoEntity = activity.Entities.Where(e => e.Type == "clientInfo").FirstOrDefault();

            var properties = new Dictionary<TelemetryProperty, string>
            {
                { TelemetryProperty.ActivityType, activity.Type },
                { TelemetryProperty.ActivityId, activity.Id },
                { TelemetryProperty.UserId, activity.From.Id },
                { TelemetryProperty.UserAadObjectId, fromObjectId },
                { TelemetryProperty.ConversationId, activity.Conversation.Id },
                {
                    TelemetryProperty.ConversationType, string.IsNullOrWhiteSpace(activity.Conversation.ConversationType)
                    ? "personal" : activity.Conversation.ConversationType
                },
                { TelemetryProperty.Locale, clientInfoEntity?.Properties["locale"]?.ToString() },
                { TelemetryProperty.Platform, clientInfoEntity?.Properties["platform"]?.ToString() },
            };
            if (!string.IsNullOrEmpty(channelData?.EventType))
            {
                properties[TelemetryProperty.TeamsEventType] = channelData.EventType;
            }

            this.logProvider.LogEvent(TelemetryEvent.UserActivity, properties);
        }
    }
}