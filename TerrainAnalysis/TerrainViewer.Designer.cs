namespace TerrainAnalysis
{
    partial class TerrainViewer
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
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.terrainModeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.obsticleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapListBox = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.upButton = new System.Windows.Forms.Button();
            this.downButton = new System.Windows.Forms.Button();
            this.tileListBox = new System.Windows.Forms.ListBox();
            this.terrainCheck = new System.Windows.Forms.CheckBox();
            this.normalsCheck = new System.Windows.Forms.CheckBox();
            this.chunksInRadiusLabel = new System.Windows.Forms.Label();
            this.loadingLabel = new System.Windows.Forms.Label();
            this.miniMapBox = new System.Windows.Forms.PictureBox();
            this.navigatorBox = new System.Windows.Forms.PictureBox();
            this.currentTileLabel = new System.Windows.Forms.Label();
            this.viewRadiusSlider = new System.Windows.Forms.TrackBar();
            this.sliderRadiusLabel = new System.Windows.Forms.Label();
            this.terrainAnalysisMenu = new System.Windows.Forms.MenuStrip();
            this.applicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.worldToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.terrainModeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectModeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteAllObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteObsticlesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteUnitsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectModeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteObjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyTestNormalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normX = new System.Windows.Forms.TextBox();
            this.normY = new System.Windows.Forms.TextBox();
            this.normZ = new System.Windows.Forms.TextBox();
            this.unitMovementTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.terrainModeContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.miniMapBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.navigatorBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewRadiusSlider)).BeginInit();
            this.terrainAnalysisMenu.SuspendLayout();
            this.objectModeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.terrainModeContextMenu;
            this.pictureBox1.Location = new System.Drawing.Point(2, 29);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(671, 578);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseUp);
            // 
            // terrainModeContextMenu
            // 
            this.terrainModeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addObjectToolStripMenuItem});
            this.terrainModeContextMenu.Name = "viewPortMenu";
            this.terrainModeContextMenu.Size = new System.Drawing.Size(135, 26);
            // 
            // addObjectToolStripMenuItem
            // 
            this.addObjectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unitToolStripMenuItem,
            this.obsticleToolStripMenuItem});
            this.addObjectToolStripMenuItem.Name = "addObjectToolStripMenuItem";
            this.addObjectToolStripMenuItem.Size = new System.Drawing.Size(134, 22);
            this.addObjectToolStripMenuItem.Text = "Add Object";
            // 
            // unitToolStripMenuItem
            // 
            this.unitToolStripMenuItem.Name = "unitToolStripMenuItem";
            this.unitToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.unitToolStripMenuItem.Text = "Unit";
            this.unitToolStripMenuItem.Click += new System.EventHandler(this.unitToolStripMenuItem_Click);
            // 
            // obsticleToolStripMenuItem
            // 
            this.obsticleToolStripMenuItem.Name = "obsticleToolStripMenuItem";
            this.obsticleToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.obsticleToolStripMenuItem.Text = "Obsticle";
            // 
            // mapListBox
            // 
            this.mapListBox.FormattingEnabled = true;
            this.mapListBox.Location = new System.Drawing.Point(679, 29);
            this.mapListBox.Name = "mapListBox";
            this.mapListBox.Size = new System.Drawing.Size(156, 355);
            this.mapListBox.TabIndex = 2;
            this.mapListBox.SelectedIndexChanged += new System.EventHandler(this.MapFile_Changed);
            this.mapListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(679, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Zones";
            // 
            // upButton
            // 
            this.upButton.Location = new System.Drawing.Point(111, 611);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(75, 23);
            this.upButton.TabIndex = 12;
            this.upButton.Text = "Up";
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            // 
            // downButton
            // 
            this.downButton.Location = new System.Drawing.Point(111, 634);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(75, 23);
            this.downButton.TabIndex = 13;
            this.downButton.Text = "Down";
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            // 
            // tileListBox
            // 
            this.tileListBox.FormattingEnabled = true;
            this.tileListBox.Location = new System.Drawing.Point(841, 29);
            this.tileListBox.Name = "tileListBox";
            this.tileListBox.Size = new System.Drawing.Size(158, 355);
            this.tileListBox.TabIndex = 24;
            this.tileListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            // 
            // terrainCheck
            // 
            this.terrainCheck.AutoSize = true;
            this.terrainCheck.Location = new System.Drawing.Point(2, 615);
            this.terrainCheck.Name = "terrainCheck";
            this.terrainCheck.Size = new System.Drawing.Size(97, 17);
            this.terrainCheck.TabIndex = 42;
            this.terrainCheck.Text = "Render Terrain";
            this.terrainCheck.UseVisualStyleBackColor = true;
            this.terrainCheck.CheckedChanged += new System.EventHandler(this.terrainCheck_CheckedChanged);
            this.terrainCheck.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            // 
            // normalsCheck
            // 
            this.normalsCheck.AutoSize = true;
            this.normalsCheck.Location = new System.Drawing.Point(2, 638);
            this.normalsCheck.Name = "normalsCheck";
            this.normalsCheck.Size = new System.Drawing.Size(102, 17);
            this.normalsCheck.TabIndex = 43;
            this.normalsCheck.Text = "Render Normals";
            this.normalsCheck.UseVisualStyleBackColor = true;
            this.normalsCheck.CheckedChanged += new System.EventHandler(this.normalsCheck_CheckedChanged);
            this.normalsCheck.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            // 
            // chunksInRadiusLabel
            // 
            this.chunksInRadiusLabel.AutoSize = true;
            this.chunksInRadiusLabel.Location = new System.Drawing.Point(-1, 669);
            this.chunksInRadiusLabel.Name = "chunksInRadiusLabel";
            this.chunksInRadiusLabel.Size = new System.Drawing.Size(102, 13);
            this.chunksInRadiusLabel.TabIndex = 44;
            this.chunksInRadiusLabel.Text = "Chunks in Radius: 0";
            // 
            // loadingLabel
            // 
            this.loadingLabel.AutoSize = true;
            this.loadingLabel.BackColor = System.Drawing.Color.Black;
            this.loadingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadingLabel.ForeColor = System.Drawing.Color.White;
            this.loadingLabel.Location = new System.Drawing.Point(220, 262);
            this.loadingLabel.Name = "loadingLabel";
            this.loadingLabel.Size = new System.Drawing.Size(255, 25);
            this.loadingLabel.TabIndex = 45;
            this.loadingLabel.Text = "Loading Terrain Data...";
            this.loadingLabel.Visible = false;
            // 
            // miniMapBox
            // 
            this.miniMapBox.Location = new System.Drawing.Point(679, 388);
            this.miniMapBox.Name = "miniMapBox";
            this.miniMapBox.Size = new System.Drawing.Size(320, 320);
            this.miniMapBox.TabIndex = 46;
            this.miniMapBox.TabStop = false;
            this.miniMapBox.Click += new System.EventHandler(this.MiniMap_Click);
            this.miniMapBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MiniMap_MouseClick);
            this.miniMapBox.Paint += new System.Windows.Forms.PaintEventHandler(this.MiniMap_Paint);
            // 
            // navigatorBox
            // 
            this.navigatorBox.Location = new System.Drawing.Point(573, 608);
            this.navigatorBox.Name = "navigatorBox";
            this.navigatorBox.Size = new System.Drawing.Size(100, 100);
            this.navigatorBox.TabIndex = 47;
            this.navigatorBox.TabStop = false;
            this.navigatorBox.Paint += new System.Windows.Forms.PaintEventHandler(this.Navigation_Paint);
            // 
            // currentTileLabel
            // 
            this.currentTileLabel.AutoSize = true;
            this.currentTileLabel.Location = new System.Drawing.Point(-1, 693);
            this.currentTileLabel.Name = "currentTileLabel";
            this.currentTileLabel.Size = new System.Drawing.Size(67, 13);
            this.currentTileLabel.TabIndex = 48;
            this.currentTileLabel.Text = "Current Tile: ";
            // 
            // viewRadiusSlider
            // 
            this.viewRadiusSlider.Location = new System.Drawing.Point(192, 634);
            this.viewRadiusSlider.Name = "viewRadiusSlider";
            this.viewRadiusSlider.Size = new System.Drawing.Size(210, 45);
            this.viewRadiusSlider.TabIndex = 49;
            this.viewRadiusSlider.Scroll += new System.EventHandler(this.viewRadiusSlider_Scroll);
            this.viewRadiusSlider.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            // 
            // sliderRadiusLabel
            // 
            this.sliderRadiusLabel.AutoSize = true;
            this.sliderRadiusLabel.Location = new System.Drawing.Point(193, 615);
            this.sliderRadiusLabel.Name = "sliderRadiusLabel";
            this.sliderRadiusLabel.Size = new System.Drawing.Size(72, 13);
            this.sliderRadiusLabel.TabIndex = 50;
            this.sliderRadiusLabel.Text = "View Radius: ";
            // 
            // terrainAnalysisMenu
            // 
            this.terrainAnalysisMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applicationToolStripMenuItem,
            this.worldToolStripMenuItem});
            this.terrainAnalysisMenu.Location = new System.Drawing.Point(0, 0);
            this.terrainAnalysisMenu.Name = "terrainAnalysisMenu";
            this.terrainAnalysisMenu.Size = new System.Drawing.Size(999, 24);
            this.terrainAnalysisMenu.TabIndex = 52;
            this.terrainAnalysisMenu.Text = "menuStrip1";
            // 
            // applicationToolStripMenuItem
            // 
            this.applicationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitMenuItem});
            this.applicationToolStripMenuItem.Name = "applicationToolStripMenuItem";
            this.applicationToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.applicationToolStripMenuItem.Text = "Application";
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // worldToolStripMenuItem
            // 
            this.worldToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.terrainModeMenuItem,
            this.objectModeMenuItem,
            this.toolStripSeparator1,
            this.deleteAllObjectsToolStripMenuItem,
            this.deleteObsticlesToolStripMenuItem,
            this.deleteUnitsToolStripMenuItem});
            this.worldToolStripMenuItem.Name = "worldToolStripMenuItem";
            this.worldToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.worldToolStripMenuItem.Text = "World";
            // 
            // terrainModeMenuItem
            // 
            this.terrainModeMenuItem.CheckOnClick = true;
            this.terrainModeMenuItem.Name = "terrainModeMenuItem";
            this.terrainModeMenuItem.Size = new System.Drawing.Size(167, 22);
            this.terrainModeMenuItem.Text = "Terrain Mode";
            this.terrainModeMenuItem.Click += new System.EventHandler(this.terrainModeMenuItem_Click);
            // 
            // objectModeMenuItem
            // 
            this.objectModeMenuItem.CheckOnClick = true;
            this.objectModeMenuItem.Name = "objectModeMenuItem";
            this.objectModeMenuItem.Size = new System.Drawing.Size(167, 22);
            this.objectModeMenuItem.Text = "Object Mode";
            this.objectModeMenuItem.Click += new System.EventHandler(this.objectModeMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(164, 6);
            // 
            // deleteAllObjectsToolStripMenuItem
            // 
            this.deleteAllObjectsToolStripMenuItem.Name = "deleteAllObjectsToolStripMenuItem";
            this.deleteAllObjectsToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.deleteAllObjectsToolStripMenuItem.Text = "Delete All Objects";
            // 
            // deleteObsticlesToolStripMenuItem
            // 
            this.deleteObsticlesToolStripMenuItem.Name = "deleteObsticlesToolStripMenuItem";
            this.deleteObsticlesToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.deleteObsticlesToolStripMenuItem.Text = "Delete Obsticles";
            // 
            // deleteUnitsToolStripMenuItem
            // 
            this.deleteUnitsToolStripMenuItem.Name = "deleteUnitsToolStripMenuItem";
            this.deleteUnitsToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.deleteUnitsToolStripMenuItem.Text = "Delete Units";
            // 
            // objectModeContextMenu
            // 
            this.objectModeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectObjectToolStripMenuItem,
            this.deleteObjectToolStripMenuItem,
            this.toolStripSeparator2,
            this.actionsToolStripMenuItem});
            this.objectModeContextMenu.Name = "objectModeContextMenu";
            this.objectModeContextMenu.Size = new System.Drawing.Size(146, 76);
            // 
            // selectObjectToolStripMenuItem
            // 
            this.selectObjectToolStripMenuItem.Name = "selectObjectToolStripMenuItem";
            this.selectObjectToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.selectObjectToolStripMenuItem.Text = "Select Object";
            this.selectObjectToolStripMenuItem.Click += new System.EventHandler(this.selectObjectToolStripMenuItem_Click);
            // 
            // deleteObjectToolStripMenuItem
            // 
            this.deleteObjectToolStripMenuItem.Name = "deleteObjectToolStripMenuItem";
            this.deleteObjectToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.deleteObjectToolStripMenuItem.Text = "Delete Object";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(142, 6);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveToolStripMenuItem,
            this.rotateToolStripMenuItem,
            this.applyTestNormalToolStripMenuItem});
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.actionsToolStripMenuItem.Text = "Actions";
            // 
            // moveToolStripMenuItem
            // 
            this.moveToolStripMenuItem.Name = "moveToolStripMenuItem";
            this.moveToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.moveToolStripMenuItem.Text = "Move";
            this.moveToolStripMenuItem.Click += new System.EventHandler(this.moveToolStripMenuItem_Click);
            // 
            // rotateToolStripMenuItem
            // 
            this.rotateToolStripMenuItem.Name = "rotateToolStripMenuItem";
            this.rotateToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.rotateToolStripMenuItem.Text = "Rotate";
            this.rotateToolStripMenuItem.Click += new System.EventHandler(this.rotateToolStripMenuItem_Click);
            // 
            // applyTestNormalToolStripMenuItem
            // 
            this.applyTestNormalToolStripMenuItem.Name = "applyTestNormalToolStripMenuItem";
            this.applyTestNormalToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.applyTestNormalToolStripMenuItem.Text = "Apply Test Normal";
            this.applyTestNormalToolStripMenuItem.Click += new System.EventHandler(this.applyTestNormalToolStripMenuItem_Click);
            // 
            // normX
            // 
            this.normX.Location = new System.Drawing.Point(196, 685);
            this.normX.Name = "normX";
            this.normX.Size = new System.Drawing.Size(27, 20);
            this.normX.TabIndex = 53;
            // 
            // normY
            // 
            this.normY.Location = new System.Drawing.Point(229, 685);
            this.normY.Name = "normY";
            this.normY.Size = new System.Drawing.Size(27, 20);
            this.normY.TabIndex = 54;
            // 
            // normZ
            // 
            this.normZ.Location = new System.Drawing.Point(262, 685);
            this.normZ.Name = "normZ";
            this.normZ.Size = new System.Drawing.Size(27, 20);
            this.normZ.TabIndex = 55;
            // 
            // unitMovementTimer
            // 
            this.unitMovementTimer.Enabled = true;
            this.unitMovementTimer.Interval = 25;
            this.unitMovementTimer.Tick += new System.EventHandler(this.unitMovementTimer_Tick);
            // 
            // TerrainViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(999, 709);
            this.Controls.Add(this.normZ);
            this.Controls.Add(this.normY);
            this.Controls.Add(this.normX);
            this.Controls.Add(this.terrainAnalysisMenu);
            this.Controls.Add(this.sliderRadiusLabel);
            this.Controls.Add(this.viewRadiusSlider);
            this.Controls.Add(this.currentTileLabel);
            this.Controls.Add(this.miniMapBox);
            this.Controls.Add(this.navigatorBox);
            this.Controls.Add(this.loadingLabel);
            this.Controls.Add(this.chunksInRadiusLabel);
            this.Controls.Add(this.normalsCheck);
            this.Controls.Add(this.terrainCheck);
            this.Controls.Add(this.tileListBox);
            this.Controls.Add(this.downButton);
            this.Controls.Add(this.upButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.mapListBox);
            this.Controls.Add(this.pictureBox1);
            this.MainMenuStrip = this.terrainAnalysisMenu;
            this.Name = "TerrainViewer";
            this.Text = "ADT Terrain Analysis";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Key_Down);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.terrainModeContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.miniMapBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.navigatorBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.viewRadiusSlider)).EndInit();
            this.terrainAnalysisMenu.ResumeLayout(false);
            this.terrainAnalysisMenu.PerformLayout();
            this.objectModeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ListBox mapListBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.ListBox tileListBox;
        private System.Windows.Forms.CheckBox terrainCheck;
        private System.Windows.Forms.CheckBox normalsCheck;
        private System.Windows.Forms.Label chunksInRadiusLabel;
        private System.Windows.Forms.Label loadingLabel;
        private System.Windows.Forms.PictureBox miniMapBox;
        private System.Windows.Forms.PictureBox navigatorBox;
        private System.Windows.Forms.Label currentTileLabel;
        private System.Windows.Forms.TrackBar viewRadiusSlider;
        private System.Windows.Forms.Label sliderRadiusLabel;
        private System.Windows.Forms.ContextMenuStrip terrainModeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem addObjectToolStripMenuItem;
        private System.Windows.Forms.MenuStrip terrainAnalysisMenu;
        private System.Windows.Forms.ToolStripMenuItem applicationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem worldToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem terrainModeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectModeMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAllObjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteObsticlesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteUnitsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem unitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem obsticleToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip objectModeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem selectObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteObjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rotateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyTestNormalToolStripMenuItem;
        private System.Windows.Forms.TextBox normX;
        private System.Windows.Forms.TextBox normY;
        private System.Windows.Forms.TextBox normZ;
        private System.Windows.Forms.Timer unitMovementTimer;

    }
}

