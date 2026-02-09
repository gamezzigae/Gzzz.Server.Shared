using Gzzz;
using Gzzz.CommandInvoker;
using Gzzz.Db.DynamoDb;
using System.Diagnostics.CodeAnalysis;

namespace Project1.Controllers;

[Controller("dynamodb")]
public class DynamoDbController
{
	readonly DynamoDbService _dynamoDbService;
	readonly ApiContext _apiContext;

	public DynamoDbController(DynamoDbService dynamoDbService, ApiContext apiContext)
	{
		_dynamoDbService = dynamoDbService;
		_apiContext = apiContext;
	}

	[AnonymousCommand("/putitem")]
	public async Task PutItemAsync(PutItemRequest request)
	{
		var attributeMap = AttributeMap.ConvertFrom(request);
		await _dynamoDbService.PutItemAsync(attributeMap, _apiContext.RequestTime);
	}
}
