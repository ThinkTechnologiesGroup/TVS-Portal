using System;
using System.Collections.Generic;
using System.Text;

using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows.Media;
using ThinkVoip;
using ThinkVoipTool.Properties;

namespace ThinkVoipTool
{
    class ThemeControl
    {

        public ThemeControl()
        {


        }

        public static void SetTheme(MainWindow window)
        {

            var paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            IBaseTheme baseTheme = window.isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);

            var test = theme.PrimaryMid;

            paletteHelper.SetTheme(theme);



            Settings.Default.isDark = window.isDark;
            Settings.Default.Save();

            


        }
    }
}
