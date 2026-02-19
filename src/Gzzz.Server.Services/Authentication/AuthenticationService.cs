using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gzzz.Authentication;



public class TokenServiceConfig
{
	public static readonly string EnvironmentVariableName = "ZZ_AUTHENTICATION_CONFIG";

	[JsonRequired] public TimeSpan AccessTokenLifetime { get; set; }
	[JsonRequired] public TimeSpan RefreshTokenLifetime { get; set; }
	[JsonRequired] public string HashKey { get; set; }
}

public class DecodeTokenResult
{
	public DecodeTokenResult(bool success, UnauthorizedErrorCode errorCode)
	{
		IsSuccess = success;
		ErrorCode = errorCode;
	}
	public bool IsSuccess { get; }
	public UnauthorizedErrorCode ErrorCode { get; }

	public static readonly DecodeTokenResult Success = new DecodeTokenResult(true, 0);
	public static readonly DecodeTokenResult NotPresent = new DecodeTokenResult(false, UnauthorizedErrorCode.NotPresent);
	public static readonly DecodeTokenResult DecodeFail = new DecodeTokenResult(false, UnauthorizedErrorCode.DecodeFail);
	public static readonly DecodeTokenResult MismatchType = new DecodeTokenResult(false, UnauthorizedErrorCode.MismatchType);
	public static readonly DecodeTokenResult ExpiredToken = new DecodeTokenResult(false, UnauthorizedErrorCode.ExpiredToken);
}

public enum UnauthorizedErrorCode
{
	None,
	NotPresent = 401_1,
	DecodeFail = 401_2,
	MismatchType = 401_3,
	ExpiredToken = 401_4,
	DiscardedAuthentication = 401_5
}
