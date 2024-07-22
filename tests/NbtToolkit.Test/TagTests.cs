using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NbtToolkit.Test;

public class TagTests
{
    [Fact]
    public void OperatorEquality()
    {
        TagByte b1 = 1, b2 = 1;
        Assert.True(b1 == b2);

        TagShort s1 = 1, s2 = 1;
        Assert.True(s1 == s2);

        TagInt i1 = 1, i2 = 1;
        Assert.True(i1 == i2);

        TagLong l1 = 1, l2 = 1;
        Assert.True(l1 == l2);

        TagFloat f1 = 1.1f, f2 = 1.1f;
        Assert.True(f1 == f2);

        TagDouble d1 = 1.1, d2 = 1.1;
        Assert.True(d1 == d2);

        TagByteArray ba1 = new sbyte[] { 1, 2 }, ba2 = new sbyte[] { 1, 2 };
        Assert.True(ba1 == ba2);

        TagIntArray ia1 = new int[] { 1, 2 }, ia2 = new int[] { 1, 2 };
        Assert.True(ia1 == ia2);

        TagLongArray la1 = new long[] { 1, 2 }, la2 = new long[] { 1, 2 };
        Assert.True(la1 == la2);

        TagList<int> list1 = [1, 2, 3], list2 = [1, 2, 3];
        Assert.True(list1 == list2);

        TagCompound comp1 = new(), comp2 = new();
        comp1["key1"] = new TagInt(1);
        comp2["key1"] = new TagInt(1);
        Assert.True(comp1 == comp2);
    }
}
