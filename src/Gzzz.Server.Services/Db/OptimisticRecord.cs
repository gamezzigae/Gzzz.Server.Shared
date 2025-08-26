namespace Gzzz.Db;

public record OptimisticRecord<T>(
    string Key,
    T Value,
    long Timestamp, // Unix timestamp in milliseconds
    bool IsFromCache
);
