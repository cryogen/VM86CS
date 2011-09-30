using System;

namespace x86CS.Devices
{
    public struct MemoryMapRegion
    {
        public int Base;
        public int Length;
    }

    public interface IDevice
    {
        int[] PortsUsed { get; }

        ushort Read(ushort addr);
        void Write(ushort addr, ushort value);
    }

    public interface INeedsIRQ
    {
        int IRQNumber { get; }
        event EventHandler IRQ;
    }

    public interface INeedsDMA
    {
        int DMAChannel { get; }

        event EventHandler<Util.ByteArrayEventArgs> DMA;
    }

    public interface INeedsClock
    {
        void Cycle(double frequency, ulong tickCount);
    }

    public interface INeedsMMIO
    {
        MemoryMapRegion[] MemoryMap { get; }
    }
}
