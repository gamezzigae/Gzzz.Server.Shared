using Newtonsoft.Json.Linq;

public static class AssertX
{
    static readonly JsonDiffPatchDotNet.JsonDiffPatch _jsonDiffPatch= new ();
	public static void JsonEquals(object expected, object actual)
	{
		var expectedJObject = JObject.FromObject(expected);
		var actualJObject = JObject.FromObject(actual);

		var diff = _jsonDiffPatch.Diff(expectedJObject, actualJObject);

		Assert.Null(diff);
	}

	public static void JsonNotEquals(object expected, object actual)
	{
		var expectedJObject = JObject.FromObject(expected);
		var actualJObject = JObject.FromObject(actual);

		var diff = _jsonDiffPatch.Diff(expectedJObject, actualJObject);

		Assert.NotNull(diff);
	}
}
