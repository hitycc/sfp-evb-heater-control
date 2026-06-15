using System;
using System.Windows.Forms;

namespace EVBHeaterControl
{
    public partial class Form1 : Form
    {
        private readonly SFP_EVB_Heater _heater;

        public Form1()
        {
            InitializeComponent();
            _heater = new SFP_EVB_Heater();
            // 初始禁用断开、获取按钮
            btnDisconnect.Enabled = false;
            btnGetData.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool ok = _heater.Open();
            if (ok)
            {
                MessageBox.Show("设备连接成功！");
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnGetData.Enabled = true;
            }
            else
            {
                MessageBox.Show("连接失败，请检查设备IP与线路");
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _heater.Close();
            MessageBox.Show("已断开设备连接");

            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            btnGetData.Enabled = false;

            lblPower.Text = "功率：-- mW";
            lblCurrent.Text = "电流：-- uA";
            lblVoltage.Text = "电压：-- mV";
        }

        private void btnGetData_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }
            string power = _heater.GetPower();
            string current = _heater.GetCurrent();
            string voltage = _heater.GetVoltage();

            lblPower.Text = $"功率：{(power ?? "无数据")} mW";
            lblCurrent.Text = $"电流：{(current ?? "无数据")} uA";
            lblVoltage.Text = $"电压：{(voltage ?? "无数据")} mV";
        }
    }
}