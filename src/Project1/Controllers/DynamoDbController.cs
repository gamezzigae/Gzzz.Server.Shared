using Amazon.DynamoDBv2.Model;
using Gzzz;
using Gzzz.CommandInvoker;
using Gzzz.Db.DynamoDb;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

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
			.AddAttribute("Value", new AttributeValue(string.Empty.PadRight(request.Length,'A')));

		await _dynamoDbService.PutItemAsync(attributeMap, _apiContext.RequestTime);
	}

	[AnonymousCommand("/getitem")]
	public async Task<JsonDocument> GetItemAsync(PutItemRequest request)
	{
		var attributeMap = await _dynamoDbService.GetAttirubtesAsync(_partitionKey, request.Key);
		var items = AttributeMap.ConvertTo<JsonDocument>(attributeMap);
		return items;
	}

	[AnonymousCommand("/round-trip")]
	public async Task<JsonDocument> RoundTripAsync(PutItemRequest request)
	{
		var attributeMap = await _dynamoDbService.GetAttirubtesAsync(_partitionKey, request.Key);
		attributeMap["Value"].S = string.Empty.PadRight(request.Length, 'A');
		await _dynamoDbService.PutItemAsync(attributeMap, _apiContext.RequestTime, attributeMap.GetUpdatedAt());
		var items = AttributeMap.ConvertTo<JsonDocument>(attributeMap);
		return items;
	}
}

public class PutItemRequest
{
	public string Key { get; set; }
	public int Length { get; set; }
}
