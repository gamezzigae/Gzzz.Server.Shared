using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Gzzz;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Project2;

public class Function
{
	private static async Task Main()
	{
		await LambdaBootstrapBuilder
			.Create< FunctionUrlRequest, FunctionUrlResponse>(FunctionHandler, new SourceGeneratorLambdaJsonSerializer<FunctionUrlJsonContext>())
			.Build()
			.RunAsync();
	}

	public static FunctionUrlResponse FunctionHandler(FunctionUrlRequest input)
	{
		var apiContext = new ApiContext()
		{
			Path = input.RequestContext.Http.Path,
			Ip = input.RequestContext.Http.SourceIp,
			RequestTime = DateTime.UtcNow,
		};

		LambdaLogger.Log($"ApiContext: {JsonSerializer.Serialize(apiContext, LambdaFunctionJsonSerializerContext.Default.ApiContext)}");
		return new FunctionUrlResponse()
		{
			StatusCode = 200,
			Body = $"Hello, you requested path: {input.RequestContext.Http.Path}",
			Headers = new Dictionary<string, string>()
		};
	}
}

[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(ApiContext))]

[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(JsonDocument))]
//
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
	// By using this partial class derived from JsonSerializerContext, we can generate reflection free JSON Serializer code at compile time
	// which can deserialize our class and properties. However, we must attribute this class to tell it what types to generate serialization code for.
	// See https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-source-generation
}
