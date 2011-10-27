using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using x86CS.Configuration;
using x86CS.ATADevice;

namespace x86CS.Devices
{
    public class ATA : IDevice
    {
        private readonly int[] portsUsed = { 
                                               0x1f0, 0x1f1, 0x1f2, 0x1f3, 0x1f4, 0x1f5, 0x1f6, 0x1f7,
                                               0x170, 0x171, 0x172, 0x173, 0x174, 0x175, 0x176, 0x177,
                                               0x3f6, 0x376
                                           };
        private readonly List<ATADrive> diskDrives = new List<ATADrive>();

        private byte[] deviceControl = new byte[2];
        private bool primarySelected;

        public HardDisk[] HardDrives
        {
            get
            {
                List<HardDisk> hdds = new List<HardDisk>();
                foreach (ATADrive drive in diskDrives)
                {
                    if (drive is HardDisk)
                        hdds.Add(drive as HardDisk);
                }

                return hdds.ToArray();
            }
        }

        public ATA()
        {
            foreach (DiskElement drive in SystemConfig.Machine.Disks)
            {
                ATADrive newDrive;

                if (drive.Type == DriveType.HardDisk)
                    newDrive = new HardDisk();
                else if (drive.Type == DriveType.CDROM)
                    newDrive = new CDROM();
                else
                    break;

                newDrive.LoadImage(drive.Image);

                diskDrives.Add(newDrive);
            }

            primarySelected = true;
        }

        public void Reset(int controller)
        {
            if (controller == 0)
            {
                diskDrives[0].Reset();
                if (diskDrives.Count > 1)
                    diskDrives[1].Reset();
            }
            else
            {
                diskDrives[3].Reset();
                if (diskDrives.Count > 1)
                    diskDrives[4].Reset();
            }
        }

        public void RunCommand(int controller, byte command)
        {
            ATADrive drive;

            if (controller == 0)
            {
                if (primarySelected)
                    drive = diskDrives[0];
                else
                    drive = diskDrives[1];

                drive.RunCommand(command);
            }
        }

