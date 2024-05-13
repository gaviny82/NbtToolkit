using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MinecraftToolkit.Nbt.Parsing;

public partial class NbtReader
{
    internal class ReversedEndiannessNbtBinaryReader : NbtBinaryReader
    {
        public ReversedEndiannessNbtBinaryReader(Stream stream) : this(stream, Encoding.UTF8) { }

        public ReversedEndiannessNbtBinaryReader(Stream stream, Encoding encoding, bool leaveOpen = false)
            : base(stream, encoding, leaveOpen) { }


        public override sbyte ReadSByte()
        {
            return base.ReadSByte();
        }

        public override ushort ReadUInt16()
        {
            ushort value = base.ReadUInt16();
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public override short ReadInt16()
        {
            short value = base.ReadInt16();
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public override int ReadInt32()
        {
            int value = base.ReadInt32();
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public override long ReadInt64()
        {
            long value = base.ReadInt64();
            return BinaryPrimitives.ReverseEndianness(value);
        }

        public override float ReadSingle()
        {
            float value = base.ReadSingle();
            uint bits = Unsafe.As<float, uint>(ref value);
            bits = BinaryPrimitives.ReverseEndianness(bits);
            return Unsafe.As<uint, float>(ref bits);
        }

        public override double ReadDouble()
        {
            double value = base.ReadDouble();
            ulong bits = Unsafe.As<double, ulong>(ref value);
            bits = BinaryPrimitives.ReverseEndianness(bits);
            return Unsafe.As<ulong, double>(ref bits);
        }

        public override int[] ReadInt32Array()
        {
            int[] arr = base.ReadInt32Array();
            BinaryPrimitives.ReverseEndianness(arr, arr);
            return arr;
        }

        public override long[] ReadInt64Array()
        {
            long[] arr = base.ReadInt64Array();
            BinaryPrimitives.ReverseEndianness(arr, arr);
            return arr;
        }
    }
}
