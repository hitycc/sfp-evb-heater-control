using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SfpXfpMultiPortTester
{
    public partial class FourDutFrm : Form
    {
        TestSourceFrm frm1;

        TestDut2Frm frm2;

        TestDut3Frm frm3;

        TestDut4Frm frm4;

        public FourDutFrm()
        {
            InitializeComponent();
        }

        private void FourDutFrm_Load(object sender, EventArgs e)
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
            frm2.Dock = DockStyle.Fill;                  // 填充Pane2
            frm2.TopLevel = false;                       // 设置为非顶级窗体

            //创建第三个子窗体
            frm3 = new TestDut3Frm();
            frm3.FormBorderStyle = FormBorderStyle.None; // 无边框
            frm3.Dock = DockStyle.Fill;                  // 填充Pane3
            frm3.TopLevel = false;                       // 设置为非顶级窗体

            //创建第四个子窗体
            frm4 = new TestDut4Frm();
            frm4.FormBorderStyle = FormBorderStyle.None; // 无边框
            frm4.Dock = DockStyle.Fill;                  // 填充Pane4
            frm4.TopLevel = false;                       // 设置为非顶级窗体

            // 清空Panel内容
            panel1.Controls.Clear();
            panel2.Controls.Clear();
            panel3.Controls.Clear();
            panel4.Controls.Clear();

            // 将配置好的窗体添加到对应的Panel
            panel1.Controls.Add(frm1);
            panel2.Controls.Add(frm2);
            panel3.Controls.Add(frm3);
            panel4.Controls.Add(frm4);

            // 4. 设置可见性并刷新
            frm1.Visible = true; // 现在显示
            frm2.Visible = true; // 现在显示
            frm3.Visible = true; // 现在显示
            frm4.Visible = true; // 现在显示

            // 5. 强制布局和重绘
            // 对于容器和其子控件，调用 PerformLayout 和 Invalidate/Update 可能有帮助
            panel1.PerformLayout();
            panel2.PerformLayout();
            panel3.PerformLayout();
            panel4.PerformLayout();

            // 有时需要强制更新显示
            frm1.Update();
            frm2.Update();
            frm3.Update();
            frm4.Update();
            panel1.Update(); // Update() 比 Refresh() 更好，因为它只重绘无效区域
            panel2.Update();
            panel3.Update(); // Update() 比 Refresh() 更好，因为它只重绘无效区域
            panel4.Update();
            this.Update(); // 也可能需要更新父窗体
            // timer1不再启动：子窗体各自有独立定时器负责DDM刷新，父窗体空转timer浪费CPU且会引起不必要的重绘

            // 注册窗体关闭事件，在关闭时停止所有子窗体的定时器，避免关闭卡顿
            this.FormClosing += FourDutFrm_FormClosing;
        }

        private void FourDutFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 停止所有子窗体的定时器，防止关闭时定时器仍在执行阻塞UI线程
                frm1?.StopTimers();
                frm2?.StopTimers();
                frm3?.StopTimers();
                frm4?.StopTimers();
                // 等待一小段时间让正在执行的定时器回调完成
                System.Threading.Thread.Sleep(100);
            }
            catch { }
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
            // 移除了所有强制Update()调用，避免同步重绘阻塞UI线程
            // 子窗体通过各自的timer异步更新DDM，WinForms会自动处理重绘
        }
    }
}
