// <copyright file="SkipEventDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Dialog
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Models.Enums;
    using Microsoft.Teams.Apps.Celebration.Resources;
    using Microsoft.Teams.Apps.Celebration.Utilities;
    using Microsoft.Teams.Apps.Common.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Handle the event skip action.
    /// </summary>
    [Serializable]
    public class SkipEventDialog : IDialog<object>
    {
        [NonSerialized]
        private readonly EventHelper eventHelper;
        private readonly IConnectorClient connectorClient;
        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipEventDialog"/> class.
        /// </summary>
        /// <param name="connectorClient">Connector client </param>
        /// <param name="eventHelper">EventHelper instance</param>
        /// <param name="logProvider">Logging component</param>
        public SkipEventDialog(IConnectorClient connectorClient, EventHelper eventHelper, ILogProvider logProvider)
        {
            this.eventHelper = eventHelper;
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
            context.Wait(this.HandleEventSkipActions);
        }

        /// <summary>
        /// Handles event skip action.
        /// </summary>
        /// <param name="context">IDialogContext object.</param>
        /// <param name="activity">IAwaitable message activity.</param>
        /// <returns>Task.</returns>
        public async Task HandleEventSkipActions(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = (Activity)await activity;

            if (message.Value != null)
            {
                var replyMessage = string.Empty;
                var previewCardPayload = ((JObject)message.Value).ToObject<PreviewCardPayload>();

                // Get event by eventId to check if it exist or not.
                CelebrationEvent celebrationEvent = await this.eventHelper.GetEventByEventIdAsync(previewCardPayload.EventId, previewCardPayload.OwnerAadObjectId);

                if (celebrationEvent != null)
                {
                    if (previewCardPayload.UpcomingEventDate > DateTime.UtcNow.Date)
                    {
                        await this.eventHelper.UpdateRecurringEventAsync(previewCardPayload.EventId, EventStatus.Skipped);

                        EventMessageActivity eventMessageActivity = new EventMessageActivity
                        {
                            Id = celebrationEvent.Id,
                            OwnerName = previewCardPayload.OwnerName,
                            ImageUrl = celebrationEvent.ImageURL,
                            Message = celebrationEvent.Message,
                            Title = celebrationEvent.Title,
                        };

                        // Update the card
                        IMessageActivity updatedMessage = context.MakeMessage();
                        updatedMessage.Attachments.Add(CelebrationCard.GetPreviewCard(eventMessageActivity, false).ToAttachment());
                        updatedMessage.ReplyToId = message.ReplyToId;
                        await this.connectorClient.Conversations.UpdateActivityAsync(message.Conversation.Id, message.ReplyToId, (Activity)updatedMessage);

                        replyMessage = string.Format(Strings.EventSkippedMessage, message.From.Name);
                    }
                    else
                    {
                        // event occurrence has already passed for current year.
                        replyMessage = string.Format(Strings.EventPassedMessage);
                    }
                }
                else
                {
                    replyMessage = string.Format(Strings.EventNotExistMessage, previewCardPayload.Title);
                }

                await context.PostAsync(replyMessage);
                context.Done<object>(null);
            }
        }
    }
}
