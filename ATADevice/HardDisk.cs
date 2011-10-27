using System.IO;
using System.Runtime.InteropServices;
using System;

namespace x86CS.ATADevice
{
    public class HardDisk : ATADrive
    {
        private FileStream stream;
        private BinaryReader reader;
        private Footer footer;
        private DiskHeader header;
        private ushort[] identifyBuffer;
        private byte lastCommand;

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
            byte[] buffer;

            stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            reader = new BinaryReader(stream);
            stream.Seek(-512, SeekOrigin.End);
            buffer = reader.ReadBytes(512);

            footer = Util.ByteArrayToStructureBigEndian<Footer>(buffer);

            stream.Seek(512, SeekOrigin.Begin);
            buffer = reader.ReadBytes(1024);

            header = Util.ByteArrayToStructureBigEndian<DiskHeader>(buffer);

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

        private byte[] ReadSector(long sector)
        {
            long blockNumber = sector / (header.BlockSize / 512);
            uint blockOffset;
            long sectorInBlock;
            byte[] bitmap;

            stream.Seek((long)((long)header.TableOffset + (blockNumber * 4)), SeekOrigin.Begin);
            blockOffset = Util.SwapByteOrder(reader.ReadUInt32());

            if (blockOffset == 0xffffffff)
                return new byte[512];

            stream.Seek(blockOffset * 512, SeekOrigin.Begin);

            bitmap = reader.ReadBytes((int)(header.BlockSize / 512 / 8));
            byte bitmapByte = bitmap[sector / 8];
            byte offset = (byte)(sector % 8);

            if ((bitmapByte & (1 << (7 - offset))) == 0)
                return new byte[512];

            sectorInBlock = sector % (header.BlockSize / 512);
            stream.Seek(sectorInBlock * 512, SeekOrigin.Current);

            return reader.ReadBytes(512);
        }

        private void Read()
        {
            int addr = (Cylinder * footer.Heads + (DriveHead & 0x0f)) * footer.SectorsPerCylinder + (SectorNumber - 1);
            sectorBuffer = new ushort[(SectorCount * 512) / 2];

            for (int i = 0; i < SectorCount; i++)
            {
                Util.ByteArrayToUShort(ReadSector(addr + i), ref sectorBuffer, i * 256);
            }
        }

        private void WriteSector(long sector, byte[] data)
        {
            long blockNumber = sector / (header.BlockSize / 512);
            uint blockOffset;
            long sectorInBlock;
            byte[] bitmap;
            BinaryWriter writer = new BinaryWriter(stream);

            stream.Seek((long)((long)header.TableOffset + (blockNumber * 4)), SeekOrigin.Begin);
            blockOffset = Util.SwapByteOrder(reader.ReadUInt32());

            if (blockOffset == 0xffffffff)
            {
                // Create new block
                byte[] oldFooter;
                byte[] newBlock = new byte[header.BlockSize];
                byte[] newBitmap = new byte[header.BlockSize / 512 / 8];
                long offsetPosition;

                stream.Seek(-512, SeekOrigin.End);
                offsetPosition = stream.Position;
                oldFooter = reader.ReadBytes(512);
                stream.Seek(-512, SeekOrigin.End);
                writer.Write(newBitmap);
                writer.Write(newBlock);
                writer.Write(oldFooter);

                stream.Seek((long)((long)header.TableOffset + (blockNumber * 4)), SeekOrigin.Begin);
                blockOffset = (uint)(offsetPosition / 512);
                writer.Write(Util.SwapByteOrder(blockOffset));
            }

            stream.Seek(blockOffset * 512, SeekOrigin.Begin);

            bitmap = reader.ReadBytes((int)(header.BlockSize / 512 / 8));
            bitmap[sector / 8] |= (byte)(1 << (byte)(7 - (sector % 8)));
            stream.Seek(blockOffset * 512, SeekOrigin.Begin);
            writer.Write(bitmap);

            sectorInBlock = sector % (header.BlockSize / 512);
            stream.Seek(sectorInBlock * 512, SeekOrigin.Current);
            writer.Write(data);
        }

        private void Write()
        {
            int addr = (Cylinder * footer.Heads + (DriveHead & 0x0f)) * footer.SectorsPerCylinder + (SectorNumber - 1);

            for (int i = 0; i < SectorCount; i++)
            {
                byte[] sector = new byte[512];

                Util.UShortArrayToByte(sectorBuffer, ref sector, i * 256);
                WriteSector(addr + i, sector);
            }
        }

        public override void FinishCommand()
        {
            switch (lastCommand)
            {
                case 0x30:
                    Write();
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
           
        }

        public override void RunCommand(byte command)
        {
            Status |= DeviceStatus.Busy;
            switch (command)
            {
                case 0x20: // Read sector   
                    Read();
                    break;
                case 0x30: // Write Sector
                    sectorBuffer = new ushort[(SectorCount * 512) / 2];
                    break;
                case 0xec: // Identify
                    sectorBuffer = identifyBuffer;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
            Status |= DeviceStatus.DataRequest;
            Status &= ~DeviceStatus.Busy;
            bufferIndex = 0;
            lastCommand = command;
        }

        public override void FinishRead()
        {
            
        }
    }
}
