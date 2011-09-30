using System;
using System.Windows.Forms;
using System.Globalization;

namespace x86CS
{
    public partial class Breakpoints : Form
    {
        public event EventHandler<Util.IntEventArgs> ItemAdded;
        public event EventHandler<Util.IntEventArgs> ItemDeleted;

        public Breakpoints()
        {
            InitializeComponent();
        }

        private void OnItemAdded(int item)
        {
            var intEvent = ItemAdded;

            if (intEvent != null)
                intEvent(this, new Util.IntEventArgs(item));
        }

        private void OnItemDeleted(int item)
        {
            var intEvent = ItemDeleted;

            if (intEvent != null)
                intEvent(this, new Util.IntEventArgs(item));
        }

        private void AddButtonClick(object sender, EventArgs e)
        {
            uint seg, off;

            if (!uint.TryParse(segment.Text, NumberStyles.HexNumber, null, out seg))
                seg = 0;
            if (!uint.TryParse(offset.Text, NumberStyles.HexNumber, null, out off))
                off = 0;

            var item = new ListViewItem {Text = seg.ToString("X")};
            item.SubItems.Add(off.ToString("X"));
            breakpointList.Items.Add(item);

            var addr = (seg << 4) + off;

            OnItemAdded((int)addr);
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
