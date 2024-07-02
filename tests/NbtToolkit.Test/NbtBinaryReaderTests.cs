using NbtToolkit.Binary;
using System.Buffers.Binary;
using System.Text;

namespace NbtToolkit.Test;

public class NbtBinaryReaderTests
{
    #region Read a single value

    [Fact]
    public void ReadByte_Successful_ReturnsCorrectValue()
    {
        // Arrange
        byte expectedValue = 0x1A;
        byte[] data = [expectedValue];
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        // Act
        byte actualValue = reader.ReadByte();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadByte_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        var reader = new NbtBinaryReader(new MemoryStream());

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadByte());
    }

    [Fact]
    public void ReadSByte_Successful_ReturnsCorrectValue()
    {
        // Arrange
        sbyte expectedValue = -10;
        byte[] data = [(byte)expectedValue];
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        // Act
        sbyte actualValue = reader.ReadSByte();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadSByte_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        using var reader = new NbtBinaryReader(new MemoryStream());

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadSByte());
    }

    [Fact]
    public void ReadUInt16_Successful_ReturnsCorrectValue()
    {
        // Arrange
        ushort expectedValue = 12345;
        byte[] bytes = new byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, expectedValue);
        using var reader = new NbtBinaryReader(new MemoryStream(bytes));

        // Act
        int actualValue = reader.ReadUInt16();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadUInt16_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        var stream = new MemoryStream(new byte[sizeof(ushort) - 1]); // Less than enough bytes
        using var reader = new NbtBinaryReader(stream);

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadUInt16());
    }

    [Fact]
    public void ReadInt16_Successful_ReturnsCorrectValue()
    {
        // Arrange
        short expectedValue = -12345;
        byte[] bytes = new byte[sizeof(short)];
        BinaryPrimitives.WriteInt16BigEndian(bytes, expectedValue);
        using var reader = new NbtBinaryReader(new MemoryStream(bytes));

        // Act
        int actualValue = reader.ReadInt16();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadInt16_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        var stream = new MemoryStream(new byte[sizeof(short) - 1]); // Less than enough bytes
        using var reader = new NbtBinaryReader(stream);

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadInt16());
    }

    [Fact]
    public void ReadInt32_Successful_ReturnsCorrectValue()
    {
        // Arrange
        int expectedValue = -123456789;
        byte[] bytes = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, expectedValue);
        using var reader = new NbtBinaryReader(new MemoryStream(bytes));

        // Act
        int actualValue = reader.ReadInt32();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadInt32_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        var stream = new MemoryStream(new byte[sizeof(int) - 1]); // Less than enough bytes
        using var reader = new NbtBinaryReader(stream);

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadInt32());
    }

    [Fact]
    public void ReadInt64_Successful_ReturnsCorrectValue()
    {
        // Arrange
        long expectedValue = -123456789123456;
        byte[] bytes = new byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, expectedValue);
        using var reader = new NbtBinaryReader(new MemoryStream(bytes));

        // Act
        long actualValue = reader.ReadInt64();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadInt64_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        var stream = new MemoryStream(new byte[sizeof(long) - 1]); // Less than enough bytes
        using var reader = new NbtBinaryReader(stream);

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadInt64());
    }

    [Fact]
    public void ReadSingle_Successful_ReturnsCorrectValue()
    {
        // Arrange
        float expectedValue = 3.14f;
        byte[] bytes = new byte[sizeof(float)];
        BinaryPrimitives.WriteSingleBigEndian(bytes, expectedValue);
        using var reader = new NbtBinaryReader(new MemoryStream(bytes));

        // Act
        float actualValue = reader.ReadSingle();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadSingle_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        var stream = new MemoryStream(new byte[sizeof(float) - 1]); // Less than enough bytes
        using var reader = new NbtBinaryReader(stream);

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadSingle());
    }

    [Fact]
    public void ReadDouble_Successful_ReturnsCorrectValue()
    {
        // Arrange
        double expectedValue = 3.1415926;
        byte[] bytes = new byte[sizeof(double)];
        BinaryPrimitives.WriteDoubleBigEndian(bytes, expectedValue);
        using var reader = new NbtBinaryReader(new MemoryStream(bytes));

        // Act
        double actualValue = reader.ReadDouble();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadDouble_EndOfStream_ThrowsEOSException()
    {
        // Arrange
        var stream = new MemoryStream(new byte[sizeof(double) - 1]); // Less than enough bytes
        using var reader = new NbtBinaryReader(stream);

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadDouble());
    }

    #endregion

    #region ReadString()

    [Fact]
    public void ReadString_ShortString_ReturnsCorrectValue()
        => ReadStringTest("Hello, world!");

    [Fact]
    public void ReadString_ShortString_NonAscii_ReturnsCorrectValue()
        => ReadStringTest("你好，世界！");

    [Fact]
    public void ReadString_LongString_ReturnsCorrectValue()
        => ReadStringTest(new string('a', 1024));

    [Fact]
    public void ReadString_EmptyString_ReturnsCorrectValue()
    {
        // Arrange
        string expectedValue = "";
        byte[] data = [0x00, 0x00]; // length (ushort): 0
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        // Act
        string actualValue = reader.ReadString();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadString_NotEnoughBytes_ThrowsEOSException()
    {
        // Arrange
        byte[] strBytes = Encoding.UTF8.GetBytes("Hello, world!");

        ushort length = (ushort)strBytes.Length;
        byte[] data = new byte[sizeof(ushort) + length];
        BinaryPrimitives.WriteUInt16BigEndian(data, length);
        strBytes.CopyTo(data.AsSpan(sizeof(ushort)));

        data = data[0..^1]; // 1 less byte

        using var reader = new NbtBinaryReader(new MemoryStream(data));

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => reader.ReadString());
    }

    private static void ReadStringTest(string expectedValue)
    {
        // Arrange
        byte[] strBytes = Encoding.UTF8.GetBytes(expectedValue);

        ushort length = (ushort)strBytes.Length;
        byte[] data = new byte[sizeof(ushort) + length];
        BinaryPrimitives.WriteUInt16BigEndian(data, length);
        strBytes.CopyTo(data.AsSpan(sizeof(ushort)));

        using var reader = new NbtBinaryReader(new MemoryStream(data));

        // Act
        string actualValue = reader.ReadString();

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    #endregion

    #region Read a span of values


    #endregion
}
