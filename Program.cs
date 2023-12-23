using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace BB__Unpacker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            FileAttributes attr = File.GetAttributes(args[0]);
            bool isDir = false;
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                isDir = true;
            if (!isDir)
            {
                Extract(args[0]);
                return;
            }
            else
            {
                Rebuild(args[0]);
                return;
            }
        }
        public static void Extract(string archive)
        {
            var reader = new BinaryReader(File.OpenRead(archive));
            reader.BaseStream.Position += 4;
            int fileCount = reader.ReadInt32();
            int[] fileStart = new int[fileCount];
            int[] fileEnd = new int[fileCount];
            for (int i = 0; i < fileCount; i++)
            {
                fileStart[i] = reader.ReadInt32();
                fileEnd[i] = reader.ReadInt32();
            }
            string inputDirectory = Path.GetFileNameWithoutExtension(archive) + "\\";
            Directory.CreateDirectory(Path.GetFileNameWithoutExtension(archive));
            for (int i = 0; i < fileCount; i++)
            {
                reader.BaseStream.Position = fileStart[i];
                byte[] file = reader.ReadBytes(fileEnd[i]);
                File.WriteAllBytes(inputDirectory + "File" + i + ".bmg", file);
            }
        }
        public static void Rebuild(string input)
        {
            string[] files = Directory.GetFiles(input, "*.bmg", SearchOption.TopDirectoryOnly);
            int[] filePointers = new int[files.Length];
            int[] fileLength = new int[files.Length];
            using (BinaryWriter writer = new BinaryWriter(File.Create(input + ".bb")))
            {
                writer.Write(Encoding.UTF8.GetBytes("BB"));
                writer.BaseStream.Position += 2;
                writer.Write(files.Length);
                writer.Write(new byte[files.Length * 8]);
                for (int i = 0; i < files.Length; i++)
                {
                    byte[] file = File.ReadAllBytes(files[i]);
                    fileLength[i] = file.Length;
                    filePointers[i] = (int)writer.BaseStream.Position;
                    writer.Write(file);
                }
                writer.BaseStream.Position = 0x8;
                for (int i = 0; i < files.Length; i++)
                {
                    writer.Write(filePointers[i]);
                    writer.Write(fileLength[i]);
                }
            }
        }
    }
}
