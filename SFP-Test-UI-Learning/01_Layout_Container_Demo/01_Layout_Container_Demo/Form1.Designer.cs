namespace TestDemo
{
    partial class Form1
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
            this.flowTopBtn = new System.Windows.Forms.FlowLayoutPanel();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnReadSFP = new System.Windows.Forms.Button();
            this.btnStartTest = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.groupLeftSelect = new System.Windows.Forms.GroupBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.cboSFPType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tableParam = new System.Windows.Forms.TableLayoutPanel();
            this.rxPower = new System.Windows.Forms.TextBox();
            this.txPower = new System.Windows.Forms.TextBox();
            this.txtTemp = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtVoltage = new System.Windows.Forms.TextBox();
            this.panelLogBox = new System.Windows.Forms.Panel();
            this.panelStatusBottom = new System.Windows.Forms.Panel();
            this.lblStatus = new System.Windows.Forms.Label();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.flowTopBtn.SuspendLayout();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.groupLeftSelect.SuspendLayout();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tableParam.SuspendLayout();
            this.panelLogBox.SuspendLayout();
            this.panelStatusBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowTopBtn
            // 
            this.flowTopBtn.BackColor = System.Drawing.Color.LightPink;
            this.flowTopBtn.Controls.Add(this.btnConnect);
            this.flowTopBtn.Controls.Add(this.btnReadSFP);
            this.flowTopBtn.Controls.Add(this.btnStartTest);
            this.flowTopBtn.Controls.Add(this.btnClearLog);
            this.flowTopBtn.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowTopBtn.Location = new System.Drawing.Point(0, 0);
            this.flowTopBtn.Name = "flowTopBtn";
            this.flowTopBtn.Size = new System.Drawing.Size(543, 32);
            this.flowTopBtn.TabIndex = 0;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(3, 3);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(128, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "连接测试设备";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnReadSFP
            // 
            this.btnReadSFP.Location = new System.Drawing.Point(137, 3);
            this.btnReadSFP.Name = "btnReadSFP";
            this.btnReadSFP.Size = new System.Drawing.Size(128, 23);
            this.btnReadSFP.TabIndex = 1;
            this.btnReadSFP.Text = "读取 SFP 参数";
            this.btnReadSFP.UseVisualStyleBackColor = true;
            this.btnReadSFP.Click += new System.EventHandler(this.btnReadSFP_Click);
            // 
            // btnStartTest
            // 
            this.btnStartTest.Location = new System.Drawing.Point(271, 3);
            this.btnStartTest.Name = "btnStartTest";
            this.btnStartTest.Size = new System.Drawing.Size(128, 23);
            this.btnStartTest.TabIndex = 2;
            this.btnStartTest.Text = "自动开始测试";
            this.btnStartTest.UseVisualStyleBackColor = true;
            this.btnStartTest.Click += new System.EventHandler(this.btnStartTest_Click);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(405, 3);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(128, 23);
            this.btnClearLog.TabIndex = 3;
            this.btnClearLog.Text = "清空日志";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 32);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.groupLeftSelect);
            this.splitMain.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel1_Paint);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.splitContainer);
            this.splitMain.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitMain.Size = new System.Drawing.Size(543, 539);
            this.splitMain.SplitterDistance = 220;
            this.splitMain.TabIndex = 1;
            // 
            // groupLeftSelect
            // 
            this.groupLeftSelect.Controls.Add(this.checkBox3);
            this.groupLeftSelect.Controls.Add(this.checkBox2);
            this.groupLeftSelect.Controls.Add(this.checkBox1);
            this.groupLeftSelect.Controls.Add(this.radioButton2);
            this.groupLeftSelect.Controls.Add(this.radioButton1);
            this.groupLeftSelect.Controls.Add(this.cboSFPType);
            this.groupLeftSelect.Controls.Add(this.label1);
            this.groupLeftSelect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftSelect.Location = new System.Drawing.Point(0, 0);
            this.groupLeftSelect.Name = "groupLeftSelect";
            this.groupLeftSelect.Size = new System.Drawing.Size(220, 539);
            this.groupLeftSelect.TabIndex = 0;
            this.groupLeftSelect.TabStop = false;
            this.groupLeftSelect.Text = "SFP测试选项配置";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(6, 339);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(78, 16);
            this.checkBox3.TabIndex = 6;
            this.checkBox3.Text = "checkBox3";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 286);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(84, 16);
            this.checkBox2.TabIndex = 5;
            this.checkBox2.Text = "光功率监测";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(6, 239);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "电压监测";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(6, 197);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(95, 16);
            this.radioButton2.TabIndex = 3;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "自动循环测试";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 153);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(95, 16);
            this.radioButton1.TabIndex = 2;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "手动单发指令";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // cboSFPType
            // 
            this.cboSFPType.FormattingEnabled = true;
            this.cboSFPType.Items.AddRange(new object[] {
            "SFP",
            "SFP+",
            "SFP28",
            "800G"});
            this.cboSFPType.Location = new System.Drawing.Point(3, 105);
            this.cboSFPType.Name = "cboSFPType";
            this.cboSFPType.Size = new System.Drawing.Size(121, 20);
            this.cboSFPType.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择模块型号";
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.tableParam);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panelLogBox);
            this.splitContainer.Size = new System.Drawing.Size(319, 539);
            this.splitContainer.SplitterDistance = 200;
            this.splitContainer.TabIndex = 0;
            // 
            // tableParam
            // 
            this.tableParam.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableParam.BackColor = System.Drawing.SystemColors.Control;
            this.tableParam.ColumnCount = 2;
            this.tableParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableParam.Controls.Add(this.rxPower, 1, 3);
            this.tableParam.Controls.Add(this.txPower, 1, 2);
            this.tableParam.Controls.Add(this.txtTemp, 1, 1);
            this.tableParam.Controls.Add(this.label2, 0, 0);
            this.tableParam.Controls.Add(this.label3, 0, 1);
            this.tableParam.Controls.Add(this.label4, 0, 2);
            this.tableParam.Controls.Add(this.label5, 0, 3);
            this.tableParam.Controls.Add(this.txtVoltage, 1, 0);
            this.tableParam.Location = new System.Drawing.Point(0, 0);
            this.tableParam.Name = "tableParam";
            this.tableParam.RowCount = 4;
            this.tableParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableParam.Size = new System.Drawing.Size(319, 200);
            this.tableParam.TabIndex = 0;
            // 
            // rxPower
            // 
            this.rxPower.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.rxPower.Location = new System.Drawing.Point(93, 164);
            this.rxPower.Name = "rxPower";
            this.rxPower.Size = new System.Drawing.Size(223, 21);
            this.rxPower.TabIndex = 7;
            // 
            // txPower
            // 
            this.txPower.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txPower.Location = new System.Drawing.Point(93, 114);
            this.txPower.Name = "txPower";
            this.txPower.Size = new System.Drawing.Size(223, 21);
            this.txPower.TabIndex = 6;
            // 
            // txtTemp
            // 
            this.txtTemp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTemp.Location = new System.Drawing.Point(93, 64);
            this.txtTemp.Name = "txtTemp";
            this.txtTemp.Size = new System.Drawing.Size(223, 21);
            this.txtTemp.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "模块电压 (V)";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "内部温度 (℃)";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(12, 119);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "发射光功率";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 169);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "接收光功率";
            // 
            // txtVoltage
            // 
            this.txtVoltage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVoltage.Location = new System.Drawing.Point(93, 14);
            this.txtVoltage.Name = "txtVoltage";
            this.txtVoltage.Size = new System.Drawing.Size(223, 21);
            this.txtVoltage.TabIndex = 4;
            // 
            // panelLogBox
            // 
            this.panelLogBox.BackColor = System.Drawing.Color.LightGray;
            this.panelLogBox.Controls.Add(this.panelStatusBottom);
            this.panelLogBox.Controls.Add(this.rtbLog);
            this.panelLogBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelLogBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLogBox.Location = new System.Drawing.Point(0, 0);
            this.panelLogBox.Name = "panelLogBox";
            this.panelLogBox.Size = new System.Drawing.Size(319, 335);
            this.panelLogBox.TabIndex = 0;
            // 
            // panelStatusBottom
            // 
            this.panelStatusBottom.BackColor = System.Drawing.Color.LightPink;
            this.panelStatusBottom.Controls.Add(this.lblStatus);
            this.panelStatusBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelStatusBottom.Location = new System.Drawing.Point(0, 262);
            this.panelStatusBottom.Name = "panelStatusBottom";
            this.panelStatusBottom.Size = new System.Drawing.Size(319, 73);
            this.panelStatusBottom.TabIndex = 1;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(125, 12);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "设备未连接，位置靠左";
            // 
            // rtbLog
            // 
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Location = new System.Drawing.Point(0, 0);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbLog.Size = new System.Drawing.Size(319, 335);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 571);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.flowTopBtn);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.flowTopBtn.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            this.splitMain.ResumeLayout(false);
            this.groupLeftSelect.ResumeLayout(false);
            this.groupLeftSelect.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.tableParam.ResumeLayout(false);
            this.tableParam.PerformLayout();
            this.panelLogBox.ResumeLayout(false);
            this.panelStatusBottom.ResumeLayout(false);
            this.panelStatusBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowTopBtn;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnReadSFP;
        private System.Windows.Forms.Button btnStartTest;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.GroupBox groupLeftSelect;
        private System.Windows.Forms.ComboBox cboSFPType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TableLayoutPanel tableParam;
        private System.Windows.Forms.TextBox rxPower;
        private System.Windows.Forms.TextBox txPower;
        private System.Windows.Forms.TextBox txtTemp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtVoltage;
        private System.Windows.Forms.Panel panelLogBox;
        private System.Windows.Forms.Panel panelStatusBottom;
        private System.Windows.Forms.RichTextBox rtbLog;
        private System.Windows.Forms.Label lblStatus;
    }
}

