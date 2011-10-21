using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0x8d)]
        public void LoadEffectiveAddress(Operand dest, Operand source)
        {
            dest.Value = source.Memory.Address;
            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fa2)]
        public void CpuID(Operand source)
        {
            switch (source.Value)
            {
                case 0:
                    EAX = 2;
                    EBX = 0x756e6547; /* "Genu" */
                    EDX = 0x49656e69; /* "ineI" */
                    ECX = 0x6c65746e; /* "ntel" */
                    break;
                case 1:
                    EAX = 0x0610;     /* Pentium Pro (ish) */
                    EDX = 0x3ce4787; /* support some stuff, other stuff not so much (i will comment or move this to enums at some point)*/
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }

        [CPUFunction(OpCode = 0xd7)]
        public void XLate()
        {
            if (addressSize == 16)
            {
                AL = SegReadByte(disasm.OverrideSegment == SegmentRegister.Default ? SegmentRegister.DS : disasm.OverrideSegment, (uint)(BX + AL));
            }
            else
            {
                AL = SegReadByte(disasm.OverrideSegment == SegmentRegister.Default ? SegmentRegister.DS : disasm.OverrideSegment, (uint)(EBX + AL));
            }
        }
    }
}
