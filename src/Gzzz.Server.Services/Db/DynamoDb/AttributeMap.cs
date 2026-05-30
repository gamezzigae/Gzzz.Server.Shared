using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Gzzz.Serialize;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Gzzz.Db.DynamoDb;

[RequiresUnreferencedCode("Types might be removed")]
public static class AttributeMap
{

	public static Dictionary<string, AttributeValue> New(string partitionKey, string sortKey, DateTimeOffset now)
	{
		var result = new Dictionary<string, AttributeValue>();

		result.Add(DynamoDbKeys.PartitionKey, new AttributeValue(partitionKey));
		result.Add(DynamoDbKeys.SortKey, new AttributeValue(sortKey));
		result.Add(DynamoDbKeys.UpdatedAt, new AttributeValue() { N = now.ToLong().ToString() });
		result.Add(DynamoDbKeys.AuthenticatedAt, new AttributeValue() { N = now.ToLong().ToString() });

		return result;
	}

	


	public static Dictionary<string, AttributeValue> ConvertFrom<T>(T item) => Document.FromJson(Json.Serialize(item)).ToAttributeMap();

	public static T ConvertTo<T>(Dictionary<string, AttributeValue> attributeMap)
	{
		if (attributeMap==default)
			return default;

		var json = Document.FromAttributeMap(attributeMap).ToJson();
		return Json.Deserialize<T>(json);
	}

	public static long ToLong(this DateTimeOffset dateTimeOffset) => dateTimeOffset.ToUniversalTime().ToUnixTimeMilliseconds();
}
