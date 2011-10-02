using System;
using System.Collections.Generic;

namespace x86CS.CPU
{
    [Flags]
    public enum OPPrefix
    {
        Lock = 0x00000001,
        RepeatNEqual = 0x00000002,
        Repeat = 0x00000004,
        CSOverride = 0x00000008,
        SSOverride = 0x00000010,
        DSOverride = 0x00000020,
        ESOverride = 0x00000040,
        FSOverride = 0x00000080,
        GSOverride = 0x00000100,
        OPSize = 0x00000200,
        AddressSize = 0x00000400,
        Extended = 0x00000800
    }

    public class RegMemData
    {
        public byte Mode;
        public byte Register;
        public byte RegMem;
        public byte Base;
        public byte Scale;
        public byte Index;
        public bool IsRegister;
        public bool HasDisplacement;
        public uint Displacement;
        public string Operand;
        public string RegisterName;
    }

    public enum RepeatPrefix
    {
        None = 0,
        Repeat,
        RepeatNotZero
    }

    public partial class CPU
    {
        private OPPrefix setPrefixes = 0;
        private SegmentRegister overrideSegment = SegmentRegister.DS;
        private bool extPrefix;
        private RepeatPrefix repeatPrefix = RepeatPrefix.None;
        private int opSize = 16;
        private int memSize = 16;

        #region Read Functions
        private byte DecodeReadByte()
        {
            return DecodeReadByte(true);
        }

        private byte DecodeReadByte(bool log)
        {
            var ret = (uint)(Memory.ReadByte(CurrentAddr, log) & 0xff);

            CurrentAddr++;

            return (byte)ret;
        }

        private ushort DecodeReadWord()
        {
            return DecodeReadWord(true);
        }

        private ushort DecodeReadWord(bool log)
        {
            var ret = (uint)(Memory.ReadWord(CurrentAddr, log) & 0xffff);

            CurrentAddr += 2;

            return (ushort)ret;
        }

        private uint DecodeReadDWord()
        {
            return DecodeReadDWord(true);
        }

        private uint DecodeReadDWord(bool log)
        {
            uint ret = Memory.ReadDWord(CurrentAddr, log);

            CurrentAddr += 4;

            return ret;
        }
        #endregion

        private string GetPrefixString(SegmentRegister defaultseg)
        {
            if (overrideSegment == defaultseg)
                return "";
            
            if (overrideSegment == SegmentRegister.DS)
                return defaultseg + ":";
            
            return overrideSegment + ":";
        }

