namespace Gzzz.Services.Authentication;

public record TokenClaims(byte Type, DateTime CreatedAt, byte Lifetime, string UserId)
{
	public DateTime GetExpireAt()=>CreatedAt.AddMinutes(Lifetime * Type);
}

public enum TokenType : byte
{
	Access = 10,
	Refresh = 240,
}
