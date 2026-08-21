namespace XFP模块测试程序
{
    partial class I2C_Form
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
            this.testSQL_button = new System.Windows.Forms.Button();
            this.update_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.sqlserver_comboBox = new System.Windows.Forms.ComboBox();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.OK_button = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // testSQL_button
            // 
            this.testSQL_button.Location = new System.Drawing.Point(93, 214);
            this.testSQL_button.Name = "testSQL_button";
            this.testSQL_button.Size = new System.Drawing.Size(75, 45);
            this.testSQL_button.TabIndex = 18;
            this.testSQL_button.Text = "测试连接";
            this.testSQL_button.UseVisualStyleBackColor = true;
            this.testSQL_button.Click += new System.EventHandler(this.testSQL_button_Click);
            // 
            // update_button
            // 
            this.update_button.Location = new System.Drawing.Point(12, 214);
            this.update_button.Name = "update_button";
            this.update_button.Size = new System.Drawing.Size(75, 45);
            this.update_button.TabIndex = 17;
            this.update_button.Text = "刷新列表";
            this.update_button.UseVisualStyleBackColor = true;
            this.update_button.Click += new System.EventHandler(this.update_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 162);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 16;
            this.label1.Text = "模块终测数据库：";
            // 
            // sqlserver_comboBox
            // 
            this.sqlserver_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sqlserver_comboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sqlserver_comboBox.FormattingEnabled = true;
            this.sqlserver_comboBox.Location = new System.Drawing.Point(12, 188);
            this.sqlserver_comboBox.Name = "sqlserver_comboBox";
            this.sqlserver_comboBox.Size = new System.Drawing.Size(163, 20);
            this.sqlserver_comboBox.TabIndex = 15;
            // 
            // Cancel_button
            // 
            this.Cancel_button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_button.Location = new System.Drawing.Point(93, 261);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(75, 45);
            this.Cancel_button.TabIndex = 14;
            this.Cancel_button.Text = "取消";
            this.Cancel_button.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Checked = true;
            this.radioButton2.Location = new System.Drawing.Point(17, 106);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(47, 16);
            this.radioButton2.TabIndex = 2;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "并口";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // OK_button
            // 
            this.OK_button.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK_button.Location = new System.Drawing.Point(12, 261);
            this.OK_button.Name = "OK_button";
            this.OK_button.Size = new System.Drawing.Size(75, 45);
            this.OK_button.TabIndex = 13;
            this.OK_button.Text = "确定";
            this.OK_button.UseVisualStyleBackColor = true;
            this.OK_button.Click += new System.EventHandler(this.OK_button_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Location = new System.Drawing.Point(15, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(131, 143);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "通信选择";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(17, 48);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(41, 16);
            this.radioButton1.TabIndex = 1;
            this.radioButton1.Text = "USB";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // I2C_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(184, 313);
            this.Controls.Add(this.testSQL_button);
            this.Controls.Add(this.update_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sqlserver_comboBox);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.OK_button);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "I2C_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "通信选择";
            this.Load += new System.EventHandler(this.I2C_Form_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button testSQL_button;
        private System.Windows.Forms.Button update_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox sqlserver_comboBox;
        private System.Windows.Forms.Button Cancel_button;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Button OK_button;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton1;
    }
}

