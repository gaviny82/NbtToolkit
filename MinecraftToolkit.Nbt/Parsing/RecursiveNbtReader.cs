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
using static MinecraftToolkit.Nbt.Parsing.NbtReader;

namespace MinecraftToolkit.Nbt.Parsing;

public class RecursiveNbtReader : IDisposable
{
    public Stream Stream { get; init; }

    private readonly NbtBinaryReader _reader;

    public RecursiveNbtReader(Stream stream, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(stream, CompressionMode.Decompress),
            NbtCompression.ZLib => new ZLibStream(stream, CompressionMode.Decompress),
            NbtCompression.None => stream,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };
        _reader = BitConverter.IsLittleEndian
            ? new ReversedEndiannessNbtBinaryReader(Stream, Encoding.UTF8, leaveOpen)
            : new NbtBinaryReader(Stream, Encoding.UTF8, leaveOpen);
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
    internal Tag ReadTagList()
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
                for (int i = 0; i < length; i++)
                    sbytes.Add(_reader.ReadSByte());
                return sbytes;
            case TagId.Short:
                TagList<short> shorts = new(length);
                for (int i = 0; i < length; i++)
                    shorts.Add(_reader.ReadInt16());
                return shorts;
            case TagId.Int:
                TagList<int> ints = new(length);
                for (int i = 0; i < length; i++)
                    ints.Add(_reader.ReadInt32());
                return ints;
            case TagId.Long:
                TagList<long> longs = new(length);
                for (int i = 0; i < length; i++)
                    longs.Add(_reader.ReadInt64());
                return longs;
            case TagId.Float:
                TagList<float> floats = new(length);
                for (int i = 0; i < length; i++)
                    floats.Add(_reader.ReadSingle());
                return floats;
            case TagId.Double:
                TagList<double> doubles = new(length);
                for (int i = 0; i < length; i++)
                    doubles.Add(_reader.ReadDouble());
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