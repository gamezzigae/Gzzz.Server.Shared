using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Gzzz.Serialize;
using Gzzz.Server.Shared.Tests.DbTests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Xunit.Sdk;

namespace Gzzz.Server.Services.Tests.DbTests;

public class DynamoDbServiceTests : DynamoDbFixture
{
	readonly string _pk = RandomX.GetRandomText();
	readonly string _sk = RandomX.GetRandomText();
	readonly DateTimeOffset _now = DateTimeOffset.UtcNow;
	[Fact]
	public async Task InsertItemTestAsync()
	{
		var map = AttributeMap.New(_pk, _sk, _now);
		await _dynamoDbService.InsertAsync(map);

		var retrieved = await _dynamoDbService.GetAsync(_pk, _sk);
		Assert.NotNull(retrieved);
	}


	[Fact]
	public async Task InsertDuplicatedItemErrorAsync()
	{
		var map = AttributeMap.New(_pk, _sk, _now);
		await _dynamoDbService.InsertAsync(map);
		var exception = await Assert.ThrowsAsync<HttpException>(() => _dynamoDbService.InsertAsync(map));

		Assert.Equal("condition error", exception.Message);
	}

	[Fact]
	public async Task PutItemTestAsync()
	{
		await InsertItemTestAsync();
		var item = await _dynamoDbService.GetAsync(_pk, _sk);
		Assert.NotNull(item);

		var newKey = RandomX.GetRandomText();
		Assert.False(item.ContainsKey(newKey));

		var newValue = RandomX.GetRandomText();

		item.Add(newKey, new AttributeValue(newValue));

		await _dynamoDbService.PutAsync(item, _now.AddMilliseconds(1));
		var retrieved = await _dynamoDbService.GetAsync(_pk, _sk);
		Assert.True(retrieved.TryGetValue(newKey, out var value));
		Assert.Equal(newValue, value.S);
		Assert.Equal(_now.AddMilliseconds(1).ToUnixTimeMilliseconds().ToString(), retrieved[DynamoDbKeys.UpdatedAt].N);
	}

	[Fact]
	public async Task UpdateNotExistsItemErrorAsync()
	{
		var item = AttributeMap.New(_pk, _sk, _now);
		var exception = await Assert.ThrowsAsync<HttpException>(() => _dynamoDbService.PutAsync(item, _now.AddMilliseconds(1)));

		Assert.Equal("condition error", exception.Message);
	}


	[Theory]
	[InlineData(0)]
	[InlineData(-1000)]
	[InlineData(999)] //milliseconds 단위를 벗어나지 못함
	public async Task UpdateItemUpdatedAtErrorAsync(int microSeconds)
	{
		var now = DateTimeOffset.FromUnixTimeMilliseconds(DateTime.UtcNow.ToUnixTimeMilliseconds());
		var map = AttributeMap.New(_pk, _sk, now);
		await _dynamoDbService.InsertAsync(map);
		var item = await _dynamoDbService.GetAsync(_pk, _sk);

		var exception = await Assert.ThrowsAsync<ArgumentException>(() => _dynamoDbService.PutAsync(item, now.AddMicroseconds(microSeconds)));
		Assert.Equal("dynamodb update item time condition error", exception.Message);
	}

	[Fact]
	public async Task PartialUpdateTestAsync()
	{
		var a = RandomX.GetRandomText();
		var b = RandomX.GetRandomText();

		var map = AttributeMap.New(_pk, _sk, _now);
		map["a"] = new AttributeValue(a);
		map["b"] = new AttributeValue(b);

		await _dynamoDbService.InsertAsync(map);
		{
			var retrieved = await _dynamoDbService.GetAsync(_pk, _sk);
			Assert.NotNull(retrieved);

			Assert.Equal(a, retrieved["a"].S);
			Assert.Equal(b, retrieved["b"].S);
		}
		map["a"].S = a + b;
		map.Remove(b);
		await _dynamoDbService.UpdateItemAsync(map, _now.AddMilliseconds(1));

		{
			var retrieved = await _dynamoDbService.GetAsync(_pk, _sk);
			Assert.NotNull(retrieved);

			Assert.Equal(a + b, retrieved["a"].S);
			Assert.Equal(b, retrieved["b"].S);
		}


	}
}
