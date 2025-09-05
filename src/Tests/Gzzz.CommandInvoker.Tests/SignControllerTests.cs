namespace Gzzz.CommandInvoker.Tests;
public class SignControllerTests : AwsFunctionUrlHandlerFixture
{
	readonly IApiClient _client;
	readonly MockApiClient _mockClient;

	public SignControllerTests()
	{
		_mockClient = (MockApiClient)CreateEmptyClient();
		_client = _mockClient;
	}

	[Test]
	public async Task ImpersonateSignInTestAsync()
	{
		var userId = RandomX.GetRandomText(64);
		var client =await CreateSignedClientAsync(userId);
		Assert.That(client.AuthenticationTokens.UserId, Is.EqualTo(userId));
	}
	[Test]
	public async Task GuestSignInTestAsync()
	{
		var response = await _client.GuestSignInAsync();

		Assert.That(response.AccessToken, Is.EqualTo(_client.AuthenticationTokens.AccessToken));
		Assert.That(response.RefreshToken, Is.EqualTo(_client.AuthenticationTokens.RefreshToken));
	}

	[Test]
	public async Task RefreshSignTestAsync()
	{
		await GuestSignInTestAsync();
		var beforeAccessToken = _client.AuthenticationTokens.AccessToken;
		var beforeRefreshToken = _client.AuthenticationTokens.RefreshToken;
		var newTokens = await _client.RefreshTokensAsync();

		Assert.That(newTokens.AccessToken, Is.Not.EqualTo(beforeAccessToken));
		Assert.That(newTokens.RefreshToken, Is.Not.EqualTo(beforeRefreshToken));

		Assert.That(newTokens.AccessToken, Is.EqualTo(_client.GetAccessToken()));
		Assert.That(newTokens.RefreshToken, Is.EqualTo(_client.AuthenticationTokens.RefreshToken));
	}
}
