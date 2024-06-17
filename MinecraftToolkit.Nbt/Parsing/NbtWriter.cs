using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static MinecraftToolkit.Nbt.Parsing.NbtReader;

namespace MinecraftToolkit.Nbt.Parsing;

public partial class NbtWriter : IDisposable
{
    public Stream Stream { get; init; }

    internal DefaultEndiannessBinaryWriter BinaryWriter => _writer;

    private readonly DefaultEndiannessBinaryWriter _writer;

    public NbtWriter(Stream stream, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(stream, CompressionMode.Decompress),
            NbtCompression.ZLib => new ZLibStream(stream, CompressionMode.Decompress),
            NbtCompression.None => stream,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };
        _writer = BitConverter.IsLittleEndian
            ? new ReversedEndiannessBinaryWriter(Stream, Encoding.UTF8, leaveOpen)
            : new DefaultEndiannessBinaryWriter(Stream, Encoding.UTF8, leaveOpen);
    }

    public void WriteRootTag(TagCompound tag)
    {
        tag.WriteTag(this, "");
    }

    internal void Write(TagId id)
    {
        byte data = (byte)id;
        if (data > (byte)TagId.LongArray)
            throw new ArgumentException("Invalid TagId", nameof(id));
        _writer.Write(data);
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}
