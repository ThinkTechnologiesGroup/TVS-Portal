using Microsoft.Win32;

namespace ThinkVoipTool
{
    internal class ReadRegistry
    {
        private const string KeyName = "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
        private const string Value = "AppsUseLightTheme";

        //private static bool _isDarkEnabled => IsDarkEnabled();


        public static bool IsDarkEnabled()
        {
            var isDark = false;
            using var key = Registry.CurrentUser.OpenSubKey(KeyName);
            var o = key?.GetValue(Value);
            if(o == null)
            {
                return false;
            }

            if(o.ToString() == "0")
            {
                isDark = true;
            }

            return isDark;
        }
    }
}