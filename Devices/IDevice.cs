using System;

namespace x86CS.Devices
{
    public interface IDevice
    {
        int[] PortsUsed { get; }
        int IRQNumber { get; }
        int DMAChannel { get; }

        event EventHandler IRQ;

        void Cycle(double frequency, ulong tickCount);
        ushort Read(ushort addr);
        void Write(ushort addr, ushort value);
    }
}
