using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS.Devices
{
    public class Misc
    {
        private sbyte controlPortA;

        public ushort Read(ushort addr)
        {
            switch (addr)
            {
                case 0x92:
                    if (Memory.A20)
                        controlPortA |= 0x2;
                    else
                        controlPortA &= ~0x2;
                    return (byte)controlPortA;
            }

            return 0;
        }

        public void Write(ushort addr, ushort value)
        {
            switch (addr)
            {
                case 0x92:
                    controlPortA = (sbyte)value;
                    if ((controlPortA & 0x2) == 0x2)
                        Memory.A20 = true;
                    else
                        Memory.A20 = false;
                    break;
            }
        }
    }
}
