using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MinecraftToolkit.Nbt.Parsing;

public partial class NbtWriter
{
    internal class DefaultEndiannessBinaryWriter : BinaryWriter
    {
        public DefaultEndiannessBinaryWriter(Stream stream) : this(stream, Encoding.UTF8) { }

        public DefaultEndiannessBinaryWriter(Stream stream, Encoding encoding, bool leaveOpen = false)
            : base(stream, encoding, leaveOpen) { }

        public sealed override void Write(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int length = bytes.Length;
            if (length <= 0 || length > ushort.MaxValue)
                throw new OverflowException("Length of string is longer than the limit of UInt16");
            // TODO: Use modified UTF-8 encoding
            // TODO: Optimize to avoid creating byte[]
            base.Write((ushort)bytes.Length);
            base.Write(bytes, 0, bytes.Length);
        }

        public void Write(sbyte[] value)
        {
            base.Write(value.Length); // Write an Int32 length

            Span<byte> bytes = MemoryMarshal.AsBytes<sbyte>(value);
            base.Write(bytes); // Write data
        }

        public virtual void Write(int[] value)
        {
            base.Write(value.Length); // Write an Int32 length

            Span<byte> bytes = MemoryMarshal.AsBytes<int>(value);
            base.Write(bytes); // Write data
        }

        public virtual void Write(long[] value)
        {
            base.Write(value.Length); // Write an Int32 length

            Span<byte> bytes = MemoryMarshal.AsBytes<long>(value);
            base.Write(bytes); // Write data
        }
    }
}
