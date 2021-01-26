using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

using ThinkVoip;

using ThinkVoipTool.Properties;

namespace ThinkVoipTool
{
    public class AdAuthClient
    {

        public static bool validateUserByBind(string username, string password, string domain = "ttg.local", string url = "auth.think-team.com")
        {
            username = username.StripDomain();
            var credentials = new NetworkCredential(username, password, domain);
            var serverId = new LdapDirectoryIdentifier(url);
            var conn = new LdapConnection(serverId, credentials);

            bool result;
            try
            {
                conn.Bind();
                result = true;
            }
            catch (Exception)
            {
                result = false;
                conn.Dispose();
                return result;
            }

            var dName = "DC=ttg,Dc=Local";
            var ldapFilter = $"samAccountName={username}";
            var AttributeList = new string[3] { @"SAMAccountName", "memberOf", "cn" };
            try
            {
                SearchResponse response = (SearchResponse)conn.SendRequest(new SearchRequest(dName, ldapFilter, SearchScope.Subtree, AttributeList));
                var memberships = response.Entries[0].Attributes["memberOf"];
                HashSet<string> groupsList = new HashSet<string>();
                foreach (byte[] group in memberships)
                {
                    var TextGroup = Encoding.UTF8.GetString(group);
                    groupsList.Add(TextGroup.ToString());
                }
                if (groupsList.Contains("CN=TVS Restricted Access,OU=TTG Security Groups,DC=ttg,DC=local"))
                {
                    MainWindow.isAdmin = true;
                }

            }
            catch (Exception ex) when (ex.Message == "The supplied credential is invalid.")
            {
                MessageBox.Show("The supplied credentials are invalid.", "Error");
            }

            conn.Dispose();
            return result;
        }

        public static string TryGetUser()
        {
            try
            {
                var storedUser = Settings.Default.userName;
                var userEntropy = Settings.Default.userEntropy;
                byte[] encodedUser = ProtectedData.Unprotect(storedUser, userEntropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(encodedUser);
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string TryGetPassword()
        {
            try
            {
                var storedPassword = Settings.Default.passWord;
                var entropy = Settings.Default.entropy;
                byte[] encodedPassword = ProtectedData.Unprotect(storedPassword, entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(encodedPassword);
            }
            catch
            {
                return string.Empty;
            }

        }


    }
}
