
namespace OTP12_SCPI_Control
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.lblIpTip = new System.Windows.Forms.Label();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnQueryIDN = new System.Windows.Forms.Button();
            this.btnReadDate = new System.Windows.Forms.Button();
            this.btnSetDate = new System.Windows.Forms.Button();
            this.lblOutput = new System.Windows.Forms.Label();
            this.btnReadTime = new System.Windows.Forms.Button();
            this.btnQueryAlarm = new System.Windows.Forms.Button();
            this.btnReadOPMPower = new System.Windows.Forms.Button();
            this.btnReadBoardSN = new System.Windows.Forms.Button();
            this.btnQueryAllBoard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblIpTip
            // 
            this.lblIpTip.AutoSize = true;
            this.lblIpTip.Location = new System.Drawing.Point(13, 13);
            this.lblIpTip.Name = "lblIpTip";
            this.lblIpTip.Size = new System.Drawing.Size(53, 12);
            this.lblIpTip.TabIndex = 0;
            this.lblIpTip.Text = "设备IP：";
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(72, 10);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(100, 21);
            this.txtIp.TabIndex = 1;
            this.txtIp.Text = "192.168.1.222";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(178, 8);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "连接设备";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(259, 8);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 3;
            this.btnDisconnect.Text = "断开设备";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnQueryIDN
            // 
            this.btnQueryIDN.Location = new System.Drawing.Point(56, 64);
            this.btnQueryIDN.Name = "btnQueryIDN";
            this.btnQueryIDN.Size = new System.Drawing.Size(90, 23);
            this.btnQueryIDN.TabIndex = 4;
            this.btnQueryIDN.Text = "查询设备信息";
            this.btnQueryIDN.UseVisualStyleBackColor = true;
            this.btnQueryIDN.Click += new System.EventHandler(this.btnQueryIDN_Click);
            // 
            // btnReadDate
            // 
            this.btnReadDate.Location = new System.Drawing.Point(152, 64);
            this.btnReadDate.Name = "btnReadDate";
            this.btnReadDate.Size = new System.Drawing.Size(88, 23);
            this.btnReadDate.TabIndex = 5;
            this.btnReadDate.Text = "读取系统日期";
            this.btnReadDate.UseVisualStyleBackColor = true;
            this.btnReadDate.Click += new System.EventHandler(this.btnReadDate_Click);
            // 
            // btnSetDate
            // 
            this.btnSetDate.Location = new System.Drawing.Point(178, 167);
            this.btnSetDate.Name = "btnSetDate";
            this.btnSetDate.Size = new System.Drawing.Size(88, 23);
            this.btnSetDate.TabIndex = 6;
            this.btnSetDate.Text = "设置日期";
            this.btnSetDate.UseVisualStyleBackColor = true;
            this.btnSetDate.Click += new System.EventHandler(this.btnSetDate_Click);
            // 
            // lblOutput
            // 
            this.lblOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblOutput.Location = new System.Drawing.Point(0, 288);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(800, 162);
            this.lblOutput.TabIndex = 8;
            this.lblOutput.Text = "输出结果：";
            this.lblOutput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnReadTime
            // 
            this.btnReadTime.Location = new System.Drawing.Point(58, 119);
            this.btnReadTime.Name = "btnReadTime";
            this.btnReadTime.Size = new System.Drawing.Size(88, 23);
            this.btnReadTime.TabIndex = 9;
            this.btnReadTime.Text = "读取系统时间";
            this.btnReadTime.UseVisualStyleBackColor = true;
            this.btnReadTime.Click += new System.EventHandler(this.btnReadTime_Click);
            // 
            // btnQueryAlarm
            // 
            this.btnQueryAlarm.Location = new System.Drawing.Point(152, 119);
            this.btnQueryAlarm.Name = "btnQueryAlarm";
            this.btnQueryAlarm.Size = new System.Drawing.Size(88, 23);
            this.btnQueryAlarm.TabIndex = 10;
            this.btnQueryAlarm.Text = "查询当前告警";
            this.btnQueryAlarm.UseVisualStyleBackColor = true;
            this.btnQueryAlarm.Click += new System.EventHandler(this.btnQueryAlarm_Click);
            // 
            // btnReadOPMPower
            // 
            this.btnReadOPMPower.Location = new System.Drawing.Point(246, 119);
            this.btnReadOPMPower.Name = "btnReadOPMPower";
            this.btnReadOPMPower.Size = new System.Drawing.Size(125, 23);
            this.btnReadOPMPower.TabIndex = 11;
            this.btnReadOPMPower.Text = "读取 OPM 通道功率";
            this.btnReadOPMPower.UseVisualStyleBackColor = true;
            this.btnReadOPMPower.Click += new System.EventHandler(this.btnReadOPMPower_Click);
            // 
            // btnReadBoardSN
            // 
            this.btnReadBoardSN.Location = new System.Drawing.Point(246, 64);
            this.btnReadBoardSN.Name = "btnReadBoardSN";
            this.btnReadBoardSN.Size = new System.Drawing.Size(88, 23);
            this.btnReadBoardSN.TabIndex = 13;
            this.btnReadBoardSN.Text = "读取单板序列号";
            this.btnReadBoardSN.UseVisualStyleBackColor = true;
            this.btnReadBoardSN.Click += new System.EventHandler(this.btnReadBoardSN_Click);
            // 
            // btnQueryAllBoard
            // 
            this.btnQueryAllBoard.Location = new System.Drawing.Point(58, 167);
            this.btnQueryAllBoard.Name = "btnQueryAllBoard";
            this.btnQueryAllBoard.Size = new System.Drawing.Size(114, 23);
            this.btnQueryAllBoard.TabIndex = 14;
            this.btnQueryAllBoard.Text = "查询所有在线槽位单板";
            this.btnQueryAllBoard.UseVisualStyleBackColor = true;
            this.btnQueryAllBoard.Click += new System.EventHandler(this.btnQueryAllBoard_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnQueryAllBoard);
            this.Controls.Add(this.btnReadBoardSN);
            this.Controls.Add(this.btnReadOPMPower);
            this.Controls.Add(this.btnQueryAlarm);
            this.Controls.Add(this.btnReadTime);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.btnSetDate);
            this.Controls.Add(this.btnReadDate);
            this.Controls.Add(this.btnQueryIDN);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtIp);
            this.Controls.Add(this.lblIpTip);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIpTip;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnQueryIDN;
        private System.Windows.Forms.Button btnReadDate;
        private System.Windows.Forms.Button btnSetDate;
        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Button btnReadTime;
        private System.Windows.Forms.Button btnQueryAlarm;
        private System.Windows.Forms.Button btnReadOPMPower;
        private System.Windows.Forms.Button btnReadBoardSN;
        private System.Windows.Forms.Button btnQueryAllBoard;
    }
}

