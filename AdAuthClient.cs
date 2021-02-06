using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ThinkVoipTool.Properties;

namespace ThinkVoipTool
{
    public class AdAuthClient
    {
        public static bool ValidateUserByBind(string username, string password, string domain = "ttg.local", string url = "auth.think-team.com")
        {
            username = username.StripDomain();
            var credentials = new NetworkCredential(username, password, domain);
            var serverId = new LdapDirectoryIdentifier(url);
            var conn = new LdapConnection(serverId, credentials);

            try
            {
                conn.Bind();
            }
            catch (Exception)
            {
                conn.Dispose();
                return false;
            }

            const string dName = "DC=ttg,Dc=Local";
            var ldapFilter = $"samAccountName={username}";
            var attributeList = new[] {@"SAMAccountName", "memberOf", "cn"};
            try
            {
                var response = ((SearchResponse) conn.SendRequest(new SearchRequest(dName, ldapFilter, SearchScope.Subtree, attributeList)))!;
                {
                    var memberships = response.Entries[0].Attributes["memberOf"];
                    var groupsList = new HashSet<string>();
                    foreach (byte[]? group in memberships)
                    {
                        var textGroup = Encoding.UTF8.GetString(group!);
                        groupsList.Add(textGroup);
                    }

                    if(groupsList.Contains("CN=TVS Restricted Access,OU=TTG Security Groups,DC=ttg,DC=local"))
                    {
                        MainWindow.IsAdmin = true;
                    }
                }
            }
            catch (Exception ex) when (ex.Message == "The supplied credential is invalid.")
            {
                MessageBox.Show("The supplied credentials are invalid.", "Error");
            }

            conn.Dispose();
            return true;
        }

        public static string TryGetUser()
        {
            try
            {
                var storedUser = Settings.Default.userName;
                var userEntropy = Settings.Default.userEntropy;
                var encodedUser = ProtectedData.Unprotect(storedUser, userEntropy, DataProtectionScope.CurrentUser);
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
                var encodedPassword = ProtectedData.Unprotect(storedPassword, entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(encodedPassword);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}