        private void DecodePrefixes()
        {
            var readPrefix = true;

            setPrefixes = 0;
            repeatPrefix = RepeatPrefix.None;
            overrideSegment = SegmentRegister.DS;
            opSize = PMode ? 32 : 16;

            extPrefix = false;

            do
            {
                byte opCode = DecodeReadByte(false);

                switch (opCode)
                {
                    case 0x0f:
                        setPrefixes |= OPPrefix.Extended;
                        break;
                    case 0x26:
                        setPrefixes |= OPPrefix.ESOverride;
                        break;
                    case 0x2e:
                        setPrefixes |= OPPrefix.CSOverride;
                        break;
                    case 0x36:
                        setPrefixes |= OPPrefix.SSOverride;
                        break;
                    case 0x3e:
                        setPrefixes |= OPPrefix.DSOverride;
                        break;
                    case 0x64:
                        setPrefixes |= OPPrefix.FSOverride;
                        break;
                    case 0x65:
                        setPrefixes |= OPPrefix.GSOverride;
                        break;
                    case 0x66:
                        setPrefixes |= OPPrefix.OPSize;
                        break;
                    case 0x67:
                        setPrefixes |= OPPrefix.AddressSize;
                        break;
                    case 0xf0:
                        setPrefixes |= OPPrefix.Lock;
                        break;
                    case 0xf2:
                        setPrefixes |= OPPrefix.RepeatNEqual;
                        break;
                    case 0xf3:
                        setPrefixes |= OPPrefix.Repeat;
                        break;
                    default:
                        readPrefix = false;
                        CurrentAddr--;
                        break;
                }
            } while(readPrefix);

            if ((setPrefixes & OPPrefix.CSOverride) == OPPrefix.CSOverride)
                 overrideSegment = SegmentRegister.CS;
            else if ((setPrefixes & OPPrefix.DSOverride) == OPPrefix.DSOverride)
                overrideSegment = SegmentRegister.DS;
            else if ((setPrefixes & OPPrefix.ESOverride) == OPPrefix.ESOverride)
                overrideSegment = SegmentRegister.ES;
            else if ((setPrefixes & OPPrefix.FSOverride) == OPPrefix.FSOverride)
                overrideSegment = SegmentRegister.FS;
            else if ((setPrefixes & OPPrefix.GSOverride) == OPPrefix.GSOverride)
                overrideSegment = SegmentRegister.GS;
            else if ((setPrefixes & OPPrefix.SSOverride) == OPPrefix.SSOverride)
                overrideSegment = SegmentRegister.SS;

            if ((setPrefixes & OPPrefix.OPSize) == OPPrefix.OPSize)
                opSize = PMode ? 16 : 32;
            if ((setPrefixes & OPPrefix.AddressSize) == OPPrefix.AddressSize)
                memSize = PMode ? 16 : 32;
            if ((setPrefixes & OPPrefix.Extended) == OPPrefix.Extended)
                extPrefix = true;

            if ((setPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat)
                repeatPrefix = RepeatPrefix.Repeat;
            if ((setPrefixes & OPPrefix.RepeatNEqual) == OPPrefix.RepeatNEqual)
                repeatPrefix = RepeatPrefix.RepeatNotZero;
        }

        private RegMemData DecodeRM(bool word)
        {
            var ret = new RegMemData();

            byte modRegRm = DecodeReadByte(false);

            ret.Mode = (byte)(modRegRm >> 6);
            ret.Register = (byte)((modRegRm >> 3) & 0x7);
            ret.RegMem = (byte)(modRegRm & 0x07);

            switch (ret.Mode)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                    switch (ret.RegMem)
                    {
                        case 0x0:
                            if (memSize == 32)
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[EAX";
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[BX + SI";
                            break;
                        case 0x1:
                            if (memSize == 32)
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[ECX";
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[BX + DI";
                            break;
                        case 0x2:
                            if (memSize == 32)
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[EDX";
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.SS) + "[BP + SI";
                            break;
                        case 0x3:
                            if (memSize == 32)
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[EBX";
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.SS) + "[BP + DI";
                            break;
                        case 0x4:
                            if (memSize == 32)
                            {
                                byte sib = DecodeReadByte(false);

                                ret.Base = (byte)(sib & 0x7);
                                ret.Index = (byte)((sib >> 3) & 0x7);
                                ret.Scale = (byte)(sib >> 6);

                                if (ret.Base != 5)
                                {
                                    if (ret.Index == 4)
                                        ret.Operand = GetPrefixString(SegmentRegister.SS) + "[" + GetRegStr(ret.Base);
                                    else
                                        ret.Operand = GetPrefixString(SegmentRegister.DS) + "[" + GetRegStr(ret.Base);

                                    if (ret.Scale != 0)
                                        ret.Operand += " * " + (2 << (ret.Scale - 1));

                                    if (ret.Index != 4)
                                        ret.Operand += " + " + GetRegStr(ret.Index);
                                }
                            }
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[SI";
                            break;
                        case 0x5:
                            if (memSize == 32)
                            {
                                if (ret.Mode == 0x1 || ret.Mode == 0x2)
                                    ret.Operand = GetPrefixString(SegmentRegister.SS) + "[EBP";
                                else
                                    ret.Operand = GetPrefixString(SegmentRegister.DS) + "[";
                            }
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[DI";
                            break;
                        case 0x6:
                            if (memSize == 32)
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[ESI";
                            else
                            {
                                if (ret.Mode == 0x1 || ret.Mode == 0x2)
                                    ret.Operand = GetPrefixString(SegmentRegister.SS) + "[BP";
                                else
                                    ret.Operand = GetPrefixString(SegmentRegister.DS) + "[";
                            }
                            break;
                        case 0x7:
                            if (PMode)
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[EDI";
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[BX";
                            break;
                        default:
                            ret.Operand = "INVALID";
                            break;
                    }
                    if (ret.Mode == 0x1)
                    {
                        byte byteOp = DecodeReadByte();
                        var tmpDisp = (int)(sbyte)byteOp;

                        if (tmpDisp < 0)
                            ret.Operand += " - " + (-tmpDisp).ToString("X") + "]";
                        else
                            ret.Operand += " + " + byteOp.ToString("X") + "]";

                        ret.Displacement = (uint)tmpDisp;
                        ret.HasDisplacement = true;
                    }
                    else if (ret.Mode == 0x2 && memSize == 16)
                    {
                        ushort wordOp = DecodeReadWord();
                        int tmpDisp = (short)wordOp;

                        if (tmpDisp < 0)
                            ret.Operand += " - " + (-tmpDisp).ToString("X") + "]";
                        else
                            ret.Operand += " + " + wordOp.ToString("X") + "]";

                        ret.Displacement = (uint)tmpDisp;
                        ret.HasDisplacement = true;
                    }
                    else if (ret.Mode == 0x2)
                    {
                        uint dWordOp = DecodeReadDWord(false);
                        var tmpDisp = (int)dWordOp;

                        if (tmpDisp < 0)
                            ret.Operand += " - " + (-tmpDisp).ToString("X") + "]";
                        else
                            ret.Operand += " + " + dWordOp.ToString("X") + "]";

                        ret.Displacement = (uint)tmpDisp;
                        ret.HasDisplacement = true;
                    }
                    else if (ret.RegMem == 0x5 && memSize == 32)
                    {
                        ret.Displacement = DecodeReadWord(false);
                        ret.HasDisplacement = true;

                        ret.Operand += "+" + ret.Displacement.ToString("X") + "]";
                    }
                    else if (ret.RegMem == 0x6 && memSize == 16)
                    {
                        ret.Displacement = DecodeReadWord(false);
                        ret.HasDisplacement = true;

                        ret.Operand += ret.Displacement.ToString("X") + "]";
                    }
                    else
                        ret.Operand += "]";
                    break;
                case 0x03:
                    ret.Operand = word ? GetRegStr(ret.RegMem) : GetByteRegStr(ret.RegMem);

                    ret.IsRegister = true;
                    break;
                default:
                    ret.Operand = "INVALID";
                    break;
            }

            ret.RegisterName = word ? GetRegStr(ret.Register) : GetByteRegStr(ret.Register);

            return ret;
        }

        public string DecodeOpString(byte opCode, object[] operands)
        {
            string opStr = "";
            string grpStr = "";
            RegMemData rmData = null;
            sbyte signedByte = 0;
            short signedWord = 0;
            int signedDWord = 0;
            uint offset = 0;

            if (operands.Length > 0)
            {
                rmData = operands[0] as RegMemData;

                if (operands[0] is byte)
                {
                    signedByte = (sbyte)(byte)operands[0];
                    if(opSize == 32)
                        offset = (uint)(CurrentAddr + signedByte);
                    else
                        offset = (ushort)(CurrentAddr + signedByte);
                }

                if (operands[0] is ushort)
                {
                    signedWord = (short)(ushort)operands[0];
                    if(opSize == 32)
                        offset = (uint)(CurrentAddr + signedWord);
                    else
                        offset = (ushort)(CurrentAddr + signedWord);
                }

                if (operands[0] is uint)
                {
                    signedDWord = (int)(uint)operands[0];
                    if(opSize == 32)
                        offset = (uint)(CurrentAddr + signedDWord);
                    else
                        offset = (ushort)(CurrentAddr + signedDWord);
                }
            }

            if (extPrefix)
            {
                switch (opCode)
                {
                    case 0x01:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 2:
                                opStr = String.Format("LGDT {0}", rmData.Operand);
                                break;
                            case 3:
                                opStr = String.Format("LIDT {0}", rmData.Operand);
                                break;
                            default:
                                System.Diagnostics.Debugger.Break();
                                break;
                        }
                        break;
                    case 0x20:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV E{0}, {1}", rmData.RegisterName, GetControlRegStr(rmData.RegMem));
                        break;
                    case 0x22:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV {0}, E{1}", GetControlRegStr(rmData.RegMem), rmData.RegisterName);
                        break;
                    case 0x80:
                        opStr = String.Format("JO {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x81:
                        opStr = String.Format("JNO {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x82:
                        opStr = String.Format("JB {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x83:
                        opStr = String.Format("JNB {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x84:
                        opStr = String.Format("JZ {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x85:
                        opStr = String.Format("JNZ {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x86:
                        opStr = String.Format("JBE {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x87:
                        opStr = String.Format("JNBE {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x88:
                        opStr = String.Format("JS {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x89:
                        opStr = String.Format("JNS {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x8a:
                        opStr = String.Format("JP {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x8b:
                        opStr = String.Format("JS {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x8c:
                        opStr = String.Format("JL {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x8d:
                        opStr = String.Format("JNL {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x8e:
                        opStr = String.Format("JLE {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0x8f:
                        opStr = String.Format("JNLE {0:X} ({1:X})", memSize == 32 ? signedDWord : signedWord, offset);
                        break;
                    case 0xb6:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOVZX {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                }
            }
            else
            {
                switch (opCode)
                {
                    case 0x00:
                    case 0x01:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("ADD {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x08:
                    case 0x09:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("OR {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x10:
                    case 0x11:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("ADC {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x18:
                    case 0x19:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("SBB {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x20:
                    case 0x21:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("AND {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x28:
                    case 0x29:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("SUB {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x30:
                    case 0x31:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("XOR {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x38:
                    case 0x39:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("CMP {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x02:
                    case 0x03:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("ADD {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x0a:
                    case 0x0b:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("OR {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x12:
                    case 0x13:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("ADC {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x1a:
                    case 0x1b:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("SBB {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x22:
                    case 0x23:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("AND {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x2a:
                    case 0x2b:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("SUB {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x32:
                    case 0x33:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("XOR {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x3a:
                    case 0x3b:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("CMP {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x04:
                        opStr = String.Format("ADD AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x0c:
                        opStr = String.Format("OR AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x14:
                        opStr = String.Format("ADC AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x1c:
                        opStr = String.Format("SBB AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x24:
                        opStr = String.Format("AND AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x2c:
                        opStr = String.Format("SUB AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x34:
                        opStr = String.Format("XOR AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x3c:
                        opStr = String.Format("CMP AL, {0:X}", (byte)operands[0]);
                        break;
                    case 0x05:
                        opStr = opSize == 32 ? String.Format("ADD EAX, {0:X}", (uint)operands[0]) : String.Format("ADD AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x0d:
                        opStr = opSize == 32 ? String.Format("OR EAX, {0:X}", (uint)operands[0]) : String.Format("OR AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x15:
                        opStr = opSize == 32 ? String.Format("ADC EAX, {0:X}", (uint)operands[0]) : String.Format("ADC AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x1d:
                        opStr = opSize == 32 ? String.Format("SBB EAX, {0:X}", (uint)operands[0]) : String.Format("SBB AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x25:
                        opStr = opSize == 32 ? String.Format("AND EAX, {0:X}", (uint)operands[0]) : String.Format("AND AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x2d:
                        opStr = opSize == 32 ? String.Format("SUB EAX, {0:X}", (uint)operands[0]) : String.Format("SUB AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x35:
                        opStr = opSize == 32 ? String.Format("XOR EAX, {0:X}", (uint)operands[0]) : String.Format("XOR AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x3d:
                        opStr = opSize == 32 ? String.Format("CMP EAX, {0:X}", (uint)operands[0]) : String.Format("CMP AX, {0:X}", (ushort)operands[0]);
                        break;
                    case 0x06:
                        opStr = "PUSH ES";
                        break;
                    case 0x07:
                        opStr = "POP ES";
                        break;
                    case 0x0e:
                        opStr = "PUSH CS";
                        break;
                    case 0x16:
                        opStr = "PUSH SS";
                        break;
                    case 0x17:
                        opStr = "POP SS";
                        break;
                    case 0x1e:
                        opStr = "PUSH DS";
                        break;
                    case 0x1f:
                        opStr = "POP DS";
                        break;
                    case 0x27:
                        opStr = "DAA";
                        break;
                    case 0x2f:
                        opStr = "DAS";
                        break;
                    case 0x37:
                        opStr = "AAA";
                        break;
                    case 0x3f:
                        opStr = "AAS";
                        break;
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x46:
                    case 0x47:
                        opStr = "INC " + GetRegStr(opCode - 0x40);
                        break;
                    case 0x48:
                    case 0x49:
                    case 0x4a:
                    case 0x4b:
                    case 0x4c:
                    case 0x4d:
                    case 0x4e:
                    case 0x4f:
                        opStr = "DEC " + GetRegStr(opCode - 0x48);
                        break;
                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x54:
                    case 0x55:
                    case 0x56:
                    case 0x57:
                        opStr = "PUSH " + GetRegStr(opCode - 0x50);
                        break;
                    case 0x58:
                    case 0x59:
                    case 0x5a:
                    case 0x5b:
                    case 0x5c:
                    case 0x5d:
                    case 0x5e:
                    case 0x5f:
                        opStr = "POP " + GetRegStr(opCode - 0x58);
                        break;
                    case 0x60:
                        opStr = "PUSHA";
                        break;
                    case 0x61:
                        opStr = "POPA";
                        break;
                    case 0x68:
                        opStr = opSize == 32 ? String.Format("PUSH {0:X}", (uint)operands[0]) : String.Format("PUSH {0:X}", (ushort)operands[0]);
                        break;
                    case 0x69:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("IMUL {0}, {1}, {2:X}", rmData.RegisterName, rmData.Operand, (ushort)operands[1]);
                        break;
                    case 0x6a:
                        opStr = String.Format("PUSH {0:X}", (byte)operands[0]);
                        break;
                    case 0x6b:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("IMUL {0}, {1}, {2:X}", rmData.RegisterName, rmData.Operand, (ushort)((sbyte)operands[1]));
                        break;
                    case 0x6c:
                        opStr = "INSB";
                        break;
                    case 0x6d:
                        opStr = "INSW";
                        break;
                    case 0x6e:
                        opStr = "OUTSB";
                        break;
                    case 0x6f:
                        opStr = "OUTSW";
                        break;
                    case 0x70:
                        opStr = String.Format("JO {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x71:
                        opStr = String.Format("JNO {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x72:
                        opStr = String.Format("JB {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x73:
                        opStr = String.Format("JNB {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x74:
                        opStr = String.Format("JZ {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x75:
                        opStr = String.Format("JNZ {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x76:
                        opStr = String.Format("JBE {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x77:
                        opStr = String.Format("JNBE {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x78:
                        opStr = String.Format("JS {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x79:
                        opStr = String.Format("JNS {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x7a:
                        opStr = String.Format("JPE {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x7b:
                        opStr = String.Format("JPO {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x7c:
                        opStr = String.Format("JL {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x7d:
                        opStr = String.Format("JNL {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x7e:
                        opStr = String.Format("JLE {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x7f:
                        opStr = String.Format("JNLE {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0x80:
                    case 0x82:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "AND";
                                break;
                            case 0x1:
                                grpStr = "OR";
                                break;
                            case 0x2:
                                grpStr = "ADC";
                                break;
                            case 0x3:
                                grpStr = "SBB";
                                break;
                            case 0x4:
                                grpStr = "AND";
                                break;
                            case 0x5:
                                grpStr = "SUB";
                                break;
                            case 0x6:
                                grpStr = "XOR";
                                break;
                            case 0x7:
                                grpStr = "CMP";
                                break;
                        }
                        opStr = String.Format("{0} {1}, {2:X}", grpStr, rmData.Operand, (byte)operands[1]);
                        break;
                    case 0x81:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "AND";
                                break;
                            case 0x1:
                                grpStr = "OR";
                                break;
                            case 0x2:
                                grpStr = "ADC";
                                break;
                            case 0x3:
                                grpStr = "SBB";
                                break;
                            case 0x4:
                                grpStr = "AND";
                                break;
                            case 0x5:
                                grpStr = "SUB";
                                break;
                            case 0x6:
                                grpStr = "XOR";
                                break;
                            case 0x7:
                                grpStr = "CMP";
                                break;
                        }
                        opStr = opSize == 32 ? String.Format("{0} {1}, {2:X}", grpStr, rmData.Operand, (uint)operands[1]) : String.Format("{0} {1}, {2:X}", grpStr, rmData.Operand, (ushort)operands[1]);
                        break;
                    case 0x83:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "ADD";
                                break;
                            case 0x1:
                                grpStr = "OR";
                                break;
                            case 0x2:
                                grpStr = "ADC";
                                break;
                            case 0x3:
                                grpStr = "SBB";
                                break;
                            case 0x4:
                                grpStr = "AND";
                                break;
                            case 0x5:
                                grpStr = "SUB";
                                break;
                            case 0x6:
                                grpStr = "XOR";
                                break;
                            case 0x7:
                                grpStr = "CMP";
                                break;
                        }
                        opStr = opSize == 32 ? String.Format("{0} {1}, {2:X}", grpStr, rmData.Operand, (uint)((sbyte)(byte)operands[1])) : String.Format("{0} {1}, {2:X}", grpStr, rmData.Operand, (ushort)((sbyte)(byte)operands[1]));
                        break;
                    case 0x84:
                    case 0x85:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("TEST {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x86:
                    case 0x87:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("XCHG {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x88:
                    case 0x89:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV {0}, {1}", rmData.Operand, rmData.RegisterName);
                        break;
                    case 0x8a:
                    case 0x8b:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x8c:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV {0}, {1}", rmData.Operand, ((SegmentRegister)rmData.Register).ToString());
                        break;
                    case 0x8d:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("LEA {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0x8e:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV {0}, {1}", ((SegmentRegister)rmData.Register).ToString(), rmData.Operand);
                        break;
                    case 0x8f:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("POP {0}", rmData.Operand);
                        break;
                    case 0x90:
                        opStr = "NOP";
                        break;
                    case 0x91:
                    case 0x92:
                    case 0x93:
                    case 0x94:
                    case 0x95:
                    case 0x96:
                    case 0x97:
                        opStr = String.Format("XCHG {0}, AX", GetRegStr(opCode - 0x90));
                        break;
                    case 0x98:
                        opStr = "CBW";
                        break;
                    case 0x99:
                        opStr = "CWD";
                        break;
                    case 0x9a:
                        opStr = String.Format("CALL FAR {0:X}:{1:X}", operands[1], operands[0]);
                        break;
                    case 0x9c:
                        opStr = "PUSHF";
                        break;
                    case 0x9d:
                        opStr = "POPF";
                        break;
                    case 0x9e:
                        opStr = "SAHF";
                        break;
                    case 0x9f:
                        opStr = "LAHF";
                        break;
                    case 0xa0:
                        opStr = String.Format("MOV AL, {0}{1:X}", GetPrefixString(SegmentRegister.DS), operands[0]);
                        break;
                    case 0xa1:
                        opStr = String.Format("MOV AX, {0}{1:X}", GetPrefixString(SegmentRegister.DS), operands[0]);
                        break;
                    case 0xa2:
                        opStr = String.Format("MOV {0}{1:X}, AL", GetPrefixString(SegmentRegister.DS), operands[0]);
                        break;
                    case 0xa3:
                        opStr = String.Format("MOV {0}{1:X}, AX", GetPrefixString(SegmentRegister.DS), operands[0]);
                        break;
                    case 0xa4:
                        opStr = "MOVSB";
                        break;
                    case 0xa5:
                        opStr = "MOVSW";
                        break;
                    case 0xa6:
                        opStr = "CMPSB";
                        break;
                    case 0xa7:
                        opStr = "CMPSW";
                        break;
                    case 0xa8:
                        opStr = String.Format("TEST AL, {0:X}", operands[0]);
                        break;
                    case 0xa9:
                        opStr = String.Format("TEST AX, {0:X}", operands[0]);
                        break;
                    case 0xaa:
                        opStr = "STOSB";
                        break;
                    case 0xab:
                        opStr = "STOSW";
                        break;
                    case 0xac:
                        opStr = "LODSB";
                        break;
                    case 0xad:
                        opStr = "LODSW";
                        break;
                    case 0xae:
                        opStr = "SCASB";
                        break;
                    case 0xaf:
                        opStr = "SCASW";
                        break;
                    case 0xb0:
                    case 0xb1:
                    case 0xb2:
                    case 0xb3:
                    case 0xb4:
                    case 0xb5:
                    case 0xb6:
                    case 0xb7:
                        opStr = String.Format("MOV {0}, {1:X}", GetByteRegStr(opCode - 0xb0), operands[0]);
                        break;
                    case 0xb8:
                    case 0xb9:
                    case 0xba:
                    case 0xbb:
                    case 0xbc:
                    case 0xbd:
                    case 0xbe:
                    case 0xbf:
                        opStr = String.Format("MOV {0}, {1:X}", GetRegStr(opCode - 0xb8), operands[0]);
                        break;
                    case 0xc0:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "ROL";
                                break;
                            case 0x1:
                                grpStr = "ROR";
                                break;
                            case 0x2:
                                grpStr = "RCL";
                                break;
                            case 0x3:
                                grpStr = "RCR";
                                break;
                            case 0x4:
                                grpStr = "SHL";
                                break;
                            case 0x5:
                                grpStr = "SHR";
                                break;
                            case 0x6:
                                grpStr = "SAL";
                                break;
                            case 0x7:
                                grpStr = "SAR";
                                break;
                        }
                        opStr = String.Format("{0} {1}, {2:X}", grpStr, rmData.Operand, operands[1]);
                        break;
                    case 0xc1:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "ROL";
                                break;
                            case 0x1:
                                grpStr = "ROR";
                                break;
                            case 0x2:
                                grpStr = "RCL";
                                break;
                            case 0x3:
                                grpStr = "RCR";
                                break;
                            case 0x4:
                                grpStr = "SHL";
                                break;
                            case 0x5:
                                grpStr = "SHR";
                                break;
                            case 0x6:
                                grpStr = "SAL";
                                break;
                            case 0x7:
                                grpStr = "SAR";
                                break;
                        }
                        opStr = String.Format("{0} {1}, {2:X}", grpStr, rmData.Operand, (ushort)((sbyte)(byte)operands[1]));
                        break;
                    case 0xc2:
                        opStr = String.Format("RETN {0:X}", operands[0]);
                        break;
                    case 0xc3:
                        opStr = "RETN";
                        break;
                    case 0xc4:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("LES {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0xc5:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("LDS {0}, {1}", rmData.RegisterName, rmData.Operand);
                        break;
                    case 0xc6:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV {0}, {1:X}", rmData.Operand, operands[1]);
                        break;
                    case 0xc7:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        opStr = String.Format("MOV {0}, {1:X}", rmData.Operand, operands[1]);
                        break;
                    case 0xc8:
                        opStr = String.Format("ENTER {0:X}, {1:X}", operands[0], operands[1]);
                        break;
                    case 0xc9:
                        opStr = "LEAVE";
                        break;
                    case 0xca:
                        opStr = String.Format("RET FAR {0:X}", operands[0]);
                        break;
                    case 0xcb:
                        opStr = "RET FAR";
                        break;
                    case 0xcc:
                        opStr = "INT 3 (Special)";
                        break;
                    case 0xcd:
                        opStr = String.Format("INT {0:X}", operands[0]);
                        break;
                    case 0xce:
                        opStr = "INTO";
                        break;
                    case 0xcf:
                        opStr = "IRET";
                        break;
                    case 0xd0:
                    case 0xd1:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "ROL";
                                break;
                            case 0x1:
                                grpStr = "ROR";
                                break;
                            case 0x2:
                                grpStr = "RCL";
                                break;
                            case 0x3:
                                grpStr = "RCR";
                                break;
                            case 0x4:
                                grpStr = "SHL";
                                break;
                            case 0x5:
                                grpStr = "SHR";
                                break;
                            case 0x6:
                                grpStr = "SAL";
                                break;
                            case 0x7:
                                grpStr = "SAR";
                                break;
                        }
                        opStr = String.Format("{0} {1}, 1", grpStr, rmData.Operand);
                        break;
                    case 0xd2:
                    case 0xd3:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "ROL";
                                break;
                            case 0x1:
                                grpStr = "ROR";
                                break;
                            case 0x2:
                                grpStr = "RCL";
                                break;
                            case 0x3:
                                grpStr = "RCR";
                                break;
                            case 0x4:
                                grpStr = "SHL";
                                break;
                            case 0x5:
                                grpStr = "SHR";
                                break;
                            case 0x6:
                                grpStr = "SAL";
                                break;
                            case 0x7:
                                grpStr = "SAR";
                                break;
                        }
                        opStr = String.Format("{0} {1}, CL", grpStr, rmData.Operand);
                        break;
                    case 0xd4:
                        opStr = "AAM";
                        break;
                    case 0xd5:
                        opStr = "AAD";
                        break;
                    case 0xe0:
                        opStr = String.Format("LOOPNZ {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0xe1:
                        opStr = String.Format("LOOPZ {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0xe2:
                        opStr = String.Format("LOOP {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0xe3:
                        opStr = String.Format("JCXZ {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0xe4:
                        opStr = String.Format("IN AL, {0:X}", operands[0]);
                        break;
                    case 0xe5:
                        opStr = String.Format("IN AX, {0:X}", operands[0]);
                        break;
                    case 0xe6:
                        opStr = String.Format("OUT {0:X}, AL", operands[0]);
                        break;
                    case 0xe7:
                        opStr = String.Format("OUT {0:X}, AX", operands[0]);
                        break;
                    case 0xe8:
                        opStr = opSize == 32
                                    ? String.Format("CALL {0:X} ({1:X})", signedDWord, offset)
                                    : String.Format("CALL {0:X} ({1:X})", signedWord, offset);
                        break;
                    case 0xe9:
                        opStr = String.Format("JMP {0:X} ({1:X})", signedWord, offset);
                        break;
                    case 0xea:
                        opStr = String.Format("JMP FAR {0:X}:{1:X}", operands[1], operands[0]);
                        break;
                    case 0xeb:
                        opStr = String.Format("JMP {0:X} ({1:X})", signedByte, offset);
                        break;
                    case 0xec:
                        opStr = String.Format("IN AL, DX");
                        break;
                    case 0xed:
                        opStr = String.Format("IN AX, DX");
                        break;
                    case 0xee:
                        opStr = String.Format("OUT DX, AL");
                        break;
                    case 0xef:
                        opStr = String.Format("OUT DX, AX");
                        break;
                    case 0xf4:
                        opStr = "HLT";
                        break;
                    case 0xf5:
                        opStr = "CMC";
                        break;
                    case 0xf6:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                            case 0x1:
                                opStr = String.Format("TEST {0}, {1:X}", rmData.Operand, operands[1]);
                                break;
                            case 0x2:
                                opStr = String.Format("NOT {0}", rmData.Operand);
                                break;
                            case 0x3:
                                opStr = String.Format("NEG {0}", rmData.Operand);
                                break;
                            case 0x4:
                                opStr = String.Format("MUL {0}", rmData.Operand);
                                break;
                            case 0x5:
                                opStr = String.Format("IMUL {0}", rmData.Operand);
                                break;
                            case 0x6:
                                opStr = String.Format("DIV {0}", rmData.Operand);
                                break;
                            case 0x7:
                                opStr = String.Format("IDIV {0}", rmData.Operand);
                                break;
                        }
                        break;
                    case 0xf7:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                            case 0x1:
                                opStr = String.Format("TEST {0}, {1:X}", rmData.Operand, operands[1]);
                                break;
                            case 0x2:
                                opStr = String.Format("NOT {0}", rmData.Operand);
                                break;
                            case 0x3:
                                opStr = String.Format("NEG {0}", rmData.Operand);
                                break;
                            case 0x4:
                                opStr = String.Format("MUL {0}", rmData.Operand);
                                break;
                            case 0x5:
                                opStr = String.Format("IMUL {0}", rmData.Operand);
                                break;
                            case 0x6:
                                opStr = String.Format("DIV {0}", rmData.Operand);
                                break;
                            case 0x7:
                                opStr = String.Format("IDIV {0}", rmData.Operand);
                                break;
                        }
                        break;
                    case 0xf8:
                        opStr = "CLC";
                        break;
                    case 0xf9:
                        opStr = "STC";
                        break;
                    case 0xfa:
                        opStr = "CLI";
                        break;
                    case 0xfb:
                        opStr = "STI";
                        break;
                    case 0xfc:
                        opStr = "CLD";
                        break;
                    case 0xfd:
                        opStr = "STD";
                        break;
                    case 0xfe:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                grpStr = "INC";
                                break;
                            case 0x1:
                                grpStr = "DEC";
                                break;
                        }
                        opStr = String.Format("{0} {1}", grpStr, rmData.Operand);
                        break;
                    case 0xff:
                        System.Diagnostics.Debug.Assert(rmData != null, "rmData != null");

                        switch (rmData.Register)
                        {
                            case 0x0:
                                opStr = String.Format("INC {0}", rmData.Operand);
                                break;
                            case 0x1:
                                opStr = String.Format("DEC {0}", rmData.Operand);
                                break;
                            case 0x2:
                                opStr = String.Format("CALL {0}", rmData.Operand);
                                break;
                            case 0x3:
                                opStr = String.Format("CALL FAR {0}", rmData.Operand);
                                break;
                            case 0x4:
                                opStr = String.Format("JMP {0}", rmData.Operand);
                                break;
                            case 0x5:
                                opStr = String.Format("JMP FAR {0:X}:{1:X}", operands[1], operands[0]);
                                break;
                            case 0x6:
                                opStr = String.Format("PUSH {0}", rmData.Operand);
                                break;
                        }
                        break;
                }
            }
            return opStr;
        }

        public int Decode(uint eip, out byte opCode, out object[] operands)
        {
            var args = new List<object>();
            RegMemData rmData;
// ReSharper disable TooWideLocalVariableScope
            byte sourceByte;
            ushort destWord, sourceWord;
            uint sourceDWord;
            // ReSharper restore TooWideLocalVariableScope

            CurrentAddr = segments[(int)SegmentRegister.CS].GDTEntry.BaseAddress + eip; 
            var baseAddr = CurrentAddr;

            DecodePrefixes();

            opCode = DecodeReadByte(false);

            if (extPrefix)
            {
                #region extended opcodes
                switch (opCode)
                {
                    case 0x01:          /* R/M ops */
                    case 0x20:
                    case 0x22:
                    case 0xb6:
                        rmData = DecodeRM(true);
                        args.Add(rmData);
                        break;
                    case 0x82:          /* Word ops */
                    case 0x83:
                    case 0x84:
                    case 0x85:
                    case 0x86:
                    case 0x87:
                    case 0x88:
                    case 0x89:
                    case 0x8a:
                    case 0x8b:
                    case 0x8c:
                    case 0x8d:
                    case 0x8e:
                    case 0x8f:
                        if (opSize == 32)
                        {
                            sourceDWord = DecodeReadDWord();
                            args.Add(sourceDWord);
                        }
                        else
                        {
                            sourceWord = DecodeReadWord();
                            args.Add(sourceWord);
                        }
                        break;
                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }
                #endregion
            }
            else
            {
                #region Opcode processing
                switch (opCode)
                {
                    case 0x04: /* byte operand */
                    case 0x0c:
                    case 0x14:
                    case 0x1c:
                    case 0x24:
                    case 0x2c:
                    case 0x34:
                    case 0x3c:
                    case 0x6a:
                    case 0x70:
                    case 0x71:
                    case 0x72:
                    case 0x73:
                    case 0x74:
                    case 0x75:
                    case 0x76:
                    case 0x77:
                    case 0x79:
                    case 0x7a:
                    case 0x7b:
                    case 0x7c:
                    case 0x7d:
                    case 0x7e:
                    case 0x7f:
                    case 0xa8:
                    case 0xb0:
                    case 0xb1:
                    case 0xb2:
                    case 0xb3:
                    case 0xb4:
                    case 0xb5:
                    case 0xb6:
                    case 0xb7:
                    case 0xcd:
                    case 0xd4:
                    case 0xd5:
                    case 0xe0:
                    case 0xe1:
                    case 0xe2:
                    case 0xe3:
                    case 0xe4:
                    case 0xe6:
                    case 0xe7:
                    case 0xeb:
                        sourceByte = DecodeReadByte();

                        args.Add(sourceByte);
                        break;
                    case 0x05: /* Word operand */
                    case 0x0d:
                    case 0x15:
                    case 0x1d:
                    case 0x25:
                    case 0x2d:
                    case 0x35:
                    case 0x3d:
                    case 0x68:
                    case 0xe5:
                    case 0xe8:
                    case 0xa9:
                    case 0xb8:
                    case 0xb9:
                    case 0xba:
                    case 0xbb:
                    case 0xbc:
                    case 0xbd:
                    case 0xbe:
                    case 0xbf:
                    case 0xc2:
                    case 0xca:
                    case 0xe9:
                        if (opSize == 32)
                        {
                            sourceDWord = DecodeReadDWord();
                            args.Add(sourceDWord);
                        }
                        else
                        {
                            sourceWord = DecodeReadWord();
                            args.Add(sourceWord);
                        }
                        break;

                    case 0xa0:
                    case 0xa1: /* Word, memory op */
                    case 0xa2:
                    case 0xa3:
                        if (memSize == 32)
                        {
                            sourceDWord = DecodeReadDWord();
                            args.Add(sourceDWord);
                        }
                        else
                        {
                            sourceWord = DecodeReadWord();
                            args.Add(sourceWord);
                        }
                        break;
                    case 0xc8: /* Word, byte operand */
                        destWord = DecodeReadWord();
                        sourceByte = DecodeReadByte();

                        args.Add(destWord);
                        args.Add(sourceByte);
                        break;
                    case 0x9a: /* word, word operand */
                    case 0xea:
                        if (opSize == 32)
                        {
                            sourceDWord = DecodeReadDWord();
                            destWord = DecodeReadWord();

                            args.Add(sourceDWord);
                            args.Add(destWord);
                        }
                        else
                        {
                            destWord = DecodeReadWord();
                            sourceWord = DecodeReadWord();

                            args.Add(destWord);
                            args.Add(sourceWord);
                        }
                        break;
                    case 0x00: /* reg8/mem8 operand */
                    case 0x02:
                    case 0x08:
                    case 0x0a:
                    case 0x10:
                    case 0x12:
                    case 0x18:
                    case 0x1a:
                    case 0x20:
                    case 0x22:
                    case 0x28:
                    case 0x2a:
                    case 0x30:
                    case 0x32:
                    case 0x38:
                    case 0x3a:
                    case 0x84:
                    case 0x86:
                    case 0x88:
                    case 0x8a:
                        rmData = DecodeRM(false);

                        args.Add(rmData);
                        break;
                    case 0x01: /* reg16/mem16 operand */
                    case 0x03:
                    case 0x09:
                    case 0x0b:
                    case 0x11:
                    case 0x13:
                    case 0x19:
                    case 0x1b:
                    case 0x21:
                    case 0x23:
                    case 0x29:
                    case 0x2b:
                    case 0x31:
                    case 0x33:
                    case 0x39:
                    case 0x89:
                    case 0x85:
                    case 0x87:
                    case 0x8b:
                    case 0x8c:
                    case 0x8d:
                    case 0x8e:
                    case 0x3b:
                    case 0x63:
                    case 0xc4:
                    case 0xc5:
                        rmData = DecodeRM(true);

                        args.Add(rmData);
                        break;
                    case 0x6b: /* reg/mem16, byte operands */
                        rmData = DecodeRM(true);
                        sourceByte = DecodeReadByte();

                        args.Add(rmData);
                        args.Add(sourceByte);
                        break;
                    case 0x69: /* reg/mem16, word operands */
                        rmData = DecodeRM(true);
                        sourceWord = DecodeReadWord();

                        args.Add(rmData);
                        args.Add(sourceWord);
                        break;
                    case 0x80: /* Group - imm8 */
                    case 0xc0:
                        rmData = DecodeRM(false);

                        sourceByte = DecodeReadByte();
                        args.Add(rmData);
                        args.Add(sourceByte);
                        break;
                    case 0x81: /* Group - imm16 */
                        rmData = DecodeRM(true);

                        args.Add(rmData);
                        if (opSize == 32)
                        {
                            sourceDWord = DecodeReadDWord();
                            args.Add(sourceDWord);
                        }
                        else
                        {
                            sourceWord = DecodeReadWord();
                            args.Add(sourceWord);
                        }
                        break;
                    case 0x8f: /* Group */
                        rmData = DecodeRM(true);

                        switch (rmData.Register)
                        {
                            case 0x0:
                                args.Add(rmData);
                                break;
                            default:
                                System.Diagnostics.Debugger.Break();
                                break;
                        }
                        break;
                    case 0x83: /* Group reg16/32, imm8 (SE) */
                    case 0xc1:
                        rmData = DecodeRM(true);

                        sourceByte = DecodeReadByte();
                        args.Add(rmData);
                        args.Add(sourceByte);
                        break;
                    case 0xc6: /* Group */
                        rmData = DecodeRM(false);
                        sourceByte = DecodeReadByte();

                        switch (rmData.Register)
                        {
                            case 0x0:
                                args.Add(rmData);
                                args.Add(sourceByte);
                                break;
                        }
                        break;
                    case 0xc7: /* Group */
                        if (opSize == 32)
                        {
                            rmData = DecodeRM(true);
                            sourceDWord = DecodeReadDWord();

                            switch (rmData.Register)
                            {
                                case 0x0:
                                    args.Add(rmData);
                                    args.Add(sourceDWord);
                                    break;
                            }
                        }
                        else
                        {
                            rmData = DecodeRM(true);
                            sourceWord = DecodeReadWord();

                            switch (rmData.Register)
                            {
                                case 0x0:
                                    args.Add(rmData);
                                    args.Add(sourceWord);
                                    break;
                            }
                        }
                        break;
                    case 0xd0: /* Group reg8 */
                    case 0xd2:
                    case 0xfe:
                        rmData = DecodeRM(false);

                        args.Add(rmData);
                        break;
                    case 0xf6: /* Special */
                        rmData = DecodeRM(false);

                        args.Add(rmData);
                        if (rmData.Register == 0 || rmData.Register == 1)
                            args.Add(DecodeReadByte());
                        break;
                    case 0xf7: /* Special */
                        rmData = DecodeRM(true);

                        args.Add(rmData);
                        if (rmData.Register == 0 || rmData.Register == 1)
                        {
                            if (opSize == 32)
                            {
                                sourceDWord = DecodeReadDWord();
                                args.Add(sourceDWord);
                            }
                            else
                            {
                                sourceWord = DecodeReadWord();
                                args.Add(sourceWord);
                            }
                        }
                        break;
                    case 0xd1:          /* Group reg16 */
                    case 0xd3:
                    case 0xff:
                        rmData = DecodeRM(true);

                        args.Add(rmData);
                        break;
                    case 0x06:          /* No operand */
                    case 0x07:
                    case 0x0e:
                    case 0x16:
                    case 0x17:
                    case 0x1e:
                    case 0x1f:
                    case 0x27:
                    case 0x2f:
                    case 0x37:
                    case 0x3f:
                    case 0x40:
                    case 0x41:
                    case 0x42:
                    case 0x43:
                    case 0x44:
                    case 0x45:
                    case 0x46:
                    case 0x47:
                    case 0x48:
                    case 0x49:
                    case 0x4a:
                    case 0x4b:
                    case 0x4c:
                    case 0x4d:
                    case 0x4e:
                    case 0x4f:
                    case 0x50:
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x54:
                    case 0x55:
                    case 0x56:
                    case 0x57:
                    case 0x58:
                    case 0x59:
                    case 0x5a:
                    case 0x5b:
                    case 0x5c:
                    case 0x5d:
                    case 0x5e:
                    case 0x5f:
                    case 0x60:
                    case 0x61:
                    case 0x6c:
                    case 0x6d:
                    case 0x6e:
                    case 0x6f:
                    case 0x90:
                    case 0x91:
                    case 0x92:
                    case 0x93:
                    case 0x94:
                    case 0x95:
                    case 0x96:
                    case 0x97:
                    case 0x98:
                    case 0x99:
                    case 0x9c:
                    case 0x9d:
                    case 0x9e:
                    case 0x9f:
                    case 0xa4:
                    case 0xa5:
                    case 0xa6:
                    case 0xa7:
                    case 0xaa:
                    case 0xab:
                    case 0xac:
                    case 0xad:
                    case 0xae:
                    case 0xaf:
                    case 0xc3:
                    case 0xc9:
                    case 0xcb:
                    case 0xcc:
                    case 0xce:
                    case 0xcf:
                    case 0xd7:
                    case 0xec:
                    case 0xed:
                    case 0xee:
                    case 0xef:
                    case 0xf4:
                    case 0xf8:
                    case 0xfc:
                    case 0xfa:
                    case 0xf5:
                    case 0xf9:
                    case 0xfb:
                    case 0xfd:
                        break;
                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }
                #endregion
            }

            operands = args.ToArray();

            return (int)(CurrentAddr - baseAddr);
        }
    }
}