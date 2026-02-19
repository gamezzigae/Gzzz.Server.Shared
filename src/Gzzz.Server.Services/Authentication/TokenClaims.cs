namespace Gzzz.Authentication;

public readonly struct TokenClaims
{
	public readonly byte Type;
	public readonly DateTimeOffset CreatedAt;
	public readonly string UserId;

	public TokenClaims(byte type, DateTimeOffset expireAt, string userId)
	{
		Type = type;
		CreatedAt = expireAt;
		UserId = userId;
	}
}

public enum TokenType : byte
{
	AccessTokenV1 = 10,
	RefreshTokenV1 = 240,
}
