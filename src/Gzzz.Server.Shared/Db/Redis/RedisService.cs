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

public record RedisConfig(string Endpoint, string User=null, string Password = null);
