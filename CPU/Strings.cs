using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS
{
    public partial class CPU
    {
        private void StringInByte(bool repeat)
        {
            byte value;

            if (repeat)
            {
                while (CX > 0)
                {
                    value = (byte)DoIORead(DX);
                    SegWriteByte(SegmentRegister.ES, DI, value);
                    if (DF)
                        DI--;
                    else
                        DI++;

                    CX--;
                }
            }
            else
            {
                value = (byte)DoIORead(DX);
                SegWriteByte(SegmentRegister.ES, DI, value);
                if (DF)
                    DI--;
                else
                    DI++;
            }
        }

        private void StringOutByte(bool repeat)
        {
            byte value;

            if (repeat)
            {
                while (CX > 0)
                {
                    value = SegReadByte(overrideSegment, SI);
                    DoIOWrite(DX, value);
                    if (DF)
                        SI--;
                    else
                        SI++;

                    CX--;
                }
            }
            else
            {
                value = SegReadByte(overrideSegment, SI);
                DoIOWrite(DX, value);
                if (DF)
                    SI--;
                else
                    SI++;
            }
        }

        private void StringReadByte(bool repeat)
        {
            if (repeat)
            {
                while (CX > 0)
                {
                    AL = SegReadByte(overrideSegment, SI);
                    if (DF)
                        SI--;
                    else
                        SI++;

                    CX--;
                }
            }
            else
            {
                AL = SegReadByte(overrideSegment, SI);
                if (DF)
                    SI--;
                else
                    SI++;
            }
        }

        private void StringWriteByte(bool repeat)
        {
            if (repeat)
            {
                while (CX > 0)
                {
                    SegWriteByte(SegmentRegister.ES, DI, AL);
                    if (DF)
                        DI--;
                    else
                        DI++;

                    CX--;
                }
            }
            else
            {
                SegWriteByte(SegmentRegister.ES, DI, AL);
                if (DF)
                    DI--;
                else
                    DI++;
            }
        }

        private void StringScanByte(bool repeat)
        {
            byte source;

            if (repeat)
            {
                while (CX > 0)
                {
                    source = SegReadByte(SegmentRegister.ES, DI);
                    Subtract(AL, source);
                    if (DF)
                        DI--;
                    else
                        DI++;

                    CX--;
                }
            }
            else
            {
                source = SegReadByte(SegmentRegister.ES, DI);
                Subtract(AL, source);
                if (DF)
                    DI--;
                else
                    DI++;
            }
        }

        private void StringCopyByte(bool repeat)
        {
            if (repeat)
            {
                while (CX > 0)
                {
                    SegWriteByte(SegmentRegister.ES, DI, SegReadByte(overrideSegment, SI));
                    if (DF)
                        SI--;
                    else
                        SI++;

                    CX--;
                }
            }
            else
            {
                SegWriteByte(SegmentRegister.ES, DI, SegReadByte(overrideSegment, SI));
                if (DF)
                    SI--;
                else
                    SI++;
            }
        }

        private void StringInWord(bool repeat)
        {
            ushort value;

            if (repeat)
            {
                while (CX > 0)
                {
                    value = DoIORead(DX);
                    SegWriteWord(SegmentRegister.ES, DI, value);
                    if (DF)
                        DI -= 2;
                    else
                        DI += 2;

                    CX--;
                }
            }
            else
            {
                value = DoIORead(DX);
                SegWriteWord(SegmentRegister.ES, DI, value);
                if (DF)
                    DI -= 2;
                else
                    DI += 2;
            }
        }

        private void StringOutWord(bool repeat)
        {
            ushort value;

            if (repeat)
            {
                while (CX > 0)
                {
                    value = SegReadWord(overrideSegment, SI);
                    DoIOWrite(DX, value);
                    if (DF)
                        SI--;
                    else
                        SI++;

                    CX--;
                }
            }
            else
            {
                value = SegReadWord(overrideSegment, SI);
                DoIOWrite(DX, value);
                if (DF)
                    SI--;
                else
                    SI++;
            }
        }

        private void StringReadWord(bool repeat)
        {
            if (repeat)
            {
                while (CX > 0)
                {
                    AX = SegReadWord(overrideSegment, SI);
                    if (DF)
                        SI -= 2;
                    else
                        SI += 2;

                    CX--;
                }
            }
            else
            {
                AX = SegReadWord(overrideSegment, SI);
                if (DF)
                    SI -= 2;
                else
                    SI += 2;
            }
        }

        private void StringWriteWord(bool repeat)
        {
            if (repeat)
            {
                while (CX > 0)
                {
                    SegWriteWord(SegmentRegister.ES, DI, AX);
                    if (DF)
                        DI -= 2;
                    else
                        DI += 2;

                    CX--;
                }
            }
            else
            {
                SegWriteWord(SegmentRegister.ES, DI, AX);
                if (DF)
                    DI -= 2;
                else
                    DI += 2;
            }
        }

        private void StringCopyWord(bool repeat)
        {
            if (repeat)
            {
                while (CX > 0)
                {
                    SegWriteWord(SegmentRegister.ES, DI, SegReadWord(overrideSegment, SI));
                    if (DF)
                        SI -= 2;
                    else
                        SI += 2;

                    CX--;
                }
            }
            else
            {
                SegWriteWord(SegmentRegister.ES, DI, SegReadWord(overrideSegment, SI));
                if (DF)
                    SI -= 2;
                else
                    SI += 2;
            }
        }

        private void StringScanWord(bool repeat)
        {
            ushort source;

            if (repeat)
            {
                while (CX > 0)
                {
                    source = SegReadWord(SegmentRegister.ES, DI);
                    Subtract(AX, source);
                    if (DF)
                        DI--;
                    else
                        DI++;

                    CX--;
                }
            }
            else
            {
                source = SegReadWord(SegmentRegister.ES, DI);
                Subtract(AX, source);
                if (DF)
                    DI--;
                else
                    DI++;
            }
        }
    }
}