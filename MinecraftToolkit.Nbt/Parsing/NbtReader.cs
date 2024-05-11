using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Parsing;

public class NbtReader
{
    public Stream Stream { get; init; }

    private BinaryReader _reader;
    private bool _needsReversedByteOrder = BitConverter.IsLittleEndian;

    public NbtReader(Stream stream, NbtCompression compression = NbtCompression.None)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(stream, CompressionMode.Decompress),
            NbtCompression.ZLib => new ZLibStream(stream, CompressionMode.Decompress),
            NbtCompression.None => stream,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };

        _reader = new BinaryReader(Stream, Encoding.UTF8);
    }

    public TagCompound ReadRootTag()
    {
        Span<byte> valueBuffer = stackalloc byte[8];
        NbtReaderInternal nbtReader = new(ref valueBuffer);

        // Read root tag ID and name
        TagId tagId = ReadTagId();
        if (tagId != TagId.Compound)
            throw new InvalidDataException($"Invalid root tag ID {tagId}");
        string tagName = ReadString(ref valueBuffer);
        if (tagName != "")
            throw new InvalidDataException($"Invalid root tag name {tagName}");

        // Start the state machine
        Stack<(Tag, string?)> stack = new(); // Stack of compound or list tags (string is the name of currentTag if the parent tag is TagCompound)
        Tag currentTag = new TagCompound(); // Start with an empty compound tag for the root tag
        stack.Push((currentTag, tagName)); // Push the root tag onto the stack
        nbtReader.State = NbtReaderState.ParsingTagCompound;
        while (true)
        {
            if (stack.Count == 0)
                break;

            if (nbtReader.State == NbtReaderState.ParsingTagList)
            {
                // Pop stack when the current TAG_List is finished
                if (nbtReader.ListRemainingLength == 0)
                {
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
                        nbtReader.ListTagId = tagListList.ItemId; // Restore the list tag ID of the parent list tag
                        nbtReader.ListRemainingLength = currentList.Capacity - currentList.Count; // Restore the list length of the parent list tag
                        // Stay in the state for parsing TagList
                        continue;
                    }
                    else if (t is TagList tagListSimple) // Finish parsing a TagList of simple tags or a TagList<TagCompound>
                    {
                        currentTag = t;
                        nbtReader.ListTagId = tagListSimple.ItemId; // Restore the list tag ID of the parent list tag
                        // Stay in the state for parsing TagList
                        continue;
                    }
                    else // Impossible case
                    {
                        throw new InvalidDataException($"Invalid parent tag type {t.GetType()}");
                    }
                }

                // Parse the next list element while nbtReader.ListLength != 0
                if (nbtReader.ListTagId == TagId.Compound) // TagCompound in a TagList<TagCompound>
                {
                    // TODO: Move this to branch of parsing TagCompound
                    if (currentTag is TagCompound tagCompound)
                    {
                        stack.Push((currentTag, tagName));
                    }
                    else if (currentTag is TagList<TagCompound> tagListCompound)
                    {
                        stack.Push((currentTag, null));
                    }
                    else
                    {
                        throw new InvalidDataException($"Invalid parent tag type {currentTag.GetType()}");
                    }
                    currentTag = new TagCompound();
                    nbtReader.State = NbtReaderState.ParsingTagCompound; // Transition to the state for parsing TagCompound
                    continue;
                }
                if (nbtReader.ListTagId == TagId.List) // TagList in a TagList<TagList>
                {
                    stack.Push((currentTag, null));
                    // Read list tag ID and length
                    TagId listTagId = ReadTagId();
                    int listLength = ReadInt(ref valueBuffer);
                    if (listLength < 0)
                        throw new InvalidDataException($"Invalid TAG_LIST length {listLength}");
                    if (listLength == 0) // Empty list may be represented as a list of TAG_Byte or TAG_End rather than a list of the correct type
                    {
                        // TODO: Create TagList<T> for the correct type
                        TagList<TagList> tagListList = (TagList<TagList>)currentTag;
                        tagListList[tagListList.Capacity - nbtReader.ListRemainingLength] = new TagList<Tag>(); // QUESTION: Consider using a global singleton for empty lists?
                        continue; // No need to push stack
                    }

                    // Push the current tag onto the stack and start parsing the TagList
                    nbtReader.State = NbtReaderState.ParsingTagList; // Remain in the state for parsing TagList
                    nbtReader.ListTagId = listTagId;
                    nbtReader.ListRemainingLength = listLength;
                    continue;
                }

                // Other simple tags // Consider moving this part to a separate method and call it in the branch for parsing TagCompound
                int length = nbtReader.ListRemainingLength;
                switch (nbtReader.ListTagId)
                {
                    case TagId.End:
                        throw new InvalidDataException("Invalid TAG_LIST tag ID End");
                    case TagId.Byte:
                        TagList<sbyte> sbytes = new(length);
                        for (int i = 0; i < length; i++)
                            sbytes.Add(ReadByte());
                        currentTag = sbytes;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.Short:
                        TagList<short> shorts = new(length);
                        for (int i = 0; i < length; i++)
                            shorts.Add(ReadShort(ref valueBuffer));
                        currentTag = shorts;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.Int:
                        TagList<int> ints = new(length);
                        for (int i = 0; i < length; i++)
                            ints.Add(ReadInt(ref valueBuffer));
                        currentTag = ints;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.Long:
                        TagList<long> longs = new(length);
                        for (int i = 0; i < length; i++)
                            longs.Add(ReadLong(ref valueBuffer));
                        currentTag = longs;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.Float:
                        TagList<float> floats = new(length);
                        for (int i = 0; i < length; i++)
                            floats.Add(ReadFloat(ref valueBuffer));
                        currentTag = floats;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.Double:
                        TagList<double> doubles = new(length);
                        for (int i = 0; i < length; i++)
                            doubles.Add(ReadDouble(ref valueBuffer));
                        currentTag = doubles;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.ByteArray:
                        TagList<TagByteArray> byteArrays = new(length);
                        for (int i = 0; i < length; i++)
                            byteArrays.Add(ReadTagByteArray(ref valueBuffer));
                        currentTag = byteArrays;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.String:
                        TagList<string> strings = new(length);
                        for (int i = 0; i < length; i++)
                            strings.Add(ReadString(ref valueBuffer));
                        currentTag = strings;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.IntArray:
                        TagList<TagIntArray> intArrays = new(length);
                        for (int i = 0; i < length; i++)
                            intArrays.Add(ReadTagIntArray(ref valueBuffer));
                        currentTag = intArrays;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    case TagId.LongArray:
                        TagList<TagLongArray> longArrays = new(length);
                        for (int i = 0; i < length; i++)
                            longArrays.Add(ReadTagLongArray(ref valueBuffer));
                        currentTag = longArrays;
                        nbtReader.ListRemainingLength = 0;
                        continue;
                    default:
                        throw new InvalidDataException($"Invalid tag ID {tagId}");
                }
            }
            else if (nbtReader.State == NbtReaderState.ParsingTagCompound)
            {
                // Read the 1-byte tag type (ID)
                tagId = ReadTagId();

                if (tagId == TagId.End) // End of the current compound tag
                {
                    // Pop the stack
                    (Tag t, string? n) = stack.Pop();
                    if (t is TagCompound tagCompound) // Finish parsing a TagCompound in a TagCompound
                    {
                        tagCompound[n!] = (TagCompound)currentTag;
                        currentTag = t;
                        continue;
                    }
                    else if (t is TagList<TagCompound> tagListCompound) // Finish parsing a TagCompound in a TagList
                    {
                        tagListCompound.Add((TagCompound)currentTag);
                        currentTag = t;
                        nbtReader.ListRemainingLength--; // Decrement the list length of the parent list tag
                        continue;
                    }
                    else // Impossible case
                    {
                        throw new InvalidDataException($"Invalid parent tag type {t.GetType()}");
                    }
                }

                // Read tag name length as big-endian ushort (2 bytes)

                // Read tag name as UTF-8 string
                tagName = ReadString(ref valueBuffer);

                // Read tag payload

                // Push the current tag onto the stack and start parsing a nested TagCompound
                if (tagId == TagId.Compound)
                {
                    stack.Push((currentTag, tagName));
                    currentTag = new TagCompound();
                    nbtReader.State = NbtReaderState.ParsingTagCompound;
                    continue;
                }

                // Push the current tag onto the stack for parsing a TagList (if needed)
                if (tagId == TagId.List)
                {
                    // Read list tag ID and length
                    TagId listTagId = ReadTagId();
                    int length = ReadInt(ref valueBuffer);

                    // Check for invalid and emtpy list
                    if (length < 0)
                        throw new InvalidDataException($"Invalid TAG_LIST length {length}");
                    if (length == 0) // Empty list may be represented as a list of TAG_Byte or TAG_End rather than a list of the correct type
                    {
                        // TODO: Create TagList<T> for the correct type
                        ((TagCompound)currentTag)[tagName] = new TagList<Tag>(); // QUESTION: Consider using a global singleton for empty lists?
                        continue; // No need to push stack
                    }

                    // Push the current tag onto the stack and start parsing the TagList
                    stack.Push((currentTag, tagName));
                    nbtReader.State = NbtReaderState.ParsingTagList;
                    nbtReader.ListTagId = listTagId;
                    nbtReader.ListRemainingLength = length;
                    continue;
                }

                // Read the payload for simple tags
                Tag tag = tagId switch
                {
                    TagId.Byte => new TagByte(ReadByte()),
                    TagId.Short => new TagShort(ReadShort(ref valueBuffer)),
                    TagId.Int => new TagInt(ReadInt(ref valueBuffer)),
                    TagId.Long => new TagLong(ReadLong(ref valueBuffer)),
                    TagId.Float => new TagFloat(ReadFloat(ref valueBuffer)),
                    TagId.Double => new TagDouble(ReadDouble(ref valueBuffer)),
                    TagId.ByteArray => ReadTagByteArray(ref valueBuffer),
                    TagId.String => new TagString(ReadString(ref valueBuffer)),
                    TagId.IntArray => ReadTagIntArray(ref valueBuffer),
                    TagId.LongArray => ReadTagLongArray(ref valueBuffer),
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal sbyte ReadByte()
    {
        return (sbyte)_reader.ReadByte();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ushort ReadUShort(ref Span<byte> valueBuffer)
    {
        Span<byte> buffer = valueBuffer[0..2];
        _reader.Read(buffer);
        if (_needsReversedByteOrder)
            buffer.Reverse();
        return BitConverter.ToUInt16(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal short ReadShort(ref Span<byte> valueBuffer)
    {
        Span<byte> buffer = valueBuffer[0..2];
        _reader.Read(buffer);
        if (_needsReversedByteOrder)
            buffer.Reverse();
        return BitConverter.ToInt16(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int ReadInt(ref Span<byte> valueBuffer)
    {
        Span<byte> buffer = valueBuffer[0..4];
        _reader.Read(buffer);
        if (_needsReversedByteOrder)
            buffer.Reverse();
        return BitConverter.ToInt32(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal long ReadLong(ref Span<byte> valueBuffer)
    {
        _reader.Read(valueBuffer);
        if (_needsReversedByteOrder)
            valueBuffer.Reverse();
        return BitConverter.ToInt64(valueBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float ReadFloat(ref Span<byte> valueBuffer)
    {
        Span<byte> buffer = valueBuffer[0..4];
        _reader.Read(buffer);
        if (_needsReversedByteOrder)
            buffer.Reverse();
        return BitConverter.ToSingle(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double ReadDouble(ref Span<byte> valueBuffer)
    {
        _reader.Read(valueBuffer);
        if (_needsReversedByteOrder)
            valueBuffer.Reverse();
        return BitConverter.ToDouble(valueBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal string ReadString(ref Span<byte> valueBuffer)
    {
        ushort length = ReadUShort(ref valueBuffer);
        // The number of characters in the string is unknown due to the UTF-8 encoding, so string.Create<T> cannot be used
        // TODO: Use modified UTF-8 encoding
        return Encoding.UTF8.GetString(_reader.ReadBytes(length));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagByteArray ReadTagByteArray(ref Span<byte> valueBuffer)
    {
        int length = ReadInt(ref valueBuffer);
        sbyte[] data = new sbyte[length];
        _reader.Read(Unsafe.As<byte[]>(data), 0, length);
        return new TagByteArray(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagIntArray ReadTagIntArray(ref Span<byte> valueBuffer)
    {
        int length = ReadInt(ref valueBuffer);
        int[] data = new int[length];
        Span<byte> buffer = stackalloc byte[4];
        for (int i = 0; i < length; i++)
        {
            _reader.Read(buffer);
            if (_needsReversedByteOrder)
                buffer.Reverse();
            data[i] = BitConverter.ToInt32(buffer);
        }
        return new TagIntArray(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal TagLongArray ReadTagLongArray(ref Span<byte> valueBuffer)
    {
        int length = ReadInt(ref valueBuffer);
        long[] data = new long[length];
        Span<byte> buffer = stackalloc byte[8];
        for (int i = 0; i < length; i++)
        {
            _reader.Read(buffer);
            if (_needsReversedByteOrder)
                buffer.Reverse();
            data[i] = BitConverter.ToInt64(buffer);
        }
        return new TagLongArray(data);
    }
}
