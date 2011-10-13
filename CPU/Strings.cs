using x86Disasm;
namespace x86CS.CPU
{
    public partial class CPU
    {
        private uint GetCount()
        {
            if ((disasm.SetPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat || ((disasm.SetPrefixes & OPPrefix.RepeatNotEqual) == OPPrefix.RepeatNotEqual))
                return addressSize == 32 ? ECX : CX;
            return 1;
        }

        private void SetCount(uint value)
        {
            if ((disasm.SetPrefixes & OPPrefix.Repeat) != OPPrefix.Repeat && ((disasm.SetPrefixes & OPPrefix.RepeatNotEqual) != OPPrefix.RepeatNotEqual))
                return;

            if (addressSize == 32)
                ECX = value;
            else
                CX = (ushort)value;
        }

        [CPUFunction(OpCode = 0xaa)]
        [CPUFunction(OpCode = 0xab)]
        public void StringStore(Operand dest, Operand source)
        {
            uint count = GetCount();
            int size = (int)(dest.Size / 8);

            while (count > 0)
            {
                uint addr;

                if (addressSize == 32)
                    addr = EDI;
                else
                    addr = DI;

                SegWrite(SegmentRegister.ES, addr, source.Value, (int)source.Size);
                if (DF)
                {
                    if (addressSize == 32)
                        EDI = (uint)(EDI - size);
                    else
                        DI = (ushort)(DI - size);
                }
                else
                {
                    if (addressSize == 32)
                        EDI = (uint)(EDI + size);
                    else
                        DI = (ushort)(DI + size);
                }

                count--;
            }
            SetCount(count);
        }

        [CPUFunction(OpCode = 0xae)]
        [CPUFunction(OpCode = 0xaf)]
        public void StringScan(Operand dest, Operand source)
        {
            uint count = GetCount();
            int size = (int)(dest.Size / 8);

            while (count > 0)
            {
                uint addr;

                if (addressSize == 32)
                    addr = EDI;
                else
                    addr = DI;

                dest.Value = SegRead(SegmentRegister.ES, addr, (int)dest.Size);
                Compare(dest, source);
                if (DF)
                {
                    if (addressSize == 32)
                        EDI = (uint)(EDI - size);
                    else
                        DI = (ushort)(DI - size);
                }
                else
                {
                    if (addressSize == 32)
                        EDI = (uint)(EDI + size);
                    else
                        DI = (ushort)(DI + size);
                }

                count--;

                if ((((disasm.SetPrefixes & OPPrefix.Repeat) == OPPrefix.Repeat) && !ZF) ||
                    ((disasm.SetPrefixes & OPPrefix.RepeatNotEqual) == OPPrefix.RepeatNotEqual) && ZF)
                    break;
            }
            SetCount(count);
        }
   }
}