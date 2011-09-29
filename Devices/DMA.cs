using System;

namespace x86CS.Devices
{
    public class DMA : IDevice
    {
        private readonly int[] portsUsed = {0x80};
        private readonly byte[] extraPageRegisters;

        private const int IrqNumber = -1;
        private const int DmaChannel = -1;

        public DMA()
        {
            extraPageRegisters = new byte[16];
        }

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public int IRQNumber
        {
            get { return IrqNumber; }
        }

        public int DMAChannel
        {
            get { return DmaChannel; }
        }

        public event EventHandler IRQ;

        public void Cycle(double frequency, ulong tickCount)
        {
        }

        public ushort Read(ushort address)
        {
            switch (address)
            {
                case 0x80:
                    return extraPageRegisters[0];
                default:
                    break;
            }

            return 0;
        }

        public void Write(ushort address, ushort value)
        {
            switch (address)
            {
                case 0x80:
                    extraPageRegisters[0] = (byte)value;
                    break;
                default:
                    break;
            }
        }
    }
}
