using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace x86CS
{
    public partial class MainForm : Form
    {
        private bool running = false;
        private Machine machine = new Machine();
        private string[] screenText = new string[25];
        private int currLine = 0;
        Font panelFont = new Font("Courier New", 9.64f);
        bool clearDebug = true;

        public MainForm()
        {
            machine = new Machine();
            machine.WriteText += new EventHandler<TextEventArgs>(machine_WriteText);
            machine.CPU.DebugText += new EventHandler<TextEventArgs>(CPU_DebugText);

            InitializeComponent();

            mainPanel.Paint += new PaintEventHandler(mainPanel_Paint);

            PrintRegisters();

            mainPanel.Select();
        }

        void CPU_DebugText(object sender, TextEventArgs e)
        {
            if (clearDebug)
                cpuLabel.Text = "";

            if (e.Text.EndsWith("\n"))
            {
                cpuLabel.Text += e.Text.Substring(0, e.Text.Length - 1);
                clearDebug = true;
            }
            else
            {
                cpuLabel.Text += e.Text;
                clearDebug = false;
            }
        }

        void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < 25; i++)
            {
                string line = screenText[i];
                e.Graphics.DrawString(line, panelFont, Brushes.White, new PointF(0, i * panelFont.Height * 1.06f));
            }
        }

        void machine_WriteText(object sender, TextEventArgs e)
        {
            Graphics g = mainPanel.CreateGraphics();
            SizeF stringSize = g.MeasureString(e.Text, panelFont);
            int numLines;

            numLines = (int)stringSize.Width / mainPanel.Width;

            screenText[currLine++] = e.Text;
            if (currLine >= 25)
                currLine = 0;

            mainPanel.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            running = false;
            clockTimer.Stop();
            Application.Exit();
        }

        private void PrintRegisters()
        {
            CPU cpu = machine.CPU;

            EAX.Text = cpu.EAX.ToString("X8");
            EBX.Text = cpu.EBX.ToString("X8");
            ECX.Text = cpu.ECX.ToString("X8");
            EDX.Text = cpu.EDX.ToString("X8");
            ESI.Text = cpu.ESI.ToString("X8");
            EDI.Text = cpu.EDI.ToString("X8");
            EBP.Text = cpu.EBP.ToString("X8");
            ESP.Text = cpu.ESP.ToString("X8");
            CS.Text = cpu.CS.ToString("X4");
            DS.Text = cpu.DS.ToString("X4");
            ES.Text = cpu.ES.ToString("X4");
            FS.Text = cpu.FS.ToString("X4");
            GS.Text = cpu.GS.ToString("X4");
            SS.Text = cpu.SS.ToString("X4");

            CF.Text = cpu.CF ? "CF" : "NC";
            PF.Text = cpu.PF ? "PO" : "PE";
            AF.Text = cpu.AF ? "AC" : "NA";
            ZF.Text = cpu.ZF ? "ZR" : "NZ";
            SF.Text = cpu.SF ? "NG" : "PL";
            TF.Text = cpu.TF ? "TP" : "NT";
            IF.Text = cpu.IF ? "EI" : "DI";
            DF.Text = cpu.DF ? "DN" : "UP";
            OF.Text = cpu.OF ? "OV" : "NV";
            IOPL.Text = cpu.IOPL.ToString("X2");
            AC.Text = cpu.AC ? "AF" : "NA";
            NT.Text = cpu.NT ? "NT" : "NN";
            RF.Text = cpu.RF ? "RF" : "NR";
            VM.Text = cpu.VM ? "VM" : "RM";
            VIF.Text = cpu.VIF ? "VF" : "NF";
            VIP.Text = cpu.VIP ? "VP" : "NP";   
        }

        private void clockTimer_Tick(object sender, EventArgs e)
        {
            if (!running)
                return;

            PrintRegisters();
            machine.RunCycle();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            running = true;
            runToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            clockTimer.Start();
            machine.Start();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopToolStripMenuItem.Enabled = false;
            runToolStripMenuItem.Enabled = true;
            running = false;
            clockTimer.Stop();
            machine.Stop();
        }

        private void mountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (floppyOpen.ShowDialog() != DialogResult.OK)
                return;

            machine.FloppyDrive.MountImage(floppyOpen.FileName);
        }

        private void stepButton_Click(object sender, EventArgs e)
        {
            if (!machine.Running)
                machine.Start();
            PrintRegisters();
            machine.RunCycle();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            clockTimer.Start();
        }
    }
}
