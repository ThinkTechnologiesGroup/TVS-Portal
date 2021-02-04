using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace ThinkVoipTool.Skyswitch
{
    public class SkySwitchTelcoToken
    {
        private const string UserName = "sky-api@thinkvoipservices.com";
        private const string PassWord = "cqb7sn48q7bnj";
        private const string ClientId = "91";
        private const string ClientSecret = "xntm584xcne8csdbxsh8e6muay3y5sjpcqngqm";
        private const string Scope = "*";

        private static readonly RestClient RestClient = new RestClient("https://telco-api.skyswitch.com/oauth/token");
        private static readonly RestRequest RestRequest = new RestRequest(Method.POST);

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
            RestRequest.AddParameter("grant_type", "password");
            RestRequest.AddParameter("username", UserName);
            RestRequest.AddParameter("password", PassWord);
            RestRequest.AddParameter("scope", Scope);
            RestRequest.AddParameter("client_id", ClientId);
            RestRequest.AddParameter("client_secret", ClientSecret);
        }


        public string AccessToken
        {
            get
            {
                if(_accessToken == null)
                {
                    JsonConvert.PopulateObject(RestClient.Execute(RestRequest).Content, this);
                    ExpirationTime = DateTime.Now.AddMinutes(_expiresIn - 15);
                }

                if(IsExpired())
                {
                    _ = RefreshToken(this);
                }

                return _accessToken;
            }
            set => _accessToken = value;
        }


        private bool IsExpired() => ExpirationTime > DateTime.Now;

        public async Task RefreshToken(SkySwitchTelcoToken token)
        {
            var restClient = new RestClient("https://telco-api.skyswitch.com/oauth/token");
            var restRequest = new RestRequest(Method.POST);
            restRequest.AddParameter("grant_type", "refresh_token");
            restRequest.AddParameter("client_id", ClientId);
            restRequest.AddParameter("client_secret", ClientSecret);
            restRequest.AddParameter("refresh_token", _refreshToken);
            JsonConvert.PopulateObject((await restClient.ExecuteAsync(restRequest).ConfigureAwait(false)).Content, token);
            ExpirationTime = DateTime.Now.AddMinutes(_expiresIn - 15);
        }
    }
}