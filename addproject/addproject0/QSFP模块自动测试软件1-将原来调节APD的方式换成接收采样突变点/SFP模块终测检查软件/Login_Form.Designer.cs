namespace SFP模块终测检查软件
{
    partial class Login_Form
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sqlserver_comboBox = new System.Windows.Forms.ComboBox();
            this.ok_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.testSQL_button = new System.Windows.Forms.Button();
            this.update_button = new System.Windows.Forms.Button();
            this.bn_textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.type_comboBox = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.readFibertopbn_button1 = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.label_otp12 = new System.Windows.Forms.Label();
            this.textBox_otp12Ip = new System.Windows.Forms.TextBox();
            this.button_testOtp12 = new System.Windows.Forms.Button();
            this.button_testHeater = new System.Windows.Forms.Button();
            this.textBox_heaterIp = new System.Windows.Forms.TextBox();
            this.label_heater = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_testHeater);
            this.groupBox1.Controls.Add(this.textBox_heaterIp);
            this.groupBox1.Controls.Add(this.label_heater);
            this.groupBox1.Controls.Add(this.button_testOtp12);
            this.groupBox1.Controls.Add(this.textBox_otp12Ip);
            this.groupBox1.Controls.Add(this.label_otp12);
            this.groupBox1.Location = new System.Drawing.Point(245, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(268, 102);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "设备连接";
            // 
            // sqlserver_comboBox
            // 
            this.sqlserver_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sqlserver_comboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sqlserver_comboBox.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.sqlserver_comboBox.FormattingEnabled = true;
            this.sqlserver_comboBox.Items.AddRange(new object[] {
            "192.168.0.10",
            "null"});
            this.sqlserver_comboBox.Location = new System.Drawing.Point(16, 233);
            this.sqlserver_comboBox.Name = "sqlserver_comboBox";
            this.sqlserver_comboBox.Size = new System.Drawing.Size(108, 21);
            this.sqlserver_comboBox.TabIndex = 1;
            this.sqlserver_comboBox.SelectedIndexChanged += new System.EventHandler(this.sqlserver_comboBox_SelectedIndexChanged);
            // 
            // ok_button
            // 
            this.ok_button.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok_button.Enabled = false;
            this.ok_button.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ok_button.Location = new System.Drawing.Point(257, 226);
            this.ok_button.Name = "ok_button";
            this.ok_button.Size = new System.Drawing.Size(90, 28);
            this.ok_button.TabIndex = 7;
            this.ok_button.Text = "确 定";
            this.ok_button.UseVisualStyleBackColor = true;
            this.ok_button.Click += new System.EventHandler(this.ok_button_Click);
            // 
            // cancel_button
            // 
            this.cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel_button.Location = new System.Drawing.Point(257, 276);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(90, 33);
            this.cancel_button.TabIndex = 8;
            this.cancel_button.Text = "退 出";
            this.cancel_button.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 217);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "服务器 IP 地址";
            // 
            // testSQL_button
            // 
            this.testSQL_button.Location = new System.Drawing.Point(132, 226);
            this.testSQL_button.Name = "testSQL_button";
            this.testSQL_button.Size = new System.Drawing.Size(88, 33);
            this.testSQL_button.TabIndex = 3;
            this.testSQL_button.Text = "测试连接";
            this.testSQL_button.UseVisualStyleBackColor = true;
            this.testSQL_button.Click += new System.EventHandler(this.testSQL_button_Click);
            // 
            // update_button
            // 
            this.update_button.Location = new System.Drawing.Point(132, 276);
            this.update_button.Name = "update_button";
            this.update_button.Size = new System.Drawing.Size(88, 33);
            this.update_button.TabIndex = 4;
            this.update_button.Text = "从服务器更新";
            this.update_button.UseVisualStyleBackColor = true;
            this.update_button.Click += new System.EventHandler(this.update_button_Click);
            // 
            // bn_textBox
            // 
            this.bn_textBox.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bn_textBox.Location = new System.Drawing.Point(82, 18);
            this.bn_textBox.MaxLength = 10;
            this.bn_textBox.Name = "bn_textBox";
            this.bn_textBox.Size = new System.Drawing.Size(62, 23);
            this.bn_textBox.TabIndex = 0;
            this.bn_textBox.Text = "00000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "生产单号";
            // 
            // type_comboBox
            // 
            this.type_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.type_comboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.type_comboBox.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.type_comboBox.FormattingEnabled = true;
            this.type_comboBox.Location = new System.Drawing.Point(16, 282);
            this.type_comboBox.Name = "type_comboBox";
            this.type_comboBox.Size = new System.Drawing.Size(108, 21);
            this.type_comboBox.TabIndex = 2;
            this.type_comboBox.SelectedIndexChanged += new System.EventHandler(this.type_comboBox_SelectedIndexChanged);
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
            this.groupBox2.Location = new System.Drawing.Point(16, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(217, 188);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "生产单号信息";
            // 
            // readFibertopbn_button1
            // 
            this.readFibertopbn_button1.Location = new System.Drawing.Point(150, 18);
            this.readFibertopbn_button1.Name = "readFibertopbn_button1";
            this.readFibertopbn_button1.Size = new System.Drawing.Size(54, 24);
            this.readFibertopbn_button1.TabIndex = 1;
            this.readFibertopbn_button1.Text = "读取";
            this.readFibertopbn_button1.UseVisualStyleBackColor = true;
            this.readFibertopbn_button1.Click += new System.EventHandler(this.readFibertopbn_button1_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 126);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "外壳批次号";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "PCBA批次号";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(82, 122);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(122, 21);
            this.textBox4.TabIndex = 5;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(82, 97);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(122, 21);
            this.textBox3.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "Bosa";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "Rosa批次号";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(82, 71);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(122, 21);
            this.textBox2.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Tosa批次号";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(82, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(122, 21);
            this.textBox1.TabIndex = 2;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 265);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 97;
            this.label8.Text = "模块类型";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButton3);
            this.groupBox3.Controls.Add(this.radioButton4);
            this.groupBox3.Location = new System.Drawing.Point(245, 120);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(203, 76);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "测试工序";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Checked = true;
            this.radioButton3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton3.ForeColor = System.Drawing.Color.Red;
            this.radioButton3.Location = new System.Drawing.Point(21, 19);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(58, 20);
            this.radioButton3.TabIndex = 0;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "初测";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton4.ForeColor = System.Drawing.Color.Red;
            this.radioButton4.Location = new System.Drawing.Point(21, 47);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(58, 20);
            this.radioButton4.TabIndex = 1;
            this.radioButton4.Text = "终测";
            this.radioButton4.UseVisualStyleBackColor = true;
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
            // textBox_otp12Ip
            // 
            this.textBox_otp12Ip.Location = new System.Drawing.Point(81, 18);
            this.textBox_otp12Ip.Name = "textBox_otp12Ip";
            this.textBox_otp12Ip.Size = new System.Drawing.Size(100, 21);
            this.textBox_otp12Ip.TabIndex = 1;
            this.textBox_otp12Ip.Text = "192.168.100.156";
            // 
            // button_testOtp12
            // 
            this.button_testOtp12.Location = new System.Drawing.Point(187, 16);
            this.button_testOtp12.Name = "button_testOtp12";
            this.button_testOtp12.Size = new System.Drawing.Size(75, 23);
            this.button_testOtp12.TabIndex = 2;
            this.button_testOtp12.Text = "测试连接";
            this.button_testOtp12.UseVisualStyleBackColor = true;
            this.button_testOtp12.Click += new System.EventHandler(this.button_testOtp12_Click);
            // 
            // button_testHeater
            // 
            this.button_testHeater.Location = new System.Drawing.Point(187, 54);
            this.button_testHeater.Name = "button_testHeater";
            this.button_testHeater.Size = new System.Drawing.Size(75, 23);
            this.button_testHeater.TabIndex = 5;
            this.button_testHeater.Text = "测试连接";
            this.button_testHeater.UseVisualStyleBackColor = true;
            this.button_testHeater.Click += new System.EventHandler(this.button_testHeater_Click);
            // 
            // textBox_heaterIp
            // 
            this.textBox_heaterIp.Location = new System.Drawing.Point(81, 56);
            this.textBox_heaterIp.Name = "textBox_heaterIp";
            this.textBox_heaterIp.Size = new System.Drawing.Size(100, 21);
            this.textBox_heaterIp.TabIndex = 4;
            this.textBox_heaterIp.Text = "129.168.1.133";
            // 
            // label_heater
            // 
            this.label_heater.AutoSize = true;
            this.label_heater.Location = new System.Drawing.Point(10, 61);
            this.label_heater.Name = "label_heater";
            this.label_heater.Size = new System.Drawing.Size(71, 12);
            this.label_heater.TabIndex = 3;
            this.label_heater.Text = "加热台 IP：";
            // 
            // Login_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(641, 368);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.type_comboBox);
            this.Controls.Add(this.testSQL_button);
            this.Controls.Add(this.update_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.ok_button);
            this.Controls.Add(this.sqlserver_comboBox);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Login_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "登录设置界面";
            this.Load += new System.EventHandler(this.Login_Form_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox sqlserver_comboBox;
        private System.Windows.Forms.Button ok_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button testSQL_button;
        private System.Windows.Forms.Button update_button;
        private System.Windows.Forms.TextBox bn_textBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox type_comboBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button readFibertopbn_button1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.Button button_testHeater;
        private System.Windows.Forms.TextBox textBox_heaterIp;
        private System.Windows.Forms.Label label_heater;
        private System.Windows.Forms.Button button_testOtp12;
        private System.Windows.Forms.TextBox textBox_otp12Ip;
        private System.Windows.Forms.Label label_otp12;
    }
}

