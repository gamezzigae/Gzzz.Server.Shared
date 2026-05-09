using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.CommandInvoker;
using Gzzz.Services.Authentication;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Gzzz.Controllers;

[Controller("/s")]
public class SignController
{
	readonly TokenService _authenticationService;
	readonly IUserAuthenticatedInfoUpdater _userAuthenticatedInfoUpdater;
	readonly ApiContext _apiContext;

	public SignController(TokenService authenticationService, IUserAuthenticatedInfoUpdater userAuthenciatedInfoUpdater, ApiContext apiContext)
	{
		_authenticationService = authenticationService;
		_userAuthenticatedInfoUpdater = userAuthenciatedInfoUpdater;
		_apiContext = apiContext;
	}

	async Task<AuthenticationTokens> CreateTokensAsync(string userId)
	{
		var accessToken = _authenticationService.CreateAccessToken(userId, _apiContext.RequestTime);
		var refreshToken = _authenticationService.CreateRefreshToken(userId, _apiContext.RequestTime);

		await _userAuthenticatedInfoUpdater.UpdateAuthenticatedInfoAsync(userId, _apiContext.RequestTime);

		return new (userId, accessToken, refreshToken);
	}

	[AnonymousCommand("/_____impersonate_____")]
	public Task<AuthenticationTokens> ImpersonateSignInAsync([FromService] InternalIpService internalIpService, string userId)
	{
		if(internalIpService.IsAllowed(_apiContext.Ip) == false)
		{
			throw new HttpException(404, "xx");
		}
		return CreateTokensAsync(userId);
	}


	[AnonymousCommand("/gst", LoggingType.TrimResponseBody)]
	public Task<AuthenticationTokens> GuestSignInAsync()
	{
		var result = CreateTokensAsync(RandomX.CreateRandomBase64String(21));

		return result;
	}


	[AnonymousCommand("/rtkn", LoggingType.TrimResponseBody)]
	public async Task<AuthenticationTokens> SignInByRefreshTokenAsync(string refreshToken)
	{
		var authenticationResult = _authenticationService.ValidateToken(TokenType.RefreshTokenV1, refreshToken, _apiContext, out _);
		if (authenticationResult.IsSuccess == false)
		{
			throw new HttpException(401, string.Empty, (int)authenticationResult.ErrorCode);
		}
		
		return await CreateTokensAsync(_apiContext.UserId);
	}
}
public record AuthenticationTokens(
	[property: JsonPropertyName("uid")] string UserId,
	[property: JsonPropertyName("atkn")] string AccessToken,
	[property: JsonPropertyName("rtkn")] string RefreshToken
);
