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
        public string Nmumonic;
    }

    internal struct Argument
    {
        public ArgumentType Type;
        public uint Size;
        public int Value;
        public bool High;
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
    }

    public struct RegisterOperand
    {
        public GeneralRegister Register;
        public bool High;
    }

    public struct MemoryOperand
    {
        public GeneralRegister Base;
        public GeneralRegister Index;
        public int Displacement;
        public int Scale;
        public SegmentRegister Segment;
    }
}
