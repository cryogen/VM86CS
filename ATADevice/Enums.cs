using System;
namespace x86CS.ATADevice
{
    [Flags]
    public enum DeviceStatus
    {
        Error = 0x1,
        Index = 0x2,
        CorrectedData = 0x4,
        DataRequest = 0x8,
        SeekComplete = 0x10,
        WriteFault = 0x20,
        Ready = 0x40,
        Busy = 0x80
    }
}
