﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        public const string TvsT48s = "Yealink T48S-tvs_yealinkt4x (tvs_yealinkt4x.ph.xml)";
        public const string YealinkCp960 = "Yealink CP960";
        public const string YealinkT57W = "Yealink T57W";
        public const string YealinkT40G = "Yealink T40G";
        public const string YealinkT42S = "Yealink T42S";
        public const string YealinkT46S = "Yealink T46S";
        public const string YealinkT48S = "Yealink T48S";
        public const string FanvilH5 = "Fanvil H5";




        public static List<Phone> phoneModels = new List<Phone>()
        {
            new Phone{ Model = TvsT46s, ModelDisplayName = "TVS - Yealink T46s"},
            new Phone{ Model = TvsT48s, ModelDisplayName = "TVS - Yealink T48s"},
            new Phone{ Model = YealinkCp960},
            new Phone{ Model = YealinkT40G},
            new Phone{ Model = YealinkT42S},
            new Phone{ Model = YealinkT46S},
            new Phone{ Model = YealinkT48S},
            new Phone{ Model = YealinkT57W},
            new Phone{ Model = FanvilH5}

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
        public bool isFirstLaunch = Settings.Default.firstLaunch;
        public bool showTtg => Settings.Default.showTtgClients;
        public static string savedUser => AdAuthClient.TryGetUser();
        public static string savedPass => AdAuthClient.TryGetPassword();
        public static string authU = string.Empty;
        public static string authP = string.Empty;
        public static string CwApiUser = string.Empty;
        public static string CwApiKey = string.Empty;
        public static bool isAdmin = false;
        private static ConnectWiseConnection CwClient;
        private static System.Timers.Timer timer = new System.Timers.Timer(300000);


        public MainWindow()
        {
            InitializeComponent();
            ShowMenu();


        }


        private void ShowMenu()
        {
            MainMenu.Visibility = Visibility.Visible;
            ShowqTtgCheckbox.IsChecked = showTtg;

        }

        private async void Window_Initialized(object sender, EventArgs e)
        {


            // Copy user settings from previous application version if necessary
            if (Settings.Default.UpdateSettings)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                Settings.Default.Save();
            }

            if (isFirstLaunch)
            {
                isDark = ReadRegistry.isDarkEnabled;
                Settings.Default.firstLaunch = false;
                Settings.Default.Save();
            }

            ThemeControl.SetTheme(this);

            if (!isAuthenticated)
            {

                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.LeftAlt))
                {
                    if (Settings.Default.RememberMe)
                    {

                        if (LoginWindow.TryLogin(savedUser, savedPass))
                        {
                            MainWindow.isAuthenticated = true;

                        }
                    }
                }


                if (!isAuthenticated)
                {
                    var window = new LoginWindow();
                    window.ShowDialog();
                }
                if (!isAuthenticated)
                {
                    this.Close();
                    return;
                }


                if (isAdmin)
                {
                    AdminMenu.Visibility = Visibility.Visible;
                    extensionRemoveButton.IsEnabled = true;
                    resetPassordMenuItem.IsEnabled = true;
                    makeExtAdminMenuItem.IsEnabled = true;

                }

                authU = await Secrets.GetSecretValue("AdAuthUser");
                authP = await Secrets.GetSecretValue("AdAuthPass");
            }
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if (CwApiUser == "")
            {
                CwApiUser = await Secrets.GetSecretValue("CWApiUser");
            }
            if (CwApiKey == "")
            {
                CwApiKey = await Secrets.GetSecretValue("CWApiKey");

            }

            CwClient = new ConnectWiseConnection(CwApiUser, CwApiKey);
            await updateCustomerList();
        }
        private void OnContentRendered(object sender, EventArgs e)
        {

        }


        private async Task updateCustomerList()
        {

            var allVoipClients = await CwClient.GetAllTvsVoIpClients();

            if (showTtg)
            {
                allVoipClients.AddRange(await CwClient.GetAllThinkVoIpClients());
            }
            CustomersList.ItemsSource = allVoipClients.OrderBy(a => a.company.name);
            CustomersList.Visibility = Visibility.Visible;



        }

        private async void CustomersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var listBoxSender = sender as ListBox;
            if (listBoxSender.SelectedItems.Count == 0) return;

            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
                CompanyId = selectedCompany.company.id;

            }

            //if (Mouse.LeftButton == MouseButtonState.Pressed)
            //{
            //    await UpdateSelectedCompanyInfo();
            //}

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
                resetPassordMenuItem.Visibility = Visibility.Visible;
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
            RefreshButton.Visibility = Visibility.Hidden;
            RefreshSeperator.Visibility = Visibility.Hidden;
            Open3cxButton.Visibility = Visibility.Hidden;
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


            //ListViewGrid.Visibility = Visibility.Hidden;

            //VoimailOnlyExtensionsDisplay.Visibility = Visibility.Hidden;
            //ForwardingOnlyExtensionsDisplay.Visibility = Visibility.Hidden;
            //BilledUserExtensionsDisplay.Visibility = Visibility.Hidden;
            //ExtensionsTotalDisplay.Visibility = Visibility.Hidden;
            //ExtensionsTotalInvalid.Visibility = Visibility.Hidden;
            //ExtensionsTotalValid.Visibility = Visibility.Hidden;
            //PhonesTotalDisplay.Visibility = Visibility.Hidden;
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
            var user = await Secrets.GetSecretValue("AdAuthUser");
            var pass = await Secrets.GetSecretValue("AdAuthPass");


            lastView = Views.valid;
            ExtensionsTotalDisplay.Visibility = Visibility.Hidden;
            var company = await CwClient.GetCompany(companyId);
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
            RefreshButton.Visibility = Visibility.Visible;
            RefreshSeperator.Visibility = Visibility.Visible;
            Open3cxButton.Visibility = Visibility.Visible;
            OpenConfluenceButton.Visibility = Visibility.Visible;

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

        private void Menu_Item_ResetPassword(object sender, RoutedEventArgs e)
        {
            if (CustomersList.SelectedItem != null)
            {
                var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
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
            ThemeControl.SetTheme(this);


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

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {
            return;
        }


        public static string GetUserSid()
        {
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            SecurityIdentifier sid = user.User;
            return sid.ToString();
        }

        private async void openConfluenceButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            CompanyId = selectedCompany.company.id;
            var company = await CwClient.GetCompany(CompanyId);
            var url = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty), true);

            OpenUrl($"https://docs.think-team.com" + url);
        }

        private async void open3cxPage_Click(object sender, RoutedEventArgs e)
        {
            var company = await CwClient.GetCompany(CompanyId);
            var pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty));
            var loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId);


            var hostName = loginInfo.HostName;
            var cleanedHostName = Regex.Replace(hostName, @"/api/", string.Empty);

            OpenUrl(cleanedHostName);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateDisplay();
        }

        private async void makeExtAdminMenuItem_Click(object sender, RoutedEventArgs e)
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



            var result = await ThreeCxClient.MakeExtensionAdmin(CurrentExtension);
            if (result == "Failed")
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
    }
}

//  {"Path":{"ObjectId":"72","PropertyPath":[{"Name":"AccessEnabled"}]},"PropertyValue":true}
//  {"Path":{"ObjectId":"72","PropertyPath":[{"Name":"AccessEnabled"}]},"PropertyValue":true}
// {"Path":{"ObjectId":"6","PropertyPath":[{"Name":"AccessRole"}]},"PropertyValue":"AccessRole.GlobalExtensionManager"}

// {"Path":{"ObjectId":"6","PropertyPath":[{"Name":"AccessAdmin"}]},"PropertyValue":true}

// {"Path":{"ObjectId":"6","PropertyPath":[{"Name":"AccessReporter"}]},"PropertyValue":true}

// {"Path":{"ObjectId":"6","PropertyPath":[{"Name":"AccessReporterRecording"}]},"PropertyValue":true}
