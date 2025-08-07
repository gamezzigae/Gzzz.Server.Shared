namespace Gzzz;

public static class RandomX
{
    #region
    static readonly char[] _characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ09123456789".ToCharArray();
    #endregion

    public static int GetRandom(int min =0, int max=int.MaxValue)=>Random.Shared.Next(min,max);

    public static string GetRandomText(int size = 32)
    {
        var builder = new System.Text.StringBuilder(size);

        var span = _characters.AsSpan();

        foreach (var c in Enumerable.Range(0, size).Select(i => Random.Shared.Next(0, _characters.Length)))
        {
            builder.Append(_characters[c]);
        }

        return builder.ToString();
    }
}
