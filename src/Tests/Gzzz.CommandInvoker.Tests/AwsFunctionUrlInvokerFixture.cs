using Gzzz.Authentication;
using Gzzz.AwsFunctionUrlInvoker;
using Gzzz.AwsFunctionUrlInvoker.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Gzzz.CommandInvoker.Tests;

public class AwsFunctionUrlInvokerFixture : AwsFunctionUrlInvokerFixtureBase
{
	protected override Assembly[] GetAssemblies() => [typeof(TestController).Assembly, typeof(Gzzz.Controllers.SignController).Assembly];
	protected override void ConfigureServices(IServiceCollection services)
	{
	}

	public AwsFunctionUrlInvokerFixture(ITestOutputHelper testLogger) : base(testLogger)
	{
	}
}
