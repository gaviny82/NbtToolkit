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
using NbtToolkit.Binary;
using static NbtToolkit.Binary.NbtReader;

namespace NbtToolkit.Benchmark;

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

    private readonly NbtBinaryReader _reader;

    public NbtReader_StateMachineImpl(Stream stream, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(stream, CompressionMode.Decompress),
            NbtCompression.ZLib => new ZLibStream(stream, CompressionMode.Decompress),
            NbtCompression.None => stream,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };
        _reader = new(stream, leaveOpen);
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
                    TagId.ByteArray => new TagByteArray(ReadSByteArray()),
                    TagId.String => new TagString(_reader.ReadString()),
                    TagId.IntArray => new TagIntArray(ReadInt32Array()),
                    TagId.LongArray => new TagLongArray(ReadInt64Array()),
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

    /// <summary>
    /// Reads a signed byte array prefixed by an <see cref="int"/> length
    /// </summary>
    /// <returns>A signed byte array</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte[] ReadSByteArray()
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
    public virtual int[] ReadInt32Array()
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
    public virtual long[] ReadInt64Array()
    {
        int length = _reader.ReadInt32();
        long[] data = new long[length];
        _reader.ReadInt64Span(data);
        return data;
    }

    internal TagList ReadTagList(TagId itemId, int length, out bool isCompleted)
    {
        isCompleted = true; // For TagList<simple tags>, the list is completely parsed; set to false for TagList<TagCompound> and TagList<TagList>

        switch (itemId)
        {
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
                _reader.ReadFloatSpan(CollectionsMarshal.AsSpan(floats._items));
                return floats;
            case TagId.Double:
                TagList<double> doubles = new(0);
                CollectionsMarshal.SetCount(doubles._items, length);
                _reader.ReadDoubleSpan(CollectionsMarshal.AsSpan(doubles._items));
                return doubles;
            case TagId.ByteArray:
                TagList<TagByteArray> byteArrays = new(length);
                for (int i = 0; i < length; i++)
                    byteArrays.Add(new TagByteArray(ReadSByteArray()));
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
                    intArrays.Add(new TagIntArray(ReadInt32Array()));
                return intArrays;
            case TagId.LongArray:
                TagList<TagLongArray> longArrays = new(length);
                for (int i = 0; i < length; i++)
                    longArrays.Add(new TagLongArray(ReadInt64Array()));
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
