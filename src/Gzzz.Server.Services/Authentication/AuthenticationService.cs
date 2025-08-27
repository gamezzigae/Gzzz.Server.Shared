using Microsoft.Extensions.DependencyInjection;

namespace Gzzz.Services.Authentication;
public class AuthenticationService
{
	readonly AuthenticationConfig _authenticationConfig;
	readonly TokenService _tokenService;
	public AuthenticationService(AuthenticationConfig authenticationConfig, TokenService tokenService)
	{
		_authenticationConfig = authenticationConfig;
		_tokenService = tokenService;
	}

	public DateTime GetExpireAt(TokenType tokenType, DateTime createdAt)
	{
		byte lifetime = tokenType switch
		{
			TokenType.Access => _authenticationConfig.AccessTokenLIfetime,
			TokenType.Refresh => _authenticationConfig.RefreshTokenLifetime,
			_ => throw new ArgumentOutOfRangeException(nameof(tokenType), "invalid result type")
		};
		return createdAt.AddMinutes(lifetime * (byte)tokenType);
	}

	public string CreateAccessToken(string userId, DateTime createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Access, createdAt, _authenticationConfig.AccessTokenLIfetime, userId);
		return _tokenService.CreateToken(claims);
	}
	public string CreateRefreshToken(string userId, DateTime createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Refresh, createdAt, _authenticationConfig.RefreshTokenLifetime, userId);
		return _tokenService.CreateToken(claims);
	}

	public async Task ValidateAccessTokenAsync(string token, ApiContext context, IServiceProvider services)
	{
		if(string.IsNullOrEmpty(token))
			throw new HttpException(401, "missing tkn");

		if (_tokenService.VerifyToken(token, out var claims)==false)
			throw new HttpException(401, "Invalid tkn");

		if (claims.Type != (byte)TokenType.Access)
			throw new HttpException(401, "no acctkn");

		context.UserId = claims.UserId;

		var now = context.RequestTime;
		var expireAt = claims.GetExpireAt();

		if(now > expireAt)
			throw new HttpException(401, "expired acctkn");

		await this.ValidateClaims(claims, services);
	}

	public virtual Task ValidateClaims(TokenClaims cliams, IServiceProvider service)=> Task.CompletedTask;
}

public class  AuthenticationConfig
{
	public byte AccessTokenLIfetime { get; set; }
	public byte RefreshTokenLifetime { get; set; }
	public string HashKey { get; set; }
}
