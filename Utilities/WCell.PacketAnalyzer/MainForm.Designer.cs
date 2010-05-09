//namespace WCell.PacketAnalyzer
//{
//    partial class MainForm
//    {
//        /// <summary>
//        /// Required designer variable.
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;

//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code

//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            this.tabControl = new System.Windows.Forms.TabControl();
//            this.tabRawInput = new System.Windows.Forms.TabPage();
//            this.btnAnalyze = new System.Windows.Forms.Button();
//            this.txtRawInput = new System.Windows.Forms.TextBox();
//            this.tabAnalyzed = new System.Windows.Forms.TabPage();
//            this.lblLengthValue = new System.Windows.Forms.Label();
//            this.lblLength = new System.Windows.Forms.Label();
//            this.lblOpcodeValue = new System.Windows.Forms.Label();
//            this.lblOpcode = new System.Windows.Forms.Label();
//            this.txtAnalyzed = new System.Windows.Forms.TextBox();
//            this.tabOutput = new System.Windows.Forms.TabPage();
//            this.txtOutput = new System.Windows.Forms.TextBox();
//            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
//            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
//            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
//            this.packetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
//            this.a9ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
//            this.compressedA9ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
//            this.tabControl.SuspendLayout();
//            this.tabRawInput.SuspendLayout();
//            this.tabAnalyzed.SuspendLayout();
//            this.tabOutput.SuspendLayout();
//            this.menuStrip1.SuspendLayout();
//            this.SuspendLayout();
//            // 
//            // tabControl
//            // 
//            this.tabControl.Controls.Add(this.tabRawInput);
//            this.tabControl.Controls.Add(this.tabAnalyzed);
//            this.tabControl.Controls.Add(this.tabOutput);
//            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.tabControl.Location = new System.Drawing.Point(0, 24);
//            this.tabControl.Name = "tabControl";
//            this.tabControl.SelectedIndex = 0;
//            this.tabControl.Size = new System.Drawing.Size(751, 428);
//            this.tabControl.TabIndex = 1;
//            // 
//            // tabRawInput
//            // 
//            this.tabRawInput.Controls.Add(this.btnAnalyze);
//            this.tabRawInput.Controls.Add(this.txtRawInput);
//            this.tabRawInput.Location = new System.Drawing.Point(4, 22);
//            this.tabRawInput.Name = "tabRawInput";
//            this.tabRawInput.Padding = new System.Windows.Forms.Padding(3);
//            this.tabRawInput.Size = new System.Drawing.Size(743, 402);
//            this.tabRawInput.TabIndex = 0;
//            this.tabRawInput.Text = "Raw Input";
//            this.tabRawInput.UseVisualStyleBackColor = true;
//            // 
//            // btnAnalyze
//            // 
//            this.btnAnalyze.Location = new System.Drawing.Point(334, 371);
//            this.btnAnalyze.Name = "btnAnalyze";
//            this.btnAnalyze.Size = new System.Drawing.Size(75, 23);
//            this.btnAnalyze.TabIndex = 1;
//            this.btnAnalyze.Text = "Analyze";
//            this.btnAnalyze.UseVisualStyleBackColor = true;
//            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
//            // 
//            // txtRawInput
//            // 
//            this.txtRawInput.Location = new System.Drawing.Point(8, 6);
//            this.txtRawInput.MaxLength = 150000;
//            this.txtRawInput.Multiline = true;
//            this.txtRawInput.Name = "txtRawInput";
//            this.txtRawInput.Size = new System.Drawing.Size(719, 359);
//            this.txtRawInput.TabIndex = 0;
//            // 
//            // tabAnalyzed
//            // 
//            this.tabAnalyzed.Controls.Add(this.lblLengthValue);
//            this.tabAnalyzed.Controls.Add(this.lblLength);
//            this.tabAnalyzed.Controls.Add(this.lblOpcodeValue);
//            this.tabAnalyzed.Controls.Add(this.lblOpcode);
//            this.tabAnalyzed.Controls.Add(this.txtAnalyzed);
//            this.tabAnalyzed.Location = new System.Drawing.Point(4, 22);
//            this.tabAnalyzed.Name = "tabAnalyzed";
//            this.tabAnalyzed.Padding = new System.Windows.Forms.Padding(3);
//            this.tabAnalyzed.Size = new System.Drawing.Size(743, 402);
//            this.tabAnalyzed.TabIndex = 1;
//            this.tabAnalyzed.Text = "Analyzed";
//            this.tabAnalyzed.UseVisualStyleBackColor = true;
//            this.tabAnalyzed.Click += new System.EventHandler(this.tabPage2_Click);
//            // 
//            // lblLengthValue
//            // 
//            this.lblLengthValue.AutoSize = true;
//            this.lblLengthValue.Location = new System.Drawing.Point(375, 7);
//            this.lblLengthValue.Name = "lblLengthValue";
//            this.lblLengthValue.Size = new System.Drawing.Size(13, 13);
//            this.lblLengthValue.TabIndex = 4;
//            this.lblLengthValue.Text = "0";
//            // 
//            // lblLength
//            // 
//            this.lblLength.AutoSize = true;
//            this.lblLength.Location = new System.Drawing.Point(326, 7);
//            this.lblLength.Name = "lblLength";
//            this.lblLength.Size = new System.Drawing.Size(43, 13);
//            this.lblLength.TabIndex = 3;
//            this.lblLength.Text = "Length:";
//            // 
//            // lblOpcodeValue
//            // 
//            this.lblOpcodeValue.AutoSize = true;
//            this.lblOpcodeValue.Location = new System.Drawing.Point(58, 7);
//            this.lblOpcodeValue.Name = "lblOpcodeValue";
//            this.lblOpcodeValue.Size = new System.Drawing.Size(13, 13);
//            this.lblOpcodeValue.TabIndex = 2;
//            this.lblOpcodeValue.Text = "0";
//            // 
//            // lblOpcode
//            // 
//            this.lblOpcode.AutoSize = true;
//            this.lblOpcode.Location = new System.Drawing.Point(4, 7);
//            this.lblOpcode.Name = "lblOpcode";
//            this.lblOpcode.Size = new System.Drawing.Size(48, 13);
//            this.lblOpcode.TabIndex = 1;
//            this.lblOpcode.Text = "Opcode:";
//            // 
//            // txtAnalyzed
//            // 
//            this.txtAnalyzed.Location = new System.Drawing.Point(4, 23);
//            this.txtAnalyzed.MaxLength = 150000;
//            this.txtAnalyzed.Multiline = true;
//            this.txtAnalyzed.Name = "txtAnalyzed";
//            this.txtAnalyzed.ScrollBars = System.Windows.Forms.ScrollBars.Both;
//            this.txtAnalyzed.Size = new System.Drawing.Size(725, 362);
//            this.txtAnalyzed.TabIndex = 0;
//            // 
//            // tabOutput
//            // 
//            this.tabOutput.Controls.Add(this.txtOutput);
//            this.tabOutput.Location = new System.Drawing.Point(4, 22);
//            this.tabOutput.Name = "tabOutput";
//            this.tabOutput.Padding = new System.Windows.Forms.Padding(3);
//            this.tabOutput.Size = new System.Drawing.Size(743, 402);
//            this.tabOutput.TabIndex = 2;
//            this.tabOutput.Text = "Output";
//            this.tabOutput.UseVisualStyleBackColor = true;
//            // 
//            // txtOutput
//            // 
//            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.txtOutput.Location = new System.Drawing.Point(3, 3);
//            this.txtOutput.Multiline = true;
//            this.txtOutput.Name = "txtOutput";
//            this.txtOutput.Size = new System.Drawing.Size(737, 396);
//            this.txtOutput.TabIndex = 0;
//            // 
//            // menuStrip1
//            // 
//            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
//            this.fileToolStripMenuItem,
//            this.packetsToolStripMenuItem});
//            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
//            this.menuStrip1.Name = "menuStrip1";
//            this.menuStrip1.Size = new System.Drawing.Size(751, 24);
//            this.menuStrip1.TabIndex = 2;
//            this.menuStrip1.Text = "menuStrip1";
//            // 
//            // fileToolStripMenuItem
//            // 
//            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
//            this.exitToolStripMenuItem});
//            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
//            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
//            this.fileToolStripMenuItem.Text = "File";
//            // 
//            // exitToolStripMenuItem
//            // 
//            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
//            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
//            this.exitToolStripMenuItem.Text = "E&xit";
//            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
//            // 
//            // packetsToolStripMenuItem
//            // 
//            this.packetsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
//            this.a9ToolStripMenuItem,
//            this.compressedA9ToolStripMenuItem});
//            this.packetsToolStripMenuItem.Name = "packetsToolStripMenuItem";
//            this.packetsToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
//            this.packetsToolStripMenuItem.Text = "Packets";
//            // 
//            // a9ToolStripMenuItem
//            // 
//            this.a9ToolStripMenuItem.Name = "a9ToolStripMenuItem";
//            this.a9ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
//            this.a9ToolStripMenuItem.Text = "A9";
//            this.a9ToolStripMenuItem.Click += new System.EventHandler(this.a9ToolStripMenuItem_Click);
//            // 
//            // compressedA9ToolStripMenuItem
//            // 
//            this.compressedA9ToolStripMenuItem.Name = "compressedA9ToolStripMenuItem";
//            this.compressedA9ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
//            this.compressedA9ToolStripMenuItem.Text = "Compressed A9";
//            this.compressedA9ToolStripMenuItem.Click += new System.EventHandler(this.compressedA9ToolStripMenuItem_Click);
//            // 
//            // MainForm
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(751, 452);
//            this.Controls.Add(this.tabControl);
//            this.Controls.Add(this.menuStrip1);
//            this.Name = "MainForm";
//            this.Text = "Packet Analyzer";
//            this.tabControl.ResumeLayout(false);
//            this.tabRawInput.ResumeLayout(false);
//            this.tabRawInput.PerformLayout();
//            this.tabAnalyzed.ResumeLayout(false);
//            this.tabAnalyzed.PerformLayout();
//            this.tabOutput.ResumeLayout(false);
//            this.tabOutput.PerformLayout();
//            this.menuStrip1.ResumeLayout(false);
//            this.menuStrip1.PerformLayout();
//            this.ResumeLayout(false);
//            this.PerformLayout();

//        }

//        #endregion

//        private System.Windows.Forms.TabControl tabControl;
//        private System.Windows.Forms.TabPage tabRawInput;
//        private System.Windows.Forms.TabPage tabAnalyzed;
//        private System.Windows.Forms.MenuStrip menuStrip1;
//        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
//        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
//        private System.Windows.Forms.TextBox txtRawInput;
//        private System.Windows.Forms.Button btnAnalyze;
//        private System.Windows.Forms.TextBox txtAnalyzed;
//        private System.Windows.Forms.Label lblOpcodeValue;
//        private System.Windows.Forms.Label lblOpcode;
//        private System.Windows.Forms.Label lblLengthValue;
//        private System.Windows.Forms.Label lblLength;
//        private System.Windows.Forms.ToolStripMenuItem packetsToolStripMenuItem;
//        private System.Windows.Forms.ToolStripMenuItem a9ToolStripMenuItem;
//        private System.Windows.Forms.ToolStripMenuItem compressedA9ToolStripMenuItem;
//        private System.Windows.Forms.TabPage tabOutput;
//        private System.Windows.Forms.TextBox txtOutput;

//    }
//}

