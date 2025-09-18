using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon;
using System.Diagnostics.CodeAnalysis;
using Gzzz.Serialize;

namespace Gzzz.Db.DynamoDb;

[RequiresUnreferencedCode("Types might be removed")]
public static class AttributeMap
{
	
    
    public static Dictionary<string, AttributeValue> AddKeys(this Dictionary<string, AttributeValue> attributeMap, string partitionKey, string sortKey)
	{
        attributeMap.Add(DynamoDbKeys.PartitionKey, new AttributeValue(partitionKey));
		attributeMap.Add(DynamoDbKeys.SortKey, new AttributeValue(sortKey));
		return attributeMap;
    }

    public static Dictionary<string, AttributeValue> CreateKeys(string partitionKey, string sortKey) => new() { { DynamoDbKeys.PartitionKey, new(partitionKey) }, { DynamoDbKeys.SortKey, new(sortKey) } };


	public static Dictionary<string, AttributeValue> ConvertFrom<T>(T item) => Document.FromJson(Json.Serialize(item)).ToAttributeMap();

	public static T ConvertTo<T>(Dictionary<string, AttributeValue> attributeMap)
	{
		if (attributeMap==default)
			return default;

		var json = Document.FromAttributeMap(attributeMap).ToJson();
		return Json.Deserialize<T>(json);
	}

	public static DateTimeOffset GetUpdatedAt(this Dictionary<string, AttributeValue> attributeMap)
	{
		var unixms = long.Parse(attributeMap[DynamoDbKeys.UpdatedAt].N);
		return DateTimeOffset.FromUnixTimeMilliseconds(unixms);
	}
	/// <summary>
	/// DateTimeOffset에서 milliseconds 미만 단위를 제거합니다.
	/// </summary>
	public static DateTimeOffset TrimBelowMilliseconds(this DateTimeOffset datetimeOffset)
	{
		// ticks = 100ns 단위
		long ticksPerMillisecond = TimeSpan.TicksPerMillisecond;
		long trimmedTicks = datetimeOffset.Ticks - (datetimeOffset.Ticks % ticksPerMillisecond);
		return new DateTimeOffset(trimmedTicks, datetimeOffset.Offset);
	}
}
