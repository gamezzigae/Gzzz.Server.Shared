namespace Gzzz.Db.DynamoDb;

public class DynamoDbConfig
{
	public static readonly string EnvironmentVariableName = "ZZ_DYNAMODB_CONFIG";
	
	public string TableName { get; init; }
	public string ServiceURL { get; init; }
	public string ReturnConsumedCapacity { get; init; }
}
