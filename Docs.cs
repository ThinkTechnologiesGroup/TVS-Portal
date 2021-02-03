using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;

// ReSharper disable InconsistentNaming


// ReSharper disable UnusedMember.Global

namespace ThinkVoipTool
{
    internal class Docs
    {
        public static Docs ConfClient = new Docs("https://docs.think-team.com/rest/api/", MainWindow.AuthU, MainWindow.AuthP);
        private readonly string _authKey;
        private readonly string _baseUrl;

        private readonly string _scaffoldingUrl;
        private RestClient _restClient;

        private RestRequest _restRequest;

        public Docs(string baseUrl, string userName, string password)
        {
            _baseUrl = baseUrl;
            _scaffoldingUrl = baseUrl.Replace("api/", "");
            _restClient = new RestClient(_baseUrl);
            _authKey = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));
        }

        public static async Task<string> GetUserName() => await Secrets.GetSecretValue("AdAuthUser") ?? string.Empty;

        public string FindThreeCxPageId(string spaceKey)
        {
            _restClient = new RestClient(_baseUrl + $"content/search?cql=space={spaceKey} and label=\"3cxinfo\" and label =\"test\"");
            _restRequest = new RestRequest(Method.GET);
            _restRequest.AddHeader("Authorization", "Basic " + _authKey);

            try
            {
                var response = _restClient.Execute(_restRequest).Content;
                var obj = JsonConvert.DeserializeObject<JObject>(response);
                var list = obj.GetValue("results");
                Debug.Assert(list != null, nameof(list) + " != null");
                var results = JsonConvert.DeserializeObject<List<Page>>(list.ToString());
                return results.First().Id;
            }
            catch (Exception e)
            {
                Logging.Logger.Error($"Failed to find pageId \"{spaceKey}\" : " + e.Message);
                throw;
            }
        }

        private static string StripHtml(string input) => Regex.Replace(input, "<.*?>", string.Empty);

        public string FindThreeCxPageIdByTitle(string spaceTitle, bool getUrl = false)
        {
            spaceTitle = spaceTitle.Replace("&", string.Empty);
            spaceTitle = spaceTitle.Replace("!", string.Empty);

            spaceTitle = spaceTitle.Replace(", LLC", string.Empty);

            if(spaceTitle.ToLower() == "think")
            {
                return "115671322";
            }

            _restClient = new RestClient(_baseUrl + $"content/search?cql=space.title ~ \"{spaceTitle}\" and label=\"3cxinfo\"");
            _restRequest = new RestRequest(Method.GET);
            _restRequest.AddHeader("Authorization", "Basic " + _authKey);

            try
            {
                var response = _restClient.Execute(_restRequest).Content;
                var obj = JsonConvert.DeserializeObject<JObject>(response);
                var list = obj.GetValue("results");
                Debug.Assert(list != null, nameof(list) + " != null");
                var results = JsonConvert.DeserializeObject<List<Page>>(list.ToString());
                if(getUrl)
                {
                    return results.First().Links.tinyui;
                }

                return results.First().Id;
            }
            catch (Exception e)
            {
                Logging.Logger.Error($"Failed to find pageId \"{spaceTitle}\" : " + e.Message);
                MessageBox.Show("No 3cx Confluence page found for selected client");
                throw;
            }
        }

        public string FindThreeCxPageUrlByTitle(string spaceTitle)
        {
            spaceTitle = spaceTitle.Replace("&", string.Empty);
            spaceTitle = spaceTitle.Replace(", LLC", string.Empty);

            _restClient = new RestClient(_baseUrl + $"content/search?cql=space.title ~ \"{spaceTitle}\" and label=\"3cxinfo\"");
            _restRequest = new RestRequest(Method.GET);
            _restRequest.AddHeader("Authorization", "Basic " + _authKey);

            try
            {
                var response = _restClient.Execute(_restRequest).Content;
                var obj = JsonConvert.DeserializeObject<JObject>(response);
                var list = obj.GetValue("results");
                Debug.Assert(list != null, nameof(list) + " != null");
                var results = JsonConvert.DeserializeObject<List<Page>>(list.ToString());
                return results.First().Links.tinyui;
            }
            catch (Exception e)
            {
                Logging.Logger.Error($"Failed to find pageId \"{spaceTitle}\" : " + e.Message);
                MessageBox.Show("No 3cx Confluence page found for selected client");
                throw;
            }
        }

        public ThreeCxLoginInfo GetThreeCxLoginInfo(string pageId)
        {
            _restClient = new RestClient(_scaffoldingUrl + "scaffolding/1.0/api/form/" + pageId);
            _restRequest = new RestRequest(Method.GET);
            _restRequest.AddHeader("Authorization", "Basic " + _authKey);

            try
            {
                var response = _restClient.Execute(_restRequest).Content;
                var result = JsonConvert.DeserializeObject<List<ThreeCxPageMacros>>(response);
                var hostName = result.FirstOrDefault(table => table.Name == "Text-3cxManagementAdminURL")?.Value;
                hostName = StripHtml(hostName);

                return new ThreeCxLoginInfo
                {
                    HostName = hostName + "/api/",
                    Username = result.FirstOrDefault(table => table.Name == "Text-3cxManagementAdminUsername")?.Value,
                    Password = result.FirstOrDefault(table => table.Name == "Text-3cxManagementAdminPassword")?.Value
                };
            }
            catch (Exception e)
            {
                Log.Error($"Failed to find login info for page Id {pageId}  : " + e.Message);
                throw;
            }
        }

        public ResponseStatus UpdateThreeCxPassword(string pageId, string newPassword)
        {
            var macroList = new List<ThreeCxPageMacros>();
            _restClient = new RestClient(_scaffoldingUrl + "scaffolding/1.0/api/form/" + pageId);
            _restRequest = new RestRequest(Method.PUT);
            _restRequest.AddHeader("Authorization", "Basic " + _authKey);
            var passwordMacro = new ThreeCxPageMacros();
            passwordMacro.Macro = "text-data";
            passwordMacro.Name = "Text-3cxManagementAdminPassword";
            passwordMacro.Value = newPassword;
            macroList.Add(passwordMacro);
            var serializedInfo = JsonConvert.SerializeObject(macroList);
            _restRequest.AddJsonBody(serializedInfo);
            var response = _restClient.Execute(_restRequest);
            return response.ResponseStatus;
        }

        public IEnumerable<ThreeCxPageMacrosBase> GetThreeCxDocumentPageTables(string pageId)
        {
            _restClient = new RestClient(_scaffoldingUrl + "scaffolding/1.0/api/form/" + pageId);
            _restRequest = new RestRequest(Method.GET);
            try
            {
                _restRequest.AddHeader("Authorization", "Basic " + _authKey);
                var response = _restClient.Execute(_restRequest).Content;
                var result = JsonConvert.DeserializeObject<List<ThreeCxPageMacrosBase>>(response, new MacroConverter());
                return result;
            }
            catch (Exception e)
            {
                Logging.Logger.Error($"Failed to find scaffolding tables for page Id {pageId} : " + e.Message);
                throw;
            }
        }

        public void UpdateConfluenceWithChanges(List<ThreeCxPageMacrosBase> updatesList, string pageId)
        {
            var serializedInfo = JsonConvert.SerializeObject(updatesList);
            _restClient = new RestClient(_scaffoldingUrl + "scaffolding/1.0/api/form/" + pageId);
            _restRequest = new RestRequest(Method.PUT);
            _restRequest.AddHeader("Authorization", "Basic " + _authKey);
            _restRequest.AddJsonBody(serializedInfo);
            try
            {
                var response = _restClient.Execute(_restRequest);
                Console.WriteLine(response.StatusCode + response.StatusDescription);
            }
            catch (Exception e)
            {
                Logging.Logger.Error($"Failed to update confluence with changes for page Id {pageId} : " + e.Message);
                throw;
            }
        }
    }

    public class Page
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Id { get; set; }


        [JsonProperty("_links")]
        public Links Links { get; set; }

        //public string type { get; set; }
        //public string status { get; set; }
        //public string Title { get; set; }
        //public List<ThreeCxPageMacros> PageTables { get; set; }
    }

    public class Links
    {
        public string webui { get; set; }
        public string tinyui { get; set; }
        public string self { get; set; }
        public string @base { get; set; }
        public string context { get; set; }
    }

    public class ThreeCxLoginInfo
    {
        public string HostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ThreeCxPageMacros
    {
        [JsonProperty("macro")]
        public string Macro { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public dynamic Value { get; set; }
    }

    public abstract class ThreeCxPageMacrosBase
    {
        [JsonProperty("required")]
        public bool Required;

        [JsonProperty("shouldSync")]
        public bool ShouldSync;

        [JsonProperty("macro")]
        public string Macro { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public abstract void Sync(ThreeCxServer server);
    }

    public class LabelMaker2000 : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxInstalledOn : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public dynamic Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxLocationOnWan : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public dynamic Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxAzureVmPublicIp : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server) => Value = server.SystemStatus.Ip;
    }

    public class ThreeCxManagementAdminUrl : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server) => Value = server.SystemStatus.Fqdn;
    }

    public class ThreeCxManagementAdminUserName : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxManagementAdminPassWord : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxManagementSharedAdminTableRow : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JObject Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxLicenseKey : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server) => Value = server.ThreeCxLicense.Key;
    }

    public class ThreeCxConcurrentCalls : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server) => Value = server.ThreeCxLicense.MaxSimCalls.ToString();
    }

    public class ThreeCxExpirationDate : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
            var time = DateTime.Parse(server.SystemStatus.MaintenanceExpiresAt.ToString(CultureInfo.InvariantCulture));
            Value = time.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class ThreeCxAzureVmAdminUserName : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxAzureVmAdminPassWord : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxAzureVmAdditionalNotes : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipGatewayYes : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipGatewayNo : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipGatewayPriProviderName : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderRouterConfig : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipGatewayType : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipGatewayDevice : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSipTrunkSkySwitch : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSipTrunkNexVortex : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSipTrunkOther : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSipTrunkYes : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSipTrunkNo : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSipTrunkProviderNexVortex : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSipTrunkProvider : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderOther : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class TextOtherSipProvider : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class TextNumberOfConcurrentCalls : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ListInternational : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ListDid : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class TextProviderIpHost : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class TextProviderInboundPort : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class TextProviderOutboundPort : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSkySwitchSipTrunkPurpose : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSkySwitchCustomerDomainName : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSkySwitchSwitchTrunkName : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSkySwitchSwitchUserName : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class VoipProviderSkySwitchSwitchPassWord : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class SipTrunkProviderPrimaryNumber : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
            var primaryNUmber = server.SipTrunks[0].ExternalNumber;
            Value = primaryNUmber;
        }
    }

    public class SipTrunkProviderAuthenticationId : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
            var aobject = server.SipTrunkSettings.GetValue("ActiveObject");
            Debug.Assert(aobject != null, nameof(aobject) + " != null");
            var auth = aobject.Value<JToken>("AuthId");
            var authId = auth.Value<string>("_value");
            Value = authId;
        }
    }

    public class SipTrunkProviderAuthenticationPassWord : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
            var apobject = server.SipTrunkSettings.GetValue("ActiveObject");
            Debug.Assert(apobject != null, nameof(apobject) + " != null");
            var authPass = apobject.Value<JToken>("AuthPassword");
            var authPassValue = authPass.Value<string>("_value");
            Value = authPassValue;
        }
    }

    public class ThreeCxServerUpdateDay : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
            Value = new JArray(server.UpdateDay);
        }
    }

    public class ThreeCxDidList : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JObject Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
            Value = new JObject();
            var i = 0;
            foreach (var did in server.InboundRulesList)
            {
                var tmpArray = new JArray();
                var number = new ThreeCxPageMacros
                {
                    Macro = "text-data",
                    Name = "Text-DIDNumber",
                    Value = did.Did.Replace("*", string.Empty)
                };
                var assigned = new ThreeCxPageMacros
                {
                    Macro = "text-data",
                    Name = "Text-AssignedTo",
                    Value = did.InOfficeRouting
                };
                tmpArray.Add(JObject.FromObject(number));
                tmpArray.Add(JObject.FromObject(assigned));
                Value.Add(new JProperty(i.ToString(), tmpArray));
                i++;
            }
        }
    }

    public class VoipPhonesTable : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JObject Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
            Value = new JObject();
            var i = 0;
            var foundPhones = new Dictionary<string, Phone>();
            foreach (var phone in server.Phones)
            {
                if(foundPhones.ContainsKey(phone.Model))
                {
                    continue;
                }

                var tmpArray = new JArray();
                var manufacturer = new ThreeCxPageMacros
                {
                    Macro = "text-data",
                    Name = "Text-VoipPhonesManucturer",
                    Value = phone.Vendor
                };
                var model = new ThreeCxPageMacros
                {
                    Macro = "text-data",
                    Name = "Text-VoipPhonesModels",
                    Value = phone.Model
                };
                foundPhones.Add(phone.Model, phone);
                tmpArray.Add(JObject.FromObject(manufacturer));
                tmpArray.Add(JObject.FromObject(model));
                Value.Add(new JProperty(i.ToString(), tmpArray));
                i++;
            }
        }
    }

    public class TextVoicemailPin : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class TextVoicemailPToEmailEnabled : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class EfaxThroughThinkYes : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class EfaxThroughThinkNo : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JArray Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class EfaxTable : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JObject Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class ThreeCxCallFlow : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class UserExtensionSetup : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public JObject Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }

    public class AdditionalNotes : ThreeCxPageMacrosBase
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        public override void Sync(ThreeCxServer server)
        {
        }
    }
}