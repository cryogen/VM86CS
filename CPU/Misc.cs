using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0x8d)]
        public void LoadEffectiveAddress(Operand dest, Operand source)
        {
            dest.Value = source.Memory.Address;
            WriteOperand(dest);
        }
    }
}
