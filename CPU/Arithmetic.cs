namespace x86CS.CPU
{
    public enum ShiftType
    {
        Left,
        ArithmaticLeft,
        Right,
        ArithmaticRight
    }

    public enum RotateType
    {
        LeftWithCarry,
        Left,
        Right,
        RightWithCarry
    }

    public partial class CPU
    {
        private void ProcessArithmetic(Operand[] operands)
        {
            uint source, dest;

            dest = GetOperandValue(operands[0]);
            source = GetOperandValue(operands[1]);

            switch (currentInstruction.Instruction.Opcode)
            {
                case 0x3c:
                    Subtract(dest, source, (int)operands[0].OperandSize);
                    break;
                case 0x04:
                case 0x05:
                case 0x83:
                    dest = Add(GetOperandValue(operands[0]), GetOperandValue(operands[1]), (int)operands[0].OperandSize);
                    SetOperandValue(operands[0], dest);
                    break;
                case 0x80:
                    switch (currentInstruction.Instruction.Mnemonic)
                    {
                        case "cmp ":
                            Subtract(dest, source, (int)operands[0].OperandSize);
                            break;
                        case "add ":
                            dest = Add(GetOperandValue(operands[0]), GetOperandValue(operands[1]), (int)operands[0].OperandSize);
                            SetOperandValue(operands[0], dest);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        private void CheckOverflow(short result)
        {
            if ((sbyte)(byte)result > sbyte.MaxValue || (sbyte)(byte)result < sbyte.MinValue)
                OF = true;
            else
                OF = false;
        }

        private void CheckOverflow(int result)
        {
            if ((short)(ushort)result > short.MaxValue || (short)(ushort)result < short.MinValue)
                OF = true;
            else
                OF = false;
        }

        private void CheckOverflow(long result)
        {
            if ((int)(uint)result > int.MaxValue || (int)(uint)result < int.MinValue)
                OF = true;
            else
                OF = false;
        }

        #region Addition

        private uint Add(uint dest, uint source, int size)
        {
            return DoAdd(dest, source, size, false);
        }

        private uint AddWithCarry(uint dest, uint source, int size)
        {
            return DoAdd(dest, source, size, true);
        }

        private uint DoAdd(uint dest, uint source, int size, bool carry)
        {
            ulong ret;
            
            if (carry)
                source += (uint)(CF ? 1 : 0);

            switch (size)
            {
                case 8:
                    ret = (ushort)((byte)source + (byte)dest);
                    CheckOverflow((short)ret);
                    CF = ret > byte.MaxValue;
                    SetCPUFlags((byte)ret);
                    return (byte)ret;
                case 16:
                    ret = (uint)((ushort)source + (ushort)dest);
                    CheckOverflow((int)ret);
                    CF = ret > ushort.MaxValue;
                    SetCPUFlags((ushort)ret);
                    return (ushort)ret;
                default:
                    ret = (ulong)((uint)source + (uint)dest);
                    CheckOverflow((long)ret);
                    CF = ret > uint.MaxValue;
                    SetCPUFlags((uint)ret);
                    return (uint)ret;
            }
        }

        #endregion

        #region Subtraction

        private uint Subtract(uint dest, uint source, int size)
        {
            return DoSub(dest, source, size, false);
        }

        private uint SubWithBorrow(uint dest, uint source, int size)
        {
            return DoSub(dest, source, size, true);
        }

        private uint DoSub(uint dest, uint source, int size, bool borrow)
        {
            long result;

            if (borrow && CF)
                source++;
            CF = dest < source;

            switch (size)
            {
                case 8:
                    result = (byte)((byte)dest - (byte)source);
                    SetCPUFlags((byte)result);
                    CheckOverflow((short)result);
                    break;
                case 16:
                    result = (ushort)((ushort)dest - (ushort)(short)source);
                    SetCPUFlags((ushort)result);
                    CheckOverflow((int)result);
                    break;
                default:
                    result = (int)(dest - source);
                    SetCPUFlags((uint)result);
                    CheckOverflow(result);
                    break;
            }
            return (uint)result;
        }

        #endregion

        #region Inc/Dec

        private byte Increment(byte dest)
        {
            var tempCF = CF;

            var ret = Add(dest, 1, 8);

            CF = tempCF;

            return (byte)ret;
        }

        private ushort Increment(ushort dest)
        {
            var tempCF = CF;

            var ret = Add(dest, 1, 16);

            CF = tempCF;

            return(ushort) ret;
        }

        private uint Increment(uint dest)
        {
            bool tempCF = CF;

            uint ret = Add(dest, 1, 32);

            CF = tempCF;

            return ret;
        }

        private byte Decrement(byte dest)
        {
            bool tempCF = CF;

            byte ret = (byte)Subtract(dest, 1, 8);

            CF = tempCF;

            return ret;
        }

        private ushort Decrement(ushort dest)
        {
            bool tempCF = CF;

            ushort ret = (ushort)Subtract(dest, 1, 16);

            CF = tempCF;

            return ret;
        }

        private uint Decrement(uint dest)
        {
            bool tempCF = CF;

            uint ret = Subtract(dest, 1, 32);

            CF = tempCF;

            return ret;
        }

        #endregion

        #region Multiply

        private void SignedMultiply(byte source)
        {
            var signedSource = (sbyte) source;
            var temp = (short) ((sbyte) AX*signedSource);

            AX = (ushort) temp;

            if (AH == 0x00 || AH == 0xff)
            {
                CF = false;
                OF = false;
            }
            else
            {
                CF = true;
                OF = true;
            }
        }

        private void SignedMultiply(ushort source)
        {
            var signedSource = (short) source;
            var temp = (short)AX*signedSource;

            AX = (ushort) (temp & 0xFFFF);
            DX = (ushort) ((temp >> 16) & 0xFFFF);

            if (DX == 0x0000 || DX == 0xffff)
            {
                CF = false;
                OF = false;
            }
            else
            {
                CF = true;
                OF = true;
            }
        }

        private void SignedMultiply(uint source)
        {
            var signedSource = (int) source;
            long temp = ((int) EAX*signedSource);

            EAX = (uint) (temp & 0xffffffff);
            EDX = (uint) ((temp >> 32) & 0xffffffff);

            if (EDX == 0x0000 || EDX == 0xffffffff)
            {
                CF = false;
                OF = false;
            }
            else
            {
                CF = true;
                OF = true;
            }
        }

        private ushort SignedMultiply(ushort dest, byte source)
        {
            return SignedMultiply(dest, (ushort)(sbyte)source);
        }

        private uint SignedMultiply(uint dest, byte source)
        {
            return SignedMultiply(dest, (uint)(sbyte)source);
        }

        private ushort SignedMultiply(ushort dest, ushort source)
        {
            var signedDest = (short) dest;
            var signedSource = (short) source;
            var temp = signedDest*signedSource;
            var ret = (short) (signedDest*signedSource);

            if (temp != ret)
            {
                CF = true;
                OF = true;
            }
            else
            {
                CF = false;
                OF = false;
            }

            return (ushort) ret;
        }

        private uint SignedMultiply(uint dest, uint source)
        {
            var signedDest = (int) dest;
            var signedSource = (int) source;

            long temp = signedDest*signedSource;
            var ret = signedDest*signedSource;

            if (temp != ret)
            {
                CF = true;
                OF = true;
            }
            else
            {
                CF = false;
                OF = false;
            }

            return (uint) ret;
        }

        private void Multiply(byte source)
        {
            AX = (ushort) (AL*source);

            if (AH == 0)
            {
                OF = false;
                CF = false;
            }
            else
            {
                OF = true;
                CF = true;
            }
        }

        private void Multiply(ushort source)
        {
            uint mulResult = (uint) AX*source;

            AX = (ushort) (mulResult & 0xFFFF);
            DX = (ushort) (mulResult >> 16);

            if (DX == 0)
            {
                OF = false;
                CF = false;
            }
            else
            {
                OF = true;
                CF = true;
            }
        }

        private void Multiply(uint source)
        {
            ulong mulResult = (ulong) EAX*source;

            EAX = (ushort) (mulResult & 0xffffffff);
            EDX = (ushort) (mulResult >> 32);

            if (EDX == 0)
            {
                OF = false;
                CF = false;
            }
            else
            {
                OF = true;
                CF = true;
            }
        }

        #endregion

        #region Divides

        private void Divide(byte source)
        {
            var temp = (byte) (AX/source);
            var remainder = (byte)(AX%source);

            AL = temp;
            AH = remainder;
        }

        private void SDivide(byte source)
        {
            var temp = (sbyte) ((short) AX/(sbyte) source);
            var remainder = (sbyte)((short)AX % (sbyte)source);

            AL = (byte) temp;
            AH = (byte)remainder;
        }

        private void Divide(ushort source)
        {
            var dividend = (uint) (((DX << 16) & 0xFFFF0000) + AX);
            var temp = (ushort) (dividend/source);
            var remainder = (ushort)(dividend%source);

            AX = temp;
            DX = remainder;
        }

        private void Divide(uint source)
        {
            var dividend = ((EDX << 32) & 0xffffffff00000000) + EAX;
            var temp = (uint) (dividend/source);
            var remainder = (uint) (dividend%source);

            EAX = temp;
            EDX = remainder;
        }

        private void SDivide(ushort source)
        {
            var dividend = (uint) ((DX << 16 & 0xffff0000) + AX);
            var temp = (short) ((int) dividend/(short) source);
            var remainder = (short) ((int) dividend%(short) source);

            AX = (ushort) temp;
            DX = (ushort) remainder;
        }

        private void SDivide(uint source)
        {
            var dividend = (EDX << 32 & 0xffffffff00000000) + EAX;
            var temp = (int) ((long) dividend/(int) source);
            var remainder = (int) ((long) dividend%(int) source);

            EAX = (uint) temp;
            EDX = (uint) remainder;
        }

        #endregion

        private byte Rotate(byte dest, byte count, RotateType type)
        {
            return (byte) DoRotate(dest, count, type, 8);
        }

        private ushort Rotate(ushort dest, ushort count, RotateType type)
        {
            return (ushort) DoRotate(dest, count, type, 16);
        }

        private uint Rotate(uint dest, uint count, RotateType type)
        {
            return DoRotate(dest, count, type, 32);
        }

        private uint DoRotate(uint dest, uint count, RotateType type, int size)
        {
            long tempCount;
            bool tempCF;
            uint ret = dest;

            if (type == RotateType.LeftWithCarry || type == RotateType.RightWithCarry)
            {
                if (size == 8)
                    tempCount = ((count & 0x1f)%9);
                else if (size == 16)
                    tempCount = ((count & 0x1f)%17);
                else
                    tempCount = (count & 0x1f);
            }
            else
            {
                tempCount = count%size;
            }
            if (type == RotateType.LeftWithCarry)
            {
                while (tempCount != 0)
                {
                    if (size == 8)
                        tempCF = ((ret & 0x80) == 0x80);
                    else if (size == 16)
                        tempCF = ((ret & 0x8000) == 0x8000);
                    else
                        tempCF = ((ret & 0x80000000) == 0x80000000);

                    ret = (ushort) ((ret*2) + (CF ? 1 : 0));

                    CF = tempCF;
                    tempCount--;
                }

                if (count == 1)
                {
                    if (size == 8)
                        OF = ((ret & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else if (size == 16)
                        OF = ((ret & 0x8000) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((ret & 0x80000000) ^ (CF ? 1 : 0)) != 0;
                }
            }
            else if (type == RotateType.RightWithCarry)
            {
                if (count == 1)
                {
                    if (size == 8)
                        OF = ((dest & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else if (size == 16)
                        OF = ((dest & 0x8000) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((dest & 0x80000000) ^ (CF ? 1 : 0)) != 0;
                }

                while (tempCount != 0)
                {
                    tempCF = ((dest & 0x01) == 0x01);
                    ret /= 2;

                    if (size == 8)
                        ret += (uint) (CF ? 256 : 0);
                    else if (size == 16)
                        ret += (uint) (CF ? 65536 : 0);
                    else
                        ret += (uint) (CF ? 4294967296 : 0);

                    CF = tempCF;

                    tempCount--;
                }
            }
            else if (type == RotateType.Left)
            {
                while (tempCount != 0)
                {
                    if (size == 8)
                        tempCF = ((ret & 0x80) == 0x80);
                    else if (size == 16)
                        tempCF = ((ret & 0x8000) == 0x8000);
                    else
                        tempCF = ((ret & 0x80000000) == 0x80000000);

                    ret = (ushort) ((ret*2) + (CF ? 1 : 0));
                    tempCount--;
                }
                CF = ((ret & 0x01) == 0x01);
                if (count == 1)
                {
                    if (size == 8)
                        OF = ((dest & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else if (size == 16)
                        OF = ((dest & 0x8000) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((dest & 0x80000000) ^ (CF ? 1 : 0)) != 0;
                }
            }
            else
            {
                while (tempCount != 0)
                {
                    tempCF = ((dest & 0x01) == 0x01);
                    ret /= 2;
                    switch (size)
                    {
                        case 8:
                            ret += (uint) (CF ? byte.MaxValue + 1 : 0);
                            break;
                        case 16:
                            ret += (uint) (CF ? ushort.MaxValue + 1 : 0);
                            break;
                        default:
                            ret += (uint) (CF ? 4294967296 : 0);
                            break;
                    }

                    tempCount--;
                }
                switch (size)
                {
                    case 8:
                        CF = ((ret & 0x80) == 0x80);
                        break;
                    case 32:
                        CF = ((ret & 0x8000) == 0x8000);
                        break;
                    default:
                        CF = ((ret & 0x80000000) == 0x80000000);
                        break;
                }

                if (count == 1)
                {
                    switch (size)
                    {
                        case 8:
                            OF = ((((ret & 0x80) == 0x80) ^ ((ret & 0x40) == 0x40)));
                            break;
                        case 16:
                            OF = ((((ret & 0x8000) == 0x8000) ^ ((ret & 0x4000) == 0x4000)));
                            break;
                        default:
                            OF = ((((ret & 0x80000000) == 0x80000000) ^ ((ret & 0x40000000) == 0x40000000)));
                            break;
                    }
                }
            }

            return ret;
        }

        private byte Shift(byte source, byte count, ShiftType type)
        {
            var dest = source;
            var tempCount = (byte) (count & 0x1f);
            var tempDest = dest;

            while (tempCount != 0)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    CF = ((dest & 0x80) == 0x80);
                    dest *= 2;
                }
                else
                {
                    CF = ((dest & 0x01) == 0x01);
                    if (type == ShiftType.ArithmaticRight)
                        dest = (byte) ((sbyte) dest/2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    var tmp = (byte) ((dest & 0x80) ^ (CF ? 1 : 0));
                    OF = (tmp != 0);
                }
                else if (type == ShiftType.ArithmaticRight)
                    OF = false;
                else
                    OF = ((tempDest & 0x80) == 0x80);
            }

            return dest;
        }

        private ushort Shift(ushort source, ushort count, ShiftType type)
        {
            var dest = source;
            var tempCount = (ushort) (count & 0x1f);
            var tempDest = dest;

            while (tempCount != 0)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    CF = ((dest & 0x8000) == 0x8000);
                    dest *= 2;
                }
                else
                {
                    CF = ((dest & 0x0001) == 0x0001);
                    if (type == ShiftType.ArithmaticRight)
                        dest = (ushort) ((short) dest/2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    var tmp = (ushort) ((dest & 0x8000) ^ (CF ? 1 : 0));
                    OF = (tmp != 0);
                }
                else if (type == ShiftType.ArithmaticRight)
                    OF = false;
                else
                    OF = ((tempDest & 0x8000) == 0x8000);
            }

            return dest;
        }

        private uint Shift(uint source, uint count, ShiftType type)
        {
            var dest = source;
            var tempCount = count & 0x1f;
            var tempDest = dest;

            while (tempCount != 0)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    CF = ((dest & 0x80000000) == 0x8000000);
                    dest *= 2;
                }
                else
                {
                    CF = ((dest & 0x1) == 0x1);
                    if (type == ShiftType.ArithmaticRight)
                        dest = (uint) ((int) dest/2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    var tmp = (uint) ((dest & 0x80000000) ^ (CF ? 1 : 0));
                    OF = (tmp != 0);
                }
                else if (type == ShiftType.ArithmaticRight)
                    OF = false;
                else
                    OF = ((tempDest & 0x80000000) == 0x80000000);
            }

            return dest;
        }
    }
}