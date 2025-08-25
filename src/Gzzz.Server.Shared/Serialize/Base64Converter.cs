namespace Gzzz.Serialize;

public static class Base64Converter
{
	public static byte[] Deserialize(string base64)
	{
		var padding = base64.Length % 4;
		if (padding > 0)
		{
			base64 += new string('=', 4 - padding);
		}
		var bytes = Convert.FromBase64String(base64);
		return bytes;
	}

	public static string Serialize(byte[] bytes, bool trim = false)
	{
		var base64 = Convert.ToBase64String(bytes);
		if (trim)
		{
			base64 = base64.TrimEnd('=');
		}
		return base64;
	}

	public static string Trim(string base64)=> base64.TrimEnd('=');
}
