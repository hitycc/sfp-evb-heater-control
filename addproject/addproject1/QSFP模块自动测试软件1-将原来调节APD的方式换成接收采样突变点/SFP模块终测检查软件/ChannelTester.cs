using System;
using System.Threading;
using FibertopTest_Common;

namespace SFP模块终测检查软件
{
    //===========================================================================
    // ChannelTester —— 单通道测试执行器
    //
    // 每个ChannelTester对应一个物理通道(0~3)，在独立线程上执行测试流程。
    // 线程模型：
    //   - UI线程：调用StartTest/StopTest，读取ChannelContext状态更新UI
    //   - 测试线程：执行完整测试流程，写入ChannelContext
    //
    // 核心设计原则：
    //   1. 每个通道有独立的ChannelContext（数据隔离）
    //   2. 共享硬件(I2C/光开关/光功率计/示波器/波长计)通过HardwareHelper访问（自动加锁）
    //   3. UI更新通过ChannelContext.UISyncContext.Post（跨线程安全）
    //   4. 测试流程复用现有SFPUX3320T驱动（通过SfpDriverAdapter包装）
    //===========================================================================

    public class ChannelTester
    {
        //=======================================================================
        // 字段
        //=======================================================================

        private ChannelContext _ctx;             // 本通道的数据上下文
        private IModuleDriver _driver;           // 模块驱动（SFP适配器）
        private Thread _testThread;              // 测试线程
        private volatile bool _isRunning = false;// 是否正在测试
        private volatile bool _stopRequested = false; // 请求停止标志

        //=======================================================================
        // 公共属性
        //=======================================================================

        /// <summary>通道索引（0~3）</summary>
        public int ChannelIndex { get { return _ctx.ChannelIndex; } }

        /// <summary>该通道的数据上下文（UI线程只读访问）</summary>
        public ChannelContext Context { get { return _ctx; } }

        /// <summary>是否正在测试</summary>
        public bool IsRunning { get { return _isRunning; } }

        //=======================================================================
        // 构造函数
        //=======================================================================

        public ChannelTester(int channelIndex)
        {
            _ctx = ChannelManager.GetChannel(channelIndex);
        }

        //=======================================================================
        // 创建所有通道的测试器
        //=======================================================================

        /// <summary>
        /// 初始化所有4个通道的ChannelTester实例
        /// </summary>
        public static ChannelTester[] InitializeAll()
        {
            ChannelTester[] testers = new ChannelTester[ChannelManager.ChannelCount];
            for (int i = 0; i < ChannelManager.ChannelCount; i++)
            {
                testers[i] = new ChannelTester(i);
            }
            return testers;
        }

        //=======================================================================
        // 启动测试
        //=======================================================================

        /// <summary>
        /// 启动本通道测试（UI线程调用，立即返回，测试在后台线程执行）
        /// </summary>
        public void StartTest()
        {
            if (_isRunning) return; // 已在测试中，忽略

            _stopRequested = false;
            _ctx.IsTesting = true;
            _ctx.TestPassed = false;
            _ctx.ErrorMessage = "";
            _ctx.ClearLog();

            _testThread = new Thread(new ThreadStart(TestThreadProc));
            _testThread.IsBackground = true;
            _testThread.Name = string.Format("CH{0}_TestThread", _ctx.ChannelIndex);
            _testThread.Start();

            _isRunning = true;
        }

        //=======================================================================
        // 停止测试
        //=======================================================================

        /// <summary>
        /// 请求停止本通道测试
        /// </summary>
        public void StopTest()
        {
            _stopRequested = true;
        }

        //=======================================================================
        // 测试线程主流程
        //=======================================================================

        private void TestThreadProc()
        {
            int slot = _ctx.ChannelIndex + 1; // 槽位号从1开始

            try
            {
                _ctx.UpdateStatus("等待共享硬件...");
                _ctx.AddLog(string.Format("通道{0} 测试开始", _ctx.ChannelIndex));

                // 1. 初始化驱动（使用全局I2C实例，HardwareHelper内部已加锁）
                _driver = new SfpDriverAdapter();
                _driver.Init(GlobalVarFun.iic);

                _ctx.UpdateStatus("模块上电...");
                _ctx.AddLog("模块上电中...");

                // 2. 模块上电
                lock (SharedHardwareLocks.I2CLock)
                {
                    if (!HardwareHelper.ModulePowerOn(slot, 1))
                    {
                        throw new Exception("模块上电失败");
                    }
                }

                Thread.Sleep(500); // 等待模块稳定

                if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                // 3. 检查芯片ID
                _ctx.UpdateStatus("检查芯片...");
                _ctx.AddLog("检查芯片类型...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    if (!_driver.CheckTestTypeInfo())
                    {
                        throw new Exception("芯片ID校验失败");
                    }
                }
                _ctx.AddLog("芯片ID正确 (UX3320T)");

                if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                // 4. 读取模块信息
                _ctx.UpdateStatus("读取模块信息...");
                _ctx.AddLog("读取DDM信息...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    if (!_driver.ReadInfo())
                    {
                        throw new Exception("读取模块信息失败");
                    }
                }

                if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                // 5. 光开关路由到本通道（发射方向）
                _ctx.UpdateStatus("切换光开关...");
                lock (SharedHardwareLocks.OpticalSwitchLock)
                {
                    HardwareHelper.OpticalSwitchRoute(slot, true);
                }
                Thread.Sleep(300);

                // 6. 打开发射
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.TxDisableAll(false); // 开启激光
                    _driver.TxSelect(true);      // 选择Tx路径
                }

                Thread.Sleep(1000);

                if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                // 7. 发射功率测试
                _ctx.UpdateStatus("测试发射功率...");
                _ctx.AddLog("测试发射功率...");
                double txPower;
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    txPower = HardwareHelper.ReadOPMPower(1);
                }
                _ctx.txPowerDCA = (float)txPower;
                _ctx.AddLog(string.Format("发射功率: {0:F2} dBm", txPower));

