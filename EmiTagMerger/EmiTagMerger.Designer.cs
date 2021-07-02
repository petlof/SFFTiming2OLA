namespace EmiTagMerger
{
    partial class EmiTagMerger
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
            this.button12 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.btnRefreshPorts = new System.Windows.Forms.Button();
            this.btnConnectSource = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lstCard1 = new System.Windows.Forms.ListBox();
            this.lblCard1 = new System.Windows.Forms.Label();
            this.lblCard2 = new System.Windows.Forms.Label();
            this.lstCard2 = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lstMerged = new System.Windows.Forms.ListBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnConnectTarget = new System.Windows.Forms.Button();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtEcuTime = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(718, 6);
            this.button12.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(112, 32);
            this.button12.TabIndex = 24;
            this.button12.Text = "Sync";
            this.button12.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(392, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 20);
            this.label2.TabIndex = 23;
            this.label2.Text = "Atomic Time";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(494, 9);
            this.textBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(216, 26);
            this.textBox3.TabIndex = 22;
            // 
            // btnRefreshPorts
            // 
            this.btnRefreshPorts.Location = new System.Drawing.Point(695, 55);
            this.btnRefreshPorts.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnRefreshPorts.Name = "btnRefreshPorts";
            this.btnRefreshPorts.Size = new System.Drawing.Size(145, 32);
            this.btnRefreshPorts.TabIndex = 21;
            this.btnRefreshPorts.Text = "Refresh ports";
            this.btnRefreshPorts.UseVisualStyleBackColor = true;
            this.btnRefreshPorts.Click += new System.EventHandler(this.btnRefreshPorts_Click);
            // 
            // btnConnectSource
            // 
            this.btnConnectSource.Location = new System.Drawing.Point(574, 55);
            this.btnConnectSource.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConnectSource.Name = "btnConnectSource";
            this.btnConnectSource.Size = new System.Drawing.Size(112, 32);
            this.btnConnectSource.TabIndex = 20;
            this.btnConnectSource.Text = "Connect";
            this.btnConnectSource.UseVisualStyleBackColor = true;
            this.btnConnectSource.Click += new System.EventHandler(this.btnConnectSource_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(15, 55);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(548, 28);
            this.comboBox1.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 20);
            this.label1.TabIndex = 25;
            this.label1.Text = "Source Port";
            // 
            // lstCard1
            // 
            this.lstCard1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstCard1.FormattingEnabled = true;
            this.lstCard1.ItemHeight = 20;
            this.lstCard1.Location = new System.Drawing.Point(15, 174);
            this.lstCard1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstCard1.Name = "lstCard1";
            this.lstCard1.Size = new System.Drawing.Size(259, 264);
            this.lstCard1.TabIndex = 26;
            this.lstCard1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // lblCard1
            // 
            this.lblCard1.AutoSize = true;
            this.lblCard1.Location = new System.Drawing.Point(14, 149);
            this.lblCard1.Name = "lblCard1";
            this.lblCard1.Size = new System.Drawing.Size(54, 20);
            this.lblCard1.TabIndex = 28;
            this.lblCard1.Text = "Card1";
            // 
            // lblCard2
            // 
            this.lblCard2.AutoSize = true;
            this.lblCard2.Location = new System.Drawing.Point(294, 149);
            this.lblCard2.Name = "lblCard2";
            this.lblCard2.Size = new System.Drawing.Size(54, 20);
            this.lblCard2.TabIndex = 30;
            this.lblCard2.Text = "Card2";
            // 
            // lstCard2
            // 
            this.lstCard2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstCard2.FormattingEnabled = true;
            this.lstCard2.ItemHeight = 20;
            this.lstCard2.Location = new System.Drawing.Point(295, 174);
            this.lstCard2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstCard2.Name = "lstCard2";
            this.lstCard2.Size = new System.Drawing.Size(268, 264);
            this.lstCard2.TabIndex = 29;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(570, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 20);
            this.label3.TabIndex = 32;
            this.label3.Text = "Merged";
            // 
            // lstMerged
            // 
            this.lstMerged.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstMerged.FormattingEnabled = true;
            this.lstMerged.ItemHeight = 20;
            this.lstMerged.Location = new System.Drawing.Point(572, 174);
            this.lstMerged.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstMerged.Name = "lstMerged";
            this.lstMerged.Size = new System.Drawing.Size(259, 264);
            this.lstMerged.TabIndex = 31;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(606, 464);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(223, 96);
            this.btnSend.TabIndex = 33;
            this.btnSend.Text = "Send Merged Card";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 20);
            this.label4.TabIndex = 37;
            this.label4.Text = "Target Port";
            // 
            // btnConnectTarget
            // 
            this.btnConnectTarget.Location = new System.Drawing.Point(574, 114);
            this.btnConnectTarget.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConnectTarget.Name = "btnConnectTarget";
            this.btnConnectTarget.Size = new System.Drawing.Size(112, 32);
            this.btnConnectTarget.TabIndex = 35;
            this.btnConnectTarget.Text = "Connect";
            this.btnConnectTarget.UseVisualStyleBackColor = true;
            this.btnConnectTarget.Click += new System.EventHandler(this.btnConnectTarget_Click);
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(15, 114);
            this.comboBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(548, 28);
            this.comboBox2.TabIndex = 34;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(76, 11);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(80, 20);
            this.label5.TabIndex = 39;
            this.label5.Text = "Ecu Time";
            // 
            // txtEcuTime
            // 
            this.txtEcuTime.Location = new System.Drawing.Point(159, 9);
            this.txtEcuTime.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEcuTime.Name = "txtEcuTime";
            this.txtEcuTime.Size = new System.Drawing.Size(226, 26);
            this.txtEcuTime.TabIndex = 38;
            // 
            // EmiTagMerger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(843, 577);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtEcuTime);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnConnectTarget);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lstMerged);
            this.Controls.Add(this.lblCard2);
            this.Controls.Add(this.lstCard2);
            this.Controls.Add(this.lblCard1);
            this.Controls.Add(this.lstCard1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.btnRefreshPorts);
            this.Controls.Add(this.btnConnectSource);
            this.Controls.Add(this.comboBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "EmiTagMerger";
            this.Text = "EmiTag Merger";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button btnRefreshPorts;
        private System.Windows.Forms.Button btnConnectSource;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lstCard1;
        private System.Windows.Forms.Label lblCard1;
        private System.Windows.Forms.Label lblCard2;
        private System.Windows.Forms.ListBox lstCard2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lstMerged;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnConnectTarget;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtEcuTime;
    }
}

