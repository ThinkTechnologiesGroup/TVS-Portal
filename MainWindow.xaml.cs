using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThinkVoipTool.Models;
using ThinkVoipTool.Properties;

//using ThinkVoipTool.Skyswitch;

namespace ThinkVoipTool
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public const string TvsT46S = "Yealink T46S-tvs_yealinkt4x (tvs_yealinkt4x.ph.xml)";
        public const string TvsT48S = "Yealink T48S-tvs_yealinkt4x (tvs_yealinkt4x.ph.xml)";
        public const string YealinkCp960 = "Yealink CP960";
        public const string YealinkT57W = "Yealink T57W";
        public const string YealinkT40G = "Yealink T40G";
        public const string YealinkT42S = "Yealink T42S";
        public const string YealinkT46S = "Yealink T46S";
        public const string YealinkT48S = "Yealink T48S";
        public const string FanvilH5 = "Fanvil H5";


        public static List<Phone> PhoneModels = new List<Phone>
        {
            new Phone {Model = TvsT46S, ModelDisplayName = "TVS - Yealink T46S"},
            new Phone {Model = TvsT48S, ModelDisplayName = "TVS - Yealink T48S"},
            new Phone {Model = YealinkCp960},
            new Phone {Model = YealinkT40G},
            new Phone {Model = YealinkT42S},
            new Phone {Model = YealinkT46S},
            new Phone {Model = YealinkT48S},
            new Phone {Model = YealinkT57W},
            new Phone {Model = FanvilH5}
        };

        public static bool IsAuthenticated;
        public static List<Extension> ExtensionList;
        public static ThreeCxClient ThreeCxClient;
        public static int CompanyId;

        public static string CurrentExtension;

        //public static List<Extension> ToBeAdded = new List<Extension>();
        public static string ThreeCxPassword;
        public static List<Extension> SystemExtensions;
        public static Extension CurrentExtensionClass;
        public static IList ToBeUpdated;
        public static string AuthU = string.Empty;
        public static string AuthP = string.Empty;
        public static string CwApiUser = string.Empty;
        public static string CwApiKey = string.Empty;
        public static bool IsAdmin;

        private static ConnectWiseConnection _cwClient;
        //private static SkySwitchTelcoToken _skySwitchTelcoToken = new SkySwitchTelcoToken();

        public bool IsDark = Settings.Default.isDark;
        public bool IsFirstLaunch = Settings.Default.firstLaunch;
        public Views LastView;
        public ThreeCxLoginInfo LoginInfo;


        public MainWindow()
        {
            InitializeComponent();
            ShowMenu();
        }

        public MainWindow(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
            InitializeComponent();
            ShowMenu();
        }

        public bool ShowTtg => Settings.Default.showTtgClients;
        public static string SavedUser => AdAuthClient.TryGetUser();
        public static string SavedPass => AdAuthClient.TryGetPassword();


        private void ShowMenu()
        {
            MainMenu.Visibility = Visibility.Visible;
            ShowTtgCheckbox.IsChecked = ShowTtg;
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            await LoginProcess();
        }

        private async Task LoginProcess()
        {
            // Copy user settings from previous application version if necessary
            if(Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Settings.Default.Save();
            }

            if(IsFirstLaunch)
            {
                IsDark = ReadRegistry.isDarkEnabled;
                Settings.Default.firstLaunch = false;
                Settings.Default.Save();
            }

            ThemeControl.SetTheme(this);

            if(IsAuthenticated)
            {
                return;
            }

            if(!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftAlt))
            {
                if(Settings.Default.RememberMe)
                {
                    if(LoginWindow.TryLogin(SavedUser, SavedPass))
                    {
                        IsAuthenticated = true;
                    }
                }
            }


            if(!IsAuthenticated)
            {
                var window = new LoginWindow();
                window.ShowDialog();
            }

            if(!IsAuthenticated)
            {
                Close();
                return;
            }


            if(IsAdmin)
            {
                AdminMenu.Visibility = Visibility.Visible;
                ExtensionRemoveButton.IsEnabled = true;
                ResetPasswordMenuItem.IsEnabled = true;
                MakeExtAdminMenuItem.IsEnabled = true;
                BillingButton.IsEnabled = true;
            }

            AuthU = await Secrets.GetSecretValue("AdAuthUser");
            AuthP = await Secrets.GetSecretValue("AdAuthPass");
            if(!IsVisible)
            {
                Show();
            }
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(CwApiUser == "")
            {
                CwApiUser = await Secrets.GetSecretValue("CWApiUser");
            }

            if(CwApiKey == "")
            {
                CwApiKey = await Secrets.GetSecretValue("CWApiKey");
            }

            _cwClient = new ConnectWiseConnection(CwApiUser, CwApiKey);
            await UpdateCustomerList();
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
        }


        private async Task UpdateCustomerList()
        {
            var allVoipClients = await _cwClient.GetAllTvsVoIpClients();

            if(ShowTtg)
            {
                allVoipClients.AddRange(await _cwClient.GetAllThinkVoIpClients());
            }

            if(IsAdmin)
            {
                var ttgCompany = new BaseModels.Company {name = "!Think", id = 250};
                var ttgAgreement = new CompanyModel.Agreement {company = ttgCompany};
                allVoipClients.Add(ttgAgreement);
            }

            CustomersList.ItemsSource = allVoipClients.OrderBy(a => a.company.name);
            CustomersList.Visibility = Visibility.Visible;
        }

        private async void CustomersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListBox listBoxSender && listBoxSender.SelectedItems.Count == 0)
            {
                return;
            }

            if(Mouse.RightButton == MouseButtonState.Pressed)
            {
                var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
                if(selectedCompany != null)
                {
                    CompanyId = selectedCompany.company.id;
                }
            }

            await UpdateSelectedCompanyInfo();
        }


        public async Task UpdateSelectedCompanyInfo()
        {
            CleanExtensionDataGrid();
            LastView = Views.None;
            PleaseWaitTextBlock.SetValue(TextBlock.TextProperty, "Please wait...");
            PleaseWaitTextBlock.Visibility = Visibility.Visible;
            ThreeCxClient = null;
            var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
            if(selectedCompany != null)
            {
                CompanyId = selectedCompany.company.id;
            }

            try
            {
                ResetPasswordMenuItem.Visibility = Visibility.Visible;
                await DisplayClientInfo(CompanyId);
            }
            catch
            {
                PleaseWaitTextBlock.SetValue(TextBlock.TextProperty, "Failed to Open Client");
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
            RefreshButton.Visibility = Visibility.Hidden;
            RefreshSeperator.Visibility = Visibility.Hidden;
            Open3CxButton.Visibility = Visibility.Hidden;
            OpenConfluenceButton.Visibility = Visibility.Hidden;

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
            //var user = await Secrets.GetSecretValue("AdAuthUser");
            //var pass = await Secrets.GetSecretValue("AdAuthPass");

            LastView = Views.Valid;
            ExtensionsTotalDisplay.Visibility = Visibility.Hidden;
            var company = await _cwClient.GetCompany(companyId);
            var pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty));
            var loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId);
            LoginInfo = loginInfo;
            ThreeCxPassword = loginInfo.Password;
            ThreeCxClient = new ThreeCxClient(loginInfo.HostName, loginInfo.Username, loginInfo.Password);
            SystemExtensions = await ThreeCxClient.GetSystemExtensions();

            if(ThreeCxClient != null)
            {
                ExtensionList = await ThreeCxClient.GetExtensionsList();
                await UpdateDisplay();
            }
        }

        private async Task UpdateExtensionsCountDisplay()
        {
            var extCount = ExtensionList.Count;
            var invalidExtensions = GetUnbilledExtensionsCount(ExtensionList);
            var phones = await ThreeCxClient.GetPhonesList();
            UpdateExtensionDisplayGridNames();
            ExtensionsTotal.Text = extCount.ToString();
            InValidExtensions.Text = invalidExtensions.ToString();
            TotalValidExtensions.Text = (extCount - invalidExtensions).ToString();


            PhonesTotal.Text = phones.Where(phone => !phone.Model.ToLower().Contains("windows"))
                .Count(phone => !phone.Model.ToLower().Contains("web client")).ToString();


            VoicemailOnlyExtensionsCount.Text = GetVoicemailOnlyExtensions(ExtensionList).Count.ToString();
            ForwardingOnlyExtensionsCount.Text = GetForwardingOnlyExtensions(ExtensionList).Count.ToString();

            BilledUserExtensionsCount.Text = GetBilledUserExtensions(ExtensionList).Count.ToString();
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

        private static int GetUnbilledExtensionsCount(IEnumerable<Extension> extensions)
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

            if(CompanyId == 19532)
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
            RefreshButton.Visibility = Visibility.Visible;
            RefreshSeperator.Visibility = Visibility.Visible;
            Open3CxButton.Visibility = Visibility.Visible;
            OpenConfluenceButton.Visibility = Visibility.Visible;
        }

        private async void ExtensionsTotalDisplay_Click(object sender, RoutedEventArgs e)
        {
            LastView = Views.Total;
            var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            await DisplayExtensionInfo();
        }

        private async Task DisplayExtensionInfo()
        {
            ExtensionList = await ThreeCxClient.GetExtensionsList();
            ExtensionList = ExtensionList.OrderBy(a => a.Number).ToList();
            ListViewGrid.ItemsSource = ExtensionList;
            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.Visibility = Visibility.Visible;
        }

        private async void ExtensionsTotalInvalid_Click(object sender, RoutedEventArgs e)
        {
            LastView = Views.Invalid;
            var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            await DisplayInvalidExtensionInfo();
        }

        private async Task DisplayInvalidExtensionInfo()
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
            if(!cleanedExtensions.Any())
            {
                ListViewGrid.Visibility = Visibility.Hidden;
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
            LastView = Views.Valid;
            var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
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
            LastView = Views.Phones;
            var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            await DisplayPhones();
        }

        private async Task DisplayPhones()
        {
            var phones = await ThreeCxClient.GetPhonesList();
            var cleanedPhones = phones
                .Where(phone => !phone.Model.ToLower().Contains("windows"))
                .Where(phone => !phone.Model.ToLower().Contains("app"))
                .Where(phone => !phone.Model.ToLower().Contains("web client"));
            // ReSharper disable once PossibleMultipleEnumeration
            if(!cleanedPhones.Any())
            {
                cleanedPhones = new List<Phone>();
            }


            ListViewGrid.Visibility = Visibility.Hidden;
            // ReSharper disable once PossibleMultipleEnumeration
            cleanedPhones = cleanedPhones.ToList().OrderBy(a => a.ExtensionNumber);
            PhoneListViewGrid.ItemsSource = cleanedPhones;
            PhoneListViewGrid.Visibility = Visibility.Visible;
        }


        private async void MenuItem_Add_New_Phone_Click(object sender, RoutedEventArgs e)
        {
            if(ListViewGrid.SelectedItem == null || ListViewGrid.SelectedItem.ToString() == "{NewItemPlaceholder}")
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
                    var phoneWindow = new AddPhoneToExtWindow(CurrentExtension)
                        {PhonesDropDownList = {ItemsSource = PhoneModels, DisplayMemberPath = "ModelDisplayName"}};
                    phoneWindow.ShowDialog();
                    await UpdateView();
                    await DisplayExtensionInfo();

                    break;
                }
            }
        }


        private async void OnRemoveClick(object sender, RoutedEventArgs e)
        {
            if(ListViewGrid.SelectedItem == null)
            {
                return;
            }

            switch (ListViewGrid.SelectedItem.GetType().Name)
            {
                case "Extension":
                {
                    var extensions = ListViewGrid.SelectedItems.Cast<Extension>().ToList();
                    var extensionListString = "";
                    if(extensions.Count == 1)
                    {
                        extensionListString = extensions[0].Number;
                    }
                    else
                    {
                        for (var i = 0; i < extensions.Count; i++)
                            if(i != extensions.Count - 1)
                            {
                                extensionListString += extensions[i].Number + ", ";
                            }
                            else
                            {
                                if(extensions.Count == 2)
                                {
                                    extensionListString = extensionListString.Replace(",", "");
                                }

                                extensionListString += "and " + extensions[i].Number;
                            }
                    }

                    var result = MessageBox.Show($"Are you sure you want to remove extension(s) {extensionListString}?", "Are you sure?",
                        MessageBoxButton.YesNo);

                    if(result == MessageBoxResult.Yes)
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

        private void Menu_Item_ResetPassword(object sender, RoutedEventArgs e)
        {
            if(CustomersList.SelectedItem != null)
            {
                var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
                Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
                var companyId = selectedCompany.company.id;
                CompanyId = companyId;

                var window = new PasswordResetWindow(companyId);
                window.ShowDialog();
            }

            else
            {
                MessageBox.Show("Please select a client first.", "Error");
            }
        }


        private void AddExtension_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
            if(selectedCompany != null)
            {
                var companyId = selectedCompany.company.id;
                CompanyId = companyId;
            }

            var window = new ExtensionTypeSelectionWindow(ThreeCxClient, this);
            window.ShowDialog();
        }


        private void Grid_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
        }

        public async Task UpdateDisplay()
        {
            UpdateExtensionDataGrid();
            await UpdateView();
            switch (LastView)
            {
                case Views.None:
                    await DisplayExtensionInfo();

                    break;
                case Views.Valid:
                    await DisplayValidExtensions(CompanyId);

                    break;
                case Views.Invalid:
                    await DisplayInvalidExtensionInfo();

                    break;
                case Views.Total:
                    await DisplayExtensionInfo();

                    break;
                case Views.Phones:
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
            }
        }


        private void OnThemeClick(object sender, RoutedEventArgs e)
        {
            IsDark = !IsDark;
            ThemeControl.SetTheme(this);
        }


        private void OnStandardizeClick(object sender, RoutedEventArgs e)
        {
            var extensionsToEditList = ListViewGrid.SelectedItems;

            if(ListViewGrid.SelectedItem == null || ListViewGrid.SelectedItem.ToString() == "{NewItemPlaceholder}")
            {
                MessageBox.Show("Please select an extension first.", "Error :(");
                return;
            }

            ToBeUpdated = extensionsToEditList;

            var selectedItem = ListViewGrid.SelectedItem as Extension;
            CurrentExtension = selectedItem?.Number;
            CurrentExtensionClass = selectedItem;

            var window = new ExtensionTypeSelectionWindow(ThreeCxClient, this, true);
            window.Show();
        }

        private void VoicemailOnlyExtensionsDisplay_Click(object sender, RoutedEventArgs e)
        {
            DisplayVoicemailExtensionsInGrid();
        }

        private async void DisplayVoicemailExtensionsInGrid()
        {
            ExtensionList = await ThreeCxClient.GetExtensionsList();
            LastView = Views.VoicemailOnly;
            var vmOnlyList = GetVoicemailOnlyExtensions(ExtensionList);

            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.ItemsSource = vmOnlyList.OrderBy(a => a.Number).ToList();
            ListViewGrid.Visibility = Visibility.Visible;
        }

        private async void DisplayForwardingExtensionsInGrid()
        {
            ExtensionList = await ThreeCxClient.GetExtensionsList();
            LastView = Views.ForwardingOnly;
            var fwdOnlyList = GetForwardingOnlyExtensions(ExtensionList);


            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.ItemsSource = fwdOnlyList.OrderBy(a => a.Number).ToList();
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
            LastView = Views.BilledToClient;
            var billedToClient = GetBilledUserExtensions(ExtensionList);

            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.ItemsSource = billedToClient.OrderBy(a => a.Number).ToList();
            ListViewGrid.Visibility = Visibility.Visible;
        }


        private async void OnShowTtgClientsClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.showTtgClients = ShowTtgCheckbox.IsChecked;
            Settings.Default.Save();
            await UpdateCustomerList();
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
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }


        private async void openConfluenceButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompany = (CompanyModel.Agreement) CustomersList.SelectedItems[0];
            if(selectedCompany != null)
            {
                CompanyId = selectedCompany.company.id;
            }

            var company = await _cwClient.GetCompany(CompanyId);
            var url = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty), true);

            OpenUrl("https://docs.think-team.com" + url);
        }

        private async void open3cxPage_Click(object sender, RoutedEventArgs e)
        {
            var company = await _cwClient.GetCompany(CompanyId);
            var pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty));
            var loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId);


            var hostName = loginInfo.HostName;
            var cleanedHostName = Regex.Replace(hostName, @"/api/", string.Empty);

            OpenUrl(cleanedHostName);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await UpdateDisplay();
            }
            catch
            {
                await UpdateSelectedCompanyInfo();
            }
        }

        private async void makeExtAdminMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var extensionsToEditList = ListViewGrid.SelectedItems;

            if(ListViewGrid.SelectedItem == null || ListViewGrid.SelectedItem.ToString() == "{NewItemPlaceholder}")
            {
                MessageBox.Show("Please select an extension first.", "Error :(");
                return;
            }

            ToBeUpdated = extensionsToEditList;

            var selectedItem = ListViewGrid.SelectedItem as Extension;
            CurrentExtension = selectedItem?.Number;
            CurrentExtensionClass = selectedItem;


            var result = await ThreeCxClient.MakeExtensionAdmin(CurrentExtension);
            if(result == "Failed")
            {
                MessageBox.Show("Failed to set as admin", "Error");
            }
            else
            {
                MessageBox.Show("Success");
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private async void LogOff_OnClick(object sender, RoutedEventArgs e)
        {
            IsAuthenticated = false;
            IsAdmin = false;
            AdminMenu.Visibility = Visibility.Hidden;
            ExtensionRemoveButton.IsEnabled = false;
            ResetPasswordMenuItem.IsEnabled = false;
            MakeExtAdminMenuItem.IsEnabled = false;
            Hide();
            await LoginProcess();
        }

        private void Billing_OnClick(object sender, RoutedEventArgs e)
        {
            PhoneListViewGrid.Visibility = Visibility.Hidden;
            ListViewGrid.Visibility = Visibility.Hidden;

            var billing = new Billing.Billing();
            var test = billing.LastSixMonths;
        }
    }
}