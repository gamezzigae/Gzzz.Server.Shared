using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Gzzz.Authentication;



public class TokenServiceConfig
{
	public static readonly string EnvironmentVariableName = "ZZ_AUTHENTICATION_CONFIG";
	public uint AccessTokenLIfetime { get; set; }
	public uint RefreshTokenLifetime { get; set; }
	public string HashKey { get; set; }
}

public class DecodeTokenResult
{
	public DecodeTokenResult(bool success, string errorMessage)
	{
		IsSuccess = success;
		ErrorMessage = errorMessage;
	}
	public bool IsSuccess { get; }
	public string ErrorMessage { get; }

	public static readonly DecodeTokenResult Success = (new DecodeTokenResult(true, null));
	public static readonly DecodeTokenResult NotPresent = (new DecodeTokenResult(false, "not present"));
	public static readonly DecodeTokenResult DecodeFail = (new DecodeTokenResult(false, "decode fail"));
	public static readonly DecodeTokenResult MismatchType = (new DecodeTokenResult(false, "mismatch type"));
	public static readonly DecodeTokenResult ExpiredToken = (new DecodeTokenResult(false, "expired"));
}
