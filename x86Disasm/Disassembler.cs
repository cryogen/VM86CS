using System.Collections.Generic;
using System;
using System.Reflection;
namespace x86Disasm
{
    public partial class Disassembler
    {
        private ReadCallback readFunction;
        private OPPrefix setPrefixes;
        private Operand[] operands;
        private Dictionary<ushort, Operation> operations;
        private Instruction currentInstruction;

        public Operand[] Operands
        {
            get { return operands; }
        }

        public int NumberOfOperands { get { return currentInstruction.NumberOfArguments; } }

        public Disassembler(ReadCallback readCallback)
        {
            readFunction = readCallback;
            operands = new Operand[3];
            operations = new Dictionary<ushort, Operation>();
        }

        private byte ReadByte(uint offset)
        {
            return (byte)readFunction(offset, 8);
        }

        private ushort ReadWord(uint offset)
        {
            return (ushort)readFunction(offset, 16);
        }

        private uint ReadDWord(uint offset)
        {
            return readFunction(offset, 32);
        }

        private void ProcessRegMem(Operand operand)
        {

        }

        private uint ProcessArgument(Argument argument, int operandNumber, uint offset)
        {
            Operand operand = new Operand();

            switch (argument.Type)
            {
                case ArgumentType.Address:
                    operand.Type = OperandType.Immediate;
                    operand.Size = 32;
                    operand.Value = (uint)(ReadWord(offset) + (ReadWord(offset+2) << 4));
                    offset += 4;
                    break;
                case ArgumentType.RegMem:
                case ArgumentType.RegMemGeneral:
                case ArgumentType.RegMemMemory:
                case ArgumentType.RegMemSegment:
                    ProcessRegMem(operand);
                    break;
                default:
                    break;
            }

            operands[operandNumber] = operand;

            return offset;
        }

        public void AddOperation(ushort opCode, MethodInfo method, int numArgs)
        {
            Operation operation = new Operation { OpCode = opCode, Method=method, NumberOfArgs=numArgs };

            operations.Add(opCode, operation);
        }

        public int Disassemble(uint virtualAddr)
        {
            uint offset = 0;
            byte opCode;

            opCode = (byte)readFunction(offset, 8);
            currentInstruction = instructions[opCode];
            offset++;

            System.Diagnostics.Debug.Assert(opCode == currentInstruction.OpCode);

            if (currentInstruction.Type == InstructionType.Invalid)
                return -1;

            while (currentInstruction.Type == InstructionType.Prefix)
            {
                setPrefixes |= (OPPrefix)currentInstruction.Value;
                opCode = ReadByte(offset);
                currentInstruction = instructions[opCode];
                offset++;
            }

            if (currentInstruction.Arg1.Type != ArgumentType.None)
                offset = ProcessArgument(currentInstruction.Arg1, 0, offset);
            if (currentInstruction.Arg2.Type != ArgumentType.None)
                offset = ProcessArgument(currentInstruction.Arg2, 1, offset);
            if (currentInstruction.Arg3.Type != ArgumentType.None)
                offset = ProcessArgument(currentInstruction.Arg3, 2, offset);            
                
            return (int)offset;
        }

        public void Execute(object invoker, params uint[] operands)
        {
            Operation operation;
            object[] parameters;

            if(!operations.TryGetValue(currentInstruction.OpCode, out operation))
                throw new Exception("Invalid operation");

            if (operands.Length != operation.NumberOfArgs)
                throw new Exception("Not enough arguments");

            if (operation.NumberOfArgs == 0)
            {
                operation.Method.Invoke(invoker, null);
                return;
            }

            parameters = new object[operation.NumberOfArgs];

            for (int i = 0; i < operation.NumberOfArgs; i++)
            {
                parameters[i] = operands[i];
            }

            operation.Method.Invoke(invoker, parameters);
        }
    }
}
