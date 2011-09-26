namespace x86CS
{
    partial class Breakpoints
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
            this.segment = new System.Windows.Forms.TextBox();
            this.offset = new System.Windows.Forms.TextBox();
            this.breakpointList = new System.Windows.Forms.ListView();
            this.segmentHeader = new System.Windows.Forms.ColumnHeader();
            this.OffsetColumn = new System.Windows.Forms.ColumnHeader();
            this.offsetLabel = new System.Windows.Forms.Label();
            this.segmentLabel = new System.Windows.Forms.Label();
            this.addButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // segment
            // 
            this.segment.Location = new System.Drawing.Point(53, 146);
            this.segment.Name = "segment";
            this.segment.Size = new System.Drawing.Size(34, 20);
            this.segment.TabIndex = 1;
            // 
            // offset
            // 
            this.offset.Location = new System.Drawing.Point(129, 146);
            this.offset.Name = "offset";
            this.offset.Size = new System.Drawing.Size(75, 20);
            this.offset.TabIndex = 2;
            // 
            // breakpointList
            // 
            this.breakpointList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.segmentHeader,
            this.OffsetColumn});
            this.breakpointList.FullRowSelect = true;
            this.breakpointList.Location = new System.Drawing.Point(0, 0);
            this.breakpointList.Name = "breakpointList";
            this.breakpointList.Size = new System.Drawing.Size(394, 128);
            this.breakpointList.TabIndex = 3;
            this.breakpointList.UseCompatibleStateImageBehavior = false;
            this.breakpointList.View = System.Windows.Forms.View.Details;
            // 
            // segmentHeader
            // 
            this.segmentHeader.Text = "Segment";
            this.segmentHeader.Width = 196;
            // 
            // OffsetColumn
            // 
            this.OffsetColumn.Text = "Offset";
            this.OffsetColumn.Width = 194;
            // 
            // offsetLabel
            // 
            this.offsetLabel.AutoSize = true;
            this.offsetLabel.Location = new System.Drawing.Point(93, 149);
            this.offsetLabel.Name = "offsetLabel";
            this.offsetLabel.Size = new System.Drawing.Size(35, 13);
            this.offsetLabel.TabIndex = 4;
            this.offsetLabel.Text = "Offset";
            // 
            // segmentLabel
            // 
            this.segmentLabel.AutoSize = true;
            this.segmentLabel.Location = new System.Drawing.Point(4, 149);
            this.segmentLabel.Name = "segmentLabel";
            this.segmentLabel.Size = new System.Drawing.Size(49, 13);
            this.segmentLabel.TabIndex = 5;
            this.segmentLabel.Text = "Segment";
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(210, 144);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 23);
            this.addButton.TabIndex = 6;
            this.addButton.Text = "&Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButtonClick);
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(307, 144);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 8;
            this.deleteButton.Text = "&Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButtonClick);
            // 
            // Breakpoints
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 180);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.segmentLabel);
            this.Controls.Add(this.offsetLabel);
            this.Controls.Add(this.breakpointList);
            this.Controls.Add(this.offset);
            this.Controls.Add(this.segment);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Breakpoints";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Breakpoints";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox segment;
        private System.Windows.Forms.TextBox offset;
        private System.Windows.Forms.ListView breakpointList;
        private System.Windows.Forms.ColumnHeader segmentHeader;
        private System.Windows.Forms.ColumnHeader OffsetColumn;
        private System.Windows.Forms.Label offsetLabel;
        private System.Windows.Forms.Label segmentLabel;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button deleteButton;
    }
}