using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ThinkVoipTool
{
    internal class MacroConverter : CustomCreationConverter<ThreeCxPageMacrosBase>
    {
        public override ThreeCxPageMacrosBase Create(Type objectType) => throw new NotImplementedException();

        private static ThreeCxPageMacrosBase Create(JObject jObject)
        {
            var type = (string) jObject.Property("name")!;
            return type switch
            {
                "List-LabelMaker2000" => new LabelMaker2000 {Required = true},
                "List-3CXInstalledOn" => new ThreeCxInstalledOn(),
                "List-3CXServerLocationOnWan" => new ThreeCxLocationOnWan(),
                "List-3CXServerLocationOnWAN" => new ThreeCxLocationOnWan(),
                "Text-3CXAzureVMPublicIP" => new ThreeCxAzureVmPublicIp {ShouldSync = true},
                "Text-3cxManagementAdminURL" => new ThreeCxManagementAdminUrl(),
                "Text-3cxManagementAdminUsername" => new ThreeCxManagementAdminUserName(),
                "Text-3cxManagementAdminPassword" => new ThreeCxManagementAdminPassWord(),
                "Table-SharedAdmin" => new ThreeCxManagementSharedAdminTableRow(),
                "Text-3cxlicensekey" => new ThreeCxLicenseKey {ShouldSync = true},
                "Text-3cxlicenseconcurrentcalls" => new ThreeCxConcurrentCalls {ShouldSync = true},
                "Date-Expiration" => new ThreeCxExpirationDate {ShouldSync = true},
                "Text-AzureVMAdmin-Username" => new ThreeCxAzureVmAdminUserName(),
                "Text-AzureVMAdmin-Password" => new ThreeCxAzureVmAdminPassWord(),
                "Text-AzureVMAdmin-AddtlNotes" => new ThreeCxAzureVmAdditionalNotes(),
                "List-VOIPProviderSIPTrunk-SkySwitchBilling" => new VoipProviderSipTrunkSkySwitch(),
                "List-VOIPProviderSIPTrunk-NexVortex" => new VoipProviderSipTrunkNexVortex(),
                "List-VOIPProviderSIPTrunkProvider-NexVortex" => new VoipProviderSipTrunkProviderNexVortex(),
                "List-VOIPProviderSIPTrunk-Other" => new VoipProviderSipTrunkOther(),
                "List-VOIPProviderSIPTrunk-Yes" => new VoipProviderSipTrunkYes(),
                "List-VOIPProviderSIPTrunk-No" => new VoipProviderSipTrunkNo(),
                "List-VOIPProviderSIPTrunkProvider" => new VoipProviderSipTrunkProvider(),
                "Text-VoIPProviderOther" => new VoipProviderOther(),
                "Text-OtherSIPProvider" => new TextOtherSipProvider(),
                "List-SkySwitchBilling-SIPTrunkPurpose" => new VoipProviderSkySwitchSipTrunkPurpose(),
                "Text-SkySwitchBilling-CustomerDomainName" => new VoipProviderSkySwitchCustomerDomainName(),
                "Text-SkySwitchBilling-SIPTrunkUsername" => new VoipProviderSkySwitchSwitchUserName(),
                "Text-SkySwitchBilling-SIPTrunkPassword" => new VoipProviderSkySwitchSwitchPassWord(),
                "Text-SkySwitchBilling-SIPTrunkName" => new VoipProviderSkySwitchSwitchTrunkName(),
                "Text-providerPrimaryNumber" => new SipTrunkProviderPrimaryNumber {ShouldSync = true},
                "Text-providerAuthenticationID" => new SipTrunkProviderAuthenticationId {ShouldSync = true},
                "Text-ProviderAuthenticationPass" => new SipTrunkProviderAuthenticationPassWord {ShouldSync = true},
                "List-AutoUpdateDay" => new ThreeCxServerUpdateDay {ShouldSync = true},
                "Table-DID" => new ThreeCxDidList {ShouldSync = true},
                "List-efax-ThroughThink-Yes" => new EfaxThroughThinkYes(),
                "List-efax-ThroughThink-No" => new EfaxThroughThinkNo(),
                "list-international" => new ListInternational(),
                "list-DID" => new ListDid(),
                "Table-eFax" => new EfaxTable(),
                "Text-CallFlow" => new ThreeCxCallFlow(),
                "Text-NumberOfConcurrentCalls" => new TextNumberOfConcurrentCalls(),
                "Repeat-UserExtensionSetup" => new UserExtensionSetup(),
                "Text-AdditionalNotes" => new AdditionalNotes(),
                "Text-providerIPHost" => new TextProviderIpHost(),
                "Text-providerinboundport" => new TextProviderInboundPort(),
                "Text-provideroutboundport" => new TextProviderOutboundPort(),
                "List-VOIPGateway-Yes" => new VoipGatewayYes(),
                "List-VOIPGateway-No" => new VoipGatewayNo(),
                "Text-VoIPGatewayPRIProviderName" => new VoipGatewayPriProviderName(),
                "List-VoIPGatewayDevice" => new VoipGatewayDevice(),
                "Text-VoIPRouterConfig" => new VoipProviderRouterConfig(),
                "List-VOIPGatewayType" => new VoipGatewayType(),
                "Table-VoIPPhones" => new VoipPhonesTable {ShouldSync = true},
                "Text-VoicemailPin" => new TextVoicemailPin(),
                "List-VoicemailtoEmailEnabled" => new TextVoicemailPToEmailEnabled(),
                _ => throw new ApplicationException($"The macro type {type} is not yet supported.")
            };
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream 
            var jObject = JObject.Load(reader);

            // Create target object based on JObject 
            var target = Create(jObject);

            // Populate the object properties 
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }
}