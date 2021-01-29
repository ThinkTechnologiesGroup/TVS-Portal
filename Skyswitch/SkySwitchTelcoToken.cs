using System;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace ThinkVoipTool
{

    internal class SkySwitchTelcoToken
    {
        const string userName = "sky-api@thinkvoipservices.com";
        const string passWord = "cqb7sn48q7bnj";
        const string clientId = "91";
        const string clientSecret = "xntm584xcne8csdbxsh8e6muay3y5sjpcqngqm";
        const string scope = "*";

        static RestClient restClient = new RestClient("https://telco-api.skyswitch.com/oauth/token");
        static RestRequest restRequest = new RestRequest(Method.POST);

        [JsonProperty("access_token")]
        private string _accessToken;

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

        public SkySwitchTelcoToken()
        {

            restRequest.AddParameter("grant_type", "password");
            restRequest.AddParameter("username", userName);
            restRequest.AddParameter("password", passWord);
            restRequest.AddParameter("scope", scope);
            restRequest.AddParameter("client_id", clientId);
            restRequest.AddParameter("client_secret", clientSecret);

        }


        public string accessToken
        {
            get
            {
                if (_accessToken == null)
                {
                    JsonConvert.PopulateObject(restClient.Execute(restRequest).Content, this);
                    ExpirationTime = DateTime.Now.AddMinutes(_expiresIn - 15);
                }
                if (IsExpired(this))
                {
                    _ = RefreshToken(this);
                }
                return _accessToken;
            }
            set
            {
                _accessToken = value;
            }
        }


        private bool IsExpired(SkySwitchTelcoToken token) => this.ExpirationTime > DateTime.Now ? true : false;

        public async Task RefreshToken(SkySwitchTelcoToken token)
        {
            var restClient = new RestClient("https://telco-api.skyswitch.com/oauth/token");
            var restRequest = new RestRequest(Method.POST);
            restRequest.AddParameter("grant_type", "refresh_token");
            restRequest.AddParameter("client_id", clientId);
            restRequest.AddParameter("client_secret", clientSecret);
            restRequest.AddParameter("refresh_token", _refreshToken);
            JsonConvert.PopulateObject((await restClient.ExecuteAsync(restRequest).ConfigureAwait(false)).Content, token);
            ExpirationTime = DateTime.Now.AddMinutes(_expiresIn - 15);
        }

    }
}