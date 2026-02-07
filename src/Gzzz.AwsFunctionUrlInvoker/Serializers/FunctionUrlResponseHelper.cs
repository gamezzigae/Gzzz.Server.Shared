using Amazon.Runtime.Internal.Transform;
using Gzzz.AwsFunctionUrlInvoker.Models;
using System.Diagnostics.CodeAnalysis;

namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
[RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
public static class FunctionUrlResponseHelper
{

	static readonly KeyValuePair<string, string> _contentTypeHeader = new KeyValuePair<string, string>("Content-Type", "application/json");

	public static FunctionUrlResponse Success(string body)
	{
		return new FunctionUrlResponse
		{
			StatusCode = 200,
			Body = body,
			Headers = new Dictionary<string, string>
			{
				{ _contentTypeHeader }
			},
		};
	}

	public static FunctionUrlResponse Error(int statusCode, string message, string body, int errorCode)
	{
		var result = new FunctionUrlResponse
		{
			StatusCode = statusCode,
			Headers = new Dictionary<string, string>
			{
				{ _contentTypeHeader },
				{ "zz-em", message },
			},
			Body = body
		};

		if (errorCode > 0)
		{
			result.Headers.Add("zz-ec", errorCode.ToString());
		}
		return result;
	}
	public static FunctionUrlResponse Error(int statusCode)
	{
		var result = new FunctionUrlResponse
		{
			StatusCode = statusCode,
			Headers = new Dictionary<string, string>
			{
				{ _contentTypeHeader },
			},
		};

		return result;
	}

}
