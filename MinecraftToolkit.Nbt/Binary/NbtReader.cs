using MinecraftToolkit.Nbt.Binary;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Binary;

public partial class NbtReader : IDisposable
{
    public Stream Stream { get; init; }

    private readonly DefaultEndiannessBinaryReader _reader;

    public NbtReader(Stream stream, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(stream, CompressionMode.Decompress),
            NbtCompression.ZLib => new ZLibStream(stream, CompressionMode.Decompress),
            NbtCompression.None => stream,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };
        _reader = BitConverter.IsLittleEndian
            ? new ReversedEndiannessBinaryReader(Stream, Encoding.UTF8, leaveOpen)
            : new DefaultEndiannessBinaryReader(Stream, Encoding.UTF8, leaveOpen);
    }

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
                TagId.ByteArray => new TagByteArray(_reader.ReadSByteArray()),
                TagId.String => new TagString(_reader.ReadString()),
                TagId.List => ReadTagList(), // May be recursive
                TagId.Compound => ReadTagCompound(), // Recursive
                TagId.IntArray => new TagIntArray(_reader.ReadInt32Array()),
                TagId.LongArray => new TagLongArray(_reader.ReadInt64Array()),
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                TagList<sbyte> sbytes = new(length);
                var sbytesByteSpan = MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(sbytes._items));
                _reader.Read(sbytesByteSpan);
                return sbytes;
            case TagId.Short:
                TagList<short> shorts = new(length);
                var shortsSpan = CollectionsMarshal.AsSpan(shorts._items);
                var shortsByteSpan = MemoryMarshal.AsBytes(shortsSpan);
                _reader.Read(shortsByteSpan);
                BinaryPrimitives.ReverseEndianness(shortsSpan, shortsSpan);
                return shorts;
            case TagId.Int:
                TagList<int> ints = new(length);
                var intsSpan = CollectionsMarshal.AsSpan(ints._items);
                var intsByteSpan = MemoryMarshal.AsBytes(intsSpan);
                _reader.Read(intsByteSpan);
                BinaryPrimitives.ReverseEndianness(intsSpan, intsSpan);
                return ints;
            case TagId.Long:
                TagList<long> longs = new(length);
                var longsSpan = CollectionsMarshal.AsSpan(longs._items);
                var longsByteSpan = MemoryMarshal.AsBytes(longsSpan);
                _reader.Read(longsByteSpan);
                BinaryPrimitives.ReverseEndianness(longsSpan, longsSpan);
                return longs;
            case TagId.Float:
                TagList<float> floats = new(length);
                var floatsSpan = CollectionsMarshal.AsSpan(floats._items);
                var floatsByteSpan = MemoryMarshal.AsBytes(floatsSpan);
                _reader.Read(floatsByteSpan);
                Span<int> floatsAsIntSpan = MemoryMarshal.Cast<float, int>(floatsSpan);
                BinaryPrimitives.ReverseEndianness(floatsAsIntSpan, floatsAsIntSpan);
                return floats;
            case TagId.Double:
                TagList<double> doubles = new(length);
                var doublesSpan = CollectionsMarshal.AsSpan(doubles._items);
                var doublesByteSpan = MemoryMarshal.AsBytes(doublesSpan);
                _reader.Read(doublesByteSpan);
                Span<long> doublesAsLongSpan = MemoryMarshal.Cast<double, long>(doublesSpan);
                BinaryPrimitives.ReverseEndianness(doublesAsLongSpan, doublesAsLongSpan);
                return doubles;
            case TagId.ByteArray:
                TagList<TagByteArray> byteArrays = new(length);
                for (int i = 0; i < length; i++)
                    byteArrays.Add(new TagByteArray(_reader.ReadSByteArray()));
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
                TagList<TagIntArray> intArrays = new(length);
                for (int i = 0; i < length; i++)
                    intArrays.Add(new TagIntArray(_reader.ReadInt32Array()));
                return intArrays;
            case TagId.LongArray:
                TagList<TagLongArray> longArrays = new(length);
                for (int i = 0; i < length; i++)
                    longArrays.Add(new TagLongArray(_reader.ReadInt64Array()));
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