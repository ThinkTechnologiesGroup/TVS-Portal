using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool.Billing
{
    public class Usage
    {
        private readonly RestClient _client;
        private readonly RestRequest _request;
        private readonly string _url;
        private BillingResponse _monthly;


        public Usage(Month month, string clientUrl)
        {
            if(int.Parse(month.MonthNumber) == DateTime.Today.Month)
            {
                month.LastDay = DateTime.Today.Day.ToString();
            }

            var token = new SkySwitchToken();
            var authToken = "Bearer " + token.Token;
            _url =
                "https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=" +
                $"{clientUrl}&range_interval=5%20HOUR&end_date={month.Year}-{month.MonthNumber}-{month.LastDay}" +
                $"%2023:59:59&start_date={month.Year}-{month.MonthNumber}-{month.StartDay}%2000:00:00";
            _client = new RestClient(_url);
            _request = new RestRequest(Method.POST);
            _request.AddHeader("Authorization", authToken);
            _request.AlwaysMultipartFormData = true;
        }

        public async Task<string> Monthly()
        {
            var response = await _client.ExecutePostAsync(_request).ConfigureAwait(false);
            _monthly = JsonConvert.DeserializeObject<BillingResponse>(response.Content);
            return _monthly.MinutesUsed;
        }

        // public BillingResponse Monthly
        // {
        //     get => _monthly;
        //     set => _monthly = value;
        // }
    }
}