using Gzzz.CommandInvoker;
using Gzzz.Services.Authentication;
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

	[AnonymousCommand("/sign/guest")]
	public async Task<SignResponse> GuestSignInAsync()
	{
		var userId = RandomX.CreateRandomBase64String(18);
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
