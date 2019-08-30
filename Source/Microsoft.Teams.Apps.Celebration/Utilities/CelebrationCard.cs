// <copyright file="CelebrationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Utilities
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Connector;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Celebration.Models;
    using Microsoft.Teams.Apps.Celebration.Resources;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// utility class for celebration bot cards.
    /// </summary>
    public static class CelebrationCard
    {
        /// <summary>
        /// Create and return preview card.
        /// </summary>
        /// <param name="eventMessageActivity">EventMessageActivity instance</param>
        /// <param name="isSkipAllowed">true/false</param>
        /// <returns>AdaptiveCard.</returns>
        public static HeroCard GetPreviewCard(EventMessageActivity eventMessageActivity, bool isSkipAllowed = true)
        {
            string imagePath = Common.GetImagePath(eventMessageActivity.ImageUrl);

            List<CardAction> cardActions = new List<CardAction>()
            {
                new CardAction()
                {
                    Title = "Edit",
                    Type = "openUrl",
                    Value = new Uri(Common.GetDeepLinkUrlToEventsTab("EventsTab", "Tabs/Events", "{\"subEntityId\":\"" + eventMessageActivity.Id + "\"}")),
                },
            };

            if (isSkipAllowed)
            {
                cardActions.Insert(0, new CardAction()
                {
                    Title = "Skip",
                    Type = "invoke",
                    Value = JObject.FromObject(new PreviewCardPayload
                    {
                        Action = "SkipEvent",
                        EventId = eventMessageActivity.Id,
                        OwnerAadObjectId = eventMessageActivity.OwnerAadObjectId,
                        OwnerName = eventMessageActivity.OwnerName,
                        Title = eventMessageActivity.Title,
                        UpcomingEventDate = Common.GetUpcomingEventDate(eventMessageActivity.EventDate, DateTime.UtcNow.Date),
                    }).ToString(),
                });
            }

            HeroCard previewCard = new HeroCard()
            {
                Title = string.Format(Strings.PreviewHeader, eventMessageActivity.OwnerName, eventMessageActivity.Title) + "\n\n",
                Text = eventMessageActivity.Message,
                Buttons = cardActions,
                Images = new List<CardImage>() { new CardImage(url: imagePath) },
            };

            return previewCard;
        }

        /// <summary>
        /// Create and return CelebrationEvent card.
        /// </summary>
        /// <param name="eventMessageActivity">EventMessageActivity Instance.</param>
        /// <returns>HeroCard</returns>
        public static HeroCard GetEventCard(EventMessageActivity eventMessageActivity)
        {
            string imagePath = Common.GetImagePath(eventMessageActivity.ImageUrl);
            List<CardImage> cardImages = new List<CardImage>() { new CardImage(url: imagePath) };

            HeroCard eventCard = new HeroCard()
            {
                Title = string.Format(Strings.EventCardTitle, eventMessageActivity.OwnerName, eventMessageActivity.Title) + "\n\n",
                Text = eventMessageActivity.Message,
                Images = cardImages,
            };

            return eventCard;
        }

        /// <summary>
        /// Create and return welcome card for installer of bot.
        /// </summary>
        /// <returns>AdaptiveCard</returns>
        public static AdaptiveCard GetWelcomeCardForInstaller()
        {
            AdaptiveCard welcomeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = "60",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveImage()
                                            {
                                                Url = new Uri(Common.GetWelcomeImage("celebration_bot_full-color.png")),
                                                Size = AdaptiveImageSize.Medium,
                                                Style = AdaptiveImageStyle.Default,
                                            },
                                        },
                                    },
                                    new AdaptiveColumn()
                                    {
                                        Width = "400",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = Strings.WelcomeMessagePart1,
                                                Size = AdaptiveTextSize.Default,
                                                Wrap = true,
                                                Weight = AdaptiveTextWeight.Default,
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                              Text = Strings.WelcomeMessagePart2,
                                              Size = AdaptiveTextSize.Default,
                                              Wrap = true,
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                              Text = Strings.WelcomeMessagePart3,
                                              Size = AdaptiveTextSize.Default,
                                              Wrap = true,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Get started",
                        Url = new Uri(Common.GetDeepLinkUrlToEventsTab("EventsTab", "Tabs/Events")),
                    },
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Take a tour",
                        Url = new Uri(TakeATourUrl()),
                    },
                },
            };

            return welcomeCard;
        }

        /// <summary>
        /// Create and return welcome card as a reply.
        /// </summary>
        /// <returns>AdaptiveCard</returns>
        public static AdaptiveCard GetWelcomeCardInResponseToUserMessage()
        {
            AdaptiveCard welcomeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = "60",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveImage()
                                            {
                                                Url = new Uri(Common.GetWelcomeImage("celebration_bot_full-color.png")),
                                                Size = AdaptiveImageSize.Medium,
                                                Style = AdaptiveImageStyle.Default,
                                            },
                                        },
                                    },
                                    new AdaptiveColumn()
                                    {
                                        Width = "400",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Text = "Hi!",
                                                Size = AdaptiveTextSize.Large,
                                                Weight = AdaptiveTextWeight.Bolder,
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                                Text = Strings.WelcomeMessagePart4,
                                                Size = AdaptiveTextSize.Default,
                                                Wrap = true,
                                                Spacing = AdaptiveSpacing.None,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Get started",
                        Url = new Uri(Common.GetDeepLinkUrlToEventsTab("EventsTab", "Tabs/Events")),
                    },
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Take a tour",
                        Url = new Uri(TakeATourUrl()),
                    },
                },
            };

            return welcomeCard;
        }

        /// <summary>
        /// Create and return welcome card for team members and general channel.
        /// </summary>
        /// <param name="botInstallerName">bot installer name.</param>
        /// <param name="teamName">TeamName</param>
        /// <returns>AdaptiveCard</returns>
        public static AdaptiveCard GetWelcomeMessageForGeneralChannelAndTeamMembers(string botInstallerName, string teamName)
        {
            AdaptiveCard welcomeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet()
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn()
                                    {
                                        Width = "60",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveImage()
                                            {
                                                // TODO:Change the URL
                                                Url = new Uri(Common.GetWelcomeImage("celebration_bot_full-color.png")),
                                                Size = AdaptiveImageSize.Medium,
                                                Style = AdaptiveImageStyle.Default,
                                            },
                                        },
                                    },
                                    new AdaptiveColumn()
                                    {
                                        Width = "400",
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock()
                                            {
                                                Text = "Hi!",
                                                Size = AdaptiveTextSize.Large,
                                                Weight = AdaptiveTextWeight.Bolder,
                                            },
                                            new AdaptiveTextBlock()
                                            {
                                                Text = string.Format(Strings.WelcomeMessageForTeam, botInstallerName, teamName),
                                                Size = AdaptiveTextSize.Default,
                                                Wrap = true,
                                                Spacing = AdaptiveSpacing.None,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Get started",
                        Url = new Uri(Common.GetDeepLinkUrlToEventsTab("EventsTab", "Tabs/Events")),
                    },
                    new AdaptiveOpenUrlAction()
                    {
                        Title = "Take a tour",
                        Url = new Uri(TakeATourUrl()),
                    },
                },
            };

            return welcomeCard;
        }

        /// <summary>
        /// Create and return attachment to share the existing events of user with team.
        /// </summary>
        /// <param name="teamId">Team id</param>
        /// <param name="teamName">Team name to share the event with.</param>
        /// <param name="userAadObjectId">AadObject Id of user</param>
        /// <returns>Attachment</returns>
        public static Attachment GetShareEventAttachement(string teamId, string teamName, string userAadObjectId)
        {
            return new HeroCard()
            {
                Text = string.Format(Strings.EventShareMessage, teamName),
                Buttons = new List<CardAction>()
                            {
                                new CardAction()
                                {
                                    Title = "Share",
                                    DisplayText = "Share",
                                    Type = ActionTypes.MessageBack,
                                    Text = "Share",
                                    Value = JObject.FromObject(new ShareEventPayload
                                    {
                                        Action = "ShareEvent",
                                        TeamId = teamId,
                                        TeamName = teamName,
                                        UserAadObjectId = userAadObjectId,
                                    }),
                                },

                                new CardAction()
                                {
                                    Title = "No, thanks",
                                    DisplayText = "No Thanks",
                                    Type = ActionTypes.MessageBack,
                                    Text = "No Thanks",
                                    Value = JObject.FromObject(new ShareEventPayload
                                    {
                                        Action = "IgnorEventShare",
                                        TeamId = teamId,
                                        TeamName = teamName,
                                        UserAadObjectId = userAadObjectId,
                                    }),
                                },
                            },
            }.ToAttachment();
        }

        /// <summary>
        /// Create and return attachment to share the existing events of user with team.
        /// </summary>
        /// <param name="teamName">Team name to share the event with.</param>
        /// <returns>Attachment</returns>
        public static Attachment GetShareEventAttachementWithoutActionButton(string teamName)
        {
            return new HeroCard()
            {
                Text = string.Format(Strings.EventShareMessage, teamName),
            }.ToAttachment();
        }

        /// <summary>
        /// Create a URL for Take a tour action button.
        /// </summary>
        /// <returns>Take a tour URL.</returns>
        private static string TakeATourUrl()
        {
            var baseUrl = ApplicationSettings.BaseUrl;
            var htmlUrl = Uri.EscapeDataString($"{baseUrl}/Content/tour.html?theme={{theme}}");
            var tourTitle = "Tour";
            var appId = ApplicationSettings.ManifestAppId;
            return $"https://teams.microsoft.com/l/task/{appId}?url={htmlUrl}&height=533px&width=600px&title={tourTitle}";
        }
    }
}