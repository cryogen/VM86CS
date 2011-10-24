namespace x86CS
{
    partial class ConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainTabs = new System.Windows.Forms.TabControl();
            this.machinePage = new System.Windows.Forms.TabPage();
            this.generalGroup = new System.Windows.Forms.GroupBox();
            this.memoryLabel = new System.Windows.Forms.Label();
            this.memory = new System.Windows.Forms.TextBox();
            this.memoryTrack = new System.Windows.Forms.TrackBar();
            this.diskPage = new System.Windows.Forms.TabPage();
            this.ideGroup = new System.Windows.Forms.GroupBox();
            this.secondarySlaveClear = new System.Windows.Forms.Button();
            this.secondarySlaveOpen = new System.Windows.Forms.Button();
            this.secondarySlave = new System.Windows.Forms.TextBox();
            this.secondaryMasterClear = new System.Windows.Forms.Button();
            this.secondaryMasterOpen = new System.Windows.Forms.Button();
            this.secondaryMaster = new System.Windows.Forms.TextBox();
            this.primarySlaveClear = new System.Windows.Forms.Button();
            this.primarySlaveOpen = new System.Windows.Forms.Button();
            this.primarySlave = new System.Windows.Forms.TextBox();
            this.primaryMasterClear = new System.Windows.Forms.Button();
            this.primaryMasterOpen = new System.Windows.Forms.Button();
            this.primaryMaster = new System.Windows.Forms.TextBox();
            this.primarySlaveType = new System.Windows.Forms.ComboBox();
            this.secondaryMasterType = new System.Windows.Forms.ComboBox();
            this.secondarySlaveType = new System.Windows.Forms.ComboBox();
            this.primaryMasterType = new System.Windows.Forms.ComboBox();
            this.primarySlaveLabel = new System.Windows.Forms.Label();
            this.secondaryMasterLabel = new System.Windows.Forms.Label();
            this.secondarySlaveLabel = new System.Windows.Forms.Label();
            this.primaryMasterLabel = new System.Windows.Forms.Label();
            this.floppyGroup = new System.Windows.Forms.GroupBox();
            this.secondaryClear = new System.Windows.Forms.Button();
            this.primaryClear = new System.Windows.Forms.Button();
            this.openSecondary = new System.Windows.Forms.Button();
            this.openPrimary = new System.Windows.Forms.Button();
            this.primaryFloppy = new System.Windows.Forms.TextBox();
            this.secondaryFloppy = new System.Windows.Forms.TextBox();
            this.secondaryFloppyLabel = new System.Windows.Forms.Label();
            this.primaryFloppyLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.imageOpen = new System.Windows.Forms.OpenFileDialog();
            this.mainTabs.SuspendLayout();
            this.machinePage.SuspendLayout();
            this.generalGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memoryTrack)).BeginInit();
            this.diskPage.SuspendLayout();
            this.ideGroup.SuspendLayout();
            this.floppyGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTabs
            // 
            this.mainTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTabs.Controls.Add(this.machinePage);
            this.mainTabs.Controls.Add(this.diskPage);
            this.mainTabs.Location = new System.Drawing.Point(0, 0);
            this.mainTabs.Name = "mainTabs";
            this.mainTabs.SelectedIndex = 0;
            this.mainTabs.Size = new System.Drawing.Size(544, 259);
            this.mainTabs.TabIndex = 0;
            // 
            // machinePage
            // 
            this.machinePage.BackColor = System.Drawing.SystemColors.Control;
            this.machinePage.Controls.Add(this.generalGroup);
            this.machinePage.Location = new System.Drawing.Point(4, 22);
            this.machinePage.Name = "machinePage";
            this.machinePage.Padding = new System.Windows.Forms.Padding(3);
            this.machinePage.Size = new System.Drawing.Size(536, 233);
            this.machinePage.TabIndex = 0;
            this.machinePage.Text = "Machine Settings";
            // 
            // generalGroup
            // 
            this.generalGroup.BackColor = System.Drawing.SystemColors.Control;
            this.generalGroup.Controls.Add(this.memoryLabel);
            this.generalGroup.Controls.Add(this.memory);
            this.generalGroup.Controls.Add(this.memoryTrack);
            this.generalGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.generalGroup.Location = new System.Drawing.Point(3, 3);
            this.generalGroup.Name = "generalGroup";
            this.generalGroup.Size = new System.Drawing.Size(530, 227);
            this.generalGroup.TabIndex = 0;
            this.generalGroup.TabStop = false;
            this.generalGroup.Text = "Settings:";
            // 
            // memoryLabel
            // 
            this.memoryLabel.AutoSize = true;
            this.memoryLabel.Location = new System.Drawing.Point(6, 26);
            this.memoryLabel.Name = "memoryLabel";
            this.memoryLabel.Size = new System.Drawing.Size(47, 13);
            this.memoryLabel.TabIndex = 5;
            this.memoryLabel.Text = "Memory:";
            // 
            // memory
            // 
            this.memory.Location = new System.Drawing.Point(391, 26);
            this.memory.Name = "memory";
            this.memory.Size = new System.Drawing.Size(42, 20);
            this.memory.TabIndex = 4;
            this.memory.KeyDown += new System.Windows.Forms.KeyEventHandler(this.memory_KeyDown);
            this.memory.Leave += new System.EventHandler(this.memory_Leave);
            // 
            // memoryTrack
            // 
            this.memoryTrack.LargeChange = 32;
            this.memoryTrack.Location = new System.Drawing.Point(53, 19);
            this.memoryTrack.Maximum = 512;
            this.memoryTrack.Minimum = 16;
            this.memoryTrack.Name = "memoryTrack";
            this.memoryTrack.Size = new System.Drawing.Size(335, 45);
            this.memoryTrack.SmallChange = 4;
            this.memoryTrack.TabIndex = 3;
            this.memoryTrack.TickFrequency = 8;
            this.memoryTrack.Value = 16;
            this.memoryTrack.Scroll += new System.EventHandler(this.memoryTrack_Scroll);
            // 
            // diskPage
            // 
            this.diskPage.BackColor = System.Drawing.SystemColors.Control;
            this.diskPage.Controls.Add(this.ideGroup);
            this.diskPage.Controls.Add(this.floppyGroup);
            this.diskPage.Location = new System.Drawing.Point(4, 22);
            this.diskPage.Name = "diskPage";
            this.diskPage.Padding = new System.Windows.Forms.Padding(3);
            this.diskPage.Size = new System.Drawing.Size(536, 233);
            this.diskPage.TabIndex = 1;
            this.diskPage.Text = "Disks";
            // 
            // ideGroup
            // 
            this.ideGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ideGroup.Controls.Add(this.secondarySlaveClear);
            this.ideGroup.Controls.Add(this.secondarySlaveOpen);
            this.ideGroup.Controls.Add(this.secondarySlave);
            this.ideGroup.Controls.Add(this.secondaryMasterClear);
            this.ideGroup.Controls.Add(this.secondaryMasterOpen);
            this.ideGroup.Controls.Add(this.secondaryMaster);
            this.ideGroup.Controls.Add(this.primarySlaveClear);
            this.ideGroup.Controls.Add(this.primarySlaveOpen);
            this.ideGroup.Controls.Add(this.primarySlave);
            this.ideGroup.Controls.Add(this.primaryMasterClear);
            this.ideGroup.Controls.Add(this.primaryMasterOpen);
            this.ideGroup.Controls.Add(this.primaryMaster);
            this.ideGroup.Controls.Add(this.primarySlaveType);
            this.ideGroup.Controls.Add(this.secondaryMasterType);
            this.ideGroup.Controls.Add(this.secondarySlaveType);
            this.ideGroup.Controls.Add(this.primaryMasterType);
            this.ideGroup.Controls.Add(this.primarySlaveLabel);
            this.ideGroup.Controls.Add(this.secondaryMasterLabel);
            this.ideGroup.Controls.Add(this.secondarySlaveLabel);
            this.ideGroup.Controls.Add(this.primaryMasterLabel);
            this.ideGroup.Location = new System.Drawing.Point(3, 101);
            this.ideGroup.Name = "ideGroup";
            this.ideGroup.Size = new System.Drawing.Size(530, 129);
            this.ideGroup.TabIndex = 4;
            this.ideGroup.TabStop = false;
            this.ideGroup.Text = "IDE Devices:";
            // 
            // secondarySlaveClear
            // 
            this.secondarySlaveClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.secondarySlaveClear.Location = new System.Drawing.Point(452, 98);
            this.secondarySlaveClear.Name = "secondarySlaveClear";
            this.secondarySlaveClear.Size = new System.Drawing.Size(30, 23);
            this.secondarySlaveClear.TabIndex = 22;
            this.secondarySlaveClear.Text = "X";
            this.secondarySlaveClear.UseVisualStyleBackColor = true;
            this.secondarySlaveClear.Click += new System.EventHandler(this.secondarySlaveClear_Click);
            // 
            // secondarySlaveOpen
            // 
            this.secondarySlaveOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.secondarySlaveOpen.Location = new System.Drawing.Point(488, 98);
            this.secondarySlaveOpen.Name = "secondarySlaveOpen";
            this.secondarySlaveOpen.Size = new System.Drawing.Size(30, 23);
            this.secondarySlaveOpen.TabIndex = 21;
            this.secondarySlaveOpen.Text = "...";
            this.secondarySlaveOpen.UseVisualStyleBackColor = true;
            this.secondarySlaveOpen.Click += new System.EventHandler(this.secondarySlaveOpen_Click);
            // 
            // secondarySlave
            // 
            this.secondarySlave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.secondarySlave.Location = new System.Drawing.Point(191, 100);
            this.secondarySlave.Name = "secondarySlave";
            this.secondarySlave.ReadOnly = true;
            this.secondarySlave.Size = new System.Drawing.Size(256, 20);
            this.secondarySlave.TabIndex = 20;
            // 
            // secondaryMasterClear
            // 
            this.secondaryMasterClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.secondaryMasterClear.Location = new System.Drawing.Point(452, 72);
            this.secondaryMasterClear.Name = "secondaryMasterClear";
            this.secondaryMasterClear.Size = new System.Drawing.Size(30, 23);
            this.secondaryMasterClear.TabIndex = 19;
            this.secondaryMasterClear.Text = "X";
            this.secondaryMasterClear.UseVisualStyleBackColor = true;
            this.secondaryMasterClear.Click += new System.EventHandler(this.secondaryMasterClear_Click);
            // 
            // secondaryMasterOpen
            // 
            this.secondaryMasterOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.secondaryMasterOpen.Location = new System.Drawing.Point(488, 72);
            this.secondaryMasterOpen.Name = "secondaryMasterOpen";
            this.secondaryMasterOpen.Size = new System.Drawing.Size(30, 23);
            this.secondaryMasterOpen.TabIndex = 18;
            this.secondaryMasterOpen.Text = "...";
            this.secondaryMasterOpen.UseVisualStyleBackColor = true;
            this.secondaryMasterOpen.Click += new System.EventHandler(this.secondaryMasterOpen_Click);
            // 
            // secondaryMaster
            // 
            this.secondaryMaster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.secondaryMaster.Location = new System.Drawing.Point(191, 74);
            this.secondaryMaster.Name = "secondaryMaster";
            this.secondaryMaster.ReadOnly = true;
            this.secondaryMaster.Size = new System.Drawing.Size(256, 20);
            this.secondaryMaster.TabIndex = 17;
            // 
            // primarySlaveClear
            // 
            this.primarySlaveClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.primarySlaveClear.Location = new System.Drawing.Point(452, 49);
            this.primarySlaveClear.Name = "primarySlaveClear";
            this.primarySlaveClear.Size = new System.Drawing.Size(30, 23);
            this.primarySlaveClear.TabIndex = 16;
            this.primarySlaveClear.Text = "X";
            this.primarySlaveClear.UseVisualStyleBackColor = true;
            this.primarySlaveClear.Click += new System.EventHandler(this.primarySlaveClear_Click);
            // 
            // primarySlaveOpen
            // 
            this.primarySlaveOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.primarySlaveOpen.Location = new System.Drawing.Point(488, 49);
            this.primarySlaveOpen.Name = "primarySlaveOpen";
            this.primarySlaveOpen.Size = new System.Drawing.Size(30, 23);
            this.primarySlaveOpen.TabIndex = 15;
            this.primarySlaveOpen.Text = "...";
            this.primarySlaveOpen.UseVisualStyleBackColor = true;
            this.primarySlaveOpen.Click += new System.EventHandler(this.primarySlaveOpen_Click);
            // 
            // primarySlave
            // 
            this.primarySlave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.primarySlave.Location = new System.Drawing.Point(191, 51);
            this.primarySlave.Name = "primarySlave";
            this.primarySlave.ReadOnly = true;
            this.primarySlave.Size = new System.Drawing.Size(256, 20);
            this.primarySlave.TabIndex = 14;
            // 
            // primaryMasterClear
            // 
            this.primaryMasterClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryMasterClear.Location = new System.Drawing.Point(452, 24);
            this.primaryMasterClear.Name = "primaryMasterClear";
            this.primaryMasterClear.Size = new System.Drawing.Size(30, 23);
            this.primaryMasterClear.TabIndex = 13;
            this.primaryMasterClear.Text = "X";
            this.primaryMasterClear.UseVisualStyleBackColor = true;
            this.primaryMasterClear.Click += new System.EventHandler(this.primaryMasterClear_Click);
            // 
            // primaryMasterOpen
            // 
            this.primaryMasterOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryMasterOpen.Location = new System.Drawing.Point(488, 24);
            this.primaryMasterOpen.Name = "primaryMasterOpen";
            this.primaryMasterOpen.Size = new System.Drawing.Size(30, 23);
            this.primaryMasterOpen.TabIndex = 12;
            this.primaryMasterOpen.Text = "...";
            this.primaryMasterOpen.UseVisualStyleBackColor = true;
            this.primaryMasterOpen.Click += new System.EventHandler(this.primaryMasterOpen_Click);
            // 
            // primaryMaster
            // 
            this.primaryMaster.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryMaster.Location = new System.Drawing.Point(191, 26);
            this.primaryMaster.Name = "primaryMaster";
            this.primaryMaster.ReadOnly = true;
            this.primaryMaster.Size = new System.Drawing.Size(256, 20);
            this.primaryMaster.TabIndex = 11;
            // 
            // primarySlaveType
            // 
            this.primarySlaveType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.primarySlaveType.FormattingEnabled = true;
            this.primarySlaveType.Location = new System.Drawing.Point(107, 50);
            this.primarySlaveType.Name = "primarySlaveType";
            this.primarySlaveType.Size = new System.Drawing.Size(78, 21);
            this.primarySlaveType.TabIndex = 10;
            this.primarySlaveType.SelectedIndexChanged += new System.EventHandler(this.DiskTypeSelectedIndexChanged);
            // 
            // secondaryMasterType
            // 
            this.secondaryMasterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.secondaryMasterType.FormattingEnabled = true;
            this.secondaryMasterType.Location = new System.Drawing.Point(107, 77);
            this.secondaryMasterType.Name = "secondaryMasterType";
            this.secondaryMasterType.Size = new System.Drawing.Size(78, 21);
            this.secondaryMasterType.TabIndex = 9;
            this.secondaryMasterType.SelectedIndexChanged += new System.EventHandler(this.DiskTypeSelectedIndexChanged);
            // 
            // secondarySlaveType
            // 
            this.secondarySlaveType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.secondarySlaveType.FormattingEnabled = true;
            this.secondarySlaveType.Location = new System.Drawing.Point(107, 102);
            this.secondarySlaveType.Name = "secondarySlaveType";
            this.secondarySlaveType.Size = new System.Drawing.Size(78, 21);
            this.secondarySlaveType.TabIndex = 8;
            this.secondarySlaveType.SelectedIndexChanged += new System.EventHandler(this.DiskTypeSelectedIndexChanged);
            // 
            // primaryMasterType
            // 
            this.primaryMasterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.primaryMasterType.FormattingEnabled = true;
            this.primaryMasterType.Location = new System.Drawing.Point(107, 23);
            this.primaryMasterType.Name = "primaryMasterType";
            this.primaryMasterType.Size = new System.Drawing.Size(78, 21);
            this.primaryMasterType.TabIndex = 7;
            this.primaryMasterType.SelectedIndexChanged += new System.EventHandler(this.DiskTypeSelectedIndexChanged);
            // 
            // primarySlaveLabel
            // 
            this.primarySlaveLabel.AutoSize = true;
            this.primarySlaveLabel.Location = new System.Drawing.Point(27, 53);
            this.primarySlaveLabel.Name = "primarySlaveLabel";
            this.primarySlaveLabel.Size = new System.Drawing.Size(74, 13);
            this.primarySlaveLabel.TabIndex = 6;
            this.primarySlaveLabel.Text = "Primary Slave:";
            // 
            // secondaryMasterLabel
            // 
            this.secondaryMasterLabel.AutoSize = true;
            this.secondaryMasterLabel.Location = new System.Drawing.Point(5, 80);
            this.secondaryMasterLabel.Name = "secondaryMasterLabel";
            this.secondaryMasterLabel.Size = new System.Drawing.Size(96, 13);
            this.secondaryMasterLabel.TabIndex = 5;
            this.secondaryMasterLabel.Text = "Secondary Master:";
            // 
            // secondarySlaveLabel
            // 
            this.secondarySlaveLabel.AutoSize = true;
            this.secondarySlaveLabel.Location = new System.Drawing.Point(13, 107);
            this.secondarySlaveLabel.Name = "secondarySlaveLabel";
            this.secondarySlaveLabel.Size = new System.Drawing.Size(88, 13);
            this.secondarySlaveLabel.TabIndex = 4;
            this.secondarySlaveLabel.Text = "Seconday Slave:";
            // 
            // primaryMasterLabel
            // 
            this.primaryMasterLabel.AutoSize = true;
            this.primaryMasterLabel.Location = new System.Drawing.Point(22, 26);
            this.primaryMasterLabel.Name = "primaryMasterLabel";
            this.primaryMasterLabel.Size = new System.Drawing.Size(79, 13);
            this.primaryMasterLabel.TabIndex = 3;
            this.primaryMasterLabel.Text = "Primary Master:";
            // 
            // floppyGroup
            // 
            this.floppyGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.floppyGroup.Controls.Add(this.secondaryClear);
            this.floppyGroup.Controls.Add(this.primaryClear);
            this.floppyGroup.Controls.Add(this.openSecondary);
            this.floppyGroup.Controls.Add(this.openPrimary);
            this.floppyGroup.Controls.Add(this.primaryFloppy);
            this.floppyGroup.Controls.Add(this.secondaryFloppy);
            this.floppyGroup.Controls.Add(this.secondaryFloppyLabel);
            this.floppyGroup.Controls.Add(this.primaryFloppyLabel);
            this.floppyGroup.Location = new System.Drawing.Point(2, 6);
            this.floppyGroup.Name = "floppyGroup";
            this.floppyGroup.Size = new System.Drawing.Size(532, 89);
            this.floppyGroup.TabIndex = 3;
            this.floppyGroup.TabStop = false;
            this.floppyGroup.Text = "Floppy Drives:";
            // 
            // secondaryClear
            // 
            this.secondaryClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.secondaryClear.Location = new System.Drawing.Point(453, 45);
            this.secondaryClear.Name = "secondaryClear";
            this.secondaryClear.Size = new System.Drawing.Size(30, 23);
            this.secondaryClear.TabIndex = 9;
            this.secondaryClear.Text = "X";
            this.secondaryClear.UseVisualStyleBackColor = true;
            this.secondaryClear.Click += new System.EventHandler(this.secondaryClear_Click);
            // 
            // primaryClear
            // 
            this.primaryClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryClear.Location = new System.Drawing.Point(453, 20);
            this.primaryClear.Name = "primaryClear";
            this.primaryClear.Size = new System.Drawing.Size(30, 23);
            this.primaryClear.TabIndex = 8;
            this.primaryClear.Text = "X";
            this.primaryClear.UseVisualStyleBackColor = true;
            this.primaryClear.Click += new System.EventHandler(this.primaryClear_Click);
            // 
            // openSecondary
            // 
            this.openSecondary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openSecondary.Location = new System.Drawing.Point(489, 45);
            this.openSecondary.Name = "openSecondary";
            this.openSecondary.Size = new System.Drawing.Size(30, 23);
            this.openSecondary.TabIndex = 7;
            this.openSecondary.Text = "...";
            this.openSecondary.UseVisualStyleBackColor = true;
            this.openSecondary.Click += new System.EventHandler(this.openSecondary_Click);
            // 
            // openPrimary
            // 
            this.openPrimary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.openPrimary.Location = new System.Drawing.Point(489, 20);
            this.openPrimary.Name = "openPrimary";
            this.openPrimary.Size = new System.Drawing.Size(30, 23);
            this.openPrimary.TabIndex = 6;
            this.openPrimary.Text = "...";
            this.openPrimary.UseVisualStyleBackColor = true;
            this.openPrimary.Click += new System.EventHandler(this.openPrimary_Click);
            // 
            // primaryFloppy
            // 
            this.primaryFloppy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryFloppy.Location = new System.Drawing.Point(73, 22);
            this.primaryFloppy.Name = "primaryFloppy";
            this.primaryFloppy.ReadOnly = true;
            this.primaryFloppy.Size = new System.Drawing.Size(375, 20);
            this.primaryFloppy.TabIndex = 4;
            // 
            // secondaryFloppy
            // 
            this.secondaryFloppy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.secondaryFloppy.Location = new System.Drawing.Point(73, 47);
            this.secondaryFloppy.Name = "secondaryFloppy";
            this.secondaryFloppy.ReadOnly = true;
            this.secondaryFloppy.Size = new System.Drawing.Size(375, 20);
            this.secondaryFloppy.TabIndex = 5;
            // 
            // secondaryFloppyLabel
            // 
            this.secondaryFloppyLabel.AutoSize = true;
            this.secondaryFloppyLabel.Location = new System.Drawing.Point(6, 50);
            this.secondaryFloppyLabel.Name = "secondaryFloppyLabel";
            this.secondaryFloppyLabel.Size = new System.Drawing.Size(61, 13);
            this.secondaryFloppyLabel.TabIndex = 1;
            this.secondaryFloppyLabel.Text = "Secondary:";
            // 
            // primaryFloppyLabel
            // 
            this.primaryFloppyLabel.AutoSize = true;
            this.primaryFloppyLabel.Location = new System.Drawing.Point(23, 25);
            this.primaryFloppyLabel.Name = "primaryFloppyLabel";
            this.primaryFloppyLabel.Size = new System.Drawing.Size(44, 13);
            this.primaryFloppyLabel.TabIndex = 0;
            this.primaryFloppyLabel.Text = "Primary:";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(359, 264);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "&Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(462, 264);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // imageOpen
            // 
            this.imageOpen.DefaultExt = "img";
            this.imageOpen.Filter = "Image Files|*.img|All Files|*.*";
            this.imageOpen.Title = "Select Image";
            // 
            // ConfigForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(545, 295);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.mainTabs);
            this.Name = "ConfigForm";
            this.Text = "Machine Configuration";
            this.Load += new System.EventHandler(this.ConfigForm_Load);
            this.mainTabs.ResumeLayout(false);
            this.machinePage.ResumeLayout(false);
            this.generalGroup.ResumeLayout(false);
            this.generalGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memoryTrack)).EndInit();
            this.diskPage.ResumeLayout(false);
            this.ideGroup.ResumeLayout(false);
            this.ideGroup.PerformLayout();
            this.floppyGroup.ResumeLayout(false);
            this.floppyGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl mainTabs;
        private System.Windows.Forms.TabPage machinePage;
        private System.Windows.Forms.TabPage diskPage;
        private System.Windows.Forms.GroupBox generalGroup;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox memory;
        private System.Windows.Forms.TrackBar memoryTrack;
        private System.Windows.Forms.Label memoryLabel;
        private System.Windows.Forms.GroupBox floppyGroup;
        private System.Windows.Forms.Button openSecondary;
        private System.Windows.Forms.Button openPrimary;
        private System.Windows.Forms.TextBox primaryFloppy;
        private System.Windows.Forms.TextBox secondaryFloppy;
        private System.Windows.Forms.Label secondaryFloppyLabel;
        private System.Windows.Forms.Label primaryFloppyLabel;
        private System.Windows.Forms.OpenFileDialog imageOpen;
        private System.Windows.Forms.Button secondaryClear;
        private System.Windows.Forms.Button primaryClear;
        private System.Windows.Forms.GroupBox ideGroup;
        private System.Windows.Forms.Label primaryMasterLabel;
        private System.Windows.Forms.ComboBox primaryMasterType;
        private System.Windows.Forms.Label primarySlaveLabel;
        private System.Windows.Forms.Label secondaryMasterLabel;
        private System.Windows.Forms.Label secondarySlaveLabel;
        private System.Windows.Forms.ComboBox primarySlaveType;
        private System.Windows.Forms.ComboBox secondaryMasterType;
        private System.Windows.Forms.ComboBox secondarySlaveType;
        private System.Windows.Forms.Button secondarySlaveClear;
        private System.Windows.Forms.Button secondarySlaveOpen;
        private System.Windows.Forms.TextBox secondarySlave;
        private System.Windows.Forms.Button secondaryMasterClear;
        private System.Windows.Forms.Button secondaryMasterOpen;
        private System.Windows.Forms.TextBox secondaryMaster;
        private System.Windows.Forms.Button primarySlaveClear;
        private System.Windows.Forms.Button primarySlaveOpen;
        private System.Windows.Forms.TextBox primarySlave;
        private System.Windows.Forms.Button primaryMasterClear;
        private System.Windows.Forms.Button primaryMasterOpen;
        private System.Windows.Forms.TextBox primaryMaster;
    }
}