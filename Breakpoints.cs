using System;
using System.Windows.Forms;
using System.Globalization;
using x86CS.Properties;

namespace x86CS
{
    public partial class Breakpoints : Form
    {
        public event EventHandler<IntEventArgs> ItemAdded;
        public event EventHandler<IntEventArgs> ItemDeleted;

        public Breakpoints()
        {
            InitializeComponent();
        }

        private void OnItemAdded(int item)
        {
            var intEvent = ItemAdded;

            if (intEvent != null)
                intEvent(this, new IntEventArgs(item));
        }

        private void OnItemDeleted(int item)
        {
            var intEvent = ItemDeleted;

            if (intEvent != null)
                intEvent(this, new IntEventArgs(item));
        }

        private void AddButtonClick(object sender, EventArgs e)
        {
            ushort seg, off;

            try
            {
                seg = ushort.Parse(segment.Text, NumberStyles.HexNumber);
                off = ushort.Parse(offset.Text, NumberStyles.HexNumber);
            }
            catch
            {
                MessageBox.Show(Resources.Invalid_segment_offset, Resources.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var item = new ListViewItem {Text = seg.ToString("X4")};
            item.SubItems.Add(off.ToString("X4"));
            breakpointList.Items.Add(item);

            var addr = (seg << 4) + off;

            OnItemAdded(addr);
        }

        private void DeleteButtonClick(object sender, EventArgs e)
        {
            if (breakpointList.SelectedItems.Count == 0)
                return;

            var item = breakpointList.SelectedItems[0];

            var seg = ushort.Parse(item.Text, NumberStyles.HexNumber);
            var off = ushort.Parse(item.SubItems[1].Text, NumberStyles.HexNumber);

            var addr = (seg << 4) + off;

            breakpointList.Items.Remove(item);

            OnItemDeleted(addr);
        }
    }
}
