using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NbtToolkit.Binary;

internal sealed class NbtBinaryReader : BinaryReader
{
    private const int MaxStackBufferSize = 128;

    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public NbtBinaryReader(Stream input, bool leaveOpen = false) : base(input, Encoding.UTF8, leaveOpen)
    {
        if (!input.CanRead)
            throw new ArgumentException("Stream is not readable", nameof(input));

        _stream = input;
        _leaveOpen = leaveOpen;
    }

    #region Reading a single value

    public sealed override string ReadString()
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
    public override byte ReadByte() => base.ReadByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override sbyte ReadSByte() => base.ReadSByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override ushort ReadUInt16()
    {
        ushort value = base.ReadUInt16();
        if (BitConverter.IsLittleEndian)
            return BinaryPrimitives.ReverseEndianness(value);
        else
            return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override short ReadInt16()
    {
        short value = base.ReadInt16();
        if (BitConverter.IsLittleEndian)
            return BinaryPrimitives.ReverseEndianness(value);
        else
            return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int ReadInt32()
    {
        int value = base.ReadInt32();
        if (BitConverter.IsLittleEndian)
            return BinaryPrimitives.ReverseEndianness(value);
        else
            return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override long ReadInt64()
    {
        long value = base.ReadInt64();
        if (BitConverter.IsLittleEndian)
            return BinaryPrimitives.ReverseEndianness(value);
        else
            return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float ReadSingle()
    {
        float value = base.ReadSingle();
        if (BitConverter.IsLittleEndian)
        {
            int valueAsInt = BinaryPrimitives.ReverseEndianness(Unsafe.As<float, int>(ref value));
            return Unsafe.As<int, float>(ref valueAsInt);
        }
        else
            return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override double ReadDouble()
    {
        double value = base.ReadDouble();
        if (BitConverter.IsLittleEndian)
        {
            long valueAsLong = BinaryPrimitives.ReverseEndianness(Unsafe.As<double, long>(ref value));
            return Unsafe.As<long, double>(ref valueAsLong);
        }
        else
            return value;
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
    public void ReadSingleSpan(Span<float> span)
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