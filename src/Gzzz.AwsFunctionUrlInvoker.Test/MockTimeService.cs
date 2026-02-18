namespace Gzzz.AwsFunctionUrlInvoker.Test;

public class MockTimeService : TimeService
{
	DateTimeOffset? _now;
	
	public override DateTimeOffset GetNow()
	{
		if (_now.IsNotDefault())
			return _now.Value;
		return DateTime.UtcNow;
	}
	public DateTimeOffset SetNow(DateTimeOffset time)
	{
		_now = time;
		return time;
	}
	public DateTimeOffset SetNow() => this.SetNow(DateTimeOffset.UtcNow);
}
