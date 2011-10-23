using System.Windows.Forms;
using System;
namespace x86CS.GUI
{
    public abstract class UI
    {
        public event EventHandler<UIntEventArgs> KeyDown;
        public event EventHandler<UIntEventArgs> KeyUp;

        public UI(Form uiForm)
        {
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
