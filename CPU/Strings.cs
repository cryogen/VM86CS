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

        [CPUFunction(OpCode = 0xac)]
        [CPUFunction(OpCode = 0xad)]
        public void StringLoad(Operand dest, Operand source)
        {
            uint count = GetCount();
            int size = (int)(dest.Size / 8);

            while (count > 0)
            {
                uint addr;

                if (addressSize == 32)
                    addr = ESI;
                else
                    addr = SI;

                dest.Value = source.Value;
                WriteOperand(dest);
                if (DF)
                {
                    if (addressSize == 32)
                        ESI = (uint)(ESI - size);
                    else
                        SI = (ushort)(SI - size);
                }
                else
                {
                    if (addressSize == 32)
                        ESI = (uint)(ESI + size);
                    else
                        SI = (ushort)(SI + size);
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

        [CPUFunction(OpCode = 0xa4)]
        public void StringMoveByte(Operand dest, Operand source)
        {
            DoStringMove(dest, source, 8);
        }

        [CPUFunction(OpCode = 0xa5)]
        public void StringMoveWord(Operand dest, Operand source)
        {
            DoStringMove(dest, source, opSize);
        }

        private void DoStringMove(Operand dest, Operand source, int size)
        {
            uint count = GetCount();

            while (count > 0)
            {
                uint destAddr, sourceAddr;

                if (addressSize == 32)
                {
                    destAddr = EDI;
                    sourceAddr = ESI;
                }
                else
                {
                    destAddr = DI;
                    sourceAddr = SI;
                }

                SegWrite(SegmentRegister.ES, destAddr, SegRead(disasm.OverrideSegment == SegmentRegister.Default ? SegmentRegister.DS : disasm.OverrideSegment, sourceAddr, (int)source.Size), (int)dest.Size);
                if (DF)
                {
                    if (addressSize == 32)
                    {
                        EDI = (uint)(EDI - (size / 8));
                        ESI = (uint)(ESI - (size / 8));
                    }
                    else
                    {
                        DI = (ushort)(DI - (size / 8));
                        SI = (ushort)(SI - (size / 8));
                    }
                }
                else
                {
                    if (addressSize == 32)
                    {
                        EDI = (uint)(EDI + (size / 8));
                        ESI = (uint)(ESI + (size / 8));
                    }
                    else
                    {
                        DI = (ushort)(DI + (size / 8));
                        SI = (ushort)(SI + (size / 8));
                    }
                }

                count--;
            }
            SetCount(count);
        }

        [CPUFunction(OpCode = 0xa6)]
        public void StringCompareByte(Operand dest, Operand source)
        {
            DoStringCompare(dest, source, 8);
        }

        [CPUFunction(OpCode = 0xa7)]
        public void StringCompareWord(Operand dest, Operand source)
        {
            DoStringCompare(dest, source, opSize);
        }

        private void DoStringCompare(Operand dest, Operand source, int size)
        {
            uint count = GetCount();

            while (count > 0)
            {
                uint destAddr, sourceAddr;

                if (addressSize == 32)
                {
                    destAddr = EDI;
                    sourceAddr = ESI;
                }
                else
                {
                    destAddr = DI;
                    sourceAddr = SI;
                }

                source.Value = SegRead(disasm.OverrideSegment == SegmentRegister.Default ? SegmentRegister.DS : disasm.OverrideSegment, sourceAddr, (int)source.Size);
                dest.Value = SegRead(SegmentRegister.ES, destAddr, (int)dest.Size);

                Compare(dest, source);

                if (DF)
                {
                    if (addressSize == 32)
                    {
                        EDI = (uint)(EDI - (size / 8));
                        ESI = (uint)(ESI - (size / 8));
                    }
                    else
                    {
                        DI = (ushort)(DI - (size / 8));
                        SI = (ushort)(SI - (size / 8));
                    }
                }
                else
                {
                    if (addressSize == 32)
                    {
                        EDI = (uint)(EDI + (size / 8));
                        ESI = (uint)(ESI + (size / 8));
                    }
                    else
                    {
                        DI = (ushort)(DI + (size / 8));
                        SI = (ushort)(SI + (size / 8));
                    }
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