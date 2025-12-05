using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Nihon.ContentView;
using Nihon.Static;
using Nihon.UserControlView;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Nihon.Interfaces;

public static class WebSocket
{
    private static Data.WebSocketHolder Whitelist = new Data.WebSocketHolder { };

    public static WebSocketServer Server;
    private static UserInterface Interface;

    public class Socket : WebSocketBehavior
    {
        public Func<string, Task<bool>> Message { get; set; }
        public static bool Init;

        public Socket() => this.OriginValidator = (string Origin) =>
        {
            if (Origin == null)
                return false;

            foreach (Data.WebSocketEntry Entry in Whitelist.Entries)
            {
                if (Entry.Origin == Origin)
                    return true;
            }

            if (MessageBox.Show($"Would You Like To Allow Connections From \"{Origin}\" For This Session?", "Nihon Socket-Security",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                Data.WebSocketEntry HostEntry = new Data.WebSocketEntry
                {
                    Origin = Origin,
                    IsLocal = this.Context.IsLocal,
                    IsSecure = this.Context.IsSecureConnection
                };
                Whitelist.Entries.Add(HostEntry);

                return true;
            }

            return false;
        };

        protected override void OnOpen()
        {
            if (Init) return;

            this.Context.WebSocket.OnClose += (object Sender, CloseEventArgs e) =>
            {
                Init = false;

                Console.WriteLine("Socket - Information : \n" +
                    $"  Origin : {this.Context.Origin}\n" +
                    $"  Reason : {(e.Reason != string.Empty ? e.Reason : "None")} | Code - {e.Code}");
            };

            this.Context.WebSocket.OnMessage += (object Source, MessageEventArgs e) => this.Message?.Invoke(e.Data);

            Init = true;
        }
    }

    public static async Task StartAsync(int Port, UserInterface UserInterface)
    {
        Interface = UserInterface;

        HttpRequest Request = new HttpRequest
        {
            Url = "[PRIVATE_INFO]",
            Method = HttpMethod.Get,

            Headers = new WebHeaderCollection
            { { HttpRequestHeader.UserAgent, "Nihon-3.0" } }
        };
        HttpResponse Response = await HttpService.CreateRequest(Request);

        Whitelist = JsonConvert.DeserializeObject<Data.WebSocketHolder>
            (Response.Text);

        Server = new WebSocketServer(Port)
        {
            WaitTime = TimeSpan.FromMilliseconds(250),
            ReuseAddress = true
        };

        Server.AddWebSocketService("/editor", () => new Socket
        {
            Message = async (string Message) =>
            {
                await Interface.Dispatcher.Invoke(()
                    => Interface.Editor.ExecuteScript(WebView.CommandType.SetText, Message));

                return true;
            }
        });

        Server.AddWebSocketService("/module", () => new Socket
        {
            Message = async (string Message) => await Interface.Library.RunScript(Message)
        });

        Server.AddWebSocketService("/output", () => new Socket
        {
            Message = async (string Message) =>
            {
                await Task.Delay(50);

                Data.InteractObject Interaction = JsonConvert.DeserializeObject<Data.InteractObject>
                    (Message);

                Console.WriteLine("Interact - Information : \n" + 
                    $"Type '{Interaction.Type}'\n" + 
                    $"Message - {Interaction.Message}");

                return true;
            }
        }) ;

        Server.Start();

        Console.WriteLine($"Server Started On Port : {Port} [\"Local Host\"] Machine {Environment.MachineName}");
    }

    public static void Stop() => Server.Stop();
}
