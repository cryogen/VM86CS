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
                PMode = true;
            else if (PMode && ((CR0 & 0x1) == 0))
                PMode = false;

            segment = (uint)((dest.Value & 0xffff0000) >> 4);
            offset = (uint)dest.Value & 0x0000ffff;

            CS = segment;
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
                EIP = (uint)(EIP + (int)dest.Value);
            else
                EIP = (ushort)(EIP + (int)dest.Value);
        }

        [CPUFunction(OpCode = 0xe8)]
        [CPUFunction(OpCode = 0xff03)]
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

        [CPUFunction(OpCode = 0x74)]
        public void JumpIfZero(Operand dest)
        {
            if (ZF)
                Jump(dest);
        }

        [CPUFunction(OpCode = 0x75)]
        public void JumpIfNotZero(Operand dest)
        {
            if (!ZF)
                Jump(dest);
        }

/*        private void DoCall()
        {
            if (opSize == 16)
                StackPush(IP);
            else
                StackPush(EIP);

            EIP = (ushort)currentInstruction.Instruction.AddrValue;
        }

        private void DoFarCall()
        {
            StackPush(CS);

            if (opSize == 16)
                StackPush(IP);
            else
                StackPush(EIP);
        }

        private void ProcessCall()
        {
            switch (currentInstruction.Instruction.Opcode)
            {
                case 0xe8:
                    DoCall();
                    break;
                case 0x9a:
                    DoFarCall();
                    break;
                default:
                    break;
            }
        }

        private void ProcessRet()
        {
            switch (currentInstruction.Instruction.Opcode)
            {
                case 0xc3:
                    EIP = (ushort)StackPop();
                    break;
                default:
                    break;
            }
        }

        private void ProcessControlTransfer(Operand[] operands)
        {
            switch ((BeaConstants.BranchType)currentInstruction.Instruction.BranchType)
            {
                case BeaConstants.BranchType.JmpType:
                    ProcessJump();
                    break;
                case BeaConstants.BranchType.CallType:
                    ProcessCall();
                    break;
                case BeaConstants.BranchType.RetType:
                    ProcessRet();
                    break;
                case BeaConstants.BranchType.JE:
                    if (ZF)
                        DoJump();
                    break;
                case BeaConstants.BranchType.JNE:
                    if(!ZF)
                        DoJump();
                    break;
                case BeaConstants.BranchType.JC:
                    if (CF)
                        DoJump();
                    break;
                case BeaConstants.BranchType.JA:
                    if (!CF && !ZF)
                        DoJump();
                    break;
                default:
                    break;
            }
        }*/
    }
}
