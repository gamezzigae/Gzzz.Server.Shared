namespace Gzzz.Db;

public interface IOptimisticRepository<T>
{
	Task<OptimisticRecord<T>> GetItemOrDefaultAsync(string itemKey);
	Task<long> PutItemAsync(string sortKey, T item, DateTime now, long checkTimestamp = 0);
}
