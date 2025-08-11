using System;
using System.IO;

// Span을 활용하여 고성능 재사용 가능한 메모리 스트림 클래스를 구현합니다.
// 버퍼를 재할당하지 않고 재사용함으로써 가비지 컬렉터의 부담을 줄입니다.
public class GeminiReusableMemoryStream : Stream
{
	// 내부 버퍼
	private byte[] _buffer;
	// 현재 스트림의 데이터 길이
	private int _length;
	// 현재 읽기/쓰기 위치
	private int _position;

	/// <summary>
	/// ReusableMemoryStream의 새 인스턴스를 초기 용량과 함께 초기화합니다.
	/// </summary>
	/// <param name="initialCapacity">스트림의 초기 버퍼 크기입니다.</param>
	public GeminiReusableMemoryStream(int initialCapacity = 4096)
	{
		// 초기 버퍼를 할당합니다.
		_buffer = new byte[initialCapacity];
		_length = 0;
		_position = 0;
	}

	/// <summary>
	/// 스트림의 내부 버퍼를 재활용하여 스트림을 초기화합니다.
	/// 이 메서드는 새로운 버퍼 할당 없이 재사용할 수 있게 해줍니다.
	/// </summary>
	public void Reset()
	{
		_length = 0;
		_position = 0;
	}

	// --- Stream 추상 클래스 구현 ---

	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite => true;
	public override long Length => _length;

	public override long Position
	{
		get => _position;
		set
		{
			if (value < 0 || value > _length)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Position must be within the stream bounds.");
			}
			_position = (int)value;
		}
	}

	public override void Flush()
	{
		// 메모리 스트림이므로 할 일이 없습니다.
	}

	/// <summary>
	/// Span을 사용하여 바이트 배열을 효율적으로 스트림에 씁니다.
	/// </summary>
	public override void Write(byte[] buffer, int offset, int count)
	{
		// 입력 배열의 일부를 ReadOnlySpan으로 래핑합니다.
		var span = new ReadOnlySpan<byte>(buffer, offset, count);
		Write(span);
	}

	/// <summary>
	/// Span을 사용하여 데이터를 스트림에 씁니다. 메모리 복사 횟수를 최소화합니다.
	/// </summary>
	public void Write(ReadOnlySpan<byte> span)
	{
		// 쓰기 전에 버퍼 용량이 충분한지 확인합니다.
		EnsureCapacity(_position + span.Length);

		// ReadOnlySpan의 데이터를 내부 버퍼의 Span으로 복사합니다.
		// 이 과정에서 추가적인 메모리 할당은 발생하지 않습니다.
		span.CopyTo(new Span<byte>(_buffer, _position, span.Length));
		_position += span.Length;

		// 현재 위치가 스트림의 길이보다 길면 길이를 업데이트합니다.
		if (_position > _length)
		{
			_length = _position;
		}
	}

	/// <summary>
	/// Span을 사용하여 스트림의 데이터를 바이트 배열에 효율적으로 읽습니다.
	/// </summary>
	public override int Read(byte[] buffer, int offset, int count)
	{
		// 출력 배열의 일부를 Span으로 래핑합니다.
		var readSpan = new Span<byte>(buffer, offset, count);
		return Read(readSpan);
	}

	/// <summary>
	/// Span을 사용하여 스트림의 데이터를 효율적으로 읽습니다.
	/// </summary>
	public int Read(Span<byte> destination)
	{
		var remaining = _length - _position;
		var bytesToRead = Math.Min(destination.Length, remaining);

		// 내부 버퍼의 ReadOnlySpan을 대상 Span에 복사합니다.
		new ReadOnlySpan<byte>(_buffer, _position, bytesToRead).CopyTo(destination);

		_position += bytesToRead;
		return bytesToRead;
	}

	/// <summary>
	/// 스트림의 전체 내용을 ReadOnlySpan으로 반환합니다.
	/// </summary>
	public ReadOnlySpan<byte> GetBufferAsSpan()
	{
		return new ReadOnlySpan<byte>(_buffer, 0, _length);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
			case SeekOrigin.Begin:
				Position = offset;
				break;
			case SeekOrigin.Current:
				Position += offset;
				break;
			case SeekOrigin.End:
				Position = _length + offset;
				break;
		}
		return Position;
	}

	public override void SetLength(long value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value), "Length cannot be negative.");
		}
		EnsureCapacity((int)value);
		_length = (int)value;
	}

	/// <summary>
	/// 필요한 용량을 확보합니다. 용량이 부족하면 버퍼를 확장합니다.
	/// </summary>
	private void EnsureCapacity(int requiredCapacity)
	{
		if (requiredCapacity > _buffer.Length)
		{
			// 새로운 용량을 계산합니다.
			var newCapacity = Math.Max(_buffer.Length * 2, requiredCapacity);
			var newBuffer = new byte[newCapacity];

			// 기존 버퍼의 데이터를 새로운 버퍼로 복사합니다.
			// 이때도 Span을 사용하여 효율적으로 복사합니다.
			new ReadOnlySpan<byte>(_buffer, 0, _length).CopyTo(new Span<byte>(newBuffer));
			_buffer = newBuffer;
		}
	}
}
