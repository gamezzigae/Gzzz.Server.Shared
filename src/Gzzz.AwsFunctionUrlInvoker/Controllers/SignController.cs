using Gzzz.Authentication;
using Gzzz.CommandInvoker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gzzz.Controllers;

[Controller]
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

	async Task<SignResponse> CreateTokensAsync(string userId)
	{
		var accessToken = _authenticationService.CreateAccessToken(userId, _apiContext.RequestTime);
		var refreshToken = _authenticationService.CreateRefreshToken(userId, _apiContext.RequestTime);

		await _accountScopedRepository.SetTokensAsync(accessToken, refreshToken);
		return new SignResponse()
		{
			UserId = userId,
			AccessToken = accessToken,
			RefreshToken = refreshToken,
		};
	}

	[AnonymousCommand("/sign/guest")]	public Task<SignResponse> GuestSignInAsync()=>CreateTokensAsync(RandomX.CreateRandomBase64String(18));
	[AnonymousCommand("/sign/impersonate")]	public Task<SignResponse> ImpersonateSignInAsync(string userId)=>CreateTokensAsync(userId);


	[AnonymousCommand("/sign/refresh")]
	public async Task<SignResponse> SignInByRefreshTokenAsync(string refreshToken)
	{
		await _authenticationService.ValidateTokenAsync(TokenType.Refresh, refreshToken, _apiContext, _accountScopedRepository);
		return await CreateTokensAsync(_apiContext.UserId);
	}
}

public class SignResponse
{
	[JsonPropertyName("uid")]
	public string UserId { get; set; }

	[JsonPropertyName("atkn")]
	public string AccessToken { get; set; }

	[JsonPropertyName("rtkn")]
	public string RefreshToken { get; set; }
}
