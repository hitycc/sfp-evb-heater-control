using System;

namespace FibertopTest_Common
{
    /// <summary>
    /// 模块驱动统一接口 — 多通道测试架构使用
    /// SFP和QSFP驱动适配器都实现此接口，ChannelTester通过此接口操作模块
    /// </summary>
    public interface IModuleDriver
    {
        // ===== 初始化与信息 =====
        void Init(I2C i2c);
        bool CheckTestTypeInfo();
        bool ReadInfo();
        bool SetLowPowerMode(bool enable);
        bool TxDisableAll(bool disable);
        bool TxSelect(bool isTxTest);
        bool SetTxRate(int rateGbps);
        bool SetRxRate(int rateGbps);

        // ===== DDM读取 =====
        double ReadTemperature();
        double ReadVoltage();
        double ReadTxBias(int ch);
        double ReadTxPower(int ch);
        double ReadRxPower(int ch);

        // ===== 发射测试 =====
        bool SetTxPower(int ch, double targetPowerDbm, out double finalPowerDbm);
        bool SetER(int ch, double targetER, out double finalER);
        bool SetBias(int ch, int biasDac);
        bool SetModulation(int ch, int modDac);
        int GetBias(int ch);
        int GetModulation(int ch);
        bool CalibrateTxPower(int ch, double opmPowerDbm);
        bool CalibrateTxBias(int ch, double measuredMa);

        // ===== 接收测试 =====
        bool SetLosDac(int ch, int losDac);
        int GetLosDac(int ch);
        string GetRxLos(int ch);
        bool SetRxDDEM(int ch, bool enable);
        bool CalibrateRxPower(int ch, double opmPowerDbm, bool isRxPowerTest = false);

        // ===== 接收调试（APD突变点检测）=====
        /// <summary>读取Rx ADC原始值（用于突变点扫描）</summary>
        ushort ReadRxADC(int ch);
        /// <summary>设置APD偏压DAC值</summary>
        bool SetAPD(int ch, ushort dacValue);

        // ===== 告警标志 =====
        bool GetTxLosFlag(int ch);
        bool GetRxLosFlag(int ch);

        // ===== DDM辅助 =====
        bool GetDDMAnalogValues();
        bool GetDDMFlagsInterrupt();

        // ===== Flash操作 =====
        bool WriteFlashData();
        bool ReadFlashData();
        byte[] GetFlashBuffer();
        bool CheckFlashData();
        bool EEPROMCheckSum();

        // ===== 阈值设置 =====
        bool WriteAlarmThresholds(int ch, double tHigh, double tLow, double vHigh, double vLow,
            double biasHigh, double biasLow, double txHigh, double txLow, double rxHigh, double rxLow);

        // ===== 辅助方法 =====
        void Delay(int ms);
        bool SelectTable(int page);
    }
}