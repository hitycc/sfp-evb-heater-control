using System;
using System.Windows.Forms;

namespace LastEVBControlDemoApp
{
    public partial class Form1 : Form
    {
        private readonly SFP_EVB_Heater _heater;
        private const int DefaultSlot = 1; // 默认槽位1，可按需修改

        public Form1()
        {
            InitializeComponent();
            _heater = new SFP_EVB_Heater();
            // 初始状态：所有功能按钮禁用，只保留连接按钮可用
            SetControlEnable(false);
        }

        #region 连接控制功能
        private void btnConnect_Click(object sender, EventArgs e)
        {
            bool ok = _heater.Open();
            if (ok)
            {
                MessageBox.Show("设备连接成功！");
                lblConnectStatus.Text = "状态：已连接";
                SetControlEnable(true);
                // 连接后自动刷新一次数据
                btnGetData_Click(null, null);
                btnQueryAllPin_Click(null, null);
            }
            else
            {
                MessageBox.Show("连接失败，请检查设备IP、网线和供电");
                lblConnectStatus.Text = "状态：连接失败";
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            _heater.Close();
            MessageBox.Show("已断开设备连接");
            lblConnectStatus.Text = "状态：未连接";
            SetControlEnable(false);
            // 重置所有显示
            ResetAllDisplay();
        }

        /// <summary>
        /// 批量设置控件启用/禁用状态
        /// </summary>
        private void SetControlEnable(bool isEnable)
        {
            // 连接相关
            btnConnect.Enabled = !isEnable;
            btnDisconnect.Enabled = isEnable;

            // 电压设置
            txtVoltageInput.Enabled = isEnable;
            btnSetVoltage.Enabled = isEnable;

            // 模块使能
            btnPowerENOn.Enabled = isEnable;
            btnPowerENOff.Enabled = isEnable;

            // 引脚控制
            btnTxDisHigh.Enabled = isEnable;
            btnTxDisLow.Enabled = isEnable;
            btnRs0HighHigh.Enabled = isEnable;
            btnRs0HighLow.Enabled = isEnable;
            btnRs1HighHigh.Enabled = isEnable;
            btnRs1HighLow.Enabled = isEnable;

            // 状态查询
            btnQueryAllPin.Enabled = isEnable;

            // 数据刷新
            btnGetData.Enabled = isEnable;
        }

        /// <summary>
        /// 重置所有显示内容
        /// </summary>
        private void ResetAllDisplay()
        {
            // 核心数据
            lblPower.Text = "功率：-- mW";
            lblCurrent.Text = "电流：-- uA";
            lblVoltage.Text = "电压：-- mV";

            // 模块使能状态
            lblPowerENStatus.Text = "当前状态：--";

            // 引脚控制状态
            lblTxDisStatus.Text = "状态：--";
            lblRs0HighStatus.Text = "状态：--";
            lblRs1HighStatus.Text = "状态：--";

            // 引脚查询状态
            lblTxFalu.Text = "TX_FALU：--";
            lblRxLos.Text = "RX_LOS：--";
            lblABS.Text = "ABS：--";
        }
        #endregion

        #region 电压设置功能
        private void btnSetVoltage_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            // 验证输入的电压是否符合3.15~3.3V范围
            if (!double.TryParse(txtVoltageInput.Text, out double voltage))
            {
                MessageBox.Show("请输入有效的数字！");
                return;
            }

            if (voltage < 3.15 || voltage > 3.3)
            {
                MessageBox.Show("电压范围必须在3.15V~3.3V之间！");
                return;
            }

            // 执行设置
            bool ok = _heater.SetVoltage(voltage, DefaultSlot);
            if (ok)
            {
                MessageBox.Show($"电压设置成功！当前电压：{voltage}V");
                // 刷新电压显示
                btnGetData_Click(null, null);
            }
            else
            {
                MessageBox.Show("电压设置失败，请检查设备！");
            }
        }
        #endregion

        #region 模块使能功能
        private void btnPowerENOn_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetPowerEN(1, DefaultSlot);
            if (ok)
            {
                MessageBox.Show("模块使能成功！");
                lblPowerENStatus.Text = "当前状态：高电平(使能)";
            }
            else
            {
                MessageBox.Show("模块使能设置失败！");
            }
        }

