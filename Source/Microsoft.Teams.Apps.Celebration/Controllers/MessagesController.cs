// <copyright file="MessagesController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using AdaptiveCards;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.Apps.Celebration.Dialog;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Models.Enums;
    using Microsoft.Teams.Apps.Celebration.Utilities;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using Microsoft.Teams.Apps.Common.Telemetry;

    /// <summary>
    /// Messaging Controller.
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private readonly ILifetimeScope scope;
        private readonly ILogProvider logProvider;
        private readonly UserManagementHelper userManagementHelper;
        private readonly EventHelper eventHelper;
        private IConnectorClient connectorClient;
        private ConnectorServiceHelper connectorServiceHelper;
        private string serviceUrl = string.Empty;
        private IList<ChannelAccount> teamMembers = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesController"/> class.
        /// </summary>
        /// <param name="scope">ILifetimeScope</param>
        /// <param name="logProvider">The instance of <see cref="ILogProvider"/></param>
        /// <param name="userManagementHelper">UserManagementHelper instance</param>
        /// <param name="eventHelper">EventHelper</param>
        public MessagesController(ILifetimeScope scope, ILogProvider logProvider, UserManagementHelper userManagementHelper, EventHelper eventHelper)
        {
            this.scope = scope;
            this.logProvider = logProvider;
            this.userManagementHelper = userManagementHelper;
            this.eventHelper = eventHelper;
        }

        /// <summary>
        /// Receives message from user and reply to it.
        /// </summary>
        /// <param name="activity">activity object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            UserTelemetryInitializer.SetTelemetryUserId(HttpContext.Current, activity.From.Id);

            this.LogUserActivity(activity);
            this.teamMembers = null;

            if (activity.Type == ActivityTypes.Invoke || (activity.Type == ActivityTypes.Message && activity.Value != null))
            {
                using (var dialogScope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                {
                    var dialog = dialogScope.Resolve<RootDialog>();
                    await Conversation.SendAsync(activity, () => dialog);
                }
            }
            else
            {
                using (var dialogScope = DialogModule.BeginLifetimeScope(this.scope, activity))
                {
                    IConnectorClient connectorClient = dialogScope.Resolve<IConnectorClient>();
                    this.connectorServiceHelper = new ConnectorServiceHelper(connectorClient, this.logProvider);
                    if (activity.Type == ActivityTypes.Message)
                    {
                        Activity welcomeActivity = activity.CreateReply();
                        welcomeActivity.Attachments.Add(CelebrationCard.GetWelcomeCardInResponseToUserMessage().ToAttachment());
                        await connectorClient.Conversations.ReplyToActivityAsync(welcomeActivity);
                    }
                    else
                    {
                        await this.HandleSystemMessageAsync(connectorClient, activity);
                    }
                }
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
        /// Handles actions for rest of activity type except message.
        /// </summary>
        /// <param name="connectorClient">IConnectorClient</param>
        /// <param name="message">activity object.</param>
        /// <returns>Task.</returns>
        private async Task HandleSystemMessageAsync(IConnectorClient connectorClient, Activity message)
        {
            this.connectorClient = connectorClient;
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                if (message.MembersAdded?.Count > 0)
                {
                    await this.HandleMemberAddedActionAsync(message);
                }
                else if (message.MembersRemoved?.Count > 0)
                {
                    await this.HandleMemberDeletedActionAsync(message);
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
        /// <returns>Task.</returns>
        private async Task HandleMemberAddedActionAsync(Activity activity)
        {
            this.logProvider.LogInfo("Performing member added action.");

            bool isBotAdded = activity.MembersAdded.Any(member => member.Id == activity.Recipient.Id);
            var channelData = activity.GetChannelData<TeamsChannelData>();
            this.serviceUrl = activity.ServiceUrl;
            string teamId = channelData?.Team?.Id;
            string installerName = activity.From?.Name;
            IList<ChannelAccount> teamMembers = null;
            bool isBotInstalledInTeam = teamId == null ? false : true;
            TeamDetails teamDetails = null;

            if (string.IsNullOrWhiteSpace(installerName) && !string.IsNullOrWhiteSpace(teamId))
            {
                installerName = await this.GetUserNameAsync(teamId, activity.From);
            }

            if (!string.IsNullOrEmpty(teamId))
            {
                this.logProvider.LogInfo($"fetching team details using connector client for team Id:{teamId}.");
                teamDetails = await this.connectorClient.GetTeamsConnectorClient().Teams.FetchTeamDetailsAsync(teamId);
            }

            if (isBotAdded)
            {
                this.LogBotInstallationInformation(teamId, installerName, isBotInstalledInTeam);

                if (!string.IsNullOrWhiteSpace(teamId))
                {
                    // Add Team details,where the bot is installed.
                    await this.AddTeamDetailsAsync(teamId, teamDetails.Name);

                    teamMembers = await this.connectorClient.Conversations.GetConversationMembersAsync(teamId);
                }

                // Send welcome message in General Channel
                await this.SendWelcomeMessageToGeneralChannel(activity, isBotInstalledInTeam, installerName, teamDetails.Name);
            }
            else
            {
                teamMembers = activity.MembersAdded;
            }

            if (teamMembers != null && teamMembers.Count > 0)
            {
            // Send Welcome message to all the team members.
                await this.SendWelcomeMessageToTeamMembers(teamMembers, channelData, installerName, teamDetails.Name);
            }

            this.logProvider.LogInfo("Completed member added action.");
        }

        // log bot installation information
        private void LogBotInstallationInformation(string teamId, string installerName, bool isBotInstalledInTeam)
        {
            if (isBotInstalledInTeam)
            {
                this.logProvider.LogInfo($"{installerName} installed bot in team: {teamId}");
            }
            else
            {
                this.logProvider.LogInfo("Bot installed personally.");
            }
        }

        // send welcome message to all team members
        private async Task SendWelcomeMessageToTeamMembers(IList<ChannelAccount> teamMembers, TeamsChannelData channelData, string installerName, string teamName)
        {
            this.logProvider.LogInfo($"Sending welcome message to all the team members. Total team members of team: {channelData.Team.Id} = {teamMembers.Count}");
            string conversationId = string.Empty;
            foreach (var member in teamMembers)
            {
                conversationId = this.connectorServiceHelper.CreateOrGetConversationIdAsync(channelData.Tenant.Id, member.Id);
                List<Attachment> attachmentList = new List<Attachment> { CelebrationCard.GetWelcomeMessageForGeneralChannelAndTeamMembers(installerName, teamName).ToAttachment() };
                string userAadObjectId = (member.Properties["objectId"] ?? member.Properties["aadObjectId"]).ToString();
                List<CelebrationEvent> celebrationEvents = await (await this.eventHelper.GetEventsByOwnerObjectIdAsync(userAadObjectId)).ToListAsync();

                if (celebrationEvents.Count > 0)
                {
                    attachmentList.Add(CelebrationCard.GetShareEventAttachement(channelData.Team.Id, channelData.Team.Name, userAadObjectId));
                }

                this.logProvider.LogInfo($"Sending personal welcome message to {member.Name}", new Dictionary<string, string>
                {
                    { "TeamId", channelData.Team.Id },
                    { "UserTeamId", member.Id },
                    { "UserObjectId", userAadObjectId },
                    { "ConversationId", conversationId },
                    { "Attachment", attachmentList.ToString() },
                });

                await this.connectorServiceHelper.SendPersonalMessageAsync(string.Empty, attachmentList, conversationId);

                this.logProvider.LogInfo("Saving member details to database.");

                // Save team member details.
                await this.SaveMemberDetailsAsync(member, channelData, conversationId);
            }
        }

        /// <summary>
        /// Save member details in database
        /// </summary>
        /// <param name="member">ChannelAccount information of team member</param>
        /// <param name="channelData">TeamsChannelData</param>
        /// <param name="conversationId">Conversation id</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SaveMemberDetailsAsync(ChannelAccount member, TeamsChannelData channelData, string conversationId)
        {
            // Add UserTeamMembership
            await this.AddUserTeamMembershipAsync(channelData.Team.Id, member);

            // Add user if it does not exist.
            await this.AddUserDetailsAsync(channelData, member, conversationId);
        }

        /// <summary>
        /// Send welcome message in general channel.
        /// </summary>
        /// <param name="activity">Activity instance.</param>
        /// <param name="isBotInstalledInTeam">true/false.</param>
        /// <param name="installerName">bot installer name.</param>
        /// <param name="teamName">TeamName</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task SendWelcomeMessageToGeneralChannel(Activity activity, bool isBotInstalledInTeam, string installerName, string teamName)
        {
            this.logProvider.LogInfo("Sending welcome message to general channel.");

            Activity welcomeActivity = activity.CreateReply();
            AdaptiveCard welcomeCard;

            if (isBotInstalledInTeam)
            {
                welcomeCard = CelebrationCard.GetWelcomeMessageForGeneralChannelAndTeamMembers(installerName, teamName);
            }
            else
            {
                welcomeCard = CelebrationCard.GetWelcomeCardForInstaller();
            }

            this.logProvider.LogInfo($"Welcome card json: {welcomeCard.ToJson()}");

            welcomeActivity.Attachments.Add(welcomeCard.ToAttachment());
            await this.connectorClient.Conversations.ReplyToActivityAsync(welcomeActivity);

            this.logProvider.LogInfo("Welcome message sent to general Chanel.");
        }

        /// <summary>
        /// Add user.
        /// </summary>
        /// <param name="channelData">TeamsChannelData information.</param>
        /// <param name="member">ChannelAccount information of user.</param>
        /// <param name="conversationId">Conversation Id to initiate the conversation between bot and user</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task AddUserDetailsAsync(TeamsChannelData channelData, ChannelAccount member, string conversationId)
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
                    TenantId = channelData.Tenant.Id,
                    ConversationId = conversationId,
                };

                this.logProvider.LogInfo("Save user details in database", new Dictionary<string, string> { { "User", user.ToString() } });
                await this.userManagementHelper.SaveUserAsync(user);
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
        /// <param name="teamName">TeamName</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task AddTeamDetailsAsync(string teamId, string teamName)
        {
            // Add Team Details
            var team = new Team
            {
                Id = teamId,
                Name = teamName,
            };

            this.logProvider.LogInfo("Adding team details in database.", new Dictionary<string, string> { { "TeamId", team.Id }, { "TeamName", team.Name } });

            await this.userManagementHelper.SaveTeamDetailsAsync(team);
        }

        /// <summary>
        /// Handles the action when member is deleted from team.
        /// </summary>
        /// <param name="activity">Activity instance</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task HandleMemberDeletedActionAsync(Activity activity)
        {
            this.logProvider.LogInfo("Performing member deleted action");

            bool isBotRemoved = activity.MembersRemoved.Any(member => member.Id == activity.Recipient.Id);
            var channelData = activity.GetChannelData<TeamsChannelData>();
            var teamId = channelData?.Team?.Id;

            if (isBotRemoved)
            {
                this.logProvider.LogInfo("Bot un-installed");

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
                    this.logProvider.LogInfo($"{member.Name} : member deleted from team: {teamId}.");

                    // Delete UserTeamMembership
                    await this.userManagementHelper.DeleteUserTeamMembershipAsync(member.Id, teamId);
                }
            }

            this.logProvider.LogInfo("Completed member deleted action");
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