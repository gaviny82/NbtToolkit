using NbtToolkit.Binary;

namespace NbtToolkit.Test;

public class NbtWriterTests
{
    [Fact]
    public void WriteTagList_Byte_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Byte_Bytes;
        sbyte[] values = NbtBinaryTestCases.TagList_Byte_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<sbyte> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_Short_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Short_Bytes;
        short[] values = NbtBinaryTestCases.TagList_Short_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<short> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_Int_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Int_Bytes;
        int[] values = NbtBinaryTestCases.TagList_Int_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<int> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_Long_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Long_Bytes;
        long[] values = NbtBinaryTestCases.TagList_Long_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<long> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_Float_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Float_Bytes;
        float[] values = NbtBinaryTestCases.TagList_Float_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<float> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_Double_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Double_Bytes;
        double[] values = NbtBinaryTestCases.TagList_Double_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<double> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_String_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_String_Bytes;
        string[] values = NbtBinaryTestCases.TagList_String_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<string> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_ByteArray_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_ByteArray_Bytes;
        sbyte[][] values = NbtBinaryTestCases.TagList_ByteArray_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<sbyte[]> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_IntArray_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_IntArray_Bytes;
        int[][] values = NbtBinaryTestCases.TagList_IntArray_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<int[]> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_LongArray_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_LongArray_Bytes;
        long[][] values = NbtBinaryTestCases.TagList_LongArray_Values;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        TagList<long[]> list = [.. values];

        // Act
        list.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagCompound_Simple_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagCompound_Simple_Bytes;
        TagCompound value = NbtBinaryTestCases.TagCompound_Simple_Value;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        // Act
        value.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }

    [Fact]
    public void WriteTagList_NestedList_WriteCorrectBytes()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_IntList_Bytes;
        TagList<TagList> value = NbtBinaryTestCases.TagList_IntList_Value;

        using MemoryStream stream = new();
        using NbtBinaryWriter writer = new(stream);

        // Act
        value.WriteBinaryPayload(writer);

        // Assert
        Assert.Equal(bytes, stream.ToArray());
    }
}
