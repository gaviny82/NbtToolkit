using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Binary;

public partial class NbtWriter : IDisposable
{
    public Stream Stream { get; init; }

    private readonly NbtBinaryWriter _writer;

    public NbtWriter(Stream output, NbtCompression compression = NbtCompression.None, bool leaveOpen = false)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(output, CompressionMode.Compress),
            NbtCompression.ZLib => new ZLibStream(output, CompressionMode.Compress),
            NbtCompression.None => output,
            _ => throw new ArgumentException("Invalid compression type", nameof(compression))
        };
        _writer = new NbtBinaryWriter(Stream, leaveOpen);
    }

    public void WriteRootTag(TagCompound tag)
    {
        tag.WriteBinary(_writer, "");
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}
