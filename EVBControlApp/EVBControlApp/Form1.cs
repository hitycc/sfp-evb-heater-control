using System;
using System.Windows.Forms;

namespace EVBControlApp
{
    public partial class Form1 : Form
    {
        // 全局加热台控制对象
        private readonly SFP_EVB_Heater _heater;

        public Form1()
        {
            InitializeComponent();
            // 初始化通讯类
            _heater = new SFP_EVB_Heater();

            // 初始状态：断开和获取按钮禁用
            btnDisconnect.Enabled = false;
            btnGetData.Enabled = false;
        }

        // 连接按钮点击事件
        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool connected = _heater.Open();
            if (connected)
            {
                MessageBox.Show("设备连接成功！");
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnGetData.Enabled = true;
            }
            else
            {
                MessageBox.Show("连接失败，请检查IP和线路");
            }
        }

        // 断开按钮点击事件
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _heater.Close();
            MessageBox.Show("连接已断开");

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnGetData.Enabled = false;

            // 重置显示
            lblPower.Text = "功率：-- mW";
            lblCurrent.Text = "电流：-- uA";
            lblVoltage.Text = "电压：-- mV";
        }

        // 获取数据按钮点击事件
        private void btnGetData_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备");
                return;
            }

            // 读取数据
            string power = _heater.GetPower();
            string current = _heater.GetCurrent();
            string voltage = _heater.GetVoltage();

            // 更新界面显示
            lblPower.Text = $"功率：{(power ?? "无数据")} mW";
            lblCurrent.Text = $"电流：{(current ?? "无数据")} uA";
            lblVoltage.Text = $"电压：{(voltage ?? "无数据")} mV";
        }
    }
}