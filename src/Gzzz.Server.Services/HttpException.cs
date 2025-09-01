namespace Gzzz;

public class HttpException : Exception
{
	public HttpException(int statusCode, string message, int errorCode=0) : base(message)
	{
		StatusCode = statusCode;
		ErrorCode = errorCode;
	}
	public int StatusCode { get; }
	public int ErrorCode { get; }

	public static TResult Assert<T1, T2, TResult>(Func<T1,T2,TResult> func,T1 p1, T2 p2, int statusCode, string message, int errorCode = 0)
	{
		try
		{
			return func(p1, p2);
		}
		catch (Exception)
		{
			throw new HttpException(statusCode, message, errorCode);
		}
	}
	public static async Task<T> AssertAsync<T>(Func<Task<T>> func, int statusCode, string message, int errorCode = 0)
	{
		try
		{
			return await func();
		}
		catch (Exception)
		{
			throw new HttpException(statusCode, message, errorCode);
		}
	}
	public static async Task<TResult> AssertAsync2<T1,TResult>(Func<T1, Task<TResult>> func, T1 p1, int statusCode, string message, int errorCode = 0)
	{
		try
		{
			return await func(p1);
		}
		catch (Exception)
		{
			throw new HttpException(statusCode, message, errorCode);
		}
	}
}
