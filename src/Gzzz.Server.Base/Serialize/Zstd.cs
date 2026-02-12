using System.Text;

namespace Gzzz.Serialize;

public static class Zstd
{
	public static byte[] Compress(byte[] bytes)
	{
		using var memoryStream = new MemoryStream();
		using var zstdStream = new ZstdSharp.CompressionStream(memoryStream);
		
		zstdStream.Write(bytes, 0, bytes.Length);
		zstdStream.Flush();
		
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] bytes)
	{
		using var memoryStream = new MemoryStream(bytes);
		using var outputStream = new MemoryStream();
		using var decompressStream = new ZstdSharp.DecompressionStream(memoryStream);
		
		decompressStream.CopyTo(outputStream);
		
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
