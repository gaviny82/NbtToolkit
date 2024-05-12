using BenchmarkDotNet.Attributes;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftToolkit.Nbt.Benchmark;

public class ReverseByteOrder
{
    [Params(1000)]
    public int N { get; set; }

    private byte[] _data = [0x00, 0x00, 0x11, 0x12];
    private MemoryStream _stream = null!;
    private BinaryReader _reader = null!;

    private int _value;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _stream = new MemoryStream(_data);
        _reader = new BinaryReader(_stream);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _reader.Dispose();
        _stream.Dispose();
    }

    [Benchmark]
    public void ReverseBytes_Explicit()
    {
        Span<byte> buffer = stackalloc byte[4];
        for (int i = 0; i < N; i++)
        {
            _stream.Position = 0;
            buffer[3] = _reader.ReadByte();
            buffer[2] = _reader.ReadByte();
            buffer[1] = _reader.ReadByte();
            buffer[0] = _reader.ReadByte();
            _value = BitConverter.ToInt32(buffer);
        }
    }

    [Benchmark(Baseline = true)]
    public void ReverseBytes_SpanReverse()
    {
        Span<byte> buffer = stackalloc byte[4];
        for (int i = 0; i < N; i++)
        {
            _stream.Position = 0;
            _reader.Read(buffer);
            buffer.Reverse();
            _value = BitConverter.ToInt32(buffer);
        }
    }

    [Benchmark]
    public void ReverseBytes_BitOperation()
    {
        for (int i = 0; i < N; i++)
        {
            _stream.Position = 0;
            uint v = (uint)_reader.ReadInt32();
            _value = (int)((v >> 24) /* & 0x000000FF */ |
                           (v >> 8) & 0x0000FF00 |
                           (v << 8) & 0x00FF0000 |
                           (v << 24) /* & 0xFF000000 */);
        }
    }

    [Benchmark]
    public void ReverseBytes_BuiltIn()
    {
       for (int i = 0; i < N; i++)
        {
            _stream.Position = 0;
            int v = _reader.ReadInt32();
            _value = BinaryPrimitives.ReverseEndianness(v);
        }
    }
}
