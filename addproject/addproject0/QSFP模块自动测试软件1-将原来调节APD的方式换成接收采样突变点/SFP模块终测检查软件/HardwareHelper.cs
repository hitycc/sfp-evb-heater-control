using System;
using FibertopTest_Common;

namespace SFP模块终测检查软件
{
    //===========================================================================
    // HardwareHelper —— 线程安全的硬件操作帮助类
    //
    // 封装4通道并行测试所需的OTP12硬件操作。
    // 所有方法都通过SendScpiToSlot原子操作，避免多线程竞争。
    //
    // 使用示例：
    //   HardwareHelper hh = new HardwareHelper(otp12, channelIndex:0);
    //   hh.SetTxSwitch();       // 把SLOT-11输入1切到输出2（模块1发射通路）
    //   double pwr = hh.ReadOpmPower();  // 读SLOT-05通道1光功率
    //   hh.SetTxVoaAtt(5.0);   // 设置SLOT-07 ch1衰减5dB
    //===========================================================================

    public class HardwareHelper
    {
        private readonly OTP12Driver _otp;
        private readonly int _channelIndex; // 0~3
        private readonly VoaLocation _txVoa;
        private readonly VoaLocation _rxVoa;
        private readonly SwitchLocation _txSwitch;
        private readonly SwitchLocation _rxSwitch;
        private readonly string _opmSlot;
        private readonly int _opmCh;
        private readonly string _ermSlot;
        private readonly int _ermCh;

        public HardwareHelper(OTP12Driver otp, int channelIndex)
        {
            _otp = otp;
            _channelIndex = channelIndex;
            _txVoa = HardwareMap.GetTxVoa(channelIndex);
            _rxVoa = HardwareMap.GetRxVoa(channelIndex);
            _txSwitch = HardwareMap.GetTxSwitch(channelIndex);
            _rxSwitch = HardwareMap.GetRxSwitch(channelIndex);
            HardwareMap.GetOpm(channelIndex, out _opmSlot, out _opmCh);
            HardwareMap.GetErm(channelIndex, out _ermSlot, out _ermCh);
        }

        public int ChannelIndex { get { return _channelIndex; } }

        //=======================================================================
        // 发射端光开关（模块Tx → OPM/ERM）
        //=======================================================================

