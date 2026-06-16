using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace LastEVBControlDemoApp
{
    /// <summary>
    /// SFP EVB加热台 TCP 全功能控制类
    /// 完整覆盖Excel指令表所有功能
    /// </summary>
    public class SFP_EVB_Heater
    {
        private readonly object _lock = new object();
        private Socket Client;

        // 设备配置属性
        public string DefaultIP { get; set; }
        public int DefaultPort { get; set; }
        public int DefaultTimeout { get; set; }

        // 连接状态只读属性
        public bool IsOpen => Client != null && Client.Connected;

        // 构造函数：初始化默认参数
        public SFP_EVB_Heater()
        {
            DefaultIP = "129.168.1.133";
            DefaultPort = 9000;
            DefaultTimeout = 5000;
            Client = null;
        }

        #region 连接方法重载
        public bool Open() => Open(DefaultIP, DefaultPort, DefaultTimeout);
        public bool Open(string ipAddress) => Open(ipAddress, DefaultPort, DefaultTimeout);
        public bool Open(string ipAddress, int port) => Open(ipAddress, port, DefaultTimeout);

        /// <summary>
        /// 核心连接方法
        /// </summary>
        public bool Open(string ipAddress, int port, int timeOut)
        {
            if (string.IsNullOrEmpty(ipAddress)) ipAddress = DefaultIP;
            if (port <= 0) port = DefaultPort;
            if (timeOut <= 0) timeOut = DefaultTimeout;

            lock (_lock)
            {
                if (IsOpen) return true;
                try
                {
                    Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        ReceiveTimeout = timeOut,
                        SendTimeout = timeOut
                    };
                    Client.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
                    return true;
                }
                catch
                {
                    Client?.Close();
                    Client = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// 断开设备连接
        /// </summary>
        public void Close()
        {
            lock (_lock)
            {
                if (IsOpen)
                {
                    Client.Close();
                    Client = null;
                }
            }
        }
        #endregion

        #region 通用指令收发方法
        public string SendCommand(string cmd) => SendCommand(cmd, 300);

        /// <summary>
        /// 核心指令收发方法
        /// </summary>
        public string SendCommand(string cmd, int delay)
        {
            lock (_lock)
            {
                if (!IsOpen) return null;
                try
                {
                    byte[] wbuf = Encoding.UTF8.GetBytes(cmd + "\r\n");
                    Client.Send(wbuf, SocketFlags.None);
                    Thread.Sleep(delay);
                    byte[] rbuf = new byte[1024];
                    int count = Client.Receive(rbuf, SocketFlags.None);
                    if (count > 0)
                        return Encoding.UTF8.GetString(rbuf, 0, count).Trim();
                }
                catch { }
                return null;
            }
        }
        #endregion

        #region 一、EVB 功率/电流/电压 读写功能
        /// <summary>
        /// 查询槽位功率
        /// </summary>
        public string GetPower(int slot = 1)
        {
            string cmd = $"evb{slot}:getpower?";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res)) return null;
            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        /// <summary>
        /// 查询槽位电流
        /// </summary>
        public string GetCurrent(int slot = 1)
        {
            string cmd = $"evb{slot}:getcurrent?";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res)) return null;
            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        /// <summary>
        /// 查询槽位电压
        /// </summary>
        public string GetVoltage(int slot = 1)
        {
            string cmd = $"evb{slot}:getvoltage?";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res)) return null;
            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        /// <summary>
        /// 设置槽位电压（范围3.15~3.3V）
        /// </summary>
        public bool SetVoltage(double voltage, int slot = 1)
        {
            string cmd = $"evb{slot}:setvoltage {voltage}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("set Voltage ok!");
        }
        #endregion

        #region 二、IO 引脚控制功能（完整覆盖Excel所有引脚）
        #region 1. PowerEN 模块使能引脚
        /// <summary>
        /// 设置模块使能引脚
        /// </summary>
        public bool SetPowerEN(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setPowerEN {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"POWER_EN:{state}");
        }

        /// <summary>
        /// 查询模块使能引脚状态
        /// </summary>
        public string GetPowerEN(int slot = 1)
        {
            string cmd = $"IO{slot}:getPowerEN";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("POWER_EN:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 2. TxDis 引脚
        /// <summary>
        /// 设置TX_DIS引脚
        /// </summary>
        public bool SetTxDis(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setTxDis {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"TX_DIS:{state}");
        }

        /// <summary>
        /// 查询TX_DIS引脚状态
        /// </summary>
        public string GetTxDis(int slot = 1)
        {
            string cmd = $"IO{slot}:getTxDis";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("TX_DIS:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 3. Rs0High 引脚
        /// <summary>
        /// 设置RS0_HIGH引脚
        /// </summary>
        public bool SetRs0High(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setRs0High {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"RS0_HIGH:{state}");
        }

        /// <summary>
        /// 查询RS0_HIGH引脚状态
        /// </summary>
        public string GetRs0High(int slot = 1)
        {
            string cmd = $"IO{slot}:getRs0High";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("RS0_HIGH:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 4. Rs1High 引脚
        /// <summary>
        /// 设置RS1_HIGH引脚
        /// </summary>
        public bool SetRs1High(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setRs1High {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"RS1_HIGH:{state}");
        }

        /// <summary>
        /// 查询RS1_HIGH引脚状态
        /// </summary>
        public string GetRs1High(int slot = 1)
        {
            string cmd = $"IO{slot}:getRs1High";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("RS1_HIGH:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 5. 只读引脚查询（Excel完整覆盖）
        /// <summary>
        /// 查询TX_FALU引脚状态
        /// </summary>
        public string GetTxFalu(int slot = 1)
        {
            string cmd = $"IO{slot}:getTxFalu";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("TX_FALU:")) return null;
            return res.Split(':')[1].Trim();
        }

        /// <summary>
        /// 查询RX_LOS引脚状态
        /// </summary>
        public string GetRxLos(int slot = 1)
        {
            string cmd = $"IO{slot}:getRxLos";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("RX_LOS:")) return null;
            return res.Split(':')[1].Trim();
        }

        /// <summary>
        /// 查询ABS引脚状态
        /// </summary>
        public string GetABS(int slot = 1)
        {
            string cmd = $"IO{slot}:getABS";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("ABS:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion
        #endregion

        #region 三、IIC 读写功能（Excel完整覆盖）
        /// <summary>
        /// IIC 写入数据
        /// </summary>
        public bool IIC_Set(string deviceAddr, string regAddr, string dataLength, string data, int slot = 1)
        {
            string cmd = $"IIC{slot}:set {deviceAddr},{regAddr},{dataLength},{data}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("iic set ok");
        }

        /// <summary>
        /// IIC 读取数据
        /// </summary>
        public string IIC_Get(string deviceAddr, string regAddr, string dataLength, int slot = 1)
        {
            string cmd = $"IIC{slot}:get {deviceAddr},{regAddr},{dataLength}";
            return SendCommand(cmd);
        }
        #endregion

        #region 四、设备IP设置功能
        /// <summary>
        /// 设置设备IP地址
        /// </summary>
        public bool SetDeviceIP(string newIP)
        {
            string cmd = $"setip {newIP}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("set ip ok");
        }
        #endregion
    }
}