        private void btnPowerENOff_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetPowerEN(0, DefaultSlot);
            if (ok)
            {
                MessageBox.Show("模块禁用成功！");
                lblPowerENStatus.Text = "当前状态：低电平(禁用)";
            }
            else
            {
                MessageBox.Show("模块禁用设置失败！");
            }
        }
        #endregion

        #region 引脚控制功能
        #region TX_DIS引脚
        private void btnTxDisHigh_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetTxDis(1, DefaultSlot);
            if (ok)
            {
                lblTxDisStatus.Text = "状态：高电平";
                MessageBox.Show("TX_DIS引脚设置为高电平成功！");
            }
            else
            {
                MessageBox.Show("TX_DIS引脚设置失败！");
            }
        }

        private void btnTxDisLow_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetTxDis(0, DefaultSlot);
            if (ok)
            {
                lblTxDisStatus.Text = "状态：低电平";
                MessageBox.Show("TX_DIS引脚设置为低电平成功！");
            }
            else
            {
                MessageBox.Show("TX_DIS引脚设置失败！");
            }
        }
        #endregion

        #region RS0_HIGH引脚
        private void btnRs0HighHigh_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetRs0High(1, DefaultSlot);
            if (ok)
            {
                lblRs0HighStatus.Text = "状态：高电平";
                MessageBox.Show("RS0_HIGH引脚设置为高电平成功！");
            }
            else
            {
                MessageBox.Show("RS0_HIGH引脚设置失败！");
            }
        }

        private void btnRs0HighLow_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetRs0High(0, DefaultSlot);
            if (ok)
            {
                lblRs0HighStatus.Text = "状态：低电平";
                MessageBox.Show("RS0_HIGH引脚设置为低电平成功！");
            }
            else
            {
                MessageBox.Show("RS0_HIGH引脚设置失败！");
            }
        }
        #endregion

        #region RS1_HIGH引脚
        private void btnRs1HighHigh_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetRs1High(1, DefaultSlot);
            if (ok)
            {
                lblRs1HighStatus.Text = "状态：高电平";
                MessageBox.Show("RS1_HIGH引脚设置为高电平成功！");
            }
            else
            {
                MessageBox.Show("RS1_HIGH引脚设置失败！");
            }
        }

        private void btnRs1HighLow_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            bool ok = _heater.SetRs1High(0, DefaultSlot);
            if (ok)
            {
                lblRs1HighStatus.Text = "状态：低电平";
                MessageBox.Show("RS1_HIGH引脚设置为低电平成功！");
            }
            else
            {
                MessageBox.Show("RS1_HIGH引脚设置失败！");
            }
        }
        #endregion
        #endregion

        #region 引脚状态查询功能
        private void btnQueryAllPin_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            // 一键查询所有只读引脚
            string txFalu = _heater.GetTxFalu(DefaultSlot);
            string rxLos = _heater.GetRxLos(DefaultSlot);
            string abs = _heater.GetABS(DefaultSlot);

            // 更新显示
            lblTxFalu.Text = $"TX_FALU：{(txFalu ?? "无数据")}";
            lblRxLos.Text = $"RX_LOS：{(rxLos ?? "无数据")}";
            lblABS.Text = $"ABS：{(abs ?? "无数据")}";

            // 同时更新可写引脚的当前状态
            lblPowerENStatus.Text = $"当前状态：{(_heater.GetPowerEN(DefaultSlot) ?? "--")}";
            lblTxDisStatus.Text = $"状态：{(_heater.GetTxDis(DefaultSlot) ?? "--")}";
            lblRs0HighStatus.Text = $"状态：{(_heater.GetRs0High(DefaultSlot) ?? "--")}";
            lblRs1HighStatus.Text = $"状态：{(_heater.GetRs1High(DefaultSlot) ?? "--")}";
        }
        #endregion

        #region 核心数据刷新功能
        private void btnGetData_Click(object sender, EventArgs e)
        {
            if (!_heater.IsOpen)
            {
                MessageBox.Show("请先连接设备！");
                return;
            }

            // 读取功率、电流、电压
            string power = _heater.GetPower(DefaultSlot);
            string current = _heater.GetCurrent(DefaultSlot);
            string voltage = _heater.GetVoltage(DefaultSlot);

            // 更新界面显示
            lblPower.Text = $"功率：{(power ?? "无数据")} mW";
            lblCurrent.Text = $"电流：{(current ?? "无数据")} uA";
            lblVoltage.Text = $"电压：{(voltage ?? "无数据")} mV";
        }
        #endregion
    }
}