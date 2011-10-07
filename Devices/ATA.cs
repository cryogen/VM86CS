using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace x86CS.Devices
{
    public class ATA : IDevice
    {
        private readonly int[] portsUsed = { 
                                               0x1f0, 0x1f1, 0x1f2, 0x1f3, 0x1f4, 0x1f5, 0x1f6, 0x1f7,
                                               0x170, 0x171, 0x172, 0x173, 0x174, 0x175, 0x176, 0x177
                                           };
        #region IDevice Members

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public ushort Read(ushort addr)
        {
            return 0;
        }

        public void Write(ushort addr, ushort value)
        {

        }

        #endregion
    }
}
