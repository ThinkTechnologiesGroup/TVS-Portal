using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace ThinkVoipTool
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Sync
    {
        public static List<ThreeCxPageMacrosBase> NewUpdatedDocs(ThreeCxServer server, List<ThreeCxPageMacrosBase> pageList)
        {
            var updates = new List<ThreeCxPageMacrosBase>();

            foreach (var table in pageList.Where(a => a.ShouldSync))
            {
                table.Sync(server);
                updates.Add(table);
            }

            return updates;
        }

        public static List<ThreeCxPageMacros> UpdatedDocs(ThreeCxServer server, List<ThreeCxPageMacros> pageList)
        {
            var updates = new List<ThreeCxPageMacros>();
            var sipTrunkName = server.SipTrunks[0].Name; //Should loop around the whole list and then just skip the WebClient one.
            var gateway = server.SipTrunks[0].Type; //Same as above, these would just fall as steps to that method
            var host = server.SipTrunks[0].Host; //Same as above, these would just fall as steps to that method
            var nexVortex = false;
            var skySwitch = false;
            var azure = false;
            foreach (var table in pageList)
            {
                switch (table.Name)
                {
                    case "List-LabelMaker2000":
                        foreach (var value in table.Value)
                        {
                            if(value == "3cxazure")
                            {
                                azure = true;
                            }

                            break;
                        }

                        updates.Add(table);
                        break;
                    //case "List-3CXServerLocationOnWan":
                    //    updates.Add(table);
                    //    break;
                    case "List-VOIPProviderSIPTrunk-Yes":
                        if(gateway == "TypeOfGateway.VoipProvider" || gateway == "Provider")
                        {
                            table.Value = new JArray("Yes");
                        }

                        updates.Add(table);
                        break;
                    case "List-VOIPProviderSIPTrunk-No":
                        if(gateway == "TypeOfGateway.VoipProvider" || gateway == "Provider")
                        {
                            table.Value = new JArray();
                        }
                        else
                        {
                            table.Value = new JArray("No");
                        }

                        updates.Add(table);
                        break;
                    case "List-VOIPGateway-Yes":
                        if(gateway == "TypeOfGateway.Gateway" || gateway == "T1")
                        {
                            table.Value = new JArray("Yes");
                        }

                        updates.Add(table);
                        break;
                    case "List-VOIPGateway-No":
                        if(gateway == "TypeOfGateway.Gateway" || gateway == "T1")
                        {
                            table.Value = new JArray();
                        }
                        else
                        {
                            table.Value = new JArray("No");
                        }

                        updates.Add(table);
                        break;
                    case "List-VOIPProviderSIPTrunk-SkySwitchBilling":
                        if((gateway == "TypeOfGateway.VoipProvider" || gateway == "Provider") && host.Contains("22335.service"))
                        {
                            table.Value = new JArray("SkySwitchBilling");
                            skySwitch = true;
                        }
                        else
                        {
                            table.Value = new JArray();
                        }

                        updates.Add(table);
                        break;
                    case "List-VOIPProviderSIPTrunk-NexVortex":
                        if((gateway == "TypeOfGateway.VoipProvider" || gateway == "Provider") && host == "nexvortex.com" && azure)
                            ////
                            ////this is gonna be wrong most of the time. Need to actually figure out what type it is somehow.
                            ////
                        {
                            table.Value = new JArray("NexVortex - Wholesale");
                        }
                        else
                        {
                            table.Value = new JArray();
                        }

                        nexVortex = true;
                        updates.Add(table);
                        break;
                    case "List-VOIPProviderSIPTrunkProvider-NexVortex":
                        if((gateway == "TypeOfGateway.VoipProvider" || gateway == "Provider") && host == "nexvortex.com")
                        {
                            table.Value = new JArray("NexVortex");
                        }

                        nexVortex = true;
                        updates.Add(table);
                        break;
                    case "List-VOIPProviderSIPTrunkProvider":
                        if(host == "nexvortex.com")
                        {
                            table.Value = new JArray();
                        }

                        updates.Add(table);
                        break;
                    case "List-VOIPProviderSIPTrunk-Other":
                        if(host == "nexvortex.com")
                        {
                            table.Value = new JArray();
                        }

                        updates.Add(table);
                        break;
                    case "List-SkySwitchBilling-SIPTrunkPurpose":
                        if(skySwitch)
                        {
                            table.Value = new JArray("Local Calling (US & Canada)");
                        }

                        updates.Add(table);
                        break;
                    case "Text-SkySwitchBilling-CustomerDomainName":
                        if(skySwitch)
                        {
                            table.Value = host;
                        }

                        updates.Add(table);
                        break;
                    case "Text-SkySwitchBilling-SIPTrunkName":
                        if(skySwitch)
                        {
                            table.Value = sipTrunkName;
                        }

                        updates.Add(table);
                        break;
                    case "Text-SkySwitchBilling-SIPTrunkUsername":
                        var skySwitchAuthobject = server.SipTrunkSettings.GetValue("ActiveObject");
                        var skySwitchauth = skySwitchAuthobject.Value<JToken>("AuthId");
                        var skySwitchauthId = skySwitchauth.Value<string>("_value");
                        table.Value = skySwitchauthId;
                        updates.Add(table);
                        break;
                    case "Text-SkySwitchBilling-SIPTrunkPassword":
                        var skySwitchPassobject = server.SipTrunkSettings.GetValue("ActiveObject");
                        var skySwitchPassauth = skySwitchPassobject.Value<JToken>("AuthId");
                        var skySwitchPass = skySwitchPassauth.Value<string>("_value");
                        table.Value = skySwitchPass;
                        updates.Add(table);
                        break;
                    case "Text-NumberOfConcurrentCalls":
                        var conCalls = server.SipTrunks[0].SimCalls;
                        table.Value = conCalls;
                        updates.Add(table);
                        break;
                    case "list-international":
                        table.Value = new JArray("No");
                        updates.Add(table);
                        break;
                    case "list-DID":
                        table.Value = new JArray("No");
                        updates.Add(table);
                        break;
                    case "Text-providerIPHost":
                        if(nexVortex)
                        {
                            table.Value = " ";
                        }

                        updates.Add(table);
                        break;
                    case "Text-providerinboundport":
                        if(nexVortex)
                        {
                            table.Value = " ";
                        }

                        updates.Add(table);
                        break;
                    case "Text-provideroutboundport":
                        if(nexVortex)
                        {
                            table.Value = " ";
                        }

                        updates.Add(table);
                        break;
                    case "Text-providerPrimaryNumber":
                        var primaryNUmber = server.SipTrunks[0].ExternalNumber;
                        table.Value = primaryNUmber;
                        updates.Add(table);
                        break;
                    case "Text-providerAuthenticationID":
                        var aobject = server.SipTrunkSettings.GetValue("ActiveObject");
                        var auth = aobject.Value<JToken>("AuthId");
                        var authId = auth.Value<string>("_value");
                        table.Value = authId;
                        updates.Add(table);
                        break;
                    case "Text-ProviderAuthenticationPass":
                        var apobject = server.SipTrunkSettings.GetValue("ActiveObject");
                        var authPass = apobject.Value<JToken>("AuthPassword");
                        var authPassValue = authPass.Value<string>("_value");
                        table.Value = authPassValue;
                        updates.Add(table);
                        break;
                    case "Text-3CXAzureVMPublicIP":
                        table.Value = server.SystemStatus.Ip;
                        updates.Add(table);
                        break;
                    case "Text-3cxlicensekey":
                        table.Value = server.ThreeCxLicense.Key;
                        updates.Add(table);
                        break;
                    case "Text-3cxlicenseconcurrentcalls":
                        table.Value = server.ThreeCxLicense.MaxSimCalls;
                        updates.Add(table);
                        break;
                    case "Date-Expiration":
                        var time = DateTime.Parse(server.SystemStatus.MaintenanceExpiresAt.ToString(CultureInfo.InvariantCulture));
                        table.Value = time.ToString("yyyy-MM-dd HH:mm:ss");
                        updates.Add(table);
                        break;
                    case "List-AutoUpdateDay":
                        table.Value = new JArray(server.UpdateDay);
                        updates.Add(table);
                        break;
                    case "Table-DID":
                        table.Value = new JObject();
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
                            table.Value.Add(new JProperty(i.ToString(), tmpArray));
                            i++;
                        }

                        updates.Add(table);
                        break;
                    case "Table-VoIPPhones":
                        table.Value = new JObject();
                        var j = 0;
                        var phones = new Dictionary<string, Phone>();
                        foreach (var phone in server.Phones.Distinct())
                        {
                            if(phones.ContainsKey(phone.Model))
                            {
                                continue;
                            }

                            var tmpArray = new JArray();
                            var phoneVendor = new ThreeCxPageMacros {Macro = "text-data", Name = "Text-VoipPhonesManucturer", Value = phone.Vendor};
                            var phoneModel = new ThreeCxPageMacros {Macro = "text-data", Name = "Text-VoipPhonesModels", Value = phone.Model};
                            tmpArray.Add(JObject.FromObject(phoneVendor));
                            tmpArray.Add(JObject.FromObject(phoneModel));
                            table.Value.Add(new JProperty(j.ToString(), tmpArray));
                            j++;
                            phones.Add(phone.Model, phone);
                        }

                        updates.Add(table);
                        break;
                }
            }

            return updates;
        }
    }
}