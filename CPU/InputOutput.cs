using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        [CPUFunction(OpCode = 0xe4)]
        [CPUFunction(OpCode = 0xe5)]
        [CPUFunction(OpCode = 0xec)]
        [CPUFunction(OpCode = 0xed)]
        public void DoIORead(Operand dest, Operand source)
        {
            ReadCallback ioRead = IORead;

            if (ioRead != null)
                dest.Value = ioRead((ushort)source.Value, (int)dest.Size);

            dest.Value = 0;
        }

        [CPUFunction(OpCode = 0xe6)]
        [CPUFunction(OpCode = 0xe7)]
        [CPUFunction(OpCode = 0xee)]
        [CPUFunction(OpCode = 0xef)]
        public void DoIOWrite(Operand dest, Operand source)
        {
            WriteCallback ioWrite = IOWrite;

            if (ioWrite != null)
                ioWrite((ushort)dest.Value, source.Value, (int)source.Size);
        }
    }
}
