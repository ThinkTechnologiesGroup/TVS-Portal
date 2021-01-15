using System;

using Microsoft.Win32;

namespace ThinkVoipTool
{
    class ReadRegistry
    {
        const string KEY_NAME = "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
        const string VALUE = "AppsUseLightTheme";

        public static bool isDarkEnabled => IsDarkEnabled();


        private static bool IsDarkEnabled()
        {
            var isDark = false;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KEY_NAME))
            {

                if (key != null)
                {
                    Object o = key.GetValue(VALUE);
                    if (o != null)
                    {
                        if (o.ToString() == "0")
                        {
                            isDark = true;
                        }
                    }
                }
                return isDark;
            }
        }
    }


}