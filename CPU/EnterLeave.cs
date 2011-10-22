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

        [CPUFunction(OpCode = 0xc8)]
        public void Enter(Operand size, Operand nesting)
        {
            byte nestingLevel = (byte)(nesting.Value % 32);
            uint frameTemp;

            if (opSize == 32)
            {
                StackPush(EBP);
                frameTemp = ESP;
            }
            else
            {
                StackPush(BP);
                frameTemp = SP;
            }

            if (nestingLevel > 0)
            {
                for (int i = 1; i < nestingLevel - 1; i++)
                {
                    if (opSize == 32)
                    {
                        EBP -= 4;
                        StackPush(EBP);
                    }
                    else
                    {
                        BP -= 2;
                        StackPush(BP);
                    }
                }
                StackPush(frameTemp);
            }

            if (opSize == 32)
            {
                EBP = frameTemp;
                ESP = EBP - size.Value;   
            }
            else
            {
                BP = (ushort)frameTemp;
                SP = (ushort)(BP - size.Value);
            }
        }
    }
}
