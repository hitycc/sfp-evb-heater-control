using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace FibertopTest_Common
{
    /// <summary>
    /// 数字光衰减器控制类 
    /// </summary>
    public class OpticalAttenuator : IDisposable
    {
        private SerialPort _serialPort;
        private bool _isConnected = false;

        private readonly object _lock = new object();

        public float CurrentAtt
        {
            get { lock (_lock) { return _currentAtt; } }
            private set { lock (_lock) { _currentAtt = value; } }
        }
        private float _currentAtt = 0f;

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 析构函数，确保资源释放
        /// </summary>
        ~OpticalAttenuator()
        {
            Dispose(false);
        }

        /// <summary>
        /// 连接光衰减器
        /// </summary>
        /// <param name="portName">串口号 (e.g., "COM1")</param>
        /// <param name="baudRate">波特率 (传0则默认115200)</param>
        /// <returns>是否连接成功</returns>
        public bool Connect(string portName, int baudRate = 0)
        {
                try
                {
                    if (_isConnected)
                    {
                        DisconnectInternal();
                    }

                    _serialPort = new SerialPort();
                    _serialPort.PortName = portName;
                    _serialPort.BaudRate = (baudRate != 0) ? baudRate : 115200;
                    _serialPort.ReadTimeout = 1000;
                    if (_serialPort.IsOpen) _serialPort.Close();
                    _serialPort.Open();

                    // 发送 *IDN? 识别设备 (0x2A 0x49 0x44 0x4E 0x3F 0x0D 0x0A)
                    byte[] writeBuffer = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
                    byte[] readBuffer = new byte[40];

                    _serialPort.Write(writeBuffer, 0, 7);
                    Thread.Sleep(100);
                    int readLen = _serialPort.Read(readBuffer, 0, 34);

                    // 验证返回头 PSS 以及读取长度
                    if (readLen == 34 && readBuffer[0] == 0x50 && readBuffer[1] == 0x53 && readBuffer[2] == 0x53)
                    {
                        _isConnected = true;
                        _currentAtt = 0f; // 连接成功后重置当前衰减值
                        return true;
                    }
                    else
                    {
                        DisconnectInternal();
                        return false;
                    }
                }
                catch
                {
                    DisconnectInternal();
                    return false;
                }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
             DisconnectInternal();
        }

        /// <summary>
        /// 内部断开实现
        /// </summary>
        private void DisconnectInternal()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
            }
            catch { }
            finally
            {
                _isConnected = false;
                _serialPort = null;
            }
        }

        /// <summary>
        /// 设置光衰减器的衰减值
        /// </summary>
        /// <param name="attVal">目标衰减值 (0-60 dB)</param>
        /// <param name="delayMsPerDb">每dB需要的硬件响应延时(ms)，默认60ms</param>
        /// <param name="baseDelayMs">基础固定延时(ms)，默认200ms</param>
        /// <returns>
        /// 0x00: 操作成功
        /// 0x01: 设备未连接
        /// 0x02: 参数错误 (超出范围或格式异常)
        /// </returns>
        public char SetAttenuation(float attVal, int delayMsPerDb = 60, int baseDelayMs = 200)
        {
            lock (_lock)
            {
                if (!_isConnected)
                {
                    return (char)0x01; // 设备未连接
                }

                attVal = Math.Abs(attVal);

                if (attVal > 60) // 取完绝对值后，只需要判断是否大于 60 即可
                {
                    return (char)0x02;
                }

                // 格式化衰减值，保留一位小数 (例如 20.5 -> "20.5")
                string strVal = attVal.ToString("F1");
                byte[] valBytes = Encoding.ASCII.GetBytes(strVal);
                int valLen = valBytes.Length;

                if (valLen > 4)
                {
                    return (char)0x02; // 参数错误：长度异常
                }

                // 构建指令：Configure:Atten -XX.X\r\n
                // 原始模板：0x43 0x6F 0x6E 0x66 0x69 0x67 0x75 0x72 0x65 0x3A 0x41 0x74 0x74 0x65 0x6E 0x20 0x2D (即 "Configure:Atten -")
                byte[] writeBuffer = new byte[23] {
                    0x43, 0x6F, 0x6E, 0x66, 0x69, 0x67, 0x75, 0x72, 0x65, 0x3A,
                    0x41, 0x74, 0x74, 0x65, 0x6E, 0x20, 0x2D, 0x32, 0x30, 0x2E,
                    0x30, 0x0D, 0x0A
                };

    
                for (int i = 0; i < valLen; i++)
                {
                    writeBuffer[17 + i] = valBytes[i];
                }
     
                writeBuffer[17 + valLen] = 0x0D;
                writeBuffer[18 + valLen] = 0x0A;

                try
                {
                    // 发送指令
                    _serialPort.Write(writeBuffer, 0, 19 + valLen);

                    // 计算硬件响应延时：每dB延时 * 衰减变化的绝对值 + 基础延时
                    int dynamicDelay = (int)(delayMsPerDb * Math.Abs(_currentAtt - attVal));
                    Thread.Sleep(dynamicDelay + baseDelayMs);

                    // 更新当前衰减值
                    _currentAtt = attVal;
                    return (char)0x00; // 操作成功
                }
                catch
                {
                    return (char)0x03; // 串口发送异常
                }
            }
        }


        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}