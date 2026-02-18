using Amazon.DynamoDBv2.Model;
using BenchmarkDotNet.Attributes;
using Gzzz;
using Gzzz.Db.DynamoDb;
using Gzzz.Serialize;

[MemoryDiagnoser]
public class RedisDynamoDbAttributeSerializeBenchmark
{

	readonly Dictionary<string, AttributeValue> _attributeMap;
	readonly byte[] _bytes = new byte[10000];
	public RedisDynamoDbAttributeSerializeBenchmark()
	{
		_attributeMap = AttributeMap.New("User", RandomX.GetRandomText(), DateTimeOffset.UtcNow);
		for(int i =0;i<100;i++)
		{
			_attributeMap.Add("k" + i, new AttributeValue(RandomX.GetRandomText()));
			_attributeMap.Add("kk" + i, new AttributeValue() { N = RandomX.GetRandom().ToString() });
		}
		//Random.Shared.NextBytes(_bytes);
		
	}


	[Benchmark]
	public int ToRedisValue1()
	{
		var jsonBytes = Json.SerializeBytes(_attributeMap);
		var compressed = Zstd.Compress(jsonBytes);
		return compressed.Length;
	}

	[Benchmark]
	public int ToRedisValue2()
	{
		var attributes = new Dictionary<string, AttributeValue>();

		foreach (var (key, value) in _attributeMap)
		{
			if (key == DynamoDbKeys.PartitionKey || key == DynamoDbKeys.SortKey || key == DynamoDbKeys.UpdatedAt)
				continue;

			attributes.Add(key, value);
		}

		var jsonBytes = Json.SerializeBytes(attributes);
		var compressed = Zstd.Compress(jsonBytes);
		return compressed.Length;
	}

}



/*
EnvironmentX.SetObject(DynamoDbConfig.EnvironmentVariableName, new DynamoDbConfig() { TableName = "TestTable", ReturnConsumedCapacity="TOTAL" });

var services = new ServiceCollection()
		.AddSingleton<IContextSerializer, JsonContextSerializer>()
		.AddDynamoDbService()
		.AddSingleton<DynamoDbService>()
		.AddSingleton<ITextLogger, JsonLogger>()
		.AddAwsFallbackCredentials()
		.BuildWithValidation();

var ddb = services.GetRequiredService<DynamoDbService>();

var pk = "KEY-KJFA";
var sk1 = "Data-MemoryStream";
var sk2 = "Data-Base64";
var map = new Dictionary<string, AttributeValue>();
//var now = new DateTimeOffset(2026, 2, 13, 0, 0, 0, TimeSpan.FromHours(9));


var length = int.Parse(args[0]);
var random = new Random(1000);
var bytes = new byte[length];
random.NextBytes(bytes);

Console.Write(sk1);
var item1 = await ddb.GetAsync(pk, sk1);
item1["Value"] = new AttributeValue() { B = new MemoryStream(bytes) };
await ddb.UpdateAsync(item1, DateTimeOffset.Now);


Console.Write(sk2);
var item2 = await ddb.GetAsync(pk, sk2);
item2["Value"] = new AttributeValue() { S = Convert.ToBase64String(bytes) };
await ddb.UpdateAsync(item2, DateTimeOffset.Now);

//Console.WriteLine("bytes.Length:"+     bytes.Length);




//await ddb.InsertItemAsync(pk, sk1, map, now);

//var items = await ddb.GetAttirubtesAsync(pk, sk1);

//random.NextBytes(bytes);

////items["Value"].S = null;
////items["Value"] = new AttributeValue() { B = new MemoryStream(bytes) };

//items["Value"].B = null;
//items["Value"] = new AttributeValue() { S = Convert.ToBase64String(bytes) };
//await ddb.UpdateItemAsync(items, DateTimeOffset.Now);

//var temp = new JsonCompressBenchmark();
//Console.WriteLine(temp.Case1());
//Console.WriteLine(await temp.Case2());

//



Console.WriteLine(	);
*/
