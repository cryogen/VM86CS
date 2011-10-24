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
            imageOpen.Filter = "Image Files|*.img|All Files|*.*";
            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                primaryFloppy.Text = imageOpen.FileName;
            }
        }

        private void openSecondary_Click(object sender, EventArgs e)
        {
            imageOpen.Filter = "Image Files|*.img|All Files|*.*";
            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                secondaryFloppy.Text = imageOpen.FileName;
            }
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            primaryMasterType.DataSource = Enum.GetNames(typeof(DriveType)); ;
            primarySlaveType.DataSource = Enum.GetNames(typeof(DriveType)); ;
            secondaryMasterType.DataSource = Enum.GetNames(typeof(DriveType)); ;
            secondarySlaveType.DataSource = Enum.GetNames(typeof(DriveType)); ;

            memoryTrack.Value = SystemConfig.Machine.MemorySize;
            memory.Text = memoryTrack.Value.ToString();
            
            primaryFloppy.Text = SystemConfig.Machine.Floppies[0].Image;
            secondaryFloppy.Text = SystemConfig.Machine.Floppies[1].Image;

            primaryMasterType.SelectedIndex = (int)SystemConfig.Machine.Disks[0].Type;
            primaryMaster.Text = SystemConfig.Machine.Disks[0].Image;
            primarySlaveType.SelectedIndex = (int)SystemConfig.Machine.Disks[1].Type;
            primarySlave.Text = SystemConfig.Machine.Disks[1].Image;
            secondaryMasterType.SelectedIndex = (int)SystemConfig.Machine.Disks[2].Type;
            secondaryMaster.Text = SystemConfig.Machine.Disks[2].Image;
            secondarySlaveType.SelectedIndex = (int)SystemConfig.Machine.Disks[3].Type;
            secondarySlave.Text = SystemConfig.Machine.Disks[3].Image;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            FloppyElement floppy;
            DiskElement disk;

            DialogResult = DialogResult.OK;

            SystemConfig.Machine.MemorySize = memoryTrack.Value;
            SystemConfig.Machine.Floppies.Clear();
            SystemConfig.Machine.Disks.Clear();

            floppy = new FloppyElement();
            floppy.Id = 0;
            floppy.Image = primaryFloppy.Text;
            SystemConfig.Machine.Floppies.Add(floppy);

            floppy = new FloppyElement();
            floppy.Id = 1;
            floppy.Image = secondaryFloppy.Text;
            SystemConfig.Machine.Floppies.Add(floppy);

            disk = new DiskElement();
            disk.Id = 0;
            disk.Image = primaryMaster.Text;
            disk.Type = (DriveType)primaryMasterType.SelectedIndex;
            SystemConfig.Machine.Disks.Add(disk);

            disk = new DiskElement();
            disk.Id = 1;
            disk.Image = primarySlave.Text;
            disk.Type = (DriveType)primarySlaveType.SelectedIndex;
            SystemConfig.Machine.Disks.Add(disk);

            disk = new DiskElement();
            disk.Id = 2;
            disk.Image = secondaryMaster.Text;
            disk.Type = (DriveType)secondaryMasterType.SelectedIndex;
            SystemConfig.Machine.Disks.Add(disk);

            disk = new DiskElement();
            disk.Id = 3;
            disk.Image = secondarySlave.Text;
            disk.Type = (DriveType)secondarySlaveType.SelectedIndex;
            SystemConfig.Machine.Disks.Add(disk);

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

        private void primaryMasterClear_Click(object sender, EventArgs e)
        {
            primaryMaster.Text = "";
        }

        private void primarySlaveClear_Click(object sender, EventArgs e)
        {
            primarySlave.Text = "";
        }

        private void secondaryMasterClear_Click(object sender, EventArgs e)
        {
            secondaryMaster.Text = "";
        }

        private void secondarySlaveClear_Click(object sender, EventArgs e)
        {
            secondarySlave.Text = "";
        }

        private void primaryMasterOpen_Click(object sender, EventArgs e)
        {
            if((string)primaryMasterType.SelectedItem == "HardDisk")
                imageOpen.Filter = "Hard Disk Images|*.vhd|All Files|*.*";
            else
                imageOpen.Filter = "ISO Images|*.iso|All Files|*.*";

            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                primaryMaster.Text = imageOpen.FileName;
            }
        }

        private void DiskTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox dropDown = sender as ComboBox;

            if ((string)dropDown.SelectedItem == "None")
            {
                if (dropDown == primaryMasterType)
                {
                    primaryMasterOpen.Enabled = false;
                    primaryMasterClear.Enabled = false;
                }
                else if (dropDown == primarySlaveType)
                {
                    primarySlaveOpen.Enabled = false;
                    primarySlaveClear.Enabled = false;
                }
                else if (dropDown == secondaryMasterType)
                {
                    secondaryMasterOpen.Enabled = false;
                    secondaryMasterClear.Enabled = false;
                }
                else if (dropDown == secondarySlaveType)
                {
                    secondarySlaveOpen.Enabled = false;
                    secondarySlaveClear.Enabled = false;
                }
            }
            else
            {
                if (dropDown == primaryMasterType)
                {
                    primaryMasterOpen.Enabled = true;
                    primaryMasterClear.Enabled = true;
                }
                else if (dropDown == primarySlaveType)
                {
                    primarySlaveOpen.Enabled = true;
                    primarySlaveClear.Enabled = true;
                }
                else if (dropDown == secondaryMasterType)
                {
                    secondaryMasterOpen.Enabled = true;
                    secondaryMasterClear.Enabled = true;
                }
                else if (dropDown == secondarySlaveType)
                {
                    secondarySlaveOpen.Enabled = true;
                    secondarySlaveClear.Enabled = true;
                }
            }
        }

        private void primarySlaveOpen_Click(object sender, EventArgs e)
        {
            if ((string)primarySlaveType.SelectedItem == "HardDisk")
                imageOpen.Filter = "Hard Disk Images|*.vhd|All Files|*.*";
            else
                imageOpen.Filter = "ISO Images|*.iso|All Files|*.*";

            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                primarySlave.Text = imageOpen.FileName;
            }

        }

        private void secondaryMasterOpen_Click(object sender, EventArgs e)
        {
            if ((string)secondaryMasterType.SelectedItem == "HardDisk")
                imageOpen.Filter = "Hard Disk Images|*.vhd|All Files|*.*";
            else
                imageOpen.Filter = "ISO Images|*.iso|All Files|*.*";

            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                secondaryMaster.Text = imageOpen.FileName;
            }
        }

        private void secondarySlaveOpen_Click(object sender, EventArgs e)
        {
            if ((string)secondarySlaveType.SelectedItem == "HardDisk")
                imageOpen.Filter = "Hard Disk Images|*.vhd|All Files|*.*";
            else
                imageOpen.Filter = "ISO Images|*.iso|All Files|*.*";

            if (imageOpen.ShowDialog() == DialogResult.OK)
            {
                secondarySlave.Text = imageOpen.FileName;
            }
        }

    }
}
