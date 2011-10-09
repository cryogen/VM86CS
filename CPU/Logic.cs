using System.Collections;

namespace x86CS.CPU
{
    public partial class CPU
    {
        private void ProcessLogic(Operand[] operands)
        {
            uint source, dest;

            dest = GetOperandValue(operands[0]);
            source = GetOperandValue(operands[1]);

            switch (currentInstruction.Instruction.Opcode)
            {
                case 0x31:
                    dest = Xor(dest, source, operands[0].OperandSize);
                    SetOperandValue(operands[0], dest);
                    break;
                default:
                    break;
            }
        }

        private void SetCPUFlags(byte operand)
        {
            var signed = (sbyte)operand;

            ZF = operand == 0;
            SF = signed < 0;

            SetParity(operand);
        }

        private void SetCPUFlags(ushort operand)
        {
            var signed = (short)operand;

            ZF = operand == 0;

            SF = signed < 0;

            SetParity(operand);
        }

        private void SetCPUFlags(uint operand)
        {
            var signed = (int)operand;

            ZF = operand == 0;

            SF = signed < 0;

            SetParity(operand);
        }

        private void SetParity(uint value)
        {
            value ^= value >> 1;
            value ^= value >> 2;
            value ^= value >> 4;
            value ^= value >> 8;
            value ^= value >> 16;
            PF = ((value & 1) == 1);
        }

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
            return DoAnd(dest, (ushort)(sbyte)source);
        }

        private uint And(uint dest, byte source)
        {
            return DoAnd(dest, (uint)(sbyte)source);
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
            var temp = (byte)(source & dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private ushort DoAnd(ushort dest, ushort source)
        {
            var temp = (ushort)(source & dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private uint DoAnd(uint dest, uint source)
        {
            var temp = source & dest;

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
            return DoOr(dest, (ushort)(sbyte)source);
        }

        private uint Or(uint dest, byte source)
        {
            return DoOr(dest, (uint)(sbyte)source);
        }

        private ushort Or(ushort dest, ushort source)
        {
            return DoOr(dest, source);
        }

        private uint Or(uint dest, uint source)
        {
            return DoOr(dest, source);
        }

        private byte DoOr(byte dest, byte source)
        {
            var temp = (byte)(source | dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private ushort DoOr(ushort dest, ushort source)
        {
            var temp = (ushort)(source | dest);

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private uint DoOr(uint dest, uint source)
        {
            var temp = source | dest;

            SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private uint Xor(uint dest, uint source, uint size)
        {
            uint ret;

            switch (size)
            {
                case 8:
                    ret = (byte)((byte)source ^ (byte)dest);
                    SetCPUFlags((byte)ret);
                    break;
                case 16:
                    ret = (ushort)((ushort)source ^ (ushort)(short)dest);
                    SetCPUFlags((ushort)ret);
                    break;
                default:
                    ret = source ^ dest;
                    SetCPUFlags(ret);
                    break;
            }

            CF = false;
            OF = false;
            return ret;
        }
   }
}