using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Runtime.Internal;
using Gzzz;
using Gzzz.CommandInvoker;
using System.Diagnostics.CodeAnalysis;
using System.Text;

public class Program
{
	/// <summary>
	/// The main entry point for the custom runtime.
	/// </summary>
	/// <param name="args">Command line arguments.</param>
	[RequiresUnreferencedCode("Calls Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer.DefaultLambdaJsonSerializer()")]
	private static async Task Main()
	{
		await LambdaBootstrapBuilder.Create(StreamHandleAsync)
			.Build()
			.RunAsync();
	}
	[RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
	static async Task<Stream> StreamHandleAsync(Stream requestStream, ILambdaContext lambdaContext)
	{
		var request = await System.Text.Json.JsonSerializer.DeserializeAsync(requestStream, CustomJsonContext.Default.FunctionUrlRequest);

		var obj = new Test("Hello", "World");

		var response = new FunctionUrlResponse()
		{
			StatusCode = 200,
			Headers = new Dictionary<string, string>
		{
			{ "Content-Type", "application/json" },
			{ "X-Program-Url-Response", "Hello from FunctionUrlResponse!" }
		},
			IsBase64Encoded = false,
			Body = System.Text.Json.JsonSerializer.Serialize(obj, CustomJsonContext.Default.Options)
		};

		var responseStream = new MemoryStream();
		await System.Text.Json.JsonSerializer.SerializeAsync(responseStream, response, CustomJsonContext.Default.FunctionUrlResponse);
		responseStream.Position = 0;
		return responseStream;
	}

}

public record Test(string A, string B);

