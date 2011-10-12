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

        [CPUFunction(OpCode=0xab)]
        public void StringStore(Operand dest, Operand source)
        {
            uint count = GetCount();

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
                        EDI -= (uint)dest.Size / 8;
                    else
                        DI -= (ushort)(dest.Size / 8);
                }
                else
                {
                    if (addressSize == 32)
                        EDI += (uint)source.Size / 8;
                    else
                        DI += (ushort)(source.Size / 8);
                }

                count--;
            }
            SetCount(count);
        }

      /*  private void StringInByte()
        {
            var count = GetCount();

            while (count > 0)
            {
                var value = (byte)DoIORead(DX, 8);

                SegWriteByte(SegmentRegister.ES, opSize == 32 ? EDI : DI, value);
                if (DF)
                    EDI--;
                else
                    EDI++;

                count--;
            }
            SetCount(count);
        }

        private void StringInWord()
        {
            uint count = GetCount();
            while (count > 0)
            {
                ushort value = (ushort)DoIORead(DX, 16);
                SegWriteWord(SegmentRegister.ES, opSize == 32 ? EDI : DI, value);
                if (DF)
                    EDI -= 2;
                else
                    EDI += 2;

                count--;
            }
            SetCount(count);
        }

        private void StringInDWord()
        {
            uint count = GetCount();
            while (count > 0)
            {
                uint value = DoIORead(DX, 32);
                SegWriteDWord(SegmentRegister.ES, opSize == 32 ? EDI : DI, value);
                if (DF)
                    EDI -= 4;
                else
                    EDI += 4;

                count--;
            }
            SetCount(count);
        }

        private void StringOutByte()
        {
         /*   uint count = GetCount();

            while (count > 0)
            {
                var value = SegReadByte(overrideSegment, opSize == 32 ? ESI : SI);
                DoIOWrite(DX, value, 8);
                if (DF)
                    ESI--;
                else
                    ESI++;

                count--;
            }
            SetCount(count);
        }

        private void StringOutWord()
        {
        /*    uint count = GetCount();

            while (count > 0)
            {
                var value = SegReadWord(overrideSegment, opSize == 32 ? ESI : SI);
                DoIOWrite(DX, value, 16);
                if (DF)
                    ESI -= 2;
                else
                    ESI += 2;

                count--;
            }
            SetCount(count);
        }

        private void StringOutDWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                var value = SegReadDWord(overrideSegment, opSize == 32 ? ESI : SI);
                DoIOWrite(DX, value, 32);
                if (DF)
                    ESI -= 4;
                else
                    ESI += 4;

                count--;
            }
            SetCount(count);
        }

        private void StringReadByte()
        {
            uint count = GetCount();

            while (count > 0)
            {
                AL = SegReadByte(overrideSegment, opSize == 32 ? ESI : SI);
                if (DF)
                    ESI--;
                else
                    ESI++;

                count--;
            }
            SetCount(count);
        }

        private void StringReadWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                AX = SegReadWord(overrideSegment, opSize == 32 ? ESI : SI);
                if (DF)
                    ESI -= 2;
                else
                    ESI += 2;

                count--;
            }
            SetCount(count);
        }

        private void StringReadDWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                EAX = SegReadDWord(overrideSegment, opSize == 32 ? ESI : SI);
                if (DF)
                    ESI -= 4;
                else
                    ESI += 4;

                count--;
            }
            SetCount(count);
        }
        */
        /*
        private void StringWriteByte()
        {
            uint count = GetCount();

            while (count > 0)
            {
                SegWriteByte(SegmentRegister.ES, opSize == 32 ? EDI : DI, AL);
                if (DF)
                    EDI--;
                else
                    EDI++;

                count--;
            }
            SetCount(count);
        }

        private void StringWriteWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                SegWriteWord(SegmentRegister.ES, opSize == 32 ? EDI : DI, AX);
                if (DF)
                    EDI -= 2;
                else
                    EDI += 2;

                count--;
            }
            SetCount(count);
        }

        private void StringWriteDWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                SegWriteDWord(SegmentRegister.ES, opSize == 32 ? EDI : DI, EAX);
                if (DF)
                    EDI -= 4;
                else
                    EDI += 4;

                count--;
            }
            SetCount(count);
        }

        private void StringScanByte()
        {
            uint count = GetCount();

            while (count > 0)
            {
                var source = SegReadByte(SegmentRegister.ES, opSize == 32 ? EDI :DI);
                Subtract(AL, source);
                if (DF)
                    EDI--;
                else
                    EDI++;

                count--;
                if (repeatPrefix == RepeatPrefix.RepeatNotZero && ZF)
                    break;
            }
            SetCount(count);
        }

        private void StringScanWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                var source = SegReadWord(SegmentRegister.ES, opSize == 32 ? EDI : DI);
                Subtract(AX, source);
                if (DF)
                    EDI -= 2;
                else
                    EDI += 2;

                count--;
            }
            SetCount(count);
        }

        private void StringScanDWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                var source = SegReadDWord(SegmentRegister.ES, opSize == 32 ? EDI : DI);
                Subtract(EAX, source);
                if (DF)
                    EDI -= 4;
                else
                    EDI += 4;

                count--;
            }
            SetCount(count);
        }

        private void StringCopyByte()
        {
            uint count = GetCount();

            while (count > 0)
            {
                SegWriteByte(SegmentRegister.ES, opSize == 32 ? EDI : DI, SegReadByte(overrideSegment, opSize == 32 ? ESI : SI));
                if (DF)
                {
                    ESI--;
                    EDI--;
                }
                else
                {
                    ESI++;
                    EDI++;
                }

                count--;
            }
            SetCount(count);
        }

        private void StringCopyWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                SegWriteWord(SegmentRegister.ES, opSize == 32 ? EDI : DI, SegReadWord(overrideSegment, opSize == 32 ? EDI : SI));
                if (DF)
                {
                    ESI -= 2;
                    EDI -= 2;
                }
                else
                {
                    ESI += 2;
                    EDI += 2;
                }

                count--;
            }
            SetCount(count);
        }

        private void StringCopyDWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                SegWriteDWord(SegmentRegister.ES, opSize == 32 ? EDI : DI, SegReadWord(overrideSegment, opSize == 32 ? EDI : SI));
                if (DF)
                {
                    ESI -= 4;
                    EDI -= 4;
                }
                else
                {
                    ESI += 4;
                    EDI += 4;
                }

                count--;
            }
            SetCount(count);
        }
        
        private void StringCompareByte()
        {
            uint count = GetCount();

            while (count > 0)
            {
                byte source = SegReadByte(overrideSegment, opSize == 32 ? ESI : SI);
                byte dest = SegReadByte(SegmentRegister.ES, opSize == 32 ? EDI : DI);

                Subtract(dest, source);
                if (DF)
                {
                    ESI--;
                    EDI--;
                }
                else
                {
                    ESI++;
                    EDI++;
                }

                count--;
                if (repeatPrefix == RepeatPrefix.Repeat && !ZF || repeatPrefix == RepeatPrefix.RepeatNotZero && ZF)
                    break;
            }
            SetCount(count);
        }

        private void StringCompareWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                ushort source = SegReadWord(overrideSegment, opSize == 32 ? ESI : SI);
                ushort dest = SegReadWord(SegmentRegister.ES, opSize == 32 ? EDI : DI);

                Subtract(dest, source);
                if (DF)
                {
                    ESI -= 2;
                    EDI -= 2;
                }
                else
                {
                    ESI += 2;
                    EDI += 2;
                }

                count--;
                if (repeatPrefix == RepeatPrefix.Repeat && !ZF || repeatPrefix == RepeatPrefix.RepeatNotZero && ZF)
                    break;
            }
            SetCount(count);
        }

        private void StringCompareDWord()
        {
            uint count = GetCount();

            while (count > 0)
            {
                uint source = SegReadDWord(overrideSegment, opSize == 32 ? ESI : SI);
                uint dest = SegReadDWord(SegmentRegister.ES, opSize == 32 ? EDI : DI);

                Subtract(dest, source);
                if (DF)
                {
                    ESI -= 4;
                    EDI -= 4;
                }
                else
                {
                    ESI += 4;
                    EDI += 4;
                }

                count--;
                if (repeatPrefix == RepeatPrefix.Repeat && !ZF || repeatPrefix == RepeatPrefix.RepeatNotZero && ZF)
                    break;
            }
            SetCount(count);
        }*/
    }
}