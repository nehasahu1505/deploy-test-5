// <copyright file="TabsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
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
        /// <returns>Manage event task module view.</returns>
        [Route("MangeEvents")]
        [HttpGet]
        public ActionResult ManageEvents()
        {
          return this.View();
        }

        /// <summary>
        /// Save user event.
        /// </summary>
        /// <param name="events">Events object.</param>
        /// <returns>Events View.</returns>
        [Route("SaveEvent")]
        public ActionResult SaveEvent(Events events)
        {
            return this.View();
        }
    }
}