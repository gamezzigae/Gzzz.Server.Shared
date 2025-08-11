using System;
using System.IO;

/// <summary>
/// 재사용 가능한 간단한 스트림 구현
/// Write는 byte[] 전체만 지원하고, pooling 없이 변수로 선언하여 재사용 가능
/// </summary>
public class ClaudeReusableStream : Stream
{
	private byte[] _buffer;
	private int _position;
	private int _length;
	private int _capacity;
	private bool _disposed;

	public ClaudeReusableStream(int initialCapacity = 4096)
	{
		_buffer = new byte[initialCapacity];
		_capacity = initialCapacity;
		_position = 0;
		_length = 0;
		_disposed = false;
	}

	/// <summary>
	/// 스트림을 초기화하여 재사용 가능하게 만듭니다.
	/// </summary>
	public void Reset()
	{
		_position = 0;
		_length = 0;
	}

	/// <summary>
	/// byte[] 전체를 스트림에 씁니다.
	/// </summary>
	/// <param name="buffer">쓸 데이터</param>
	public void Write(byte[] buffer)
	{
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));

		Write(buffer, 0, buffer.Length);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(ClaudeReusableStream));
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));
		if (offset < 0 || count < 0 || offset + count > buffer.Length)
			throw new ArgumentOutOfRangeException();

		EnsureCapacity(_position + count);

		Buffer.BlockCopy(buffer, offset, _buffer, _position, count);
		_position += count;

		if (_position > _length)
			_length = _position;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(ClaudeReusableStream));
		if (buffer == null)
			throw new ArgumentNullException(nameof(buffer));
		if (offset < 0 || count < 0 || offset + count > buffer.Length)
			throw new ArgumentOutOfRangeException();

		int bytesToRead = Math.Min(count, _length - _position);
		if (bytesToRead <= 0)
			return 0;

		Buffer.BlockCopy(_buffer, _position, buffer, offset, bytesToRead);
		_position += bytesToRead;
		return bytesToRead;
	}

	/// <summary>
	/// 현재 스트림의 내용을 byte[]로 반환합니다.
	/// </summary>
	/// <returns>스트림 내용의 복사본</returns>
	public byte[] ToArray()
	{
		var result = new byte[_length];
		Buffer.BlockCopy(_buffer, 0, result, 0, _length);
		return result;
	}

	/// <summary>
	/// 내부 버퍼에 직접 접근할 수 있는 ArraySegment를 반환합니다. (복사 없음)
	/// </summary>
	/// <returns>내부 버퍼의 유효한 부분</returns>
	public ArraySegment<byte> GetBuffer()
	{
		return new ArraySegment<byte>(_buffer, 0, _length);
	}

	private void EnsureCapacity(int requiredCapacity)
	{
		if (_capacity >= requiredCapacity)
			return;

		int newCapacity = Math.Max(_capacity * 2, requiredCapacity);
		var newBuffer = new byte[newCapacity];
		Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _length);
		_buffer = newBuffer;
		_capacity = newCapacity;
	}

	public override bool CanRead => !_disposed;
	public override bool CanSeek => !_disposed;
	public override bool CanWrite => !_disposed;
	public override long Length => _length;

	public override long Position
	{
		get => _position;
		set
		{
			if (value < 0 || value > _length)
				throw new ArgumentOutOfRangeException(nameof(value));
			_position = (int)value;
		}
	}

	public override void Flush()
	{
		// 메모리 스트림이므로 아무것도 하지 않음
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(ClaudeReusableStream));

		long newPosition = origin switch
		{
			SeekOrigin.Begin => offset,
			SeekOrigin.Current => _position + offset,
			SeekOrigin.End => _length + offset,
			_ => throw new ArgumentException("Invalid seek origin", nameof(origin))
		};

		if (newPosition < 0 || newPosition > _length)
			throw new ArgumentOutOfRangeException(nameof(offset));

		_position = (int)newPosition;
		return _position;
	}

	public override void SetLength(long value)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(ClaudeReusableStream));
		if (value < 0 || value > int.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(value));

		int newLength = (int)value;
		EnsureCapacity(newLength);
		_length = newLength;

		if (_position > _length)
			_position = _length;
	}

	protected override void Dispose(bool disposing)
	{
		if (!_disposed && disposing)
		{
			_disposed = true;
			_buffer = null;
		}
		base.Dispose(disposing);
	}
}
