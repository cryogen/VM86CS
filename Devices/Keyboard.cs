using System;
using System.Collections.Generic;

namespace x86CS.Devices
{
    [Flags]
    public enum KeyboardFlags
    {
        OutputBufferFull = 0x1,
        InputBufferFull = 0x2,
        SystemFlag = 0x4,
        CommandFlag = 0x8,
        UnLocked = 0x10,
        MouseData = 0x20,
        Timeout = 0x40,
        ParityError = 0x80
    }

    public class Keyboard
    {
        private readonly Queue<byte> outputBuffer;
        private byte inputBuffer;
        private byte commandByte;
        private KeyboardFlags statusRegister;
        private bool enabled;
        private bool setCommandByte;

        public Keyboard()
        {
            statusRegister |= KeyboardFlags.UnLocked;
            outputBuffer = new Queue<byte>();
        }

        private void SetStatusCode(byte status)
        {
            statusRegister |= KeyboardFlags.OutputBufferFull;
            outputBuffer.Enqueue(status);
        }

        private void ProcessCommand()
        {
            if (setCommandByte)
            {
                SetStatusCode(0xfa);
                setCommandByte = false;
            }
            else
            {

                switch (inputBuffer)
                {
                    case 0x60:
                        setCommandByte = true;
                        break;
                    case 0xaa:
                        statusRegister |= KeyboardFlags.SystemFlag;
                        SetStatusCode(0x55);
                        break;
                    case 0xab:
                        SetStatusCode(0x00);
                        break;
                    case 0xae:
                        enabled = true;
                        break;
                    case 0xf5:
                        SetStatusCode(0xfa);
                        break;
                    case 0xff:
                        SetStatusCode(0xfa);
                        SetStatusCode(0xaa);
                        break;
                }
            }
        }

        public ushort Read(ushort address)
        {
            switch (address)
            {
                case 0x60:
                    byte ret = setCommandByte ? commandByte : outputBuffer.Dequeue();

                    if (outputBuffer.Count == 0)
                        statusRegister &= ~KeyboardFlags.OutputBufferFull;
                    setCommandByte = false;
                    return ret;
                case 0x64:
                    return (ushort) statusRegister;
                default:
                    break;
            }

            return 0;
        }

        public void Write(ushort address, ushort value)
        {
            switch (address)
            {
                case 0x60:
                    if (setCommandByte)
                        commandByte = (byte)value;
                    else
                        inputBuffer = (byte)value;

                    statusRegister &= ~KeyboardFlags.CommandFlag;
                    ProcessCommand();
                    break;
                case 0x64:
                    inputBuffer = (byte)value;
                    statusRegister |= KeyboardFlags.CommandFlag;
                    ProcessCommand();
                    break;
                default:
                    break;
            }
        }
    }
}
