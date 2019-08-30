// <copyright file="UserManagementHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Utilities;

    /// <summary>
    /// Stores methods to perform the crud operation in document DB.
    /// </summary>
    public class UserManagementHelper
    {
        // Request the minimum throughput by default
        private const int DefaultRequestThroughput = 400;

        private readonly TelemetryClient telemetryClient;
        private readonly Lazy<Task> initializeTask;

        private Database database;
        private DocumentCollection teamsCollection;
        private DocumentCollection usersCollection;
        private DocumentCollection userTeamMembershipCollection;

        private DocumentClient documentClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementHelper"/> class.
        /// </summary>
        /// <param name="telemetryClient">TelemetryClient instance</param>
        public UserManagementHelper(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;
            this.initializeTask = new Lazy<Task>(() => this.IntializeDatabaseAsync());
        }

        /// <summary>
        /// Get TeamDetails by list of team Id.
        /// </summary>
        /// <param name="teamId">TeamId</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Document> GetTeamsDetailsByTeamIdAsync(string teamId)
        {
            await this.EnsureInitializedAsync();
            var documentUri = UriFactory.CreateDocumentUri(this.database.Id, this.teamsCollection.Id, teamId);
            Document document = null;
            try
            {
                document = await this.documentClient.ReadDocumentAsync(documentUri, new RequestOptions { PartitionKey = new PartitionKey(teamId) });
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("Entity with the specified id does not exist in the system."))
                {
                    this.telemetryClient.TrackTrace(
                                                    string.Format("Document with the teamId '{0}' does not exist in Teams collection.", teamId),
                                                    SeverityLevel.Error);
                }
            }

            return document;
        }

        /// <summary>
        /// Get TeamDetails by list of team Ids.
        /// </summary>
        /// <param name="teamIds">List of TeamId</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IDocumentQuery<Team>> GetTeamsDetailsByTeamIdsAsync(List<string> teamIds)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return this.documentClient.CreateDocumentQuery<Team>(this.teamsCollection.SelfLink, options).Where(x => teamIds.Contains(x.Id)).AsDocumentQuery();
        }

        /// <summary>
        /// Save Team Details.
        /// </summary>
        /// <param name="team">Team instance</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveTeamDetailsAsync(Team team)
        {
            await this.EnsureInitializedAsync();
            await this.documentClient.UpsertDocumentAsync(this.teamsCollection.SelfLink, team);
        }

        /// <summary>
        /// Delete Team Detail.
        /// </summary>
        /// <param name="teamId">teamId</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteTeamDetailsAsync(string teamId)
        {
            await this.EnsureInitializedAsync();
            var documentUri = UriFactory.CreateDocumentUri(this.database.Id, this.teamsCollection.Id, teamId);
            Document document = null;
            try
            {
                document = await this.documentClient.ReadDocumentAsync(documentUri, new RequestOptions { PartitionKey = new PartitionKey(teamId) });
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("Entity with the specified id does not exist in the system."))
                {
                    this.telemetryClient.TrackTrace(
                                                    string.Format("Document with the teamId '{0}' does not exist in Teams collection.", teamId),
                                                    SeverityLevel.Error);
                }
            }

            if (document != null)
            {
                await this.documentClient.DeleteDocumentAsync(documentUri, new RequestOptions { PartitionKey = new PartitionKey(teamId) });
            }
        }

        /// <summary>
        /// Get User by aadObjectId.
        /// </summary>
        /// <param name="aadObjectId">AadObjectId of user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<Models.User> GetUserByAadObjectIdAsync(string aadObjectId)
        {
            await this.EnsureInitializedAsync();

            return (await this.documentClient.CreateDocumentQuery<Models.User>(this.usersCollection.SelfLink)
                   .Where(x => x.AadObjectId == aadObjectId).AsDocumentQuery().ToListAsync()).FirstOrDefault();
        }

        /// <summary>
        /// Get Users by aadObjectIds.
        /// </summary>
        /// <param name="aadObjectIds">list of AadObjectId of user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<Models.User>> GetUsersByAadObjectIdsAsync(List<string> aadObjectIds)
        {
            await this.EnsureInitializedAsync();

            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            return await this.documentClient.CreateDocumentQuery<Models.User>(this.usersCollection.SelfLink, options)
                   .Where(x => aadObjectIds.Contains(x.AadObjectId)).AsDocumentQuery().ToListAsync();
        }

        /// <summary>
        /// Add or update user
        /// </summary>
        /// <param name="user">User object</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SaveUserAsync(Models.User user)
        {
            await this.EnsureInitializedAsync();
            await this.documentClient.UpsertDocumentAsync(this.usersCollection.SelfLink, user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="userTeamsId">User teams Id.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteUserByTeamsIdAsync(string userTeamsId)
        {
            await this.EnsureInitializedAsync();
            var document = await this.GetUserByAadObjectIdAsync(userTeamsId);

            if (document != null)
            {
                await this.documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(document.AadObjectId) });
            }
        }

        /// <summary>
        /// Add record in UserTeamMembership collection.
        /// </summary>
        /// <param name="userTeamMembership">UserTeamMembership object</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddUserTeamMembershipAsync(UserTeamMembership userTeamMembership)
        {
            await this.EnsureInitializedAsync();
            await this.documentClient.CreateDocumentAsync(this.userTeamMembershipCollection.SelfLink, userTeamMembership);
        }

        /// <summary>
        /// Delete UserTeamMembership record.
        /// </summary>
        /// <param name="userTeamsId">User's teamsId</param>
        /// <param name="teamId">TeamId.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteUserTeamMembershipAsync(string userTeamsId, string teamId)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { PartitionKey = new PartitionKey(userTeamsId) };
            var document = (await this.documentClient.CreateDocumentQuery<UserTeamMembership>(this.userTeamMembershipCollection.SelfLink, options)
                           .Where(x => x.TeamId == teamId && x.UserTeamsId == userTeamsId)
                           .AsDocumentQuery().ToListAsync()).FirstOrDefault();

            if (document != null)
            {
                await this.documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(userTeamsId) });
            }
        }

        /// <summary>
        /// Delete UserTeamMembership record.
        /// </summary>
        /// <param name="teamId">TeamId.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteUserTeamMembershipByTeamIdAsync(string teamId)
        {
            await this.EnsureInitializedAsync();
            var documents = await this.GetUserTeamMembershipByTeamIdAsync(teamId);

            if (documents != null)
            {
                foreach (var document in documents)
                {
                    await this.documentClient.DeleteDocumentAsync(document.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(document.UserTeamsId) });
                }
            }
        }

        /// <summary>
        /// Returns UserTeamMembership list.
        /// </summary>
        /// <param name="teamId">teamId</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<UserTeamMembership>> GetUserTeamMembershipByTeamIdAsync(string teamId)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return await this.documentClient.CreateDocumentQuery<UserTeamMembership>(this.userTeamMembershipCollection.SelfLink, options)
                           .Where(x => x.TeamId == teamId).AsDocumentQuery().ToListAsync();
        }

        /// <summary>
        /// Returns UserTeamMembership list.
        /// </summary>
        /// <param name="userTeamsId">User's teamId</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<UserTeamMembership>> GetUserTeamMembershipByTeamsIdAsync(string userTeamsId)
        {
            await this.EnsureInitializedAsync();
            var options = new FeedOptions { EnableCrossPartitionQuery = true };

            return await this.documentClient.CreateDocumentQuery<UserTeamMembership>(this.userTeamMembershipCollection.SelfLink, options)
                           .Where(x => x.UserTeamsId == userTeamsId).AsDocumentQuery().ToListAsync();
        }

        private async Task IntializeDatabaseAsync()
        {
            this.telemetryClient.TrackTrace("Initializing data store");

            var documentDbEndpointUrl = new Uri(ApplicationSettings.CosmosDBEndpointUrl);
            var documentDbPrimaryKey = ApplicationSettings.CosmosDBKey;
            var requestOptions = new RequestOptions { OfferThroughput = DefaultRequestThroughput };
            bool useSharedOffer = true;

            this.documentClient = new DocumentClient(documentDbEndpointUrl, documentDbPrimaryKey);

            // Create the database if needed
            try
            {
                this.database = await this.documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = Constant.DatabaseId }, requestOptions);
            }
            catch (DocumentClientException ex)
            {
                if (ex.Error?.Message?.Contains("SharedOffer is Disabled") ?? false)
                {
                    this.telemetryClient.TrackTrace("Database shared offer is disabled for the account, will provision throughput at container level", SeverityLevel.Information);
                    useSharedOffer = false;

                    this.database = await this.documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = Constant.DatabaseId });
                }
                else
                {
                    throw;
                }
            }

            // Get a reference to the Teams collection, creating it if needed
            var teamsCollectionDefinition = new DocumentCollection
            {
                Id = Constant.TeamsCollectionId,
            };

            teamsCollectionDefinition.PartitionKey.Paths.Add("/id");
            this.teamsCollection = await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(this.database.SelfLink, teamsCollectionDefinition, useSharedOffer ? null : requestOptions);

            // Get a reference to the Users collection, creating it if needed
            var usersCollectionDefinition = new DocumentCollection
            {
                Id = Constant.UsersCollectionId,
            };

            usersCollectionDefinition.PartitionKey.Paths.Add("/aadObjectId");
            this.usersCollection = await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(this.database.SelfLink, usersCollectionDefinition, useSharedOffer ? null : requestOptions);

            // Get a reference to the userTeamMembership collection, creating it if needed
            var userTeamMembershipCollectionDefinition = new DocumentCollection
            {
                Id = Constant.UserTeamMembershipCollectionId,
            };

            userTeamMembershipCollectionDefinition.PartitionKey.Paths.Add("/userTeamsId");
            this.userTeamMembershipCollection = await this.documentClient.CreateDocumentCollectionIfNotExistsAsync(this.database.SelfLink, userTeamMembershipCollectionDefinition, useSharedOffer ? null : requestOptions);

            this.telemetryClient.TrackTrace("Data store initialized");
        }

        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }
    }
}