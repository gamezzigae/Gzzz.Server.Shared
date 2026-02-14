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
		using var decompressStream = new ZstdSharp.DecompressionStream(memoryStream);
		using var outputStream = new MemoryStream();
		
		decompressStream.CopyTo(outputStream);
		
		return outputStream.ToArray();
	}

}
