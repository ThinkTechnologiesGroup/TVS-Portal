using System;
using System.Collections.Generic;
using System.Text;

namespace ThinkVoipTool.Skyswitch
{
    class SkyswitchAccount
    {
        const string TvsAccoutnId = "c6cb9e70-42b9-11ea-b482-e365812db6e4";
        static string url = $"https://telco-api.skyswitch.com/accounts/{TvsAccoutnId}";

        private string id;
        private string parent_id;
        private string name;
        private int account_number;
        private int orginizational;
        private DateTime created_at;
        private DateTime updated_at;
        private DateTime deleted_at;


        public SkyswitchAccount()
        {

        }

    }
}
