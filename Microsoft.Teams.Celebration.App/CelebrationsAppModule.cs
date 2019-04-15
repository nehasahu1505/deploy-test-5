// <copyright file="CelebrationsAppModule.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Celebration.App
{
    using System;
    using Autofac;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Common.Configuration;
    using Microsoft.Teams.Apps.Common.Logging;

    /// <summary>
    /// Autofac Module
    /// </summary>
    public class CelebrationsAppModule : Module
    {
        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var configProvider = new LocalConfigProvider();
            builder.Register(c => configProvider)
                .Keyed<IConfigProvider>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            var appInsightsLogProvider = new AppInsightsLogProvider(configProvider);
            builder.Register(c => appInsightsLogProvider)
                .Keyed<ILogProvider>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            var store = new DocumentDbBotDataStore(
                new Uri(configProvider.GetSetting(ApplicationConfig.DocumentDbUrl)),
                configProvider.GetSetting(ApplicationConfig.DocumentDbKey));
            builder.Register(c => store)
                 .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                 .AsSelf()
                 .SingleInstance();
            builder.Register(c => new CachingBotDataStore(store, CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                 .As<IBotDataStore<BotData>>()
                 .AsSelf()
                 .InstancePerLifetimeScope();
        }
    }
}