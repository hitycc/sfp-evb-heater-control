using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FibertopTest_Common
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
        public string DefaultIp = "192.168.100.156";
        public int DefaultPort = 5024;
        public int Timeout = 5000;
        // 原来固定常量删除，改成可修改字段
        private string _rack = "0";
        private string _slot = "01";
        private string BoardPrefix => $"LINS{_rack}{_slot}";

        // 新增动态设置槽位方法
        public void SetSlot(string slotNum)
        {
            lock (_lock)
            {
                _slot = slotNum;
            }
        }

        /// <summary>
        /// 线程安全：向指定槽位发送SCPI命令（原子操作）
        /// 【多线程关键】SetSlot + SendScpiCmd 在同一个lock内完成，
        /// 避免多线程并发时slot被其他线程篡改导致命令发错槽位。
        /// </summary>
        /// <param name="slot">目标槽位，如"05"、"07"、"11"</param>
        /// <param name="cmd">不包含板卡前缀的SCPI命令部分，如":READ1:SCALar:POWer:DC?"</param>
        /// <param name="delayMs">发送后等待延时</param>
        /// <returns>设备响应字符串</returns>
        public string SendScpiToSlot(string slot, string cmd, int delayMs = 300)
        {
            lock (_lock)
            {
                if (!IsConnected) return null;
                string savedSlot = _slot;
                try
                {
                    _slot = slot;
                    string fullCmd = BoardPrefix + cmd;
                    byte[] sendBuf = Encoding.UTF8.GetBytes(fullCmd + "\r\n");
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
                finally
                {
                    _slot = savedSlot;
                }
            }
        }

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
        #endregion

        #region 光通道设置
        /// <summary>
        /// 光开关设置通道
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <returns>设备返回字符串</returns>
        public string SW_SetChannel(int ch)
        {
            string cmd = $"{BoardPrefix}:ROUTe:SCAN {ch}";
            // 复用现有唯一收发函数 SendScpiCmd
            return SendScpiCmd(cmd);
        }
        #endregion

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
        /// <summary>5.3 复位单板/系统 修改操作 界面不使用 下发后对应槽板重启</summary>
        public bool ResetBoard(bool isSystemReset = false)
        {
            string cmd = isSystemReset ? "*RST" : $"{BoardPrefix}:RST";
            string res = SendScpiCmd(cmd);
            return res != null && (res.Contains("Command execute successfully") || res.Contains("system reboot"));
        }
        #endregion

        #region 6 OPM模块
        /// <summary>【文档6.1 READ查询光功率】
        /// SCPI:LINSxyy:READ<ch>:SCALar:POWer:DC?
        /// 响应：功率数值（浮点数），无成功文本，返回string
        /// </summary>
        /// <param name="ch">OPM通道1~6</param>
        /// <returns>光功率字符串</returns>
        public string OPM_ReadPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:SCALar:POWer:DC?");
        }

        /// <summary>【文档6.2 设置平均次数】
        /// SCPI:LINSxyy:SENSe<ch>:AVERage:COUNt <count>
        /// 文档成功响应：Command execute successfully → 返回bool
        /// </summary>
        /// <param name="ch">通道</param>
        /// <param name="averageCount">1~1000</param>
        /// <returns>true配置成功，false失败/超时</returns>
        public bool OPM_SetAverCount(int ch, int averageCount)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt {averageCount}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【文档6.3 查询平均次数（支持MAX/MIN/DEF参数）】
        /// SCPI:LINSxyy:SENSe<ch>:AVERage:COUNt? [MAX|MIN|DEF]
        /// 响应：纯数字，无成功标识，保留string
        /// </summary>
        public string OPM_GetAverCount(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt?");
        }
        // 重载：支持传MAX/MIN/DEF
        public string OPM_GetAverCount(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt? {queryType}");
        }

        /// <summary>【文档6.4 设置参考功率】
        /// 成功返回Command execute successfully
        /// </summary>
        public bool OPM_SetRefPower(int ch, double refPower, string unit = "DBM")
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence {refPower} {unit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【文档6.5 查询参考功率，支持MAX/MIN/DEF】
        /// 响应为数值，返回string
        /// </summary>
        public string OPM_GetRefPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence?");
        }
        public string OPM_GetRefPower(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence? {queryType}");
        }

        /// <summary>【文档6.6 设置参考开关状态】
        /// 成功返回Command execute successfully
        /// </summary>
        public bool OPM_SetRefState(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【文档6.7 查询参考开关，响应0/1】
        public string OPM_GetRefState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence:STATe?");
        }

        /// <summary>【文档6.8 设置工作波长】
        /// 成功返回Command execute successfully
        /// </summary>
        public bool OPM_SetWaveLength(int ch, int waveNm)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength {waveNm} NM");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【文档6.9 查询波长，支持MAX/MIN/DEF】
        public string OPM_GetWaveLength(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength?");
        }
        public string OPM_GetWaveLength(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength? {queryType}");
        }

        /// <summary>【文档6.10 设置功率单位】
        /// 成功返回Command execute successfully
        /// </summary>
        public bool OPM_SetPowerUnit(int ch, string unit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:UNIT{ch}:POWer {unit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【文档6.11 查询功率单位，响应DBM/DB/W】
        public string OPM_GetPowerUnit(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:UNIT{ch}:POWer?");
        }
        #endregion

        #region 7 VOA模块 完整读写封装（对应VOA命令文档1.1~1.27）
        /// <summary>【7.1 设置】设置通道工作模式
        /// SCPI:LINSxxx:CONTrol{ch}:MODE <mode>
        /// 参数：ATTenuation / POWer
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">VOA通道 1/2</param>
        /// <param name="mode">工作模式</param>
        /// <returns>true=设置成功；false=超时/报错</returns>
        public bool VOA_SetMode(int ch, string mode)
        {
            string res = SendScpiCmd($"{BoardPrefix}:CONTrol{ch}:MODE {mode}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.2 查询】读取当前工作模式
        /// SCPI:LINSxxx:CONTrol{ch}:MODE?
        /// 返回：ATTENUATION / POWER
        /// </summary>
        /// <param name="ch">VOA通道，默认1</param>
        /// <returns>模式字符串</returns>
        public string VOA_GetMode(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol{ch}:MODE?");
        }

        /// <summary>【7.3 查询】获取单板支持的模式列表
        /// SCPI:LINSxxx:CONTrol{ch}:MODE:CATalog?
        /// 返回逗号分隔支持模式
        /// </summary>
        public string VOA_GetModeList(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol{ch}:MODE:CATalog?");
        }

        /// <summary>【7.4 查询】读取最小衰减分辨率
        /// SCPI:LINSxxx:INPut{ch}:ARESolution?
        /// 返回：0.005 DB 格式字符串
        /// </summary>
        public string VOA_GetAttRes(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:ARESolution?");
        }

        /// <summary>【7.5 设置】设置通道衰减值
        /// SCPI:LINSxxx:INPut{ch}:ATT <attDb> DB
        /// 支持传MAX/MIN/DEF代替数值
        /// 成功响应：Command execute successfully
        /// </summary>
        public bool VOA_SetAttenuation(int ch, double attDb)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:ATT {attDb} DB");
            return res != null && res.Contains("Command execute successfully");
        }
        // 重载：支持MAX/MIN/DEF参数
        public bool VOA_SetAttenuation(int ch, string limit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:ATT {limit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.6 查询】读取当前衰减值
        /// SCPI:LINSxxx:INPut{ch}:ATT?
        /// </summary>
        public string VOA_GetAttenuation(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:ATT?");
        }
        // 重载：查询MAX/MIN/DEF默认值
        public string VOA_GetAttenuation(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:ATT? {queryType}");
        }

        /// <summary>【7.7 设置】设置衰减偏移值
        /// SCPI:LINSxxx:INPut{ch}:OFFSet <offsetDb> DB
        /// 成功响应：Command execute successfully
        /// </summary>
        public bool VOA_SetOffset(int ch, double offsetDb)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:OFFSet {offsetDb} DB");
            return res != null && res.Contains("Command execute successfully");
        }
        public bool VOA_SetOffset(int ch, string limit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:OFFSet {limit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.8 查询】读取衰减偏移值
        /// SCPI:LINSxxx:INPut{ch}:OFFSet?
        /// </summary>
        public string VOA_GetOffset(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:OFFSet?");
        }
        public string VOA_GetOffset(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:OFFSet? {queryType}");
        }

        /// <summary>【7.9 设置】设置相对衰减值
        /// SCPI:LINSxxx:INPut{ch}:RATTenuation <relDb> DB
        /// 成功响应：Command execute successfully
        /// </summary>
        public bool VOA_SetRelAtt(int ch, double relDb)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:RATTenuation {relDb} DB");
            return res != null && res.Contains("Command execute successfully");
        }
        public bool VOA_SetRelAtt(int ch, string limit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:RATTenuation {limit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.10 查询】读取相对衰减值
        /// SCPI:LINSxxx:INPut{ch}:RATTenuation?
        /// </summary>
        public string VOA_GetRelAtt(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:RATTenuation?");
        }
        public string VOA_GetRelAtt(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:RATTenuation? {queryType}");
        }

        /// <summary>【7.11 设置】设置相对衰减参考值
        /// SCPI:LINSxxx:INPut{ch}:REFerence <refDb> DB
        /// 成功响应：Command execute successfully
        /// </summary>
        public bool VOA_SetRefAtt(int ch, double refDb)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:REFerence {refDb} DB");
            return res != null && res.Contains("Command execute successfully");
        }
        public bool VOA_SetRefAtt(int ch, string limit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:REFerence {limit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.12 查询】读取相对衰减参考值
        /// SCPI:LINSxxx:INPut{ch}:REFerence?
        /// </summary>
        public string VOA_GetRefAtt(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:REFerence?");
        }
        public string VOA_GetRefAtt(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:REFerence? {queryType}");
        }

        /// <summary>【7.13 设置】设置工作波长 nm
        /// SCPI:LINSxxx:INPut{ch}:WAVelength <waveNm> NM
        /// 范围1260~1650，成功返回执行成功
        /// </summary>
        public bool VOA_SetWave(int ch, int waveNm)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:WAVelength {waveNm} NM");
            return res != null && res.Contains("Command execute successfully");
        }
        public bool VOA_SetWave(int ch, string limit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:WAVelength {limit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.14 查询】读取当前波长（米科学计数）
        /// SCPI:LINSxxx:INPut{ch}:WAVelength?
        /// </summary>
        public string VOA_GetWave(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:WAVelength?");
        }
        public string VOA_GetWave(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:WAVelength? {queryType}");
        }

        /// <summary>【7.15 查询】读取输入光功率
        /// SCPI:LINSxxx:INPut{ch}:POWer?
        /// 返回DBM数值
        /// </summary>
        public string VOA_GetInputPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:INPut{ch}:POWer?");
        }

        /// <summary>【7.16 设置】输出光路开关 ON/OFF
        /// SCPI:LINSxxx:OUTPut{ch}:STATe <state>
        /// state:ON/1 OFF/0
        /// </summary>
        public bool VOA_SetOutputState(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.17 查询】读取输出开关状态 0=关 1=开
        /// SCPI:LINSxxx:OUTPut{ch}:STATe?
        /// </summary>
        public string VOA_GetOutputState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:STATe?");
        }

        /// <summary>【7.18 设置】ALC自动功率跟踪开关 ON/OFF
        /// SCPI:LINSxxx:OUTPut{ch}:ALC:STATe <state>
        /// </summary>
        public bool VOA_SetAlcState(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:ALC:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.19 查询】读取ALC开关状态 0/1
        /// SCPI:LINSxxx:OUTPut{ch}:ALC:STATe?
        /// </summary>
        public string VOA_GetAlcState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:ALC:STATe?");
        }

        /// <summary>【7.20 设置】操作模式 ABSolute / REFerence
        /// SCPI:LINSxxx:OUTPut{ch}:APMode <mode>
        /// </summary>
        public bool VOA_SetApMode(int ch, string mode)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:APMode {mode}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.21 查询】读取当前操作模式
        /// SCPI:LINSxxx:OUTPut{ch}:APMode?
        /// </summary>
        public string VOA_GetApMode(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:APMode?");
        }

        /// <summary>【7.22 设置】功率跟踪容差门限 DB
        /// SCPI:LINSxxx:OUTPut{ch}:DTOlerance <tolDb>
        /// 范围0.05~1
        /// </summary>
        public bool VOA_SetTolerance(int ch, double tolDb)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:DTOlerance {tolDb}");
            return res != null && res.Contains("Command execute successfully");
        }
        public bool VOA_SetTolerance(int ch, string limit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:DTOlerance {limit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.23 查询】读取功率跟踪门限
        /// SCPI:LINSxxx:OUTPut{ch}:DTOlerance?
        /// </summary>
        public string VOA_GetTolerance(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:DTOlerance?");
        }
        public string VOA_GetTolerance(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:DTOlerance? {queryType}");
        }

        /// <summary>【7.24 设置】目标锁定输出功率 DBM
        /// SCPI:LINSxxx:OUTPut{ch}:POWer <powerDbm> DBM
        /// </summary>
        public bool VOA_SetOutPower(int ch, double powerDbm)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:POWer {powerDbm} DBM");
            return res != null && res.Contains("Command execute successfully");
        }
        public bool VOA_SetOutPower(int ch, string limit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:POWer {limit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【7.25 查询】读取锁定目标功率
        /// SCPI:LINSxxx:OUTPut{ch}:POWer?
        /// </summary>
        public string VOA_GetOutPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:POWer?");
        }
        public string VOA_GetOutPower(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:POWer? {queryType}");
        }

        /// <summary>【7.26 查询】读取输出实时光功率
        /// SCPI:LINSxxx:READ{ch}:SCALar:POWer:DC?
        /// </summary>
        public string VOA_GetOutputPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:SCALar:POWer:DC?");
        }

        /// <summary>【7.27 设置】OLT光功率校准（仅特定型号支持）
        /// SCPI:LINSxxx:INPut{ch}:CORRection:OLT <waveCode>
        /// 参数：1270 / 1310 / 9999
        /// </summary>
        public bool VOA_SetOltCal(int ch, int waveCode)
        {
            string res = SendScpiCmd($"{BoardPrefix}:INPut{ch}:CORRection:OLT {waveCode}");
            return res != null && res.Contains("Command execute successfully");
        }
        #endregion

        #region 8 SWITCH光开关
        
        /// <summary>
        /// 根据模块槽位和测试方向切换光开关路由
        /// </summary>
        /// <param name="moduleSlot">模块槽位 1~4</param>
        /// <param name="isTxTest">true=发射测试(模块→功率计), false=接收测试(光源→模块)</param>
        /// <returns>true=切换成功</returns>
        public bool SW_SetRouteForModule(int moduleSlot, bool isTxTest)
        {
            // SLOT-11: 模块1和模块2; SLOT-12: 模块3和模块4
            string slotNum = (moduleSlot <= 2) ? "11" : "12";
            
            int inCh, outCh;
            if (isTxTest)
            {
                // 发射测试：模块光 → 光功率计
                // 模块1/3: 输入1→输出2; 模块2/4: 输入3→输出4
                if (moduleSlot == 1 || moduleSlot == 3) { inCh = 1; outCh = 2; }
                else                                    { inCh = 3; outCh = 4; }
            }
            else
            {
                // 接收测试：光源 → 模块（反向）
                // 模块1/3: 输入2→输出1; 模块2/4: 输入4→输出3
                if (moduleSlot == 1 || moduleSlot == 3) { inCh = 2; outCh = 1; }
                else                                    { inCh = 4; outCh = 3; }
            }
            
            return SendScpiToSlot(slotNum, $":ROUTe{inCh}:SCAN {outCh}") != null;
        }

        /// <summary>【8.1 查询】获取开关型号
        /// SCPI:LINSxyy:ROUTe:PATH:CATalog?
        /// 返回示例：1x4 / 1x24
        /// </summary>
        /// <returns>开关类型字符串</returns>
        public string SW_GetSwitchType()
        {
            return SendScpiCmd($"{BoardPrefix}:ROUTe:PATH:CATalog?");
        }

        /// <summary>【8.2 设置】指定输入通道切换到目标输出通道
        /// SCPI:LINSxyy:ROUTe{inCh}:SCAN <outCh>
        /// 成功响应：Command execute successfully
        /// inCh：1*N/单路2*N/双路2*N输入端口；outCh对应各规格输出通道
        /// </summary>
        /// <param name="inCh">光开关输入端口</param>
        /// <param name="outCh">目标输出端口</param>
        /// <returns>true=切换成功，false=超时/报错/无响应</returns>
        public bool SW_SetChannel(int inCh, int outCh)
        {
            string res = SendScpiCmd($"{BoardPrefix}:ROUTe{inCh}:SCAN {outCh}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【8.3 查询】读取当前开关所在输出端口，无通路返回0
        /// SCPI:LINSxyy:ROUTe{inCh}:SCAN?
        /// 返回示例：1 / 0
        /// </summary>
        /// <param name="inCh">输入端口，默认1</param>
        /// <returns>当前输出通道数字字符串</returns>
        public string SW_GetCurrentChannel(int inCh = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:ROUTe{inCh}:SCAN?");
        }

        /// <summary>【8.4 设置】切换至下一个输出端口，末尾循环到1
        /// SCPI:LINSxyy:ROUTe{inCh}:SCAN:NEXT
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="inCh">输入端口，默认1</param>
        /// <returns>true=切换成功，false=超时/报错/无响应</returns>
        public bool SW_NextChannel(int inCh = 1)
        {
            string res = SendScpiCmd($"{BoardPrefix}:ROUTe{inCh}:SCAN:NEXT");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【8.5 设置】切换至上一个输出端口，首位循环到最大通道
        /// SCPI:LINSxyy:ROUTe{inCh}:SCAN:PREV
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="inCh">输入端口，默认1</param>
        /// <returns>true=切换成功，false=超时/报错/无响应</returns>
        public bool SW_PrevChannel(int inCh)
        {
            string res = SendScpiCmd($"{BoardPrefix}:ROUTe{inCh}:SCAN:PREV");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【8.6 查询】读取光开关硬件总切换次数
        /// SCPI:LINSxyy:ROUTe:SWITch:COUNt?
        /// 返回示例：2530
        /// </summary>
        /// <returns>累计切换次数字符串</returns>
        public string SW_GetSwitchTotalCount()
        {
            return SendScpiCmd($"{BoardPrefix}:ROUTe:SWITch:COUNt?");
        }
        #endregion

        #region 9 ERM消光比模块
        /// <summary>【9.1 查询】读取通道光功率+消光比，返回格式power,er
        /// SCPI:LINSxxx:READ{ch}:ER?
        /// 返回示例：-9.001,12.001
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>功率,消光比 逗号分隔字符串</returns>
        public string ERM_ReadERData(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:ER?");
        }

        /// <summary>【9.2 设置】设置消光比修正参考值
        /// SCPI:LINSxxx:SET{ch}:REFerence <er_test>,<er_ref>
        /// 成功响应：Command execute successfully
        /// erTest传0自动采集当前ER，erRef范围0~30dB
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="erTest">消光测量值，填0自动获取实时消光</param>
        /// <param name="erRef">消光修正参考值 0~30dB</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool ERM_SetRef(int ch, double erTest, double erRef)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SET{ch}:REFerence {erTest},{erRef}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【9.3 查询】读取消光修正参考(power_ref,er_ref)
        /// SCPI:LINSxxx:GET{ch}:REFerence?
        /// 返回示例：0.001,0.001
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>参考功率,参考消光比</returns>
        public string ERM_GetRef(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:GET{ch}:REFerence?");
        }

        /// <summary>【9.4 设置】配置信号速率：1.25G/2.5G/10G（全局生效）
        /// SCPI:LINSxxx:SET{ch}:RATe <rate>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道</param>
        /// <param name="rate">可选速率：1.25G / 2.5G / 10G</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool ERM_SetRate(int ch, string rate)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SET{ch}:RATe {rate}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【9.5 查询】读取当前配置速率
        /// SCPI:LINSxxx:GET{ch}:RATe?
        /// 返回示例：1.25G
        /// </summary>
        /// <param name="ch">通道，默认1</param>
        /// <returns>当前速率字符串</returns>
        public string ERM_GetRate(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:GET{ch}:RATe?");
        }

        /// <summary>【9.6 设置】配置校准仪器型号，支持86105C/86105D/DSA8200/DSA8300
        /// SCPI:LINSxxx:SET{ch}:CLBR:INST <model>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道</param>
        /// <param name="model">校准仪器型号</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool ERM_SetCalibrateModel(int ch, string model)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SET{ch}:CLBR:INST {model}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【9.7 查询】读取当前使用的校准仪器型号
        /// SCPI:LINSxxx:GET{ch}:CLBR:INST?
        /// 返回示例：86105C
        /// </summary>
        /// <param name="ch">通道，默认1</param>
        /// <returns>仪器型号字符串</returns>
        public string ERM_GetCalibrateModel(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:GET{ch}:CLBR:INST?");
        }
        #endregion

        #region 10 LAC光源模块
        /// <summary>【10.1 设置】设置光源输出开关 ON/OFF
        /// SCPI:LINSxxx:LAC{ch}:STATe <state>
        /// 成功响应：Command execute successfully
        /// state支持参数：ON/1 开启，OFF/0 关闭
        /// </summary>
        /// <param name="ch">光源通道，默认1</param>
        /// <param name="state">ON / 1 开启；OFF / 0 关闭</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAC_SetState(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAC{ch}:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【10.2 查询】读取光源输出状态 0=关 1=开
        /// SCPI:LINSxxx:LAC{ch}:STATe?
        /// 返回示例：0 / 1
        /// </summary>
        /// <param name="ch">光源通道，默认1</param>
        /// <returns>状态数字字符串</returns>
        public string LAC_GetState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAC{ch}:STATe?");
        }

        /// <summary>【10.3 设置】设置光源输出功率 DBM
        /// SCPI:LINSxxx:LAC{ch}:POWer <powerVal>
        /// 成功响应：Command execute successfully
        /// C波段范围8.8~17.8，L波段9.0~14.5
        /// </summary>
        /// <param name="ch">光源通道</param>
        /// <param name="powerVal">目标功率值(DBM)</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAC_SetPower(int ch, double powerVal)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAC{ch}:POWer {powerVal}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【10.4 查询】读取当前输出功率(DBM，科学计数返回)
        /// SCPI:LINSxxx:LAC{ch}:POWer?
        /// 返回示例：1.3000000E+01
        /// </summary>
        /// <param name="ch">光源通道，默认1</param>
        /// <returns>功率科学计数字符串</returns>
        public string LAC_GetPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAC{ch}:POWer?");
        }

        /// <summary>【10.5 设置】设置工作波长 单位NM
        /// SCPI:LINSxxx:LAC{ch}:WAVelength <waveNm>
        /// 成功响应：Command execute successfully
        /// C:1527.6~1568.6  L:1568.8~1611.7
        /// </summary>
        /// <param name="ch">光源通道</param>
        /// <param name="waveNm">波长数值(NM)</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAC_SetWave(int ch, int waveNm)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAC{ch}:WAVelength {waveNm}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【10.6 查询】读取当前波长(返回米科学计数)
        /// SCPI:LINSxxx:LAC{ch}:WAVelength?
        /// 返回示例：1.550000E-06
        /// </summary>
        /// <param name="ch">光源通道，默认1</param>
        /// <returns>波长米单位科学计数字符串</returns>
        public string LAC_GetWave(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAC{ch}:WAVelength?");
        }
        #endregion

        #region 11 LAG可调光源
        /// <summary>【11.1 设置】LAG输出开关 ON/OFF
        /// SCPI:LINSxxx:LAG{ch}:STATe <state>
        /// 成功响应：Command execute successfully
        /// state支持：ON/1 开启，OFF/0 关断
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <param name="state">ON / 1 开启；OFF / 0 关断</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAG_SetState(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAG{ch}:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【11.2 查询】读取LAG输出状态 0=关 1=开
        /// SCPI:LINSxxx:LAG{ch}:STATe?
        /// 返回示例：0 / 1
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>状态数字字符串</returns>
        public string LAG_GetState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:STATe?");
        }

        /// <summary>【11.3 设置】设置输出光功率 范围7.5~13.5dBm
        /// SCPI:LINSxxx:LAG{ch}:POWer <power>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="power">目标功率dBm，区间7.5~13.5</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAG_SetPower(int ch, double power)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAG{ch}:POWer {power}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【11.4 查询】读取当前输出功率
        /// SCPI:LINSxxx:LAG{ch}:POWer?
        /// 返回示例：10
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>功率数值字符串</returns>
        public string LAG_GetPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:POWer?");
        }

        /// <summary>【11.5 设置】设置工作通道 范围1~104
        /// SCPI:LINSxxx:LAG{ch}:CHANnel <workCh>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="workCh">工作通道1~104</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAG_SetChannel(int ch, int workCh)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAG{ch}:CHANnel {workCh}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【11.6 查询】读取当前工作通道
        /// SCPI:LINSxxx:LAG{ch}:CHANnel?
        /// 返回示例：47
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>通道数字字符串</returns>
        public string LAG_GetChannel(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:CHANnel?");
        }

        /// <summary>【11.7 设置】设置工作频率 191100~196250 GHz
        /// SCPI:LINSxxx:LAG{ch}:FREQ <freq>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="freq">频率GHz，区间191100~196250</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAG_SetFreq(int ch, int freq)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAG{ch}:FREQ {freq}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【11.8 查询】读取当前工作频率
        /// SCPI:LINSxxx:LAG{ch}:FREQ?
        /// 返回示例：191100
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>频率数字字符串</returns>
        public string LAG_GetFreq(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:FREQ?");
        }

        /// <summary>【11.9 查询】读取计算出的工作波长(nm)
        /// SCPI:LINSxxx:LAG{ch}:WAVelength?
        /// 返回示例：1.568362E+03
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>波长科学计数字符串</returns>
        public string LAG_GetWave(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:WAVelength?");
        }

        /// <summary>【11.10 查询】读取光源寿命百分比 0~100
        /// SCPI:LAG{ch}:AGE?
        /// 返回示例：0（100代表寿命耗尽）
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>寿命百分比字符串</returns>
        public string LAG_GetAge(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:AGE?");
        }

        /// <summary>【11.11 设置】设置通道间隔Grid 1~6553 GHz，光源关闭才可设置
        /// SCPI:LAG{ch}:GRID <gridVal>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="gridVal">网格间隔1~6553GHz</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAG_SetGrid(int ch, int gridVal)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAG{ch}:GRID {gridVal}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【11.12 查询】读取当前通道间隔
        /// SCPI:LAG{ch}:GRID?
        /// 返回示例：50
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>网格间隔数字字符串</returns>
        public string LAG_GetGrid(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:GRID?");
        }

        /// <summary>【11.13 查询】读取频率微调范围(MHz)
        /// SCPI:LAG{ch}:LIMit:FINetune?
        /// 返回示例：6000
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>微调范围字符串</returns>
        public string LAG_GetFineTuneLimit(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:LIMit:FINetune?");
        }

        /// <summary>【11.14 查询】读取频率上下限范围
        /// SCPI:LAG{ch}:LIMit:FREQ?
        /// 返回示例：191100,196250
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>最小,最大频率</returns>
        public string LAG_GetFreqLimit(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:LIMit:FREQ?");
        }

        /// <summary>【11.15 查询】读取功率上下限范围
        /// SCPI:LAG{ch}:LIMit:POWer?
        /// 返回示例：7.5,13.5
        /// </summary>
        /// <param name="ch">通道号，默认1</param>
        /// <returns>最小,最大功率dBm</returns>
        public string LAG_GetPowerLimit(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:LIMit:POWer?");
        }

        /// <summary>【11.16 设置】写入寄存器 addr=0x开头16进制，value=0x开头，delay延时ms
        /// SCPI:LAG{ch}:SET:REGister "addr,value,delay"
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="addr">寄存器地址，0x开头16进制</param>
        /// <param name="value">写入值，0x开头16进制</param>
        /// <param name="delay">写入等待延时ms</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool LAG_SetRegData(int ch, string addr, string value, int delay)
        {
            string res = SendScpiCmd($"{BoardPrefix}:LAG{ch}:SET:REGister \"{addr},{value},{delay}\"");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【11.17 查询】读取指定寄存器值 addr=0x开头16进制
        /// SCPI:LAG{ch}:GET:REGister "<addr>"
        /// 返回示例：0x10
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="addr">寄存器地址，0x开头16进制</param>
        /// <returns>寄存器16进制数值</returns>
        public string LAG_GetRegData(int ch, string addr)
        {
            return SendScpiCmd($"{BoardPrefix}:LAG{ch}:GET:REGister \"{addr}\"");
        }
        #endregion

        #region 12 BERT误码仪
        /// <summary>【12.1 设置】设置传输速率码值 0~0x10
        /// SCPI:LINSxyy:SYS:RATE <rateCode>
        /// 成功响应：:SYS:RATE:OK
        /// </summary>
        /// <param name="rateCode">速率编码 0~0x10</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool BERT_SetRate(string rateCode)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SYS:RATE {rateCode}");
            return res != null && res.Contains(":SYS:RATE:OK");
        }

        /// <summary>【12.2 查询】读取当前速率码值
        /// SCPI:LINSxyy:SYS:RATE?
        /// 返回示例：0x01
        /// </summary>
        /// <returns>速率十六进制码字符串</returns>
        public string BERT_GetRate()
        {
            return SendScpiCmd($"{BoardPrefix}:SYS:RATE?");
        }

        /// <summary>【12.3 设置】设置PRBS码型 0~7
        /// SCPI:LINSxyy:SYS:PATT <pattCode>
        /// 成功响应：:SYS:PATT:OK
        /// </summary>
        /// <param name="pattCode">码型编号0~7</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool BERT_SetPattern(int pattCode)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SYS:PATT {pattCode}");
            return res != null && res.Contains(":SYS:PATT:OK");
        }

        /// <summary>【12.4 查询】读取当前码型
        /// SCPI:LINSxyy:SYS:PATT?
        /// 返回示例：0x00
        /// </summary>
        /// <returns>码型编号字符串</returns>
        public string BERT_GetPattern()
        {
            return SendScpiCmd($"{BoardPrefix}:SYS:PATT?");
        }

        /// <summary>【12.5 设置】DUT发送状态掩码 0~0xFF
        /// SCPI:LINSxyy:SYS:DUT:STATUS <mask>
        /// 成功响应：:SYS:DUT:STATUS OK
        /// </summary>
        /// <param name="mask">状态掩码0~0xFF</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool BERT_SetDutStatus(int mask)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SYS:DUT:STATUS {mask}");
            return res != null && res.Contains(":SYS:DUT:STATUS OK");
        }

        /// <summary>【12.6 查询】读取DUT收发状态寄存器
        /// SCPI:LINSxyy:SYS:DUT:STATUS?
        /// 返回示例：:SYS:DUT:STATUS 0x00
        /// </summary>
        /// <returns>DUT状态完整返回字符串</returns>
        public string BERT_GetDutStatus()
        {
            return SendScpiCmd($"{BoardPrefix}:SYS:DUT:STATUS?");
        }

        /// <summary>【12.7 查询】指定通道PG发送幅度
        /// SCPI:LINSxyy:PG:AMPL:CHAN? <ch>
        /// 返回示例：0x0f
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <returns>幅度十六进制编码</returns>
        public string BERT_GetPGAmpl(int ch)
        {
            return SendScpiCmd($"{BoardPrefix}:PG:AMPL:CHAN? {ch}");
        }

        /// <summary>【12.8 设置】所有通道PG发送幅度 0~0x2F
        /// SCPI:LINSxyy:PG:AMPL:ALL <amplCode>
        /// 成功响应：:PG:AMPL:ALL:OK
        /// </summary>
        /// <param name="amplCode">幅度编码0~0x2F</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool BERT_SetAllAmpl(string amplCode)
        {
            string res = SendScpiCmd($"{BoardPrefix}:PG:AMPL:ALL {amplCode}");
            return res != null && res.Contains(":PG:AMPL:ALL:OK");
        }

        /// <summary>【12.9 查询】指定通道极性
        /// SCPI:LINSxyy:PG:POL:CHAN? <ch>
        /// 返回：0=正常，1=反转
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <returns>极性值字符串</returns>
        public string BERT_GetPGPol(int ch)
        {
            return SendScpiCmd($"{BoardPrefix}:PG:POL:CHAN? {ch}");
        }

        /// <summary>【12.10 设置】所有通道极性 0正常/1反转
        /// SCPI:LINSxyy:PG:POL:ALL <pol>
        /// 成功响应：:PG:POL:ALL:OK
        /// </summary>
        /// <param name="pol">0正常，1反转</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool BERT_SetAllPol(int pol)
        {
            string res = SendScpiCmd($"{BoardPrefix}:PG:POL:ALL {pol}");
            return res != null && res.Contains(":PG:POL:ALL:OK");
        }

        /// <summary>【12.11 查询】读取通道误码/总比特/锁定状态
        /// SCPI:LINSxyy:ED:DATA:CHAN? <ch>
        /// 返回格式：误码数,总比特,锁定标记
        /// 示例：0 13616087040 1
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <returns>三组数据逗号/空格分隔字符串</returns>
        public string BERT_GetErrData(int ch)
        {
            return SendScpiCmd($"{BoardPrefix}:ED:DATA:CHAN? {ch}");
        }

        /// <summary>【12.12 控制】清空所有通道误码计数
        /// SCPI:LINSxyy:ED:CLEAR:ALL
        /// 成功响应：:ED:CLEAR:ALL:OK
        /// </summary>
        /// <returns>true=清除成功，false=超时/无响应/报错</returns>
        public bool BERT_ClearAllErr()
        {
            string res = SendScpiCmd($"{BoardPrefix}:ED:CLEAR:ALL");
            return res != null && res.Contains(":ED:CLEAR:ALL:OK");
        }
        #endregion

        #region 13 PCS/PCG偏振模块 完整读写封装（对应PCSPCG文档全部SCPI指令）
        /// <summary>【13.1 PCS专用】启动偏振随机扫描
        /// SCPI:LINSxxx:CONTrol:POLarization:SCAN 1,<pol_count>
        /// 参数pol_count：扫描点数 1~1000
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="polCount">扫描点数，取值范围1~1000</param>
        /// <returns>true=扫描启动成功；false=超时/无响应/报错</returns>
        public bool PCS_StartPolScan(int polCount)
        {
            string res = SendScpiCmd($"{BoardPrefix}:CONTrol:POLarization:SCAN 1,{polCount}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【13.2 查询】读取偏振扫描运行状态
        /// SCPI:LINSxxx:CONTrol:POLarization:SCAN?
        /// 返回：1=扫描中，0=停止
        /// 返回示例：1
        /// </summary>
        /// <returns>扫描状态数字字符串</returns>
        public string PCS_GetScanState()
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol:POLarization:SCAN?");
        }

        /// <summary>【13.3 PCG专用】设置固定偏振态
        /// SCPI:LINSxxx:CONTrol:POLarization:STATe <pol_state>
        /// 1=90°、2=0°、3=45°、4=-45°、5=LHC、6=RHC
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="polState">偏振态编号 1~6</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool PCG_SetPolarState(int polState)
        {
            string res = SendScpiCmd($"{BoardPrefix}:CONTrol:POLarization:STATe {polState}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【13.4 查询】读取当前偏振态编号
        /// SCPI:LINSxxx:CONTrol:POLarization:STATe?
        /// 返回值范围1~6
        /// 返回示例：1
        /// </summary>
        /// <returns>偏振态编号字符串</returns>
        public string PCG_GetPolarState()
        {
            return SendScpiCmd($"{BoardPrefix}:CONTrol:POLarization:STATe?");
        }
        #endregion

        #region 14 OPMT多通道功率计
        /// <summary>【14.1 查询】读取指定通道实时光功率
        /// SCPI:LINSxxx:READ{ch}:SCALar:POWer:DC?
        /// 返回示例：1.000000E-01，单位由UNIT配置
        /// </summary>
        /// <param name="ch">通道号，OPMT02/04/06分别支持1~2/1~4/1~6，默认1</param>
        /// <returns>光功率数值字符串</returns>
        public string OPMT_ReadPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:READ{ch}:SCALar:POWer:DC?");
        }

        /// <summary>【14.2 设置】设置通道平均采样次数 1~1000
        /// SCPI:LINSxxx:SENSe{ch}:AVERage:COUNt <cnt>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道号</param>
        /// <param name="count">平均次数1~1000</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool OPMT_SetAverCount(int ch, int count)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt {count}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.3 查询】读取当前平均次数/MAX/MIN/DEF
        /// SCPI:LINSxxx:SENSe{ch}:AVERage:COUNt?
        /// 返回示例：100
        /// </summary>
        /// <param name="ch">通道，默认1</param>
        /// <returns>平均次数</returns>
        public string OPMT_GetAverCount(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt?");
        }
        /// <summary>【14.3 重载】查询最大/最小/默认平均次数
        /// </summary>
        public string OPMT_GetAverCount(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:AVERage:COUNt? {queryType}");
        }

        /// <summary>【14.4 设置】设置参考光功率，支持DBM/W单位
        /// SCPI:LINSxxx:SENSe{ch}:POWer:REFerence <val> DBM
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道</param>
        /// <param name="refVal">参考功率数值</param>
        /// <param name="unit">单位DBM/W，默认DBM</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool OPMT_SetRefPower(int ch, double refVal, string unit = "DBM")
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence {refVal} {unit}");
            return res != null && res.Contains("Command execute successfully");
        }
        /// <summary>【14.4 重载】设置最大/最小/默认参考功率
        /// </summary>
        public bool OPMT_SetRefPower(int ch, string limitKey)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence {limitKey}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.5 查询】读取当前参考功率
        /// SCPI:LINSxxx:SENSe{ch}:POWer:REFerence?
        /// 返回示例：-10
        /// </summary>
        /// <param name="ch">通道，默认1</param>
        public string OPMT_GetRefPower(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence?");
        }
        public string OPMT_GetRefPower(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:REFerence? {queryType}");
        }

        /// <summary>【14.6 设置】参考模式开关 ON/OFF
        /// SCPI:LINSxxx:SENSe{ch}:POWer:REFerence:STATe <state>
        /// state支持ON/1、OFF/0；成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道</param>
        /// <param name="state">开关状态</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool OPMT_SetRefState(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.7 查询】参考模式状态 0/1
        /// SCPI:LINSxxx:SENSe{ch}:POWer:REFerence:STATe?
        /// 返回：0关闭，1开启
        /// </summary>
        public string OPMT_GetRefState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:REFerence:STATe?");
        }

        /// <summary>【14.8 设置】工作波长 nm
        /// SCPI:LINSxxx:SENSe{ch}:POWer:WAVelength <nm> NM
        /// 范围800~1650；成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道</param>
        /// <param name="waveNm">波长数值nm</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool OPMT_SetWave(int ch, int waveNm)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength {waveNm} NM");
            return res != null && res.Contains("Command execute successfully");
        }
        public bool OPMT_SetWave(int ch, string limitKey)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength {limitKey}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.9 查询】当前波长（米科学计数）
        /// SCPI:LINSxxx:SENSe{ch}:POWer:WAVelength?
        /// 返回示例：1.310000E-06
        /// </summary>
        public string OPMT_GetWave(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength?");
        }
        public string OPMT_GetWave(int ch, string queryType)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:POWer:WAVelength? {queryType}");
        }

        /// <summary>【14.10 设置】功率单位 DBM/DB/W/W/W
        /// SCPI:LINSxxx:UNIT{ch}:POWer <unit>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">通道</param>
        /// <param name="unit">功率单位</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool OPMT_SetPowerUnit(int ch, string unit)
        {
            string res = SendScpiCmd($"{BoardPrefix}:UNIT{ch}:POWer {unit}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.11 查询】当前功率单位
        /// SCPI:LINSxxx:UNIT{ch}:POWer?
        /// 返回示例：DBM
        /// </summary>
        public string OPMT_GetPowerUnit(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:UNIT{ch}:POWer?");
        }

        /// <summary>【14.12 设置】模拟输出开关 ON/OFF
        /// SCPI:LINSxxx:OUTPut{ch}:ANALog:STATe <state>
        /// state：ON/1 使能，OFF/0关闭；成功响应：Command execute successfully
        /// </summary>
        public bool OPMT_SetAnalogState(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:ANALog:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.13 查询】模拟输出状态 0/1
        /// SCPI:LINSxxx:OUTPut{ch}:ANALog:STATe?
        /// </summary>
        public string OPMT_GetAnalogState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:ANALog:STATe?");
        }

        /// <summary>【14.14 设置】模拟输出功率量程 min,max
        /// SCPI:LINSxxx:OUTPut{ch}:ANALog:RANGe <min>,<max>
        /// 范围-70~30dBm；成功响应：Command execute successfully
        /// </summary>
        public bool OPMT_SetAnalogRange(int ch, double minDbm, double maxDbm)
        {
            string res = SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:ANALog:RANGe {minDbm},{maxDbm}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.15 查询】模拟输出功率上下限
        /// SCPI:LINSxxx:OUTPut{ch}:ANALog:RANGe?
        /// 返回示例：-70,10
        /// </summary>
        public string OPMT_GetAnalogRange(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:OUTPut{ch}:ANALog:RANGe?");
        }

        /// <summary>【14.16 设置单通道触发模式 START/STOP
        /// SCPI:LINSxxx:SENSe{ch}:FUNCtion:STATe LOGGing,<sta>
        /// sta=START/STOP；成功响应：Command execute successfully
        /// </summary>
        public bool OPMT_SetTriggerSingle(int ch, string state)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:FUNCtion:STATe LOGGing,{state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.17 查询单通道触发完成状态
        /// SCPI:LINSxxx:SENSe{ch}:FUNCtion:STATe?
        /// 返回：COMPLETE/INCOMPLETE
        /// </summary>
        public string OPMT_GetTriggerSingleState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:FUNCtion:STATe?");
        }

        /// <summary>【14.18 设置全部通道触发状态
        /// SCPI:LINSxxx:SENSe{ch}:FUNCtion:STATe:ALL cnt,sta1,sta2...
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="ch">OPMT通道号</param>
        /// <param name="chCount">通道总数2/4/6</param>
        /// <param name="stateList">各通道状态0=关闭 1=开启</param>
        /// <returns>true=设置成功；false=超时/无响应/报错</returns>
        public bool OPMT_SetTriggerAll(int ch, int chCount, params int[] stateList)
        {
            /*string args = string.Join(",", stateList);*/
            // 改成 .NET 3.5 兼容的写法
            string[] strArray = Array.ConvertAll(stateList, x => x.ToString());
            string args = string.Join(",", strArray);
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:FUNCtion:STATe:ALL {chCount},{args}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.19 查询所有通道触发开关
        /// SCPI:LINSxxx:SENSe{ch}:FUNCtion:STATe:ALL?
        /// 返回逗号分隔各通道状态
        /// </summary>
        public string OPMT_GetTriggerAllState(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:FUNCtion:STATe:ALL?");
        }

        /// <summary>【14.20 读取触发采样二进制数据
        /// SCPI:LINSxxx:SENSe{ch}:FUNCtion:RESult?
        /// 二进制原始数据，直接返回字符串
        /// </summary>
        public string OPM_GetTraceResult(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:FUNCtion:RESult?");
        }

        /// <summary>【14.21 设置连续采样周期+点数
        /// SCPI:LINSxxx:SENSe{ch}:TRACedata:POINT <period>,<cnt>
        /// period:1~100ms，点数1~100000；成功响应：Command execute successfully
        /// </summary>
        public bool OPMT_SetTracePoint(int ch, int periodMs, int pointCnt)
        {
            string res = SendScpiCmd($"{BoardPrefix}:SENSe{ch}:TRACedata:POINT {periodMs},{pointCnt}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【14.22 查询当前采样周期、总点数、已采点数
        /// SCPI:LINSxxx:SENSe{ch}:TRACedata:POINT?
        /// 返回示例：1,100000,90000
        /// </summary>
        public string OPM_GetTracePoint(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:TRACedata:POINT?");
        }

        /// <summary>【14.23 读取连续采样二进制功率数据
        /// SCPI:LINSxxx:SENSe{ch}:TRACedata:RESult?
        /// 二进制原始数据包
        /// </summary>
        public string OPM_GetTraceData(int ch = 1)
        {
            return SendScpiCmd($"{BoardPrefix}:SENSe{ch}:TRACedata:RESult?");
        }
        #endregion

        #region 15 TRIGGER触发模块 完整读写封装（对应TRIGGER全部SCPI指令）
        /// <summary>【15.1 设置】配置触发帧类型+边沿
        /// SCPI:TRIGger:IN:SLOPe <fhType>,<slope>
        /// fhType可选 BIG_FH / LITTLE_FH；slope可选 NEGative / POSitive / PULSE
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="fhType">帧头类型 BIG_FH / LITTLE_FH</param>
        /// <param name="slope">触发边沿 NEGative/POSitive/PULSE</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool TRIG_SetSlope(string fhType, string slope)
        {
            string res = SendScpiCmd($"TRIGger:IN:SLOPe {fhType},{slope}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【15.2 查询】读取当前触发边沿
        /// SCPI:TRIGger:IN:SLOPe? <fhType>
        /// 返回示例：NEGATIVE
        /// </summary>
        /// <param name="fhType">帧头类型，默认LITTLE_FH</param>
        /// <returns>当前触发沿字符串</returns>
        public string TRIG_GetSlope(string fhType = "LITTLE_FH")
        {
            return SendScpiCmd($"TRIGger:IN:SLOPe? {fhType}");
        }

        /// <summary>【15.3 设置】选择触发输入源
        /// SCPI:TRIGger:IN:SOURce <src>
        /// 可选：IN1/IN2/IN3/SYNC_IN/INTERNALx
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="source">触发源</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool TRIG_SetSource(string source)
        {
            string res = SendScpiCmd($"TRIGger:IN:SOURce {source}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【15.4 查询】读取当前触发输入源
        /// SCPI:TRIGger:IN:SOURce?
        /// 返回示例：IN1
        /// </summary>
        /// <returns>触发源字符串</returns>
        public string TRIG_GetSource()
        {
            return SendScpiCmd("TRIGger:IN:SOURce?");
        }

        /// <summary>【15.5 设置】扫描起止波长+步长
        /// SCPI:TRIGger:IN:WAVelength <start>,<stop>,<step>
        /// 波长单位nm，范围1260~1650，步长0~0.01nm
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="startWave">起始波长nm</param>
        /// <param name="stopWave">终止波长nm</param>
        /// <param name="stepWave">扫描步长nm</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool TRIG_SetWaveRange(double startWave, double stopWave, double stepWave)
        {
            string res = SendScpiCmd($"TRIGger:IN:WAVelength {startWave},{stopWave},{stepWave}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【15.6 查询】读取扫描波长配置
        /// SCPI:TRIGger:IN:WAVelength?
        /// 返回示例：1530.000, 1560.000, 0.010
        /// </summary>
        /// <returns>起始,终止,步长</returns>
        public string TRIG_GetWaveRange()
        {
            return SendScpiCmd("TRIGger:IN:WAVelength?");
        }

        /// <summary>【15.7 设置】触发延时，单位ns，必须640整数倍
        /// SCPI:TRIGger:IN:DELay <delayNs>
        /// 取值0~2684354560ns；成功响应：Command execute successfully
        /// </summary>
        /// <param name="delayNs">延时纳秒值</param>
        /// <returns>true=设置成功，false=超时/无响应/报错</returns>
        public bool TRIG_SetDelay(long delayNs)
        {
            string res = SendScpiCmd($"TRIGger:IN:DELay {delayNs}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【15.8 查询】读取当前触发延时
        /// SCPI:TRIGger:IN:DELay?
        /// 返回示例：160000ns
        /// </summary>
        /// <returns>延时数值带单位</returns>
        public string TRIG_GetDelay()
        {
            return SendScpiCmd("TRIGger:IN:DELay?");
        }

        /// <summary>【15.9 设置】扫描点数+点间隔时间
        /// SCPI:TRIGger:IN:PARameter:LOGGing <cnt>,<time>
        /// sampleCnt：1~50000；intervalUs：10~1000us，10整数倍
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="sampleCnt">单次扫描点数</param>
        /// <param name="intervalUs">点间隔微秒</param>
        /// <returns>true=设置成功，false=超时/无响应</returns>
        public bool TRIG_SetLogParam(int sampleCnt, int intervalUs)
        {
            string res = SendScpiCmd($"TRIGger:IN:PARameter:LOGGing {sampleCnt},{intervalUs}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【15.10 查询】读取扫描点数与间隔
        /// SCPI:TRIGger:IN:PARameter:LOGGing?
        /// 返回示例：8191,10us
        /// </summary>
        /// <returns>点数,间隔时间</returns>
        public string TRIG_GetLogParam()
        {
            return SendScpiCmd("TRIGger:IN:PARameter:LOGGing?");
        }

        /// <summary>【15.11 设置】系统扫描启停 START / STOP
        /// SCPI:SWS:STATe <state>
        /// 成功响应：Command execute successfully
        /// </summary>
        /// <param name="state">START启动 / STOP停止</param>
        /// <returns>true=设置成功，false=超时/无响应</returns>
        public bool SWS_SetTriggerState(string state)
        {
            string res = SendScpiCmd($"SWS:STATe {state}");
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>【15.12 查询】读取系统扫描状态
        /// SCPI:SWS:STATe?
        /// 返回：START / STOP
        /// </summary>
        /// <returns>当前扫描状态</returns>
        public string SWS_GetTriggerState()
        {
            return SendScpiCmd("SWS:STATe?");
        }

        /// <summary>【15.13 查询】读取完整扫描数据
        /// SCPI:SWS:RESult?
        /// 返回复杂波长+多通道功率原始数据包
        /// </summary>
        /// <returns>扫描原始结果字符串</returns>
        public string SWS_GetTriggerResult()
        {
            return SendScpiCmd("SWS:RESult?");
        }
        #endregion
    }
}