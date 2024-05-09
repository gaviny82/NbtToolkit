using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt;

public class TagByte : Tag
{
    public override TagType Type => TagType.Int;

    public sbyte Value { get; set; }

    public TagByte(sbyte value)
    {
        Value = value;
    }

    public static implicit operator TagByte(sbyte value) => new TagByte(value);
}

public class TagShort : Tag
{
    public override TagType Type => TagType.Short;

    public short Value { get; set; }

    public TagShort(short value)
    {
        Value = value;
    }

    public static implicit operator TagShort(short value) => new TagShort(value);
}


public class TagInt : Tag
{
    public override TagType Type => TagType.Int;

    public int Value { get; set; }

    public TagInt(int value)
    {
        Value = value;
    }

    public static implicit operator TagInt(int value) => new TagInt(value);
}

public class TagLong : Tag
{
    public override TagType Type => TagType.Long;

    public long Value { get; set; }

    public TagLong(long value)
    {
        Value = value;
    }

    public static implicit operator TagLong(long value) => new TagLong(value);
}

public class TagFloat : Tag
{
    public override TagType Type => TagType.Float;

    public float Value { get; set; }

    public TagFloat(float value)
    {
        Value = value;
    }

    public static implicit operator TagFloat(float value) => new TagFloat(value);
}

public class TagDouble : Tag
{
    public override TagType Type => TagType.Double;

    public double Value { get; set; }

    public TagDouble(double value)
    {
        Value = value;
    }

    public static implicit operator TagDouble(double value) => new TagDouble(value);
}

public class TagBool : Tag
{
    public override TagType Type => TagType.Byte;

    public bool Value { get; set; }

    public TagBool(bool value)
    {
        Value = value;
    }

    public static implicit operator TagBool(bool value) => new TagBool(value);
}

public class TagString : Tag
{
    public override TagType Type => TagType.String;

    public string Value { get; set; }

    public TagString(string value)
    {
        Value = value;
    }

    public static implicit operator TagString(string value) => new TagString(value);
}

public class TagByteArray : Tag
{
    public override TagType Type => TagType.ByteArray;

    public sbyte[] Value { get; set; }

    public TagByteArray(sbyte[] value)
    {
        Value = value;
    }

    public static implicit operator TagByteArray(sbyte[] value) => new TagByteArray(value);
}

public class TagIntArray : Tag
{
    public override TagType Type => TagType.IntArray;

    public int[] Value { get; set; }

    public TagIntArray(int[] value)
    {
        Value = value;
    }

    public static implicit operator TagIntArray(int[] value) => new TagIntArray(value);
}

public class TagLongArray : Tag
{
    public override TagType Type => TagType.LongArray;

    public long[] Value { get; set; }

    public TagLongArray(long[] value)
    {
        Value = value;
    }

    public static implicit operator TagLongArray(long[] value) => new TagLongArray(value);
}