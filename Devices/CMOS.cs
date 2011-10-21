using System;

namespace x86CS.Devices
{
    public class CMOS : IDevice
    {
        private readonly int[] portsUsed = {0x70, 0x71};
        private byte currentReg;
        private byte statusA;
        private byte statusB;
        private byte statusC;
        private byte statusD;

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public CMOS()
        {
            statusA = 0x26; /* default 32.768 divider and default rate selection */
            statusB = 0x02;  /* no DST, 12 hour clock, BCD, all flags cleared */
            statusC = 0x00;
            statusD = 0x80;
        }

        public uint Read(ushort addr, int size)
        {
            DateTime currTime = DateTime.Now;
            ushort ret = 0;

            switch (addr)
            {
                case 0x70: /* Write only */
                    ret = 0xff;
                    break;
                case 0x71:
                    switch (currentReg)
                    {
                        case 0x00:
                            return Util.ToBCD(currTime.Second);
                        case 0x02:
                            return Util.ToBCD(currTime.Minute);
                        case 0x04:
                            return Util.ToBCD(currTime.Hour);
                        case 0x06:
                            return Util.ToBCD((int)currTime.DayOfWeek);
                        case 0x07:
                            return Util.ToBCD(currTime.Day);
                        case 0x08:
                            return Util.ToBCD(currTime.Month);
                        case 0x09:
                            return Util.ToBCD(currTime.Year % 100);
                        case 0x10:
                            return 0x40;  /* 1.44M floppy drive */
                        case 0x12:
                            return 0x00;
                        case 0x0a:
                            return statusA; 
                        case 0x0b:
                            return statusB;
                        case 0x0c:
                            return statusC;
                        case 0x0d:
                            return statusD;
                        case 0x0f:
                            return 0x00;
                        case 0x14:
                            return 0x05;  /* Machine config byte */
                        case 0x15:
                            return 0x71;  /* Low byte of 640K memory available */
                        case 0x16:
                            return 0x02;  /* High byte of above */
                        case 0x17:
                            return 0x00;  /* Low byte of memory up to 64M */
                        case 0x18:
                            return 0x80;  /* High byte of above */
                        case 0x30:
                            return 0x00;    /* memory above 1M */
                        case 0x31:
                            return 0xbc;    /* High byte */
                        case 0x32:
                            return Util.ToBCD(currTime.Year / 100);
                        case 0x34:
                            return 0x00;  /* Low byte of memory up to 4GB */
                        case 0x35:
                            return 0x02;  /* High byte */
                        case 0x3d:
                            return 0x21;  /* 1st and 2nd boot devices */
                        case 0x38:
                            return 0x00;  /* 3rd boot device */
                        case 0x5b:
                            return 0x00;
                        case 0x5c:
                            return 0x00;
                        case 0x5d:
                            return 0x00;
                        default:
                            System.Diagnostics.Debugger.Break();
                            break;
                    }
                    break;
            }
            return ret;
        }

        public void Write(ushort addr, uint value, int size)
        {
            var tmp = (ushort)(value & 0x7f);

            switch (addr)
            {
                case 0x70:         
                    currentReg = (byte)tmp;
                    break;
                case 0x71:
                    switch (currentReg)
                    {
                        case 0x0a:
                            statusA = (byte)value;
                            break;
                        case 0x0b:
                            statusB = (byte)value;
                            break;
                        case 0x0f:
                            break;
                        default:
                            System.Diagnostics.Debugger.Break();
                            break;
                    }
                    break;
            }
        }
    }
}
