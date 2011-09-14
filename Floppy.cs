using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace x86CS
{
    public class Floppy
    {
        private FileStream floppyStream;
        BinaryReader floppyReader;
        bool mounted = false;

        public bool Mounted
        {
            get { return mounted; }
        }

        public Floppy()
        {
        }

        public bool MountImage(string imagePath)
        {
            if (!File.Exists(imagePath))
                return false;

            try
            {
                floppyStream = File.OpenRead(imagePath);
                floppyReader = new BinaryReader(floppyStream);
            }
            catch (Exception)
            {
                return false;
            }

            mounted = true;
            return true;
        }

        public byte ReadByte()
        {
            return floppyReader.ReadByte();
        }

        public ushort ReadWord()
        {
            return floppyReader.ReadUInt16();
        }

        public byte[] ReadBytes(int count)
        {
            return floppyReader.ReadBytes(count);
        }

        public void Reset()
        {
            floppyStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
