using System.Windows;

namespace ThinkVoipTool
{
    /// <summary>
    /// Interaction logic for PasswordResetWindow.xaml
    /// </summary>
    public partial class PasswordResetWindow : Window
    {

        private int CompanyID;
        private ThreeCxClient ThreeCxClient;
        private string pageId;
        private ThreeCxLoginInfo loginInfo;

        public PasswordResetWindow()
        {
            InitializeComponent();


        }
        public PasswordResetWindow(int companyId)
        {
            InitializeComponent();

            this.CompanyID = companyId;
            PasswordResetInit(companyId);



        }

        private async void PasswordReset_Click(object sender, RoutedEventArgs e)
        {
            var password1 = FirstPassword.Password;
            var password2 = SecondPassword.Password;
            var loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId.Replace(", PA", ""));
            var originalPassword = loginInfo.Password;

            if (password1 != password2)
            {
                MessageBoxResult result = MessageBox.Show(this, "Passwords do not match.", "Failed");

            }

            var confUpdateResponse = Docs.ConfClient.UpdateThreeCxPassword(pageId, password1);

            if (confUpdateResponse == RestSharp.ResponseStatus.Error)
            {
                MessageBox.Show("Failed to update confluence - aborting.", "Failed");
            }


            //update 3cx password

            var passwordUpdateResult = await ThreeCxClient.ResetPassword(password1);

            //check for failing on 3cx and then revert the confluence change back to stored original if needed 

            if (passwordUpdateResult == "OK")
            {
                //Everything worked
                MessageBox.Show(this, "Password has been updated :)", "Success");

            }
            else
            {
                //Meh...
                var revertConfluenceUpdateStatus = Docs.ConfClient.UpdateThreeCxPassword(pageId, originalPassword);

                if (revertConfluenceUpdateStatus == RestSharp.ResponseStatus.Error)
                {
                    //Fuck, did the internet die?
                    MessageBox.Show(this, $"Failed to update 3cx and was unable to revert changes to confluence \n" +
                        $"Please check page ID:{pageId} and update the password back to \"{originalPassword}\"", "Failed");
                }
                MessageBox.Show(this, "Failed to update 3cx server. Changes to confluence have been reverted.", "Failed");

            }

            this.Close();

        }

        private void Cancel_PasswordReset_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public async void PasswordResetInit(int companyId)
        {
            var CwClient = new ConnectWiseConnection(MainWindow.CwApiUser, MainWindow.CwApiKey);

            var company = await CwClient.GetCompany(companyId);
            pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", ""));
            loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId);
            ThreeCxClient = new ThreeCxClient(loginInfo.HostName, loginInfo.Username, loginInfo.Password);
        }
    }
}
