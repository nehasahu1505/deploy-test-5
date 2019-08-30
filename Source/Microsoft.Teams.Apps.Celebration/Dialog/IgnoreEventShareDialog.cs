// <copyright file="IgnoreEventShareDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Dialog
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Utilities;
    using Microsoft.Teams.Apps.Common.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Ignore Event Sharing in team.
    /// </summary>
    [Serializable]
    public class IgnoreEventShareDialog : IDialog<object>
    {
        private readonly IConnectorClient connectorClient;
        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreEventShareDialog"/> class.
        /// </summary>
        /// <param name="connectorClient">Connector client </param>
        /// <param name="logProvider">Logging component</param>
        public IgnoreEventShareDialog(IConnectorClient connectorClient, ILogProvider logProvider)
        {
            this.connectorClient = connectorClient;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// The start of the code that represents the conversational dialog.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <returns> A task that represents the dialog start.</returns>
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.HandleIgnoreEventSharingAction);
        }

        /// <summary>
        /// Handles Ignore event sharing action.
        /// </summary>
        /// <param name="context">IDialogContext object.</param>
        /// <param name="activity">IAwaitable message activity.</param>
        /// <returns>Task.</returns>
        public async Task HandleIgnoreEventSharingAction(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = (Activity)await activity;

            if (message.Value != null)
            {
                var replyMessage = "Ok, if you change your mind you can share the events from the Events tab.";
                ShareEventPayload shareEventPayload = ((JObject)message.Value).ToObject<ShareEventPayload>();

                // Update the card
                IMessageActivity updatedMessage = context.MakeMessage();
                updatedMessage.Attachments.Add(CelebrationCard.GetShareEventAttachementWithoutActionButton(shareEventPayload.TeamName));
                updatedMessage.ReplyToId = message.ReplyToId;
                await this.connectorClient.Conversations.UpdateActivityAsync(message.Conversation.Id, message.ReplyToId, (Activity)updatedMessage);

                await context.PostAsync(replyMessage);
                context.Done<object>(null);
            }
        }
    }
}