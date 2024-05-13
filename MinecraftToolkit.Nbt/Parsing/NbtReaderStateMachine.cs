using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Parsing;

internal ref struct NbtReaderStateMachine
{
    public NbtReaderState State;
    public TagId ListItemId; // Tag ID of a list (only used when parsing a list)
    public int ListRemainingLength; // Length of a list (only used when parsing a list)
    public Tag CurrentTag;
    public Stack<(Tag, string?)> Stack = new(); // Stack of compound or list tags (string is the name of currentTag if the parent tag is TagCompound; otherwise, null)

    public NbtReaderStateMachine(NbtReaderState initialState, Tag rootTag)
    {
        State = initialState;
        CurrentTag = rootTag;
        ListItemId = TagId.End;
        ListRemainingLength = 0;
    }
}

internal enum NbtReaderState
{
    Start,
    ParsingTagCompound,
    ParsingTagList
}