using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MinecraftToolkit.NBT;

public struct TagValue // sizeof(TagValue) = 24 (_value: 8 + _object: 8 + Type: 4 + alignment: 4)
{
    private TagValueUnion _value; // Value type data that can be stored inside TagValue
    private object? _object = null; // Reference type data that is stored as a reference inside TagValue

    /// <summary>
    /// Data type of the value stored in <see cref="TagValue"/>
    /// </summary>
    public TagType Type { get; private set; }

    public T GetValue<T>() where T: notnull
    {
        if (Type == TagType.Bool && _value.BoolValue is T boolValue)
        {
            return boolValue;
        }
        if (_object is T obj)
        {
            return obj;
        }
        // TODO: Add more types
        throw new InvalidOperationException($"The value stored is not of type {typeof(T)}");
    }

    /// <summary>
    /// Default constructor of <see cref="TagValue"/> with a default type of <see cref="TagType.Int"/> and a default value of 0.
    /// </summary>
    public TagValue()
    {
        Type = TagType.Int;
        _value.IntValue = 0;
    }

    /// <summary>
    /// Create a new instance of <see cref="TagValue"/> with a specified <paramref name="type"/> and <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">Data type of the tag</typeparam>
    /// <param name="value">Value stored in the tag</param>
    /// <returns>A new instance of <see cref="TagValue"/></returns>
    public static TagValue Create<T>(T value)
    {
        TagValue tagValue = new();
        // TODO:
        return tagValue;
    }
}

[StructLayout(LayoutKind.Explicit)]
internal struct TagValueUnion // sizeof(TagValueUnion) = 8
{
    [FieldOffset(0)]
    public sbyte ByteValue;

    [FieldOffset(0)]
    public short ShortValue;

    [FieldOffset(0)]
    public int IntValue;

    [FieldOffset(0)]
    public long LongValue;

    [FieldOffset(0)]
    public float FloatValue;

    [FieldOffset(0)]
    public double DoubleValue;

    [FieldOffset(0)]
    public bool BoolValue;
}
