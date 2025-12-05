using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Nihon.Static;

namespace Nihon.ContentView;

public partial class UserManager : Window
{
    public UserManager()
    {
        this.InitializeComponent();

        this.CopyrightLabel.Content = $"Version - {Manager.VersionNumber}";
        this.MainGrid.Margin = new Thickness(0, 0, 0, 350);
        this.BigLogo.Margin = new Thickness(120, 205, 130, 125);
        this.Border1.Margin = new Thickness(-6, -23, 275, 0);
        this.Border2.Margin = new Thickness(-13, 60, 282, 0);
        this.Border3.Margin = new Thickness(288, 80, -20, 0);
        this.DownloadLabel.Margin = new Thickness(-1, 277, 270, 34);
        this.CopyrightLabel.Margin = new Thickness(-1, 309, 270, 14);
        this.BigLogo.Opacity = 0;
        this.SmallLogo.Opacity = 0;
        this.TitleLabel.Opacity = 0;
        this.SloganLogo.Opacity = 0;
    }

    private async void OnLoad(object sender, RoutedEventArgs e)
    {
        this.DownloadLabel.Content = "Preparing Nihon V2";
        bool Result = await this.Init();

        await Task.Delay(1000);
        Animations.Fade(this.MainGrid, 0, 1, 1, Ease.QuinticEase);
        Animations.Shift(this.MainGrid, this.MainGrid.Margin, new Thickness(), 1, Ease.QuinticEase);
        await Task.Delay(900);
        Animations.Fade(this.BigLogo, 0, .5, 1, Ease.QuarticEase);
        Animations.Shift(this.BigLogo, this.BigLogo.Margin, new Thickness(20, 90, 25, 40), 1, Ease.QuarticEase);
        await Task.Delay(900);
        Animations.Fade(this.BigLogo, .5, .1, 1, Ease.QuinticEase);
        Animations.Fade(this.Border1, 0, 1, 1, Ease.QuarticEase);
        Animations.Shift(this.Border1, this.Border1.Margin, new Thickness(-5, -15, -5, 0), 1, Ease.QuarticEase);
        await Task.Delay(200);
        Animations.Fade(this.Border2, 0, 1, 1, Ease.QuarticEase);
        Animations.Shift(this.Border2, this.Border2.Margin, new Thickness(-15, 38, 57, 0), 1, Ease.QuarticEase);
        await Task.Delay(200);
        Animations.Fade(this.Border3, 0, 1, 1, Ease.QuarticEase);
        Animations.Shift(this.Border3, this.Border3.Margin, new Thickness(121, 64, -20, 0), 1, Ease.QuarticEase);
        await Task.Delay(500);
        Animations.Fade(this.SmallLogo, 0, .6, 1, Ease.QuinticEase);
        await Task.Delay(200);
        Animations.Fade(this.TitleLabel, 0, 1, 1, Ease.QuinticEase);
        await Task.Delay(200);
        Animations.Fade(this.SloganLogo, 0, 1, 1, Ease.QuinticEase);
        await Task.Delay(500);
        Animations.Fade(this.DownloadLabel, 0, 1, 1, Ease.QuinticEase);
        Animations.Shift(this.DownloadLabel, this.DownloadLabel.Margin, new Thickness(0, 277, 45, 34), 1, Ease.QuarticEase);
        await Task.Delay(200);
        Animations.Fade(this.CopyrightLabel, 0, 1, 1, Ease.QuinticEase);
        Animations.Shift(this.CopyrightLabel, this.CopyrightLabel.Margin, new Thickness(-1, 309, 141, 14), 1, Ease.QuarticEase);
        await Task.Delay(1000);
        DispatcherTimer timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1) };
        timer.Tick += delegate
        {
            if (this.ProgressBar.Value != 100)
                this.ProgressBar.Value += 1;
            else
                timer.Stop();
        };
        timer.Start();

        await Task.Delay(1500);
        Animations.Fade(this.DownloadLabel, 1, 0, 1, Ease.QuinticEase);
        await Task.Delay(100);
        Animations.Fade(this.ProgressBar, 1, 0, 1, Ease.QuarticEase);
        await Task.Delay(1000);
        Animations.Shift(this.CopyrightLabel, this.CopyrightLabel.Margin, new Thickness(-1, 329, 141, -6), .5, Ease.QuarticEase);
        await Task.Delay(500);
        new UserInterface().Show();
        await Task.Delay(500);

        if (Result == true)
            this.Hide();

        else
        {
            MessageBox.Show("An Error Occured During Initialization Sorry For The Inconvience", "Error: Unknown", MessageBoxButton.OK, MessageBoxImage.Hand);
            Process.GetCurrentProcess().Kill();
        }
    }

    public async Task<bool> Init()
    {
        await Task.Delay(150);

        string[] Directories = { "Bin", "Scripts", "Workspace", "AutoExec", "Bin\\Content" };
        for (int i = 0; i < Directories.Length; i++)
            if (!Directory.Exists(Directories[i])) Directory.CreateDirectory(Directories[i]);

        return true;
    }
}
