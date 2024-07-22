using NbtToolkit.Binary;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NbtToolkit;

/// <summary>
/// Base class of all NBT tags
/// </summary>
public abstract class Tag
{
    /// <summary>
    /// Data type of the value stored in <see cref="TagValue"/>
    /// </summary>
    public abstract TagType Type { get; }

    #region Methods for accessing the stored value

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as an <see cref="sbyte"/> value.
    /// </summary>
    /// <returns>An <see cref="sbyte"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not an <see cref="sbyte"/></exception>
    public sbyte AsByte()
    {
        if (this is TagByte tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(sbyte)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="short"/> value.
    /// </summary>
    /// <returns>A <see cref="short"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="short"/></exception>
    public short AsShort()
    {
        if (this is TagShort tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(short)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as an <see cref="int"/> value.
    /// </summary>
    /// <returns>An <see cref="int"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not an <see cref="int"/></exception>
    public int AsInt() => ((TagInt)this).Value;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long"/> value.
    /// </summary>
    /// <returns>A <see cref="long"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long"/></exception>
    public long AsLong() => ((TagLong)this).Value;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="float"/> value.
    /// </summary>
    /// <returns>A <see cref="float"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="float"/></exception>
    public float AsFloat() => ((TagFloat)this).Value;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="double"/> value.
    /// </summary>
    /// <returns>A <see cref="double"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="double"/></exception>
    public double AsDouble() => ((TagDouble)this).Value;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="bool"/> value.
    /// </summary>
    /// <returns>A <see cref="bool"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="bool"/></exception>
    public bool AsBool() => ((TagByte)this).Value == 1;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="string"/> value.
    /// </summary>
    /// <returns>A <see cref="string"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="string"/></exception>
    public string AsString() => ((TagString)this).Value;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="TagList"/> value.
    /// </summary>
    /// <returns>A <see cref="TagList"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="TagList"/></exception>
    public TagList<T> AsTagList<T>() where T : notnull => ((TagList<T>)this);

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="TagCompound"/> value.
    /// </summary>
    /// <returns>A <see cref="TagCompound"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="TagCompound"/></exception>
    public TagCompound AsTagCompound() => ((TagCompound)this);

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="sbyte[]"/> value.
    /// </summary>
    /// <returns>A <see cref="sbyte[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="sbyte[]"/></exception>
    public sbyte[] AsByteArray() => ((TagByteArray)this).Value;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long[]"/> value.
    /// </summary>
    /// <returns>A <see cref="long[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long[]"/></exception>
    public int[] AsIntArray() => ((TagIntArray)this).Value;

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long[]"/> value.
    /// </summary>
    /// <returns>A <see cref="long[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long[]"/></exception>
    public long[] AsLongArray() => ((TagLongArray)this).Value;

    #endregion

    public static bool operator ==(Tag left, Tag right)
        => left.Equals(right);

    public static bool operator !=(Tag left, Tag right)
        => !left.Equals(right);

    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();

    // Virtual function is used because many type checks are needed to determine which function to call
    // when writing a tag to a stream. Virtual function call has a consistent overhead independent of
    // the number of child types. // TODO: benchmark needed
    /// <summary>
    /// Writes the tag ID, name and payload using a given <see cref="NbtWriter"/>
    /// </summary>
    /// <param name="writer">The <see cref="NbtWriter"/> given</param>
    internal abstract void WriteBinary(NbtBinaryWriter writer, string tagName);
}

#region Simple tags

public class TagByte : Tag
{
    public override TagType Type => TagType.Int;

    public sbyte Value { get; set; }

    public TagByte(sbyte value)
    {
        Value = value;
    }

    public static bool operator ==(TagByte left, TagByte right)
        => left.Value == right.Value;

    public static bool operator !=(TagByte left, TagByte right)
        => left.Value != right.Value;

    public override bool Equals(object? obj)
        => obj is TagByte tag && tag.Value == Value;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagByte(sbyte value) => new TagByte(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.Byte);
        writer.WriteString(tagName);
        writer.Write(Value);
    }
}

public class TagShort : Tag
{
    public override TagType Type => TagType.Short;

    public short Value { get; set; }

    public TagShort(short value)
    {
        Value = value;
    }

    public static bool operator ==(TagShort left, TagShort right)
        => left.Value == right.Value;

    public static bool operator !=(TagShort left, TagShort right)
        => left.Value != right.Value;

    public override bool Equals(object? obj)
        => obj is TagShort tag && tag.Value == Value;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagShort(short value) => new TagShort(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.Short);
        writer.WriteString(tagName);
        writer.Write(Value);
    }
}


public class TagInt : Tag
{
    public override TagType Type => TagType.Int;

    public int Value { get; set; }

    public TagInt(int value)
    {
        Value = value;
    }

    public static bool operator ==(TagInt left, TagInt right)
        => left.Value == right.Value;

    public static bool operator !=(TagInt left, TagInt right)
        => left.Value != right.Value;

    public override bool Equals(object? obj)
        => obj is TagInt tag && tag.Value == Value;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagInt(int value) => new TagInt(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.Int);
        writer.WriteString(tagName);
        writer.Write(Value);
    }
}

public class TagLong : Tag
{
    public override TagType Type => TagType.Long;

