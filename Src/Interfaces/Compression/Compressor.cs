using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Nihon.Interfaces;

public class Compressor
{
    public enum Type
    {
        GZip = 1,
        Deflate,
        Lz4,
    }

    public static string Compress(Type CompressionType, string Text)
    {
        switch (CompressionType)
        {
            case Type.GZip:
                byte[] Bytes = Encoding.UTF8.GetBytes(Text);
                using (MemoryStream CompressedStream = new MemoryStream())
                {
                    using (GZipStream GZipStream = new GZipStream(CompressedStream, CompressionMode.Compress))
                        GZipStream.Write(Bytes, 0, Bytes.Length);

                    Bytes = CompressedStream.ToArray();
                }
                return Convert.ToBase64String(Bytes);
            case Type.Deflate:
                Bytes = Encoding.UTF8.GetBytes(Text);
                using (MemoryStream CompressedStream = new MemoryStream())
                {
                    using (DeflateStream DeflateStream = new DeflateStream(CompressedStream, CompressionMode.Compress))
                        DeflateStream.Write(Bytes, 0, Bytes.Length);

                    Bytes = CompressedStream.ToArray();
                }
                return Convert.ToBase64String(Bytes);
            case Type.Lz4:
                return Lz4.CompressUri(Text);
        }

        return $"Failed To Compress Or Invalid Compression Type {CompressionType}.";
    }

    public static string Decompress(Type DecompressionType, string Text)
    {
        switch (DecompressionType)
        {
            case Type.GZip:
                byte[] Bytes = Convert.FromBase64String(Text);
                using (MemoryStream DecompressedStream = new MemoryStream())
                {
                    using (MemoryStream CompressedStream = new MemoryStream(Bytes))
                    {
                        using (GZipStream GZipStream = new GZipStream(CompressedStream, CompressionMode.Decompress))
                            GZipStream.CopyTo(DecompressedStream);
                    }

                    Bytes = DecompressedStream.ToArray();
                }
                return Encoding.UTF8.GetString(Bytes);
            case Type.Deflate:
                Bytes = Convert.FromBase64String(Text);
                using (MemoryStream DecompressedStream = new MemoryStream())
                {
                    using (MemoryStream CompressedStream = new MemoryStream(Bytes))
                    {
                        using (DeflateStream DeflateStream = new DeflateStream(CompressedStream, CompressionMode.Decompress))
                            DeflateStream.CopyTo(DecompressedStream);
                    }

                    Bytes = DecompressedStream.ToArray();
                }
                return Encoding.UTF8.GetString(Bytes);
            case Type.Lz4:
                return Lz4.DecompressUri(Text);
        }

        return $"Failed To Decompress Or Invalid Compression Type {DecompressionType}.";
    }
}
