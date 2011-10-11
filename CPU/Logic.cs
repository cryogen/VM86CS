using System.Collections;
using x86Disasm;

namespace x86CS.CPU
{
    public partial class CPU
    {
        private void SetCPUFlags(Operand operand)
        {
            int signed;

            switch (operand.Size)
            {
                case 8:
                    signed = (sbyte)operand.Value;
                    break;
                case 16:
                    signed = (short)operand.Value;
                    break;
                default:
                    signed = (int)operand.Value;
                    break;
            }

            ZF = operand.Value == 0;
            SF = signed < 0;

            SetParity(operand.Value);
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
            return 0;
          /*  uint temp;

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

            return temp;*/
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

          //  SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private ushort DoOr(ushort dest, ushort source)
        {
            var temp = (ushort)(source | dest);

         //   SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        private uint DoOr(uint dest, uint source)
        {
            var temp = source | dest;

          //  SetCPUFlags(temp);

            CF = false;
            OF = false;

            return temp;
        }

        [CPUFunction(OpCode = 0x30)]
        [CPUFunction(OpCode = 0x31)]
        [CPUFunction(OpCode = 0x32)]
        [CPUFunction(OpCode = 0x33)]
        [CPUFunction(OpCode = 0x34)]
        [CPUFunction(OpCode = 0x35)]
        [CPUFunction(OpCode = 0x0680)]
        [CPUFunction(OpCode = 0x0681)]
        [CPUFunction(OpCode = 0x0683)]
        public void Xor(Operand dest, Operand source)
        {
            dest.Value = dest.Value ^ source.Value;
            SetCPUFlags(dest);

            CF = false;
            OF = false;

            WriteOperand(dest);
        }
   }
}