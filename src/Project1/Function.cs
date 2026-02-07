using Gzzz;
using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Serializer;
using Gzzz.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Project1.Controllers;
using System.Text.Json;
using System.Text.Json.Serialization;

var jsonOptions = CustomJsonContext.Default.Options;

await new FunctionHandler(
	assemblies: [typeof(TestController).Assembly, typeof(SignController).Assembly],
	jsonSerializerOptions: jsonOptions,
	services => services
		.AddSingleton<IContextSerializer, JsonContextSerializer>()
	)
	.RunAsync();



[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(ApiContext))]
//
[JsonSerializable(typeof(AuthenticationConfig))]
[JsonSerializable(typeof(AuthenticationTokens))]
public partial class CustomJsonContext : JsonSerializerContext
{
}
