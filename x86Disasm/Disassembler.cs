namespace x86Disasm
{
    public partial class Disassembler
    {
        private ReadCallback readFunction;
        private OPPrefix setPrefixes;

        public Disassembler(ReadCallback readCallback)
        {
            readFunction = readCallback;
        }

        public int Disassemble(uint virtualAddr)
        {
            uint offset = 0;
            byte opCode;
            Instruction instruction;

            opCode = (byte)readFunction(offset, 8);
            instruction = instructions[opCode];

                
            return (int)offset;
        }
    }
}
