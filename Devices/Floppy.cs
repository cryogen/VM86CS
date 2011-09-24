using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace x86CS
{
    public class Floppy
    {
        private FileStream floppyStream;
        private BinaryReader floppyReader;
        bool mounted = false;
        private DisketteParamTable dpt;
        private StreamWriter logFile = File.CreateText("floppy.txt");

        public bool Mounted
        {
            get { return mounted; }
        }

        public DisketteParamTable DPT
        {
            get { return dpt; }
        }

        // Most of these default values have been nicked from SeaBIOS, but apparently are the settings for a 1.44M Floppy
        public Floppy()
        {
            dpt = new DisketteParamTable();

            dpt.HeadUnload = 0x0f;          /* 240ms */
            dpt.StepRate = 0x0a;            /* 12ms */
            dpt.HeadLoad = 0x01;            /* 4ms */
            dpt.DMA = 0x01;
            dpt.MotorOff = 37;              /* ~2s apparently */
            dpt.SectorSize = 0x02;          /* 512 byte sectors */
            dpt.LastTrack = 18;
            dpt.GapLength = 0x1b;
            dpt.DataXferLength = 0xff;      /* Not used */
            dpt.FormatGapLength = 0x6c;     /* 3.5" */
            dpt.FillChar = 0xf6;            /* default fill byte/char */
            dpt.HeadSettle = 0x0f;          /* 15ms */
            dpt.MotorOn = 0x08;             /* 1s */

            logFile.AutoFlush = true;
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

            logFile.WriteLine("Mounted image file {0}", imagePath);
            mounted = true;
            return true;
        }

        public byte ReadByte()
        {
            byte ret = floppyReader.ReadByte();

            logFile.WriteLine(String.Format("Floppy Read Byte {0:X4} {1:X2}", floppyReader.BaseStream.Position, ret));

            return ret;
        }

        public ushort ReadWord()
        {
            ushort ret = floppyReader.ReadUInt16();

            logFile.WriteLine(String.Format("Floppy Read Word {0:X4} {1:X4}", floppyReader.BaseStream.Position, ret));

            return ret;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] ret = floppyReader.ReadBytes(count);;

            logFile.WriteLine(String.Format("Floppy Read Bytes {0:X4} {1}", floppyReader.BaseStream.Position, count));

            return ret;
        }

        public void Reset()
        {
            logFile.WriteLine("Floppy RESET");
            floppyStream.Seek(0, SeekOrigin.Begin);
        }

        public byte[] ReadSector(int offset)
        {
            floppyStream.Seek(offset * 512, SeekOrigin.Begin);
            logFile.WriteLine("Floppy Read Sector {0:X4}", offset);
            return floppyReader.ReadBytes(512);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DisketteParamTable
    {
        private byte stepRateHeadUnload;
        private byte dmaHeadLoad;

        public byte StepRate
        {
            get { return stepRateHeadUnload.GetHigh(); }
            set { stepRateHeadUnload = stepRateHeadUnload.SetHigh(value); }
        }

        public byte HeadUnload
        {
            get { return stepRateHeadUnload.GetLow(); }
            set { stepRateHeadUnload = stepRateHeadUnload.SetLow(value); }
        }

        public byte HeadLoad
        {
            get { return (byte)((dmaHeadLoad >> 2) & 0x3f); }
            set { dmaHeadLoad = (byte)(((value & 0x3f) << 2) + (dmaHeadLoad & 0x01)); }
        }

        public byte DMA
        {
            get { return (byte)(dmaHeadLoad & 0x01); }
            set { dmaHeadLoad = (byte)((dmaHeadLoad & 0xfc) + (value & 0x01)); }
        }

        public byte MotorOff;
        public byte SectorSize;
        public byte LastTrack;
        public byte GapLength;
        public byte DataXferLength;
        public byte FormatGapLength;
        public byte FillChar;
        public byte HeadSettle;
        public byte MotorOn;
    }
}
