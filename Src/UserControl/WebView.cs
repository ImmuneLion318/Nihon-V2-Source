using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Nihon.Static;

namespace Nihon.UserControlView;

public class WebView : WebView2
{
    public static bool IsWebViewInstalled = CoreWebView2Environment.GetAvailableBrowserVersionString(null) != null;

    public enum CommandType
    {
        HideCode,
        ShowCode,

        GetText,
        SetText,
        SetTheme,
        SetScroll,
        Refresh,
        AddIntellisense,

        SetMiniMap,
        SetLinks,
        SetFontSize,
        SetFontFamily,
        SetReadonly,
        SetLineHeight
    }

    public event EventHandler EditorReady;

    public Data.EditorOptions EditorOptions = new Data.EditorOptions
    {
        Links = true,
        MiniMap = false,
        FontSize = 14,
        ReadOnly = false,
        Theme = "Dark"
    };

    public new bool IsLoaded = false;

    public WebView() : base() => this.InitializeComponents();

    private async void InitializeComponents()
    {
        CoreWebView2Environment Enviroment = await CoreWebView2Environment.CreateAsync(
             null,
             Path.GetTempPath(),
             new CoreWebView2EnvironmentOptions
             {
                 Language = "en-US"
             }
             );

        await this.EnsureCoreWebView2Async(Enviroment);

        this.DefaultBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        this.Source = new Uri("https://google.com");

        this.CoreWebView2.Settings.AreDevToolsEnabled = false;
        this.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

        this.CoreWebView2.ProcessFailed += (Sender, Event) =>
        {
            MessageBox.Show($"{Event.ProcessFailedKind} - {Event.Reason}", $"Error - {Event.ProcessFailedKind}",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };

        this.CoreWebView2.SourceChanged += async (Source, Object) =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            this.IsLoaded = true;

            this.EditorReady?.Invoke(
                this,
                EventArgs.Empty);
        };
    }

    public async Task<string> ExecuteScript<T>(CommandType Command, T Object = default)
    {
        if (!this.IsLoaded == true || Object is null)
            return $"WebView Control Not Available | Command, {Command} Does Not Exist, Or" +
                $"Arguments Of {Object} Are Invalid.";

        string Output = await this.CoreWebView2.ExecuteScriptAsync($"{Enum.GetName(typeof(CommandType), Command)}({(Object is bool || Object is int ?
            Object.ToString().ToLower() : $"\"{HttpUtility.JavaScriptStringEncode(Object as string)}\"")})");

        if (Output.Length > 0 && Output[0] == '"')
            Output = Output.Substring(1, Output.Length - 2);

        return Output is string && Output != string.Empty ? Regex.Unescape(Output) : Output;
    }
}