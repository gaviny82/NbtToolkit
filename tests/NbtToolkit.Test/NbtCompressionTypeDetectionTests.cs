using NbtToolkit.Binary;
using System.IO.Compression;
using System.Text;

namespace NbtToolkit.Test;

public class NbtCompressionTypeDetectionTests
{
    [Fact]
    public void DetectsGzipCompression()
    {
        // Arrange
        MemoryStream stream = CreateGzipStream("Test");

        // Act
        NbtCompression compressionType = NbtReader.DetectCompressionType(stream);

        // Assert
        Assert.Equal(NbtCompression.GZip, compressionType);
    }

    [Fact]
    public void DetectsZlibCompression()
    {
        // Arrange
        MemoryStream stream = CreateZlibStream("Test");

        // Act
        NbtCompression compressionType = NbtReader.DetectCompressionType(stream);

        // Assert
        Assert.Equal(NbtCompression.ZLib, compressionType);
    }

    [Fact]
    public void DetectsNoCompression()
    {
        // Arrange
        MemoryStream stream = new MemoryStream([0x0A /* TAG_Compound */, 0x00 /* TAG_End */]);

        // Act
        NbtCompression compressionType = NbtReader.DetectCompressionType(stream);

        // Assert
        Assert.Equal(NbtCompression.None, compressionType);
    }

    [Fact]
    public void DetectsEOF()
    {
        // Arrange
        MemoryStream stream = new MemoryStream();

        // Act & Assert
        Assert.Throws<EndOfStreamException>(() => NbtReader.DetectCompressionType(stream));
    }

    private static MemoryStream CreateGzipStream(string content)
    {
        var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
        using (var writer = new StreamWriter(gzipStream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(content);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    private static MemoryStream CreateZlibStream(string content)
    {
        var memoryStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
        using (var writer = new StreamWriter(zlibStream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(content);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }
}
