using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS
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
        private void CheckOverflow(byte dest, byte source, byte result)
        {
            sbyte signedDest, signedSource, signedResult;

            signedDest = (sbyte)dest;
            signedSource = (sbyte)source;
            signedResult = (sbyte)result;

            if (signedDest > 0 && signedSource > 0 && signedResult < 0)
                OF = true;
            else if (signedDest < 0 && signedSource < 0 && signedResult > 0)
                OF = true;
            else
                OF = false;
        }

        private void CheckOverflow(ushort dest, ushort source, ushort result)
        {
            short signedDest, signedSource, signedResult;

            signedDest = (short)dest;
            signedSource = (short)source;
            signedResult = (short)result;

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

        private byte Add(byte dest, byte source)
        {
            return DoAdd(dest, source, false);
        }

        private ushort Add(ushort dest, byte source)
        {
            return DoAdd(dest, (ushort)(short)(sbyte)source, false);
        }

        private ushort Add(ushort dest, ushort source)
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


        private byte AddWithCarry(byte dest, byte source)
        {
            return DoAdd(dest, source, true);
        }

        private ushort AddWithCarry(ushort dest, byte source)
        {
            return DoAdd(dest, (ushort)(short)(sbyte)source, true);
        }

        private ushort AddWithCarry(ushort dest, ushort source)
        {
            return DoAdd(dest, source, true);
        }

        private byte DoAdd(byte dest, byte source, bool carry)
        {
            short ret;

            ret = (short)(source + dest);
            if (carry)
                ret += CF ? (short)1 : (short)0;

            CheckOverflow(dest, source, (byte)ret);

            if (ret > byte.MaxValue)
                CF = true;
            else
                CF = false;

            SetCPUFlags((byte)ret);

            return (byte)ret;
        }

        private ushort DoAdd(ushort dest, ushort source, bool carry)
        {
            int ret;

            ret = (int)(source + dest);

            if (carry)
                ret += CF ? 1 : 0;

            CheckOverflow(dest, source, (ushort)ret);

            if (ret > ushort.MaxValue)
                CF = true;
            else
                CF = false;

            SetCPUFlags((ushort)ret);

            return (ushort)ret;
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

        private byte Subtract(byte dest, byte source)
        {
            return DoSub(dest, source, false);
        }

        private ushort Subtract(ushort dest, byte source)
        {
            return DoSub(dest, (ushort)(short)(sbyte)source, false);
        }

        private ushort Subtract(ushort dest, ushort source)
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

        private byte SubWithBorrow(byte dest, byte source)
        {
            return DoSub(dest, source, true);
        }

        private ushort SubWithBorrow(ushort dest, byte source)
        {
            return DoSub(dest, (ushort)(short)(sbyte)source, true);
        }

        private ushort SubWithBorrow(ushort dest, ushort source)
        {
            return DoSub(dest, source, true);
        }

        private byte DoSub(byte dest, byte source, bool borrow)
        {
            sbyte result;

            if (borrow && CF)
                result = (sbyte)(dest - (source + 1));
            else
                result = (sbyte)(dest - source);

            if (dest < source)
                CF = true;
            else
                CF = false;

            CheckOverflow(dest, source, (byte)result);
            SetCPUFlags((byte)result);

            return (byte)result;
        }

        private ushort DoSub(ushort dest, ushort source, bool borrow)
        {
            short result;

            if (borrow && CF)
                result = (short)(dest - (source + 1));
            else
                result = (short)(dest - source);

            if (dest < source)
                CF = true;
            else
                CF = false;

            CheckOverflow(dest, source, (ushort)result);
            SetCPUFlags((ushort)result);

            return (ushort)result;
        }
        #endregion

        #region Inc/Dec
        private byte Increment(byte dest)
        {
            bool tempCF = CF;
            byte ret;

            ret = Add(dest, 1);

            CF = tempCF;

            return ret;
        }

        private ushort Increment(ushort dest)
        {
            bool tempCF = CF;
            ushort ret;

            ret = Add(dest, 1);

            CF = tempCF;

            return ret;
        }
        private byte Decrement(byte dest)
        {
            bool tempCF = CF;
            byte ret;

            ret = Subtract(dest, 1);

            CF = tempCF;

            return ret;
        }

        private ushort Decrement(ushort dest)
        {
            bool tempCF = CF;
            ushort ret;

            ret = Subtract(dest, 1);

            CF = tempCF;

            return ret;
        }
        #endregion

        #region Multiply
        private void SignedMultiply(byte source)
        {
            sbyte signedSource;
            short temp;

            signedSource = (sbyte)source;
            temp = (short)((sbyte)AX * signedSource);

            AX = (ushort)temp;

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
            short signedSource;
            int temp;

            signedSource = (short)source;
            temp = (int)((short)AX * signedSource);

            AX = (ushort)(temp & 0xFFFF);
            DX = (ushort)((temp >> 16) & 0xFFFF);

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

        private ushort SignedMultiple(ushort dest, byte source)
        {
            return SignedMultiply(dest, (ushort)(short)(sbyte)source);
        }

        private ushort SignedMultiply(ushort dest, ushort source)
        {
            short signedDest, signedSource;
            short ret;
            int temp;

            signedDest = (short)dest;
            signedSource = (short)source;

            temp = signedDest * signedSource;
            ret = (short)(signedDest * signedSource);

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

            return (ushort)ret;
        }

        private void Multiply(byte source)
        {
            AX = (ushort)(AL * source);

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
            uint mulResult = (uint)AX * source;

            AX = (ushort)(mulResult & 0xFFFF);
            DX = (ushort)(mulResult >> 16);

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
        #endregion

        #region Divides
        private void Divide(byte source)
        {
            byte temp;

            temp = (byte)(AX / source);

            AL = temp;
            AH = (byte)(AX % source);
        }

        private void SDivide(byte source)
        {
            sbyte temp;

            temp = (sbyte)((short)AX / (sbyte)source);
            AL = (byte)temp;
            AH = (byte)(sbyte)((short)AX % (sbyte)source);
        }

        private void Divide(ushort source)
        {
            uint dividend = (uint)(((DX << 16) & 0xFFFF0000) + AX);
            ushort temp;

            temp = (ushort)(dividend / source);

            AX = temp;
            DX = (ushort)(dividend % source);
        }

        private void SDivide(ushort source)
        {
            uint dividend = (uint)((DX << 16 & 0xffff0000) + AX);
            short temp;

            temp = (short)((int)dividend / (short)source);
            AX = (ushort)temp;
            DX = (ushort)((int)dividend / (short)source);
        }
        #endregion

        private byte Rotate(byte dest, byte count, RotateType type)
        {
            return (byte)DoRotate((ushort)dest, (ushort)count, type, 8);
        }

        private ushort Rotate(ushort dest, ushort count, RotateType type)
        {
            return DoRotate(dest, count, type, 16);
        }

        private ushort DoRotate(ushort dest, ushort count, RotateType type, int size)
        {
            int tempCount;
            bool tempCF;
            ushort ret = dest;

            if (type == RotateType.LeftWithCarry || type == RotateType.RightWithCarry)
            {
                if (size == 8)
                    tempCount = ((count & 0x1f) % 9);
                else
                    tempCount = ((count & 0x1f) % 17);
            }
            else
            {
                if (size == 8)
                    tempCount = count % 8;
                else
                    tempCount = count % 16;
            }
            if (type == RotateType.LeftWithCarry)
            {
                while (tempCount != 0)
                {
                    if (size == 8)
                        tempCF = ((ret & 0x80) == 0x80);
                    else
                        tempCF = ((ret & 0x8000) == 0x8000);

                    ret = (ushort)((ret * 2) + (CF ? 1 : 0));
                    
                    CF = tempCF;
                    tempCount--;
                }

                if (count == 1)
                {
                    if (size == 8)
                        OF = ((ret & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((ret & 0x8000) ^ (CF ? 1 : 0)) != 0;
                }
            }
            else if (type == RotateType.RightWithCarry)
            {
                if (count == 1)
                {
                    if (size == 8)
                        OF = ((dest & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((dest & 0x8000) ^ (CF ? 1 : 0)) != 0;
                }

                while (tempCount != 0)
                {
                    tempCF = ((dest & 0x01) == 0x01);
                    ret /= 2;

                    if (size == 8)
                        ret += (ushort)(CF ? 256 : 0);
                    else
                        ret += (ushort)(CF ? 65536 : 0);

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
                    else
                        tempCF = ((ret & 0x8000) == 0x8000);

                    ret = (ushort)((ret * 2) + (CF ? 1 : 0));
                    tempCount--;
                }
                CF = ((ret & 0x01) == 0x01);
                if (count == 1)
                {
                    if (size == 8)
                        OF = ((dest & 0x80) ^ (CF ? 1 : 0)) != 0;
                    else
                        OF = ((dest & 0x8000) ^ (CF ? 1 : 0)) != 0;
                }
            }
            else
            {
                while (tempCount != 0)
                {
                    tempCF = ((dest & 0x01) == 0x01);
                    ret /= 2;
                    if (size == 8)
                        ret += (ushort)(CF ? 256 : 0);
                    else
                        ret += (ushort)(CF ? 65536 : 0);

                    tempCount--;
                }
                if (size == 8)
                    CF = ((ret & 0x80) == 0x80);
                else
                    CF = ((ret & 0x8000) == 0x8000);

                if (count == 1)
                {
                    if (size == 8)
                        OF = ((((ret & 0x80) == 0x80) ^ ((ret & 0x40) == 0x40)));
                    else
                        OF = ((((ret & 0x8000) == 0x8000) ^ ((ret & 0x4000) == 0x4000)));
                }
            }

            return ret;
        }

        private byte Shift(byte source, byte count, ShiftType type)
        {
            byte tempCount, tempDest, dest;

            dest = source;

            tempCount = (byte)(count & 0x1f);
            tempDest = dest;

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
                        dest = (byte)((sbyte)dest / 2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    byte tmp = (byte)((dest & 0x80) ^ (CF ? 1 : 0));
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
            ushort tempCount, tempDest, dest;

            dest = source;

            tempCount = (ushort)(count & 0x1f);
            tempDest = dest;

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
                        dest = (ushort)((short)dest / 2);
                    else
                        dest /= 2;
                }
                tempCount--;
            }

            if (count == 1)
            {
                if (type == ShiftType.ArithmaticLeft || type == ShiftType.Left)
                {
                    ushort tmp = (ushort)((dest & 0x8000) ^ (CF ? 1 : 0));
                    OF = (tmp != 0);
                }
                else if (type == ShiftType.ArithmaticRight)
                    OF = false;
                else
                    OF = ((tempDest & 0x8000) == 0x8000);
            }

            return dest;
        }
    }
}
