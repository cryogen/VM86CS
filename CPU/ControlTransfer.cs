using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0xea)]
        public void FarJump(Operand dest)
        {
            uint segment, offset;

            if (PMode == false && ((CR0 & 0x1) == 0x1))
                PMode = true;
            else if (PMode && ((CR0 & 0x1) == 0))
                PMode = false;

            segment = dest.Value;
            offset = (uint)dest.Address;

            CS = segment;
            if (opSize == 32)
                EIP = offset;
            else
                EIP = (ushort)offset;
        }

        [CPUFunction(OpCode = 0xff05)]
        public void FarJumpMemory(Operand dest)
        {
            dest.Value = SegRead(dest.Memory.Segment, dest.Memory.Address+2, 16);
            dest.Address = SegRead(dest.Memory.Segment, dest.Memory.Address, (int)dest.Size);

            FarJump(dest);
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

        [CPUFunction(OpCode = 0x70)]
        [CPUFunction(OpCode = 0x0f80)]
        public void JumpIfOverflow(Operand dest)
        {
            if (OF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x71)]
        [CPUFunction(OpCode = 0x0f81)]
        public void JumpIfNotOverflow(Operand dest)
        {
            if (!OF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x72)]
        [CPUFunction(OpCode = 0x0f82)]
        public void JumpIfBelow(Operand dest)
        {
            if (CF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x73)]
        [CPUFunction(OpCode = 0x0f83)]
        public void JumpIfNotBelow(Operand dest)
        {
            if (!CF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x0f88)]
        public void JumpIfSigned(Operand dest)
        {
            if (SF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x0f89)]
        public void JumpIfNotSigned(Operand dest)
        {
            if (!SF)
                Jump(dest);
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

        [CPUFunction(OpCode = 0x76)]
        [CPUFunction(OpCode = 0x0f86)]
        public void JumpIfBelowOrEqual(Operand dest)
        {
            if (CF || ZF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x77)]
        [CPUFunction(OpCode = 0x0f87)]
        public void JumpIfNotBelowOrEqual(Operand dest)
        {
            if (!CF && !ZF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x7c)]
        [CPUFunction(OpCode = 0x0f8c)]
        public void JumpIfLess(Operand dest)
        {
            if (SF != OF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x7d)]
        [CPUFunction(OpCode = 0x0f8d)]
        public void JumpIfNotLess(Operand dest)
        {
            if (SF == OF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x7e)]
        [CPUFunction(OpCode = 0x0f8e)]
        public void JumpIfLessOrEqual(Operand dest)
        {
            if (ZF || (SF != OF))
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x7f)]
        [CPUFunction(OpCode = 0x0f8f)]
        public void JumpIfNotLessOrEqual(Operand dest)
        {
            if (!ZF && SF == OF)
                Jump(dest);
        }

        [CPUFunction(OpCode=0xe3)]
        public void JumpIfCXIsZero(Operand dest)
        {
            if (opSize == 16 && CX == 0)
                Jump(dest);
            else if (opSize == 32 && ECX == 0)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0xe8)]
        public void Call(Operand dest)
        {
            if (opSize == 32)
            {
                StackPush(EIP);
                EIP = (uint)(EIP + dest.SignedValue);
            }
            else
            {
                StackPush((ushort)EIP);
                EIP = (ushort)(EIP + dest.SignedValue);
            }
        }

        [CPUFunction(OpCode = 0xff02)]
        public void CallAbsolute(Operand dest)
        {
            if (opSize == 16)
            {
                StackPush(IP);
                EIP = (ushort)dest.Value;
            }
            else
            {
                StackPush(EIP);
                EIP = dest.Value;
            }
        }

        [CPUFunction(OpCode = 0x009a)]
        public void CallFar(Operand dest)
        {
            uint segment, offset;

            segment = dest.Value;
            offset = (uint)dest.Address;

            if (opSize == 16)
            {
                StackPush(CS);
                StackPush(IP);
                CS = segment;
                EIP = (ushort)offset;
            }
            else
            {
                StackPush(CS);
                StackPush(EIP);
                CS = segment;
                EIP = offset;
            }
        }

        [CPUFunction(OpCode = 0xff03)]
        public void CallFarMemory(Operand dest)
        {
            dest.Value = SegRead(dest.Memory.Segment, dest.Memory.Address + 2, 16);
            dest.Address = SegRead(dest.Memory.Segment, dest.Memory.Address, (int)dest.Size);

            CallFar(dest);
        }

        [CPUFunction(OpCode = 0xc3)]
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

        [CPUFunction(OpCode = 0xc2)]
        public void Return(Operand dest)
        {
            Return();
            if (opSize == 32)
                ESP = (uint)(ESP + dest.SignedValue);
            else
                SP = (ushort)(SP + dest.SignedValue);
        }

        [CPUFunction(OpCode = 0xcb)]
        public void FarReturn()
        {
            if (opSize == 16)
            {
                EIP = (ushort)StackPop();
                CS = StackPop();
            }
            else
            {
                EIP = StackPop();
                CS = StackPop();
            }
        }

        [CPUFunction(OpCode = 0xca)]
        public void FarReturn(Operand dest)
        {
            FarReturn();
            if (opSize == 32)
                ESP = (uint)(ESP + dest.SignedValue);
            else
                SP = (ushort)(SP + dest.SignedValue);
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

            CS = Memory.Read((uint)(dest.Value * 4) + 2, 16);
            EIP = Memory.Read((uint)(dest.Value * 4), 16);
        }

        [CPUFunction(OpCode = 0xcf)]
        public void InterruptReturn()
        {
            EIP = (ushort)StackPop();
            CS = StackPop();
            eFlags = (CPUFlags)StackPop();
            DumpRegisters();
        }

        [CPUFunction(OpCode = 0xe0)]
        public void LoopNotZero(Operand dest)
        {
            uint count;

            if (addressSize == 32)
                count = ECX;
            else
                count = CX;

            count--;

            if (count != 0 && !ZF)
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
