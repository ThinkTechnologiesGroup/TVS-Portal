using System;
using Newtonsoft.Json;
using RestSharp;
using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool.Billing
{
    public class Usage
    {
        private BillingResponse _monthly;


        public Usage(Month month, string clientUrl)
        {
            if(int.Parse(month.MonthNumber) == DateTime.Today.Month)
            {
                month.LastDay = DateTime.Today.Day.ToString();
            }

            var token = new SkySwitchToken();
            var authToken = "Bearer " + token.Token;
            var url =
                "https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=" +
                $"{clientUrl}&range_interval=5%20HOUR&end_date={month.Year}-{month.MonthNumber}-{month.LastDay}" +
                $"%2023:59:59&start_date={month.Year}-{month.MonthNumber}-{month.StartDay}%2000:00:00";
            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", authToken);
            request.AlwaysMultipartFormData = true;
            var response = client.Execute(request);
            _monthly = JsonConvert.DeserializeObject<BillingResponse>(response.Content);
        }


        public BillingResponse Monthly
        {
            get => _monthly;
            set => _monthly = value;
        }
    }
}