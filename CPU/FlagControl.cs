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

        [CPUFunction(OpCode = 0x9c)]
        public void PushFlags()
        {
            if (opSize == 32)
            {
                StackPush((uint)eFlags);
            }
            else
            {
                StackPush((ushort)eFlags);
            }
        }

        [CPUFunction(OpCode = 0x9d)]
        public void PopFlags()
        {
            eFlags = (CPUFlags)StackPop();
        }
    }
}
