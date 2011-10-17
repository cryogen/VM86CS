using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0x88, Count = 5)]
        [CPUFunction(OpCode = 0x8e)]
        [CPUFunction(OpCode = 0xa0, Count = 4)]
        [CPUFunction(OpCode = 0xb0, Count = 16)]
        [CPUFunction(OpCode = 0x00c6)]
        [CPUFunction(OpCode = 0x00c7)]
        [CPUFunction(OpCode = 0x0f20)]
        [CPUFunction(OpCode = 0x0f22)]
        public void Move(Operand dest, Operand source)
        {
            dest.Value = source.Value;
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fb6)]
        [CPUFunction(OpCode = 0x0fb7)]
        public void MoveZeroExtend(Operand dest, Operand source)
        {
            dest.Value = source.Value;
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x86)]
        [CPUFunction(OpCode = 0x87)]
        [CPUFunction(OpCode = 0x91, Count = 7)]
        public void Exchange(Operand dest, Operand source)
        {
            Operand temp;

            temp = dest;
            dest.Value = source.Value;
            source.Value = temp.Value;

            WriteOperand(source);
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x06)]
        [CPUFunction(OpCode = 0x0e)]
        [CPUFunction(OpCode = 0x16)]
        [CPUFunction(OpCode = 0x1e)]
        [CPUFunction(OpCode = 0x68)]
        [CPUFunction(OpCode = 0x6a)]
        [CPUFunction(OpCode = 0x50, Count=8)]
        [CPUFunction(OpCode = 0xff06)]
        public void Push(Operand dest)
        {
            StackPush(dest.Value);
        }

        [CPUFunction(OpCode = 0x60)]
        public void PushAll()
        {
            uint oldESP = ESP;

            if (opSize == 32)
            {
                StackPush(AX);
                StackPush(CX);
                StackPush(DX);
                StackPush(BX);
                StackPush((ushort)oldESP);
                StackPush(BP);
                StackPush(SI);
                StackPush(DI);
            }
            else
            {
                StackPush(EAX);
                StackPush(ECX);
                StackPush(EDX);
                StackPush(EBX);
                StackPush(oldESP);
                StackPush(EBP);
                StackPush(ESI);
                StackPush(EDI);
            }
        }

        [CPUFunction(OpCode = 0x61)]
        public void PopAll()
        {
            if (opSize == 32)
            {
                EDI = StackPop();
                ESI = StackPop();
                EBP = StackPop();
                ESP += 4;
                EBX = StackPop();
                EDX = StackPop();
                ECX = StackPop();
                EAX = StackPop();
            }
            else
            {
                DI = (ushort)StackPop();
                SI = (ushort)StackPop();
                BP = (ushort)StackPop();
                SP += 2;
                BX = (ushort)StackPop();
                DX = (ushort)StackPop();
                CX = (ushort)StackPop();
                AX = (ushort)StackPop();
            }
        }

        [CPUFunction(OpCode = 0x07)]
        [CPUFunction(OpCode = 0x0f)]
        [CPUFunction(OpCode = 0x17)]
        [CPUFunction(OpCode = 0x1f)]
        [CPUFunction(OpCode = 0x58, Count = 8)]
        [CPUFunction(OpCode = 0x8f)]
        public void Pop(Operand dest)
        {
            dest.Value = StackPop();
            WriteOperand(dest);
        }
    }
}
