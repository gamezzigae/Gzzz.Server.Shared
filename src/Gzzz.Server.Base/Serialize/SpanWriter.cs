namespace Gzzz.Serialize;

public ref struct SpanWriter
{
	private Span<byte> _destination;
	public int Position { get; private set; }

	public SpanWriter(Span<byte> destination)
	{
		_destination = destination;
	}

	public SpanWriter Write(byte value)
	{
		_destination[Position] = value;
		Position += 1;
		return this;
	}
	public SpanWriter Write(Int64 value)
	{
		var xx = BitConverter.TryWriteBytes(_destination.Slice(Position), value);
		Position += sizeof(Int64);
		return this;
	}
	public SpanWriter WriteBytes(Span<byte> source)
	{
		var length = source.Length;
		Write((byte)length);
		source.CopyTo(_destination.Slice(Position, length));
		Position += length;
		return this;
	}
	public SpanWriter WriteBase64String(string base64string)
	{
		var lengthPosition = this.Position++;
		var bytes = Convert.TryFromBase64String(base64string, _destination.Slice(Position), out var length);
		Position += length;
		_destination[lengthPosition] = (byte)length;

		return this;
	}
}
