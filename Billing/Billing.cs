using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool.Billing
{
    internal class Billing
    {
        private readonly List<Month> _lastSixMonths;
        private readonly List<SkySwitchDomains> _skySwitchDomains;

        public Billing()
        {
            _skySwitchDomains = new List<SkySwitchDomains>();
            _lastSixMonths = new List<Month>();
            var currentMonth = DateTime.Today.Month;
            for (var i = 0; i < 6; i++)
            {
                var monthNum = currentMonth - i;
                if(monthNum <= 0)
                {
                    monthNum = 12 + monthNum;
                }

                var month = new Month(monthNum);
                _lastSixMonths.Add(month);
            }
        }

        public IEnumerable<Month> LastSixMonths => _lastSixMonths;


        public async Task<List<SkySwitchDomains>> SkySwitchDomains()
        {
            if(_skySwitchDomains.Count != 0)
            {
                return _skySwitchDomains;
            }

            //var token = new SkySwitchTelcoToken();
            var authToken = "Bearer " + MainWindow.SkySwitchTelcoToken.AccessToken;
            var url = "https://telco-api.skyswitch.com/accounts/c6cb9e70-42b9-11ea-b482-e365812db6e4/pbx/domains";
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", authToken);
            var response = await client.ExecuteAsync(request);
            var result = JsonConvert.DeserializeObject<JsonObject>(response.Content);
            foreach (JObject domain in result.Values)
            {
                var skySwitchDomain = new SkySwitchDomains
                {
                    Domain = domain["domain"]?.Value<string>(),
                    Reseller = domain["reseller"]?.Value<string>(),
                    Description = domain["description"]?.Value<string>()
                };
                _skySwitchDomains.Add(skySwitchDomain);
            }

            return _skySwitchDomains;
        }
    }
}