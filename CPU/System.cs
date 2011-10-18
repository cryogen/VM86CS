using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode=0xf4)]
        public void Halt()
        {
            Halted = true;
        }

        [CPUFunction(OpCode = 0x90)]
        public void Nop()
        {
        }

        [CPUFunction(OpCode = 0x0f0103)]
        public void LoadIDT(Operand dest)
        {
            if (opSize == 16)
            {
                idtRegister.Limit = SegReadWord(dest.Memory.Segment, dest.Memory.Address);
                idtRegister.Base = SegReadDWord(dest.Memory.Segment, dest.Memory.Address + 2) & 0x00ffffff;
            }
            else
            {
                idtRegister.Limit = SegReadWord(dest.Memory.Segment, dest.Memory.Address);
                idtRegister.Base = SegReadDWord(dest.Memory.Segment, dest.Memory.Address + 2);
            }
        }

        [CPUFunction(OpCode = 0x0f0102)]
        public void LoadGDT(Operand dest)
        {
            if (opSize == 16)
            {
                gdtRegister.Limit = SegReadWord(dest.Memory.Segment, dest.Memory.Address);
                gdtRegister.Base = SegReadDWord(dest.Memory.Segment, dest.Memory.Address + 2) & 0x00ffffff;
            }
            else
            {
                gdtRegister.Limit = SegReadWord(dest.Memory.Segment, dest.Memory.Address);
                gdtRegister.Base = SegReadDWord(dest.Memory.Segment, dest.Memory.Address + 2);
            }
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
    }
}
