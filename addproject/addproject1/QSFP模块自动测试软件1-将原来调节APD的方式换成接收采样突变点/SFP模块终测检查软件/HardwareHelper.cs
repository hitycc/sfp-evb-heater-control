using System;
using System.Threading;
using FibertopTest_Common;

namespace FibertopTest_Common
{
    /// <summary>
    /// 硬件操作线程安全助手
    /// 所有共享硬件访问都加锁，防止多线程争抢
    ///
    /// 设备一览:
    ///   SFP_EVB_Heater heater     — SFP加热台(电源/TxDis/I2C转发/RxLOS)  per-slot独立
    ///   OTP12Driver  otp12        — 光开关/光功率计/衰减器/光源 多槽位一体, 内部已线程安全
    ///   DCA_86100    scope_86100d — 眼图仪(示波器), 单台
    ///   Keysight86120C kt86120c  — 波长计, 单台
    ///   SerialPort   opticalSwitch — 光开关串口(旧), 如不用otp12则用此
    /// </summary>
    public static class HardwareHelper
    {
        // ------------------------------------------------------------------
        // 加热台 (SFP_EVB_Heater) —— per-slot调用自身带锁, 但GPIO写也要锁
        // ------------------------------------------------------------------

        /// <summary>模块上电: state=1开, state=0关</summary>
        public static bool ModulePowerOn(int slot, int state)
        {
            if (GlobalVarFun.heater == null || !GlobalVarFun.heater.IsOpen) return false;
            return GlobalVarFun.heater.SetPowerEN(state, slot);
        }

        /// <summary>设置模块供电电压(3.3V)</summary>
        public static bool ModuleSetVoltage(int slot, double voltage)
        {
            if (GlobalVarFun.heater == null || !GlobalVarFun.heater.IsOpen) return false;
            return GlobalVarFun.heater.SetVoltage(voltage, slot);
        }

        /// <summary>Tx发射使能: state=0开激光(低有效), state=1关激光</summary>
        public static bool ModuleTxEnable(int slot, int state)
        {
            if (GlobalVarFun.heater == null || !GlobalVarFun.heater.IsOpen) return false;
            return GlobalVarFun.heater.SetTxDis(state, slot);
        }

        /// <summary>读取硬件RxLOS状态(来自加热台引脚): "1"=LOS告警(无光), "0"=有光</summary>
        public static string ModuleGetRxLOS(int slot)
        {
            if (GlobalVarFun.heater == null || !GlobalVarFun.heater.IsOpen) return "";
            return GlobalVarFun.heater.GetRxLos(slot);
        }

        /// <summary>读取模块电流(A)</summary>
        public static double ModuleGetCurrent(int slot)
        {
            if (GlobalVarFun.heater == null || !GlobalVarFun.heater.IsOpen) return -1;
            string s = GlobalVarFun.heater.GetCurrent(slot);
            double v;
            return double.TryParse(s, out v) ? v : -1;
        }

        // ------------------------------------------------------------------
        // 光开关/光功率计/光源 (OTP12Driver) —— 内部已线程安全(SendScpiToSlot原子)
        // ------------------------------------------------------------------

        /// <summary>
        /// SFP光开关路由：将光开关切换到指定模块的Tx/Rx路径
        /// slot: 1~4
        /// isTxTest: true=发射测试(模块→光功率计/DCA), false=接收测试(光源→模块)
        /// </summary>
        public static bool OpticalSwitchRoute(int slot, bool isTxTest)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            return GlobalVarFun.otp12.SW_SetRouteForModule(slot, isTxTest);
        }

