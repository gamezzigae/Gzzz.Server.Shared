using Amazon.DynamoDBv2.Model;
using Gzzz.Controllers;
using Gzzz.Db.DynamoDb;

BenchmarkRunner.Run<ApiLogBenchmark>();

[MemoryDiagnoser]
public class ApiLogBenchmark
{
	JsonLogger _jsonLogger = new JsonLogger();

	readonly ApiContext _success = new ApiContext()
	{
		API = RandomX.GetRandomText(),
		Duration = 10,
		Ip = RandomX.GetRandomText(),
		RequestModel = new Dictionary<string, object>
		{
			{ "key1", RandomX.GetRandomText()},
			{"key2", RandomX.GetRandomText()},
			{"key3", RandomX.GetRandomText()},
		},
		ResponseModel = new Dictionary<string, object>
		{
			{ "key1", RandomX.GetRandomText()},
			{"key2", RandomX.GetRandomText()},
			{"key3", RandomX.GetRandomText()},
		},
		RequestTime = DateTime.Now,
		Status = 200,
		UserId = RandomX.GetRandomText(),
	};



	/*
| Method            | Mean     | Error    | StdDev   | Median   | Gen0   | Gen1   | Allocated |
|------------------ |---------:|---------:|---------:|---------:|-------:|-------:|----------:|
| UseWriter         | 943.3 ns | 33.61 ns | 97.51 ns | 935.0 ns | 0.5865 | 0.0095 |    4.8 KB |
| UseSerializer     | 871.5 ns | 17.20 ns | 26.26 ns | 863.0 ns | 0.1421 |      - |   1.16 KB | 그냥 serializer가 빠르고 메모리도 덜 할당한다
| UseWriterToString | 949.3 ns | 18.86 ns | 54.70 ns | 932.6 ns | 0.6933 | 0.0076 |   5.66 KB |
	 */

	int _length = 100000;
	[Benchmark]
	public void UseSerializer1()
	{
		for (int i = 0; i < _length; i++)
		{
			Json.Serialize(_success);
			//_jsonLogger.WriteObject(_success);
		}
	}
	[Benchmark]
	public void UseSerializer2()
	{
		for (int i = 0; i < _length; i++)
		{
			JsonSerializer.Serialize(_success, CustomJsonContext.Default.Options);
			//_jsonLogger.WriteObject(_success);
		}
	}
}

[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, AttributeValue>))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(JsonDocument))]
[JsonSerializable(typeof(DynamoDbConfig))]
//
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(ApiContext))]
//
[JsonSerializable(typeof(TokenServiceConfig))]
[JsonSerializable(typeof(AuthenticationTokens))]
//
public partial class CustomJsonContext : JsonSerializerContext
{
}
