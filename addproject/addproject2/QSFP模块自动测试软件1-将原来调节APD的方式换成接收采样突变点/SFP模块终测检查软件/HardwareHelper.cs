using System;
using System.Threading;
using FibertopTest_Common;

namespace SFP模块终测检查软件
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
    /// </summary>
    public static class HardwareHelper
    {
        // ------------------------------------------------------------------
        // 加热台 (SFP_EVB_Heater) —— per-slot调用
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

        /// <summary>创建指定槽位的I2C通信对象</summary>
        public static I2C CreateI2CForSlot(int slot)
        {
            if (GlobalVarFun.heater == null || !GlobalVarFun.heater.IsOpen) return null;
            return new I2C_Heater(GlobalVarFun.heater, slot);
        }

        // ------------------------------------------------------------------
        // 光开关/光功率计/光源 (OTP12Driver) —— 使用HardwareMap多槽位路由
        // ------------------------------------------------------------------

        /// <summary>
        /// SFP光开关路由：将光开关切换到指定模块的Tx/Rx路径（使用HardwareMap）
        /// slot: 1~4
        /// isTxTest: true=发射测试(模块→光功率计/DCA), false=接收测试(光源→模块)
        /// </summary>
        public static bool OpticalSwitchRoute(int slot, bool isTxTest)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            int chIdx = slot - 1;
            var sw = isTxTest ? HardwareMap.GetTxSwitch(chIdx) : HardwareMap.GetRxSwitch(chIdx);
            return GlobalVarFun.otp12.SW_SetChannelToSlot(sw.Slot, sw.InCh, sw.OutCh);
        }

        /// <summary>读取指定模块的Tx VOA衰减(dB), slot:1~4</summary>
        public static bool SetTxVOA(int slot, double attDb)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            int chIdx = slot - 1;
            VoaLocation loc = HardwareMap.GetTxVoa(chIdx);
            return GlobalVarFun.otp12.VOA_SetAttenuationToSlot(loc.Slot, loc.Channel, attDb);
        }

        /// <summary>读取指定模块的Rx VOA衰减(dB), slot:1~4</summary>
        public static bool SetRxVOA(int slot, double attDb)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            int chIdx = slot - 1;
            VoaLocation loc = HardwareMap.GetRxVoa(chIdx);
            return GlobalVarFun.otp12.VOA_SetAttenuationToSlot(loc.Slot, loc.Channel, attDb);
        }

        /// <summary>设置VOA衰减(dB)到指定OTP通道 — 默认方法(向后兼容)</summary>
        public static bool SetVOAAttenuation(int channel, double attDb)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            return GlobalVarFun.otp12.VOA_SetAttenuation(channel, attDb);
        }

        /// <summary>读取指定模块的Tx光功率(OPM), slot:1~4</summary>
        public static double ReadTxOPMPower(int slot)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return -99;
            string opmSlot;
            int opmCh;
            HardwareMap.GetOpm(slot - 1, out opmSlot, out opmCh);
            string res = GlobalVarFun.otp12.OPM_ReadPowerFromSlot(opmSlot, opmCh);
            double v;
            if (double.TryParse(res, out v)) return v;
            return -99;
        }

        /// <summary>读取OPM光功率(dBm) — 默认方法(向后兼容, 读slot=05 ch=1)</summary>
        public static double ReadOPMPower(int channel = 1)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return -99;
            string res = GlobalVarFun.otp12.OPM_ReadPower(channel);
            double v;
            if (double.TryParse(res, out v)) return v;
            return -99;
        }

        /// <summary>读取VOA当前衰减(dB) — 默认方法(向后兼容)</summary>
        public static double GetVOAAttenuation(int channel = 1)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return -1;
            string res = GlobalVarFun.otp12.VOA_GetAttenuation(channel);
            double v;
            return double.TryParse(res, out v) ? v : -1;
        }

        /// <summary>
        /// 打开/关闭指定模块的Rx测试光源（slot:1~4，按HardwareMap映射到对应LAC通道）
        /// </summary>
        public static bool SourceSetState(int slot, string state)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            var loc = HardwareMap.GetLac(slot - 1);
            return GlobalVarFun.otp12.LAC_SetStateToSlot(loc.Slot, loc.Channel, state);
        }

        /// <summary>
        /// 设置指定模块的Rx测试光源功率（dBm），slot:1~4
        /// </summary>
        public static bool SourceSetPower(int slot, double powerDbm)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            var loc = HardwareMap.GetLac(slot - 1);
            return GlobalVarFun.otp12.LAC_SetPowerToSlot(loc.Slot, loc.Channel, powerDbm);
        }

        /// <summary>
        /// 设置指定模块的Rx测试光源波长（nm），slot:1~4
        /// </summary>
        public static bool SourceSetWavelength(int slot, int waveNm)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            var loc = HardwareMap.GetLac(slot - 1);
            return GlobalVarFun.otp12.LAC_SetWaveToSlot(loc.Slot, loc.Channel, waveNm);
        }

        /// <summary>
        /// 读取指定模块Rx端输入光功率（从Rx VOA输入侧读取），slot:1~4
        /// </summary>
        public static double ReadRxInputPower(int slot)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return -99;
            var loc = HardwareMap.GetRxVoa(slot - 1);
            string res = GlobalVarFun.otp12.VOA_GetInputPowerFromSlot(loc.Slot, loc.Channel);
            double v;
            return double.TryParse(res, out v) ? v : -99;
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

        // ------------------------------------------------------------------
        // ERM 消光比仪 (OTP12 SLOT-06) —— 共享仪器, 需加锁
        // ------------------------------------------------------------------

        /// <summary>
        /// 读取指定模块的ER值（从ERM-04消光比仪），slot:1~4
        /// 返回消光比(dB)，失败返回-1
        /// </summary>
        public static double ErmReadER(int slot)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return -1;
            lock (SharedHardwareLocks.OtpDriverLock)
            {
                string ermSlot;
                int ermCh;
                HardwareMap.GetErm(slot - 1, out ermSlot, out ermCh);
                string res = GlobalVarFun.otp12.ERM_ReadERData(ermCh);
                if (string.IsNullOrEmpty(res)) return -1;
                // 返回格式: "power,er"
                string[] parts = res.Split(',');
                if (parts.Length >= 2)
                {
                    double er;
                    if (double.TryParse(parts[1].Trim(), out er)) return er;
                }
                return -1;
            }
        }

        /// <summary>
        /// 读取指定模块的ER和光功率（从ERM-04），slot:1~4
        /// </summary>
        public static bool ErmReadPowerAndER(int slot, out double powerDbm, out double erDb)
        {
            powerDbm = -99;
            erDb = -1;
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            lock (SharedHardwareLocks.OtpDriverLock)
            {
                string ermSlot;
                int ermCh;
                HardwareMap.GetErm(slot - 1, out ermSlot, out ermCh);
                string res = GlobalVarFun.otp12.ERM_ReadERData(ermCh);
                if (string.IsNullOrEmpty(res)) return false;
                string[] parts = res.Split(',');
                if (parts.Length >= 2)
                {
                    double p, e;
                    bool okP = double.TryParse(parts[0].Trim(), out p);
                    bool okE = double.TryParse(parts[1].Trim(), out e);
                    if (okP) powerDbm = p;
                    if (okE) erDb = e;
                    return okE;
                }
                return false;
            }
        }

        /// <summary>
        /// 设置ERM速率（SFP 1.25G/2.5G等）
        /// </summary>
        public static bool ErmSetRate(int slot, string rate)
        {
            if (GlobalVarFun.otp12 == null || !GlobalVarFun.otp12.IsConnected) return false;
            lock (SharedHardwareLocks.OtpDriverLock)
            {
                string ermSlot;
                int ermCh;
                HardwareMap.GetErm(slot - 1, out ermSlot, out ermCh);
                return GlobalVarFun.otp12.ERM_SetRate(ermCh, rate);
            }
        }

        /// <summary>
        /// Rx ADC突变点检测（APD/PIN跳变点扫描）
        /// 通过扫描APD偏压，监测Rx ADC值的突变点来确定最佳工作点
        /// 返回突变点对应的APD DAC值，失败返回-1
        /// </summary>
        /// <param name="slot">模块槽位1~4</param>
        /// <param name="readAdcFunc">读取Rx ADC值的回调函数(返回UInt16)</param>
        /// <param name="setApdFunc">设置APD DAC的回调函数(UInt16)</param>
        /// <param name="apdMin">APD扫描最小值</param>
        /// <param name="apdMax">APD扫描最大值</param>
        /// <param name="step">扫描步长</param>
        /// <param name="jumpThreshold">ADC突变阈值(检测跳变的最小变化量)</param>
        public static int FindRxAdcJumpPoint(int slot,
            Func<ushort> readAdcFunc, Action<ushort> setApdFunc,
            int apdMin, int apdMax, int step, int jumpThreshold)
        {
            if (readAdcFunc == null || setApdFunc == null) return -1;

            int bestApd = -1;
            int maxDiff = 0;
            int prevAdc = 0;

            for (int apd = apdMin; apd <= apdMax; apd += step)
            {
                setApdFunc((ushort)apd);
                System.Threading.Thread.Sleep(20); // 等待APD稳定
                int currAdc = readAdcFunc();

                if (apd > apdMin)
                {
                    int diff = Math.Abs(currAdc - prevAdc);
                    if (diff > maxDiff && diff >= jumpThreshold)
                    {
                        maxDiff = diff;
                        bestApd = apd;
                    }
                }
                prevAdc = currAdc;
            }

            return bestApd;
        }
    }
}
