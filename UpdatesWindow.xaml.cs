using System.Windows;

using static ThinkVoip.ExtensionTypeSelectionWindow;

namespace ThinkVoip
{
    /// <summary>
    /// Interaction logic for UpdatesWindow.xaml
    /// </summary>
    public partial class UpdatesWindow : Window
    {
        private string macAddress;
        private string extensionNumber;

        public UpdatesWindow(string extensionNumber, ExtensionTypes extType)
        {
            this.extensionNumber = extensionNumber;


            InitializeComponent();


        }
    }
}
