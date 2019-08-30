// <copyright file="CelebrationsAppModule.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration
{
    using System;
    using Autofac;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Builder.Internals.Fibers;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Dialog;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Common;
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

            builder.Register(c =>
            {
                return new TelemetryClient(new TelemetryConfiguration(configProvider.GetSetting(CommonConfig.ApplicationInsightsInstrumentationKey)));
            }).SingleInstance();

            builder.RegisterType<UserManagementHelper>().SingleInstance();
            builder.RegisterType<EventHelper>().SingleInstance();

            // Override some Bot Framework registrations
            builder.Register(c => c.Resolve<IConnectorClientFactory>().MakeConnectorClient())
                .Keyed<IConnectorClient>(FiberModule.Key_DoNotSerialize) // Tag IConnectorClient as DoNotSerialize
                .As<IConnectorClient>()
                .ExternallyOwned();

            var appInsightsLogProvider = new AppInsightsLogProvider(configProvider);
            builder.Register(c => appInsightsLogProvider)
                .Keyed<ILogProvider>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            var store = new DocumentDbBotDataStore(
                new Uri(configProvider.GetSetting(ApplicationConfig.CosmosDBEndpointUrl)),
                configProvider.GetSetting(ApplicationConfig.CosmosDBKey));
            builder.Register(c => store)
                 .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                 .AsSelf()
                 .SingleInstance();
            builder.Register(c => new CachingBotDataStore(store, CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                 .As<IBotDataStore<BotData>>()
                 .AsSelf()
                 .InstancePerLifetimeScope();

            // Register dialogs
            builder.RegisterType<RootDialog>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<DialogFactory>()
                .Keyed<DialogFactory>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .InstancePerMatchingLifetimeScope(DialogModule.LifetimeScopeTag);

            builder.RegisterType<SkipEventDialog>()
                .AsSelf()
                .InstancePerDependency();

            builder.RegisterType<ShareEventDialog>()
              .AsSelf()
              .InstancePerDependency();

            builder.RegisterType<IgnoreEventShareDialog>()
             .AsSelf()
             .InstancePerDependency();
        }
    }
}