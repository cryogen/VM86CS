using System.Collections.Generic;
using System;
using System.Reflection;

namespace x86Disasm
{
    public partial class Disassembler
    {
        private ReadCallback readFunction;
        private Operand[] operands;
        private Dictionary<uint, Operation> operations;
        private Instruction currentInstruction;
        private bool gotRM;
        private bool gotSIB;
        private byte rmByte;
        private byte sibByte;
        private bool buildString;
        private uint virtualAddr;

        public OPPrefix SetPrefixes { get; private set; }
        public string InstructionText { get; private set; }
        public int CodeSize { get; set; }
        public int OperandSize { get; private set; }
        public int AddressSize { get; private set; }
        public SegmentRegister OverrideSegment { get; private set; }

        public Operand[] Operands
        {
            get { return operands; }
        }

        public int NumberOfOperands { get { return currentInstruction.NumberOfArguments; } }
        public uint CurrentOpCode { get { return currentInstruction.OpCode; } }

        public Disassembler(ReadCallback readCallback)
        {
            readFunction = readCallback;
            operands = new Operand[4];
            operations = new Dictionary<uint, Operation>();
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
                operand.Register = registers8Bit[index];
            else if (operand.Size == 16)
                operand.Register = registers16Bit[index];
            else
                operand.Register = registers32Bit[index];

            if(buildString)
                InstructionText += operand.Register;
        }

        private void ProcessRegMemSegment(ref Operand operand, Argument argument, byte rmByte)
        {
            byte index = (byte)(rmByte & 0x7);

            operand.Type = OperandType.Register;
            operand.Register = registersSegment[index];
            if(buildString)
                InstructionText += operand.Register;
        }

        private void ProcessRegMemControl(ref Operand operand, Argument argument, byte rmByte)
        {
            byte index = (byte)(rmByte & 0x7);

            operand.Type = OperandType.Register;
            operand.Register = registersControl[index];
            if (buildString)
                InstructionText += operand.Register;
        }

        private uint ProcessRegMemMemory32(ref Operand operand, Argument argument, byte rmByte, uint offset)
        {
            byte mod, rm, reg;
            bool needDisp = false;

            operand.Type = OperandType.Memory;
            operand.Memory = regMemMemory32[rmByte & 0x7];

            mod = (byte)((rmByte >> 6) & 0x3);
            rm = (byte)(rmByte & 0x7);
            reg = (byte)((rmByte >> 3) & 0x7);

            if (rm == 4)
            {
                byte scale, baseIndex;
                MemoryOperand memory;
                
                if (!gotSIB)
                {
                    sibByte = ReadByte(offset++);
                    gotSIB = true;
                }

                baseIndex = (byte)(sibByte & 0x3f);
                scale = (byte)((sibByte >> 6) & 0x3);

                memory = regMemMemorySib[baseIndex];
                memory.Scale = scale;

                if ((baseIndex & 7) == 5)
                {
                    memory.Index = GeneralRegister.None;
                    needDisp = true;
                }

                operand.Memory.Base = memory.Base;
                operand.Memory.Index = memory.Index;
                operand.Memory.Segment = memory.Segment;
                operand.Memory.Scale = memory.Scale;
            }

            if (mod == 0 && rm == 5)
            {
                operand.Memory.Base = GeneralRegister.None;
                operand.Memory.Displacement = (int)ReadDWord(offset);
                operand.Memory.Segment = SegmentRegister.DS;
                offset += 4;
            }
            else if (rm == 4 && needDisp)
            {
                if (mod == 0)
                {
                    operand.Memory.Displacement = (int)ReadDWord(offset);
                    operand.Memory.Segment = SegmentRegister.DS;
                    offset += 4;
                }
                else if (mod == 1)
                {
                    operand.Memory.Displacement = (int)ReadByte(offset++);
                    operand.Memory.Segment = SegmentRegister.SS;
                    operand.Memory.Index = GeneralRegister.EBP;
                }
                else if (mod == 2)
                {
                    operand.Memory.Displacement = (int)ReadDWord(offset);
                    operand.Memory.Segment = SegmentRegister.SS;
                    operand.Memory.Index = GeneralRegister.EBP;
                    offset += 4;
                }
            }
            else if (mod == 1)
                operand.Memory.Displacement = (sbyte)ReadByte(offset++);
            else if (mod == 2)
            {
                operand.Memory.Displacement = (int)ReadDWord(offset);
                offset += 4;
            }

            if (OverrideSegment != SegmentRegister.Default)
                operand.Memory.Segment = OverrideSegment;

            if (buildString)
                InstructionText += operand.Memory;

            return offset;
        }

