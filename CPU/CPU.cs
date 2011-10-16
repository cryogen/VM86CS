using System;
using System.Runtime.InteropServices;
using log4net;
using System.Collections.Generic;
using x86Disasm;
using System.Reflection;
using System.Threading;

namespace x86CS.CPU
{
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
        private int opSize = 16;
        private int addressSize = 16;
        private Disassembler disasm;
        private Operand interruptOperand;

        public bool Halted { get; private set; }
        public uint CurrentAddr { get; private set; }
        public bool PMode { get; private set; }
        public int OpLen { get; private set; }

        public int InterruptLevel;

        public string InstructionText
        {
            get { return disasm.InstructionText; }
        }

        public uint StackPointer
        {
            get
            {
                return segments[(int)SegmentRegister.SS].GDTEntry.BaseAddress + ESP;
            }
        }

        public uint BasePointer
        {
            get
            {
                return segments[(int)SegmentRegister.SS].GDTEntry.BaseAddress + EBP;
            }
        }

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

            disasm.CodeSize = 16;
            ProcessOperations();
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

            interruptOperand = new Operand();
            interruptOperand.Size = 8;
            interruptOperand.Type = OperandType.Immediate;

            Reset();
        }

        public void Reset()
        {
            eFlags = CPUFlags.ZF | CPUFlags.IF;

            PMode = false;

            EIP = 0;
            CS = 0;
            EAX = 0;
            EBX = 0;
            ECX = 0;
            EDX = 0;
            EBP = 0;
            ESP = 0;
            ESI = 0;
            EDI = 0;
            DS = 0;
            ES = 0;
            FS = 0;
            GS = 0;

            Halted = false;
            opSize = addressSize = 16;
            disasm.CodeSize = 16;
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

        private void ProcessOperations()
        {
            foreach (MethodInfo method in (typeof(CPU)).GetMethods())
            {
                foreach (var attribute in method.GetCustomAttributes(typeof(CPUFunction), true))
                {
                    CPUFunction function = attribute as CPUFunction;
                    Delegate methodDelegate;

                    try
                    {
                        switch (method.GetParameters().Length)
                        {
                            case 0:
                                methodDelegate = Delegate.CreateDelegate(typeof(CPUCallbackNoargs), this, method);
                                break;
                            case 1:
                                methodDelegate = Delegate.CreateDelegate(typeof(CPUCallback1args), this, method);
                                break;
                            case 2:
                                methodDelegate = Delegate.CreateDelegate(typeof(CPUCallback2args), this, method);
                                break;
                            case 3:
                                methodDelegate = Delegate.CreateDelegate(typeof(CPUCallback3args), this, method);
                                break;
                            case 4:
                                methodDelegate = Delegate.CreateDelegate(typeof(CPUCallback4args), this, method);
                                break;
                            default:
                                throw new Exception("Method signature not supported");
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("Method signature not supported");
                    }

                    for (int i = 0; i < function.Count; i++)
                    {
                        disasm.AddOperation((uint)(function.OpCode + i), methodDelegate, method.GetParameters().Length);
                    }
                }
            }
        }

        private void WriteOperand(Operand operand)
        {
            switch (operand.Type)
            {
                case OperandType.Register:
                    switch (operand.Register.Type)
                    {
                        case RegisterType.GeneralRegister:
                            switch (operand.Size)
                            {
                                case 8:
                                    if (operand.Register.High)
                                        registers[(int)operand.Register.Index].HighByte = (byte)operand.Value;
                                    else
                                        registers[(int)operand.Register.Index].LowByte = (byte)operand.Value;
                                    break;
                                case 16:
                                    registers[(int)operand.Register.Index].Word = (ushort)operand.Value;
                                    break;
                                case 32:
                                    registers[(int)operand.Register.Index].DWord = operand.Value;
                                    break;
                            }
                            break;
                        case RegisterType.SegmentRegister:
                            SetSelector((SegmentRegister)operand.Register.Index, operand.Value);
                            break;
                        case RegisterType.ControlRegister:
                            controlRegisters[operand.Register.Index] = operand.Value;
                            break;
                        default:
                            System.Diagnostics.Debugger.Break();
                            break;
                    }
                    break;
                case OperandType.Memory:
                    SegWrite(operand.Memory.Segment, operand.Memory.Address, operand.Value, (int)operand.Size);
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }

        private Operand ProcessOperand(Operand operand)
        {
            switch (operand.Type)
            {
                case OperandType.Immediate:
                case OperandType.Address:
                    return operand;
                case OperandType.Register:
                    switch (operand.Register.Type)
                    {
                        case RegisterType.GeneralRegister:
                            switch (operand.Size)
                            {
                                case 8:
                                    if (operand.Register.High)
                                        operand.Value = registers[(int)operand.Register.Index].HighByte;
                                    else
                                        operand.Value = registers[(int)operand.Register.Index].LowByte;
                                    break;
                                case 16:
                                    operand.Value = registers[(int)operand.Register.Index].Word;
                                    break;
                                case 32:
                                    operand.Value = registers[(int)operand.Register.Index].DWord;
                                    break;
                            }
                            break;
                        case RegisterType.SegmentRegister:
                            operand.Value = segments[operand.Register.Index].Selector;
                            break;
                        case RegisterType.ControlRegister:
                            operand.Value = controlRegisters[operand.Register.Index];
                            break;
                        default:
                            System.Diagnostics.Debugger.Break();
                            break;
                    }
                    break;
                case OperandType.Memory:
                    if (operand.Memory.Base != GeneralRegister.None)
                        operand.Memory.Address = registers[(int)operand.Memory.Base].Word;
                    else
                        operand.Memory.Address = 0;

                    if (operand.Memory.Index != GeneralRegister.None)
                        operand.Memory.Address += registers[(int)operand.Memory.Index].Word;

                    if(operand.Size == 16)
                        operand.Memory.Address = (ushort)(operand.Memory.Address + operand.Memory.Displacement);
                    else
                        operand.Memory.Address = (uint)(operand.Memory.Address + operand.Memory.Displacement);
                    operand.Value = SegRead(operand.Memory.Segment, operand.Memory.Address, (int)operand.Size);
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }

            return operand;
        }

        private Operand[] ProcessOperands()
        {
            Operand[] arguments;
            Operand[] operands = disasm.Operands;

            if (operands.Length == 0)
                return null;

            arguments = new Operand[disasm.NumberOfOperands];
            for (int i = 0; i < disasm.NumberOfOperands; i++)
            {
                arguments[i] = ProcessOperand(operands[i]);
            }

            if (disasm.NumberOfOperands == 2 && arguments[0].Size > 8 && arguments[1].Size == 8 && arguments[1].Type == OperandType.Immediate)
            {
                arguments[1].Size = arguments[0].Size;
                arguments[1].Value = (uint)(int)(sbyte)arguments[1].Value;
            }

            return arguments;
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

        private void SegWrite(SegmentRegister segment, uint offset, uint value, int size)
        {
            uint virtAddr = GetVirtualAddress(segment, offset);

            Memory.Write(virtAddr, value, size);
        }

        private void SegWriteByte(SegmentRegister segment, uint offset, byte value)
        {
            SegWrite(segment, offset, value, 8);
        }

        private void SegWriteWord(SegmentRegister segment, uint offset, ushort value)
        {
            SegWrite(segment, offset, value, 16);
        }

        private void SegWriteDWord(SegmentRegister segment, uint offset, uint value)
        {
            SegWrite(segment, offset, value, 32);
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
            int entrySize = Marshal.SizeOf(typeof(GDTEntry)) - 4;
            var gdtBytes = new byte[entrySize];

            Memory.BlockRead(gdtRegister.Base + selector, gdtBytes, gdtBytes.Length);
            IntPtr p = Marshal.AllocHGlobal(entrySize);
            Marshal.Copy(gdtBytes, 0, p, entrySize);
            var entry = (GDTEntry)Marshal.PtrToStructure(p, typeof(GDTEntry));
            Marshal.FreeHGlobal(p);

            entry.RefreshBase();

            return entry;
        }

        public uint GetSelectorBase(SegmentRegister segment)
        {
            return segments[(int)segment].GDTEntry.BaseAddress;
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

        private void DumpRegisters()
        {
            Logger.Debug(String.Format("AX {0:X4} BX {1:X4} CX {2:X4} DX {3:X4}", AX, BX, CX, DX));
            Logger.Debug(String.Format("SI {0:X4} DI {1:X4} SP {2:X4} BP {3:X4}", SI, DI, SP, BP));
            Logger.Debug(String.Format("CS {0:X4} DS {1:X4} ES {2:X4} SS {3:X4}", CS, DS, ES, SS));
        }

        public void ExecuteInterrupt(byte vector)
        {
            interruptOperand.Value = vector;

            if (Halted)
                Halted = false;

            DumpRegisters();
            opSize = PMode ? 32 : 16;
            addressSize = PMode ? 32 : 16;
            Interrupt(interruptOperand);
            IF = false;
            Fetch(true);
        }

        public void Fetch()
        {
            Fetch(false);
        }

        public void Fetch(bool doStrings)
        {
            if (Halted)
                return;

            CurrentAddr = segments[(int)SegmentRegister.CS].GDTEntry.BaseAddress + EIP;
            OpLen = disasm.Disassemble(CurrentAddr, doStrings);
            opSize = disasm.OperandSize;
            addressSize = disasm.AddressSize;
        }

        public void ReFetch()
        {
            CurrentAddr = segments[(int)SegmentRegister.CS].GDTEntry.BaseAddress + EIP;
            OpLen = disasm.Disassemble(CurrentAddr, true);
        }

        public void Cycle()
        {
            Cycle(false);
        }

        public void Cycle(bool logging)
        {
            if (Halted)
                return;

            Operand[] operands = ProcessOperands();

            if (logging)
                Logger.Info(String.Format("{0:X}:{1:X} {2}", CS, EIP, disasm.InstructionText));

            EIP += (uint)OpLen;
            disasm.Execute(operands);
        }
    }
}
