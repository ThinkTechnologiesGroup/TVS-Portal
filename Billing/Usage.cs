using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace ThinkVoipTool.Billing
{
    public class Usage
    {
        private readonly RestClient _client;
        private readonly RestRequest _request;
        private BillingResponse _monthly;


        public Usage(Month month, string? clientUrl)
        {
            if(int.Parse(month.MonthNumber) == DateTime.Today.Month)
            {
                month.LastDay = DateTime.Today.Day.ToString();
            }

            var authToken = "Bearer " + MainWindow.SkySwitchToken.Token;
            var url = "https://pbx.skyswitch.com/ns-api/?type=Off-net&object=cdr2&action=count&format=json&domain=" +
                      $"{clientUrl}&range_interval=5%20HOUR&end_date={month.Year}-{month.MonthNumber}-{month.LastDay}" +
                      $"%2023:59:59&start_date={month.Year}-{month.MonthNumber}-{month.StartDay}%2000:00:00";
            _client = new RestClient(url);
            _request = new RestRequest(Method.POST);
            _request.AddHeader("Authorization", authToken);
            _request.AlwaysMultipartFormData = true;
        }

        private async Task Monthly()
        {
            var response = await _client.ExecutePostAsync(_request).ConfigureAwait(false);
            _monthly = JsonConvert.DeserializeObject<BillingResponse>(response.Content);
        }

        public async Task<string> MonthlyMinutes()
        {
            if(_monthly == null)
            {
                await Monthly();
            }

            return _monthly.MinutesUsed;
        }

        public async Task<string> MonthlyCalls()
        {
            if(_monthly == null)
            {
                await Monthly();
            }

            return _monthly.TotalCalls;
        }
    }
}