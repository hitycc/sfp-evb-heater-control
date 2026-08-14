using System;
using System.IO.Ports;
using System.Threading;

namespace FibertopTest_Common
{
    /// <summary>
    /// 光开关设备控制类
    /// 封装串口连接、断开、通道切换功能
    /// </summary>
    public class OpticalSwitchController
    {
        private readonly SerialPort _opticalSwitchPort;
        private bool _isConnected;

        private readonly object _lock = new object();

        /// <summary>
        /// 是否已连接光开关
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 串口名称
        /// </summary>
        public string PortName
        {
            get { lock (_lock) { return _opticalSwitchPort.PortName; } }
            set { lock (_lock) { _opticalSwitchPort.PortName = value; } }
        }

        public OpticalSwitchController()
        {
            _opticalSwitchPort = new SerialPort
            {
                BaudRate = 115200,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        /// <summary>
        /// 连接光开关设备
        /// </summary>
        /// <param name="portName">串口号（COMx）</param>
        /// <returns>连接成功返回true</returns>
        public bool Connect(string portName)
        {
            lock (_lock)
            {
                try
                {
                    // 关闭已有连接
                    if (_opticalSwitchPort.IsOpen)
                        _opticalSwitchPort.Close();

                    _opticalSwitchPort.PortName = portName;
                    _opticalSwitchPort.Open();

                    // 发送识别指令
                    SendCommand("*IDN?");
                    Thread.Sleep(1000);
                    string response = ReadResponse();

                    // 校验设备身份
                    if (response != null && response.Length > 0 && response.Contains("PSS"))
                    {
                        _isConnected = true;
                        return true;
                    }

                    DisconnectInternal();
                    return false;
                }
                catch
                {
                    DisconnectInternal();
                    return false;
                }
            }
        }

        /// <summary>
        /// 断开光开关连接
        /// </summary>
        public void Disconnect()
        {
            lock (_lock)
            {
                DisconnectInternal();
            }
        }

        /// <summary>
        /// 内部断开实现
        /// </summary>
        private void DisconnectInternal()
        {
            try
            {
                if (_opticalSwitchPort.IsOpen)
                    _opticalSwitchPort.Close();
            }
            catch { }
            finally
            {
                _isConnected = false;
            }
        }

        /// <summary>
        /// 发送指令
        /// </summary>
        private void SendCommand(string command)
        {
            if (!_isConnected || !_opticalSwitchPort.IsOpen)
                throw new InvalidOperationException("光开关未连接！");

            _opticalSwitchPort.WriteLine(command);
        }

        /// <summary>
        /// 读取返回数据
        /// </summary>
        private string ReadResponse()
        {
            if (!_isConnected || !_opticalSwitchPort.IsOpen)
                return string.Empty;

            try
            {
                return _opticalSwitchPort.ReadLine().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 设置光开关通道
        /// </summary>
        /// <param name="channel">通道号</param>
        /// <returns>设置成功返回true</returns>
        public bool SetChannel(int channel)
        {
            lock (_lock)
            {
                if (!_isConnected)
                    return false;

                try
                {
                    string command = $"Configure:WorkChannel "+ channel.ToString();
                    SendCommand(command);
                    string response = ReadResponse();

                    return response != null && response.Contains(channel.ToString());
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
            _opticalSwitchPort?.Dispose();
        }
    }
}