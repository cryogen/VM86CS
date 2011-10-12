using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode=0xf4)]
        public void Halt()
        {
            Halted = true;
        }
    }
}
