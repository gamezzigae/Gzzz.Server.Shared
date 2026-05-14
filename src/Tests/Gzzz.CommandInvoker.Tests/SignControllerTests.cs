using Gzzz.Authentication;

namespace Gzzz.CommandInvoker.Tests;
public class SignControllerTests : AwsFunctionUrlInvokerFixture
{
	readonly IApiClient _client;
	readonly MockApiClient _mockClient;
	public SignControllerTests(ITestOutputHelper testLogger) : base(testLogger)
	{
		_mockClient = (MockApiClient)CreateEmptyClient();
		_client = _mockClient;
	}

	[Fact]
	public async Task ImpersonateSignInTestAsync()
	{
		var userId = RandomX.GetRandomText(64);
		var client =await CreateSignedClientAsync(userId);
		
		Assert.Equal(client.AuthenticationTokens.UserId, userId);
	}
	[Fact]
	public async Task GuestSignInTestAsync()
	{
		var response = await _client.GuestSignInAsync();

		Assert.Equal(response.AccessToken, _client.AuthenticationTokens.AccessToken);
		Assert.Equal(response.RefreshToken, _client.AuthenticationTokens.RefreshToken);

		var apiLog = _mockJsonLogger.DequeueApiLog();
		Assert.Null(apiLog.ResponseModel);
	}

	

	[Fact]
	public async Task RefreshSignTestAsync()
	{
		await GuestSignInTestAsync();
		var beforeAccessToken = _client.AuthenticationTokens.AccessToken;
		var beforeRefreshToken = _client.AuthenticationTokens.RefreshToken;
		var newTokens = await _client.RefreshTokensAsync();


		Assert.NotEqual(newTokens.AccessToken, beforeAccessToken);
		Assert.NotEqual(newTokens.RefreshToken, beforeRefreshToken);

		Assert.Equal(newTokens.AccessToken, _client.GetAccessToken());
		Assert.Equal(newTokens.RefreshToken,_client.AuthenticationTokens.RefreshToken);
	}
	[Fact]
	public async Task ExpiredRefreshSignErrorTestAsync()
	{
		await GuestSignInTestAsync();
		var beforeAccessToken = _client.AuthenticationTokens.AccessToken;
		var beforeRefreshToken = _client.AuthenticationTokens.RefreshToken;
		
		_mockTimeService.SetNow(DateTime.MaxValue); //좀 더 정밀한 시간을 넣어주는게 좋다
		
		var exception= await Assert.ThrowsAsync<HttpException>(_client.RefreshTokensAsync);
		Assert.Equal(401, exception.StatusCode); //401은 인증성공시
		Assert.Equal((int)UnauthorizedErrorCode.ExpiredToken, exception.ErrorCode);
	}
}
