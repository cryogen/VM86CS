using System.Reflection;
using System;
using System.Runtime.InteropServices;
namespace x86Disasm
{
    internal struct Instruction
    {
        public ushort OpCode;
        public InstructionType Type;
        public uint Value;
        public int NumberOfArguments;
        public Argument Arg1;
        public Argument Arg2;
        public Argument Arg3;
        public Argument Arg4;
        public string Nmumonic;
    }

    internal struct Argument
    {
        public ArgumentType Type;
        public uint Size;
        public int Value;
        public bool High;
        public bool SignExtend;
    }

    public struct Operation
    {
        public ushort OpCode;
        public Delegate Method;
        public int NumberOfArgs;
    }

    public struct Operand
    {
        public OperandType Type;
        private uint value;
        public uint Size;
        public RegisterOperand Register;
        public MemoryOperand Memory;
        public uint Address;

        public byte MSB
        {
            get
            {
                switch (Size)
                {
                    case 8:
                        return (byte)(value & 0x80);
                    case 16:
                        return (byte)(value & 0x8000);
                    default:
                        return (byte)(value & 0x80000000);
                }
            }
        }

        public int SignedValue
        {
            get
            {
                switch (Size)
                {
                    case 8:
                        return (sbyte)value;
                    case 16:
                        return (short)value;
                    default:
                        return (int)value;
                }
            }
        }

        public uint Value
        {
            get
            {
                switch (Size)
                {
                    case 8:
                        return (byte)value;
                    case 16:
                        return (ushort)value;
                    default:
                        return value;
                }
            }
            set
            {
                switch (Size)
                {
                    case 8:
                        this.value = value;
                        break;
                    case 16:
                        this.value = value;
                        break;
                    default:
                        this.value = value;
                        break;
                }
            }
        }

        public override string ToString()
        {
            switch (Size)
            {
                case 8:
                    return ((byte)Value).ToString("X");
                case 16:
                    return ((ushort)Value).ToString("X");
                default:
                    return Value.ToString("X");
            }
        }
    }

    public struct RegisterOperand
    {
        public int Index;
        public RegisterType Type;
        public bool High;
    }

    public struct MemoryOperand
    {
        public GeneralRegister Base;
        public GeneralRegister Index;
        public int Displacement;
        public int Scale;
        public SegmentRegister Segment;
        public uint Address;
    }
}
