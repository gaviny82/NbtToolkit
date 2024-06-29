using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace NbtToolkit.Binary;

internal sealed class NbtBinaryWriter : IDisposable
{
    private const int MaxStackBufferSize = 128;
    private const int MaxNbtStringByteCount = ushort.MaxValue; // 64 KB (limit of NbtString)
    private const int MaxArrayPoolSize = 64 * 1024;

    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public NbtBinaryWriter(Stream output, bool leaveOpen = false)
    {
        if (!output.CanWrite)
            throw new ArgumentException("Stream is not writable", nameof(output));

        _stream = output;
        _leaveOpen = leaveOpen;
    }

    #region Stream operations

    // Closes this writer and releases any system resources associated with the
    // writer. Following a call to Close, any operations on the writer
    // may raise exceptions.
    public void Close()
    {
        Dispose();
    }

    // Clears all buffers for this writer and causes any buffered data to be
    // written to the underlying device.
    public void Flush()
    {
        _stream.Flush();
    }

    public long Seek(int offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public void Dispose()
    {
        if (_leaveOpen)
            _stream.Flush();
        else
            _stream.Close();
    }

    #endregion

    // TODO: try-finally for ArrayPool return

    #region Writing a single value

    // TODO: Use modified UTF-8 encoding
    public void WriteString(ReadOnlySpan<char> str)
    {
        Encoding encoding = Encoding.UTF8;

        // Reference: System.IO.BinaryWriter.Write(string)

        // Each char takes a maximum of 3 bytes
        if (str.Length <= MaxStackBufferSize / 3) // Use stack buffer for small number of characters
        {
            Span<byte> buffer = stackalloc byte[MaxStackBufferSize];
            int actualByteCount = encoding.GetBytes(str, buffer); // Avoid 2-pass calculation
            Write((ushort)actualByteCount); // Guaranteed to be less than ushort.MaxValue
            _stream.Write(buffer[0..actualByteCount]);
        }
        else if (str.Length <= MaxNbtStringByteCount / 3) // Use ArrayPool buffer for medium number of characters
        {
            byte[] rented = ArrayPool<byte>.Shared.Rent(str.Length * 3);
            int actualByteCount = encoding.GetBytes(str, rented); // Avoid 2-pass calculation
            Write((ushort)actualByteCount); // Guaranteed to be less than ushort.MaxValue
            _stream.Write(rented, 0, actualByteCount);
            ArrayPool<byte>.Shared.Return(rented);
        }
        else // Fall back: use 2-pass calculation for large number of characters
        {
            int actualBytecount = encoding.GetByteCount(str);
            if (actualBytecount > MaxNbtStringByteCount)
                throw new ArgumentException("String is too long", nameof(str));
            Write((ushort)actualBytecount);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(actualBytecount);
            encoding.GetBytes(str, buffer);
            _stream.Write(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void Write(byte value) => _stream.WriteByte(value);

    public void Write(sbyte value) => _stream.WriteByte((byte)value);

    public void Write(ushort value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void Write(short value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(short)];
        BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void Write(int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void Write(long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void Write(float value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(float)];
        BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        _stream.Write(buffer);
    }

    public void Write(double value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        _stream.Write(buffer);
    }

    #endregion

    #region Writing a span of values

    public void Write(ReadOnlySpan<sbyte> values)
    {
        var bytes = MemoryMarshal.AsBytes(values);
        _stream.Write(bytes);
    }

    public void Write(ReadOnlySpan<short> values)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Allocate the buffer on the stack if it's small enough, otherwise rent from the pool
            int byteCount = values.Length * sizeof(short);
            if (byteCount <= MaxStackBufferSize)
            {
                Span<byte> buffer = stackalloc byte[byteCount];
                Span<short> bufferAsShort = MemoryMarshal.Cast<byte, short>(buffer);
                BinaryPrimitives.ReverseEndianness(values, bufferAsShort);
                _stream.Write(buffer);
            }
            else if (byteCount <= MaxArrayPoolSize)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                Span<short> bufferAsShort = MemoryMarshal.Cast<byte, short>(buffer);
                BinaryPrimitives.ReverseEndianness(values, bufferAsShort);
                _stream.Write(buffer);
                ArrayPool<byte>.Shared.Return(buffer);
            }
            else
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(MaxArrayPoolSize);
                Span<short> bufferAsShort = MemoryMarshal.Cast<byte, short>(buffer);

                while (values.Length > bufferAsShort.Length)
                {
                    BinaryPrimitives.ReverseEndianness(values[0..bufferAsShort.Length], bufferAsShort);
                    _stream.Write(buffer);
                    values = values[bufferAsShort.Length..];
                }

                // Reaminder
                BinaryPrimitives.ReverseEndianness(values, bufferAsShort);
                _stream.Write(buffer, 0, values.Length * sizeof(short));

                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        else
        {
            var bytes = MemoryMarshal.AsBytes(values);
            _stream.Write(bytes);
        }
    }

    public void Write(ReadOnlySpan<int> values)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Allocate the buffer on the stack if it's small enough, otherwise rent from the pool
            int byteCount = values.Length * sizeof(int);
            if (byteCount <= MaxStackBufferSize)
            {
                Span<byte> buffer = stackalloc byte[byteCount];
                Span<int> bufferAsInt = MemoryMarshal.Cast<byte, int>(buffer);
                BinaryPrimitives.ReverseEndianness(values, bufferAsInt);
                _stream.Write(buffer);
            }
            else if (byteCount <= MaxArrayPoolSize)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                Span<int> bufferAsInt = MemoryMarshal.Cast<byte, int>(buffer);
                BinaryPrimitives.ReverseEndianness(values, bufferAsInt);
                _stream.Write(buffer);
                ArrayPool<byte>.Shared.Return(buffer);
            }
            else
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(MaxArrayPoolSize);
                Span<int> bufferAsInt = MemoryMarshal.Cast<byte, int>(buffer);

                while (values.Length > bufferAsInt.Length)
                {
                    BinaryPrimitives.ReverseEndianness(values[0..bufferAsInt.Length], bufferAsInt);
                    _stream.Write(buffer);
                    values = values[bufferAsInt.Length..];
                }

                // Reaminder
                BinaryPrimitives.ReverseEndianness(values, bufferAsInt);
                _stream.Write(buffer, 0, values.Length * sizeof(int));

                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        else
        {
            var bytes = MemoryMarshal.AsBytes(values);
            _stream.Write(bytes);
        }
    }

    public void Write(ReadOnlySpan<long> values)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Allocate the buffer on the stack if it's small enough, otherwise rent from the pool
            int byteCount = values.Length * sizeof(long);
            if (byteCount <= MaxStackBufferSize)
            {
                Span<byte> buffer = stackalloc byte[byteCount];
                Span<long> bufferAsLong = MemoryMarshal.Cast<byte, long>(buffer);
                BinaryPrimitives.ReverseEndianness(values, bufferAsLong);
                _stream.Write(buffer);
            }
            else if (byteCount <= MaxArrayPoolSize)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                Span<long> bufferAsLong = MemoryMarshal.Cast<byte, long>(buffer);
                BinaryPrimitives.ReverseEndianness(values, bufferAsLong);
                _stream.Write(buffer);
                ArrayPool<byte>.Shared.Return(buffer);
            }
            else
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(MaxArrayPoolSize);
                Span<long> bufferAsLong = MemoryMarshal.Cast<byte, long>(buffer);

                while (values.Length > bufferAsLong.Length)
                {
                    BinaryPrimitives.ReverseEndianness(values[0..bufferAsLong.Length], bufferAsLong);
                    _stream.Write(buffer);
                    values = values[bufferAsLong.Length..];
                }

                // Reaminder
                BinaryPrimitives.ReverseEndianness(values, bufferAsLong);
                _stream.Write(buffer, 0, values.Length * sizeof(long));

                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
        else
        {
            var bytes = MemoryMarshal.AsBytes(values);
            _stream.Write(bytes);
        }
    }

    // Same logic as Int32 (4 bytes) and Int64 (8 bytes) types
    public void Write(ReadOnlySpan<float> values) => Write(MemoryMarshal.Cast<float, int>(values));
    public void Write(ReadOnlySpan<double> values) => Write(MemoryMarshal.Cast<double, long>(values));

    #endregion
}