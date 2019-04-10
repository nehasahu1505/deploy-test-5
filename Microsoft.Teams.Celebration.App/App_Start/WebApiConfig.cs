// <copyright file="WebApiConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System.Web.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents Web Api configurations.
    /// </summary>
    public static class WebApiConfig
    {
        /// <summary>
        /// Register the routes and configuration settings.
        /// </summary>
        /// <param name="config">config.</param>
        public static void Register(HttpConfiguration config)
        {
            // Json settings
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Include;
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
            };

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
               name: "DefaultApi",
               routeTemplate: "api/{controller}/{id}",
               defaults: new { id = RouteParameter.Optional });
        }
    }
}
