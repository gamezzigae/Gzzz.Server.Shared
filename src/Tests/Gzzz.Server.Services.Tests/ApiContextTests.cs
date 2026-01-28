namespace Gzzz.Server.Services.Tests;
public class ApiContextTests
{
	ApiContext NewContext()=> new ApiContext()
	{
		RequestModel = new object(),
		ResponseModel = new object(),
	};

	[Fact]
	public void TrimRequestBodyTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.TrimRequestBody);

		Assert.Null(context.RequestModel);
		Assert.NotNull(context.ResponseModel);
		Assert.False(context.SkipLogging);
	}

	[Fact]
	public void TrimResponseBodyTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.TrimResponseBody);

		Assert.NotNull(context.RequestModel);
		Assert.Null(context.ResponseModel);
		Assert.False(context.SkipLogging);
	}
	[Fact]
	public void SimpleLogTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.Simple);

		Assert.Null(context.RequestModel);
		Assert.Null(context.ResponseModel);
		Assert.False(context.SkipLogging);
	}


	[Fact]
	public void SkipLoggingTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.Ignored);

		Assert.True(context.SkipLogging);
	}
	[Fact]
	public void DetailedTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.Detailed);

		Assert.NotNull(context.RequestModel);
		Assert.NotNull(context.ResponseModel);
		Assert.False(context.SkipLogging);
	}
}
