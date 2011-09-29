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
        public bool EOIPending { get; set; }

        public PIController()
        {
            MaskRegister = 0xff;
            currentICW = 0;
            Init = false;
            StatusRegister = 0;
            DataRegister = 0;
            EOIPending = false;
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

        public void Cycle(double frequency, ulong tickCount)
        {
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
                    controller.EOIPending = false;
                controller.CommandRegister = (byte)value;
            }
            else
                controller.MaskRegister = (byte)value;
        }

        public int FindInterruptVector(int irq)
        {
            PIController controller = irq < 8 ? controllers[0] : controllers[1];

            if (((controller.MaskRegister >> irq) & 0x1) == 0x0)
            {
                if (controller.EOIPending)
                    return -1;

                controller.EOIPending = true;
                return controller.VectorBase + irq;
            }

            return -1;
        }

        public void AckAll()
        {
            controllers[0].EOIPending = false;
            controllers[1].EOIPending = false;
        }
    }
}
