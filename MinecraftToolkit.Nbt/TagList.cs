using MinecraftToolkit.Nbt.Parsing;
using System;
using System.Collections;
using System.Linq;
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

    internal sealed override void WriteTag(NbtWriter writer, string tagName)
    {
        writer.Write(TagId.List);
        writer.BinaryWriter.Write(tagName);

        // TODO: payload
        switch (ItemType)
        {
            case TagType.Byte:
                writer.Write(TagId.Byte);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.Short:
                writer.Write(TagId.Short);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.Int:
                writer.Write(TagId.Int);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.Long:
                writer.Write(TagId.Long);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.Float:
                writer.Write(TagId.Float);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.Double:
                writer.Write(TagId.Double);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.String:
                writer.Write(TagId.String);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.List:
                writer.Write(TagId.List);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.Compound:
                writer.Write(TagId.Compound);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.ByteArray:
                writer.Write(TagId.ByteArray);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.IntArray:
                writer.Write(TagId.IntArray);
                writer.BinaryWriter.Write(_items.Count);
                break;
            case TagType.LongArray:
                writer.Write(TagId.LongArray);
                writer.BinaryWriter.Write(_items.Count);
                break;
            default:
                throw new InvalidOperationException("Invalid TagList type");
        }
    }
}
