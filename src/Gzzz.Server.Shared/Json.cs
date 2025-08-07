using Newtonsoft.Json;
using System.Text;

namespace Gzzz.Server.Shared;

public static class DefaultConfig
{
    public static readonly string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
    public static readonly Encoding Encoding = new UTF8Encoding(false);
}

public static class Json
{
    static readonly JsonSerializerSettings _settings= new JsonSerializerSettings
    {
        DateFormatString = DefaultConfig.DateTimeFormat,
    };

    public static string Serialize(object item)=> Newtonsoft.Json.JsonConvert.SerializeObject(item, _settings);
    public static T Deserialize<T>(string json) => Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, _settings);

}