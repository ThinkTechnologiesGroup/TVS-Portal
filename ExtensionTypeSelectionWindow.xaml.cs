using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#pragma warning disable 8620

// ReSharper disable InconsistentNaming

namespace ThinkVoipTool
{
    /// <summary>
    ///     Interaction logic for ExtensionTypeSelectionWindow.xaml
    /// </summary>
    public partial class ExtensionTypeSelectionWindow
    {
        private readonly CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        private readonly MainWindow mainWindow;
        private readonly ThreeCxClient threeCxClient;
        private readonly bool update;
        private HashSet<string> availableExtensionNumbers = new HashSet<string>();
        private AvailableExtensionNumbers? availableExtensionsObj;
        private string? currentExtension;
        private string? emailAddress;
        private string? extNumber;
        private string? firstName;
        private string? lastName;
        private string? mobileNumber;

        public ExtensionTypeSelectionWindow(ThreeCxClient? threeCxClient, MainWindow mainWindow, bool update = false)
        {
            this.update = update;
            this.mainWindow = mainWindow;
            this.threeCxClient = threeCxClient!;
            currentExtension = MainWindow.CurrentExtension!;

            if(update)
            {
                firstName = MainWindow.CurrentExtensionClass?.FirstName!;
                firstName = cultureInfo.TextInfo.ToTitleCase(firstName?.ToLower()!);
                lastName = MainWindow.CurrentExtensionClass?.LastName!;
                lastName = cultureInfo.TextInfo.ToTitleCase(lastName?.ToLower()!);
                emailAddress = MainWindow.CurrentExtensionClass?.Email!;
                mobileNumber = MainWindow.CurrentExtensionClass?.MobileNumber!;
            }

            InitializeComponent();

            ExtensionTypeDropDownList.ItemsSource = Enum.GetValues(typeof(ExtensionTypes));
            ExtensionTypeDropDownList.SelectedIndex = 0;
        }

