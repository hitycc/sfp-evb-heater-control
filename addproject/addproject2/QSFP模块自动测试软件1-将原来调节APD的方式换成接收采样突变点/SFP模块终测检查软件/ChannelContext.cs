using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SFP模块终测检查软件
{
    //===========================================================================
    // ChannelContext —— 每个通道/每个模块的"独立工作包"
    //
    // 【为什么需要这个类？】
    //   原来的代码中 TestSet、TestResult、DOA、MEMTER 全部是 static（全局共享）。
    //   单线程顺序测试没问题，因为永远只有一个通道在用这些变量。
    //   但多线程时，4个线程同时读写同一组 static 变量，数据会互相覆盖、乱套！
    //
    //   解决办法：把"每通道独有"的数据全部放进这个类，每个线程 new 一个自己的
    //   ChannelContext 实例，各用各的，互不干扰。
    //
    // 【打个比方】
    //   原来就像4个人共用一张草稿纸（static变量），你写我擦，全乱了。
    //   现在给每个人发一张独立的草稿纸（ChannelContext实例），各写各的。
    //===========================================================================
    public class ChannelContext
    {
        //=======================================================================
        // 基本标识
        //=======================================================================

        private int _channelIndex;
        private StringBuilder _logBuilder;

        /// <summary>通道编号（0~3）</summary>
        public int ChannelIndex { get { return _channelIndex; } }

        /// <summary>该通道的测试日志文本</summary>
        public StringBuilder LogBuilder { get { return _logBuilder; } }

        /// <summary>该通道是否正在测试中</summary>
        public volatile bool IsTesting = false;

        /// <summary>该通道测试是否通过</summary>
        public volatile bool TestPassed = false;

        /// <summary>该通道是否有模块插入</summary>
        public volatile bool ModuleOnline = true;

        /// <summary>错误信息</summary>
        public string ErrorMessage = "";

        //=======================================================================
        // 模块驱动引用
        //=======================================================================

        /// <summary>当前通道的模块驱动对象（SfpDriverAdapter等）</summary>
        public object ModuleTest = null;

        //=======================================================================
        // 模块身份信息
        //=======================================================================

        public string fibertop_bn = "00000";
        public string tosa_sn = "11111";
        public string rosa_sn = "22222";
        public string fibertop_sn = "";
        public string fibertop_pn = "";
        public string sn = "";
        public string pn = "";
        public string vn = "";
        public string date = "";
        public string chipType = "";
        public string bitRate = "";
        public string softType = "";
        public string softVer = "";
        public bool chipIsOK = false;
        public bool wpIsEn = false;
        public bool moduleIsSR = false;

        //=======================================================================
        // Flash数据
        //=======================================================================

        public byte[] flash_data = new byte[2048];
        public int flash_data_len = 2048;

        //=======================================================================
        // DDM实时监控值（模块整体）
        //=======================================================================

        public float current = 0;         // 模块工作电流(A)
        public float wavelength = 0;      // 波长(nm)
        public float txErDCA = 0;         // DCA/ERM读取消光比(dB)

        public float tempDDM = 0;
        public float vccDDM = 0;

        public float tempHA = 0, tempLA = 0, tempHW = 0, tempLW = 0;
        public float vccHA = 0, vccLA = 0, vccHW = 0, vccLW = 0;

        public bool tempHA_flag = false, tempLA_flag = false, tempHW_flag = false, tempLW_flag = false;
        public bool vccHA_flag = false, vccLA_flag = false, vccHW_flag = false, vccLW_flag = false;

        //=======================================================================
        // 当前Lane（模块内部光通道 0~3）
        //=======================================================================

        public int CurrentLane = 0;

        //=======================================================================
        // 每个Lane的测试参数设置
        //=======================================================================

        public UInt16[] txapc = new UInt16[4];
        public UInt16[] txmod = new UInt16[4];
        public byte[] rxlos = new byte[4];
        public byte[] rxapd = new byte[4];
        public UInt16[] tosa_temp = new UInt16[4];
        public byte[] tx_von = new byte[4];
        public Byte txpeVal = 0;

        public UInt16 txapcVal = 0;
        public UInt16 txmodVal = 0;
        public UInt16 rxlosVal = 0;
        public UInt16 rxapdVal = 0;

        public float rxPowerDDM = 0;

        public UInt16 txapcFindMin = 0, txapcFindMax = 0;
        public UInt16 txmodFindMin = 0, txmodFindMax = 0;
        public byte rxlosFindMin = 0, rxlosFindMax = 0;
        public bool txapcFirstFind = true;
        public bool txmodFirstFind = true;
        public bool rxlosFirstFind = true;

        public float txapc_val = 0.1f;
        public float txer_val = 0.3f;
        public float rxlos_val = 0.5f;

        public float[] rxPwr_Real = new float[5];
        public float[] rxPwr_Check = new float[5];

        public float rxPwrMaxErr = 1f;
        public float txPwrMaxErr = 1f;
        public float erValMaxErr = 1f;

        public float[] rxOverLoadAtt = new float[4];
        public float[] rxSenAtt = new float[4];
        public float[] rxALosAtt = new float[4];
        public float[] rxDLosAtt = new float[4];

        public string PssChannel = "";

        //=======================================================================
        // 波长调试
        //=======================================================================

        public float[] wl_min = new float[4];
        public float[] wl_max = new float[4];
        public float[] wl_target = new float[4];
        public UInt16[] tosa_tempbufmin = new UInt16[4];
        public UInt16[] tosa_tempbufmax = new UInt16[4];
        public UInt16 tosa_tempValmin = 0;
        public UInt16 tosa_tempValmax = 0;
        public int EMLTestType = 0;

        //=======================================================================
        // 测试结果 — 发射参数
        //=======================================================================

        public float txPowerDCA = 0;
        public float[] txPower = new float[4];
        public float[] txEr = new float[4];
        public float[] txESN = new float[4];
        public float[] txCrossing = new float[4];
        public float[] txJiterRMS = new float[4];
        public float[] txJiterPP = new float[4];
        public float[] txJiterTT = new float[4];

        public byte[] txEye_image_ch0 = null;
        public byte[] txEye_image_ch1 = null;
        public byte[] txEye_image_ch2 = null;
        public byte[] txEye_image_ch3 = null;
        public int bimage_len = 0;

        public float[] txBiasDDM = new float[4];
        public float[] txPowerDDM = new float[4];
        public float txBiasDDMSingle = 0;
        public float txPowerDDMSingle = 0;

        public float[] txBiasHA = new float[4], txBiasLA = new float[4], txBiasHW = new float[4], txBiasLW = new float[4];
        public float[] txPowerHA = new float[4], txPowerLA = new float[4], txPowerHW = new float[4], txPowerLW = new float[4];

        public bool[] txBiasHA_flag = new bool[4], txBiasLA_flag = new bool[4], txBiasHW_flag = new bool[4], txBiasLW_flag = new bool[4];
        public bool[] txPwrHA_flag = new bool[4], txPwrLA_flag = new bool[4], txPwrHW_flag = new bool[4], txPwrLW_flag = new bool[4];

        public float[] txPwrErr = new float[4];
        public float[] txErErr = new float[4];

        //=======================================================================
        // 测试结果 — SFP单通道便捷属性（用于ChannelTester）
        //=======================================================================

        /// <summary>工作电流(mA) — 便捷属性</summary>
        public float currentValue = 0;

        /// <summary>发射光功率(dBm) OPM实测 — 便捷属性</summary>
        public float TxPowerDbm = 0;

        /// <summary>偏置电流(mA) — 便捷属性</summary>
        public float BiasMa = 0;

        /// <summary>消光比(dB) ERM实测 — 便捷属性</summary>
        public float ErDb = 0;

        //=======================================================================
        // 测试结果 — 接收参数
        //=======================================================================

        public float[] rxPowerDDMSingle = new float[4];
        public float[][] rxPwrErr = new float[4][];
        public float[][] rxPwrReal = new float[4][];
        public float[][] rxPwrDDM = new float[4][];

        public float[] rxSen = new float[4];
        public float[] rxDLos = new float[4];
        public float[] rxALos = new float[4];
        public float[] rxOverLoad = new float[4];

        public float[] rxPowerHA = new float[4], rxPowerLA = new float[4], rxPowerHW = new float[4], rxPowerLW = new float[4];

        public bool[] rxPwrHA_flag = new bool[4], rxPwrLA_flag = new bool[4], rxPwrHW_flag = new bool[4], rxPwrLW_flag = new bool[4];

        public double[] wLength = new double[4];

        //=======================================================================
        // SFP单通道专用字段（SFF-8472协议）
        //=======================================================================

        // 芯片状态
        public byte chipid = 0;
        public bool initok = false;
        public bool txoff_flag = false;
        public UInt64 max_Fsn = 999999999999;

        // DDM单通道值（SFP只用1个通道）
        public float temp = 0;
        public float vcc = 0;
        public float bias = 0;
        public float tx_power = 0;
        public float rx_power = 0;

        // 告警标志（单通道版）
        public bool tempHA_flag_s = false, tempLA_flag_s = false, tempHW_flag_s = false, tempLW_flag_s = false;
        public bool vccHA_flag_s = false, vccLA_flag_s = false, vccHW_flag_s = false, vccLW_flag_s = false;
        public bool txBiasHA_flag_s = false, txBiasLA_flag_s = false, txBiasHW_flag_s = false, txBiasLW_flag_s = false;
        public bool txPwrHA_flag_s = false, txPwrLA_flag_s = false, txPwrHW_flag_s = false, txPwrLW_flag_s = false;
        public bool rxPwrHA_flag_s = false, rxPwrLA_flag_s = false, rxPwrHW_flag_s = false, rxPwrLW_flag_s = false;
        public bool rxLosHA_flag = false, rxLosLA_flag = false;
        public bool txFaultH_flag = false;
        public bool dataNReady_flag = false;

        // SFP阈值（A2h 0-55）
        public byte[] sfp_threshold = new byte[56];
        // SFP外校准（A2h 56-94）
        public byte[] sfp_ex_cal = new byte[39];

        // 3段线性接收功率校准结果
        public float rxpwrNoPwrADC = 0;
        public UInt16 rxpwrNoPwrADCval = 0;

        //=======================================================================
        // 校准系数
        //=======================================================================

        public float txPwrCal_k = 0;
        public float txPwrCal_b = 0;
        public float[] rxPwrCal_c = new float[5];
        public byte rxNoPwrVal = 0;

        public float[] rxPwrCal_k = new float[3];
        public float[] rxPwrCal_b = new float[3];
        public UInt16[] rxAdcCal = new UInt16[6];

        public UInt16[] rxAdc = new UInt16[6];

        //=======================================================================
        // 衰减器/光开关状态
        //=======================================================================

        public float currentAtt = 0;
        public int optoAttDelay = 8;
        public float optoAttOffset = 0;
        public float ERCalOffset = 0;
        public int meterDelay = 300;
        public bool meterTypeDesktop = true;

        public string gpibAddress = "GPIB0::07::INSTR";
        public string tester_no = "";

        //=======================================================================
        // UI同步上下文
        //=======================================================================

        public SynchronizationContext UISyncContext { get; set; }

        //=======================================================================
        // 事件
        //=======================================================================

        public event Action<string> LogUpdated;
        public event Action<string> StatusUpdated;
        public event Action<bool> TestCompleted;
        public event Action DataUpdated;

        //=======================================================================
        // 构造函数
        //=======================================================================

        public ChannelContext(int channelIndex)
        {
            _channelIndex = channelIndex;
            _logBuilder = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                rxPwrErr[i] = new float[5];
                rxPwrReal[i] = new float[5];
                rxPwrDDM[i] = new float[5];
            }

            SetDefaults();
        }

        //=======================================================================
        // 默认值初始化
        //=======================================================================

        private void SetDefaults()
        {
            rxPwr_Check[0] = 0.5f;
            rxPwr_Check[1] = 1.0f;
            rxPwr_Check[2] = 1.5f;
            rxPwr_Check[3] = 2.0f;
            rxPwr_Check[4] = 2.0f;

            rxPwr_Real[0] = -8f;
            rxPwr_Real[1] = -11f;
            rxPwr_Real[2] = -16f;
            rxPwr_Real[3] = -22f;
            rxPwr_Real[4] = -26f;

            for (int i = 0; i < 4; i++)
            {
                rxlos[i] = 0x38;
                rxapd[i] = 0xFF;
                tosa_temp[i] = 830;
                tx_von[i] = 0;
                rxOverLoadAtt[i] = 0;
                rxSenAtt[i] = 19;
                rxALosAtt[i] = 20;
                rxDLosAtt[i] = 25;
                wl_min[i] = 0;
                wl_max[i] = 0;
                wl_target[i] = 0;
                tosa_tempbufmin[i] = 830;
                tosa_tempbufmax[i] = 1830;
            }

            optoAttDelay = 8;
            meterDelay = 300;
            currentAtt = 0;
            txPowerDCA = -40;
            TxPowerDbm = -40;
            bimage_len = 0;
            txapc_val = 0.1f;
            txer_val = 0.3f;
            rxlos_val = 0.5f;
        }

        //=======================================================================
        // 辅助方法
        //=======================================================================

        /// <summary>添加日志（线程安全）</summary>
        public void AddLog(string message)
        {
            if (message == null || message.Trim().Length == 0) return;

            string line = string.Format("[CH{0}] {1}\r\n", _channelIndex, message);
            _logBuilder.Append(line);

            Action<string> handler = LogUpdated;
            if (UISyncContext != null)
            {
                UISyncContext.Post(delegate(object state) {
                    Action<string> h = LogUpdated;
                    if (h != null) h(line);
                }, null);
            }
            else
            {
                if (handler != null) handler(line);
            }
        }

        /// <summary>更新状态提示</summary>
        public void UpdateStatus(string status)
        {
            Action<string> handler = StatusUpdated;
            if (UISyncContext != null)
            {
                UISyncContext.Post(delegate(object state) {
                    Action<string> h = StatusUpdated;
                    if (h != null) h(status);
                }, null);
            }
            else
            {
                if (handler != null) handler(status);
            }
        }

        /// <summary>通知UI刷新数据</summary>
        public void NotifyDataUpdated()
        {
            Action handler = DataUpdated;
            if (UISyncContext != null)
            {
                UISyncContext.Post(delegate(object state) {
                    Action h = DataUpdated;
                    if (h != null) h();
                }, null);
            }
            else
            {
                if (handler != null) handler();
            }
        }

        /// <summary>清除日志</summary>
        public void ClearLog()
        {
            _logBuilder.Length = 0;
        }

        /// <summary>重置所有测试数据</summary>
        public void ResetTestResults()
        {
            Reset();
        }

        /// <summary>重置所有测试数据</summary>
        public void Reset()
        {
            ErrorMessage = "";
            TestPassed = false;
            ModuleOnline = true;
            CurrentLane = 0;

            txPowerDCA = -40;
            TxPowerDbm = -40;
            ErDb = 0;
            BiasMa = 0;
            currentValue = 0;
            bimage_len = 0;
            for (int i = 0; i < 4; i++)
            {
                txPower[i] = -40;
                txEr[i] = 0;
                txESN[i] = 0;
                txCrossing[i] = 0;
                txJiterRMS[i] = 0;
                txJiterPP[i] = 0;
                txJiterTT[i] = 0;
                txBiasDDM[i] = 0;
                txPowerDDM[i] = -60;
                rxPowerDDMSingle[i] = -60;
                rxSen[i] = 0;
                rxDLos[i] = 0;
                rxALos[i] = 0;
                rxOverLoad[i] = 0;
                wLength[i] = 0;
                txPwrErr[i] = 0;
                txErErr[i] = 0;
            }
            txBiasDDMSingle = 0;
            txPowerDDMSingle = -60;
        }
    }
}