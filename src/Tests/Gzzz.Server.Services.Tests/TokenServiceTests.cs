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
		Assert.That(claims.Lifetime, Is.EqualTo(_authenticationConfig.AccessTokenLIfetime));
		Assert.That(claims.CreatedAt, Is.EqualTo(_now));

		var context = new ApiContext() { RequestTime = claims.GetExpireAt() }; //만료시간 딱 맞춰서
		await _authenticationService.ValidateAccessTokenAsync(accessToken, context, _services);
		Assert.That(context.UserId, Is.EqualTo(_userId));
	}

	[Test]
	public void CreateAccessTokenAndExpiredTest()
	{
		var accessToken = _authenticationService.CreateAccessToken(_userId, _now);
		Assert.That(_tokenService.VerifyToken(accessToken, out var claims), Is.True);

		//1ms만 늦어도 exception
		var context = new ApiContext() { RequestTime = claims.GetExpireAt().AddMilliseconds(1) };
		var exception = Assert.ThrowsAsync<HttpException>(() => _authenticationService.ValidateAccessTokenAsync(accessToken, context, _services));
		Assert.That(context.UserId, Is.EqualTo(_userId));
	}

	[Test]
	public void CreateRefreshTokenAndVerifyFailTest()
	{
		var refreshToken = _authenticationService.CreateRefreshToken(_userId, _now);
		Assert.That(_tokenService.VerifyToken(refreshToken, out var claims), Is.True);
		Assert.That(claims.Type, Is.EqualTo((byte)TokenType.Refresh));
		Assert.That(claims.UserId, Is.EqualTo(_userId));
		Assert.That(claims.Lifetime, Is.EqualTo(_authenticationConfig.RefreshTokenLifetime));
		Assert.That(claims.CreatedAt, Is.EqualTo(_now));

		var context = new ApiContext() { RequestTime = _now };
		var exception = Assert.ThrowsAsync<HttpException>(() => _authenticationService.ValidateAccessTokenAsync(refreshToken, context, _services));
		Assert.That(context.UserId, Is.Null);
		Assert.That(exception.StatusCode, Is.EqualTo(401));
		Assert.That(exception.Message, Is.EqualTo("no acctkn"));
	}
}
