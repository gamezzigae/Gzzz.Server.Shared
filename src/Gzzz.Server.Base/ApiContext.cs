using System.Text.Json.Serialization;

namespace Gzzz;

public class ApiContext
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("cold")]
	public bool IsColdStart { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("memory_size")]
	public int MemorySize { get; set; }

	[JsonPropertyName("subject")]
	public string Subject { get; } = "API";

	[JsonPropertyName("ip")]
	public string Ip { get; set; }

	[JsonPropertyName("t")]
	public DateTime RequestTime { get; set; }

	[JsonPropertyName("path")]
	public string Path { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("uid")]
	public string UserId { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("req")]
	public object RequestModel { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("res")]
	public object ResponseModel { get; set; }

	[JsonPropertyName("status")]
	public int Status { get; set; }

	[JsonPropertyName("elapsed")]
	public int Elapsed { get; set; }


	//

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("reqRaw")]
	public object RequestRaw { get; set; }

	//
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("ecode")]
	public int ErrorCode { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("emsg")]
	public string ErrorMessage { get; set; }

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("exception")]
	public string Exception { get; set; }


	[JsonIgnore]
	public bool SkipLogging { get; set; }

	public void TrimSuccess(LoggingType loggingType)
	{
		switch (loggingType)
		{
			case LoggingType.Ignored:
				SkipLogging = true;
				return;
			case LoggingType.Simple:
				RequestModel = default;
				ResponseModel = default;
				return;
			case LoggingType.IgnoreRequestBody:
				RequestModel = default;
				return;
			case LoggingType.IgnoreResponseBody:
				ResponseModel = default;
				return;
			default:
				return;
		}
	}

}
