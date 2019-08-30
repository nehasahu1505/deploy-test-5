// <copyright file="ConnectorServiceHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;

    /// <summary>
    /// Helper class to handle the request to connector service.
    /// </summary>
    public class ConnectorServiceHelper
    {
        private readonly ILogProvider logProvider;
        private IConnectorClient connectorClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectorServiceHelper"/> class.
        /// </summary>
        /// <param name="connectorClient">IConnectorClient</param>
        /// <param name="logProvider">The instance of <see cref="ILogProvider"/></param>
        public ConnectorServiceHelper(IConnectorClient connectorClient, ILogProvider logProvider)
        {
            this.connectorClient = connectorClient;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// Send message in carousel of Hero card format.
        /// </summary>
        /// <param name="messageText">Message.</param>
        /// <param name="attachments">attachments</param>
        /// <param name="conversationId">Conversation id that is required to start the conversation.</param>
        /// <param name="entities">List of Entity</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendMessageInCarouselFormatAsync(string messageText, List<Attachment> attachments, string conversationId, List<Entity> entities = null)
        {
            Activity activity = Common.CreateNewActivity(conversationId);
            activity.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            if (entities != null)
            {
                activity.Entities = entities;
            }

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                activity.Text = messageText;
            }

            if (attachments != null)
            {
                activity.Attachments = attachments;
            }

            this.logProvider.LogInfo($"Activity sent by bot: {Newtonsoft.Json.JsonConvert.SerializeObject(activity)}");
            await this.connectorClient.Conversations.SendToConversationAsync(activity);
        }

        /// <summary>
        /// Send Personal message to user.
        /// </summary>
        /// <param name="messageText">Message.</param>
        /// <param name="attachments">List of Attachments.</param>
        /// <param name="conversationId">Conversation id that is required to start the conversation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendPersonalMessageAsync(string messageText, List<Attachment> attachments, string conversationId)
        {
            Activity activity = Common.CreateNewActivity(conversationId);

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                activity.Text = messageText;
            }

            if (attachments != null)
            {
                activity.Attachments = attachments;
            }

            this.logProvider.LogInfo($"Activity sent by bot: {Newtonsoft.Json.JsonConvert.SerializeObject(activity)}");
            await this.connectorClient.Conversations.SendToConversationAsync(activity);
        }

        /// <summary>
        /// returns conversation id to initiate the connection between user and bot.
        /// </summary>
        /// <param name="tenantId">Tenant Id</param>
        /// <param name="userTeamsId">teamsId of user.</param>
        /// <returns>conversationId.</returns>
        public string CreateOrGetConversationIdAsync(string tenantId, string userTeamsId)
        {
            ChannelAccount bot = new ChannelAccount { Id = Common.GetTeamsBotId() };
            ChannelAccount user = new ChannelAccount { Id = userTeamsId };
            return this.connectorClient.Conversations.CreateOrGetDirectConversation(bot, user, tenantId).Id;
        }
    }
}