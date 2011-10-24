using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using log4net;
using x86CS.Properties;
using System.Collections.Generic;
using System.Drawing;
using x86CS.Configuration;

namespace x86CS
{
    public partial class MainForm : Form
    {
        private readonly Machine machine;
        private readonly Thread machineThread;
        private readonly Breakpoints breakpoints = new Breakpoints();
        private double frequency = 100000.0f;
        private ulong timerTicks;
        private bool running;
        private Form uiForm;

        public MainForm()
        {
            uiForm = new Form();

            uiForm.ClientSize = new Size(640, 400);
            uiForm.FormBorderStyle = FormBorderStyle.FixedSingle;
            uiForm.MaximizeBox = false;
            uiForm.StartPosition = FormStartPosition.CenterScreen;
            uiForm.Text = "C# x86 Emulator";
            timerTicks = 0;
            machine = new Machine(uiForm);
            Application.ApplicationExit += ApplicationApplicationExit;

            breakpoints.ItemAdded += BreakpointsItemAdded;
            breakpoints.ItemDeleted += BreakpointsItemDeleted;

            InitializeComponent();

            PrintRegisters();

            machine.FloppyDrive.MountImage(@"C:\disk1.img");
            
            machineThread = new Thread(RunMachine);
            running = true;
            machineThread.Start();

            machine.Start();
            SetCPULabel(machine.CPU.InstructionText);
            PrintRegisters();
        }

        void ApplicationApplicationExit(object sender, EventArgs e)
        {
            LogManager.Shutdown();
        }

        void BreakpointsItemDeleted(object sender, UIntEventArgs e)
        {
            machine.ClearBreakpoint(e.Number);
        }

        void BreakpointsItemAdded(object sender, UIntEventArgs e)
        {
            machine.SetBreakpoint(e.Number);
        }

        private void RunMachine()
        {
            var stopwatch = new Stopwatch();
            double lastSeconds = 0;

            stopwatch.Start();

            while (running)
            {
                if (!machine.Running)
                {
                    System.Threading.Thread.Sleep(100);
                    continue;
                }

                ++timerTicks;

                if (timerTicks % 50000 == 0)
                {
                    frequency = 50000 / (stopwatch.Elapsed.TotalSeconds - lastSeconds);
                    lastSeconds = stopwatch.Elapsed.TotalSeconds;
                    if (Created)
                        BeginInvoke((MethodInvoker)delegate { tpsLabel.Text = frequency.ToString("n") + "TPS"; });
                }

                try
                {
                    machine.RunCycle(loggingToolStripMenuItem.Checked, false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Resources.ErrorTitle);
                    return;
                }

                if (machine.CheckBreakpoint())
                {
                    Invoke((MethodInvoker)(() =>
                                               {
                                                   PrintRegisters();
                                                   machine.CPU.ReFetch();
                                                   SetCPULabel(machine.CPU.InstructionText);
                                                   stepButton.Enabled = true;
                                                   stepOverButton.Enabled = true;
                                                   PrintStack();
                                               }));
                    machine.Running = false;
                }
            }
        }

        private void SetCPULabel(string text)
        {
            cpuLabel.Text = String.Format("{0:X}:{1:X} {2}", machine.CPU.CS, machine.CPU.EIP, text);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            machine.Running = false;
            running = false;
            machine.Stop();
            Application.Exit();
        }

        private void PrintRegisters()
        {
            CPU.CPU cpu = machine.CPU;

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

            CF.Text = cpu.CF ? "CF" : "cf";
            PF.Text = cpu.PF ? "PF" : "pf";
            AF.Text = cpu.AF ? "AF" : "af";
            ZF.Text = cpu.ZF ? "ZF" : "zf";
            SF.Text = cpu.SF ? "SF" : "sf";
            TF.Text = cpu.TF ? "TF" : "tf";
            IF.Text = cpu.IF ? "IF" : "if";
            DF.Text = cpu.DF ? "DF" : "df";
            OF.Text = cpu.OF ? "OF" : "of";
            IOPL.Text = cpu.IOPL.ToString("X2");
            AC.Text = cpu.AC ? "AC" : "ac";
            NT.Text = cpu.NT ? "NT" : "nt";
            RF.Text = cpu.RF ? "RF" : "rf";
            VM.Text = cpu.VM ? "VM" : "vm";
            VIF.Text = cpu.VIF ? "VIF" : "vif";
            VIP.Text = cpu.VIP ? "VIP" : "vip";   
        }

        private void RunToolStripMenuItemClick(object sender, EventArgs e)
        {
            runToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            machine.Start();
        }

