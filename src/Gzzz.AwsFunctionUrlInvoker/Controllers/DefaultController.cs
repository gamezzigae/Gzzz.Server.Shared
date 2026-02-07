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
	[AnonymousCommand("/__version__")]
	public Task<JsonDocument> GetVersionAsync()
	{
		var buffer = new ArrayBufferWriter<byte>();
		using Utf8JsonWriter jsonWriter = new Utf8JsonWriter(buffer);

		jsonWriter.WriteStartObject();
		jsonWriter.WriteString("entry", Assembly.GetEntryAssembly().ToString());
		jsonWriter.WriteString("executing", Assembly.GetExecutingAssembly().ToString());
		jsonWriter.WriteEndObject();
		jsonWriter.Flush();

		string json = DefaultConfig.Encoding.GetString(buffer.WrittenSpan);

		JsonDocument doc = JsonDocument.Parse(json);
		return Task.FromResult(doc);
	}

	[AnonymousCommand("/__422__")]
	public Task Exception422Async()
	{
		throw new HttpException(422, "This is a test exception for 422 status code.", 0);
	}


	[AnonymousCommand("/__500__")]
	public Task Exception500Async()
	{
		throw new HttpException(500, "This is a test exception for 500 status code.", 0);
	}

	[AnonymousCommand("/__ex__")]
	public Task UnhandledExceptionAsync()
	{
		throw new Exception("unhandled exception");
	}
	[AnonymousCommand("/__jex__")]
	public Task JsonExceptionAsync()
	{
		JsonSerializer.Deserialize<object>("this is not json");
		return Task.CompletedTask;
	}
}
