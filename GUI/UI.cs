using System.Windows.Forms;
namespace x86CS.GUI
{
    public abstract class UI
    {
        public UI(Form uiForm)
        {
        }

        public abstract void Init();
        public abstract void Cycle();
    }
}
