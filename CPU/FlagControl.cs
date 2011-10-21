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

        [CPUFunction(OpCode = 0xfb)]
        public void SetInterruptFlag()
        {
            IF = true;
        }

        [CPUFunction(OpCode = 0xf8)]
        public void ClearCarryFlag()
        {
            CF = false;
        }

        [CPUFunction(OpCode = 0xf9)]
        public void SetCarryFlag()
        {
            CF = true;
        }

        [CPUFunction(OpCode = 0xf5)]
        public void ComplementCarryFlag()
        {
            CF = !CF;
        }

        [CPUFunction(OpCode = 0xfc)]
        public void ClearDirectionFlag()
        {
            DF = false;
        }

        [CPUFunction(OpCode = 0xfd)]
        public void SetDirectionFlag()
        {
            DF = true;
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

        [CPUFunction(OpCode = 0x9f)]
        public void LoadFlags()
        {
            AH = (byte)eFlags;
            AH |= 0x2;
            AH &= 0xd7;
        }

        [CPUFunction(OpCode = 0x9e)]
        public void StoreFlags()
        {
            eFlags = (CPUFlags)AH;
            eFlags |= CPUFlags.Spare;
            eFlags &= ~(CPUFlags.Spare2 | CPUFlags.Spare3);
        }
    }
}
