using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
struct Header
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5), FieldOffset(0)]
    public string magic;
    [FieldOffset(4)]
    public uint totalSize;
    [FieldOffset(8)]
    public ulong numFiles;
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
struct Descriptor
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5), FieldOffset(0)]
    public string rootDir;
    [FieldOffset(4)]
    public uint offset;
    [FieldOffset(8)]
    public uint length;
    [FieldOffset(0xC)]
    public uint strTabOffset;
}