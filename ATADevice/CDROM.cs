using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace x86CS.ATADevice
{
    public class CDROM : ATADrive
    {
        private ushort[] identifyBuffer;
        private byte lastCommand;
        private FileStream isoStream;

        public CDROM()
        {
            identifyBuffer = new ushort[256];

            identifyBuffer[0] = 0x85c0;
            Util.ByteArrayToUShort(Encoding.ASCII.GetBytes("12345678901234567890"), identifyBuffer, 10, true);
            Util.ByteArrayToUShort(Encoding.ASCII.GetBytes("x86 CS CDROM                            "), identifyBuffer, 27, true);
            identifyBuffer[49] = 0x0300;
            identifyBuffer[50] = 0x4000;
            identifyBuffer[53] = 0x0003;
            identifyBuffer[63] = 0x0103;
            identifyBuffer[64] = 0x0001;
            identifyBuffer[65] = 0x00b4;
            identifyBuffer[66] = 0x00b4;
            identifyBuffer[67] = 0x012c; 
            identifyBuffer[68] = 0x00b4;
            identifyBuffer[71] = 0x001e;
            identifyBuffer[72] = 0x001e;
            identifyBuffer[80] = 0x007e;
            identifyBuffer[82] = 0x4010;
            identifyBuffer[83] = 0x4000;
            identifyBuffer[84] = 0x4000;
            identifyBuffer[85] = 0x4010;
            identifyBuffer[86] = 0x4000;
            identifyBuffer[87] = 0x4000;
        }

        public override void LoadImage(string filename)
        {
            isoStream = File.OpenRead(filename);
        }

        public override void Reset()
        {
            Error = 0;
            SectorNumber = 1;
            SectorCount = 1;
            CylinderLow = 0x14;
            CylinderHigh = 0xeb;
            Status = DeviceStatus.Busy | DeviceStatus.SeekComplete;
        }

        public override void RunCommand(byte command)
        {
            switch (command)
            {
                case 0xa0:
                    sectorBuffer = new ushort[6];
                    Status |= DeviceStatus.DataRequest;
                    bufferIndex = 0;
                    break;
                case 0xa1:
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

        public override void FinishCommand()
        {
            switch ((byte)sectorBuffer[0])
            {
                case 0x3:
                    RequestSense();
                    break;
                case 0x25:
                    ReadCapacity();
                    break;
                case 0x28:
                    Read10();
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
            
        }

        private void RequestSense()
        {
            SectorCount = 0x9;
            CylinderHigh = 0;
            CylinderLow = 0x12;
            sectorBuffer = new ushort[9];

            sectorBuffer[0] = 0x80;
            sectorBuffer[1] = 0x00;

            Status |= DeviceStatus.DataRequest;
            bufferIndex = 0;
        }

        private void ReadCapacity()
        {
            byte[] capacityData = new byte[8];
            byte[] capac, trackSize;

            trackSize = BitConverter.GetBytes((uint)2048);
            capac = BitConverter.GetBytes((uint)((650 * 1024 * 1024) / 2048));

            Array.Copy(capac, capacityData, 4);
            Array.Copy(trackSize, 0, capacityData, 4, 4);

            Util.ByteArrayToUShort(capacityData, sectorBuffer, 0);

            Status |= DeviceStatus.DataRequest;
            bufferIndex = 0;
            SectorCount = 3;
        }

        private void Read10()
        {
            uint lba;
            ushort length;
            byte[] sectorBytes = new byte[sectorBuffer.Length * 2];

            Util.UShortArrayToByte(sectorBuffer, sectorBytes, 0);

            lba = Util.SwapByteOrder(BitConverter.ToUInt32(sectorBytes, 2));
            length = Util.SwapByteOrder(BitConverter.ToUInt16(sectorBytes, 7));

            sectorBytes = new byte[2048 * length];
            sectorBuffer = new ushort[sectorBytes.Length / 2];

            isoStream.Seek(lba * 2048, SeekOrigin.Begin);
            isoStream.Read(sectorBytes, 0, length * 2048);

            Util.ByteArrayToUShort(sectorBytes, sectorBuffer, 0);
            bufferIndex = 0;
            Status |= DeviceStatus.DataRequest;
            Cylinder = (ushort)sectorBytes.Length;
        }

        public override void FinishRead()
        {
            SectorCount = 3;
        }
    }
}
