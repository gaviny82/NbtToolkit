using NbtToolkit.Binary;
using Newtonsoft.Json;

namespace NbtToolkit.Test;

public class NbtReaderTests
{
    [Fact]
    public void ReadTagList_Byte_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Byte_Bytes;
        sbyte[] values = NbtBinaryTestCases.TagList_Byte_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<sbyte> result = (TagList<sbyte>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }

    [Fact]
    public void ReadTagList_Short_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Short_Bytes;
        short[] values = NbtBinaryTestCases.TagList_Short_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<short> result = (TagList<short>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }

    [Fact]
    public void ReadTagList_Int_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Int_Bytes;
        int[] values = NbtBinaryTestCases.TagList_Int_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<int> result = (TagList<int>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }

    [Fact]
    public void ReadTagList_Long_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Long_Bytes;
        long[] values = NbtBinaryTestCases.TagList_Long_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<long> result = (TagList<long>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }

    [Fact]
    public void ReadTagList_Float_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Float_Bytes;
        float[] values = NbtBinaryTestCases.TagList_Float_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<float> result = (TagList<float>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
        Assert.Equal(1, Math.Sign(1 / result[2])); // Check for +0
        Assert.Equal(-1, Math.Sign(1 / result[3])); // Check for -0
    }

    [Fact]
    public void ReadTagList_Double_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_Double_Bytes;
        double[] values = NbtBinaryTestCases.TagList_Double_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<double> result = (TagList<double>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
        Assert.Equal(1, Math.Sign(1 / result[2])); // Check for +0
        Assert.Equal(-1, Math.Sign(1 / result[3])); // Check for -0
    }

    [Fact]
    public void ReadTagList_String_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_String_Bytes;
        string[] values = NbtBinaryTestCases.TagList_String_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<string> result = (TagList<string>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }

    [Fact]
    public void ReadTagList_ByteArray_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_ByteArray_Bytes;
        sbyte[][] values = NbtBinaryTestCases.TagList_ByteArray_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<sbyte[]> result = (TagList<sbyte[]>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }


    [Fact]
    public void ReadTagList_IntArray_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_IntArray_Bytes;
        int[][] values = NbtBinaryTestCases.TagList_IntArray_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<int[]> result = (TagList<int[]>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }


    [Fact]
    public void ReadTagList_LongArray_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_LongArray_Bytes;
        long[][] values = NbtBinaryTestCases.TagList_LongArray_Values;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<long[]> result = (TagList<long[]>)reader.ReadTagList();

        // Assert
        Assert.Equal(values.Length, result.Count);
        Assert.Equal(bytes.Length, stream.Position);
        Assert.Equal(values, result);
    }

    [Fact]
    public void ReadTagCompound_Simple_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagCompound_Simple_Bytes;
        TagCompound value = NbtBinaryTestCases.TagCompound_Simple_Value;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagCompound result = reader.ReadTagCompound();

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void ReadTagList_NestedList_ReadCorrectValues()
    {
        // Arrange
        byte[] bytes = NbtBinaryTestCases.TagList_IntList_Bytes;
        TagList<TagList> value = NbtBinaryTestCases.TagList_IntList_Value;

        using MemoryStream stream = new(bytes);
        using NbtReader reader = new(stream, NbtCompression.None); // Bypass compression detection to test internal method ReadTagCompound

        // Act
        TagList<TagList> result = (TagList<TagList>)reader.ReadTagList();

        // Assert
        Assert.Equal(value, result);
    }

    // TODO: Corner cases and errors
    // e.g. negative list length, invalid tag ID, insufficient items, etc.
}
