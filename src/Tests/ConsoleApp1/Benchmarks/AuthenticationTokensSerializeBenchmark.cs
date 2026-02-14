using BenchmarkDotNet.Attributes;
using Gzzz;
using Gzzz.Authentication;
using Gzzz.Controllers;
using Gzzz.Serialize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp1.Benchmarks;

[MemoryDiagnoser]
public class AuthenticationTokensSerializeBenchmark
{
	AuthenticationTokens _tokens = new AuthenticationTokens()
	{
		UserId = RandomX.CreateRandomBase64String(64),
		AccessToken= RandomX.CreateRandomBase64String(128),
		RefreshToken = RandomX.CreateRandomBase64String(128),
	};

	[Benchmark]
	public void JsonSerializerTest()
	{
		_= JsonSerializer.Serialize(_tokens);
	}
	[Benchmark]
	public void JsonWriterTest()
	{
		using var stream = new MemoryStream();
		using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
		{
			writer.WriteStartObject();
			writer.WriteString("uid", _tokens.UserId);
			writer.WriteString("atkn", _tokens.AccessToken);
			writer.WriteString("rtkn", _tokens.RefreshToken);
			writer.WriteEndObject();
		}
		_=Encoding.UTF8.GetString(stream.ToArray());
	}

	[Benchmark]
	public void StringBuilderTest()
	{
		_=string.Format("{{\"uid\":\"{0}\",\"atkn\": \"{1}\",\"rtkn\": \"{2}\"}}",_tokens.UserId, _tokens.AccessToken, _tokens.RefreshToken);
	}
}
