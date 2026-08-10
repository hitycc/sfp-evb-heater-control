using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FibertopTest_Common
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

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// 核心异步指令收发
        /// </summary>
        private async Task<string> SendCommandInternalAsync(string cmd, int delayMs)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!IsOpen) return null;

                byte[] wbuf = Encoding.UTF8.GetBytes(cmd + "\r\n");

                // ✅ 异步发送
                await TaskCompletionSourceSendAsync(Client, wbuf, 0, wbuf.Length, SocketFlags.None);

                // ✅ 非阻塞延迟
                if (delayMs > 0)
                    await Task.Delay(delayMs);

                byte[] rbuf = new byte[1024];

                // ✅ 异步接收
                int count = await TaskCompletionSourceReceiveAsync(Client, rbuf, 0, rbuf.Length, SocketFlags.None);

                if (count > 0)
                    return Encoding.UTF8.GetString(rbuf, 0, count).Trim();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendCommand error: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
            return null;
        }

        /// <summary>
        /// 将 Socket.BeginSend/EndSend 包装为 Task
        /// </summary>
        private static Task<int> TaskCompletionSourceSendAsync(
            Socket socket, byte[] buffer, int offset, int size, SocketFlags flags)
        {
            var tcs = new TaskCompletionSource<int>();
            socket.BeginSend(buffer, offset, size, flags, ar =>
            {
                try
                {
                    tcs.SetResult(socket.EndSend(ar));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        /// <summary>
        /// 将 Socket.BeginReceive/EndReceive 包装为 Task
        /// </summary>
        private static Task<int> TaskCompletionSourceReceiveAsync(
            Socket socket, byte[] buffer, int offset, int size, SocketFlags flags)
        {
            var tcs = new TaskCompletionSource<int>();
            socket.BeginReceive(buffer, offset, size, flags, ar =>
            {
                try
                {
                    tcs.SetResult(socket.EndReceive(ar));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);
            return tcs.Task;
        }

        /// <summary>
        /// 同步发送命令（内部走异步实现）
        /// </summary>
        public string SendCommand(string cmd) => SendCommand(cmd, 100);

        public string SendCommand(string cmd, int delay)
        {
            try
            {
                return SendCommandInternalAsync(cmd, delay).GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 一、EVB 功率/电流/电压 读写功能
        public string GetPower(int slot = 1)
        {
            string cmd = $"evb{slot}:getpower?";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res)) return null;
            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        public string GetCurrent(int slot = 1)
        {
            string cmd = $"evb{slot}:getcurrent?";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res)) return null;
            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        public string GetVoltage(int slot = 1)
        {
            string cmd = $"evb{slot}:getvoltage?";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res)) return null;
            string numberOnly = Regex.Replace(res, @"[^0-9]", "");
            return !string.IsNullOrEmpty(numberOnly) ? numberOnly : null;
        }

        public bool SetVoltage(double voltage, int slot = 1)
        {
            string cmd = $"evb{slot}:setvoltage {voltage}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("set Voltage ok!");
        }
        #endregion

        #region 二、IO 引脚控制功能
        #region 1. PowerEN
        public bool SetPowerEN(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setPowerEN {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"POWER_EN:{state}");
        }

        public string GetPowerEN(int slot = 1)
        {
            string cmd = $"IO{slot}:getPowerEN";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("POWER_EN:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 2. TxDis
        public bool SetTxDis(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setTxDis {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"TX_DIS:{state}");
        }

        public string GetTxDis(int slot = 1)
        {
            string cmd = $"IO{slot}:getTxDis";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("TX_DIS:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 3. Rs0High
        public bool SetRs0High(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setRs0High {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"RS0_HIGH:{state}");
        }

        public string GetRs0High(int slot = 1)
        {
            string cmd = $"IO{slot}:getRs0High";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("RS0_HIGH:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 4. Rs1High
        public bool SetRs1High(int state, int slot = 1)
        {
            string cmd = $"IO{slot}:setRs1High {state}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains($"RS1_HIGH:{state}");
        }

        public string GetRs1High(int slot = 1)
        {
            string cmd = $"IO{slot}:getRs1High";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("RS1_HIGH:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion

        #region 5. 只读引脚查询
        public string GetTxFalu(int slot = 1)
        {
            string cmd = $"IO{slot}:getTxFault";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("TX_FAULT:")) return null;
            return res.Split(':')[1].Trim();
        }

        public string GetRxLos(int slot = 1)
        {
            string cmd = $"IO{slot}:getRxLos";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("RX_LOS:")) return null;
            return res.Split(':')[1].Trim();
        }

        public string GetABS(int slot = 1)
        {
            string cmd = $"IO{slot}:getABS";
            string res = SendCommand(cmd);
            if (string.IsNullOrEmpty(res) || !res.Contains("ABS:")) return null;
            return res.Split(':')[1].Trim();
        }
        #endregion
        #endregion

        #region 三、IIC 读写功能
        public bool IIC_Set(string deviceAddr, string regAddr, string dataLength, string data, int slot = 1)
        {
            string cmd = $"IIC{slot}:set {deviceAddr},{regAddr},{dataLength},{data}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("iic set ok");
        }

        public string IIC_Get(string deviceAddr, string regAddr, string dataLength, int slot = 1)
        {
            string cmd = $"IIC{slot}:get {deviceAddr},{regAddr},{dataLength}";
            return SendCommand(cmd);
        }
        #endregion

        #region 四、设备IP设置功能
        public bool SetDeviceIP(string newIP)
        {
            string cmd = $"setip {newIP}";
            string res = SendCommand(cmd);
            return !string.IsNullOrEmpty(res) && res.Contains("set ip ok");
        }
        #endregion
    }
}