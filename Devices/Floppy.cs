using System;
using System.IO;

namespace x86CS.Devices
{
    public class Floppy : IDevice
    {
        private const int IrqNumber = 6;
        private const int DmaChannel = 2;
        private readonly int[] portsUsed = {0x3f0, 0x3f1, 0x3f2, 0x3f4, 0x3f5, 0x3f7};

        private readonly byte[] data;

        private FileStream floppyStream;
        private BinaryReader floppyReader;
        private bool mounted;
        private byte digitalOutput;
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
        public event EventHandler<Util.ByteArrayEventArgs> DMA;


        public Floppy()
        {
            mounted = false;
            mainStatus = MainStatus.RQM;
            data = new byte[16];
        }

        public void OnDMA(Util.ByteArrayEventArgs e)
        {
            EventHandler<Util.ByteArrayEventArgs> handler = DMA;
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

            mounted = true;
            return true;
        }

        private void ReadSector()
        {
            int addr = (data[1] * 2 + data[2]) * 512 + (data[3] - 1);
            byte[] sector;

            floppyStream.Seek(addr, SeekOrigin.Begin);
            sector = floppyReader.ReadBytes(512);

            resultCount = 7;
            resultIdx = 0;
            data[0] = 0;
            data[1] = 0;
            data[2] = 0;
            data[3] = 0;
            data[4] = 0;
            data[5] = 0;
            data[6] = 0;

            OnDMA(new Util.ByteArrayEventArgs(sector));

            mainStatus |= MainStatus.DIO;
            statusZero = 0;

            OnIRQ(new EventArgs());
        }

        private void RunCommand()
        {
            switch (command)
            {
                case FloppyCommand.Recalibrate:
                    floppyStream.Seek(0, SeekOrigin.Begin);
                    headPosition = 0;
                    currentCyl = 0;
                    statusZero = 0x20;
                    interruptInProgress = true;
                    OnIRQ(new EventArgs());
                    break;
                case FloppyCommand.SenseInterrupt:
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
                default:

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
                        break;
                    default:
                        break;
                }
            }
        }

        #region IDevice Members

        public void Cycle(double frequency, ulong tickCount)
        {

        }

        public ushort Read(ushort addr)
        {
            byte ret;

            switch (addr)
            {
                case 0x3f2:
                    return digitalOutput;
                case 0x3f4:
                    return (ushort)mainStatus;
                case 0x3f5:
                    ret = data[resultIdx++];
                    if (resultIdx == resultCount)
                        mainStatus &= ~MainStatus.DIO;
                    return ret;
                default:
                    break;
            }
            return 0;
        }

        public void Write(ushort addr, ushort value)
        {
            switch (addr)
            {
                case 0x3f2:
                    digitalOutput = (byte) value;
                    break;
                case 0x3f5:
                    ProcessCommandAndArgs(value);
                    break;
                default:
                    break;
            }
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