using System;
using System.Runtime.InteropServices;
using log4net;
using System.Collections.Generic;
using x86Disasm;

namespace x86CS.CPU
{
    public enum RepeatPrefix
    {
        None = 0,
        Repeat,
        RepeatNotZero
    }

    public partial class CPU
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CPU));

        private readonly Segment[] segments;
        private readonly Register[] registers;
        private readonly uint[] controlRegisters;
        private CPUFlags eFlags;
        public event ReadCallback IORead;
        public event WriteCallback IOWrite;
        private TableRegister idtRegister, gdtRegister;
        private readonly GDTEntry realModeEntry;
        private bool inInterrupt;
        private byte interruptToRun;
        private RepeatPrefix repeatPrefix = RepeatPrefix.None;
        private int opSize = 16;
        private int addressSize = 16;
        private Disassembler disasm;

        public bool Halted { get; private set; }
        public uint CurrentAddr { get; private set; }
        public bool PMode { get; private set; }
        public int InterruptLevel { get; private set; }

        #region Registers

        public uint CR0
        {
            get { return controlRegisters[0]; }
            set { controlRegisters[0] = value; }
        }

        public uint CR1
        {
            get { return controlRegisters[1]; }
            set { controlRegisters[1] = value; }
        }

        public uint CR2
        {
            get { return controlRegisters[2]; }
            set { controlRegisters[2] = value; }
        }

        public uint CR3
        {
            get { return controlRegisters[3]; }
            set { controlRegisters[3] = value; }
        }

        public uint CR4
        {
            get { return controlRegisters[4]; }
            set { controlRegisters[4] = value; }
        }

        public uint EAX
        {
            get { return registers[(int)CPURegister.EAX].DWord; }
            set { registers[(int)CPURegister.EAX].DWord = value; }
        }

        public ushort AX
        {
            get { return registers[(int)CPURegister.EAX].Word; }
            set { registers[(int)CPURegister.EAX].Word = value; }
        }

        public byte AL
        {
            get { return registers[(int)CPURegister.EAX].LowByte; }
            set { registers[(int)CPURegister.EAX].LowByte = value; }
        }

        public byte AH
        {
            get { return registers[(int)CPURegister.EAX].HighByte; }
            set { registers[(int)CPURegister.EAX].HighByte = value; }
        }

        public uint EBX
        {
            get { return registers[(int)CPURegister.EBX].DWord; }
            set { registers[(int)CPURegister.EBX].DWord = value; }
        }

        public ushort BX
        {
            get { return registers[(int)CPURegister.EBX].Word; }
            set { registers[(int)CPURegister.EBX].Word = value; }
        }

        public byte BL
        {
            get { return registers[(int)CPURegister.EBX].LowByte; }
            set { registers[(int)CPURegister.EBX].LowByte = value; }
        }

        public byte BH
        {
            get { return registers[(int)CPURegister.EBX].HighByte; }
            set { registers[(int)CPURegister.EBX].HighByte = value; }
        }

        public uint ECX
        {
            get { return registers[(int)CPURegister.ECX].DWord; }
            set { registers[(int)CPURegister.ECX].DWord = value; }
        }

        public ushort CX
        {
            get { return registers[(int)CPURegister.ECX].Word; }
            set { registers[(int)CPURegister.ECX].Word = value; }
        }

        public byte CL
        {
            get { return registers[(int)CPURegister.ECX].LowByte; }
            set { registers[(int)CPURegister.ECX].LowByte = value; }
        }

        public byte CH
        {
            get { return registers[(int)CPURegister.ECX].HighByte; }
            set { registers[(int)CPURegister.ECX].HighByte = value; }
        }

        public uint EDX
        {
            get { return registers[(int)CPURegister.EDX].DWord; }
            set { registers[(int)CPURegister.EDX].DWord = value; }
        }

        public ushort DX
        {
            get { return registers[(int)CPURegister.EDX].Word; }
            set { registers[(int)CPURegister.EDX].Word = value; }
        }

        public byte DL
        {
            get { return registers[(int)CPURegister.EDX].LowByte; }
            set { registers[(int)CPURegister.EDX].LowByte = value; }
        }

        public byte DH
        {
            get { return registers[(int)CPURegister.EDX].HighByte; }
            set { registers[(int)CPURegister.EDX].HighByte = value; }
        }

        public uint ESI
        {
            get { return registers[(int)CPURegister.ESI].DWord; }
            set { registers[(int)CPURegister.ESI].DWord = value; }
        }

        public ushort SI
        {
            get { return registers[(int)CPURegister.ESI].Word; }
            set { registers[(int)CPURegister.ESI].Word = value; }
        }

        public uint EDI
        {
            get { return registers[(int)CPURegister.EDI].DWord; }
            set { registers[(int)CPURegister.EDI].DWord = value; }
        }

        public ushort DI
        {
            get { return registers[(int)CPURegister.EDI].Word; }
            set { registers[(int)CPURegister.EDI].Word = value; }
        }

        public uint EBP
        {
            get { return registers[(int)CPURegister.EBP].DWord; }
            set { registers[(int)CPURegister.EBP].DWord = value; }
        }

        public ushort BP
        {
            get { return registers[(int)CPURegister.EBP].Word; }
            set { registers[(int)CPURegister.EBP].Word = value; }
        }

        public uint EIP
        {
            get { return registers[(int)CPURegister.EIP].DWord; }
            set { registers[(int)CPURegister.EIP].DWord = value; }
        }

        public ushort IP
        {
            get { return registers[(int)CPURegister.EIP].Word; }
            set { registers[(int)CPURegister.EIP].Word = value; }
        }

        public uint ESP
        {
            get { return registers[(int)CPURegister.ESP].DWord; }
            set { registers[(int)CPURegister.ESP].DWord = value; }
        }

        public ushort SP
        {
            get { return registers[(int)CPURegister.ESP].Word; }
            set { registers[(int)CPURegister.ESP].Word = value; }
        }

        #endregion
        #region Segments
        public uint CS
        {
            get { return (ushort)segments[(int)SegmentRegister.CS].Selector; }
            set { SetSelector(SegmentRegister.CS, value); }
        }

        public ushort DS
        {
            get { return (ushort)segments[(int)SegmentRegister.DS].Selector; }
            set { SetSelector(SegmentRegister.DS, value); }
        }
        public ushort ES
        {
            get { return (ushort)segments[(int)SegmentRegister.ES].Selector; }
            set { SetSelector(SegmentRegister.ES, value); }
        }
        public ushort SS
        {
            get { return (ushort)segments[(int)SegmentRegister.SS].Selector; }
            set { SetSelector(SegmentRegister.SS, value); }
        }
        public ushort FS
        {
            get { return (ushort)segments[(int)SegmentRegister.FS].Selector; }
            set { SetSelector(SegmentRegister.FS, value); }
        }
        public ushort GS
        {
            get { return (ushort)segments[(int)SegmentRegister.GS].Selector; }
            set { SetSelector(SegmentRegister.GS, value); }
        }
        #endregion
        #region Flags
        public ushort EFlags
        {
            get { return (ushort)eFlags; }
            set { eFlags = (CPUFlags)value; }
        }

        public CPUFlags Flags
        {
            get { return eFlags; }
        }

        public bool CF
        {
            get { return GetFlag(CPUFlags.CF); }
            set { SetFlag(CPUFlags.CF, value); }
        }

        public bool PF
        {
            get { return GetFlag(CPUFlags.PF); }
            set { SetFlag(CPUFlags.PF, value); }
        }

        public bool AF
        {
            get { return GetFlag(CPUFlags.AF); }
            set { SetFlag(CPUFlags.AF, value); }
        }

        public bool ZF
        {
            get { return GetFlag(CPUFlags.ZF); }
            set { SetFlag(CPUFlags.ZF, value); }
        }

        public bool SF
        {
            get { return GetFlag(CPUFlags.SF); }
            set { SetFlag(CPUFlags.SF, value); }
        }

        public bool TF
        {
            get { return GetFlag(CPUFlags.TF); }
            set { SetFlag(CPUFlags.TF, value); }
        }

        public bool IF
        {
            get { return GetFlag(CPUFlags.IF); }
            set { SetFlag(CPUFlags.IF, value); }
        }

        public bool DF
        {
            get { return GetFlag(CPUFlags.DF); }
            set { SetFlag(CPUFlags.DF, value); }
        }

        public bool OF
        {
            get { return GetFlag(CPUFlags.OF); }
            set { SetFlag(CPUFlags.OF, value); }
        }

        public byte IOPL
        {
            get { return (byte)(((int)eFlags & 0x3000) >> 12); }
            set { eFlags = (CPUFlags)(value & 0x3000); }
        }

        public bool NT
        {
            get { return GetFlag(CPUFlags.NT); }
            set { SetFlag(CPUFlags.NT, value); }
        }

        public bool RF
        {
            get { return GetFlag(CPUFlags.RF); }
            set { SetFlag(CPUFlags.RF, value); }
        }

        public bool VM
        {
            get { return GetFlag(CPUFlags.VM); }
            set { SetFlag(CPUFlags.VM, value); }
        }

        public bool AC
        {
            get { return GetFlag(CPUFlags.AC); }
            set { SetFlag(CPUFlags.AC, value); }
        }

        public bool VIF
        {
            get { return GetFlag(CPUFlags.VIF); }
            set { SetFlag(CPUFlags.VIF, value); }
        }

        public bool VIP
        {
            get { return GetFlag(CPUFlags.VIP); }
            set { SetFlag(CPUFlags.VIP, value); }
        }

        public bool ID
        {
            get { return GetFlag(CPUFlags.ID); }
            set { SetFlag(CPUFlags.ID, value); }
        }

        #endregion

        public CPU()
        {
            PMode = false;
            segments = new Segment[6];
            registers = new Register[9];
            controlRegisters = new uint[5];
            idtRegister = new TableRegister();
            gdtRegister = new TableRegister();
            disasm = new Disassembler(DisassemblerRead);
            realModeEntry = new GDTEntry
                                {
                                    BaseAddress = 0,
                                    Is32Bit = false,
                                    IsAccessed = true,
                                    IsCode = false,
                                    Limit = 0xffff,
                                    IsWritable = true
                                };

            Halted = false;
            Reset();
        }

        public void Reset()
        {
            eFlags = CPUFlags.ZF | CPUFlags.IF;

            EIP = 0;
            CS = 0;
            EAX = 0;
            EBX = 0;
            ECX = 0;
            EDX = 0;
            EBP = 0;
            ESP = 0;
            DS = 0;
            ES = 0;
            FS = 0;
            GS = 0;
            inInterrupt = false;
        }

        private bool GetFlag(CPUFlags flag)
        {
            return (eFlags & flag) == flag;
        }

        private void SetFlag(CPUFlags flag, bool value)
        {
            if (value)
                eFlags |= flag;
            else
                eFlags &= ~flag;
        }

        private uint DisassemblerRead(uint offset, int size)
        {
            return Memory.Read(CurrentAddr + offset, size);
        }

        private uint GetVirtualAddress(SegmentRegister segment, uint offset)
        {
            Segment seg = segments[(int)segment];

            return seg.GDTEntry.BaseAddress + offset;
        }

        private uint SegRead(SegmentRegister segment, uint offset, int size)
        {
            uint virtAddr = GetVirtualAddress(segment, offset);

            return Memory.Read(virtAddr, size);
        }

        private byte SegReadByte(SegmentRegister segment, uint offset)
        {
            return (byte)SegRead(segment, offset, 8);
        }

        private ushort SegReadWord(SegmentRegister segment, uint offset)
        {
            return (ushort)SegRead(segment, offset, 16);
        }

        private uint SegReadDWord(SegmentRegister segment, uint offset)
        {
            return SegRead(segment, offset, 32);
        }

        private void SegWriteByte(SegmentRegister segment, uint offset, byte value)
        {
            uint virtAddr = GetVirtualAddress(segment, offset);

            Memory.WriteByte(virtAddr, value);
        }

        private void SegWriteWord(SegmentRegister segment, uint offset, ushort value)
        {
            uint virtAddr = GetVirtualAddress(segment, offset);

            Memory.WriteWord(virtAddr, value);
        }

        private void SegWriteDWord(SegmentRegister segment, uint offset, uint value)
        {
            uint virtAddr = GetVirtualAddress(segment, offset);

            Memory.WriteDWord(virtAddr, value);
        }

        public uint StackPop()
        {
            uint ret;

            if (PMode)
            {
                if (opSize == 32)
                {
                    ret = SegReadWord(SegmentRegister.SS, ESP);
                    ESP += 2;
                }
                else
                {
                    ret = SegReadDWord(SegmentRegister.SS, ESP);
                    ESP += 4;
                }
            }
            else
            {
                if (opSize == 32)
                {
                    ret = SegReadDWord(SegmentRegister.SS, SP);
                    SP += 4;
                }
                else
                {
                    ret = SegReadWord(SegmentRegister.SS, SP);
                    SP += 2;
                }
            }
            return ret;
        }

        /*public void StackPush(ushort value)
        {
            if (opSize == 32)
                ESP -= 2;
            else
                SP -= 2;

            SegWriteWord(SegmentRegister.SS, SP, value);
        }*/

        public void StackPush(uint value)
        {
            if (opSize == 32)
            {
                ESP -= 4;
                SegWriteDWord(SegmentRegister.SS, ESP, value);
            }
            else
            {
                SP -= 2;
                SegWriteWord(SegmentRegister.SS, SP, (ushort) value);
            }
        }

        private GDTEntry GetSelectorEntry(uint selector)
        {
            int entrySize = Marshal.SizeOf(typeof(GDTEntry));
            var gdtBytes = new byte[entrySize];

            Memory.BlockRead(gdtRegister.Base + selector, gdtBytes, gdtBytes.Length);
            IntPtr p = Marshal.AllocHGlobal(entrySize);
            Marshal.Copy(gdtBytes, 0, p, entrySize);
            var entry = (GDTEntry)Marshal.PtrToStructure(p, typeof(GDTEntry));
            Marshal.FreeHGlobal(p);

            return entry;
        }

        private void SetSelector(SegmentRegister segment, uint selector)
        {
            if (PMode)
            {
                segments[(int)segment].Selector = selector;
                segments[(int)segment].GDTEntry = GetSelectorEntry(selector);
            }
            else
            {
                segments[(int)segment].Selector = selector;
                segments[(int)segment].GDTEntry = realModeEntry;
                segments[(int)segment].GDTEntry.BaseAddress = selector << 4;
            }
        }

        private void ProcedureEnter(ushort size, byte level)
        {
            var nestingLevel = (ushort)(level % 32);

            StackPush(BP);
            ushort frameTemp = SP;

            if (nestingLevel > 0)
            {
                for (int i = 1; i < nestingLevel - 1; i++)
                {
                    BP -= 2;
                    StackPush(SegReadWord(SegmentRegister.SS, BP));
                }
                StackPush(frameTemp);
            }

            BP = frameTemp;
            SP = (ushort)(BP - size);
        }

        private void ProcedureLeave()
        {
            SP = BP;
            BP = (ushort)StackPop();
        }

        private void DoJump(uint segment, uint offset, bool relative)
        {
            Segment codeSegment = segments[(int)SegmentRegister.CS];
            uint tempEIP;

            if (PMode == false && ((CR0 & 0x1) == 0x1))
                PMode = true;
            else if (PMode && ((CR0 & 0x1) == 0))
                PMode = false;

            if (segment == CS)
            {
                if (relative)
                {
                    var relOffset = (int)offset;

                    tempEIP = (uint)(EIP + relOffset);
                }
                else
                    tempEIP = offset;

                if (tempEIP > codeSegment.GDTEntry.Limit)
                    throw new Exception("General Fault Code 0");
            }
            else
            {
                if (PMode)
                {
                    if (segment == 0)
                        throw new Exception("Null segment selector");

                    if (segment > (gdtRegister.Limit))
                        throw new Exception("Selector out of range");

                    GDTEntry newEntry = GetSelectorEntry(segment);

                    if (!newEntry.IsCode)
                        throw new Exception("Segment is not code");

                    CS = segment;
                    if (relative)
                    {
                        var relOffset = (int)offset;

                        tempEIP = (uint)(EIP + relOffset);
                    }
                    else
                        tempEIP = offset;                    
                }
                else
                {
                    if (relative)
                        tempEIP = EIP + offset;
                    else
                        tempEIP = offset;
                    if (tempEIP > codeSegment.GDTEntry.Limit)
                        throw new Exception("EIP Out of range");

                    CS = segment;

                    if (opSize == 32)
                        EIP = tempEIP;
                    else
                        EIP = (ushort)tempEIP;
                }
            }
            if (opSize == 32)
                EIP = tempEIP;
            else
                EIP = (ushort)tempEIP;
        }

        private void CallInterrupt(byte vector)
        {
            //Logger.Debug("INT" + vector.ToString("X"));
            StackPush((ushort)Flags);
            IF = false;
            TF = false;
            AC = false;
            StackPush(CS);
            StackPush(IP);

            CS = Memory.Read((uint)(vector * 4) + 2, 16);
            EIP = Memory.Read((uint)(vector * 4), 16);

            InterruptLevel++;
        }

        public void Interrupt(int vector, int irq)
        {
            inInterrupt = true;
            interruptToRun = (byte)vector;
        }

        private void DumpRegisters()
        {
            Logger.Debug(String.Format("AX {0:X4} BX {1:X4} CX {2:X4} DX {3:X4}", AX, BX, CX, DX));
            Logger.Debug(String.Format("SI {0:X4} DI {1:X4} SP {2:X4} BP {3:X4}", SI, DI, SP, BP));
            Logger.Debug(String.Format("CS {0:X4} DS {1:X4} ES {2:X4} SS {3:X4}", CS, DS, ES, SS));
        }

        [Flags]
        private enum RegisterType
        {
            NoRegister = 0x0000,
            MMX = 0x10000,
            General = 0x20000,
            FPU = 0x40000,
            SSE = 0x80000,
            CR = 0x100000,
            DR = 0x200000,
            Special = 0x400000,
            MemoryManagement = 0x800000,
            Segment = 0x1000000
        }

        private SegmentRegister MemorySegmentToActualSegment(MemorySegment segment)
        {
            switch (segment)
            {
                case MemorySegment.CS:
                    return SegmentRegister.CS;
                case MemorySegment.DS:
                    return SegmentRegister.DS;
                case MemorySegment.ES:
                    return SegmentRegister.ES;
                case MemorySegment.FS:
                    return SegmentRegister.FS;
                case MemorySegment.GS:
                    return SegmentRegister.GS;
                case MemorySegment.SS:
                    return SegmentRegister.SS;
            }

            return SegmentRegister.Default;
        }

        private uint ReadGeneralRegsiter(uint register, int size, bool high)
        {
            if (size == 32)
                return registers[register].DWord;
            else if (size == 16)
                return registers[register].Word;
            else if (size == 8 && high)
                return registers[register].HighByte;
            else if (size == 8)
                return registers[register].LowByte;
            else
                System.Diagnostics.Debugger.Break();

            return 0xffffffff;
        }

