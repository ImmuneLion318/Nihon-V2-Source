using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Nihon.Authentication;
using Nihon.Static;

namespace Nihon.ContentView;

public partial class UserLogin : Window
{
    public UserLogin() => this.InitializeComponent();

    private async void OnLoad(object sender, RoutedEventArgs e)
    {
        this.CopyrightLabel.Content = $"Version - {Manager.VersionNumber}";

        //while (Manager.Core == null)
        //    await Task.Delay(250);

        //if (MessageBox.Show("Would You Like To Join Nihon's Official Discord Server, We Host Community Events Post Updates And Much More", "Nihon Discord", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
        //    await App.DiscordIntegration.JoinDiscord(Manager.Core.Invite);

        Assembly ClientAssembly = Assembly.GetExecutingAssembly();
        Process ClientProcess = Process.GetCurrentProcess();

        foreach (Process Proc in Process.GetProcessesByName(ClientProcess.ProcessName))
        {
            if (Proc.Id != ClientProcess.Id && ClientAssembly.Location.
                     Replace("/", "\\") == ClientProcess.MainModule.FileName)
            {
                MessageBox.Show($"It Appears Another Instance Of Nihon Is Running\nAbort The Process With Id \"{Proc.Id}\" To Start Another Instance Of Nihon."
                    , "Error: Instance Already Active", MessageBoxButton.OK, MessageBoxImage.Error);

                ClientProcess.Kill();
            }

            continue;
        }

        string File = Assembly.GetEntryAssembly().Location;

        if (new FileInfo(File).Attributes == FileAttributes.ReadOnly ||
            File.IndexOf(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase) == 0)
        {
            MessageBox.Show("It Appears You Are Running Nihon From A Temporary Path (.Rar) Or Zipped Folder (.Zip), Please Extract Nihon And Try Again.",
                "Error: Temporary Path", MessageBoxButton.OK, MessageBoxImage.Information);

            ClientProcess.Kill();
        }

        //if (Manager.Core.Interface.Version != Manager.VersionNumber)
        //{
        //    MessageBox.Show($"Incorrect Version Of Nihon \nRedownload From https://wearedevs.net/d/Nihon, \nYour Version Is \"{Manager.VersionNumber}\" Latest Version : \"{Manager.Core.Interface.Version}\"", "Error: Invalid Version",
        //        MessageBoxButton.OK, MessageBoxImage.Error);

        //    Application.Current.Shutdown(0);
        //}

        //if (Manager.Core.Interface.IsEnabled != true)
        //{
        //    MessageBox.Show("Sorry It Appear's Nihon Has Been Temporarily Disabled, Please Await An Announcement And Try Again Later.", "Error - Temporarily Disabled",
        //        MessageBoxButton.OK, MessageBoxImage.Error);
        //}

        if (/*await Authenticator.AutoVerify() == true ||*/ Manager.IsDev == true)
        {
            this.Hide();
            new UserInterface().Show();
            return;
        }

        this.Show();
    }

    private void MouseDownHandler(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }

    private void GetKeyButton_Click(object sender, RoutedEventArgs e) => Process.Start(Authenticator.Link);

    private async void EnterButton_Click(object sender, RoutedEventArgs e)
    {
        if (await Authenticator.VerifyKey(this.PasswordBox.Text) == false)
        {
            MessageBox.Show("It Appears The Key You Entered Is Invalid Please Generate A New One", "Error - Invalid Key",
                MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        Animations.Fade(this, 1, 0, 1, Ease.QuarticEase);
        await Task.Delay(1000);
        this.Hide(); new UserInterface().Show();
    }
}
