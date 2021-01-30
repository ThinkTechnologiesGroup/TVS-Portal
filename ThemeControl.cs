using MaterialDesignThemes.Wpf;
using ThinkVoipTool.Properties;

namespace ThinkVoipTool
{
    internal class ThemeControl
    {
        public static void SetTheme(MainWindow window)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();

            var baseTheme = window.IsDark ? new MaterialDesignDarkTheme() : (IBaseTheme) new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);

            paletteHelper.SetTheme(theme);


            Settings.Default.isDark = window.IsDark;
            Settings.Default.Save();
        }
    }
}