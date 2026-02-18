using Gzzz.CommandInvoker;
using Gzzz.Serialize;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker.Controllers;

[Controller]
public class MeController
{
	readonly RequestInfo _requestInfo;

	public MeController(RequestInfo requestInfo)
	{
		_requestInfo = requestInfo;
	}

	[AnonymousCommand("/__me__")]
	public Task<RequestInfo> GetRequestInfoAsync()
	{
		var result = new RequestInfo()
		{
			Ip = _requestInfo.Ip,
			UserId = _requestInfo.UserId,
			RequestTime = _requestInfo.RequestTime,	
		};

		return Task.FromResult(result);
	}
}
