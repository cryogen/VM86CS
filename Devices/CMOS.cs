using System;

namespace x86CS.Devices
{
    public class CMOS
    {
        private readonly byte[] registers = new byte[0x20];
        private byte currentReg;

        public CMOS()
        {
            DateTime currTime = DateTime.Now;

            registers[0x00] = Util.ToBCD(currTime.Second);
            registers[0x02] = Util.ToBCD(currTime.Minute);
            registers[0x04] = Util.ToBCD(currTime.Hour);
            registers[0x06] = Util.ToBCD((int)currTime.DayOfWeek);
            registers[0x08] = Util.ToBCD(currTime.Month);
            registers[0x09] = Util.ToBCD(currTime.Year);
            registers[0x0a] = 0x26;  /* default 32.768 divider and default rate selection */
            registers[0x0b] = 0x02;  /* no DST, 24 hour clock, BCD, all flags cleared */
            registers[0x0c] = 0x00;  /* all int flags cleared */
            registers[0x0d] = 0x80;  /* VRT */
            registers[0x14] = 0x05;  /* Machine config byte */
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
            var tmp = (ushort)(value & 0x7f);

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
