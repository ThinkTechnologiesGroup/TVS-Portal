using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ThinkVoipTool
{
    /// <summary>
    ///     Interaction logic for AddPhoneToExtWindow.xaml
    /// </summary>
    public partial class AddPhoneToExtWindow
    {
        public AddPhoneToExtWindow(string? extensionNumber)
        {
            InitializeComponent();
            var extensionDisplayString = "Selected Extension: " + extensionNumber;
            ExtTextBlock.Text = extensionDisplayString;
            ExtTextBlock.Visibility = Visibility.Visible;
        }

        private async void AddPhoneToExtension_Click(object sender, RoutedEventArgs e)
        {
            var extensionNumber = MainWindow.CurrentExtension;
            if(PhonesDropDownList.SelectedItem is Phone selectedPhone)
            {
                var phoneType = selectedPhone.Model;
                using (new OverrideCursor(Cursors.Wait))
                {
                    await SavePhone(phoneType, MacAddressTextBlock.Text.CleanUpMacAddress(), extensionNumber!);
                }
            }

            Close();
        }

        private async Task SavePhone(string phoneType, string macAddress, string extensionNUmber)
        {
            using (new OverrideCursor(Cursors.Wait))
            {
                var result = await MainWindow.ThreeCxClient!.CreatePhoneOnServer(phoneType, macAddress, extensionNUmber);

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
}