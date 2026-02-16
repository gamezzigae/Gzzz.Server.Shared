using MessagePack;

namespace Gzzz.Authentication;

[MessagePackObject]
public class TokenClaims
{
	[Key(0)]
    public byte Type { get; set; }
	[Key(1)]
	public DateTimeOffset ExpireAt { get; set; }
	[Key(2)]
	public string UserId { get; set; }

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
