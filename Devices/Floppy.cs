using System;
using System.IO;
using System.Runtime.InteropServices;

namespace x86CS.Devices
{
    public class Floppy : IDevice
    {
        private FileStream floppyStream;
        private BinaryReader floppyReader;
        private readonly int[] portsUsed = { 0x3f0, 0x3f1, 0x3f2, 0x3f4, 0x3f5, 0x3f7 };

        private const int IrqNumber = 6;
        private const int DmaChannel = 2;
        public bool Mounted { get; private set; }

        public Floppy()
        {
            Mounted = false;
        }

        public bool MountImage(string imagePath)
        {
            if (!File.Exists(imagePath))
                return false;

            try
            {
                floppyStream = File.OpenRead(imagePath);
                floppyReader = new BinaryReader(floppyStream);
            }
            catch (Exception)
            {
                return false;
            }

            Mounted = true;
            return true;
        }

        #region IDevice Members

        public void Cycle(double frequency, ulong tickCount)
        {
            
        }

        public ushort Read(ushort addr)
        {
            return 0;
        }

        public void Write(ushort addr, ushort value)
        {
            
        }

        #endregion

        #region IDevice Members

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

        #endregion
    }
}