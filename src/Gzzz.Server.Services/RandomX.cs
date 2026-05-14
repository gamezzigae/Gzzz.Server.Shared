using System.Security.Cryptography;

namespace Gzzz;

public static class RandomX
{
    #region
    static readonly char[] _characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ09123456789".ToCharArray();
    #endregion

    public static int GetRandom(int min =0, int max=int.MaxValue)=>Random.Shared.Next(min,max);

	public static string CreateRandomBase64String(int byteLength)
	{
		Span<byte> bytes = stackalloc byte[byteLength];
		RandomNumberGenerator.Fill(bytes);
		return Convert.ToBase64String(bytes);
	}


	public static string GetRandomText(int size = 32)
    {
        var builder = new System.Text.StringBuilder(size);

        var span = _characters.AsSpan();

        foreach (var c in Enumerable.Range(0, size).Select(i => new Random().Next(0, _characters.Length)))
        {
            builder.Append(_characters[c]);
        }

        return builder.ToString();
    }
}
