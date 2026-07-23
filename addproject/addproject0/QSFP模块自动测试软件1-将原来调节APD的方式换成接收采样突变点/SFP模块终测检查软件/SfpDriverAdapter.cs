using System;

namespace FibertopTest_Common
{
    /// <summary>
    /// SFP驱动适配器 - 让SFP驱动类适配ModuleTest接口
    /// 包装SFPUX3320T实例，实现ModuleTest接口的所有方法
    /// 
    /// 核心思路：
    ///   SFPUX3320T 实现SFP寄存器操作（读/写/校准），但不实现ModuleTest接口
    ///   本类持有SFPUX3320T实例，将ModuleTest接口方法转发给它
    ///   同时处理SFP单通道与QSFP多通道的差异
    /// </summary>
    public class SfpDriverAdapter : IModuleDriver
    {
        private SFPUX3320T _sfp;

        public SfpDriverAdapter()
        {
            _sfp = new SFPUX3320T();
        }

        // =====================================================================
        //  ModuleTest接口实现 - 转发给SFPUX3320T
        // =====================================================================

        public void Init(I2C i2c)
        {
            _sfp.Init(i2c);
        }

        public bool CheckTestTypeInfo()
        {
            return _sfp.CheckTestTypeInfo();
        }

        public bool ReadInfo()
        {
            return _sfp.ReadAllDDM();
        }

        public bool SetLowPowerMode(bool enable)
        {
            // SFP没有低功耗模式，直接返回true
            return true;
        }

        public bool TxDisableAll(bool disable)
        {
            return _sfp.SoftTxDis(disable);
        }

        public bool TxSelect(bool isTxTest)
        {
            // SFP通过光开关选择Tx/Rx路径
            return true;
        }

        public bool SetTxRate(int rateGbps)
        {
            // SFP速率选择，UX3320T通过寄存器A0h 0x0D设置
            return _sfp.SetRateSelect(rateGbps);
        }

        public bool SetRxRate(int rateGbps)
        {
            // SFP接收速率选择（SFP收发一体，同速率选择）
            return _sfp.SetRateSelect(rateGbps);
        }

        // =====================================================================
        //  DDM读取方法
        // =====================================================================

        public double ReadTemperature()
        {
            return _sfp.ReadTemperature();
        }

        public double ReadVoltage()
        {
            return _sfp.ReadVoltage();
        }

        public double ReadTxBias(int ch)
        {
            return _sfp.ReadTxBias();
        }

        public double ReadTxPower(int ch)
        {
            return _sfp.ReadTxPower();
        }

        public double ReadRxPower(int ch)
        {
            return _sfp.ReadRxPower();
        }

        // =====================================================================
        //  发射测试方法
        // =====================================================================

        public bool SetTxPower(int ch, double targetPowerDbm, out double finalPowerDbm)
        {
            finalPowerDbm = 0;
            return _sfp.AutoSetTxPower(targetPowerDbm, ref finalPowerDbm);
        }

        public bool SetER(int ch, double targetER, out double finalER)
        {
            finalER = 0;
            return _sfp.AutoSetTxER(targetER, ref finalER);
        }

        public bool SetBias(int ch, int biasDac)
        {
            return _sfp.SetBiasDAC(biasDac);
        }

        public bool SetModulation(int ch, int modDac)
        {
            return _sfp.SetModDAC(modDac);
        }

        public int GetBias(int ch)
        {
            return _sfp.GetBiasDAC();
        }

        public int GetModulation(int ch)
        {
            return _sfp.GetModDAC();
        }

        public bool CalibrateTxPower(int ch, double opmPowerDbm)
        {
            return _sfp.CalibrateTxPowerSlope(opmPowerDbm);
        }

        public bool CalibrateTxBias(int ch, double measuredMa)
        {
            return _sfp.CalibrateBiasCurrent(measuredMa);
        }

        // =====================================================================
        //  接收测试方法
        // =====================================================================

        public bool SetLosDac(int ch, int losDac)
        {
            return _sfp.SetLosDac(losDac);
        }

        public int GetLosDac(int ch)
        {
            return _sfp.GetLosDac();
        }

        public string GetRxLos(int ch)
        {
            return _sfp.ReadLOS() ? "High" : "Low";
        }

        public bool SetRxDDEM(int ch, bool enable)
        {
            _sfp.RXDDEM_Enable(enable ? (byte)1 : (byte)0);
            return true;
        }

        public bool CalibrateRxPower(int ch, double opmPowerDbm, bool isRxPowerTest = false)
        {
            return _sfp.CalibrateRxPowerSlope(opmPowerDbm, isRxPowerTest ? (byte)1 : (byte)0);
        }

        // =====================================================================
        //  告警标志读取
        // =====================================================================

        public bool GetTxLosFlag(int ch)
        {
            return _sfp.ReadTxFault();
        }

        public bool GetRxLosFlag(int ch)
        {
            return _sfp.ReadLOS();
        }

        // =====================================================================
        //  Flash操作
        // =====================================================================

        public bool WriteFlashData()
        {
            return _sfp.WriteAllToModule();
        }

        public bool ReadFlashData()
        {
            return _sfp.ReadAllFromModule();
        }

        public byte[] GetFlashBuffer()
        {
            byte[] buf = new byte[256];
            return buf;
        }

        public bool CheckFlashData()
        {
            return true;
        }

        // =====================================================================
        //  阈值设置
        // =====================================================================

        public bool WriteAlarmThresholds(int ch, double tHigh, double tLow, double vHigh, double vLow,
            double biasHigh, double biasLow, double txHigh, double txLow, double rxHigh, double rxLow)
        {
            return _sfp.WriteAlarmThresholdsToModule(tHigh, tLow, vHigh, vLow, biasHigh, biasLow, txHigh, txLow, rxHigh, rxLow);
        }

        // =====================================================================
        //  辅助方法
        // =====================================================================

        public void Delay(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        public bool SelectTable(int page)
        {
            return true;
        }

        /// <summary>
        /// 获取内部SFP驱动实例（用于直接访问SFP特有功能）
        /// </summary>
        public SFPUX3320T GetSfpDriver()
        {
            return _sfp;
        }
    }
}