using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Binary;

/// <summary>
/// Writes binary NBT data to a stream.
/// </summary>
public partial class NbtWriter : IDisposable
{
    /// <summary>
    /// The <see cref="System.IO.Stream"/> to which this <see cref="NbtWriter"/> writes.
    /// </summary>
    public Stream Stream { get; init; }

    private readonly NbtBinaryWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="NbtWriter"/> class for the specified stream.
    /// </summary>
    /// <param name="output">The <see cref="System.IO.Stream"/> to which this <see cref="NbtWriter"/> writes.</param>
    /// <param name="compression">Compression type of the data written to the <paramref name="output"/> stream.</param>
    /// <param name="leaveOpen">If the <paramref name="output"/> stream is left open when this <see cref="NbtWriter"/> is disposed.</param>
    /// <exception cref="ArgumentException">Invalid <see cref="NbtCompression"/> provided by <paramref name="compression"/>.</exception>
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

    /// <summary>
    /// Writes a <see cref="TagCompound"/> as the root tag to <see cref="Stream"/> using NBT binary format.
    /// </summary>
    /// <param name="tag">The root tag to be written.</param>
    public void WriteRootTag(TagCompound tag)
    {
        tag.WriteBinary(_writer, "");
    }

    /// <summary>
    /// Disposes the <see cref="NbtWriter"/> and the underlying <see cref="System.IO.Stream"/> (unless it is configured to be left open).
    /// </summary>
    public void Dispose()
    {
        _writer.Dispose();
    }
}
