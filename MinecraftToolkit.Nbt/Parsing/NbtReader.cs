using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Parsing;

public class NbtReader
{
    public Stream Stream { get; init; }

    private BinaryReader _reader;

    public NbtReader(Stream stream, NbtCompression compression = NbtCompression.None)
    {
        Stream = compression switch
        {
            NbtCompression.GZip => new GZipStream(stream, CompressionMode.Decompress),
            NbtCompression.ZLib => new ZLibStream(stream, CompressionMode.Decompress),
            NbtCompression.None => stream,
            _ => throw new ArgumentException(nameof(compression))
        };

        _reader = new BinaryReader(Stream, Encoding.UTF8);
    }

    /// <summary>
    /// Reads the payload of a compound tag
    /// </summary>
    /// <returns>A <see cref="TagCompound"/> instance</returns>
    public TagCompound ReadTagCompound()
    {
        var tagCompound = new TagCompound();
        Span<byte> tagNameLengthBuffer = stackalloc byte[2];
        do
        {
            // Read the 1-byte tag type (ID)
            byte tagId = _reader.ReadByte();
            TagId tagType = tagId switch
            {
                < TagIdValues.Min or > TagIdValues.Max => throw new InvalidDataException($"Invalid tag ID {tagId}"),
                _ => (TagId)tagId
            };

            if (tagType == TagId.End) // End of compound tag
                return tagCompound;

            // Read tag name length as big-endian ushort (2 bytes)
            tagNameLengthBuffer[1] = _reader.ReadByte();
            tagNameLengthBuffer[0] = _reader.ReadByte();
            ushort tagNameLength = BitConverter.ToUInt16(tagNameLengthBuffer);

            // Read tag name as UTF-8 string
            string tagName = string.Create(tagNameLength, _reader, (span, reader) =>
            {
                int charsRead = _reader.Read(span);
                if (charsRead != span.Length)
                    throw new InvalidDataException($"Expected tag name length: {span.Length}");
            });

            // Read tag payload
            Tag tag = tagType switch
            {
                TagId.Byte => ReadTagByte(),
                TagId.Short => ReadTagShort(),
                TagId.Int => ReadTagInt(),
                TagId.Long => ReadTagLong(),
                TagId.Float => ReadTagFloat(),
                TagId.Double => ReadTagDouble(),
                TagId.ByteArray => ReadTagByteArray(),
                TagId.String => ReadTagString(),
                TagId.List => ReadTagList(), // May be recursive
                TagId.Compound => ReadTagCompound(), // Recursive
                TagId.IntArray => ReadTagIntArray(),
                TagId.LongArray => ReadTagLongArray(),
                _ => throw new InvalidDataException($"Invalid tag ID {tagId}")
            };

            // Add tag to compound
            tagCompound.Add(tagName, tag);
        } while (true);
    }

    internal TagByte ReadTagByte()
    {
        return new TagByte((sbyte)_reader.ReadByte());
    }

    internal TagShort ReadTagShort()
    {
        Span<byte> buffer = stackalloc byte[2];
        buffer[1] = _reader.ReadByte();
        buffer[0] = _reader.ReadByte();
        return new TagShort(BitConverter.ToInt16(buffer));
    }

    internal TagInt ReadTagInt()
    {
        Span<byte> buffer = stackalloc byte[4];
        buffer[3] = _reader.ReadByte();
        buffer[2] = _reader.ReadByte();
        buffer[1] = _reader.ReadByte();
        buffer[0] = _reader.ReadByte();
        return new TagInt(BitConverter.ToInt32(buffer));
    }

    internal TagLong ReadTagLong()
    {
        Span<byte> buffer = stackalloc byte[8];
        buffer[7] = _reader.ReadByte();
        buffer[6] = _reader.ReadByte();
        buffer[5] = _reader.ReadByte();
        buffer[4] = _reader.ReadByte();
        buffer[3] = _reader.ReadByte();
        buffer[2] = _reader.ReadByte();
        buffer[1] = _reader.ReadByte();
        buffer[0] = _reader.ReadByte();
        return new TagLong(BitConverter.ToInt64(buffer));
    }

    internal TagFloat ReadTagFloat()
    {
        Span<byte> buffer = stackalloc byte[4];
        buffer[3] = _reader.ReadByte();
        buffer[2] = _reader.ReadByte();
        buffer[1] = _reader.ReadByte();
        buffer[0] = _reader.ReadByte();
        return new TagFloat(BitConverter.ToSingle(buffer));
    }

    internal TagDouble ReadTagDouble()
    {
        Span<byte> buffer = stackalloc byte[8];
        buffer[7] = _reader.ReadByte();
        buffer[6] = _reader.ReadByte();
        buffer[5] = _reader.ReadByte();
        buffer[4] = _reader.ReadByte();
        buffer[3] = _reader.ReadByte();
        buffer[2] = _reader.ReadByte();
        buffer[1] = _reader.ReadByte();
        buffer[0] = _reader.ReadByte();
        return new TagDouble(BitConverter.ToDouble(buffer));
    }

    internal TagByteArray ReadTagByteArray()
    {
        throw new NotImplementedException();
    }

    internal TagString ReadTagString()
    {
        throw new NotImplementedException();
    }

    internal Tag ReadTagList()
    {
        throw new NotImplementedException();
    }

    internal TagIntArray ReadTagIntArray()
    {
        throw new NotImplementedException();
    }

    internal TagLongArray ReadTagLongArray()
    {
        throw new NotImplementedException();
    }
}
