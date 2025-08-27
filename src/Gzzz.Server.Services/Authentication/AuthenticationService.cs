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

	public string CreateAccessToken(string userId, DateTime createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Access, createdAt.AddMinutes(_authenticationConfig.AccessTokenLIfetime), userId);
		return _tokenService.CreateToken(claims);
	}

	public string CreateRefreshToken(string userId, DateTime createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.Refresh, createdAt.AddMinutes(_authenticationConfig.RefreshTokenLifetime), userId);
		return _tokenService.CreateToken(claims);
	}

	public virtual Task<AuthenticationResult> ValidateAccessTokenAsync(string token, ApiContext context, IServiceProvider services)
	{
		if (string.IsNullOrEmpty(token))
			return AuthenticationResult.MissingTokenTask;

		if (_tokenService.VerifyToken(token, out var claims)==false)
			return AuthenticationResult.InvalidTokenTask;

		if (claims.Type != (byte)TokenType.Access)
			return AuthenticationResult.InvalidTokenTypeTask;

		context.UserId = claims.UserId;

		var now = context.RequestTime;

		if(now > claims.ExpireAt)
			return AuthenticationResult.ExpiredTokenTask;

		var accountRepository = services.GetRequiredService<IAccountScopedRepository>();
		return accountRepository.ValidateClaimsAsync(claims);
	}
}

public class  AuthenticationConfig
{
	public uint AccessTokenLIfetime { get; set; }
	public uint RefreshTokenLifetime { get; set; }
	public string HashKey { get; set; }
}

public class AuthenticationResult
{
	public static readonly AuthenticationResult Success = new() { IsSuccess = true };
	public static readonly AuthenticationResult MissingToken = new() { IsSuccess = false, ErrorMessage = "missing" };
	public static readonly AuthenticationResult InvalidToken = new() { IsSuccess = false, ErrorMessage = "Invalid" };
	public static readonly AuthenticationResult InvalidTokenType = new() { IsSuccess = false, ErrorMessage = "invalid type" };
	public static readonly AuthenticationResult ExpiredToken = new() { IsSuccess = false, ErrorMessage = "expired" };

	public static readonly Task<AuthenticationResult> SuccessTask = Task.FromResult(Success);
	public static readonly Task<AuthenticationResult> MissingTokenTask = Task.FromResult(MissingToken);
	public static readonly Task<AuthenticationResult> InvalidTokenTask = Task.FromResult(InvalidToken);
	public static readonly Task<AuthenticationResult> InvalidTokenTypeTask = Task.FromResult(InvalidTokenType);
	public static readonly Task<AuthenticationResult> ExpiredTokenTask = Task.FromResult(ExpiredToken);

	public bool IsSuccess { get; set; }
	public string ErrorMessage { get; set; }
}

