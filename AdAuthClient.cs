using System;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.Net;

namespace ThinkVoipTool
{
    public class AdAuthClient
    {
        private LdapConnection connection;

        public AdAuthClient(string domain = "ttg.local", string url = "auth.think-team.com")
        {
            var user = ConfigurationManager.AppSettings["AdAuthUser"];
            var pass = ConfigurationManager.AppSettings["AdAuthPass"];
            var credentials = new NetworkCredential(user, pass, domain);
            var serverId = new LdapDirectoryIdentifier(url);

            connection = new LdapConnection(serverId, credentials);
            connection.Bind();

        }

        public bool validateUserByBind(string username, string password)
        {
            bool result = true;
            var credentials = new NetworkCredential(username, password);
            var serverId = new LdapDirectoryIdentifier(connection.SessionOptions.HostName);

            var conn = new LdapConnection(serverId, credentials);
            try
            {
                conn.Bind();
            }
            catch (Exception)
            {
                result = false;
            }

            conn.Dispose();

            return result;
        }

    }
}
