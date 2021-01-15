using System;
using System.Linq;
using System.Threading.Tasks;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Identity.Client;

namespace ThinkVoipTool
{
    class Secrets
    {

        const string keyVaultName = "TTG-Secrets";
        const string kvUri = "https://TTG-Secrets.vault.azure.net";
        const string ClientId = "b8aad1d2-dfec-44f0-9e1f-d1896e5e1031";
        const string clientSecret = "saFZRKIQpvnK5i1xt88~~y__G5_iFTo.1~";
        private static readonly SecretClient client = new SecretClient(new Uri(kvUri),
            new ClientSecretCredential("9d1d44a0-b14f-4e5f-9da3-775d779bb1f5",
                ClientId,
                clientSecret));

        public static async Task<AuthenticationResult> GetAuthToken()
        {
            var ClientId = "b8aad1d2-dfec-44f0-9e1f-d1896e5e1031";
            string Authority = "https://login.microsoftonline.com/9d1d44a0-b14f-4e5f-9da3-775d779bb1f5";
            string[] Scopes = new string[] { "user.read" };
            var app = PublicClientApplicationBuilder.Create(ClientId)
                .WithRedirectUri("http://localhost")
                .WithAuthority(Authority)
                .Build();
            var accounts = await app.GetAccountsAsync();


            AuthenticationResult result;
            try
            {
                result = await app.AcquireTokenSilent(Scopes, accounts.FirstOrDefault())
                            .ExecuteAsync();
                return result;

            }
            catch (MsalUiRequiredException)
            {
                result = await app.AcquireTokenInteractive(Scopes)
                            .ExecuteAsync();
                return result;

            }
        }

        public async static Task<string> GetSecretValue(string requestedSecret)
        {
            var result =  await client.GetSecretAsync(requestedSecret);
            return result.Value.Value ?? string.Empty ;
        }

        public async static Task<string> SetSecretValue(string requestedSecret, string newSecretValue)
        {
            var result = await client.SetSecretAsync(requestedSecret, newSecretValue);
            return result.Value.Value;
        }

    }
}
