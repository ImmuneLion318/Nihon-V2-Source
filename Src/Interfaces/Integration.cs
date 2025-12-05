using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Newtonsoft.Json;
using Octokit;

namespace Nihon.Interfaces;

public class DiscordIntegration
{
    public DiscordRpcClient Client;

    public bool IsOpen => this.Client.IsInitialized;

    public DiscordIntegration(string Id)
    {
        this.Client = new DiscordRpcClient(Id)
        {
            Logger = new ConsoleLogger
            {
                Coloured = true,
                Level = LogLevel.Error
            },
            ShutdownOnly = true
        };

        this.Client.Initialize();

        this.Client.OnReady += (object Source, ReadyMessage OnReady)
            => Console.WriteLine($"On Ready. Presence Version: {OnReady.Version}");

        this.Client.OnClose += (object Source, CloseMessage OnClose)
            => Console.WriteLine($"Lost Connection With Client Because Of '{OnClose.Reason}'");

        this.Client.OnError += (object Source, ErrorMessage OnError)
            => Console.WriteLine($"An Error Occured With Discord. ({OnError.Code}) {OnError.Message}");

        this.Client.OnConnectionEstablished += (object Source, ConnectionEstablishedMessage OnConnect)
            => Console.WriteLine($"Pipe Connection Established. Valid on Pipe #{{0}}", OnConnect.ConnectedPipe);

        this.Client.OnConnectionFailed += (object Source, ConnectionFailedMessage ConnectFail)
            => Console.WriteLine($"Pipe Connection Failed. Could Not Connect To Pipe #{{0}}", ConnectFail.FailedPipe);

        this.Client.OnPresenceUpdate += (object Source, PresenceMessage OnUpdate)
            => Console.WriteLine($"Rich Presence Updated. Playing {(OnUpdate.Presence == null ? "Nothing (Null)" : OnUpdate.Presence.State)}");
    }

    public void CreatePresence(RichPresence Presence) =>
        this.Client.SetPresence(Presence);

    public class InviteObject
    {
        [JsonProperty("args")]
        public ArgumentObject Parameters { get; set; }

        public class ArgumentObject
        {
            [JsonProperty("code")]
            public string Code { get; set; }
        }

        [JsonProperty("cmd")]
        public string Command { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }
    }

    public async Task<bool> JoinDiscord(string Link)
    {
        Console.Write($"Discord Link - {Link}");

        HttpRequest Request = new HttpRequest
        {
            Url = "[PRIVATE_INFO]",
            Body = JsonConvert.SerializeObject(new InviteObject
            {
                Parameters = new InviteObject.ArgumentObject
                {
                    Code = Link
                },
                Command = "INVITE_BROWSER",
                Nonce = "."
            }),
            Method = HttpMethod.Post,
            Encoding = Encoding.ASCII,

            Headers = new WebHeaderCollection
            {
                { HttpRequestHeader.ContentType, "application/json" },
                { "Origin", "https://discord.com" },
            }
        };
        HttpResponse Response = await HttpService.CreateRequest(Request);

        return Response.StatusCode == HttpStatusCode.OK
            ? true : false;
    }
}

public static class ScriptIntegration
{
    public static string Token = "[PRIVATE_INFO]";
    public static bool IsVerified = false;

    public static async Task<string> GetScriptAsync(string Id)
    {
        HttpRequest Request = new HttpRequest
        {
            Url = "[PRIVATE_INFO]",
            Body = null,
            Method = HttpMethod.Get,

            Headers = new WebHeaderCollection
            { { HttpRequestHeader.Cookie, $"token={Token}" }}
        };
        HttpResponse Response = await HttpService.CreateRequest(Request);

        return Compressor.Decompress(Compressor.Type.Lz4, Response.Text);
    }

    public static async Task<string> PostScriptAsync(ScriptObject Object)
    {
        HttpRequest Request = new HttpRequest
        {
            Url = "[PRIVATE_INFO]",
            Body = JsonConvert.SerializeObject(Object),
            Method = HttpMethod.Post,

            Headers = new WebHeaderCollection
            { { HttpRequestHeader.Cookie, $"token={Token}" }}
        };
        HttpResponse Response = await HttpService.CreateRequest(Request);

        return Response.Text;
    }

    public class ScriptObject
    {
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("script")]
        public string Script;

        [JsonProperty("category")]
        public string Category;

        [JsonProperty("games")]
        public string Games;
    }
}

public class GithubIntegration
{
    public string Token;
    public GitHubClient Client;

