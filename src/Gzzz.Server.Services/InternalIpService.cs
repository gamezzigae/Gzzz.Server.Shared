namespace Gzzz;

public class InternalIpService
{
	public static readonly string EnvironmentVariableName = "ZZ_INTERNAL_IP_LIST";

	readonly HashSet<string> _allowedIps;

	public InternalIpService()
	{
		var ipList = EnvironmentX.GetValue(EnvironmentVariableName);
		_allowedIps = ipList
			.Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.ToHashSet(StringComparer.Ordinal);
	}

	public bool IsAllowed(string ip)
	{
		if (string.IsNullOrWhiteSpace(ip))
			return false;

		return _allowedIps.Contains(ip);
	}
}
