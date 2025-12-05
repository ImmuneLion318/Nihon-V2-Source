using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nihon.Interfaces;

/* If Only The Nihon 3.0 Source Was Public And You Could Access The One With HttpClient Instead And Way Better And Faster :( */
public struct HttpResponse
{
    public Version
    Protocal;

    public string
    Text,
    StatusDescription;

    public HttpStatusCode
    StatusCode;

    public WebHeaderCollection
    Headers;
}

public class HttpRequest
{
    public Encoding
    Encoding = Encoding.UTF8;

    public IWebProxy
    Proxy = null;

    public TimeSpan
    TimeOut = TimeSpan.FromSeconds(100);

    public string
    Url, Body = string.Empty;

    public HttpMethod
    Method;

    public WebHeaderCollection
    Headers = null;
}

public class HttpService : WebClient
{
    private static HttpResponse Default = new HttpResponse
    {
        Text = string.Empty,
        Headers = null,
        Protocal = null,
        StatusCode = HttpStatusCode.OK,
        StatusDescription = string.Empty
    };

    protected override WebResponse GetWebResponse(WebRequest Request, IAsyncResult AsyncOutput)
    {
        WebResponse WebResponse = null;

        try
        {
            WebResponse = base.GetWebResponse(Request);
            HttpWebResponse BaseResponse = WebResponse as HttpWebResponse;

            Default.StatusCode = BaseResponse.StatusCode;
            Default.StatusDescription = BaseResponse.StatusDescription;
            Default.Protocal = BaseResponse.ProtocolVersion;
        }
        catch (WebException Exception)
        {
            WebResponse ??= Exception.Response;
        }
        return WebResponse;
    }

    public static async Task<HttpResponse> CreateRequest(HttpRequest Request)
    {
        HttpResponse Response = new HttpResponse
        {
            Text = string.Empty,
            Headers = null,
            Protocal = null,
            StatusCode = HttpStatusCode.OK,
            StatusDescription = string.Empty
        };

        WebClient Http = new WebClient { Proxy = Request.Proxy };

        if (Request.Headers != null)
        {
            for (int i = 0; i < Request.Headers.Count; ++i)
            {
                if (Request.Headers[i].GetType() == typeof(HttpRequestHeader))
                    Http.Headers.Add(Enum.GetName(typeof(HttpRequestHeader), Request.Headers.GetKey(i)), Request.Headers[i]);

                else Http.Headers.Add(Request.Headers.GetKey(i), Request.Headers[i]);
            }
        }

        switch (Request.Method.Method)
        {
            case "POST":
                Response = new HttpResponse
                {
                    Text = await Http.UploadStringTaskAsync(Request.Url, Request.Method.Method, Request.Body),
                    Headers = Http.ResponseHeaders,
                    StatusCode = Default.StatusCode,
                    StatusDescription = Default.StatusDescription,
                    Protocal = Default.Protocal
                };
                break;
            case "GET":
                Response = new HttpResponse
                {
                    Text = await Http.DownloadStringTaskAsync(Request.Url),
                    Headers = Http.ResponseHeaders,
                    StatusCode = Default.StatusCode,
                    StatusDescription = Default.StatusDescription,
                    Protocal = Default.Protocal
                };
                break;
            case "DELETE":
                Response = new HttpResponse
                {
                    Text = await Http.UploadStringTaskAsync(Request.Url, Request.Method.Method, Request.Body),
                    Headers = Http.ResponseHeaders,
                    StatusCode = Default.StatusCode,
                    StatusDescription = Default.StatusDescription,
                    Protocal = Default.Protocal
                };
                break;
            default:
                break;
        }

        Http.Dispose();
        return Response;
    }

    public static async Task DownloadFileAsync(string Url, string Path)
    {
        using (WebClient Http = new WebClient { Proxy = null })
            await Http.DownloadFileTaskAsync(Url, Path);
    }

    public static async Task<string> GetAsync(string Url)
    {
        using (WebClient Http = new WebClient { Proxy = null })
            return await Http.DownloadStringTaskAsync(Url);
    }
}
