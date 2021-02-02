using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using RestSharp;
using ThinkVoipTool.Skyswitch;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ThinkVoipTool.Billing
{
    internal class Billing
    {
        private readonly List<Month> _lastSixMonths;
        private SkySwitchToken _token;

        public Billing(string clientUrl)
        {
            _lastSixMonths = new List<Month>();
            _token = new SkySwitchToken();
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

            foreach (var month in _lastSixMonths)
            {
                var authToken = "Bearer " + _token.Token;
                var url = 
                    $"https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=" +
                    $"{clientUrl}&range_interval=5%20HOUR&end_date={month.Year}-{month.MonthNumber}-{month.LastDay}" +
                    $"%2023:59:59&start_date={month.Year}-{month.StartDay}-{month.MonthNumber}%000:00:00";
                var client = new RestClient(url);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", authToken);
                request.AlwaysMultipartFormData = true;
                var response = client.Execute(request);
                month.Consumed = JsonConvert.DeserializeObject<BillingResponse>(response.Content);

            }
            
        }

        public IEnumerable<Month> LastSixMonths => _lastSixMonths;



    }
}