// <copyright file="ReliableDeliveryFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ReliableDeliveryFunctionApp
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.Celebration.Notification;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// send post request to celebration bot to retry for failed messages.
    /// </summary>
    public static class ReliableDeliveryFunction
    {
        /// <summary>
        /// Timer trigger azure function that runs in every 15 minutes and sends request to retry the failed messages.
        /// </summary>
        /// <param name="myTimer">timer instance.</param>
        /// <param name="log">ILogger instance.</param>
        /// <param name="context">ExecutionContext.</param>
        [FunctionName("ReliableDelivery")]
        public static void Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Token token = GetToken();
            RestClient client = new RestClient(Environment.GetEnvironmentVariable("CONTROLLER_BASE_URL"));
            RestRequest request = new RestRequest($"api/{Environment.GetEnvironmentVariable("RELIABLE_DELIVERY_END_POINT_NAME")}", Method.POST);
            request.AddHeader("Authorization", token.TokenType + " " + token.AccessToken);
            request.AddHeader("Content-Type", "application/json");

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