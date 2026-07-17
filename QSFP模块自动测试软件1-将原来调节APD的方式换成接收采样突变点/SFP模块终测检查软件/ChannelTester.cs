using System;
using System.Threading;
using FibertopTest_Common;

namespace SFP模块终测检查软件
{
    /// <summary>
    /// ============================================================
    ///  单通道测试执行器（多线程核心类）—— .NET 3.5 / C# 3.0 兼容版
    /// ============================================================
    ///
    /// 【核心思路】
    /// 原来的程序只有1套"状态"（TestSet、TestResult全是static），
    /// 所以一次只能测1个模块。
    ///
    /// 现在创建4个ChannelTester，每个：
    ///   1. 有自己独立的 ChannelContext（测试参数和结果互不干扰）
    ///   2. 有自己独立的 I2C_Heater（通过slot号区分通道，TCP连接共享heater）
    ///   3. 有自己独立的 QSFP 测试对象
    ///   4. 有自己独立的 HardwareHelper（封装VOA/光开关/OPM/ERM硬件映射）
    ///   5. 运行在独立的 Thread（后台线程）上
    ///
    /// 【.NET 3.5 说明】
    /// 本项目使用 .NET Framework 3.5 / C# 3.0，没有 Task、async/await、
    /// CancellationTokenSource 这些高级特性。我们用 Thread + volatile bool 来实现：
    ///   - Task.Run()          → new Thread()
    ///   - CancellationToken   → volatile bool _stopRequested
    ///   - ct.ThrowIfCancellationRequested()  → CheckStop()
    ///
    /// 【共享资源保护】
    /// - I2C/加热台TCP：SFP_EVB_Heater内部已有lock，自动安全
    /// - OTP12 TCP：OTP12Driver.SendScpiToSlot() 原子操作，自动安全
    /// - DCA眼图仪/GPIB：使用 lock(ResourceLock.DcaLock) 保护
    /// - SQL数据库：使用 lock(ResourceLock.DbLock) 保护
    ///
    /// 【硬件映射】（参见HardwareMap.cs）
    /// 槽位号对应关系（根据用户提供的硬件信息）：
    ///   SLOT-07 VOA-02: ch1→模块1, ch2→模块2  (发射VOA)
    ///   SLOT-08 VOA-02: ch1→模块3, ch2→模块4  (发射VOA)
    ///   SLOT-09 VOA-02: ch1→模块1, ch2→模块2  (接收VOA)
    ///   SLOT-10 VOA-02: ch1→模块3, ch2→模块4  (接收VOA)
    ///   SLOT-11 SWD2-02: 模块1(in1→out2), 模块2(in3→out4)
    ///   SLOT-12 SWD2-02: 模块3(in1→out2), 模块4(in3→out4)
    ///   SLOT-05 OPM-04: ch1~4 对应模块1~4（共享仪器）
    ///   SLOT-06 ERM-04: ch1~4 对应模块1~4（共享仪器）
    /// </summary>
    public class ChannelTester
    {
        #region 字段

        private int _channelIndex;
        private ChannelContext _context;
        private I2C_Heater _i2c;
        private QSFP _qsfp;
        private HardwareHelper _hw;       // 线程安全的OTP12硬件操作帮助类
        private Thread _testThread;
        private volatile bool _stopRequested = false;

        /// <summary>通道索引（0~3）</summary>
        public int ChannelIndex { get { return _channelIndex; } }

        /// <summary>槽位号（1~4，对应加热台slot）</summary>
        public int SlotNumber { get { return _channelIndex + 1; } }

        /// <summary>此通道的上下文</summary>
        public ChannelContext Context { get { return _context; } }

        /// <summary>测试是否正在运行</summary>
        public bool IsRunning
        {
            get { return _testThread != null && _testThread.IsAlive; }
        }

        #endregion

        #region 静态工厂方法

        /// <summary>
        /// 初始化所有4个通道的Tester对象（仅创建数据结构，不连接硬件、不启动测试）
        /// 在Main_Form_Load中调用一次
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

        #endregion

        #region 构造函数

