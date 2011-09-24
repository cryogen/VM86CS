using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using x86CS.Devices;

namespace x86CS
{
    public class TextEventArgs : EventArgs
    {
        private string text;

        public TextEventArgs(string textToWrite)
        {
            text = textToWrite;
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
    }

    public class CharEventArgs : EventArgs
    {
        private char ch;

        public CharEventArgs(char charToWrite)
        {
            ch = charToWrite;
        }

        public char Char
        {
            get { return ch; }
            set { ch = value; }
        }
    }

    public class IntEventArgs : EventArgs
    {
        private int number;

        public IntEventArgs(int num)
        {
            number = num;
        }

        public int Number
        {
            get { return number; }
            set { number = value; }
        }
    }

    public delegate void InteruptHandler();

    public struct IOEntry
    {
        public ReadCallback Read;
        public WriteCallback Write;
    }

    public struct SystemConfig
    {
        public ushort NumBytes;
        public byte Model;
        public byte SubModel;
        public byte Revision;
        public byte Feature1;
        public byte Feature2;
        public byte Feature3;
        public byte Feature4;
        public byte Feature5;
    }

    public class Machine
    {
        private CPU cpu = new CPU();
        private Floppy floppyDrive;
        private bool running = false;
        public event EventHandler<TextEventArgs> WriteText;
        public event EventHandler<CharEventArgs> WriteChar;
        private Stack<char> keyPresses = new Stack<char>();
        private Dictionary<int, int> breakpoints = new Dictionary<int, int>();
        private Dictionary<ushort, IOEntry> ioPorts;
        private StreamWriter logFile = File.CreateText("machinelog.txt");
        private CMOS cmos;
        private Misc misc;
        private object[] operands;
        private int opLen;
        private byte opCode;
        private string opStr = "";

        public string Operation
        {
            get { return opStr; }
        }

        public Stack<char> KeyPresses
        {
            get { return keyPresses; }
            set { keyPresses = value; }
        }

        public Floppy FloppyDrive
        {
            get { return floppyDrive; }
        }

        public bool Running
        {
            get { return running; }
        }

        public CPU CPU
        {
            get { return cpu; }
        }

        public Machine()
        {
            floppyDrive = new Floppy();
            cmos = new CMOS();
            misc = new Misc();

            logFile.AutoFlush = true;

            SetupSystem();

            cpu.IORead += new ReadCallback(cpu_IORead);
            cpu.IOWrite += new WriteCallback(cpu_IOWrite);
        }

        private void SetupIOEntry(ushort port, ReadCallback read, WriteCallback write)
        {
            IOEntry entry = new IOEntry();

            entry.Read = read;
            entry.Write = write;

            ioPorts.Add(port, entry);
        }

        private ushort cpu_IORead(ushort addr)
        {
            IOEntry entry;
            ushort ret;

            if (!ioPorts.TryGetValue(addr, out entry))
                ret = 0xffff;
            else
                ret = entry.Read(addr);

            logFile.WriteLine(String.Format("IO Read {0:X4} returned {1:X4}", addr, ret));

            return ret;
        }

        private void cpu_IOWrite(ushort addr, ushort value)
        {
            IOEntry entry;

            if (ioPorts.TryGetValue(addr, out entry))
                entry.Write(addr, value);

            logFile.WriteLine(String.Format("IO Write {0:X4} value {1:X4}", addr, value));
        }

        private void LoadBIOS()
        {
            FileStream biosStream = File.OpenRead("BIOS-bochs-latest");
            byte[] buffer = new byte[biosStream.Length];

            uint startAddr = (uint)(0xfffff - buffer.Length) + 1;

            biosStream.Read(buffer, 0, buffer.Length);
            Memory.BlockWrite(startAddr, buffer, buffer.Length);
        }

        private void SetupSystem()
        {
            ioPorts = new Dictionary<ushort, IOEntry>();

            SetupIOEntry(0x70, new ReadCallback(cmos.Read), new WriteCallback(cmos.Write));
            SetupIOEntry(0x71, new ReadCallback(cmos.Read), new WriteCallback(cmos.Write));
            SetupIOEntry(0x92, new ReadCallback(misc.Read), new WriteCallback(misc.Write));

            LoadBIOS();

            cpu.CS = 0xf000;
            cpu.IP = 0xfff0;
        }

        public void Restart()
        {
            running = false;
            cpu.Reset();
            SetupSystem();
            running = true;
        }

        public void SetBreakpoint(int addr)
        {
            if (breakpoints.ContainsKey(addr))
                return;

            breakpoints.Add(addr, addr);
        }

        public void ClearBreakpoint(int addr)
        {
            if (!breakpoints.ContainsKey(addr))
                return;

            breakpoints.Remove(addr);
        }

        public bool CheckBreakpoint()
        {
            uint cpuAddr = (cpu.CS << 4) + cpu.EIP;

            foreach (KeyValuePair<int, int> kvp in breakpoints)
            {
                if (kvp.Value == cpuAddr)
                    return true;
            }
            return false;
        }

        private char GetChar()
        {
            if (keyPresses.Count > 0)
                return keyPresses.Pop();

            while (keyPresses.Count == 0)
                ;

            return keyPresses.Pop();
        }

        private bool ReadSector()
        {
            int count = cpu.AL;
            byte sector, cyl, head;
            DisketteParamTable dpt;
            byte[] buffer = new byte[Marshal.SizeOf(typeof(DisketteParamTable))];          
            IntPtr p;

            sector = (byte)(cpu.CL & 0x3f);
            cyl = cpu.CH;
            head = cpu.DH;

            Console.WriteLine("Sector {0:X2}, Cyl {1:X2}, Head {2:X2}, Count {3:X2}", sector, cyl, head, count);

            Memory.BlockRead((uint)((Memory.ReadWord(0x7a) << 16) + Memory.ReadWord(0x78)), buffer, buffer.Length);
            p = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, p, buffer.Length);
            dpt = (DisketteParamTable)Marshal.PtrToStructure(p, typeof(DisketteParamTable));

            int addr = (cyl * 2 + head) * dpt.LastTrack + (sector - 1);

            byte[] fileBuffer = floppyDrive.ReadSector(addr);

            Memory.SegBlockWrite(cpu.ES, cpu.BX, fileBuffer, fileBuffer.Length);

            return true;
        }

        private byte ToBCD(int value)
        {
            byte ret;
            int tens, ones;

            tens = value / 10;
            ones = value % 10;

            ret = (byte)(((byte)tens << 4) + (byte)ones);

            return ret;
        }

        private void DoWriteText(string text)
        {
            EventHandler<TextEventArgs> textEvent = WriteText;

            if (textEvent != null)
                textEvent(this, new TextEventArgs(text));
        }

        private void DoWriteChar(char ch)
        {
            EventHandler<CharEventArgs> charEvent = WriteChar;

            if (charEvent != null)
                charEvent(this, new CharEventArgs(ch));
        }

        public void Start()
        {
            opLen = cpu.Decode(cpu.EIP, out opCode, out opStr, out operands);
            opStr = String.Format("{0:X4}:{1:X} {2}", cpu.CS, cpu.EIP, opStr);
            running = true;
        }

        public void Stop()
        {
            running = false;
        }

        public void RunCycle()
        {
            if (running)
            {
                logFile.WriteLine("{0}", opStr); 
                cpu.Cycle(opLen, opCode, operands);
                opLen = cpu.Decode(cpu.EIP, out opCode, out opStr, out operands);
                opStr = String.Format("{0:X4}:{1:X} {2}", cpu.CS, cpu.EIP, opStr);
            }
        }
    }
}
