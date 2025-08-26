using System.Security.Cryptography;

namespace Gzzz.Services;

public record TokenClaims(byte Type, long CreatedAt, long ExpireAt, string UserId);

public class TokenService
{
	readonly HMACSHA256 _hmac;
	readonly int _signatureLength;
	public TokenService(string hashKey)
	{
		
		var bytesKey = Convert.FromBase64String(hashKey);
		_hmac = new HMACSHA256(bytesKey);
		_signatureLength = _hmac.HashSize / 8;
	}

	public string Encode(TokenClaims claims)
	{
		Span<byte> result = stackalloc byte[256];

		var payloadLength = CopyTo(claims, result.Slice(1)); //0은 length
		result[0] = (byte)payloadLength;
		var payload = result.Slice(1, payloadLength);
		SignTo(payload, result.Slice(1 + payloadLength));

		return Convert.ToBase64String(result.Slice(0, payloadLength + _signatureLength + 1));
	}

	public TokenClaims Decode(string token)
	{
		var tokenSpan = Convert.FromBase64String(token).AsSpan();
		//
		var payloadLength = tokenSpan[0];
		var payloadSpan = tokenSpan.Slice(1, payloadLength);
		var tokenSignature = tokenSpan.Slice(1 + payloadLength);
		//
		Span<byte> computedSignature = stackalloc byte[_signatureLength];
		SignTo(payloadSpan, computedSignature);
		//
		if (tokenSignature.SequenceEqual(computedSignature) == false)
		{
			return null;
		}
		return FromSpan(payloadSpan);
	}

	void SignTo(Span<byte> payload, Span<byte> destination)
	{
		if (!_hmac.TryComputeHash(payload, destination, out _))
			throw new InvalidOperationException("Hash computation failed");
	}

	TokenClaims FromSpan(Span<byte> span)=> new (span[0], BitConverter.ToInt64(span.Slice(1)), BitConverter.ToInt64(span.Slice(9)), Convert.ToBase64String(span.Slice(17)));
	
	int CopyTo(TokenClaims claims, Span<byte> span)
	{
		var cursor = 0;
		span[cursor++] = claims.Type;
		WriteTo(BitConverter.GetBytes(claims.CreatedAt), span, ref cursor);
		WriteTo(BitConverter.GetBytes(claims.ExpireAt), span, ref cursor);
		WriteTo(Convert.FromBase64String(claims.UserId), span, ref cursor);
		return cursor;
	}
	static void WriteTo(Span<byte> source, Span<byte> dest, ref int destCursor)
	{
		var length = source.Length;
		source.CopyTo(dest.Slice(destCursor, length));
		destCursor += length;
	}
}

public class AuthenticationService
{
	public AuthenticationService(TokenService tokenService)
	{

	}
}
