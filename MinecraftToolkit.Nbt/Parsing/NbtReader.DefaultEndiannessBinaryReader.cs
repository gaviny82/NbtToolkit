using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Parsing;

public partial class NbtReader
{
    internal class NbtBinaryReader : BinaryReader
    {
        public NbtBinaryReader(Stream stream) : this(stream, Encoding.UTF8) { }

        public NbtBinaryReader(Stream stream, Encoding encoding, bool leaveOpen = false)
            : base(stream, encoding, leaveOpen) { }

        /// <summary>
        /// Reads a modified UTF-8 string prefixed by an <see cref="ushort"/> length
        /// </summary>
        /// <returns>A <see cref="string"/> of length encoded in the stream</returns>
        public sealed override string ReadString()
        {
            ushort length = ReadUInt16();
            // The number of characters in the string is unknown due to the variable length UTF-8 encoding, so string.Create<T> cannot be used
            // TODO: Use modified UTF-8 encoding
            // TODO: Optimize reading strings by allocating a buffer for short strings, only create new byte[] for long strings
            return Encoding.UTF8.GetString(base.ReadBytes(length));
        }

        /// <summary>
        /// Reads a signed byte array prefixed by an <see cref="int"/> length
        /// </summary>
        /// <returns>A signed byte array</returns>
        public virtual sbyte[] ReadSByteArray()
        {
            int length = ReadInt32();
            sbyte[] data = new sbyte[length];
            Read(MemoryMarshal.AsBytes<sbyte>(data));
            return data;
        }

        /// <summary>
        /// Reads an int array prefixed by an <see cref="int"/> length
        /// </summary>
        /// <returns>An int array</returns>
        public virtual int[] ReadInt32Array()
        {
            int length = ReadInt32();
            int[] data = new int[length];
            Read(MemoryMarshal.AsBytes<int>(data));
            return data;
        }

        /// <summary>
        /// Reads a long array prefixed by an <see cref="int"/> length
        /// </summary>
        /// <returns>A long array</returns>
        public virtual long[] ReadInt64Array()
        {
            int length = ReadInt32();
            long[] data = new long[length];
            Read(MemoryMarshal.AsBytes<long>(data));
            return data;
        }
    }
}
