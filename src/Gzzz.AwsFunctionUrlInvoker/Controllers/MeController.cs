using Amazon.DynamoDBv2.Model;
using Gzzz.CommandInvoker;
using Gzzz.Serialize;
using Gzzz.Services.Authentication;
using System.Text.Json;

namespace Gzzz.AwsFunctionUrlInvoker.Controllers;

[Controller]
public class MeController
{
	readonly IUserRepository _userRepository;

	public MeController(IUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	[Command("/__me__")]
	public Task<Dictionary<string, AttributeValue>> GetRequestInfoAsync()
	{
		return Task.FromResult(_userRepository.AttributeMap);
	}
}