        /// <summary>切换发射端光开关到本模块（OPM/ERM可测到本模块光信号）</summary>
        public bool SetTxSwitch()
        {
            string cmd = string.Format(":ROUTe{0}:SCAN {1}", _txSwitch.InCh, _txSwitch.OutCh);
            string res = _otp.SendScpiToSlot(_txSwitch.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>查询发射端光开关当前状态</summary>
        public string GetTxSwitchState()
        {
            string cmd = string.Format(":ROUTe{0}:SCAN?", _txSwitch.InCh);
            return _otp.SendScpiToSlot(_txSwitch.Slot, cmd);
        }

        //=======================================================================
        // 接收端光开关（光源/衰减器 → 模块Rx）
        //=======================================================================

        /// <summary>切换接收端光开关到本模块（外部光信号送入本模块Rx口）</summary>
        public bool SetRxSwitch()
        {
            string cmd = string.Format(":ROUTe{0}:SCAN {1}", _rxSwitch.InCh, _rxSwitch.OutCh);
            string res = _otp.SendScpiToSlot(_rxSwitch.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        //=======================================================================
        // 发射端VOA衰减器
        //=======================================================================

        /// <summary>设置发射端VOA衰减值(dB)</summary>
        public bool SetTxVoaAtt(double attDb)
        {
            string cmd = string.Format(":INPut{0}:ATT {1} DB", _txVoa.Channel, attDb);
            string res = _otp.SendScpiToSlot(_txVoa.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>读取发射端VOA当前衰减值</summary>
        public string GetTxVoaAtt()
        {
            string cmd = string.Format(":INPut{0}:ATT?", _txVoa.Channel);
            return _otp.SendScpiToSlot(_txVoa.Slot, cmd);
        }

        /// <summary>设置发射端VOA输出开关 ON/OFF</summary>
        public bool SetTxVoaOutput(string state)
        {
            string cmd = string.Format(":OUTPut{0}:STATe {1}", _txVoa.Channel, state);
            string res = _otp.SendScpiToSlot(_txVoa.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>设置发射端VOA波长</summary>
        public bool SetTxVoaWave(int waveNm)
        {
            string cmd = string.Format(":INPut{0}:WAVelength {1} NM", _txVoa.Channel, waveNm);
            string res = _otp.SendScpiToSlot(_txVoa.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>读取发射端VOA输出光功率</summary>
        public string ReadTxVoaOutputPower()
        {
            string cmd = string.Format(":READ{0}:SCALar:POWer:DC?", _txVoa.Channel);
            return _otp.SendScpiToSlot(_txVoa.Slot, cmd);
        }

        //=======================================================================
        // 接收端VOA衰减器
        //=======================================================================

        /// <summary>设置接收端VOA衰减值(dB)</summary>
        public bool SetRxVoaAtt(double attDb)
        {
            string cmd = string.Format(":INPut{0}:ATT {1} DB", _rxVoa.Channel, attDb);
            string res = _otp.SendScpiToSlot(_rxVoa.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>读取接收端VOA当前衰减值</summary>
        public string GetRxVoaAtt()
        {
            string cmd = string.Format(":INPut{0}:ATT?", _rxVoa.Channel);
            return _otp.SendScpiToSlot(_rxVoa.Slot, cmd);
        }

        /// <summary>设置接收端VOA输出开关 ON/OFF</summary>
        public bool SetRxVoaOutput(string state)
        {
            string cmd = string.Format(":OUTPut{0}:STATe {1}", _rxVoa.Channel, state);
            string res = _otp.SendScpiToSlot(_rxVoa.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>设置接收端VOA波长</summary>
        public bool SetRxVoaWave(int waveNm)
        {
            string cmd = string.Format(":INPut{0}:WAVelength {1} NM", _rxVoa.Channel, waveNm);
            string res = _otp.SendScpiToSlot(_rxVoa.Slot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>读取接收端VOA输出光功率</summary>
        public string ReadRxVoaOutputPower()
        {
            string cmd = string.Format(":READ{0}:SCALar:POWer:DC?", _rxVoa.Channel);
            return _otp.SendScpiToSlot(_rxVoa.Slot, cmd);
        }

        //=======================================================================
        // OPM光功率计（SLOT-05，共享仪器）
        //=======================================================================

        /// <summary>读取本模块通道的光功率(dBm)</summary>
        public double ReadOpmPower()
        {
            string cmd = string.Format(":READ{0}:SCALar:POWer:DC?", _opmCh);
            string res = _otp.SendScpiToSlot(_opmSlot, cmd);
            double pwr;
            if (res != null && double.TryParse(res, out pwr))
                return pwr;
            return -999;
        }

        /// <summary>设置OPM波长</summary>
        public bool SetOpmWave(int waveNm)
        {
            string cmd = string.Format(":SENSe{0}:POWer:WAVelength {1} NM", _opmCh, waveNm);
            string res = _otp.SendScpiToSlot(_opmSlot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        /// <summary>设置OPM平均次数</summary>
        public bool SetOpmAverCount(int count)
        {
            string cmd = string.Format(":SENSe{0}:AVERage:COUNt {1}", _opmCh, count);
            string res = _otp.SendScpiToSlot(_opmSlot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }

        //=======================================================================
        // ERM消光比仪（SLOT-06，共享仪器）
        //=======================================================================

        /// <summary>读取本模块通道的消光比数据（返回 "power,er" 字符串）</summary>
        public string ReadErmData()
        {
            string cmd = string.Format(":READ{0}:ER?", _ermCh);
            return _otp.SendScpiToSlot(_ermSlot, cmd);
        }

        /// <summary>解析ERM返回值，返回(powerDbm, erDb)元组</summary>
        public bool ParseErmData(string ermResp, out double powerDbm, out double erDb)
        {
            powerDbm = -999;
            erDb = 0;
            if (string.IsNullOrEmpty(ermResp)) return false;
            string[] parts = ermResp.Split(',');
            if (parts.Length < 2) return false;
            return double.TryParse(parts[0].Trim(), out powerDbm)
                && double.TryParse(parts[1].Trim(), out erDb);
        }

        /// <summary>设置ERM速率</summary>
        public bool SetErmRate(string rate)
        {
            string cmd = string.Format(":SET{0}:RATe {1}", _ermCh, rate);
            string res = _otp.SendScpiToSlot(_ermSlot, cmd);
            return res != null && res.Contains("Command execute successfully");
        }
    }
}