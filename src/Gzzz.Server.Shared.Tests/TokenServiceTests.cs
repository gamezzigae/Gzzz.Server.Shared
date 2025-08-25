using Gzzz.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using static Gzzz.Services.TokenService;

namespace Gzzz.Server.Shared.Tests;

public class TokenServiceTests
{

	static string CreateRandomString()
	{
		Span<byte> bytes = stackalloc byte[15];
		RandomNumberGenerator.Fill(bytes);
		return Convert.ToBase64String(bytes);
	}
	readonly TokenService _tokenService = new();
	readonly TokenClaims _sampleClaims = new(111, 1, 2, CreateRandomString());

	[Test]
	public void temp()
	{

		var token = _tokenService.Encode(_sampleClaims);
		var decodedClaims = _tokenService.Decode(token);

		Assert.That(decodedClaims, Is.Not.Null);
		Assert.That(decodedClaims.Type, Is.EqualTo(_sampleClaims.Type));
		Assert.That(decodedClaims.CreatedAt, Is.EqualTo(_sampleClaims.CreatedAt));
		Assert.That(decodedClaims.ExpireAt, Is.EqualTo(_sampleClaims.ExpireAt));
		Assert.That(decodedClaims.UserId, Is.EqualTo(_sampleClaims.UserId));
	}
}
