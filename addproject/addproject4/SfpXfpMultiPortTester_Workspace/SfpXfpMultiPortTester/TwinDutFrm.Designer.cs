namespace SfpXfpMultiPortTester
{
    partial class TwinDutFrm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.连接ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.连接ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.软件使用说明ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(6, 20);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(345, 238);
            this.panel1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(15, 20);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(391, 254);
            this.panel2.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.连接ToolStripMenuItem,
            this.帮助ToolStripMenuItem,
            this.日志ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1118, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 连接ToolStripMenuItem
            // 
            this.连接ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.连接ToolStripMenuItem1});
            this.连接ToolStripMenuItem.Name = "连接ToolStripMenuItem";
            this.连接ToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.连接ToolStripMenuItem.Text = "通信连接";
            // 
            // 连接ToolStripMenuItem1
            // 
            this.连接ToolStripMenuItem1.Name = "连接ToolStripMenuItem1";
            this.连接ToolStripMenuItem1.Size = new System.Drawing.Size(100, 22);
            this.连接ToolStripMenuItem1.Text = "连接";
            // 
            // 帮助ToolStripMenuItem
            // 
            this.帮助ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.软件使用说明ToolStripMenuItem});
            this.帮助ToolStripMenuItem.Name = "帮助ToolStripMenuItem";
            this.帮助ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.帮助ToolStripMenuItem.Text = "帮助";
            this.帮助ToolStripMenuItem.Click += new System.EventHandler(this.帮助ToolStripMenuItem_Click);
            // 
            // 软件使用说明ToolStripMenuItem
            // 
            this.软件使用说明ToolStripMenuItem.Name = "软件使用说明ToolStripMenuItem";
            this.软件使用说明ToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.软件使用说明ToolStripMenuItem.Text = "软件使用说明";
            // 
            // 日志ToolStripMenuItem
            // 
            this.日志ToolStripMenuItem.Name = "日志ToolStripMenuItem";
            this.日志ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.日志ToolStripMenuItem.Text = "日志";
            this.日志ToolStripMenuItem.Click += new System.EventHandler(this.日志ToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Location = new System.Drawing.Point(685, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(421, 292);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Location = new System.Drawing.Point(24, 38);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(368, 274);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // QuadrupleDutFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1118, 725);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "QuadrupleDutFrm";
            this.Text = "SFPXFP双端口自动测试软件 V1.0.1    2026.07.20";
            this.Load += new System.EventHandler(this.TwinDutFrm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 连接ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 连接ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 软件使用说明ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ToolStripMenuItem 日志ToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
    }
}

