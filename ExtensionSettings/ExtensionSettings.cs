namespace ThinkVoipTool.ExtensionSettings
{
    public class ExtensionSettings
    {
        public bool DisAllowUseOffLan = false;
        public string EmailAddress = "";
        public bool FwdOnly;
        public string MobileNumber = "";
        public bool VmOnly;

        public string VmPin = "1234";

        public ExtensionSettings(bool vmOnly = false, bool fwdOnly = false)
        {
            VmOnly = vmOnly;
            FwdOnly = fwdOnly;
        }

        public ExtensionSettings(string voicemailPin, bool vmOnly = false, bool fwdOnly = false)
        {
            VmPin = voicemailPin;
            VmOnly = vmOnly;
            FwdOnly = fwdOnly;
        }

        public ExtensionSettings(string voicemailPin, string emailAddress, bool vmOnly = false, bool fwdOnly = false)
        {
            VmPin = voicemailPin;
            EmailAddress = emailAddress;
            VmOnly = vmOnly;
            FwdOnly = fwdOnly;
        }
    }
}