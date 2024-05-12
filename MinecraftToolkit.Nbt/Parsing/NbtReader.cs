using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Parsing;

public class NbtReader : IDisposable
{
    public Stream Stream { get; init; }

    private BinaryReader _reader;
    private static bool s_needsReversedEndianness = BitConverter.IsLittleEndian;

    public NbtReader(Stream stream, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
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
        Span<byte> valueBuffer = stackalloc byte[8];
        NbtReaderInternal nbtReader = new();

        // Read root tag ID and name
        TagId tagId = ReadTagId();
        if (tagId != TagId.Compound)
            throw new InvalidDataException($"Invalid root tag ID {tagId}");
        string tagName = ReadString();
        if (tagName != "")
            throw new InvalidDataException($"Invalid root tag name {tagName}");

        // Start the state machine
        Stack<(Tag, string?)> stack = new(); // Stack of compound or list tags (string is the name of currentTag if the parent tag is TagCompound)
        Tag currentTag = new TagCompound(); // Start with an empty compound tag for the root tag
        //stack.Push((currentTag, tagName)); // Push the root tag onto the stack
        nbtReader.State = NbtReaderState.ParsingTagCompound;
        while (true)
        {
            if (nbtReader.State == NbtReaderState.ParsingTagList)
            {
                // Pop stack when the current TAG_List is finished
                if (nbtReader.ListRemainingLength == 0)
                {
                    if (stack.Count == 0)
                        break;

                    TagList currentList = (TagList)currentTag;
                    (Tag t, string? n) = stack.Pop();
                    if (t is TagCompound tagCompound) // Finish parsing TagList in a TagCompound
                    {
                        tagCompound[n!] = currentList;
                        currentTag = t;
                        nbtReader.State = NbtReaderState.ParsingTagCompound; // Transition to the state for parsing TagCompound
                        continue;
                    }
                    else if (t is TagList<TagList> tagListList) // Finish parsing TagList in a TagList
                    {
                        tagListList.Add(currentList);
                        currentTag = t;
                        nbtReader.ListItemId = tagListList.ItemId; // Restore the list tag ID of the parent list tag
                        nbtReader.ListRemainingLength = currentList.Capacity - currentList.Count; // Restore the list length of the parent list tag
                        // Stay in the state for parsing TagList
                        continue;
                    }
                    else // Impossible case
                    {
                        throw new InvalidDataException($"Invalid parent tag type {t.GetType()}");
                    }
                }

                // Parse the next list element while nbtReader.ListLength != 0
                if (nbtReader.ListItemId == TagId.Compound) // TagCompound in a TagList<TagCompound>
                {
                    stack.Push((currentTag, null)); // currentTag is a TagList<TagCompound>
                    currentTag = new TagCompound();
                    nbtReader.State = NbtReaderState.ParsingTagCompound; // Transition to the state for parsing TagCompound
                    continue;
                }
                if (nbtReader.ListItemId == TagId.List) // TagList in a TagList<TagList>
                {
                    // Read list tag ID and length
                    TagId listTagId = ReadTagId();
                    int listLength = ReadInt();

                    // Check for invalid and emtpy list
                    if (listLength < 0)
                        throw new InvalidDataException($"Invalid TAG_LIST length {listLength}");
                    if (listLength == 0) // Empty list may be represented as a list of TAG_Byte or TAG_End rather than a list of the correct type
                    {
                        // TODO: Create TagList<T> for the correct type
                        TagList<TagList> tagListList = (TagList<TagList>)currentTag;
                        tagListList.Add(new TagList<Tag>()); // QUESTION: Consider using a global singleton for empty lists?
                        continue; // No need to push stack
                    }

                    // Initialize the TagList
                    TagList tagList = InitializeTagList(listTagId, listLength, ref valueBuffer, out bool isCompleted);

                    if (isCompleted) // TagList<simple tags>
                    {
                        ((TagList<TagList>)currentTag).Add(tagList);
                        nbtReader.ListRemainingLength--; // Decrement the list length of the current list tag
                        continue;
                    }
                    else
                    {
                        // Push the current tag onto the stack and start parsing a TagList<TagCompound> or TagList<TagList>
                        stack.Push((currentTag, null)); // currentTag is a TagList<TagList>
                        nbtReader.State = NbtReaderState.ParsingTagList;
                        nbtReader.ListItemId = listTagId;
                        nbtReader.ListRemainingLength = listLength;
                        continue;
                    }
                }
            }
            else if (nbtReader.State == NbtReaderState.ParsingTagCompound)
            {
                // Read the 1-byte tag type (ID)
                tagId = ReadTagId();

                if (tagId == TagId.End) // End of the current compound tag
                {
                    if (stack.Count == 0)
                        break;

                    // Pop the stack
                    (Tag t, string? n) = stack.Pop();
                    if (t is TagCompound tagCompound) // Finish parsing a TagCompound in a TagCompound
                    {
                        tagCompound[n!] = (TagCompound)currentTag;
                        currentTag = t;
                        continue;
                    }
                    else if (t is TagList<TagCompound> tagListCompound) // Finish parsing a single TagCompound in a TagList<TagCompound>
                    {
                        // QUESTION: For TagList<TagCompound>, can we avoid switching between states for ParsingTagCompound and ParsingTagList?
                        tagListCompound.Add((TagCompound)currentTag);

                        // Transition to the state for parsing TagList
                        nbtReader.ListRemainingLength = tagListCompound.Capacity - tagListCompound.Count;
                        nbtReader.State = NbtReaderState.ParsingTagList;
                        nbtReader.ListItemId = TagId.Compound;
                        currentTag = t;
                        continue;
                    }
                    else // Impossible case
                    {
                        throw new InvalidDataException($"Invalid parent tag type {t.GetType()}");
                    }
                }

                // Read tag name length as big-endian ushort (2 bytes)

                // Read tag name as UTF-8 string
                tagName = ReadString();

                // Read tag payload

                // Push the current tag onto the stack and start parsing a nested TagCompound
                if (tagId == TagId.Compound)
                {
                    stack.Push((currentTag, tagName));
                    currentTag = new TagCompound();
                    nbtReader.State = NbtReaderState.ParsingTagCompound;
                    continue;
                }

                // Either read the payload of a TAG_List (for TagList<simple tags>),
                // or push the current TAG_Compound onto the stack and transition to the state for parsing a TAG_List (for TagList<TagCompound> or TagList<TagList>)
                if (tagId == TagId.List)
                {
                    // Read list tag ID and length
                    TagId listTagId = ReadTagId();
                    int length = ReadInt();

                    // Check for invalid and emtpy list
                    if (length < 0)
                        throw new InvalidDataException($"Invalid TAG_LIST length: {length}");
                    if (length == 0) // Empty list may be represented as a list of TAG_Byte or TAG_End rather than a list of the correct type
                    {
                        // TODO: Create TagList<T> for the correct type
                        ((TagCompound)currentTag)[tagName] = new TagList<Tag>(); // QUESTION: Consider using a global singleton for empty lists?
                        continue; // No need to push stack
                    }

                    // Initialize the TagList
                    TagList tagList = InitializeTagList(listTagId, length, ref valueBuffer, out bool isCompleted);

                    if (isCompleted) // TagList<simple tags>
                    {
                        ((TagCompound)currentTag)[tagName] = tagList;
                        continue;
                    }
                    else
                    {
                        // Push the current tag onto the stack and start parsing a TagList<TagCompound> or TagList<TagList>
                        stack.Push((currentTag, tagName));
                        currentTag = tagList;
                        nbtReader.State = NbtReaderState.ParsingTagList;
                        nbtReader.ListItemId = listTagId;
                        nbtReader.ListRemainingLength = length;
                        continue;
                    }
                }

                // Read the payload for simple tags
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
                    TagId.IntArray => ReadTagIntArray(),
                    TagId.LongArray => ReadTagLongArray(),
                    _ => throw new InvalidDataException($"Invalid tag ID {tagId}")
                };

                // Add simple tag to compound
                ((TagCompound)currentTag)[tagName] = tag;
            }
            else
            {
                throw new InvalidDataException($"Invalid state {nbtReader.State}");
            }
        }
        return (TagCompound)currentTag;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagId ReadTagId()
    {
        byte tagId = _reader.ReadByte();
        TagId tagType = tagId switch
        {
            < TagIdValues.Min or > TagIdValues.Max => throw new InvalidDataException($"Invalid tag ID {tagId}"),
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
    internal TagByteArray ReadTagByteArray()
    {
        int length = ReadInt();
        sbyte[] data = new sbyte[length];
        _reader.Read(Unsafe.As<byte[]>(data), 0, length);
        return new TagByteArray(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagIntArray ReadTagIntArray()
    {
        int length = ReadInt();
        int[] data = new int[length];
        for (int i = 0; i < length; i++) // TODO: Use BinaryPrimitives.ReverseEndianness for the entire array
        {
            int value = ReadInt();
            if (s_needsReversedEndianness)
                value = BinaryPrimitives.ReverseEndianness(value);
            data[i] = value;
        }
        return new TagIntArray(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagLongArray ReadTagLongArray()
    {
        int length = ReadInt();
        long[] data = new long[length];
        Span<byte> buffer = stackalloc byte[8];
        for (int i = 0; i < length; i++)
        {
            long value = _reader.ReadInt64();
            if (s_needsReversedEndianness)
                value = BinaryPrimitives.ReverseEndianness(value);
            data[i] = value;
        }
        return new TagLongArray(data);
    }

    internal TagList InitializeTagList(TagId itemId, int length, ref Span<byte> valueBuffer, out bool isCompleted)
    {
        isCompleted = true; // For TagList<simple tags>, the list is completely parsed; set to false for TagList<TagCompound> and TagList<TagList>

        // TODO: Speed up reading for simple tags since the length is known
        switch (itemId)
        {
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
            case TagId.Compound:
                TagList<TagCompound> compounds = new(length);
                isCompleted = false;
                return compounds;
            case TagId.List:
                TagList<TagList> lists = new(length);
                isCompleted = false;
                return lists;
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
                throw new InvalidDataException($"Invalid TAG_List item ID: {itemId}");
        }
    }

    public void Dispose()
    {
        _reader.Dispose(); // Whether to dispose the underlying stream is handled by the BinaryReader
    }
}
