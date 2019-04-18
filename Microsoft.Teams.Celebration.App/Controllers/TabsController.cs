// <copyright file="TabsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System.Collections.Generic;
    using System.Linq;
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
                TeamDetails = new List<TeamDetails>(),
                CelebrationEvent = await EventHelper.GetTeamEventByEventId(eventId),
                TimeZonelist = Common.GetTimeZoneList(),
            };
            return this.View(manageEventModel);
        }

        /// <summary>
        /// Save user event.
        /// </summary>
        /// <param name="events">Events object.</param>
        /// <returns>Events View.</returns>
        [Route("SaveEvent")]
        public ActionResult SaveEvent(CelebrationEvent events)
        {
            return this.View();
        }
    }
}