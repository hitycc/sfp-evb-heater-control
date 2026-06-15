using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace EVBHeaterControl
{
    /// <summary>
    /// SFP EVB加热台TCP通讯类
    /// </summary>
    public class SFP_EVB_Heater
    {
        private static readonly object _lock = new object();
        private Socket Client;

        public string DefaultIP { get; set; }
        public int DefaultPort { get; set; }
        public int DefaultTimeout { get; set; }

        public bool IsOpen => Client != null && Client.Connected;

        public SFP_EVB_Heater()
        {
            DefaultIP = "129.168.1.133";
            DefaultPort = 9000;
            DefaultTimeout = 5000;
            Client = null;
        }

        public bool Open() => Open(DefaultIP, DefaultPort, DefaultTimeout);
        public bool Open(string ip) => Open(ip, DefaultPort, DefaultTimeout);
        public bool Open(string ip, int port) => Open(ip, port, DefaultTimeout);

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

        public string SendCommand(string cmd, int delay = 300)
        {
            lock (_lock)
            {
                if (!IsOpen) return null;
                try
                {
                    byte[] sendBuf = Encoding.UTF8.GetBytes(cmd + "\r\n");
                    Client.Send(sendBuf, SocketFlags.None);
                    Thread.Sleep(delay);
                    byte[] recvBuf = new byte[1024];
                    int len = Client.Receive(recvBuf, SocketFlags.None);
                    if (len > 0)
                        return Encoding.UTF8.GetString(recvBuf, 0, len).Trim();
                }
                catch { }
                return null;
            }
        }

        public string GetPower(int slot = 1)
        {
            var res = SendCommand($"evb{slot}:getpower?");
            return res == null ? null : Regex.Replace(res, @"[^0-9]", "");
        }

        public string GetCurrent(int slot = 1)
        {
            var res = SendCommand($"evb{slot}:getcurrent?");
            return res == null ? null : Regex.Replace(res, @"[^0-9]", "");
        }

        public string GetVoltage(int slot = 1)
        {
            var res = SendCommand($"evb{slot}:getvoltage?");
            return res == null ? null : Regex.Replace(res, @"[^0-9]", "");
        }
    }
}