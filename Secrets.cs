using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Identity.Client;

namespace ThinkVoipTool
{
    internal class Secrets
    {
        private const string KvUri = "https://TTG-Secrets.vault.azure.net";
        private const string ClientId = "b8aad1d2-dfec-44f0-9e1f-d1896e5e1031";
        private const string ClientSecret = "saFZRKIQpvnK5i1xt88~~y__G5_iFTo.1~";

        private static readonly SecretClient Client = new(new Uri(KvUri),
            new ClientSecretCredential("9d1d44a0-b14f-4e5f-9da3-775d779bb1f5",
                ClientId,
                ClientSecret));

        public static async Task<AuthenticationResult> GetAuthToken()
        {
            const string clientId = "b8aad1d2-dfec-44f0-9e1f-d1896e5e1031";
            const string authority = "https://login.microsoftonline.com/9d1d44a0-b14f-4e5f-9da3-775d779bb1f5";
            var scopes = new[] {"user.read"};
            var app = PublicClientApplicationBuilder.Create(clientId)
                .WithRedirectUri("http://localhost")
                .WithAuthority(authority)
                .Build();
            var accounts = await app.GetAccountsAsync();


            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                    .ExecuteAsync();
                return result;
            }
            catch (MsalUiRequiredException)
            {
                result = await app.AcquireTokenInteractive(scopes)
                    .ExecuteAsync();
                return result;
            }
        }

        public static async Task<string> GetSecretValue(string requestedSecret)
        {
            var result = await Client.GetSecretAsync(requestedSecret);
            return result.Value.Value ?? string.Empty;
        }

        public static async Task<string> SetSecretValue(string requestedSecret, string newSecretValue)
        {
            var result = await Client.SetSecretAsync(requestedSecret, newSecretValue);
            return result.Value.Value;
        }
    }
}