using System;
using System.Collections.Generic;
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
        private readonly Dictionary<int, int> breakpoints = new Dictionary<int, int>();
        private readonly TextWriter logFile = TextWriter.Synchronized(File.CreateText("machinelog.txt"));
        private readonly MachineForm machineForm = new MachineForm();
        private readonly IDevice[] devices;
        private readonly PIC8259 picDevice;
        private readonly VGA vgaDevice;
        private readonly DMAController dmaController;

        private Dictionary<ushort, IOEntry> ioPorts;
        private object[] operands;
        private int opLen;
        private byte opCode;

        public string Operation { get; private set; }
        public Floppy FloppyDrive { get; private set; }
        public bool Running { get; private set; }
        public CPU.CPU CPU { get; private set; }

        public Machine()
        {
            picDevice = new PIC8259();
            vgaDevice = new VGA();
            FloppyDrive = new Floppy();
            dmaController = new DMAController();

            devices = new IDevice[]
                          {
                              FloppyDrive, new CMOS(), new Misc(), new PIT8253(), picDevice, new Keyboard(), dmaController,
                              vgaDevice
                          };

            CPU = new CPU.CPU();
            Operation = "";

            SetupSystem();

            CPU.IORead += CPUIORead;
            CPU.IOWrite += CPUIOWrite;

            machineForm.Paint += MachineFormPaint;
            machineForm.Show();
            machineForm.BringToFront();
        }

        void DMARaised(object sender, Util.ByteArrayEventArgs e)
        {
            var device = sender as INeedsDMA;

            if (device == null)
                return;

            dmaController.DoTransfer(device.DMAChannel, e.ByteArray);
        }

        void IRQRaised(object sender, EventArgs e)
        {
            var device = sender as INeedsIRQ;

            if (device == null)
                return;

            picDevice.RequestInterrupt((byte)device.IRQNumber);
        }

        private void MachineFormPaint(object sender, PaintEventArgs e)
        {
           vgaDevice.GDIDraw(e.Graphics);
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

            LoadBIOS();
            LoadVGABios();

            foreach(IDevice device in devices)
            {
                INeedsIRQ irqDevice = device as INeedsIRQ;
                INeedsDMA dmaDevice = device as INeedsDMA;

                if(irqDevice != null)
                    irqDevice.IRQ += IRQRaised;

                if(dmaDevice != null)
                    dmaDevice.DMA += DMARaised;

                foreach(int port in device.PortsUsed)
                    SetupIOEntry((ushort)port, device.Read, device.Write);
            }

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
            CPU.FlushLog();
        }

        public void RunCycle(double frequency, ulong timerTicks)
        {
            if (Running)
            {
                string tempOpStr;
                int irq, vector;

                foreach (IDevice device in devices)
                {
                    var clockDevice = device as INeedsClock;

                    if(clockDevice != null)
                        clockDevice.Cycle(frequency, timerTicks);
                }
                if (!CPU.Halted)
                    logFile.WriteLine(Operation);

                if(picDevice.InterruptService(out irq, out vector))
                {
                    if(CPU.IF)
                    {
                        CPU.Interrupt(vector, irq);
                        picDevice.AckInterrupt((byte)irq);
                    }
                }
                CPU.Cycle(opLen, opCode, operands);
                opLen = CPU.Decode(CPU.EIP, out opCode, out tempOpStr, out operands);
                Operation = String.Format("{0:X4}:{1:X} {2}", CPU.CS, CPU.EIP, tempOpStr);
                if(timerTicks % 10000 == 0)
                    machineForm.Invalidate();
            }
        }
    }
}