        #region IDevice Members

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public uint Read(ushort addr, int size)
        {
            switch (addr)
            {
                case 0x1f0:
                    if (primarySelected && diskDrives.Count > 0)
                        return diskDrives[0].SectorBuffer;
                    else if (diskDrives.Count > 1)
                        return diskDrives[1].SectorBuffer;
                    else
                        return 0;
                case 0x172:
                    if (primarySelected && diskDrives.Count > 2)
                        return diskDrives[2].SectorCount;
                    else if (diskDrives.Count > 2)
                        return diskDrives[3].SectorCount;
                    else
                        return 0;
                case 0x1f2:
                    if (primarySelected && diskDrives.Count > 0)
                        return diskDrives[0].SectorCount;
                    else if (diskDrives.Count > 1)
                        return diskDrives[1].SectorCount;
                    else
                        return 0;
                case 0x173:
                    if (primarySelected && diskDrives.Count > 2)
                        return diskDrives[2].SectorNumber;
                    else if (diskDrives.Count > 3)
                        return diskDrives[3].SectorNumber;
                    else
                        return 0;
                case 0x1f3:
                    if (primarySelected && diskDrives.Count > 0)
                        return diskDrives[0].SectorNumber;
                    else if (diskDrives.Count > 1)
                        return diskDrives[1].SectorNumber;
                    else
                        return 0;
                case 0x1f4:
                    if (primarySelected && diskDrives.Count > 0)
                        return diskDrives[0].CylinderLow;
                    else if (diskDrives.Count > 1)
                        return diskDrives[1].CylinderLow;
                    else
                        return 0;
                case 0x1f5:
                    if (primarySelected && diskDrives.Count > 0)
                        return diskDrives[0].CylinderHigh;
                    else if (diskDrives.Count > 1)
                        return diskDrives[1].CylinderHigh;
                    else
                        return 0;
                case 0x1f7:
                    if (primarySelected && diskDrives.Count > 1)
                        return (byte)diskDrives[0].Status;
                    else if (diskDrives.Count > 1)
                        return (byte)diskDrives[1].Status;
                    else
                        return 0;
                case 0x3f6:
                    return deviceControl[0];
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
            return 0;
        }

        public void Write(ushort addr, uint value, int size)
        {
            switch (addr)
            {
                case 0x1f0:
                    if (primarySelected && diskDrives.Count > 0)
                        diskDrives[0].SectorBuffer = (ushort)value;
                    else if (diskDrives.Count > 1)
                        diskDrives[1].SectorBuffer = (ushort)value;
                    break;
                case 0x1f1:     // Precomp, do nothing
                    break;
                case 0x172:
                    if (primarySelected && diskDrives.Count > 2)
                        diskDrives[2].SectorCount = (byte)value;
                    else if (diskDrives.Count > 3)
                        diskDrives[3].SectorCount = (byte)value;
                    break;
                case 0x1f2:
                    if (primarySelected && diskDrives.Count > 0)
                        diskDrives[0].SectorCount = (byte)value;
                    else if (diskDrives.Count > 1)
                        diskDrives[1].SectorCount = (byte)value;
                    break;
                case 0x173:
                    if (primarySelected && diskDrives.Count > 2)
                        diskDrives[2].SectorNumber = (byte)value;
                    else if (diskDrives.Count > 3)
                        diskDrives[2].SectorNumber = (byte)value;
                    break;
                case 0x1f3:
                    if (primarySelected && diskDrives.Count > 0)
                        diskDrives[0].SectorNumber = (byte)value;
                    else if (diskDrives.Count > 1)
                        diskDrives[1].SectorNumber = (byte)value;
                    break;
                case 0x1f4:
                    if (primarySelected && diskDrives.Count > 0)
                        diskDrives[0].CylinderLow = (byte)value;
                    else if (diskDrives.Count > 1)
                        diskDrives[1].CylinderLow = (byte)value;
                    break;
                case 0x1f5:
                    if (primarySelected && diskDrives.Count > 0)
                        diskDrives[0].CylinderHigh = (byte)value;
                    else if (diskDrives.Count > 1)
                        diskDrives[1].CylinderHigh = (byte)value;
                    break;
                case 0x176:
                    if ((value & 0x10) == 0x10)
                        primarySelected = false;
                    else
                        primarySelected = true;

                    if (primarySelected && diskDrives.Count > 2)
                        diskDrives[2].DriveHead = (byte)value;
                    else if (diskDrives.Count > 3)
                        diskDrives[3].DriveHead = (byte)value;
                    break;
                case 0x1f6:
                    if ((value & 0x10) == 0x10)
                        primarySelected = false;
                    else
                        primarySelected = true;

                    if (primarySelected && diskDrives.Count > 0)
                        diskDrives[0].DriveHead = (byte)value;
                    else if (diskDrives.Count > 1)
                        diskDrives[1].DriveHead = (byte)value;
                    break;
                case 0x1f7:
                    RunCommand(0, (byte)value);
                    break;
                case 0x3f6:
                    if ((value & 0x4) == 0x4)
                    {
                        if ((deviceControl[0] & 0x4) != 0x4)
                            Reset(0);
                    }
                    else if ((deviceControl[0] & 0x4) == 0x4)
                    {
                        if (diskDrives.Count > 0)
                        {
                            diskDrives[0].Status &= ~DeviceStatus.Busy;
                            diskDrives[0].Status |= DeviceStatus.Ready;
                        }
                        if (diskDrives.Count > 1)
                        {
                            diskDrives[1].Status &= ~DeviceStatus.Busy;
                            diskDrives[1].Status |= DeviceStatus.Ready;
                        }
                    }

                    deviceControl[0] = (byte)value;
                    break;
                case 0x376:
                    if ((value & 0x4) == 0x4)
                    {
                        if ((deviceControl[1] & 0x4) != 0x4)
                            Reset(1);
                    }
                    else if ((deviceControl[1] & 0x4) == 0x4)
                    {
                        if (diskDrives.Count > 2)
                        {
                            diskDrives[2].Status &= ~DeviceStatus.Busy;
                            diskDrives[2].Status |= DeviceStatus.Ready;
                        }
                        if (diskDrives.Count > 3)
                        {
                            diskDrives[3].Status &= ~DeviceStatus.Busy;
                            diskDrives[3].Status |= DeviceStatus.Ready;
                        }
                    }

                    deviceControl[0] = (byte)value;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }

        #endregion
    }
}
