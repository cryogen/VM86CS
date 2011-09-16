using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace x86CS
{
    public partial class MainForm : Form
    {
        private Machine machine;
        private string[] screenText = new string[25];
        private int currLine, currPos;
        Font panelFont = new Font("Courier New", 9.64f);
        bool clearDebug = true;
        bool stepping = false;
        Thread machineThread;

        public MainForm()
        {
            machine = new Machine();
            machine.WriteText += new EventHandler<TextEventArgs>(machine_WriteText);
            machine.WriteChar += new EventHandler<CharEventArgs>(machine_WriteChar);
            machine.CPU.DebugText += new EventHandler<TextEventArgs>(CPU_DebugText);

            currLine = currPos = 0;

            InitializeComponent();

            PrintRegisters();

            mainPanel.Select();

            machineThread = new Thread(new ThreadStart(RunMachine));
            machineThread.Start();

            for (int i = 0; i < screenText.Length; i++)
            {
                screenText[i] = new string(' ', 80);
            }
        }

        private void RunMachine()
        {
            while (true)
            {
                if(machine.Running && !stepping)
                    machine.RunCycle();
            }
        }

        void machine_WriteChar(object sender, CharEventArgs e)
        {
            switch (e.Char)
            {
                case '\r':
                    currPos = 0;
                    break;
                case '\n':
                    currLine++;
                    break;
                default:
                    char[] chars = screenText[currLine].ToCharArray();

                    chars[currPos] = e.Char;

                    screenText[currLine] = new string(chars);
                    currPos++;
                    break;
            }

            if (currPos == 80)
            {
                currPos = 0;
                currLine++;
            }

            if (currLine >= 24)
            {
                currLine = 0;
                currPos = 0;
            }

            //mainPanel.Invalidate(new Rectangle(0, currLine * panelFont.Height, mainPanel.Width, panelFont.Height*2));
            mainPanel.Invalidate();
        }

        private void SetCPULabel(string text)
        {
            if (clearDebug)
                cpuLabel.Text = "";

            if (text.EndsWith("\n"))
            {
                cpuLabel.Text += text.Substring(0, text.Length - 1);
                clearDebug = true;
            }
            else
            {
                cpuLabel.Text += text;
                clearDebug = false;
            }
        }

        void CPU_DebugText(object sender, TextEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate { SetCPULabel(e.Text); });
        }

        void mainPanel_Paint(object sender, PaintEventArgs e)
        {
            if (e.ClipRectangle.Height == 0)
                return;
            for (int i = 0; i < 25; i++)
            {
                string line = screenText[i];
                if (String.IsNullOrEmpty(line))
                    continue;
                e.Graphics.DrawString(line, panelFont, Brushes.White, new PointF(0, i * panelFont.Height * 1.06f));
            }
        }

        void machine_WriteText(object sender, TextEventArgs e)
        {
            screenText[currLine++] = e.Text;
            if (currLine >= 25)
                currLine = 0;

            mainPanel.Invalidate();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            machine.Stop();
            machineThread.Abort();
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
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            machine.Start();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopToolStripMenuItem.Enabled = false;
            runToolStripMenuItem.Enabled = true;
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
            stepping = true;
            if (!machine.Running)
                machine.Start();
            PrintRegisters();
            machine.CPU.Debug = true;
            machine.RunCycle();
        }

        private void goButton_Click(object sender, EventArgs e)
        {
            if (!machine.Running)
                machine.Start();
            machine.CPU.Debug = false;
            stepping = false;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            machineThread.Abort();
        }

        private void mainPanel_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            machine.KeyPresses.Push((char)e.KeyValue);
        }

        private void mainPanel_Click(object sender, EventArgs e)
        {
            mainPanel.Select();
        }

        private void memoryButton_Click(object sender, EventArgs e)
        {
            ushort seg;
            ushort off;
            int addr;

            seg = ushort.Parse(memSegment.Text, NumberStyles.HexNumber);
            off = ushort.Parse(memOffset.Text, NumberStyles.HexNumber);
                
            addr = (seg << 4) + off;

            memByte.Text = Memory.ReadByte(addr).ToString("X2");
            memWord.Text = Memory.ReadWord(addr).ToString("X4");
        }
    }
}
