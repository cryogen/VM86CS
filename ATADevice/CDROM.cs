using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace x86CS.ATADevice
{
    public class CDROM : ATADrive
    {
        private ushort[] identifyBuffer;

        public CDROM()
        {
            identifyBuffer = new ushort[256];

            identifyBuffer[0] = 0x8580;
            Util.ByteArrayToUShort(Encoding.ASCII.GetBytes("123456789012345678"), ref identifyBuffer, 23);
            Util.ByteArrayToUShort(Encoding.ASCII.GetBytes("x86 CS CDROM                            "), ref identifyBuffer, 27);
            identifyBuffer[49] = 0x30;
            identifyBuffer[62] = 0x0001; // Mode 0 active and supported
        }

        public override void LoadImage(string filename)
        {
            
        }

        public override void Reset()
        {
            Error = 0;
            SectorNumber = 1;
            SectorCount = 1;
            CylinderLow = 0x14;
            CylinderHigh = 0xeb;
            Status |= DeviceStatus.Busy;
        }

        public override void RunCommand(byte command)
        {
            switch (command)
            {
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
            throw new NotImplementedException();
        }
    }
}
