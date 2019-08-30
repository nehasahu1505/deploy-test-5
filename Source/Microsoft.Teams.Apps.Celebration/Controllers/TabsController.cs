// <copyright file="TabsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Utilities;
    using Microsoft.Teams.Apps.Common.Extensions;
    using Microsoft.Teams.Apps.Common.Logging;
    using TimeZoneConverter;

    /// <summary>
    /// Represents the tab action methods.
    /// </summary>
    public class TabsController : Controller
    {
        private readonly EventHelper eventHelper;
        private readonly UserManagementHelper userManagementHelper;
        private readonly ILogProvider logProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabsController"/> class.
        /// </summary>
        /// <param name="eventHelper">EventHelper Instance.</param>
        /// <param name="userManagementHelper">UserManagementHelper instance.</param>
        /// <param name="logProvider">ILogProvider </param>
        public TabsController(EventHelper eventHelper, UserManagementHelper userManagementHelper, ILogProvider logProvider)
        {
            this.eventHelper = eventHelper;
            this.userManagementHelper = userManagementHelper;
            this.logProvider = logProvider;
        }

        /// <summary>
        /// returns view for events tab.
        /// </summary>
        /// <param name="userObjectId">User Object Id.</param>
        /// <returns>Events View.</returns>
        [Route("Events")]
        [HttpGet]
        public ActionResult Events(string userObjectId)
        {
            this.ViewBag.EmptyView = true;
            this.ViewBag.userObjectId = userObjectId;

            return this.View(new List<CelebrationEvent>());
        }

        /// <summary>
        /// returns view for events tab.
        /// </summary>
        /// <param name="userObjectId">User Object Id.</param>
        /// <returns>Events View.</returns>
        [Route("EventsData")]
        [HttpPost]
        public async Task<ActionResult> EventsData(string userObjectId)
        {
            this.ViewBag.EmptyView = false;
            return this.PartialView("Events", await this.GetEventsByOwnerObjectIdAsync(userObjectId));
        }

        /// <summary>
        /// Get and return TotalEvent count of user
        /// </summary>
        /// <param name="userObjectId">User Object Id</param>
        /// <returns>event count</returns>
        [HttpGet]
        public async Task<ActionResult> GetTotalEventCountOfUser(string userObjectId)
        {
            var events = await this.GetEventsByOwnerObjectIdAsync(userObjectId);

            return this.Content(events.Count.ToString());
        }

        /// <summary>
        /// Returns empty view for ManageEvent task module
        /// </summary>
        /// <param name="userObjectId">User Object Id.</param>
        /// <param name="eventId">eventId.</param>
        /// <param name="clientTimeZone">Client's machine timeZone id</param>
        /// <returns>Manage event task module view.</returns>
        [Route("ManageEvents")]
        [HttpGet]
        public ActionResult ManageEvents(string userObjectId, string eventId, string clientTimeZone)
        {
            this.ViewBag.EmptyView = true;
            this.ViewBag.userObjectId = userObjectId;
            this.ViewBag.eventId = eventId;
            this.ViewBag.clientTimeZone = clientTimeZone;

            ManageEventModel manageEventModel = new ManageEventModel()
            {
                TeamDetails = new List<Team>(),
                TimeZoneList = new List<TimeZoneDisplayInfo>(),
            };

            return this.View(manageEventModel);
        }

        /// <summary>
        /// Returns view for ManageEvent task module
        /// </summary>
        /// <param name="userObjectId">User Object Id</param>
        /// <param name="eventId">eventId</param>
        /// <param name="clientTimeZone">Client's machine timeZone id</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Route("ManageEventData")]
        [HttpPost]
        public async Task<ActionResult> ManageEventData(string userObjectId, string eventId, string clientTimeZone)
        {
            this.ViewBag.EmptyView = false;
            string windowsTimeZoneId;
            TZConvert.TryIanaToWindows(clientTimeZone, out windowsTimeZoneId);
            ManageEventModel manageEventModel = new ManageEventModel()
            {
                TeamDetails = await this.GetTeamDetailsWhereBothBotAndUsersAreInAsync(userObjectId),
                TimeZoneList = Common.GetTimeZoneList(),
                SelectedTimeZoneId = windowsTimeZoneId,
            };

            if (!string.IsNullOrWhiteSpace(eventId))
            {
                manageEventModel.CelebrationEvent = await this.eventHelper.GetEventByEventIdAsync(eventId, userObjectId);
            }

            return this.PartialView("ManageEvents", manageEventModel);
        }

        /// <summary>
        /// Save celebration event.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent object.</param>
        /// <returns>Events View.</returns>
        [Route("SaveEvent")]
        [HttpPost]
        public async Task<ActionResult> SaveEvent(CelebrationEvent celebrationEvent)
        {
            this.ViewBag.EmptyView = false;
            if (this.ModelState.IsValid)
            {
                await this.eventHelper.SaveEventAsync(celebrationEvent);
            }

            return this.View("Events", await this.GetEventsByOwnerObjectIdAsync(celebrationEvent.OwnerAadObjectId));
        }

        /// <summary>
        /// update celebration event.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent object.</param>
        /// <returns>Events View.</returns>
        [Route("UpdateEvent")]
        [HttpPost]
        public async Task<ActionResult> UpdateEvent(CelebrationEvent celebrationEvent)
        {
            this.ViewBag.EmptyView = false;
            if (this.ModelState.IsValid)
            {
                CelebrationEvent fetchedEvent = await this.eventHelper.GetEventByEventIdAsync(celebrationEvent.Id, celebrationEvent.OwnerAadObjectId);

                await this.eventHelper.SaveEventAsync(celebrationEvent);

                // If event date or timezone is changed then delete record from Occurrences and EventMessages collections.
                if (fetchedEvent.Date != celebrationEvent.Date || fetchedEvent.TimeZoneId != celebrationEvent.TimeZoneId)
                {
                    await this.eventHelper.DeleteRecurringEventAsync(celebrationEvent.Id);
                    await this.eventHelper.DeleteEventMessagesAsync(celebrationEvent.Id);

                    // Add record in Occurrences collection if recurring event is within 72 hours.
                    DateTime upcomingEventDate = Common.GetUpcomingEventDate(celebrationEvent.Date, DateTime.UtcNow.Date);

                    if ((upcomingEventDate - DateTime.Now).TotalDays <= 72)
                    {
                        await this.AddRecurringEventAsync(celebrationEvent, upcomingEventDate);
                    }
                }
            }

            return this.View("Events", await this.GetEventsByOwnerObjectIdAsync(celebrationEvent.OwnerAadObjectId));
        }

        /// <summary>
        /// Delete event.
        /// </summary>
        /// <param name="userObjectId">AadObjectId of owner.</param>
        /// <param name="eventId">event Id.</param>
        /// <returns>Task.</returns>
        [Route("DeleteEvent")]
        [HttpPost]
        public async Task<ActionResult> DeleteEvent(string userObjectId, string eventId)
        {
            this.ViewBag.EmptyView = false;
            await this.eventHelper.DeleteEventAsync(eventId, userObjectId);
            await this.eventHelper.DeleteRecurringEventAsync(eventId);
            await this.eventHelper.DeleteEventMessagesAsync(eventId);

            return this.View("Events", await this.GetEventsByOwnerObjectIdAsync(userObjectId));
        }

        /// <summary>
        /// Check if event exist in Database for given event id.
        /// </summary>
        /// <param name="eventId">event id.</param>
        /// <param name="ownerAadObjectId">AadObjectId of owner of the event.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        public async Task<ActionResult> CheckIfEventExist(string eventId, string ownerAadObjectId)
        {
            HttpStatusCode documentStatus = HttpStatusCode.NotFound;
            var document = await this.eventHelper.GetEventByEventIdAsync(eventId, ownerAadObjectId);
            if (document != null)
            {
                documentStatus = HttpStatusCode.OK;
            }

            return this.Content(documentStatus.ToString());
        }

        /// <summary>
        /// Returns Team details where bot and users both are in.
        /// </summary>
        /// <param name="userObjectId">AadObjectId of user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<List<Team>> GetTeamDetailsWhereBothBotAndUsersAreInAsync(string userObjectId)
        {
            List<Team> teamDetails;
            try
            {
                var user = await this.userManagementHelper.GetUserByAadObjectIdAsync(userObjectId);
                var userTeamMembership = await this.userManagementHelper.GetUserTeamMembershipByTeamsIdAsync(user.TeamsId);
                teamDetails = await (await this.userManagementHelper.GetTeamsDetailsByTeamIdsAsync(userTeamMembership.Select(x => x.TeamId).ToList())).ToListAsync();

                teamDetails = teamDetails.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                this.logProvider.LogError("Failed to get Team details from method GetTeamDetailsWhereBothBotAndUsersAreIn. error:" + ex.ToString());
                teamDetails = new List<Team>();
            }

            return teamDetails;
        }

        /// <summary>
        /// Add Recurring event.
        /// </summary>
        /// <param name="celebrationEvent">CelebrationEvent instance.</param>
        /// <param name="upcomingEventDate">upcoming event date.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task AddRecurringEventAsync(CelebrationEvent celebrationEvent, DateTime upcomingEventDate)
        {
            var timespan = Array.ConvertAll<string, int>(ApplicationSettings.TimeToPostCelebration.Split(':'), Convert.ToInt32);
            DateTime upcomingEventDateTime = upcomingEventDate.AddHours(timespan[0]).AddMinutes(timespan[1]).AddSeconds(timespan[2]);
            DateTimeOffset upcomingEventDateTimeInUTC = TimeZoneInfo.ConvertTimeToUtc(upcomingEventDateTime, TimeZoneInfo.FindSystemTimeZoneById(celebrationEvent.TimeZoneId));

            EventOccurrence eventOccurrence = new EventOccurrence
            {
                EventId = celebrationEvent.Id,
                Date = upcomingEventDateTimeInUTC,
            };

            await this.eventHelper.AddRecurringEventAsync(eventOccurrence);
        }

        private async Task<List<CelebrationEvent>> GetEventsByOwnerObjectIdAsync(string userObjectId)
        {
            return await (await this.eventHelper.GetEventsByOwnerObjectIdAsync(userObjectId)).ToListAsync();
        }
    }
}