using BenchmarkDotNet.Attributes;
using Gzzz.CommandInvoker;
using System.Reflection;

[MemoryDiagnoser]
public class DispatchBenchmark
{
	static readonly DispatchBenchmark _instance = new();
	readonly MethodInfo _methodInfo;
	readonly Invoke<object> _fastInvoker1;
	readonly MethodInvoker _methodInvoker;
	readonly object _parameter = new object();
	public DispatchBenchmark()
	{
		this._methodInfo = typeof(DispatchBenchmark).GetMethod("Test", BindingFlags.NonPublic | BindingFlags.Instance);
		this._methodInvoker = MethodInvoker.Create(this._methodInfo);
		this._fastInvoker1 = FastInvoker.Create<object>(this._methodInfo);
	}


	/*
	 
| Method               | Mean      | Error     | StdDev    | Gen0   | Allocated |
|--------------------- |----------:|----------:|----------:|-------:|----------:|
| MethodInfoInvoke2    | 15.550 ns | 0.2668 ns | 0.2495 ns | 0.0038 |      32 B |
| NativeMethodInvoker2 |  6.988 ns | 0.1719 ns | 0.3143 ns |      - |         - |
| FastInvokerInvoke2   |  4.522 ns | 0.1377 ns | 0.3555 ns | 0.0038 |      32 B |

	 
	 */


	//[Benchmark] public int DirectCall() => _instance.Test(_parameter);
	[Benchmark] public object MethodInfoInvoke2() => _methodInfo.Invoke(_instance, [_parameter]);
	[Benchmark] public object NativeMethodInvoker2() => _methodInvoker.Invoke(_instance, _parameter);
	[Benchmark] public object FastInvokerInvoke2() => _fastInvoker1(_instance, [_parameter]);


	object Test(object p)
	{
		return p;
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
