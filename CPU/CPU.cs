using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace x86CS
{
    public partial class CPU
    {
        private Segment[] segments;
        private Register[] registers;
        private uint[] controlRegisters;
        private CPUFlags eFlags;
        private bool debug = false;
        private bool pMode = false;
        public event ReadCallback IORead;
        public event WriteCallback IOWrite;
        private StreamWriter logFile = File.CreateText("cpulog.txt");
        private TableRegister idtRegister, gdtRegister;
        private GDTEntry realModeEntry;

        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }

        public bool PMode
        {
            get { return pMode; }
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
            set { SetFlag(CPUFlags.OF, value); }
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
            segments = new Segment[6];
            registers = new Register[9];
            controlRegisters = new uint[5];
            idtRegister = new TableRegister();
            gdtRegister = new TableRegister();
            realModeEntry = new GDTEntry();

            realModeEntry.BaseAddress = 0;
            realModeEntry.Is32Bit = false;
            realModeEntry.IsAccessed = true;
            realModeEntry.IsCode = false;
            realModeEntry.Limit = 0xffff;
            realModeEntry.IsWritable = true;

            logFile.AutoFlush = true;

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

        private uint GetVirtualAddress(SegmentRegister segment, uint offset)
        {
            Segment seg = segments[(int)segment];

            return seg.GDTEntry.BaseAddress + offset;
        }

        private byte SegReadByte(SegmentRegister segment, uint offset)
        {
            uint virtAddr;
            byte ret;

            virtAddr = GetVirtualAddress(segment, offset);

            ret = Memory.ReadByte(virtAddr);

            if(segment != SegmentRegister.CS)
                logFile.WriteLine(String.Format("Memory Read Byte {0:X} {1:X}", virtAddr, ret)); 

            return ret;
        }

        private ushort SegReadWord(SegmentRegister segment, uint offset)
        {
            uint virtAddr;
            ushort ret;

            virtAddr = GetVirtualAddress(segment, offset);

            ret = Memory.ReadWord(virtAddr);

            if (segment != SegmentRegister.CS)
                logFile.WriteLine(String.Format("Memory Read Word {0:X} {1:X}", virtAddr, ret));

            return ret;
        }

        private uint SegReadDWord(SegmentRegister segment, uint offset)
        {
            uint virtAddr;
            uint ret;

            virtAddr = GetVirtualAddress(segment, offset);

            ret = Memory.ReadDWord(virtAddr);

            if (segment != SegmentRegister.CS)
                logFile.WriteLine(String.Format("Memory Read DWord {0:X} {1:X}", virtAddr, ret));

            return ret;
        }

        private void SegWriteByte(SegmentRegister segment, uint offset, byte value)
        {
            uint virtAddr;

            virtAddr = GetVirtualAddress(segment, offset);

            logFile.WriteLine(String.Format("Memory Write Byte {0:X8} {1:X2}", virtAddr, value)); 

            Memory.WriteByte(virtAddr, value);
        }

        private void SegWriteWord(SegmentRegister segment, uint offset, ushort value)
        {
            uint virtAddr;

            virtAddr = GetVirtualAddress(segment, offset);

            logFile.WriteLine(String.Format("Memory Write word {0:X} {1:X}", virtAddr, value)); 

            Memory.WriteWord(virtAddr, value);
        }

        private void SegWriteDWord(SegmentRegister segment, uint offset, uint value)
        {
            uint virtAddr;

            virtAddr = GetVirtualAddress(segment, offset);

            logFile.WriteLine(String.Format("Memory Write word {0:X} {1:X}", virtAddr, value));

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

        public void StackPush(ushort value)
        {
            if (opSize == 32)
                ESP -= 2;
            else
                SP -= 2;

            SegWriteWord(SegmentRegister.SS, SP, value);
        }

        public void StackPush(uint value)
        {
            if (opSize == 32)
                ESP -= 4;
            else
                SP -= 2;

            SegWriteDWord(SegmentRegister.SS, opSize == 32 ? ESP : SP, value);
        }

        private string GetByteRegStr(int offset)
        {
            switch (offset)
            {
                case 0x0:
                    return "AL";
                case 0x1:
                    return "CL";
                case 0x2:
                    return "DL";
                case 0x3:
                    return "BL";
                case 0x4:
                    return "AH";
                case 0x5:
                    return "CH";
                case 0x6:
                    return "DH";
                case 0x7:
                    return "BH";
                default:
                    return "";
            }
        }

        private string GetControlRegStr(int offset)
        {
            switch (offset)
            {
                case 0:
                    return "CR0";
                case 1:
                    return "CR1";
                case 2:
                    return "CR2";
                case 3:
                    return "CR3";
                case 4:
                    return "CR4";
            }

            return "";
        }

        private string GetRegStr(int offset)
        {
            string ret = "";

            if (opSize == 32)
                ret = "E";

            switch (offset)
            {
                case 0x0:
                    ret += "AX";
                    break;
                case 0x1:
                    ret += "CX";
                    break;
                case 0x2:
                    ret += "DX";
                    break;
                case 0x3:
                    ret += "BX";
                    break;
                case 0x4:
                    ret += "SP";
                    break;
                case 0x5:
                    ret += "BP";
                    break;
                case 0x6:
                    ret += "SI";
                    break;
                case 0x7:
                    ret += "DI";
                    break;
                default:
                    return "";
            }

            return ret;
        }

        private void SetCPUFlags(byte operand)
        {
            sbyte signed = (sbyte)operand;

            if (operand == 0)
                ZF = true;
            else
                ZF = false;

            if (signed < 0)
                SF = true;
            else
                SF = false;

            SetParity(operand);
        }

        private GDTEntry GetSelectorEntry(uint selector)
        {
            byte[] gdtBytes;
            IntPtr p;
            int entrySize = Marshal.SizeOf(typeof(GDTEntry));
            GDTEntry entry;

            gdtBytes = new byte[entrySize];

            Memory.BlockRead((uint)(gdtRegister.Base + selector), gdtBytes, gdtBytes.Length);
            p = Marshal.AllocHGlobal(entrySize);
            Marshal.Copy(gdtBytes, 0, p, entrySize);
            entry = (GDTEntry)Marshal.PtrToStructure(p, typeof(GDTEntry));
            Marshal.FreeHGlobal(p);

            return entry;
        }

        private void SetSelector(SegmentRegister segment, uint selector)
        {
            if (pMode)
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

        private byte GetByteReg(byte offset)
        {
            byte byteOp = 0;

            switch (offset)
            {
                case 0x00:
                    byteOp = AL;
                    break;
                case 0x01:
                    byteOp = CL;
                    break;
                case 0x2:
                    byteOp = DL;
                    break;
                case 0x3:
                    byteOp = BL;
                    break;
                case 0x4:
                    byteOp = AH;
                    break;
                case 0x5:
                    byteOp = CH;
                    break;
                case 0x6:
                    byteOp = DH;
                    break;
                case 0x7:
                    byteOp = BH;
                    break;
            }

            return byteOp;
        }

        private void SetByteReg(byte offset, byte byteOp)
        {
            switch (offset)
            {
                case 0x00:
                    AL = byteOp;
                    break;
                case 0x01:
                    CL = byteOp;
                    break;
                case 0x2:
                    DL = byteOp;
                    break;
                case 0x3:
                    BL = byteOp;
                    break;
                case 0x4:
                    AH = byteOp;
                    break;
                case 0x5:
                    CH = byteOp;
                    break;
                case 0x6:
                    DH = byteOp;
                    break;
                case 0x7:
                    BH = byteOp;
                    break;
            }
        }

        private byte DoIORead(byte addr)
        {
            return (byte)(DoIORead((ushort)(addr & 0x00ff)) & 0x00ff);
        }

        private ushort DoIORead(ushort addr)
        {
            ReadCallback ioRead = IORead;

            if (ioRead != null)
                return ioRead(addr);

            return 0xffff;
        }

        private void DoIOWrite(byte addr, byte value)
        {
            DoIOWrite((ushort)(addr & 0x00ff), (ushort)(value & 0x00ff));
        }

        private void DoIOWrite(ushort addr, ushort value)
        {
            WriteCallback ioWrite = IOWrite;

            if (ioWrite != null)
                ioWrite(addr, value);
        }

        private uint GetRegMemAddr(RegMemData rmData, out SegmentRegister segToUse)
        {
            uint address = 0;
            segToUse = SegmentRegister.DS;

            switch (rmData.RegMem)
            {
                case 0:
                    if (opSize == 32)
                        address = EAX;
                    else
                        address = (uint)(BX + SI);
                    break;
                case 1:
                    if (opSize == 32)
                        address = ECX;
                    else
                        address = (uint)(BX + DI);
                    break;
                case 2:
                    if (opSize == 32)
                        address = EDX;
                    else
                    {
                        address = (uint)(BP + SI);
                        segToUse = SegmentRegister.SS;
                    }
                    break;
                case 3:
                    if (opSize == 32)
                        address = EBX;
                    else
                    {
                        address = (uint)(BP + DI);
                        segToUse = SegmentRegister.SS;
                    }
                    break;
                case 4:
                    if (opSize == 32)
                    {
                        if (rmData.Base != 5)
                        {
                            if (rmData.Index != 4)
                                address = registers[rmData.Index].DWord * rmData.Scale + registers[rmData.Base].DWord;
                            else
                                address = registers[rmData.Base].DWord;
                        }
                        else
                        {
                        }
                    }
                    else
                        address = SI;
                    break;
                case 5:
                    if (opSize == 32)
                        address = 0;
                    else
                        address = DI;
                    break;
                case 6:
                    if (opSize == 32)
                        address = ESI;
                    else
                    {
                        if (rmData.Mode == 0)
                            address = 0;
                        else
                        {
                            address = BP;
                            segToUse = SegmentRegister.SS;
                        }
                    }
                    break;
                case 7:
                    if (opSize == 32)
                        address = EDI;
                    else
                        address = BX;
                    break;
            }
            return address;
        }

        private uint ProcessRegMem(RegMemData rmData, out byte registerValue, out byte regMemValue)
        {
            uint address = 0;
            SegmentRegister segToUse = SegmentRegister.DS;

            registerValue = GetByteReg(rmData.Register);

            if (rmData.IsRegister)
                regMemValue = GetByteReg(rmData.RegMem);
            else
            {
                address = GetRegMemAddr(rmData, out segToUse);
                if (rmData.HasDisplacement)
                    address += (ushort)rmData.Displacement;

                if (segToUse != overrideSegment)
                    segToUse = overrideSegment;

                regMemValue = SegReadByte(segToUse, address);
            }
            return address;
        }

        private uint ProcessRegMem(RegMemData rmData, out ushort registerValue, out ushort regMemValue)
        {
            uint address = 0;
            SegmentRegister segToUse = SegmentRegister.DS;

            registerValue = registers[rmData.Register].Word;

            if (rmData.IsRegister)
                regMemValue = registers[rmData.RegMem].Word;
            else
            {
                address = GetRegMemAddr(rmData, out segToUse);
                if (rmData.HasDisplacement)
                    address += (uint)(int)rmData.Displacement;

                if (segToUse != overrideSegment && segToUse != SegmentRegister.SS)
                    segToUse = overrideSegment;

                regMemValue = SegReadWord(segToUse, address);
            }

            return address;
        }

        private uint ProcessRegMem(RegMemData rmData, out uint registerValue, out uint regMemValue)
        {
            uint address = 0;
            SegmentRegister segToUse = SegmentRegister.DS;

            registerValue = registers[rmData.Register].DWord;

            if (rmData.IsRegister)
                regMemValue = registers[rmData.RegMem].DWord;
            else
            {
                address = GetRegMemAddr(rmData, out segToUse);
                if (rmData.HasDisplacement)
                    address += (uint)rmData.Displacement;

                if (segToUse != overrideSegment)
                    segToUse = overrideSegment;

                regMemValue = SegReadDWord(segToUse, address);
            }

            return address;
        }

        private void WriteRegMem(RegMemData rmData, uint address, byte value)
        {
            SegmentRegister writeSegment = SegmentRegister.DS;

            if (rmData.IsRegister)
                SetByteReg(rmData.RegMem, value);
            else
            {
                if (rmData.RegMem == 2 || rmData.RegMem == 3)
                {
                    if (overrideSegment != SegmentRegister.SS)
                        writeSegment = overrideSegment;
                    else
                        writeSegment = SegmentRegister.SS;
                }
                else
                {
                    if (overrideSegment != SegmentRegister.DS)
                        writeSegment = overrideSegment;
                }
                SegWriteByte(writeSegment, address, value);
            }
        }

        private void WriteRegMem(RegMemData rmData, uint address, ushort value)
        {
            SegmentRegister writeSegment = SegmentRegister.DS;

            if (rmData.IsRegister)
                registers[rmData.RegMem].Word = value;
            else
            {
                if (rmData.RegMem == 2 || rmData.RegMem == 3)
                {
                    if (overrideSegment != SegmentRegister.SS)
                        writeSegment = overrideSegment;
                    else
                        writeSegment = SegmentRegister.SS;
                }
                else
                {
                    if (overrideSegment != SegmentRegister.DS)
                        writeSegment = overrideSegment;
                }
                SegWriteWord(writeSegment, address, value);
            }
        }

        private void WriteRegMem(RegMemData rmData, uint address, uint value)
        {
            SegmentRegister writeSegment = SegmentRegister.DS;

            if (rmData.IsRegister)
                registers[rmData.RegMem].DWord = value;
            else
            {
                if (rmData.RegMem == 2 || rmData.RegMem == 3)
                {
                    if (overrideSegment != SegmentRegister.SS)
                        writeSegment = overrideSegment;
                    else
                        writeSegment = SegmentRegister.SS;
                }
                else
                {
                    if (overrideSegment != SegmentRegister.DS)
                        writeSegment = overrideSegment;
                }
                SegWriteDWord(writeSegment, address, value);
            }
        }

        private void CallRegMem(RegMemData rmData, uint address, bool far, bool call)
        {
            SegmentRegister readSegment = SegmentRegister.DS;
            uint segment;
            uint offset;

            if (rmData.IsRegister)
            {
                if (opSize == 32)
                    offset = registers[rmData.RegMem].DWord;
                else
                    offset = registers[rmData.RegMem].Word;
            }
            else
            {
                if (rmData.RegMem == 2 || rmData.RegMem == 3)
                {
                    if (overrideSegment != SegmentRegister.SS)
                        readSegment = overrideSegment;
                    else
                        readSegment = SegmentRegister.SS;
                }
                else
                {
                    if (overrideSegment != SegmentRegister.DS)
                        readSegment = overrideSegment;
                }

                if (opSize == 32)
                    offset = SegReadDWord(readSegment, address);
                else
                    offset = SegReadWord(readSegment, address);
            }


            if (far)
                segment = SegReadWord(readSegment, (uint)(address + 2));
            else
                segment = CS;

            if (call)
                CallProcedure(segment, offset, false);
            else
            {
                CS = segment;
                EIP = offset;
            }
        }

        private void CallProcedure(uint segment, uint offset, bool relative)
        {
            if (segment == CS)
            {
                if (opSize == 32)
                    StackPush(EIP);
                else
                    StackPush(IP);

                if (relative)
                    EIP += offset;
                else
                    EIP = offset;
            }
            else
            {
                StackPush(CS);
                if (opSize == 32)
                    StackPush(EIP);
                else
                    StackPush(IP);

                CS = segment;
                if (relative)
                    EIP += offset;
                else
                    EIP = offset;
            }
        }

        private void ProcedureEnter(ushort size, byte level)
        {
            ushort nestingLevel = (ushort)(level % 32);
            ushort frameTemp;

            StackPush(BP);
            frameTemp = SP;

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

            if (pMode == false && ((CR0 & 0x1) == 0x1))
                pMode = true;
            else if (pMode == true && ((CR0 & 0x1) == 0))
                pMode = false;

            if (segment == CS)
            {
                if (relative)
                {
                    int relOffset = (int)offset;

                    tempEIP = (uint)(EIP + relOffset);
                }
                else
                    tempEIP = offset;

                if (tempEIP > codeSegment.GDTEntry.Limit)
                    throw new Exception("General Fault Code 0");
            }
            else
            {
                if (pMode)
                {
                    GDTEntry newEntry;

                    if (segment == 0)
                        throw new Exception("Null segment selector");

                    if (segment > (gdtRegister.Limit))
                        throw new Exception("Selector out of range");

                    newEntry = GetSelectorEntry(segment);

                    if (!newEntry.IsCode)
                        throw new Exception("Segment is not code");

                    CS = segment;
                    if (relative)
                    {
                        int relOffset = (int)offset;

                        tempEIP = (uint)(EIP + relOffset);
                    }
                    else
                        tempEIP = offset;                    
                }
                else
                {
                    tempEIP = EIP + offset;
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
            StackPush((ushort)Flags);
            IF = false;
            TF = false;
            AC = false;
            StackPush(CS);
            StackPush(IP);

            CS = Memory.ReadWord((uint)(vector * 4) + 2);
            EIP = Memory.ReadWord((uint)(vector * 4));
        }

        public void Cycle(int len, byte opCode, object[] operands)
        {
            byte destByte, sourceByte, tempByte;
            ushort destWord, sourceWord, tempWord;
            uint destDWord, sourceDWord, tempDWord;
            ulong tempQWord;
            uint memAddress;
            uint offset = 0;
            sbyte signedByte;
            short signedWord;
            int signedDWord;
            RegMemData rmData = null;
             
            EIP += (ushort)len;

            if (operands.Length > 0)
            {
                rmData = operands[0] as RegMemData;

                if (operands[0] is byte)
                {
                    signedByte = (sbyte)(byte)operands[0];
                    offset = (ushort)(EIP + signedByte);
                }
                else if (operands[0] is ushort)
                {
                    signedWord = (short)(ushort)operands[0];
                    offset = (ushort)(EIP + signedWord);
                }
                else if (operands[0] is uint)
                {
                    signedDWord = (int)(uint)operands[0];
                    offset = (uint)(EIP + signedDWord);
                }
            }

            if (extPrefix)
            {  
                switch (opCode)
                {
                    case 0x01:
                        memAddress = ProcessRegMem(rmData, out tempDWord, out sourceDWord);
                        switch (rmData.Register)
                        {
                            case 2:
                                sourceWord = SegReadWord(overrideSegment, memAddress);
                                sourceDWord = SegReadDWord(overrideSegment, (uint)(memAddress + 2));
                                if (opSize == 32)
                                {
                                    gdtRegister.Limit = sourceWord;
                                    gdtRegister.Base = sourceDWord;
                                }
                                else
                                {
                                    gdtRegister.Limit = (ushort)sourceWord;
                                    gdtRegister.Base = (sourceDWord & 0x00ffffff);
                                }
                                break;
                            case 3:
                                sourceWord = SegReadWord(overrideSegment, memAddress);
                                sourceDWord = SegReadDWord(overrideSegment, (uint)(memAddress + 2));
                                if (opSize == 32)
                                {
                                    idtRegister.Limit = sourceWord;
                                    idtRegister.Base = sourceDWord;
                                }
                                else
                                {
                                    idtRegister.Limit = (ushort)sourceWord;
                                    idtRegister.Base = (sourceDWord & 0x00ffffff);
                                }
                                break;
                        }
                        break;
                    case 0x20:
                        registers[rmData.Register].DWord = controlRegisters[rmData.RegMem];
                        break;
                    case 0x22:
                        controlRegisters[rmData.RegMem] = registers[rmData.Register].DWord;
                        break;
                    case 0x80:
                        if (OF)
                            EIP = offset;
                        break;
                    case 0x81:
                        if (!OF)
                            EIP = offset;
                        break;
                    case 0x82:
                        if (CF)
                            EIP = offset;
                        break;
                    case 0x83:
                        if (!CF)
                            EIP = offset;
                        break;
                    case 0x84:
                        if (ZF)
                            EIP = offset;
                        break;
                    case 0x85:
                        if (!ZF)
                            EIP = offset;
                        break;
                    case 0x86:
                        if (CF || ZF)
                            EIP = offset;
                        break;
                    case 0x87:
                        if (!CF && !ZF)
                            EIP = offset;
                        break;
                    case 0x88:
                        if (SF)
                            EIP = offset;
                        break;
                    case 0x89:
                        if (!SF)
                            EIP = offset;
                        break;
                    case 0x8a:
                        if (PF)
                            EIP = offset;
                        break;
                    case 0x8b:
                        if (!PF)
                            EIP = offset;
                        break;
                    case 0x8c:
                        if (SF != OF)
                            EIP = offset;
                        break;
                    case 0x8d:
                        if (SF == OF)
                            EIP = offset;
                        break;
                    case 0x8e:
                        if (ZF || (SF != OF))
                            EIP = offset;
                        break;
                    case 0x8f:
                        if (!ZF && SF == OF)
                            EIP = offset;
                        break;
                }
            }
            else
            {
                switch (opCode)
                {
                    #region Add with carry
                    case 0x10:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = AddWithCarry(destByte, sourceByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x11:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = AddWithCarry(destDWord, sourceDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = AddWithCarry(destWord, sourceWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x12:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = AddWithCarry(destByte, sourceByte);
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x13:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = AddWithCarry(destDWord, sourceDWord);
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = AddWithCarry(destWord, sourceWord);
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x14:
                        AddWithCarry((byte)operands[0]);
                        break;
                    case 0x15:
                        if (opSize == 32)
                            AddWithCarry((uint)operands[0]);
                        else
                            AddWithCarry((ushort)operands[0]);
                        break;
                    #endregion
                    #region Add
                    case 0x00:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = Add(destByte, sourceByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x01:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = Add(destDWord, sourceDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = Add(destWord, sourceWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x02:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = Add(destByte, sourceByte);
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x03:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = Add(destDWord, sourceDWord);
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = Add(destWord, sourceWord);
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x04:
                        Add((byte)operands[0]);
                        break;
                    case 0x05:
                        if(opSize == 32)
                            Add((uint)operands[0]);
                        else
                            Add((ushort)operands[0]);
                        break;
                    #endregion
                    #region Sub With Borrow
                    case 0x18:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = SubWithBorrow(destByte, sourceByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x19:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = SubWithBorrow(destDWord, sourceDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = SubWithBorrow(destWord, sourceWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x1a:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = SubWithBorrow(destByte, sourceByte);
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x1b:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = SubWithBorrow(destDWord, sourceDWord);
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = SubWithBorrow(destWord, sourceWord);
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x1c:
                        SubWithBorrow((byte)operands[0]);
                        break;
                    case 0x1d:
                        SubWithBorrow((ushort)operands[0]);
                        break;
                    #endregion
                    #region Sub
                    case 0x28:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = Subtract(destByte, sourceByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x29:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = Subtract(destDWord, sourceDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = Subtract(destWord, sourceWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x2a:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = Subtract(destByte, sourceByte);
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x2b:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = Subtract(destDWord, sourceDWord);
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = Subtract(destWord, sourceWord);
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x2c:
                        Subtract((byte)operands[0]);
                        break;
                    case 0x2d:
                        if (opSize == 32)
                            Subtract((uint)operands[0]);
                        else
                            Subtract((ushort)operands[0]);
                        break;
                    #endregion
                    #region And
                    case 0x20:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = And(destByte, sourceByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x21:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = And(destDWord, sourceDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = And(destWord, sourceWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x22:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = And(destByte, sourceByte);
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x23:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = And(destDWord, sourceDWord);
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = And(destWord, sourceWord);
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x24:
                        AL = And((byte)operands[0]);
                        break;
                    case 0x25:
                        if (opSize == 32)
                            EAX = And((uint)operands[0]);
                        else
                            AX = And((ushort)operands[0]);
                        break;
                    #endregion
                    #region Or
                    case 0x08:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = Or(destByte, sourceByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x09:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = Or(destDWord, sourceDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = Or(destWord, sourceWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x0a:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = Or(destByte, sourceByte);
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x0b:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = Or(destDWord, sourceDWord);
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = Or(destWord, sourceWord);
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x0c:
                        Or((byte)operands[0]);
                        break;
                    case 0x0d:
                        if (opSize == 32)
                            Or((uint)operands[0]);
                        else
                            Or((ushort)operands[0]);
                        break;
                    #endregion
                    #region Xor
                    case 0x30:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = Xor(destByte, sourceByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x31:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = Xor(destDWord, sourceDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = Xor(destWord, sourceWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x32:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = Xor(destByte, sourceByte);
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x33:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = Xor(destDWord, sourceDWord);
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = Xor(destWord, sourceWord);
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x34:
                        Xor((byte)operands[0]);
                        break;
                    case 0x35:
                        if (opSize == 32)
                            Xor((uint)operands[0]);
                        else
                            Xor((ushort)operands[0]);
                        break;
                    #endregion
                    #region Compare
                    case 0x38:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = Subtract(destByte, sourceByte);
                        break;
                    case 0x39:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = Subtract(destDWord, sourceDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = Subtract(destWord, sourceWord);
                        }
                        break;
                    case 0x3a:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = Subtract(destByte, sourceByte);
                        break;
                    case 0x3b:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = Subtract(destDWord, sourceDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = Subtract(destWord, sourceWord);
                        }
                        break;
                    case 0x3c:
                        DoSub(AL, (byte)operands[0], false);
                        break;
                    case 0x3d:
                        if (opSize == 32)
                            DoSub(EAX, (uint)operands[0], false);
                        else
                            DoSub(AX, (ushort)operands[0], false);
                        break;
                    #endregion
                    #region Test
                    case 0x84:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        And(destByte, sourceByte);
                        break;
                    case 0x85:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            And(destDWord, sourceDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            And(destWord, sourceWord);
                        }
                        break;
                    case 0xa8:
                        DoAnd(AL, (byte)operands[0]);
                        break;
                    case 0xa9:
                        if (opSize == 32)
                            DoAnd(EAX, (uint)operands[0]);
                        else
                            DoAnd(AX, (ushort)operands[0]);
                        break;
                    #endregion
                    #region Move
                    case 0x88:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        destByte = sourceByte;
                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x89:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            destDWord = sourceDWord;
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            destWord = sourceWord;
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x8a:
                        memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                        destByte = sourceByte;
                        SetByteReg(rmData.Register, destByte);
                        break;
                    case 0x8b:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            destDWord = sourceDWord;
                            registers[rmData.Register].DWord = destDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            destWord = sourceWord;
                            registers[rmData.Register].Word = destWord;
                        }
                        break;
                    case 0x8c:
                        memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                        WriteRegMem(rmData, memAddress, (ushort)segments[rmData.Register].Selector);
                        break;
                    case 0x8e:
                        memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                        SetSelector((SegmentRegister)rmData.Register, sourceWord);
                        break;
                    case 0xa0:
                        sourceByte = (byte)operands[0];
                        AL = SegReadByte(overrideSegment, sourceByte);
                        break;
                    case 0xa1:
                        if (opSize == 32)
                        {
                            sourceDWord = (uint)operands[0];
                            EAX = SegReadDWord(overrideSegment, sourceDWord);
                        }
                        else
                        {
                            sourceWord = (ushort)operands[0];
                            AX = SegReadWord(overrideSegment, sourceWord);
                        }
                        break;
                    case 0xa2:
                        sourceByte = (byte)operands[0];
                        SegWriteByte(overrideSegment, sourceByte, AL);
                        break;
                    case 0xa3:
                        if (opSize == 32)
                        {
                            sourceDWord = (uint)operands[0];
                            SegWriteDWord(overrideSegment, sourceDWord, EAX);
                        }
                        else
                        {
                            sourceWord = (ushort)operands[0];
                            SegWriteWord(overrideSegment, sourceWord, AX);
                        }
                        break;
                    case 0xb0:
                    case 0xb1:
                    case 0xb2:
                    case 0xb3:
                    case 0xb4:
                    case 0xb5:
                    case 0xb6:
                    case 0xb7:
                        SetByteReg((byte)(opCode - 0xb0), (byte)operands[0]);
                        break;
                    case 0xb8:
                    case 0xb9:
                    case 0xba:
                    case 0xbb:
                    case 0xbc:
                    case 0xbd:
                    case 0xbe:
                    case 0xbf:
                        if (opSize == 32)
                            registers[opCode - 0xb8].DWord = (uint)operands[0];
                        else
                            registers[opCode - 0xb8].Word = (ushort)operands[0];
                        break;
                    case 0xc6:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        WriteRegMem(rmData, memAddress, (byte)operands[1]);
                        break;
                    case 0xc7:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            WriteRegMem(rmData, memAddress, (uint)operands[1]);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            WriteRegMem(rmData, memAddress, (ushort)operands[1]);
                        }
                        break;
                    #endregion
                    #region Exchange
                    case 0x86:
                        memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                        WriteRegMem(rmData, memAddress, destByte);
                        SetByteReg(rmData.Register, sourceByte);
                        break;
                    case 0x87:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                            registers[rmData.Register].DWord = sourceDWord;
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            WriteRegMem(rmData, memAddress, destWord);
                            registers[rmData.Register].Word = sourceWord;
                        }
                        break;
                    case 0x90:
                        break;
                    case 0x91:
                    case 0x92:
                    case 0x93:
                    case 0x94:
                    case 0x95:
                    case 0x96:
                    case 0x97:
                        if (opSize == 32)
                        {
                            destDWord = registers[opCode - 0x90].DWord;
                            registers[opCode - 0x90].DWord = EAX;
                            EAX = destDWord;
                        }
                        else
                        {
                            destWord = registers[opCode - 0x90].Word;
                            registers[opCode - 0x90].Word = AX;
                            AX = destWord;
                        }
                        break;
                    #endregion
                    #region Call Procedure
                    case 0x9a:
                        CallProcedure((ushort)operands[1], (ushort)operands[0], false);
                        break;
                    case 0xe8:
                        CallProcedure(CS, offset, false);
                        break;
                    #endregion
                    #region BCD
                    case 0x27:
                        DecAdjustAfterAddition();
                        break;
                    case 0x37:
                        ASCIIAdjustAfterAdd();
                        break;
                    case 0x2f:
                        DecAdjustAfterSubtract();
                        break;
                    case 0x3f:
                        ASCIIAdjustAfterSubtract();
                        break;
                    case 0xd4:
                        ASCIIAdjustAfterMultiply((byte)operands[0]);
                        break;
                    case 0xd5:
                        ASCIIAdjustAfterDivide((byte)operands[0]);
                        break;
                    #endregion
                    #region Inc/Dec
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x46:
                    case 0x47:
                        if(opSize == 32)
                            registers[opCode - 0x40].DWord = Increment(registers[opCode - 0x40].DWord);
                        else
                            registers[opCode - 0x40].Word = Increment(registers[opCode - 0x40].Word);
                        break;
                    case 0x48:
                    case 0x49:
                    case 0x4a:
                    case 0x4b:
                    case 0x4c:
                    case 0x4d:
                    case 0x4e:
                    case 0x4f:
                        if(opSize == 32)
                            registers[opCode - 0x48].DWord = Decrement(registers[opCode - 0x48].DWord);
                        else
                            registers[opCode - 0x48].Word = Decrement(registers[opCode - 0x48].Word);
                        break;
                    #endregion
                    #region Multiply
                    case 0x69:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            tempDWord = (uint)operands[1];

                            destDWord = SignedMultiply(sourceDWord, tempDWord);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            tempWord = (ushort)operands[1];

                            destWord = SignedMultiply(sourceWord, tempWord);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x6b:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out destDWord, out sourceDWord);
                            sourceByte = (byte)operands[1];

                            destDWord = SignedMultiply(sourceDWord, sourceByte);
                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                            sourceByte = (byte)operands[1];

                            destWord = SignedMultiply(sourceWord, sourceByte);
                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    #endregion
                    #region String ops
                    case 0x6c:
                        StringInByte();
                        break;
                    case 0x6d:
                        if (opSize == 32)
                            StringInDWord();
                        else
                            StringInWord();
                        break;
                    case 0x6e:
                        StringOutByte();
                        break;
                    case 0x6f:
                        if (opSize == 32)
                            StringOutDWord();
                        else
                            StringOutWord();
                        break;
                    case 0xa4:
                        StringCopyByte();
                        break;
                    case 0xa5:
                        if (opSize == 32)
                            StringCopyDWord();
                        else
                            StringCopyWord();
                        break;
                    case 0xaa:
                        StringWriteByte();
                        break;
                    case 0xab:
                        if(opSize == 32)
                            StringWriteDWord();
                        else
                            StringWriteWord();
                        break;
                    case 0xac:
                        StringReadByte();
                        break;
                    case 0xad:
                        if (opSize == 32)
                            StringReadDWord();
                        else
                            StringReadWord();
                        break;
                    case 0xae:
                        StringScanByte();
                        break;
                    case 0xaf:
                        if (opSize == 32)
                            StringScanDWord();
                        else
                            StringScanWord();
                        break;
                    #endregion
                    #region Jumps
                    case 0x70:
                        if (OF)
                            EIP = offset;
                        break;
                    case 0x71:
                        if (!OF)
                            EIP = offset;
                        break;
                    case 0x72:
                        if (CF)
                            EIP = offset;
                        break;
                    case 0x73:
                        if (!CF)
                            EIP = offset;
                        break;
                    case 0x74:
                        if (ZF)
                            EIP = offset;
                        break;
                    case 0x75:
                        if (!ZF)
                            EIP = offset;
                        break;
                    case 0x76:
                        if (CF || ZF)
                            EIP = offset;
                        break;
                    case 0x77:
                        if (!CF && !ZF)
                            EIP = offset;
                        break;
                    case 0x78:
                        if (SF)
                            EIP = offset;
                        break;
                    case 0x79:
                        if (!SF)
                            EIP = offset;
                        break;
                    case 0x7a:
                        if (PF)
                            EIP = offset;
                        break;
                    case 0x7b:
                        if (!PF)
                            EIP = offset;
                        break;
                    case 0x7c:
                        if (SF != OF)
                            EIP = offset;
                        break;
                    case 0x7d:
                        if (SF == OF)
                            EIP = offset;
                        break;
                    case 0x7e:
                        if (ZF || (SF != OF))
                            EIP = offset;
                        break;
                    case 0x7f:
                        if (!ZF && SF == OF)
                            EIP = offset;
                        break;
                    case 0xe3:
                        if (CX == 0)
                            EIP = offset;
                        break;
                    #endregion
                    #region Stack ops
                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x54:
                    case 0x55:
                    case 0x56:
                    case 0x57:
                        if(opSize == 32)
                            StackPush(registers[opCode - 0x50].DWord);
                        else
                            StackPush(registers[opCode - 0x50].Word);
                        break;
                    case 0x58:
                    case 0x59:
                    case 0x5a:
                    case 0x5b:
                    case 0x5c:
                    case 0x5d:
                    case 0x5e:
                    case 0x5f:
                        if(opSize == 32)
                            registers[opCode - 0x58].DWord = StackPop();
                        else
                            registers[opCode - 0x58].Word = (ushort)StackPop();
                        break;
                    case 0x60:
                        if (opSize == 32)
                        {
                            tempDWord = SegReadDWord(SegmentRegister.SS, ESP);
                            StackPush(EAX);
                            StackPush(ECX);
                            StackPush(EDX);
                            StackPush(EBX);
                            StackPush(tempDWord);
                            StackPush(EBP);
                            StackPush(ESI);
                            StackPush(EDI);
                        }
                        else
                        {
                            tempWord = SegReadWord(SegmentRegister.SS, SP);
                            StackPush(AX);
                            StackPush(CX);
                            StackPush(DX);
                            StackPush(BX);
                            StackPush(tempWord);
                            StackPush(BP);
                            StackPush(SI);
                            StackPush(DI);
                        }
                        break;
                    case 0x61:
                        if (opSize == 32)
                        {
                            EDI = StackPop();
                            ESI = StackPop();
                            EBP = StackPop();
                            ESP += 4;
                            EBX = StackPop();
                            EDX = StackPop();
                            ECX = StackPop();
                            EAX = StackPop();
                        }
                        else
                        {
                            DI = (ushort)StackPop();
                            SI = (ushort)StackPop();
                            BP = (ushort)StackPop();
                            SP += 2;
                            BX = (ushort)StackPop();
                            DX = (ushort)StackPop();
                            CX = (ushort)StackPop();
                            AX = (ushort)StackPop();
                        }
                        break;
                    case 0x06:
                        StackPush(ES);
                        break;
                    case 0x07:
                        ES = (ushort)StackPop();
                        break;
                    case 0x16:
                        StackPush(SS);
                        break;
                    case 0x17:
                        SS = (ushort)StackPop();
                        break;
                    case 0x0e:
                        StackPush(CS);
                        break;
                    case 0x1e:
                        StackPush(DS);
                        break;
                    case 0x1f:
                        DS = (ushort)StackPop();
                        break;
                    case 0x6a:
                        StackPush((ushort)(byte)operands[0]);
                        break;
                    case 0x68:
                        if (opSize == 32)
                            StackPush((uint)operands[0]);
                        else
                            StackPush((ushort)operands[0]);
                        break;
                    case 0x8f:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            sourceDWord = StackPop();
                            WriteRegMem(rmData, memAddress, sourceDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            sourceWord = (ushort)StackPop();
                            WriteRegMem(rmData, memAddress, sourceWord);
                        }
                        break;
                    case 0x9c:
                        if (opSize == 32)
                            StackPush((uint)eFlags);
                        else
                            StackPush((ushort)Flags);
                        break;
                    case 0x9d:
                        if (opSize == 32)
                            eFlags = (CPUFlags)StackPop();
                        else
                            eFlags = (CPUFlags)(ushort)StackPop();
                        break;
                    #endregion
                    #region Rotate/shift
                    case 0xc0:
                        memAddress = ProcessRegMem(rmData, out tempByte, out destByte);
                        sourceByte = (byte)operands[1];

                        switch (rmData.Register)
                        {
                            case 0:
                                destByte = Rotate(destByte, sourceByte, RotateType.Left);
                                break;
                            case 1:
                                destByte = Rotate(destByte, sourceByte, RotateType.Right);
                                break;
                            case 2:
                                destByte = Rotate(destByte, sourceByte, RotateType.LeftWithCarry);
                                break;
                            case 3:
                                destByte = Rotate(destByte, sourceByte, RotateType.RightWithCarry);
                                break;
                            case 4:
                            case 6:
                                destByte = Shift(destByte, sourceByte, ShiftType.Left);
                                break;
                            case 5:
                                destByte = Shift(destByte, sourceByte, ShiftType.Right);
                                break;
                            case 7:
                                destByte = Shift(destByte, sourceByte, ShiftType.ArithmaticRight);
                                break;
                        }

                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0xc1:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out tempDWord, out destDWord);
                            sourceDWord = (uint)operands[1];

                            switch (rmData.Register)
                            {
                                case 0:
                                    destDWord = Rotate(destDWord, sourceDWord, RotateType.Left);
                                    break;
                                case 1:
                                    destDWord = Rotate(destDWord, sourceDWord, RotateType.Right);
                                    break;
                                case 2:
                                    destDWord = Rotate(destDWord, sourceDWord, RotateType.LeftWithCarry);
                                    break;
                                case 3:
                                    destDWord = Rotate(destDWord, sourceDWord, RotateType.RightWithCarry);
                                    break;
                                case 4:
                                case 6:
                                    destDWord = Shift(destDWord, sourceDWord, ShiftType.Left);
                                    break;
                                case 5:
                                    destDWord = Shift(destDWord, sourceDWord, ShiftType.Right);
                                    break;
                                case 7:
                                    destDWord = Shift(destDWord, sourceDWord, ShiftType.ArithmaticRight);
                                    break;
                            }

                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out tempWord, out destWord);
                            sourceWord = (ushort)operands[1];

                            switch (rmData.Register)
                            {
                                case 0:
                                    destWord = Rotate(destWord, sourceWord, RotateType.Left);
                                    break;
                                case 1:
                                    destWord = Rotate(destWord, sourceWord, RotateType.Right);
                                    break;
                                case 2:
                                    destWord = Rotate(destWord, sourceWord, RotateType.LeftWithCarry);
                                    break;
                                case 3:
                                    destWord = Rotate(destWord, sourceWord, RotateType.RightWithCarry);
                                    break;
                                case 4:
                                case 6:
                                    destWord = Shift(destWord, sourceWord, ShiftType.Left);
                                    break;
                                case 5:
                                    destWord = Shift(destWord, sourceWord, ShiftType.Right);
                                    break;
                                case 7:
                                    destWord = Shift(destWord, sourceWord, ShiftType.ArithmaticRight);
                                    break;
                            }

                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0xd0:
                        memAddress = ProcessRegMem(rmData, out tempByte, out destByte);

                        switch (rmData.Register)
                        {
                            case 0:
                                destByte = Rotate(destByte, 1, RotateType.Left);
                                break;
                            case 1:
                                destByte = Rotate(destByte, 1, RotateType.Right);
                                break;
                            case 2:
                                destByte = Rotate(destByte, 1, RotateType.LeftWithCarry);
                                break;
                            case 3:
                                destByte = Rotate(destByte, 1, RotateType.RightWithCarry);
                                break;
                            case 4:
                            case 6:
                                destByte = Shift(destByte, 1, ShiftType.Left);
                                break;
                            case 5:
                                destByte = Shift(destByte, 1, ShiftType.Right);
                                break;
                            case 7:
                                destByte = Shift(destByte, 1, ShiftType.ArithmaticRight);
                                break;
                        }

                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0xd1:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out tempDWord, out destDWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    destDWord = Rotate(destDWord, 1, RotateType.Left);
                                    break;
                                case 1:
                                    destDWord = Rotate(destDWord, 1, RotateType.Right);
                                    break;
                                case 2:
                                    destDWord = Rotate(destDWord, 1, RotateType.LeftWithCarry);
                                    break;
                                case 3:
                                    destDWord = Rotate(destDWord, 1, RotateType.RightWithCarry);
                                    break;
                                case 4:
                                case 6:
                                    destDWord = Shift(destDWord, 1, ShiftType.Left);
                                    break;
                                case 5:
                                    destDWord = Shift(destDWord, 1, ShiftType.Right);
                                    break;
                                case 7:
                                    destDWord = Shift(destDWord, 1, ShiftType.ArithmaticRight);
                                    break;
                            }

                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out tempWord, out destWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    destWord = Rotate(destWord, 1, RotateType.Left);
                                    break;
                                case 1:
                                    destWord = Rotate(destWord, 1, RotateType.Right);
                                    break;
                                case 2:
                                    destWord = Rotate(destWord, 1, RotateType.LeftWithCarry);
                                    break;
                                case 3:
                                    destWord = Rotate(destWord, 1, RotateType.RightWithCarry);
                                    break;
                                case 4:
                                case 6:
                                    destWord = Shift(destWord, 1, ShiftType.Left);
                                    break;
                                case 5:
                                    destWord = Shift(destWord, 1, ShiftType.Right);
                                    break;
                                case 7:
                                    destWord = Shift(destWord, 1, ShiftType.ArithmaticRight);
                                    break;
                            }

                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0xd2:
                        memAddress = ProcessRegMem(rmData, out tempByte, out destByte);

                        switch (rmData.Register)
                        {
                            case 0:
                                destByte = Rotate(destByte, CL, RotateType.Left);
                                break;
                            case 1:
                                destByte = Rotate(destByte, CL, RotateType.Right);
                                break;
                            case 2:
                                destByte = Rotate(destByte, CL, RotateType.LeftWithCarry);
                                break;
                            case 3:
                                destByte = Rotate(destByte, CL, RotateType.RightWithCarry);
                                break;
                            case 4:
                            case 6:
                                destByte = Shift(destByte, CL, ShiftType.Left);
                                break;
                            case 5:
                                destByte = Shift(destByte, CL, ShiftType.Right);
                                break;
                            case 7:
                                destByte = Shift(destByte, CL, ShiftType.ArithmaticRight);
                                break;
                        }

                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0xd3:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out tempDWord, out destDWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    destDWord = Rotate(destDWord, CL, RotateType.Left);
                                    break;
                                case 1:
                                    destDWord = Rotate(destDWord, CL, RotateType.Right);
                                    break;
                                case 2:
                                    destDWord = Rotate(destDWord, CL, RotateType.LeftWithCarry);
                                    break;
                                case 3:
                                    destDWord = Rotate(destDWord, CL, RotateType.RightWithCarry);
                                    break;
                                case 4:
                                case 6:
                                    destDWord = Shift(destDWord, CL, ShiftType.Left);
                                    break;
                                case 5:
                                    destDWord = Shift(destDWord, CL, ShiftType.Right);
                                    break;
                                case 7:
                                    destDWord = Shift(destDWord, CL, ShiftType.ArithmaticRight);
                                    break;
                            }

                            WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out tempWord, out destWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    destWord = Rotate(destWord, CL, RotateType.Left);
                                    break;
                                case 1:
                                    destWord = Rotate(destWord, CL, RotateType.Right);
                                    break;
                                case 2:
                                    destWord = Rotate(destWord, CL, RotateType.LeftWithCarry);
                                    break;
                                case 3:
                                    destWord = Rotate(destWord, CL, RotateType.RightWithCarry);
                                    break;
                                case 4:
                                case 6:
                                    destWord = Shift(destWord, CL, ShiftType.Left);
                                    break;
                                case 5:
                                    destWord = Shift(destWord, CL, ShiftType.Right);
                                    break;
                                case 7:
                                    destWord = Shift(destWord, CL, ShiftType.ArithmaticRight);
                                    break;
                            }

                            WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    #endregion
                    #region Misc
                    case 0x8d:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            registers[rmData.Register].DWord = memAddress;
                        
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            registers[rmData.Register].Word = (ushort)memAddress;
                        }
                        break;
                    case 0x9f:
                        AH = (byte)eFlags;
                        break;
                    case 0xc2:
                        if (opSize == 32)
                            EIP = StackPop();
                        else
                            EIP = (ushort)StackPop();
                        SP += (ushort)operands[0];
                        break;
                    case 0xc3:
                        if (opSize == 32)
                            EIP = StackPop();
                        else
                            EIP = (ushort)StackPop();
                        break;
                    case 0xc4:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            registers[rmData.Register].DWord = SegReadWord(overrideSegment, memAddress);
                            ES = SegReadWord(overrideSegment, (uint)(memAddress + 2));
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            registers[rmData.Register].Word = SegReadWord(overrideSegment, memAddress);
                            ES = SegReadWord(overrideSegment, (uint)(memAddress + 2));
                        }
                        break;
                    case 0xc5:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out sourceDWord, out destDWord);
                            registers[rmData.Register].Word = SegReadWord(overrideSegment, memAddress);
                            DS = SegReadWord(overrideSegment, (uint)(memAddress + 2));
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                            registers[rmData.Register].Word = SegReadWord(overrideSegment, memAddress);
                            DS = SegReadWord(overrideSegment, (uint)(memAddress + 2));
                        }
                        break;
                    case 0xc9:
                        ProcedureLeave();
                        break;
                    case 0xca:
                        if (opSize == 32)
                            EIP = StackPop();
                        else
                            EIP = (ushort)StackPop();
                        CS = (ushort)StackPop();
                        SP += (ushort)operands[0];
                        break;
                    case 0xcb:
                        if (opSize == 32)
                            EIP = StackPop();
                        else
                            EIP = (ushort)StackPop();
                        CS = (ushort)StackPop();
                        break;
                    case 0xcc:
                        CallInterrupt(3);
                        break;
                    case 0xcd:
                        CallInterrupt((byte)operands[0]);
                        break;
                    case 0xce:
                        if (OF)
                            CallInterrupt(4);
                        break;
                    case 0xcf:
                        if (opSize == 32)
                            EIP = StackPop();
                        else
                            EIP = (ushort)StackPop();
                        CS = (ushort)StackPop();
                        eFlags = (CPUFlags)StackPop();
                        break;
                    case 0xe0:
                        CX--;
                        if (!ZF && CX != 0)
                            EIP = offset;
                        break;
                    case 0xe1:
                        CX--;
                        if (ZF && CX != 0)
                            EIP = offset;
                        break;
                    case 0xe2:
                        CX--;
                        if (CX != 0)
                            EIP = offset;
                        break;
                    case 0xe4:
                        AL = DoIORead((byte)operands[0]);
                        break;
                    case 0xe5:
                        if(opSize == 32)
                            EAX = DoIORead((ushort)(byte)operands[0]);
                        else
                            AX = DoIORead((ushort)(byte)operands[0]);
                        break;
                    case 0xe6:
                        DoIOWrite((byte)operands[0], AL);
                        break;
                    case 0xe7:
                        if(opSize == 32)
                            DoIOWrite((ushort)(byte)operands[0], (ushort)EAX);
                        else
                            DoIOWrite((ushort)(byte)operands[0], AX);
                        break;
                    case 0xea:
                        if (opSize == 32)
                            DoJump((uint)(ushort)operands[1], (uint)operands[0], false);
                        else
                            DoJump((uint)(ushort)operands[1], (uint)(ushort)operands[0], false);
                        break;
                    case 0xe9:
                    case 0xeb:
                        EIP = offset;
                        break;
                    case 0xec:
                        AL = (byte)DoIORead(DX);
                        break;
                    case 0xed:
                        if (opSize == 32)
                            EAX = DoIORead(DX);
                        else
                            AX = DoIORead(DX);
                        break;
                    case 0xee:
                        DoIOWrite(DX, AL);
                        break;
                    case 0xef:
                        if(opSize == 32)
                            DoIOWrite(DX, (ushort)EAX);
                        else
                            DoIOWrite(DX, AX);
                        break;
                    case 0x98:
                        if(opSize == 32)
                            EAX = (uint)(int)(short)AX;
                        else
                            AX = (ushort)(short)(sbyte)AL;
                        break;
                    case 0x99:
                        if (opSize == 32)
                        {
                            tempDWord = (uint)(int)(short)EAX;
                            DX = (ushort)(tempDWord >> 16);
                        }
                        else
                        {
                            tempQWord = (ulong)(long)(int)EAX;
                            EDX = (uint)(tempQWord >> 16);
                        }
                        break;
                    case 0xc8:
                        destWord = (ushort)operands[0];
                        sourceByte = (byte)operands[1];

                        ProcedureEnter(destWord, sourceByte);
                        break;
                    case 0xf4:
                        throw new Exception("Halt");
                    case 0xf5:
                        CF = !CF;
                        break;
                    case 0xf8:
                        CF = false;
                        break;
                    case 0xf9:
                        CF = true;
                        break;
                    case 0xfb:
                        IF = true;
                        break;
                    case 0xfc:
                        DF = false;
                        break;
                    case 0xfd:
                        DF = true;
                        break;
                    case 0xfa:
                        IF = false;
                        break;
                    #endregion
                    #region Groups
                    case 0x80:
                        memAddress = ProcessRegMem(rmData, out tempByte, out destByte);
                        sourceByte = (byte)operands[1];

                        switch (rmData.Register)
                        {
                            case 0:
                                destByte = Add(destByte, sourceByte);
                                break;
                            case 1:
                                destByte = Or(destByte, sourceByte);
                                break;
                            case 2:
                                destByte = AddWithCarry(destByte, sourceByte);
                                break;
                            case 3:
                                destByte = SubWithBorrow(destByte, sourceByte);
                                break;
                            case 4:
                                destByte = And(destByte, sourceByte);
                                break;
                            case 5:
                                destByte = Subtract(destByte, sourceByte);
                                break;
                            case 6:
                                destByte = Xor(destByte, sourceByte);
                                break;
                            case 7:
                                Subtract(destByte, sourceByte);
                                break;
                        }

                        WriteRegMem(rmData, memAddress, destByte);
                        break;
                    case 0x81:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out tempDWord, out destDWord);
                            sourceDWord = (uint)operands[1];

                            switch (rmData.Register)
                            {
                                case 0:
                                    destDWord = Add(destDWord, sourceDWord);
                                    break;
                                case 1:
                                    destDWord = Or(destDWord, sourceDWord);
                                    break;
                                case 2:
                                    destDWord = AddWithCarry(destDWord, sourceDWord);
                                    break;
                                case 3:
                                    destDWord = SubWithBorrow(destDWord, sourceDWord);
                                    break;
                                case 4:
                                    destDWord = And(destDWord, sourceDWord);
                                    break;
                                case 5:
                                    destDWord = Subtract(destDWord, sourceDWord);
                                    break;
                                case 6:
                                    destDWord = Xor(destDWord, sourceDWord);
                                    break;
                                case 7:
                                    Subtract(destDWord, sourceDWord);
                                    break;
                            }

                            if (rmData.Register != 7)
                                WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out tempWord, out destWord);
                            sourceWord = (ushort)operands[1];

                            switch (rmData.Register)
                            {
                                case 0:
                                    destWord = Add(destWord, sourceWord);
                                    break;
                                case 1:
                                    destWord = Or(destWord, sourceWord);
                                    break;
                                case 2:
                                    destWord = AddWithCarry(destWord, sourceWord);
                                    break;
                                case 3:
                                    destWord = SubWithBorrow(destWord, sourceWord);
                                    break;
                                case 4:
                                    destWord = And(destWord, sourceWord);
                                    break;
                                case 5:
                                    destWord = Subtract(destWord, sourceWord);
                                    break;
                                case 6:
                                    destWord = Xor(destWord, sourceWord);
                                    break;
                                case 7:
                                    Subtract(destWord, sourceWord);
                                    break;
                            }

                            if(rmData.Register != 7)
                                WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0x83:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out tempDWord, out destDWord);
                            sourceByte = (byte)operands[1];

                            switch (rmData.Register)
                            {
                                case 0:
                                    destDWord = Add(destDWord, sourceByte);
                                    break;
                                case 1:
                                    destDWord = Or(destDWord, sourceByte);
                                    break;
                                case 2:
                                    destDWord = AddWithCarry(destDWord, sourceByte);
                                    break;
                                case 3:
                                    destDWord = SubWithBorrow(destDWord, sourceByte);
                                    break;
                                case 4:
                                    destDWord = And(destDWord, sourceByte);
                                    break;
                                case 5:
                                    destDWord = Subtract(destDWord, sourceByte);
                                    break;
                                case 6:
                                    destDWord = Xor(destDWord, sourceByte);
                                    break;
                                case 7:  /* cmp */
                                    Subtract(destDWord, sourceByte);
                                    break;
                            }

                            if (rmData.Register != 7)
                                WriteRegMem(rmData, memAddress, destDWord);
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out tempWord, out destWord);
                            sourceByte = (byte)operands[1];

                            switch (rmData.Register)
                            {
                                case 0:
                                    destWord = Add(destWord, sourceByte);
                                    break;
                                case 1:
                                    destWord = Or(destWord, sourceByte);
                                    break;
                                case 2:
                                    destWord = AddWithCarry(destWord, sourceByte);
                                    break;
                                case 3:
                                    destWord = SubWithBorrow(destWord, sourceByte);
                                    break;
                                case 4:
                                    destWord = And(destWord, sourceByte);
                                    break;
                                case 5:
                                    destWord = Subtract(destWord, sourceByte);
                                    break;
                                case 6:
                                    destWord = Xor(destWord, sourceByte);
                                    break;
                                case 7:
                                    Subtract(destWord, sourceByte);
                                    break;
                            }

                            if(rmData.Register != 7)
                                WriteRegMem(rmData, memAddress, destWord);
                        }
                        break;
                    case 0xf6:
                        memAddress = ProcessRegMem(rmData, out tempByte, out sourceByte);

                        switch (rmData.Register)
                        {
                            case 0:
                                And(sourceByte, (byte)operands[0]);
                                break;
                            case 2:
                                sourceByte = (byte)~sourceByte;
                                WriteRegMem(rmData, memAddress, sourceByte);

                                break;
                            case 3:
                                if (sourceByte == 0)
                                    CF = false;
                                else
                                    CF = true;

                                sourceByte = (byte)-sourceByte;
                                WriteRegMem(rmData, memAddress, sourceByte);
                                break;
                            case 4:
                                Multiply(sourceByte);
                                break;
                            case 5:
                                SignedMultiply(sourceByte);
                                break;
                            case 6:
                                Divide(sourceByte);
                                break;
                            case 7:
                                SDivide(sourceByte);
                                break;
                        }
                        break;
                    case 0xf7:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out tempDWord, out sourceDWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    And(sourceDWord, (uint)operands[0]);
                                    break;
                                case 2:
                                    sourceDWord = (uint)~sourceDWord;
                                    WriteRegMem(rmData, memAddress, sourceDWord);

                                    break;
                                case 3:
                                    if (sourceDWord == 0)
                                        CF = false;
                                    else
                                        CF = true;

                                    sourceDWord = (uint)-sourceDWord;
                                    WriteRegMem(rmData, memAddress, sourceDWord);
                                    break;
                                case 4:
                                    Multiply(sourceDWord);
                                    break;
                                case 5:
                                    SignedMultiply(sourceDWord);
                                    break;
                                case 6:
                                    Divide(sourceDWord);
                                    break;
                                case 7:
                                    SDivide(sourceDWord);
                                    break;
                            }
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out tempWord, out sourceWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    And(sourceWord, (ushort)operands[0]);
                                    break;
                                case 2:
                                    sourceWord = (ushort)~sourceWord;
                                    WriteRegMem(rmData, memAddress, sourceWord);

                                    break;
                                case 3:
                                    if (sourceWord == 0)
                                        CF = false;
                                    else
                                        CF = true;

                                    sourceWord = (ushort)-sourceWord;
                                    WriteRegMem(rmData, memAddress, sourceWord);
                                    break;
                                case 4:
                                    Multiply(sourceWord);
                                    break;
                                case 5:
                                    SignedMultiply(sourceWord);
                                    break;
                                case 6:
                                    Divide(sourceWord);
                                    break;
                                case 7:
                                    SDivide(sourceWord);
                                    break;
                            }
                        }
                        break;
                    case 0xfe:
                        memAddress = ProcessRegMem(rmData, out tempByte, out destByte);

                        switch (rmData.Register)
                        {
                            case 0:
                                destByte = Increment(destByte);
                                WriteRegMem(rmData, memAddress, destByte);
                                break;
                            case 1:
                                destByte = Decrement(destByte);
                                WriteRegMem(rmData, memAddress, destByte);
                                break;
                        }
                        break;
                    case 0xff:
                        if (opSize == 32)
                        {
                            memAddress = ProcessRegMem(rmData, out tempDWord, out destDWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    destDWord = Increment(destDWord);
                                    WriteRegMem(rmData, memAddress, destDWord);
                                    break;
                                case 1:
                                    destDWord = Decrement(destDWord);
                                    WriteRegMem(rmData, memAddress, destDWord);
                                    break;
                                case 2:
                                    CallRegMem(rmData, memAddress, false, true);
                                    break;
                                case 3:
                                    CallRegMem(rmData, memAddress, true, true);
                                    break;
                                case 4:
                                    CallRegMem(rmData, memAddress, false, false);
                                    break;
                                case 5:
                                    CallRegMem(rmData, memAddress, true, false);
                                    break;
                                case 6:
                                    StackPush(destDWord);
                                    break;
                            }
                        }
                        else
                        {
                            memAddress = ProcessRegMem(rmData, out tempWord, out destWord);

                            switch (rmData.Register)
                            {
                                case 0:
                                    destWord = Increment(destWord);
                                    WriteRegMem(rmData, memAddress, destWord);
                                    break;
                                case 1:
                                    destWord = Decrement(destWord);
                                    WriteRegMem(rmData, memAddress, destWord);
                                    break;
                                case 2:
                                    CallRegMem(rmData, memAddress, false, true);
                                    break;
                                case 3:
                                    CallRegMem(rmData, memAddress, true, true);
                                    break;
                                case 4:
                                    CallRegMem(rmData, memAddress, false, false);
                                    break;
                                case 5:
                                    CallRegMem(rmData, memAddress, true, false);
                                    break;
                                case 6:
                                    StackPush(destWord);
                                    break;
                            }
                        }
                        break;
                    #endregion
                }
            }
            currentAddr = (uint)(segments[(int)SegmentRegister.CS].GDTEntry.BaseAddress + EIP);
        }
    }
}
