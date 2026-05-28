namespace Gzzz.Db.DynamoDb;

public static class DynamoDbKeys
{
	public static readonly string PartitionKey = "PK";
	public static readonly string SortKey = "SK";
	public static readonly string UpdatedAt = "UA";
	public static readonly string AuthenticatedAt = "AA";
	public static readonly string LastRequestId = "LRID";
	public static readonly string LastIdempotencyResponse= "LRES";
}

public static class DynamoDbTable
{
	public static readonly string User = "User";

}
