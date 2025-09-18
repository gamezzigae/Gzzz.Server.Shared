namespace Gzzz.Db;

public interface IOptimisticRepository<T>
{
	Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string itemKey);
	Task PutItemAsync(string sortKey, T item, DateTimeOffset now, DateTimeOffset lastUpdatedAt = default);
}
