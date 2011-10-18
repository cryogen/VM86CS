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
    }
}
