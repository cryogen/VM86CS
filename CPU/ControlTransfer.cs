using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode=0xea)]
        public void FarJump(Operand dest)
        {
            uint segment, offset;

            if (PMode == false && ((CR0 & 0x1) == 0x1))
            {
                PMode = true;
                disasm.CodeSize = 32;
            }
            else if (PMode && ((CR0 & 0x1) == 0))
            {
                PMode = false;
                disasm.CodeSize = 16;
            }

            segment = (uint)((dest.Value & 0xffff0000) >> 4);
            offset = (uint)dest.Value & 0x0000ffff;

            SetSelector(SegmentRegister.CS, segment);
            if (opSize == 32)
                EIP = offset;
            else
                EIP = (ushort)offset;
        }

        [CPUFunction(OpCode = 0xeb)]
        [CPUFunction(OpCode = 0xe9)]
        public void Jump(Operand dest)
        {
            if (opSize == 32)
                EIP = (uint)(EIP + dest.SignedValue);
            else
                EIP = (ushort)(EIP + dest.SignedValue);
        }

        [CPUFunction(OpCode = 0xff04)]
        public void JumpAbsolute(Operand dest)
        {
            if (opSize == 32)
                EIP = dest.Value;
            else
                EIP = (ushort)dest.Value;
        }

        [CPUFunction(OpCode = 0x74)]
        [CPUFunction(OpCode = 0x0f84)]
        public void JumpIfZero(Operand dest)
        {
            if (ZF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x75)]
        [CPUFunction(OpCode = 0x0f85)]
        public void JumpIfNotZero(Operand dest)
        {
            if (!ZF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x72)]
        public void JumpIfBelow(Operand dest)
        {
            if (CF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x76)]
        public void JumpIfBelowOrEqual(Operand dest)
        {
            if (CF && ZF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x73)]
        public void JumpIfNotBelow(Operand dest)
        {
            if (!CF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x77)]
        public void JumpIfNotBelowOrEqual(Operand dest)
        {
            if (!CF && !ZF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x7c)]
        public void JumpIfLess(Operand dest)
        {
            if (SF != OF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0xe8)]
        [CPUFunction(OpCode = 0xff02)]
        public void Call(Operand dest)
        {
            if (opSize == 16)
            {
                StackPush(IP);
                EIP = (uint)(ushort)(IP + (ushort)dest.Value);
            }
            else
            {
                StackPush(EIP);
                EIP = dest.Value;
            }
        }

        [CPUFunction(OpCode = 0xff03)]
        public void CallFar(Operand dest)
        {
            if (opSize == 16)
            {
                StackPush(CS);
                StackPush(IP);
                SetSelector(SegmentRegister.CS, (ushort)(dest.Value >> 16));
                EIP = (ushort)dest.Value;
            }
            else
            {
                StackPush(CS);
                StackPush(EIP);
                SetSelector(SegmentRegister.CS, (ushort)(dest.Value >> 16));
                EIP = dest.Value;
            }
        }


        [CPUFunction(OpCode = 0xc3)]
        [CPUFunction(OpCode = 0xcb)]
        public void Return()
        {
            if (opSize == 16)
            {
                EIP = (ushort)StackPop();
            }
            else
            {
                EIP = StackPop();
            }
        }

        [CPUFunction(OpCode = 0xcd)]
        public void Interrupt(Operand dest)
        {
            StackPush((ushort)Flags);
            IF = false;
            TF = false;
            AC = false;
            StackPush(CS);
            StackPush(IP);

            SetSelector(SegmentRegister.CS, Memory.Read((uint)(dest.Value * 4) + 2, 16));
            EIP = Memory.Read((uint)(dest.Value * 4), 16);
        }

        [CPUFunction(OpCode = 0xcf)]
        public void InterruptReturn()
        {
            EIP = (ushort)StackPop();
            SetSelector(SegmentRegister.CS, StackPop());
            eFlags = (CPUFlags)StackPop();
        }

        [CPUFunction(OpCode = 0xe2)]
        public void Loop(Operand dest)
        {
            uint count;

            if (addressSize == 32)
                count = ECX;
            else
                count = CX;

            count--;

            if (count != 0)
            {
                if (opSize == 32)
                    EIP = (uint)(EIP + dest.SignedValue);
                else
                    EIP = (ushort)(EIP + dest.SignedValue);
            }

            if (addressSize == 32)
                ECX = count;
            else
                CX = (ushort)count;
        }
    }
}
