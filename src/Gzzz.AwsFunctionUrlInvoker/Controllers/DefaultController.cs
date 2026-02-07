using Gzzz.CommandInvoker;
using Gzzz.Serialize;
using System;
using System.Buffers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gzzz.AwsFunctionUrlInvoker.Controllers;

[Controller]
public class DefaultController
{
	readonly ApiContext _apiContext;

	public DefaultController(ApiContext apiContext)
	{
		_apiContext = apiContext;
	}
	[AnonymousCommand("/__version__")]
	public Task<JsonDocument> GetVersionAsync()
	{
		var buffer = new ArrayBufferWriter<byte>();
		using Utf8JsonWriter jsonWriter= new Utf8JsonWriter(buffer);

		jsonWriter.WriteStartObject();
		jsonWriter.WriteString("entry", Assembly.GetEntryAssembly().ToString());
		jsonWriter.WriteString("executing", Assembly.GetExecutingAssembly().ToString());
		jsonWriter.WriteEndObject();
		jsonWriter.Flush();
		
		string json = DefaultConfig.Encoding.GetString(buffer.WrittenSpan);
		_apiContext.ResponseModel = json;

		JsonDocument doc = JsonDocument.Parse(json);
		return Task.FromResult(doc);
	}
}

[JsonSerializable(typeof(JsonDocument))]
public partial class JsonDocumentContext : JsonSerializerContext
{
}
