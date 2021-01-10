using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ThinkVoip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum PhoneModels
        {
            [Description("Yealink Cp 960")]
            YealinkCp960,
            [Description("Yealink T40g")]
            YealinkT40G,
            [Description("Yealink T46s")]
            YealinkT46S,
            [Description("Yealink T48s")]
            YealinkT48S,
            [Description("Yealink T57W")]
            YealinkT57W,
            [Description("Fanvil H5")]
            FanvilH5,
            [Description("Yealink T48s - TVS custom")]
            YealinkT46STVS
        }

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
            new Phone{ Model = TvsT46s}

        };


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
        private bool debug = false;
        public Views lastView;


        public MainWindow()
        {
            InitializeComponent();
            ShowMenu();
            IsDebug();

        }

        [Conditional("DEBUG")]
        private void ShowMenu()
        {

            MainMenu.Visibility = Visibility.Visible;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var allVoipClients = await ConnectWiseConnection.CwClient.GetAllTvsVoIpClients();
            if (debug)
            {
                allVoipClients.AddRange(await ConnectWiseConnection.CwClient.GetAllThinkVoIpClients());
            }
            CustomersList.ItemsSource = allVoipClients.OrderBy(a => a.company.name);
        }

        [Conditional("DEBUG")]
        private void IsDebug()
        {
            debug = true;
        }
        private async void CustomersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateSelectedCompanyInfo();
        }

        public async Task UpdateSelectedCompanyInfo()
        {
            PleaseWaitTextBlock.SetValue(TextBlock.TextProperty, "Please wait...");
            PleaseWaitTextBlock.Visibility = Visibility.Visible;
            ThreeCxClient = null;
            var selectedCompany = (Models.CompanyModel.Agreement)CustomersList.SelectedItems[0];
            CompanyId = selectedCompany.company.id;
            CleanExtensionDataGrid();
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
            ExtensionData.Visibility = Visibility.Hidden;

            AddExt.Visibility = Visibility.Hidden;
            AddPhoneButton.Visibility = Visibility.Hidden;
            ExtensionsHeader.Visibility = Visibility.Hidden;
            ExtSeperator.Visibility = Visibility.Hidden;
            PhoneSeperator.Visibility = Visibility.Hidden;
            ExtensionsTotalDisplay.Content = "";
            VoimailOnlyExtensionsDisplay.Content = "";
            ExtensionsTotalInvalid.Content = "";
            ExtensionsTotalValid.Content = "";
            PhonesTotalDisplay.Content = "";
            ExtensionsTotal.Text = "";
            InValidExtensions.Text = "";
            TotalValidExtensions.Text = "";
            PhonesTotal.Text = "";
            VoicemailOnlyExtensionsCount.Text = "";
            ExtensionsTotalDisplay.Visibility = Visibility.Visible;
            ExtensionsTotalInvalid.Visibility = Visibility.Visible;
            ExtensionsTotalValid.Visibility = Visibility.Visible;
            PhonesTotalDisplay.Visibility = Visibility.Visible;
            VoimailOnlyExtensionsDisplay.Visibility = Visibility.Visible;

        }

        public void UpdateExtensionDataGrid()
        {


            ExtensionData.Visibility = Visibility.Hidden;
            VoimailOnlyExtensionsDisplay.Content = "";
            ExtensionsTotalDisplay.Content = "";
            ExtensionsTotalInvalid.Content = "";
            ExtensionsTotalValid.Content = "";
            PhonesTotalDisplay.Content = "";
            ExtensionsTotal.Text = "";
            InValidExtensions.Text = "";
            TotalValidExtensions.Text = "";
            PhonesTotal.Text = "";
            VoicemailOnlyExtensionsCount.Text = "";
        }

        public async Task DisplayClientInfo(int companyId)
        {


            lastView = Views.valid;
            ExtensionsTotalDisplay.Content = "";
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

        }

        private List<Extension> GetVoicemailOnlyExtensions(List<Extension> extensions)
        {
            return extensions.Where(a => a.FirstName.ToLower().Contains("voicemail only")).ToList();
        }

        private void UpdateExtensionDisplayGridNames()
        {
            ExtensionsTotalDisplay.Content = "Total:";
            ExtensionsTotalInvalid.Content = "UnBilled:";
            ExtensionsTotalValid.Content = "Valid:";
            PhonesTotalDisplay.Content = "Phones:";
            VoimailOnlyExtensionsDisplay.Content = "Voicemail Only:";
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

            ExtensionData.Visibility = Visibility.Visible;
            ExtensionData.ItemsSource = ExtensionList;
            ExtensionData.BorderBrush = Brushes.Black;
            ExtensionData.Columns[0].Visibility = Visibility.Collapsed;
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
                ExtensionData.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                ExtensionData.Visibility = Visibility.Visible;
                ExtensionData.ItemsSource = cleanedExtensions;
                ExtensionData.BorderBrush = Brushes.Black;
                ExtensionData.Columns[0].Visibility = Visibility.Collapsed;
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


            ExtensionData.ItemsSource = cleanedExtensions;
            ExtensionData.Visibility = Visibility.Visible;
            ExtensionData.BorderBrush = Brushes.Black;
            ExtensionData.Columns[0].Visibility = Visibility.Collapsed;
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



            ExtensionData.ItemsSource = cleanedPhones;
            ExtensionData.Visibility = Visibility.Visible;

            foreach (var extensionColumn in ExtensionData.Columns.Where(column =>
                column.DisplayIndex != 6 &&
                column.DisplayIndex != 7 &&
                column.DisplayIndex != 8 &&
                column.DisplayIndex != 9 &&
                column.DisplayIndex != 30 &&
                column.DisplayIndex != 31 &&
                column.DisplayIndex != 16 &&
                column.DisplayIndex != 13))
            {
                extensionColumn.Visibility = Visibility.Hidden;
            }
        }


        private void MenuItem_ExtDetails_Click(object sender, RoutedEventArgs e)
        {
            if (ExtensionData.SelectedItem == null)
            {
                return;
            }

            switch (ExtensionData.SelectedItem.GetType().Name)
            {
                case "Extension":
                    {
                        var extension = ExtensionData.SelectedItem as Extension;
                        Debug.Assert(extension != null, nameof(extension) + " != null");
                        MessageBox.Show($"{extension.Number} {extension.FirstName}");
                        break;
                    }
                case "Phone":
                    {
                        var phone = ExtensionData.SelectedItem as Phone;
                        Debug.Assert(phone != null, nameof(phone) + " != null");
                        MessageBox.Show($"{phone.Vendor} {phone.Model}");
                        break;
                    }
            }
        }

        private async void MenuItem_Add_New_Phone_Click(object sender, RoutedEventArgs e)
        {

            if (ExtensionData.SelectedItem == null || ExtensionData.SelectedItem.ToString() == "{NewItemPlaceholder}")
            {
                MessageBox.Show("Please select an extension first.", "Error :(");
                return;
            }


            switch (ExtensionData.SelectedItem.GetType().Name)
            {
                case "Extension":
                    {
                        var selectedItem = ExtensionData.SelectedItem as Extension;
                        CurrentExtension = selectedItem?.Number;
                        CurrentExtensionClass = selectedItem;
                        //var phoneWindow = new AddPhoneToExtWindow(CurrentExtension) { PhonesDropDownList = { ItemsSource = Enum.GetValues(typeof(PhoneModels))} };
                        var phoneWindow = new AddPhoneToExtWindow(CurrentExtension) { PhonesDropDownList = { ItemsSource = phoneModels, DisplayMemberPath = "Model"} };
                        phoneWindow.ShowDialog();
                        await UpdateView();
                        await DisplayExtensionInfo(CompanyId);

                        break;
                    }
                case "Phone":
                    {
                        var phone = ExtensionData.SelectedItem as Phone;
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

        private async void RemoveMEnuITem_click(object sender, RoutedEventArgs e)
        {
            if (ExtensionData.SelectedItem == null)
            {
                return;
            }

            switch (ExtensionData.SelectedItem.GetType().Name)
            {
                case "Extension":
                    {
                        var extensions = ExtensionData.SelectedItems.Cast<Extension>().ToList();
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
                            
                        }

                        break;
                    }
                case "Phone":
                    {
                        var phone = ExtensionData.SelectedItem as Phone;
                        MessageBox.Show($"Add new phone eventually.....");
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
            window.Show();

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
                    await DisplayValidExtensions(CompanyId);
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
                default:
                    break;
            }
            
        }

        private void FileMenu_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void OnTestButtonClick(object sender, RoutedEventArgs e)
        {

            return;
        }

        private void MenuItem_Click_Standardize_Stun(object sender, RoutedEventArgs e)
        {

            var extensionsToEditList = ExtensionData.SelectedItems;

            if (ExtensionData.SelectedItem == null || ExtensionData.SelectedItem.ToString() == "{NewItemPlaceholder}")
            {
                MessageBox.Show("Please select an extension first.", "Error :(");
                return;
            }

            ToBeUpdated = extensionsToEditList;

            var selectedItem = ExtensionData.SelectedItem as Extension;
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



            ExtensionData.ItemsSource = vmOnlyList;
            ExtensionData.Columns[0].Visibility = Visibility.Hidden;
            ExtensionData.Visibility = Visibility.Visible;
        }
    }
    
}