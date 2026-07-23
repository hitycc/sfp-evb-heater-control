using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Ivi.Visa;  // 核心 VISA 接口
using Ivi.Visa.Interop;  // 核心 COM 接口

namespace FibertopTest_Common
{
    public class Keysight86120C : IDisposable
    {
        // 设备通信对象
        private FormattedIO488 _io;
        private ResourceManager _rm;
        private bool _isConnected;

        public bool IsConnected
        {
             get { return _isConnected; }
        }
        public void OpticalModuleController()
        {
            _rm = new ResourceManager();
            _io = new FormattedIO488();
        }

        // 连接设备
        public bool Connect()  
        {  
            
            return Connect("GPIB0::20::INSTR");
  
        }
        public bool Connect(string address)
        {

            if (_isConnected)
            {
                return true;
            }
           
            try
            {
                if (_io == null)//
                {
                    _io = new FormattedIO488();
                }

                _io.IO = (IMessage)_rm.Open(
                    address,
                    AccessMode.NO_LOCK,
                    3000,
                    ""
                );

                // 验证连接
                WriteCommand("*IDN?");
                var idn = ReadString();
                if (string.IsNullOrEmpty(idn))
                {
                    return false;
                }

                _isConnected = true;
                return true;
               
            }
            catch
            {
                Disconnect();
                return false;
                
            }
        }

        // 断开连接
        public bool Disconnect()
        {
            bool success = true;
            try
            {

                if (_io != null) 
                {
                    if (_io.IO != null) 
                    {
                        _io.IO.Close();
                    }
                }
            }
            catch
            {
                success = false;
            }
            finally
            {
                _isConnected = false;
            }
            return success;
        }

        // 获取波长
        public double GetWavelength()
        {
            double wavelength_m = 0;
            double wavelength_nm = 0;
            ValidateConnection();
            try
            {
                WriteCommand(":FETC:SCAL:POW:WAV?");
            }
            catch
            {
                //return 0;
            }
            int originalTimeout = _io.IO.Timeout;
             _io.IO.Timeout = 10000; // 10秒超时
            var response = ReadString().Trim();
             _io.IO.Timeout = originalTimeout;                             
            wavelength_m = double.Parse(response);
            wavelength_nm = wavelength_m * 1e9;
            return wavelength_nm; // 转换为nm
        }

        // 获取光功率
        public double GetPower()
        {
            double powervalue = 0;
            ValidateConnection();
            WriteCommand(":FETC:SCAL:POW?");
             int originalTimeout = _io.IO.Timeout;
            _io.IO.Timeout = 10000; // 10秒超时
            var response = ReadString().Trim();
            _io.IO.Timeout = originalTimeout;          
            powervalue = double.Parse(response);
            return powervalue;
        }

        // 基础通信方法
        private void WriteCommand(string command)
        {
            if (_io == null)//
            {
                _io = new FormattedIO488();
            }
            _io.IO.Clear();
            _io.WriteString("*OPC?\n", true);  // 查询操作完成状态
            _io.ReadString();  // 等待直到设备返回"1"
            _io.WriteString(command + "\n", true);
        }

        private string ReadString()
        {
            return _io.ReadString().Trim();
        }

        private void ValidateConnection()
        {
            if (!_isConnected)
                return;
        }

        public void Dispose()
        {
            Disconnect();
            _rm = null;
        }
    }
}