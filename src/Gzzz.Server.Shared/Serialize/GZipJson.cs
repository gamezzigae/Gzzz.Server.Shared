using Microsoft.Extensions.ObjectPool;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Gzzz.Serialize;

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

public class MemoryStreamPool : DefaultObjectPool<MemoryStream>
{
	public static readonly MemoryStreamPool Shared = new();
	public MemoryStreamPool() : base(new DefaultPooledObjectPolicy<MemoryStream>(), 100)
	{
	}
	public override void Return(MemoryStream item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		item.SetLength(0); // Reset the stream length
		base.Return(item);
	}
}


public static class GZipJson2
{
	
	public static async Task<string> SerializeToBase64Async<T>(T obj)
	{
		var stream = MemoryStreamPool.Shared.Get();
		var stream2 = MemoryStreamPool.Shared.Get();
		try
		{
			await JsonSerializer.SerializeAsync(stream, obj);
;
			using (var gzipStream = new GZipStream(stream2, CompressionLevel.Optimal, leaveOpen: true))
			{
				stream.Position = 0;
				stream.CopyTo(gzipStream);
			}
			var compressed = stream2.ToArray();
			return Convert.ToBase64String(compressed);
		}
		finally
		{
			MemoryStreamPool.Shared.Return(stream);
			MemoryStreamPool.Shared.Return(stream2);
		}
	}

	public static T DeserializeFromBase64<T>(string base64)
	{
		var compressed = Convert.FromBase64String(base64);
		var decompressed = GZip.Decompress(compressed);
		var json = DefaultConfig.Encoding.GetString(decompressed);
		return Json.Deserialize<T>(json);
	}
}
