using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

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
        private Dictionary<int, InteruptHandler> interuptVectors = new Dictionary<int, InteruptHandler>();
        private Stack<char> keyPresses = new Stack<char>();
        private Dictionary<int, int> breakpoints = new Dictionary<int, int>();
        private SystemConfig sysConfig = new SystemConfig();
        private uint systemAddr = 0xf0000;
        private uint sysConfigAddr;
        DateTime currTime = DateTime.Now;
        private byte midnightCounter = 0;
        private Dictionary<ushort, IOEntry> ioPorts;
        private StreamWriter logFile = File.CreateText("machinelog.txt");
        private CMOS cmos = new CMOS();
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

            logFile.AutoFlush = true;

            SetupSystem();

            cpu.InteruptFired += new EventHandler<IntEventArgs>(cpu_InteruptFired);
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

        private void SetupSystem()
        {
            InteruptHandler defaultHandler = new InteruptHandler(UnhandledInt);

            ioPorts = new Dictionary<ushort, IOEntry>();

            for (uint i = 0; i < 0x10; i++)
                SetupInterrupt(i, defaultHandler);
            SetupInterrupt(0x10, new InteruptHandler(Int10));
            SetupInterrupt(0x11, new InteruptHandler(Int11));
            SetupInterrupt(0x12, new InteruptHandler(Int12));
            SetupInterrupt(0x13, new InteruptHandler(Int13));
            SetupInterrupt(0x14, defaultHandler);
            SetupInterrupt(0x15, new InteruptHandler(Int15));
            SetupInterrupt(0x16, new InteruptHandler(_Int16));
            SetupInterrupt(0x17, defaultHandler);
            SetupInterrupt(0x18, defaultHandler);
            SetupInterrupt(0x19, new InteruptHandler(Int19));
            SetupInterrupt(0x1a, new InteruptHandler(Int1a));
            for (uint i = 0x1b; i < 0x1e; i++)
                SetupInterrupt(i, defaultHandler);

            SetupSystemConfig();
            SetupDPT();

            SetupIOEntry(0x70, new ReadCallback(cmos.Read), new WriteCallback(cmos.Write));
            SetupIOEntry(0x71, new ReadCallback(cmos.Read), new WriteCallback(cmos.Write));

            /* BDA */
            Memory.WriteByte(0x496, 0x10);
        }

        private void UnhandledInt()
        {

        }

        private void SetupInterrupt(uint vector, Delegate handler)
        {
            GCHandle handle;
            IntPtr p;

            handle = GCHandle.Alloc(handler);
            p = Marshal.GetFunctionPointerForDelegate(handler);

            Memory.BlockWrite(systemAddr, BitConverter.GetBytes(p.ToInt32()), 4);

            WriteIVTEntry(vector, systemAddr);
            systemAddr += 4;
        }

        private void WriteIVTEntry(uint offset, uint addr)
        {
            ushort seg;
            ushort off;
            
            off = (ushort)(addr & 0xffff);
            addr -= off;
            seg = (ushort)((addr >> 4) & 0xffff);

            Memory.WriteWord(offset * 4, off);
            Memory.WriteWord(offset * 4 + 2, seg);
        }

        private void SetupSystemConfig()
        {
            IntPtr p;
            byte[] tmp;
            uint configSize = (uint)Marshal.SizeOf(sysConfig);

            sysConfig.NumBytes = 8;
            sysConfig.Model = 0xfc;
            sysConfig.SubModel = 0;
            sysConfig.Revision = 0;
            sysConfig.Feature1 = 0x74;
            sysConfig.Feature2 = 0x40;
            sysConfig.Feature3 = 0;
            sysConfig.Feature4 = 0;
            sysConfig.Feature5 = 0;

            p = Marshal.AllocHGlobal((int)configSize);
            Marshal.StructureToPtr(sysConfig, p, false);
            tmp = new byte[configSize];

            Marshal.Copy(p, tmp, 0, (int)configSize);
            Memory.BlockWrite(systemAddr, tmp, (int)configSize);

            Marshal.FreeHGlobal(p);

            sysConfigAddr = systemAddr;

            systemAddr += configSize;
        }

        private void SetupDPT()
        {
            IntPtr p;
            byte[] tmp;
            uint dptSize = (uint)Marshal.SizeOf(floppyDrive.DPT);

            p = Marshal.AllocHGlobal((int)dptSize);
            Marshal.StructureToPtr(floppyDrive.DPT, p, false);
            tmp = new byte[dptSize];
            Marshal.Copy(p, tmp, 0, (int)dptSize);

            Memory.BlockWrite(systemAddr, tmp, (int)dptSize);

            WriteIVTEntry(0x1E, systemAddr);

            Marshal.FreeHGlobal(p);

            systemAddr += dptSize;
        }

        public void Restart()
        {
            running = false;
            cpu.Reset();
            SetupSystem();
            Int19();
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
            int cpuAddr = (cpu.CS << 4) + cpu.IP;

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

        void cpu_InteruptFired(object sender, IntEventArgs e)
        {
            InteruptHandler handler = null;
            ushort seg, off;
            uint addr;
            IntPtr p;

            off = Memory.ReadWord((uint)(e.Number * 4));
            seg = Memory.ReadWord((uint)(e.Number * 4 + 2));

            addr = (uint)((seg << 4) + off);

            p = new IntPtr(Memory.ReadDWord(addr));

            /*            if (!interuptVectors.TryGetValue(e.Number, out handler))
                        {
                            Console.WriteLine("Missing interrupt {0:X2}", e.Number);
                            return;
                        }*/

            cpu.StackPush(cpu.EFlags);
            cpu.IF = false;
            cpu.TF = false;
            cpu.AC = false;
            cpu.StackPush(cpu.CS);
            cpu.StackPush(cpu.IP);

            if (seg == 0xf000)
                handler = (InteruptHandler)Marshal.GetDelegateForFunctionPointer(p, typeof(InteruptHandler));
            else
            {

                // Marshal failed, interrupt has been overridden
                Console.WriteLine("Call to {0:X2}, has been overriden to {1:X4}:{2:X4}", e.Number, seg, off);

                cpu.CS = seg;
                cpu.IP = off;

                return;
            }

            if (handler != null)
            {
                handler();
                cpu.IP = cpu.StackPop();
                cpu.CS = cpu.StackPop();
                cpu.EFlags = cpu.StackPop();
            }
        }

        private void Int10()
        {
            switch (cpu.AH)
            {
                case 0x0e:
                    DoWriteChar((char)cpu.AL);
                    break;
                default:
                    break;
            }
        }

        private void Int11()
        {
            cpu.AX = 0x4227;
        }

        private void Int12()
        {
            cpu.AX = 639;
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

        private void Int13()
        {
            ushort tmpIP, tmpCS;
            CPUFlags tmpFlags;

            tmpIP = cpu.StackPop();
            tmpCS = cpu.StackPop();
            tmpFlags = (CPUFlags)cpu.StackPop();

            switch (cpu.AH)
            {
                case 0x00:
                    floppyDrive.Reset();
                    tmpFlags &= ~CPUFlags.CF;
                    break;
                case 0x02:
                    if (ReadSector())
                    {
                        tmpFlags &= ~CPUFlags.CF;
                        cpu.AH = 0;
                    }
                    else
                        tmpFlags |= CPUFlags.CF;
                    break;
                case 0x08:
                    tmpFlags |= CPUFlags.CF;
                    cpu.AH = 0x01;
                    break;
                default:
                    break;
            }

            cpu.StackPush((ushort)tmpFlags);
            cpu.StackPush(tmpCS);
            cpu.StackPush(tmpIP);
        }

        private void Int15()
        {
            ushort seg;
            ushort off;
            uint addr = sysConfigAddr;

            off = (ushort)(addr & 0xffff);
            addr -= off;
            seg = (ushort)((addr >> 4) & 0xffff);

            switch (cpu.AH)
            {
                case 0xc0:
                    cpu.CF = false;
                    cpu.AH = 0;

                    cpu.ES = seg;
                    cpu.BX = off;
                    break;
                default:
                    break;
            }
        }

        private void _Int16()
        {
            switch (cpu.AH)
            {
                case 0x00:
                    char c = GetChar();
                    break;
                default:
                    break;
            }
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

        private void Int1a()
        {
            switch (cpu.AH)
            {
                case 0x0:
                    uint numTicks;
                    float tmp;

                    numTicks = (uint)((currTime.Hour * 60 * 60) + (currTime.Minute * 60) + currTime.Second);
                    tmp = numTicks * 18.2f;
                    numTicks = (uint)tmp;

                    cpu.CX = (ushort)(numTicks >> 16);
                    cpu.AX = (ushort)(numTicks & 0xffff);
                    cpu.AL = midnightCounter;

                    midnightCounter--;

                    break;
                case 0x2:
                    cpu.CH = ToBCD(currTime.Hour);
                    cpu.CL = ToBCD(currTime.Minute);
                    cpu.DH = ToBCD(currTime.Second);

                    if (currTime.IsDaylightSavingTime())
                        cpu.DL = 0x1;
                    else
                        cpu.DL = 0x0;

                    cpu.CF = false;
                    break;
                case 0x4:
                    cpu.CH = ToBCD(currTime.Year / 100);
                    cpu.CL = ToBCD(currTime.Year % 100);
                    cpu.DH = ToBCD(currTime.Month);
                    cpu.DL = ToBCD(currTime.Day);

                    cpu.CF = false;
                    break;
                default:
                    break;
            }
        }

        private void Int19()
        {
            bool gotBootSector = false;

            // Try and find a boot loader on the floppy drive image if there is one
            if (floppyDrive.Mounted)
            {
                byte[] bootSect;

                floppyDrive.Reset();

                bootSect = floppyDrive.ReadBytes(512);

                if (bootSect[510] == 0x55 || bootSect[511] == 0xAA)
                {
                    Memory.BlockWrite(0x07c0 << 4, bootSect, 512);
                    gotBootSector = true;
                }
                else
                {
                    DoWriteText("Non bootable Disk in floppy drive");
                }
            }

            if (gotBootSector)
            {
                cpu.CS = 0;
                cpu.IP = 0x7c00;
                running = true;
            }
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
            Int19();
            cpu.Decode(cpu.CS, cpu.IP, out opCode, out opStr, out operands);
        }

        public void Stop()
        {
            running = false;
        }

        public void RunCycle()
        {
            if (running)
            {
                cpu.Cycle(opLen, opCode, operands);
                cpu.Decode(cpu.CS, cpu.IP, out opCode, out opStr, out operands);
            }

            if (DateTime.Now.Day != currTime.Day)
                midnightCounter++;

            currTime = DateTime.Now;
        }
    }
}
