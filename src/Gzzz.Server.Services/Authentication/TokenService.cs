using Amazon.Auth.AccessControlPolicy;
using Gzzz.Serialize;
using System.Buffers.Binary;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gzzz.Authentication;

public class TokenService
{
	readonly HMACSHA256 _hmac;
	readonly int _signatureLength;
	readonly TimeSpan _offset;
	readonly TokenServiceConfig _authenticationConfig;

	public TokenService(TokenServiceConfig authenticationConfig)
	{
		var bytesKey = Convert.FromBase64String(authenticationConfig.HashKey);
		_hmac = new HMACSHA256(bytesKey);
		_signatureLength = _hmac.HashSize / 8;
		_authenticationConfig = authenticationConfig;
	}


	public string CreateAccessToken(string userId, DateTimeOffset createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.AccessTokenV1, createdAt.AddMinutes(_authenticationConfig.AccessTokenLIfetime), userId);
		return EncodeToken(claims);
	}

	public string CreateRefreshToken(string userId, DateTimeOffset createdAt)
	{
		var claims = new TokenClaims((byte)TokenType.RefreshTokenV1, createdAt.AddMinutes(_authenticationConfig.RefreshTokenLifetime), userId);
		return EncodeToken(claims);
	}
	public DecodeTokenResult ValidateToken(TokenType tokenType, string token, ApiContext context, out TokenClaims claims)
	{
		if (string.IsNullOrEmpty(token))
		{
			claims = default;
			return DecodeTokenResult.NotPresent;
		}

		if (DecodeToken(token, out claims) == false)
		{
			return DecodeTokenResult.DecodeFail;
		}
		context.UserId = claims.UserId;

		if (claims.Type != (byte)tokenType)
		{
			return DecodeTokenResult.MismatchType;
		}

		if (context.RequestTime > claims.ExpireAt)
		{
			return DecodeTokenResult.ExpiredToken;
		}

		return DecodeTokenResult.Success;
	}


	public string EncodeToken(TokenClaims claims)
	{
		Span<byte> result = stackalloc byte[256];
		var payloadLength = new SpanWriter(result.Slice(1))
			.Write(claims.Type)
			.Write(claims.ExpireAt.Ticks)
			.WriteBase64String(claims.UserId)
			.Position;

		result[0] = (byte)payloadLength;

		var payload = result.Slice(1, payloadLength);
		SignTo(payload, result.Slice(1 + payloadLength));

		return Convert.ToBase64String(result.Slice(0, payloadLength + _signatureLength + 1));
	}

	public bool DecodeToken(string token, out TokenClaims result)
	{
		Span<byte> tokenSpan = stackalloc byte[token.Length];
		if (Convert.TryFromBase64String(token, tokenSpan, out var bytesWritten) == false)
		{
			result = default;
			return false;
		}

		tokenSpan = tokenSpan.Slice(0, bytesWritten);
		//
		var payloadLength = tokenSpan[0];
		var payloadSpan = tokenSpan.Slice(1, payloadLength);
		var signatureSpan = tokenSpan.Slice(1 + payloadLength);
		//
		if (signatureSpan.Length != _signatureLength)
		{
			result = default;
			return false;
		}
		Span<byte> computedSignature = stackalloc byte[_signatureLength];
		SignTo(payloadSpan, computedSignature);
		//
		if (signatureSpan.SequenceEqual(computedSignature) == false)
		{
			result = default;
			return false;
		}
		result = FromSpan(payloadSpan);
		return true;
	}

	void SignTo(Span<byte> payload, Span<byte> destination)
	{
		if (!_hmac.TryComputeHash(payload, destination, out _))
			throw new InvalidOperationException("Hash computation failed");
	}

	TokenClaims FromSpan(Span<byte> span)
	{
		var reader = new SpanReader(span);

		return new(
			reader.ReadByte(),
			reader.ReadDateTimeOffset(_offset),
			reader.ReadBase64String()
		);
	}
}
