using BenchmarkDotNet.Attributes;
using System;
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
    private int _value;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _stream = new MemoryStream(_data);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _stream.Dispose();
    }

    [Benchmark]
    public void ReverseBytes_Explicit()
    {
        Span<byte> buffer = stackalloc byte[4];
        using var reader = new BinaryReader(_stream);
        for (int i = 0; i < N; i++)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            buffer[3] = reader.ReadByte();
            buffer[2] = reader.ReadByte();
            buffer[1] = reader.ReadByte();
            buffer[0] = reader.ReadByte();
            _value = BitConverter.ToInt32(buffer);
        }
    }

    [Benchmark]
    public void ReverseBytes_SpanReverse()
    {
        Span<byte> buffer = stackalloc byte[4];
        using var reader = new BinaryReader(_stream);
        for (int i = 0; i < N; i++)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            reader.Read(buffer);
            buffer.Reverse();
            _value = BitConverter.ToInt32(buffer);
        }
    }

    [Benchmark]
    public void ReverseBytes_BitOperation()
    {
        using var reader = new BinaryReader(_stream);
        for (int i = 0; i < N; i++)
        {
            _stream.Seek(0, SeekOrigin.Begin);
            int v = reader.ReadInt32();
            unchecked
            {
                var v2 = (uint)v;
                _value = (int)((v2 >> 24) & 0x000000FF |
                             (v2 >> 8) & 0x0000FF00 |
                             (v2 << 8) & 0x00FF0000 |
                             (v2 << 24) & 0xFF000000);
            }
        }
    }
}
