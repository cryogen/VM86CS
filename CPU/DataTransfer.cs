namespace x86CS.CPU
{
    public partial class CPU
    {
        private void ProcessDataTransfer(Operand[] operands)
        {
            switch (currentInstruction.Instruction.Opcode)
            {
                case 0x88:
                case 0x89:
                case 0x8e:
                case 0xb0:
                case 0xb1:
                case 0xb2:
                case 0xb3:
                case 0xb4:
                case 0xb5:
                case 0xb6:
                case 0xb7:
                case 0xb8:
                case 0xb9:
                case 0xba:
                case 0xbb:
                case 0xbc:
                case 0xbd:
                case 0xbe:
                case 0xbf:
                    SetOperandValue(operands[0], GetOperandValue(operands[1]));
                    break;
                case 0x50:
                case 0x51:
                case 0x52:
                case 0x53:
                case 0x54:
                case 0x55:
                case 0x56:
                case 0x57:
                case 0x68:
                case 0xff:
                    StackPush(GetOperandValue(operands[1]));
                    break;
                default:
                    break;
            }
        }
    }
}
