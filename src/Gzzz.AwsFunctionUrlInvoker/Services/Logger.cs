using Gzzz.Serialize;
using System.Buffers;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker.Services;
public class JsonLogger : ITextLogger
{
	public virtual void Write(string message)
	{
		Amazon.Lambda.Core.LambdaLogger.Log(message);
	}
	public void WriteObject(object obj)
	{
		var json = Json.Serialize(obj);
		this.Write(json);
	}

	public void Write<TState>(TState state, Action<Utf8JsonWriter, TState> action)
	{
		var buffer = new ArrayBufferWriter<byte>();
		using Utf8JsonWriter writer = new Utf8JsonWriter(buffer);
		writer.WriteStartObject();
		action(writer, state);
		writer.WriteEndObject();
		writer.Flush();

		//var stdout = Console.OpenStandardOutput();
		//stdout.Write(buffer.WrittenSpan);
	}

}

/*
 {
    "requestTime": "2025-08-18T16:00:49.4819233+00:00",
    "path": "/echo",
    "requestBody": "aaa",
    "ecode": 0,
    "emsg": "request body deserialize error",
    "statusCode": 400,
    "elapsed": 12
}
 */
