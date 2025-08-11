using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Gzzz;
using System.Text;
using System.Text.Json.Serialization;

namespace Gostop1.Server;

public class Function
{
	/// <summary>
	/// The main entry point for the custom runtime.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	private static async Task Main()
	{
		await LambdaBootstrapBuilder.Create<FunctionUrlRequest>(StreamHandleAsync, new DefaultLambdaJsonSerializer())
			.Build()
			.RunAsync();
	}

	static readonly Encoding _encoding = new UTF8Encoding(false);
	static readonly GeminiReusableMemoryStream _reusableStream = new GeminiReusableMemoryStream(4096);
	static async Task<Stream> StreamHandleAsync(FunctionUrlRequest inputStream)
	{
		var json = Json.Serialize(inputStream);
		_reusableStream.Reset();
		_reusableStream.Write(_encoding.GetBytes(json));
		return _reusableStream;
	}
}


public sealed class FunctionUrlRequest
{
	[JsonPropertyName("headers")]
	public Dictionary<string, string> Headers { get; init; }

	[JsonPropertyName("isBase64Encoded")]
	public bool IsBase64Encoded { get; init; }

	[JsonPropertyName("rawPath")]
	public string RawPath { get; init; }

	[JsonPropertyName("requestContext")]
	public RequestContext RequestContext { get; init; }

	[JsonPropertyName("body")]
	public string Body { get; init; }
}

public class RequestContext
{
	[JsonPropertyName("http")]
	public FunctionUrlHttpContext Http { get; init; }
}

public partial class FunctionUrlHttpContext
{
	[JsonPropertyName("path")]
	public string Path { get; init; }

	[JsonPropertyName("protocol")]
	public string Protocol { get; init; }

	[JsonPropertyName("method")]
	public string Method { get; init; }

	[JsonPropertyName("sourceIp")]
	public string SourceIp { get; init; }

	[JsonPropertyName("userAgent")]
	public string UserAgent { get; init; }
}
