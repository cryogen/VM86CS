using System;

namespace x86CS.Devices
{
    public class PIT8253 : IDevice, INeedsIRQ, INeedsClock
    {
        private const int IrqNumber = 0;

        private readonly int[] portsUsed = { 0x40, 0x41, 0x42, 0x43 };
        private readonly Counter[] counters;

        public event EventHandler IRQ;

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public int IRQNumber
        {
            get { return IrqNumber; }
        }

        public PIT8253()
        {
            counters = new Counter[3];
            counters[0] = new Counter();
            counters[0].TimerTick += PIT8253TimerCycle;
            counters[1] = new Counter();
            counters[2] = new Counter();
        }

        public void Cycle(double frequency, ulong timerTicks)
        {
            foreach (Counter counter in counters)
            {
                counter.Cycle(frequency, timerTicks);
            }
        }

        void PIT8253TimerCycle(object sender, EventArgs e)
        {
            OnIRQ(e);
        }

        public void OnIRQ(EventArgs e)
        {
            EventHandler handler = IRQ;
            if (handler != null)
                handler(this, e);
        }

        public ushort Read(ushort address)
        {
            switch (address)
            {
                case 0x40:
                    return 0x000;
                case 0x43:
                    return 0xffff;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }

            return 0;
        }

        public void Write(ushort address, ushort value)
        {
            switch (address)
            {
                case 0x40:
                    counters[0].Reload = value;
                    break;
                case 0x43:

                    var bcdMode = (byte)(value & 0x1);
                    var operatingMode = (byte)((value >> 1) & 0x7);
                    var accessMode = (byte)((value >> 4) & 0x3);
                    var channel = (byte)((value >> 6) & 0x3);

                    counters[channel].AccessMode = (AccessMode)accessMode;
                    counters[channel].OperatingMode = (OperatingMode)operatingMode;
                    counters[channel].BCDMode = (bcdMode == 1);
                    break;
            }
        }
    }

    public enum OperatingMode
    {
        InterruptOnTerminalCount = 0,
        HardwareOneShot,
        RateGenerator,
        SquareWaveGenerator,
        SoftwareStrobe,
        HardwareStrobe,
        RateGenerator2,
        SquareWaveGenerator2
    }

    public enum AccessMode
    {
        LatchCountValue = 0,
        LoByteOnly,
        HibyteOnly,
        LoByteHiByte
    }

    public class Counter
    {
        private AccessMode accessMode;
        private bool setHiByte;
        private byte reloadLow;
        private byte reloadHigh;
        private ulong counter, lastTicks;
        private bool running;

        public event EventHandler TimerTick;

        public OperatingMode OperatingMode { get; set; }

        public AccessMode AccessMode
        {
            get { return accessMode; }
            set { accessMode = value; }
        }

        public bool BCDMode { get; set; }

        public ushort Reload
        {
            get { return (ushort)((reloadHigh << 8) + reloadLow); }
            set
            {
                if (accessMode == AccessMode.HibyteOnly || (accessMode == AccessMode.LoByteHiByte && setHiByte))
                {
                    reloadHigh = (byte)value;
                    setHiByte = false;
                    if (Reload == 0)
                        counter = 1193182 / 0x10000;
                    else
                        counter = (ulong)1193182 / Reload;
                    running = true;
                }
                else if (accessMode == AccessMode.LoByteOnly || accessMode == AccessMode.LoByteHiByte)
                {
                    reloadLow = (byte)value;
                    setHiByte = true;
                    if (accessMode == AccessMode.LoByteOnly)
                    {
                        if (Reload == 0)
                            counter = 1193182 / 0x10000;
                        else
                            counter = (ulong)1193182 / Reload;
                        running = true;
                    }
                }
            }
        }

        public Counter()
        {
            counter = 0;
            running = false;
        }

        public void Cycle(double frequency, ulong timerTicks)
        {
            if (!running)
                return;

            double pulse = 1193182 / frequency;
            double ticks = timerTicks - lastTicks;

            if(ticks > pulse)
            {
                counter--;
                lastTicks = timerTicks;
            }

            if (counter == 1)
            {
                OnTimerTick(new EventArgs());
                if (Reload == 0)
                    counter = 1193182 / 0x10000;
                else
                    counter = (ulong)1193182 / Reload;
            }
        }

        private void OnTimerTick(EventArgs e)
        {
            EventHandler handler = TimerTick;
            if (handler != null)
                handler(this, e);
        }
    }
}
