namespace ThinkVoipTool.Skyswitch
{
    internal class SkyswitchDomain
    {
        private const string TvsAccoutnId = "c6cb9e70-42b9-11ea-b482-e365812db6e4";
        private static string _url = $"https://telco-api.skyswitch.com/accounts/{TvsAccoutnId}/pbx/domains";
        private string _description;

        private string _domain;
        private string _reseller;
    }
}