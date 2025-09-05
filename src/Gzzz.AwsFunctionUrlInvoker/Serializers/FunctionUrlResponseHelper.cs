using Gzzz.AwsFunctionUrlInvoker.Models;
using System.Diagnostics.CodeAnalysis;

namespace Gzzz.AwsFunctionUrlInvoker.Serializer;

[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
[RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
public static class FunctionUrlResponseHelper
{
	public static FunctionUrlResponse Success(string body)
	{
		return new FunctionUrlResponse
		{
			StatusCode = 200,
			Body = body,
			Headers = new Dictionary<string, string>
			{
				{ "Content-Type", "application/json" }
			},
		};
	}

	public static FunctionUrlResponse Error(int statusCode, string message, int errorCode)
	{
		var result = new FunctionUrlResponse
		{
			StatusCode = statusCode,
			Headers = new Dictionary<string, string>
			{
				{ "Content-Type", "application/json" },
				{ "zz-em", message },
			},
		};

		if(errorCode > 0)
		{
			result.Headers.Add("zz-ec", errorCode.ToString());
		}
		return result;
	}
	
}
