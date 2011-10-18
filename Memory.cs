using System;
using System.Runtime.InteropServices;
using System.IO;
using log4net;

namespace x86CS
{
    public class Memory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Memory));

        private const uint MemorySize = 0xFFFFF;
        private static readonly byte[] memory;
        public static bool LoggingEnabled = Logger.IsDebugEnabled;

        public static bool A20 { get; set; }
        public static byte[] MemoryArray { get { return memory; } }

        static Memory()
        {
            memory = new byte[MemorySize * 48];
        }

        public static void SegBlockWrite(ushort segment, ushort offset, byte[] buffer, int length)
        {
            var virtualPtr = (uint)((segment << 4) + offset);

            Memory.BlockWrite(virtualPtr, buffer, length);
        }

        public static void BlockWrite(uint addr, byte[] buffer, int length)
        {
            if(LoggingEnabled)
                Logger.Debug(String.Format("Block write {0:X} length {1:X} ends {2:X}", addr, length, addr + length));

            Buffer.BlockCopy(buffer, 0, memory, (int)addr, length);
        }

        public static int BlockRead(uint addr, byte[] buffer, int length)
        {
            Buffer.BlockCopy(memory, (int)addr, buffer, 0, length);

            if(LoggingEnabled)
                Logger.Debug(String.Format("Block read {0:X} length {1:X} ends {2:X}", addr, length, addr + length));

            return buffer.Length;
        }

        public static uint Read(uint addr, int size)
        {
            uint ret;
            bool passedMem = false;

            if (addr > (48 * MemorySize))
                passedMem = true;

            switch (size)
            {
                case 8:
                    if (passedMem)
                        ret = 0xff;
                    else
                        ret = memory[addr];
                    break;
                case 16:
                    if (passedMem)
                        ret = 0xffff;
                    else
                        ret = (ushort)(memory[addr] | memory[addr + 1] << 8);
                    break;
                default:
                    if (passedMem)
                        ret = 0xffffffff;
                    else
                        ret = (uint)(memory[addr] | memory[addr + 1] << 8 | memory[addr + 2] << 16 | memory[addr + 3] << 24);
                    break;
            }

            if(LoggingEnabled)
                Logger.Debug(String.Format("Read {0} address {1:X} value {2:X}{3}", size, addr, ret, passedMem ? " (OverRead)" : ""));

            return ret;
        }

        public static void Write(uint addr, uint value, int size)
        {
            if (addr > (48 * MemorySize))
            {
                if(LoggingEnabled)
                    Logger.Debug(String.Format("Write {0} address {1:X} value {2:X} (OverWrite, ignored)", size, addr, value));
                return;
            }

            if (LoggingEnabled)
                Logger.Debug(String.Format("Write {0} address {1:X} value {2:X}", size, addr, value));

            switch (size)
            {
                case 8:
                    memory[addr] = (byte)value;
                    break;
                case 16:
                    memory[addr] = (byte)value;
                    memory[addr + 1] = (byte)((ushort)value).GetHigh();
                    break;
                default:
                    memory[addr] = (byte)value;
                    memory[addr + 1] = (byte)(value >> 8);
                    memory[addr + 2] = (byte)(value >> 16);
                    memory[addr + 3] = (byte)(value >> 24);
                    break;
            }
        }
    }
}
