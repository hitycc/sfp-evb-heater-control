namespace XFP模块测试程序
{
    partial class DSPRegDebugForm
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
            this.checkBox_ByteSelect = new System.Windows.Forms.CheckBox();
            this.button_RegRead = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_debug = new System.Windows.Forms.CheckBox();
            this.textBox_trackBar_val = new System.Windows.Forms.TextBox();
            this.trackBar_Reg = new System.Windows.Forms.TrackBar();
            this.tBReg_DataLTH = new System.Windows.Forms.TextBox();
            this.tBRegLSB = new System.Windows.Forms.TextBox();
            this.tBRegMSB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tBDevAddr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRead = new System.Windows.Forms.Button();
            this.columnHeader17 = new System.Windows.Forms.ColumnHeader();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader10 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader11 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader12 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader13 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader14 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader15 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader16 = new System.Windows.Forms.ColumnHeader();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Reg)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBox_ByteSelect
            // 
            this.checkBox_ByteSelect.AutoSize = true;
            this.checkBox_ByteSelect.Checked = true;
            this.checkBox_ByteSelect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_ByteSelect.Location = new System.Drawing.Point(386, 111);
            this.checkBox_ByteSelect.Name = "checkBox_ByteSelect";
            this.checkBox_ByteSelect.Size = new System.Drawing.Size(60, 16);
            this.checkBox_ByteSelect.TabIndex = 15;
            this.checkBox_ByteSelect.Text = "单字节";
            this.checkBox_ByteSelect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox_ByteSelect.UseVisualStyleBackColor = true;
            // 
            // button_RegRead
            // 
            this.button_RegRead.Location = new System.Drawing.Point(323, 102);
            this.button_RegRead.Name = "button_RegRead";
            this.button_RegRead.Size = new System.Drawing.Size(61, 32);
            this.button_RegRead.TabIndex = 14;
            this.button_RegRead.Text = "Read";
            this.button_RegRead.UseVisualStyleBackColor = true;
            this.button_RegRead.Click += new System.EventHandler(this.button_RegRead_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_ByteSelect);
            this.groupBox1.Controls.Add(this.button_RegRead);
            this.groupBox1.Controls.Add(this.checkBox_debug);
            this.groupBox1.Controls.Add(this.textBox_trackBar_val);
            this.groupBox1.Controls.Add(this.trackBar_Reg);
            this.groupBox1.Controls.Add(this.tBReg_DataLTH);
            this.groupBox1.Controls.Add(this.tBRegLSB);
            this.groupBox1.Controls.Add(this.tBRegMSB);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tBDevAddr);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.btnRead);
            this.groupBox1.Location = new System.Drawing.Point(0, 207);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(595, 142);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DSP寄存器调试";
            // 
            // checkBox_debug
            // 
            this.checkBox_debug.AutoSize = true;
            this.checkBox_debug.Location = new System.Drawing.Point(386, 79);
            this.checkBox_debug.Name = "checkBox_debug";
            this.checkBox_debug.Size = new System.Drawing.Size(48, 16);
            this.checkBox_debug.TabIndex = 12;
            this.checkBox_debug.Text = "调试";
            this.checkBox_debug.UseVisualStyleBackColor = true;
            this.checkBox_debug.CheckedChanged += new System.EventHandler(this.checkBox_debug_CheckedChanged);
            // 
            // textBox_trackBar_val
            // 
            this.textBox_trackBar_val.Location = new System.Drawing.Point(323, 74);
            this.textBox_trackBar_val.Name = "textBox_trackBar_val";
            this.textBox_trackBar_val.Size = new System.Drawing.Size(57, 21);
            this.textBox_trackBar_val.TabIndex = 11;
            this.textBox_trackBar_val.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox_trackBar_val.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_trackBar_val_KeyDown);
            // 
            // trackBar_Reg
            // 
            this.trackBar_Reg.Location = new System.Drawing.Point(6, 75);
            this.trackBar_Reg.Maximum = 255;
            this.trackBar_Reg.Name = "trackBar_Reg";
            this.trackBar_Reg.Size = new System.Drawing.Size(311, 45);
            this.trackBar_Reg.TabIndex = 10;
            this.trackBar_Reg.TickFrequency = 64;
            this.trackBar_Reg.ValueChanged += new System.EventHandler(this.trackBar_Reg_ValueChanged);
            // 
            // tBReg_DataLTH
            // 
            this.tBReg_DataLTH.Location = new System.Drawing.Point(420, 27);
            this.tBReg_DataLTH.Name = "tBReg_DataLTH";
            this.tBReg_DataLTH.Size = new System.Drawing.Size(50, 21);
            this.tBReg_DataLTH.TabIndex = 8;
            this.tBReg_DataLTH.Text = "00";
            this.tBReg_DataLTH.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tBRegLSB
            // 
            this.tBRegLSB.Location = new System.Drawing.Point(238, 27);
            this.tBRegLSB.Name = "tBRegLSB";
            this.tBRegLSB.Size = new System.Drawing.Size(49, 21);
            this.tBRegLSB.TabIndex = 8;
            this.tBRegLSB.Text = "00";
            this.tBRegLSB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tBRegMSB
            // 
            this.tBRegMSB.Location = new System.Drawing.Point(188, 27);
            this.tBRegMSB.Name = "tBRegMSB";
            this.tBRegMSB.Size = new System.Drawing.Size(49, 21);
            this.tBRegMSB.TabIndex = 8;
            this.tBRegMSB.Text = "00";
            this.tBRegMSB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(373, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "Length";
            // 
            // tBDevAddr
            // 
            this.tBDevAddr.Location = new System.Drawing.Point(58, 27);
            this.tBDevAddr.Name = "tBDevAddr";
            this.tBDevAddr.Size = new System.Drawing.Size(50, 21);
            this.tBDevAddr.TabIndex = 8;
            this.tBDevAddr.Text = "00";
            this.tBDevAddr.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(159, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "Reg";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "DevAddr";
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(485, 16);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(104, 53);
            this.btnRead.TabIndex = 6;
            this.btnRead.Text = "读取";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "0F";
            this.columnHeader17.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader17.Width = 35;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12,
            this.columnHeader13,
            this.columnHeader14,
            this.columnHeader15,
            this.columnHeader16,
            this.columnHeader17});
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(595, 201);
            this.listView1.TabIndex = 11;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = " ";
            this.columnHeader1.Width = 35;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "00";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 35;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "01";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader3.Width = 31;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "02";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader4.Width = 35;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "03";
            this.columnHeader5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader5.Width = 35;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "04";
            this.columnHeader6.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader6.Width = 35;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "05";
            this.columnHeader7.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader7.Width = 35;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "06";
            this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader8.Width = 35;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "07";
            this.columnHeader9.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader9.Width = 35;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "08";
            this.columnHeader10.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader10.Width = 35;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "09";
            this.columnHeader11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader11.Width = 35;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "0A";
            this.columnHeader12.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader12.Width = 35;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "0B";
            this.columnHeader13.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader13.Width = 35;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "0C";
            this.columnHeader14.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader14.Width = 35;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "0D";
            this.columnHeader15.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader15.Width = 35;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "0E";
            this.columnHeader16.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader16.Width = 35;
            // 
            // DSPRegDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 348);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView1);
            this.Name = "DSPRegDebugForm";
            this.Text = "DSPRegDebugForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Reg)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_ByteSelect;
        private System.Windows.Forms.Button button_RegRead;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_debug;
        private System.Windows.Forms.TextBox textBox_trackBar_val;
        private System.Windows.Forms.TrackBar trackBar_Reg;
        private System.Windows.Forms.TextBox tBReg_DataLTH;
        private System.Windows.Forms.TextBox tBRegLSB;
        private System.Windows.Forms.TextBox tBRegMSB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tBDevAddr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.ColumnHeader columnHeader16;
    }
}