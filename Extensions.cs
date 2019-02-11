using System;
using System.IO;
using System.Text;
using static System.Runtime.InteropServices.Marshal;

public static class Extensions
{
    private static IntPtr Ptr;

    public static T ReadStruct<T>(this BinaryReader reader) where T : struct
    {
        var len = SizeOf<T>();
        Ptr = AllocHGlobal(len);
        Copy(reader.ReadBytes(len), 0, Ptr, len);
        return PtrToStructure<T>(Ptr);
    }

    public static T[] ReadStructs<T>(this BinaryReader reader, int numStructs) where T : struct
    {
        var buf = new T[numStructs];
        var len = SizeOf<T>();
        Ptr = AllocHGlobal(len * numStructs);
        Copy(reader.ReadBytes(len * numStructs), 0, Ptr, len * numStructs);
        for (int i = 0; i < numStructs; i++)
            buf[i] = PtrToStructure<T>(Ptr + i * len);
        FreeHGlobal(Ptr);
        return buf;
    }

    public static void Free() => FreeHGlobal(Ptr);

    public static string ReadASCII(this BinaryReader input, int size) => Encoding.ASCII.GetString(input.ReadBytes(size), 0, size);

    public static string ReadASCIIZ(this BinaryReader input)
    {
        var start = input.BaseStream.Position;
        var size = 0;

        while (input.BaseStream.ReadByte() - 1 > 0)
            size++;

        input.BaseStream.Position = start;
        var text = input.ReadASCII(size);
        input.BaseStream.Position++;
        return text;
    }
}