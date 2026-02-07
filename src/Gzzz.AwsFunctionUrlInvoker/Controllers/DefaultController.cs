using Gzzz.CommandInvoker;
using Gzzz.Serialize;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gzzz.AwsFunctionUrlInvoker.Controllers;

[Controller]
public class DefaultController
{

	[AnonymousCommand("/__version__")]
	public async Task<JsonDocument> GetVersionAsync()
	{
		var json = await File.ReadAllTextAsync("version.json", DefaultConfig.Encoding);
		JsonDocument result = JsonSerializer.Deserialize<JsonDocument>(json, JsonDocumentContext.Default.Options);
		return result;
	}
}

[JsonSerializable(typeof(JsonDocument))]
public partial class JsonDocumentContext : JsonSerializerContext
{
}
