using Gzzz.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace Gzzz.Server.Shared.Tests;

//public class TokenServiceTests
//{
//	readonly TokenClaims _sampleClaims = new(111, 1, 2, RandomX.CreateRandomBase64String(15));

//	[Test]
//	public void temp()
//	{
//		var hashkey = RandomX.CreateRandomBase64String(256);
//		TokenService tokenService = new TokenService(hashkey);
//		var token = tokenService.CreateToken(_sampleClaims);
//		Assert.That(tokenService.VerifyToken(token, out var decodedClaims), Is.True);

//		Assert.That(decodedClaims, Is.Not.Null);
//		Assert.That(decodedClaims.Type, Is.EqualTo(_sampleClaims.Type));
//		Assert.That(decodedClaims.CreatedAt, Is.EqualTo(_sampleClaims.CreatedAt));
//		Assert.That(decodedClaims.Lifetime, Is.EqualTo(_sampleClaims.Lifetime));
//		Assert.That(decodedClaims.UserId, Is.EqualTo(_sampleClaims.UserId));
//	}
//}

public class AuthenticationServiceTests
{
	readonly AuthenticationService _authenticationService;
	readonly AuthenticationConfig _authenticationConfig;
	readonly TokenService _tokenService;
	readonly DateTime _now = DateTime.UtcNow;
	readonly string _userId = RandomX.CreateRandomBase64String(15);
	readonly IServiceProvider _services;
	public AuthenticationServiceTests()
	{
		_services = new ServiceCollection()
			.AddSingleton(new AuthenticationConfig() { AccessTokenLIfetime = 15, RefreshTokenLifetime = 255, HashKey = RandomX.CreateRandomBase64String(256) })
			.AddSingleton<TokenService>()
			.AddSingleton<AuthenticationService>()
			.ValidatedBuild();

		_authenticationService = _services.GetRequiredService<AuthenticationService>();
		_authenticationConfig = _services.GetRequiredService<AuthenticationConfig>();
		_tokenService = _services.GetRequiredService<TokenService>();
	}

	[Test]
	public async Task CreateAccessTokenAndVerifyOkTestAsync()
	{
		var accessToken = _authenticationService.CreateAccessToken(_userId, _now);
		Assert.That(_tokenService.VerifyToken(accessToken, out var claims), Is.True);
		Assert.That(claims.Type, Is.EqualTo((byte)TokenType.Access));
		Assert.That(claims.UserId, Is.EqualTo(_userId));
		Assert.That(claims.ExpireAt, Is.EqualTo(_now.AddMinutes(_authenticationConfig.AccessTokenLIfetime)));

		var context = new ApiContext() { RequestTime = claims.ExpireAt }; //만료시간 딱 맞춰서
		await _authenticationService.ValidateAccessTokenAsync(accessToken, context, _services);
		Assert.That(context.UserId, Is.EqualTo(_userId));
	}

	[Test]
	public async Task CreateAccessTokenAndExpiredTestAsync()
	{
		var accessToken = _authenticationService.CreateAccessToken(_userId, _now);
		Assert.That(_tokenService.VerifyToken(accessToken, out var claims), Is.True);

		//1ms만 늦어도 exception
		var context = new ApiContext() { RequestTime = claims.ExpireAt.AddMilliseconds(1) };
		var result = await _authenticationService.ValidateAccessTokenAsync(accessToken, context, _services);
		Assert.That(result.IsSuccess, Is.False);
		Assert.That(result.ErrorMessage, Is.EqualTo(AuthenticationResult.ExpiredToken.ErrorMessage));
		Assert.That(context.UserId, Is.EqualTo(_userId));
	}

	[Test]
	public async Task CreateRefreshTokenAndVerifyFailTestAsync()
	{
		var refreshToken = _authenticationService.CreateRefreshToken(_userId, _now);
		Assert.That(_tokenService.VerifyToken(refreshToken, out var claims), Is.True);
		Assert.That(claims.Type, Is.EqualTo((byte)TokenType.Refresh));
		Assert.That(claims.UserId, Is.EqualTo(_userId));
		Assert.That(claims.ExpireAt, Is.EqualTo(_now.AddMinutes(_authenticationConfig.RefreshTokenLifetime)));

		var context = new ApiContext() { RequestTime = _now };
		var result = await _authenticationService.ValidateAccessTokenAsync(refreshToken, context, _services);
		Assert.That(context.UserId, Is.Null);
		Assert.That(result.ErrorMessage, Is.EqualTo(AuthenticationResult.InvalidTokenType.ErrorMessage));
	}
}
