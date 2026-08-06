using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SfpXfpMultiPortTester
{
    public partial class TwinDutFrm : Form
    {
        TestSourceFrm frm1;
        //DUT2Form frm2;
        TestDut2Frm frm2;
        public TwinDutFrm()
        {
            InitializeComponent();
        }

        private void TwinDutFrm_Load(object sender, EventArgs e)
        {
            // 创建第一个子窗体
            frm1 = new TestSourceFrm();
            frm1.FormBorderStyle = FormBorderStyle.None; // 无边框
            frm1.Dock = DockStyle.Fill;                  // 填充Panel
            frm1.TopLevel = false;                       // 设置为非顶级窗体

            // 创建第二个子窗体
            //frm2 = new DUT2Form();
            frm2 = new TestDut2Frm();
            frm2.FormBorderStyle = FormBorderStyle.None; // 无边框
            frm2.Dock = DockStyle.Fill;                  // 填充Panel
            frm2.TopLevel = false;                       // 设置为非顶级窗体

            // 清空Panel内容
            panel1.Controls.Clear();
            panel2.Controls.Clear();

            // 将配置好的窗体添加到对应的Panel
            panel1.Controls.Add(frm1);
            panel2.Controls.Add(frm2);

            // 4. 设置可见性并刷新
            frm1.Visible = true; // 现在显示
            frm2.Visible = true; // 现在显示

            // 5. 强制布局和重绘
            // 对于容器和其子控件，调用 PerformLayout 和 Invalidate/Update 可能有帮助
            panel1.PerformLayout();
            panel2.PerformLayout();

            // 有时需要强制更新显示
            frm1.Update();
            frm2.Update();
            panel1.Update(); // Update() 比 Refresh() 更好，因为它只重绘无效区域
            panel2.Update();
            this.Update(); // 也可能需要更新父窗体
            timer1.Start();
        }

        private void 日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogFrm log_form = new LogFrm();
            try
            {
                log_form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            frm1.Update();
            frm2.Update();
            panel1.Update(); // Update() 比 Refresh() 更好，因为它只重绘无效区域
            panel2.Update();
            this.Update(); // 也可能需要更新父窗体
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
