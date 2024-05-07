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

    /// <summary>
    /// Get the value stored in <see cref="TagValue"/> as an object.
    /// <para>Boxing occurs when the value stored has a value type.</para>
    /// </summary>
    public readonly object Value => Type switch
    {
        TagType.Byte => _value.ByteValue,
        TagType.Short => _value.ShortValue,
        TagType.Int => _value.IntValue,
        TagType.Long => _value.LongValue,
        TagType.Float => _value.FloatValue,
        TagType.Double => _value.DoubleValue,
        TagType.Bool => _value.BoolValue,
        TagType.String => _object!,
        TagType.List => throw new NotImplementedException(),
        TagType.Compound => _object!,
        TagType.ByteArray => _object!,
        TagType.IntArray => _object!,
        TagType.LongArray => _object!,
        _ => throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(object)}")
    };


    /// <summary>
    /// Default constructor of <see cref="TagValue"/> with a default type of <see cref="TagType.Int"/> and a default value of 0.
    /// </summary>
    public TagValue()
    {
        Type = TagType.Int;
        _value.IntValue = 0;
    }

    private TagValue(TagType type, TagValueUnion value)
    {
        Type = type;
        _value = value;
    }

    private TagValue(TagType type, object value)
    {
        Type = type;
        _object = value;
    }

    #region Factory Methods

    public static TagValue CreateByte(sbyte value)
        => new TagValue(TagType.Byte, new TagValueUnion { ByteValue = value });

    public static TagValue CreateShort(short value)
        => new TagValue(TagType.Short, new TagValueUnion { ShortValue = value });

    public static TagValue CreateInt(int value)
        => new TagValue(TagType.Int, new TagValueUnion { IntValue = value });

    public static TagValue CreateLong(long value)
        => new TagValue(TagType.Long, new TagValueUnion { LongValue = value });

    public static TagValue CreateFloat(float value)
        => new TagValue(TagType.Float, new TagValueUnion { FloatValue = value });

    public static TagValue CreateDouble(double value)
        => new TagValue(TagType.Double, new TagValueUnion { DoubleValue = value });

    public static TagValue CreateBool(bool value)
        => new TagValue(TagType.Bool, new TagValueUnion { BoolValue = value });

    public static TagValue CreateString(string value)
        => new TagValue(TagType.String, value);

    // TODO: CreateList

    public static TagValue CreateCompound(TagCompound value)
        => new TagValue(TagType.Compound, value);

    public static TagValue CreateByteArray(byte[] value)
        => new TagValue(TagType.ByteArray, value);

    public static TagValue CreateIntArray(int[] value)
        => new TagValue(TagType.IntArray, value);

    public static TagValue CreateLongArray(long[] value)
        => new TagValue(TagType.LongArray, value);

    #endregion

    #region Implicit conversions from value

    public static implicit operator TagValue(sbyte value) => CreateByte(value);
    public static implicit operator TagValue(short value) => CreateShort(value);
    public static implicit operator TagValue(int value) => CreateInt(value);
    public static implicit operator TagValue(long value) => CreateLong(value);
    public static implicit operator TagValue(float value) => CreateFloat(value);
    public static implicit operator TagValue(double value) => CreateDouble(value);
    public static implicit operator TagValue(bool value) => CreateBool(value);
    public static implicit operator TagValue(string value) => CreateString(value);
    // TODO: implicit operator for TagList
    public static implicit operator TagValue(TagCompound value) => CreateCompound(value);
    public static implicit operator TagValue(byte[] value) => CreateByteArray(value);
    public static implicit operator TagValue(int[] value) => CreateIntArray(value);
    public static implicit operator TagValue(long[] value) => CreateLongArray(value);

    #endregion

    #region Methods for accessing the stored value

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as an <see cref="sbyte"/> value.
    /// </summary>
    /// <returns>An <see cref="sbyte"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not an <see cref="sbyte"/></exception>
    public readonly sbyte AsByte()
    {
        if (Type == TagType.Byte)
            return _value.ByteValue;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(sbyte)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="short"/> value.
    /// </summary>
    /// <returns>A <see cref="short"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="short"/></exception>
    public readonly short AsShort()
    {
        if (Type == TagType.Short)
            return _value.ShortValue;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(short)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as an <see cref="int"/> value.
    /// </summary>
    /// <returns>An <see cref="int"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not an <see cref="int"/></exception>
    public readonly int AsInt()
    {
        if (Type == TagType.Int)
            return _value.IntValue;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(int)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long"/> value.
    /// </summary>
    /// <returns>A <see cref="long"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long"/></exception>
    public readonly long AsLong()
    {
        if (Type == TagType.Long)
            return _value.LongValue;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(long)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="float"/> value.
    /// </summary>
    /// <returns>A <see cref="float"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="float"/></exception>
    public readonly float AsFloat()
    {
        if (Type == TagType.Float)
            return _value.FloatValue;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(float)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="double"/> value.
    /// </summary>
    /// <returns>A <see cref="double"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="double"/></exception>
    public readonly double AsDouble()
    {
        if (Type == TagType.Double)
            return _value.DoubleValue;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(double)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="bool"/> value.
    /// </summary>
    /// <returns>A <see cref="bool"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="bool"/></exception>
    public readonly bool AsBool()
    {
        if (Type == TagType.Bool)
            return _value.BoolValue;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(bool)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="string"/> value.
    /// </summary>
    /// <returns>A <see cref="string"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="string"/></exception>
    public readonly string AsString()
    {
        if (Type == TagType.String && _object is string value)
            return value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(string)}");
    }

    // TODO: AsList

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="TagCompound"/> value.
    /// </summary>
    /// <returns>A <see cref="TagCompound"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="TagCompound"/></exception>
    public readonly TagCompound AsCompound()
    {
        if (Type == TagType.Compound && _object is TagCompound value)
            return value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(TagCompound)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="byte[]"/> value.
    /// </summary>
    /// <returns>A <see cref="byte[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="byte[]"/></exception>
    public readonly byte[] AsByteArray()
    {
        if (Type == TagType.ByteArray && _object is byte[] value)
            return value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(byte[])}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long[]"/> value.
    /// </summary>
    /// <returns>A <see cref="long[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long[]"/></exception>
    public readonly int[] AsIntArray()
    {
        if (Type == TagType.IntArray && _object is int[] value)
            return value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(int[])}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long[]"/> value.
    /// </summary>
    /// <returns>A <see cref="long[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long[]"/></exception>
    public readonly long[] AsLongArray()
    {
        if (Type == TagType.LongArray && _object is long[] value)
            return value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(long[])}");
    }

    #endregion
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
