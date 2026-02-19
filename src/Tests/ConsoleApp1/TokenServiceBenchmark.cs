using BenchmarkDotNet.Attributes;
using Gzzz;
using Gzzz.Authentication;

[MemoryDiagnoser]
public class TokenServiceBenchmark
{
	readonly TokenClaims _tokenClaims1;
	readonly TokenService _tokenService1;
	readonly string _token1;
	readonly TokenClaims _tokenClaims2;
	readonly TokenService _tokenService2;
	readonly string _token2;

	public TokenServiceBenchmark()
	{
		var config = new TokenServiceConfig()
		{
			AccessTokenLIfetime = 30,
			RefreshTokenLifetime = 43200,
			HashKey = "46LNjT9Bol95r05aEI/TKUsC/u8VvM4gnTZkGnZSMdIYi6hgoAKCo6cdRoJwmva77wB8BPNkfsX5xODEjDv98F=="
		};

		var id = RandomX.GetRandomText();
		_tokenService1 = new(config);
		_tokenClaims1 = new((byte)TokenType.AccessTokenV1, DateTimeOffset.UtcNow, id);
		_token1 = _tokenService1.EncodeToken(_tokenClaims1);


		_tokenService2 = new(config);
		_tokenClaims2 = new((byte)TokenType.AccessTokenV1, DateTimeOffset.UtcNow, id);
		_token2 = _tokenService2.EncodeToken(_tokenClaims2);
	}

	[Benchmark]
	public int Encode1()
	{
		var token = _tokenService1.EncodeToken(_tokenClaims1);
		return token.Length;
	}
	[Benchmark]
	public int Encode2()
	{
		var token = _tokenService2.EncodeToken(_tokenClaims2);
		return token.Length;
	}
	[Benchmark]
	public void Decode1()
	{
		_tokenService1.DecodeToken(_token1, out _);
	}
	[Benchmark]
	public void Decode2()
	{
		_tokenService2.DecodeToken(_token2, out _);
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
