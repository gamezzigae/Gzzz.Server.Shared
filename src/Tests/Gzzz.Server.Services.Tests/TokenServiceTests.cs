using Gzzz.Services;
using System.Security.Cryptography;

namespace Gzzz.Server.Shared.Tests;

public class TokenServiceTests
{
	readonly TokenClaims _sampleClaims = new(111, 1, 2, RandomX.CreateRandomBase64String(15));

	[Test]
	public void temp()
	{
		var hashkey = RandomX.CreateRandomBase64String(256);
		TokenService tokenService = new TokenService(hashkey);
		var token = tokenService.Encode(_sampleClaims);
		var decodedClaims = tokenService.Decode(token);

		Assert.That(decodedClaims, Is.Not.Null);
		Assert.That(decodedClaims.Type, Is.EqualTo(_sampleClaims.Type));
		Assert.That(decodedClaims.CreatedAt, Is.EqualTo(_sampleClaims.CreatedAt));
		Assert.That(decodedClaims.ExpireAt, Is.EqualTo(_sampleClaims.ExpireAt));
		Assert.That(decodedClaims.UserId, Is.EqualTo(_sampleClaims.UserId));
	}
}
