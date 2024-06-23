using MinecraftToolkit.Nbt.Parsing;
using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt;

/// <summary>
/// Base class for all list tags
/// </summary>
public abstract class TagList : Tag
{
    public override TagType Type => TagType.List;

    public abstract TagType ItemType { get; }
    public abstract int Capacity { get; }
    public abstract int Count { get; }

    internal TagId ItemId => ItemType switch
    {
        TagType.Byte => TagId.Byte,
        TagType.Short => TagId.Short,
        TagType.Int => TagId.Int,
        TagType.Long => TagId.Long,
        TagType.Float => TagId.Float,
        TagType.Double => TagId.Double,
        TagType.String => TagId.String,
        TagType.Compound => TagId.Compound,
        TagType.List => TagId.List,
        TagType.ByteArray => TagId.ByteArray,
        TagType.IntArray => TagId.IntArray,
        TagType.LongArray => TagId.LongArray,
        _ => throw new InvalidOperationException($"Invalid TagType value: {ItemType}")
    };
}

public sealed class TagList<T> : TagList, IList<T> where T : notnull
{
    public override TagType ItemType
    {
        get
        {
            if (typeof(T) == typeof(sbyte))
                return TagType.Byte;
            else if (typeof(T) == typeof(short))
                return TagType.Short;
            else if (typeof(T) == typeof(int))
                return TagType.Int;
            else if (typeof(T) == typeof(long))
                return TagType.Long;
            else if (typeof(T) == typeof(float))
                return TagType.Float;
            else if (typeof(T) == typeof(double))
                return TagType.Double;
            else if (typeof(T) == typeof(string))
                return TagType.String;
            else if (typeof(T) == typeof(TagCompound))
                return TagType.Compound;
            else if (typeof(T) == typeof(TagList<T>))
                return TagType.List;
            else if (typeof(T) == typeof(sbyte[]))
                return TagType.ByteArray;
            else if (typeof(T) == typeof(int[]))
                return TagType.IntArray;
            else if (typeof(T) == typeof(long[]))
                return TagType.LongArray;
            else
                throw new InvalidOperationException($"Cannot determine the type of the list item {typeof(T)}");
        }
    }
    public override int Capacity => _items.Capacity;

    internal readonly List<T> _items;

    // QUESTION: What's the type of an empty list?
    public TagList()
    {
        _items = new();
    }

    public TagList(int capacity)
    {
        _items = new(capacity);
    }

    #region IList<T> Implementation

    /// <inheritdoc/>
    public override int Count => _items.Count; // TODO: Fix this, make it consistent with the base class for IList

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public T this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    /// <inheritdoc/>
    public void Add(T item) => _items.Add(item);

    /// <inheritdoc/>
    public void Clear() => _items.Clear();

    /// <inheritdoc/>
    public bool Contains(T item) => _items.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    /// <inheritdoc/>
    public int IndexOf(T item) => _items.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, T item) => _items.Insert(index, item);

    /// <inheritdoc/>
    public bool Remove(T item) => _items.Remove(item);

    /// <inheritdoc/>
    public void RemoveAt(int index) => _items.RemoveAt(index);

    #endregion

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.List);
        writer.WriteString(tagName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBinaryPayload(NbtBinaryWriter writer)
    {
        if (this is TagList<sbyte> byteList)
        {
            writer.Write((byte)TagId.Byte);
            writer.Write(byteList._items.Count);
            writer.Write(CollectionsMarshal.AsSpan(byteList._items));
        }
        else if (this is TagList<short> shortList)
        {
            writer.Write((byte)TagId.Short);
            writer.Write(shortList._items.Count);
            writer.Write(CollectionsMarshal.AsSpan(shortList._items));
        }
        else if (this is TagList<int> intList)
        {
            writer.Write((byte)TagId.Int);
            writer.Write(intList._items.Count);
            writer.Write(CollectionsMarshal.AsSpan(intList._items));
        }
        else if (this is TagList<long> longList)
        {
            writer.Write((byte)TagId.Long);
            writer.Write(longList._items.Count);
            writer.Write(CollectionsMarshal.AsSpan(longList._items));
        }
        else if (this is TagList<float> floatList)
        {
            writer.Write((byte)TagId.Float);
            writer.Write(floatList._items.Count);
            writer.Write(CollectionsMarshal.AsSpan(floatList._items));
        }
        else if (this is TagList<double> doubleList)
        {
            writer.Write((byte)TagId.Double);
            writer.Write(doubleList._items.Count);
            writer.Write(CollectionsMarshal.AsSpan(doubleList._items));
        }
        else if (this is TagList<string> stringList)
        {
            writer.Write((byte)TagId.String);
            writer.Write(stringList._items.Count);
            foreach (var item in stringList._items)
            {
                writer.WriteString(item);
            }
        }
        else if (this is TagList<TagCompound> compoundList)
        {
            writer.Write((byte)TagId.Compound);
            writer.Write(compoundList._items.Count);
            foreach (var item in compoundList._items)
            {
                item.WriteBinaryPayload(writer);
            }
        }
        else if (this is TagList<TagList<T>> listOfLists)
        {
            writer.Write((byte)TagId.List);
            writer.Write(listOfLists._items.Count);
            foreach (var item in listOfLists._items)
            {
                item.WriteBinaryPayload(writer);
            }
        }
        else if (this is TagList<sbyte[]> listOfByteArrays)
        {
            writer.Write((byte)TagId.ByteArray);
            writer.Write(listOfByteArrays._items.Count);
            foreach (var item in listOfByteArrays._items)
            {
                writer.Write(item.Length);
                writer.Write(item);
            }
        }
        else if (this is TagList<int[]> listOfIntArrays)
        {
            writer.Write((byte)TagId.IntArray);
            writer.Write(listOfIntArrays._items.Count);
            foreach (var item in listOfIntArrays._items)
            {
                writer.Write(item.Length);
                writer.Write(item);
            }
        }
        else if (this is TagList<long[]> listOfLongArrays)
        {
            writer.Write((byte)TagId.LongArray);
            writer.Write(listOfLongArrays._items.Count);
            foreach (var item in listOfLongArrays._items)
            {
                writer.Write(item.Length);
                writer.Write(item);
            }
        }
        else
        {
            throw new InvalidOperationException("Invalid element type of TagList");
        }
    }
}
