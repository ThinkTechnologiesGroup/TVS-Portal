﻿using System;

using Newtonsoft.Json;

using RestSharp;

namespace ThinkVoipTool
{
    public class SkySwitchToken
    {
        [JsonProperty("access_token")]
        private readonly string _accessToken;

        [JsonProperty("apiversion")]
        private string _apiVersion;

        [JsonProperty("domain")]
        private string _domain;

        [JsonProperty("expires_in")]
        private int _expiresIn;

        [JsonProperty("legacy")]
        private bool _legacy;

        [JsonProperty("refresh_token")]
        private string _refreshToken;

        [JsonProperty("scope")]
        private string _scope;

        [JsonProperty("token_type")]
        private string _tokenType;

        public DateTime ExpirationTime;

        public SkySwitchToken()
        {
            var restClient = new RestClient("https://pbx.skyswitch.com/ns-api/oauth2/token/");
            var restRequest = new RestRequest(Method.POST);
            const string userName = "api@thinkvoipservices";
            const string passWord = "HqR7k4Zm";
            const string clientId = "22335.client";
            const string clientSecret = "47a19dee95778610121a7d43b0f708bd";
            const string scope = "*";
            restRequest.AddParameter("grant_type", "password");
            restRequest.AddParameter("username", userName);
            restRequest.AddParameter("password", passWord);
            restRequest.AddParameter("scope", scope);
            restRequest.AddParameter("client_id", clientId);
            restRequest.AddParameter("client_secret", clientSecret);
            JsonConvert.PopulateObject(restClient.Execute(restRequest).Content, this);
            ExpirationTime = DateTime.Now.AddMinutes(_expiresIn - 15);
        }

        public string Token
        {
            get
            {
                if (IsExpired(this))
                {
                    SkySwitchTokenRefresh();
                }

                return _accessToken;
            }
        }

        private bool IsExpired(SkySwitchToken token) => token.ExpirationTime > DateTime.Now ? true : false;

        public void SkySwitchTokenRefresh()
        {
            var restClient = new RestClient("https://pbx.skyswitch.com/ns-api/oauth2/token/");
            var restRequest = new RestRequest(Method.POST);
            const string clientId = "22335.client";
            const string clientSecret = "47a19dee95778610121a7d43b0f708bd";
            restRequest.AddParameter("grant_type", "refresh_token");
            restRequest.AddParameter("client_id", clientId);
            restRequest.AddParameter("client_secret", clientSecret);
            restRequest.AddParameter("refresh_token", _refreshToken);
            var testRequest = restRequest;
            var test = restClient.Execute(restRequest);
            JsonConvert.PopulateObject(restClient.Execute(restRequest).Content, this);
            ExpirationTime = DateTime.Now.AddMinutes(_expiresIn - 15);
        }
    }
}