using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace x86CS
{
    public delegate ushort ReadCallback(ushort addr);
    public delegate void WriteCallback(ushort addr, ushort value);

    public enum SegmentRegister
    {
        ES = 0,
        CS,
        SS,
        DS,
        FS,
        GS
    }

    public enum CPURegister
    {
        EAX,
        ECX,
        EDX,
        EBX,
        ESP,
        EBP,
        ESI,
        EDI,
        EIP,
    }

    [Flags]
    public enum CPUFlags : uint
    {
        CF = 0x0001,
        Spare = 0x0002,
        PF = 0x0004,
        Spare2 = 0x0008,
        AF = 0x0010,
        Spare3 = 0x0020,
        ZF = 0x0040,
        SF = 0x0080,
        TF = 0x0100,
        IF = 0x0200,
        DF = 0x0400,
        OF = 0x0800,
        IOPL = 0x1000,
        NT = 0x4000,
        Spare4 = 0x9000,
        RF = 0x10000,
        VM = 0x20000,
        AC = 0x40000,
        VIF = 0x00080000,
        VIP = 0x00100000,
        ID = 0x00200000,
        Spare5 = 0x00400000,
        Spare6 = 0x00800000,
        Spare7 = 0x01000000,
        Spare8 = 0x02000000,
        Spare9 = 0x04000000,
        Spare10 = 0x08000000,
        Spare11 = 0x10000000,
        Spare12 = 0x20000000,
        Spare13 = 0x40000000,
        Spare14 = 0x80000000,
    }

    public struct Segment
    {
        private ushort value;
        private int virtualAddr;

        public int Addr
        {
            get { return value; }
            set
            {
                this.value = (ushort)value;
                virtualAddr = value << 4;
            }
        }

        public int VirtualAddr
        {
            get { return virtualAddr; }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Register
    {
        [FieldOffset(0)]
        public uint DWord;
        [FieldOffset(0)]
        public ushort Word;
        [FieldOffset(0)]
        private byte Byte;

        public byte HighByte
        {
            get { return (byte)Word.GetHigh(); }
            set { Word = Word.SetHigh(value); }
        }

        public byte LowByte
        {
            get { return (byte)Word.GetLow();  }
            set { Byte = value; }
        }
    }

    public partial class CPU
    {
        private Segment[] segments;
        private Register[] registers;
        private CPUFlags eFlags;
        private bool debug = false;
        public event EventHandler<IntEventArgs> InteruptFired;
        public event ReadCallback IORead;
        public event WriteCallback IOWrite;
        private StreamWriter logFile = File.CreateText("cpulog.txt");

        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }

        #region Registers

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
        public ushort CS
        {
            get { return (ushort)segments[(int)SegmentRegister.CS].Addr; }
            set { segments[(int)SegmentRegister.CS].Addr = value; }
        }

        public ushort DS
        {
            get { return (ushort)segments[(int)SegmentRegister.DS].Addr; }
            set { segments[(int)SegmentRegister.DS].Addr = value; }
        }
        public ushort ES
        {
            get { return (ushort)segments[(int)SegmentRegister.ES].Addr; }
            set { segments[(int)SegmentRegister.ES].Addr = value; }
        }
        public ushort SS
        {
            get { return (ushort)segments[(int)SegmentRegister.SS].Addr; }
            set { segments[(int)SegmentRegister.SS].Addr = value; }
        }
        public ushort FS
        {
            get { return (ushort)segments[(int)SegmentRegister.FS].Addr; }
            set { segments[(int)SegmentRegister.FS].Addr = value; }
        }
        public ushort GS
        {
            get { return (ushort)segments[(int)SegmentRegister.GS].Addr; }
            set { segments[(int)SegmentRegister.GS].Addr = value; }
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

            logFile.AutoFlush = true;

            Reset();
        }

        public void Reset()
        {
            eFlags = CPUFlags.ZF | CPUFlags.IF;

            IP = 0;
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

        private void FireInterrupt(int intNum)
        {
            EventHandler<IntEventArgs> intEvent = InteruptFired;

            if (intEvent != null)
                intEvent(this, new IntEventArgs(intNum));
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

        private byte SegReadByte(SegmentRegister segment, int offset)
        {
            uint virtAddr;
            int seg = segments[(int)segment].VirtualAddr;
            byte ret;

            virtAddr = (uint)(seg + offset);

            ret = Memory.ReadByte(virtAddr);

            if(segment != SegmentRegister.CS)
                logFile.WriteLine(String.Format("Memory Read Byte {0:X8} {1:X2}", virtAddr, ret)); 

            return ret;
        }

        private ushort SegReadWord(SegmentRegister segment, int offset)
        {
            uint virtAddr;
            int segPtr = segments[(int)segment].VirtualAddr;
            ushort ret;

            virtAddr = (uint)(segPtr + offset);

            ret = Memory.ReadWord(virtAddr);

            if(segment != SegmentRegister.CS)
                logFile.WriteLine(String.Format("Memory Read Word {0:X8} {1:X4}", virtAddr, ret)); 

            return ret;
        }

        private void SegWriteByte(SegmentRegister segment, int offset, byte value)
        {
            uint virtAddr;
            int segPtr = segments[(int)segment].VirtualAddr;

            virtAddr = (uint)(segPtr + offset);

            logFile.WriteLine(String.Format("Memory Write Byte {0:X8} {1:X2}", virtAddr, value)); 

            Memory.WriteByte(virtAddr, value);
        }

        private void SegWriteWord(SegmentRegister segment, int offset, ushort value)
        {
            uint virtAddr;
            int segPtr = segments[(int)segment].VirtualAddr;

            virtAddr = (uint)(segPtr + offset);

            logFile.WriteLine(String.Format("Memory Write word {0:X8} {1:X4}", virtAddr, value)); 

            Memory.WriteWord(virtAddr, value);
        }

        public ushort StackPop()
        {
            ushort ret;

            ret = SegReadWord(SegmentRegister.SS, SP);
            SP += 2;

            return ret;
        }

        public void StackPush(ushort value)
        {
            SP -= 2;

            SegWriteWord(SegmentRegister.SS, SP, value);
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

        private string GetRegStr(int offset)
        {
            switch (offset)
            {
                case 0x0:
                    return "AX";
                case 0x1:
                    return "CX";
                case 0x2:
                    return "DX";
                case 0x3:
                    return "BX";
                case 0x4:
                    return "SP";
                case 0x5:
                    return "BP";
                case 0x6:
                    return "SI";
                case 0x7:
                    return "DI";
                default:
                    return "";
            }
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

        private void SetCPUFlags(ushort operand)
        {
            short signed = (short)operand;

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

        private void SetParity(int value)
        {
            BitArray bitCount = new BitArray(new int[] { value });

            if (bitCount.CountSet() % 2 == 0)
                PF = true;
            else
                PF = false;
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

        private ushort GetRegMemAddr(byte regMem, out SegmentRegister segToUse)
        {
            ushort address = 0;
            segToUse = SegmentRegister.DS;

            switch (regMem)
            {
                case 0:
                    address = (ushort)(BX + SI);
                    break;
                case 1:
                    address = (ushort)(BX + DI);
                    break;
                case 2:
                    address = (ushort)(BP + SI);
                    segToUse = SegmentRegister.SS;
                    break;
                case 3:
                    address = (ushort)(BP + DI);
                    segToUse = SegmentRegister.SS;
                    break;
                case 4:
                    address = SI;
                    break;
                case 5:
                    address = DI;
                    break;
                case 6:
                    address = 0;
                    break;
                case 7:
                    address = BX;
                    break;
            }
            return address;
        }

        private ushort ProcessRegMem(RegMemData rmData, out byte registerValue, out byte regMemValue)
        {
            ushort address = 0;
            SegmentRegister segToUse = SegmentRegister.DS;

            registerValue = GetByteReg(rmData.Register);

            if (rmData.IsRegister)
                regMemValue = GetByteReg(rmData.RegMem);
            else
            {
                address = GetRegMemAddr(rmData.RegMem, out segToUse);
                if (rmData.HasDisplacement)
                    address += rmData.Displacement;

                if (segToUse != overrideSegment)
                    segToUse = overrideSegment;

                regMemValue = SegReadByte(segToUse, address);
            }
            return address;
        }

        private ushort ProcessRegMem(RegMemData rmData, out ushort registerValue, out ushort regMemValue)
        {
            ushort address = 0;
            SegmentRegister segToUse = SegmentRegister.DS;

            registerValue = registers[rmData.Register].Word;

            if (rmData.IsRegister)
                regMemValue = registers[rmData.RegMem].Word;
            else
            {
                address = GetRegMemAddr(rmData.RegMem, out segToUse);
                if (rmData.HasDisplacement)
                    address += rmData.Displacement;

                if (segToUse != overrideSegment)
                    segToUse = overrideSegment;

                regMemValue = SegReadWord(segToUse, address);
            }

            return address;
        }

        private void WriteRegMem(RegMemData rmData, ushort address, byte value)
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

        private void WriteRegMem(RegMemData rmData, ushort address, ushort value)
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

        private void CallRegMem(RegMemData rmData, ushort address, bool far, bool call)
        {
            SegmentRegister readSegment = SegmentRegister.DS;
            ushort segment, offset;

            if (rmData.IsRegister)
                throw new Exception("Not supported");
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

                offset = SegReadWord(readSegment, address);
                if (far)
                    segment = SegReadWord(readSegment, address + 2);
                else
                    segment = CS;

                if (call)
                    CallProcedure(segment, offset);
                else
                {
                    CS = segment;
                    IP = offset;
                }
            }
        }

        private void CallProcedure(ushort segment, ushort offset)
        {
            if (segment == CS)
            {
                StackPush(IP);
                IP = offset;
            }
            else
            {
                StackPush(CS);
                StackPush(IP);

                CS = segment;
                IP = offset;
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
            BP = StackPop();
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
            IP = Memory.ReadWord((uint)(vector * 4));
        }

        public void Cycle(int len, byte opCode, object[] operands)
        {
            byte destByte, sourceByte, tmpByte;
            ushort destWord, sourceWord, tmpWord;
            ushort memAddress, offset = 0;
            sbyte signedByte;
            short signedWord;
            RegMemData rmData;
             
            IP += (ushort)len;

            rmData = operands[0] as RegMemData;

            if (operands[0] is byte)
            {
                signedByte = (sbyte)(byte)operands[0];
                offset = (ushort)(IP + signedByte);
            }

            if (operands[0] is ushort)
            {
                signedWord = (short)operands[0];
                offset = (ushort)(IP + signedWord);
            }

            switch (opCode)
            {
                #region Add with carry
                case 0x10:
                    memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                    destByte = AddWithCarry(destByte, sourceByte);
                    WriteRegMem(rmData, memAddress, destByte);
                    break;
                case 0x11:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = AddWithCarry(destWord, sourceWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x12:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = AddWithCarry(destByte, sourceByte);
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x13:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = AddWithCarry(destWord, sourceWord);
                    registers[rmData.Register].Word = destWord;
                    break;
                case 0x14:
                    AddWithCarry((byte)operands[0]);
                    break;
                case 0x15:
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
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = Add(destWord, sourceWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x02:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = Add(destByte, sourceByte);
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x03:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = Add(destWord, sourceWord);
                    registers[rmData.Register].Word = destWord;
                    break;
                case 0x04:
                    Add((byte)operands[0]);
                    break;
                case 0x05:
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
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = SubWithBorrow(destWord, sourceWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x1a:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = SubWithBorrow(destByte, sourceByte);
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x1b:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = SubWithBorrow(destWord, sourceWord);
                    registers[rmData.Register].Word = destWord;
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
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = Subtract(destWord, sourceWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x2a:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = Subtract(destByte, sourceByte);
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x2b:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = Subtract(destWord, sourceWord);
                    registers[rmData.Register].Word = destWord;
                    break;
                case 0x2c:
                    Subtract((byte)operands[0]);
                    break;
                case 0x2d:
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
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = And(destWord, sourceWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x22:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = And(destByte, sourceByte);
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x23:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = And(destWord, sourceWord);
                    registers[rmData.Register].Word = destWord;
                    break;
                case 0x24:
                    And((byte)operands[0]);
                    break;
                case 0x25:
                    And((ushort)operands[0]);
                    break;
                #endregion
                #region Or
                case 0x08:
                    memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                    destByte = Or(destByte, sourceByte);
                    WriteRegMem(rmData, memAddress, destByte);
                    break;
                case 0x09:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = Or(destWord, sourceWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x0a:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = Or(destByte, sourceByte);
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x0b:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = Or(destWord, sourceWord);
                    registers[rmData.Register].Word = destWord;
                    break;
                case 0x0c:
                    Or((byte)operands[0]);
                    break;
                case 0x0d:
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
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = Xor(destWord, sourceWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x32:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = Xor(destByte, sourceByte);
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x33:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = Xor(destWord, sourceWord);
                    registers[rmData.Register].Word = destWord;
                    break;
                case 0x34:
                    Xor((byte)operands[0]);
                    break;
                case 0x35:
                    Xor((ushort)operands[0]);
                    break;
                #endregion
                #region Compare
                case 0x38:
                    memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                    destByte = Subtract(destByte, sourceByte);
                    break;
                case 0x39:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = Subtract(destWord, sourceWord);
                    break;
                case 0x3a:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = Subtract(destByte, sourceByte);
                    break;
                case 0x3b:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = Subtract(destWord, sourceWord);
                    break;
                case 0x3c:
                    DoSub(AL, (byte)operands[0], false);
                    break;
                case 0x3d:
                    DoSub(AX, (ushort)operands[0], false);
                    break;
                #endregion
                #region Test
                case 0x84:
                    memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                    And(destByte, sourceByte);
                    break;
                case 0x85:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    And(destWord, sourceWord);
                    break;
                case 0xa8:
                    DoAnd(AL, (byte)operands[0]);
                    break;
                case 0xa9:
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
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    destWord = sourceWord;
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x8a:
                    memAddress = ProcessRegMem(rmData, out destByte, out sourceByte);
                    destByte = sourceByte;
                    SetByteReg(rmData.Register, destByte);
                    break;
                case 0x8b:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    destWord = sourceWord;
                    registers[rmData.Register].Word = destWord;
                    break;
                case 0x8c:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    WriteRegMem(rmData, memAddress, (ushort)segments[rmData.Register].Addr);
                    break;
                case 0x8e:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    segments[rmData.Register].Addr = destWord;
                    break;
                case 0xa0:
                    sourceByte = (byte)operands[0];
                    AL = SegReadByte(overrideSegment, sourceByte);
                    break;
                case 0xa1:
                    sourceWord = (ushort)operands[0];
                    AX = SegReadWord(overrideSegment, sourceWord);
                    break;
                case 0xa2:
                    sourceByte = (byte)operands[0];
                    SegWriteByte(overrideSegment, sourceByte, AL);
                    break;
                case 0xa3:
                    sourceWord = (ushort)operands[0];
                    SegWriteWord(overrideSegment, sourceWord, AX);
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
                    registers[opCode - 0xb8].Word = (ushort)operands[0];
                    break;
                case 0xc6:
                    memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                    WriteRegMem(rmData, memAddress, (byte)operands[0]);
                    break;
                case 0xc7:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    WriteRegMem(rmData, memAddress, (ushort)operands[0]);
                    break;
                #endregion
                #region Exchange
                case 0x86:
                    memAddress = ProcessRegMem(rmData, out sourceByte, out destByte);
                    WriteRegMem(rmData, memAddress, destByte);
                    SetByteReg(rmData.Register, sourceByte);
                    break;
                case 0x87:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    registers[rmData.Register].Word = sourceWord;
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
                    destWord = registers[opCode - 0x90].Word;
                    registers[opCode - 0x90].Word = registers[0].Word;
                    registers[0].Word = destWord;
                    break;
                #endregion
                #region Call Procedure
                case 0x9a:
                    CallProcedure((ushort)operands[1], (ushort)operands[0]);
                    break;
                case 0xe8:
                    CallProcedure(CS, offset);
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
                    registers[opCode - 0x48].Word = Decrement(registers[opCode - 0x48].Word);
                    break;
                #endregion
                #region Multiply
                case 0x69:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    tmpWord = (ushort)operands[1];

                    destWord = SignedMultiply(sourceWord, tmpWord);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x6b:
                    memAddress = ProcessRegMem(rmData, out destWord, out sourceWord);
                    sourceByte = (byte)operands[1];

                    destWord = SignedMultiply(sourceWord, sourceByte);
                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                #endregion
                #region String ops
                case 0x6c:
                    StringInByte((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0x6d:
                    StringInWord((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0x6e:
                    StringOutByte((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0x6f:
                    StringOutWord((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xa4:
                    StringCopyByte((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xa5:
                    StringCopyWord((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xaa:
                    StringWriteByte((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xab:
                    StringWriteWord((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xac:
                    StringReadByte((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xad:
                    StringReadWord((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xae:
                    StringScanByte((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                case 0xaf:
                    StringScanWord((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat);
                    break;
                #endregion
                #region Jumps
                case 0x70:
                    if (OF)
                        IP = offset;
                    break;
                case 0x71:
                    if (!OF)
                        IP = offset;
                    break;
                case 0x72:
                    if (CF)
                        IP = offset;
                    break;
                case 0x73:
                    if (!CF)
                        IP = offset;
                    break;
                case 0x74:
                    if (ZF)
                        IP = offset;
                    break;
                case 0x75:
                    if (!ZF)
                        IP = offset;
                    break;
                case 0x76:
                    if (CF || ZF)
                        IP = offset;
                    break;
                case 0x77:
                    if (!CF && !ZF)
                        IP = offset;
                    break;
                case 0x78:
                    if (SF)
                        IP = offset;
                    break;
                case 0x79:
                    if (!SF)
                        IP = offset;
                    break;
                case 0x7a:
                    if (PF)
                        IP = offset;
                    break;
                case 0x7b:
                    if (!PF)
                        IP = offset;
                    break;
                case 0x7c:
                    if (SF != OF)
                        IP = offset;
                    break;
                case 0x7d:
                    if (SF == OF)
                        IP = offset;
                    break;
                case 0x7e:
                    if (ZF || (SF != OF))
                        IP = offset;
                    break;
                case 0x7f:
                    if (!ZF && SF == OF)
                        IP = offset;
                    break;
                case 0xe3:
                    if (CX == 0)
                        IP = offset;
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
                    StackPush(registers[opCode - 0x58].Word);
                    break;
                case 0x58:
                case 0x59:
                case 0x5a:
                case 0x5b:
                case 0x5c:
                case 0x5d:
                case 0x5e:
                case 0x5f:
                    registers[opCode - 0x58].Word = StackPop();
                    break;
                case 0x60:
                    tmpWord = SegReadWord(SegmentRegister.SS, SP);
                    StackPush(AX);
                    StackPush(CX);
                    StackPush(DX);
                    StackPush(BX);
                    StackPush(tmpWord);
                    StackPush(BP);
                    StackPush(SI);
                    StackPush(DI);
                    break;
                case 0x61:
                    DI = StackPop();
                    SI = StackPop();
                    BP = StackPop();
                    SP += 2;
                    BX = StackPop();
                    DX = StackPop();
                    CX = StackPop();
                    AX = StackPop();
                    break;
                case 0x06:
                    StackPush(ES);
                    break;
                case 0x07:
                    ES = StackPop();
                    break;
                case 0x16:
                    StackPush(SS);
                    break;
                case 0x17:
                    SS = StackPop();
                    break;
                case 0x0e:
                    StackPush(CS);
                    break;
                case 0x1e:
                    StackPush(DS);
                    break;
                case 0x1f:
                    DS = StackPop();
                    break;
                case 0x6a:
                    StackPush((ushort)(byte)operands[0]);
                    break;
                case 0x68:
                    StackPush((ushort)operands[0]);
                    break;
                case 0x8f:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    sourceWord = StackPop();
                    WriteRegMem(rmData, memAddress, sourceWord);
                    break;
                case 0x9c:
                    StackPush((ushort)Flags);
                    break;
                case 0x9d:
                    eFlags = (CPUFlags)StackPop();
                    break;
                #endregion
                #region Rotate/shift
                case 0xc0:
                    memAddress = ProcessRegMem(rmData, out tmpByte, out destByte);
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
                    }

                    WriteRegMem(rmData, memAddress, destByte);
                    break;
                case 0xc1:
                    memAddress = ProcessRegMem(rmData, out tmpWord, out destWord);
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
                    }

                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0xd0:
                    memAddress = ProcessRegMem(rmData, out tmpByte, out destByte);

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
                    memAddress = ProcessRegMem(rmData, out tmpWord, out destWord);

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
                    break;
                case 0xd2:
                    memAddress = ProcessRegMem(rmData, out tmpByte, out destByte);

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
                    memAddress = ProcessRegMem(rmData, out tmpWord, out destWord);

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
                    break;
                #endregion
                #region Misc
                case 0x8d:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    registers[rmData.Register].Word = memAddress;
                    break;
                case 0x9f:
                    AH = (byte)eFlags;
                    break;
                    case 0xc2:
                        IP = StackPop();
                        SP += (ushort)operands[0];
                        break;
                case 0xc3:
                    IP = StackPop();
                    break;
                case 0xc4:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    registers[rmData.Register].Word = SegReadByte(overrideSegment, memAddress);
                    ES = SegReadByte(overrideSegment, memAddress + 2);
                    break;
                case 0xc5:
                    memAddress = ProcessRegMem(rmData, out sourceWord, out destWord);
                    registers[rmData.Register].Word = SegReadByte(overrideSegment, memAddress);
                    DS = SegReadByte(overrideSegment, memAddress + 2);
                    break;
                case 0xc9:
                    ProcedureLeave();
                    break;
                case 0xca:
                    IP = StackPop();
                    CS = StackPop();
                    SP += (ushort)operands[0];
                    break;
                case 0xcb:
                    IP = StackPop();
                    CS = StackPop();
                    break;
                case 0xcc:
                    CallInterrupt(3);
                    break;
                case 0xcd:
                    CallInterrupt((byte)operands[0]);
                    break;
                case 0xce:
                    if(OF)
                        CallInterrupt(4);
                    break;
                case 0xcf:
                    IP = StackPop();
                    CS = StackPop();
                    eFlags = (CPUFlags)StackPop();
                    break;
                case 0xe0:
                    CX--;
                    if (!ZF && CX != 0)
                        IP = offset;
                    break;
                case 0xe1:
                    CX--;
                    if (ZF && CX != 0)
                        IP = offset;
                    break;
                case 0xe2:
                    CX--;
                    if (CX != 0)
                        IP = offset;
                    break;
                case 0xe4:
                    AL = DoIORead((byte)operands[0]);
                    break;
                case 0xe5:
                    AX = DoIORead((ushort)(byte)operands[0]);
                    break;
                case 0xe6:
                    DoIOWrite((byte)operands[0], AL);
                    break;
                case 0xe7:
                    DoIOWrite((ushort)(byte)operands[0], AX);
                    break;
                case 0xea:
                    IP = (ushort)operands[0];
                    CS = (ushort)operands[1];
                    break;
                case 0xe9:
                case 0xeb:
                    IP = offset;
                    break;
                case 0xec:
                    AL = (byte)DoIORead(DX);
                    break;
                case 0xed:
                    AX = DoIORead(DX);
                    break;
                case 0xee:
                    DoIOWrite(DX, AL);
                    break;
                case 0xef:
                    DoIOWrite(DX, AX);
                    break;
                case 0x98:
                    AX = (ushort)(short)(sbyte)AL;
                    break;
                case 0x99:
                    uint tempDWord = (uint)(int)(short)AX;
                    DX = (ushort)(tempDWord >> 16);
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
                    memAddress = ProcessRegMem(rmData, out tmpByte, out destByte);
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
                    memAddress = ProcessRegMem(rmData, out tmpWord, out destWord);
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

                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0x83:
                    memAddress = ProcessRegMem(rmData, out tmpWord, out destWord);
                    sourceByte = (byte)operands[1];

                    switch (rmData.Register)
                    {
                        case 0:
                            destWord = Add(destWord, sourceByte);
                            break;
                        case 1:
                            destWord = Add(destWord, sourceByte);
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

                    WriteRegMem(rmData, memAddress, destWord);
                    break;
                case 0xf6:
                    memAddress = ProcessRegMem(rmData, out tmpByte, out sourceByte);

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
                    memAddress = ProcessRegMem(rmData, out tmpWord, out sourceWord);

                    switch (rmData.Register)
                    {
                        case 0:
                            And(sourceWord, (ushort)operands[0]);
                            break;
                        case 2:
                            sourceWord = (byte)~sourceWord;
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
                    break;
                case 0xfe:
                    memAddress = ProcessRegMem(rmData, out tmpByte, out destByte);

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
                    memAddress = ProcessRegMem(rmData, out tmpWord, out destWord);

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
                    break;
                #endregion
            }

            setPrefixes = 0;
            overrideSegment = SegmentRegister.DS;
            currentAddr = (uint)((CS << 4) + IP);
        }
    }
}
