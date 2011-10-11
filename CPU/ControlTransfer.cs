namespace x86CS.CPU
{
    public partial class CPU
    {/*
        private void DoFarJump()
        {
            uint segment, offset;

            if (PMode == false && ((CR0 & 0x1) == 0x1))
                PMode = true;
            else if (PMode && ((CR0 & 0x1) == 0))
                PMode = false;

            segment = (uint)((currentInstruction.Instruction.AddrValue & 0xffff0000) >> 4);
            offset = (uint)currentInstruction.Instruction.AddrValue & 0x0000ffff;

            CS = segment;
            if (opSize == 32)
                EIP = offset;
            else
                EIP = (ushort)offset;
        }

        private void DoJump()
        {
            if(opSize == 32)
                EIP = (uint)currentInstruction.Instruction.AddrValue;
            else
                EIP = (ushort)currentInstruction.Instruction.AddrValue;
        }

        private void ProcessJump()
        {
            switch (currentInstruction.Instruction.Opcode)
            {
                case 0xe9:
                case 0xeb:
                    DoJump();
                    break;
                case 0xea:
                    DoFarJump();
                    break;
                case 0xff:
                    break;
                default:
                    break;
            }
        }

        private void DoCall()
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
