
namespace _03_TestFunction_Demo
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
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblDevStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTestStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.timerStatus = new System.Windows.Forms.Timer(this.components);
            this.flowBtnTop = new System.Windows.Forms.FlowLayoutPanel();
            this.btnStartTest = new System.Windows.Forms.Button();
            this.btnStopTest = new System.Windows.Forms.Button();
            this.btnSaveTxtLog = new System.Windows.Forms.Button();
            this.btnSaveCsvReport = new System.Windows.Forms.Button();
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.panelProgress = new System.Windows.Forms.Panel();
            this.progressTest = new System.Windows.Forms.ProgressBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblSingleData = new System.Windows.Forms.Label();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.statusStrip1.SuspendLayout();
            this.flowBtnTop.SuspendLayout();
            this.panelProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblDevStatus,
            this.lblTestStatus,
            this.lblTime});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblDevStatus
            // 
            this.lblDevStatus.Name = "lblDevStatus";
            this.lblDevStatus.Size = new System.Drawing.Size(80, 17);
            this.lblDevStatus.Text = "设备：未连接";
            // 
            // lblTestStatus
            // 
            this.lblTestStatus.Name = "lblTestStatus";
            this.lblTestStatus.Size = new System.Drawing.Size(68, 17);
            this.lblTestStatus.Text = "测试：空闲";
            // 
            // lblTime
            // 
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(110, 17);
            this.lblTime.Text = "时间：00：00：00";
            // 
            // timerStatus
            // 
            this.timerStatus.Enabled = true;
            this.timerStatus.Interval = 1000;
            this.timerStatus.Tick += new System.EventHandler(this.timerStatus_Tick);
            // 
            // flowBtnTop
            // 
            this.flowBtnTop.BackColor = System.Drawing.Color.BlanchedAlmond;
            this.flowBtnTop.Controls.Add(this.btnStartTest);
            this.flowBtnTop.Controls.Add(this.btnStopTest);
            this.flowBtnTop.Controls.Add(this.btnSaveTxtLog);
            this.flowBtnTop.Controls.Add(this.btnSaveCsvReport);
            this.flowBtnTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowBtnTop.Location = new System.Drawing.Point(0, 0);
            this.flowBtnTop.Name = "flowBtnTop";
            this.flowBtnTop.Size = new System.Drawing.Size(800, 33);
            this.flowBtnTop.TabIndex = 1;
            // 
            // btnStartTest
            // 
            this.btnStartTest.Location = new System.Drawing.Point(3, 3);
            this.btnStartTest.Name = "btnStartTest";
            this.btnStartTest.Size = new System.Drawing.Size(103, 23);
            this.btnStartTest.TabIndex = 0;
            this.btnStartTest.Text = "开始批量测试";
            this.toolTipHelp.SetToolTip(this.btnStartTest, "模拟多通道 SFP 自动测试，进度条实时更新测试进度");
            this.btnStartTest.UseVisualStyleBackColor = true;
            this.btnStartTest.Click += new System.EventHandler(this.btnStartTest_Click);
            // 
            // btnStopTest
            // 
            this.btnStopTest.Location = new System.Drawing.Point(112, 3);
            this.btnStopTest.Name = "btnStopTest";
            this.btnStopTest.Size = new System.Drawing.Size(75, 23);
            this.btnStopTest.TabIndex = 1;
            this.btnStopTest.Text = "停止测试";
            this.toolTipHelp.SetToolTip(this.btnStopTest, "中断当前正在执行的批量测试");
            this.btnStopTest.UseVisualStyleBackColor = true;
            this.btnStopTest.Click += new System.EventHandler(this.btnStopTest_Click);
            // 
            // btnSaveTxtLog
            // 
            this.btnSaveTxtLog.Location = new System.Drawing.Point(193, 3);
            this.btnSaveTxtLog.Name = "btnSaveTxtLog";
            this.btnSaveTxtLog.Size = new System.Drawing.Size(96, 23);
            this.btnSaveTxtLog.TabIndex = 2;
            this.btnSaveTxtLog.Text = "保存TXT日志";
            this.toolTipHelp.SetToolTip(this.btnSaveTxtLog, "将本次运行全部日志保存到按日期分类的本地文件夹");
            this.btnSaveTxtLog.UseVisualStyleBackColor = true;
            this.btnSaveTxtLog.Click += new System.EventHandler(this.btnSaveTxtLog_Click);
            // 
            // btnSaveCsvReport
            // 
            this.btnSaveCsvReport.Location = new System.Drawing.Point(295, 3);
            this.btnSaveCsvReport.Name = "btnSaveCsvReport";
            this.btnSaveCsvReport.Size = new System.Drawing.Size(106, 23);
            this.btnSaveCsvReport.TabIndex = 3;
            this.btnSaveCsvReport.Text = "导出CSV测试报告";
            this.toolTipHelp.SetToolTip(this.btnSaveCsvReport, "导出本次所有模块电压、温度测试数据，生成 Excel 可打开的 CSV 表格");
            this.btnSaveCsvReport.UseVisualStyleBackColor = true;
            this.btnSaveCsvReport.Click += new System.EventHandler(this.btnSaveCsvReport_Click);
            // 
            // panelProgress
            // 
            this.panelProgress.Controls.Add(this.progressTest);
            this.panelProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProgress.Location = new System.Drawing.Point(0, 33);
            this.panelProgress.Name = "panelProgress";
            this.panelProgress.Size = new System.Drawing.Size(800, 40);
            this.panelProgress.TabIndex = 2;
            // 
            // progressTest
            // 
            this.progressTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressTest.Location = new System.Drawing.Point(0, 0);
            this.progressTest.Margin = new System.Windows.Forms.Padding(10);
            this.progressTest.Name = "progressTest";
            this.progressTest.Size = new System.Drawing.Size(800, 40);
            this.progressTest.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 73);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lblSingleData);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rtbLog);
            this.splitContainer1.Size = new System.Drawing.Size(800, 355);
            this.splitContainer1.SplitterDistance = 100;
            this.splitContainer1.TabIndex = 3;
            // 
            // lblSingleData
            // 
            this.lblSingleData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSingleData.AutoSize = true;
            this.lblSingleData.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSingleData.Location = new System.Drawing.Point(339, 37);
            this.lblSingleData.Name = "lblSingleData";
            this.lblSingleData.Size = new System.Drawing.Size(136, 16);
            this.lblSingleData.TabIndex = 0;
            this.lblSingleData.Text = "（清空默认文字）";
            this.lblSingleData.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTipHelp.SetToolTip(this.lblSingleData, "实时展示当前模块电压温度，超标自动红字显示");
            // 
            // rtbLog
            // 
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Location = new System.Drawing.Point(0, 0);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbLog.Size = new System.Drawing.Size(800, 251);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelProgress);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.flowBtnTop);
            this.Name = "Form1";
            this.Text = "Form1";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.flowBtnTop.ResumeLayout(false);
            this.panelProgress.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblDevStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblTestStatus;
        private System.Windows.Forms.ToolStripStatusLabel lblTime;
        private System.Windows.Forms.Timer timerStatus;
        private System.Windows.Forms.FlowLayoutPanel flowBtnTop;
        private System.Windows.Forms.Button btnStartTest;
        private System.Windows.Forms.ToolTip toolTipHelp;
        private System.Windows.Forms.Button btnStopTest;
        private System.Windows.Forms.Button btnSaveTxtLog;
        private System.Windows.Forms.Button btnSaveCsvReport;
        private System.Windows.Forms.Panel panelProgress;
        private System.Windows.Forms.ProgressBar progressTest;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblSingleData;
        private System.Windows.Forms.RichTextBox rtbLog;
    }
}

