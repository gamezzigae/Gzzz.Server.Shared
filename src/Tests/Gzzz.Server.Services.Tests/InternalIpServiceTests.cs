using Gzzz;

namespace Gzzz.Server.Shared.Tests;

public class InternalIpServiceTests
{
	[Fact]
	public void IsAllowedTest()
	{
		EnvironmentX.SetValue(InternalIpService.EnvironmentVariableName, "127.0.0.1, 10.0.0.1");
		var service = new InternalIpService();

		Assert.True(service.IsAllowed("127.0.0.1"));
		Assert.True(service.IsAllowed("10.0.0.1"));
		Assert.False(service.IsAllowed("10.0.0.2"));
		Assert.False(service.IsAllowed("invalid"));
	}
}
