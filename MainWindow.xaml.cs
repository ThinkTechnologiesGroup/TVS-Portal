using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            YealinkCp960,
            YealinkT40G,
            YealinkT46S,
            YealinkT48S,
            YealinkT57W,
            FanvilH5,
            YealinkT46STVS
        }

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
            GetThinkVolipClients();

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
        private void GetThinkVolipClients()
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
            Debug.Assert(selectedCompany != null, nameof(selectedCompany) + " != null");
            var companyId = selectedCompany.company.id;
            CleanExtensionDataGrid();
            CompanyId = companyId;
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
            DataEntryGrid.Visibility = Visibility.Hidden;
            SaveExtensions.Visibility = Visibility.Hidden;
            AddExt.Visibility = Visibility.Hidden;
            AddPhoneButton.Visibility = Visibility.Hidden;
            ExtensionsHeader.Visibility = Visibility.Hidden;
            ExtensionsTotalDisplay.Content = "";
            ExtensionsTotalInvalid.Content = "";
            ExtensionsTotalValid.Content = "";
            PhonesTotalDisplay.Content = "";
            ExtensionsTotal.Text = "";
            InValidExtensions.Text = "";
            TotalValidExtensions.Text = "";
            PhonesTotal.Text = "";
            ExtensionsTotalDisplay.Visibility = Visibility.Visible;
            ExtensionsTotalInvalid.Visibility = Visibility.Visible;
            ExtensionsTotalValid.Visibility = Visibility.Visible;
            PhonesTotalDisplay.Visibility = Visibility.Visible;
            //AddExt.Visibility = Visibility.Visible;
        }

        public void UpdateExtensionDataGrid()
        {
            if (SaveExtensions.Visibility == Visibility.Visible)
            {
                SaveExtensions.Visibility = Visibility.Hidden;
            }

            ExtensionData.Visibility = Visibility.Hidden;
            ExtensionsTotalDisplay.Content = "";
            ExtensionsTotalInvalid.Content = "";
            ExtensionsTotalValid.Content = "";
            PhonesTotalDisplay.Content = "";
            ExtensionsTotal.Text = "";
            InValidExtensions.Text = "";
            TotalValidExtensions.Text = "";
            PhonesTotal.Text = "";
        }

        public async Task DisplayClientInfo(int companyId)
        {
            switch (companyId)
            {
                case 19532:
                    ExtensionsTotalDisplay.Content = "MainStreets phone system has \nbeen shutdown.";
                    this.PleaseWaitTextBlock.Visibility = Visibility.Hidden;
                    return;

            }

            if (SaveExtensions.Visibility == Visibility.Visible)
            {
                SaveExtensions.Visibility = Visibility.Hidden;
            }

            ExtensionsTotalDisplay.Content = "";
            var company = await ConnectWiseConnection.CwClient.GetCompany(companyId);
            var pageId = Docs.ConfClient.FindThreeCxPageIdByTitle(company.name.Replace(", PA", string.Empty));
            var loginInfo = Docs.ConfClient.GetThreeCxLoginInfo(pageId);
            this.loginInfo = loginInfo;
            ThreeCxPassword = loginInfo.Password;
            ThreeCxClient = new ThreeCxClient(loginInfo.HostName, loginInfo.Username, loginInfo.Password);
            systemExtensions = await ThreeCxClient.GetSystemExtensions();
            var extensions = await ThreeCxClient.GetExtensionsList();
            var extCount = extensions.Count();
            var invalidExtensions = extensions.Count(ext =>
                ext.FirstName.ToLower().Contains("test") ||
                ext.LastName.ToLower().Contains("test") ||
                ext.FirstName.ToLower().Contains("copy me") ||
                ext.FirstName.ToLower().Equals("operator") ||
                ext.FirstName.ToLower().Contains("template") ||
                ext.FirstName.ToLower().Contains("voicemail only") ||
                ext.FirstName.ToLower().Contains("forward only") ||
                ext.LastName.ToLower().Contains("template"));
            var phones = await ThreeCxClient.GetPhonesList();
            ExtensionsTotalDisplay.Content = "Total:";
            ExtensionsTotalInvalid.Content = "UnBilled:";
            ExtensionsTotalValid.Content = "Valid:";
            PhonesTotalDisplay.Content = "Phones:";
            ExtensionsTotal.Text = extCount.ToString();

            InValidExtensions.Text = invalidExtensions.ToString();
            TotalValidExtensions.Text = (extCount - invalidExtensions).ToString();
            PhonesTotal.Text = phones.Where(phone => !phone.Model.ToLower().Contains("windows"))
                .Count(phone => !phone.Model.ToLower().Contains("web client")).ToString();
            PleaseWaitTextBlock.Visibility = Visibility.Hidden;
            await DisplayValidExtensions(companyId);
            AddExt.Visibility = Visibility.Visible;
            AddPhoneButton.Visibility = Visibility.Visible;
            ExtensionsHeader.Visibility = Visibility.Visible;

        }

        public async Task UpdateView()
        {
            if (CompanyId == 19532)
            {
                ExtensionsTotalDisplay.Content = "No valid Info";
                return;
            }

            if (SaveExtensions.Visibility == Visibility.Visible)
            {
                SaveExtensions.Visibility = Visibility.Hidden;
            }

            ExtensionsTotalDisplay.Content = "";
            var extensions = await ThreeCxClient.GetExtensionsList();
            var extCount = extensions.Count();
            var invalidExtensions = extensions.Count(ext =>
                ext.FirstName.ToLower().Contains("test") ||
                ext.LastName.ToLower().Contains("test") ||
                ext.FirstName.ToLower().Contains("copy me") ||
                ext.FirstName.ToLower().Equals("operator") ||
                ext.FirstName.ToLower().Contains("template") ||
                ext.FirstName.ToLower().Contains("voicemail only") ||
                ext.FirstName.ToLower().Contains("forward only") ||
                ext.LastName.ToLower().Contains("template"));
            var phones = await ThreeCxClient.GetPhonesList();
            ExtensionsTotalDisplay.Content = "Total:";
            ExtensionsTotalInvalid.Content = "UnBilled:";
            ExtensionsTotalValid.Content = "Valid:";
            PhonesTotalDisplay.Content = "Phones:";

            ExtensionsTotal.Text = extCount.ToString();
            InValidExtensions.Text = invalidExtensions.ToString();
            TotalValidExtensions.Text = (extCount - invalidExtensions).ToString();

            PhonesTotal.Text = phones.Where(phone => !phone.Model.ToLower().Contains("windows"))
                .Count(phone => !phone.Model.ToLower().Contains("web client")).ToString();
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
            var extensions = await ThreeCxClient.GetExtensionsList();
            if (DataEntryGrid.Visibility == Visibility.Visible)
            {
                DataEntryGrid.Visibility = Visibility.Hidden;
            }

            if (SaveExtensions.Visibility == Visibility.Visible)
            {
                SaveExtensions.Visibility = Visibility.Hidden;
            }

            ExtensionData.Visibility = Visibility.Visible;
            ExtensionData.ItemsSource = extensions;
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
            if (DataEntryGrid.Visibility == Visibility.Visible)
            {
                DataEntryGrid.Visibility = Visibility.Hidden;
            }

            if (SaveExtensions.Visibility == Visibility.Visible)
            {
                SaveExtensions.Visibility = Visibility.Hidden;
            }

            var extensions = await ThreeCxClient.GetExtensionsList();
            var cleanedExtensions = extensions
                .Where(ext =>
                    ext.FirstName.ToLower().Contains("test") ||
                    ext.LastName.ToLower().Contains("test") ||
                    ext.FirstName.ToLower().Contains("copy me") ||
                    ext.FirstName.ToLower().Equals("operator") ||
                    ext.FirstName.ToLower().Contains("template") ||
                    ext.FirstName.ToLower().Contains("voicemail only") ||
                    ext.FirstName.ToLower().Contains("forward only") ||
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
            if (DataEntryGrid.Visibility == Visibility.Visible)
            {
                DataEntryGrid.Visibility = Visibility.Hidden;
            }

            if (SaveExtensions.Visibility == Visibility.Visible)
            {
                SaveExtensions.Visibility = Visibility.Hidden;
            }

            var extensions = await ThreeCxClient.GetExtensionsList();
            var cleanedExtensions = new List<Extension>();
            cleanedExtensions.AddRange(extensions
                .Where(ext => !ext.FirstName.ToLower().Contains("test"))
                .Where(ext => !ext.LastName.ToLower().Contains("test"))
                .Where(ext => !ext.FirstName.ToLower().Contains("copy me"))
                .Where(ext => !ext.FirstName.ToLower().Equals("operator"))
                .Where(ext => !ext.FirstName.ToLower().Contains("template"))
                .Where(ext => !ext.LastName.ToLower().Contains("template"))
                .Where(ext => !ext.FirstName.ToLower().Contains("voicemail only"))
                .Where(ext => !ext.LastName.ToLower().Contains("voicemail only"))
                .Where(ext => !ext.LastName.ToLower().Contains("forward only"))
                .Where(ext => !ext.FirstName.ToLower().Contains("forward only")));


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

            if (DataEntryGrid.Visibility == Visibility.Visible)
            {
                DataEntryGrid.Visibility = Visibility.Hidden;
            }

            if (SaveExtensions.Visibility == Visibility.Visible)
            {
                SaveExtensions.Visibility = Visibility.Hidden;
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
                        var phoneWindow = new AddPhoneToExtWindow(CurrentExtension) { PhonesDropDownList = { ItemsSource = Enum.GetValues(typeof(PhoneModels)) } };
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

        private async void SaveExtensionButton_click(object sender, RoutedEventArgs e)
        {
            foreach (var ext in ToBeAdded)
            {
                if (!Validate(ext))
                {
                    MessageBox.Show("Invalid data entered. All fields are required.");
                }
                else
                {
                    await ThreeCxClient.CreateExtensionOnServer(ext.Number, ext.FirstName, ext.LastName, ext.Email,
                        "EmailNotificationType.SendVMailAsAttachmentAndDelete", ext.MobileNumber, ext.MobileNumber, "1234");
                    ToBeAdded = new List<Extension>();
                    DataEntryGrid.Visibility = Visibility.Hidden;
                    SaveExtensions.Visibility = Visibility.Hidden;
                    ExtensionData.Visibility = Visibility.Visible;
                    UpdateExtensionDataGrid();
                    await UpdateView();
                    await DisplayValidExtensions(CompanyId);
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

                           // await UpdateView();
                            UpdateExtensionDataGrid();
                            await UpdateView();
                            await UpdateDisplay();
                            //await DisplayValidExtensions(CompanyId);
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
            await UpdateView();
            UpdateExtensionDataGrid();
            await UpdateView();
            switch (lastView)
            {
                case Views.none:
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
                default:
                    break;
            }
        }

        private void FileMenu_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
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
    }
}