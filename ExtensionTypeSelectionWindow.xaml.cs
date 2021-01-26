using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ThinkVoip
{
    /// <summary>
    /// Interaction logic for ExtensionTypeSelectionWindow.xaml
    /// </summary>
    /// 



    public partial class ExtensionTypeSelectionWindow : Window
    {

        public enum ExtensionTypes
        {
            StandardUser,
            VoiceMailOnly,
            ForwardingOnly

        }
        private ThreeCxClient threeCxClient;
        private string firstName;
        private string lastName;
        private string extNumber;
        private string emailAdddress;
        private MainWindow mainWindow;
        private int companyId;
        private string mobileNumber;
        private AvailableExtensionNumbers availableExtensionsObj;
        private HashSet<string> availableExtensionNumbers = new HashSet<string>();
        private bool update;
        private string currentExtension;
        private CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
        private IList extensionsToBeUpdated;

        private string voiceMailOptions => emailAdddress != "" ? "EmailNotificationType.SendVMailAsAttachmentAndDelete" : "EmailNotificationType.DoNotSend";

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public ExtensionTypeSelectionWindow(ThreeCxClient threeCxClient, MainWindow mainWindow, bool update = false)
        {
            this.update = update;
            this.mainWindow = mainWindow;
            this.threeCxClient = threeCxClient;
            this.companyId = MainWindow.CompanyId;
            this.currentExtension = MainWindow.CurrentExtension;

            if (update)
            {
                this.extensionsToBeUpdated = MainWindow.ToBeUpdated;
                this.firstName = MainWindow.CurrentExtensionClass.FirstName;
                firstName = cultureInfo.TextInfo.ToTitleCase(firstName.ToLower());
                this.lastName = MainWindow.CurrentExtensionClass.LastName;
                lastName = cultureInfo.TextInfo.ToTitleCase(lastName.ToLower());
                this.emailAdddress = MainWindow.CurrentExtensionClass.Email;
                this.mobileNumber = MainWindow.CurrentExtensionClass.MobileNumber;

            }

            InitializeComponent();

            this.ExtensionTypeDropDownList.ItemsSource = Enum.GetValues(typeof(ExtensionTypes));
            this.ExtensionTypeDropDownList.SelectedIndex = 0;
        }

        private async void SetExtensionType_Click(object sender, RoutedEventArgs e)
        {
            var extType = ExtensionTypeDropDownList.SelectedItem;
            this.ExtensionDropDownTitle.Text = "";



            if (update)
            {
                this.PleaseWaitTextBlock.Visibility = Visibility.Visible;

                foreach (Extension ext in MainWindow.ToBeUpdated)
                {
                    currentExtension = ext.Number;
                    emailAdddress = ext.Email;
                    firstName = ext.FirstName;
                    firstName = cultureInfo.TextInfo.ToTitleCase(firstName.ToLower());
                    lastName = ext.LastName;
                    lastName = cultureInfo.TextInfo.ToTitleCase(lastName.ToLower());
                    mobileNumber = ext.MobileNumber;

                    var extensionId = await threeCxClient.GetExtensionId(currentExtension);
                    var phonesList = await threeCxClient.GetListOfPhonesForExtension(currentExtension, extensionId);
                    var vmPin = await threeCxClient.GetExtensionPinNumber(currentExtension);
                    if (vmPin == null)
                    {
                        vmPin = "1234";
                    }
                    if (phonesList.Count > 1)
                    {
                        //pop up new window to pick from or choose all and return new list to loop around of whats to update

                        //var phoneToUpdate = DoACoolThingHere();

                    }
                    switch (extType)
                    {
                        case ExtensionTypes.StandardUser:

                            this.firstName = firstName
                                .Replace("- Voicemail Only", "")
                                .Replace("Voicemail Only", "")
                                .Replace("- Forward Only", "");

                            await this.threeCxClient.CreateExtensionOnServer(currentExtension, firstName, lastName, emailAdddress, voiceMailOptions,
                            disAllowUseOffLan: false, pin: vmPin);

                            foreach (var phone in phonesList)
                            {
                                await this.threeCxClient.UpdatePhoneSettingsOnExtension(extensionId, phone.MacAddress, currentExtension);
                            }

                            break;

                        case ExtensionTypes.VoiceMailOnly:

                            this.firstName = firstName
                                .Replace("- Voicemail Only", "")
                                .Replace("Voicemail Only", "")
                                .Replace("- Forward Only", "");

                            firstName = firstName.Trim() + " - Voicemail Only";

                            await this.threeCxClient.CreateExtensionOnServer(currentExtension, firstName, lastName, emailAdddress, voiceMailOptions,
                                disAllowUseOffLan: true, VmOnly: true, pin: vmPin);

                            break;

                        case ExtensionTypes.ForwardingOnly:

                            this.firstName = firstName
                                .Replace("- Voicemail Only", "")
                                .Replace("Voicemail Only", "")
                                .Replace("- Forward Only", "");

                            firstName = firstName.Trim() + " - Forward Only";

                            await this.threeCxClient.CreateExtensionOnServer(currentExtension, firstName, lastName, emailAdddress, voiceMailOptions,
                                mobileNumber: this.mobileNumber, disAllowUseOffLan: true);

                            break;
                    }

                }
                await mainWindow.UpdateDisplay();
                this.Close();
                return;
            }

            this.SetExtensionBack.Visibility = Visibility.Visible;
            this.ExtensionTypeDropDownList.Visibility = Visibility.Hidden;
            this.SetExtensionType.Visibility = Visibility.Hidden;
            this.AvailableExtensionsDropDownList.Visibility = Visibility.Visible;

            switch (extType)
            {
                case ExtensionTypes.StandardUser:
                    //this.FirstName.Text = "First Name";
                    //this.Email.SetValue(Grid.ColumnProperty, 4);
                    this.EmailEntry.SetValue(Grid.ColumnProperty, 4);
                   // this.ExtNumber.Visibility = Visibility.Visible;
                    //this.ExtNumberEntry.Visibility = Visibility.Visible;
                    this.ExtNumberEntry.Visibility = Visibility.Hidden;
                    //this.FirstName.Visibility = Visibility.Visible;
                    this.FirstNameEntry.Visibility = Visibility.Visible;
                    //this.LastName.Visibility = Visibility.Visible;
                    this.LastNameEntry.Visibility = Visibility.Visible;
                    //this.Email.Visibility = Visibility.Visible;
                    this.EmailEntry.Visibility = Visibility.Visible;
                    this.AddExtentionButton.Visibility = Visibility.Visible;
                    break;

                case ExtensionTypes.VoiceMailOnly:
                   // this.ExtNumber.Visibility = Visibility.Visible;
                    //this.ExtNumberEntry.Visibility = Visibility.Visible;
                    this.ExtNumberEntry.Visibility = Visibility.Hidden;
                   // this.FirstName.Text = "Display Name";
                   // this.FirstName.Visibility = Visibility.Visible;
                    this.FirstNameEntry.Visibility = Visibility.Visible;
                   // this.Email.SetValue(Grid.ColumnProperty, 3);
                    this.EmailEntry.SetValue(Grid.ColumnProperty, 3);
                   // this.Email.Visibility = Visibility.Visible;
                    this.EmailEntry.Visibility = Visibility.Visible;
                    this.AddExtentionButton.Visibility = Visibility.Visible;
                    break;

                case ExtensionTypes.ForwardingOnly:
                   // this.ExtNumber.Visibility = Visibility.Visible;
                    //this.ExtNumberEntry.Visibility = Visibility.Visible;
                    this.ExtNumberEntry.Visibility = Visibility.Hidden;
                   // this.FirstName.Text = "Display Name";
                   // this.FirstName.Visibility = Visibility.Visible;
                    this.FirstNameEntry.Visibility = Visibility.Visible;
                   // this.MobileNumber.Visibility = Visibility.Visible;
                    this.MobileNumberEntry.Visibility = Visibility.Visible;
                    this.AddExtentionButton.Visibility = Visibility.Visible;
                    break;

            }

        }

        private async void AddExtention_Click(object sender, RoutedEventArgs e)
        {
            await AddExtension();
        }

        private async Task AddExtension()
        {
            this.PleaseWaitTextBlock.Visibility = Visibility.Visible;
            var extType = ExtensionTypeDropDownList.SelectedItem;

            this.firstName = this.FirstNameEntry.Text;
            this.firstName = cultureInfo.TextInfo.ToTitleCase(firstName.ToLower());

            this.lastName = this.LastNameEntry.Text;
            this.lastName = cultureInfo.TextInfo.ToTitleCase(lastName.ToLower());

            this.extNumber = this.AvailableExtensionsDropDownList.Text;

            if (IsValidEmail(this.EmailEntry.Text))
            {
                this.emailAdddress = this.EmailEntry.Text;
            }
            else
            {
                this.emailAdddress = "";
            }
            this.mobileNumber = this.MobileNumberEntry.Text
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("+", "");

            switch (extType)
            {
                case ExtensionTypes.StandardUser:
                    await this.threeCxClient.CreateExtensionOnServer(extNumber, firstName, lastName, emailAdddress, voiceMailOptions,
                        disAllowUseOffLan: false);

                    break;

                case ExtensionTypes.VoiceMailOnly:
                    firstName = firstName.Trim() + " - Voicemail Only";
                    await this.threeCxClient.CreateExtensionOnServer(extNumber, firstName, lastName, emailAdddress, voiceMailOptions,
                        disAllowUseOffLan: true, VmOnly: true);

                    break;

                case ExtensionTypes.ForwardingOnly:
                    firstName = firstName.Trim() + " - Forward Only";
                    await this.threeCxClient.CreateExtensionOnServer(extNumber, firstName, lastName, emailAdddress, voiceMailOptions,
                        mobileNumber: this.mobileNumber, disAllowUseOffLan: true);

                    break;
            }

            await mainWindow.UpdateDisplay();
            this.Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {

        }


        private async void FirstName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AddExtension();
            }
        }

        private async void LastName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AddExtension();
            }
        }

        private async void Email_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AddExtension();
            }
        }

        private void SetExtensionBack_Click(object sender, RoutedEventArgs e)
        {
            //this.MobileNumber.Visibility = Visibility.Hidden;
            this.MobileNumberEntry.Visibility = Visibility.Hidden;
            this.AddExtentionButton.Visibility = Visibility.Hidden;
            this.SetExtensionBack.Visibility = Visibility.Hidden;
            //this.ExtNumber.Visibility = Visibility.Hidden;
            this.ExtNumberEntry.Visibility = Visibility.Hidden;
            this.AvailableExtensionsDropDownList.Visibility = Visibility.Hidden;
            //this.FirstName.Visibility = Visibility.Hidden;
            this.FirstNameEntry.Visibility = Visibility.Hidden;
            //this.LastName.Visibility = Visibility.Hidden;
            this.LastNameEntry.Visibility = Visibility.Hidden;
            //this.Email.Visibility = Visibility.Hidden;
            this.EmailEntry.Visibility = Visibility.Hidden;
            this.AddExtentionButton.Visibility = Visibility.Hidden;
            this.ExtensionDropDownTitle.Text = "Choose Extension Type";
            this.ExtensionTypeDropDownList.Visibility = Visibility.Visible;
            this.SetExtensionType.Visibility = Visibility.Visible;


        }



        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            availableExtensionsObj = new AvailableExtensionNumbers(await threeCxClient.GetExtensionsList(), await threeCxClient.GetSystemExtensions());
            availableExtensionNumbers = availableExtensionsObj.possibleExtensions;
            AvailableExtensionsDropDownList.ItemsSource = availableExtensionNumbers;
        }
    }

    public class AvailableExtensionNumbers
    {
        private List<string> userExtensions;
        public HashSet<string> usedExtensions;
        public HashSet<string> possibleExtensions = new HashSet<string>();
        private List<string> systemExtensions;
        private int extensionDigitCount;
        private string startingExtensionNumber;
        private string finalExtensionNumber;
        private HashSet<string> extensionsToFiler = new HashSet<string>();

        public AvailableExtensionNumbers(List<Extension> extensions, List<Extension> SystemExtensions)
        {

            this.userExtensions = extensions.Select(a => a.Number).ToList();
            this.systemExtensions = SystemExtensions.Select(a => a.Number).ToList();
            this.extensionDigitCount = userExtensions.FirstOrDefault().Length;
            this.extensionsToFiler.UnionWith(userExtensions);
            this.extensionsToFiler.UnionWith(systemExtensions);

            var startNumber = new StringBuilder();
            var endNumber = new StringBuilder();

            for (var i = 0; i < extensionDigitCount; i++)
            {
                startNumber.Append("0");
                endNumber.Append("9");
            }

            this.startingExtensionNumber = startNumber.ToString();
            this.finalExtensionNumber = endNumber.ToString();

            usedExtensions = new HashSet<string>();
            usedExtensions.UnionWith(userExtensions);
            usedExtensions.UnionWith(systemExtensions);

            //wtf man.
            var range = Enumerable.Range(int.Parse(startingExtensionNumber), int.Parse(finalExtensionNumber));
            foreach (var i in range)
            {
                possibleExtensions.Add(i.ToString());
            }

            possibleExtensions.RemoveWhere(a => a.Length < extensionDigitCount);
            possibleExtensions.RemoveWhere(a => usedExtensions.Contains(a));

        }

    }

}
