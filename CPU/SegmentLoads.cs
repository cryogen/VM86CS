using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0xc4)]
        public void SegmentLoadES(Operand dest, Operand source)
        {
            ES = (ushort)(source.Value >> 16);
            dest.Value = source.Value & 0xffff;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc5)]
        public void SegmentLoadDS(Operand dest, Operand source)
        {
            DS = (ushort)(source.Value >> 16);
            dest.Value = source.Value & 0xffff;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fb4)]
        public void SegmentLoadFS(Operand dest, Operand source)
        {
            FS = (ushort)(source.Value >> 16);
            dest.Value = source.Value & 0xffff;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fb5)]
        public void SegmentLoadGS(Operand dest, Operand source)
        {
            GS = (ushort)(source.Value >> 16);
            dest.Value = source.Value & 0xffff;

            WriteOperand(dest);
        }
    }
}
