using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker.Services;
public class JsonLogger
{
	readonly JsonSerializerOptions _jsonSerializerOptions;

	public JsonLogger(JsonSerializerOptions jsonSerializerOptions)
	{
		_jsonSerializerOptions = jsonSerializerOptions;
	}

	public virtual void Write(string message)
	{
		Amazon.Lambda.Core.LambdaLogger.Log(message);
	}
	public void Write(object obj)
	{
		var json = JsonSerializer.Serialize(obj, _jsonSerializerOptions);
		this.Write(json);
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
