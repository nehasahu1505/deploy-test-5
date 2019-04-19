// <copyright file="TabsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.Celebration.App.Helpers;
    using Microsoft.Teams.Celebration.App.Models;
    using Microsoft.Teams.Celebration.App.Utilities;

    /// <summary>
    /// Represents the tab action methods.
    /// </summary>
    public class TabsController : Controller
    {
        /// <summary>
        /// returns view for events tab.
        /// </summary>
        /// <param name="userObjectId">User Object Id.</param>
        /// <returns>Events View.</returns>
        [Route("Events")]
        [HttpGet]
        public async Task<ActionResult> Events(string userObjectId)
        {
            var events = await EventHelper.GetEventsbyOwnerObjectId(userObjectId).ToListAsync();
            return this.View(events);
        }

        /// <summary>
        /// Manage Events view.
        /// </summary>
        /// <param name="userObjectId">User Object Id.</param>
        /// <param name="eventId">eventId.</param>
        /// <returns>Manage event task module view.</returns>
        [Route("ManageEvents")]
        [HttpGet]
        public async Task<ActionResult> ManageEvents(string userObjectId, string eventId)
        {
            ManageEventModel manageEventModel = new ManageEventModel()
            {
                TeamDetails = new List<TeamDetails>(), // TODO : list of teams where the bot and user both in.
                CelebrationEvent = await EventHelper.GetTeamEventByEventId(eventId),
                TimeZonelist = Common.GetTimeZoneList(),
            };
            return this.View(manageEventModel);
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
            var timespan = Array.ConvertAll<string, int>(ApplicationSettings.TimeToPostCelebration.Split(':'), Convert.ToInt32);
            celebrationEvent.TimeToPostEvent = new TimeSpan(timespan[0], timespan[1], timespan[2]);

            await EventHelper.CreateNewEventAsync(celebrationEvent);
            return this.View("Events", new List<CelebrationEvent>());
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
            await EventHelper.UpdateEventAsync(celebrationEvent);
            return this.View("Events", new List<CelebrationEvent>());
        }

        /// <summary>
        /// Delete event.
        /// </summary>
        /// <param name="eventId">event Id.</param>
        /// <param name="userObjectId">User Object Id.</param>
        /// <param name="eventType">Event Type.</param>
        /// <returns>Task.</returns>
        public async Task<ActionResult> DeleteEvent(string eventId, string userObjectId, string eventType)
        {
            await EventHelper.DeleteEvent(eventId, eventType);
            return this.View("Events", new List<CelebrationEvent>());
        }
    }
}