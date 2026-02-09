using Gzzz;
using Gzzz.CommandInvoker;
using Gzzz.Db.DynamoDb;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Amazon.DynamoDBv2.Model;

namespace Project1.Controllers;

[Controller("dynamodb")]
public class DynamoDbController
{
	readonly DynamoDbService _dynamoDbService;
	readonly ApiContext _apiContext;
	static readonly string _partitionKey = "TEST";
	public DynamoDbController(DynamoDbService dynamoDbService, ApiContext apiContext)
	{
		_dynamoDbService = dynamoDbService;
		_apiContext = apiContext;
	}

	[AnonymousCommand("/putitem")]
	public async Task PutItemAsync(PutItemRequest request)
	{
		var attributeMap = new Dictionary<string, AttributeValue>()
			.AddKeys(_partitionKey, request.Key)
			.AddAttribute("Value", new AttributeValue(request.Value));

		await _dynamoDbService.PutItemAsync(attributeMap, _apiContext.RequestTime);
	}

	[AnonymousCommand("/getitem")]
	public async Task<JsonDocument> GetItemAsync(PutItemRequest request)
	{
		var attributeMap = await _dynamoDbService.GetAttirubtesAsync(_partitionKey, request.Key);
		var items = AttributeMap.ConvertTo<JsonDocument>(attributeMap);
		return items;
	}
}

public class PutItemRequest
{
	public string Key { get; set; }
	public string Value { get; set; }
}
