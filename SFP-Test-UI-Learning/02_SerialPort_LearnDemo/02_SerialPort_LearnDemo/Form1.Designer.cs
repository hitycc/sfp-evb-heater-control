
namespace _02_SerialPort_LearnDemo
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
            this.flowTopSetting = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.cboComPort = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboBaud = new System.Windows.Forms.ComboBox();
            this.btnOpenClose = new System.Windows.Forms.Button();
            this.splitMainUD = new System.Windows.Forms.SplitContainer();
            this.tableSendParam = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTemp = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtSendCmd = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtVolt = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.panelLog = new System.Windows.Forms.Panel();
            this.rtbSerialLog = new System.Windows.Forms.RichTextBox();
            this.flowTopSetting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMainUD)).BeginInit();
            this.splitMainUD.Panel1.SuspendLayout();
            this.splitMainUD.Panel2.SuspendLayout();
            this.splitMainUD.SuspendLayout();
            this.tableSendParam.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowTopSetting
            // 
            this.flowTopSetting.BackColor = System.Drawing.Color.LightGreen;
            this.flowTopSetting.Controls.Add(this.label1);
            this.flowTopSetting.Controls.Add(this.cboComPort);
            this.flowTopSetting.Controls.Add(this.label2);
            this.flowTopSetting.Controls.Add(this.cboBaud);
            this.flowTopSetting.Controls.Add(this.btnOpenClose);
            this.flowTopSetting.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowTopSetting.Location = new System.Drawing.Point(0, 0);
            this.flowTopSetting.Name = "flowTopSetting";
            this.flowTopSetting.Size = new System.Drawing.Size(800, 32);
            this.flowTopSetting.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "串口号";
            // 
            // cboComPort
            // 
            this.cboComPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboComPort.FormattingEnabled = true;
            this.cboComPort.Location = new System.Drawing.Point(50, 4);
            this.cboComPort.Name = "cboComPort";
            this.cboComPort.Size = new System.Drawing.Size(121, 20);
            this.cboComPort.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(177, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "波特率";
            // 
            // cboBaud
            // 
            this.cboBaud.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.cboBaud.FormattingEnabled = true;
            this.cboBaud.Location = new System.Drawing.Point(224, 4);
            this.cboBaud.Name = "cboBaud";
            this.cboBaud.Size = new System.Drawing.Size(121, 20);
            this.cboBaud.TabIndex = 3;
            // 
            // btnOpenClose
            // 
            this.btnOpenClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenClose.Location = new System.Drawing.Point(351, 3);
            this.btnOpenClose.Name = "btnOpenClose";
            this.btnOpenClose.Size = new System.Drawing.Size(75, 23);
            this.btnOpenClose.TabIndex = 4;
            this.btnOpenClose.Text = "打开串口";
            this.btnOpenClose.UseVisualStyleBackColor = true;
            this.btnOpenClose.Click += new System.EventHandler(this.btnOpenClose_Click);
            // 
            // splitMainUD
            // 
            this.splitMainUD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMainUD.Location = new System.Drawing.Point(0, 32);
            this.splitMainUD.Name = "splitMainUD";
            this.splitMainUD.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMainUD.Panel1
            // 
            this.splitMainUD.Panel1.Controls.Add(this.tableSendParam);
            // 
            // splitMainUD.Panel2
            // 
            this.splitMainUD.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitMainUD.Panel2.Controls.Add(this.panelLog);
            this.splitMainUD.Size = new System.Drawing.Size(800, 418);
            this.splitMainUD.SplitterDistance = 179;
            this.splitMainUD.TabIndex = 1;
            // 
            // tableSendParam
            // 
            this.tableSendParam.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableSendParam.ColumnCount = 3;
            this.tableSendParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableSendParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableSendParam.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 600F));
            this.tableSendParam.Controls.Add(this.label5, 0, 2);
            this.tableSendParam.Controls.Add(this.txtTemp, 1, 2);
            this.tableSendParam.Controls.Add(this.label4, 0, 1);
            this.tableSendParam.Controls.Add(this.txtSendCmd, 1, 0);
            this.tableSendParam.Controls.Add(this.label3, 0, 0);
            this.tableSendParam.Controls.Add(this.txtVolt, 1, 1);
            this.tableSendParam.Controls.Add(this.btnSend, 2, 2);
            this.tableSendParam.Location = new System.Drawing.Point(0, 0);
            this.tableSendParam.Name = "tableSendParam";
            this.tableSendParam.RowCount = 3;
            this.tableSendParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableSendParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableSendParam.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tableSendParam.Size = new System.Drawing.Size(800, 179);
            this.tableSendParam.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "模块温度(℃)";
            // 
            // txtTemp
            // 
            this.txtTemp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTemp.Location = new System.Drawing.Point(103, 138);
            this.txtTemp.Name = "txtTemp";
            this.txtTemp.ReadOnly = true;
            this.txtTemp.Size = new System.Drawing.Size(94, 21);
            this.txtTemp.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "模块电压(V)";
            // 
            // txtSendCmd
            // 
            this.txtSendCmd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSendCmd.Location = new System.Drawing.Point(103, 19);
            this.txtSendCmd.Name = "txtSendCmd";
            this.txtSendCmd.Size = new System.Drawing.Size(94, 21);
            this.txtSendCmd.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(44, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 1;
            this.label3.Text = "发送指令";
            // 
            // txtVolt
            // 
            this.txtVolt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtVolt.Location = new System.Drawing.Point(103, 78);
            this.txtVolt.Name = "txtVolt";
            this.txtVolt.ReadOnly = true;
            this.txtVolt.Size = new System.Drawing.Size(94, 21);
            this.txtVolt.TabIndex = 2;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSend.Location = new System.Drawing.Point(203, 137);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 6;
            this.btnSend.Text = "发送指令";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // panelLog
            // 
            this.panelLog.BackColor = System.Drawing.Color.FloralWhite;
            this.panelLog.Controls.Add(this.rtbSerialLog);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLog.Location = new System.Drawing.Point(0, 0);
            this.panelLog.Name = "panelLog";
            this.panelLog.Size = new System.Drawing.Size(800, 235);
            this.panelLog.TabIndex = 0;
            // 
            // rtbSerialLog
            // 
            this.rtbSerialLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbSerialLog.Location = new System.Drawing.Point(0, 0);
            this.rtbSerialLog.Name = "rtbSerialLog";
            this.rtbSerialLog.ReadOnly = true;
            this.rtbSerialLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbSerialLog.Size = new System.Drawing.Size(800, 235);
            this.rtbSerialLog.TabIndex = 0;
            this.rtbSerialLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitMainUD);
            this.Controls.Add(this.flowTopSetting);
            this.Name = "Form1";
            this.Text = "Form1";
            this.flowTopSetting.ResumeLayout(false);
            this.flowTopSetting.PerformLayout();
            this.splitMainUD.Panel1.ResumeLayout(false);
            this.splitMainUD.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMainUD)).EndInit();
            this.splitMainUD.ResumeLayout(false);
            this.tableSendParam.ResumeLayout(false);
            this.tableSendParam.PerformLayout();
            this.panelLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowTopSetting;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboComPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboBaud;
        private System.Windows.Forms.Button btnOpenClose;
        private System.Windows.Forms.SplitContainer splitMainUD;
        private System.Windows.Forms.TableLayoutPanel tableSendParam;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtTemp;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSendCmd;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtVolt;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.RichTextBox rtbSerialLog;
    }
}

