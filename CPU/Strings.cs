using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS
{
    public partial class CPU
    {
        private uint GetCount()
        {
            if (repeatPrefix == RepeatPrefix.Repeat)
            {
                if (opSize == 32)
                    return ECX;
                else
                    return CX;
            }
            else
                return 1;
        }

        private void SetCount(uint value)
        {
            if (repeatPrefix == RepeatPrefix.None)
                return;

            if (opSize == 32)
                ECX = value;
            else
                CX = (ushort)value;
        }

        private void StringInByte()
        {
            byte value;
            uint count;

            count = GetCount();

            while (count > 0)
            {
                value = (byte)DoIORead(DX);
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
            ushort value;
            uint count;

            count = GetCount();
            while (count > 0)
            {
                value = DoIORead(DX);
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
            uint value;
            uint count;

            count = GetCount();
            while (count > 0)
            {
                value = DoIORead(DX);
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
            byte value;
            uint count;

            count = GetCount();
            while (count > 0)
            {
                value = SegReadByte(overrideSegment, opSize == 32 ? ESI : SI);
                DoIOWrite(DX, value);
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
            ushort value;
            uint count;

            count = GetCount();

            while (count > 0)
            {
                value = SegReadWord(overrideSegment, opSize == 32 ? ESI : SI);
                DoIOWrite(DX, value);
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
            uint value;
            uint count;

            count = GetCount();

            while (count > 0)
            {
                value = SegReadDWord(overrideSegment, opSize == 32 ? ESI : SI);
                DoIOWrite(DX, (ushort)value);
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
            byte source;
            uint count = GetCount();

            while (count > 0)
            {
                source = SegReadByte(SegmentRegister.ES, opSize == 32 ? EDI :DI);
                Subtract(AL, source);
                if (DF)
                    EDI--;
                else
                    EDI++;

                count--;
            }
            SetCount(count);
        }

        private void StringScanWord()
        {
            ushort source;
            uint count = GetCount();

            while (count > 0)
            {
                source = SegReadWord(SegmentRegister.ES, opSize == 32 ? EDI : DI);
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
            uint source;
            uint count = GetCount();

            while (count > 0)
            {
                source = SegReadDWord(SegmentRegister.ES, opSize == 32 ? EDI : DI);
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
                    ESI--;
                else
                    ESI++;

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
                    ESI -= 2;
                else
                    ESI += 2;

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
                    ESI -= 4;
                else
                    ESI += 4;

                count--;
            }
            SetCount(count);
        }
    }
}