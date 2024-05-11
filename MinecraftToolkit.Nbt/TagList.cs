using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt;

public class TagList<T> : Tag, IList<T> where T : notnull
{
    public override TagType Type => TagType.List;

    public TagType ItemType
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

    private readonly List<T> _items;

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
    public int Count => _items.Count;

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
}
