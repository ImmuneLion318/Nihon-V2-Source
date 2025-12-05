using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Nihon.Interfaces;
using Nihon.Static;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;

namespace Nihon.Authentication;

public class Hardware
{
    public static string GetHardwareId(out string RawId)
    {
        string ProcessorId = GetProperty("win32_processor", "processorID");
        string BiosSerial = GetProperty("win32_bios", "SerialNumber");
        string DeviceId = GetProperty("win32_processor", "DeviceID");
        string MemoryCapacity = GetProperty("Win32_PhysicalMemory", "Capacity");
        string Motherboard = GetProperty("Win32_ComputerSystemProduct", "UUID");

        string CombinedId = ProcessorId + BiosSerial + DeviceId + MemoryCapacity + Motherboard;

        byte[] BytesToHash = Encoding.UTF8.GetBytes(CombinedId);
        byte[] HashBytes;

        using (SHA512 Sha512 = SHA512.Create())
            HashBytes = Sha512.ComputeHash(BytesToHash);

        string Base64Hash = Convert.ToBase64String(HashBytes);

        BytesToHash = Encoding.UTF8.GetBytes(Base64Hash);

        using (SHA384 Sha384 = SHA384.Create())
            HashBytes = Sha384.ComputeHash(BytesToHash);

        string Output = BitConverter.ToString(HashBytes).Replace("-", "").ToLower();
        RawId = Output;

        char[] Characters = Output.ToCharArray();
        StringBuilder BinaryString = new StringBuilder();

        foreach (char Character in Characters)
            BinaryString.Append(Convert.ToString(Character, 2).PadLeft(8, '0'));

        string[] BinaryChunks = Enumerable.Range(0, BinaryString.Length / 8)
                                           .Select(i => BinaryString.ToString().Substring(i * 8, 8))
                                           .ToArray();

        string[] DecimalChunks = BinaryChunks.Select(x => Convert.ToInt32(x, 2).ToString()).ToArray();

        return string.Join(string.Empty, DecimalChunks);
    }

    public static string GetProperty(string Path, string Property)
    {
        using (ManagementClass Management = new ManagementClass(Path))
        {
            using (ManagementObjectCollection ObjectCollection = Management.GetInstances())
                foreach (ManagementBaseObject Object in ObjectCollection)
                    return Object.Properties[Property].Value.ToString();
        }

        return null;
    }
}

public static class Authenticator
{
    public static async Task<bool> AutoVerify()
    {
        string Id = Hardware.GetHardwareId(out _);

        HttpRequest Request = new HttpRequest
        {
            Url = "[PRIVATE_INFO]",
            Body = JsonConvert.SerializeObject(new Data.VerificationObject
            {
                Hardware = Id
            }, Formatting.Indented),

            Method = HttpMethod.Post,
            Proxy = null
        };
        HttpResponse Response = await HttpService.CreateRequest(Request);

        if (Response.StatusCode != HttpStatusCode.OK || Response.StatusDescription != string.Empty)
            MessageBox.Show("Failed To Automatically Verify, Please Try Again Later.", "Error - Request Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);

        return Rsa.VerifySignature(Response.Text, Id + "Autologin");
    }

    public static async Task<bool> VerifyKey(string Key)
    {
        string Id = Hardware.GetHardwareId(out _);

        HttpRequest Request = new HttpRequest
        {
            Url = "[PRIVATE_INFO]",
            Body = JsonConvert.SerializeObject(new Data.VerificationObject
            {
                Hardware = Id,
                Key = Key,
            }, Formatting.Indented),

            Method = HttpMethod.Post,
            Proxy = null
        };
        HttpResponse Response = await HttpService.CreateRequest(Request);

        if (Response.StatusCode != HttpStatusCode.OK || Response.StatusDescription != string.Empty)
            MessageBox.Show("Failed To Verify Key, Please Try Again.", "Error - Request Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);

        return Rsa.VerifySignature(Response.Text, Id + Key);
    }

    public static string Link { get => Task.Run(CreateLink).Result; }

    private static async Task<string> CreateLink()
    {
        string Id = Hardware.GetHardwareId(out string Raw);

        Console.WriteLine($"Hardware - Information : \n" +
             $"  Id : {Id}\n" +
             $"  Raw Id : {Raw}");

        HttpRequest Request = new HttpRequest
        {
            Url = "[PRIVATE_INFO]",
            Body = JsonConvert.SerializeObject(new Data.VerificationObject
            {
                Hardware = Id,
            }, Formatting.Indented),

            Method = HttpMethod.Post,
            Proxy = null
        };
        HttpResponse Response = await HttpService.CreateRequest(Request);

        if (Response.StatusCode != HttpStatusCode.OK || Response.StatusDescription != string.Empty)
            MessageBox.Show("Failed To Generate Ad Link, Please Try Again.", "Error - Request Failed",
                MessageBoxButton.OK, MessageBoxImage.Error);

        return Response.Text;
    }
}

public class Rsa
{
    private static readonly string PublicKey = "[PRIVATE_INFO]";

    public static bool VerifySignature(string Signature, string Response)
    {
        if (Signature == "false")
            return false;

        AsymmetricKeyParameter Key;
        using (StringReader Reader = new StringReader(PublicKey))
        {
            PemReader PemReader = new PemReader(Reader);
            Key = (AsymmetricKeyParameter)PemReader.ReadObject();
        }

        byte[] ResponseBytes = Encoding.UTF8.GetBytes(Response);
        byte[] SignatureBytes = Convert.FromBase64String(Signature);
        PssSigner Signer = new PssSigner(new RsaEngine(), new Sha256Digest(), 32);

        Signer.Init(false, Key);
        Signer.BlockUpdate(ResponseBytes, 0, ResponseBytes.Length);
        return Signer.VerifySignature(SignatureBytes);
    }
}