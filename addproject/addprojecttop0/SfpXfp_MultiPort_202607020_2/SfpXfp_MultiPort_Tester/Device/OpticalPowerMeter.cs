using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace FibertopTest_Common
{
    /// <summary>
    /// 光功率计控制类 (支持光讯手持式 和 普塞斯台式) 
    /// </summary>
    public class OpticalPowerMeter : IDisposable
    {
        private SerialPort _serialPort;
        private bool _isConnected = false;
        private int _deviceType = 0; // 0:光讯手持, 1:普塞斯台式

        private readonly object _lock = new object();

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// 析构函数，确保资源释放
        /// </summary>
        ~OpticalPowerMeter()
        {
            Dispose(false);
        }

        /// <summary>
        /// 连接光功率计
        /// </summary>
        public bool Connect(string portName, int deviceType = 0, int baudRate = 0)
        {
            lock (_lock)
            {
                try
                {
                    if (_isConnected)
                    {
                        DisconnectInternal();
                    }

                    _deviceType = deviceType;
                    _serialPort = new SerialPort();
                    _serialPort.PortName = portName;
                    _serialPort.ReadTimeout = 1000;

                    // 根据设备类型设置波特率
                    if (baudRate != 0)
                    {
                        _serialPort.BaudRate = baudRate;
                    }
                    else
                    {
                        _serialPort.BaudRate = (_deviceType == 0) ? 9600 : 115200;
                    }

                    _serialPort.Open();

                    bool result = false;
                    if (_deviceType == 0) // 光讯手持式
                    {
                        result = Handheld_Init();
                    }
                    else // 普塞斯台式
                    {
                        result = Desktop_Init();
                    }

                    if (result)
                    {
                        _isConnected = true;
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
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            //lock (_lock)
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
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                   // _serialPort.Dispose();
                }
            }
            catch { }
            finally
            {
                //_serialPort.Dispose();
                _isConnected = false;
                _serialPort = null;
            }
        }

        /// <summary>
        /// 读取光功率值
        /// </summary>
        public float ReadPower(int channel, int delayMs)
        {
            //lock (_lock)
            {
                if (!_isConnected) return -100;
                float pwr = -100;
                Thread.Sleep(delayMs);
                for (int attemp = 0; attemp < 5; attemp++)
                {
                    try
                    {
                        if (_deviceType == 0) // 光讯手持式
                        {
                            pwr = Handheld_Read();
                        }
                        else // 普塞斯台式
                        {
                            pwr = Desktop_Read(channel);
                        }
                    }
                    catch(Exception ex)
                    {
                        GlobalVarFun.meter_error_message = ex.ToString();
                        pwr = -100;
                    }
                    if (pwr != -100) break;
                    Thread.Sleep(delayMs);
                }
                return pwr;
            }
        }

        #region 光讯手持式 私有方法

        private bool Handheld_Init()
        {
            byte[] writeBuffer = new byte[7] { 0xef, 0xef, 0x04, 0x04, 0x60, 0x06, 0x4c };
            byte[] readBuffer = new byte[14];

            _serialPort.Write(writeBuffer, 0, 7);
            Thread.Sleep(100);

            int len = _serialPort.Read(readBuffer, 0, 14);
            return (len >= 2 && readBuffer[0] == 0xed && readBuffer[1] == 0xfa);
        }

        private float Handheld_Read()
        {
            byte[] writeBuffer = new byte[7] { 0xef, 0xef, 0x04, 0x04, 0x60, 0x06, 0x4c };
            byte[] readBuffer = new byte[14];

            _serialPort.Write(writeBuffer, 0, 7);
            Thread.Sleep(150);
            _serialPort.Read(readBuffer, 0, 14);

            if (readBuffer[0] == 0xed && readBuffer[1] == 0xfa)
            {
                float dispdata = (readBuffer[7] * 256) + readBuffer[8];
                int k = 0;

                // 单位判断
                switch (readBuffer[9] & 0x30)
                {
                    case 0x30: k = 1000; break;
                    case 0x20: k = 100; break;
                    case 0x10: k = 10; break;
                }

                // 转换为dBm
                switch (readBuffer[9] & 0x07)
                {
                    case 1: // mW
                        return (float)(10 * Math.Log10((dispdata / k) + 1E-6));
                    case 2: // uW
                        return (float)(10 * Math.Log10((dispdata / k) / 1000 + 1E-6));
                    case 3: // nW
                        return (float)(10 * Math.Log10((dispdata / k) / 1000000 + 1E-6));
                    case 4: // dBm
                        return (dispdata - 9000) / 100.0f;
                    default:
                        return -100;
                }
            }
            return -100;
        }

        #endregion

        #region 普塞斯台式 私有方法

        private bool Desktop_Init()
        {
            // *IDN? 指令
            byte[] writeBuffer_IDN = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
            byte[] readBuffer = new byte[40];
            try
            {
                _serialPort.Write(writeBuffer_IDN, 0, 7);
                Thread.Sleep(300);

                int len = _serialPort.Read(readBuffer, 0, 36);
                string response = System.Text.Encoding.ASCII.GetString(readBuffer, 0, len);
                return response.Contains("PSS") && response.Contains("OPM");
            }
            catch//(Exception ex)
            {
               // MessageBox.Show(ex.ToString());
                return false;
            }
            // 简单判断是否包含 PSS OPM 关键字
            
        }

        private float Desktop_Read(int channel)
        {
            byte[] writeBuffer;
            if (channel == 1)
            {
                writeBuffer = new byte[21] { 0x52, 0x65, 0x61, 0x64, 0x3A, 0x50, 0x6F, 0x77, 0x65, 0x72, 0x20, 0x43, 0x68, 0x61, 0x6E, 0x6E, 0x65, 0x6C, 0x31, 0x0D, 0x0A };
            }
            else
            {
                writeBuffer = new byte[21] { 0x52, 0x65, 0x61, 0x64, 0x3A, 0x50, 0x6F, 0x77, 0x65, 0x72, 0x20, 0x43, 0x68, 0x61, 0x6E, 0x6E, 0x65, 0x6C, 0x32, 0x0D, 0x0A };
            }
            writeBuffer[18] = (byte)(0x30 + channel);
            byte[] readBuffer = new byte[20];
            _serialPort.Write(writeBuffer, 0, writeBuffer.Length);
            Thread.Sleep(300);

            int readLen = _serialPort.Read(readBuffer, 0, readBuffer.Length);
            if (readLen < 7) return -100;

            try
            {
                string str = System.Text.Encoding.ASCII.GetString(readBuffer, 0, readLen).Trim();
                // 如果读取到的是命令回显 (e.g., "Read:Power Channel1")，则再读一行
                if (readBuffer[readLen - 1] == 0x0A)
                {
                    //string str = System.Text.Encoding.ASCII.GetString(readBuffer);
                    if (str.Length == 15)
                    {
                        str = str.Substring(readLen - 8, 7);//负值
                    }
                    else
                    {
                        str = str.Substring(readLen - 7, 6);//正值
                    }
                    // float pwrValue = Convert.ToSingle(str);
                    return Convert.ToSingle(str.Trim());
                }
                else
                {
                    if (str.Contains("annel"))
                    {
                        if (_serialPort.BytesToRead > 0)
                        {
                            str = _serialPort.ReadLine();
                        }

                    }
                    else
                    {
                        // 去掉可能的换行符
                        if (str.EndsWith("\r\n") || str.EndsWith("\n"))
                            str = str.Substring(0, str.Length - 2);
                    }
                }
                return Convert.ToSingle(str);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return -100;
            }
        }

        #endregion

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose 调用公开的 Disconnect，自动获取锁
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