using System;
using System.Threading;
using System.Diagnostics;

namespace x86CS.Devices
{
    public class PIT8253 : IDevice, INeedsIRQ, IShutdown
    {
        private const int IrqNumber = 0;

        private readonly int[] portsUsed = { 0x40, 0x41, 0x42, 0x43, 0x61 };
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

        public uint Read(ushort address, int size)
        {
            switch (address)
            {
                case 0x40:
                    return 0x0000;
                case 0x43:
                    return 0xffff;
                case 0x61:
                    return (uint)((counters[0].Count & 1) << 4);
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }

            return 0;
        }

        public void Write(ushort address, uint value, int size)
        {
            switch (address)
            {
                case 0x40:
                    counters[0].Reload = (ushort)value;
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

        public void Shutdown()
        {
            counters[0].Running = false;
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
        private double counter;
        private Thread counterThread;

        public event EventHandler TimerTick;

        public OperatingMode OperatingMode { get; set; }
        public bool Running { get; set; }

        public AccessMode AccessMode
        {
            get { return accessMode; }
            set { accessMode = value; }
        }

        public bool BCDMode { get; set; }

        public ulong Count
        {
            get { return (ulong)counter; }
        }

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
                        counter = 0x10000;
                    else
                        counter = Reload;
                    Running = true;
                    if(!counterThread.IsAlive)
                        counterThread.Start();

                }
                else if (accessMode == AccessMode.LoByteOnly || accessMode == AccessMode.LoByteHiByte)
                {
                    reloadLow = (byte)value;
                    setHiByte = true;
                    if (accessMode == AccessMode.LoByteOnly)
                    {
                        if (Reload == 0)
                            counter = 0x10000;
                        else
                            counter = Reload;
                        Running = true;
                        counterThread.Start();
                    }
                }
            }
        }

        public Counter()
        {
            counter = 0;
            Running = false;

            counterThread = new Thread(RunTimer);
        }

        private void RunTimer()
        {
            Stopwatch stopwatch = new Stopwatch();
            const double frequency = (double)1 / (3579545 / 3);

            stopwatch.Start();
            
            while (Running)
            {
                counter -= stopwatch.Elapsed.TotalSeconds / frequency;
                stopwatch.Reset();
                stopwatch.Start();

                if (counter <= 1)
                {
                    OnTimerTick(new EventArgs());
                    if (Reload == 0)
                        counter = 0x10000;
                    else
                        counter = Reload;
                }
                Thread.Sleep(0);
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
