// <copyright file="ShareEventDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Dialog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Utilities;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Share event with in team.
    /// </summary>
    [Serializable]
    public class ShareEventDialog : IDialog<object>
    {
        [NonSerialized]
        private readonly EventHelper eventHelper;
        private readonly IConnectorClient connectorClient;
        private readonly ILogProvider logProvider;
        [NonSerialized]
        private readonly UserManagementHelper userManagementHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShareEventDialog"/> class.
        /// </summary>
        /// <param name="connectorClient">Connector client </param>
        /// <param name="eventHelper">EventHelper instance</param>
        /// <param name="userManagementHelper">UserManagementHelper instance</param>
        /// <param name="logProvider">Logging component</param>
        public ShareEventDialog(IConnectorClient connectorClient, EventHelper eventHelper, UserManagementHelper userManagementHelper, ILogProvider logProvider)
        {
            this.eventHelper = eventHelper;
            this.connectorClient = connectorClient;
            this.logProvider = logProvider;
            this.userManagementHelper = userManagementHelper;
        }

        /// <summary>
        /// The start of the code that represents the conversational dialog.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <returns> A task that represents the dialog start.</returns>
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.HandleEventShareAction);
        }

        /// <summary>
        /// Handles event Share action.
        /// </summary>
        /// <param name="context">IDialogContext object.</param>
        /// <param name="activity">IAwaitable message activity.</param>
        /// <returns>Task.</returns>
        public async Task HandleEventShareAction(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = (Activity)await activity;
            var replyMessage = string.Empty;
            if (message.Value != null)
            {
                ShareEventPayload shareEventPayload = ((JObject)message.Value).ToObject<ShareEventPayload>();

                try
                {
                    var teamMembers = await this.connectorClient.Conversations.GetConversationMembersAsync(shareEventPayload.TeamId);

                    var user = teamMembers.Where(x => x.Properties["objectId"].ToString() == shareEventPayload.UserAadObjectId).ToList().FirstOrDefault();

                    var document = await this.userManagementHelper.GetTeamsDetailsByTeamIdAsync(shareEventPayload.TeamId);
                    bool isBotUnintsalledFromTeam = document == null ? true : false;

                    if (user == null)
                    {
                        replyMessage = $"You are no longer a member of {shareEventPayload.TeamName}.";
                    }
                    else if (isBotUnintsalledFromTeam)
                    {
                        replyMessage = "Someone uninstalled me from your team, I can no longer share these events there";
                    }
                    else
                    {
                        List<CelebrationEvent> celebrationEvents = await (await this.eventHelper.GetEventsByOwnerObjectIdAsync(
                                                             shareEventPayload.UserAadObjectId)).ToListAsync();
                        if (celebrationEvents.Count > 0)
                        {
                            foreach (var celebrationEvent in celebrationEvents)
                            {
                                celebrationEvent.Teams.Add(new Team { Id = shareEventPayload.TeamId });
                                CelebrationEvent updatedEvent = (dynamic)celebrationEvent;
                                updatedEvent.Teams = celebrationEvent.Teams;
                                await this.eventHelper.UpdateEventAsync(updatedEvent);
                            }
                        }

                        replyMessage = "I’ve set those events to be shared with the team when they occur.";

                        // Update the card
                        IMessageActivity updatedMessage = context.MakeMessage();
                        updatedMessage.Attachments.Add(CelebrationCard.GetShareEventAttachementWithoutActionButton(shareEventPayload.TeamName));
                        updatedMessage.ReplyToId = message.ReplyToId;
                        await this.connectorClient.Conversations.UpdateActivityAsync(message.Conversation.Id, message.ReplyToId, (Activity)updatedMessage);
                    }
                }
                catch (Exception ex)
                {
                    this.logProvider.LogError("Failed to share the existing event with team", ex, new Dictionary<string, string>()
                    {
                        {
                            "TeamId", shareEventPayload.TeamId
                        },
                        {
                            "TeamName", shareEventPayload.TeamName
                        },
                        {
                            "UserAadObjectId", shareEventPayload.UserAadObjectId
                        },
                    });

                    replyMessage = "Some error occurred to share the event with team. Please try again.";
                }

                await context.PostAsync(replyMessage);
                context.Done<object>(null);
            }
        }
    }
}