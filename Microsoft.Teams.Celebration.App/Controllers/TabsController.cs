// <copyright file="TabsController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System.Web.Mvc;

    /// <summary>
    /// Represents the tab action methods.
    /// </summary>
    public class TabsController : Controller
    {
        /// <summary>
        /// returns view for configuration tab.
        /// </summary>
        /// <returns>Configuration tab view.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// returns view for events tab.
        /// </summary>
        /// <returns>Events tab view.</returns>
        [HttpGet]
        public ActionResult Events()
        {
            return this.View();
        }
    }
}