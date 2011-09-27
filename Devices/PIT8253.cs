using System;
using System.Threading;
using System.Diagnostics;

namespace x86CS.Devices
{
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
        private readonly Thread timerThread;
        private readonly Stopwatch stopwatch;
        private ulong counter;

        public event EventHandler TimerCycle;

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
                    stopwatch.Start();
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
                        stopwatch.Start();
                    }
                }
            }
        }

        public Counter()
        {
            stopwatch = new Stopwatch();
            counter = 0;
            timerThread = new Thread(TimerRun);
            timerThread.Start();
        }

        private void TimerRun()
        {
            while (true)
            {
                if(!stopwatch.IsRunning)
                    Thread.Sleep(100);
                else
                {
                    if (stopwatch.ElapsedTicks > 9)
                        counter--;
                    if (counter == 1)
                    {
                        OnTimerCycle(new EventArgs());
                        if (Reload == 0)
                            counter = 1193182 / 0x10000;
                        else
                            counter = (ulong)1193182 / Reload;
                        stopwatch.Reset();
                        stopwatch.Start();
                    }
                }
            }
        }

        private void OnTimerCycle(EventArgs e)
        {
            EventHandler handler = TimerCycle;
            if (handler != null) 
                handler(this, e);
        }
    }

    public class PIT8253
    {
        private readonly Counter[] counters;

        public event EventHandler InteruptRequested;

        public void OnInteruptRequested(EventArgs e)
        {
            EventHandler handler = InteruptRequested;
            if (handler != null) 
                handler(this, e);
        }

        public PIT8253()
        {
            counters = new Counter[3];
            counters[0] = new Counter();
            counters[0].TimerCycle += PIT8253TimerCycle;
            counters[1] = new Counter();
            counters[2] = new Counter();
        }

        void PIT8253TimerCycle(object sender, EventArgs e)
        {
            OnInteruptRequested(e);
        }

        public ushort Read(ushort address)
        {
            switch (address)
            {
                case 0x43:
                    return 0xffff;
                default:
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
}
