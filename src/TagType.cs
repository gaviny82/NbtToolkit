using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit;

/// <summary>
/// Data types that can be stored in a NBT tag listed on <see href="https://minecraft.wiki/w/NBT_format#Data_types"/>.
/// </summary>
/// <remarks>Boolean type is represented by a byte of either 1 or 0.</remarks>
public enum TagType
{
    Byte,
    Short,
    Int,
    Long,
    Float,
    Double,
    String,
    List,
    Compound,
    ByteArray,
    IntArray,
    LongArray
}
