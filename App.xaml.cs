using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Threading;
using DiscordRPC;
using Newtonsoft.Json;
using Nihon.Interfaces;
using Nihon.Static;

namespace Nihon;

public partial class App : Application
{
    public static DiscordIntegration DiscordIntegration = null;/*new DiscordIntegration("[PRIVATE_INFO]")*/

    string[] IgnoredExceptions = { "Microsoft.Web.WebView2.Wpf", "Microsoft.Web.WebView2.Winforms", "Microsoft.Web.WebView2.Core" };
    Queue<Exception> ExceptionQueue = new Queue<Exception> { };
    const int MaxExceptionCount = 2;

    protected override async void OnStartup(StartupEventArgs e)
    {
        if (!Directory.Exists("Scripts"))
            Directory.CreateDirectory("Scripts");

        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
        | SecurityProtocolType.Tls12
        | SecurityProtocolType.Tls11
        | SecurityProtocolType.Tls;

        ServicePointManager.ServerCertificateValidationCallback += (object Sender, X509Certificate Certificate, X509Chain Chain, SslPolicyErrors Errors)
            => true;

        Current.DispatcherUnhandledException += delegate (object Source, DispatcherUnhandledExceptionEventArgs e)
        {
            lock (this.ExceptionQueue)
            {
                foreach (string Ignored in this.IgnoredExceptions)
                    if (e.Exception.Source == Ignored)
                    {
                        e.Handled = true;
                        return;
                    }

                this.ExceptionQueue.Enqueue(e.Exception);

                if (this.ExceptionQueue.Count > 1)
                    return;

                Exception PeekException = this.ExceptionQueue.Peek();

                int ExceptionCount = 0;

                foreach (Exception Exceptions in this.ExceptionQueue)
                {
                    if (Exceptions.Message == PeekException.Message && Exceptions.GetType() == PeekException.GetType())
                        ExceptionCount++;
                }

                if (ExceptionCount > MaxExceptionCount)
                {
                    this.ExceptionQueue.Dequeue();
                    return;
                }

                this.ExceptionQueue.Dequeue();

                if (MessageBox.Show("Nihon Has Caught An Exception, Would You Like To Copy This To Clipboard?", "Error: Unhandled Exception", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                    Clipboard.SetText($"Exception Message: {e.Exception.Message}\nException Stack Trace: {e.Exception.StackTrace}\nException Context: {e.Exception.Source}\n Other Data: {e.Exception.InnerException}");

                e.Handled = true;
            }
        };

        //HttpRequest Request = new HttpRequest
        //{
        //    Url = "[PRIVATE_INFO]",
        //    Method = HttpMethod.Get,

        //    Headers = new WebHeaderCollection
        //    { { HttpRequestHeader.UserAgent, "Nihon-3.0" } }
        //};
        //HttpResponse Response = await HttpService.CreateRequest(Request);

        Manager.Core = default;

        //Manager.Core = JsonConvert.DeserializeObject<Data.InterfaceInfo>
        //    (Response.Text);

        //DiscordIntegration.CreatePresence(new RichPresence
        //{
        //    Details = "#1 Best Executor",
        //    State = "Supports 99% Of Scripts.",
        //    Buttons = new Button[]
        //    {
        //        new Button
        //        {
        //            Label = "Download",
        //            Url = "https://wearedevs.net/d/Nihon"
        //        },
        //        new Button
        //        {
        //            Label = "Discord",
        //            Url = "https://discord.gg/nihonrbx"
        //        }
        //    },
        //    Assets = new Assets
        //    {
        //        LargeImageKey = "nihon",
        //        LargeImageText = "Nihon"
        //    },
        //    Timestamps = Timestamps.Now
        //});

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (DiscordIntegration.IsOpen == true)
        {
            DiscordIntegration.Client.ClearPresence();

            if (!DiscordIntegration.Client.IsDisposed)
                DiscordIntegration.Client.Dispose();
        }

        Current.Shutdown(0);

        base.OnExit(e);
    }
}