        /// <summary>读取OPM光功率(dBm), channel: OPM通道号(默认1)</summary>
        public static double ReadOPMPower(int channel = 1)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return -99;
            // 用SendScpiToSlot保证线程安全，使用默认VOA/OPM所在槽位
            string res = GlobalVarFun.otp12.OPM_ReadPower(channel);
            double v;
            if (double.TryParse(res, out v)) return v;
            return -99;
        }

        /// <summary>设置VOA衰减(dB), channel: VOA通道(默认1)</summary>
        public static bool SetVOAAttenuation(int channel, double attDb)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            return GlobalVarFun.otp12.VOA_SetAttenuation(channel, attDb);
        }

        /// <summary>读取VOA当前衰减(dB)</summary>
        public static double GetVOAAttenuation(int channel = 1)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return -1;
            string res = GlobalVarFun.otp12.VOA_GetAttenuation(channel);
            double v;
            return double.TryParse(res, out v) ? v : -1;
        }

        /// <summary>光源开关(OTP12 LAC模块): state="ON"/"OFF", channel:LAC通道(默认1)</summary>
        public static bool SourceSetState(int channel, string state)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            return GlobalVarFun.otp12.LAC_SetState(channel, state);
        }

        /// <summary>设置光源功率(dBm)</summary>
        public static bool SourceSetPower(int channel, double powerDbm)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            return GlobalVarFun.otp12.LAC_SetPower(channel, powerDbm);
        }

        /// <summary>设置光源波长(nm)</summary>
        public static bool SourceSetWavelength(int channel, int waveNm)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            return GlobalVarFun.otp12.LAC_SetWave(channel, waveNm);
        }

        // ------------------------------------------------------------------
        // 波长计 (Keysight 86120C) —— 单台, 需加锁
        // ------------------------------------------------------------------

        /// <summary>读取波长(m) → 返回nm</summary>
        public static double ReadWavelength()
        {
            lock (SharedHardwareLocks.WavelengthMeterLock)
            {
                if (GlobalVarFun.kt86120c == null || !GlobalVarFun.kt86120c.IsConnected) return -1;
                try
                {
                    double wl_m = GlobalVarFun.kt86120c.GetWavelength();
                    return wl_m * 1e9; // m→nm
                }
                catch { return -1; }
            }
        }

        // ------------------------------------------------------------------
        // 眼图仪 (DCA_86100D/N1092X) —— 单台, 需加锁
        // ------------------------------------------------------------------

        private static string GpibAddr => GlobalVarFun.gpibname ?? "";

        /// <summary>清屏+运行眼图</summary>
        public static void DCARun()
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return;
                GlobalVarFun.scope_86100d.SetClearDisplay(addr, 10);
                GlobalVarFun.scope_86100d.SetRun(addr);
            }
        }

        /// <summary>自动缩放</summary>
        public static void DCAAutoScale()
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return;
                GlobalVarFun.scope_86100d.SetAutoScale(addr, 25);
            }
        }

        /// <summary>读取眼图光功率(dBm)</summary>
        public static float DCAReadPower()
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return -99;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return -99;
                try { return GlobalVarFun.scope_86100d.GetPower(addr); }
                catch { return -99; }
            }
        }

        /// <summary>读取消光比(dB), calOffset: 校准偏差</summary>
        public static float DCAReadER(float calOffset = 0f)
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return -1;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return -1;
                try
                {
                    float er = GlobalVarFun.scope_86100d.GetExtRatio(addr);
                    return er + calOffset;
                }
                catch { return -1; }
            }
        }

        /// <summary>读取交叉点(%)</summary>
        public static float DCAReadCrossing()
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return -1;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return -1;
                try { return GlobalVarFun.scope_86100d.GetCrossing(addr); }
                catch { return -1; }
            }
        }

        /// <summary>读取Jitter RMS (ps)</summary>
        public static double DCAReadJitterRMS()
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return -1;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return -1;
                try { return GlobalVarFun.scope_86100d.GetJitterRMS(addr); }
                catch { return -1; }
            }
        }

        /// <summary>读取Jitter PP (ps)</summary>
        public static double DCAReadJitterPP()
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return -1;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return -1;
                try { return GlobalVarFun.scope_86100d.GetJitterPP(addr); }
                catch { return -1; }
            }
        }

        /// <summary>读取ESN(dB)</summary>
        public static float DCAReadESN()
        {
            lock (SharedHardwareLocks.OscilloscopeLock)
            {
                if (GlobalVarFun.scope_86100d == null) return -99;
                string addr = GpibAddr;
                if (string.IsNullOrEmpty(addr)) return -99;
                try { return GlobalVarFun.scope_86100d.GetEyeSNR(addr); }
                catch { return -99; }
            }
        }
    }
}