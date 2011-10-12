using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0xfa)]
        public void ClearInterruptFlag()
        {
            IF = false;
        }

        [CPUFunction(OpCode = 0xfc)]
        public void ClearDirectionFlag()
        {
            DF = false;
        }

    }
}
