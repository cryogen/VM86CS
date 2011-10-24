using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using x86CS.Configuration;

namespace x86CS
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void memoryTrack_Scroll(object sender, EventArgs e)
        {
            memory.Text = memoryTrack.Value.ToString();
        }

        private void openPrimary_Click(object sender, EventArgs e)
        {
            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                primaryFloppy.Text = imageOpen.FileName;
            }
        }

        private void openSecondary_Click(object sender, EventArgs e)
        {
            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                secondaryFloppy.Text = imageOpen.FileName;
            }
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            memoryTrack.Value = SystemConfig.Machine.MemorySize;
            memory.Text = memoryTrack.Value.ToString();
            primaryFloppy.Text = SystemConfig.Machine.Floppies[0].Image;
            secondaryFloppy.Text = SystemConfig.Machine.Floppies[1].Image;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            FloppyElement floppy;

            DialogResult = DialogResult.OK;

            SystemConfig.Machine.MemorySize = memoryTrack.Value;
            SystemConfig.Machine.Floppies.Clear();

            floppy = new FloppyElement();
            floppy.Id = 0;
            floppy.Image = primaryFloppy.Text;

            SystemConfig.Machine.Floppies.Add(floppy);

            floppy = new FloppyElement();
            floppy.Id = 1;
            floppy.Image = secondaryFloppy.Text;

            SystemConfig.Machine.Floppies.Add(floppy);

            SystemConfig.Save();
        }

        private void memory_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.PageUp:
                case Keys.PageDown:
                    e.SuppressKeyPress = false;
                    return;
                default:
                    break;
            }

            char currentKey = (char)e.KeyCode;
            bool modifier = e.Control || e.Alt || e.Shift;
            bool nonNumber = char.IsLetter(currentKey) ||
                             char.IsSymbol(currentKey) ||
                             char.IsWhiteSpace(currentKey) ||
                             char.IsPunctuation(currentKey);

            if (!modifier && nonNumber)
                e.SuppressKeyPress = true;
        }

        private void memory_Leave(object sender, EventArgs e)
        {
            int size = int.Parse(memory.Text);

            if (size < 16)
                size = 16;
            if (size > 512)
                size = 512;

            memoryTrack.Value = size;

            memory.Text = memoryTrack.Value.ToString();
        }

        private void primaryClear_Click(object sender, EventArgs e)
        {
            primaryFloppy.Text = "";
        }

        private void secondaryClear_Click(object sender, EventArgs e)
        {
            secondaryFloppy.Text = "";
        }

    }
}
