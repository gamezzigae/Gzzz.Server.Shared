namespace Gzzz.Db.Redis;

public interface ITextSerializer<T>
{
	T Deserialize(string serializedText);
	string Serialize(T item);
}

public class GZipJsonSerializer<T> : ITextSerializer<T>
{
	public string Serialize(T item) => GZipJson.SerializeToBase64(item);
	public T Deserialize(string serializedText) => GZipJson.DeserializeFromBase64<T>(serializedText);
}

public class JsonSerializer<T> : ITextSerializer<T>
{
	public string Serialize(T item) => Json.Serialize(item);
	public T Deserialize(string serializedText) => Json.Deserialize<T>(serializedText);
}
