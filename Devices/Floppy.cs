using System;
using System.IO;
using log4net;

namespace x86CS.Devices
{
    public class Floppy : IDevice, INeedsIRQ, INeedsDMA
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Floppy));

        private const int IrqNumber = 6;
        private const int DmaChannel = 2;
        private readonly int[] portsUsed = {0x3f0, 0x3f1, 0x3f2, 0x3f4, 0x3f5, 0x3f7};

        private readonly byte[] data;

        private FileStream floppyStream;
        private BinaryReader floppyReader;
        private DORSetting digitalOutput;
        private MainStatus mainStatus;
        private bool inCommand;
        private byte paramCount;
        private byte resultCount;
        private byte paramIdx;
        private byte resultIdx;
        private FloppyCommand command;
        private byte statusZero;
        private byte headPosition;
        private byte currentCyl;
        private bool interruptInProgress;

        public event EventHandler IRQ;
        public event EventHandler<ByteArrayEventArgs> DMA;

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


        public Floppy()
        {
            mainStatus = MainStatus.RQM;
            data = new byte[16];
        }

        public void OnDMA(ByteArrayEventArgs e)
        {
            EventHandler<ByteArrayEventArgs> handler = DMA;
            if (handler != null) 
                handler(this, e);
        }

        public void OnIRQ(EventArgs e)
        {
            EventHandler handler = IRQ;
            if (handler != null)
                handler(this, e);
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

            return true;
        }

        private void Reset()
        {
            Logger.Info("Reset issued");
            digitalOutput &= ~DORSetting.Reset;
            OnIRQ(new EventArgs());
        }

        private void ReadSector()
        {
            int addr = (data[1] * 2 + data[2]) * 18 + (data[3] - 1);
            int numSectors = data[5] - data[3] + 1;

            if (numSectors == -1)
                numSectors = 1;

            floppyStream.Seek(addr * 512, SeekOrigin.Begin);
            byte[] sector = floppyReader.ReadBytes(512 * numSectors);

            Logger.Info(String.Format("Reading {0} sectors from sector offset {1}", numSectors, addr));

            resultCount = 7;
            resultIdx = 0;
            data[0] = 0;
            data[1] = 0;
            data[2] = 0;
            data[3] = 0;
            data[4] = 0;
            data[5] = 0;
            data[6] = 0;

            OnDMA(new ByteArrayEventArgs(sector));

            mainStatus |= MainStatus.DIO;
            statusZero = 0;

            OnIRQ(new EventArgs());
        }

        private void RunCommand()
        {
            switch (command)
            {
                case FloppyCommand.Recalibrate:
                    Logger.Info("Recalibrate issued");
                    floppyStream.Seek(0, SeekOrigin.Begin);
                    headPosition = 0;
                    currentCyl = 0;
                    statusZero = 0x20;
                    interruptInProgress = true;
                    OnIRQ(new EventArgs());
                    break;
                case FloppyCommand.SenseInterrupt:
                    Logger.Info("Sense interrupt isssued");
                    if (!interruptInProgress)
                        statusZero = 0x80;
                    interruptInProgress = false;
                    mainStatus |= MainStatus.DIO;
                    resultIdx = 0;
                    resultCount = 2;
                    data[0] = statusZero;
                    data[1] = currentCyl;
                    break;
                case FloppyCommand.ReadData:
                    ReadSector();
                    break;
                case FloppyCommand.WriteData:
                    resultCount = 7;
                    resultIdx = 0;
                    data[0] = 0;
                    data[1] = 0x2;
                    data[2] = 0;
                    data[3] = 0;
                    data[4] = 0;
                    data[5] = 0;
                    data[6] = 0;

                    mainStatus |= MainStatus.DIO;
                    statusZero = 0;

                    OnIRQ(new EventArgs());
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }

        private void ProcessCommandAndArgs(ushort value)
        {
            if (inCommand)
            {
                data[paramIdx++] = (byte) value;
                if (paramIdx == paramCount)
                {
                    RunCommand();
                    inCommand = false;
                }
            }
            else
            {
                inCommand = true;
                paramIdx = 0;
                command = (FloppyCommand)(value & 0x0f);
                switch (command)
                {
                    case FloppyCommand.Recalibrate:
                        paramCount = 1;
                        break;
                    case FloppyCommand.SenseInterrupt:
                        paramCount = 0;
                        RunCommand();
                        inCommand = false;
                        break;
                    case FloppyCommand.ReadData:
                        paramCount = 8;
                        break;
                    case FloppyCommand.WriteData:
                        paramCount = 8;
                        break;
                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }
            }
        }

        #region IDevice Members

        public uint Read(ushort addr, int size)
        {
            switch (addr)
            {
                case 0x3f2:
                    return (ushort)digitalOutput;
                case 0x3f4:
                    return (ushort)mainStatus;
                case 0x3f5:
                    byte ret = data[resultIdx++];
                    if (resultIdx == resultCount)
                        mainStatus &= ~MainStatus.DIO;
                    return ret;
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
                case 0x3f2:
                    if(((digitalOutput & DORSetting.Reset) == 0) && (((DORSetting)value & DORSetting.Reset) == DORSetting.Reset))
                        Reset();

                    digitalOutput = (DORSetting) value;
                    break;
                case 0x3f5:
                    ProcessCommandAndArgs((ushort)value);
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }

        #endregion
    }

    [Flags]
    enum MainStatus
    {
        Drive0Busy = 0x1,
        Drive1Busy = 0x2,
        Drive2Busy = 0x4,
        Drive3Busy = 0x8,
        CommandBusy = 0x10,
        NonDMA = 0x20,
        DIO = 0x40,
        RQM = 0x80
    }

    [Flags]
    enum DORSetting
    {
        Drive = 0x1,
        Reset = 0x4,
        Dma = 0x8,
        Drive0Motor = 0x10,
        Drive1Motor = 0x20,
        Drive2Motor = 0x40,
        Drive3Motor = 0x80
    }

    enum FloppyCommand
    {
        ReadTrack = 2,
        SPECIFY = 3,
        SenseDriveStatus = 4,
        WriteData = 5,
        ReadData = 6, 
        Recalibrate = 7,   
        SenseInterrupt = 8, 
        WriteDeletedData = 9,
        ReadID = 10,	
        ReadDeletedData = 12,
        FormatTrack = 13,
        Seek = 15,     
        Version = 16,
        ScanEqual = 17,
        PerpendicularMode = 18,	
        Configure = 19,    
        Lock = 20,     
        Verify = 22,
        ScanLowOrEqual = 25,
        ScanHighOrEqual = 29
    };
}