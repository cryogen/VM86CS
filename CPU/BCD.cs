﻿using System;

namespace x86CS.CPU
{
    public partial class CPU
    {
        private void ASCIIAdjustAfterAdd()
        {
            if (((AL & 0xf) > 9) || AF)
            {
                AL += 6;
                AH++;
                AF = true;
                CF = true;
            }
            else
            {
                AF = false;
                CF = false;
            }

            AL &= 0xf;
        }

        private void ASCIIAdjustAfterDivide(byte baseNum)
        {
            byte tempAL = AL;
            byte tempAH = AH;

            AL = (byte)((tempAL + (tempAH * baseNum)) & 0xff);
            AH = 0;

            SetCPUFlags(AL);
        }

        private void ASCIIAdjustAfterMultiply(byte baseNum)
        {
            if (baseNum == 0)
                throw new Exception("Divide Error");

            byte tempAL = AL;
            AH = (byte)(tempAL / baseNum);
            AL = (byte)(tempAL % baseNum);

            SetCPUFlags(AL);
        }

        private void ASCIIAdjustAfterSubtract()
        {
            if (((AL & 0xf) > 9) || AF)
            {
                AL -= 6;
                AH--;
                AF = true;
                CF = true;
            }
            else
            {
                AF = false;
                CF = false;
            }

            AL &= 0xf;
        }

        private void DecAdjustAfterAddition()
        {
            if (((AL & 0xf) > 9) || AF)
            {
                bool carry = false;

                var temp = (ushort)(AL + 6);
                if (temp > byte.MaxValue)
                    carry = true;

                CF = (CF | carry);

                AF = true;
                AL += 6;
            }
            else
                AF = false;

            if (((AL & 0xf0) > 0x90) || CF)
            {
                AL += 0x60;
                CF = true;
            }
            else
                CF = false;
        }

        private void DecAdjustAfterSubtract()
        {
            if (((AL & 0x0f) > 9) || AF)
            {
                bool carry = false;

                if (AL < 6)
                    carry = true;

                AL -= 6;

                CF = CF | carry;
                AF = true;
            }
            else
                AF = false;
            if ((AL > 0x9f) || CF)
            {
                AL -= 0x60;
                CF = true;
            }
            else
                CF = false;
        }
    }
}