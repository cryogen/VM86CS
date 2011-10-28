using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        private void GetSegmentAndOffset(Operand address, out ushort segment, out uint offset)
        {
            offset = SegRead(address.Memory.Segment, address.Memory.Address, (int)address.Size);
            segment = (ushort)SegRead(address.Memory.Segment, address.Memory.Address + (address.Size / 8), 16);
        }

        [CPUFunction(OpCode = 0xc4)]
        public void SegmentLoadES(Operand dest, Operand source)
        {
            ushort segment;
            uint offset;

            GetSegmentAndOffset(source, out segment, out offset);
            ES = segment;
            dest.Value = offset;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xc5)]
        public void SegmentLoadDS(Operand dest, Operand source)
        {
            ushort segment;
            uint offset;

            GetSegmentAndOffset(source, out segment, out offset);
            DS = segment;
            dest.Value = offset;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fb4)]
        public void SegmentLoadFS(Operand dest, Operand source)
        {
            ushort segment;
            uint offset;

            GetSegmentAndOffset(source, out segment, out offset);
            FS = segment;
            dest.Value = offset;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fb5)]
        public void SegmentLoadGS(Operand dest, Operand source)
        {
            ushort segment;
            uint offset;

            GetSegmentAndOffset(source, out segment, out offset);
            GS = segment;
            dest.Value = offset;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0x0fb2)]
        public void SegmentLoadSS(Operand dest, Operand source)
        {
            ushort segment;
            uint offset;

            GetSegmentAndOffset(source, out segment, out offset);
            SS = segment;
            dest.Value = offset;

            WriteOperand(dest);
        }
    }
}
