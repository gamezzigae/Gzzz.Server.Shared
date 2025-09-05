namespace Gzzz.Authentication;

public class TokenClaims
{
	public TokenClaims(byte type, DateTime expireAt, string userId)
	{
		Type = type;
		ExpireAt = expireAt;
		UserId = userId;
	}

	public byte Type { get; }
	public DateTime ExpireAt { get; }
	public string UserId { get; }
}

public enum TokenType : byte
{
	Access = 10,
	Refresh = 240,
}
