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
        private bool gotRM;
        private byte rmByte;

        public string InstructionText { get; private set; }

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

        private void ProcessRegMemRegister(ref Operand operand, Argument argument, byte rmByte)
        {
            byte index = (byte)(rmByte & 0x7);

            operand.Type = OperandType.Register;
            operand.Size = argument.Size;
            if (operand.Size == 8)
            {
                operand.Register = registers8Bit[index];
                InstructionText += registerStrings8BitLow[index];
            }
            else
            {
                operand.Register = registers16Bit[index];
                if (operand.Size == 16)
                    InstructionText += registerStrings16Bit[(int)operand.Register.Register];
                else
                    InstructionText += registerStrings32Bit[(int)operand.Register.Register];
            }
        }

        private void ProcessRegMemMemory(ref Operand operand, Argument argument, byte rmByte)
        {
            operand.Type = OperandType.Memory;
            operand.Size = argument.Size;
            operand.Memory = regMemMemory16[rmByte & 0x7];
            if (rmByte < 0x40)
                operand.Memory.Base = 0;
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
                    InstructionText += operand.Value.ToString("X");
                    offset += 4;
                    break;
                case ArgumentType.Immediate:
                    operand.Type = OperandType.Immediate;
                    operand.Size = argument.Size;
                    operand.Value = readFunction(offset, (int)operand.Size);
                    InstructionText += operand.Value.ToString("X");
                    offset += operand.Size / 8;
                    break;
                case ArgumentType.RegMem:
                case ArgumentType.RegMemGeneral:
                case ArgumentType.RegMemMemory:
                case ArgumentType.RegMemSegment:
                    if (!gotRM)
                    {
                        rmByte = ReadByte(offset++);
                        gotRM = true;
                    }
                    switch (argument.Type)
                    {
                        case ArgumentType.RegMem:
                            if (rmByte >= 0xc0)
                                ProcessRegMemRegister(ref operand, argument, rmByte);
                            else
                                ProcessRegMemMemory(ref operand, argument, rmByte);
                            break;
                        case ArgumentType.RegMemGeneral:
                            ProcessRegMemRegister(ref operand, argument, (byte)(rmByte >> 3));
                            break;
                        case ArgumentType.RegMemMemory:
                            ProcessRegMemRegister(ref operand, argument, rmByte);
                            break;
                        default:
                            break;
                    }
                    break;
                case ArgumentType.GeneralRegister:
                    operand.Type = OperandType.Register;
                    operand.Size = argument.Size;
                    if (operand.Size == 8)
                    {
                        operand.Register = registers8Bit[argument.Value];
                        if (operand.Register.High)
                            InstructionText += registerStrings8BitHigh[(int)operand.Register.Register];
                        else
                            InstructionText += registerStrings8BitLow[(int)operand.Register.Register];
                    }
                    else
                    {
                        operand.Register = registers16Bit[argument.Value];
                        if (operand.Size == 16)
                            InstructionText += registerStrings16Bit[(int)operand.Register.Register];
                        else
                            InstructionText += registerStrings32Bit[(int)operand.Register.Register];
                    }
                    break;
                default:
                    break;
            }

            operands[operandNumber] = operand;

            return offset;
        }

        public void AddOperation(ushort opCode, Delegate method, int numArgs)
        {
            Operation operation = new Operation { OpCode = opCode, Method=method, NumberOfArgs=numArgs };

            operations.Add(opCode, operation);
        }

        public int Disassemble(uint virtualAddr)
        {
            uint offset = 0;
            byte opCode;

            InstructionText = "";
            gotRM = false;

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

            if ((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat)
                InstructionText = "REP ";
            else if ((setPrefixes & OPPrefix.RepeatNotEqual) == OPPrefix.RepeatNotEqual)
                InstructionText = "REPNE ";

            InstructionText += currentInstruction.Nmumonic + " ";

            if (currentInstruction.Arg1.Type != ArgumentType.None)
                offset = ProcessArgument(currentInstruction.Arg1, 0, offset);
            if (currentInstruction.Arg2.Type != ArgumentType.None)
            {
                InstructionText += ", ";
                offset = ProcessArgument(currentInstruction.Arg2, 1, offset);
            }
            if (currentInstruction.Arg3.Type != ArgumentType.None)
            {
                InstructionText += ", ";
                offset = ProcessArgument(currentInstruction.Arg3, 2, offset);
            }
                
            return (int)offset;
        }

        public void Execute(object invoker, params Operand[] operands)
        {
            Operation operation;

            if(!operations.TryGetValue(currentInstruction.OpCode, out operation))
                throw new Exception("Invalid operation");

            if (operands.Length != operation.NumberOfArgs)
                throw new Exception("wrong number of arguments");

            switch (operation.NumberOfArgs)
            {
                case 0:
                    CPUCallbackNoargs func = operation.Method as CPUCallbackNoargs;
                    func();
                    break;
                case 1:
                    CPUCallback1args func1 = operation.Method as CPUCallback1args;
                    func1(operands[0]);
                    break;
                case 2:
                    CPUCallback2args func2 = operation.Method as CPUCallback2args;
                    func2(operands[0], operands[1]);
                    break;
                case 3:
                    CPUCallback3args func3 = operation.Method as CPUCallback3args;
                    func3(operands[0], operands[1], operands[2]);
                    break;
            }
        }
    }
}
