using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0xe4, Count=2)]
        [CPUFunction(OpCode = 0xec, Count=2)]
        public void DoIORead(Operand dest, Operand source)
        {
            ReadCallback ioRead = IORead;

            if (ioRead != null)
                dest.Value = ioRead((ushort)source.Value, (int)dest.Size);
            else
                dest.Value = 0;

            WriteOperand(dest);
        }

        [CPUFunction(OpCode = 0xe6, Count=2)]
        [CPUFunction(OpCode = 0xee, Count=2)]
        public void DoIOWrite(Operand dest, Operand source)
        {
            WriteCallback ioWrite = IOWrite;

            if (ioWrite != null)
                ioWrite((ushort)dest.Value, source.Value, (int)source.Size);
        }
    }
}
