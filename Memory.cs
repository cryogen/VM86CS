using System;
using System.Runtime.InteropServices;
using System.IO;

namespace x86CS
{
    public class Memory
    {
        private static IntPtr realMemBase;
        private const uint MemorySize = 0xFFFFF;
        private static readonly StreamWriter LogFile = File.CreateText("memlog.txt");

        public static bool A20 { get; set; }

        static Memory()
        {
            realMemBase = Marshal.AllocHGlobal((int)MemorySize);
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
            IntPtr realOffset = GetRealAddress(virtualPtr);

            Marshal.Copy(buffer, 0, realOffset, length);

            LogFile.WriteLine(String.Format("Seg Write Block: {0:X}:{1:X} ({2:X}) {3}", segment, offset, virtualPtr, length));
        }

        public static void BlockWrite(uint addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.Copy(buffer, 0, realOffset, length);

            LogFile.WriteLine(String.Format("Mem Write Block: {0:X} {1}", addr, length));
        }

        public static int BlockRead(uint addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.Copy(realOffset, buffer, 0, length);

            LogFile.WriteLine(String.Format("Mem Read Block: {0:X} {1}", addr, length));

            return buffer.Length;
        }

        public static byte ReadByte(uint addr)
        {
            IntPtr realOffset = GetRealAddress(addr);

            byte ret = Marshal.ReadByte(realOffset);
            LogFile.WriteLine(String.Format("Mem Read Byte: {0:X} {1:X}", addr, ret));

            return ret;
        }

        public static ushort ReadWord(uint addr)
        {
            IntPtr realOffset = GetRealAddress(addr);

            var ret = (ushort)Marshal.ReadInt16(realOffset);
            LogFile.WriteLine(String.Format("Mem Read Word: {0:X} {1:X}", addr, ret));
        
            return ret;
        }

        public static uint ReadDWord(uint addr)
        {
            IntPtr realOffset = GetRealAddress(addr);

            var ret = (uint)Marshal.ReadInt32(realOffset);
            LogFile.WriteLine(String.Format("Mem Read DWord: {0:X} {1:X}", addr, ret));

            return ret;
        }

        public static void WriteByte(uint addr, byte value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            LogFile.WriteLine(String.Format("Mem Write Byte: {0:X} {1:X}", addr, value));

            Marshal.WriteByte(realOffset, value);
        }

        public static void WriteWord(uint addr, ushort value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            LogFile.WriteLine(String.Format("Mem Write Word: {0:X} {1:X}", addr, value));

            Marshal.WriteInt16(realOffset, (short)value);
        }

        public static void WriteDWord(uint addr, uint value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            LogFile.WriteLine(String.Format("Mem Write Word: {0:X} {1:X}", addr, value));

            Marshal.WriteInt32(realOffset, (int)value);
        }
    }
}
