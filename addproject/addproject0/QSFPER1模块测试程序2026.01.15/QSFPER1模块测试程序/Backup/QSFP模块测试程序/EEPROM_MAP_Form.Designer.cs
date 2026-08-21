namespace XFP模块测试程序
{
    partial class EEPROM_MAP_Form
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
            this.checkBox_debug = new System.Windows.Forms.CheckBox();
            this.textBox_trackBar_val = new System.Windows.Forms.TextBox();
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
            this.columnHeader17 = new System.Windows.Forms.ColumnHeader();
            this.checkBox_ByteSelect = new System.Windows.Forms.CheckBox();
            this.button_RegRead = new System.Windows.Forms.Button();
            this.trackBar_Reg = new System.Windows.Forms.TrackBar();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.button_PWD = new System.Windows.Forms.Button();
            this.label42 = new System.Windows.Forms.Label();
            this.textBox_RegVal1 = new System.Windows.Forms.TextBox();
            this.textBox_PW03 = new System.Windows.Forms.TextBox();
            this.textBox_PW02 = new System.Windows.Forms.TextBox();
            this.textBox_PW01 = new System.Windows.Forms.TextBox();
            this.textBox_PW00 = new System.Windows.Forms.TextBox();
            this.label46 = new System.Windows.Forms.Label();
            this.textBox_Page = new System.Windows.Forms.TextBox();
            this.btnSaveFile = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rBPageSelect = new System.Windows.Forms.RadioButton();
            this.rBLowerPage = new System.Windows.Forms.RadioButton();
            this.tBPage = new System.Windows.Forms.TextBox();
            this.BtnRead = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Reg)).BeginInit();
            this.groupBox6.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_debug
            // 
            this.checkBox_debug.AutoSize = true;
            this.checkBox_debug.Location = new System.Drawing.Point(386, 61);
            this.checkBox_debug.Name = "checkBox_debug";
            this.checkBox_debug.Size = new System.Drawing.Size(48, 16);
            this.checkBox_debug.TabIndex = 9;
            this.checkBox_debug.Text = "调试";
            this.checkBox_debug.UseVisualStyleBackColor = true;
            this.checkBox_debug.CheckedChanged += new System.EventHandler(this.checkBox_debug_CheckedChanged_1);
            // 
            // textBox_trackBar_val
            // 
            this.textBox_trackBar_val.Location = new System.Drawing.Point(323, 56);
            this.textBox_trackBar_val.Name = "textBox_trackBar_val";
            this.textBox_trackBar_val.Size = new System.Drawing.Size(57, 21);
            this.textBox_trackBar_val.TabIndex = 8;
            this.textBox_trackBar_val.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox_trackBar_val.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_trackBar_val_KeyDown);
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
            this.listView1.Location = new System.Drawing.Point(1, 1);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(601, 201);
            this.listView1.TabIndex = 100;
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
            this.columnHeader3.Width = 35;
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
            // columnHeader17
            // 
            this.columnHeader17.Text = "0F";
            this.columnHeader17.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader17.Width = 35;
            // 
            // checkBox_ByteSelect
            // 
            this.checkBox_ByteSelect.AutoSize = true;
            this.checkBox_ByteSelect.Checked = true;
            this.checkBox_ByteSelect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_ByteSelect.Location = new System.Drawing.Point(140, 108);
            this.checkBox_ByteSelect.Name = "checkBox_ByteSelect";
            this.checkBox_ByteSelect.Size = new System.Drawing.Size(60, 16);
            this.checkBox_ByteSelect.TabIndex = 7;
            this.checkBox_ByteSelect.Text = "单字节";
            this.checkBox_ByteSelect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox_ByteSelect.UseVisualStyleBackColor = true;
            this.checkBox_ByteSelect.CheckedChanged += new System.EventHandler(this.checkBox_debug_CheckedChanged);
            // 
            // button_RegRead
            // 
            this.button_RegRead.Location = new System.Drawing.Point(464, 58);
            this.button_RegRead.Name = "button_RegRead";
            this.button_RegRead.Size = new System.Drawing.Size(107, 39);
            this.button_RegRead.TabIndex = 6;
            this.button_RegRead.Text = "Read";
            this.button_RegRead.UseVisualStyleBackColor = true;
            this.button_RegRead.Click += new System.EventHandler(this.button_RegRead_Click);
            // 
            // trackBar_Reg
            // 
            this.trackBar_Reg.Enabled = false;
            this.trackBar_Reg.Location = new System.Drawing.Point(6, 57);
            this.trackBar_Reg.Maximum = 255;
            this.trackBar_Reg.Name = "trackBar_Reg";
            this.trackBar_Reg.Size = new System.Drawing.Size(311, 45);
            this.trackBar_Reg.TabIndex = 1;
            this.trackBar_Reg.TickFrequency = 64;
            this.trackBar_Reg.ValueChanged += new System.EventHandler(this.trackBar_Reg_ValueChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.checkBox_debug);
            this.groupBox6.Controls.Add(this.textBox_trackBar_val);
            this.groupBox6.Controls.Add(this.checkBox_ByteSelect);
            this.groupBox6.Controls.Add(this.button_RegRead);
            this.groupBox6.Controls.Add(this.trackBar_Reg);
            this.groupBox6.Controls.Add(this.button_PWD);
            this.groupBox6.Controls.Add(this.label42);
            this.groupBox6.Controls.Add(this.textBox_RegVal1);
            this.groupBox6.Controls.Add(this.textBox_PW03);
            this.groupBox6.Controls.Add(this.textBox_PW02);
            this.groupBox6.Controls.Add(this.textBox_PW01);
            this.groupBox6.Controls.Add(this.textBox_PW00);
            this.groupBox6.Controls.Add(this.label46);
            this.groupBox6.Controls.Add(this.textBox_Page);
            this.groupBox6.Location = new System.Drawing.Point(2, 284);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(596, 134);
            this.groupBox6.TabIndex = 101;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "LDD/LA 寄存器调试";
            // 
            // button_PWD
            // 
            this.button_PWD.Location = new System.Drawing.Point(172, 12);
            this.button_PWD.Name = "button_PWD";
            this.button_PWD.Size = new System.Drawing.Size(78, 39);
            this.button_PWD.TabIndex = 4;
            this.button_PWD.Text = "Write";
            this.button_PWD.UseVisualStyleBackColor = true;
            this.button_PWD.Click += new System.EventHandler(this.button_PWD_Click);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(6, 24);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(23, 12);
            this.label42.TabIndex = 3;
            this.label42.Text = "PWD";
            // 
            // textBox_RegVal1
            // 
            this.textBox_RegVal1.Location = new System.Drawing.Point(81, 105);
            this.textBox_RegVal1.Name = "textBox_RegVal1";
            this.textBox_RegVal1.Size = new System.Drawing.Size(50, 21);
            this.textBox_RegVal1.TabIndex = 2;
            this.textBox_RegVal1.Text = "80";
            this.textBox_RegVal1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_PW03
            // 
            this.textBox_PW03.Location = new System.Drawing.Point(133, 19);
            this.textBox_PW03.Name = "textBox_PW03";
            this.textBox_PW03.Size = new System.Drawing.Size(33, 21);
            this.textBox_PW03.TabIndex = 2;
            this.textBox_PW03.Text = "54";
            this.textBox_PW03.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_PW02
            // 
            this.textBox_PW02.Location = new System.Drawing.Point(99, 19);
            this.textBox_PW02.Name = "textBox_PW02";
            this.textBox_PW02.Size = new System.Drawing.Size(33, 21);
            this.textBox_PW02.TabIndex = 2;
            this.textBox_PW02.Text = "50";
            this.textBox_PW02.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_PW01
            // 
            this.textBox_PW01.Location = new System.Drawing.Point(65, 19);
            this.textBox_PW01.Name = "textBox_PW01";
            this.textBox_PW01.Size = new System.Drawing.Size(33, 21);
            this.textBox_PW01.TabIndex = 2;
            this.textBox_PW01.Text = "46";
            this.textBox_PW01.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox_PW00
            // 
            this.textBox_PW00.Location = new System.Drawing.Point(31, 19);
            this.textBox_PW00.Name = "textBox_PW00";
            this.textBox_PW00.Size = new System.Drawing.Size(33, 21);
            this.textBox_PW00.TabIndex = 2;
            this.textBox_PW00.Text = "A9";
            this.textBox_PW00.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(13, 109);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(29, 12);
            this.label46.TabIndex = 1;
            this.label46.Text = "Page";
            // 
            // textBox_Page
            // 
            this.textBox_Page.Location = new System.Drawing.Point(41, 105);
            this.textBox_Page.Name = "textBox_Page";
            this.textBox_Page.Size = new System.Drawing.Size(33, 21);
            this.textBox_Page.TabIndex = 0;
            this.textBox_Page.Text = "B0";
            this.textBox_Page.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnSaveFile
            // 
            this.btnSaveFile.Location = new System.Drawing.Point(315, 17);
            this.btnSaveFile.Name = "btnSaveFile";
            this.btnSaveFile.Size = new System.Drawing.Size(107, 53);
            this.btnSaveFile.TabIndex = 5;
            this.btnSaveFile.Text = "保存为bin文件";
            this.btnSaveFile.UseVisualStyleBackColor = true;
            this.btnSaveFile.Click += new System.EventHandler(this.btnSaveFile_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnSaveFile);
            this.groupBox1.Controls.Add(this.rBPageSelect);
            this.groupBox1.Controls.Add(this.rBLowerPage);
            this.groupBox1.Controls.Add(this.tBPage);
            this.groupBox1.Controls.Add(this.BtnRead);
            this.groupBox1.Location = new System.Drawing.Point(2, 203);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(596, 80);
            this.groupBox1.TabIndex = 99;
            this.groupBox1.TabStop = false;
            // 
            // rBPageSelect
            // 
            this.rBPageSelect.AutoSize = true;
            this.rBPageSelect.Checked = true;
            this.rBPageSelect.Location = new System.Drawing.Point(13, 49);
            this.rBPageSelect.Name = "rBPageSelect";
            this.rBPageSelect.Size = new System.Drawing.Size(47, 16);
            this.rBPageSelect.TabIndex = 4;
            this.rBPageSelect.TabStop = true;
            this.rBPageSelect.Text = "Page";
            this.rBPageSelect.UseVisualStyleBackColor = true;
            // 
            // rBLowerPage
            // 
            this.rBLowerPage.AutoSize = true;
            this.rBLowerPage.Location = new System.Drawing.Point(13, 20);
            this.rBLowerPage.Name = "rBLowerPage";
            this.rBLowerPage.Size = new System.Drawing.Size(83, 16);
            this.rBLowerPage.TabIndex = 3;
            this.rBLowerPage.Text = "Lower Page";
            this.rBLowerPage.UseVisualStyleBackColor = true;
            // 
            // tBPage
            // 
            this.tBPage.Location = new System.Drawing.Point(65, 49);
            this.tBPage.Name = "tBPage";
            this.tBPage.Size = new System.Drawing.Size(54, 21);
            this.tBPage.TabIndex = 2;
            this.tBPage.Text = "B0";
            this.tBPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // BtnRead
            // 
            this.BtnRead.Location = new System.Drawing.Point(162, 17);
            this.BtnRead.Name = "BtnRead";
            this.BtnRead.Size = new System.Drawing.Size(104, 53);
            this.BtnRead.TabIndex = 1;
            this.BtnRead.Text = "读取";
            this.BtnRead.UseVisualStyleBackColor = true;
            this.BtnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // EEPROM_MAP_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 420);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.groupBox1);
            this.Name = "EEPROM_MAP_Form";
            this.Text = "EEPROM_MAP_Form";
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_Reg)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_debug;
        private System.Windows.Forms.TextBox textBox_trackBar_val;
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
        private System.Windows.Forms.ColumnHeader columnHeader17;
        private System.Windows.Forms.CheckBox checkBox_ByteSelect;
        private System.Windows.Forms.Button button_RegRead;
        private System.Windows.Forms.TrackBar trackBar_Reg;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button button_PWD;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TextBox textBox_RegVal1;
        private System.Windows.Forms.TextBox textBox_PW03;
        private System.Windows.Forms.TextBox textBox_PW02;
        private System.Windows.Forms.TextBox textBox_PW01;
        private System.Windows.Forms.TextBox textBox_PW00;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.TextBox textBox_Page;
        private System.Windows.Forms.Button btnSaveFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rBPageSelect;
        private System.Windows.Forms.RadioButton rBLowerPage;
        private System.Windows.Forms.TextBox tBPage;
        private System.Windows.Forms.Button BtnRead;
    }
}