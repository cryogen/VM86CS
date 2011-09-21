using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS
{
    public class CMOS
    {
        private byte[] registers = new byte[0x10];
        private byte currentReg;

        public CMOS()
        {
            registers[0x0a] = 0x26;  /* default 32.768 divider and default rate selection */
            registers[0x0b] = 0x02;  /* no DST, 24 hour clock, BCD, all flags cleared */
            registers[0x0c] = 0x00;  /* all int flags cleared */
            registers[0x0d] = 0x80;  /* VRT */
        }

        public ushort Read(ushort addr)
        {
            ushort ret;

            switch (addr)
            {
                case 0x70: /* Write only */
                    ret = 0xff;
                    break;
                case 0x71:
                    ret = registers[currentReg];
                    if (currentReg == 0x0c)
                        registers[0x0c] = 0;
                    break;
                default:
                    ret = 0;
                    break;
            }

            return ret;
        }

        public void Write(ushort addr, ushort value)
        {
            ushort tmp;

            tmp = (ushort)(value & 0x7f);

            switch (addr)
            {
                case 0x70:         
                    currentReg = (byte)tmp;
                    break;
                case 0x71:
                    registers[currentReg] = (byte)value;
/*                    switch (currentReg)
                    {
                    }*/
                    break;
            }
        }
    }
}
