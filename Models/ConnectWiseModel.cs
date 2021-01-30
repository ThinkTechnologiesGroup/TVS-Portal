using System.Collections.Generic;

namespace ThinkVoipTool.Models
{
    public class ConnectWiseModel
    {
        public string CompanyName { get; set; }
        public string Codebase { get; set; }
        public string VersionCode { get; set; }
        public string VersionNumber { get; set; }
        public string CompanyId { get; set; }
        public bool IsCloud { get; set; }
        public string SiteUrl { get; set; }
        public List<string> AllowedOrigins { get; set; }
    }
}