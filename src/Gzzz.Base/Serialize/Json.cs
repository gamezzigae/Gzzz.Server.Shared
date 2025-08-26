using System.Text.Json;
using System.Text;

namespace Gzzz.Serialize;

public static class DefaultConfig
{
    public static readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
    public static readonly Encoding Encoding = new UTF8Encoding(false);
}

public static class Json
{
	public static string Serialize(object item)=> JsonSerializer.Serialize(item);
    public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json);
}
