using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ThinkVoipTool.Properties;

namespace ThinkVoipTool
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow
    {
        //private readonly MainWindow _mainWindow;

        public LoginWindow()
        {
            InitializeComponent();
        }


        private void Window_Activated(object sender, System.EventArgs e)
        {
            UserNameEntry.Text = AdAuthClient.TryGetUser();
            RememberMeCheckBox.IsChecked = false;
            Settings.Default.RememberMe = false;
            Settings.Default.Save();
        }


        private async void OnLoginClick(object sender, RoutedEventArgs e)
        {
            ResultLabel.Visibility = Visibility.Visible;


            var username = UserNameEntry.Text.StripDomain();
            var password = PasswordEntry.Password;

            if(!ValidateEntries(username, password))
            {
                DisplayLoginResultInfo(Brushes.Red, "Please fill out all fields.");
                return;
            }

            DisplayLoginResultInfo(Brushes.Green, "Attempting Login...");


            if(await RunLogonProcess(username, password))
            {
                DisplayLoginResultInfo(Brushes.Green, "Success");
                MainWindow.IsAuthenticated = true;
                SaveUsername();

                if(RememberMeCheckBox.IsChecked != null && (bool) RememberMeCheckBox.IsChecked)
                {
                    SavePassword(password);
                }


                Close();
            }
            else
            {
                DisplayLoginResultInfo(Brushes.Red, "Login Failed");
            }
        }

        private void DisplayLoginResultInfo(Brush brush, string result)
        {
            ResultLabel.Foreground = brush;
            ResultLabel.Text = result;
            ResultLabel.Visibility = Visibility.Visible;
        }

        private static bool ValidateEntries(string userName, string passWord) => userName != "" && passWord != "";


        private static async Task<bool> RunLogonProcess(string userName, string passWord)
        {
            var result = Task.Run(() => TryLogin(userName, passWord));

            return await result;
        }


        private static void SavePassword(string passWord)
        {
            var plaintext = Encoding.UTF8.GetBytes(passWord);
            var entropy = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(entropy);
            }

            var ciphertext = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.CurrentUser);


            Settings.Default.passWord = ciphertext;
            Settings.Default.entropy = entropy;
            Settings.Default.RememberMe = true;
            Settings.Default.Save();
        }

        private void SaveUsername()
        {
            var plaintext = Encoding.UTF8.GetBytes(UserNameEntry.Text.StripDomain());
            var userEntropy = new byte[20];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(userEntropy);
            }

            var ciphertext = ProtectedData.Protect(plaintext, userEntropy, DataProtectionScope.CurrentUser);


            Settings.Default.userName = ciphertext;
            Settings.Default.userEntropy = userEntropy;
            Settings.Default.Save();
        }

        public static bool TryLogin(string userName, string pass1)
        {
            try
            {
                return AdAuthClient.ValidateUserByBind(userName, pass1);
            }
            catch
            {
                MessageBox.Show("Unable to connect to domain for authentication", "Error");
                return false;
            }
        }
    }
}