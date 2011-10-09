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

        uint Read(ushort addr, int size);
        void Write(ushort addr, uint value, int size);
    }

    public interface INeedsIRQ
    {
        int IRQNumber { get; }
        event EventHandler IRQ;
    }

    public interface INeedsDMA
    {
        int DMAChannel { get; }

        event EventHandler<ByteArrayEventArgs> DMA;
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
