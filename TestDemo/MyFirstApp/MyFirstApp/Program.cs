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
            string cmd = string.Format("IO{0}:getTxFault", slot);
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
    /// 测试入口：4槽位完整查询
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
                Console.WriteLine("连接失败，按任意键退出...");
                Console.ReadKey();
                return;
            }

            // 2. 查询所有4个slot的状态
            QueryAllSlots(heater);

            // 3. 断开连接
            Console.WriteLine("\n===== 查询完成，断开连接 =====");
            heater.Close();

            Console.WriteLine("\n按任意键关闭窗口...");
            Console.ReadKey();
        }

        /// <summary>
        /// 查询所有4个slot的完整状态
        /// </summary>
        static void QueryAllSlots(SFP_EVB_Heater heater)
        {
            Console.WriteLine("\n==================================================");
            Console.WriteLine("          SFP EVB 加热台 - 4槽位状态查询");
            Console.WriteLine("==================================================");

            for (int slot = 1; slot <= 4; slot++)
            {
                QuerySlotStatus(heater, slot);
            }
        }

        /// <summary>
        /// 查询单个slot的所有参数：功率、电流、电压、各引脚状态
        /// </summary>
        /// <param name="heater">SFP_EVB_Heater实例</param>
        /// <param name="slot">槽位号(1-4)</param>
        static void QuerySlotStatus(SFP_EVB_Heater heater, int slot)
        {
            Console.WriteLine("\n--------------------------------------------------");
            Console.WriteLine(string.Format("  Slot {0} 状态查询", slot));
            Console.WriteLine("--------------------------------------------------");

            // ---- 电气参数 ----
            Console.WriteLine("【电气参数】");
            string power = heater.GetPower(slot);
            string current = heater.GetCurrent(slot);
            string voltage = heater.GetVoltage(slot);

            Console.WriteLine(string.Format("  功率(Power)    : {0}",
                !string.IsNullOrEmpty(power) ? power + " mW" : "读取失败"));
            Console.WriteLine(string.Format("  电流(Current)  : {0}",
                !string.IsNullOrEmpty(current) ? current + " uA" : "读取失败"));
            Console.WriteLine(string.Format("  电压(Voltage)  : {0}",
                !string.IsNullOrEmpty(voltage) ? voltage + " mV" : "读取失败"));

            // ---- 控制引脚（EVB输出给模块的信号，不放模块也能读到EVB当前设置值）----
            Console.WriteLine("【控制引脚（EVB→模块）】");
            string powerEN = heater.GetPowerEN(slot);
            string txDis = heater.GetTxDis(slot);
            string rs0High = heater.GetRs0High(slot);
            string rs1High = heater.GetRs1High(slot);

            Console.WriteLine(string.Format("  PowerEN(模块使能) : {0}",
                FormatPinState(powerEN, "使能(高)", "禁用(低)")));
            Console.WriteLine(string.Format("  TX_DIS(发射关断)  : {0}",
                FormatPinState(txDis, "高电平(关断)", "低电平(正常发射)")));
            Console.WriteLine(string.Format("  RS0_HIGH(速率选择0): {0}",
                FormatPinState(rs0High, "高电平", "低电平")));
            Console.WriteLine(string.Format("  RS1_HIGH(速率选择1): {0}",
                FormatPinState(rs1High, "高电平", "低电平")));

            // ---- 状态引脚（模块输出给EVB的信号，没放模块时读到的是默认电平）----
            Console.WriteLine("【状态引脚（模块→EVB）】");
            string txFalu = heater.GetTxFalu(slot);
            string rxLos = heater.GetRxLos(slot);

            Console.WriteLine(string.Format("  TX_FALU(发射故障)  : {0}",
                FormatPinState(txFalu, "高电平(故障告警)", "低电平(正常)")));
            Console.WriteLine(string.Format("  RX_LOS(接收信号丢失): {0}",
                FormatPinState(rxLos, "高电平(信号丢失)", "低电平(信号正常)")));

            // ---- ABS模块在位检测引脚 ----
            Console.WriteLine("【模块检测】");
            string abs = heater.GetABS(slot);
            if (!string.IsNullOrEmpty(abs))
            {
                // ABS引脚电平含义取决于硬件设计，通常低电平表示模块在位(接地)
                // 这里同时输出原始值和推断状态，用户可根据实际硬件调整判断逻辑
                bool modulePresent = abs == "0";
                Console.WriteLine(string.Format("  ABS(模块在位)     : {0}  =>  {1}",
                    abs, modulePresent ? "模块已插入" : "模块未插入/不在位"));
            }
            else
            {
                Console.WriteLine("  ABS(模块在位)     : 读取失败");
            }
        }

        /// <summary>
        /// 格式化引脚状态显示
        /// </summary>
        /// <param name="value">读取到的引脚值</param>
        /// <param name="highDesc">高电平(1)的描述</param>
        /// <param name="lowDesc">低电平(0)的描述</param>
        /// <returns>格式化后的字符串</returns>
        static string FormatPinState(string value, string highDesc, string lowDesc)
        {
            if (string.IsNullOrEmpty(value))
                return "读取失败";

            if (value.Trim() == "1")
                return string.Format("1 ({0})", highDesc);
            else if (value.Trim() == "0")
                return string.Format("0 ({0})", lowDesc);
            else
                return value;
        }
    }
}