namespace Gzzz.Db;

public static class Extensions
{
    static readonly DateTime _ttt = new DateTime(2025, 6, 30);
    public static long ToTimescore(this DateTime time) => (time - _ttt).Ticks;
    public static DateTime ToTimescore(this long timescore) => _ttt.AddTicks(timescore);
}
