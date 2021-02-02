using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool.Billing
{
    internal class Billing
    {
        private readonly List<(Month,Usage)> _lastSixMonths;

        public Billing(string clientUrl)
        {
            _lastSixMonths = new List<(Month,Usage)>();
            var token = new SkySwitchToken();
            var currentMonth = DateTime.Today.Month;
            for (var i = 0; i < 6; i++)
            {
                var monthNum = currentMonth - i;
                if(monthNum <= 0)
                {
                    monthNum = 12 + monthNum;
                }
                var month = new Month(monthNum);
                _lastSixMonths.Add((month,new Usage(month, "LostTreeClub.22335.service")));
            }

            foreach (var month in _lastSixMonths)
            {
                // var authToken = "Bearer " + token.Token;
                // var url = 
                //     $"https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=" +
                //     $"{clientUrl}&range_interval=5%20HOUR&end_date={month.Item1.Year}-{month.Item1.MonthNumber}-{month.Item1.LastDay}" +
                //     $"%2023:59:59&start_date={month.Item1.Year}-{month.Item1.StartDay}-{month.Item1.MonthNumber}%000:00:00";
                // var client = new RestClient(url);
                // var request = new RestRequest(Method.POST);
                // request.AddHeader("Authorization", authToken);
                // request.AlwaysMultipartFormData = true;
                // var response = client.Execute(request);
                // month.Item1.Consumed = JsonConvert.DeserializeObject<BillingResponse>(response.Content);
                

            }
            
        }

        public IEnumerable<(Month,Usage)> LastSixMonths => _lastSixMonths;



    }
}