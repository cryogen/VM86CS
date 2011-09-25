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
        private byte vectorBase;
        private byte linkIRQ;

        public PIController()
        {
            MaskRegister = 0xff;
            currentICW = 0;
            Init = false;
            StatusRegister = 0;
            DataRegister = 0;
        }

        public byte MaskRegister { get; set; }
        public bool Init { get; set; }
        public byte CommandRegister { get; set; }
        public byte StatusRegister { get; private set; }
        public byte DataRegister { get; private set;  }

        public void ProcessICW(byte icw)
        {
            switch (currentICW)
            {
                case 0:
                    expectICW4 = (icw & 0x1) != 0;
                    break;
                case 1:
                    vectorBase = icw;
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

    public class PIC8259
    {
        private readonly PIController[] controllers;

        public PIC8259()
        {
            controllers = new PIController[2];
            controllers[0] = new PIController();
            controllers[1] = new PIController();
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
            else if (addr % 10 == 0)
                controller.CommandRegister = (byte)value;
            else
                controller.MaskRegister = (byte)value;
        }
    }
}
