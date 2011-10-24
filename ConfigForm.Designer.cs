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
            this.mainTabs.Size = new System.Drawing.Size(453, 259);
            this.mainTabs.TabIndex = 0;
            // 
            // machinePage
            // 
            this.machinePage.BackColor = System.Drawing.SystemColors.Control;
            this.machinePage.Controls.Add(this.generalGroup);
            this.machinePage.Location = new System.Drawing.Point(4, 22);
            this.machinePage.Name = "machinePage";
            this.machinePage.Padding = new System.Windows.Forms.Padding(3);
            this.machinePage.Size = new System.Drawing.Size(445, 233);
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
            this.generalGroup.Size = new System.Drawing.Size(439, 227);
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
            this.diskPage.Controls.Add(this.floppyGroup);
            this.diskPage.Location = new System.Drawing.Point(4, 22);
            this.diskPage.Name = "diskPage";
            this.diskPage.Padding = new System.Windows.Forms.Padding(3);
            this.diskPage.Size = new System.Drawing.Size(445, 233);
            this.diskPage.TabIndex = 1;
            this.diskPage.Text = "Disks";
            // 
            // floppyGroup
            // 
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
            this.floppyGroup.Size = new System.Drawing.Size(441, 89);
            this.floppyGroup.TabIndex = 3;
            this.floppyGroup.TabStop = false;
            this.floppyGroup.Text = "Floppy Drives:";
            // 
            // secondaryClear
            // 
            this.secondaryClear.Location = new System.Drawing.Point(362, 45);
            this.secondaryClear.Name = "secondaryClear";
            this.secondaryClear.Size = new System.Drawing.Size(30, 23);
            this.secondaryClear.TabIndex = 9;
            this.secondaryClear.Text = "X";
            this.secondaryClear.UseVisualStyleBackColor = true;
            this.secondaryClear.Click += new System.EventHandler(this.secondaryClear_Click);
            // 
            // primaryClear
            // 
            this.primaryClear.Location = new System.Drawing.Point(362, 20);
            this.primaryClear.Name = "primaryClear";
            this.primaryClear.Size = new System.Drawing.Size(30, 23);
            this.primaryClear.TabIndex = 8;
            this.primaryClear.Text = "X";
            this.primaryClear.UseVisualStyleBackColor = true;
            this.primaryClear.Click += new System.EventHandler(this.primaryClear_Click);
            // 
            // openSecondary
            // 
            this.openSecondary.Location = new System.Drawing.Point(398, 45);
            this.openSecondary.Name = "openSecondary";
            this.openSecondary.Size = new System.Drawing.Size(30, 23);
            this.openSecondary.TabIndex = 7;
            this.openSecondary.Text = "...";
            this.openSecondary.UseVisualStyleBackColor = true;
            this.openSecondary.Click += new System.EventHandler(this.openSecondary_Click);
            // 
            // openPrimary
            // 
            this.openPrimary.Location = new System.Drawing.Point(398, 22);
            this.openPrimary.Name = "openPrimary";
            this.openPrimary.Size = new System.Drawing.Size(30, 23);
            this.openPrimary.TabIndex = 6;
            this.openPrimary.Text = "...";
            this.openPrimary.UseVisualStyleBackColor = true;
            this.openPrimary.Click += new System.EventHandler(this.openPrimary_Click);
            // 
            // primaryFloppy
            // 
            this.primaryFloppy.Location = new System.Drawing.Point(73, 22);
            this.primaryFloppy.Name = "primaryFloppy";
            this.primaryFloppy.ReadOnly = true;
            this.primaryFloppy.Size = new System.Drawing.Size(284, 20);
            this.primaryFloppy.TabIndex = 4;
            // 
            // secondaryFloppy
            // 
            this.secondaryFloppy.Location = new System.Drawing.Point(73, 47);
            this.secondaryFloppy.Name = "secondaryFloppy";
            this.secondaryFloppy.ReadOnly = true;
            this.secondaryFloppy.Size = new System.Drawing.Size(284, 20);
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
            this.okButton.Location = new System.Drawing.Point(268, 264);
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
            this.cancelButton.Location = new System.Drawing.Point(371, 264);
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
            // 
            // ConfigForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(454, 295);
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
    }
}