// <copyright file="EventNotification.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace PreviewFunctionApp
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Celebration.Notification;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Send post request to celebration bot to send notification on the day of event.
    /// </summary>
    public static class EventNotification
    {
        /// <summary>
        /// Timer triggered azure function that runs every hour and sends request to celebration app to send the notification in teams on the day of event.
        /// </summary>
        /// <param name="myTimer">timer instance.</param>
        /// <param name="log">ILogger instance.</param>
        /// <param name="context">ExecutionContext.</param>
        [FunctionName("EventNotification")]
        public static void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Token token = GetToken();
            RestClient client = new RestClient(Environment.GetEnvironmentVariable("CONTROLLER_BASE_URL"));
            RestRequest request = new RestRequest($"api/{Environment.GetEnvironmentVariable("EVENT_NOTIFICATION_END_POINT_NAME")}", Method.POST);
            request.AddHeader("Authorization", token.TokenType + " " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("currentDateTime", DateTimeOffset.UtcNow.ToString("o"));
            var response = client.Execute(request);
        }

        /// <summary>
        /// Get token information.
        /// </summary>
        /// <returns>Token.</returns>
        private static Token GetToken()
        {
            var clientAddress = new RestClient(Environment.GetEnvironmentVariable("AUTH_URL"));
            var requestType = new RestRequest(Method.POST);

            requestType.AddHeader("cache-control", "no-cache");
            requestType.AddHeader("content-type", "application/x-www-form-urlencoded");
            requestType.AddParameter($"application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=" + Environment.GetEnvironmentVariable("CLIENT_ID") + "&client_secret=" + Environment.GetEnvironmentVariable("CLIENT_SECRET") + "&scope=" + Environment.GetEnvironmentVariable("CLIENT_SCOPE"), ParameterType.RequestBody);
            IRestResponse restResponse = clientAddress.Execute(requestType);

            return JsonConvert.DeserializeObject<Token>(restResponse.Content);
        }
    }
}
