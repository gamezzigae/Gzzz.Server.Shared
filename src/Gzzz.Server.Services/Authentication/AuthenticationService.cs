using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

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

	public string CreateAccessToken(string userId, DateTime createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Access, createdAt.AddMinutes(_authenticationConfig.AccessTokenLIfetime), userId);
		return _tokenService.EncodeToken(claims);
	}

	public string CreateRefreshToken(string userId, DateTime createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Refresh, createdAt.AddMinutes(_authenticationConfig.RefreshTokenLifetime), userId);
		return _tokenService.EncodeToken(claims);
	}


	public bool ValidateToken(TokenType tokenType, string token, ApiContext context, out string errorMessage)
	{
		if (string.IsNullOrEmpty(token))
		{
			errorMessage = "not presented";
			return false;
		}

		if (_tokenService.DecodeToken(token, out var claims) == false)
		{
			errorMessage = "Invalid";
			return false;
		}

		if (claims.Type != (byte)tokenType)
		{
			errorMessage= "invalid type";
			return false;
		}

		context.UserId = claims.UserId;

		var now = context.RequestTime;

		if (now > claims.ExpireAt)
		{
			errorMessage = "expired";
			return false;
		}

		errorMessage = null;
		return true;
	}
}

public class AuthenticationConfig
{
	public uint AccessTokenLIfetime { get; set; }
	public uint RefreshTokenLifetime { get; set; }
	public string HashKey { get; set; }
}
