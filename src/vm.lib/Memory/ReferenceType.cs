using System.Runtime.InteropServices;

namespace vm.lib.Memory;

public enum ReferenceType : byte
{
    Array,
    Struct,
}

public enum DataType : byte
{
    I64,
    F64,
    Ptr,
}

[StructLayout(LayoutKind.Explicit)]
public struct Header
{
    [FieldOffset(0)]
    public ReferenceType Type;

    public const int Size = 8;
}

[StructLayout(LayoutKind.Explicit)]
public struct ArrayHeader
{
    [FieldOffset(Header.Size + 0)]
    public int Length;

    [FieldOffset(Header.Size + 4)]
    public int Stride;

    public const int Size = 
        Header.Size + 
        sizeof(int) + 
        sizeof(int);
}

[StructLayout(LayoutKind.Explicit)]
public struct StructHeader
{
    [FieldOffset(Header.Size + 0)]
    public int NumFields;

    [FieldOffset(Header.Size + 4)]
    public int StructInfoIndex;

    public static readonly int Size =
        Header.Size +
        sizeof(int) +
        sizeof(int);
}

public struct StructInfo
{
    public FieldInfo[] Fields;
}

[StructLayout(LayoutKind.Explicit)]
public struct FieldInfo
{
    [FieldOffset(0)]
    public int Offset;
    [FieldOffset(4)]
    public DataType DataType;
}