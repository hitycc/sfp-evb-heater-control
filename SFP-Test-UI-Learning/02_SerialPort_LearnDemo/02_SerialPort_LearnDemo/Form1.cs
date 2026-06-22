using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;

namespace _02_SerialPort_LearnDemo
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort = new SerialPort();
        public Form1()
        {
            InitializeComponent();
            // 1. 初始化串口下拉框（COM、波特率）
            InitSerialComboBox();
            serialPort.DataReceived+= SerialPort_DataReceived;
        }
        /// <summary>
        /// 初始化串口号、波特率下拉列表
        /// </summary>
        private void InitSerialComboBox()
        {
            string[] allCom = SerialPort.GetPortNames();
            cboComPort.Items.AddRange(allCom);
            // 如果存在串口，默认选中第一个
            if(cboComPort.Items.Count>0)
            {
                cboComPort.SelectedIndex = 0;
            }
            //2. 填充常用波特率
            int[] baudList = { 9600, 19200, 38400, 115200 };
            foreach(int baud in baudList)
            {
                cboBaud.Items.Add(baud);
            }
            cboBaud.Text = "115200";
        }
        /// <summary>
        /// 统一打印日志，自动加时间戳、自动滚动到底部
        /// </summary>
        private void AddLog(string msg)
        {
            // 获取当前时分秒
            string time = DateTime.Now.ToString("HH:mm:ss");
            // VS2019支持字符串插值$，简洁拼接
            string logText = $"[{time}]{msg}\r\n";

            // 追加文字到日志框
            rtbSerialLog.AppendText(logText);

            // 光标移动到文本末尾
            rtbSerialLog.SelectionStart = rtbSerialLog.TextLength;

            // 自动滚动到光标位置（最新日志）
            rtbSerialLog.ScrollToCaret();
        }

        /// <summary>
        /// 串口收到数据自动触发（运行在子线程！不能直接操作控件）
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // 读取全部收到的字符串
            string recvStr = serialPort.ReadExisting();
            //跨线程调用主线程方法刷新UI
            this.Invoke(new Action<string>(HandleRecvData), recvStr);
        }
        /// <summary>
        /// 在主线程处理接收报文、解析数据、更新文本框
        /// </summary>
        private void HandleRecvData(string recvData)
        {
            // 打印接收日志
            AddLog($"[接受]{recvData}");
            // 解析设备返回报文 格式示例 VOLT=3.28,TEMP=26.5
            if(recvData.Contains("VOLT=")&&(recvData.Contains("TEMP=")))
            {

                // 按逗号分割两段数据
                string[] dataArr = recvData.Split(',');
                foreach(string item in dataArr)
                {
                    if(item.StartsWith("VOLT="))
                    {
                        // 去掉前缀，只保留数值
                        string voltVal = item.Replace("VOLT=", "");
                        txtVolt.Text = voltVal;
                    }
                    if(item.StartsWith("TEMP="))
                    {
                        string tempVal = item.Replace("TEMP=", "");
                        txtTemp.Text = tempVal;
                    }
                }
            }
        }
        private void btnOpenClose_Click(object sender, EventArgs e)
        {
            // 判断串口当前是否打开
            if(serialPort.IsOpen)
            {
                //关闭串口
                {
                    serialPort.Close();
                    btnOpenClose.Text="打开串口";
                    AddLog("串口已关闭");
                }
            }
            else
            {
                try
                {
                    // 配置串口参数
                    serialPort.PortName = cboComPort.Text;
                    serialPort.BaudRate = int.Parse(cboBaud.Text);
                    serialPort.DataBits = 8;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Parity = Parity.None;


                    // 打开串口
                    serialPort.Open();
                    btnOpenClose.Text= "关闭串口";
                    AddLog($"串口{serialPort.PortName}打开成功，波特率：{serialPort.BaudRate}");
                }
                catch(Exception ex)
                {
                    // 捕获所有异常：端口被占用、不存在、权限不足等
                    MessageBox.Show($"串口打开错误:{ex.Message}","错误");
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // 校验：串口未打开禁止发送
            if(!serialPort.IsOpen)
            {
                MessageBox.Show("请打开串口");
                return;
            }

            // 获取输入框指令，去除首尾空格
            string sendCmd = txtSendCmd.Text.Trim();
            // 空指令不发送
            if(string.IsNullOrEmpty(sendCmd))
            {
                return;
            }
            // 下发指令，自动追加换行符
            serialPort.WriteLine(sendCmd);
            //打印发送日志
            AddLog($"[发送]{sendCmd}");
        }
    }
}
