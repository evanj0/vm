using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm.lib;

public class Assembly
{
    public Assembly(Op[] ops, ProcInfo[] procTable, string[] strings)
    {
        Ops = ops;
        ProcTable = procTable;
        Strings = strings;
    }

    public const int MagicNumber = 0xF000;
    public const int Version = 1;
    public Op[] Ops { get; set; }
    public ProcInfo[] ProcTable { get; set; }
    public string[] Strings { get; set; }

    public byte[] Serialize()
    {
        var bw = new BinaryWriter()
            .I32_I32(MagicNumber, Version)
            .I32(Ops.Length);
        foreach (var op in Ops)
        {
            bw.Op(op);
        }
        return bw.ToByteArray();
    }

    public static Assembly Deserialize(byte[] data)
    {
        var ops = new List<Op>();
        var br = new BinaryReader(data)
            .I32_I32(out var magicNumber, out var version)
            .I32(out var opsLength);
        if (magicNumber != MagicNumber || version != Version)
        {
            throw new Exception("Invalid assembly file.");
        }
        for (var i = 0; i < opsLength; i++)
        {
            br.Op(out var op);
            ops.Add(op);
        }
        return new Assembly(ops.ToArray(), new ProcInfo[] { }, new string[] { });
    }
}

public class BinaryWriter
{
    public BinaryWriter()
    {
        _data = new();
    }

    private List<Word> _data;

    public BinaryWriter I64(long value)
    {
        _data.Add(Word.FromI64(value));
        return this;
    }

    public BinaryWriter I32(int value)
    {
        _data.Add(Word.FromI32(value));
        return this;
    }

    public BinaryWriter I32_I32(int value1, int value2)
    {
        var word = new Word();
        word.SetI32(sizeof(int) * 0, value1);
        word.SetI32(sizeof(int) * 1, value2);
        _data.Add(word);
        return this;
    }

    public BinaryWriter Op(Op op)
    {
        _data.Add(Word.FromI32((int)op.OpCode));
        _data.Add(op.Data);
        return this;
    }

    public byte[] ToByteArray()
    {
        var bytes = new byte[_data.Count * 8];

        for (var i = 0; i < _data.Count; i++)
        {
            bytes[(i * 8) + 0] = _data[i].Byte0;
            bytes[(i * 8) + 1] = _data[i].Byte1;
            bytes[(i * 8) + 2] = _data[i].Byte2;
            bytes[(i * 8) + 3] = _data[i].Byte3;
            bytes[(i * 8) + 4] = _data[i].Byte4;
            bytes[(i * 8) + 5] = _data[i].Byte5;
            bytes[(i * 8) + 6] = _data[i].Byte6;
            bytes[(i * 8) + 7] = _data[i].Byte7;
        }

        return bytes;
    }
}

public class BinaryReader
{
    private List<Word> _data;
    private int _index;

    public BinaryReader(byte[] bytes)
    {
        if (bytes.Length % 8 != 0)
        {
            throw new ArgumentException($"{nameof(bytes)}.Length must be a multiple of 8.");
        }

        _data = new();
        _index = 0;

        for (var i = 0; i < bytes.Length; i += 8)
        {
            var word = new Word
            {
                Byte0 = bytes[i + 0],
                Byte1 = bytes[i + 1],
                Byte2 = bytes[i + 2],
                Byte3 = bytes[i + 3],
                Byte4 = bytes[i + 4],
                Byte5 = bytes[i + 5],
                Byte6 = bytes[i + 6],
                Byte7 = bytes[i + 7]
            };
            _data.Add(word);
        }
    }

    public BinaryReader I64(out long value)
    {
        value = _data[_index].ToI64();
        _index++;
        return this;
    }

    public BinaryReader I32(out int value)
    {
        value = _data[_index].ToI32();
        _index++;
        return this;
    }

    public BinaryReader I32_I32(out int value1, out int value2)
    {
        value1 = _data[_index].ReadI32(sizeof(int) * 0);
        value2 = _data[_index].ReadI32(sizeof(int) * 1);
        _index++;
        return this;
    }

    public BinaryReader Op(out Op op)
    {
        op = new Op
        {
            OpCode = (OpCode)_data[_index].ToI32(),
            Data = _data[_index + 1]
        };
        _index += 2;
        return this;
    }
}