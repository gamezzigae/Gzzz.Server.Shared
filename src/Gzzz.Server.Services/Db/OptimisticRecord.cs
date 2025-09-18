namespace Gzzz.Db;

public record OptimisticRecord<T>(
    string Key,
    T Value,
	DateTimeOffset UpdatedAt, // Unix timestamp in milliseconds
    bool IsFromCache
);
