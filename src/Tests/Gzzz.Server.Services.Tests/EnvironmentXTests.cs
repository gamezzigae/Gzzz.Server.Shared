using System;
using System.Collections.Generic;
using Gzzz.Serialize;
using Xunit;

namespace Gzzz.Server.Shared.Tests;

public class EnvironmentXTests
{
	[Fact]
	public void GetRequiredValueNotExsistTest()
	{
		var key = RandomX.GetRandomText();

		var exception = Assert.Throws<Exception>(() => EnvironmentX.GetValue(key));
		Assert.Contains(":" + key, exception.Message);
	}

	[Fact]
	public void GetRequiredObjectNotExsistTest()
	{
		var key = RandomX.GetRandomText();

		var exception = Assert.Throws<Exception>(() => EnvironmentX.GetRequiredObject<object>(key));
		Assert.Contains(":" + key, exception.Message);
	}

	[Fact]
	public void GetRequiredObjectTest()
	{
		var key = RandomX.GetRandomText();
		var value = new { Name = RandomX.GetRandomText(), Age = "30", Text = RandomX.GetRandomText() };
		var json = Json.Serialize(value);

		EnvironmentX.SetValue(key, json);

		var retrievedItem = EnvironmentX.GetRequiredObject<Dictionary<string, string>>(key);
		Assert.Equal(3, retrievedItem.Count);
		Assert.Equal(value.Name, retrievedItem["Name"]);
		Assert.Equal(value.Age, retrievedItem["Age"]);
		Assert.Equal(value.Text, retrievedItem["Text"]);
	}

	[Fact]
	public void GetValueOrDefaultTest()
	{
		var key = RandomX.GetRandomText(10);
		var defaultValue = RandomX.GetRandomText(10);

		EnvironmentX.SetValue(key, defaultValue);

		var result = EnvironmentX.GetValueOrDefault(key, defaultValue);
		Assert.Equal(defaultValue, result);
	}
}
