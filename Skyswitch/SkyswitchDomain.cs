using System;
using System.Collections.Generic;
using System.Text;

namespace ThinkVoipTool.Skyswitch
{
    class SkyswitchDomain
    {
        const string TvsAccoutnId = "c6cb9e70-42b9-11ea-b482-e365812db6e4";
        static string url = $"https://telco-api.skyswitch.com/accounts/{TvsAccoutnId}/pbx/domains";

        private string domain;
        private string reseller;
        private string description;

        public SkyswitchDomain()
        {

        }


      
    }
}