    public GithubIntegration(string UserToken)
    {
        this.Token = UserToken;

        this.Client = new GitHubClient(new ProductHeaderValue("Nihon-WinInet"))
        {
            Credentials = new Credentials(this.Token)
        };
    }

    public Dictionary<string, string> GistInfo = new Dictionary<string, string>();

    public async Task<Octokit.User> GetUser(string Username = null) =>
        Username == null ? await this.Client.User.Current() : await this.Client.User.Get(Username);

    public KeyValuePair<string, GistFile> GetGist(string Id) =>
      this.Client.Gist.Get(Id).Result.Files.FirstOrDefault();

    public IReadOnlyList<Gist> GetAllGists() =>
        this.Client.Gist.GetAll().GetAwaiter().GetResult();

    public IReadOnlyList<Gist> GetUserGists(string User) =>
        this.Client.Gist.GetAllForUser(User).GetAwaiter().GetResult();

    public void UploadGist(string Name, string Content, string Description, bool IsPublic = false)
    {
        NewGist Gist = new NewGist()
        {
            Description = Description,
            Public = IsPublic,
        };
        Gist.Files.Add(Name, Content);

        Process.Start(this.Client.Gist.Create(Gist).GetAwaiter().GetResult().HtmlUrl);
        Console.WriteLine($"Gist Created, Title : {Name} Content : {Content}");
    }

    public void DeleteGist(string Id)
    {
        this.Client.Gist.Delete(Id);
        Console.WriteLine($"Deleted Gist : {Id}");
    }

    public void EditGist(string FileName, string Content)
    {
        GistUpdate Update = new GistUpdate
        {
            Description = "My Created Gist File"
        };
        GistFileUpdate FileUpdate = new GistFileUpdate
        {
            NewFileName = FileName,
            Content = Content
        };

        Update.Files.Add("MyGist.cs", FileUpdate);

        this.Client.Gist.Edit("1", Update);
    }
}

public class PastebinIntegration
{
    public string Key = "";

    public class User
    {
        public string
        Username,
        Password,
        UserKey;

        private PastebinIntegration Core;

        public User(string User, string Pass)
        {
            this.Username = User;
            this.Password = Pass;
        }

        public string GetKey()
        {
            using (WebClient Http = new WebClient())
            {
                NameValueCollection Data = new NameValueCollection
                {
                    { "api_dev_key", this.Core.Key },
                    { "api_user_name", this.Username },
                    { "api_user_password", this.Password }
                };

                try
                {
                    byte[] Response = Http.UploadValues(new Uri("https://pastebin.com/api/api_login.php"), Data);
                    return Encoding.ASCII.GetString(Response);
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.Message);
                    return null;
                }
            }
        }
    }

    public PastebinIntegration(string AuthenticateKey) => this.Key = AuthenticateKey;

    public void CreatePaste(string PostName, string PostText, User User)
    {
        using (WebClient Http = new WebClient())
        {
            NameValueCollection Data = new NameValueCollection
            {
                { "api_option", "paste" },
                { "api_paste_name", PostName },
                { "api_dev_key", this.Key },
                { "api_paste_code", PostText },
                { "api_user_key", User.UserKey }
            };

            try
            {
                Http.UploadValuesAsync(new Uri("https://pastebin.com/api/api_post.php"), Data);
            }
            catch (Exception Exception)
            {
                Console.WriteLine(Exception.Message);
            }

            Http.UploadValuesCompleted += (object Sender, UploadValuesCompletedEventArgs Event) => MessageBox.Show(Encoding.ASCII.GetString(Event.Result));
        }
    }

    public void GetPastes(User User)
    {
        using (WebClient Http = new WebClient())
        {
            NameValueCollection Data = new NameValueCollection
            {
                { "api_dev_key",this.Key },
                { "api_results_limit", "100" },
                { "api_user_key", User.UserKey },
                { "api_option", "list" },
            };
            try
            {
                Console.WriteLine("Attempting To Fer Pastes");
                Http.UploadValuesAsync(new Uri("https://pastebin.com/api/api_post.php"), Data);
            }
            catch (Exception Exception)
            {
                Console.WriteLine("Error " + Exception.Message);
            }

            Http.UploadValuesCompleted += (object Sender, UploadValuesCompletedEventArgs Event) =>
            {
                string ParsedResult = XDocument.Parse($"<?xml version='1.0' encoding='utf-8'?><result>{Event.Result}</result>").ToString();
                MessageBox.Show(ParsedResult);
            };
        }
    }
}