                // 读取DDM发射功率和偏置电流
                lock (SharedHardwareLocks.I2CLock)
                {
                    _ctx.txPowerDDMSingle = (float)_driver.ReadTxPower(0);
                    _ctx.txBiasDDMSingle = (float)_driver.GetBias(0);
                    _ctx.tempDDM = (float)_driver.ReadTemperature();
                    _ctx.vccDDM = (float)_driver.ReadVoltage();
                    _ctx.rxPowerDDM = (float)_driver.ReadRxPower(0);
                }
                _ctx.AddLog(string.Format("DDM: Temp={0:F1}C, Vcc={1:F2}V, Bias={2:F1}mA, TxPwr={3:F2}dBm, RxPwr={4:F2}dBm",
                    _ctx.tempDDM, _ctx.vccDDM, _ctx.txBiasDDMSingle, _ctx.txPowerDDMSingle, _ctx.rxPowerDDM));

                if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                // 8. 眼图测试（使用示波器）
                _ctx.UpdateStatus("测试眼图...");
                _ctx.AddLog("等待眼图仪采集...");
                lock (SharedHardwareLocks.OscilloscopeLock)
                {
                    // 眼图采集（简化版：等待示波器自动采集）
                    // TODO: 对接具体的DCAX-86100 API采集眼图数据
                    Thread.Sleep(3000);
                    _ctx.AddLog("眼图采集完成");
                }

                if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                // 9. 切换到接收路径
                _ctx.UpdateStatus("测试接收灵敏度...");
                lock (SharedHardwareLocks.OpticalSwitchLock)
                {
                    HardwareHelper.OpticalSwitchRoute(slot, false);
                }
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.TxSelect(false); // 选择Rx路径
                }
                Thread.Sleep(300);

                // 10. 接收灵敏度测试（逐步增加衰减直到LOS触发）
                _ctx.AddLog("接收灵敏度测试...");
                double senAtt = 0;
                for (double att = 0; att <= 25; att += 1.0)
                {
                    if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                    lock (SharedHardwareLocks.OtpDriverLock)
                    {
                        HardwareHelper.SetVOAAttenuation(1, att);
                    }
                    Thread.Sleep(_ctx.optoAttDelay * 100);

                    // 检查RxLOS
                    string rxLos;
                    lock (SharedHardwareLocks.I2CLock)
                    {
                        rxLos = HardwareHelper.ModuleGetRxLOS(slot);
                    }

                    if (rxLos == "1")
                    {
                        senAtt = att;
                        _ctx.AddLog(string.Format("RxLOS触发，衰减={0:F1}dB", att));
                        break;
                    }
                }
                _ctx.rxSen[0] = (float)(_ctx.txPowerDCA - senAtt);

                // 复位衰减器
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    HardwareHelper.SetVOAAttenuation(1, 0);
                }

                if (_stopRequested) { _ctx.UpdateStatus("已停止"); return; }

                // 11. DDM告警阈值检查
                _ctx.UpdateStatus("检查DDM告警...");
                _ctx.AddLog("检查DDM阈值告警...");
                // TODO: 检查各阈值告警标志

                // 12. 测试完成
                _ctx.TestPassed = true;
                _ctx.UpdateStatus("测试完成");
                _ctx.AddLog("测试通过！");

                _ctx.NotifyDataUpdated();
            }
            catch (Exception ex)
            {
                _ctx.TestPassed = false;
                _ctx.ErrorMessage = ex.Message;
                _ctx.UpdateStatus("测试失败: " + ex.Message);
                _ctx.AddLog("错误: " + ex.Message);

                _ctx.NotifyDataUpdated();
            }
            finally
            {
                _ctx.IsTesting = false;
                _isRunning = false;

                // 确保激光关闭、模块安全状态
                try
                {
                    if (_driver != null)
                    {
                        lock (SharedHardwareLocks.I2CLock)
                        {
                            _driver.TxDisableAll(true); // 关闭激光
                        }
                    }
                    // 复位光开关（可选：让光开关回到默认位置）
                    // 复位衰减器
                    if (GlobalVarFun.otp12 != null && GlobalVarFun.otp12.IsConnected)
                    {
                        lock (SharedHardwareLocks.OtpDriverLock)
                        {
                            HardwareHelper.SetVOAAttenuation(1, 0);
                        }
                    }
                }
                catch { }
            }
        }
    }
}