using Newtonsoft.Json;

namespace ThinkVoipTool.Billing
{
    public class BillingResponse
    {
        private int _minutesUsed;
        private int _secondsUsed;
        private int _totalCalls;


        [JsonProperty("total")]
        public string TotalCalls
        {
            get => _totalCalls.ToString();
            set => _totalCalls = int.Parse(value);
        }

        [JsonProperty("minutes")]
        public string MinutesUsed
        {
            get => _minutesUsed.ToString();
            set => _minutesUsed = int.Parse(value);
        }

        [JsonProperty("seconds")]
        public string SecondsUsed
        {
            get => _secondsUsed.ToString();
            set => _secondsUsed = int.Parse(value);
        }
    }
}