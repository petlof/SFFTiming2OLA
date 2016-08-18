namespace SSFTimingOlaSync
{
    partial class FrmSelectClass
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
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbHeat1 = new System.Windows.Forms.ComboBox();
            this.cmbHeat2 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbHeat3 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Startlist is for class";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(15, 25);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(375, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 127);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(93, 13);
            this.lblInfo.TabIndex = 2;
            this.lblInfo.Text = "Startlist is for class";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(315, 122);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(15, 52);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(77, 17);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "With heats";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Heat 1";
            this.label2.Visible = false;
            // 
            // cmbHeat1
            // 
            this.cmbHeat1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHeat1.FormattingEnabled = true;
            this.cmbHeat1.Location = new System.Drawing.Point(15, 88);
            this.cmbHeat1.Name = "cmbHeat1";
            this.cmbHeat1.Size = new System.Drawing.Size(77, 21);
            this.cmbHeat1.TabIndex = 6;
            this.cmbHeat1.Visible = false;
            // 
            // cmbHeat2
            // 
            this.cmbHeat2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHeat2.FormattingEnabled = true;
            this.cmbHeat2.Location = new System.Drawing.Point(98, 88);
            this.cmbHeat2.Name = "cmbHeat2";
            this.cmbHeat2.Size = new System.Drawing.Size(77, 21);
            this.cmbHeat2.TabIndex = 8;
            this.cmbHeat2.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(95, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Heat 2";
            this.label3.Visible = false;
            // 
            // cmbHeat3
            // 
            this.cmbHeat3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHeat3.FormattingEnabled = true;
            this.cmbHeat3.Location = new System.Drawing.Point(181, 88);
            this.cmbHeat3.Name = "cmbHeat3";
            this.cmbHeat3.Size = new System.Drawing.Size(77, 21);
            this.cmbHeat3.TabIndex = 10;
            this.cmbHeat3.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(178, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Heat 3";
            this.label4.Visible = false;
            // 
            // FrmSelectClass
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 152);
            this.Controls.Add(this.cmbHeat3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbHeat2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbHeat1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FrmSelectClass";
            this.Text = "Startlist for class";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbHeat1;
        private System.Windows.Forms.ComboBox cmbHeat2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbHeat3;
        private System.Windows.Forms.Label label4;
    }
}