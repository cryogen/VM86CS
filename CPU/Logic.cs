using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private byte And(byte dest, byte source)
        {
            return DoAnd(dest, source);
        }

        private ushort And(ushort dest, byte source)
        {
            return DoAnd(dest, (ushort)(short)(sbyte)source);
        }

        private ushort And(ushort dest, ushort source)
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

        private byte Or(byte source)
        {
            return DoOr(AL, source);
        }

        private ushort Or(ushort source)
        {
            return DoOr(AX, source);
        }

        private byte Or(byte dest, byte source)
        {
            return DoOr(dest, source);
        }

        private ushort Or(ushort dest, byte source)
        {
            return DoOr(dest, (ushort)(short)(sbyte)source);
        }

        private ushort Or(ushort dest, ushort source)
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

        private byte Xor(byte source)
        {
            return DoOr(AL, source);
        }

        private ushort Xor(ushort source)
        {
            return DoOr(AX, source);
        }

        private byte Xor(byte dest, byte source)
        {
            return DoOr(dest, source);
        }

        private ushort Xor(ushort dest, byte source)
        {
            return DoOr(dest, (ushort)(short)(sbyte)source);
        }

        private ushort Xor(ushort dest, ushort source)
        {
            return DoAnd(dest, source);
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
   }
}