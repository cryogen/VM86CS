using System.Collections;
using x86Disasm;

namespace x86CS.CPU
{
    public partial class CPU
    {
        private void SetCPUFlags(Operand operand)
        {
            switch (operand.Size)
            {
                case 8:
                    SF = ((operand.Value & 0x80) == 0x80);
                    break;
                case 16:
                    SF = ((operand.Value & 0x8000) == 0x8000);
                    break;
                default:
                    SF = ((operand.Value & 0x80000000) == 0x80000000);
                    break;
            }

            ZF = operand.Value == 0;
            PF = (((((byte)operand.Value * 0x0101010101010101UL) & 0x8040201008040201UL) % 0x1FF) & 1) == 0;
        }

        [CPUFunction(OpCode = 0x20, Count = 6)]
        [CPUFunction(OpCode = 0x8004)]
        [CPUFunction(OpCode = 0x8104)]
        [CPUFunction(OpCode = 0x8304)]
        public void And(Operand dest, Operand source)
        {
            dest.Value = dest.Value & source.Value;
            SetCPUFlags(dest);
            CF = false;
            OF = false;
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

        [CPUFunction(OpCode = 0x30, Count=6)]
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