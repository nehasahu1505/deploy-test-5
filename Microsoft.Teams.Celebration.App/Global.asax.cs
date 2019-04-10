// <copyright file="Global.asax.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System;
    using System.Web.Http;
    using System.Web.Routing;
    using Autofac;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Celebration.App.Helpers;

    /// <summary>
    /// Define the events and data accessed globally in application.
    /// </summary>
    public class Global : System.Web.HttpApplication
    {
        /// <summary>
        /// Application start event.
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var store = new DocumentDbBotDataStore(new Uri(ApplicationSettings.DocumentDbUrl), ApplicationSettings.DocumentDbKey);

            Conversation.UpdateContainer(
                        builder =>
                        {
                           builder.Register(c => store)
                                .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                                .AsSelf()
                                .SingleInstance();

                           builder.Register(c => new CachingBotDataStore(store, CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                                .As<IBotDataStore<BotData>>()
                                .AsSelf()
                                .InstancePerLifetimeScope();
                        });
        }
    }
}
