using Gzzz.AwsFunctionUrlInvoker.Services;
using Gzzz.Serialize;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker.Test;

public class MockJsonLogger : JsonLogger
{

	public readonly Queue<string> Queue = new Queue<string>();

	public override void Write(string message)
	{
		Queue.Enqueue(message);
	}

	public ApiContext DequeueApiLog()
	{
		var log = Queue.Dequeue();
		return Json.Deserialize<ApiContext>(log)!;
	}
}
