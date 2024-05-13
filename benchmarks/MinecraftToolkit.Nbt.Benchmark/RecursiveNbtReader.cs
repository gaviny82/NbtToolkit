using MinecraftToolkit.Nbt.Parsing;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Benchmark;

public class RecursiveNbtReader : IDisposable
{
    public Stream Stream { get; init; }

    private BinaryReader _reader;
    private static bool s_needsReversedEndianness = BitConverter.IsLittleEndian;

    public RecursiveNbtReader(Stream stream, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(stream, CompressionMode.Decompress),
            NbtCompression.ZLib => new ZLibStream(stream, CompressionMode.Decompress),
            NbtCompression.None => stream,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };

        _reader = new BinaryReader(Stream, Encoding.UTF8, leaveOpen);
    }

    public TagCompound ReadRootTag()
    {
        TagId tagId = ReadTagId();
        if (tagId != TagId.Compound)
            throw new InvalidDataException($"Invalid root tag ID {tagId}");
        string tagName = ReadString();
        if (tagName != "")
            throw new InvalidDataException($"Invalid root tag name {tagName}");
        return ReadTagCompound();
    }

    /// <summary>
    /// Reads the payload of a compound tag
    /// </summary>
    /// <returns>A <see cref="TagCompound"/> instance</returns>
    public TagCompound ReadTagCompound()
    {
        var tagCompound = new TagCompound();
        do
        {
            // Read the 1-byte tag type (ID)
            TagId tagId = ReadTagId();

            if (tagId == TagId.End) // End of compound tag
                return tagCompound;

            // Read tag name length as big-endian ushort (2 bytes)

            // Read tag name as UTF-8 string
            string tagName = ReadString();

            // Read tag payload
            Tag tag = tagId switch
            {
                TagId.Byte => new TagByte(ReadByte()),
                TagId.Short => new TagShort(ReadShort()),
                TagId.Int => new TagInt(ReadInt()),
                TagId.Long => new TagLong(ReadLong()),
                TagId.Float => new TagFloat(ReadFloat()),
                TagId.Double => new TagDouble(ReadDouble()),
                TagId.ByteArray => ReadTagByteArray(),
                TagId.String => new TagString(ReadString()),
                TagId.List => ReadTagList(), // May be recursive
                TagId.Compound => ReadTagCompound(), // Recursive
                TagId.IntArray => ReadTagIntArray(),
                TagId.LongArray => ReadTagLongArray(),
                _ => throw new InvalidDataException($"Invalid tag ID {tagId}")
            };

            // Add tag to compound
            tagCompound[tagName] = tag;
        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagId ReadTagId()
    {
        byte tagId = _reader.ReadByte();
        TagId tagType = tagId switch
        {
            < (int)TagId.End or > (int)TagId.LongArray => throw new InvalidDataException($"Invalid tag ID {tagId}"),
            _ => (TagId)tagId
        };
        return tagType;
    }

    #region Read values with endianness conversion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal sbyte ReadByte()
    {
        return (sbyte)_reader.ReadByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ushort ReadUShort()
    {
        ushort value = _reader.ReadUInt16();
        if (s_needsReversedEndianness)
            value = BinaryPrimitives.ReverseEndianness(value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal short ReadShort()
    {
        short value = _reader.ReadInt16();
        if (s_needsReversedEndianness)
            value = BinaryPrimitives.ReverseEndianness(value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int ReadInt()
    {
        int value = _reader.ReadInt32();
        if (s_needsReversedEndianness)
            value = BinaryPrimitives.ReverseEndianness(value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal long ReadLong()
    {
        long value = _reader.ReadInt64();
        if (s_needsReversedEndianness)
            value = BinaryPrimitives.ReverseEndianness(value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float ReadFloat()
    {
        float value = _reader.ReadSingle();
        if (s_needsReversedEndianness)
        {
            uint bits = Unsafe.As<float, uint>(ref value);
            value = BinaryPrimitives.ReverseEndianness(bits);
        }
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double ReadDouble()
    {
        double value = _reader.ReadDouble();
        if (s_needsReversedEndianness)
        {
            ulong bits = Unsafe.As<double, ulong>(ref value);
            value = BinaryPrimitives.ReverseEndianness(bits);
        }
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagByteArray ReadTagByteArray()
    {
        int length = ReadInt();
        sbyte[] data = new sbyte[length];
        _reader.Read(MemoryMarshal.AsBytes<sbyte>(data));
        return new TagByteArray(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagIntArray ReadTagIntArray()
    {
        int length = ReadInt();
        int[] data = new int[length];
        _reader.Read(MemoryMarshal.AsBytes<int>(data));

        if (s_needsReversedEndianness)
            BinaryPrimitives.ReverseEndianness(data, data);

        return new TagIntArray(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagLongArray ReadTagLongArray()
    {
        int length = ReadInt();
        long[] data = new long[length];
        _reader.Read(MemoryMarshal.AsBytes<long>(data));
        if (s_needsReversedEndianness)
            BinaryPrimitives.ReverseEndianness(data, data);
        return new TagLongArray(data);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal string ReadString()
    {
        ushort length = ReadUShort();
        // The number of characters in the string is unknown due to the UTF-8 encoding, so string.Create<T> cannot be used
        // TODO: Use modified UTF-8 encoding
        return Encoding.UTF8.GetString(_reader.ReadBytes(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Tag ReadTagList()
    {
        TagId tagId = ReadTagId();
        int length = ReadInt();
        if (length < 0)
            throw new InvalidDataException($"Invalid TAG_LIST length {length}");
        if (length == 0)
            return new TagList<Tag>(0); // Empty list
        switch (tagId)
        {
            case TagId.End:
                throw new InvalidDataException("Invalid TAG_LIST tag ID End");
            case TagId.Byte:
                TagList<sbyte> sbytes = new(length);
                for (int i = 0; i < length; i++)
                    sbytes.Add(ReadByte());
                return sbytes;
            case TagId.Short:
                TagList<short> shorts = new(length);
                for (int i = 0; i < length; i++)
                    shorts.Add(ReadShort());
                return shorts;
            case TagId.Int:
                TagList<int> ints = new(length);
                for (int i = 0; i < length; i++)
                    ints.Add(ReadInt());
                return ints;
            case TagId.Long:
                TagList<long> longs = new(length);
                for (int i = 0; i < length; i++)
                    longs.Add(ReadLong());
                return longs;
            case TagId.Float:
                TagList<float> floats = new(length);
                for (int i = 0; i < length; i++)
                    floats.Add(ReadFloat());
                return floats;
            case TagId.Double:
                TagList<double> doubles = new(length);
                for (int i = 0; i < length; i++)
                    doubles.Add(ReadDouble());
                return doubles;
            case TagId.ByteArray:
                TagList<TagByteArray> byteArrays = new(length);
                for (int i = 0; i < length; i++)
                    byteArrays.Add(ReadTagByteArray());
                return byteArrays;
            case TagId.String:
                TagList<string> strings = new(length);
                for (int i = 0; i < length; i++)
                    strings.Add(ReadString());
                return strings;
            case TagId.List:
                TagList<TagList<Tag>> lists = new(length);
                for (int i = 0; i < length; i++)
                    lists.Add((TagList<Tag>)ReadTagList());
                return lists;
            case TagId.Compound:
                TagList<TagCompound> compounds = new(length);
                for (int i = 0; i < length; i++)
                    compounds.Add(ReadTagCompound());
                return compounds;
            case TagId.IntArray:
                TagList<TagIntArray> intArrays = new(length);
                for (int i = 0; i < length; i++)
                    intArrays.Add(ReadTagIntArray());
                return intArrays;
            case TagId.LongArray:
                TagList<TagLongArray> longArrays = new(length);
                for (int i = 0; i < length; i++)
                    longArrays.Add(ReadTagLongArray());
                return longArrays;
            default:
                throw new InvalidDataException($"Invalid tag ID {tagId}");
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
    }
}