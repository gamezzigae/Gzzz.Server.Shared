using StackExchange.Redis;
namespace Gzzz.Db.Redis;

public sealed class RedisService
{
	readonly ConnectionMultiplexer _connectionMultiplexer;

	public RedisService(RedisConfig redisConfig)
	{
		_connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig.Endpoint, option=>
		{
			option.User = redisConfig.User;
			option.Password = redisConfig.Password;
		});
	}

	public IDatabase GetDatabase() => _connectionMultiplexer.GetDatabase();
	public ITransaction CreateTransaction() => GetDatabase().CreateTransaction();
}

public class RedisConfig
{
	public static readonly string EnvironmentVariableName = "ZZ_REDIS_CONFIG";
	public string Endpoint { get; set; }
	public string User { get; set; }
	public string Password { get; set; }
}
