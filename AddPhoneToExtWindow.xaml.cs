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
            var selectedPhone = PhonesDropDownList.SelectedItem as Phone;
            var phoneType = selectedPhone.Model;
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