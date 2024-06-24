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
using MinecraftToolkit.Nbt.Binary;
using static MinecraftToolkit.Nbt.Binary.NbtReader;

namespace MinecraftToolkit.Nbt.Benchmark;

internal ref struct NbtReaderStateMachine
{
    public NbtReaderState State;
    public TagId ListItemId; // Tag ID of a list (only used when parsing a list)
    public int ListRemainingLength; // Length of a list (only used when parsing a list)
    public Tag CurrentTag;
    public Stack<(Tag, string?)> Stack = new(); // Stack of compound or list tags (string is the name of currentTag if the parent tag is TagCompound; otherwise, null)

    public NbtReaderStateMachine(NbtReaderState initialState, Tag rootTag)
    {
        State = initialState;
        CurrentTag = rootTag;
        ListItemId = TagId.End;
        ListRemainingLength = 0;
    }
}

internal enum NbtReaderState
{
    Start,
    ParsingTagCompound,
    ParsingTagList
}

public class NbtReader_StateMachineImpl : IDisposable
{
    public Stream Stream { get; init; }

    private readonly DefaultEndiannessBinaryReader _reader;

    public NbtReader_StateMachineImpl(Stream stream, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
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
        // Read root tag ID and name
        TagId tagId = ReadTagId();
        if (tagId != TagId.Compound)
            throw new InvalidDataException($"Invalid root tag ID {tagId}");
        string tagName = _reader.ReadString();
        if (tagName != "")
            throw new InvalidDataException($"Invalid root tag name {tagName}");

        // Start the state machine
        // Start with an empty compound tag for the root tag
        NbtReaderStateMachine stateMachine = new(NbtReaderState.ParsingTagCompound, new TagCompound());
        while (true)
        {
            if (stateMachine.State == NbtReaderState.ParsingTagList)
            {
                // Pop stack when the current TAG_List is finished
                if (stateMachine.ListRemainingLength == 0)
                {
                    if (stateMachine.Stack.Count == 0)
                        break;

                    TagList currentList = (TagList)stateMachine.CurrentTag;
                    (Tag t, string? n) = stateMachine.Stack.Pop();
                    if (t is TagCompound tagCompound) // Finish parsing TagList in a TagCompound
                    {
                        tagCompound[n!] = currentList;
                        stateMachine.CurrentTag = t;
                        stateMachine.State = NbtReaderState.ParsingTagCompound; // Transition to the state for parsing TagCompound
                        continue;
                    }
                    else if (t is TagList<TagList> tagListList) // Finish parsing TagList in a TagList
                    {
                        tagListList.Add(currentList);
                        stateMachine.CurrentTag = t;
                        stateMachine.ListItemId = tagListList.ItemId; // Restore the list tag ID of the parent list tag
                        stateMachine.ListRemainingLength = currentList.Capacity - currentList.Count; // Restore the list length of the parent list tag
                        // Stay in the state for parsing TagList
                        continue;
                    }
                    else // Impossible case
                    {
                        throw new InvalidDataException($"Invalid parent tag type {t.GetType()}");
                    }
                }

                // Parse the next list element while nbtReader.ListLength != 0
                if (stateMachine.ListItemId == TagId.Compound) // TagCompound in a TagList<TagCompound>
                {
                    stateMachine.Stack.Push((stateMachine.CurrentTag, null)); // currentTag is a TagList<TagCompound>
                    stateMachine.CurrentTag = new TagCompound();
                    stateMachine.State = NbtReaderState.ParsingTagCompound; // Transition to the state for parsing TagCompound
                    continue;
                }
                if (stateMachine.ListItemId == TagId.List) // TagList in a TagList<TagList>
                {
                    // Read list tag ID and length
                    TagId listTagId = ReadTagId();
                    int listLength = _reader.ReadInt32();

                    // Check for invalid and emtpy list
                    if (listLength < 0)
                        throw new InvalidDataException($"Invalid TAG_LIST length {listLength}");
                    if (listLength == 0) // Empty list may be represented as a list of TAG_Byte or TAG_End rather than a list of the correct type
                    {
                        // TODO: Create TagList<T> for the correct type
                        TagList<TagList> tagListList = (TagList<TagList>)stateMachine.CurrentTag;
                        tagListList.Add(new TagList<Tag>()); // QUESTION: Consider using a global singleton for empty lists?
                        continue; // No need to push stack
                    }

                    // Initialize the TagList
                    TagList tagList = ReadTagList(listTagId, listLength, out bool isCompleted);

                    if (isCompleted) // TagList<simple tags>
                    {
                        ((TagList<TagList>)stateMachine.CurrentTag).Add(tagList);
                        stateMachine.ListRemainingLength--; // Decrement the list length of the current list tag
                        continue;
                    }
                    else
                    {
                        // Push the current tag onto the stack and start parsing a TagList<TagCompound> or TagList<TagList>
                        stateMachine.Stack.Push((stateMachine.CurrentTag, null)); // currentTag is a TagList<TagList>
                        stateMachine.State = NbtReaderState.ParsingTagList;
                        stateMachine.ListItemId = listTagId;
                        stateMachine.ListRemainingLength = listLength;
                        continue;
                    }
                }
            }
            else if (stateMachine.State == NbtReaderState.ParsingTagCompound)
            {
                // Read the 1-byte tag type (ID)
                tagId = ReadTagId();

                if (tagId == TagId.End) // End of the current compound tag
                {
                    if (stateMachine.Stack.Count == 0)
                        break;

                    // Pop the stack
                    (Tag t, string? n) = stateMachine.Stack.Pop();
                    if (t is TagCompound tagCompound) // Finish parsing a TagCompound in a TagCompound
                    {
                        tagCompound[n!] = (TagCompound)stateMachine.CurrentTag;
                        stateMachine.CurrentTag = t;
                        continue;
                    }
                    else if (t is TagList<TagCompound> tagListCompound) // Finish parsing a single TagCompound in a TagList<TagCompound>
                    {
                        // QUESTION: For TagList<TagCompound>, can we avoid switching between states for ParsingTagCompound and ParsingTagList?
                        tagListCompound.Add((TagCompound)stateMachine.CurrentTag);

                        // Transition to the state for parsing TagList
                        stateMachine.ListRemainingLength = tagListCompound.Capacity - tagListCompound.Count;
                        stateMachine.State = NbtReaderState.ParsingTagList;
                        stateMachine.ListItemId = TagId.Compound;
                        stateMachine.CurrentTag = t;
                        continue;
                    }
                    else // Impossible case
                    {
                        throw new InvalidDataException($"Invalid parent tag type {t.GetType()}");
                    }
                }

                // Read tag name length as big-endian ushort (2 bytes)

                // Read tag name as UTF-8 string
                tagName = _reader.ReadString();

                // Read tag payload

                // Push the current tag onto the stack and start parsing a nested TagCompound
                if (tagId == TagId.Compound)
                {
                    stateMachine.Stack.Push((stateMachine.CurrentTag, tagName));
                    stateMachine.CurrentTag = new TagCompound();
                    stateMachine.State = NbtReaderState.ParsingTagCompound;
                    continue;
                }

                // Either read the payload of a TAG_List (for TagList<simple tags>),
                // or push the current TAG_Compound onto the stack and transition to the state for parsing a TAG_List (for TagList<TagCompound> or TagList<TagList>)
                if (tagId == TagId.List)
                {
                    // Read list tag ID and length
                    TagId listTagId = ReadTagId();
                    int length = _reader.ReadInt32();

                    // Check for invalid and emtpy list
                    if (length < 0)
                        throw new InvalidDataException($"Invalid TAG_LIST length: {length}");
                    if (length == 0) // Empty list may be represented as a list of TAG_Byte or TAG_End rather than a list of the correct type
                    {
                        // TODO: Create TagList<T> for the correct type
                        ((TagCompound)stateMachine.CurrentTag)[tagName] = new TagList<Tag>(); // QUESTION: Consider using a global singleton for empty lists?
                        continue; // No need to push stack
                    }

                    // Initialize the TagList
                    TagList tagList = ReadTagList(listTagId, length, out bool isCompleted);

                    if (isCompleted) // TagList<simple tags>
                    {
                        ((TagCompound)stateMachine.CurrentTag)[tagName] = tagList;
                        continue;
                    }
                    else
                    {
                        // Push the current tag onto the stack and start parsing a TagList<TagCompound> or TagList<TagList>
                        stateMachine.Stack.Push((stateMachine.CurrentTag, tagName));
                        stateMachine.CurrentTag = tagList;
                        stateMachine.State = NbtReaderState.ParsingTagList;
                        stateMachine.ListItemId = listTagId;
                        stateMachine.ListRemainingLength = length;
                        continue;
                    }
                }

                // Read the payload for simple tags
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
                    TagId.IntArray => new TagIntArray(_reader.ReadInt32Array()),
                    TagId.LongArray => new TagLongArray(_reader.ReadInt64Array()),
                    _ => throw new InvalidDataException($"Invalid tag ID {tagId}")
                };

                // Add simple tag to compound
                ((TagCompound)stateMachine.CurrentTag)[tagName] = tag;
            }
            else
            {
                throw new InvalidDataException($"Invalid state {stateMachine.State}");
            }
        }
        return (TagCompound)stateMachine.CurrentTag;
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

    internal TagList ReadTagList(TagId itemId, int length, out bool isCompleted)
    {
        isCompleted = true; // For TagList<simple tags>, the list is completely parsed; set to false for TagList<TagCompound> and TagList<TagList>

        // TODO: Speed up reading for simple tags since the length is known
        switch (itemId)
        {
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
                    intArrays.Add(new TagIntArray(_reader.ReadInt32Array()));
                return intArrays;
            case TagId.LongArray:
                TagList<TagLongArray> longArrays = new(length);
                for (int i = 0; i < length; i++)
                    longArrays.Add(new TagLongArray(_reader.ReadInt64Array()));
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
