using System.Text.Json.Serialization;

namespace Gzzz.AwsFunctionUrlInvoker.Models;

public class FunctionUrlRequest
{
	[JsonPropertyName("headers")]
	public required FunctionUrlHeaders Headers { get; init; }
	[JsonPropertyName("isBase64Encoded")]
	public bool IsBase64Encoded { get; init; }
	[JsonPropertyName("rawPath")]
	public required string RawPath { get; init; }
	[JsonPropertyName("requestContext")]
	public required RequestContext RequestContext { get; init; }
	[JsonPropertyName("body")]
	public string Body { get; init; }

	public string GetRequestBody()
	{
		if (IsBase64Encoded)
		{
			var bytes = Convert.FromBase64String(Body);
			return System.Text.Encoding.UTF8.GetString(bytes);
		}
		return Body;
	}
}

public class RequestContext
{
	[JsonPropertyName("http")]
	public required FunctionUrlHttpContext Http { get; init; }
}

public class FunctionUrlHttpContext
{
	[JsonPropertyName("path")] public required string Path { get; init; }
	[JsonPropertyName("sourceIp")] public required string SourceIp { get; init; }
}


public class FunctionUrlHeaders
{
	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	[JsonPropertyName("content-length")]
	public int ContentLength { get; init; }

	[JsonPropertyName("content-type")]
	public string ContentType { get; init; }

	[JsonPropertyName("zz-req-id")]
	public string RequestId { get; init; }

	[JsonPropertyName("zz-atkn")]
	public string AccessToken { get; init; }
}
