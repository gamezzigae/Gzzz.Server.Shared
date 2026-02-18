

BenchmarkRunner.Run<JsonBenchmark2>();

[MemoryDiagnoser]
public class JsonBenchmark2
{
	readonly TokenClaims _item = new TokenClaims(9, DateTimeOffset.UtcNow, RandomX.GetRandomText());
	[Benchmark]
	public void WithObject()
	{
		Json.Serialize(_item);
	}
	[Benchmark]
	public void WithGeneric()
	{
		JsonSerializer.Serialize<TokenClaims>(_item, DefaultConfig.JsonSerializerOptions);
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
