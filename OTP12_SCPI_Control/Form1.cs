using System;
using System.Windows.Forms;

namespace OTP12_SCPI_Control
{
    public partial class Form1 : Form
    {
        // 全局OTP设备驱动实例，整个窗口共用
        private readonly OTP12Driver _otpDriver;

        public Form1()
        {
            InitializeComponent();
            _otpDriver = new OTP12Driver(); // 实例化通信类
        }

        // 【连接设备】按钮点击事件
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = txtIp.Text.Trim();
            bool connectOk = _otpDriver.Connect(ip);
            if (connectOk)
            {
                lblOutput.Text = "✅ 设备TCP连接成功";
            }
            else
            {
                lblOutput.Text = "❌ 连接失败，检查IP、网线、设备上电";
            }
        }

        // 【断开连接】按钮点击事件
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _otpDriver.DisConnect();
            lblOutput.Text = "已断开设备连接";
        }

        // 【查询设备信息】按钮点击
        private void btnQueryIDN_Click(object sender, EventArgs e)
        {
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "请先连接设备！";
                return;
            }
            string info = _otpDriver.QueryDeviceInfo();
            lblOutput.Text = "设备信息：" + info;
        }

        // 【读取日期】按钮
        private void btnReadDate_Click(object sender, EventArgs e)
        {
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "请先连接设备！";
                return;
            }
            string date = _otpDriver.GetSystemDate();
            lblOutput.Text = "当前系统日期：" + date;
        }

        //读取单板序列
        private void btnReadBoardSN_Click(object sender, EventArgs e)
        {
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "【提示】未连接设备";
                return;
            }
            string res = _otpDriver.QueryBoardSN();
            lblOutput.Text = $"单板序列号：{res}";
        }

        // 读取系统时间
        private void btnReadTime_Click(object sender, EventArgs e)
        {
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "【提示】未连接设备";
                return;
            }
            string res = _otpDriver.GetSystemTime();
            lblOutput.Text = $"系统当前时间：{res}";
        }

        //查询当前告警
        private void btnQueryAlarm_Click(object sender, EventArgs e)
        {
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "【提示】未连接设备";
                return;
            }
            string res = _otpDriver.QueryCurrentAlarm();
            lblOutput.Text = $"当前告警信息：{res}";
        }

        //读取 OPM 通道功率
        private void btnReadOPMPower_Click(object sender, EventArgs e)
        {
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "【提示】未连接设备";
                return;
            }
            string res = _otpDriver.OPM_ReadPower(1);
            lblOutput.Text = $"OPM通道1光功率：{res}";
        }

        // 【设置日期】按钮 固定2026,06,25示例
        private void btnSetDate_Click(object sender, EventArgs e)
        {
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "请先连接设备！";
            }
            bool ok = _otpDriver.SetSystemDate(2026, 6, 25);
            if (ok)
                lblOutput.Text = "日期设置完成";
            else
                lblOutput.Text = "日期设置失败";
        }

        private void btnQueryAllBoard_Click(object sender, EventArgs e)
        {
            // 先判断是否连上设备
            if (!_otpDriver.IsConnected)
            {
                lblOutput.Text = "【提示】请先连接设备！";
                return;
            }
            // 调用封装好的查询全部单板SCPI方法
            string boardInfo = _otpDriver.QueryAllBoardCatalog();
            // 把返回内容显示到底部输出标签
            lblOutput.Text = $"全部槽位单板信息：{boardInfo}";
        }
    }
}