        private string voiceMailOptions =>
            emailAddress != "" ? "EmailNotificationType.SendVMailAsAttachmentAndDelete" : "EmailNotificationType.DoNotSend";

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async void SetExtensionType_Click(object sender, RoutedEventArgs e)
        {
            var extType = ExtensionTypeDropDownList.SelectedItem;
            ExtensionDropDownTitle.Text = "";


            if(update)
            {
                PleaseWaitTextBlock.Visibility = Visibility.Visible;
                using (new OverrideCursor(Cursors.Wait))
                {
                    if(MainWindow.ToBeUpdated != null)
                    {
                        foreach (Extension? ext in MainWindow.ToBeUpdated)
                        {
                            currentExtension = ext?.Number;
                            emailAddress = ext?.Email;
                            firstName = ext?.FirstName;
                            firstName = cultureInfo.TextInfo.ToTitleCase(firstName?.ToLower()!);
                            lastName = ext?.LastName;
                            lastName = cultureInfo.TextInfo.ToTitleCase(lastName?.ToLower()!);
                            mobileNumber = ext?.MobileNumber;

                            var extensionId = await threeCxClient.GetExtensionId(currentExtension);
                            var phonesList = await threeCxClient.GetListOfPhonesForExtension(currentExtension, extensionId);
                            var vmPin = await threeCxClient.GetExtensionPinNumber(currentExtension);

                            if(phonesList.Count > 1)
                            {
                                //pop up new window to pick from or choose all and return new list to loop around of whats to update

                                //var phoneToUpdate = DoACoolThingHere();
                            }

                            switch (extType)
                            {
                                case ExtensionTypes.StandardUser:

                                    firstName = firstName
                                        .Replace("- Voicemail Only", "")
                                        .Replace("Voicemail Only", "")
                                        .Replace("- Forward Only", "");

                                    await threeCxClient.CreateExtensionOnServer(currentExtension, firstName, lastName, emailAddress, voiceMailOptions,
                                        disAllowUseOffLan: false, pin: vmPin);

                                    foreach (var phone in phonesList)
                                    {
                                        await threeCxClient.UpdatePhoneSettingsOnExtension(extensionId, phone.MacAddress, currentExtension);
                                    }

                                    break;

                                case ExtensionTypes.VoiceMailOnly:

                                    firstName = firstName
                                        .Replace("- Voicemail Only", "")
                                        .Replace("Voicemail Only", "")
                                        .Replace("- Forward Only", "");

                                    firstName = firstName.Trim() + " - Voicemail Only";

                                    await threeCxClient.CreateExtensionOnServer(currentExtension, firstName, lastName, emailAddress, voiceMailOptions,
                                        disAllowUseOffLan: true, vmOnly: true, pin: vmPin);

                                    break;

                                case ExtensionTypes.ForwardingOnly:


                                    firstName = firstName
                                        .Replace("- Voicemail Only", "")
                                        .Replace("Voicemail Only", "")
                                        .Replace("- Forward Only", "");

                                    firstName = firstName.Trim() + " - Forward Only";

                                    await threeCxClient.CreateExtensionOnServer(currentExtension, firstName, lastName, emailAddress, voiceMailOptions,
                                        mobileNumber, disAllowUseOffLan: true);

                                    break;
                            }
                        }
                    }

                    await mainWindow.UpdateDisplay();
                }

                Close();
                return;
            }

            SetExtensionBack.Visibility = Visibility.Visible;
            ExtensionTypeDropDownList.Visibility = Visibility.Hidden;
            SetExtensionType.Visibility = Visibility.Hidden;
            AvailableExtensionsDropDownList.Visibility = Visibility.Visible;

            switch (extType)
            {
                case ExtensionTypes.StandardUser:
                    //this.FirstName.Text = "First Name";
                    //this.Email.SetValue(Grid.ColumnProperty, 4);
                    EmailEntry.SetValue(Grid.ColumnProperty, 4);
                    // this.ExtNumber.Visibility = Visibility.Visible;
                    //this.ExtNumberEntry.Visibility = Visibility.Visible;
                    ExtNumberEntry.Visibility = Visibility.Hidden;
                    //this.FirstName.Visibility = Visibility.Visible;
                    FirstNameEntry.Visibility = Visibility.Visible;
                    //this.LastName.Visibility = Visibility.Visible;
                    LastNameEntry.Visibility = Visibility.Visible;
                    //this.Email.Visibility = Visibility.Visible;
                    EmailEntry.Visibility = Visibility.Visible;
                    AddExtensionButton.Visibility = Visibility.Visible;
                    break;

                case ExtensionTypes.VoiceMailOnly:
                    // this.ExtNumber.Visibility = Visibility.Visible;
                    //this.ExtNumberEntry.Visibility = Visibility.Visible;
                    ExtNumberEntry.Visibility = Visibility.Hidden;
                    // this.FirstName.Text = "Display Name";
                    // this.FirstName.Visibility = Visibility.Visible;
                    FirstNameEntry.Visibility = Visibility.Visible;
                    // this.Email.SetValue(Grid.ColumnProperty, 3);
                    EmailEntry.SetValue(Grid.ColumnProperty, 3);
                    // this.Email.Visibility = Visibility.Visible;
                    EmailEntry.Visibility = Visibility.Visible;
                    AddExtensionButton.Visibility = Visibility.Visible;
                    break;

                case ExtensionTypes.ForwardingOnly:
                    // this.ExtNumber.Visibility = Visibility.Visible;
                    //this.ExtNumberEntry.Visibility = Visibility.Visible;
                    ExtNumberEntry.Visibility = Visibility.Hidden;
                    // this.FirstName.Text = "Display Name";
                    // this.FirstName.Visibility = Visibility.Visible;
                    FirstNameEntry.Visibility = Visibility.Visible;
                    // this.MobileNumber.Visibility = Visibility.Visible;
                    MobileNumberEntry.Visibility = Visibility.Visible;
                    AddExtensionButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private async void AddExtenstion_Click(object sender, RoutedEventArgs e)
        {
            using (new OverrideCursor(Cursors.Wait))
            {
                await AddExtension();
            }
        }

        private async Task AddExtension()
        {
            PleaseWaitTextBlock.Visibility = Visibility.Visible;
            var extType = ExtensionTypeDropDownList.SelectedItem;

            firstName = FirstNameEntry.Text;
            firstName = cultureInfo.TextInfo.ToTitleCase(firstName.ToLower());

            lastName = LastNameEntry.Text;
            lastName = cultureInfo.TextInfo.ToTitleCase(lastName.ToLower());

            extNumber = AvailableExtensionsDropDownList.Text;

            emailAddress = IsValidEmail(EmailEntry.Text) ? EmailEntry.Text : "";

            mobileNumber = MobileNumberEntry.Text
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("+", "");

            switch (extType)
            {
                case ExtensionTypes.StandardUser:
                    await threeCxClient.CreateExtensionOnServer(extNumber, firstName, lastName, emailAddress, voiceMailOptions,
                        disAllowUseOffLan: false);

                    break;

                case ExtensionTypes.VoiceMailOnly:
                    firstName = firstName.Trim() + " - Voicemail Only";
                    await threeCxClient.CreateExtensionOnServer(extNumber, firstName, lastName, emailAddress, voiceMailOptions,
                        disAllowUseOffLan: true, vmOnly: true);

                    break;

                case ExtensionTypes.ForwardingOnly:
                    firstName = firstName.Trim() + " - Forward Only";
                    await threeCxClient.CreateExtensionOnServer(extNumber, firstName, lastName, emailAddress, voiceMailOptions,
                        mobileNumber, disAllowUseOffLan: true);

                    break;
            }

            await mainWindow.UpdateDisplay();
            Close();
        }


        private async void FirstName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                await AddExtension();
            }
        }

