using System.Text;
using System.IO.Compression;

namespace Gzzz;


public static class GZipJson
{
    public static string SerializeToBase64<T>(T obj)
    {
        var json = Json.Serialize(obj);
        var bytes = DefaultConfig.Encoding.GetBytes(json);
        var compressed = GZip.Compress(bytes);
        return Convert.ToBase64String(compressed);
    }

    public static T DeserializeFromBase64<T>(string base64)
    {
        var compressed = Convert.FromBase64String(base64);
        var decompressed = GZip.Decompress(compressed);
        var json = DefaultConfig.Encoding.GetString(decompressed);
        return Json.Deserialize<T>(json);
    }
}



public static class GZip
{
	public static byte[] Compress(byte[] bytes)
	{
		using var memoryStream = new MemoryStream();

		using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
		{
			gzipStream.Write(bytes, 0, bytes.Length);
		}

		return memoryStream.ToArray();

	}

	public static byte[] Decompress(byte[] bytes)
	{
		using var memoryStream = new MemoryStream(bytes);

		using var outputStream = new MemoryStream();

		using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
		{
			decompressStream.CopyTo(outputStream);
		}

		return outputStream.ToArray();
	}



	public static string CompressToBase64(string text)
	{
		var bytes = Encoding.UTF8.GetBytes(text);
		var compressed = Compress(bytes);
		var base64 = Convert.ToBase64String(compressed);
		return base64;
	}

	public static string DecompressFromBase64(string text)
	{
		var bytes = Convert.FromBase64String(text);
		var decompressedBytes = Decompress(bytes);
		var originalText = Encoding.UTF8.GetString(decompressedBytes);
		return originalText;
	}
}
