using System;

namespace ThinkVoipTool.Skyswitch
{
    internal class SkyswitchAccount
    {
        private const string TvsAccountId = "c6cb9e70-42b9-11ea-b482-e365812db6e4";
        private static string _url = $"https://telco-api.skyswitch.com/accounts/{TvsAccountId}";
        private int _accountNumber;
        private DateTime _createdAt;
        private DateTime _deletedAt;

        private string _id;
        private string _name;
        private int _orginizational;
        private string _parentId;
        private DateTime _updatedAt;
    }
}