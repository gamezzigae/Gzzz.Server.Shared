using System.Text.Json.Serialization;

namespace Gzzz.Client;

public interface IApiClient
{
	Task<T> RequestAsync<T>(string path, ApiOption apiOption= ApiOption.Authenticated, object requestBody = null);

	public AuthenticationTokens AuthenticationTokens { get; set; }

	public string GetAccessToken() => AuthenticationTokens?.AccessToken ?? throw new Exception("invalid authentication token");

	public async Task<AuthenticationTokens> GuestSignInAsync()
	{
		AuthenticationTokens = await RequestAsync<AuthenticationTokens>("/s/gst", ApiOption.Anonymous);
		return AuthenticationTokens;
	}
	public async Task<AuthenticationTokens> RefreshTokensAsync()
	{
		AuthenticationTokens = await RequestAsync<AuthenticationTokens>("/s/rtkn", ApiOption.Anonymous, AuthenticationTokens.RefreshToken);
		return AuthenticationTokens;
	}
}
public enum ApiOption
{
	Anonymous,
	Authenticated,
	Idempotency,
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
