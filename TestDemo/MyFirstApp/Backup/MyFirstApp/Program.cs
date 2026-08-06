using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace VP_Communication.Communication.Heater
{
    /// <summary>
    /// SFP EVB加热台 TCP 全功能控制类
    /// 适配 VS2008 + .NET 3.5
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
        public bool IsOpen
        {
            get
            {
                return Client != null && Client.Connected;
            }
        }
        // 构造函数：初始化默认参数
        public SFP_EVB_Heater()
        {
            DefaultIP = "129.168.1.133"; // 你的设备真实IP
            DefaultPort = 9000;
            DefaultTimeout = 5000; // 已调整为5秒，避免超时
            Client = null;
        }

        #region 连接方法重载（适配VS2008，无默认参数）
        public bool Open()
        {
            return Open(DefaultIP, DefaultPort, DefaultTimeout);
        }

        public bool Open(string ipAddress)
        {
            return Open(ipAddress, DefaultPort, DefaultTimeout);
        }

        public bool Open(string ipAddress, int port)
        {
            return Open(ipAddress, port, DefaultTimeout);
        }
        #endregion

        /// <summary>
        /// 核心连接方法
        /// </summary>
        public bool Open(string ipAddress, int port, int timeOut)
        {
            if (ipAddress == null)
                ipAddress = DefaultIP;
            if (port <= 0)
                port = DefaultPort;
            if (timeOut <= 0)
                timeOut = DefaultTimeout;

            lock (_lock)
            {
                if (IsOpen)
                {
                    return true;
                }

                try
                {
                    Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Client.ReceiveTimeout = timeOut;
                    Client.SendTimeout = timeOut;

                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    Client.Connect(endPoint);
                    Console.WriteLine("设备连接成功");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("连接失败：{0}", ex.Message));
                    if (Client != null)
                    {
                        Client.Close();
                        Client = null;
                    }
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
                    Console.WriteLine("连接已断开");
                }
            }
        }

        #region 通用指令收发方法重载
        public string SendCommand(string cmd)
        {
            return SendCommand(cmd, 300); // 已调整为300ms延迟，适配设备响应
        }
        #endregion

        /// <summary>
        /// 核心指令收发方法
        /// </summary>
        public string SendCommand(string cmd, int delay)
        {
            lock (_lock)
            {
                if (!IsOpen)
                {
                    return null;
                }

                try
                {
                    byte[] wbuf = Encoding.UTF8.GetBytes(cmd + "\r\n");
                    Client.Send(wbuf, SocketFlags.None);
                    Thread.Sleep(delay);

                    byte[] rbuf = new byte[1024];
                    int count = Client.Receive(rbuf, SocketFlags.None);
                    if (count > 0)
                    {
                        string res = Encoding.UTF8.GetString(rbuf, 0, count).Trim();
                        Console.WriteLine(string.Format("发送指令：{0}\n设备返回：{1}", cmd, res));
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("指令异常：{0}", ex.Message));
                }
                return null;
            }
        }

        #region 一、IIC 读写功能（完整覆盖Excel指令）
        #region IIC_Set 重载
        public bool IIC_Set(int slot)
        {
            return IIC_Set(slot, "a0", "0", "5", "a1,9,1b,2c,3d");
        }

        public bool IIC_Set(int slot, string deviceAddr)
        {
            return IIC_Set(slot, deviceAddr, "0", "5", "a1,9,1b,2c,3d");
        }

        public bool IIC_Set(int slot, string deviceAddr, string regAddr)
        {
            return IIC_Set(slot, deviceAddr, regAddr, "5", "a1,9,1b,2c,3d");
        }

        public bool IIC_Set(int slot, string deviceAddr, string regAddr, string dataLength)
        {
            return IIC_Set(slot, deviceAddr, regAddr, dataLength, "a1,9,1b,2c,3d");
        }
        #endregion

        /// <summary>
        /// IIC 写入数据
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <param name="deviceAddr">器件地址</param>
        /// <param name="regAddr">寄存器地址</param>
        /// <param name="dataLength">数据长度</param>
        /// <param name="data">要写入的16进制数据，逗号分隔</param>
        /// <returns>true=写入成功</returns>
        public bool IIC_Set(int slot, string deviceAddr, string regAddr, string dataLength, string data)
        {
            string cmd = string.Format("IIC{0}:set {1},{2},{3},{4}", slot, deviceAddr, regAddr, dataLength, data);
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("iic set ok");
        }

        #region IIC_Get 重载
        public string IIC_Get(int slot)
        {
            return IIC_Get(slot, "a0", "0", "9");
        }

        public string IIC_Get(int slot, string deviceAddr)
        {
            return IIC_Get(slot, deviceAddr, "0", "9");
        }

        public string IIC_Get(int slot, string deviceAddr, string regAddr)
        {
            return IIC_Get(slot, deviceAddr, regAddr, "9");
        }
        #endregion

        /// <summary>
        /// IIC 读取数据
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <param name="deviceAddr">器件地址</param>
        /// <param name="regAddr">寄存器地址</param>
        /// <param name="dataLength">要读取的数据长度</param>
        /// <returns>设备返回的完整数据</returns>
        public string IIC_Get(int slot, string deviceAddr, string regAddr, string dataLength)
        {
            string cmd = string.Format("IIC{0}:get {1},{2},{3}", slot, deviceAddr, regAddr, dataLength);
            return SendCommand(cmd);
        }
        #endregion

        #region 二、EVB 功率/电流/电压 读写功能（完整覆盖Excel指令）
        #region GetPower 重载
        public string GetPower()
        {
            return GetPower(1);
        }
        #endregion

        /// <summary>
        /// 查询槽位功率
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>功率数值（单位mW）</returns>
        public string GetPower(int slot)
        {
            string cmd = string.Format("evb{0}:getpower?", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res))
                return null;

            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        #region GetCurrent 重载
        public string GetCurrent()
        {
            return GetCurrent(1);
        }
        #endregion

        /// <summary>
        /// 查询槽位电流
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>电流数值（单位uA）</returns>
        public string GetCurrent(int slot)
        {
            string cmd = string.Format("evb{0}:getcurrent?", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res))
                return null;

            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        #region GetVoltage 重载
        public string GetVoltage()
        {
            return GetVoltage(1);
        }
        #endregion

        /// <summary>
        /// 查询槽位电压
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>电压数值（单位mV）</returns>
        public string GetVoltage(int slot)
        {
            string cmd = string.Format("evb{0}:getvoltage?", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res))
                return null;

            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        #region SetVoltage 重载
        public bool SetVoltage(int slot)
        {
            return SetVoltage(slot, 3.3);
        }
        #endregion

        /// <summary>
        /// 设置槽位电压（范围3.15~3.3V）
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <param name="voltage">电压值（单位V，如3.3）</param>
        /// <returns>true=设置成功</returns>
        public bool SetVoltage(int slot, double voltage)
        {
            string cmd = string.Format("evb{0}:setvoltage {1}", slot, voltage);
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("set Voltage ok!");
        }
        #endregion

        #region 三、IO 引脚控制功能（完整覆盖Excel指令）
        #region 1. PowerEN 模块使能引脚
        #region SetPowerEN 重载
        public bool SetPowerEN(int slot)
        {
            return SetPowerEN(slot, 1);
        }
        #endregion

        /// <summary>
        /// 设置模块使能引脚
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <param name="state">1=高电平使能，0=低电平禁用</param>
        /// <returns>true=设置成功</returns>
        public bool SetPowerEN(int slot, int state)
        {
            string cmd = string.Format("IO{0}:setPowerEN {1}", slot, state);
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains(string.Format("POWER_EN:{0}", state));
        }

        #region GetPowerEN 重载
        public string GetPowerEN()
        {
            return GetPowerEN(1);
        }
        #endregion

        /// <summary>
        /// 查询模块使能引脚状态
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>1=高电平，0=低电平</returns>
        public string GetPowerEN(int slot)
        {
            string cmd = string.Format("IO{0}:getPowerEN", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res) || !res.Contains("POWER_EN:"))
                return null;

            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 2. TxDis 引脚
        #region SetTxDis 重载
        public bool SetTxDis(int slot)
        {
            return SetTxDis(slot, 0);
        }
        #endregion

        /// <summary>
        /// 设置TX_DIS引脚
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <param name="state">1=高电平，0=低电平</param>
        /// <returns>true=设置成功</returns>
        public bool SetTxDis(int slot, int state)
        {
            string cmd = string.Format("IO{0}:setTxDis {1}", slot, state);
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains(string.Format("TX_DIS:{0}", state));
        }

        #region GetTxDis 重载
        public string GetTxDis()
        {
            return GetTxDis(1);
        }
        #endregion

        /// <summary>
        /// 查询TX_DIS引脚状态
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>1=高电平，0=低电平</returns>
        public string GetTxDis(int slot)
        {
            string cmd = string.Format("IO{0}:getTxDis", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res) || !res.Contains("TX_DIS:"))
                return null;

            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 3. Rs0High 引脚
        #region SetRs0High 重载
        public bool SetRs0High(int slot)
        {
            return SetRs0High(slot, 1);
        }
        #endregion

        /// <summary>
        /// 设置RS0_HIGH引脚
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <param name="state">1=高电平，0=低电平</param>
        /// <returns>true=设置成功</returns>
        public bool SetRs0High(int slot, int state)
        {
            string cmd = string.Format("IO{0}:setRs0High {1}", slot, state);
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains(string.Format("RS0_HIGH:{0}", state));
        }

        #region GetRs0High 重载
        public string GetRs0High()
        {
            return GetRs0High(1);
        }
        #endregion

        /// <summary>
        /// 查询RS0_HIGH引脚状态
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>1=高电平，0=低电平</returns>
        public string GetRs0High(int slot)
        {
            string cmd = string.Format("IO{0}:getRs0High", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res) || !res.Contains("RS0_HIGH:"))
                return null;

            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 4. Rs1High 引脚
        #region SetRs1High 重载
        public bool SetRs1High(int slot)
        {
            return SetRs1High(slot, 1);
        }
        #endregion

        /// <summary>
        /// 设置RS1_HIGH引脚
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <param name="state">1=高电平，0=低电平</param>
        /// <returns>true=设置成功</returns>
        public bool SetRs1High(int slot, int state)
        {
            string cmd = string.Format("IO{0}:setRs1High {1}", slot, state);
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains(string.Format("RS1_HIGH:{0}", state));
        }

        #region GetRs1High 重载
        public string GetRs1High()
        {
            return GetRs1High(1);
        }
        #endregion

        /// <summary>
        /// 查询RS1_HIGH引脚状态
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>1=高电平，0=低电平</returns>
        public string GetRs1High(int slot)
        {
            string cmd = string.Format("IO{0}:getRs1High", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res) || !res.Contains("RS1_HIGH:"))
                return null;

            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 5. 其他只读引脚查询
        #region GetTxFalu 重载
        public string GetTxFalu()
        {
            return GetTxFalu(1);
        }
        #endregion

        /// <summary>
        /// 查询TX_FALU引脚状态
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>1=高电平，0=低电平</returns>
        public string GetTxFalu(int slot)
        {
            string cmd = string.Format("IO{0}:getTxFalu", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res) || !res.Contains("TX_FALU:"))
                return null;

            return res.Split(':')[1].Trim();
        }

        #region GetRxLos 重载
        public string GetRxLos()
        {
            return GetRxLos(1);
        }
        #endregion

        /// <summary>
        /// 查询RX_LOS引脚状态
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>1=高电平，0=低电平</returns>
        public string GetRxLos(int slot)
        {
            string cmd = string.Format("IO{0}:getRxLos", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res) || !res.Contains("RX_LOS:"))
                return null;

            return res.Split(':')[1].Trim();
        }

        #region GetABS 重载
        public string GetABS()
        {
            return GetABS(1);
        }
        #endregion

        /// <summary>
        /// 查询ABS引脚状态
        /// </summary>
        /// <param name="slot">槽位号</param>
        /// <returns>1=高电平，0=低电平</returns>
        public string GetABS(int slot)
        {
            string cmd = string.Format("IO{0}:getABS", slot);
            string res = SendCommand(cmd);

            if (string.IsNullOrEmpty(res) || !res.Contains("ABS:"))
                return null;

            return res.Split(':')[1].Trim();
        }
        #endregion
        #endregion

        #region 四、系统功能（完整覆盖Excel指令）
        /// <summary>
        /// 设置设备IP地址
        /// </summary>
        /// <param name="newIp">新的IP地址，格式129.168.1.xxx</param>
        /// <returns>true=设置成功</returns>
        public bool SetDeviceIP(string newIp)
        {
            string cmd = string.Format("setip {0}", newIp);
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains(string.Format("setip:{0}", newIp));
        }

        /// <summary>
        /// 查询设备支持的所有命令
        /// </summary>
        /// <returns>设备返回的帮助信息</returns>
        public string GetHelp()
        {
            return SendCommand("help");
        }
        #endregion
    }

    /// <summary>
    /// 测试入口：完整功能演示
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            SFP_EVB_Heater heater = new SFP_EVB_Heater();

            // 1. 连接设备
            Console.WriteLine("===== 正在连接设备 =====");
            if (!heater.Open())
            {
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return;
            }

            // 2. 基础信息查询演示
            Console.WriteLine("\n===== 基础信息查询 =====");
            string power = heater.GetPower();
            string current = heater.GetCurrent();
            string voltage = heater.GetVoltage();

            if (!string.IsNullOrEmpty(power))
                Console.WriteLine("当前功率：" + power + " mW");
            if (!string.IsNullOrEmpty(current))
                Console.WriteLine("当前电流：" + current + " uA");
            if (!string.IsNullOrEmpty(voltage))
                Console.WriteLine("当前电压：" + voltage + " mV");

            // 3. 引脚状态查询演示
            Console.WriteLine("\n===== 引脚状态查询 =====");
            string powerEN = heater.GetPowerEN();
            string txDis = heater.GetTxDis();
            string rs0High = heater.GetRs0High();

            if (!string.IsNullOrEmpty(powerEN))
                Console.WriteLine("模块使能状态：" + powerEN);
            if (!string.IsNullOrEmpty(txDis))
                Console.WriteLine("TX_DIS状态：" + txDis);
            if (!string.IsNullOrEmpty(rs0High))
                Console.WriteLine("RS0_HIGH状态：" + rs0High);

             // ========== 新增：IIC 读测试代码（对应你表格指令）==========
            Console.WriteLine("\n===== IIC 读写测试 =====");
            // 示例2：IIC读取 IIC2:get a0,0,9  就是你要的查询指令
            string iicReadResult = heater.IIC_Get(2, "a0", "0", "9");
            Console.WriteLine("IIC读取指令：IIC2:get a0,0,9");
            Console.WriteLine("IIC读取返回数据：\n" + iicReadResult);
            // ==========================================================

            // 4. 断开连接
            Console.WriteLine("\n===== 测试完成 =====");
            heater.Close();

            Console.WriteLine("\n按任意键关闭窗口...");
            Console.ReadKey();
        }
    }
}