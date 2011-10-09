namespace x86CS.CPU
{
    public partial class CPU
    {
        private void ProcessInputOutput(Operand[] operands)
        {
            switch (currentInstruction.Instruction.Opcode)
            {
                case 0xe4:
                    SetOperandValue(operands[0], DoIORead((ushort)GetOperandValue(operands[1]), 8));
                    break;
                case 0xe6:
                    DoIOWrite((ushort)GetOperandValue(operands[0]), GetOperandValue(operands[1]), 8);
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
        }
    }
}
