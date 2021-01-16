namespace ThinkVoipTool
{
    public class ExtensionSettings
    {

        public string vmPin = "1234";
        public bool dissallowUseOffLan = false;
        public bool vmOnly = false;
        public bool fwdOnly = false;
        public string emailAddress = "";
        public string mobileNumber = "";

        public ExtensionSettings(bool vmOnly = false, bool fwdOnly = false)
        {
            this.vmOnly = vmOnly;
            this.fwdOnly = fwdOnly;
        }
        public ExtensionSettings(string voicemailPin, bool vmOnly = false, bool fwdOnly = false)
        {
            this.vmPin = voicemailPin;
            this.vmOnly = vmOnly;
            this.fwdOnly = fwdOnly;

        }
        public ExtensionSettings(string voicemailPin, string emailAddress, bool vmOnly = false, bool fwdOnly = false)
        {
            this.vmPin = voicemailPin;
            this.emailAddress = emailAddress;
            this.vmOnly = vmOnly;
            this.fwdOnly = fwdOnly;




        }






    }
}
