using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Gzzz.Authentication;

public interface IAccountScopedRepository
{
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
}
