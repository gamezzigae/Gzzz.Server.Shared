using Amazon.DynamoDBv2.Model;
using Gzzz;
using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Serializer;
using Gzzz.Controllers;
using Gzzz.Db.DynamoDb;
using Gzzz.Serialize;
using Gzzz.Services.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Project1.Controllers;
using System.Text.Json;
using System.Text.Json.Serialization;

DefaultConfig.Initialize(CustomJsonContext.Default.Options);

await new FunctionHandler(
	assemblies: [typeof(TestController).Assembly, typeof(SignController).Assembly],
	services => services
		.AddSingleton<IContextSerializer, JsonContextSerializer>()
		.AddSingleton<IUserAuthenciatedInfoUpdater, DynamoDbAuthenciatedInfoUpdater>()
		.AddSingleton<IUserRepository, DynamoDbUserRepositoryBase>()
		.AddDynamoDbService()
		.AddAwsFallbackCredentials()
	)
	.RunAsync();



[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, AttributeValue>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(DynamoDbConfig))]
//
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(ApiContext))]
//
[JsonSerializable(typeof(TokenServiceConfig))]
[JsonSerializable(typeof(AuthenticationTokens))]

public partial class CustomJsonContext : JsonSerializerContext
{
}

