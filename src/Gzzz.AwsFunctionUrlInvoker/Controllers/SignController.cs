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
	readonly IUserAuthenciatedInfoUpdater _userAuthenciatedInfoUpdater;
	readonly ApiContext _apiContext;

	public SignController(TokenService authenticationService, IUserAuthenciatedInfoUpdater userAuthenciatedInfoUpdater, ApiContext apiContext)
	{
		_authenticationService = authenticationService;
		_userAuthenciatedInfoUpdater = userAuthenciatedInfoUpdater;
		_apiContext = apiContext;
	}

	async Task<AuthenticationTokens> CreateTokensAsync(string userId)
	{
		var accessToken = _authenticationService.CreateAccessToken(userId, _apiContext.RequestTime);
		var refreshToken = _authenticationService.CreateRefreshToken(userId, _apiContext.RequestTime);

		await _userAuthenciatedInfoUpdater.UpdateAuthenticatedInfoAsync(userId, _apiContext.RequestTime);

		return new (userId, accessToken, refreshToken);
	}

	[AnonymousCommand("/_____impersonate_____")]
	public Task<AuthenticationTokens> ImpersonateSignInAsync(string userId)
	{
		return CreateTokensAsync(userId);
	}


	[AnonymousCommand("/gst")]
	public Task<AuthenticationTokens> GuestSignInAsync()
	{
		var result = CreateTokensAsync(RandomX.CreateRandomBase64String(21));

		return result;
	}


	[AnonymousCommand("/rtkn")]
	public async Task<AuthenticationTokens> SignInByRefreshTokenAsync(string refreshToken)
	{
		var authenticationResult = _authenticationService.ValidateToken(TokenType.RefreshTokenV1, refreshToken, _apiContext, out _);
		if (authenticationResult.IsSuccess == false)
		{
			throw new HttpException(400, authenticationResult.ErrorMessage);
		}

		
		return await CreateTokensAsync(_apiContext.UserId);
	}
}
public record AuthenticationTokens(
	[property: JsonPropertyName("uid")] string UserId,
	[property: JsonPropertyName("atkn")] string AccessToken,
	[property: JsonPropertyName("rtkn")] string RefreshToken
);
