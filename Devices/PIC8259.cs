using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace x86CS.Devices
{
    internal class PIController
    {
        private byte requestRegister;
        private byte inServiceRegister;
        private byte dataRegister;
        private byte currentICW;
        private bool expectICW4;
        private byte linkIRQ;

        public byte VectorBase { get; private set; }
        public byte MaskRegister { get; set; }
        public bool Init { get; set; }
        public byte CommandRegister { get; set; }
        public byte StatusRegister { get; private set; }
        public byte DataRegister { get; private set; }

        public PIController()
        {
            MaskRegister = 0xff;
            currentICW = 0;
            Init = false;
            StatusRegister = 0;
            DataRegister = 0;
            inServiceRegister = 0;
        }

        public bool RequestInterrupt(byte irq)
        {
            if (((inServiceRegister >> irq) & 0x1) != 0)
                return false;

            if(((requestRegister >> irq) & 0x1) != 0)
                return false;

            requestRegister |= (byte)(1 << irq);

            return true;
        }

        public int LowestRunning()
        {
            for(int i = 0; i < 8; i++)
            {
                if(((inServiceRegister >> i) & 0x1) == 0x1)
                    return i;
            }
            return -1;
        }

        public int LowestPending()
        {
            for(int i = 0; i < 8; i++)
            {
                if(((requestRegister >> i) & 0x1) == 0x1)
                    return i;
            }
            return -1;
        }

        public void AckInterrupt(byte irq)
        {
            requestRegister &= (byte)~(1 << irq);
            inServiceRegister |= (byte)(1 << irq);
        }

        public void EOI()
        {
            inServiceRegister = 0;
        }

        public void ProcessICW(byte icw)
        {
            switch (currentICW)
            {
                case 0:
                    expectICW4 = (icw & 0x1) != 0;
                    break;
                case 1:
                    VectorBase = icw;
                    break;
                case 2:
                    linkIRQ = (byte)(icw & 0x7);
                    if (!expectICW4)
                        Init = true;
                    break;
                case 3:
                    Init = true;
                    break;
            }
            if (Init)
                currentICW = 0;
            else
                currentICW++;
        }
    }

    public class PIC8259 : IDevice
    {
        private readonly int[] portsUsed = { 0x20, 0x21, 0xa0, 0xa1 };
        private readonly PIController[] controllers;

        private const int IrqNumber = -1;
        private const int DmaChannel = -1;

        public PIC8259()
        {
            controllers = new PIController[2];
            controllers[0] = new PIController();
            controllers[1] = new PIController();
        }

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

        public event EventHandler IRQ;
        public event EventHandler<Util.ByteArrayEventArgs> DMA;

        public bool RequestInterrupt(byte irq)
        {
            PIController controller = irq < 8 ? controllers[0] : controllers[1];

            if (((controller.MaskRegister >> irq & 0x1)) != 0)
                return false;

            return controller.RequestInterrupt(irq);
        }

        public void Cycle(double frequency, ulong tickCount)
        {
        }

        private int LowestRunningInt()
        {
            int ret = controllers[0].LowestRunning();
            return ret == -1 ? controllers[1].LowestRunning() : ret;
        }

        private int LowestPending()
        {
            int ret = controllers[0].LowestPending();
            return ret == -1 ? controllers[1].LowestPending() : ret;
        }

        public bool InterruptService(out int irq, out int vector)
        {
            int runningIRQ = LowestRunningInt();
            int pendingIRQ = LowestPending();

            if(pendingIRQ == -1)
            {
                irq = 0;
                vector = 0;
                return false;
            }

            if(runningIRQ < 0)
            {
                irq = pendingIRQ;
                if(irq < 8)
                    vector = controllers[0].VectorBase + irq;
                else
                    vector = controllers[1].VectorBase + irq;

                return true;
            }

            irq = 0;
            vector = 0;
            return false;
        }

        public void AckInterrupt(byte irq)
        {
            if(irq < 8)
                controllers[0].AckInterrupt(irq);
            else
                controllers[1].AckInterrupt(irq);
        }

        public ushort Read(ushort addr)
        {
            PIController controller = null;

            switch (addr)
            {
                case 0x20:
                case 0x21:
                    controller = controllers[0];
                    break;
                case 0xa0:
                case 0xa1:
                    controller = controllers[1];
                    break;
            }
            Debug.Assert(controller != null, "controller != null");
            return addr % 10 == 0 ? controller.StatusRegister : controller.DataRegister;
        }

        public void Write(ushort addr, ushort value)
        {
            PIController controller = null;

            switch (addr)
            {
                case 0x20:
                case 0x21:
                    controller = controllers[0];
                    break;
                case 0xa0:
                case 0xa1:
                    controller = controllers[1];
                    break;
            }

            Debug.Assert(controller != null, "controller != null");
            if (!controller.Init)
                controller.ProcessICW((byte)value);
            else if (addr % 0x10 == 0)
            {
                if (value == 0x20)
                    controller.EOI();
                controller.CommandRegister = (byte)value;
            }
            else
                controller.MaskRegister = (byte)value;
        }
    }
}
