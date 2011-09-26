using System;

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
                    Memory.A20 = (controlPortA & 0x2) == 0x2;
                    break;
                case 0x402:
                case 0x500:
                    Console.Write((char)(byte)value);
                    break;
            }
        }
    }
}
