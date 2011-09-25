namespace x86CS.Devices
{
    public class DMA
    {
        private readonly byte[] extraPageRegisters;

        public DMA()
        {
            extraPageRegisters = new byte[16];
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
