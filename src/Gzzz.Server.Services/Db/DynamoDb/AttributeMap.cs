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
	public static AttributeValue CreateTimestamp(double timestamp)=>new (){ N = timestamp.ToString() };
    
    public static Dictionary<string, AttributeValue> AddKeys(this Dictionary<string, AttributeValue> attributeMap, string partitionKey, string sortKey)
	{
        attributeMap.Add("PK", new AttributeValue(partitionKey));
		attributeMap.Add("SK", new AttributeValue(sortKey));
		return attributeMap;
    }

    public static Dictionary<string, AttributeValue> CreateKeys(string partitionKey, string sortKey) => new() { { "PK", new(partitionKey) }, { "SK", new(sortKey) } };


	public static Dictionary<string, AttributeValue> ConvertFrom<T>(T item) => Document.FromJson(Json.Serialize(item)).ToAttributeMap();

	public static T ConvertTo<T>(Dictionary<string, AttributeValue> attributeMap)
	{
		if (attributeMap==default)
			return default;

		var json = Document.FromAttributeMap(attributeMap).ToJson();
		return Json.Deserialize<T>(json);
	}
}