        public ChannelTester(int channelIndex)
        {
            _channelIndex = channelIndex;
            _context = ChannelManager.GetChannel(channelIndex);
            if (_context == null)
            {
                _context = new ChannelContext(channelIndex);
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化此通道的硬件接口
        /// 在程序启动/连接设备后调用一次
        /// </summary>
        public void Initialize()
        {
            _i2c = new I2C_Heater(GlobalVarFun.heater, SlotNumber);
            _qsfp = new QSFP();
            _qsfp.Init(_i2c);

            // 创建硬件操作帮助类（封装了本通道对应的VOA/光开关/OPM/ERM槽位和通道号）
            if (GlobalVarFun.otp12 != null && GlobalVarFun.otp12.IsConnected)
            {
                _hw = new HardwareHelper(GlobalVarFun.otp12, _channelIndex);
            }

            _context.AddLog(string.Format("通道{0} 初始化完成 (Heater槽位={1})",
                _channelIndex + 1, SlotNumber));
        }

        /// <summary>
        /// 启动测试（非阻塞，创建新线程并立即返回）
        /// </summary>
        public void StartTest()
        {
            if (IsRunning)
            {
                _context.AddLog("警告：测试已在运行中，忽略重复启动");
                return;
            }

            _stopRequested = false;

            _testThread = new Thread(new ThreadStart(RunTestLoop));
            _testThread.IsBackground = true;
            _testThread.Name = string.Format("TestThread-CH{0}", _channelIndex);
            _testThread.Start();

            _context.AddLog(string.Format("通道{0} 测试已启动", _channelIndex + 1));
        }

        /// <summary>
        /// 请求停止测试（设置停止标志，线程会在检查点自行退出）
        /// </summary>
        public void StopTest()
        {
            if (IsRunning && !_stopRequested)
            {
                _stopRequested = true;
                _context.AddLog(string.Format("通道{0} 正在停止...", _channelIndex + 1));
            }
        }

        /// <summary>
        /// 等待测试线程结束（无限等待）
        /// </summary>
        public bool WaitForCompletion()
        {
            return WaitForCompletion(-1);
        }

        /// <summary>
        /// 等待测试线程结束（带超时）
        /// </summary>
        public bool WaitForCompletion(int timeoutMs)
        {
            if (_testThread != null && _testThread.IsAlive)
            {
                if (timeoutMs > 0)
                    return _testThread.Join(timeoutMs);
                else
                {
                    _testThread.Join();
                    return true;
                }
            }
            return true;
        }

        #endregion

        #region 测试主循环

        /// <summary>
        /// 检查是否收到停止请求
        /// </summary>
        private void CheckStop()
        {
            if (_stopRequested)
            {
                throw new ThreadInterruptedException("测试被用户停止");
            }
        }

        /// <summary>
        /// 测试主循环（在后台线程运行）
        /// </summary>
        private void RunTestLoop()
        {
            try
            {
                _context.IsTesting = true;
                _context.TestPassed = false;
                _context.Reset();
                _context.UpdateStatus("正在初始化...");

                // ========== 阶段1：上电初始化 ==========
                if (!Stage_Init()) return;

                // ========== 阶段2：发射调试 ==========
                if (GlobalVarFun.tx_test)
                {
                    _context.UpdateStatus("发射调试中...");
                    if (!Stage_TxDebug()) return;
                }

                // ========== 阶段3：接收调试 ==========
                if (GlobalVarFun.rx_ddm_test || GlobalVarFun.rx_los_test)
                {
                    _context.UpdateStatus("接收调试中...");
                    if (!Stage_RxDebug()) return;
                }

                // ========== 阶段4：写入校准数据 ==========
                _context.UpdateStatus("写入校准数据...");
                if (!Stage_WriteCalibration()) return;

                // ========== 阶段5：终测检查 ==========
                _context.UpdateStatus("终测检查...");
                if (!Stage_FinalCheck()) return;

                // ========== 阶段6：保存记录 ==========
                _context.UpdateStatus("保存记录...");
                Stage_SaveRecord();

                // ========== 测试完成 ==========
                _context.TestPassed = true;
                _context.UpdateStatus("测试通过");
                _context.AddLog(string.Format("通道{0} 测试完成", _channelIndex + 1));
            }
            catch (ThreadInterruptedException)
            {
                _context.UpdateStatus("测试已停止");
                _context.AddLog(string.Format("通道{0} 测试被用户停止", _channelIndex + 1));
            }
            catch (ThreadAbortException)
            {
                _context.UpdateStatus("测试已中止");
            }
            catch (Exception ex)
            {
                _context.TestPassed = false;
                _context.ErrorMessage = ex.Message;
                _context.UpdateStatus("测试失败");
                _context.AddLog(string.Format("通道{0} 测试异常: {1}", _channelIndex + 1, ex.Message));
            }
            finally
            {
                _context.IsTesting = false;
                _context.NotifyDataUpdated();
            }
        }

        #endregion

        #region 测试阶段实现（模板/骨架）

        /// <summary>
        /// 阶段1：上电初始化
        /// I2C通信检查、读取模块信息、芯片方案识别、上电延时
        /// 各通道独立，可4路并行
        /// </summary>
        private bool Stage_Init()
        {
            _context.AddLog("--- 初始化阶段 ---");

            // 1.1 给模块上电
            _i2c.setModuleDis(false);
            Thread.Sleep(500);
            CheckStop();

            // 1.2 检查I2C通信
            byte[] testBuf = new byte[4];
            uint bytesRead = _i2c.TWI_ReadPage(0xA0, 0x00, testBuf, 4);
            if (bytesRead != 4)
            {
                LogAndFail("I2C通信失败，请检查模块是否插好");
                return false;
            }

            // 1.3 读取模块信息
            if (!_qsfp.GetFlashInfo())
            {
                LogAndFail("读取模块Flash信息失败");
                return false;
            }
            CopyResultsFromStatic();
            CheckStop();

            // 1.4 识别芯片方案
            if (!_qsfp.CheckTestTypeInfo())
            {
                LogAndFail("模块方案识别失败");
                return false;
            }
            CopyResultsFromStatic();

            // 1.5 打开发射
            _i2c.setModuleDis(false);
            _qsfp.SoftTxDis(false);
            Thread.Sleep(2000);
            CheckStop();

            // 1.6 初始化VOA和光开关（如果OTP12已连接）
            if (_hw != null)
            {
                try
                {
                    // 打开发射端VOA输出，初始衰减设为0dB
                    _hw.SetTxVoaWave(1310);  // 默认波长1310nm，根据实际修改
                    _hw.SetTxVoaAtt(0);
                    _hw.SetTxVoaOutput("ON");

                    // 打开接收端VOA输出
                    _hw.SetRxVoaWave(1310);
                    _hw.SetRxVoaAtt(0);
                    _hw.SetRxVoaOutput("ON");

                    _context.AddLog("VOA/光开关初始化完成");
                }
                catch (Exception ex)
                {
                    _context.AddLog("VOA初始化警告: " + ex.Message);
                    // VOA初始化失败不阻断测试流程
                }
            }

            _context.AddLog("初始化完成");
            return true;
        }

        /// <summary>
        /// 阶段2：发射调试
        /// 设置APC/MOD → 等待稳定 → 切光开关 → OPM读功率/ERM读ER → DCA测眼图
        ///
        /// 【线程安全说明】
        /// - I2C写寄存器：SFP_EVB_Heater内部lock保护，4通道自动串行化，安全
        /// - OPM/ERM读数据：SendScpiToSlot原子操作（lock内"切槽位→发命令→收响应"），安全
        /// - DCA眼图仪：全局共享（1台），必须 lock(ResourceLock.DcaLock)
        ///
        /// 【硬件调用示例】（后续需从Main_Form.timer1搬APC/MOD调试逻辑到这里）：
        ///   // 设APC寄存器值
        ///   _qsfp.SetTxApcRegister(targetValue);  // I2C，安全
        ///   Thread.Sleep(500);                     // 等稳定
        ///
        ///   // 切光开关 + 读OPM功率
        ///   SwitchTxToThisChannel();               // 原子操作，安全
        ///   double txPwr = _hw.ReadOpmPower();     // 原子操作，安全
        ///
        ///   // 读ERM消光比
        ///   string erResp = _hw.ReadErmData();
        ///   double erPwr, erVal;
        ///   _hw.ParseErmData(erResp, out erPwr, out erVal);
        ///
        ///   // DCA眼图仪测量（共享设备）
        ///   lock(ResourceLock.DcaLock) {
        ///       SwitchTxToThisChannel();
        ///       // ... DCA API 调用 ...
        ///   }
        /// </summary>
        private bool Stage_TxDebug()
        {
            _context.AddLog("--- 发射调试阶段 ---");

            if (_hw != null)
            {
                // 演示：切光开关 → 读发射功率
                SwitchTxToThisChannel();
                double txPwr = _hw.ReadOpmPower();
                _context.txPowerDDMSingle = (float)txPwr;
                _context.AddLog(string.Format("OPM读发射功率: {0:F2} dBm", txPwr));

                // 演示：读ERM消光比
                string erResp = _hw.ReadErmData();
                double erPwr, erVal;
                if (_hw.ParseErmData(erResp, out erPwr, out erVal))
                {
                    _context.AddLog(string.Format("ERM: 功率={0:F2}dBm, 消光比={1:F2}dB", erPwr, erVal));
                }
            }

            // TODO: 将Main_Form中timer1的APC/MOD调试逻辑搬到这里
            // 核心循环：
            //   1. 设APC寄存器值（_qsfp方法，I2C，可并行）
            //   2. Thread.Sleep(xxx) 等待稳定
            //   3. SwitchTxToThisChannel() 切光开关（原子操作）
            //   4. _hw.ReadOpmPower() 读功率（原子操作）
            //   5. 判断是否达标，不达标则调整寄存器值回到步骤1
            //   6. MOD（消光比）调试同理，使用 _hw.ReadErmData() 读ER
            //   7. DCA眼图仪部分用 lock(ResourceLock.DcaLock) 包围

            CheckStop();
            _context.AddLog("发射调试完成（模板，需填充实际APC/MOD调试逻辑）");
            return true;
        }

        /// <summary>
        /// 阶段3：接收调试
        /// 设LOS/APD → 切接收光开关 → 调VOA衰减 → 读ADC/告警
        ///
        /// 【硬件调用示例】：
        ///   // 切换接收端光开关（光源→本模块Rx）
        ///   SwitchRxToThisChannel();
        ///
        ///   // 设置接收端VOA衰减
        ///   _hw.SetRxVoaAtt(targetAttDb);
        ///   Thread.Sleep(200);
        ///
        ///   // 在发射端OPM读回功率确认衰减值
        ///   SwitchTxToThisChannel();
        ///   double rxPwr = _hw.ReadOpmPower();
        ///
        ///   // 读模块Rx LOS状态
        ///   bool los = _i2c.HardWare_LOS_Get();
        /// </summary>
        private bool Stage_RxDebug()
        {
            _context.AddLog("--- 接收调试阶段 ---");

            if (_hw != null)
            {
                // 演示：切换接收光开关
                SwitchRxToThisChannel();
                _context.AddLog("接收端光开关已切换到本通道");
            }

            // TODO: 实现接收调试逻辑
            // 3.1 检查Rx无光状态（关VOA输出或设最大衰减 → 读LOS引脚）
            // 3.2 LOS告警点调试（逐步增加衰减 → 找到LOS触发点）
            // 3.3 灵敏度点调试（设特定衰减 → 读误码/RSSI）
            // 3.4 APD偏压调试（如果有APD，用"接收采样突变点"方法）

            CheckStop();
            _context.AddLog("接收调试完成（模板，需填充实际LOS/APD调试逻辑）");
            return true;
        }

        /// <summary>
        /// 阶段4：写入校准数据
        /// 纯I2C写操作，各通道完全独立，可4路并行
        /// （SFP_EVB_Heater的内部lock保证I2C命令不交错）
        /// </summary>
        private bool Stage_WriteCalibration()
        {
            _context.AddLog("--- 写入校准数据 ---");

            bool txOk = _qsfp.WriteTxCalData();
            bool rxOk = _qsfp.WriteRxCalData();
            CopyResultsFromStatic();

            if (!txOk || !rxOk)
            {
                LogAndFail("写入校准数据失败");
                return false;
            }

            CheckStop();
            _context.AddLog("校准数据写入完成");
            return true;
        }

        /// <summary>
        /// 阶段5：终测检查
        /// DDM检查、阈值检查、眼图验证等
        /// DCA测量部分需要lock
        /// </summary>
        private bool Stage_FinalCheck()
        {
            _context.AddLog("--- 终测检查阶段 ---");

            _qsfp.GetDDMAnalogValues();
            CopyResultsFromStatic();

            _qsfp.GetDDMThresholds();
            CopyResultsFromStatic();

            // DCA眼图仪是全局共享设备（只有1台），必须加锁
            lock (ResourceLock.DcaLock)
            {
                SwitchTxToThisChannel();
                // TODO: 调用DCA API读眼图参数 → 判pass/fail
                Thread.Sleep(100);
            }
            CheckStop();

            // OPM/ERM最终验证（原子操作，安全）
            if (_hw != null)
            {
                SwitchTxToThisChannel();
                double finalPwr = _hw.ReadOpmPower();
                _context.AddLog(string.Format("终测发射功率: {0:F2} dBm", finalPwr));
            }

            _context.AddLog("终测检查完成（模板，需填充实际验证逻辑）");
            return true;
        }

        /// <summary>
        /// 阶段6：保存测试记录到SQL
        /// SQL连接是共享资源，必须加锁
        /// </summary>
        private void Stage_SaveRecord()
        {
            lock (ResourceLock.DbLock)
            {
                // TODO: 将Context中的测试结果写入SQL数据库
                // 参考原来的SQL保存逻辑，把TestResult.xxx替换成_context.xxx
                _context.AddLog("记录已保存（模板，需填充实际SQL写入逻辑）");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 切换发射端光开关到本通道
        /// 模块Tx → 光开关 → OPM/ERM仪器
        /// 内部通过SendScpiToSlot原子操作，天然线程安全，无需额外lock
        /// </summary>
        private void SwitchTxToThisChannel()
        {
            if (_hw != null)
            {
                _hw.SetTxSwitch();
                Thread.Sleep(200); // 等待光开关切换稳定（机械开关约10~20ms，留足余量）
            }
        }

        /// <summary>
        /// 切换接收端光开关到本通道
        /// 光源/VOA → 光开关 → 模块Rx口
        /// </summary>
        private void SwitchRxToThisChannel()
        {
            if (_hw != null)
            {
                _hw.SetRxSwitch();
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 记录错误日志并设置失败状态
        /// </summary>
        private void LogAndFail(string message)
        {
            _context.ErrorMessage = message;
            _context.AddLog("错误: " + message);
            _context.TestPassed = false;
        }

        /// <summary>
        /// 从静态TestResult拷贝数据到ChannelContext
        ///
        /// 【为什么需要这个？】
        /// 目前QSFP类的方法把结果写到TestResult.xxx（静态变量）。
        /// 多线程下，4个通道如果同时写TestResult会互相覆盖。
        ///
        /// 【临时方案】
        /// I2C操作通过SFP_EVB_Heater内部lock保证同一时刻只有一个slot
        /// 在收发命令，所以QSFP方法执行期间TestResult的值是本通道的。
        /// 方法返回后立即拷贝，数据安全。
        ///
        /// 【长远建议】逐步把QSFP类改为接受ChannelContext参数，直接读写Context。
        /// </summary>
        private void CopyResultsFromStatic()
        {
            // 模块信息
            _context.sn = TestResult.sn;
            _context.pn = TestResult.pn;
            _context.vn = TestResult.vn;
            _context.date = TestResult.date;
            _context.fibertop_sn = TestResult.fibertop_sn;
            _context.fibertop_pn = TestResult.fibertop_pn;
            _context.tosa_sn = TestResult.tosa_sn;
            _context.rosa_sn = TestResult.rosa_sn;
            _context.chipType = TestResult.chipType;
            _context.bitRate = TestResult.bitRate;
            _context.softType = TestResult.softType;
            _context.softVer = TestResult.softVer;
            _context.chipIsOK = TestResult.chipIsOK;
            _context.wpIsEn = TestResult.wpIsEn;
            _context.moduleIsSR = TestResult.moduleIsSR;

            // DDM值（模块整体）
            _context.tempDDM = TestResult.tempDDM;
            _context.vccDDM = TestResult.vccDDM;
            _context.txBiasDDMSingle = TestResult.txBiasDDM;
            _context.txPowerDDMSingle = TestResult.txPowerDDM;
            _context.rxPowerDDM = TestResult.rxPowerDDM;

            // 寄存器最终值
            _context.txapcVal = TestResult.txapcVal;
            _context.txmodVal = TestResult.txmodVal;
            _context.rxlosVal = TestResult.rxlosVal;
            _context.rxapdVal = TestResult.rxapdVal;

            // 4通道DDM数组
            for (int i = 0; i < 4; i++)
            {
                if (TestResult.txBiasDDMbuf != null && i < TestResult.txBiasDDMbuf.Length)
                    _context.txBiasDDM[i] = TestResult.txBiasDDMbuf[i];
                if (TestResult.txPowerDDMbuf != null && i < TestResult.txPowerDDMbuf.Length)
                    _context.txPowerDDM[i] = TestResult.txPowerDDMbuf[i];
                if (TestResult.rxPowerDDMbuf != null && i < TestResult.rxPowerDDMbuf.Length)
                    _context.rxPowerDDMSingle[i] = TestResult.rxPowerDDMbuf[i];
            }
        }

        #endregion
    }
}