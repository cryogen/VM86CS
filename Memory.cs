using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace x86CS
{
    public class Memory
    {
        private static IntPtr realMemBase;
        private static uint memorySize = 0xFFFFF; 

        static Memory()
        {
            realMemBase = Marshal.AllocHGlobal((int)memorySize);
        }

        private static IntPtr GetRealAddress(int virtualAddr)
        {
            ushort virtualSeg = (ushort)(virtualAddr >> 16);
            ushort virtualOff = (ushort)(virtualAddr & 0xffff);
            ushort virtualPtr = (ushort)(virtualSeg + virtualOff);

            return new IntPtr(realMemBase.ToInt32() + virtualPtr);
        }

        public static void SegBlockWrite(ushort segment, ushort offset, byte[] buffer, int length)
        {
            ushort virtualPtr = (ushort)((segment << 4) + offset);

            Marshal.Copy(buffer, 0, new IntPtr(realMemBase.ToInt32() + virtualPtr), length);
        }

        public static void BlockWrite(int addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.Copy(buffer, 0, realOffset, length);
        }

        public static int BlockRead(int addr, byte[] buffer, int length)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.Copy(realOffset, buffer, 0, length);

            return buffer.Length;
        }

        public static byte ReadByte(int addr)
        {
            IntPtr realOffset = GetRealAddress(addr);

            return Marshal.ReadByte(realOffset);
        }

        public static ushort ReadWord(int addr)
        {
            IntPtr realOffset = GetRealAddress(addr);

            return (ushort)Marshal.ReadInt16(realOffset);
        }

        public static void WriteByte(int addr, byte value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.WriteByte(realOffset, value);
        }

        public static void WriteWord(int addr, ushort value)
        {
            IntPtr realOffset = GetRealAddress(addr);

            Marshal.WriteInt16(realOffset, (short)value);
        }
    }
}
