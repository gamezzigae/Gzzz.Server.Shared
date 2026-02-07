namespace Gzzz.Authentication;

public record TokenClaims(byte Type, DateTimeOffset ExpireAt, string UserId);

public enum TokenType : byte
{
	Access = 10,
	Refresh = 240,
}
