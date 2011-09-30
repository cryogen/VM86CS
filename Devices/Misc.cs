using System;

namespace x86CS.Devices
{
    public class Misc : IDevice
    {
        private readonly int[] portsUsed = { 0x92, 0x402, 0x500 };
        private sbyte controlPortA;

// ReSharper disable InconsistentNaming
        private const int irqNumber = -1;
        private const int dmaChannel = -1;
        // ReSharper restore InconsistentNaming

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public int IRQNumber
        {
            get { return irqNumber; }
        }

        public int DMAChannel
        {
            get { return dmaChannel; }
        }

        public event EventHandler IRQ;
        public event EventHandler<Util.ByteArrayEventArgs> DMA;

        public void Cycle(double frequency, ulong tickCount)
        {
        }

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
