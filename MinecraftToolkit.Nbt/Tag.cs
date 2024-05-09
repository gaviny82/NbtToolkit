using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MinecraftToolkit.Nbt;

public abstract class Tag
{
    /// <summary>
    /// Data type of the value stored in <see cref="TagValue"/>
    /// </summary>
    public abstract TagType Type { get; }

    #region Factory Methods

    public static Tag CreateByte(sbyte value) => new TagByte(value);
    public static Tag CreateShort(short value) => new TagShort(value);
    public static Tag CreateInt(int value) => new TagInt(value);
    public static Tag CreateLong(long value) => new TagLong(value);
    public static Tag CreateFloat(float value) => new TagFloat(value);
    public static Tag CreateDouble(double value) => new TagDouble(value);
    public static Tag CreateBool(bool value) => new TagBool(value);
    public static Tag CreateString(string value) => new TagString(value);
    public static Tag CreateList<T>() where T: notnull => new TagList<T>();
    public static Tag CreateCompound() => new TagCompound();
    public static Tag CreateByteArray(sbyte[] value) => new TagByteArray(value);
    public static Tag CreateIntArray(int[] value) => new TagIntArray(value);
    public static Tag CreateLongArray(long[] value) => new TagLongArray(value);

    #endregion

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
    public int AsInt()
    {
        if (this is TagInt tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(int)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long"/> value.
    /// </summary>
    /// <returns>A <see cref="long"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long"/></exception>
    public long AsLong()
    {
        if (this is TagLong tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(long)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="float"/> value.
    /// </summary>
    /// <returns>A <see cref="float"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="float"/></exception>
    public float AsFloat()
    {
        if (this is TagFloat tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(float)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="double"/> value.
    /// </summary>
    /// <returns>A <see cref="double"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="double"/></exception>
    public double AsDouble()
    {
        if (this is TagDouble tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(double)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="bool"/> value.
    /// </summary>
    /// <returns>A <see cref="bool"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="bool"/></exception>
    public bool AsBool()
    {
        if (this is TagBool tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(bool)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="string"/> value.
    /// </summary>
    /// <returns>A <see cref="string"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="string"/></exception>
    public string AsString()
    {
        if (this is TagString tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(string)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="TagList"/> value.
    /// </summary>
    /// <returns>A <see cref="TagList"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="TagList"/></exception>
    public TagList<T> AsTagList<T>() where T : notnull
    {
        if (this is TagList<T> tag)
            return tag;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(TagList<T>)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="TagCompound"/> value.
    /// </summary>
    /// <returns>A <see cref="TagCompound"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="TagCompound"/></exception>
    public TagCompound AsTagCompound()
    {
        if (this is TagCompound tag)
            return tag;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(TagCompound)}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="sbyte[]"/> value.
    /// </summary>
    /// <returns>A <see cref="sbyte[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="sbyte[]"/></exception>
    public sbyte[] AsByteArray()
    {
        if (this is TagByteArray tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(sbyte[])}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long[]"/> value.
    /// </summary>
    /// <returns>A <see cref="long[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long[]"/></exception>
    public int[] AsIntArray()
    {
        if (this is TagIntArray tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(int[])}");
    }

    /// <summary>
    /// Read the value stored in <see cref="TagValue"/> as a <see cref="long[]"/> value.
    /// </summary>
    /// <returns>A <see cref="long[]"/> value</returns>
    /// <exception cref="InvalidCastException">The value stored is not a <see cref="long[]"/></exception>
    public long[] AsLongArray()
    {
        if (this is TagLongArray tag)
            return tag.Value;
        else
            throw new InvalidCastException($"Cannot cast a tag of {Type} to {typeof(long[])}");
    }

    #endregion
}
