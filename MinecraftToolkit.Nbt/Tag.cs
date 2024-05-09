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
    public bool AsBool() => ((TagBool)this).Value;

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
}
