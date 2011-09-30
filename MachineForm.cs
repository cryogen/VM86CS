using System;
using System.Windows.Forms;

namespace x86CS
{
    public partial class MachineForm : Form
    {
        public MachineForm()
        {
            InitializeComponent();
        }

        private void MachineFormLoad(object sender, EventArgs e)
        {
            BringToFront();
        }
    }
}
