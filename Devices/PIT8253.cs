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
                }
                else if (accessMode == AccessMode.LoByteOnly || accessMode == AccessMode.LoByteHiByte)
                {
                    reloadLow = (byte)value;
                    setHiByte = true;
                }
            }
        }
    }

    public class PIT8253
    {
        private readonly Counter[] counters;

        public PIT8253()
        {
            counters = new Counter[3];
            counters[0] = new Counter();
            counters[1] = new Counter();
            counters[2] = new Counter();
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
