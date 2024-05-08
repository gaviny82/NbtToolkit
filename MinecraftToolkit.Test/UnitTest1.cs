using MinecraftToolkit.Nbt;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MinecraftToolkit.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        TagValue t = new TagValue();
        var size = Unsafe.SizeOf<TagValue>();
    }
}