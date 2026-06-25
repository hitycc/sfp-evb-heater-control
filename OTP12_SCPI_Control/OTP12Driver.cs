using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace OTP12_SCPI_Control
{
    /// <summary>OTP-12(T) SCPI V2.0 完整驱动
    /// 所有命令1:1封装；Get/Query=只读(界面可用)，Set=修改(界面不添加按钮)
    /// 默认：子架0，槽位01，SCPI端口5024，设备默认192.168.1.200
    /// </summary>
    public class OTP12Driver
    {
        #region TCP底层通信 固定不用改
        private readonly object _lock = new object();
        private Socket _clientSocket;
        public string DefaultIp = "192.168.1.222";
        public int DefaultPort = 5024;
        public int Timeout = 5000;
        // 默认子架0，槽位01，可自行修改
        private const string Rack = "0";
        private const string Slot = "01";
        private string BoardPrefix => $"LINS{Rack}{Slot}";

        public bool IsConnected => _clientSocket != null && _clientSocket.Connected;

        public bool Connect() => Connect(DefaultIp, DefaultPort, Timeout);
        public bool Connect(string ip) => Connect(ip, DefaultPort, Timeout);
        public bool Connect(string ip, int port) => Connect(ip, port, Timeout);
        public bool Connect(string ipAddress, int port, int timeout)
        {
            lock (_lock)
            {
                if (IsConnected) return true;
                try
                {
                    _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        SendTimeout = timeout,
                        ReceiveTimeout = timeout
                    };
                    IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                    _clientSocket.Connect(ep);
                    return true;
                }
                catch
                {
                    _clientSocket?.Close();
                    _clientSocket = null;
                    return false;
                }
            }
        }

        public void DisConnect()
        {
            lock (_lock)
            {
                if (IsConnected)
                {
                    _clientSocket.Close();
                    _clientSocket = null;
                }
            }
        }

        /// <summary>底层发送SCPI，自动追加\r\n</summary>
        private string SendScpiCmd(string cmd, int delayMs = 300)
        {
            lock (_lock)
            {
                if (!IsConnected) return null;
                try
                {
                    byte[] sendBuf = Encoding.UTF8.GetBytes(cmd + "\r\n");
                    _clientSocket.Send(sendBuf, SocketFlags.None);
                    Thread.Sleep(delayMs);
                    byte[] recBuf = new byte[2048];
                    int recLen = _clientSocket.Receive(recBuf, SocketFlags.None);
                    if (recLen <= 0) return null;
                    return Encoding.UTF8.GetString(recBuf, 0, recLen).Trim();
                }
                catch
                {
                    return null;
                }
            }
        }
        #endregion

        #region 4 系统级命令（全部读写封装）
        /// <summary>4.1 只读 *IDN? 查询设备厂商型号序列号</summary>
        public string QueryDeviceInfo()
        {
            return SendScpiCmd("*IDN?");
        }

        /// <summary>4.2 只读 查询所有在线单板</summary>
        public string QueryAllBoardCatalog()
        {
            return SendScpiCmd("INSTrument:CATalog:FULL?");
        }

        /// <summary>4.3 设置 系统日期（界面不使用）</summary>
        public bool SetSystemDate(int year, int month, int day)
        {
            string cmd = $"SYSTem:DATE {year},{month},{day}";
            return SendScpiCmd(cmd)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.4 只读 查询系统日期</summary>
        public string GetSystemDate()
        {
            return SendScpiCmd("SYSTem:DATE?");
        }

        /// <summary>4.5 设置 系统时间（界面不使用）</summary>
        public bool SetSystemTime(int h, int m, int s)
        {
            string cmd = $"SYSTem:TIME {h},{m},{s}";
            return SendScpiCmd(cmd)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.6 只读 查询系统时间</summary>
        public string GetSystemTime()
        {
            return SendScpiCmd("SYSTem:TIME?");
        }

        /// <summary>4.7 只读 查询SCPI协议版本</summary>
        public string GetScpiVersion()
        {
            return SendScpiCmd("SYSTem:VERSion?");
        }

        /// <summary>4.8 设置 会话超时分钟（界面不使用）</summary>
        public bool SetSessionTimeout(int minute)
        {
            string cmd = $"SYSTem:SESsion:TIMeout {minute}";
            return SendScpiCmd(cmd)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.9 只读 查询会话超时</summary>
        public string GetSessionTimeout()
        {
            return SendScpiCmd("SYSTem:SESsion:TIMeout?");
        }

        /// <summary>4.10 只读 当前SCPI连接数量</summary>
        public string GetSessionCount()
        {
            return SendScpiCmd("SYSTem:SESsion:COUNt?");
        }

        /// <summary>4.11 设置 子架ID（0~7 界面不使用）</summary>
        public bool SetRackId(int rackId)
        {
            string cmd = $"SYSTem:RACK:ID {rackId}";
            return SendScpiCmd(cmd)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.12 只读 查询子架ID</summary>
        public string GetRackId()
        {
            return SendScpiCmd("SYSTem:RACK:ID?");
        }

        /// <summary>4.13 设置网口IP ethIndex:1=NAT 2=EXT 界面不使用</summary>
        public bool SetEthIp(int ethIndex, string ip, string mask, string gateway)
        {
            string cmd = $"SYSTem:IP {ethIndex},\"{ip}\",\"{mask}\",\"{gateway}\"";
            return SendScpiCmd(cmd)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.14 只读 查询指定网口IP信息</summary>
        public string GetEthIp(int ethIndex)
        {
            return SendScpiCmd($"SYSTem:IP? {ethIndex}");
        }

        /// <summary>4.15 只读 查询当前告警，slot不传查全部</summary>
        public string QueryCurrentAlarm(int? slot = null)
        {
            string cmd = slot.HasValue ? $"ALARm:CURrent? {slot}" : "ALARm:CURrent?";
            return SendScpiCmd(cmd);
        }

        /// <summary>4.16 上传日志（写操作，界面不使用，最长60s）</summary>
        public bool UploadBoardLog(int slot, string logType)
        {
            string cmd = $"LOG:UPDate {slot},{logType}";
            return SendScpiCmd(cmd, 10000)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.17 只读 读取日志 logType:work/alarm/scpi</summary>
        public string ReadBoardLog(int slot, string logType, int lineCount = 1000)
        {
            string cmd = $"LOG? {slot},{logType},{lineCount}";
            return SendScpiCmd(cmd, 10000);
        }

        /// <summary>4.18 只读 查询单板信息 TYPE/SN/DATE等</summary>
        public string QueryBoardInfo(int slot, string infoType)
        {
            return SendScpiCmd($"INFormation:CATalog? {slot},{infoType}");
        }

        /// <summary>4.19 设置数据格式 EXP/DEC 界面不使用</summary>
        public bool SetDataFormat(string fmt)
        {
            string cmd = $"DATA:FORmat {fmt}";
            return SendScpiCmd(cmd)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.20 只读 查询当前数值格式</summary>
        public string GetDataFormat()
        {
            return SendScpiCmd("DATA:FORmat?");
        }

        /// <summary>4.21 设置自动升级开关 ON/OFF 界面不使用</summary>
        public bool SetAutoUpgrade(string state)
        {
            string cmd = $"UPGrade:AUTO {state}";
            return SendScpiCmd(cmd)?.Contains("Command execute successfully") ?? false;
        }
        /// <summary>4.22 只读 查询自动升级状态</summary>
        public string GetAutoUpgradeState()
        {
            return SendScpiCmd("UPGrade:AUTO?");
        }
        #endregion

        #region 5 单板公共命令
        /// <summary>5.1 只读 查询单板序列号</summary>
        public string QueryBoardSN()
        {
            return SendScpiCmd($"{BoardPrefix}:SNUMber?");
        }
        /// <summary>5.2 只读 查询单板状态 INIT/READY/FAULT</summary>
        public string QueryBoardStatus()
        {
            return SendScpiCmd($"{BoardPrefix}:STATus?");
        }
        /// <summary>5.3 复位单板/系统 修改操作 界面不使用</summary>
        public bool ResetBoard(bool isSystemReset = false)
        {
            string cmd = isSystemReset ? "*RST" : $"{BoardPrefix}:RST";
            string res = SendScpiCmd(cmd);
            return res != null && (res.Contains("Command execute successfully") || res.Contains("system reboot"));
        }
        #endregion

        #region 6 OPM模块 只读查询（界面可用）
        /// <summary>读取OPM通道光功率 channel默认1</summary>
        public string OPM_ReadPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:SCALar:POWer:DC?");
        }
        /// <summary>查询平均采样计数当前值</summary>
        public string OPM_GetAverCount(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt?");
        }
        /// <summary>查询参考光功率</summary>
        public string OPM_GetRefPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence?");
        }
        /// <summary>查询参考模式开关</summary>
        public string OPM_GetRefState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence:STATe?");
        }
        /// <summary>查询当前设置波长</summary>
        public string OPM_GetWaveLength(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength?");
        }
        /// <summary>查询功率单位 DBM/W</summary>
        public string OPM_GetPowerUnit(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:UNIT{ch}:POWer?");
        }
        #endregion

        #region 7 VOA模块 只读查询
        /// <summary>查询VOA工作模式 ATT/POWER</summary>
        public string VOA_GetMode(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol{ch}:MODE?");
        }
        /// <summary>查询支持的模式列表</summary>
        public string VOA_GetModeList(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol{ch}:MODE:CATalog?");
        }
        /// <summary>查询最小衰减分辨率</summary>
        public string VOA_GetAttRes(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:ARESolution?");
        }
        /// <summary>查询当前衰减值</summary>
        public string VOA_GetAttenuation(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:ATTenuation?");
        }
        /// <summary>查询输入光功率</summary>
        public string VOA_GetInputPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:POWer?");
        }
        /// <summary>查询输出光功率</summary>
        public string VOA_GetOutputPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:SCALar:POWer:DC?");
        }
        /// <summary>查询输出开关状态 0关1开</summary>
        public string VOA_GetOutputState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:STATe?");
        }
        /// <summary>查询ALC自动光功率跟踪开关</summary>
        public string VOA_GetAlcState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:ALC:STATe?");
        }
        #endregion
        #region 8 SWITCH光开关只读
        public string SW_GetSwitchType()
        {
            return SendScpiCmd($"{BoardPrefix}:ROUTe:PATH:CATalog?");
        }
        public string SW_GetCurrentChannel(int inCh = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:ROUTe{inCh}:SCAN?");
        }
        public string SW_GetSwitchTotalCount()
        {
            return SendScpiCmd($"{BoardPrefix}:ROUTe:SWITch:COUNt?");
        }
        #endregion

        #region 9 ERM消光比只读
        public string ERM_ReadERData(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:ER?");
        }
        public string ERM_GetRef(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:GET{ch}:REFerence?");
        }
        public string ERM_GetRate(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:GET{ch}:RATe?");
        }
        public string ERM_GetCalibrateModel(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:GET{ch}:CLBR:INST?");
        }
        #endregion

        #region 10 LAC光源只读
        public string LAC_GetState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAC{ch}:STATe?");
        }
        public string LAC_GetPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAC{ch}:POWer?");
        }
        public string LAC_GetWave(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAC{ch}:WAVelength?");
        }
        #endregion

        #region 11 LAG可调光源只读
        public string LAG_GetState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:STATe?");
        }
        public string LAG_GetPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:POWer?");
        }
        public string LAG_GetChannel(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:CHANnel?");
        }
        public string LAG_GetFreq(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:FREQ?");
        }
        public string LAG_GetWave(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:WAVelength?");
        }
        public string LAG_GetAge(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:AGE?");
        }
        public string LAG_GetGrid(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:GRID?");
        }
        public string LAG_GetRegData(int ch, string addr)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:GET:REGister \"{addr}\"");
        }
        #endregion

        #region 12 BERT误码仪只读查询
        public string BERT_GetRate()
        {
            return SendScpiCmd($"{BoardPrefix}:SYS:RATE?");
        }
        public string BERT_GetPattern()
        {
            return SendScpiCmd($"{BoardPrefix}:SYS:PATT?");
        }
        public string BERT_GetDutStatus()
        {
            return SendScpiCmd($"{BoardPrefix}:SYS:DUT:STATUS?");
        }
        public string BERT_GetPGAmpl(int ch)
        {
            return SendScpiCmd($"{BoardPrefix}:PG:AMPL:CHAN? {ch}");
        }
        public string BERT_GetPGPol(int ch)
        {
            return SendScpiCmd($"{BoardPrefix}:PG:POL:CHAN? {ch}");
        }
        public string BERT_GetErrData(int ch)
        {
            return SendScpiCmd($"{BoardPrefix}:ED:DATA:CHAN? {ch}");
        }
        #endregion

        #region 13 PCS/PCG偏振只读
        public string PCS_GetScanState()
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol:POLarization:SCAN?");
        }
        public string PCG_GetPolarState()
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol:POLarization:STATe?");
        }
        #endregion

        #region 14 OPMT多通道功率计只读
        public string OPMT_ReadPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:SCALar:POWer:DC?");
        }
        public string OPMT_GetAverCount(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt?");
        }
        public string OPM_GetTracePoint(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:TRACedata:POINt?");
        }
        public string OPM_GetTraceResult(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:TRACedata:RESult?");
        }
        #endregion

        #region 15 TRIGGER触发只读
        public string TRIG_GetSlope(string fhType = "LITTLE_FH")
        {
            return SendScpiCmd($"TRIGger:IN:SLOPe? {fhType}");
        }
        public string TRIG_GetSource()
        {
            return SendScpiCmd("TRIGger:IN:SOURce?");
        }
        public string TRIG_GetWaveRange()
        {
            return SendScpiCmd("TRIGger:IN:WAVelength?");
        }
        public string TRIG_GetDelay()
        {
            return SendScpiCmd("TRIGger:IN:DELay?");
        }
        public string TRIG_GetLogParam()
        {
            return SendScpiCmd("TRIGger:IN:PARameter:LOGGing?");
        }
        public string SWS_GetTriggerState()
        {
            return SendScpiCmd("SWS:STATe?");
        }
        public string SWS_GetTriggerResult()
        {
            return SendScpiCmd("SWS:RESult?");
        }
        #endregion
    }
}