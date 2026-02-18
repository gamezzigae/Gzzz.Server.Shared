namespace Gzzz.Authentication;

public readonly struct TokenClaims
{
	public readonly byte Type;
	public readonly DateTimeOffset ExpireAt;
	public readonly string UserId;

	public TokenClaims(byte type, DateTimeOffset expireAt, string userId)
	{
		Type = type;
		ExpireAt = expireAt;
		UserId = userId;
	}
}

public enum TokenType : byte
{
	AccessTokenV1 = 10,
	RefreshTokenV1 = 240,
}
