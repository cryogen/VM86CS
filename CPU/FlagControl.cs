using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS.CPU
{
    public partial class CPU
    {
        private void ProcessFlagControl(Operand[] operands)
        {
            switch (currentInstruction.Instruction.Opcode)
            {
                case 0xfa:
                    IF = false;
                    break;
                case 0xfc:
                    DF = false;
                    break;
                default:
                    break;
            }
        }
    }
}
