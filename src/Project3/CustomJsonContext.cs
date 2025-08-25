using Gzzz;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(FunctionUrlRequest))]
	[JsonSerializable(typeof(FunctionUrlResponse))]
	[JsonSerializable(typeof(Test))]

	public partial class CustomJsonContext : JsonSerializerContext
	{
	}