        private async void LastName_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                using (new OverrideCursor(Cursors.Wait))
                {
                    await AddExtension();
                }
            }
        }

        private async void Email_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                using (new OverrideCursor(Cursors.Wait))
                {
                    await AddExtension();
                }
            }
        }

        private void SetExtensionBack_Click(object sender, RoutedEventArgs e)
        {
            MobileNumberEntry.Visibility = Visibility.Hidden;
            AddExtensionButton.Visibility = Visibility.Hidden;
            SetExtensionBack.Visibility = Visibility.Hidden;
            ExtNumberEntry.Visibility = Visibility.Hidden;
            AvailableExtensionsDropDownList.Visibility = Visibility.Hidden;
            FirstNameEntry.Visibility = Visibility.Hidden;
            LastNameEntry.Visibility = Visibility.Hidden;
            EmailEntry.Visibility = Visibility.Hidden;
            AddExtensionButton.Visibility = Visibility.Hidden;
            ExtensionDropDownTitle.Text = "Choose Extension Type";
            ExtensionTypeDropDownList.Visibility = Visibility.Visible;
            SetExtensionType.Visibility = Visibility.Visible;
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (new OverrideCursor(Cursors.Wait))
            {
                availableExtensionsObj =
                    new AvailableExtensionNumbers(await threeCxClient.GetExtensionsList(), await threeCxClient.GetSystemExtensions());
                availableExtensionNumbers = availableExtensionsObj.PossibleExtensions;
                AvailableExtensionsDropDownList.ItemsSource = availableExtensionNumbers;
            }
        }

        private enum ExtensionTypes
        {
            StandardUser,
            VoiceMailOnly,
            ForwardingOnly
        }
    }

    public class AvailableExtensionNumbers
    {
        private readonly int extensionDigitCount;
        private readonly HashSet<string> extensionsToFiler = new HashSet<string>();
        public readonly HashSet<string> PossibleExtensions = new HashSet<string>();

        public AvailableExtensionNumbers(IEnumerable<Extension> extensions, IEnumerable<Extension>? systemExtensions)
        {
            var userExtensions = extensions.Select(a => a.Number).ToList();
            var systemExtensionsList = systemExtensions!.Select(a => a.Number).ToList();
            var first = userExtensions.FirstOrDefault();

            if(first != null)
            {
                extensionDigitCount = first.Length;
            }

            extensionsToFiler.UnionWith(userExtensions);
            extensionsToFiler.UnionWith(systemExtensionsList);

            var startNumber = new StringBuilder();
            var endNumber = new StringBuilder();

            for (var i = 0; i < extensionDigitCount; i++)
            {
                startNumber.Append("0");
                endNumber.Append("9");
            }

            var startingExtensionNumber = startNumber.ToString();
            var finalExtensionNumber = endNumber.ToString();

            var usedExtensions = new HashSet<string>();
            usedExtensions.UnionWith(userExtensions);
            usedExtensions.UnionWith(systemExtensionsList);

            var range = Enumerable.Range(int.Parse(startingExtensionNumber), int.Parse(finalExtensionNumber));
            foreach (var i in range)
            {
                PossibleExtensions.Add(i.ToString());
            }

            PossibleExtensions.RemoveWhere(a => a.Length < extensionDigitCount);
            PossibleExtensions.RemoveWhere(a => usedExtensions.Contains(a));
        }
    }
}