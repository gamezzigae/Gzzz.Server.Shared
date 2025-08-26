using System.Text.Json.Serialization;

namespace Gzzz.AwsFunctionUrlInvoker.Models;

public struct FunctionUrlResponse
{
	[JsonPropertyName("statusCode")]
	public required int StatusCode { get; init; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("body")]
	public string Body { get; init; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("headers")]
	public required Dictionary<string, string> Headers { get; init; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("isBase64Encoded")]
	public bool IsBase64Encoded { get; init; }


}
