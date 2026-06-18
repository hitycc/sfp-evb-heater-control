
namespace LastEVBControlDemoApp
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
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.lblConnectStatus = new System.Windows.Forms.Label();
            this.lblVoltageSet = new System.Windows.Forms.Label();
            this.txtVoltageInput = new System.Windows.Forms.TextBox();
            this.btnSetVoltage = new System.Windows.Forms.Button();
            this.lblPowerEN = new System.Windows.Forms.Label();
            this.btnPowerENOn = new System.Windows.Forms.Button();
            this.btnPowerENOff = new System.Windows.Forms.Button();
            this.lblPowerENStatus = new System.Windows.Forms.Label();
            this.grpPinControl = new System.Windows.Forms.GroupBox();
            this.btnRs1HighLow = new System.Windows.Forms.Button();
            this.btnRs1HighHigh = new System.Windows.Forms.Button();
            this.lblRs1HighStatus = new System.Windows.Forms.Label();
            this.lblRs1High = new System.Windows.Forms.Label();
            this.btnRs0HighLow = new System.Windows.Forms.Button();
            this.btnRs0HighHigh = new System.Windows.Forms.Button();
            this.lblRs0HighStatus = new System.Windows.Forms.Label();
            this.lblRs0High = new System.Windows.Forms.Label();
            this.btnTxDisLow = new System.Windows.Forms.Button();
            this.btnTxDisHigh = new System.Windows.Forms.Button();
            this.lblTxDisStatus = new System.Windows.Forms.Label();
            this.lblTxDis = new System.Windows.Forms.Label();
            this.grpPinStatus = new System.Windows.Forms.GroupBox();
            this.lblABS = new System.Windows.Forms.Label();
            this.lblRxLos = new System.Windows.Forms.Label();
            this.lblTxFalu = new System.Windows.Forms.Label();
            this.btnQueryAllPin = new System.Windows.Forms.Button();
            this.lblPower = new System.Windows.Forms.Label();
            this.lblCurrent = new System.Windows.Forms.Label();
            this.lblVoltage = new System.Windows.Forms.Label();
            this.btnGetData = new System.Windows.Forms.Label();
            this.grpPinControl.SuspendLayout();
            this.grpPinStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(29, 26);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "连接设备";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(227, 26);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnDisconnect.TabIndex = 1;
            this.btnDisconnect.Text = "断开连接";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // lblConnectStatus
            // 
            this.lblConnectStatus.AutoSize = true;
            this.lblConnectStatus.Location = new System.Drawing.Point(420, 31);
            this.lblConnectStatus.Name = "lblConnectStatus";
            this.lblConnectStatus.Size = new System.Drawing.Size(77, 12);
            this.lblConnectStatus.TabIndex = 2;
            this.lblConnectStatus.Text = "状态：未连接";
            // 
            // lblVoltageSet
            // 
            this.lblVoltageSet.AutoSize = true;
            this.lblVoltageSet.Location = new System.Drawing.Point(27, 92);
            this.lblVoltageSet.Name = "lblVoltageSet";
            this.lblVoltageSet.Size = new System.Drawing.Size(137, 12);
            this.lblVoltageSet.TabIndex = 3;
            this.lblVoltageSet.Text = "电压设置 (3.15~3.3V)：";
            // 
            // txtVoltageInput
            // 
            this.txtVoltageInput.Location = new System.Drawing.Point(202, 89);
            this.txtVoltageInput.Name = "txtVoltageInput";
            this.txtVoltageInput.Size = new System.Drawing.Size(100, 21);
            this.txtVoltageInput.TabIndex = 4;
            this.txtVoltageInput.Text = "3.3";
            // 
            // btnSetVoltage
            // 
            this.btnSetVoltage.Location = new System.Drawing.Point(422, 87);
            this.btnSetVoltage.Name = "btnSetVoltage";
            this.btnSetVoltage.Size = new System.Drawing.Size(75, 23);
            this.btnSetVoltage.TabIndex = 5;
            this.btnSetVoltage.Text = "确认设置";
            this.btnSetVoltage.UseVisualStyleBackColor = true;
            this.btnSetVoltage.Click += new System.EventHandler(this.btnSetVoltage_Click);
            // 
            // lblPowerEN
            // 
            this.lblPowerEN.AutoSize = true;
            this.lblPowerEN.Location = new System.Drawing.Point(29, 152);
            this.lblPowerEN.Name = "lblPowerEN";
            this.lblPowerEN.Size = new System.Drawing.Size(65, 12);
            this.lblPowerEN.TabIndex = 6;
            this.lblPowerEN.Text = "模块使能：";
            // 
            // btnPowerENOn
            // 
            this.btnPowerENOn.Location = new System.Drawing.Point(138, 147);
            this.btnPowerENOn.Name = "btnPowerENOn";
            this.btnPowerENOn.Size = new System.Drawing.Size(96, 23);
            this.btnPowerENOn.TabIndex = 7;
            this.btnPowerENOn.Text = "使能 (高电平)";
            this.btnPowerENOn.UseVisualStyleBackColor = true;
            this.btnPowerENOn.Click += new System.EventHandler(this.btnPowerENOn_Click);
            // 
            // btnPowerENOff
            // 
            this.btnPowerENOff.Location = new System.Drawing.Point(287, 147);
            this.btnPowerENOff.Name = "btnPowerENOff";
            this.btnPowerENOff.Size = new System.Drawing.Size(92, 23);
            this.btnPowerENOff.TabIndex = 8;
            this.btnPowerENOff.Text = "禁用 (低电平)";
            this.btnPowerENOff.UseVisualStyleBackColor = true;
            this.btnPowerENOff.Click += new System.EventHandler(this.btnPowerENOff_Click);
            // 
            // lblPowerENStatus
            // 
            this.lblPowerENStatus.AutoSize = true;
            this.lblPowerENStatus.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPowerENStatus.Location = new System.Drawing.Point(425, 153);
            this.lblPowerENStatus.Name = "lblPowerENStatus";
            this.lblPowerENStatus.Size = new System.Drawing.Size(72, 11);
            this.lblPowerENStatus.TabIndex = 9;
            this.lblPowerENStatus.Text = "当前状态：--";
            // 
            // grpPinControl
            // 
            this.grpPinControl.Controls.Add(this.btnRs1HighLow);
            this.grpPinControl.Controls.Add(this.btnRs1HighHigh);
            this.grpPinControl.Controls.Add(this.lblRs1HighStatus);
            this.grpPinControl.Controls.Add(this.lblRs1High);
            this.grpPinControl.Controls.Add(this.btnRs0HighLow);
            this.grpPinControl.Controls.Add(this.btnRs0HighHigh);
            this.grpPinControl.Controls.Add(this.lblRs0HighStatus);
            this.grpPinControl.Controls.Add(this.lblRs0High);
            this.grpPinControl.Controls.Add(this.btnTxDisLow);
            this.grpPinControl.Controls.Add(this.btnTxDisHigh);
            this.grpPinControl.Controls.Add(this.lblTxDisStatus);
            this.grpPinControl.Controls.Add(this.lblTxDis);
            this.grpPinControl.Location = new System.Drawing.Point(31, 212);
            this.grpPinControl.Name = "grpPinControl";
            this.grpPinControl.Size = new System.Drawing.Size(479, 198);
            this.grpPinControl.TabIndex = 10;
            this.grpPinControl.TabStop = false;
            this.grpPinControl.Text = "引脚控制";
            // 
            // btnRs1HighLow
            // 
            this.btnRs1HighLow.Location = new System.Drawing.Point(256, 156);
            this.btnRs1HighLow.Name = "btnRs1HighLow";
            this.btnRs1HighLow.Size = new System.Drawing.Size(75, 23);
            this.btnRs1HighLow.TabIndex = 11;
            this.btnRs1HighLow.Text = "低电平";
            this.btnRs1HighLow.UseVisualStyleBackColor = true;
            this.btnRs1HighLow.Click += new System.EventHandler(this.btnRs1HighLow_Click);
            // 
            // btnRs1HighHigh
            // 
            this.btnRs1HighHigh.Location = new System.Drawing.Point(128, 156);
            this.btnRs1HighHigh.Name = "btnRs1HighHigh";
            this.btnRs1HighHigh.Size = new System.Drawing.Size(75, 23);
            this.btnRs1HighHigh.TabIndex = 10;
            this.btnRs1HighHigh.Text = "高电平";
            this.btnRs1HighHigh.UseVisualStyleBackColor = true;
            this.btnRs1HighHigh.Click += new System.EventHandler(this.btnRs1HighHigh_Click);
            // 
            // lblRs1HighStatus
            // 
            this.lblRs1HighStatus.AutoSize = true;
            this.lblRs1HighStatus.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRs1HighStatus.Location = new System.Drawing.Point(389, 162);
            this.lblRs1HighStatus.Name = "lblRs1HighStatus";
            this.lblRs1HighStatus.Size = new System.Drawing.Size(50, 11);
            this.lblRs1HighStatus.TabIndex = 9;
            this.lblRs1HighStatus.Text = "状态：--";
            // 
            // lblRs1High
            // 
            this.lblRs1High.AutoSize = true;
            this.lblRs1High.Location = new System.Drawing.Point(15, 161);
            this.lblRs1High.Name = "lblRs1High";
            this.lblRs1High.Size = new System.Drawing.Size(83, 12);
            this.lblRs1High.TabIndex = 8;
            this.lblRs1High.Text = "RS1_HIGH 引脚";
            // 
            // btnRs0HighLow
            // 
            this.btnRs0HighLow.Location = new System.Drawing.Point(256, 91);
            this.btnRs0HighLow.Name = "btnRs0HighLow";
            this.btnRs0HighLow.Size = new System.Drawing.Size(75, 23);
            this.btnRs0HighLow.TabIndex = 7;
            this.btnRs0HighLow.Text = "低电平";
            this.btnRs0HighLow.UseVisualStyleBackColor = true;
            this.btnRs0HighLow.Click += new System.EventHandler(this.btnRs0HighLow_Click);
            // 
            // btnRs0HighHigh
            // 
            this.btnRs0HighHigh.Location = new System.Drawing.Point(128, 91);
            this.btnRs0HighHigh.Name = "btnRs0HighHigh";
            this.btnRs0HighHigh.Size = new System.Drawing.Size(75, 23);
            this.btnRs0HighHigh.TabIndex = 6;
            this.btnRs0HighHigh.Text = "高电平";
            this.btnRs0HighHigh.UseVisualStyleBackColor = true;
            this.btnRs0HighHigh.Click += new System.EventHandler(this.btnRs0HighHigh_Click);
            // 
            // lblRs0HighStatus
            // 
            this.lblRs0HighStatus.AutoSize = true;
            this.lblRs0HighStatus.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRs0HighStatus.Location = new System.Drawing.Point(389, 97);
            this.lblRs0HighStatus.Name = "lblRs0HighStatus";
            this.lblRs0HighStatus.Size = new System.Drawing.Size(50, 11);
            this.lblRs0HighStatus.TabIndex = 5;
            this.lblRs0HighStatus.Text = "状态：--";
            // 
            // lblRs0High
            // 
            this.lblRs0High.AutoSize = true;
            this.lblRs0High.Location = new System.Drawing.Point(15, 96);
            this.lblRs0High.Name = "lblRs0High";
            this.lblRs0High.Size = new System.Drawing.Size(95, 12);
            this.lblRs0High.TabIndex = 4;
            this.lblRs0High.Text = "RS0_HIGH 引脚：";
            // 
            // btnTxDisLow
            // 
            this.btnTxDisLow.Location = new System.Drawing.Point(256, 30);
            this.btnTxDisLow.Name = "btnTxDisLow";
            this.btnTxDisLow.Size = new System.Drawing.Size(75, 23);
            this.btnTxDisLow.TabIndex = 3;
            this.btnTxDisLow.Text = "低电平";
            this.btnTxDisLow.UseVisualStyleBackColor = true;
            this.btnTxDisLow.Click += new System.EventHandler(this.btnTxDisLow_Click);
            // 
            // btnTxDisHigh
            // 
            this.btnTxDisHigh.Location = new System.Drawing.Point(128, 30);
            this.btnTxDisHigh.Name = "btnTxDisHigh";
            this.btnTxDisHigh.Size = new System.Drawing.Size(75, 23);
            this.btnTxDisHigh.TabIndex = 2;
            this.btnTxDisHigh.Text = "高电平";
            this.btnTxDisHigh.UseVisualStyleBackColor = true;
            this.btnTxDisHigh.Click += new System.EventHandler(this.btnTxDisHigh_Click);
            // 
            // lblTxDisStatus
            // 
            this.lblTxDisStatus.AutoSize = true;
            this.lblTxDisStatus.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTxDisStatus.Location = new System.Drawing.Point(389, 36);
            this.lblTxDisStatus.Name = "lblTxDisStatus";
            this.lblTxDisStatus.Size = new System.Drawing.Size(50, 11);
            this.lblTxDisStatus.TabIndex = 1;
            this.lblTxDisStatus.Text = "状态：--";
            // 
            // lblTxDis
            // 
            this.lblTxDis.AutoSize = true;
            this.lblTxDis.Location = new System.Drawing.Point(15, 35);
            this.lblTxDis.Name = "lblTxDis";
            this.lblTxDis.Size = new System.Drawing.Size(83, 12);
            this.lblTxDis.TabIndex = 0;
            this.lblTxDis.Text = "TX_DIS 引脚：";
            // 
            // grpPinStatus
            // 
            this.grpPinStatus.Controls.Add(this.lblABS);
            this.grpPinStatus.Controls.Add(this.lblRxLos);
            this.grpPinStatus.Controls.Add(this.lblTxFalu);
            this.grpPinStatus.Controls.Add(this.btnQueryAllPin);
            this.grpPinStatus.Location = new System.Drawing.Point(554, 212);
            this.grpPinStatus.Name = "grpPinStatus";
            this.grpPinStatus.Size = new System.Drawing.Size(203, 198);
            this.grpPinStatus.TabIndex = 11;
            this.grpPinStatus.TabStop = false;
            this.grpPinStatus.Text = "引脚状态查询";
            // 
            // lblABS
            // 
            this.lblABS.AutoSize = true;
            this.lblABS.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblABS.Location = new System.Drawing.Point(6, 156);
            this.lblABS.Name = "lblABS";
            this.lblABS.Size = new System.Drawing.Size(46, 11);
            this.lblABS.TabIndex = 3;
            this.lblABS.Text = "ABS：--";
            // 
            // lblRxLos
            // 
            this.lblRxLos.AutoSize = true;
            this.lblRxLos.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRxLos.Location = new System.Drawing.Point(6, 115);
            this.lblRxLos.Name = "lblRxLos";
            this.lblRxLos.Size = new System.Drawing.Size(64, 11);
            this.lblRxLos.TabIndex = 2;
            this.lblRxLos.Text = "RX_LOS：--";
            // 
            // lblTxFalu
            // 
            this.lblTxFalu.AutoSize = true;
            this.lblTxFalu.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTxFalu.Location = new System.Drawing.Point(6, 79);
            this.lblTxFalu.Name = "lblTxFalu";
            this.lblTxFalu.Size = new System.Drawing.Size(76, 11);
            this.lblTxFalu.TabIndex = 1;
            this.lblTxFalu.Text = "TX_FAULT：--";
            // 
            // btnQueryAllPin
            // 
            this.btnQueryAllPin.Location = new System.Drawing.Point(6, 30);
            this.btnQueryAllPin.Name = "btnQueryAllPin";
            this.btnQueryAllPin.Size = new System.Drawing.Size(124, 23);
            this.btnQueryAllPin.TabIndex = 0;
            this.btnQueryAllPin.Text = "一键查询所有引脚";
            this.btnQueryAllPin.UseVisualStyleBackColor = true;
            this.btnQueryAllPin.Click += new System.EventHandler(this.btnQueryAllPin_Click);
            // 
            // lblPower
            // 
            this.lblPower.AutoSize = true;
            this.lblPower.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPower.Location = new System.Drawing.Point(31, 452);
            this.lblPower.Name = "lblPower";
            this.lblPower.Size = new System.Drawing.Size(68, 11);
            this.lblPower.TabIndex = 12;
            this.lblPower.Text = "功率：-- mW";
            // 
            // lblCurrent
            // 
            this.lblCurrent.AutoSize = true;
            this.lblCurrent.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurrent.Location = new System.Drawing.Point(157, 452);
            this.lblCurrent.Name = "lblCurrent";
            this.lblCurrent.Size = new System.Drawing.Size(68, 11);
            this.lblCurrent.TabIndex = 13;
            this.lblCurrent.Text = "电流：-- uA";
            // 
            // lblVoltage
            // 
            this.lblVoltage.AutoSize = true;
            this.lblVoltage.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblVoltage.Location = new System.Drawing.Point(285, 452);
            this.lblVoltage.Name = "lblVoltage";
            this.lblVoltage.Size = new System.Drawing.Size(68, 11);
            this.lblVoltage.TabIndex = 14;
            this.lblVoltage.Text = "电压：-- mV";
            // 
            // btnGetData
            // 
            this.btnGetData.AutoSize = true;
            this.btnGetData.Font = new System.Drawing.Font("宋体", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnGetData.Location = new System.Drawing.Point(420, 452);
            this.btnGetData.Name = "btnGetData";
            this.btnGetData.Size = new System.Drawing.Size(49, 11);
            this.btnGetData.TabIndex = 15;
            this.btnGetData.Text = "刷新数据";
            this.btnGetData.Click += new System.EventHandler(this.btnGetData_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 499);
            this.Controls.Add(this.btnGetData);
            this.Controls.Add(this.lblVoltage);
            this.Controls.Add(this.lblCurrent);
            this.Controls.Add(this.lblPower);
            this.Controls.Add(this.grpPinStatus);
            this.Controls.Add(this.grpPinControl);
            this.Controls.Add(this.lblPowerENStatus);
            this.Controls.Add(this.btnPowerENOff);
            this.Controls.Add(this.btnPowerENOn);
            this.Controls.Add(this.lblPowerEN);
            this.Controls.Add(this.btnSetVoltage);
            this.Controls.Add(this.txtVoltageInput);
            this.Controls.Add(this.lblVoltageSet);
            this.Controls.Add(this.lblConnectStatus);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnConnect);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.grpPinControl.ResumeLayout(false);
            this.grpPinControl.PerformLayout();
            this.grpPinStatus.ResumeLayout(false);
            this.grpPinStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label lblConnectStatus;
        private System.Windows.Forms.Label lblVoltageSet;
        private System.Windows.Forms.TextBox txtVoltageInput;
        private System.Windows.Forms.Button btnSetVoltage;
        private System.Windows.Forms.Label lblPowerEN;
        private System.Windows.Forms.Button btnPowerENOn;
        private System.Windows.Forms.Button btnPowerENOff;
        private System.Windows.Forms.Label lblPowerENStatus;
        private System.Windows.Forms.GroupBox grpPinControl;
        private System.Windows.Forms.Button btnRs1HighLow;
        private System.Windows.Forms.Button btnRs1HighHigh;
        private System.Windows.Forms.Label lblRs1HighStatus;
        private System.Windows.Forms.Label lblRs1High;
        private System.Windows.Forms.Button btnRs0HighLow;
        private System.Windows.Forms.Button btnRs0HighHigh;
        private System.Windows.Forms.Label lblRs0HighStatus;
        private System.Windows.Forms.Label lblRs0High;
        private System.Windows.Forms.Button btnTxDisLow;
        private System.Windows.Forms.Button btnTxDisHigh;
        private System.Windows.Forms.Label lblTxDisStatus;
        private System.Windows.Forms.Label lblTxDis;
        private System.Windows.Forms.GroupBox grpPinStatus;
        private System.Windows.Forms.Label lblABS;
        private System.Windows.Forms.Label lblRxLos;
        private System.Windows.Forms.Label lblTxFalu;
        private System.Windows.Forms.Button btnQueryAllPin;
        private System.Windows.Forms.Label lblPower;
        private System.Windows.Forms.Label lblCurrent;
        private System.Windows.Forms.Label lblVoltage;
        private System.Windows.Forms.Label btnGetData;
    }
}

