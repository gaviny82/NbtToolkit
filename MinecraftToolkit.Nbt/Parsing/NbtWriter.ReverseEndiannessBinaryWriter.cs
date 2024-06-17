using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Parsing;

public partial class NbtWriter
{
    internal class ReversedEndiannessBinaryWriter : DefaultEndiannessBinaryWriter
    {
        const int BufferSize = 128;
        private byte[] _buffer = new byte[BufferSize];

        public ReversedEndiannessBinaryWriter(Stream stream) : this(stream, Encoding.UTF8) { }

        public ReversedEndiannessBinaryWriter(Stream stream, Encoding encoding, bool leaveOpen = false)
            : base(stream, encoding, leaveOpen) { }

        public sealed override void Write(ushort value)
        {
            base.Write(BinaryPrimitives.ReverseEndianness(value));
        }

        public sealed override void Write(short value)
        {
            base.Write(BinaryPrimitives.ReverseEndianness(value));
        }

        public sealed override void Write(int value)
        {
            base.Write(BinaryPrimitives.ReverseEndianness(value));
        }

        public sealed override void Write(long value)
        {
            base.Write(BinaryPrimitives.ReverseEndianness(value));
        }

        public sealed override void Write(float value)
        {
            // Reverse endianness for float by reinterpreting as uint
            uint bits = Unsafe.As<float, uint>(ref value);
            bits = BinaryPrimitives.ReverseEndianness(bits);
            value = Unsafe.As<uint, float>(ref bits);

            base.Write(value);
        }

        public sealed override void Write(double value)
        {
            // Reverse endianness for double by reinterpreting as ulong
            ulong bits = Unsafe.As<double, ulong>(ref value);
            bits = BinaryPrimitives.ReverseEndianness(bits);
            value = Unsafe.As<ulong, double>(ref bits);

            base.Write(value);
        }

        // Impleted in base
        //public sealed override void Write(sbyte[] value)
        //{
        //}

        public unsafe sealed override void Write(int[] value)
        {
            Write(value.Length); // Write an Int32 length

            // TODO:
            throw new NotImplementedException();
            //base.Write(bytes); // Write data
        }

        public sealed override void Write(long[] value)
        {
            Write(value.Length); // Write an Int32 length

            // TODO:
            throw new NotImplementedException();
            //base.Write(bytes); // Write data
        }
    }
}
