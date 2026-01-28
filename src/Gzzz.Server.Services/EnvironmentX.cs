using Gzzz.Serialize;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Gzzz;

public static class EnvironmentX
{
	public static void SetValue(string name, string value)
	{
		Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
	}
	public static void SetObject(string name, object value) => SetValue(name, JsonSerializer.Serialize(value));

	public static string GetValue(string name)
    {
        var result = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(result))
			throw new Exception("필수 환경변수 누락:" + name);
        return result;
    }
    public static T GetRequiredObject<T>(string name)
    {
        var json = GetValue(name);
		return JsonSerializer.Deserialize<T>(json);
	}
    public static string GetValueOrDefault(string name, string defaultValue = null)
    {
        var result = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(result))
            return defaultValue;
        return result;
    }
}
