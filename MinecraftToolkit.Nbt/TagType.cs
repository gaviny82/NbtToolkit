using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt;

/// <summary>
/// Data types that can be stored in a NBT tag listed on <see href="https://minecraft.wiki/w/NBT_format#Data_types"/>
/// </summary>
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
