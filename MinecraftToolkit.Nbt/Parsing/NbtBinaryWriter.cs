using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace MinecraftToolkit.Nbt.Parsing;

internal sealed class NbtBinaryWriter : IDisposable
{
    private const int MaxStackBufferSize = 128;
    private const int MaxNbtStringByteCount = ushort.MaxValue; // 64 KB (limit of NbtString)

    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public NbtBinaryWriter(Stream output, bool leaveOpen)
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

    // TODO: Use modified UTF-8 encoding
    public void Write(ReadOnlySpan<char> chars)
    {
        Encoding encoding = Encoding.UTF8;

        // Reference: System.IO.BinaryWriter.Write(string)

        // Each char takes a maximum of 3 bytes
        if (chars.Length <= MaxStackBufferSize / 3) // Use stack buffer for small number of characters
        {
            Span<byte> buffer = stackalloc byte[MaxStackBufferSize];
            int actualByteCount = encoding.GetBytes(chars, buffer); // Avoid 2-pass calculation
            Write((ushort)actualByteCount); // Guaranteed to be less than ushort.MaxValue
            _stream.Write(buffer);
        }
        else if (chars.Length <= MaxNbtStringByteCount / 3) // Use ArrayPool buffer for medium number of characters
        {
            byte[] rented = ArrayPool<byte>.Shared.Rent(MaxNbtStringByteCount);
            int actualByteCount = encoding.GetBytes(chars, rented); // Avoid 2-pass calculation
            Write((ushort)actualByteCount); // Guaranteed to be less than ushort.MaxValue
            _stream.Write(rented);
            ArrayPool<byte>.Shared.Return(rented);
        }
        else // Fall back: use 2-pass calculation for large number of characters
        {
            int actualBytecount = encoding.GetByteCount(chars);
            if (actualBytecount > MaxNbtStringByteCount)
                throw new ArgumentException("String is too long", nameof(chars));
            Write((ushort)actualBytecount);

            byte[] buffer = ArrayPool<byte>.Shared.Rent(actualBytecount);
            encoding.GetBytes(chars, buffer);
            _stream.Write(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    #region Writing a single value

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
        Span<byte> buffer  = stackalloc byte[sizeof(float)];
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

    // TODO: Avoid renting huge buffers from ArrayPool

    #region Writing a sequence of values

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
            byte[]? rented = byteCount <= MaxStackBufferSize ? null : ArrayPool<byte>.Shared.Rent(byteCount);
            Span<byte> buffer = rented is null ? stackalloc byte[byteCount] : rented;

            // Cast the buffer to Span<short> for byte order conversion
            Span<short> dest = MemoryMarshal.Cast<byte, short>(buffer);
            BinaryPrimitives.ReverseEndianness(values, dest);

            _stream.Write(buffer);

            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
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
            byte[]? rented = byteCount <= MaxStackBufferSize ? null : ArrayPool<byte>.Shared.Rent(byteCount);
            Span<byte> buffer = rented is null ? stackalloc byte[byteCount] : rented;

            // Cast the buffer to Span<int> for byte order conversion
            Span<int> dest = MemoryMarshal.Cast<byte, int>(buffer);
            BinaryPrimitives.ReverseEndianness(values, dest);

            _stream.Write(buffer);

            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
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
            byte[]? rented = byteCount <= MaxStackBufferSize ? null : ArrayPool<byte>.Shared.Rent(byteCount);
            Span<byte> buffer = rented is null ? stackalloc byte[byteCount] : rented;

            // Cast the buffer to Span<short> for byte order conversion
            Span<long> dest = MemoryMarshal.Cast<byte, long>(buffer);
            BinaryPrimitives.ReverseEndianness(values, dest);

            _stream.Write(buffer);

            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
        else
        {
            var bytes = MemoryMarshal.AsBytes(values);
            _stream.Write(bytes);
        }
    }

    public void Write(ReadOnlySpan<float> values)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Allocate the buffer on the stack if it's small enough, otherwise rent from the pool
            int byteCount = values.Length * sizeof(float);
            byte[]? rented = byteCount <= MaxStackBufferSize ? null : ArrayPool<byte>.Shared.Rent(byteCount);
            Span<byte> buffer = rented is null ? stackalloc byte[byteCount] : rented;

            // Cast the buffer to Span<short> for byte order conversion
            Span<int> dest = MemoryMarshal.Cast<byte, int>(buffer);
            ReadOnlySpan<int> source = MemoryMarshal.Cast<float, int>(values); // float values are reinterpreted as int values
            BinaryPrimitives.ReverseEndianness(source, dest);

            _stream.Write(buffer);

            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
        else
        {
            var bytes = MemoryMarshal.AsBytes(values);
            _stream.Write(bytes);
        }
    }

    public void Write(ReadOnlySpan<double> values)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Allocate the buffer on the stack if it's small enough, otherwise rent from the pool
            int byteCount = values.Length * sizeof(double);
            byte[]? rented = byteCount <= MaxStackBufferSize ? null : ArrayPool<byte>.Shared.Rent(byteCount);
            Span<byte> buffer = rented is null ? stackalloc byte[byteCount] : rented;

            // Cast the buffer to Span<short> for byte order conversion
            Span<long> dest = MemoryMarshal.Cast<byte, long>(buffer);
            ReadOnlySpan<long> source = MemoryMarshal.Cast<double, long>(values); // float values are reinterpreted as int values
            BinaryPrimitives.ReverseEndianness(source, dest);

            _stream.Write(buffer);

            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
        else
        {
            var bytes = MemoryMarshal.AsBytes(values);
            _stream.Write(bytes);
        }
    }

    #endregion
}