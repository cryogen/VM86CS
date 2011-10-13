using System.Collections.Generic;
using System;
using System.Reflection;
namespace x86Disasm
{
    public partial class Disassembler
    {
        private ReadCallback readFunction;
        private Operand[] operands;
        private Dictionary<ushort, Operation> operations;
        private Instruction currentInstruction;
        private bool gotRM;
        private byte rmByte;
        private uint virtualAddr;
        private int operandSize;
        private int addressSize;

        public OPPrefix SetPrefixes { get; private set; }
        public string InstructionText { get; private set; }
        public int CodeSize { get; set; }
        public SegmentRegister OverrideSegment { get; private set; }

        public Operand[] Operands
        {
            get { return operands; }
        }

        public int NumberOfOperands { get { return currentInstruction.NumberOfArguments; } }

        public Disassembler(ReadCallback readCallback)
        {
            readFunction = readCallback;
            operands = new Operand[4];
            operations = new Dictionary<ushort, Operation>();
            OverrideSegment = SegmentRegister.Default;
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
            if (operand.Size == 8)
            {
                operand.Register = registers8Bit[index];
                InstructionText += registerStrings8BitLow[index];
            }
            else
            {
                operand.Register = registers16Bit[index];
                if (operand.Size == 16)
                    InstructionText += registerStrings16Bit[(int)operand.Register.Index];
                else
                    InstructionText += registerStrings32Bit[(int)operand.Register.Index];
            }
        }

        private void ProcessRegMemSegment(ref Operand operand, Argument argument, byte rmByte)
        {
            byte index = (byte)(rmByte & 0x7);

            operand.Type = OperandType.Register;
            operand.Size = argument.Size;
            operand.Register = registersSegment[index];
            InstructionText += registerStringsSegment[index];
        }

        private uint ProcessRegMemMemory(ref Operand operand, Argument argument, byte rmByte, uint offset)
        {
            byte mod, rm;

            operand.Type = OperandType.Memory;
            operand.Size = argument.Size;
            operand.Memory = regMemMemory16[rmByte & 0x7];
            
            mod = (byte)((rmByte >> 6) & 0x3);
            rm = (byte)(rmByte & 0x7);

            if (mod == 0 && rm == 6)
            {
                operand.Memory.Base = 0;
                operand.Memory.Displacement = (short)ReadWord(offset);
                operand.Memory.Segment = SegmentRegister.DS;
                offset += 2;
            }
            else if (mod == 1)
                operand.Memory.Displacement = (sbyte)ReadByte(offset++);
            else if (mod == 2)
            {
                operand.Memory.Displacement = (short)ReadWord(offset);
                offset += 2;
            }

            if (OverrideSegment != SegmentRegister.Default)
                operand.Memory.Segment = OverrideSegment;

            InstructionText += registerStringsSegment[(int)operand.Memory.Segment] + ":";
            if (mod == 0 && rm == 6)
                InstructionText += operand.Memory.Displacement.ToString("X");
            else
            {
                int displacement;
                string sign;

                if (operand.Memory.Displacement < 0)
                {
                    displacement = -operand.Memory.Displacement;
                    sign = "-";
                }
                else
                {
                    displacement = operand.Memory.Displacement;
                    sign = "+";
                }

                InstructionText += String.Format("[{0}{1}{2}{3}{4}]", registerStrings16Bit[(int)operand.Memory.Base], operand.Memory.Index > 0 ? "+" : "",
                    operand.Memory.Index > 0 ? registerStrings16Bit[(int)operand.Memory.Index] : "", operand.Memory.Displacement != 0 ? sign : "",
                    operand.Memory.Displacement != 0 ? displacement.ToString("X") : "");
            }

            return offset;
        }

