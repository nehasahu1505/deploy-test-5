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
    using Microsoft.Teams.Celebration.App.Helpers;
    using Microsoft.Teams.Celebration.App.Models;
    using Microsoft.Teams.Celebration.App.Models.Enums;
    using Microsoft.Teams.Celebration.App.Resources;

    /// <summary>
    /// Messaging Controller.
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly ILogProvider logProvider;
        private readonly UserManagementHelper userManagementHelper;
        private IConnectorClient connectorClient;
        private string serviceUrl = string.Empty;
        private IList<ChannelAccount> teamMembers = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="logProvider">The instance of <see cref="ILogProvider"/></param>
        /// <param name="userManagementHelper">UserManagementHelper instance</param>
        public MessagesController(ILogProvider logProvider, UserManagementHelper userManagementHelper)
        {
            this.logProvider = logProvider;
            this.userManagementHelper = userManagementHelper;
        }

        /// <summary>
        /// Receives message from user and reply to it.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity, CancellationToken cancellationToken)
        {
            UserTelemetryInitializer.SetTelemetryUserId(HttpContext.Current, activity.From.Id);
            this.LogUserActivity(activity);
            this.teamMembers = null;

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

        private async Task AddUserTeamMembershipAsync(string teamId, ChannelAccount member)
        {
            var userMembership = new UserTeamMembership
            {
                TeamId = teamId,
                UserTeamsId = member.Id,
            };

            await this.userManagementHelper.AddUserTeamMembershipAsync(userMembership);
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
                    await this.HandleMemberAddedActionAsync(message, cancellationToken);
                }
                else if (message.MembersRemoved?.Count > 0)
                {
                    await this.HandleMemberDeletedActionAsync(message, cancellationToken);
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        private async Task HandleMemberAddedActionAsync(Activity activity, CancellationToken cancellationToken)
        {
            this.serviceUrl = activity.ServiceUrl;
            this.connectorClient = new ConnectorClient(new Uri(this.serviceUrl));

            bool isBotAdded = activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id);
            var channelData = activity.GetChannelData<TeamsChannelData>();
            var teamId = channelData.Team.Id;
            IList<ChannelAccount> teamMembers = null;

            if (isBotAdded)
            {
                // Add Team details,where the bot is installed.
                await this.AddTeamDetailsAsync(teamId);

                teamMembers = await this.connectorClient.Conversations.GetConversationMembersAsync(teamId);

                // Send welcome message in General Channel
                await this.SendWelcomeMessageToGeneralChannel(activity);
            }
            else
            {
                teamMembers = activity.MembersAdded;
            }

            // Save team member details.
            await this.SaveMemberDetailsAsync(teamMembers, channelData);
        }

        private async Task SaveMemberDetailsAsync(IList<ChannelAccount> teamMembers, TeamsChannelData channelData)
        {
            foreach (var member in teamMembers)
            {
                // Add UserTeamMembership
                await this.AddUserTeamMembershipAsync(channelData.Team.Id, member);

                // Add user if it does not exist.
                await this.AddUserDetailsAsync(channelData, member);
            }
        }

        /// <summary>
        /// Send welcome message in general channel.
        /// </summary>
        /// <param name="activity">Activity instance.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendWelcomeMessageToGeneralChannel(Activity activity)
        {
            var reply = activity.CreateReply();
            reply.Text = Strings.WelcomeMessage;
            await this.connectorClient.Conversations.ReplyToActivityAsync(reply);
        }

        /// <summary>
        /// Add user.
        /// </summary>
        /// <param name="channelData">TeamsChannelData information.</param>
        /// <param name="member">ChannelAccount information of user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task AddUserDetailsAsync(TeamsChannelData channelData, ChannelAccount member)
        {
            var objectId = member.Properties["objectId"] ?? member.Properties["aadObjectId"];

            if (await this.userManagementHelper.GetUserByAadObjectIdAsync(objectId.ToString()) == null)
            {
                // Add user
                var user = new User
                {
                    AadObjectId = objectId.ToString(),
                    TeamsId = member.Id,
                    InstallationMethod = BotScope.Team,
                    ServiceUrl = this.serviceUrl,
                    UserName = await this.GetUserNameAsync(channelData.Team.Id, member),
                };

                await this.userManagementHelper.AddUserAsync(user);
            }
        }

        /// <summary>
        /// Returns user name.
        /// </summary>
        /// <param name="teamId">Team Id.</param>
        /// <param name="member">ChannelAccount information of team member.</param>
        /// <returns>user name.</returns>
        private async Task<string> GetUserNameAsync(string teamId, ChannelAccount member)
        {
            string name = string.Empty;
            if (!string.IsNullOrWhiteSpace(member.Name))
            {
                name = member.Name;
            }
            else
            {
                if (this.teamMembers == null)
                {
                    this.teamMembers = await this.connectorClient.Conversations.GetConversationMembersAsync(teamId);
                }

                var userInfo = from teamMember in this.teamMembers
                               where teamMember.Id == member.Id
                               select teamMember;

                name = userInfo.Count() == 0 ? string.Empty : userInfo.FirstOrDefault().Name;
            }

            return name;
        }

        /// <summary>
        /// Add teams details
        /// </summary>
        /// <param name="teamId">TeamId</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task AddTeamDetailsAsync(string teamId)
        {
            var teamDetails = await this.connectorClient.GetTeamsConnectorClient().Teams.FetchTeamDetailsAsync(teamId);

            // Add Team Details
            var team = new Team
            {
                Id = teamId,
                Name = teamDetails.Name,
            };

            await this.userManagementHelper.SaveTeamDetailsAsync(team);
        }

        /// <summary>
        /// Handles the action when member is deleted from team.
        /// </summary>
        /// <param name="activity">Activity instance</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task HandleMemberDeletedActionAsync(Activity activity, CancellationToken cancellationToken)
        {
            bool isBotRemoved = activity.MembersRemoved.Any(member => member.Id == activity.Recipient.Id);
            var channelData = activity.GetChannelData<TeamsChannelData>();
            var teamId = channelData.Team.Id;
            if (isBotRemoved)
            {
                // Delete team details
                await this.userManagementHelper.DeleteTeamDetailsAsync(teamId);

                var userTeamMembershipList = await this.userManagementHelper.GetUserTeamMembershipByTeamIdAsync(teamId);

                // Delete UserTeamMembership
                await this.userManagementHelper.DeleteUserTeamMembershipByTeamIdAsync(teamId);
            }
            else
            {
                foreach (var member in activity.MembersRemoved)
                {
                    // Delete UserTeamMembership
                    await this.userManagementHelper.DeleteUserTeamMembershipAsync(member.Id, teamId);
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