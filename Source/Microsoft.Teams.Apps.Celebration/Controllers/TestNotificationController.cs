// <copyright file="TestNotificationController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.Celebration.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using Microsoft.Teams.Apps.Celebration.Helpers;
    using Microsoft.Teams.Apps.Common.Models;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// Controller to test event notification.
    /// </summary>
    [RoutePrefix("api/TestNotification")]
    public class TestNotificationController : ApiController
    {
        /// <summary>
        /// Trigger the preview controller
        /// </summary>
        /// <param name="currentDateTime">Represents DateTime string to trigger the preview</param>
        /// <returns>HttpResponseMessage</returns>
        [HttpGet]
        [Route("SimulatePreviewTrigger")]
        public HttpResponseMessage SimulatePreviewTrigger(string currentDateTime)
        {
            Token token = GetToken();

            // Send request to preview controller.
            this.SendRequest("Preview", currentDateTime);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Trigger event notification controller
        /// </summary>
        /// <param name="currentDateTime">Represents DateTime string to trigger the event</param>
        /// <returns>HttpResponseMessage</returns>
        [HttpGet]
        [Route("SimulateEventNotificationTrigger")]
        public HttpResponseMessage SimulateEventNotificationTrigger(string currentDateTime)
        {
            // Send request to preview controller.
            this.SendRequest("EventNotification", currentDateTime);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Get token information
        /// </summary>
        /// <returns>Token</returns>
        private static Token GetToken()
        {
            const string authUrl = "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token";
            var clientAddress = new RestClient(authUrl);
            var requestType = new RestRequest(Method.POST);

            requestType.AddHeader("cache-control", "no-cache");
            requestType.AddHeader("content-type", "application/x-www-form-urlencoded");
            requestType.AddParameter($"application/x-www-form-urlencoded", "grant_type=client_credentials&client_id=" + ApplicationSettings.MicrosoftAppId + "&client_secret=" + ApplicationSettings.MicrosoftAppPassword + "&scope=" + ApplicationSettings.MicrosoftAppId + "/.default", ParameterType.RequestBody);
            IRestResponse restResponse = clientAddress.Execute(requestType);

            return JsonConvert.DeserializeObject<Token>(restResponse.Content);
        }

        private void SendRequest(string controllerName, string currentDateTime)
        {
            Token token = GetToken();

            RestClient client = new RestClient(ApplicationSettings.BaseUrl);
            RestRequest request = new RestRequest($"/api/{controllerName}", Method.POST);
            request.AddHeader("Authorization", token.TokenType + " " + token.AccessToken);
            request.AddQueryParameter("currentDateTime", currentDateTime.ToString());
            request.AddHeader("Content-Type", "application/json");
            var response = client.Execute(request);
        }
    }
}