using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0x88, Count = 8)]
        [CPUFunction(OpCode = 0xa0, Count = 4)]
        [CPUFunction(OpCode = 0xb0, Count = 16)]
        [CPUFunction(OpCode = 0x00c6)]
        [CPUFunction(OpCode = 0x00c7)]
        public void Move(Operand dest, Operand source)
        {
            dest.Value = source.Value;
            WriteOperand(dest);
        }
    }
}
