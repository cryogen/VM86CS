using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace x86CS
{
    public partial class Breakpoints : Form
    {
        private List<int> bpList = new List<int>();

        public event EventHandler<IntEventArgs> ItemAdded;
        public event EventHandler<IntEventArgs> ItemDeleted;

        public Breakpoints()
        {
            InitializeComponent();
        }

        private void OnItemAdded(int item)
        {
            EventHandler<IntEventArgs> intEvent = ItemAdded;

            if (intEvent != null)
                intEvent(this, new IntEventArgs(item));
        }

        private void OnItemDeleted(int item)
        {
            EventHandler<IntEventArgs> intEvent = ItemDeleted;

            if (intEvent != null)
                intEvent(this, new IntEventArgs(item));
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            ushort seg, off;

            int addr;

            try
            {
                seg = ushort.Parse(segment.Text, NumberStyles.HexNumber);
                off = ushort.Parse(offset.Text, NumberStyles.HexNumber);
            }
            catch
            {
                MessageBox.Show("Invalid segment/offset", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ListViewItem item = new ListViewItem();
            item.Text = seg.ToString("X4");
            item.SubItems.Add(off.ToString("X4"));
            breakpointList.Items.Add(item);

            addr = (seg << 4) + off;

            OnItemAdded(addr);
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            ListViewItem item;

            if (breakpointList.SelectedItems.Count == 0)
                return;

            item = breakpointList.SelectedItems[0];

            ushort seg, off;

            int addr;

            seg = ushort.Parse(item.Text, NumberStyles.HexNumber);
            off = ushort.Parse(item.SubItems[0].Text, NumberStyles.HexNumber);

            addr = (seg << 4) + off;

            breakpointList.Items.Remove(item);

            OnItemDeleted(addr);
        }
    }
}
