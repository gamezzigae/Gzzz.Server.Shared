using Gzzz.Serialize;
using System.Text.RegularExpressions;

namespace Gzzz;

public static class EnvironmentX
{
	public static void SetProcessValue(string name, string value)
	{
		Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
	}

	public static string GetRequiredValue(string name)
    {
        var result = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(result))
			throw new Exception("필수 환경변수 누락:" + name);
        return result;
    }
    public static T GetRequiredObject<T>(string name)
    {
        var json = GetRequiredValue(name);
        return Json.Deserialize<T>(json);
    }
    public static string GetValueOrDefault(string name, string defaultValue = null)
    {
        var result = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(result))
            return defaultValue;
        return result;
    }
}
