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
    }
}
