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
	readonly TokenServiceConfig _authenticationConfig;
	readonly TokenService _tokenService;
	readonly DateTime _now = DateTime.Now;
	readonly string _userId = RandomX.CreateRandomBase64String(18);
	public AuthenticationServiceTests()
	{
		var services = new ServiceCollection()
			.AddSingleton(new TokenServiceConfig() { AccessTokenLifetime = TimeSpan.FromSeconds(15), RefreshTokenLifetime = TimeSpan.FromSeconds(255), HashKey = RandomX.CreateRandomBase64String(256) })
			.AddSingleton<TokenService>()
			.BuildWithValidation();

		_authenticationConfig = services.GetRequiredService<TokenServiceConfig>();
		_tokenService = services.GetRequiredService<TokenService>();
	}

	[Fact]
	public async Task CreateAccessTokenAndVerifyOkTestAsync()
	{
		var accessToken = _tokenService.CreateAccessToken(_userId, _now);
		Assert.True(_tokenService.DecodeToken(accessToken, out var claims));
		Assert.Equal(((byte)TokenType.AccessTokenV1), claims.Type);
		Assert.Equal(claims.UserId, _userId);
		Assert.Equal(claims.CreatedAt, _now);

		var context = new ApiContext() { RequestTime = claims.CreatedAt }; //만료시간 딱 맞춰서
		var result = _tokenService.ValidateToken(TokenType.AccessTokenV1, accessToken, context, out _);
		Assert.Equal(context.UserId, _userId);
	}

	[Fact]
	public async Task ExpiredTokenTestAsync()
	{
		var accessToken = _tokenService.CreateAccessToken(_userId, _now);
		Assert.True(_tokenService.DecodeToken(accessToken, out var claims));

		//1ms만 늦어도 exception
		var context = new ApiContext() { RequestTime = claims.CreatedAt.Add(_authenticationConfig.AccessTokenLifetime).AddMicroseconds(1) };
		var result = _tokenService.ValidateToken(TokenType.AccessTokenV1, accessToken, context, out _);
		Assert.False(result.IsSuccess);
		Assert.Equal("expired", result.ErrorMessage);
		Assert.Equal(context.UserId,_userId);
	}
	[Fact]
	public void InvalidTokenTest()
	{
		var accessToken = _tokenService.CreateAccessToken(_userId, _now) + "=";
		Assert.False(_tokenService.DecodeToken(accessToken, out var claims));
	}

	[Fact]
	public async Task InvalidTokenTypeTestAsync()
	{
		var refreshToken = _tokenService.CreateRefreshToken(_userId, _now);

		var context = new ApiContext() { RequestTime = _now };
		var result =_tokenService.ValidateToken(TokenType.AccessTokenV1, refreshToken, context, out _); //다른토큰이라 안됨
		Assert.Equal(_userId, context.UserId); //타입이 달라도 userId는 뽑아내긴 함
		Assert.Equal("mismatch type", result.ErrorMessage);
	}
}
