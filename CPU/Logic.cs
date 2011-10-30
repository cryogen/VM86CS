using System.Collections;
using x86Disasm;

namespace x86CS.CPU
{
    public partial class CPU
    {
        private void SetCPUFlags(Operand operand)
        {
            SF = operand.MSB;

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
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x08, Count = 6)]
        [CPUFunction(OpCode = 0x8001)]
        [CPUFunction(OpCode = 0x8101)]
        [CPUFunction(OpCode = 0x8301)]
        public void Or(Operand dest, Operand source)
        {
            dest.Value = dest.Value | source.Value;
            SetCPUFlags(dest);
            CF = false;
            OF = false;
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x30, Count=6)]
        [CPUFunction(OpCode = 0x8006)]
        [CPUFunction(OpCode = 0x8106)]
        [CPUFunction(OpCode = 0x8306)]
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