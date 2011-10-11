namespace x86CS.CPU
{
    public partial class CPU
    {/*
        private void ProcessInputOutput(Operand[] operands)
        {
            switch (currentInstruction.Instruction.Opcode)
            {
                case 0xe4:
                case 0xe5:
                case 0xec:
                case 0xed:
                    SetOperandValue(operands[0], DoIORead((ushort)GetOperandValue(operands[1]), (int)operands[0].OperandSize));
                    break;
                case 0xe6:
                case 0xe7:
                    DoIOWrite((ushort)GetOperandValue(operands[0]), GetOperandValue(operands[1]), (int)operands[1].OperandSize);
                    break;
                case 0xee:
                    DoIOWrite(DX, AL, 8);
                    break;
                case 0xef:
                    if (currentInstruction.Argument2.ArgSize == 16)
                        DoIOWrite(DX, AX, 16);
                    else
                        DoIOWrite(DX, EAX, 32);
                    break;
                default:
                    break;
            }
        }

        private uint DoIORead(ushort addr, int size)
        {
            ReadCallback ioRead = IORead;

            if (ioRead != null)
                return ioRead(addr, size);

            return 0xffff;
        }

        private void DoIOWrite(ushort addr, uint value, int size)
        {
            WriteCallback ioWrite = IOWrite;

            if (ioWrite != null)
                ioWrite(addr, value, size);
        }*/
    }
}
