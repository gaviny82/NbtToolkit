using NbtToolkit.Binary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit;

/// <summary>
/// A compound NBT tag
/// </summary>
public class TagCompound : Tag, IDictionary<string, Tag>
{
    public override TagType Type => TagType.Compound;

    private readonly Dictionary<string, Tag> _data = new();

    public TagCompound()
    {

    }

    public static bool operator ==(TagCompound left, TagCompound right)
    {
        if (object.ReferenceEquals(left._data, right._data))
            return true;

        if (left._data.Count != right._data.Count)
            return false;

        foreach ((string key, Tag value) in left._data)
        {
            if (right._data.TryGetValue(key, out Tag? otherValue))
            {
                if (value != otherValue)
                    return false;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public static bool operator !=(TagCompound left, TagCompound right)
        => !(left == right);

    public override bool Equals(object? obj)
        => obj is TagCompound tag && tag == this;

    public override int GetHashCode() => base.GetHashCode();

    #region IDictionary<string, Tag> implementation

    public Tag this[string key] { get => ((IDictionary<string, Tag>)_data)[key]; set => ((IDictionary<string, Tag>)_data)[key] = value; }

    public Dictionary<string, Tag>.KeyCollection Keys => _data.Keys;

    ICollection<string> IDictionary<string, Tag>.Keys => ((IDictionary<string, Tag>)_data).Keys;

    public Dictionary<string, Tag>.ValueCollection Values => _data.Values;

    ICollection<Tag> IDictionary<string, Tag>.Values => ((IDictionary<string, Tag>)_data).Values;

    public int Count => _data.Count;

    bool ICollection<KeyValuePair<string, Tag>>.IsReadOnly => ((ICollection<KeyValuePair<string, Tag>>)_data).IsReadOnly;

    public void Add(string key, Tag value) => _data.Add(key, value);

    void ICollection<KeyValuePair<string, Tag>>.Add(KeyValuePair<string, Tag> item)
        => ((ICollection<KeyValuePair<string, Tag>>)_data).Add(item);

    public void Clear() => _data.Clear();

    public bool Contains(KeyValuePair<string, Tag> item) => _data.Contains(item);

    public bool ContainsKey(string key) => _data.ContainsKey(key);

    void ICollection<KeyValuePair<string, Tag>>.CopyTo(KeyValuePair<string, Tag>[] array, int arrayIndex)
        => ((ICollection<KeyValuePair<string, Tag>>)_data).CopyTo(array, arrayIndex);

    // Avoid boxing the dictionary enumerator to improve performance
    public Dictionary<string, Tag>.Enumerator GetEnumerator()
        => _data.GetEnumerator();

    IEnumerator<KeyValuePair<string, Tag>> IEnumerable<KeyValuePair<string, Tag>>.GetEnumerator()
        => ((IEnumerable<KeyValuePair<string, Tag>>)_data).GetEnumerator();

    public bool Remove(string key) => _data.Remove(key);

    bool ICollection<KeyValuePair<string, Tag>>.Remove(KeyValuePair<string, Tag> item)
        => ((ICollection<KeyValuePair<string, Tag>>)_data).Remove(item);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Tag value)
        => _data.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_data).GetEnumerator();

    #endregion

    #region Tag: writing binary

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.Compound);
        writer.WriteString(tagName);
        WriteBinaryPayload(writer);
        writer.Write((byte)TagId.End);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBinaryPayload(NbtBinaryWriter writer)
    {
        foreach ((string name, Tag tag) in _data)
        {
            tag.WriteBinary(writer, name);
        }
    }

    #endregion
}
