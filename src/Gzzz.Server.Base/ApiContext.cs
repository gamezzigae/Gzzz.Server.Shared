using System.Text.Json.Serialization;

namespace Gzzz;

public class RequestInfo
{
	[JsonPropertyOrder(100)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("uid")]
	public string UserId { get; set; }

	[JsonPropertyOrder(101)]
	[JsonPropertyName("ip")]
	public string Ip { get; set; }

	[JsonPropertyOrder(102)]
	[JsonPropertyName("t")]
	public DateTimeOffset RequestTime { get; set; }
}

public class ApiContext : RequestInfo
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("cold")]
	public bool IsColdStart { get; set; }


	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("reqPath")]
	public string RequestPath { get; set; }


	[JsonPropertyOrder(0)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("api")]
	public string API { get; set; }


	[JsonPropertyOrder(10)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("req")]
	public object RequestModel { get; set; }

	[JsonPropertyOrder(11)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("res")]
	public object ResponseModel { get; set; }

	[JsonPropertyOrder(1)]
	[JsonPropertyName("s")]
	public int Status { get; set; }

	[JsonPropertyOrder(103)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("d")]
	public int Duration { get; set; }


	//

	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("reqRaw")]
	public string RequestRaw { get; set; }

	//
	[JsonPropertyOrder(8)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("ecode")]
	public int ErrorCode { get; set; }


	[JsonPropertyOrder(9)]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	[JsonPropertyName("emsg")]
	public string ErrorMessage { get; set; }


	[JsonPropertyOrder(7)]
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
			case LoggingType.TrimRequestBody:
				RequestModel = default;
				return;
			case LoggingType.TrimResponseBody:
				ResponseModel = default;
				return;
			default:
				return;
		}
	}

}
