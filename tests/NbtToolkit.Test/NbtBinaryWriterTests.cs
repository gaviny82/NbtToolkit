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

        // Assert
        byte[] bytes = stream.ToArray();

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

        // Assert
        byte[] bytes = stream.ToArray();

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

        // Assert
        byte[] bytes = stream.ToArray();
        short actualValue = BinaryPrimitives.ReadInt16BigEndian(bytes);

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

        // Assert
        byte[] bytes = stream.ToArray();

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

        // Assert
        byte[] bytes = stream.ToArray();
        int actualValue = BinaryPrimitives.ReadInt32BigEndian(bytes);

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

        // Assert
        byte[] bytes = stream.ToArray();
        long actualValue = BinaryPrimitives.ReadInt64BigEndian(bytes);

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

        // Assert
        byte[] bytes = stream.ToArray();
        float actualValue = BinaryPrimitives.ReadSingleBigEndian(bytes);

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

        // Assert
        byte[] bytes = stream.ToArray();
        double actualValue = BinaryPrimitives.ReadDoubleBigEndian(bytes);

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
        => WriteStringTest(new string('a', 1000));

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

        // Assert
        byte[] actualValue = stream.ToArray();
        int actualLength = actualValue.Length;

        Assert.Equal(expectedLength, actualLength);
        Assert.Equal(expectedValue, actualValue);
    }

    #endregion

    #region Write(ReadOnlySpan<short>)

    [Fact]
    public void Write_Int16Span_Short_WritesCorrectValue()
        => TestInt16Span([1, 2, 3, 4, 5]);

    [Fact]
    public void Write_Int16Span_Medium_WritesCorrectValue()
    {
        short[] input = Enumerable.Range(0, 1000)
            .Select(i => (short)i)
            .ToArray();

        TestInt16Span(input);
    }

    [Fact]
    public void Write_Int16Span_Long_WritesCorrectValue()
    {
        short[] input = Enumerable.Range(0, 100_000)
            .Select(i => (short)i)
            .ToArray();

        TestInt16Span(input);
    }

    [Fact]
    public void Write_Int16Span_Empty_WritesCorrectValue()
        => TestInt16Span(Array.Empty<short>());

    private static void TestInt16Span(Span<short> expectedValues)
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        int expectedLength = expectedValues.Length * sizeof(short);

        // Act
        writer.Write(expectedValues);

        // Assert
        byte[] actualBytes = stream.ToArray();
        int actualLength = actualBytes.Length;

        short[] actualValues = new short[expectedValues.Length];
        for (int i = 0; i < expectedValues.Length; i++)
        {
            actualValues[i] = BinaryPrimitives.ReadInt16BigEndian(actualBytes.AsSpan(i * sizeof(short)));
        }

        Assert.Equal(expectedLength, actualLength);
        Assert.Equal(expectedValues, actualValues);
    }

    #endregion

    #region Write(ReadOnlySpan<int>)

    [Fact]
    public void Write_Int32Span_Short_WritesCorrectValue()
        => TestInt32Span([-1, -2, 0, 1, 2]);

    [Fact]
    public void Write_Int32Span_Medium_WritesCorrectValue()
    {
        int[] input = Enumerable.Range(-500, 1000).ToArray();

        TestInt32Span(input);
    }

    [Fact]
    public void Write_Int32Span_Long_WritesCorrectValue()
    {
        int[] input = Enumerable.Range(-50_000, 100_000).ToArray();

        TestInt32Span(input);
    }

    [Fact]
    public void Write_Int32Span_Empty_WritesCorrectValue()
        => TestInt32Span(Array.Empty<int>());

    private static void TestInt32Span(Span<int> expectedValues)
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        int expectedLength = expectedValues.Length * sizeof(int);

        // Act
        writer.Write(expectedValues);

        // Assert
        byte[] actualBytes = stream.ToArray();
        int actualLength = actualBytes.Length;

        int[] actualValues = new int[expectedValues.Length];
        for (int i = 0; i < expectedValues.Length; i++)
        {
            actualValues[i] = BinaryPrimitives.ReadInt32BigEndian(actualBytes.AsSpan(i * sizeof(int)));
        }

        Assert.Equal(expectedLength, actualLength);
        Assert.Equal(expectedValues, actualValues);
    }

    #endregion

    #region Write(ReadOnlySpan<long>)

    [Fact]
    public void Write_Int64Span_Short_WritesCorrectValue()
        => TestInt64Span([-1L, -2L, 0L, 1L, 2L]);

    [Fact]
    public void Write_Int64Span_Medium_WritesCorrectValue()
    {
        long[] input = Enumerable.Range(-500, 1000)
            .Select(i => (long)i)
            .ToArray();

        TestInt64Span(input);
    }

    [Fact]
    public void Write_Int64Span_Long_WritesCorrectValue()
    {
        long[] input = Enumerable.Range(-50_000, 100_000)
            .Select(i => (long)i)
            .ToArray();

        TestInt64Span(input);
    }

    [Fact]
    public void Write_Int64Span_Empty_WritesCorrectValue()
        => TestInt64Span(Array.Empty<long>());

    private static void TestInt64Span(Span<long> expectedValues)
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        int expectedLength = expectedValues.Length * sizeof(long);

        // Act
        writer.Write(expectedValues);

        // Assert
        byte[] actualBytes = stream.ToArray();
        int actualLength = actualBytes.Length;

        long[] actualValues = new long[expectedValues.Length];
        for (int i = 0; i < expectedValues.Length; i++)
        {
            actualValues[i] = BinaryPrimitives.ReadInt64BigEndian(actualBytes.AsSpan(i * sizeof(long)));
        }

        Assert.Equal(expectedLength, actualLength);
        Assert.Equal(expectedValues, actualValues);
    }

    #endregion

    #region Write(ReadOnlySpan<float>)

    [Fact]
    public void Write_SingleSpan_Short_WritesCorrectValue()
        => TestSingleSpan([-1.02f, -2.02f, 0.02f, 1.02f, 2.02f]);

    [Fact]
    public void Write_SingleSpan_Medium_WritesCorrectValue()
    {
        float[] input = Enumerable.Range(-500, 1000)
            .Select(i => (float)i + 0.02f)
            .ToArray();

        TestSingleSpan(input);
    }

    [Fact]
    public void Write_SingleSpan_Long_WritesCorrectValue()
    {
        float[] input = Enumerable.Range(-50_000, 100_000)
            .Select(i => (float)i + 0.02f)
            .ToArray();

        TestSingleSpan(input);
    }

    [Fact]
    public void Write_SingleSpan_Empty_WritesCorrectValue()
        => TestSingleSpan(Array.Empty<float>());

    private static void TestSingleSpan(Span<float> expectedValues)
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        int expectedLength = expectedValues.Length * sizeof(float);

        // Act
        writer.Write(expectedValues);

        // Assert
        byte[] actualBytes = stream.ToArray();
        int actualLength = actualBytes.Length;

        float[] actualValues = new float[expectedValues.Length];
        for (int i = 0; i < expectedValues.Length; i++)
        {
            actualValues[i] = BinaryPrimitives.ReadSingleBigEndian(actualBytes.AsSpan(i * sizeof(float)));
        }

        Assert.Equal(expectedLength, actualLength);
        Assert.Equal(expectedValues, actualValues);
    }

    #endregion

    #region Write(ReadOnlySpan<double>)

    [Fact]
    public void Write_DoubleSpan_Short_WritesCorrectValue()
        => TestDoubleSpan([-1.02, -2.02, 0.02, 1.02, 2.02]);

    [Fact]
    public void Write_DoubleSpan_Medium_WritesCorrectValue()
    {
        double[] input = Enumerable.Range(-500, 1000)
            .Select(i => (double)i + 0.02f)
            .ToArray();

        TestDoubleSpan(input);
    }

    [Fact]
    public void Write_DoubleSpan_Long_WritesCorrectValue()
    {
        double[] input = Enumerable.Range(-50_000, 100_000)
            .Select(i => (double)i + 0.02f)
            .ToArray();

        TestDoubleSpan(input);
    }

    [Fact]
    public void Write_DoubleSpan_Empty_WritesCorrectValue()
        => TestDoubleSpan(Array.Empty<double>());

    private static void TestDoubleSpan(Span<double> expectedValues)
    {
        // Arrange
        var stream = new MemoryStream();
        using var writer = new NbtBinaryWriter(stream);

        int expectedLength = expectedValues.Length * sizeof(double);

        // Act
        writer.Write(expectedValues);

        // Assert
        byte[] actualBytes = stream.ToArray();
        int actualLength = actualBytes.Length;

        double[] actualValues = new double[expectedValues.Length];
        for (int i = 0; i < expectedValues.Length; i++)
        {
            actualValues[i] = BinaryPrimitives.ReadDoubleBigEndian(actualBytes.AsSpan(i * sizeof(double)));
        }

        Assert.Equal(expectedLength, actualLength);
        Assert.Equal(expectedValues, actualValues);
    }

    #endregion
}
