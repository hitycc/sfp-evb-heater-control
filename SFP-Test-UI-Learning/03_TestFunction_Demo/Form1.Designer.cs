
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
            this.btnSaveTxtLog = new System.Windows.Forms.Button();
            this.btnSaveCsvReport = new System.Windows.Forms.Button();
            this.panelProgress = new System.Windows.Forms.Panel();
            this.progressTest = new System.Windows.Forms.ProgressBar();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.toolTipHelp = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1.SuspendLayout();
            this.flowBtnTop.SuspendLayout();
            this.panelProgress.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
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
            this.flowBtnTop.BackColor = System.Drawing.Color.Chartreuse;
            this.flowBtnTop.Controls.Add(this.btnStartTest);
            this.flowBtnTop.Controls.Add(this.btnSaveTxtLog);
            this.flowBtnTop.Controls.Add(this.btnSaveCsvReport);
            this.flowBtnTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowBtnTop.Location = new System.Drawing.Point(0, 0);
            this.flowBtnTop.Name = "flowBtnTop";
            this.flowBtnTop.Size = new System.Drawing.Size(800, 36);
            this.flowBtnTop.TabIndex = 1;
            // 
            // btnStartTest
            // 
            this.btnStartTest.Location = new System.Drawing.Point(3, 3);
            this.btnStartTest.Name = "btnStartTest";
            this.btnStartTest.Size = new System.Drawing.Size(99, 23);
            this.btnStartTest.TabIndex = 0;
            this.btnStartTest.Text = "开始批量测试";
            this.toolTipHelp.SetToolTip(this.btnStartTest, "模拟多通道SFP自动测试，进度条实时更新测试进度");
            this.btnStartTest.UseVisualStyleBackColor = true;
            this.btnStartTest.Click += new System.EventHandler(this.btnStartTest_Click);
            // 
            // btnSaveTxtLog
            // 
            this.btnSaveTxtLog.Location = new System.Drawing.Point(108, 3);
            this.btnSaveTxtLog.Name = "btnSaveTxtLog";
            this.btnSaveTxtLog.Size = new System.Drawing.Size(98, 23);
            this.btnSaveTxtLog.TabIndex = 1;
            this.btnSaveTxtLog.Text = "保存TXT日志";
            this.toolTipHelp.SetToolTip(this.btnSaveTxtLog, "将本次运行全部日志保存到按日期分类的本地文件夹");
            this.btnSaveTxtLog.UseVisualStyleBackColor = true;
            this.btnSaveTxtLog.Click += new System.EventHandler(this.btnSaveTxtLog_Click);
            // 
            // btnSaveCsvReport
            // 
            this.btnSaveCsvReport.Location = new System.Drawing.Point(212, 3);
            this.btnSaveCsvReport.Name = "btnSaveCsvReport";
            this.btnSaveCsvReport.Size = new System.Drawing.Size(115, 23);
            this.btnSaveCsvReport.TabIndex = 2;
            this.btnSaveCsvReport.Text = "导出CSV测试报告";
            this.toolTipHelp.SetToolTip(this.btnSaveCsvReport, "导出本次所有模块电压、温度测试数据，生成Excel可打开的CSV表格");
            this.btnSaveCsvReport.UseVisualStyleBackColor = true;
            this.btnSaveCsvReport.Click += new System.EventHandler(this.btnSaveCsvReport_Click);
            // 
            // panelProgress
            // 
            this.panelProgress.Controls.Add(this.progressTest);
            this.panelProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProgress.Location = new System.Drawing.Point(0, 36);
            this.panelProgress.Name = "panelProgress";
            this.panelProgress.Size = new System.Drawing.Size(800, 60);
            this.panelProgress.TabIndex = 2;
            // 
            // progressTest
            // 
            this.progressTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressTest.Location = new System.Drawing.Point(0, 0);
            this.progressTest.Margin = new System.Windows.Forms.Padding(10);
            this.progressTest.Name = "progressTest";
            this.progressTest.Size = new System.Drawing.Size(800, 60);
            this.progressTest.TabIndex = 0;
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 96);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.rtbLog);
            this.splitMain.Size = new System.Drawing.Size(800, 332);
            this.splitMain.SplitterDistance = 80;
            this.splitMain.TabIndex = 3;
            // 
            // rtbLog
            // 
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Location = new System.Drawing.Point(0, 0);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbLog.Size = new System.Drawing.Size(800, 248);
            this.rtbLog.TabIndex = 0;
            this.rtbLog.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.panelProgress);
            this.Controls.Add(this.flowBtnTop);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.flowBtnTop.ResumeLayout(false);
            this.panelProgress.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
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
        private System.Windows.Forms.Button btnSaveTxtLog;
        private System.Windows.Forms.Button btnSaveCsvReport;
        private System.Windows.Forms.Panel panelProgress;
        private System.Windows.Forms.ProgressBar progressTest;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.RichTextBox rtbLog;
    }
}

