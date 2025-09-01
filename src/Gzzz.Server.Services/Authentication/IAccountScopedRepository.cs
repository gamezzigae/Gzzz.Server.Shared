using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Gzzz.Authentication;

public interface IAccountScopedRepository
{
	Task<AuthenticationResult> ValidateClaimsAsync(TokenClaims claims);
	Task SetTokensAsync(string accessToken, string refreshToken);
}

public class DefaultAccountScopedRepository : IAccountScopedRepository
{
	public Task SetTokensAsync(string accessToken, string refreshToken)
	{
		using var stream = new MemoryStream();
		using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
		{
			writer.WriteStartObject();                  
			writer.WriteString("subject", "trace");
			writer.WriteString("event", "DefaultAccountScopedRepository.SetTokensAsync");
			writer.WriteString("accessToken", accessToken);
			writer.WriteString("refreshToken", refreshToken);
			writer.WriteEndObject();                   
		}

		var json = Encoding.UTF8.GetString(stream.ToArray());
		Console.WriteLine(	json);
		return Task.CompletedTask;
	}

	public Task<AuthenticationResult> ValidateClaimsAsync(TokenClaims claims)
	{
		using var stream = new MemoryStream();
		using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
		{
			writer.WriteStartObject();
			writer.WriteString("subject", "trace");
			writer.WriteString("event", "DefaultAccountScopedRepository.ValidateClaimsAsync");
			writer.WriteString("claims.userid", claims.UserId);
			writer.WriteString("claims.type", ((TokenType)claims.Type).ToString());
			writer.WriteString("claims.type", claims.ExpireAt.ToISO8601());
			writer.WriteEndObject();
		}

		var json = Encoding.UTF8.GetString(stream.ToArray());
		Console.WriteLine(json);
		return AuthenticationResult.SuccessTask;
	}
}
