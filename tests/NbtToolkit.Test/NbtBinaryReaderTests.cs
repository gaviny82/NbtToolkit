using NbtToolkit.Binary;
using System.Buffers.Binary;
using System.Text;

namespace NbtToolkit.Test;

public class NbtBinaryReaderTests
{
    #region Read a single value

    [Fact]
    public void ReadByte_Default_ReturnsCorrectValue()
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
    public void ReadSByte_Default_ReturnsCorrectValue()
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
    public void ReadUInt16_Default_ReturnsCorrectValue()
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
    public void ReadInt16_Default_ReturnsCorrectValue()
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
    public void ReadInt32_Default_ReturnsCorrectValue()
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
    public void ReadInt64_Default_ReturnsCorrectValue()
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
    public void ReadSingle_Default_ReturnsCorrectValue()
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
    public void ReadDouble_Default_ReturnsCorrectValue()
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
    public void ReadString_Short_ReturnsCorrectValue()
        => ReadStringTest("Hello, world!");

    [Fact]
    public void ReadString_ShortNonAscii_ReturnsCorrectValue()
        => ReadStringTest("你好，世界！");

    [Fact]
    public void ReadString_Long_ReturnsCorrectValue()
        => ReadStringTest(new string('a', 1024));

    [Fact]
    public void ReadString_Empty_ReturnsCorrectValue()
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

    [Fact]
    public void ReadInt8Span_Default_ReturnsCorrectValue()
    {
        // Arrange
        sbyte[] expectedValue = [0x01, 0x02, 0x03, 0x04, 0x05];
        byte[] data = new byte[expectedValue.Length];
        for (int i = 0; i < expectedValue.Length; i++)
            data[i] = (byte)expectedValue[i];
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        Span<sbyte> actualValue = new sbyte[expectedValue.Length];

        // Act
        reader.ReadInt8Span(actualValue);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadInt16Span_Default_ReturnsCorrectValue()
    {
        // Arrange
        short[] expectedValue = [0x0102, 0x0304, 0x0506, 0x0708, 0x090A];
        byte[] data = new byte[expectedValue.Length * sizeof(short)];
        for (int i = 0; i < expectedValue.Length; i++)
            BinaryPrimitives.WriteInt16BigEndian(data.AsSpan(i * sizeof(short)), expectedValue[i]);
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        Span<short> actualValue = new short[expectedValue.Length];

        // Act
        reader.ReadInt16Span(actualValue);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadInt32Span_Default_ReturnsCorrectValue()
    {
        // Arrange
        int[] expectedValue = [0x01020304, 0x05060708, 0x090A0B0C, 0x0D0E0F10, 0x11121314];
        byte[] data = new byte[expectedValue.Length * sizeof(int)];
        for (int i = 0; i < expectedValue.Length; i++)
            BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(i * sizeof(int)), expectedValue[i]);
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        Span<int> actualValue = new int[expectedValue.Length];

        // Act
        reader.ReadInt32Span(actualValue);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadInt64Span_Default_ReturnsCorrectValue()
    {
        // Arrange
        long[] expectedValue = [0x0102030405060708, 0x090A0B0C0D0E0F10, 0x1112131415161718, 0x191A1B1C1D1E1F20, 0x2122232425262728];
        byte[] data = new byte[expectedValue.Length * sizeof(long)];
        for (int i = 0; i < expectedValue.Length; i++)
            BinaryPrimitives.WriteInt64BigEndian(data.AsSpan(i * sizeof(long)), expectedValue[i]);
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        Span<long> actualValue = new long[expectedValue.Length];

        // Act
        reader.ReadInt64Span(actualValue);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadSingleSpan_Default_ReturnsCorrectValue()
    {
        // Arrange
        float[] expectedValue = [3.14f, 2.718f, 1.618f, 0.577f, 0.123f];
        byte[] data = new byte[expectedValue.Length * sizeof(float)];
        for (int i = 0; i < expectedValue.Length; i++)
            BinaryPrimitives.WriteSingleBigEndian(data.AsSpan(i * sizeof(float)), expectedValue[i]);
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        Span<float> actualValue = new float[expectedValue.Length];

        // Act
        reader.ReadSingleSpan(actualValue);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void ReadDoubleSpan_Default_ReturnsCorrectValue()
    {
        // Arrange
        double[] expectedValue = [3.14, 2.718, 1.618, 0.577, 0.123];
        byte[] data = new byte[expectedValue.Length * sizeof(double)];
        for (int i = 0; i < expectedValue.Length; i++)
            BinaryPrimitives.WriteDoubleBigEndian(data.AsSpan(i * sizeof(double)), expectedValue[i]);
        using var reader = new NbtBinaryReader(new MemoryStream(data));

        Span<double> actualValue = new double[expectedValue.Length];

        // Act
        reader.ReadDoubleSpan(actualValue);

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }

    #endregion
}
