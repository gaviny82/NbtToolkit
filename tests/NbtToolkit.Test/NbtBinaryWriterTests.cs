using NbtToolkit.Binary;
using System.Buffers.Binary;
using System.Text;

namespace NbtToolkit.Test;

public class NbtBinaryWriterTests
{
    #region Write a single value

    [Fact]
    public void Write_Byte_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        byte expectedValue = 42;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();

        // Assert
        Assert.Equal(1, stream.Position);
        Assert.Equal(expectedValue, bytes[0]);
    }

    [Fact]
    public void Write_SByte_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        sbyte expectedValue = -42;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();

        // Assert
        Assert.Equal(1, stream.Position);
        Assert.Equal((byte)expectedValue, bytes[0]);
    }

    [Fact]
    public void Write_Short_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        short expectedValue = -420;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();
        short actualValue = BinaryPrimitives.ReadInt16BigEndian(bytes);

        // Assert
        Assert.Equal(2, stream.Position);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Write_UShort_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        ushort expectedValue = 420;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();

        // Assert
        Assert.Equal(2, stream.Position);
        Assert.Equal(expectedValue, BinaryPrimitives.ReadUInt16BigEndian(bytes));
    }

    [Fact]
    public void Write_Int_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        int expectedValue = -42000000;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();
        int actualValue = BinaryPrimitives.ReadInt32BigEndian(bytes);

        // Assert
        Assert.Equal(4, stream.Position);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Write_Long_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        long expectedValue = -4200000000000000;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();
        long actualValue = BinaryPrimitives.ReadInt64BigEndian(bytes);

        // Assert
        Assert.Equal(8, stream.Position);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Write_Float_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        float expectedValue = 42.42f;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();
        float actualValue = BinaryPrimitives.ReadSingleBigEndian(bytes);

        // Assert
        Assert.Equal(4, stream.Position);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void Write_Double_Default_WritesCorrectValue()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        double expectedValue = 42.42424242;

        // Act
        writer.Write(expectedValue);
        byte[] bytes = stream.ToArray();
        double actualValue = BinaryPrimitives.ReadDoubleBigEndian(bytes);

        // Assert
        Assert.Equal(8, stream.Position);
        Assert.Equal(expectedValue, actualValue);
    }

    #endregion

    #region WriteString()

    [Fact]
    public void WriteString_Short_WritesCorrectValue() // stack buffer
        => WriteStringTest("Hello, world!");

    [Fact]
    public void WriteString_ShortNonAscii_WritesCorrectValue()
        => WriteStringTest("你好，世界！");

    [Fact]
    public void WriteString_Medium_WritesCorrectValue() // ArrayPool buffer
        => WriteStringTest(new string('a', 1024));

    [Fact]
    public void WriteString_Long_WritesCorrectValue() // ArrayPool buffer and copy in pieces
        => WriteStringTest(new string('a', ushort.MaxValue));

    [Fact]
    public void WriteString_Empty_WritesCorrectValue()
        => WriteStringTest("");

    [Fact]
    public void WriteString_Overlong_ThrowsArgumentException()
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        // Act
        void action() => writer.WriteString(new string('a', ushort.MaxValue + 1));

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    private static void WriteStringTest(string str)
    {
        // Arrange
        byte[] strBytes = Encoding.UTF8.GetBytes(str);
        ushort strBytesLength = checked((ushort)strBytes.Length);
        int expectedLength = strBytesLength + sizeof(ushort);

        byte[] expectedValue = new byte[expectedLength];
        BinaryPrimitives.WriteUInt16BigEndian(expectedValue, strBytesLength);
        Array.Copy(strBytes, 0, expectedValue, sizeof(ushort), strBytesLength);

        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        // Act
        writer.WriteString(str);
        byte[] actualValue = stream.ToArray();
        int actualLength = actualValue.Length;

        // Assert
        Assert.Equal(expectedLength, actualLength);
        Assert.Equal(expectedValue, actualValue);
    }

    #endregion

    #region Write a span of values

    #endregion
}
