using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0x84, Count = 2)]
        [CPUFunction(OpCode = 0xa8, Count = 2)]
        [CPUFunction(OpCode = 0xf600)]
        [CPUFunction(OpCode = 0xf700)]
        public void Test(Operand dest, Operand source)
        {
            dest.Value = dest.Value & source.Value;
            SetCPUFlags(dest);
            CF = false;
            OF = false;
        }

        [CPUFunction(OpCode = 0x0fbd)]
        public void BitScanReverse(Operand dest, Operand source)
        {
            uint temp;

            if (source.Value == 0)
            {
                ZF = true;
            }
            else
            {
                ZF = false;
                temp = dest.Size - 1;

                while ((source.Value & ((1 << (int)temp))) == 0)
                {
                    temp--;
                }
                dest.Value = temp;
            }
            WriteOperand(dest);
        }
    }
}
