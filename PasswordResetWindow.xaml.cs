using System.Windows;
using RestSharp;

namespace ThinkVoipTool
{
    /// <summary>
    ///     Interaction logic for PasswordResetWindow.xaml
    /// </summary>
    public partial class PasswordResetWindow
    {
        private ThreeCxLoginInfo _loginInfo;
        private string _pageId;
        private ThreeCxClient _threeCxClient;

        public PasswordResetWindow()
        {
            InitializeComponent();
        }

        public PasswordResetWindow(int companyId)
        {
            InitializeComponent();

            PasswordResetInit(companyId);
        }

        private async void PasswordReset_Click(object sender, RoutedEventArgs e)
        {
            var password1 = FirstPassword.Password;
            var password2 = SecondPassword.Password;
            _loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(_pageId.Replace(", PA", ""));
            var originalPassword = _loginInfo.Password;

            if(password1 != password2)
            {
                MessageBox.Show(this, "Passwords do not match.", "Failed");
            }

            var confUpdateResponse = Docs.ConfClient.UpdateThreeCxPassword(_pageId, password1);

            if(confUpdateResponse == ResponseStatus.Error)
            {
                MessageBox.Show("Failed to update confluence - aborting.", "Failed");
            }


            //update 3cx password

            var passwordUpdateResult = await _threeCxClient.ResetPassword(password1);

            //check for failing on 3cx and then revert the confluence change back to stored original if needed 

            if(passwordUpdateResult == "OK")
            {
                //Everything worked
                MessageBox.Show(this, "Password has been updated :)", "Success");
            }
            else
            {
                //Meh...
                var revertConfluenceUpdateStatus = Docs.ConfClient.UpdateThreeCxPassword(_pageId, originalPassword);

                if(revertConfluenceUpdateStatus == ResponseStatus.Error)
                {
                    //Fuck, did the internet die?
                    MessageBox.Show(this, "Failed to update 3cx and was unable to revert changes to confluence \n" +
                                          $"Please check page ID:{_pageId} and update the password back to \"{originalPassword}\"", "Failed");
                }

                MessageBox.Show(this, "Failed to update 3cx server. Changes to confluence have been reverted.", "Failed");
            }

            Close();
        }

        private void Cancel_PasswordReset_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public async void PasswordResetInit(int companyId)
        {
            var cwClient = new ConnectWiseConnection(MainWindow.CwApiUser, MainWindow.CwApiKey);

            var company = await cwClient.GetCompany(companyId);
            _pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", ""));
            _loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(_pageId);
            _threeCxClient = new ThreeCxClient(_loginInfo.HostName, _loginInfo.Username, _loginInfo.Password);
        }
    }
}