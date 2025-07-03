using System.Runtime.InteropServices;

namespace NbtToolkit.Benchmark.OtherImpls;

internal struct TagValue
{
    private Overlap _overlap;
    private object? _object;

    private static readonly object s_valueTypeInt = new object();
    private static readonly object s_valueTypeDouble = new object();

    public int IntValue
    {
        get
        {
            return ReferenceEquals(_object, s_valueTypeInt)
                ? _overlap.IntValue
                : throw new InvalidOperationException("Value is not an int.");
        }

        set
        {
            _overlap.IntValue = value;
            _object = s_valueTypeInt;
        }
    }

    public double DoubleValue
    {
        get
        {
            return ReferenceEquals(_object, s_valueTypeDouble)
                ? _overlap.DoubleValue
                : throw new InvalidOperationException("Value is not a double.");
        }
        set
        {
            _overlap.DoubleValue = value;
            _object = s_valueTypeDouble;
        }
    }

    public string? StringValue
    {
        get
        {
            return _overlap.ObjectType == ReferenceType.String
                ? (string?)_object
                : throw new InvalidOperationException("Value is not a string.");
        }
        set
        {
            _object = value;
            _overlap.ObjectType = ReferenceType.String;
        }
    }

    public TagCompound? CompoundValue
    {
        get
        {
            return _overlap.ObjectType == ReferenceType.Compound
                ? (TagCompound?)_object
                : throw new InvalidOperationException("Value is not a TagCompound.");
        }
        set
        {
            _object = value;
            _overlap.ObjectType = ReferenceType.Compound;
        }
    }

    public TagValue(int value)
    {
        IntValue = value;
    }
    public TagValue(double value)
    {
        DoubleValue = value;
    }
    public TagValue(string value)
    {
        StringValue = value;
    }
    public TagValue(TagCompound value)
    {
        CompoundValue = value;
    }

    #region TryGet methods

    public bool TryGetInt(out int value)
    {
        if (_object == s_valueTypeInt)
        {
            value = _overlap.IntValue;
            return true;
        }
        value = default;
        return false;
    }

    public bool TryGetDouble(out double value)
    {
        if (_object == s_valueTypeDouble)
        {
            value = _overlap.DoubleValue;
            return true;
        }
        value = default;
        return false;
    }

    public bool TryGetString(out string? value)
    {
        if (_overlap.ObjectType == ReferenceType.String)
        {
            value = (string?)_object;
            return true;
        }
        value = null;
        return false;
    }

    public bool TryGetCompound(out TagCompound? value)
    {
        if (_overlap.ObjectType == ReferenceType.Compound)
        {
            value = (TagCompound?)_object;
            return true;
        }
        value = null;
        return false;
    }

    #endregion

    private enum ReferenceType
    {
        String,
        Compound,
        List,
        // IntArray, LongArray, etc.
    }

    // Union layout
    [StructLayout(LayoutKind.Explicit)]
    private struct Overlap
    {
        [FieldOffset(0)]
        public int IntValue;
        [FieldOffset(0)]
        public double DoubleValue;
        [FieldOffset(0)]
        public ReferenceType ObjectType; // Used when the tag value is a reference type object
    }
}

internal class TagCompound
{
    private readonly Dictionary<string, TagValue> _tags = new();

    public void Add(string key, int value)
    {
        _tags[key] = new TagValue(value);
    }

    public void Add(string key, double value)
    {
        _tags[key] = new TagValue(value);
    }

    public void Add(string key, string value)
    {
        _tags[key] = new TagValue(value);
    }

    public void Add(string key, TagCompound value)
    {
        _tags[key] = new TagValue(value);
    }

    // Dictionary methods
    public bool ContainsKey(string key)
    {
        return _tags.ContainsKey(key);
    }

    public int GetInt(string key)
    {
        if (!_tags[key].TryGetInt(out int value))
            throw new InvalidCastException("Value is not an int.");
        return value;
    }

    public double GetDouble(string key)
    {
        if (!_tags[key].TryGetDouble(out double value))
            throw new InvalidCastException("Value is not a double.");
        return value;
    }

    public string? GetString(string key)
    {
        if (!_tags[key].TryGetString(out string? value))
            throw new InvalidCastException("Value is not a string.");
        return value;
    }

    public TagCompound? GetCompound(string key)
    {
        if (!_tags[key].TryGetCompound(out TagCompound? value))
            throw new InvalidCastException("Value is not a TagCompound.");
        return value;
    }

    public TagValue this[string key]
    {
        get => _tags[key];
        set => _tags[key] = value;
    }
}