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


	[AnonymousCommand("/round-trip")]
	public async Task<JsonDocument> RoundTripAsync(PutItemRequest request)
	{
		var attributeMap = await _dynamoDbService.GetAsync(_partitionKey, request.Key);
		attributeMap["Value"].S = string.Empty.PadRight(request.Length, 'A');
		await _dynamoDbService.PutAsync(attributeMap, _apiContext.RequestTime);
		var items = AttributeMap.ConvertTo<JsonDocument>(attributeMap);
		return items;
	}
}

public class PutItemRequest
{
	public string Key { get; set; }
	public int Length { get; set; }
}
