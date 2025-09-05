namespace Gzzz.Server.Services.Tests;
public class ApiContextTests
{
	ApiContext NewContext()=> new ApiContext()
	{
		RequestModel = new object(),
		ResponseModel = new object(),
	};

	[Test]
	public void TrimRequestBodyTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.TrimRequestBody);

		Assert.That(context.RequestModel, Is.Null);
		Assert.That(context.ResponseModel, Is.Not.Null);
		Assert.That(context.SkipLogging, Is.False);
	}

	[Test]
	public void TrimResponseBodyTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.TrimResponseBody);

		Assert.That(context.RequestModel, Is.Not.Null);
		Assert.That(context.ResponseModel, Is.Null);
		Assert.That(context.SkipLogging, Is.False);
	}
	[Test]
	public void SimpleLogTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.Simple);

		Assert.That(context.RequestModel, Is.Null);
		Assert.That(context.ResponseModel, Is.Null);
		Assert.That(context.SkipLogging, Is.False);
	}


	[Test]
	public void SkipLoggingTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.Ignored);

		Assert.That(context.SkipLogging, Is.True);
	}
	[Test]
	public void DetailedTest()
	{
		var context = NewContext();

		context.TrimSuccess(LoggingType.Detailed);

		Assert.That(context.RequestModel, Is.Not.Null);
		Assert.That(context.ResponseModel, Is.Not.Null);
		Assert.That(context.SkipLogging, Is.False);
	}
}
