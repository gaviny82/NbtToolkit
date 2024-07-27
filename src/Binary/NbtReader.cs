using NbtToolkit.Binary;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit.Binary;

public partial class NbtReader : IDisposable
{
    /// <summary>
    /// The <see cref="System.IO.Stream"/> from which this <see cref="NbtReader"/> reads.
    /// </summary>
    public Stream Stream { get; init; }

    private readonly NbtBinaryReader _reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="NbtReader"/> class for the specified stream and detect the compression type automatically.
    /// </summary>
    /// <param name="input">The <see cref="System.IO.Stream"/> from which this <see cref="NbtReader"/> reads.</param>
    /// <param name="leaveOpen">If the <paramref name="input"/> stream is left open when this <see cref="NbtReader"/> is disposed.</param>
    /// <exception cref="EndOfStreamException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="FormatException"></exception>
    public NbtReader(Stream input, bool leaveOpen = false)
        : this(input, DetectCompressionType(input), leaveOpen) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NbtReader"/> class for the specified stream.
    /// </summary>
    /// <param name="input">The <see cref="System.IO.Stream"/> from which this <see cref="NbtReader"/> reads.</param>
    /// <param name="compression">Compression type of the data written to the <paramref name="input"/> stream.</param>
    /// <param name="leaveOpen">If the <paramref name="input"/> stream is left open when this <see cref="NbtReader"/> is disposed.</param>
    /// <exception cref="ArgumentException">Invalid <see cref="NbtCompression"/> provided by <paramref name="compression"/>.</exception>
    public NbtReader(Stream input, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new BufferedStream(new GZipStream(input, CompressionMode.Decompress)),
            NbtCompression.ZLib => new BufferedStream(new ZLibStream(input, CompressionMode.Decompress)),
            NbtCompression.None => input,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };
        _reader = new NbtBinaryReader(Stream, leaveOpen);
    }

    // Detect the compression type of a NBT file using the first byte
    private static NbtCompression DetectCompressionType(Stream stream)
    {
        int firstByte = stream.ReadByte();
        stream.Seek(-1, SeekOrigin.Current);

        return firstByte switch
        {
            -1 => throw new EndOfStreamException(),
            0x1F => NbtCompression.GZip,
            0x78 => NbtCompression.ZLib,
            0x0A => NbtCompression.None,
            0x09 => throw new NotSupportedException("Bedrock NBT format is currently not supported"),
            _ => throw new FormatException("Invalid NBT format")
        };
    }

    /// <summary>
    /// Reads the root tag as a <see cref="TagCompound"/> from <see cref="Stream"/> using NBT binary format.
    /// </summary>
    /// <returns>The root tag read from <see cref="Stream"/>.</returns>
    /// <exception cref="InvalidDataException">Data in <see cref="Stream"/> has an invalid format.</exception>
    public TagCompound ReadRootTag()
    {
        TagId tagId = ReadTagId();
        if (tagId != TagId.Compound)
            throw new InvalidDataException($"Invalid root tag ID {tagId}");
        string tagName = _reader.ReadString();
        if (tagName != "")
            throw new InvalidDataException($"Invalid root tag name {tagName}");
        return ReadTagCompound();
    }

    internal TagCompound ReadTagCompound()
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
            string tagName = _reader.ReadString();

            // Read tag payload
            Tag tag = tagId switch
            {
                TagId.Byte => new TagByte(_reader.ReadSByte()),
                TagId.Short => new TagShort(_reader.ReadInt16()),
                TagId.Int => new TagInt(_reader.ReadInt32()),
                TagId.Long => new TagLong(_reader.ReadInt64()),
                TagId.Float => new TagFloat(_reader.ReadSingle()),
                TagId.Double => new TagDouble(_reader.ReadDouble()),
                TagId.ByteArray => new TagByteArray(ReadSByteArray()),
                TagId.String => new TagString(_reader.ReadString()),
                TagId.List => ReadTagList(), // May be recursive
                TagId.Compound => ReadTagCompound(), // Recursive
                TagId.IntArray => new TagIntArray(ReadInt32Array()),
                TagId.LongArray => new TagLongArray(ReadInt64Array()),
                _ => throw new InvalidDataException($"Invalid tag ID {tagId}")
            };

            // Add tag to compound
            tagCompound[tagName] = tag;
        } while (true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TagId ReadTagId()
    {
        byte tagId = _reader.ReadByte();
        TagId tagType = tagId switch
        {
            < (int)TagId.End or > (int)TagId.LongArray => throw new InvalidDataException($"Invalid tag ID {tagId}"),
            _ => (TagId)tagId
        };
        return tagType;
    }

    /// <summary>
    /// Reads a signed byte array prefixed by an <see cref="int"/> length
    /// </summary>
    /// <returns>A signed byte array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private sbyte[] ReadSByteArray()
    {
        int length = _reader.ReadInt32();
        sbyte[] data = new sbyte[length];
        _reader.ReadInt8Span(data);
        return data;
    }

    /// <summary>
    /// Reads an int array prefixed by an <see cref="int"/> length
    /// </summary>
    /// <returns>An int array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int[] ReadInt32Array()
    {
        int length = _reader.ReadInt32();
        int[] data = new int[length];
        _reader.ReadInt32Span(data);
        return data;
    }

    /// <summary>
    /// Reads a long array prefixed by an <see cref="int"/> length
    /// </summary>
    /// <returns>A long array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private long[] ReadInt64Array()
    {
        int length = _reader.ReadInt32();
        long[] data = new long[length];
        _reader.ReadInt64Span(data);
        return data;
    }

    internal TagList ReadTagList()
    {
        TagId tagId = ReadTagId();
        int length = _reader.ReadInt32();
        if (length < 0)
            throw new InvalidDataException($"Invalid TAG_LIST length {length}");
        if (length == 0)
            return new TagList<Tag>(0); // Empty list
        switch (tagId)
        {
            case TagId.End:
                throw new InvalidDataException("Invalid TAG_LIST tag ID End");
            case TagId.Byte:
                TagList<sbyte> sbytes = new(0);
                CollectionsMarshal.SetCount(sbytes._items, length);
                _reader.ReadInt8Span(CollectionsMarshal.AsSpan(sbytes._items));
                return sbytes;
            case TagId.Short:
                TagList<short> shorts = new(0);
                CollectionsMarshal.SetCount(shorts._items, length);
                _reader.ReadInt16Span(CollectionsMarshal.AsSpan(shorts._items));
                return shorts;
            case TagId.Int:
                TagList<int> ints = new(0);
                CollectionsMarshal.SetCount(ints._items, length);
                _reader.ReadInt32Span(CollectionsMarshal.AsSpan(ints._items));
                return ints;
            case TagId.Long:
                TagList<long> longs = new(0);
                CollectionsMarshal.SetCount(longs._items, length);
                _reader.ReadInt64Span(CollectionsMarshal.AsSpan(longs._items));
                return longs;
            case TagId.Float:
                TagList<float> floats = new(0);
                CollectionsMarshal.SetCount(floats._items, length);
                _reader.ReadSingleSpan(CollectionsMarshal.AsSpan(floats._items));
                return floats;
            case TagId.Double:
                TagList<double> doubles = new(0);
                CollectionsMarshal.SetCount(doubles._items, length);
                _reader.ReadDoubleSpan(CollectionsMarshal.AsSpan(doubles._items));
                return doubles;
            case TagId.ByteArray:
                TagList<sbyte[]> byteArrays = new(length);
                for (int i = 0; i < length; i++)
                    byteArrays.Add(ReadSByteArray());
                return byteArrays;
            case TagId.String:
                TagList<string> strings = new(length);
                for (int i = 0; i < length; i++)
                    strings.Add(_reader.ReadString());
                return strings;
            case TagId.List:
                TagList<TagList> lists = new(length);
                for (int i = 0; i < length; i++)
                    lists.Add(ReadTagList());
                return lists;
            case TagId.Compound:
                TagList<TagCompound> compounds = new(length);
                for (int i = 0; i < length; i++)
                    compounds.Add(ReadTagCompound());
                return compounds;
            case TagId.IntArray:
                TagList<int[]> intArrays = new(length);
                for (int i = 0; i < length; i++)
                    intArrays.Add(ReadInt32Array());
                return intArrays;
            case TagId.LongArray:
                TagList<long[]> longArrays = new(length);
                for (int i = 0; i < length; i++)
                    longArrays.Add(ReadInt64Array());
                return longArrays;
            default:
                throw new InvalidDataException($"Invalid tag ID {tagId}");
        }
    }

    /// <summary>
    /// Disposes the <see cref="NbtReader"/> and the underlying <see cref="System.IO.Stream"/> (unless it is configured to be left open).
    /// </summary>
    public void Dispose()
    {
        _reader.Dispose();
    }
}
