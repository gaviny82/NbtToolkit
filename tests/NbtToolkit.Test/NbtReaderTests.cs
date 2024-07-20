using NbtToolkit.Binary;

namespace NbtToolkit.Test;

public class NbtReaderTests
{
    [Fact]
    public void ReadTagList_Byte_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x01, // Content tag ID: TAG_Byte
            0x00, 0x00, 0x00, 0x04, // Length: 4
            0x7B, // Item 1: 123
            0x7F, // Item 2: 127
            0x00, // Item 2: 0
            0x80, // Item 3: -128
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<sbyte> result = (TagList<sbyte>)reader.ReadTagList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal(new sbyte[] { 123, 127, 0, -128 }, result);
        Assert.Equal(data.Length, stream.Position);
    }

    [Fact]
    public void ReadTagList_Short_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x02, // Content tag ID: TAG_Short
            0x00, 0x00, 0x00, 0x03, // Length: 3
            0x00, 0x7B, // Item 1: 123
            0x00, 0x00, // Item 2: 0
            0xFF, 0x85, // Item 3: -123
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<short> result = (TagList<short>)reader.ReadTagList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new short[] { 123, 0, -123 }, result);
        Assert.Equal(data.Length, stream.Position);
    }

    [Fact]
    public void ReadTagList_Int_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x03, // Content tag ID: TAG_Int
            0x00, 0x00, 0x00, 0x03, // Length: 3
            0x00, 0x00, 0x00, 0x7B, // Item 1: 123
            0x00, 0x00, 0x00, 0x00, // Item 2: 0
            0xFF, 0xFF, 0xFF, 0x85, // Item 3: -123
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<int> result = (TagList<int>)reader.ReadTagList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new int[] { 123, 0, -123 }, result);
        Assert.Equal(data.Length, stream.Position);
    }

    [Fact]
    public void ReadTagList_Long_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x04, // Content tag ID: TAG_Long
            0x00, 0x00, 0x00, 0x03, // Length: 3
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7B, // Item 1: 123
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 2: 0
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x85, // Item 3: -123
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<long> result = (TagList<long>)reader.ReadTagList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new long[] { 123, 0, -123 }, result);
        Assert.Equal(data.Length, stream.Position);
    }

    [Fact]
    public void ReadTagList_Float_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x05, // Content tag ID: TAG_Float
            0x00, 0x00, 0x00, 0x07, // Length: 7
            0x7F, 0x80, 0x00, 0x00, // Item 1: +Inf
            0xFF, 0x80, 0x00, 0x00, // Item 2: -Inf
            0x00, 0x00, 0x00, 0x00, // Item 3: +0
            0x80, 0x00, 0x00, 0x00, // Item 4: -0
            0x7F, 0xC0, 0x00, 0x00, // Item 5: NaN
            0x40, 0x48, 0xF5, 0xC3, // Item 6: 3.1400001049041748046875
            0xC0, 0x48, 0xF5, 0xC3, // Item 6: -3.1400001049041748046875
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<float> result = (TagList<float>)reader.ReadTagList();

        // Assert
        Assert.Equal(7, result.Count);
        Assert.Equal(new float[] { float.PositiveInfinity, float.NegativeInfinity, 0.0f, -0.0f, float.NaN, 3.1400001049041748046875f, -3.1400001049041748046875f }, result);
        Assert.Equal(1, Math.Sign(1 / result[2])); // Check for +0
        Assert.Equal(-1, Math.Sign(1 / result[3])); // Check for -0
        Assert.Equal(data.Length, stream.Position);
    }

    [Fact]
    public void ReadTagList_Double_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x06, // Content tag ID: TAG_Double
            0x00, 0x00, 0x00, 0x07, // Length: 7
            0x7F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 1: +Inf
            0xFF, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 1: -Inf
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 3: +0
            0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 4: -0
            0x7F, 0xF8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Item 5: NaN
            0x40, 0x09, 0x1E, 0xB8, 0x51, 0xEB, 0x85, 0x1F, // Item 6: 3.140000000000000124344978758017532527446746826171875
            0xC0, 0x09, 0x1E, 0xB8, 0x51, 0xEB, 0x85, 0x1F, // Item 7: -3.140000000000000124344978758017532527446746826171875
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<double> result = (TagList<double>)reader.ReadTagList();

        // Assert
        Assert.Equal(7, result.Count);
        Assert.Equal(new double[] { double.PositiveInfinity, double.NegativeInfinity, 0.0, -0.0, float.NaN, 3.140000000000000124344978758017532527446746826171875, -3.140000000000000124344978758017532527446746826171875 }, result);
        Assert.Equal(1, Math.Sign(1 / result[2])); // Check for +0
        Assert.Equal(-1, Math.Sign(1 / result[3])); // Check for -0
        Assert.Equal(data.Length, stream.Position);
    }

    [Fact]
    public void ReadTagList_String_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x08, // Content tag ID: TAG_String
            0x00, 0x00, 0x00, 0x03, // Length: 3
            0x00, 0x00, // Item 1: length = 0
                        // Item 1: empty string
            0x00, 0x06, // Item 2: length = 6
            0xE4, 0xBD, 0xA0, 0xE5, 0xA5, 0xBD, // Item 2: "你好"
            0x00, 0x04, // Item 3: length = 4
            0x6E, 0x62, 0x74, 0x21, // Item 3: "nbt!"
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<string> result = (TagList<string>)reader.ReadTagList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new string[] { "", "你好", "nbt!" }, result);
        Assert.Equal(data.Length, stream.Position);
    }

    [Fact]
    public void ReadTagList_ByteArray_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x07, // Content tag ID: TAG_Byte_Array
            0x00, 0x00, 0x00, 0x03, // Length: 3
            // Array 1: []
            0x00, 0x00, 0x00, 0x00, // Length = 0
            // Array 2: [-128, 127]
            0x00, 0x00, 0x00, 0x02, // Length = 2
            0x80, 0x7F,
            // Array 3: [-2, -1, 0, 1]
            0x00, 0x00, 0x00, 0x04, // Length = 4
            0xFE, 0xFF, 0x00, 0x01,
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<sbyte[]> result = (TagList<sbyte[]>)reader.ReadTagList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new sbyte[][] { [], [-128, 127], [-2, -1, 0, 1] }, result);
        Assert.Equal(data.Length, stream.Position);
    }


    [Fact]
    public void ReadTagList_IntArray_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x0B, // Content tag ID: TAG_Int_Array
            0x00, 0x00, 0x00, 0x03, // Length: 3
            // Array 1: []
            0x00, 0x00, 0x00, 0x00, // Length = 0
            // Array 2: [-128, 127]
            0x00, 0x00, 0x00, 0x02, // Length = 2
            0xFF, 0xFF, 0xFF, 0x80,
            0x00, 0x00, 0x00, 0x7F,
            // Array 3: [-2, -1, 0, 1]
            0x00, 0x00, 0x00, 0x04, // Length = 4
            0xFF, 0xFF, 0xFF, 0xFE,
            0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01,
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<int[]> result = (TagList<int[]>)reader.ReadTagList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new int[][] { [], [-128, 127], [-2, -1, 0, 1] }, result);
        Assert.Equal(data.Length, stream.Position);
    }


    [Fact]
    public void ReadTagList_LongArray_ReadCorrectValues()
    {
        // Arrange
        byte[] data = [
            0x0C, // Content tag ID: TAG_Long_Array
            0x00, 0x00, 0x00, 0x03, // Length: 3
            // Array 1: []
            0x00, 0x00, 0x00, 0x00, // Length = 0
            // Array 2: [-128, 127]
            0x00, 0x00, 0x00, 0x02, // Length = 2
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x80,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F,
            // Array 3: [-2, -1, 0, 1]
            0x00, 0x00, 0x00, 0x04, // Length = 4
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE,
            0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
        ];

        using MemoryStream stream = new(data);
        using NbtReader reader = new(stream);

        // Act
        TagList<long[]> result = (TagList<long[]>)reader.ReadTagList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new long[][] { [], [-128, 127], [-2, -1, 0, 1] }, result);
        Assert.Equal(data.Length, stream.Position);
    }

    // TODO: List of lists, list of compounds

    // TODO: Corner cases and errors
    // e.g. negative list length, invalid tag ID, insufficient items, etc.
}
