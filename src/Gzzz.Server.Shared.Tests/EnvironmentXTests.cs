namespace Gzzz.Server.Shared.Tests;

class EnvironmentXTests
{
    [Test]
    public void GetRequiredValueNotExsistTest()
    {
        var key = RandomX.GetRandomText();

        var exception = Assert.Throws<StartUpException>(() => EnvironmentX.GetRequiredValue(key), "예외가 발생해야 합니다.");
        Assert.That(exception.Message, Does.Contain(":"+key), "예외 메시지에 필수 환경변수 누락이 포함되어야 합니다.");
    }

    [Test]
    public void GetRequiredObjectNotExsistTest()
    {
        var key = RandomX.GetRandomText();

        var exception = Assert.Throws<StartUpException>(() => EnvironmentX.GetRequiredObject<object>(key), "예외가 발생해야 합니다.");
        Assert.That(exception.Message, Does.Contain(":" + key), "예외 메시지에 필수 환경변수 누락이 포함되어야 합니다.");
    }

    [Test]
    public void GetRequiredObjectTest()
    {
        var key = RandomX.GetRandomText();    
        var value = new { Name = RandomX.GetRandomText(), Age = 30, Text=RandomX.GetRandomText() };
        var json = Json.Serialize(value);

        EnvironmentX.SetProcessValue(key, json);

        var retrievedItem = EnvironmentX.GetRequiredObject<Dictionary<string,string>>(key);
        Assert.That(retrievedItem.Count, Is.EqualTo(3));
        Assert.That(retrievedItem["Name"], Is.EqualTo(value.Name), "객체의 Name 속성이 일치해야 합니다.");
        Assert.That(retrievedItem["Age"], Is.EqualTo(value.Age.ToString()), "객체의 Age 속성이 일치해야 합니다.");
        Assert.That(retrievedItem["Text"], Is.EqualTo(value.Text), "객체의 Text 속성이 일치해야 합니다.");
    }

    [Test]
    public void GetValueOrDefaultTest()
    {
        var key = RandomX.GetRandomText(10);
        var defaultValue = RandomX.GetRandomText(10);
        
        EnvironmentX.SetProcessValue(key, defaultValue);

        var result = EnvironmentX.GetValueOrDefault(key, defaultValue);   
        Assert.That(result, Is.EqualTo(defaultValue), "기본값이 반환되어야 합니다.");
    }
}