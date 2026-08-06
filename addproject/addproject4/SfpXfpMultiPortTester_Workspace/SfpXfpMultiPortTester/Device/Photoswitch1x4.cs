using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using SfpXfpMultiPortTester;

namespace FibertopTest_Common
{
   public class Photoswitch1x4
    {
        SerialPort Photoswitch_Port;
        bool Photoswitch_connected = false;
        public int ComWaitTime = 1000;
        private readonly object _lock = new object();

        SimpleLogger simpleLogger = new SimpleLogger("Photoswitch1x4.txt");

        public Photoswitch1x4()
        {
           // simpleLogger.FileDelete();
        }

        public bool Photoswitch_Connect(string port_name, bool enable)
        {
            try
            {
                if (enable == true)
                {
                    Photoswitch_connected = true;
                    Photoswitch_Port = new SerialPort();
                    Photoswitch_Port.PortName = port_name;
                    Photoswitch_Port.BaudRate = 115200;
                    Photoswitch_Port.ReadTimeout = 5000;
                    Photoswitch_Port.Open();
                    string command = "*IDN?";
                    string str;

                    Photoswitch_Port.WriteLine(command);
                    Thread.Sleep(ComWaitTime);
                    str = Photoswitch_Port.ReadLine();
                    return (str.Contains("PSS,OPS1X401")); // 需要修改
                }
                else if (Photoswitch_connected == true)
                {
                    Photoswitch_Port.Close();
                    Photoswitch_connected = false;
                    return true;
                }
            }
            catch (Exception e)
            {
                Photoswitch_connected = false;
                simpleLogger.LogError("error Photoswitch_Connect " + e.Message);
            }
            return false;
        }

        public bool UserConfig()
        {
            try
            {
                if (Photoswitch_connected == true)
                {
                    return SetOPSOnOff(true);
                }
            }
            catch (Exception e)
            {
                simpleLogger.LogError("error UserConfig " + e.Message);
            }
            return false;
        }

        #region RST 退出上位机模式 bool SetRst()
        public bool SetRst()
        {
            try
            {
                lock (_lock)
                {
                    if (Photoswitch_connected == true)
                    {
                        string command = "*RST";

                        Photoswitch_Port.WriteLine(command);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                simpleLogger.LogError("error SetRst " + e.Message);
            }
            return false;
        }
        #endregion

        #region 配置\查询设备工作通道 bool SetWorkChannel(int channel) int GetWorkChannel()
        // 1-8 任意数字，表示设备工作在哪个通道
        public bool SetChannel(int channel)
        {
            try
            {

                lock (_lock)
                {
                    if (Photoswitch_connected == true)
                    {
                        string command = "Configure:WorkChannel " + channel.ToString();
                        string str;

                        Photoswitch_Port.WriteLine(command);
                        Thread.Sleep(ComWaitTime);
                        str = Photoswitch_Port.ReadLine();

                        return (str == "Config:" + channel.ToString() + ":OK!");
                    }
                }
            }
            catch (Exception e)
            {
                simpleLogger.LogError("error SetWorkChannel " + e.Message);
            }
            return false;
        }
        public int GetWorkChannel()
        {
            try
            {
                lock (_lock)
                {
                    if (Photoswitch_connected == true)
                    {
                        string command = "Configure:WorkChannel?";
                        string str;

                        Photoswitch_Port.WriteLine(command);
                        Thread.Sleep(ComWaitTime);
                        str = Photoswitch_Port.ReadLine();
                        return int.Parse(str);
                    }
                }
            }
            catch (Exception e)
            {
                simpleLogger.LogError("error GetWorkChannel " + e.Message);
            }
            return -1;
        }
        #endregion

        #region 配置\查询设备工作状态 bool SetOPSOnOff(bool enable) bool GetOPSOnOff()
        public bool SetOPSOnOff(bool enable)
        {
            try
            {
                lock (_lock)
                {
                    if (Photoswitch_connected == true)
                    {
                        string command = "Configure:OPSOnOff " + ((enable) ? ("ON") : ("OFF"));
                        string str;

                        Photoswitch_Port.WriteLine(command);
                        Thread.Sleep(ComWaitTime);
                        str = Photoswitch_Port.ReadLine();
                        return (str == "Config:ON:OK!");
                    }
                }
            }
            catch (Exception e)
            {
                simpleLogger.LogError("error SetOPSOnOff " + e.Message);
            }
            return false;
        }
        public bool GetOPSOnOff()
        {
            try
            {
                lock (_lock)
                {
                    if (Photoswitch_connected == true)
                    {
                        string command = "Configure:OPSOnOff?";
                        string str;

                        Photoswitch_Port.WriteLine(command);
                        Thread.Sleep(ComWaitTime);
                        str = Photoswitch_Port.ReadLine();
                        return (str == "ON");
                    }
                }
            }
            catch (Exception e)
            {
                simpleLogger.LogError("error GetOPSOnOff " + e.Message);
            }
            return false;
        }
        #endregion
    }
}
