using System.IO;
using System.Runtime.InteropServices;
using System;

namespace x86CS.ATADevice
{
    public class HardDisk : ATADrive
    {
        private FileStream stream;
        private Footer footer;
        private ushort[] identifyBuffer;

        public ushort Cylinders
        {
            get { return footer.Cylinders; }
        }

        public byte Heads
        {
            get { return footer.Heads; }
        }

        public byte Sectors
        {
            get { return footer.SectorsPerCylinder; }
        }

        public HardDisk()
        {
            identifyBuffer = new ushort[256];

            identifyBuffer[0] = 0x40; // Fixed drive
            identifyBuffer[5] = 512; // Bytes per sector
            identifyBuffer[48] = 0x0; // double word i/o supported (disable for now)
            identifyBuffer[49] = 0x0300; // LBA and DMA supported
            identifyBuffer[62] = 0x0001; // Mode 0 active and supported
        }

        public override void LoadImage(string filename)
        {
            byte[] buffer = new byte[512];

            stream = File.OpenRead(filename);
            stream.Seek(-512, SeekOrigin.End);
            stream.Read(buffer, 0, 512);

            footer = Util.ByteArrayToStructureBigEndian<Footer>(buffer);

            stream.Seek(0, SeekOrigin.Begin);

            identifyBuffer[1] = identifyBuffer[54] = footer.Cylinders;
            identifyBuffer[3] = identifyBuffer[55] = footer.Heads;
            identifyBuffer[4] = (ushort)(footer.SectorsPerCylinder * 512);
            identifyBuffer[6] = identifyBuffer[56] = footer.SectorsPerCylinder;
            identifyBuffer[57] = identifyBuffer[60] = (ushort)(footer.CurrentSize / 512);
            identifyBuffer[58] = identifyBuffer[61] = (ushort)((footer.CurrentSize / 512) >> 16);
        }

        public override void Reset()
        {
            Error = 1;
            SectorNumber = 1;
            SectorCount = 1;
            CylinderLow = 0;
            CylinderHigh = 0;
            Status |= DeviceStatus.Busy;
        }

        public override void RunCommand(byte command)
        {
            switch (command)
            {
                case 0xec:
                    Status |= DeviceStatus.Busy;
                    sectorBuffer = identifyBuffer;
                    Status |= DeviceStatus.DataRequest;
                    Status &= ~DeviceStatus.Busy;
                    bufferIndex = 0;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }
    }
}
