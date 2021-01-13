using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using MaterialDesignThemes.Wpf;

using ThinkVoipTool;
using ThinkVoipTool.Properties;

namespace ThinkVoip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public const string TvsT46s = "Yealink T46S-tvs_yealinkt4x (tvs_yealinkt4x.ph.xml)";
        public const string YealinkCp960 = "Yealink CP960";
        public const string YealinkT57W = "Yealink T57W";
        public const string YealinkT40G = "Yealink T40G";
        public const string YealinkT46S = "Yealink T46S";
        public const string YealinkT48S = "Yealink T48s";
        public const string FanvilH5 = "Fanvil H5";




        public static List<Phone> phoneModels = new List<Phone>()
        {
            new Phone{ Model = YealinkCp960},
            new Phone{ Model = YealinkT40G},
            new Phone{ Model = YealinkT46S},
            new Phone{ Model = YealinkT48S},
            new Phone{ Model = YealinkT57W},
            new Phone{ Model = FanvilH5},
            new Phone{ Model = TvsT46s, ModelDisplayName = "TVS - Yealink T46s"}

        };

        public bool isDark = Settings.Default.isDark;
        public static bool isAuthenticated = false;
        public static List<Extension> ExtensionList;
        public static ThreeCxClient ThreeCxClient;
        public static int CompanyId;
        public static string CurrentExtension;
        public static List<Extension> ToBeAdded = new List<Extension>();
        public static string ThreeCxPassword;
        public ThreeCxLoginInfo loginInfo;
        public static List<Extension> systemExtensions;
        public static Extension CurrentExtensionClass;
        public static IList ToBeUpdated;
        public Views lastView;
        public bool showTtg => Settings.Default.showTtgClients;
        public static string savedUser => TryGetUser();
        public static string savedPass => TryGetPassword();

        public MainWindow()
        {
            InitializeComponent();
            ShowMenu();
            SetTheme();

            try
            {
                if (Settings.Default.RememberMe)
                {

                    if (Login.TryLogin(savedUser, savedPass))
                    {
                        MainWindow.isAuthenticated = true;
                    }
                }

                if (!isAuthenticated)
                {
                    var window = new Login();
                    window.ShowDialog();
                }
                if (!isAuthenticated)
                {
                    this.Close();
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Unable to connect to domain for authentication", "Error");
            }

        }

        public static string TryGetUser()
        {
            try
            {
                var storedUser = Settings.Default.userName;
                var userEntropy = Settings.Default.userEntropy;
                byte[] encodedUser = ProtectedData.Unprotect(storedUser, userEntropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(encodedUser);
            }
            catch
            {
                return string.Empty;
            }
        }
        public static string TryGetPassword()
        {
            try
            {
                var storedPassword = Settings.Default.passWord;
                var entropy = Settings.Default.entropy;
                byte[] encodedPassword = ProtectedData.Unprotect(storedPassword, entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(encodedPassword);
            }
            catch
            {
                return string.Empty;
            }

        }

        private void ShowMenu()
        {
            MainMenu.Visibility = Visibility.Visible;
            ShowqTtgCheckbox.IsChecked = showTtg;

        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await updateCustomerList();
        }

        private async Task updateCustomerList()
        {
            var allVoipClients = await ConnectWiseConnection.CwClient.GetAllTvsVoIpClients();

            if (showTtg)
            {
                allVoipClients.AddRange(await ConnectWiseConnection.CwClient.GetAllThinkVoIpClients());
            }
            CustomersList.ItemsSource = allVoipClients.OrderBy(a => a.company.name);

        }

        private async void CustomersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBoxSender = sender as ListBox;
            if (listBoxSender.SelectedItems.Count == 0) return;
            await UpdateSelectedCompanyInfo();
        }

        public async Task UpdateSelectedCompanyInfo()
        {

            CleanExtensionDataGrid();
            lastView = Views.none;
            PleaseWaitTextBlock.SetValue(TextBlock.TextProperty, "Please wait...");
            PleaseWaitTextBlock.Visibility = Visibility.Visible;
            ThreeCxClient = null;
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            CompanyId = selectedCompany.company.id;
            try
            {
                await DisplayClientInfo(CompanyId);
            }
            catch
            {
                this.PleaseWaitTextBlock.SetValue(TextBlock.TextProperty, "Failed to Open Client");
            }

        }

        public void CleanExtensionDataGrid()
        {
            ThinkyMainImage.Visibility = Visibility.Hidden;
            ListViewGrid.Visibility = Visibility.Hidden;

            AddExt.Visibility = Visibility.Hidden;
            AddPhoneButton.Visibility = Visibility.Hidden;
            ExtensionsHeader.Visibility = Visibility.Hidden;
            ExtSeperator.Visibility = Visibility.Hidden;
            PhoneSeperator.Visibility = Visibility.Hidden;
            ExtSeperatorOperators.Visibility = Visibility.Hidden;
            ForwardingOnlyExtensionsDisplay.Visibility = Visibility.Hidden;
            BilledUserExtensionsDisplay.Visibility = Visibility.Hidden;
            ForwardingOnlyExtensionsDisplay.Visibility = Visibility.Hidden;

            ForwardingOnlyExtensionsCount.Text = "";
            BilledUserExtensionsDisplay.Visibility = Visibility.Hidden;
            BilledUserExtensionsCount.Text = "";
            ExtensionsTotalDisplay.Visibility = Visibility.Hidden;
            VoimailOnlyExtensionsDisplay.Visibility = Visibility.Hidden;
            ExtensionsTotalInvalid.Visibility = Visibility.Hidden;
            ExtensionsTotalValid.Visibility = Visibility.Hidden;
            PhonesTotalDisplay.Visibility = Visibility.Hidden;
            ExtensionsTotal.Text = "";
            InValidExtensions.Text = "";
            TotalValidExtensions.Text = "";
            PhonesTotal.Text = "";
            VoicemailOnlyExtensionsCount.Text = "";

        }

        public void UpdateExtensionDataGrid()
        {


            ListViewGrid.Visibility = Visibility.Hidden;

            VoimailOnlyExtensionsDisplay.Visibility = Visibility.Hidden;
            ForwardingOnlyExtensionsDisplay.Visibility = Visibility.Hidden;
            BilledUserExtensionsDisplay.Visibility = Visibility.Hidden;
            ExtensionsTotalDisplay.Visibility = Visibility.Hidden;
            ExtensionsTotalInvalid.Visibility = Visibility.Hidden;
            ExtensionsTotalValid.Visibility = Visibility.Hidden;
            PhonesTotalDisplay.Visibility = Visibility.Hidden;
            ExtensionsTotal.Text = "";
            InValidExtensions.Text = "";
            TotalValidExtensions.Text = "";
            PhonesTotal.Text = "";
            VoicemailOnlyExtensionsCount.Text = "";
            ForwardingOnlyExtensionsCount.Text = "";
            BilledUserExtensionsCount.Text = "";
        }

        public async Task DisplayClientInfo(int companyId)
        {


            lastView = Views.valid;
            ExtensionsTotalDisplay.Visibility = Visibility.Hidden;
            var company = await ConnectWiseConnection.CwClient.GetCompany(companyId);
            var pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty));
            var loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId);
            this.loginInfo = loginInfo;
            ThreeCxPassword = loginInfo.Password;
            ThreeCxClient = new ThreeCxClient(loginInfo.HostName, loginInfo.Username, loginInfo.Password);
            systemExtensions = await ThreeCxClient.GetSystemExtensions();
            ExtensionList = await ThreeCxClient.GetExtensionsList();

            await UpdateDisplay();

        }

        private async Task UpdateExtensionsCountDisplay()
        {
            var extCount = ExtensionList.Count();
            int invalidExtensions = GetUnbilledExtensionsCount(ExtensionList);
            var phones = await ThreeCxClient.GetPhonesList();
            UpdateExtensionDisplayGridNames();
            ExtensionsTotal.Text = extCount.ToString();
            InValidExtensions.Text = invalidExtensions.ToString();
            TotalValidExtensions.Text = (extCount - invalidExtensions).ToString();


            PhonesTotal.Text = phones.Where(phone => !phone.Model.ToLower().Contains("windows"))
                .Count(phone => !phone.Model.ToLower().Contains("web client")).ToString();


            VoicemailOnlyExtensionsCount.Text = GetVoicemailOnlyExtensions(ExtensionList).Count().ToString();
            ForwardingOnlyExtensionsCount.Text = GetForwardingOnlyExtensions(ExtensionList).Count().ToString();

            BilledUserExtensionsCount.Text = GetBilledUserExtensions(ExtensionList).Count().ToString();

        }

        private List<Extension> GetBilledUserExtensions(List<Extension> extensions)
        {
            var forwarding = extensions.Where(a => a.FirstName.ToLower().Contains("forward only")).ToList();
            var voicemail = extensions.Where(a => a.FirstName.ToLower().Contains("voicemail only")).ToList();
            var op = extensions.Where(ext =>
                            ext.FirstName.ToLower().Contains("test") ||
                            ext.LastName.ToLower().Contains("test") ||
                            ext.FirstName.ToLower().Contains("copy me") ||
                            ext.FirstName.ToLower().Equals("operator") ||
                            ext.FirstName.ToLower().Contains("template") ||
                            ext.LastName.ToLower().Contains("template"));

            var billedExtensions = extensions;
            billedExtensions.RemoveAll(a => forwarding.Contains(a));
            billedExtensions.RemoveAll(a => voicemail.Contains(a));
            billedExtensions.RemoveAll(a => op.Contains(a));
            return billedExtensions;
        }

        private List<Extension> GetForwardingOnlyExtensions(List<Extension> extensions)
        {
            return extensions.Where(a => a.FirstName.ToLower().Contains("forward only")).ToList();
        }

        private List<Extension> GetVoicemailOnlyExtensions(List<Extension> extensions)
        {
            return extensions.Where(a => a.FirstName.ToLower().Contains("voicemail only")).ToList();
        }

        private void UpdateExtensionDisplayGridNames()
        {
            ExtensionsTotalDisplay.Visibility = Visibility.Visible;
            ExtensionsTotalInvalid.Visibility = Visibility.Visible;
            VoimailOnlyExtensionsDisplay.Visibility = Visibility.Visible;
            ForwardingOnlyExtensionsDisplay.Visibility = Visibility.Visible;
            ExtensionsTotalValid.Visibility = Visibility.Visible;
            BilledUserExtensionsDisplay.Visibility = Visibility.Visible;
            PhonesTotalDisplay.Visibility = Visibility.Visible;

        }

        private static int GetUnbilledExtensionsCount(List<Extension> extensions)
        {
            return extensions.Count(ext =>
                            ext.FirstName.ToLower().Contains("test") ||
                            ext.LastName.ToLower().Contains("test") ||
                            ext.FirstName.ToLower().Contains("copy me") ||
                            ext.FirstName.ToLower().Equals("operator") ||
                            ext.FirstName.ToLower().Contains("template") ||
                            ext.LastName.ToLower().Contains("template"));
        }

        public async Task UpdateView()
        {
            ExtensionList = await ThreeCxClient.GetExtensionsList();

            if (CompanyId == 19532)
            {
                ExtensionsTotalDisplay.Content = "No valid Info";
                return;
            }


            await UpdateExtensionsCountDisplay();
            ExtSeperator.Visibility = Visibility.Visible;
            ExtSeperatorOperators.Visibility = Visibility.Visible;
            PhoneSeperator.Visibility = Visibility.Visible;
            PleaseWaitTextBlock.Visibility = Visibility.Hidden;
            AddExt.Visibility = Visibility.Visible;
            AddPhoneButton.Visibility = Visibility.Visible;
            ExtensionsHeader.Visibility = Visibility.Visible;

        }

        private async void ExtensionsTotalDisplay_Click(object sender, RoutedEventArgs e)
        {
            lastView = Views.total;
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            var companyId = selectedCompany.company.id;
            await DisplayExtensionInfo(companyId);

        }

        private async Task DisplayExtensionInfo(int companyId)
        {
            ExtensionList = await ThreeCxClient.GetExtensionsList();
            ExtensionList = ExtensionList.OrderBy(a => a.Number).ToList();
            ListViewGrid.ItemsSource = ExtensionList;
            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.Visibility = Visibility.Visible;
        }

        private async void ExtensionsTotalInvalid_Click(object sender, RoutedEventArgs e)
        {
            lastView = Views.invalid;
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            var companyId = selectedCompany.company.id;
            await DisplayInvalidExtensionInfo(companyId);
        }

        private async Task DisplayInvalidExtensionInfo(int companyId)
        {

            ExtensionList = await ThreeCxClient.GetExtensionsList();
            var cleanedExtensions = ExtensionList
                .Where(ext =>
                    ext.FirstName.ToLower().Contains("test") ||
                    ext.LastName.ToLower().Contains("test") ||
                    ext.FirstName.ToLower().Contains("copy me") ||
                    ext.FirstName.ToLower().Equals("operator") ||
                    ext.FirstName.ToLower().Contains("template") ||
                    ext.LastName.ToLower().Contains("template")).ToList();
            if (!cleanedExtensions.Any())
            {
                ListViewGrid.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                ListViewGrid.ItemsSource = cleanedExtensions;
                PhoneListViewGrid.Visibility = Visibility.Hidden;

                ListViewGrid.Visibility = Visibility.Visible;
            }
        }

        private async void ExtensionsTotalValid_Click(object sender, RoutedEventArgs e)
        {
            lastView = Views.valid;
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            var companyId = selectedCompany.company.id;
            await DisplayValidExtensions(companyId);

        }

        public async Task DisplayValidExtensions(int companyId)
        {


            ExtensionList = await ThreeCxClient.GetExtensionsList();
            var cleanedExtensions = new List<Extension>();
            cleanedExtensions.AddRange(ExtensionList
                .Where(ext => !ext.FirstName.ToLower().Contains("test"))
                .Where(ext => !ext.LastName.ToLower().Contains("test"))
                .Where(ext => !ext.FirstName.ToLower().Contains("copy me"))
                .Where(ext => !ext.FirstName.ToLower().Equals("operator"))
                .Where(ext => !ext.FirstName.ToLower().Contains("template"))
                .Where(ext => !ext.LastName.ToLower().Contains("template")));

            ListViewGrid.ItemsSource = cleanedExtensions;
            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.Visibility = Visibility.Visible;
        }

        private async void PhonesTotalDisplay_Click(object sender, RoutedEventArgs e)
        {
            lastView = Views.phones;
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            var companyId = selectedCompany.company.id;
            await DisplayPhones();
        }

        private async Task DisplayPhones()
        {
            var phones = await ThreeCxClient.GetPhonesList();
            var cleanedPhones = phones
                .Where(phone => !phone.Model.ToLower().Contains("windows"))
                .Where(phone => !phone.Model.ToLower().Contains("app"))
                .Where(phone => !phone.Model.ToLower().Contains("web client"));
            if (!cleanedPhones.Any())
            {
                cleanedPhones = new List<Phone>();
            }


            ListViewGrid.Visibility = Visibility.Hidden;
            cleanedPhones = cleanedPhones.ToList().OrderBy(a => a.ExtensionNumber);
            PhoneListViewGrid.ItemsSource = cleanedPhones;
            PhoneListViewGrid.Visibility = Visibility.Visible;


        }



        private async void MenuItem_Add_New_Phone_Click(object sender, RoutedEventArgs e)
        {

            if (ListViewGrid.SelectedItem == null || ListViewGrid.SelectedItem.ToString() == "{NewItemPlaceholder}")
            {
                MessageBox.Show("Please select an extension first.", "Error :(");
                return;
            }


            switch (ListViewGrid.SelectedItem.GetType().Name)
            {
                case "Extension":
                    {
                        var selectedItem = ListViewGrid.SelectedItem as Extension;
                        CurrentExtension = selectedItem?.Number;
                        CurrentExtensionClass = selectedItem;
                        var phoneWindow = new AddPhoneToExtWindow(CurrentExtension) { PhonesDropDownList = { ItemsSource = phoneModels, DisplayMemberPath = "ModelDisplayName" } };
                        phoneWindow.ShowDialog();
                        await UpdateView();
                        await DisplayExtensionInfo(CompanyId);

                        break;
                    }
                case "Phone":
                    {
                        var phone = ListViewGrid.SelectedItem as Phone;
                        MessageBox.Show($"Add new phone eventually.....");

                        break;
                    }
            }
        }


        private static bool Validate(Extension extension)
        {
            var valid = extension.FirstName != "";
            if (extension.LastName == "")
            {
                valid = false;
            }

            if (extension.Number == "")
            {
                valid = false;
            }

            if (extension.Email == "")
            {
                valid = false;
            }

            return valid;
        }

        private async void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (ListViewGrid.SelectedItem == null)
            {
                return;
            }

            switch (ListViewGrid.SelectedItem.GetType().Name)
            {
                case "Extension":
                    {
                        var extensions = ListViewGrid.SelectedItems.Cast<Extension>().ToList();
                        var extensionListString = "";
                        if (extensions.Count == 1)
                        {
                            extensionListString = extensions[0].Number;
                        }
                        else
                        {
                            for (var i = 0; i < extensions.Count; i++)
                                if (i != extensions.Count - 1)
                                {
                                    extensionListString += extensions[i].Number + ", ";
                                }
                                else
                                {
                                    if (extensions.Count == 2)
                                    {
                                        extensionListString = extensionListString.Replace(",", "");
                                    }

                                    extensionListString += "and " + extensions[i].Number;
                                }
                        }

                        var result = MessageBox.Show($"Are you sure you want to remove extension(s) {extensionListString}?", "Are you sure?",
                            MessageBoxButton.YesNo);

                        if (result == MessageBoxResult.Yes)
                        {
                            foreach (var extension in extensions)
                            {
                                await ThreeCxClient.DeleteExtension(extension.Number);
                            }

                            UpdateExtensionDataGrid();
                            await UpdateView();
                            await UpdateDisplay();

                        }

                        break;
                    }

            }
        }

        private async void Menu_Item_ResetPassword(object sender, RoutedEventArgs e)
        {
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            var companyId = selectedCompany.company.id;
            CompanyId = companyId;

            var window = new PasswordResetWindow(companyId);
            window.ShowDialog();
            await UpdateSelectedCompanyInfo();

        }


        private void AddExtension_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            var companyId = selectedCompany.company.id;
            CompanyId = companyId;

            var window = new ExtensionTypeSelectionWindow(ThreeCxClient, this);
            window.ShowDialog();

        }


        private void Grid_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {

        }

        public async Task UpdateDisplay()
        {
            ListViewGrid.Visibility = Visibility.Hidden;
            PhoneListViewGrid.Visibility = Visibility.Hidden;
            UpdateExtensionDataGrid();
            await UpdateView();
            switch (lastView)
            {
                case Views.none:
                    await DisplayExtensionInfo(CompanyId);

                    break;
                case Views.valid:
                    await DisplayValidExtensions(CompanyId);

                    break;
                case Views.invalid:
                    await DisplayInvalidExtensionInfo(CompanyId);

                    break;
                case Views.total:
                    await DisplayExtensionInfo(CompanyId);

                    break;
                case Views.phones:
                    await DisplayPhones();

                    break;
                case Views.VoicemailOnly:
                    DisplayVoicemailExtensionsInGrid();

                    break;
                case Views.ForwardingOnly:
                    DisplayForwardingExtensionsInGrid();

                    break;
                case Views.BilledToClient:
                    DisplayBilledUserExtensions();

                    break;
                default:
                    break;
            }

        }



        private void OnThemeClick(object sender, RoutedEventArgs e)
        {
            isDark = isDark ? false : true;
            SetTheme();
        }



        private void SetTheme()
        {

            var _paletteHelper = new PaletteHelper();
            ITheme theme = _paletteHelper.GetTheme();

            IBaseTheme baseTheme = isDark ? new MaterialDesignDarkTheme() : (IBaseTheme)new MaterialDesignLightTheme();
            theme.SetBaseTheme(baseTheme);
            _paletteHelper.SetTheme(theme);

            Settings.Default.isDark = isDark;
            Settings.Default.Save();


        }

        private void OnStandardizeClick(object sender, RoutedEventArgs e)
        {

            var extensionsToEditList = ListViewGrid.SelectedItems;

            if (ListViewGrid.SelectedItem == null || ListViewGrid.SelectedItem.ToString() == "{NewItemPlaceholder}")
            {
                MessageBox.Show("Please select an extension first.", "Error :(");
                return;
            }

            ToBeUpdated = extensionsToEditList;

            var selectedItem = ListViewGrid.SelectedItem as Extension;
            CurrentExtension = selectedItem?.Number;
            CurrentExtensionClass = selectedItem;

            var window = new ExtensionTypeSelectionWindow(ThreeCxClient, this, update: true);
            window.Show();
        }

        private void VoimailOnlyExtensionsDisplay_Click(object sender, RoutedEventArgs e)
        {
            DisplayVoicemailExtensionsInGrid();
        }

        private async void DisplayVoicemailExtensionsInGrid()
        {
            ExtensionList = await ThreeCxClient.GetExtensionsList();
            lastView = Views.VoicemailOnly;
            var vmOnlyList = GetVoicemailOnlyExtensions(ExtensionList);

            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.ItemsSource = vmOnlyList.OrderBy(a => a.Number).ToList();
            ListViewGrid.Visibility = Visibility.Visible;
        }
        private async void DisplayForwardingExtensionsInGrid()
        {
            ExtensionList = await ThreeCxClient.GetExtensionsList();
            lastView = Views.ForwardingOnly;
            var FwdOnlyList = GetForwardingOnlyExtensions(ExtensionList);



            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.ItemsSource = FwdOnlyList.OrderBy(a => a.Number).ToList();
            ListViewGrid.Visibility = Visibility.Visible;
        }

        private void ForwardingOnlyExtensionsDisplay_Click(object sender, RoutedEventArgs e)
        {
            DisplayForwardingExtensionsInGrid();
        }

        private void BilledUserExtensionsDisplay_Click(object sender, RoutedEventArgs e)
        {
            DisplayBilledUserExtensions();
        }

        private void DisplayBilledUserExtensions()
        {
            lastView = Views.BilledToClient;
            var BilledToClient = GetBilledUserExtensions(ExtensionList);

            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.ItemsSource = BilledToClient.OrderBy(a => a.Number).ToList();
            ListViewGrid.Visibility = Visibility.Visible;

        }

        private void OnTestClick(object sender, RoutedEventArgs e)
        {
            return;
        }


        private async void OnShowTtgClientsClick(object sender, RoutedEventArgs e)
        {
            ThinkVoipTool.Properties.Settings.Default.showTtgClients = ShowqTtgCheckbox.IsChecked;
            ThinkVoipTool.Properties.Settings.Default.Save();
            await updateCustomerList();
        }

        private void OnTestyButtonClick(object sender, RoutedEventArgs e)
        {


        }

        private async void OnCustListDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            var company = await ConnectWiseConnection.CwClient.GetCompany(CompanyId);
            var pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty));
            var loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId);


            var hostName = loginInfo.HostName;
            var cleanedHostName = Regex.Replace(hostName, @"/api/", string.Empty);

            OpenUrl(cleanedHostName);


        }
        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}