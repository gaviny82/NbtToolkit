using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Parsing;

internal ref struct NbtReaderInternal
{
    public Span<byte> ValueBuffer;
    public NbtReaderState State = NbtReaderState.Start;
    public TagId ListItemId; // Tag ID of a list (only used when parsing a list)
    public int ListRemainingLength; // Length of a list (only used when parsing a list)

    public NbtReaderInternal(ref Span<byte> valueBuffer)
    {
        ValueBuffer = valueBuffer;
    }
}

internal enum NbtReaderState
{
    Start,
    ParsingTagCompound,
    ParsingTagList
}