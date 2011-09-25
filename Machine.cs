using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using x86CS.Devices;

namespace x86CS
{
    public class TextEventArgs : EventArgs
    {
        public TextEventArgs(string textToWrite)
        {
            Text = textToWrite;
        }

        public string Text { get; set; }
    }

    public class CharEventArgs : EventArgs
    {
        public CharEventArgs(char charToWrite)
        {
            Char = charToWrite;
        }

        public char Char { get; set; }
    }

    public class IntEventArgs : EventArgs
    {
        public IntEventArgs(int num)
        {
            Number = num;
        }

        public int Number { get; set; }
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
        private readonly CPU.CPU cpu = new CPU.CPU();
        private readonly Floppy floppyDrive;
        private bool running;
        public event EventHandler<TextEventArgs> WriteText;

        public void OnWriteText(TextEventArgs e)
        {
            EventHandler<TextEventArgs> handler = WriteText;
            if (handler != null) 
                handler(this, e);
        }

        public event EventHandler<CharEventArgs> WriteChar;

        public void OnWriteChar(CharEventArgs e)
        {
            EventHandler<CharEventArgs> handler = WriteChar;
            if (handler != null) 
                handler(this, e);
        }

        private Stack<char> keyPresses = new Stack<char>();
        private readonly Dictionary<int, int> breakpoints = new Dictionary<int, int>();
        private Dictionary<ushort, IOEntry> ioPorts;
        private readonly StreamWriter logFile = File.CreateText("machinelog.txt");
        private readonly CMOS cmos;
        private readonly Misc misc;
        private readonly PIT8253 pit;
        private readonly Keyboard keyboard;
        private readonly DMA dma;
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

        public CPU.CPU CPU
        {
            get { return cpu; }
        }

        public Machine()
        {
            floppyDrive = new Floppy();
            cmos = new CMOS();
            misc = new Misc();
            pit = new PIT8253();
            keyboard = new Keyboard();
            dma = new DMA();

            logFile.AutoFlush = true;

            SetupSystem();

            cpu.IORead += CPUIORead;
            cpu.IOWrite += CPUIOWrite;
        }

        private void SetupIOEntry(ushort port, ReadCallback read, WriteCallback write)
        {
            var entry = new IOEntry {Read = read, Write = write};

            ioPorts.Add(port, entry);
        }

        private ushort CPUIORead(ushort addr)
        {
            IOEntry entry;

            var ret = (ushort) (!ioPorts.TryGetValue(addr, out entry) ? 0xffff : entry.Read(addr));

            logFile.WriteLine(String.Format("IO Read {0:X4} returned {1:X4}", addr, ret));

            return ret;
        }

        private void CPUIOWrite(ushort addr, ushort value)
        {
            IOEntry entry;

            if (ioPorts.TryGetValue(addr, out entry))
                entry.Write(addr, value);

            logFile.WriteLine(String.Format("IO Write {0:X4} value {1:X4}", addr, value));
        }

        private void LoadBIOS()
        {
            FileStream biosStream = File.OpenRead("BIOS-bochs-latest");
            var buffer = new byte[biosStream.Length];

            uint startAddr = (uint)(0xfffff - buffer.Length) + 1;

            biosStream.Read(buffer, 0, buffer.Length);
            Memory.BlockWrite(startAddr, buffer, buffer.Length);
        }

        private void SetupSystem()
        {
            ioPorts = new Dictionary<ushort, IOEntry>();

            SetupIOEntry(0x40, pit.Read, pit.Write);
            SetupIOEntry(0x41, pit.Read, pit.Write);
            SetupIOEntry(0x42, pit.Read, pit.Write);
            SetupIOEntry(0x43, pit.Read, pit.Write);
            SetupIOEntry(0x60, keyboard.Read, keyboard.Write);
            SetupIOEntry(0x64, keyboard.Read, keyboard.Write);
            SetupIOEntry(0x70, cmos.Read, cmos.Write);
            SetupIOEntry(0x71, cmos.Read, cmos.Write);
            SetupIOEntry(0x80, dma.Read, dma.Write);
            SetupIOEntry(0x92, misc.Read, misc.Write);
            SetupIOEntry(0x402, misc.Read, misc.Write);

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

            return breakpoints.Any(kvp => kvp.Value == cpuAddr);
        }

        private byte ToBCD(int value)
        {
            int tens = value / 10;
            int ones = value % 10;

            var ret = (byte)(((byte)tens << 4) + (byte)ones);

            return ret;
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
