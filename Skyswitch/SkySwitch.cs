using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ThinkVoipTool.Skyswitch
{
    internal class SkySwitchBilling
    {
        // [JsonProperty("call_limit")]
        // private string _callLimit;
        //
        // [JsonProperty("current_phonenumbers")]
        // private string _currentPhoneNumbers;
        //
        // [JsonProperty("description")]
        // private string _description;

        [JsonProperty("domain")]
        private string _domain;

        [JsonProperty("territory")]
        private int _territory;


        private SkySwitchBilling(SkySwitchToken token, string domainNameString)
        {
            const string baseUrl = "https://pbx.skyswitch.com/ns-api/";

            var restClient = new RestClient(baseUrl);
            var restRequest = new RestRequest(Method.POST);
            restRequest.AddHeader("Authorization", "Bearer " + token.Token);
            restRequest.AddParameter("object", "domain");
            restRequest.AddParameter("action", "read");
            restRequest.AddParameter("billing", "yes");
            restRequest.AddParameter("domain", domainNameString);
            var response = restClient.Execute(restRequest).Content;
            var deserializedResponse = JsonConvert.DeserializeObject<JArray>(response);

            //_callLimit = deserializedResponse[0]["call_limit"]?.ToString();
            //_currentPhoneNumbers = deserializedResponse[0]["current_phonenumbers"]?.ToString();
            //_description = deserializedResponse[0]["description"]?.ToString();
            _territory = int.Parse(deserializedResponse[0]["territory"]?.ToString()!);
            _domain = deserializedResponse[0]["domain"]?.ToString()!;
        }

        public static SkySwitchBilling CreateInstance(SkySwitchToken token, string domainNameString) => new SkySwitchBilling(token, domainNameString);
    }
}