using System.Diagnostics;
using log4net;
using System.Threading;
using System;

namespace x86CS.Devices
{
    public class PIC8259 : IDevice
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Memory)); 

        private readonly int[] portsUsed = {0x20, 0x21, 0xa0, 0xa1};
        private readonly PIController[] controllers;

        public event EventHandler<InterruptEventArgs> Interrupt;

        public PIC8259()
        {
            controllers = new PIController[2];
            controllers[0] = new PIController();
            controllers[1] = new PIController();
        }

        private void OnInterrupt(InterruptEventArgs e)
        {
            EventHandler<InterruptEventArgs> handler = Interrupt;
            if (handler != null)
                handler(this, e);
        }

        public bool RunController()
        {
            int runningIRQ = LowestRunningInt();
            int pendingIRQ = LowestPending();
            byte irq, vector;

            if (pendingIRQ == -1)
                return false;

            if (runningIRQ < 0)
            {
                irq = (byte)pendingIRQ;
                if (irq < 8)
                    vector = (byte)(controllers[0].VectorBase + irq);
                else
                    vector = (byte)(controllers[1].VectorBase + irq);

                OnInterrupt(new InterruptEventArgs(irq, vector));
                return true;
            }
            return false;
        }

        public int[] PortsUsed
        {
            get { return portsUsed; }
        }

        public bool RequestInterrupt(byte irq)
        {
            PIController controller = irq < 8 ? controllers[0] : controllers[1];

            if (((controller.MaskRegister >> irq & 0x1)) != 0)
                return false;

            return controller.RequestInterrupt(irq);
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

            if (pendingIRQ == -1)
            {
                irq = 0;
                vector = 0;
                return false;
            }

            if (runningIRQ < 0)
            {
                irq = pendingIRQ;
                if (irq < 8)
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
            if (irq < 8)
                controllers[0].AckInterrupt(irq);
            else
                controllers[1].AckInterrupt(irq);
        }

        public uint Read(ushort addr, int size)
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

            if ((controller.CommandRegister & 0x3) == 0x3)
                return controller.InServiceRegister;

            return addr%10 == 0 ? controller.StatusRegister : controller.DataRegister;
        }

        public void Write(ushort addr, uint value, int size)
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
                controller.ProcessICW((byte) value);
            else if (addr%0x10 == 0)
            {
                if (value == 0x20)
                    controller.EOI();
                controller.CommandRegister = (byte) value;
            }
            else
                controller.MaskRegister = (byte) value;
        }
    }

    internal class PIController
    {
        private byte requestRegister;
        private byte currentICW;
        private bool expectICW4;
        private byte linkIRQ;

        public byte InServiceRegister { get; private set; }
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
            InServiceRegister = 0;
        }

        public bool RequestInterrupt(byte irq)
        {
            if (((InServiceRegister >> irq) & 0x1) != 0)
                return false;

            if (((requestRegister >> irq) & 0x1) != 0)
                return false;

            requestRegister |= (byte) (1 << irq);

            return true;
        }

        public int LowestRunning()
        {
            for (int i = 0; i < 8; i++)
            {
                if (((InServiceRegister >> i) & 0x1) == 0x1)
                    return i;
            }
            return -1;
        }

        public int LowestPending()
        {
            for (int i = 0; i < 8; i++)
            {
                if (((requestRegister >> i) & 0x1) == 0x1)
                    return i;
            }
            return -1;
        }

        public void AckInterrupt(byte irq)
        {
            requestRegister &= (byte) ~(1 << irq);
            InServiceRegister |= (byte) (1 << irq);
        }

        public void EOI()
        {
            InServiceRegister = 0;
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
                    linkIRQ = (byte) (icw & 0x7);
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
}
