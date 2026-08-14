namespace SFPXFP自动测试软件多端口
{
    partial class LoginFrm
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
            this.label9 = new System.Windows.Forms.Label();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.cbBLanguage = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label8 = new System.Windows.Forms.Label();
            this.readFibertopbn_button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.bn_textBox = new System.Windows.Forms.TextBox();
            this.type_comboBox = new System.Windows.Forms.ComboBox();
            this.ok_button = new System.Windows.Forms.Button();
            this.sqlserver_comboBox = new System.Windows.Forms.ComboBox();
            this.testSQL_button = new System.Windows.Forms.Button();
            this.update_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cancel_button = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.button_testOtp12 = new System.Windows.Forms.Button();
            this.textBox_otp12Ip = new System.Windows.Forms.TextBox();
            this.label_otp12 = new System.Windows.Forms.Label();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(279, 222);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 112;
            this.label9.Text = "Language";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton3.ForeColor = System.Drawing.Color.Red;
            this.radioButton3.Location = new System.Drawing.Point(13, 20);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(58, 20);
            this.radioButton3.TabIndex = 0;
            this.radioButton3.Text = "初测";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Checked = true;
            this.radioButton4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton4.ForeColor = System.Drawing.Color.Red;
            this.radioButton4.Location = new System.Drawing.Point(143, 20);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(58, 20);
            this.radioButton4.TabIndex = 1;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "终测";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // cbBLanguage
            // 
            this.cbBLanguage.FormattingEnabled = true;
            this.cbBLanguage.Items.AddRange(new object[] {
            "Chinese",
            "English"});
            this.cbBLanguage.Location = new System.Drawing.Point(341, 219);
            this.cbBLanguage.Name = "cbBLanguage";
            this.cbBLanguage.Size = new System.Drawing.Size(96, 20);
            this.cbBLanguage.TabIndex = 111;
            this.cbBLanguage.SelectedIndexChanged += new System.EventHandler(this.cbBLanguage_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButton3);
            this.groupBox3.Controls.Add(this.radioButton4);
            this.groupBox3.Location = new System.Drawing.Point(12, 196);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(247, 51);
            this.groupBox3.TabIndex = 107;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "测试工序";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 332);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 110;
            this.label8.Text = "模块类型";
            // 
            // readFibertopbn_button1
            // 
            this.readFibertopbn_button1.Location = new System.Drawing.Point(186, 13);
            this.readFibertopbn_button1.Name = "readFibertopbn_button1";
            this.readFibertopbn_button1.Size = new System.Drawing.Size(54, 29);
            this.readFibertopbn_button1.TabIndex = 1;
            this.readFibertopbn_button1.Text = "读取";
            this.readFibertopbn_button1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.readFibertopbn_button1);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.textBox4);
            this.groupBox2.Controls.Add(this.textBox3);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.textBox2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.bn_textBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(247, 182);
            this.groupBox2.TabIndex = 100;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "生产单号信息";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 127);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "外壳批次号";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "PCBA批次号";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(118, 123);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(122, 21);
            this.textBox4.TabIndex = 5;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(118, 97);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(122, 21);
            this.textBox3.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(5, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "Bosa";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "Rosa批次号";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(118, 71);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(122, 21);
            this.textBox2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Tosa批次号";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(118, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(122, 21);
            this.textBox1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "生产单号";
            // 
            // bn_textBox
            // 
            this.bn_textBox.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bn_textBox.Location = new System.Drawing.Point(118, 18);
            this.bn_textBox.MaxLength = 10;
            this.bn_textBox.Name = "bn_textBox";
            this.bn_textBox.Size = new System.Drawing.Size(62, 23);
            this.bn_textBox.TabIndex = 0;
            this.bn_textBox.Text = "00000";
            // 
            // type_comboBox
            // 
            this.type_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.type_comboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.type_comboBox.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.type_comboBox.FormattingEnabled = true;
            this.type_comboBox.Location = new System.Drawing.Point(12, 349);
            this.type_comboBox.Name = "type_comboBox";
            this.type_comboBox.Size = new System.Drawing.Size(137, 21);
            this.type_comboBox.TabIndex = 102;
            this.type_comboBox.SelectedIndexChanged += new System.EventHandler(this.type_comboBox_SelectedIndexChanged);
            // 
            // ok_button
            // 
            this.ok_button.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok_button.Enabled = false;
            this.ok_button.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ok_button.Location = new System.Drawing.Point(304, 276);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(133, 45);
            this.ok_button.TabIndex = 108;
            this.ok_button.Text = "确 定";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // sqlserver_comboBox
            // 
            this.sqlserver_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sqlserver_comboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sqlserver_comboBox.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.sqlserver_comboBox.FormattingEnabled = true;
            this.sqlserver_comboBox.Items.AddRange(new object[] {
            "192.168.0.10",
            "null",
            "127.0.0.1"});
            this.sqlserver_comboBox.Location = new System.Drawing.Point(12, 289);
            this.sqlserver_comboBox.Name = "sqlserver_comboBox";
            this.sqlserver_comboBox.Size = new System.Drawing.Size(137, 21);
            this.sqlserver_comboBox.TabIndex = 101;
            this.sqlserver_comboBox.SelectedIndexChanged += new System.EventHandler(this.sqlserver_comboBox_SelectedIndexChanged);
            // 
            // testSQL_button
            // 
            this.testSQL_button.Location = new System.Drawing.Point(155, 282);
            this.testSQL_button.Name = "testSQL_button";
            this.testSQL_button.Size = new System.Drawing.Size(104, 33);
            this.testSQL_button.TabIndex = 103;
            this.testSQL_button.Text = "测试连接";
            this.testSQL_button.UseVisualStyleBackColor = true;
            this.testSQL_button.Click += new System.EventHandler(this.testSQL_button_Click);
            // 
            // update_button
            // 
            this.update_button.Location = new System.Drawing.Point(155, 343);
            this.update_button.Name = "update_button";
            this.update_button.Size = new System.Drawing.Size(104, 33);
            this.update_button.TabIndex = 104;
            this.update_button.Text = "从服务器更新";
            this.update_button.UseVisualStyleBackColor = true;
            this.update_button.Click += new System.EventHandler(this.update_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 273);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 106;
            this.label1.Text = "服务器 IP 地址";
            // 
            // cancel_button
            // 
            this.cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel_button.Location = new System.Drawing.Point(304, 342);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(133, 33);
            this.cancel_button.TabIndex = 109;
            this.cancel_button.Text = "退 出";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.cancel_button_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Controls.Add(this.textBox5);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.button_testOtp12);
            this.groupBox5.Controls.Add(this.textBox_otp12Ip);
            this.groupBox5.Controls.Add(this.label_otp12);
            this.groupBox5.Location = new System.Drawing.Point(281, 28);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(268, 166);
            this.groupBox5.TabIndex = 116;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "设备连接";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(187, 54);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "测试连接";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(81, 56);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(100, 21);
            this.textBox5.TabIndex = 4;
            this.textBox5.Text = "129.168.1.133";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(10, 61);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(71, 12);
            this.label10.TabIndex = 3;
            this.label10.Text = "加热台 IP：";
            // 
            // button_testOtp12
            // 
            this.button_testOtp12.Location = new System.Drawing.Point(187, 16);
            this.button_testOtp12.Name = "button_testOtp12";
            this.button_testOtp12.Size = new System.Drawing.Size(75, 23);
            this.button_testOtp12.TabIndex = 2;
            this.button_testOtp12.Text = "测试连接";
            this.button_testOtp12.UseVisualStyleBackColor = true;
            // 
            // textBox_otp12Ip
            // 
            this.textBox_otp12Ip.Location = new System.Drawing.Point(81, 18);
            this.textBox_otp12Ip.Name = "textBox_otp12Ip";
            this.textBox_otp12Ip.Size = new System.Drawing.Size(100, 21);
            this.textBox_otp12Ip.TabIndex = 1;
            this.textBox_otp12Ip.Text = "192.168.100.156";
            // 
            // label_otp12
            // 
            this.label_otp12.AutoSize = true;
            this.label_otp12.Location = new System.Drawing.Point(10, 23);
            this.label_otp12.Name = "label_otp12";
            this.label_otp12.Size = new System.Drawing.Size(65, 12);
            this.label_otp12.TabIndex = 0;
            this.label_otp12.Text = "OTP12 IP：";
            // 
            // LoginFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 385);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.cbBLanguage);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.type_comboBox);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.sqlserver_comboBox);
            this.Controls.Add(this.testSQL_button);
            this.Controls.Add(this.update_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancel_button);
            this.Name = "LoginFrm";
            this.Text = "LoginFrom";
            this.Load += new System.EventHandler(this.LoginFrm_Load);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.ComboBox cbBLanguage;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button readFibertopbn_button1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox bn_textBox;
        private System.Windows.Forms.ComboBox type_comboBox;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.ComboBox sqlserver_comboBox;
        private System.Windows.Forms.Button testSQL_button;
        private System.Windows.Forms.Button update_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button_testOtp12;
        private System.Windows.Forms.TextBox textBox_otp12Ip;
        private System.Windows.Forms.Label label_otp12;
    }
}