        private void StopToolStripMenuItemClick(object sender, EventArgs e)
        {
            stopToolStripMenuItem.Enabled = false;
            runToolStripMenuItem.Enabled = true;
            machine.Running = false;
        }

        private void MountToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (floppyOpen.ShowDialog() != DialogResult.OK)
                return;

            machine.FloppyDrive.MountImage(floppyOpen.FileName);
        }

        private void StepButtonClick(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                machine.ClearTempBreakpoints();
                stepButton.Enabled = false;
                stepOverButton.Enabled = false;
                machine.Running = false;
                machine.RunCycle(true, true);
                if (!machine.Running)
                {
                    stepButton.Enabled = true;
                    stepOverButton.Enabled = true;
                    SetCPULabel(machine.CPU.InstructionText);
                    PrintRegisters();
                    PrintStack();
                }
            }));
        }

        private void GoButtonClick(object sender, EventArgs e)
        {
            Memory.LoggingEnabled = loggingToolStripMenuItem.Checked;
            machine.ClearTempBreakpoints();
            machine.Running = true;
            stepButton.Enabled = false;
            stepOverButton.Enabled = false;
            uiForm.BringToFront();
        }

        private void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            machine.Running = false;
            running = false;
            machine.Stop();
        }

        private void MemoryButtonClick(object sender, EventArgs e)
        {
            uint addr = 0;
            byte[] block = new byte[180];

            if (String.IsNullOrEmpty(memOffset.Text))
                addr = 0;
            else
            {
                try
                {
                    addr = uint.Parse(memOffset.Text, NumberStyles.HexNumber);
                }
                catch
                {
                    MessageBox.Show(Resources.Invalid_address, Resources.ErrorTitle);
                }
            }

            Memory.BlockRead(addr, block, block.Length);

            memoryChar.Text = ((char)block[0]).ToString();
            memoryByte.Text = block[0].ToString("X2");
            memoryWord.Text = ((ushort)(block[0] | block[1] << 8)).ToString("X4");
            memoryDWord.Text = ((uint)(block[0] | block[1] << 8 | block[2] << 16 | block[3] << 24)).ToString("X8");

            string memBlock = "";
            string charList = "";
            int offset = 0;

            memoryList.Items.Clear();
            memoryCharList.Items.Clear();

            for (int i = 0; i < block.Length / 18; i++)
            {
                for (offset = 0; offset < 18; offset++)
                {
                    byte b = block[i * 18 + offset];
                    memBlock += b.ToString("X2") + " ";

                    char c = Convert.ToChar(b);
                    if (Char.IsControl(c))
                        c = '.';
                    charList += c + " ";
                }

                memoryList.Items.Add(memBlock);
                memoryCharList.Items.Add(charList);
                memBlock = "";
                charList = "";
            }
        }

        private void BreakpointsToolStripMenuItemClick(object sender, EventArgs e)
        {
            breakpoints.ShowDialog();
        }

        private void RestartToolStripMenuItemClick(object sender, EventArgs e)
        {
            machine.Restart();
            machine.CPU.ReFetch();
            SetCPULabel(machine.CPU.InstructionText);
            PrintRegisters();
            PrintStack();
            stepButton.Enabled = true;
            stepOverButton.Enabled = true;
        }

        private void PrintStack()
        {
            stackList.Items.Clear();
            baseList.Items.Clear();

            uint stackPointer = machine.CPU.StackPointer;
            int stackSize = machine.CPU.PMode ? 4 : 2;
            for (uint i = stackPointer, j = 0; i < (uint)(stackPointer + (machine.CPU.PMode ? 52 : 26)); i += (uint)stackSize, j++)
            {
                if (i == 0)
                    break;

                stackList.Items.Add((j * stackSize).ToString("X2") + " " + (Memory.Read(i, stackSize * 8)).ToString("X"));
            }

            stackPointer = (uint)(machine.CPU.BasePointer - (6 * stackSize));
            int k = -6;
            for (uint i = stackPointer; i < (uint)(stackPointer + (machine.CPU.PMode ? 52 : 26)); i += (uint)stackSize, k++)
            {
                if (i == 0)
                    break;

                int index = k * stackSize;
                string str;

                if (index < 0)
                    str = "-" + (-index).ToString("X2");
                else
                    str = "+" + index.ToString("X2");
                baseList.Items.Add(str + " " + (Memory.Read(i, stackSize * 8)).ToString("X"));
            }
        }

        private void stepOverButton_Click(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                stepOverButton.Enabled = false;
                stepButton.Enabled = false;
                machine.ClearTempBreakpoints();
                machine.StepOver();
            }));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm config = new ConfigForm();

            config.ShowDialog();
        }
    }
}
