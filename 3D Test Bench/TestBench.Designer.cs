namespace _3D_Test_Bench
{
    partial class TestBench
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Sand Box Root");
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.worldObjects = new System.Windows.Forms.TreeView();
            this.availableObjects = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(737, 563);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // worldObjects
            // 
            this.worldObjects.Location = new System.Drawing.Point(743, 3);
            this.worldObjects.Name = "worldObjects";
            treeNode1.Name = "Root";
            treeNode1.Text = "Sand Box Root";
            this.worldObjects.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.worldObjects.Size = new System.Drawing.Size(236, 298);
            this.worldObjects.TabIndex = 1;
            // 
            // availableObjects
            // 
            this.availableObjects.FormattingEnabled = true;
            this.availableObjects.Location = new System.Drawing.Point(743, 324);
            this.availableObjects.Name = "availableObjects";
            this.availableObjects.Size = new System.Drawing.Size(236, 238);
            this.availableObjects.TabIndex = 2;
            // 
            // TestBench
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 732);
            this.Controls.Add(this.availableObjects);
            this.Controls.Add(this.worldObjects);
            this.Controls.Add(this.pictureBox1);
            this.Name = "TestBench";
            this.Text = "3D Test Bench";
            this.Load += new System.EventHandler(this.TestBench_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TreeView worldObjects;
        private System.Windows.Forms.ListBox availableObjects;
    }
}

