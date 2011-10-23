using System.Windows.Forms;
using System;
using x86CS.Devices;
namespace x86CS.GUI
{
    public abstract class UI
    {
        public event EventHandler<UIntEventArgs> KeyDown;
        public event EventHandler<UIntEventArgs> KeyUp;

        protected VGA vgaDevice;

        public UI(Form uiForm, VGA device)
        {
            vgaDevice = device;
        }

        public virtual void OnKeyDown(uint key)
        {
            EventHandler<UIntEventArgs> keyDown = KeyDown;
            if (keyDown != null)
                keyDown(this, new UIntEventArgs(key));
        }

        public virtual void OnKeyUp(uint key)
        {
            EventHandler<UIntEventArgs> keyUp = KeyUp;
            if (keyUp != null)
                keyUp(this, new UIntEventArgs(key));
        }

        public abstract void Init();
        public abstract void Cycle();
    }
}
