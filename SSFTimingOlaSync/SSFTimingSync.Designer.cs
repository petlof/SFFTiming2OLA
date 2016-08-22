namespace SSFTimingOlaSync
{
    partial class SSFTimingSync
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmbSSF = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbOLA = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startlistsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importStartlistToOLAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createXMLimportFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createSSFTimingStartlistFromOLAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createSSFTimingRelaystartlistFromOLAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createREsultsFromOLAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assignCardstorelayPersonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmbRace = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "SSFTiming-Event";
            // 
            // cmbSSF
            // 
            this.cmbSSF.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSSF.FormattingEnabled = true;
            this.cmbSSF.Location = new System.Drawing.Point(15, 40);
            this.cmbSSF.Name = "cmbSSF";
            this.cmbSSF.Size = new System.Drawing.Size(288, 21);
            this.cmbSSF.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "OLA-Event";
            // 
            // cmbOLA
            // 
            this.cmbOLA.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOLA.FormattingEnabled = true;
            this.cmbOLA.Location = new System.Drawing.Point(15, 80);
            this.cmbOLA.Name = "cmbOLA";
            this.cmbOLA.Size = new System.Drawing.Size(288, 21);
            this.cmbOLA.TabIndex = 3;
            this.cmbOLA.SelectedIndexChanged += new System.EventHandler(this.cmbOLA_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 147);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(96, 147);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(318, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Log";
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(321, 40);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(417, 225);
            this.listBox1.TabIndex = 7;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 176);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(93, 17);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Use starttime2";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.startlistsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(750, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "&Exit";
            // 
            // startlistsToolStripMenuItem
            // 
            this.startlistsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importStartlistToOLAToolStripMenuItem,
            this.createSSFTimingStartlistFromOLAToolStripMenuItem,
            this.createSSFTimingRelaystartlistFromOLAToolStripMenuItem,
            this.createREsultsFromOLAToolStripMenuItem,
            this.assignCardstorelayPersonsToolStripMenuItem});
            this.startlistsToolStripMenuItem.Name = "startlistsToolStripMenuItem";
            this.startlistsToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.startlistsToolStripMenuItem.Text = "&Startlists";
            // 
            // importStartlistToOLAToolStripMenuItem
            // 
            this.importStartlistToOLAToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createXMLimportFileToolStripMenuItem});
            this.importStartlistToOLAToolStripMenuItem.Name = "importStartlistToOLAToolStripMenuItem";
            this.importStartlistToOLAToolStripMenuItem.Size = new System.Drawing.Size(288, 22);
            this.importStartlistToOLAToolStripMenuItem.Text = "IOF 2 OLA";
            // 
            // createXMLimportFileToolStripMenuItem
            // 
            this.createXMLimportFileToolStripMenuItem.Name = "createXMLimportFileToolStripMenuItem";
            this.createXMLimportFileToolStripMenuItem.Size = new System.Drawing.Size(195, 22);
            this.createXMLimportFileToolStripMenuItem.Text = "Create XML-import file";
            this.createXMLimportFileToolStripMenuItem.Click += new System.EventHandler(this.createXMLimportFileToolStripMenuItem_Click);
            // 
            // createSSFTimingStartlistFromOLAToolStripMenuItem
            // 
            this.createSSFTimingStartlistFromOLAToolStripMenuItem.Name = "createSSFTimingStartlistFromOLAToolStripMenuItem";
            this.createSSFTimingStartlistFromOLAToolStripMenuItem.Size = new System.Drawing.Size(288, 22);
            this.createSSFTimingStartlistFromOLAToolStripMenuItem.Text = "Create SSFTiming startlist from OLA";
            this.createSSFTimingStartlistFromOLAToolStripMenuItem.Click += new System.EventHandler(this.createSSFTimingStartlistFromOLAToolStripMenuItem_Click);
            // 
            // createSSFTimingRelaystartlistFromOLAToolStripMenuItem
            // 
            this.createSSFTimingRelaystartlistFromOLAToolStripMenuItem.Name = "createSSFTimingRelaystartlistFromOLAToolStripMenuItem";
            this.createSSFTimingRelaystartlistFromOLAToolStripMenuItem.Size = new System.Drawing.Size(288, 22);
            this.createSSFTimingRelaystartlistFromOLAToolStripMenuItem.Text = "Create SSFTiming relaystartlist from OLA";
            this.createSSFTimingRelaystartlistFromOLAToolStripMenuItem.Click += new System.EventHandler(this.createSSFTimingRelaystartlistFromOLAToolStripMenuItem_Click);
            // 
            // createREsultsFromOLAToolStripMenuItem
            // 
            this.createREsultsFromOLAToolStripMenuItem.Name = "createREsultsFromOLAToolStripMenuItem";
            this.createREsultsFromOLAToolStripMenuItem.Size = new System.Drawing.Size(288, 22);
            this.createREsultsFromOLAToolStripMenuItem.Text = "Create REsultsFromOLA";
            this.createREsultsFromOLAToolStripMenuItem.Click += new System.EventHandler(this.createREsultsFromOLAToolStripMenuItem_Click);
            // 
            // assignCardstorelayPersonsToolStripMenuItem
            // 
            this.assignCardstorelayPersonsToolStripMenuItem.Name = "assignCardstorelayPersonsToolStripMenuItem";
            this.assignCardstorelayPersonsToolStripMenuItem.Size = new System.Drawing.Size(288, 22);
            this.assignCardstorelayPersonsToolStripMenuItem.Text = "Assign cardstorelayPersons";
            this.assignCardstorelayPersonsToolStripMenuItem.Click += new System.EventHandler(this.assignCardstorelayPersonsToolStripMenuItem_Click);
            // 
            // cmbRace
            // 
            this.cmbRace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRace.FormattingEnabled = true;
            this.cmbRace.Location = new System.Drawing.Point(15, 120);
            this.cmbRace.Name = "cmbRace";
            this.cmbRace.Size = new System.Drawing.Size(288, 21);
            this.cmbRace.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "OLA-Race";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(15, 199);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(64, 17);
            this.checkBox2.TabIndex = 12;
            this.checkBox2.Text = "Is Relay";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // SSFTimingSync
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 262);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.cmbRace);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmbOLA);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbSSF);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SSFTimingSync";
            this.Text = "SSFTimingSync";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SSFTimingSync_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSSF;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbOLA;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox listBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startlistsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importStartlistToOLAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createXMLimportFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createSSFTimingStartlistFromOLAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createSSFTimingRelaystartlistFromOLAToolStripMenuItem;
        private System.Windows.Forms.ComboBox cmbRace;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripMenuItem createREsultsFromOLAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assignCardstorelayPersonsToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}

