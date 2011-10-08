using System;
using System.Runtime.InteropServices;
using System.IO;
using log4net;

namespace x86CS
{
    public class Memory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Memory));

        private const bool LogMemory = false;
        private const uint MemorySize = 0xFFFFF;
        private static readonly byte[] memory;

        public static bool A20 { get; set; }

        static Memory()
        {
            memory = new byte[MemorySize * 32];
        }

        public static void SegBlockWrite(ushort segment, ushort offset, byte[] buffer, int length)
        {
            var virtualPtr = (uint)((segment << 4) + offset);

            Memory.BlockWrite(virtualPtr, buffer, length);
        }

        public static void BlockWrite(uint addr, byte[] buffer, int length)
        {
            Logger.Debug(String.Format("Block write {0:X} length {1:X} ends {2:X}", addr, length, addr + length));

            buffer.CopyTo(memory, addr);
        }

        public static int BlockRead(uint addr, byte[] buffer, int length)
        {
            Array.Copy(memory, addr, buffer, 0, length);

            Logger.Debug(String.Format("Block read {0:X} length {1:X} ends {2:X}", addr, length, addr + length));

            return buffer.Length;
        }

        public static byte ReadByte(uint addr)
        {
            return Memory.ReadByte(addr, LogMemory);
        }

        public static byte ReadByte(uint addr, bool log)
        {
            byte ret = memory[addr];

            if(log && LogMemory)
                Logger.Debug(String.Format("Read Byte {0:X} {1:X}", addr, ret));
            
            return ret;
        }

        public static ushort ReadWord(uint addr)
        {
            return Memory.ReadWord(addr, LogMemory);
        }

        public static ushort ReadWord(uint addr, bool log)
        {
            ushort ret = BitConverter.ToUInt16(memory, (int)addr);

            if(log && LogMemory)
                Logger.Debug(String.Format("Read Word {0:X} {1:X}", addr, ret));
        
            return ret;
        }

        public static uint ReadDWord(uint addr)
        {
            return Memory.ReadDWord(addr, LogMemory);
        }

        public static uint ReadDWord(uint addr, bool log)
        {
            uint ret = BitConverter.ToUInt32(memory, (int)addr);

            if(log && LogMemory)
                Logger.Debug(String.Format("Read DWord {0:X} {1:X}", addr, ret));

            return ret;
        }

        public static void WriteByte(uint addr, byte value)
        {
            if(LogMemory)
                Logger.Debug(String.Format("Write byte {0:X} {1:X}", addr, value));

            memory[addr] = value;
        }

        public static void WriteWord(uint addr, ushort value)
        {
            if(LogMemory)
                Logger.Debug(String.Format("Write Word {0:X} {1:X}", addr, value));

            Array.Copy(BitConverter.GetBytes(value), 0, memory, addr, 2);
        }

        public static void WriteDWord(uint addr, uint value)
        {
            if(LogMemory)
                Logger.Debug(String.Format("Write DWord {0:X} {1:X}", addr, value));

            Array.Copy(BitConverter.GetBytes(value), 0, memory, addr, 4);
        }
    }
}
