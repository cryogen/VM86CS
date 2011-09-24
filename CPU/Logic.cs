using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace x86CS
{
    public partial class CPU
    {
        private byte And(byte source)
        {
            return DoAnd(AL, source);
        }

        private ushort And(ushort source)
        {
            return DoAnd(AX, source);
        }

        private uint And(uint source)
        {
            return DoAnd(EAX, source);
        }

        private byte And(byte dest, byte source)
        {
            return DoAnd(dest, source);
        }

        private ushort And(ushort dest, byte source)
        {
            return DoAnd(dest, (ushort)(short)(sbyte)source);
        }

        private uint And(uint dest, byte source)
        {
            return DoAnd(dest, (uint)(int)(sbyte)source);
        }

        private ushort And(ushort dest, ushort source)
        {
            return DoAnd(dest, source);
        }

        private uint And(uint dest, uint source)
        {
            return DoAnd(dest, source);
        }

        private byte DoAnd(byte dest, byte source)
        {
            byte temp = (byte)(source & dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private ushort DoAnd(ushort dest, ushort source)
        {
            ushort temp = (ushort)(source & dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private uint DoAnd(uint dest, uint source)
        {
            uint temp = (uint)(source & dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private void Or(byte source)
        {
            AL = DoOr(AL, source);
        }

        private void Or(ushort source)
        {
            AX = DoOr(AX, source);
        }

        private void Or(uint source)
        {
            EAX = DoOr(EAX, source);
        }

        private byte Or(byte dest, byte source)
        {
            return DoOr(dest, source);
        }

        private ushort Or(ushort dest, byte source)
        {
            return DoOr(dest, (ushort)(short)(sbyte)source);
        }

        private uint Or(uint dest, byte source)
        {
            return DoOr(dest, (uint)(int)(sbyte)source);
        }

        private ushort Or(ushort dest, ushort source)
        {
            return DoAnd(dest, source);
        }

        private uint Or(uint dest, uint source)
        {
            return DoAnd(dest, source);
        }

        private byte DoOr(byte dest, byte source)
        {
            byte temp = (byte)(source | dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private ushort DoOr(ushort dest, ushort source)
        {
            ushort temp = (ushort)(source | dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private uint DoOr(uint dest, uint source)
        {
            uint temp = (uint)(source | dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private void Xor(byte source)
        {
            AL = DoXor(AL, source);
        }

        private void Xor(ushort source)
        {
            AX = DoXor(AX, source);
        }

        private void Xor(uint source)
        {
            EAX = DoXor(EAX, source);
        }

        private byte Xor(byte dest, byte source)
        {
            return DoXor(dest, source);
        }

        private ushort Xor(ushort dest, byte source)
        {
            return DoXor(dest, (ushort)(short)(sbyte)source);
        }

        private uint Xor(uint dest, byte source)
        {
            return DoXor(dest, (uint)(int)(sbyte)source);
        }

        private ushort Xor(ushort dest, ushort source)
        {
            return DoXor(dest, source);
        }

        private uint Xor(uint dest, uint source)
        {
            return DoXor(dest, source);
        }

        private byte DoXor(byte dest, byte source)
        {
            byte temp = (byte)(source ^ dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private ushort DoXor(ushort dest, ushort source)
        {
            ushort temp = (ushort)(source ^ dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private uint DoXor(uint dest, uint source)
        {
            uint temp = (uint)(source ^ dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
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

        private void SetCPUFlags(uint operand)
        {
            int signed = (int)operand;

            if (operand == 0)
                ZF = true;
            else
                ZF = false;

            if (signed < 0)
                SF = true;
            else
                SF = false;

            SetParity((int)operand);
        }

        private void SetParity(int value)
        {
            BitArray bitCount = new BitArray(new int[] { value });

            if (bitCount.CountSet() % 2 == 0)
                PF = true;
            else
                PF = false;
        }
   }
}