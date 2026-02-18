using BenchmarkDotNet.Attributes;
using Gzzz;

[MemoryDiagnoser]
public class EnsureThat
{
	readonly string _referenceNotNull = RandomX.GetRandomText();
	readonly string _referenceNull = null;

	[Benchmark]
	public void Case1Null()
	{
		try
		{
			if (_referenceNull == default)
			{
				throw new Exception("not null");
			}
		}
		catch (Exception) { }
	}

	[Benchmark]
	public void Case2Null()
	{
		try
		{
			IsNotDefault(_referenceNull, static () => throw new Exception("not null"));
		}
		catch (Exception) { }

	}



	[Benchmark]
	public void Case3Null()
	{
		try
		{
			IsNotDefault(_referenceNull, static (item) => throw new Exception(item));
		}
		catch (Exception)
		{
		}
	}

	[Benchmark]
	public void Case4Null()
	{
		try
		{
			IsNotDefault(_referenceNull, static (item) => throw new Exception("error:" + item));
		}
		catch (Exception)
		{
		}
	}
	[Benchmark]
	public void Case5Null()
	{
		try
		{
			IsNotDefault(_referenceNull, () => throw new Exception("error:" + _referenceNull));
		}
		catch (Exception)
		{
		}
	}

	static void IsNotDefault<T>(T item, Action throwAction)
	{
		if (item.IsDefault())
		{
			throwAction();
		}
	}
	static void IsNotDefault<T>(T item, Action<T> throwAction)
	{
		if (item.IsDefault())
		{
			throwAction(item);
		}
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
