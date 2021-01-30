using System.Threading.Tasks;
using System.Windows;

namespace ThinkVoipTool
{
    /// <summary>
    /// Interaction logic for AddPhoneToExtWindow.xaml
    /// </summary>
    public partial class AddPhoneToExtWindow
    {
        public AddPhoneToExtWindow(string extensionNumber)
        {
            InitializeComponent();
            extension = extensionNumber;
            var extensionDisplayString = "Selected Extension: " + extensionNumber;
            ExtTextBlock.Text = extensionDisplayString;
            ExtTextBlock.Visibility = Visibility.Visible;
        }

        public string extension { get; }

        private async void AddPhoneToExtension_Click(object sender, RoutedEventArgs e)
        {
            var extensionNumber = MainWindow.CurrentExtension;
            if(PhonesDropDownList.SelectedItem is Phone selectedPhone)
            {
                var phoneType = selectedPhone.Model;
                await SavePhone(phoneType, MacAddressTextBlock.Text.CleanUpMacAddress(), extensionNumber);
            }

            Close();
        }

        private async Task SavePhone(string phoneType, string macAddress, string extensionNUmber)
        {
            var result = await MainWindow.ThreeCxClient.CreatePhoneOnServer(phoneType, macAddress, extensionNUmber);

            var pin = await MainWindow.ThreeCxClient.GetExtensionPinNumber(extensionNUmber);

            if(result == "OK")
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