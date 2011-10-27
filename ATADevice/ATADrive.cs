using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS.ATADevice
{
    public abstract class ATADrive
    {
        public byte Error { get; set; }
        public byte SectorCount { get; set; }
        public byte SectorNumber { get; set; }
        public byte CylinderLow { get; set; }
        public byte CylinderHigh { get; set; }
        public byte DriveHead { get; set; }
        public DeviceStatus Status { get; set; }
        protected ushort[] sectorBuffer;
        protected int bufferIndex;

        public ushort Cylinder
        {
            get { return (ushort)((CylinderHigh << 8) + CylinderLow); }
            set { CylinderLow = (byte)value; CylinderHigh = (byte)(value >> 8); }
        }

        public ushort SectorBuffer
        {
            get
            {
                ushort ret = sectorBuffer[bufferIndex++];

                if (bufferIndex >= sectorBuffer.Length)
                {
                    Status &= ~DeviceStatus.DataRequest;
                    FinishRead();
                }
   
                return ret;
            }
            set
            {
                sectorBuffer[bufferIndex++] = value;

                if (bufferIndex >= sectorBuffer.Length)
                {
                    Status &= ~DeviceStatus.DataRequest;
                    FinishCommand();
                }
            }
        }

        public abstract void LoadImage(string filename);
        public abstract void Reset();
        public abstract void RunCommand(byte command);
        public abstract void FinishCommand();
        public abstract void FinishRead();
    }
}
