using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;

namespace x86CS
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
        SIMD = 0x00000800
    }

    public class RegMemData
    {
        public byte Mode;
        public byte Register;
        public byte RegMem;
        public bool IsRegister;
        public bool HasDisplacement;
        public ushort Displacement;
        public string Operand;
        public string RegisterName;
    }

    public partial class CPU
    {
        private OPPrefix setPrefixes = 0;
        private uint currentAddr;
        private SegmentRegister overrideSegment = SegmentRegister.DS;

        #region Read Functions
        private byte DecodeReadByte()
        {
            uint ret;

            ret = (uint)(Memory.ReadByte(currentAddr) & 0xff);
            currentAddr++;

            return (byte)ret;
        }

        private ushort DecodeReadWord()
        {
            uint ret;

            ret = (uint)(Memory.ReadWord(currentAddr) & 0xffff);
            currentAddr += 2;

            return (ushort)ret;
        }

        private uint DecodeReadDWord()
        {
            uint ret;
            ret = Memory.ReadWord(currentAddr);
            currentAddr += 4;

            return ret;
        }
        #endregion

        private string GetPrefixString(SegmentRegister defaultseg)
        {
            if (overrideSegment == defaultseg)
                return "";
            else
                return overrideSegment.ToString() + ":";
        }

        private void DecodePrefixes()
        {
            bool readPrefix = false;
            byte opCode;

            do
            {
                opCode = DecodeReadByte();

                switch (opCode)
                {
                    case 0x0f:
                        setPrefixes |= OPPrefix.SIMD;
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
                        currentAddr--;
                        break;
                }
            } while(readPrefix == true);

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
        }

        private RegMemData DecodeRM(bool word)
        {
            RegMemData ret = new RegMemData();
            byte modRegRm;

            modRegRm = DecodeReadByte();

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
                            ret.Operand = GetPrefixString(SegmentRegister.DS) + "[BX + SI";
                            break;
                        case 0x1:
                            ret.Operand = GetPrefixString(SegmentRegister.DS) + "[BX + DI";
                            break;
                        case 0x2:
                            ret.Operand = GetPrefixString(SegmentRegister.SS) + "[BP + SI";
                            break;
                        case 0x3:
                            ret.Operand = GetPrefixString(SegmentRegister.SS) + "[BP + DI";
                            break;
                        case 0x4:
                            ret.Operand = GetPrefixString(SegmentRegister.DS) + "[SI";
                            break;
                        case 0x5:
                            ret.Operand = GetPrefixString(SegmentRegister.DS) + "[DI";
                            break;
                        case 0x6:
                            if (ret.Mode == 0x1 || ret.Mode == 0x2)
                                ret.Operand = GetPrefixString(SegmentRegister.SS) + "[BP + ";
                            else
                                ret.Operand = GetPrefixString(SegmentRegister.DS) + "[";
                            break;
                        case 0x7:
                            ret.Operand = GetPrefixString(SegmentRegister.DS) + "[BX";
                            break;
                        default:
                            ret.Operand = "INVALID";
                            break;
                    }
                    if (ret.Mode == 0x1)
                    {
                        byte byteOp;
                        short tmpDisp;

                        byteOp = DecodeReadByte();
                        tmpDisp = (short)byteOp;

                        if (tmpDisp < 0)
                            ret.Operand += " - " + (-tmpDisp).ToString("X4") + "]";
                        else
                            ret.Operand += " + " + byteOp.ToString("X4") + "]";

                        ret.Displacement = (ushort)tmpDisp;
                        ret.HasDisplacement = true;
                    }
                    else if (ret.Mode == 0x2)
                    {
                        ushort wordOp;

                        wordOp = DecodeReadWord();

                        ret.Operand += " + " + wordOp.ToString("X4") + "]";

                        ret.Displacement = wordOp;
                        ret.HasDisplacement = true;
                    }
                    else if (ret.RegMem == 0x6)
                    {
                        ret.Displacement = DecodeReadWord();
                        ret.HasDisplacement = true;

                        ret.Operand += ret.Displacement.ToString("X4") + "]";
                    }
                    else
                        ret.Operand += "]";
                    break;
                case 0x03:
                    if (word)
                        ret.Operand = GetRegStr(ret.RegMem);
                    else
                        ret.Operand = GetByteRegStr(ret.RegMem);
                    break;
                default:
                    ret.Operand = "INVALID";
                    break;
            }

            if (word)
                ret.RegisterName = GetRegStr(ret.Register);
            else
                ret.RegisterName = GetByteRegStr(ret.Register);

            return ret;
        }

        private string DecodeOpString(byte opCode, object[] operands)
        {
            string opStr = "";
            string grpStr = "";
            RegMemData rmData;
            sbyte signedOp = 0;
            short signedWord = 0;
            ushort offset = 0;
            
            rmData = operands[0] as RegMemData;

            if (operands[0] is byte)
            {
                signedOp = (sbyte)(byte)operands[0];
                offset = (ushort)(currentAddr + signedOp);
            }

            if (operands[0] is ushort)
            {
                signedWord = (short)operands[0];
                offset = (ushort)(currentAddr + signedWord);
            }

            switch (opCode)
            {
                case 0x00:
                case 0x01:
                    opStr = String.Format("ADD {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x08:
                case 0x09:
                    opStr = String.Format("OR {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x10:
                case 0x11:
                    opStr = String.Format("ADC {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x18:
                case 0x19:
                    opStr = String.Format("SubWithBorrow {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x20:
                case 0x21:
                    opStr = String.Format("AND {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x28:
                case 0x29:
                    opStr = String.Format("SUB {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x30:
                case 0x31:
                    opStr = String.Format("XOR {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x38:
                case 0x39:
                    opStr = String.Format("CMP {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x02:
                case 0x03:
                    opStr = String.Format("ADD {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x0a:
                case 0x0b:
                    opStr = String.Format("OR {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x12:
                case 0x13:
                    opStr = String.Format("ADC {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x1a:
                case 0x1b:
                    opStr = String.Format("SubWithBorrow {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x22:
                case 0x23:
                    opStr = String.Format("AND {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x2a:
                case 0x2b:
                    opStr = String.Format("SUB {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x32:
                case 0x33:
                    opStr = String.Format("XOR {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x3a:
                case 0x3b:
                    opStr = String.Format("CMP {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x04:
                    opStr = String.Format("ADD AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x0c:
                    opStr = String.Format("OR AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x14:
                    opStr = String.Format("ADC AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x1c:
                    opStr = String.Format("SubWithBorrow AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x24:
                    opStr = String.Format("AND AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x2c:
                    opStr = String.Format("SUB AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x34:
                    opStr = String.Format("XOR AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x3c:
                    opStr = String.Format("CMP AL, {0:X2}", (byte)operands[0]);
                    break;
                case 0x05:
                    opStr = String.Format("ADD AX, {0:X4}", (ushort)operands[0]);
                    break;
                case 0x0d:
                    opStr = String.Format("OR AX, {0:X4}", (ushort)operands[0]);
                    break;
                case 0x15:
                    opStr = String.Format("ADC AX, {0:X4}", (ushort)operands[0]);
                    break;
                case 0x1d:
                    opStr = String.Format("SubWithBorrow AX, {0:X4}", (ushort)operands[0]);
                    break;
                case 0x25:
                    opStr = String.Format("AND AX, {0:X4}", (ushort)operands[0]);
                    break;
                case 0x2d:
                    opStr = String.Format("SUB AX, {0:X4}", (ushort)operands[0]);
                    break;
                case 0x35:
                    opStr = String.Format("XOR AX, {0:X4}", (ushort)operands[0]);
                    break;
                case 0x3d:
                    opStr = String.Format("CMP AX, {0:X4}", (ushort)operands[0]);
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
                    opStr = String.Format("PUSH {0:X4}", (ushort)operands[0]);
                    break;
                case 0x69:
                    opStr = String.Format("IMUL {0}, {1}, {2:X4}", rmData.RegisterName, rmData.Operand, (ushort)operands[1]);
                    break;
                case 0x6a:
                    opStr = String.Format("PUSH {0:X2}", (byte)operands[0]);
                    break;
                case 0x6b:
                    opStr = String.Format("IMUL {0}, {1}, {2:X4}", rmData.RegisterName, rmData.Operand, (ushort)((short)(sbyte)operands[1]));
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
                    opStr = String.Format("JO {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x71:
                    opStr = String.Format("JNO {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x72:
                    opStr = String.Format("JB {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x73:
                    opStr = String.Format("JNB {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x74:
                    opStr = String.Format("JZ {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x75:
                    opStr = String.Format("JNZ {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x76:
                    opStr = String.Format("JBE {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x77:
                    opStr = String.Format("JNBE {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x78:
                    opStr = String.Format("JS {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x79:
                    opStr = String.Format("JNS {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x7a:
                    opStr = String.Format("JP {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x7b:
                    opStr = String.Format("JP {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x7c:
                    opStr = String.Format("JL {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x7d:
                    opStr = String.Format("JL {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x7e:
                    opStr = String.Format("JLE {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x7f:
                    opStr = String.Format("JNLE {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0x80:
                case 0x82:
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
                            grpStr = "SubWithBorrow";
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
                    opStr = String.Format("{0} {1}, {2:X2}", grpStr, rmData.Operand, (byte)operands[1]);
                    break;
                case 0x81:
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
                            grpStr = "SubWithBorrow";
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
                    opStr = String.Format("{0} {1}, {2:X4}", grpStr, rmData.Operand, (ushort)operands[1]);
                    break;
                case 0x83:
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
                            grpStr = "SubWithBorrow";
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
                    opStr = String.Format("{0} {1}, {2:X4}", grpStr, rmData.Operand, (ushort)((short)(sbyte)(byte)operands[1]));
                    break;
                case 0x84:
                case 0x85:
                    opStr = String.Format("TEST {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x86:
                case 0x87:
                    opStr = String.Format("XCHG {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x88:
                case 0x89:
                    opStr = String.Format("MOV {0}, {1}", rmData.Operand, rmData.RegisterName);
                    break;
                case 0x8a:
                case 0x8b:
                    opStr = String.Format("XCHG {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x8c:
                    opStr = String.Format("MOV {0}, {1}", rmData.Operand, ((SegmentRegister)rmData.Register).ToString());
                    break;
                case 0x8d:
                    opStr = String.Format("LEA {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0x8e:
                    opStr = String.Format("MOV {0}, {1}", ((SegmentRegister)rmData.Register).ToString(), rmData.Operand);
                    break;
                case 0x8f:
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
                    opStr = String.Format("CALL FAR {0:X4}:{1:X4}", operands[1], operands[0]);
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
                    opStr = String.Format("MOV AL, {0}{1:X4}", GetPrefixString(SegmentRegister.DS), operands[0]);
                    break;
                case 0xa1:
                    opStr = String.Format("MOV AX, {0}{1:X4}", GetPrefixString(SegmentRegister.DS), operands[0]);
                    break;
                case 0xa2:
                    opStr = String.Format("MOV {0}{1:X4}, AL", GetPrefixString(SegmentRegister.DS), operands[0]);
                    break;
                case 0xa3:
                    opStr = String.Format("MOV {0}{1:X4}, AX", GetPrefixString(SegmentRegister.DS), operands[0]);
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
                    opStr = String.Format("TEST AL, {0:X2}", operands[0]);
                    break;
                case 0xa9:
                    opStr = String.Format("TEST AX, {0:X4}", operands[0]);
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
                    opStr = String.Format("MOV {0}, {1:X2}", GetByteRegStr(opCode - 0xb0), operands[0]);
                    break;
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    opStr = String.Format("MOV {0}, {1:X4}", GetRegStr(opCode - 0xb0), operands[0]);
                    break;
                case 0xc0:
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
                    opStr = String.Format("{0} {1}, {2:X2}", grpStr, rmData.Operand, operands[1]);
                    break;
                case 0xc1:
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
                    opStr = String.Format("{0} {1}, {2:X4}", grpStr, rmData.Operand, (ushort)((short)(sbyte)(byte)operands[1]));
                    break;
                case 0xc2:
                    opStr = String.Format("RETN {0:X4}", operands[0]);
                    break;
                case 0xc3:
                    opStr = "RETN";
                    break;
                case 0xc4:
                    opStr = String.Format("LES {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0xc5:
                    opStr = String.Format("LDS {0}, {1}", rmData.RegisterName, rmData.Operand);
                    break;
                case 0xc6:
                    opStr = String.Format("MOV {0}, {1:X2}", rmData.Operand, operands[1]);
                    break;
                case 0xc7:
                    opStr = String.Format("MOV {0}, {1:X4}", rmData.Operand, operands[1]);
                    break;
                case 0xc8:
                    opStr = String.Format("ENTER {0:X4}, {1:X2}", operands[0], operands[1]);
                    break;
                case 0xc9:
                    opStr = "LEAVE";
                    break;
                case 0xca:
                    opStr = String.Format("RET FAR {0:X4}", operands[0]);
                    break;
                case 0xcb:
                    opStr = "RET FAR";
                    break;
                case 0xcc:
                    opStr = "INT 3 (Special)";
                    break;
                case 0xcd:
                    opStr = String.Format("INT {0:X2}", operands[0]);
                    break;
                case 0xce:
                    opStr = "INTO";
                    break;
                case 0xcf:
                    opStr = "IRET";
                    break;
                case 0xd0:
                case 0xd1:
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
                    opStr = String.Format("LOOPNZ {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0xe1:
                    opStr = String.Format("LOOPZ {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0xe2:
                    opStr = String.Format("LOOP {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0xe3:
                    opStr = String.Format("JCXZ {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0xe4:
                    opStr = String.Format("IN AL, {0:X2}", operands[0]);
                    break;
                case 0xe5:
                    opStr = String.Format("IN AX, {0:X2}", operands[0]);
                    break;
                case 0xe6:
                    opStr = String.Format("OUX {0:X2}, AL", operands[0]);
                    break;
                case 0xe7:
                    opStr = String.Format("OUT {0:X2}, AX", operands[0]);
                    break;
                case 0xe8:
                    opStr = String.Format("CALL {0:X4} ({1:X4})", signedWord, offset);
                    break;
                case 0xe9:
                    opStr = String.Format("JMP {0:X4} ({1:X4})", signedWord, offset);
                    break;
                case 0xea:
                    opStr = String.Format("JMP FAR {0:X4}:{1:X4}", operands[1], operands[0]);
                    break;
                case 0xeb:
                    opStr = String.Format("JMP {0:X2} ({1:X4})", signedOp, offset);
                    break;
                case 0xec:
                    opStr = String.Format("IN AL, DX");
                    break;
                case 0xed:
                    opStr = String.Format("IN AX, DX");
                    break;
                case 0xee:
                    opStr = String.Format("OUX DX, AL");
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
                    switch (rmData.Register)
                    {
                        case 0x0:
                        case 0x1:
                            opStr = String.Format("TEST {0}, {1:X2}", rmData.Operand, operands[1]);
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
                    switch (rmData.Register)
                    {
                        case 0x0:
                        case 0x1:
                            opStr = String.Format("TEST {0}, {1:X4}", rmData.Operand, operands[1]);
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
                            opStr = String.Format("CALL FAR {0:X4}:{1:X4}", operands[1], operands[0]);
                            break;
                        case 0x4:
                            opStr = String.Format("JMP {0}", rmData.Operand);
                            break;
                        case 0x5:
                            opStr = String.Format("JMP FAR {0:X4}:{1:X4}", operands[1], operands[0]);
                            break;
                        case 0x6:
                            opStr = String.Format("PUSH {0}", rmData.Operand);
                            break;
                    }
                    break;
            }

            return opStr;
        }

        public int Decode(ushort cs, ushort ip, out byte opCode, out string opStr, out object[] operands)
        {
            List<Object> args = new List<object>();
            RegMemData regMem;
            uint baseAddr;
            byte srcByte;
            ushort destWord, srcWord;

            opCode = 0;

            currentAddr = (uint)((cs << 4) + ip);
            baseAddr = currentAddr;

            DecodePrefixes();

            opCode = DecodeReadByte();

            #region Opcode processing
            switch (opCode)
            {
                case 0x04:          /* byte operand */
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
                case 0xa2:
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
                    srcByte = DecodeReadByte();

                    args.Add(srcByte);
                    break;
                case 0x05:          /* Word operand */
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
                case 0xa0:
                case 0xa1:
                case 0xa3:
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
                case 0xea:
                    srcWord = DecodeReadWord();

                    args.Add(srcWord);
                    break;
                case 0xc8:          /* Word, byte operand */
                    destWord = DecodeReadWord();
                    srcByte = DecodeReadByte();

                    args.Add(destWord);
                    args.Add(srcByte);
                    break;
                case 0x9a:          /* word, word operand */
                    destWord = DecodeReadWord();
                    srcWord = DecodeReadWord();

                    args.Add(destWord);
                    args.Add(srcWord);
                    break;

                case 0x00:          /* reg8/mem8 operand */
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
                    regMem = DecodeRM(false);

                    args.Add(regMem);
                    break;
                case 0x01:          /* reg16/mem16 operand */
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
                    regMem = DecodeRM(true);

                    args.Add(regMem);
                    break;
                case 0x6b:          /* reg/mem16, byte operands */
                    regMem = DecodeRM(true);
                    srcByte = DecodeReadByte();

                    args.Add(regMem);
                    args.Add(srcByte);
                    break;
                case 0x69:          /* reg/mem16, word operands */
                    regMem = DecodeRM(true);
                    srcWord = DecodeReadWord();

                    args.Add(regMem);
                    args.Add(srcWord);
                    break;
                case 0x80:          /* Group - imm8 */
                case 0xc0:
                    regMem = DecodeRM(false);

                    srcByte = DecodeReadByte();
                    args.Add(regMem);
                    args.Add(srcByte);
                    break;
                case 0x81:          /* Group - imm16 */
                    regMem = DecodeRM(true);

                    srcWord = DecodeReadWord();
                    args.Add(regMem);
                    args.Add(srcWord);
                    break;
                case 0x8f:          /* Group */
                    regMem = DecodeRM(true);

                    switch (regMem.Register)
                    {
                        case 0x0:
                            args.Add(regMem);
                            break;
                        default:
                            break;
                    }
                    break;
                case 0x83:          /* Group reg16, imm8 (SE) */
                case 0xc1:
                    regMem = DecodeRM(true);

                    srcByte = DecodeReadByte();
                    args.Add(regMem);
                    args.Add(srcByte);
                    break;
                case 0xc6:          /* Group */
                    regMem = DecodeRM(false);

                    switch (regMem.Register)
                    {
                        case 0x0:
                            args.Add(regMem);
                            break;
                    }
                    break;
                case 0xc7:          /* Group */
                    regMem = DecodeRM(true);

                    switch (regMem.Register)
                    {
                        case 0x0:
                            args.Add(regMem);
                            break;
                    }
                    break;
                case 0xd0:          /* Group reg8 */
                case 0xd2:
                case 0xf6:
                case 0xfe:
                    regMem = DecodeRM(false);

                    args.Add(regMem);
                    break;
                case 0xd1:          /* Group reg16 */
                case 0xd3:
                case 0xf7:
                case 0xff:
                    regMem = DecodeRM(true);

                    args.Add(regMem);
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
            }
            #endregion

            operands = args.ToArray();

            opStr = DecodeOpString(opCode, operands);

            return (int)(currentAddr - baseAddr);
        }
    }
}