using Gzzz.Serialize;
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
