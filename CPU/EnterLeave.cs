using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0xc9)]
        public void Leave()
        {
            if (addressSize == 32)
                ESP = EBP;
            else
                SP = BP;

            if (opSize == 32)
                EBP = StackPop();
            else
                BP = (ushort)StackPop();
        }
    }
}
