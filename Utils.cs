using System.IO;

public static class Utils
{
    public static uint Flip(this uint value) => 
        (value & 0xFF) << 24 | 
        (value & 0xFF00) << 8 | 
        (value & 0xFF0000) >> 8 |
        (value & 0xFF000000) >> 24;

    public static ulong Flip(this ulong value) => 
        (value & 0xFF) << 56 |
        (value & 0xFF00) << 40 | 
        (value & 0xFF0000) << 24 | 
        (value & 0xFF000000) << 8 |
        (value & 0xFF00000000) >> 8 |
        (value & 0xFF0000000000) >> 24 | 
        (value & 0xFF000000000000) >> 40 | 
        (value & 0xFF00000000000000) >> 56;

    public static void CreateRecursiveDirectories(string pathName) => 
        Directory.CreateDirectory(pathName.Substring(0, pathName.LastIndexOf('\\')));
}