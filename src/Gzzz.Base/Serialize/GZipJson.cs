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
