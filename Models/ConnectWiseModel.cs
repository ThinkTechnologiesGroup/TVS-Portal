using System.Collections.Generic;

namespace ThinkVoipTool.Models
{
    public class ConnectWiseModel
    {
        protected string? CompanyName { get; set; }
        protected string? Codebase { get; set; }
        public string? VersionCode { get; set; }
        public string? VersionNumber { get; set; }
        public string? CompanyId { get; set; }
        public bool IsCloud { get; set; }
        protected string? SiteUrl { get; set; }
        public List<string>? AllowedOrigins { get; set; }

        public static ConnectWiseModel CreateInstance() => new ConnectWiseModel();
    }
}