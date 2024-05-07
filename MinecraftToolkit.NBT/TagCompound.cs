using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.NBT;

public class TagCompound : IDictionary<string, TagValue>
{
    private Dictionary<string, TagValue> _data = new();

    public TagCompound()
    {

    }

    #region IDictionary<string, TagValue> implementation

    public TagValue this[string key] { get => ((IDictionary<string, TagValue>)_data)[key]; set => ((IDictionary<string, TagValue>)_data)[key] = value; }

    public ICollection<string> Keys => ((IDictionary<string, TagValue>)_data).Keys;

    public ICollection<TagValue> Values => ((IDictionary<string, TagValue>)_data).Values;

    public int Count => ((ICollection<KeyValuePair<string, TagValue>>)_data).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<string, TagValue>>)_data).IsReadOnly;

    public void Add(string key, TagValue value)
    {
        ((IDictionary<string, TagValue>)_data).Add(key, value);
    }

    public void Add(KeyValuePair<string, TagValue> item)
    {
        ((ICollection<KeyValuePair<string, TagValue>>)_data).Add(item);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<string, TagValue>>)_data).Clear();
    }

    public bool Contains(KeyValuePair<string, TagValue> item)
    {
        return ((ICollection<KeyValuePair<string, TagValue>>)_data).Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, TagValue>)_data).ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, TagValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, TagValue>>)_data).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<string, TagValue>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, TagValue>>)_data).GetEnumerator();
    }

    public bool Remove(string key)
    {
        return ((IDictionary<string, TagValue>)_data).Remove(key);
    }

    public bool Remove(KeyValuePair<string, TagValue> item)
    {
        return ((ICollection<KeyValuePair<string, TagValue>>)_data).Remove(item);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out TagValue value)
    {
        return ((IDictionary<string, TagValue>)_data).TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_data).GetEnumerator();
    }

    #endregion
}
