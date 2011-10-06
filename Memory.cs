using System;
using System.Runtime.InteropServices;
using System.IO;
using log4net;

namespace x86CS
{
    public class Memory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Memory));

        private static IntPtr realMemBase;
        private const bool LogMemory = false;
        private const uint MemorySize = 0xFFFFF;

        public static bool A20 { get; set; }

        static Memory()
        {
            realMemBase = Marshal.AllocHGlobal((int)MemorySize);
            Logger.Debug("Real memory base is " + realMemBase.ToString("X"));
        }

        private static IntPtr GetRealAddress(uint virtualAddr)
        {
            var actualAddr = (int)virtualAddr;

            /*if (!a20)
                actualAddr &= ~0x80000;*/

            return new IntPtr(realMemBase.ToInt32() + actualAddr);
        }

        public static void SegBlockWrite(ushort segment, ushort offset, byte[] buffer, int length)
        {
            var virtualPtr = (uint)((segment << 4) + offset);

            Memory.BlockWrite(virtualPtr, buffer, length);
        }

        public static void BlockWrite(uint addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Logger.Debug(String.Format("Block write {0:X} length {1:X} ends {2:X}", addr, length, addr + length));

            Marshal.Copy(buffer, 0, realOffset, length);
        }

        public static int BlockRead(uint addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.Copy(realOffset, buffer, 0, length);

            Logger.Debug(String.Format("Block read {0:X} length {1:X} ends {2:X}", addr, length, addr + length));

            return buffer.Length;
        }

        public static byte ReadByte(uint addr)
        {
            return Memory.ReadByte(addr, LogMemory);
        }

        public static byte ReadByte(uint addr, bool log)
        {
            IntPtr realOffset = GetRealAddress(addr);

            byte ret = Marshal.ReadByte(realOffset);

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
            IntPtr realOffset = GetRealAddress(addr);

            var ret = (ushort)Marshal.ReadInt16(realOffset);

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
            IntPtr realOffset = GetRealAddress(addr);

            var ret = (uint)Marshal.ReadInt32(realOffset);

            if(log && LogMemory)
                Logger.Debug(String.Format("Read DWord {0:X} {1:X}", addr, ret));

            return ret;
        }

        public static void WriteByte(uint addr, byte value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            if(LogMemory)
                Logger.Debug(String.Format("Write byte {0:X} {1:X}", addr, value));


            Marshal.WriteByte(realOffset, value);
        }

        public static void WriteWord(uint addr, ushort value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            if(LogMemory)
                Logger.Debug(String.Format("Write Word {0:X} {1:X}", addr, value));

            Marshal.WriteInt16(realOffset, (short)value);
        }

        public static void WriteDWord(uint addr, uint value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            if(LogMemory)
                Logger.Debug(String.Format("Write DWord {0:X} {1:X}", addr, value));

            Marshal.WriteInt32(realOffset, (int)value);
        }
    }
}
