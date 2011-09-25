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
        private void CheckOverflow(byte dest, byte source, short result)
        {
            if (result > sbyte.MaxValue || result < sbyte.MinValue)
                OF = true;
            else
                OF = false;
        }

        private void CheckOverflow(ushort dest, ushort source, ushort result)
        {
            var signedDest = (short) dest;
            var signedSource = (short) source;
            var signedResult = (short) result;

            if (signedDest > 0 && signedSource > 0 && signedResult < 0)
                OF = true;
            else if (signedDest < 0 && signedSource < 0 && signedResult > 0)
                OF = true;
            else
                OF = false;
        }

        private void CheckOverflow(uint dest, uint source, uint result)
        {
            var signedDest = (int) dest;
            var signedSource = (int) source;
            var signedResult = (int) result;

            if (signedDest > 0 && signedSource > 0 && signedResult < 0)
                OF = true;
            else if (signedDest < 0 && signedSource < 0 && signedResult > 0)
                OF = true;
            else
                OF = false;
        }

        #region Addition

        private void Add(byte source)
        {
            AL = Add(AL, source);
        }

        private void Add(ushort source)
        {
            AX = Add(AX, source);
        }

        private void Add(uint source)
        {
            EAX = Add(EAX, source);
        }

        private byte Add(byte dest, byte source)
        {
            return DoAdd(dest, source, false);
        }

        private ushort Add(ushort dest, byte source)
        {
            return DoAdd(dest, (ushort)(sbyte)source, false);
        }

        private uint Add(uint dest, byte source)
        {
            return DoAdd(dest, (uint)(sbyte)source, false);
        }

        private ushort Add(ushort dest, ushort source)
        {
            return DoAdd(dest, source, false);
        }

        private uint Add(uint dest, uint source)
        {
            return DoAdd(dest, source, false);
        }

        private void AddWithCarry(byte source)
        {
            AL = AddWithCarry(AL, source);
        }


        private void AddWithCarry(ushort source)
        {
            AX = AddWithCarry(AX, source);
        }

        private void AddWithCarry(uint source)
        {
            EAX = AddWithCarry(EAX, source);
        }

        private byte AddWithCarry(byte dest, byte source)
        {
            return DoAdd(dest, source, true);
        }

        private ushort AddWithCarry(ushort dest, byte source)
        {
            return DoAdd(dest, (ushort)(sbyte)source, true);
        }

        private uint AddWithCarry(uint dest, byte source)
        {
            return DoAdd(dest, (uint)(sbyte)source, true);
        }

        private ushort AddWithCarry(ushort dest, ushort source)
        {
            return DoAdd(dest, source, true);
        }

        private uint AddWithCarry(uint dest, uint source)
        {
            return DoAdd(dest, source, true);
        }

        private byte DoAdd(byte dest, byte source, bool carry)
        {
            var ret = (short) (source + dest);

            if (carry)
                ret += CF ? (short) 1 : (short) 0;

            CheckOverflow(dest, source, (byte) ret);

            CF = ret > byte.MaxValue;

            SetCPUFlags((byte) ret);

            return (byte) ret;
        }

        private ushort DoAdd(ushort dest, ushort source, bool carry)
        {
            var ret = source + dest;

            if (carry)
                ret += CF ? 1 : 0;

            CheckOverflow(dest, source, (ushort) ret);

            CF = ret > ushort.MaxValue;

            SetCPUFlags((ushort)ret);

            return (ushort) ret;
        }

        private uint DoAdd(uint dest, uint source, bool carry)
        {
            var ret = (ulong) (source + dest);

            if (carry)
                ret += (ulong) (CF ? 1 : 0);

            CheckOverflow(dest, source, (uint) ret);

            CF = ret > uint.MaxValue;

            SetCPUFlags((uint) ret);

            return (uint) ret;
        }

        #endregion

        #region Subtraction

        private void Subtract(byte source)
        {
            AL = DoSub(AL, source, false);
        }

        private void Subtract(ushort source)
        {
            AX = DoSub(AX, source, false);
        }

        private void Subtract(uint source)
        {
            EAX = DoSub(EAX, source, false);
        }

        private byte Subtract(byte dest, byte source)
        {
            return DoSub(dest, source, false);
        }

        private ushort Subtract(ushort dest, byte source)
        {
            return DoSub(dest, (ushort)(sbyte)source, false);
        }

        private uint Subtract(uint dest, byte source)
        {
            return DoSub(dest, (uint)(sbyte) source, false);
        }

        private ushort Subtract(ushort dest, ushort source)
        {
            return DoSub(dest, source, false);
        }

        private uint Subtract(uint dest, uint source)
        {
            return DoSub(dest, source, false);
        }

        private void SubWithBorrow(byte source)
        {
            AL = DoSub(AL, source, true);
        }

        private void SubWithBorrow(ushort source)
        {
            AX = DoSub(AX, source, true);
        }

        private void SubWithBorrow(uint source)
        {
            EAX = DoSub(EAX, source, true);
        }

        private byte SubWithBorrow(byte dest, byte source)
        {
            return DoSub(dest, source, true);
        }

        private ushort SubWithBorrow(ushort dest, byte source)
        {
            return DoSub(dest, (ushort)(sbyte)source, true);
        }

        private uint SubWithBorrow(uint dest, byte source)
        {
            return DoSub(dest, (uint)(sbyte)source, true);
        }

        private ushort SubWithBorrow(ushort dest, ushort source)
        {
            return DoSub(dest, source, true);
        }

        private uint SubWithBorrow(uint dest, uint source)
        {
            return DoSub(dest, source, true);
        }

        private byte DoSub(byte dest, byte source, bool borrow)
        {
            if (borrow && CF)
                source++;

            var result = (sbyte) (dest - source);

            CF = dest < source;

            CheckOverflow(dest, source, (short) (dest - source));
            SetCPUFlags((byte) result);

            return (byte) result;
        }

        private ushort DoSub(ushort dest, ushort source, bool borrow)
        {
            short result;

            if (borrow && CF)
                result = (short) (dest - (source + 1));
            else
                result = (short) (dest - source);

            CF = dest < source;

            CheckOverflow(dest, source, (ushort) result);
            SetCPUFlags((ushort) result);

            return (ushort) result;
        }

        private uint DoSub(uint dest, uint source, bool borrow)
        {
            int result;

            if (borrow && CF)
                result = (int) (dest - (source + 1));
            else
                result = (int) (dest - source);

            CF = dest < source;

            CheckOverflow(dest, source, (uint) result);
            SetCPUFlags((uint) result);

            return (uint) result;
        }

        #endregion

        #region Inc/Dec

        private byte Increment(byte dest)
        {
            var tempCF = CF;

            var ret = Add(dest, 1);

            CF = tempCF;

            return ret;
        }

        private ushort Increment(ushort dest)
        {
            var tempCF = CF;

            var ret = Add(dest, 1);

            CF = tempCF;

            return ret;
        }

        private uint Increment(uint dest)
        {
            bool tempCF = CF;

            uint ret = Add(dest, 1);

            CF = tempCF;

            return ret;
        }

        private byte Decrement(byte dest)
        {
            bool tempCF = CF;

            byte ret = Subtract(dest, 1);

            CF = tempCF;

            return ret;
        }

        private ushort Decrement(ushort dest)
        {
            bool tempCF = CF;

            ushort ret = Subtract(dest, 1);

            CF = tempCF;

            return ret;
        }

        private uint Decrement(uint dest)
        {
            bool tempCF = CF;

            uint ret = Subtract(dest, 1);

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

            AL = temp;
            AH = (byte) (AX%source);
        }

        private void SDivide(byte source)
        {
            var temp = (sbyte) ((short) AX/(sbyte) source);

            AL = (byte) temp;
            AH = (byte) (sbyte) ((short) AX%(sbyte) source);
        }

        private void Divide(ushort source)
        {
            var dividend = (uint) (((DX << 16) & 0xFFFF0000) + AX);
            var temp = (ushort) (dividend/source);

            AX = temp;
            DX = (ushort) (dividend%source);
        }

        private void Divide(uint source)
        {
            var dividend = ((EDX << 32) & 0xffffffff00000000) + EAX;
            var temp = (uint) (dividend/source);

            EAX = temp;
            EDX = (uint) (dividend%source);
        }

        private void SDivide(ushort source)
        {
            var dividend = (uint) ((DX << 16 & 0xffff0000) + AX);
            var temp = (short) ((int) dividend/(short) source);

            AX = (ushort) temp;
            DX = (ushort) ((int) dividend/(short) source);
        }

        private void SDivide(uint source)
        {
            var dividend = (EDX << 32 & 0xffffffff00000000) + EAX;
            var temp = (int) ((long) dividend/(int) source);

            EAX = (uint) temp;
            EDX = (uint) ((long) dividend/(int) source);
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