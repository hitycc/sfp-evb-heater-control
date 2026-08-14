namespace SFPXFP自动测试软件多端口
{
    partial class ConnectFrm
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
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rBParallel2 = new System.Windows.Forms.RadioButton();
            this.cbBUSB2 = new System.Windows.Forms.ComboBox();
            this.rBUSB2 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rBParallel1 = new System.Windows.Forms.RadioButton();
            this.cbBUSB1 = new System.Windows.Forms.ComboBox();
            this.rBUSB1 = new System.Windows.Forms.RadioButton();
            this.btnCnt1 = new System.Windows.Forms.Button();
            this.btnClose1 = new System.Windows.Forms.Button();
            this.btnCnt2 = new System.Windows.Forms.Button();
            this.btnClose2 = new System.Windows.Forms.Button();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btnClose2);
            this.groupBox4.Controls.Add(this.btnCnt2);
            this.groupBox4.Controls.Add(this.rBParallel2);
            this.groupBox4.Controls.Add(this.cbBUSB2);
            this.groupBox4.Controls.Add(this.rBUSB2);
            this.groupBox4.Location = new System.Drawing.Point(12, 106);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(310, 88);
            this.groupBox4.TabIndex = 106;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "端口2";
            // 
            // rBParallel2
            // 
            this.rBParallel2.AutoSize = true;
            this.rBParallel2.Enabled = false;
            this.rBParallel2.Font = new System.Drawing.Font("宋体", 1
                , System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rBParallel2.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.rBParallel2.Location = new System.Drawing.Point(21, 45);
            this.rBParallel2.Name = "rBParallel2";
            this.rBParallel2.Size = new System.Drawing.Size(53, 18);
            this.rBParallel2.TabIndex = 1;
            this.rBParallel2.Text = "并口";
            this.rBParallel2.UseVisualStyleBackColor = true;
            // 
            // cbBUSB2
            // 
            this.cbBUSB2.FormattingEnabled = true;
            this.cbBUSB2.Location = new System.Drawing.Point(76, 22);
            this.cbBUSB2.Name = "cbBUSB2";
            this.cbBUSB2.Size = new System.Drawing.Size(106, 20);
            this.cbBUSB2.TabIndex = 111;
            // 
            // rBUSB2
            // 
            this.rBUSB2.AutoSize = true;
            this.rBUSB2.Checked = true;
            this.rBUSB2.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rBUSB2.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.rBUSB2.Location = new System.Drawing.Point(21, 21);
            this.rBUSB2.Name = "rBUSB2";
            this.rBUSB2.Size = new System.Drawing.Size(46, 18);
            this.rBUSB2.TabIndex = 0;
            this.rBUSB2.TabStop = true;
            this.rBUSB2.Text = "USB";
            this.rBUSB2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnClose1);
            this.groupBox1.Controls.Add(this.btnCnt1);
            this.groupBox1.Controls.Add(this.rBParallel1);
            this.groupBox1.Controls.Add(this.cbBUSB1);
            this.groupBox1.Controls.Add(this.rBUSB1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(310, 88);
            this.groupBox1.TabIndex = 107;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "端口1";
            // 
            // rBParallel1
            // 
            this.rBParallel1.AutoSize = true;
            this.rBParallel1.Checked = true;
            this.rBParallel1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rBParallel1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.rBParallel1.Location = new System.Drawing.Point(21, 45);
            this.rBParallel1.Name = "rBParallel1";
            this.rBParallel1.Size = new System.Drawing.Size(53, 18);
            this.rBParallel1.TabIndex = 1;
            this.rBParallel1.TabStop = true;
            this.rBParallel1.Text = "并口";
            this.rBParallel1.UseVisualStyleBackColor = true;
            // 
            // cbBUSB1
            // 
            this.cbBUSB1.Enabled = false;
            this.cbBUSB1.FormattingEnabled = true;
            this.cbBUSB1.Location = new System.Drawing.Point(76, 22);
            this.cbBUSB1.Name = "cbBUSB1";
            this.cbBUSB1.Size = new System.Drawing.Size(106, 20);
            this.cbBUSB1.TabIndex = 111;
            // 
            // rBUSB1
            // 
            this.rBUSB1.AutoSize = true;
            this.rBUSB1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rBUSB1.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.rBUSB1.Location = new System.Drawing.Point(21, 21);
            this.rBUSB1.Name = "rBUSB1";
            this.rBUSB1.Size = new System.Drawing.Size(46, 18);
            this.rBUSB1.TabIndex = 0;
            this.rBUSB1.Text = "USB";
            this.rBUSB1.UseVisualStyleBackColor = true;
            // 
            // btnCnt1
            // 
            this.btnCnt1.Location = new System.Drawing.Point(199, 22);
            this.btnCnt1.Name = "btnCnt1";
            this.btnCnt1.Size = new System.Drawing.Size(96, 23);
            this.btnCnt1.TabIndex = 112;
            this.btnCnt1.Text = "连接";
            this.btnCnt1.UseVisualStyleBackColor = true;
            // 
            // btnClose1
            // 
            this.btnClose1.Location = new System.Drawing.Point(199, 51);
            this.btnClose1.Name = "btnClose1";
            this.btnClose1.Size = new System.Drawing.Size(96, 23);
            this.btnClose1.TabIndex = 112;
            this.btnClose1.Text = "关闭";
            this.btnClose1.UseVisualStyleBackColor = true;
            // 
            // btnCnt2
            // 
            this.btnCnt2.Location = new System.Drawing.Point(199, 20);
            this.btnCnt2.Name = "btnCnt2";
            this.btnCnt2.Size = new System.Drawing.Size(96, 23);
            this.btnCnt2.TabIndex = 112;
            this.btnCnt2.Text = "连接";
            this.btnCnt2.UseVisualStyleBackColor = true;
            // 
            // btnClose2
            // 
            this.btnClose2.Location = new System.Drawing.Point(199, 49);
            this.btnClose2.Name = "btnClose2";
            this.btnClose2.Size = new System.Drawing.Size(96, 23);
            this.btnClose2.TabIndex = 112;
            this.btnClose2.Text = "关闭";
            this.btnClose2.UseVisualStyleBackColor = true;
            // 
            // ConnectFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 202);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox1);
            this.Name = "ConnectFrm";
            this.Text = "ConnectFrm";
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rBParallel2;
        private System.Windows.Forms.ComboBox cbBUSB2;
        private System.Windows.Forms.RadioButton rBUSB2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rBParallel1;
        private System.Windows.Forms.ComboBox cbBUSB1;
        private System.Windows.Forms.RadioButton rBUSB1;
        private System.Windows.Forms.Button btnClose1;
        private System.Windows.Forms.Button btnCnt1;
        private System.Windows.Forms.Button btnClose2;
        private System.Windows.Forms.Button btnCnt2;
    }
}