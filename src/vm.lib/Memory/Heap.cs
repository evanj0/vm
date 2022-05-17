using System.Runtime.CompilerServices;
using vm.lib.Exceptions;

namespace vm.lib.Memory;

public ref struct Heap
{
    public Heap(int size)
    {
        Data = new Span<byte>(new byte[size]);
        _index = 0;
        Structs = Array.Empty<StructInfo>();
    }

    public Span<byte> Data;
    private int _index;
    public StructInfo[] Structs;

    public int Alloc(int sizeBytes)
    {
        var pointer = _index;
        _index += sizeBytes;
        if (_index >= Data.Length)
        {
            throw new HeapOverflowException();
        }
        return pointer;
    }

    public unsafe void Write<T>(int pointer, T value, int size) where T : unmanaged
    {
        fixed(byte* dataPtr = &Data[pointer])
        {
            Buffer.MemoryCopy(&value, dataPtr, Data.Length - pointer, size);
        }
    }

    public void Write(int pointer, Word value)
    {
        Data[pointer + 0] = value.Byte0;
        Data[pointer + 1] = value.Byte1;
        Data[pointer + 2] = value.Byte2;
        Data[pointer + 3] = value.Byte3;
        Data[pointer + 4] = value.Byte4;
        Data[pointer + 5] = value.Byte5;
        Data[pointer + 6] = value.Byte6;
        Data[pointer + 7] = value.Byte7;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe byte* GetDataPtr(int pointer)
    {
        fixed (byte* dataPtr = &Data[pointer])
        {
            return dataPtr;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe T* GetDataPtr<T>(int pointer) where T : unmanaged
    {
        return (T*)GetDataPtr(pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe T Reinterpret<T>(int pointer) where T : unmanaged
    {
        return *(T*)GetDataPtr(pointer);
    }

    public unsafe Header GetHeader(int pointer)
    {
        return Reinterpret<Header>(pointer);
    }

    private unsafe void CheckHeaderType(int pointer, ReferenceType expectedType)
    {
        var header = Reinterpret<Header>(pointer);
        if (header.Type != expectedType)
        {
            throw new ObjectHeaderTypeMismatch(expectedType, header.Type);
        }
    }

    public unsafe int CreateArray(int length, int stride)
    {
        var pointer = Alloc(ArrayHeader.Size + (length * stride));
        var arrayHeader = new ArrayHeader
        {
            Length = length,
            Stride = stride,
        };
        Header* header = (Header*)&arrayHeader;
        header->Type = ReferenceType.Array;
        Write(pointer, arrayHeader, ArrayHeader.Size);
        return pointer;
    }

    public unsafe ArrayHeader GetArrayHeader(int pointer)
    {
        CheckHeaderType(pointer, ReferenceType.Array);
        return Reinterpret<ArrayHeader>(pointer);
    }

    public unsafe T GetArrayElement<T>(int pointer, int index) where T : unmanaged
    {
        var header = GetArrayHeader(pointer);
        if (index >= header.Length)
        {
            throw new IndexOutOfBoundsException(index, header.Length);
        }
        return Reinterpret<T>(pointer + ArrayHeader.Size + (index * header.Stride));
    }

    public unsafe void SetArrayElement<T>(int pointer, int index, T value, int size) where T : unmanaged
    {
        var header = GetArrayHeader(pointer);
        if (index >= header.Length)
        {
            throw new IndexOutOfBoundsException(index, header.Length);
        }
        Write(pointer + ArrayHeader.Size + (index * header.Stride), value, size);
    }

    public unsafe StructHeader GetStructHeader(int pointer)
    {
        CheckHeaderType(pointer, ReferenceType.Struct);
        return Reinterpret<StructHeader>(pointer);
    }

    public unsafe T GetStructField<T>(int pointer, int field) where T : unmanaged
    {
        var header = GetStructHeader(pointer);
        var structInfo = Structs[header.StructInfoIndex];
        if (field >= structInfo.Fields.Length)
        {
            throw new InvalidFieldException(header.StructInfoIndex, field);
        }
        return Reinterpret<T>(pointer + StructHeader.Size + structInfo.Fields[field].Offset);
    }
}