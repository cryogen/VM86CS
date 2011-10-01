using System;
using System.Runtime.InteropServices;

namespace x86CS
{
    public delegate ushort ReadCallback(ushort addr);
    public delegate void WriteCallback(ushort addr, ushort value);

    public enum SegmentRegister
    {
        ES = 0,
        CS,
        SS,
        DS,
        FS,
        GS
    }
    public enum CPURegister
    {
        EAX,
        ECX,
        EDX,
        EBX,
        ESP,
        EBP,
        ESI,
        EDI,
        EIP,
    }
    [Flags]
    public enum CPUFlags : uint
    {
        CF = 0x0001,
        Spare = 0x0002,
        PF = 0x0004,
        Spare2 = 0x0008,
        AF = 0x0010,
        Spare3 = 0x0020,
        ZF = 0x0040,
        SF = 0x0080,
        TF = 0x0100,
        IF = 0x0200,
        DF = 0x0400,
        OF = 0x0800,
        IOPL = 0x1000,
        NT = 0x4000,
        Spare4 = 0x9000,
        RF = 0x10000,
        VM = 0x20000,
        AC = 0x40000,
        VIF = 0x00080000,
        VIP = 0x00100000,
        ID = 0x00200000,
        Spare5 = 0x00400000,
        Spare6 = 0x00800000,
        Spare7 = 0x01000000,
        Spare8 = 0x02000000,
        Spare9 = 0x04000000,
        Spare10 = 0x08000000,
        Spare11 = 0x10000000,
        Spare12 = 0x20000000,
        Spare13 = 0x40000000,
        Spare14 = 0x80000000,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GDTEntry
    {
        private ushort limitLow;
        private ushort baseLow1;
        private byte baseLow2;
        private sbyte flags;
        private sbyte limitHighAndFlags2;
        private byte baseHigh;

        public uint Limit
        {
            get { return (uint)(((limitHighAndFlags2 & 0x0f) << 16) + limitLow); }
            set
            {
                limitLow = (ushort)value;
                limitHighAndFlags2 = (sbyte)(byte)((value & 0x0fffffff) >> 16);
            }
        }

        public uint BaseAddress
        {
            get { return (uint)((baseHigh << 24) + (baseLow2 << 16) + baseLow1); }
            set 
            {
                baseLow1 = (ushort)value;
                baseLow2 = (byte)(value >> 16);
                baseHigh = (byte)(value >> 24);
            }
        }

        public byte PrivLevel
        {
            get { return (byte)(flags & 0x60); }
        }

        public bool IsAccessed
        {
            get { return ((flags & 0x1) == 0x1); }
            set
            {
                if (value)
                    flags |= 0x1;
                else
                    flags &= ~0x1;
            }
        }

        public bool IsWritable
        {
            get { return !IsCode && ((flags & 0x2) == 0x2); }
            set
            {
                if (value)
                    flags |= 0x2;
                else
                    flags &= ~0x2;
            }
        }

        public bool ReadExecute
        {
            get { return IsCode && ((flags & 0x2) == 0x2); }
        }

        public bool ExpansionConforming
        {
            get { return ((flags & 0x4) == 0x4); }
        }

        public bool IsCode
        {
            get { return ((flags & 0x8) == 0x8); }
            set
            {
                if (value)
                    flags |= 0x8;
                else
                    flags &= ~0x8;
            }
        }

        public bool IsSystemDescriptor
        {
            get { return ((flags & 0x10) == 0); }
        }

        public bool IsPresent
        {
            get { return ((flags & 0x80) == 0x80); }
        }

        public bool Is32Bit
        {
            get { return ((limitHighAndFlags2 & 0x40) == 0x40); }
            set
            {
                if (value)
                    limitHighAndFlags2 |= 0x40;
                else
                    limitHighAndFlags2 &= ~0x40;
            }
        }

        public bool Granularity
        {
            get { return ((limitHighAndFlags2 & 0x80) == 0x80); }
        }
    }

    public struct Segment
    {
        public GDTEntry GDTEntry;
        public uint Selector;

    }

    public struct TableRegister
    {
        public ushort Limit;
        public uint Base;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Register
    {
        [FieldOffset(0)]
        public uint DWord;
        [FieldOffset(0)]
        public ushort Word;
        [FieldOffset(0)]
        private byte Byte;

        public byte HighByte
        {
            get { return (byte)Word.GetHigh(); }
            set { Word = Word.SetHigh(value); }
        }

        public byte LowByte
        {
            get { return (byte)Word.GetLow(); }
            set { Byte = value; }
        }
    }

    public class CharEventArgs : EventArgs
    {
        public CharEventArgs(char charToWrite)
        {
            Char = charToWrite;
        }

        public char Char { get; set; }
    }

    public class IntEventArgs : EventArgs
    {
        public IntEventArgs(int num)
        {
            Number = num;
        }

        public int Number { get; set; }
    }

    public class ByteArrayEventArgs : EventArgs
    {
        public ByteArrayEventArgs(byte[] args)
        {
            ByteArray = args;
        }

        public byte[] ByteArray { get; set; }
    }
    
    public class TextEventArgs : EventArgs
    {
        public TextEventArgs(string textToWrite)
        {
            Text = textToWrite;
        }

        public string Text { get; set; }
    }
}