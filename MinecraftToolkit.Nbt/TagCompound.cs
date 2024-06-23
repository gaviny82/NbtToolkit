using MinecraftToolkit.Nbt.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt;

public class TagCompound : Tag, IDictionary<string, Tag>
{
    public override TagType Type => TagType.Compound;

    private Dictionary<string, Tag> _data = new();

    public TagCompound()
    {

    }

    #region IDictionary<string, Tag> implementation

    public Tag this[string key] { get => ((IDictionary<string, Tag>)_data)[key]; set => ((IDictionary<string, Tag>)_data)[key] = value; }

    public ICollection<string> Keys => ((IDictionary<string, Tag>)_data).Keys;

    public ICollection<Tag> Values => ((IDictionary<string, Tag>)_data).Values;

    public int Count => ((ICollection<KeyValuePair<string, Tag>>)_data).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<string, Tag>>)_data).IsReadOnly;

    public void Add(string key, Tag value)
    {
        ((IDictionary<string, Tag>)_data).Add(key, value);
    }

    public void Add(KeyValuePair<string, Tag> item)
    {
        ((ICollection<KeyValuePair<string, Tag>>)_data).Add(item);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<string, Tag>>)_data).Clear();
    }

    public bool Contains(KeyValuePair<string, Tag> item)
    {
        return ((ICollection<KeyValuePair<string, Tag>>)_data).Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, Tag>)_data).ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, Tag>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, Tag>>)_data).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, Tag>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, Tag>>)_data).GetEnumerator();
    }

    public bool Remove(string key)
    {
        return ((IDictionary<string, Tag>)_data).Remove(key);
    }

    public bool Remove(KeyValuePair<string, Tag> item)
    {
        return ((ICollection<KeyValuePair<string, Tag>>)_data).Remove(item);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Tag value)
    {
        return ((IDictionary<string, Tag>)_data).TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_data).GetEnumerator();
    }

    #endregion

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
        foreach ((string name, Tag tag) in this)
        {
            tag.WriteBinary(writer, name);
        }
    }
}