/*        private uint ReadRegister(Operand operand)
        {
            switch (operand.RegisterType)
            {
                case RegisterType.General:
                    return ReadGeneralRegsiter(operand.Register, (int)operand.OperandSize, operand.High);
                case RegisterType.Segment:
                    return segments[operand.Register].Selector;
                default:
                    break;
            }

            return 0;
        }*/

/*        private void WriteRegister(Operand operand, uint value)
        {
            switch (operand.RegisterType)
            {
                case RegisterType.General:
                    if (operand.OperandSize == 32)
                        registers[operand.Register].DWord = value;
                    else if (operand.OperandSize == 16)
                        registers[operand.Register].Word = (ushort)value;
                    else if (operand.OperandSize == 8 && operand.High)
                        registers[operand.Register].HighByte = (byte)value;
                    else if (operand.OperandSize == 8)
                        registers[operand.Register].LowByte = (byte)value;
                    else
                        System.Diagnostics.Debugger.Break();
                    break;
                case RegisterType.Segment:
                    SetSelector((SegmentRegister)operand.Register, value);
                    break;
                default:
                    break;
            }
        }*/

        /*private uint GetOperandValue(Operand operand)
        {
            if (operand.Type == OperandType.Register)
                return ReadRegister(operand);
            else if (operand.Type == OperandType.Immediate)
                return operand.Value;
            else if (operand.Type == OperandType.Memory)
            {
                if (operand.OperandSize == 8)
                    return (byte)SegReadByte(operand.Segment, operand.Address);
                else if (operand.OperandSize == 16)
                    return (ushort)SegReadWord(operand.Segment, operand.Address);
                else return SegReadDWord(operand.Segment, operand.Address);
            }

            return 0;
        }

        private void SetOperandValue(Operand operand, uint value)
        {
            if (operand.Type == OperandType.Register)
                WriteRegister(operand, value);
            else if (operand.Type == OperandType.Memory)
            {
                if (operand.OperandSize == 8)
                    SegWriteByte(operand.Segment, operand.Address, (byte)value);
                else if (operand.OperandSize == 16)
                    SegWriteWord(operand.Segment, operand.Address, (ushort)value);
                else if (operand.OperandSize == 32)
                    SegWriteDWord(operand.Segment, operand.Address, value);
            }
            else
                System.Diagnostics.Debugger.Break();
        }

        private uint RegisterFromBeaRegister(int register)
        {
            switch (register)
            {
                case 0x1:
                    return 0;
                case 0x2:
                    return 1;
                case 0x4:
                    return 2;
                case 0x8:
                    return 3;
                case 0x10:
                    return 4;
                case 0x20:
                    return 5;
                case 0x40:
                    return 6;
                case 0x80:
                    return 7;
                default:
                    return 0xffffffff;
            }
        }*/

        /*private Operand ProcessArgument(ArgumentType argument)
        {
            Operand operand = new Operand();
            BeaConstants.ArgumentType argType;

            operand.OperandSize = (uint)argument.ArgSize;

            argType = (BeaConstants.ArgumentType)(argument.ArgType & 0xffff0000);
            if ((argType & BeaConstants.ArgumentType.MEMORY_TYPE) == BeaConstants.ArgumentType.MEMORY_TYPE)
            {
                uint baseRegister = 0;

                if (argument.Memory.IndexRegister != 0 || argument.Memory.Scale != 0)
                    System.Diagnostics.Debugger.Break();
                if (argument.Memory.BaseRegister != 0)
                {
                    baseRegister = RegisterFromBeaRegister(argument.Memory.BaseRegister);
                    operand.Address = ReadGeneralRegsiter(baseRegister, PMode ? 32 : 16, false);
                }

                operand.Address = (uint)(operand.Address + argument.Memory.Displacement);
                operand.Type = OperandType.Memory;
                operand.Segment = MemorySegmentToActualSegment((MemorySegment)argument.SegmentReg);
            }
            else if ((argType & BeaConstants.ArgumentType.REGISTER_TYPE) == BeaConstants.ArgumentType.REGISTER_TYPE)
            {
                operand.RegisterType = (RegisterType)((int)argType & ~0xf0000000);
                operand.Register = RegisterFromBeaRegister(argument.ArgType & 0x0000ffff);
                
                if (argument.ArgPosition == 1)
                    operand.High = true;
                else
                    operand.High = false;

                operand.Type = OperandType.Register;
            }
            else if ((argType & BeaConstants.ArgumentType.CONSTANT_TYPE) == BeaConstants.ArgumentType.CONSTANT_TYPE)
            {
                operand.Value = (uint)currentInstruction.Instruction.Immediate;
                operand.Type = OperandType.Immediate;
            }

            if (currentInstruction.Prefix.CSPrefix == 1)
                operand.Segment = SegmentRegister.CS;
            else if (currentInstruction.Prefix.DSPrefix == 1)
                operand.Segment = SegmentRegister.DS;
            else if (currentInstruction.Prefix.ESPrefix == 1)
                operand.Segment = SegmentRegister.ES;
            else if (currentInstruction.Prefix.FSPrefix == 1)
                operand.Segment = SegmentRegister.FS;
            else if (currentInstruction.Prefix.GSPrefix == 1)
                operand.Segment = SegmentRegister.GS;
            else if (currentInstruction.Prefix.SSPrefix == 1)
                operand.Segment = SegmentRegister.SS;

            return operand;
        }

        private Operand[] ProcessOperands()
        {
            List<Operand> operands = new List<Operand>();

            if (currentInstruction.Argument1.ArgType != (int)BeaConstants.ArgumentType.NO_ARGUMENT)
                operands.Add(ProcessArgument(currentInstruction.Argument1));
            if (currentInstruction.Argument2.ArgType != (int)BeaConstants.ArgumentType.NO_ARGUMENT)
                operands.Add(ProcessArgument(currentInstruction.Argument2));
            if (currentInstruction.Argument3.ArgType != (int)BeaConstants.ArgumentType.NO_ARGUMENT)
                operands.Add(ProcessArgument(currentInstruction.Argument3));

            return operands.ToArray();
        }*/

        public void Cycle()
        {
            if(inInterrupt)
            {
                CallInterrupt(interruptToRun);
                inInterrupt = false;
                Halted = false;
                return;
            }

            if (Halted)
                return;

            CurrentAddr = segments[(int)SegmentRegister.CS].GDTEntry.BaseAddress + EIP;
            disasm.Disassemble(CurrentAddr);

           // if(InterruptLevel == 0)
/*            if(Logger.IsDebugEnabled)
                Logger.Debug(String.Format("{0:X}:{1:X}    {2}", CS, EIP, instruction.CompleteInstr));*/

            //EIP += (uint)len;

            /*switch ((BeaConstants.InstructionType)(currentInstruction.Instruction.Category & 0x0000FFFF))
            {
                case BeaConstants.InstructionType.CONTROL_TRANSFER:
                    ProcessControlTransfer(operands);
                    break;
                case BeaConstants.InstructionType.BIT_UInt8:
                case BeaConstants.InstructionType.LOGICAL_INSTRUCTION:
                    ProcessLogic(operands);
                    break;
                case BeaConstants.InstructionType.InOutINSTRUCTION:
                    ProcessInputOutput(operands);
                    break;
                case BeaConstants.InstructionType.DATA_TRANSFER:
                    ProcessDataTransfer(operands);
                    break;
                case BeaConstants.InstructionType.ARITHMETIC_INSTRUCTION:
                    ProcessArithmetic(operands);
                    break;
                case BeaConstants.InstructionType.FLAG_CONTROL_INSTRUCTION:
                    ProcessFlagControl(operands);
                    break;
                case BeaConstants.InstructionType.STRING_INSTRUCTION:
                    ProcessString(operands);
                    break;
                case BeaConstants.InstructionType.MISCELLANEOUS_INSTRUCTION:
                    ProcessMisc(operands);
                    break;
                default:
                    break;
            }*/

            CurrentAddr = segments[(int)SegmentRegister.CS].GDTEntry.BaseAddress + EIP;
        }
    }
}
