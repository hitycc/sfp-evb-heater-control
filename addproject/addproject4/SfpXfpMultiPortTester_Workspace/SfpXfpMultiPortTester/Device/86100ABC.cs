using System;
using Agilent.AgilentInfiniiumDCA.Interop;

namespace FibertopTest_Common
{
    /// <summary>
    /// Agilent 86100A/B/C/D DCA 眼图仪控制器
    /// </summary>
    public class Agilent86100ABC : IDisposable
    {
        private readonly object _lock = new object(); 
        private AgilentInfiniiumDCA _scope;
        private bool _isConnected;
        private bool _disposed;

        /// <summary>当前是否已连接</summary>
        public bool IsConnected
        {
            get { lock (_lock) { return _isConnected; } }
        }

        public int TimeoutMs { get; set; } = 10000;

        /// <summary>
        /// 连接到 DCA 仪器
        /// </summary>
        /// <param name="address">VISA 地址，如 "GPIB0::07::INSTR"</param>
        public void Connect(string address)
        {
            lock (_lock)
            {
                if (_isConnected)
                    throw new InvalidOperationException("DCA 已处于连接状态，请先断开再重连。");

                try
                {
                    _scope = new AgilentInfiniiumDCAClass();

                    // 如果之前有残留会话，先尝试关闭
                    try { if (_scope.Initialized) _scope.Close(); } catch { }

                    _scope.Initialize(address, false, false, "");
                    _scope.System.TimeoutMilliseconds = TimeoutMs;
                    _scope.System.IO.WriteString(":CHANnel1:DISPlay ON", true);
                    _scope.System.IO.WriteString("*CLS", true);
                    _scope.System.EnableLocalControls();

                    _isConnected = true;
                }
                catch (Exception ex)
                {
                    SafeCleanupInternal();
                    throw new Exception($"DCA 连接失败 [{address}]: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 断开与 DCA 仪器的连接
        /// </summary>
        public void Disconnect()
        {
            lock (_lock)
            {
                SafeCleanupInternal();
            }
        }

        /// <summary>
        /// 获取平均光功率 (dBm)
        /// </summary>
        /// <returns>光功率值(dBm)，读取失败时返回 null</returns>
        public float? GetOpticalPower()
        {
            lock (_lock)
            {
                EnsureConnectedInternal();
                try
                {
                    System.Threading.Thread.Sleep(100);

                    _scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeOscilloscope;
                    _scope.System.IO.WriteString(":CDISPLAY", true);
                    _scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    _scope.System.IO.WriteString(":MEASURE:APOWER? DECibel", true);

                    string raw = _scope.System.IO.ReadString().Trim();
                    _scope.System.EnableLocalControls();

                    if (float.TryParse(raw, out float power))
                        return power;

                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取消光比 (dB)
        /// </summary>
        /// <param name="erValue">输出的消光比值(dB)</param>
        /// <param name="autoScale">是否先执行自动缩放</param>
        /// <returns>读取是否成功</returns>
        public bool GetExtinctionRatio(out float erValue, bool autoScale = false)
        {
            lock (_lock)
            {
                erValue = 0f;
                EnsureConnectedInternal();

                try
                {
                    _scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    _scope.System.IO.WriteString(":CDISPLAY", true);
                    _scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    _scope.System.IO.WriteString(":RUN", true);

                    if (autoScale)
                        _scope.System.IO.WriteString(":AUToscale", true);

                    _scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel", true);

                    string raw = _scope.System.IO.ReadString().Trim();
                    _scope.System.EnableLocalControls();

                    if (float.TryParse(raw, out erValue))
                        return true;

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }



        /// <summary>
        /// 获取眼图交叉点 (Crossing, %)
        /// </summary>
        /// <returns>交叉点百分比值，读取失败时返回 null</returns>
        public float? GetCrossing()
        {
            lock (_lock)
            {
                if (!EnsureConnectedInternal()) return null;
                try
                {
                    _scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    _scope.System.IO.WriteString(":CDISPLAY", true);
                    _scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    _scope.System.IO.WriteString(":MEASURE:CGRADE:CROSsing?", true);

                    string raw = _scope.System.IO.ReadString().Trim();
                    _scope.System.EnableLocalControls();

                    return float.TryParse(raw, out float val) ? val : (float?)null;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// 获取 RMS 抖动 (ps)
        /// </summary>
        /// <returns>RMS 抖动值(ps)，读取失败时返回 null</returns>
        public float? GetJitterRmsPs()
        {
            lock (_lock)
            {
                if (!EnsureConnectedInternal()) return null;
                try
                {
                    _scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    _scope.System.IO.WriteString(":CDISPLAY", true);
                    _scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    _scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? RMS", true);

                    string raw = _scope.System.IO.ReadString().Trim();
                    _scope.System.EnableLocalControls();

                    // 仪器返回单位为秒，转换为 ps
                    return float.TryParse(raw, out float val) ? val * 1e12f : (float?)null;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// 获取 Peak-to-Peak 抖动 (ps)
        /// </summary>
        /// <returns>PP 抖动值(ps)，读取失败时返回 null</returns>
        public float? GetJitterPpPs()
        {
            lock (_lock)
            {
                if (!EnsureConnectedInternal()) return null;
                try
                {
                    _scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    _scope.System.IO.WriteString(":CDISPLAY", true);
                    _scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    _scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? PP", true);

                    string raw = _scope.System.IO.ReadString().Trim();
                    _scope.System.EnableLocalControls();

                    // 仪器返回单位为秒，转换为 ps
                    return float.TryParse(raw, out float val) ? val * 1e12f : (float?)null;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// 获取总抖动 Tj (ps)
        /// </summary>
        /// <param name="moduleType">模块类型，如 "SFP+", "XFP" 等，用于选择计算系数</param>
        /// <returns>总抖动值(ps)，读取失败或依赖参数缺失时返回 null</returns>
        public float? GetJitterTotalPs(string moduleType)
        {
            lock (_lock)
            {
                if (!EnsureConnectedInternal()) return null;
                try
                {
                    // Tj 由 RMS 和 PP 计算得出，需在同一锁内连续读取保证数据一致性
                    _scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    _scope.System.IO.WriteString(":CDISPLAY", true);
                    _scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);

                    _scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? RMS", true);
                    string rawRms = _scope.System.IO.ReadString().Trim();

                    _scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? PP", true);
                    string rawPp = _scope.System.IO.ReadString().Trim();

                    _scope.System.EnableLocalControls();

                    if (!float.TryParse(rawRms, out float rmsSec) || !float.TryParse(rawPp, out float ppSec))
                        return null;

                    float rmsPs = rmsSec * 1e12f;
                    float ppPs = ppSec * 1e12f;

                    // SFP+/XFP 使用系数 1，其他模块使用系数 14
                    bool isHighSpeed = string.Equals(moduleType, "SFP+", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(moduleType, "XFP", StringComparison.OrdinalIgnoreCase);
                    float multiplier = isHighSpeed ? 1f : 14f;

                    return rmsPs * multiplier + ppPs;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// 获取有效信噪比 ESN
        /// </summary>
        /// <returns>ESN 值，读取失败时返回 null</returns>
        public float? GetEsn()
        {
            lock (_lock)
            {
                if (!EnsureConnectedInternal()) return null;
                try
                {
                    _scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    _scope.System.IO.WriteString(":CDISPLAY", true);
                    _scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    _scope.System.IO.WriteString(":MEASURE:CGRADE:ESN?", true);

                    string raw = _scope.System.IO.ReadString().Trim();
                    _scope.System.EnableLocalControls();

                    return float.TryParse(raw, out float val) ? val : (float?)null;
                }
                catch { return null; }
            }
        }


        /// <summary>内部连接检查，返回是否已连接（不抛异常）</summary>
        private bool EnsureConnectedInternal()
        {
            return _isConnected && _scope != null;
        }

        private void SafeCleanupInternal()
        {
            try
            {
                if (_scope != null)
                {
                    try { _scope.System.EnableLocalControls(); } catch { }
                    try { if (_scope.Initialized) _scope.Close(); } catch { }
                }
            }
            finally
            {
                _scope = null;
                _isConnected = false;
            }
        }

  


        public void Dispose()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    SafeCleanupInternal();
                    _disposed = true;
                }
            }
            GC.SuppressFinalize(this);
        }

        ~Agilent86100ABC() => Dispose();
    }
}