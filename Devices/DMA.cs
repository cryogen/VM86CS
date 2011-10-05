using System;
using log4net;

namespace x86CS.Devices
{
    public class DMAController : IDevice
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DMAController));

        private readonly int[] portsUsed = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x80, 0x81, 0x82, 0x83};
        
        private readonly byte[] pages;
        private readonly ushort[] memAddress;
        private readonly ushort[] counts;

        private byte mask;
        private byte command;
        private byte status;
        private byte request;
        private byte temp;
        private byte mode;
        private bool flipFlop;

        public int[] PortsUsed
        {
            get { return portsUsed; }
            }
        
        public DMAController()
        {
            pages = new byte[16];
            memAddress = new ushort[16];
            counts = new ushort[16];
            Reset();
        }

        public void DoTransfer(int channel, byte[] data)
        {
            ushort address = memAddress[channel];
            ushort length = counts[channel+1];  
            uint fullAddr = (uint)((pages[channel] << 16) + address);

            Logger.Debug(String.Format("Transferring {0} bytes from {1:X}", length + 1, fullAddr));

            Memory.BlockWrite(fullAddr, data, length + 1);
        }

        private void Reset()
        {
            mask = 0xff;
            command = status = request = temp = mode = 0;
            flipFlop = false;
        }

        private void SetAddressOrCount(byte index, byte value)
        {
            bool address = true;
            int channel = index/2;
            int tmp = index%2;
  
            if (tmp != 0)
            {
                channel += tmp;
                address = false;
            }

            if(flipFlop)
            {
                if (address)
                    memAddress[channel] = memAddress[channel].SetHigh(value);
                else
                    counts[channel] = counts[channel].SetHigh(value);
                flipFlop = false;
            }
            else
            {
                if (address)
                    memAddress[channel] = memAddress[channel].SetLow(value);
                else
                    counts[channel] = counts[channel].SetLow(value);
                flipFlop = true;
            }
        }

        public ushort Read(ushort address)
        {
            switch (address)
            {
                case 0x80:
                    return pages[0];
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
                case 0x0:
                case 0x1:
                case 0x2:
                case 0x3:
                case 0x4:
                case 0x5:
                case 0x7:
                    SetAddressOrCount((byte)address, (byte)value);
                    break;
                case 0x0a:
                    byte set = (byte)((value >> 2) & 0x1);
                    byte which = (byte)(value & 0x3);

                    if (set == 1)
                        mask |= (byte)(1 << which);
                    else
                        mask &= (byte)~(1 << which);
                    break;
                case 0x0b:
                    mode = (byte)value;
                    break;
                case 0x0c:
                    flipFlop = false;
                    break;
                case 0x0d:
                    Reset();
                    break;
                case 0x80:
                    pages[0] = (byte) value;
                    break;
                case 0x81:
                    pages[2] = (byte) value;
                    break;
                case 0x82:
                    pages[3] = (byte) value;
                    break;
                case 0x83:
                    pages[1] = (byte)value;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }
    }
}
