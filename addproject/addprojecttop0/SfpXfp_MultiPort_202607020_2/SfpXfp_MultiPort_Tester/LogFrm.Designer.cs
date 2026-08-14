namespace SFPXFP自动测试软件多端口
{
    partial class LogFrm
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
            this.logRichTextBox = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.端口1日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.端口2日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.端口3日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.端口4日志ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // logRichTextBox
            // 
            this.logRichTextBox.Location = new System.Drawing.Point(-2, 26);
            this.logRichTextBox.Name = "logRichTextBox";
            this.logRichTextBox.ReadOnly = true;
            this.logRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.logRichTextBox.Size = new System.Drawing.Size(804, 599);
            this.logRichTextBox.TabIndex = 0;
            this.logRichTextBox.Text = "";
            this.logRichTextBox.WordWrap = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.端口1日志ToolStripMenuItem,
            this.端口2日志ToolStripMenuItem,
            this.端口3日志ToolStripMenuItem,
            this.端口4日志ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(801, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 端口1日志ToolStripMenuItem
            // 
            this.端口1日志ToolStripMenuItem.Name = "端口1日志ToolStripMenuItem";
            this.端口1日志ToolStripMenuItem.Size = new System.Drawing.Size(75, 38);
            this.端口1日志ToolStripMenuItem.Text = "端口1日志";
            this.端口1日志ToolStripMenuItem.Click += new System.EventHandler(this.端口1日志ToolStripMenuItem_Click);
            // 
            // 端口2日志ToolStripMenuItem
            // 
            this.端口2日志ToolStripMenuItem.Name = "端口2日志ToolStripMenuItem";
            this.端口2日志ToolStripMenuItem.Size = new System.Drawing.Size(75, 38);
            this.端口2日志ToolStripMenuItem.Text = "端口2日志";
            this.端口2日志ToolStripMenuItem.Click += new System.EventHandler(this.端口2日志ToolStripMenuItem_Click);
            // 
            // 端口3日志ToolStripMenuItem
            // 
            this.端口3日志ToolStripMenuItem.Name = "端口3日志ToolStripMenuItem";
            this.端口3日志ToolStripMenuItem.Size = new System.Drawing.Size(75, 21);
            this.端口3日志ToolStripMenuItem.Text = "端口3日志";
            this.端口3日志ToolStripMenuItem.Click += new System.EventHandler(this.端口3日志ToolStripMenuItem_Click);
            // 
            // 端口4日志ToolStripMenuItem
            // 
            this.端口4日志ToolStripMenuItem.Name = "端口4日志ToolStripMenuItem";
            this.端口4日志ToolStripMenuItem.Size = new System.Drawing.Size(75, 21);
            this.端口4日志ToolStripMenuItem.Text = "端口4日志";
            this.端口4日志ToolStripMenuItem.Click += new System.EventHandler(this.端口4日志ToolStripMenuItem_Click);
            // 
            // LogFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 625);
            this.Controls.Add(this.logRichTextBox);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LogFrm";
            this.Text = "日志";
            this.Load += new System.EventHandler(this.LogFrm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox logRichTextBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 端口1日志ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 端口2日志ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 端口3日志ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 端口4日志ToolStripMenuItem;
    }
}