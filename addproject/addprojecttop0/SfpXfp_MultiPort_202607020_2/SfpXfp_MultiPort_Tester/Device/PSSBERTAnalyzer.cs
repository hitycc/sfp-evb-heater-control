using System;
using System.IO.Ports;
using System.Threading;

namespace FibertopTest_Common
{
    /// <summary>
    /// PSS误码仪串口控制类
    /// 封装连接、指令发送、状态读取、清除误码功能
    /// </summary>
    public class PssBertController
    {
        private readonly SerialPort _pssPort;
        private bool _isConnected;


        private readonly object _lock = new object();

        /// <summary>
        /// 是否已连接误码仪
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 串口名称
        /// </summary>
        public string PortName
        {
            get { lock (_lock) { return _pssPort.PortName; } }
            set { lock (_lock) { _pssPort.PortName = value; } }
        }

        public PssBertController()
        {
            _pssPort = new SerialPort
            {
                BaudRate = 115200,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        /// <summary>
        /// 连接误码仪
        /// </summary>
        /// <param name="portName">串口号（如COM3）</param>
        /// <returns>连接成功返回true，失败返回false</returns>
        public bool Connect(string portName)
        {
            lock (_lock)
            {
                try
                {
                    if (_pssPort.IsOpen)
                        _pssPort.Close();

                    _pssPort.PortName = portName;
                    _pssPort.Open();

                    // 发送识别指令
                    SendCommandInternal("*IDN?");
                    Thread.Sleep(20);
                    string response = ReadResponseInternal();

                    if (response != null && response.Length > 0 && response.StartsWith("PSS,BERT"))
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
        /// 断开连接
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
                if (_pssPort.IsOpen)
                    _pssPort.Close();
            }
            catch { }
            finally
            {
                _isConnected = false;
            }
        }

        /// <summary>
        /// 发送SCPI指令
        /// </summary>
        public void SendCommand(string command)
        {
            lock (_lock)
            {
                SendCommandInternal(command);
            }
        }

        /// <summary>
        /// 内部发送实现
        /// </summary>
        private void SendCommandInternal(string command)
        {
            if (!_isConnected || !_pssPort.IsOpen)
                throw new InvalidOperationException("误码仪未连接！");

            _pssPort.WriteLine(command);
        }

        /// <summary>
        /// 读取返回数据
        /// </summary>
        public string ReadResponse()
        {
            lock (_lock)
            {
                return ReadResponseInternal();
            }
        }

        /// <summary>
        /// 内部读取实现
        /// </summary>
        private string ReadResponseInternal()
        {
            if (!_isConnected || !_pssPort.IsOpen)
                return string.Empty;

            try
            {
                return _pssPort.ReadLine().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取通道状态
        /// </summary>
        public string GetChannelStatus(string channel)
        {
            lock (_lock)
            {
                SendCommandInternal("Status:Result? " + channel);
                Thread.Sleep(1000);
                return ReadResponseInternal();
            }
        }

        /// <summary>
        /// 清除通道误码
        /// </summary>
        public void ClearChannelError(string channel)
        {
            lock (_lock)
            {
                SendCommandInternal("Sense:Clear " + channel);
                Thread.Sleep(100);
            }
        }

        public void Dispose()
        {
            Disconnect();
            if (_pssPort != null)
                _pssPort.Dispose();
        }
    }
}