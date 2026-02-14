using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using static Gzzz.Authentication.AuthenticationService;

namespace Gzzz.Authentication;

public class AuthenticationService
{
	readonly AuthenticationConfig _authenticationConfig;
	readonly TokenService _tokenService;
	public AuthenticationService(AuthenticationConfig authenticationConfig, TokenService tokenService)
	{
		_authenticationConfig = authenticationConfig;
		_tokenService = tokenService;
	}

	public string CreateAccessToken(string userId, DateTimeOffset createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Access, createdAt.AddMinutes(_authenticationConfig.AccessTokenLIfetime), userId);
		return _tokenService.EncodeToken(claims);
	}

	public string CreateRefreshToken(string userId, DateTimeOffset createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Refresh, createdAt.AddMinutes(_authenticationConfig.RefreshTokenLifetime), userId);
		return _tokenService.EncodeToken(claims);
	}

	public virtual Task<ValidateTokenResult> ValidateTokenAsync(TokenType tokenType, string token, ApiContext context)
	{
		if (string.IsNullOrEmpty(token))
		{
			return ValidateTokenResult.NotPresent;
		}

		if (_tokenService.DecodeToken(token, out var claims) == false)
		{
			return ValidateTokenResult.DecodeFail;
		}

		if (claims.Type != (byte)tokenType)
		{
			return ValidateTokenResult.MismatchType;
		}

		context.UserId = claims.UserId;

		if (context.RequestTime > claims.ExpireAt)
		{
			return ValidateTokenResult.ExpiredToken;
		}

		return ValidateTokenResult.Success;
	}

}

public class AuthenticationConfig
{
	public static readonly string EnvironmentVariableName = "ZZ_AUTHENTICATION_CONFIG";
	public uint AccessTokenLIfetime { get; set; }
	public uint RefreshTokenLifetime { get; set; }
	public string HashKey { get; set; }
}

public class ValidateTokenResult
{
	public ValidateTokenResult(bool success, string errorMessage)
	{
		IsSuccess = success;
		ErrorMessage = errorMessage;
	}
	public bool IsSuccess { get; }
	public string ErrorMessage { get; }

	public static readonly Task<ValidateTokenResult> Success = Task.FromResult(new ValidateTokenResult(true, null));
	public static readonly Task<ValidateTokenResult> NotPresent = Task.FromResult(new ValidateTokenResult(false, "not present"));
	public static readonly Task<ValidateTokenResult> DecodeFail = Task.FromResult(new ValidateTokenResult(false, "decode fail"));
	public static readonly Task<ValidateTokenResult> MismatchType = Task.FromResult(new ValidateTokenResult(false, "mismatch type"));
	public static readonly Task<ValidateTokenResult> ExpiredToken = Task.FromResult(new ValidateTokenResult(false, "expired"));
}
