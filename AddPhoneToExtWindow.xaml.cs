using System.Threading.Tasks;
using System.Windows;

namespace ThinkVoip
{
    /// <summary>
    /// Interaction logic for AddPhoneToExtWindow.xaml
    /// </summary>
    public partial class AddPhoneToExtWindow : Window
    {
        private string extension;

        public AddPhoneToExtWindow(string extensionNumber)
        {

            InitializeComponent();
            this.extension = extensionNumber;
            var extensionDisplayString = "Selected Extension: " + extension;
            ExtTextBlock.Text = extensionDisplayString;
            ExtTextBlock.Visibility = Visibility.Visible;
        }

        private async void AddPhoneToExtension_Click(object sender, RoutedEventArgs e)
        {

            var extensionNumber = MainWindow.CurrentExtension;
            var phoneType = "";
            switch (PhonesDropDownList.SelectedItem)
            {
                case MainWindow.PhoneModels.YealinkT40G:
                    phoneType = "Yealink T40G";
                    break;
                case MainWindow.PhoneModels.YealinkT46S:
                    phoneType = "Yealink T46S";
                    break;
                case MainWindow.PhoneModels.YealinkT48S:
                    phoneType = "Yealink T48S";
                    break;
                case MainWindow.PhoneModels.YealinkT57W:
                    phoneType = "Yealink T57W";
                    break;
                case MainWindow.PhoneModels.YealinkCp960:
                    phoneType = "Yealink CP960";
                    break;
                case MainWindow.PhoneModels.FanvilH5:
                    phoneType = "Fanvil H5";
                    break;
                case MainWindow.PhoneModels.YealinkT46STVS:
                    phoneType = "Yealink T46S-tvs_yealinkt4x (tvs_yealinkt4x.ph.xml)";
                    break;

            }
            var test = MacAddressTextBlock.Text;
            await SavePhone(phoneType, MacAddressTextBlock.Text, extensionNumber);
            Close();
        }

        private async Task SavePhone(string phoneType, string macAddress, string extensionNUmber)
        {
            var result = await MainWindow.ThreeCxClient.CreatePhoneOnServer(phoneType, macAddress, extensionNUmber);

            var pin = await MainWindow.ThreeCxClient.GetExtensionPinNumber(extensionNUmber);

            if (result == "OK")
            {
                MessageBox.Show($"Ext: {extensionNUmber} \n Pin: {pin}", "Success! :) ");
            }
            else
            {
                MessageBox.Show("Failed :( ");
            }
        }



    }
}