using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace x86CS
{
    public class Memory
    {
        private static IntPtr realMemBase;
        private static uint memorySize = 0xFFFFF;
        private static StreamWriter logFile = File.CreateText("memlog.txt");

        static Memory()
        {
            realMemBase = Marshal.AllocHGlobal((int)memorySize);
            logFile.AutoFlush = true;
        }

        private static IntPtr GetRealAddress(uint virtualAddr)
        {
            return new IntPtr(realMemBase.ToInt32() + virtualAddr);
        }

        public static void SegBlockWrite(ushort segment, ushort offset, byte[] buffer, int length)
        {
            uint virtualPtr = (uint)((segment << 4) + offset);

            Marshal.Copy(buffer, 0, new IntPtr(realMemBase.ToInt32() + virtualPtr), length);

            logFile.WriteLine(String.Format("Seg Write Block: {0:X4}:{1:X4} ({2:X4}) {3}", segment, offset, virtualPtr, length));
        }

        public static void BlockWrite(uint addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.Copy(buffer, 0, realOffset, length);

            logFile.WriteLine(String.Format("Mem Write Block: {0:X4} {1}", addr, length));
        }

        public static int BlockRead(uint addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.Copy(realOffset, buffer, 0, length);

            logFile.WriteLine(String.Format("Mem Read Block: {0:X4} {1}", addr, length));

            return buffer.Length;
        }

        public static byte ReadByte(uint addr)
        {
            IntPtr realOffset = GetRealAddress(addr);
            byte ret;

            ret = Marshal.ReadByte(realOffset);
            logFile.WriteLine(String.Format("Mem Read Byte: {0:X4} {1:X2}", addr, ret));

            return ret;
        }

        public static ushort ReadWord(uint addr)
        {
            IntPtr realOffset = GetRealAddress(addr);
            ushort ret;

            ret = (ushort)Marshal.ReadInt16(realOffset);
            logFile.WriteLine(String.Format("Mem Read Word: {0:X4} {1:X4}", addr, ret));
        
            return ret;
        }

        public static uint ReadDWord(uint addr)
        {
            IntPtr realOffset = GetRealAddress(addr);
            uint ret;

            ret = (uint)Marshal.ReadInt32(realOffset);
            logFile.WriteLine(String.Format("Mem Read DWord: {0:X4} {1:X8}", addr, ret));

            return ret;
        }

        public static void WriteByte(uint addr, byte value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            logFile.WriteLine(String.Format("Mem Write Byte: {0:X4} {1:X2}", addr, value));

            Marshal.WriteByte(realOffset, value);
        }

        public static void WriteWord(uint addr, ushort value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            logFile.WriteLine(String.Format("Mem Write Word: {0:X4} {1:X4}", addr, value));

            Marshal.WriteInt16(realOffset, (short)value);
        }
    }
}
