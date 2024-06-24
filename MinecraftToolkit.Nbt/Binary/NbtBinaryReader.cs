using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MinecraftToolkit.Nbt.Binary;

public class NbtBinaryReader : IDisposable
{
    private const int MaxStackBufferSize = 128;

    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public NbtBinaryReader(Stream input, bool leaveOpen = false)
    {
        if (!input.CanRead)
            throw new ArgumentException("Stream is not readable", nameof(input));

        _stream = input;
        _leaveOpen = leaveOpen;
    }

    #region Stream operations

    public void Dispose()
    {
        if (!_leaveOpen)
            _stream.Close();
    }

    #endregion

    #region Reading a single value

    public string ReadString()
    {
        ushort length = ReadUInt16();

        // TODO: Use modified UTF-8 encoding

        // The number of characters in the string is unknown due to the variable length UTF-8 encoding, so string.Create<T> cannot be used

        if (length <= MaxStackBufferSize) // Use stack buffer for short strings; NBT names are usually shorter than 128 ASCII characters
        {
            Span<byte> buffer = stackalloc byte[length];
            _stream.ReadExactly(buffer);
            return Encoding.UTF8.GetString(buffer);
        }
        else // Use ArrayPool for longer strings; max length = 65535
        {
            byte[] rented = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                _stream.ReadExactly(rented, 0, length);
                return Encoding.UTF8.GetString(rented, 0, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int InternalReadByte()
    {
        int b = _stream.ReadByte();
        if (b == -1)
            throw new EndOfStreamException();
        return b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte() => (byte)InternalReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadSByte() => (sbyte)InternalReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        _stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        _stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt16BigEndian(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        _stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt32BigEndian(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        _stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadInt64BigEndian(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadSingle()
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        _stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadSingleBigEndian(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        _stream.ReadExactly(buffer);
        return BinaryPrimitives.ReadDoubleBigEndian(buffer);
    }

    #endregion

    #region Reading a span of values

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt8Span(Span<sbyte> span)
    {
        _stream.ReadExactly(MemoryMarshal.AsBytes(span));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt16Span(Span<short> span)
    {
        _stream.ReadExactly(MemoryMarshal.AsBytes(span));
        if (BitConverter.IsLittleEndian)
            BinaryPrimitives.ReverseEndianness(span, span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt32Span(Span<int> span)
    {
        _stream.ReadExactly(MemoryMarshal.AsBytes(span));
        if (BitConverter.IsLittleEndian)
            BinaryPrimitives.ReverseEndianness(span, span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadInt64Span(Span<long> span)
    {
        _stream.ReadExactly(MemoryMarshal.AsBytes(span));
        if (BitConverter.IsLittleEndian)
            BinaryPrimitives.ReverseEndianness(span, span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadFloatSpan(Span<float> span)
    {
        _stream.ReadExactly(MemoryMarshal.AsBytes(span));
        if (BitConverter.IsLittleEndian)
        {
            Span<int> intSpan = MemoryMarshal.Cast<float, int>(span);
            BinaryPrimitives.ReverseEndianness(intSpan, intSpan);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadDoubleSpan(Span<double> span)
    {
        _stream.ReadExactly(MemoryMarshal.AsBytes(span));
        if (BitConverter.IsLittleEndian)
        {
            Span<long> longSpan = MemoryMarshal.Cast<double, long>(span);
            BinaryPrimitives.ReverseEndianness(longSpan, longSpan);
        }
    }

    #endregion
}