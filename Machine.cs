using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using x86CS.Devices;
using System.Windows.Forms;

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
        public event EventHandler<TextEventArgs> WriteText;
        public event EventHandler<CharEventArgs> WriteChar;
        private readonly Dictionary<int, int> breakpoints = new Dictionary<int, int>();
        private Dictionary<ushort, IOEntry> ioPorts;
        private readonly StreamWriter logFile = File.CreateText("machinelog.txt");
        private readonly CMOS cmos;
        private readonly Misc misc;
        private readonly PIT8253 pit;
        private readonly Keyboard keyboard;
        private readonly DMA dma;
        private readonly PIC8259 pic;
        private readonly VGA vga = new VGA();
        private object[] operands;
        private int opLen;
        private byte opCode;
        private readonly MachineForm machineForm = new MachineForm();

        public string Operation { get; private set; }
        public Stack<char> KeyPresses { get; set; }
        public Floppy FloppyDrive { get; private set; }
        public bool Running { get; private set; }
        public CPU.CPU CPU { get; private set; }

        public void OnWriteText(TextEventArgs e)
        {
            var handler = WriteText;
            if (handler != null)
                handler(this, e);
        }

        public void OnWriteChar(CharEventArgs e)
        {
            var handler = WriteChar;
            if (handler != null)
                handler(this, e);
        }

        public Machine()
        {
            CPU = new CPU.CPU();
            KeyPresses = new Stack<char>();
            Operation = "";
            FloppyDrive = new Floppy();
            cmos = new CMOS();
            misc = new Misc();
            pit = new PIT8253();
            keyboard = new Keyboard();
            dma = new DMA();
            pic = new PIC8259();
            vga = new VGA();

            pit.InteruptRequested += PITInteruptRequested;

            SetupSystem();

            CPU.IORead += CPUIORead;
            CPU.IOWrite += CPUIOWrite;

            machineForm.Paint += MachineFormPaint;
            machineForm.Show();
            machineForm.BringToFront();
        }

        void PITInteruptRequested(object sender, EventArgs e)
        {
            int vector = pic.FindInterruptVector(0);

            if (vector == -1)
                return;

            CPU.Interrupt(vector, 0);
        }

        private void MachineFormPaint(object sender, PaintEventArgs e)
        {
           vga.GDIDraw(e.Graphics);
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

            lock (logFile)
            {
                logFile.WriteLine(String.Format("IO Read {0:X4} returned {1:X4}", addr, ret));
            }

            return ret;
        }

        private void CPUIOWrite(ushort addr, ushort value)
        {
            IOEntry entry;

            if (ioPorts.TryGetValue(addr, out entry))
                entry.Write(addr, value);

            lock (logFile)
            {
                logFile.WriteLine(String.Format("IO Write {0:X4} value {1:X4}", addr, value));
            }
        }

        private void LoadBIOS()
        {
            FileStream biosStream = File.OpenRead("BIOS-bochs-legacy");
            var buffer = new byte[biosStream.Length];

            uint startAddr = (uint)(0xfffff - buffer.Length) + 1;

            biosStream.Read(buffer, 0, buffer.Length);
            Memory.BlockWrite(startAddr, buffer, buffer.Length);
            
            biosStream.Close();
            biosStream.Dispose();
        }

        private void LoadVGABios()
        {
            FileStream biosStream = File.OpenRead("VGABIOS-lgpl-latest");
            var buffer = new byte[biosStream.Length];

            biosStream.Read(buffer, 0, buffer.Length);
            Memory.BlockWrite(0xc0000, buffer, buffer.Length);

            biosStream.Close();
            biosStream.Dispose();
        }

        private void SetupSystem()
        {
            ioPorts = new Dictionary<ushort, IOEntry>();

            SetupIOEntry(0x20, pic.Read, pic.Write);
            SetupIOEntry(0x21, pic.Read, pic.Write);
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
            SetupIOEntry(0xa0, pic.Read, pic.Write);
            SetupIOEntry(0xa1, pic.Read, pic.Write);
            SetupIOEntry(0x3b4, vga.Read, vga.Write);
            SetupIOEntry(0x3b5, vga.Read, vga.Write);
            SetupIOEntry(0x3ba, vga.Read, vga.Write);
            SetupIOEntry(0x3c0, vga.Read, vga.Write);
            SetupIOEntry(0x3c1, vga.Read, vga.Write);
            SetupIOEntry(0x3c2, vga.Read, vga.Write);
            SetupIOEntry(0x3c4, vga.Read, vga.Write);
            SetupIOEntry(0x3c5, vga.Read, vga.Write);
            SetupIOEntry(0x3c7, vga.Read, vga.Write);
            SetupIOEntry(0x3c8, vga.Read, vga.Write);
            SetupIOEntry(0x3c9, vga.Read, vga.Write);
            SetupIOEntry(0x3ca, vga.Read, vga.Write);
            SetupIOEntry(0x3cc, vga.Read, vga.Write);
            SetupIOEntry(0x3d4, vga.Read, vga.Write);
            SetupIOEntry(0x3d5, vga.Read, vga.Write);
            SetupIOEntry(0x3da, vga.Read, vga.Write);
            SetupIOEntry(0x402, misc.Read, misc.Write);
            SetupIOEntry(0x500, misc.Read, misc.Write);

            LoadBIOS();
            LoadVGABios();

            CPU.CS = 0xf000;
            CPU.IP = 0xfff0;
        }

        public void Restart()
        {
            Running = false;
            CPU.Reset();
            SetupSystem();
            Running = true;
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
            var cpuAddr = (uint)(CPU.CurrentAddr - opLen);

            return breakpoints.Any(kvp => kvp.Value == cpuAddr);
        }

        public void Start()
        {
            string tempOpStr;
            opLen = CPU.Decode(CPU.EIP, out opCode, out tempOpStr, out operands);
            Operation = String.Format("{0:X4}:{1:X} {2}", CPU.CS, CPU.EIP, tempOpStr);
            Running = true;
        }

        public void Stop()
        {
            Running = false;
            logFile.Flush();
        }

        public void FlushLog()
        {
            logFile.Flush();
        }

        public void RunCycle()
        {
            if (Running)
            {
                string tempOpStr;
                lock (logFile)
                {
                    logFile.WriteLine("{0} {1}", DateTime.Now, Operation);
                }
                CPU.Cycle(opLen, opCode, operands);
                opLen = CPU.Decode(CPU.EIP, out opCode, out tempOpStr, out operands);
                Operation = String.Format("{0:X4}:{1:X} {2}", CPU.CS, CPU.EIP, tempOpStr);
                machineForm.Invalidate();
            }
        }
    }
}
