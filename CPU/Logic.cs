using System.Collections;

namespace x86CS.CPU
{
    public partial class CPU
    {
     /*   private void ProcessLogic(Operand[] operands)
        {
            uint source, dest;
            int opSize = (int)operands[0].OperandSize;

            dest = GetOperandValue(operands[0]);
            source = GetOperandValue(operands[1]);

            switch (currentInstruction.Instruction.Opcode)
            {
                case 0x30:
                case 0x31:
                case 0x32:
                case 0x33:
                case 0x34:
                case 0x35:
                    dest = Xor(dest, source, opSize);
                    SetOperandValue(operands[0], dest);
                    break;
                case 0x24:
                case 0x25:
                case 0x20:
                case 0x21:
                case 0x22:
                case 0x23:
                    dest = And(dest, source, opSize);
                    SetOperandValue(operands[0], dest);
                    break;
                case 0x84:
                case 0x85:
                case 0xa8:
                case 0xa9:
                case 0xf6:
                case 0xf7:
                    dest = And(dest, source, opSize);
                    break;
                case 0x80:
                case 0x81:
                case 0x83:
                    switch (currentInstruction.Instruction.Mnemonic)
                    {
                        case "xor ":
                            dest = Xor(dest, source, opSize);
                            SetOperandValue(operands[0], dest);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }*/

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

        private uint And(uint dest, uint source, int size)
        {
            uint temp;

            switch (size)
            {
                case 8:
                    temp = (byte)((byte)dest & (byte)source);
                    SetCPUFlags((byte)temp);
                    break;
                case 16:
                    temp = (ushort)((ushort)dest & (ushort)source);
                    SetCPUFlags((ushort)temp);
                    break;
                default:
                    temp = dest & source;
                    SetCPUFlags(temp);
                    break;
            }

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

        private uint Xor(uint dest, uint source, int size)
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