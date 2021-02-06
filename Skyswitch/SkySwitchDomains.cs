using Newtonsoft.Json;

namespace ThinkVoipTool.Skyswitch
{
    public class SkySwitchDomains
    {
        private string? _description;
        //https://telco-api.skyswitch.com/accounts/c6cb9e70-42b9-11ea-b482-e365812db6e4/pbx/domains


        private string? _domain;
        private string? _reseller;


        [JsonProperty("domain")]
        public string? Domain
        {
            get => _domain;
            set => _domain = value;
        }

        [JsonProperty("reseller")]
        public string? Reseller
        {
            get => _reseller;
            set => _reseller = value;
        }

        [JsonProperty("description")]
        public string? Description
        {
            get => _description;
            set => _description = value;
        }
    }
}