        private uint ProcessRegMemMemory(ref Operand operand, Argument argument, byte rmByte, uint offset)
        {
            byte mod, rm;

            operand.Type = OperandType.Memory;
            operand.Memory = regMemMemory16[rmByte & 0x7];
            
            mod = (byte)((rmByte >> 6) & 0x3);
            rm = (byte)(rmByte & 0x7);

            if (mod == 0 && rm == 6)
            {
                operand.Memory.Base = GeneralRegister.None;
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

            if(buildString)
                InstructionText += operand.Memory;

            return offset;
        }

        private uint ProcessArgument(Argument argument, int operandNumber, uint offset)
        {
            Operand operand = new Operand();

            if (argument.Size == 16 && !argument.IgnorePrefix)
                operand.Size = (uint)OperandSize;
            else
                operand.Size = argument.Size;

            switch (argument.Type)
            {
                case ArgumentType.Address:
                    operand.Type = OperandType.Address;
                    if (OperandSize == 32)
                    {
                        operand.Address = (uint)ReadDWord(offset);
                        offset += 4;
                        operand.Value = ReadWord(offset);
                    }
                    else
                    {
                        operand.Address = (uint)ReadWord(offset);
                        offset += 2;
                        operand.Value = ReadWord(offset);
                    }
                    offset += 2;
                    if(buildString)
                        InstructionText += operand.Value.ToString("X") + ":" + operand.Address.ToString("X");
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
                    if(buildString)
                        InstructionText += operand;
                    break;
                case ArgumentType.ImmediateSuppressDefault:
                    operand.Value = readFunction(offset, (int)operand.Size);
                    offset += operand.Size / 8;
                    operand.Type = OperandType.Immediate;
                    if (operand.Value != 10)
                        InstructionText += operand;
                    break;
                case ArgumentType.Relative:
                    operand.Type = OperandType.Immediate;
                    operand.Value = readFunction(offset, (int)operand.Size);
                    offset += operand.Size / 8;
                    if (buildString)
                    {
                        if (operand.SignedValue < 0)
                            InstructionText += "-" + (-operand.SignedValue).ToString("X") + " (" + ((virtualAddr & 0xffff0000) + (ushort)((ushort)virtualAddr + operand.Value + offset)).ToString("X") + ")";
                        else
                            InstructionText += operand.Value.ToString("X") + " (" + ((virtualAddr & 0xffff0000) + (ushort)((ushort)virtualAddr + operand.Value + offset)).ToString("X") + ")";
                    }
                    break;
                case ArgumentType.RegMem:
                case ArgumentType.RegMemGeneral:
                case ArgumentType.RegMemMemory:
                case ArgumentType.RegMemSegment:
                case ArgumentType.RegMemControl:
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
                            {
                                if(CodeSize == 16)
                                    offset = ProcessRegMemMemory(ref operand, argument, rmByte, offset);
                                else
                                    offset = ProcessRegMemMemory32(ref operand, argument, rmByte, offset);
                            }
                            break;
                        case ArgumentType.RegMemGeneral:
                            ProcessRegMemRegister(ref operand, argument, (byte)(rmByte >> 3));
                            break;
                        case ArgumentType.RegMemMemory:
                            if (AddressSize == 16)
                                offset = ProcessRegMemMemory(ref operand, argument, rmByte, offset);
                            else
                                offset = ProcessRegMemMemory32(ref operand, argument, rmByte, offset);
                            break;
                        case ArgumentType.RegMemSegment:
                            ProcessRegMemSegment(ref operand, argument, (byte)(rmByte >> 3));
                            break;
                        case ArgumentType.RegMemControl:
                            ProcessRegMemControl(ref operand, argument, (byte)(rmByte >> 3));
                            break;
                        default:
                            System.Diagnostics.Debugger.Break();
                            break;
                    }
                    break;
                case ArgumentType.GeneralRegister:
                    operand.Type = OperandType.Register;
                    if (operand.Size == 8)
                        operand.Register = registers8Bit[argument.Value];
                    else if(operand.Size == 16)
                        operand.Register = registers16Bit[argument.Value];
                    else
                        operand.Register = registers32Bit[argument.Value];

                    if(buildString)
                        InstructionText += operand.Register;
                    break;
                case ArgumentType.SegmentRegister:
                    operand.Type = OperandType.Register;
                    operand.Register = registersSegment[argument.Value];
                    if(buildString)
                        InstructionText += operand.Register;
                    break;
                case ArgumentType.Memory:
                    operand.Type = OperandType.Memory;
                    if (argument.UsesES)
                    {
                        operand.Memory.Base = GeneralRegister.EDI;
                        operand.Memory.Segment = SegmentRegister.ES;
                    }
                    else
                    {
                        operand.Memory.Base = GeneralRegister.ESI;
                        if (OverrideSegment == SegmentRegister.Default)
                            operand.Memory.Segment = SegmentRegister.DS;
                        else
                            operand.Memory.Segment = OverrideSegment;
                    }
                    operand.Memory.Index = GeneralRegister.None;
                    operand.Memory.Size = CodeSize;

                    if(buildString)
                        InstructionText += operand.Memory;
                    break;
                case ArgumentType.Offset:
                    operand.Type = OperandType.Memory;

                    operand.Memory.Index = GeneralRegister.None;
                    operand.Memory.Base = GeneralRegister.None;
                    operand.Memory.Displacement = (int)readFunction(offset, AddressSize);
                    offset += (uint)(AddressSize / 8);
                    if (OverrideSegment == SegmentRegister.Default)
                        operand.Memory.Segment = SegmentRegister.DS;
                    else
                        operand.Memory.Segment = OverrideSegment;

                    if(buildString)
                        InstructionText += operand.Memory;
                    break;
                case ArgumentType.Constant:
                    operand.Type = OperandType.Immediate;
                    operand.Value = (uint)argument.Value;

                    if(buildString)
                        InstructionText += operand;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }

            operands[operandNumber] = operand;

            return offset;
        }