        private uint ProcessArgument(Argument argument, int operandNumber, uint offset)
        {
            Operand operand = new Operand();

            if (argument.Size == 16)
                operand.Size = (uint)operandSize;
            else
                operand.Size = argument.Size;

            switch (argument.Type)
            {
                case ArgumentType.Address:
                    operand.Type = OperandType.Immediate;
                    operand.Value = (uint)(ReadWord(offset) + (ReadWord(offset+2) << 4));
                    InstructionText += operand.Value.ToString("X");
                    offset += 4;
                    break;
                case ArgumentType.Immediate:
                    operand.Type = OperandType.Immediate;
                    if (argument.SignExtend)
                    {
                        switch (operand.Size)
                        {
                            case 8:
                                operand.Size = 16;
                                operand.Value = (ushort)(short)(sbyte)ReadByte(offset);
                                break;
                            case 16:
                                operand.Size = 32;
                                operand.Value = (uint)(int)(short)ReadWord(offset);
                                break;
                        }
                        offset += argument.Size / 8;
                    }
                    else
                    {
                        operand.Value = readFunction(offset, (int)operand.Size);
                        offset += operand.Size / 8;
                    }
                    InstructionText += operand;
                    break;
                case ArgumentType.Relative:
                    operand.Type = OperandType.Immediate;
                    operand.Value = readFunction(offset, (int)operand.Size);
                    offset += operand.Size / 8;
                    if(operand.SignedValue < 0)
                        InstructionText += "-" + (-operand.SignedValue).ToString("X") + " (" + ((virtualAddr & 0xffff0000) + (ushort)((ushort)virtualAddr + operand.Value + offset)).ToString("X") + ")";
                    else
                        InstructionText += operand.Value.ToString("X") + " (" + ((virtualAddr & 0xffff0000) + (ushort)((ushort)virtualAddr + operand.Value + offset)).ToString("X") + ")";
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
                                offset = ProcessRegMemMemory(ref operand, argument, rmByte, offset);
                            break;
                        case ArgumentType.RegMemGeneral:
                            ProcessRegMemRegister(ref operand, argument, (byte)(rmByte >> 3));
                            break;
                        case ArgumentType.RegMemMemory:
                            offset = ProcessRegMemMemory(ref operand, argument, rmByte, offset);
                            break;
                        case ArgumentType.RegMemSegment:
                            ProcessRegMemSegment(ref operand, argument, (byte)(rmByte >> 3));
                            break;
                        default:
                            System.Diagnostics.Debugger.Break();
                            break;
                    }
                    break;
                case ArgumentType.GeneralRegister:
                    operand.Type = OperandType.Register;
                    if (operand.Size == 8)
                    {
                        operand.Register = registers8Bit[argument.Value];
                        if (operand.Register.High)
                            InstructionText += registerStrings8BitHigh[(int)operand.Register.Index];
                        else
                            InstructionText += registerStrings8BitLow[(int)operand.Register.Index];
                    }
                    else
                    {
                        operand.Register = registers16Bit[argument.Value];
                        if (operand.Size == 16)
                            InstructionText += registerStrings16Bit[(int)operand.Register.Index];
                        else
                            InstructionText += registerStrings32Bit[(int)operand.Register.Index];
                    }
                    break;
                case ArgumentType.SegmentRegister:
                    operand.Type = OperandType.Register;
                    operand.Register = registersSegment[argument.Value];
                    InstructionText += registerStringsSegment[argument.Value];
                    break;
                case ArgumentType.Memory:
                    operand.Type = OperandType.Memory;
                    operand.Memory.Base = GeneralRegister.EDI;
                    if (OverrideSegment == SegmentRegister.Default)
                        operand.Memory.Segment = SegmentRegister.DS;
                    else
                    {
                        operand.Memory.Segment = OverrideSegment;
                        InstructionText += registersSegment[(int)operand.Memory.Segment] + ":";
                    }

                    if (operand.Size == 16)
                        InstructionText += "[" + registerStrings16Bit[(int)operand.Memory.Base] + "]";
                    else
                        InstructionText += "[" + registerStrings32Bit[(int)operand.Memory.Base] + "]";

                    break;
                case ArgumentType.Offset:
                    operand.Type = OperandType.Memory;
                    
                    operand.Memory.Displacement = (int)readFunction(offset, operandSize);
                    offset += 2;
                    if (OverrideSegment == SegmentRegister.Default)
                        operand.Memory.Segment = SegmentRegister.DS;
                    else
                        operand.Memory.Segment = OverrideSegment;

                    operand.Size = (uint)addressSize;

                    InstructionText += registersSegment[(int)operand.Memory.Segment] + ":" + operand.Memory.Displacement.ToString("X");
                    break;
                case ArgumentType.Constant:
                    operand.Type = OperandType.Immediate;
                    operand.Value = (uint)argument.Value;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
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

        public int Disassemble(uint addr)
        {
            uint offset = 0;
            ushort opCode;

            InstructionText = "";
            gotRM = false;
            virtualAddr = addr;
            SetPrefixes = 0;
            OverrideSegment = SegmentRegister.Default;
                
            opCode = (byte)readFunction(offset, 8);
            currentInstruction = instructions[opCode];
            offset++;

            System.Diagnostics.Debug.Assert(opCode == currentInstruction.OpCode);

            if (currentInstruction.Type == InstructionType.Invalid)
                throw new Exception("Invalid operation");

            while (currentInstruction.Type == InstructionType.Prefix)
            {
                SetPrefixes |= (OPPrefix)currentInstruction.Value;
                opCode = ReadByte(offset);
                currentInstruction = instructions[opCode];
                offset++;
            }

            if ((SetPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat)
                InstructionText = "REP ";
            else if ((SetPrefixes & OPPrefix.RepeatNotEqual) == OPPrefix.RepeatNotEqual)
                InstructionText = "REPNE ";
            
            if ((SetPrefixes & OPPrefix.CSOverride) == OPPrefix.CSOverride)
                OverrideSegment = SegmentRegister.CS;
            else if ((SetPrefixes & OPPrefix.DSOverride) == OPPrefix.DSOverride)
                OverrideSegment = SegmentRegister.DS;
            else if ((SetPrefixes & OPPrefix.ESOverride) == OPPrefix.ESOverride)
                OverrideSegment = SegmentRegister.ES;
            else if ((SetPrefixes & OPPrefix.FSOverride) == OPPrefix.FSOverride)
                OverrideSegment = SegmentRegister.FS;
            else if ((SetPrefixes & OPPrefix.GSOverride) == OPPrefix.GSOverride)
                OverrideSegment = SegmentRegister.GS;
            else if ((SetPrefixes & OPPrefix.SSOverride) == OPPrefix.SSOverride)
                OverrideSegment = SegmentRegister.SS;

            if ((SetPrefixes & OPPrefix.OperandSize) == OPPrefix.OperandSize)
                operandSize = CodeSize == 32 ? 16 : 32;
            else
                operandSize = CodeSize;

            if ((SetPrefixes & OPPrefix.AddressSize) == OPPrefix.AddressSize)
                addressSize = CodeSize == 32 ? 16 : 32;
            else
                addressSize = CodeSize;

            if (currentInstruction.Type == InstructionType.Group)
            {
                byte index;

                rmByte = ReadByte(offset++);
                gotRM = true;

                index = (byte)((rmByte >> 3) & 0x7);

                currentInstruction = groups[currentInstruction.Value, index];

                opCode = (ushort)((opCode << 8) + index);

                System.Diagnostics.Debug.Assert(opCode == currentInstruction.OpCode);                
            }
            else if (currentInstruction.Type == InstructionType.Extended)
            {
                byte extended;

                extended = ReadByte(offset++);

                currentInstruction = extendedInstructions[extended];

                opCode = (ushort)((opCode << 8) + extended);

                System.Diagnostics.Debug.Assert(opCode == currentInstruction.OpCode);
            }

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
            if (currentInstruction.Arg4.Type != ArgumentType.None)
            {
                InstructionText += ", ";
                offset = ProcessArgument(currentInstruction.Arg4, 3, offset);
            }
                
            return (int)offset;
        }

        public void Execute(object invoker, params Operand[] operands)
        {
            Operation operation;

            if(!operations.TryGetValue(currentInstruction.OpCode, out operation))
                throw new Exception("Instruction not implemented");

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
