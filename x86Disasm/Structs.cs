using System.Reflection;
using System;
using System.Runtime.InteropServices;
using System.Text;
namespace x86Disasm
{
    internal struct Instruction
    {
        public uint OpCode;
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
        public bool UsesES;
    }

    public struct Operation
    {
        public uint OpCode;
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
            set
            {
                switch (Size)
                {
                    case 8:
                        this.value = (byte)value;
                        break;
                    case 16:
                        this.value = (ushort)value;
                        break;
                    default:
                        this.value = (uint)value;
                        break;
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
        public int Size;
        public RegisterType Type;
        public bool High;

        public override string ToString()
        {
            switch (Type)
            {
                case RegisterType.GeneralRegister:
                    switch (Size)
                    {
                        case 8:
                            if (High)
                                return Disassembler.registerStrings8BitHigh[Index];
                            else
                                return Disassembler.registerStrings8BitLow[Index];
                        case 16:
                            return Disassembler.registerStrings16Bit[Index];
                        default:
                            return Disassembler.registerStrings32Bit[Index];
                    }
                case RegisterType.SegmentRegister:
                    return Disassembler.registerStringsSegment[Index];
                case RegisterType.ControlRegister:
                    return Disassembler.registerStringsControl[Index];
            }
            return "";
        }
    }

    public struct MemoryOperand
    {
        public GeneralRegister Base;
        public GeneralRegister Index;
        public int Displacement;
        public int Scale;
        public SegmentRegister Segment;
        public uint Address;
        public int Size;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            if (Segment == SegmentRegister.Default)
                builder.Append(Disassembler.registerStringsSegment[(int)SegmentRegister.DS]);
            else
                builder.Append(Disassembler.registerStringsSegment[(int)Segment]);

            builder.Append(":[");

            if (Base != GeneralRegister.None)
            {
                if (Size == 16)
                    builder.Append(Disassembler.registerStrings16Bit[(int)Base]);
                else
                    builder.Append(Disassembler.registerStrings32Bit[(int)Base]);

                if (Scale != 0)
                    builder.Append("*" + (1<<Scale));
            }

            if(Index != GeneralRegister.None)
            {
                if(Base != GeneralRegister.None)
                    builder.Append(" + ");
                if(Size == 16)
                    builder.Append(Disassembler.registerStrings16Bit[(int)Index]);
                else
                    builder.Append(Disassembler.registerStrings32Bit[(int)Index]);
            }

            if (Displacement != 0)
            {
                if (Displacement < 0)
                {
                    builder.Append("-");
                    builder.Append((-Displacement).ToString("X"));
                }
                else
                {
                    if (Base != GeneralRegister.None || Index != GeneralRegister.None)
                        builder.Append("+");
                    builder.Append(Displacement.ToString("X"));
                }
            }

            builder.Append("]");

            return builder.ToString();
        }
    }
}
