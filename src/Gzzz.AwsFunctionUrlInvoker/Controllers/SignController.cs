using Gzzz.Authentication;
using Gzzz.CommandInvoker;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Gzzz.Controllers;

[Controller("/s")]
public class SignController
{
	readonly AuthenticationService _authenticationService;
	readonly IAccountScopedRepository _accountScopedRepository;
	readonly ApiContext _apiContext;

	public SignController(AuthenticationService authenticationService, IAccountScopedRepository accountScopedRepository, ApiContext apiContext)
	{
		_authenticationService = authenticationService;
		_accountScopedRepository = accountScopedRepository;
		_apiContext = apiContext;
	}

	async Task<AuthenticationTokens> CreateTokensAsync(string userId)
	{
		var accessToken = _authenticationService.CreateAccessToken(userId, _apiContext.RequestTime);
		var refreshToken = _authenticationService.CreateRefreshToken(userId, _apiContext.RequestTime);

		await _accountScopedRepository.SetTokensAsync(accessToken, refreshToken);
		return new AuthenticationTokens()
		{
			UserId = userId,
			AccessToken = accessToken,
			RefreshToken = refreshToken,
		};
	}

	[AnonymousCommand("/_____impersonate_____")]
	public Task<AuthenticationTokens> ImpersonateSignInAsync(string userId)
	{
		return CreateTokensAsync(userId);
	}


	[AnonymousCommand("/gst")]	public Task<AuthenticationTokens> GuestSignInAsync()=>CreateTokensAsync(RandomX.CreateRandomBase64String(18));


	[AnonymousCommand("/rtkn")]
	public async Task<AuthenticationTokens> SignInByRefreshTokenAsync(string refreshToken)
	{
		var authenticationResult = await _authenticationService.ValidateTokenAsync(TokenType.Refresh, refreshToken, _apiContext);
		if (authenticationResult.IsSuccess == false)
		{
			throw new HttpException(400, authenticationResult.ErrorMessage);
		}

		return await CreateTokensAsync(_apiContext.UserId);
	}
}

public class AuthenticationTokens
{
	[JsonPropertyName("uid")]
	public string UserId { get; set; }

	[JsonPropertyName("atkn")]
	public string AccessToken { get; set; }

	[JsonPropertyName("rtkn")]
	public string RefreshToken { get; set; }
}
