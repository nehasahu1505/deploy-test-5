// <copyright file="MessagesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Celebration.App.Resources;

    /// <summary>
    /// Messaging Controller.
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private IConnectorClient connectorClient;

        /// <summary>
        /// Recieves message from user and reply to it.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken cancellationToken)
        {
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
    }
}