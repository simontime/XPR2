using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2 && args[0] != "-e" && args[0] != "-p")
        {
            Console.WriteLine("Usage: XPR2.exe [-e, -p] <file to extract, dir to pack> <(-p) output filename>");
            return;
        }
        else if (args[0] == "-e")
        {
            using (var input = File.OpenRead(args[1]))
            using (var reader = new BinaryReader(input))
            {
                var header = reader.ReadStruct<Header>();
                Extensions.Free();

                if (header.magic != "XPR2")
                {
                    Console.WriteLine("Error: Input file is not a valid XPR2 archive!");
                    return;
                }

                Console.WriteLine("{0}, size: 0x{1:x}, number of files: {2}\n",
                    header.magic, header.totalSize, header.numFiles.Flip());

                foreach (var descriptor in
                    reader.ReadStructs<Descriptor>((int)header.numFiles.Flip()))
                {
                    input.Position = descriptor.strTabOffset.Flip() + 0xC;

                    var filename = descriptor.rootDir + '\\' + reader.ReadASCIIZ();

                    Console.WriteLine("Extracting {0}...", filename);

                    TryExtract:
                    try
                    {
                        using (var outfile = File.OpenWrite(filename))
                        using (var writer = new BinaryWriter(outfile))
                        {
                            input.Position = descriptor.offset.Flip() + 0xC;
                            writer.Write(reader.ReadBytes((int)descriptor.length.Flip()));
                        }
                    }
                    catch
                    {
                        Utils.CreateRecursiveDirectories(filename);
                        goto TryExtract;
                    }
                }
            }
        }
        else if (args[0] == "-p")
        {
            if (args[1].Length != 4)
            {
                Console.WriteLine("Error: Input folder names should be exactly 4 characters in length!");
                return;
            }
            using (var input = File.OpenWrite(args[2]))
            using (var writer = new BinaryWriter(input))
            {
                var files = Directory.GetFiles(args[1], "*", SearchOption.AllDirectories);

                var len = files.Length;

                uint[] lengths = new uint[len],
                      offsets = new uint[len],
                      stringTableOffsets = new uint[len];

                int lastStringSize  = 0,
                    stringTableBase = 0x14 + (len * 0x10);

                for (int i = 0; i < len; i++)
                {
                    lastStringSize += i == 0 ? 0 : files[i - 1].Length - 4;
                    stringTableOffsets[i] = (uint)(stringTableBase + lastStringSize - 0xC);
                    lengths[i] = (uint)new FileInfo(files[i]).Length;
                }

                var accumulatedLength = (uint)(lastStringSize + stringTableBase + 0xC);

                for (int i = 0; i < len; i++)
                    offsets[i] = accumulatedLength += i == 0 ? 0 : lengths[i - 1];

                writer.Write("XPR2".ToCharArray());
                writer.Write(0);
                writer.Write(((ulong)len).Flip());

                for (int i = 0; i < len; i++)
                {
                    writer.Write(files[i].Substring(0, 4).ToCharArray());
                    writer.Write(offsets[i].Flip());
                    writer.Write(lengths[i].Flip());
                    writer.Write(stringTableOffsets[i].Flip());
                }

                writer.Write(0);

                for (int i = 0; i < len; i++)
                {
                    Console.WriteLine("Adding {0} to pack...", files[i].Substring(5));
                    writer.Write(files[i].Substring(5).ToCharArray());
                    writer.Write((byte)0);
                }

                writer.Write(0);

                for (int i = 0; i < len; i++)
                    writer.Write(File.ReadAllBytes(files[i]));

                writer.Write(new byte[0x800 - input.Length % 0x800 + 0xC]);

                input.Position = 4;

                writer.Write(((uint)input.Length - 0xC).Flip());
            }
        }

        Console.WriteLine("\nDone!");
    }
}