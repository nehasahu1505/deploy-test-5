// <copyright file="FontAwesomeBundleConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Web.Optimization;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(Microsoft.Teams.Celebration.App.App_Start.FontAwesomeBundleConfig), "RegisterBundles")]

namespace Microsoft.Teams.Celebration.App.App_Start
{
    /// <summary>
    /// Config fontawesome bundles
    /// </summary>
    public class FontAwesomeBundleConfig
    {
        /// <summary>
        /// Configures bundles.
        /// </summary>
        public static void RegisterBundles()
        {
            // Add @Styles.Render("~/Content/fontawesome") in the <head/> of your _Layout.cshtml view
            // When <compilation debug="true" />, MVC will render the full readable version. When set to <compilation debug="false" />, the minified version will be rendered automatically
            BundleTable.Bundles.Add(new StyleBundle("~/Content/fontawesome").Include("~/Content/fontawesome/font-awesome.css"));
        }
    }
}