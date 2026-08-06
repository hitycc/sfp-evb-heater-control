namespace SfpXfpMultiPortTester
{
    partial class FourDutFrm
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
            this.components = new System.ComponentModel.Container();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.连接ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.连接ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.帮助ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.软件使用说明ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(6, 11);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(429, 357);
            this.panel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(6, 11);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(429, 357);
            this.panel1.TabIndex = 2;
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(6, 12);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(429, 365);
            this.panel3.TabIndex = 1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.连接ToolStripMenuItem,
            this.帮助ToolStripMenuItem,
            this.日志ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(914, 25);
            this.menuStrip1.TabIndex = 3;
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
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 31);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(441, 383);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Location = new System.Drawing.Point(465, 31);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(444, 383);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.panel3);
            this.groupBox3.Location = new System.Drawing.Point(12, 420);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(441, 383);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.panel4);
            this.groupBox4.Location = new System.Drawing.Point(459, 420);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(450, 383);
            this.groupBox4.TabIndex = 6;
            this.groupBox4.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.Location = new System.Drawing.Point(12, 12);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(429, 365);
            this.panel4.TabIndex = 2;
            // 
            // FourDutFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 830);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.menuStrip1);
            this.Name = "FourDutFrm";
            this.Text = "4端口";
            this.Load += new System.EventHandler(this.FourDutFrm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 连接ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 连接ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem 帮助ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 软件使用说明ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 日志ToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Panel panel4;
    }
}