    public long Value { get; set; }

    public TagLong(long value)
    {
        Value = value;
    }

    public static bool operator ==(TagLong left, TagLong right)
        => left.Value == right.Value;

    public static bool operator !=(TagLong left, TagLong right)
        => left.Value != right.Value;

    public override bool Equals(object? obj)
        => obj is TagLong tag && tag.Value == Value;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagLong(long value) => new TagLong(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.Long);
        writer.WriteString(tagName);
        writer.Write(Value);
    }
}

public class TagFloat : Tag
{
    public override TagType Type => TagType.Float;

    public float Value { get; set; }

    public TagFloat(float value)
    {
        Value = value;
    }

    public static bool operator ==(TagFloat left, TagFloat right)
        => left.Value == right.Value;

    public static bool operator !=(TagFloat left, TagFloat right)
        => left.Value != right.Value;

    public override bool Equals(object? obj)
        => obj is TagFloat tag && tag.Value == Value;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagFloat(float value) => new TagFloat(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.Float);
        writer.WriteString(tagName);
        writer.Write(Value);
    }
}

public class TagDouble : Tag
{
    public override TagType Type => TagType.Double;

    public double Value { get; set; }

    public TagDouble(double value)
    {
        Value = value;
    }

    public static bool operator ==(TagDouble left, TagDouble right)
        => left.Value == right.Value;

    public static bool operator !=(TagDouble left, TagDouble right)
        => left.Value != right.Value;

    public override bool Equals(object? obj)
        => obj is TagDouble tag && tag.Value == Value;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagDouble(double value) => new TagDouble(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.Double);
        writer.WriteString(tagName);
        writer.Write(Value);
    }
}

public class TagString : Tag
{
    public override TagType Type => TagType.String;

    public string Value { get; set; }

    public TagString(string value)
    {
        Value = value;
    }

    public static bool operator ==(TagString left, TagString right)
        => left.Value == right.Value;

    public static bool operator !=(TagString left, TagString right)
        => left.Value != right.Value;

    public override bool Equals(object? obj)
        => obj is TagString tag && tag.Value == Value;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagString(string value) => new TagString(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.String);
        writer.WriteString(tagName);
        writer.WriteString(Value);
    }
}

public class TagByteArray : Tag
{
    public override TagType Type => TagType.ByteArray;

    public sbyte[] Value { get; set; }

    public TagByteArray(sbyte[] value)
    {
        Value = value;
    }

    public static bool operator ==(TagByteArray left, TagByteArray right)
        => Enumerable.SequenceEqual(left.Value, right.Value);

    public static bool operator !=(TagByteArray left, TagByteArray right)
        => !(left == right);

    public override bool Equals(object? obj)
        => obj is TagByteArray tag && tag == this;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagByteArray(sbyte[] value) => new TagByteArray(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.ByteArray);
        writer.WriteString(tagName);
        writer.Write(Value.Length);
        writer.Write(Value);
    }
}

public class TagIntArray : Tag
{
    public override TagType Type => TagType.IntArray;

    public int[] Value { get; set; }

    public TagIntArray(int[] value)
    {
        Value = value;
    }

    public static bool operator ==(TagIntArray left, TagIntArray right)
        => Enumerable.SequenceEqual(left.Value, right.Value);

    public static bool operator !=(TagIntArray left, TagIntArray right)
        => !(left == right);

    public override bool Equals(object? obj)
        => obj is TagIntArray tag && tag == this;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagIntArray(int[] value) => new TagIntArray(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.IntArray);
        writer.WriteString(tagName);
        writer.Write(Value.Length);
        writer.Write(Value);
    }
}

public class TagLongArray : Tag
{
    public override TagType Type => TagType.LongArray;

    public long[] Value { get; set; }

    public TagLongArray(long[] value)
    {
        Value = value;
    }

    public static bool operator ==(TagLongArray left, TagLongArray right)
        => Enumerable.SequenceEqual(left.Value, right.Value);

    public static bool operator !=(TagLongArray left, TagLongArray right)
        => !(left == right);

    public override bool Equals(object? obj)
        => obj is TagLongArray tag && tag == this;

    public override int GetHashCode() => base.GetHashCode();

    public static implicit operator TagLongArray(long[] value) => new TagLongArray(value);

    internal sealed override void WriteBinary(NbtBinaryWriter writer, string tagName)
    {
        writer.Write((byte)TagId.LongArray);
        writer.WriteString(tagName);
        writer.Write(Value.Length);
        writer.Write(Value);
    }
}

#endregion
