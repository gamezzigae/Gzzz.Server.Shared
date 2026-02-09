using Gzzz.Serialize;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gzzz;


public static class Extensions
{

	public static string ToISO8601(this DateTime time) => time.ToString(DefaultConfig.DateTimeFormat);
    public static int GetIntervalSeconds(this DateTime time1, DateTime time2) => (int)((time2 - time1).TotalSeconds);
    public static bool IsBetween(this DateTime time, DateTime startTime, DateTime endTime)
        => startTime <= time && time <= endTime;

    public static Dictionary<K, int> CountBy<K, V>(this IEnumerable<V> values, Func<V, K> groupSelector)
    {
        return values.GroupBy(groupSelector).ToDictionary(g => g.Key, g => g.Count());
    }
    public static Dictionary<K, V[]> ToDictionaryGroup<K, V>(this IEnumerable<V> values, Func<V, K> groupSelector)
    {
        return values.GroupBy(groupSelector).ToDictionary(g => g.Key, g => g.ToArray());
    }
		public static Dictionary<K, int> ToDictionaryGroupCount<K, V>(this IEnumerable<V> values, Func<V, K> groupSelector)
		{
			return values.GroupBy(groupSelector).ToDictionary(g => g.Key, g => g.ToArray().Length);
		}

		public static IEnumerable<V> Shuffle<V>(this IEnumerable<V> values, Random random = null)
		{
			if (random == null)
				random = new Random();
			return values.OrderBy(item => random.Next());
		}

		public static T AnyOne<T>(this IEnumerable<T> values, Random random= null)
		{
			if(random ==null)
				random = new Random();
			var cursor = random.Next(0, values.Count());
			return values.Skip(cursor).First();
		}

		public static bool IsEqual<T>(this T x, T y)=>EqualityComparer<T>.Default.Equals(x, y);
    public static bool IsDefault<T>(this T instance) => EqualityComparer<T>.Default.Equals(instance, default);
        
    public static bool IsNotDefault<T>(this T instance)=>EqualityComparer<T>.Default.Equals(instance, default) != true;

    
    public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
    public static bool IsNotNullOrEmpty(this string text) => string.IsNullOrEmpty(text) == false;

    public static string Wrap(this string text, char c) => c + text + c;

    public static long ToUnixTimeSeconds(this DateTime time) => new DateTimeOffset(time).ToUnixTimeSeconds();
	public static long ToUnixTimeMilliseconds(this DateTime time) => new DateTimeOffset(time).ToUnixTimeMilliseconds();


	public static DateTime FloorSeconds(this DateTime datetime) => new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, 0, 0, datetime.Kind);

    public static long Percent(this long value, long percentage) => (long)Math.Ceiling((double)value * percentage / 100);

		public static long Increase<K>(this Dictionary<K, long> dictionary, K key, long value)
		{
			if (dictionary.ContainsKey(key))
				return dictionary[key] += value;
			return dictionary[key] = value;
		}
		public static int Increase<K>(this Dictionary<K, int> dictionary, K key, int value)
		{
			if (dictionary.ContainsKey(key))
				return dictionary[key] += value;
			return dictionary[key] = value;
		}

		public static void Remove<K,V>(this Dictionary<K,V> dictionary, Func<V, bool> removeCondition)
    {
        foreach (var key in dictionary.Keys.ToArray())
        {
            var item = dictionary[key];

            if (removeCondition(item))
            {
                dictionary.Remove(key);
            }
        }

    }

    public static long Set<K>(this Dictionary<K, long> dictionary, K key, long value)
    {
        return dictionary[key] = value;
    }

    public static ReadOnlyDictionary<K, V> ToReadonly<K, V>(this IDictionary<K, V> dictionary)
        => new ReadOnlyDictionary<K, V>(dictionary);

    public static StringBuilder AppendIf(this StringBuilder builder, bool condition, string message)
        => condition ? builder.Append(message) : builder;

    public static bool IsNullOrEmpty<T>(this T[] array)
    {
        return array == null || array.Length == 0;
    }
    public static bool IsNotNullOrEmpty<T>(this T[] array)
    {
        return array != null && array.Length == 0;
    }

}
