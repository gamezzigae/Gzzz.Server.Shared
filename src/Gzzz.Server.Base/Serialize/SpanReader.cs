using System.Runtime.InteropServices;

namespace Gzzz.Serialize;

public ref struct SpanReader
{
	private ReadOnlySpan<byte> _span;
	public int Position { get; private set; }

	public SpanReader(ReadOnlySpan<byte> span) => _span = span;
	public byte ReadByte()
	{
		byte result = _span[Position];
		Position++;
		return result;
	}

	public ReadOnlySpan<byte> ReadBytes(int count)
	{
		var result = _span.Slice(Position, count);
		Position += count;
		return result;
	}


	public ReadOnlySpan<byte> ReadBytes()
	{
		var length = this.ReadByte();
		return ReadBytes(length);
	}

	public string ReadBase64String()
	{
		var bytes = ReadBytes();
		return Convert.ToBase64String(bytes);
	}

	public Int64 ReadInt64()
	{
		var value = MemoryMarshal.Read<long>(_span.Slice(Position, 8));
		Position += sizeof(long);
		return value;
	}
	public DateTimeOffset ReadDateTimeOffset(TimeSpan offset)
	{
		var ticks = ReadInt64();
		return new DateTimeOffset(ticks, offset);
	}
}
