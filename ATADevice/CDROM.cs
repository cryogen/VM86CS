using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS.ATADevice
{
    public class CDROM : ATADrive
    {
        public override void LoadImage(string filename)
        {
            
        }

        public override void Reset()
        {
            Error = 1;
            SectorNumber = 1;
            SectorCount = 1;
            CylinderLow = 0x14;
            CylinderHigh = 0xea;
        }

        public override void RunCommand(byte command)
        {
            
        }
    }
}