        public void AddOperation(uint opCode, Delegate method, int numArgs)
        {
            Operation operation = new Operation { OpCode = opCode, Method=method, NumberOfArgs=numArgs };

            operations.Add(opCode, operation);
        }

        public int Disassemble(uint addr)
        {
            return Disassemble(addr, false);
        }

        public int Disassemble(uint addr, bool doStrings)
        {
            uint offset = 0;
            uint opCode;

            InstructionText = "";
            gotRM = false;
            gotSIB = false;
            virtualAddr = addr;
            SetPrefixes = 0;
            OverrideSegment = SegmentRegister.Default;
            buildString = doStrings;
                
            opCode = (byte)readFunction(offset, 8);
            currentInstruction = instructions[opCode];
            offset++;

            System.Diagnostics.Debug.Assert(opCode == currentInstruction.OpCode);

            while (currentInstruction.Type == InstructionType.Prefix)
            {
                SetPrefixes |= (OPPrefix)currentInstruction.Value;
                opCode = ReadByte(offset);
                currentInstruction = instructions[opCode];
                offset++;
            }

            if (buildString)
            {
                if ((SetPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat)
                    InstructionText = "REP ";
                else if ((SetPrefixes & OPPrefix.RepeatNotEqual) == OPPrefix.RepeatNotEqual)
                    InstructionText = "REPNE ";
            }
            
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
                OperandSize = CodeSize == 32 ? 16 : 32;
            else
                OperandSize = CodeSize;

            if ((SetPrefixes & OPPrefix.AddressSize) == OPPrefix.AddressSize)
                AddressSize = CodeSize == 32 ? 16 : 32;
            else
                AddressSize = CodeSize;

            if (currentInstruction.Type == InstructionType.Extended)
            {
                byte extended;

                extended = ReadByte(offset++);

                currentInstruction = extendedInstructions[extended];

                opCode = (uint)((opCode << 8) + extended);

                System.Diagnostics.Debug.Assert(opCode == currentInstruction.OpCode);
            }

            if (currentInstruction.Type == InstructionType.Group)
            {
                byte index;

                rmByte = ReadByte(offset++);
                gotRM = true;

                index = (byte)((rmByte >> 3) & 0x7);

                currentInstruction = groups[currentInstruction.Value, index];

                opCode = (uint)((opCode << 8) + index);

                System.Diagnostics.Debug.Assert(opCode == currentInstruction.OpCode);                
            }

            if (currentInstruction.Type == InstructionType.Invalid)
                throw new Exception("Invalid operation");

            if(buildString)
                InstructionText += currentInstruction.Nmumonic + " ";

            if (currentInstruction.Arg1.Type != ArgumentType.None)
                offset = ProcessArgument(currentInstruction.Arg1, 0, offset);
            if (currentInstruction.Arg2.Type != ArgumentType.None)
            {
                if(buildString)
                    InstructionText += ", ";
                offset = ProcessArgument(currentInstruction.Arg2, 1, offset);
            }
            if (currentInstruction.Arg3.Type != ArgumentType.None)
            {
                if(buildString)
                    InstructionText += ", ";
                offset = ProcessArgument(currentInstruction.Arg3, 2, offset);
            }
            if (currentInstruction.Arg4.Type != ArgumentType.None)
            {
                if(buildString)
                    InstructionText += ", ";
                offset = ProcessArgument(currentInstruction.Arg4, 3, offset);
            }
                
            return (int)offset;
        }

        public void Execute(params Operand[] operands)
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
