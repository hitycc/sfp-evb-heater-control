using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // 窗体初始化，预先填充SFP型号下拉选项
            //cboSFPType.Items.AddRange(new string[] { "SFP", "SFP+", "SFP28" });
            //cboSFPType.SelectedIndex = 0;
            // 初始状态栏文字
            lblStatus.Text = "当前状态：设备未连接";
            // 初始化日志
            AddLog("软件初始化完成，等待操作...");
        }

        // 封装统一写日志方法，所有地方直接调用
        private void AddLog(string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            // VS2008 不能用$插值，改用string.Format
            rtbLog.AppendText(string.Format("[{0}] {1}\r\n", time, msg));
            // 自动滚动到最新一行
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();
        }

        // 【连接设备】按钮点击事件
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string model = cboSFPType.Text;
            // 此处同步替换插值语法
            AddLog(string.Format("尝试连接设备，选择模块型号：{0}", model));
            lblStatus.Text = "当前状态：已连接测试设备";
        }

        // 【读取SFP参数】按钮点击事件
        private void btnReadSFP_Click(object sender, EventArgs e)
        {
            // 模拟读取硬件数据，后续对接串口就替换成真实解析数据
            txtVoltage.Text = "3.28";
            txtTemp.Text = "26.5";
            txPower.Text = "-5.12";
            rxPower.Text = "-4.87";
            AddLog("成功读取SFP模块实时参数");
        }

        // 【开始测试】按钮
        private void btnStartTest_Click(object sender, EventArgs e)
        {
            AddLog("已启动自动循环测试流程...");
        }

        // 【清空日志】按钮
        private void btnClearLog_Click(object sender, EventArgs e)
        {
            rtbLog.Clear();
            AddLog("日志已清空");
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
