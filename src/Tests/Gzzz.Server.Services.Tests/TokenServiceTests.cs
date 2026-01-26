using Gzzz.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace Gzzz.Server.Shared.Tests;

//public class TokenServiceTests
//{
//	readonly TokenClaims _sampleClaims = new(111, 1, 2, RandomX.CreateRandomBase64String(15));

//	[Fact]
//	public void temp()
//	{
//		var hashkey = RandomX.CreateRandomBase64String(256);
//		TokenService tokenService = new TokenService(hashkey);
//		var token = tokenService.CreateToken(_sampleClaims);
//		Assert.That(tokenService.VerifyToken(token, out var decodedClaims), Is.True);

//		Assert.That(decodedClaims);
//		Assert.That(decodedClaims.Type, (_sampleClaims.Type));
//		Assert.That(decodedClaims.CreatedAt, (_sampleClaims.CreatedAt));
//		Assert.That(decodedClaims.Lifetime, (_sampleClaims.Lifetime));
//		Assert.That(decodedClaims.UserId, (_sampleClaims.UserId));
//	}
//}

public class AuthenticationServiceTests
{
	readonly AuthenticationService _authenticationService;
	readonly AuthenticationConfig _authenticationConfig;
	readonly TokenService _tokenService;
	readonly DateTime _now = DateTime.UtcNow;
	readonly string _userId = RandomX.CreateRandomBase64String(15);
	readonly IAccountScopedRepository _accountScopedRepository;
	public AuthenticationServiceTests()
	{
		var services = new ServiceCollection()
			.AddSingleton(new AuthenticationConfig() { AccessTokenLIfetime = 15, RefreshTokenLifetime = 255, HashKey = RandomX.CreateRandomBase64String(256) })
			.AddSingleton<TokenService>()
			.AddSingleton<AuthenticationService>()
			.AddSingleton<IAccountScopedRepository, DefaultAccountScopedRepository>()
			.ValidatedBuild();

		_authenticationService = services.GetRequiredService<AuthenticationService>();
		_authenticationConfig = services.GetRequiredService<AuthenticationConfig>();
		_tokenService = services.GetRequiredService<TokenService>();
		_accountScopedRepository= services.GetRequiredService<IAccountScopedRepository>();
	}

	[Fact]
	public async Task CreateAccessTokenAndVerifyOkTestAsync()
	{
		var accessToken = _authenticationService.CreateAccessToken(_userId, _now);
		Assert.True(_tokenService.VerifyToken(accessToken, out var claims));
		Assert.Equal(((byte)TokenType.Access), claims.Type);
		Assert.Equal(claims.UserId, (_userId));
		Assert.Equal(claims.ExpireAt, (_now.AddMinutes(_authenticationConfig.AccessTokenLIfetime)));

		var context = new ApiContext() { RequestTime = claims.ExpireAt }; //만료시간 딱 맞춰서
		await _authenticationService.ValidateTokenAsync(TokenType.Access, accessToken, context, _accountScopedRepository);
		Assert.Equal(context.UserId, (_userId));
	}

	[Fact]
	public async Task ExpiredTokenTestAsync()
	{
		var accessToken = _authenticationService.CreateAccessToken(_userId, _now);
		Assert.True(_tokenService.VerifyToken(accessToken, out var claims));

		//1ms만 늦어도 exception
		var context = new ApiContext() { RequestTime = claims.ExpireAt.AddMilliseconds(1) };
		var result = await _authenticationService.ValidateTokenAsync(TokenType.Access, accessToken, context, _accountScopedRepository);
		Assert.False(result.IsSuccess);
		Assert.Equal(result.ErrorMessage,AuthenticationResult.ExpiredToken.ErrorMessage);
		Assert.Equal(context.UserId,_userId);
	}
	[Fact]
	public void InvalidTokenTest()
	{
		var accessToken = _authenticationService.CreateAccessToken(_userId, _now) + "=";
		Assert.False(_tokenService.VerifyToken(accessToken, out var claims));
	}

	[Fact]
	public async Task InvalidTokenTypeTestAsync()
	{
		var refreshToken = _authenticationService.CreateRefreshToken(_userId, _now);

		var context = new ApiContext() { RequestTime = _now };
		var result = await _authenticationService.ValidateTokenAsync(TokenType.Access, refreshToken, context, _accountScopedRepository); //다른토큰이라 안됨
		Assert.Null(context.UserId);
		Assert.Equal(result.ErrorMessage, AuthenticationResult.InvalidTokenType.ErrorMessage);
	}
}
