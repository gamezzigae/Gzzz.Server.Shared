using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Gzzz.Authentication;

public class TokenService
{
	readonly HMACSHA256 _hmac;
	readonly int _signatureLength;
	readonly TimeSpan _offset;
	public TokenService(AuthenticationConfig authenticationConfig)
	{
		var bytesKey = Convert.FromBase64String(authenticationConfig.HashKey);
		_hmac = new HMACSHA256(bytesKey);
		_signatureLength = _hmac.HashSize / 8;
	}

	public string EncodeToken(TokenClaims claims)
	{
		Span<byte> result = stackalloc byte[256];
		var payloadLength = CopyTo(claims, result.Slice(1)); //0은 length

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
			result = null;
			return false;
		}

		tokenSpan = tokenSpan.Slice(0, bytesWritten);
		//
		var payloadLength = tokenSpan[0];
		var payloadSpan = tokenSpan.Slice(1, payloadLength);
		var tokenSignature = tokenSpan.Slice(1 + payloadLength);
		//
		if (tokenSignature.Length != _signatureLength)
		{
			result = null!;
			return false;
		}
		Span<byte> computedSignature = stackalloc byte[_signatureLength];
		SignTo(payloadSpan, computedSignature);
		//
		if (tokenSignature.SequenceEqual(computedSignature) == false)
		{
			result = null!;
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

	TokenClaims FromSpan(Span<byte> span) => new(
		span[0],
		new DateTimeOffset(BitConverter.ToInt64(span.Slice(1)), _offset),
		Convert.ToBase64String(span.Slice(9))
	);

	int CopyTo(TokenClaims claims, Span<byte> span)
	{
		var cursor = 0;
		span[cursor++] = claims.Type;
		SpanWriter.WriteTo(claims.ExpireAt.Ticks, span, ref cursor);
		SpanWriter.WriteTo(Convert.FromBase64String(claims.UserId), span, ref cursor);
		return cursor;
	}
}
public class TokenService2
{
	readonly HMACSHA256 _hmac;
	readonly int _signatureLength;
	readonly TimeSpan _offset;
	public TokenService2(AuthenticationConfig authenticationConfig)
	{
		var bytesKey = Convert.FromBase64String(authenticationConfig.HashKey);
		_hmac = new HMACSHA256(bytesKey);
		_signatureLength = _hmac.HashSize / 8;
	}

	public string EncodeToken(TokenClaims claims)
	{
		Span<byte> result = stackalloc byte[256];
		var payload = MessagePack.MessagePackSerializer.Serialize(claims);
		var payloadLength = payload.Length;

		result[0] = (byte)payloadLength;
		payload.CopyTo(result.Slice(1));
		SignTo(payload, result.Slice(1 + payloadLength));

		return Convert.ToBase64String(result.Slice(0, 1+ payloadLength + _signatureLength));
	}

	public bool DecodeToken(string token, out TokenClaims result)
	{
		Span<byte> tokenSpan = stackalloc byte[token.Length];
		if (Convert.TryFromBase64String(token, tokenSpan, out var bytesWritten) == false)
		{
			result = null;
			return false;
		}

		tokenSpan = tokenSpan.Slice(0, bytesWritten);
		//
		var payloadLength = tokenSpan[0];
		var payloadSpan = tokenSpan.Slice(1, payloadLength);
		var tokenSignature = tokenSpan.Slice(1 + payloadLength);
		//
		if (tokenSignature.Length != _signatureLength)
		{
			result = null!;
			return false;
		}
		Span<byte> computedSignature = stackalloc byte[_signatureLength];
		SignTo(payloadSpan, computedSignature);
		//
		if (tokenSignature.SequenceEqual(computedSignature) == false)
		{
			result = null!;
			return false;
		}
		result = MessagePack.MessagePackSerializer.Deserialize<TokenClaims>(payloadSpan.ToArray());
		return true;
	}

	void SignTo(Span<byte> payload, Span<byte> destination)
	{
		if (!_hmac.TryComputeHash(payload, destination, out _))
			throw new InvalidOperationException("Hash computation failed");
	}
}

public static class SpanWriter
{
	public static void WriteTo(Span<byte> source, Span<byte> dest, ref int destCursor)
	{
		var length = source.Length;
		source.CopyTo(dest.Slice(destCursor, length));
		destCursor += length;
	}
	public static void WriteTo(long value, Span<byte> dest, ref int destCursor)
	{
		var xx = BitConverter.TryWriteBytes(dest.Slice(destCursor), value);
		destCursor += 8;
	}
}
