using System.Reflection;
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
        public int Size;
        public int Value;
        public bool High;
    }

    public struct Operation
    {
        public ushort OpCode;
        public MethodInfo Method;
        public int NumberOfArgs;
    }

    public struct Operand
    {
        public OperandType Type;
        public uint Value;
        public uint Size;
        public RegisterOperand Register;
        public MemoryOperand Memory;
    }

    public struct RegisterOperand
    {
        public uint RegisterIndex;
        public bool High;
    }

    public struct MemoryOperand
    {
        public uint Base;
        public int Displacement;
        public uint Index;
        public uint Scale;
    }
}
