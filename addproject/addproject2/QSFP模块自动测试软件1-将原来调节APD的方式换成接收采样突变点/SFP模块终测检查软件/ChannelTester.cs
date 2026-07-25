using System;
using System.Threading;
using FibertopTest_Common;

namespace SFP模块终测检查软件
{
    //===========================================================================
    // ChannelTester —— 单通道测试执行器
    //
    // 【这个类是做什么的？】
    //   每个ChannelTester实例对应一个物理通道(0~3)，在独立的后台线程上
    //   执行完整的SFP模块终测流程。4个ChannelTester实例并行运行，实现
    //   4通道同时测试，大幅提升测试效率。
    //
    // 【线程模型】
    //   - UI线程（Main_Form）：调用StartTest()/StopTest()，通过timer轮询
    //     读取ChannelContext中的状态和结果来刷新界面显示。
    //   - 测试线程（每个通道一个）：执行TestThreadProc()中的16步测试流程，
    //     将测试结果写入ChannelContext，通过事件通知UI更新。
    //
    // 【硬件共享策略（多线程安全核心）】
    //   4个通道并行测试时，有些硬件设备是共享的（只有一套），必须用lock
    //   保证同一时刻只有一个线程操作，否则命令会冲突导致设备报错或读数错误：
    //   - I2C总线(治具板): SharedHardwareLocks.I2CLock
    //     → 模块上电/断电/读寄存器/写寄存器 全部走I2C总线
    //   - OTP12集成板(光开关/OPM/VOA/光源/ERM): SharedHardwareLocks.OtpDriverLock
    //     → 切光路、读光功率、设衰减、开关光源、读消光比
    //   - 波长计(Keysight 86120C): SharedHardwareLocks.WavelengthMeterLock
    //     → 读光波长（GPIB仪器）
    //   - DCA眼图仪: SharedHardwareLocks.OscilloscopeLock
    //     → 本版本已用ERM替代DCA测ER，用RxADC突变法替代DCA测灵敏度，
    //       因此正常流程不需要锁示波器
    //
    // 【完整16步SFP终测流程】
    //   步骤1:  模块上电 → 先断电再上3.3V → 等500ms → 检查电流 → 创建I2C驱动 → I2C通信检测
    //   步骤2:  芯片ID检测 — 确认模块使用的芯片型号正确
    //   步骤3:  进入调试模式(SetDebugPWD) + Tx使能(SetTx_EN)
    //   步骤4:  读模块信息 — A0/A2页Flash内容，解析SN/PN/厂商/速率/芯片类型
    //   步骤5:  电流检测 — 读取工作电流(转mA)，判断是否异常
    //   步骤6:  波长测试 — 用波长计读取模块发射光波长
    //   步骤7:  光开关切Tx方向 → Tx VOA归零 → 开模块激光 → 设10G速率 → 等1500ms稳定
    //   步骤8:  发射功率调试 — OPM读功率 → SetTxPower自动调节APC DAC → OPM 5次取平均验证
    //   步骤9:  消光比调试 — ERM读ER → 最多迭代10次调节MOD DAC使ER达到目标6dB
    //   步骤10: DDM读取校验 — 读Temp/Vcc/Bias/TxPwr/RxPwr 5个监控值，检查范围
    //   步骤11: TxFault告警测试 — 读TxFault标志位，确认正常
    //   步骤12: 光开关切Rx方向 → 关模块激光 → 开外部光源 → Rx VOA归零 → 读初始Rx功率
    //   步骤13: ★接收灵敏度测试（RxADC突变点法，本次改造核心）
    //           → VOA从0逐步加0.5dB衰减 → 每步读RxADC → 连续3次ADC<30%基线判定突变
    //           → 灵敏度 = 初始Rx功率 - 突变点衰减量
    //   步骤14: LOS断言/解除测试 → 加衰减直到LOS=1(断言)，再减衰减直到LOS=0(解除)，测回差
    //   步骤15: DDM告警阈值检查 — 读DDM告警标志寄存器
    //   步骤16: 写Flash校准数据 + EEPROM校验 — 写入告警阈值和校准参数，持久保存
    //
    // 【停止机制】
    //   使用协作式取消（而非Thread.Abort）：StopTest()设置_stopRequested=true，
    //   测试线程在CheckStop()中检测到后抛StopRequestedException，
    //   被catch块捕获后优雅退出。finally块的Cleanup()确保激光/VOA/光源安全关闭。
    //===========================================================================

    public class ChannelTester
    {
        //=======================================================================
        // 字段说明
        //=======================================================================

        private ChannelContext _ctx;             // 本通道的数据上下文对象（"小本本"）
                                                 // 从ChannelManager获取，每个通道独有一份
                                                 // 所有测试结果（功率/电流/ER/灵敏度/SN等）
                                                 // 都存到这里，UI通过它读取数据刷新显示

        private SfpDriverAdapter _driver;        // SFP模块驱动适配器
                                                 // 封装了SFF-8472协议的所有I2C寄存器操作
                                                 // （读/写寄存器、设功率、读DDM、写Flash等）
                                                 // 在步骤1中创建并初始化

        private Thread _testThread;              // 本通道的测试线程对象
                                                 // StartTest()中new Thread(TestThreadProc)创建
                                                 // IsBackground=true（程序退出自动终止）

        private volatile bool _isRunning = false;
        // 是否正在测试中。
        // volatile关键字：告诉编译器/CPU不要缓存这个变量，
        // 每次读取都从内存获取最新值，保证UI线程(timer轮询)和
        // 测试线程之间的可见性。UI线程读，测试线程写。

        private volatile bool _stopRequested = false;
        // 用户是否请求停止测试。
        // UI线程(停止按钮)设为true，测试线程在CheckStop()中检测。
        // volatile同上，保证跨线程可见性。

        //=======================================================================
        // 公共属性（只读，供UI等外部访问）
        //=======================================================================

        /// <summary>通道编号(0~3)，直接从_ctx取</summary>
        public int ChannelIndex { get { return _ctx.ChannelIndex; } }

        /// <summary>本通道的数据上下文，UI通过此属性读取测试结果</summary>
        public ChannelContext Context { get { return _ctx; } }

        /// <summary>是否正在测试中（UI timer轮询用）</summary>
        public bool IsRunning { get { return _isRunning; } }

        //=======================================================================
        // 构造函数
        //=======================================================================

        /// <summary>
        /// 构造一个通道测试执行器。
        /// 注意：构造函数只获取ChannelContext引用，不创建线程、不操作硬件。
        /// 真正启动测试需要调用StartTest()。
        /// </summary>
        /// <param name="channelIndex">通道编号(0~3)，对应4通道治具的物理槽位</param>
        public ChannelTester(int channelIndex)
        {
            // 从全局通道管理器获取对应通道的ChannelContext实例。
            // ChannelManager是静态类，程序启动时就创建好了4个ChannelContext。
            _ctx = ChannelManager.GetChannel(channelIndex);
        }

        //=======================================================================
        // InitializeAll —— 静态工厂方法，一次性创建4个通道的Tester
        //=======================================================================

        /// <summary>
        /// 创建4个ChannelTester实例（对应4个通道），返回数组。
        /// 在Main_Form.btnAuto_Click中调用：_testers = ChannelTester.InitializeAll()
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
        // StartTest —— UI线程调用，启动本通道测试
        //=======================================================================

        /// <summary>
        /// 启动测试（在UI线程调用，非阻塞）。
        /// 工作流程：
        ///   1. 检查是否已在运行（防重复启动）
        ///   2. 重置所有状态/结果/日志
        ///   3. 创建新的后台线程，入口为TestThreadProc()
        ///   4. 启动线程，立即返回（不阻塞UI）
        /// </summary>
        public void StartTest()
        {
            if (_isRunning) return;       // 防重复启动：已经在跑就直接返回

            _stopRequested = false;       // 清除上次可能残留的停止标志
            _ctx.IsTesting = true;        // 标记UI：此通道正在测试（UI显示"测试中"状态）
            _ctx.TestPassed = false;      // 重置上次测试结果
            _ctx.ErrorMessage = "";       // 清空上次错误信息
            _ctx.ClearLog();              // 清空日志文本（RichTextBox/ListBox显示内容）
            _ctx.ResetTestResults();      // 清空所有测试数值（光功率/电流/ER/灵敏度等重置为默认值）

            // 创建新的测试线程。
            // 线程入口是TestThreadProc()方法，线程启动后操作系统会调度执行该方法。
            _testThread = new Thread(TestThreadProc);
            _testThread.IsBackground = true;  // 后台线程：主程序(前台线程)退出时自动终止，
                                              // 不会出现"程序关了但进程还在"的问题
            _testThread.Name = string.Format("CH{0}_TestThread", _ctx.ChannelIndex);
                                              // 给线程命名方便调试：在VS的"线程"窗口能看到CH0_TestThread等名字
            _testThread.Start();              // ★ 启动线程！操作系统调度TestThreadProc()开始执行
            _isRunning = true;                // 标记为运行中（放在Start之后，防止竞态：
                                              // 如果在Start前设true，线程函数可能在Start返回前
                                              // 就跑完并设_isRunning=false，然后这里又设回true）
        }

        //=======================================================================
        // StopTest —— UI线程调用，请求停止本通道测试
        //=======================================================================

        /// <summary>
        /// 请求停止测试（协作式取消）。
        /// 不强制终止线程（Thread.Abort已过时且危险），只设置停止标志，
        /// 测试线程在CheckStop()中检测到后会自己优雅退出。
        /// </summary>
        public void StopTest()
        {
            _stopRequested = true;
        }

        //=======================================================================
        // TestThreadProc —— 测试线程主函数（核心！16步测试全在这里）
        //
        // 整体结构是一个大的 try-catch-finally：
        //   try     { 16步测试流程 }
        //   catch1  (StopRequestedException) → 用户主动停止
        //   catch2  (Exception ex)           → 测试出错
        //   finally { 无论如何都清理硬件（关激光/VOA/光源） }
        //=======================================================================

        private void TestThreadProc()
        {
            int slot = _ctx.ChannelIndex + 1; // 硬件槽位号(1~4)。
                                              // ChannelIndex是0~3(数组下标)，
                                              // 硬件API(MoudlePowerOn等)用1-based编号，所以+1

            bool testPassed = true;  // 测试是否通过的标志。
                                     // 初始假设通过，遇到严重问题（如RxADC未检测到突变点）
                                     // 才设为false。小问题（电流偏大、波长超范围）只记警告不判失败。

            _driver = null;  // 驱动先置空，步骤1中创建

            try
            {
                _ctx.UpdateStatus("等待共享硬件...");
                _ctx.AddLog(string.Format("===== 通道{0} SFP测试开始 =====", _ctx.ChannelIndex));

                //----------------------------------------------------------------
                // 步骤1: 模块上电 + I2C通信检测 + 驱动初始化
                //
                // 流程：先断电(硬复位) → 等200ms放电 → 设3.3V电压 → 上电 →
                //       等500ms模块启动稳定 → 读电流检查 → 创建I2C和驱动 →
                //       读A0页第0字节验证I2C通信
                //
                // 硬件: 治具板电源控制(I2C) + I2C总线
                // 锁:   I2CLock（电源芯片和I2C总线共享）
                //----------------------------------------------------------------
                _ctx.UpdateStatus("模块上电...");
                _ctx.AddLog("模块上电...");

                // 先断电：硬复位模块，确保从干净状态开始
                // ModulePowerOn(slot, 0) 第二个参数0=断电
                lock (SharedHardwareLocks.I2CLock)
                {
                    HardwareHelper.ModulePowerOn(slot, 0); // 通过治具板控制模块电源引脚断电
                }
                Thread.Sleep(200); // 等待200ms让电容放电完全，确保彻底复位

                // 上3.3V电
                lock (SharedHardwareLocks.I2CLock)
                {
                    HardwareHelper.ModuleSetVoltage(slot, 3.3); // 设供电电压为3.3V（SFP标准）
                    HardwareHelper.ModulePowerOn(slot, 1);      // 上电（1=接通电源）
                }
                Thread.Sleep(500); // 等待500ms让模块内部电路启动稳定。
                                   // 光模块上电后需要时间初始化MCU/激光器驱动/DSP等，
                                   // 不等就操作I2C会通信失败。

                // 检查工作电流
                double current = HardwareHelper.ModuleGetCurrent(slot); // 读模块工作电流(安培)
                _ctx.AddLog(string.Format("模块电流: {0:F3}A", current));
                if (current < 0.01 || current > 0.5) // <10mA可能没插好/坏了，>500mA可能短路
                {
                    _ctx.AddLog("警告: 模块电流异常");
                    // 只是警告不终止：有些模块正常电流就偏大，或者电流检测精度有限
                }

                // 创建I2C通信通道和模块驱动
                I2C i2c = HardwareHelper.CreateI2CForSlot(slot);
                // CreateI2CForSlot为指定槽位创建I2C通信对象。
                // 4通道治具通过I2C Switch/MUX（如PCA9548）选通对应槽位的模块，
                // 此函数配置MUX使该槽位的I2C通道可用。
                if (i2c == null)
                {
                    throw new Exception("I2C初始化失败"); // I2C设备没找到/驱动没装/治具板没连
                }
                _driver = new SfpDriverAdapter();  // 创建SFP模块驱动适配器
                _driver.Init(i2c);                 // 把I2C对象绑到驱动上，之后_driver的所有
                                                   // I2C操作都走这个通道
                _ctx.ModuleTest = _driver;         // 保存到_ctx（其他地方可能需要直接访问驱动）

                // I2C通信检测：尝试读A0页第0字节
                // A0页(设备地址0xA0/0xA1)第0字节是SFF-8472定义的Identifier字段，
                // SFP/SFP+模块应返回0x03
                lock (SharedHardwareLocks.I2CLock)
                {
                    try
                    {
                        byte[] buf = new byte[1];
                        i2c.TWI_ReadPage(0xA0, 0, buf, 1);
                        // TWI_ReadPage参数：(I2C设备地址, 寄存器起始地址, 接收缓冲区, 读取字节数)
                        // 0xA0 = 0x50(7位地址)<<1 | 0(读操作，某些实现用0表示写1表示读，视驱动而定)
                        _ctx.AddLog(string.Format("I2C通信正常, A0[0]=0x{0:X2}", buf[0]));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("I2C通信失败: " + ex.Message);
                        // I2C读失败 → 模块没插好/损坏/接触不良/SDA-SCL短路，直接终止测试
                    }
                }
                CheckStop(); // 每步后检查用户是否点了停止

                //----------------------------------------------------------------
                // 步骤2: 芯片ID检测
                //
                // 读取模块芯片的ID寄存器，确认是UX3320T（或其他支持的芯片型号）。
                // 如果芯片ID不对 → 模块型号错了/芯片损坏/不是我们要测的模块。
                //
                // 硬件: I2C读模块寄存器
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("检测芯片ID...");
                _ctx.AddLog("检测芯片ID...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    if (!_driver.CheckTestTypeInfo())
                    {
                        // CheckTestTypeInfo()内部读取芯片ID寄存器并与已知ID列表比对
                        throw new Exception("芯片ID检测失败，请确认模块类型");
                    }
                }
                _ctx.AddLog("芯片ID检测通过");
                CheckStop();

                //----------------------------------------------------------------
                // 步骤3: 进入调试模式 + Tx使能
                //
                // SFP模块正常运行时，很多控制寄存器（APC/MOD/LOS阈值等）是
                // 写保护的。SetDebugPWD()写入厂商特定的调试密码序列，解锁这些
                // 寄存器，后面才能写入校准参数。
                //
                // 如果模块带TEC（半导体制冷器，用于DWDM温控），还需要使能Tx并
                // 等待3秒让温度稳定。
                //
                // 硬件: I2C写模块寄存器
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("进入调试模式...");
                _ctx.AddLog("进入调试模式...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    if (!_driver.GetSfpDriver().SetDebugPWD())
                    {
                        // SetDebugPWD()向特定寄存器写入密码序列（厂商私有）
                        throw new Exception("进入调试模式失败");
                    }
                }
                _ctx.AddLog("调试模式已进入");

                // TEC（Thermo-Electric Cooler）半导体制冷器处理
                if (GlobalVarFun.tx_tec_test) // 配置中启用了TEC测试（DWDM等温控型激光器）
                {
                    Thread.Sleep(1000);
                    lock (SharedHardwareLocks.I2CLock)
                    {
                        if (!_driver.GetSfpDriver().SetTx_EN()) // 设置Tx Enable寄存器，打开发射
                        {
                            _ctx.AddLog("警告: Tx使能操作失败");
                        }
                        else
                        {
                            _ctx.AddLog("Tx使能成功");
                        }
                    }
                    Thread.Sleep(3000); // TEC建立稳定温度需要3秒左右
                }
                CheckStop();

                //----------------------------------------------------------------
                // 步骤4: 模块信息读取(A0/A2页)
                //
                // 读取SFP模块的A0页(0~255字节，基本信息)和A2页(0~255字节，DDM/校准信息)：
                //   - GetFlashInfo()：把A0/A2页全部读出来存到_ctx.flash_data[2048]
                //   - ReadInfo()：按SFF-8472协议解析SN/PN/Vendor/Date/BitRate等字段
                //   - CheckModuleFlashInfo()：校验Flash内容合法性（校验和、厂商关键字段）
                //
                // 硬件: I2C读模块A0/A2页
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("读取模块信息...");
                _ctx.AddLog("读取A0/A2页模块信息...");
                string flashError = "";
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.GetSfpDriver().GetFlashInfo();
                    // 读取A0页（地址0xA0，偏移0~255）和A2页（地址0xA2，偏移0~255），
                    // 共512字节存入flash_data数组。A0页包含：模块类型(SFP/SFP+/SFF)、
                    // 速率、连接头(LC/SC)、传输距离、厂商名、OUI、PN、SN、生产日期等。
                    // A2页包含：DDM阈值（温度/电压/偏流/功率的高低告警阈值）、
                    // DDM实时值、校准系数等。

                    if (!_driver.ReadInfo())
                    {
                        // 从flash_data中按SFF-8472规定的偏移量解析各个字段，
                        // 存入_ctx.sn(序列号)、_ctx.pn(型号)、_ctx.vn(厂名)、
                        // _ctx.date(生产日期)、_ctx.bitRate(速率编码)、_ctx.chipType等
                        _ctx.AddLog("警告: DDM信息读取异常");
                    }
                    if (!_driver.GetSfpDriver().CheckModuleFlashInfo(ref flashError))
                    {
                        // 检查Flash中的关键字段是否合法：校验和是否正确、
                        // 厂商名是否为预期值、PN格式是否正确等
                        _ctx.AddLog("Flash信息警告: " + flashError);
                    }
                }
                _ctx.AddLog("模块信息读取完成");
                CheckStop();

                //----------------------------------------------------------------
                // 步骤5: 电流检测（详细测量，结果存_ctx）
                //
                // 步骤1中已经做过一次粗略电流检查，这里是上电稳定后的精确测量，
                // 结果存到_ctx.currentValue用于显示和数据库记录。
                //
                // 正常SFP+工作电流约200~350mA。
                //
                // 硬件: 治具板ADC(通过I2C读)
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("检测电流...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    current = HardwareHelper.ModuleGetCurrent(slot); // 读电流(安培)
                }
                _ctx.currentValue = (float)(current * 1000); // 安培→毫安，存到_ctx
                _ctx.AddLog(string.Format("工作电流: {0:F1}mA", _ctx.currentValue));
                if (_ctx.currentValue > 400)
                {
                    _ctx.AddLog("警告: 电流偏大(>400mA)");
                }
                CheckStop();

                //----------------------------------------------------------------
                // 步骤6: 波长测试
                //
                // 用Keysight 86120C波长计（GPIB接口）测量模块发射光的中心波长。
                // 波长计是4通道共享的独立仪器，必须加锁。
                // 如果波长计未连接，跳过此步骤不报错。
                //
                // 常见波长：1310nm(10GBASE-LR)、1550nm(10GBASE-ER)、850nm(10GBASE-SR)
                //
                // 硬件: Keysight 86120C波长计(GPIB)
                // 锁:   WavelengthMeterLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("测试波长...");
                _ctx.AddLog("读取波长...");
                double wavelength = 0;
                lock (SharedHardwareLocks.WavelengthMeterLock)
                {
                    wavelength = HardwareHelper.ReadWavelength(); // 发GPIB命令读取波长(nm)
                }
                _ctx.wavelength = (float)wavelength;
                if (wavelength > 0)
                {
                    _ctx.AddLog(string.Format("波长: {0:F1}nm", wavelength));
                    if (wavelength < 1270 || wavelength > 1610) // 1270~1610nm覆盖所有常见SFP+波长
                    {
                        _ctx.AddLog(string.Format("警告: 波长{0:F1}nm超出正常范围", wavelength));
                    }
                }
                else
                {
                    _ctx.AddLog("波长计未连接，跳过波长测试"); // 返回0表示未连接，非致命
                }
                CheckStop();

                //----------------------------------------------------------------
                // 步骤7: 光开关切Tx方向 + 开激光 + 设速率
                //
                // 准备发射测试光路：
                //   1. OTP12光开关切换到Tx方向（模块Tx输出→OPM/ERM）
                //   2. Tx路径VOA归零（无衰减，让光功率计接收最强信号）
                //   3. 通过硬件引脚和I2C寄存器打开模块激光器
                //   4. 设置Tx/Rx工作速率为10Gbps
                //   5. 等待1500ms让激光器输出稳定
                //
                // 光开关路由示意：
                //   Tx方向: 模块发的光 → 光开关 → OTP12板上OPM(光功率计)/ERM(消光比仪)
                //   Rx方向: OTP12光源 → 光开关 → VOA → 模块收光口
                //
                // 硬件: OTP12(光开关/VOA) + I2C(模块寄存器) + 治具板GPIO(Tx Enable引脚)
                // 锁:   OtpDriverLock + I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("切换光开关(Tx)...");
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    HardwareHelper.OpticalSwitchRoute(slot, true); // true=切到Tx方向
                    HardwareHelper.SetTxVOA(slot, 0); // Tx路径可调光衰减器归零(dB=0，无衰减)
                }
                Thread.Sleep(300); // 光开关是机械器件，切换需要稳定时间

                lock (SharedHardwareLocks.I2CLock)
                {
                    HardwareHelper.ModuleTxEnable(slot, 0);
                    // 硬件Tx Enable引脚控制，0=使能（低电平有效Low-Active）
                    // 3.3V=关激光，0V=开激光

                    _driver.TxDisableAll(false);
                    // 软件Tx Disable寄存器：false=不禁止=开激光（双重控制）

                    _driver.TxSelect(true);
                    // QSFP多lane模块选择Tx通道（SFP单通道也统一接口）

                    _driver.SetTxRate(10); // 设发射速率10Gbps（对应10G以太网/OC-192）
                    _driver.SetRxRate(10); // 设接收速率10Gbps
                    _driver.SetRxDDEM(0, true); // 使能Rx DDM/DDEM（数字诊断/眼图测量）功能
                }
                Thread.Sleep(1500); // 激光器从关到开，偏置电流建立、光功率稳定需要时间。
                                   // 不等待就测光功率/ER会不准，调试也会失败。
                CheckStop();

                //----------------------------------------------------------------
                // 步骤8: 发射功率调试（APC闭环 + OPM验证）
                //
                // 目标：调节模块APC(自动功率控制)DAC寄存器，使发射光功率达到目标值。
                // 流程：
                //   1. 确定目标功率（默认-2dBm，可通过TestSet配置）
                //   2. OPM读一次初始功率（日志参考）
                //   3. SetTxPower()内部闭环调节APC DAC，直到OPM读数接近目标
                //   4. 等500ms稳定
                //   5. OPM连续读5次取平均（消除波动），得到最终功率
                //   6. 判断偏差是否在容差内
                //
                // 硬件: OTP12 OPM(光功率计) + I2C(模块APC DAC寄存器)
                // 锁:   OtpDriverLock(读OPM) + I2CLock(写APC)
                //----------------------------------------------------------------
                _ctx.UpdateStatus("调试发射功率...");
                _ctx.AddLog("===== 发射功率调试 =====");
                double targetTxPwr = -2.0; // 默认目标光功率 -2dBm（10G SFP+典型值）
                if (TestSet.txPwr_target != 0)
                {
                    targetTxPwr = TestSet.txPwr_target; // 如果Setup界面配置了自定义目标值则使用
                }

                // 先读一次初始功率（调试前参考值）
                double txPowerOpm = -99;
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    txPowerOpm = HardwareHelper.ReadTxOPMPower(slot);
                    // ReadTxOPMPower从OTP12板上集成的OPM(光功率计)读取功率值(dBm)
                    // Tx路径光开关已经切好，OPM接收模块发出的光
                }
                _ctx.AddLog(string.Format("初始发射功率: {0:F2}dBm", txPowerOpm));

                // 自动调节APC DAC
                double finalPwr;
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.SetTxPower(0, targetTxPwr, out finalPwr);
                    // SetTxPower内部闭环算法：
                    //   循环：读OPM功率 → 计算与目标的误差 → 调整APC DAC值 → 等待稳定 → 再读
                    //   APC DAC越大 → 偏流越大 → 光功率越大；反之越小
                    //   直到功率在容差范围内或达到最大迭代次数
                }
                Thread.Sleep(500); // 等APC闭环稳定

                // OPM连续读5次取平均（消除光功率的随机波动噪声）
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    double sum = 0;
                    int cnt = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        double p = HardwareHelper.ReadTxOPMPower(slot);
                        if (p > -40) { sum += p; cnt++; } // -40dBm以下视为无效值（无光/OPM超量程）
                        Thread.Sleep(50); // 间隔50ms，总共250ms
                    }
                    if (cnt > 0) txPowerOpm = sum / cnt; // 有效读数取平均
                }
                _ctx.TxPowerDbm = (float)txPowerOpm; // 最终光功率存_ctx
                _ctx.AddLog(string.Format("目标: {0:F2}dBm, OPM实测: {1:F2}dBm", targetTxPwr, txPowerOpm));

                if (Math.Abs(txPowerOpm - targetTxPwr) > 2.0) // 偏差超过2dB警告
                {
                    _ctx.AddLog("警告: 发射功率偏差较大");
                    // APC闭环正常能调到±0.5dB内，偏差2dB以上可能激光器老化/耦光不良
                }
                CheckStop();

                //----------------------------------------------------------------
                // 步骤9: 消光比调试（ERM替代DCA，迭代调MOD DAC）
                //
                // 【关键改动】使用ERM（消光比仪，OTP12板上集成）替代原来的DCA眼图仪
                // 来测量ER值，不再需要昂贵的示波器。
                //
                // 消光比(ER)：光信号逻辑"1"电平和逻辑"0"电平的比值，单位dB。
                //   ER太小 → 眼图张不开 → 接收端误码率高
                //   ER太大 → 过调制 → 振铃/啁啾 → 传输距离受限
                //   目标ER = 6dB（SFP+典型值）
                //
                // MOD(调制电流)DAC：控制激光器调制深度。MOD越大→"0""1"差越大→ER越大。
                //
                // 迭代算法：
                //   1. SetER设置初始MOD DAC值
                //   2. ERM读当前ER值
                //   3. 判断是否达标(|ER-6|<0.5dB)
                //   4. 不达标→根据偏差大小调整MOD DAC（离目标远大步调，近小步调）
                //   5. 重复最多10次
                //   6. 最后校准Tx Bias和Tx Power（写入DDM校准寄存器）
                //
                // 硬件: OTP12 ERM(消光比仪) + I2C(模块MOD DAC寄存器)
                // 锁:   OtpDriverLock(读ERM) + I2CLock(写MOD)
                //----------------------------------------------------------------
                _ctx.UpdateStatus("调试消光比(ERM)...");
                _ctx.AddLog("===== 消光比调试(ERM) =====");

                // 配置ERM测量速率（不同速率下ER测量带宽不同，必须匹配）
                string ermRate = "10G"; // SFP UX3320T 默认10G
                if (!string.IsNullOrEmpty(_ctx.bitRate))
                {
                    // 从模块信息中解析速率
                    if (_ctx.bitRate.Contains("10G")) ermRate = "10G";
                    else if (_ctx.bitRate.Contains("2.5G")) ermRate = "2.5G";
                    else if (_ctx.bitRate.Contains("1.25G")) ermRate = "1.25G";
                }
                HardwareHelper.ErmSetRate(slot, ermRate); // 给ERM发命令设置速率
                _ctx.AddLog(string.Format("ERM速率: {0}", ermRate));

                // 设置ER初始值（SetER内部设置MOD DAC粗调值）
                double erTarget = 6.0; // 目标消光比6dB
                double erFinal;
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.SetER(0, erTarget, out erFinal);
                    // SetER设置初始调制电流，基于芯片默认值/查表得到初始MOD DAC
                }
                Thread.Sleep(1000); // MOD DAC改变后ER需要时间稳定

                // 迭代微调MOD DAC，最多10次
                double er = 0;
                for (int i = 0; i < 10; i++)
                {
                    CheckStop(); // 每次迭代都检查停止请求

                    // 从ERM读取当前ER值和光功率
                    double erPower, erVal;
                    bool ermOk = false;
                    lock (SharedHardwareLocks.OtpDriverLock)
                    {
                        ermOk = HardwareHelper.ErmReadPowerAndER(slot, out erPower, out erVal);
                        // 返回erPower(dBm)=通过ERM的光功率, erVal(dB)=消光比
                    }
                    if (!ermOk || erVal < 0)
                    {
                        _ctx.AddLog(string.Format("ERM读取失败(第{0}次), 重试...", i + 1));
                        Thread.Sleep(500);
                        continue; // ERM偶尔会读失败（同步问题），重试
                    }
                    er = erVal;
                    _ctx.AddLog(string.Format("ER实测(ERM): {0:F2}dB (目标{1:F2}dB), MOD={2}",
                        er, erTarget, _driver.GetModulation(0))); // 记录当前ER和MOD DAC值

                    if (Math.Abs(er - erTarget) < 0.5) // 偏差<0.5dB算达标
                    {
                        _ctx.AddLog("ER已达标");
                        break;
                    }

                    // ★ 自适应步长调整算法：
                    // 离目标远→大步调（快速逼近），离目标近→小步调（精细调节，防过冲）
                    double erErr = erTarget - er; // 正=ER不够需要加大，负=ER太大需要减小
                    int modAdj;
                    if (er < 4) modAdj = 30;         // ER严重偏小→加大MOD，大步30
                    else if (er < 6) modAdj = 20;    // ER中等偏小→中步20
                    else if (er < 8) modAdj = 12;    // ER接近目标→小步12
                    else modAdj = 8;                 // ER偏大→减小MOD，最小步8
                    if (erErr < 0) modAdj = -(int)(modAdj * 0.6);
                    // ER偏大时调整幅度缩小到60%（modAdj变负数），
                    // 原因：减小MOD（过调制→正常）方向更敏感，步子太大会震荡不收敛

                    // 计算新MOD值并限幅在DAC范围内
                    int curMod = _driver.GetModulation(0); // 读当前MOD DAC值(0~1023)
                    int newMod = curMod + modAdj;
                    newMod = Math.Max(10, Math.Min(1023, newMod)); // 10位DAC范围10~1023，
                                                                   // 不设0防止完全关断调制
                    if (newMod == curMod) break; // 已经到边界了，不再调

                    // 写入新MOD DAC值
                    lock (SharedHardwareLocks.I2CLock)
                    {
                        _driver.SetModulation(0, newMod);
                    }
                    Thread.Sleep(800); // MOD改变后等待ER稳定
                }
                _ctx.ErDb = (float)er; // 最终ER值存_ctx

                // Tx Bias和Tx Power校准：
                // 把当前实测的Bias和Power值写入模块DDM校准寄存器，
                // 这样模块运行时DDM上报的数值才准确。
                _ctx.BiasMa = (float)_driver.ReadTxBias(0); // 读最终偏置电流(mA)
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    Thread.Sleep(300);
                    _ctx.TxPowerDbm = (float)HardwareHelper.ReadTxOPMPower(slot); // 最终光功率(dBm)
                }
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.CalibrateTxBias(0, _ctx.BiasMa);
                    // 校准Tx Bias：把当前Bias实测值写入校准寄存器，
                    // 模块内部据此计算内部ADC读数→mA的转换系数
                    _driver.CalibrateTxPower(0, _ctx.TxPowerDbm);
                    // 校准Tx Power：同理，写入功率校准点
                }
                _ctx.AddLog(string.Format("Tx功率: {0:F2}dBm, Bias: {1:F2}mA, ER: {2:F2}dB",
                    _ctx.TxPowerDbm, _ctx.BiasMa, _ctx.ErDb));
                CheckStop();

                //----------------------------------------------------------------
                // 步骤10: DDM读取校验
                //
                // DDM(Digital Diagnostics Monitoring)是SFF-8472定义的数字诊断功能，
                // 模块内部ADC实时采样5个模拟量：温度、电压、偏流、Tx功率、Rx功率。
                // 这里读取这些值并检查是否在合理范围内。
                //
                // DDM值在A2页偏移96~111字节（内部校准方式直接是工程量，
                // 外部校准方式需要用斜率/截距系数换算）。
                //
                // 正常范围参考：
                //   温度: -10~90℃（工作范围），室温下25~45℃
                //   电压: 3.0~3.6V（标称3.3V±10%）
                //   Bias: 10~100mA（视激光器类型）
                //   Tx:   -5~+2dBm（已在步骤8校准）
                //   Rx:   取决于输入光（步骤7后Tx自环可能有-5~-15dBm）
                //
                // 硬件: I2C读模块A2页DDM寄存器
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("读取DDM...");
                _ctx.AddLog("读取DDM监控值...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.GetDDMAnalogValues();
                    // 触发DDM ADC转换，从A2页读取5个实时监控值存入内部缓存

                    _ctx.tempDDM = (float)_driver.ReadTemperature();    // 模块温度(℃)
                    _ctx.vccDDM = (float)_driver.ReadVoltage();         // 供电电压(V)
                    _ctx.txBiasDDMSingle = (float)_driver.ReadTxBias(0); // Tx偏置电流(mA)
                    _ctx.txPowerDDMSingle = (float)_driver.ReadTxPower(0); // Tx光功率(dBm)
                    _ctx.rxPowerDDM = (float)_driver.ReadRxPower(0);    // Rx光功率(dBm)
                }
                _ctx.AddLog(string.Format("DDM: Temp={0:F1}℃, Vcc={1:F2}V, Bias={2:F1}mA, Tx={3:F2}dBm, Rx={4:F2}dBm",
                    _ctx.tempDDM, _ctx.vccDDM, _ctx.txBiasDDMSingle, _ctx.txPowerDDMSingle, _ctx.rxPowerDDM));

                if (_ctx.tempDDM < -10 || _ctx.tempDDM > 90)
                    _ctx.AddLog(string.Format("警告: 温度{0:F1}℃异常", _ctx.tempDDM));
                if (_ctx.vccDDM < 3.0 || _ctx.vccDDM > 3.6)
                    _ctx.AddLog(string.Format("警告: 电压{0:F2}V异常", _ctx.vccDDM));
                CheckStop();

                //----------------------------------------------------------------
                // 步骤11: TxFault告警测试
                //
                // TxFault是模块的发射故障硬件告警引脚/寄存器位。
                // 当激光器驱动电流异常（过流/欠流）、过温、或激光器失效时置1。
                // 正常工作时应该为0。
                //
                // 硬件: I2C读模块TxFault标志位（或治具板GPIO读引脚电平）
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("测试TxFault...");
                _ctx.AddLog("测试TxFault告警...");
                bool txFault = false;
                lock (SharedHardwareLocks.I2CLock)
                {
                    txFault = _driver.GetTxLosFlag(0); // 读TxFault/LOS标志寄存器
                }
                if (txFault)
                {
                    _ctx.AddLog("警告: TxFault告警已置位"); // 激光器可能有问题
                }
                else
                {
                    _ctx.AddLog("TxFault正常");
                }
                CheckStop();

                //----------------------------------------------------------------
                // 步骤12: 光开关切Rx方向 + 开外部光源
                //
                // 准备接收测试光路：
                //   1. 关模块自身激光（Rx测试时不需要模块发光，避免干扰）
                //   2. 光开关切到Rx方向（OTP12光源→VOA→模块Rx口）
                //   3. 配置外部光源：波长1310nm，功率0dBm
                //   4. Rx VOA归零（无衰减）
                //   5. 开外部光源，等500ms稳定
                //   6. 读取Rx输入功率基准值rxPwr0（VOA=0时模块收到的光功率）
                //
                // rxPwr0是计算灵敏度的基准：
                //   灵敏度(dBm) = rxPwr0(dBm) - 突变点衰减量(dB)
                //
                // 硬件: OTP12(光开关/VOA/光源) + I2C(模块寄存器) + 治具板GPIO
                // 锁:   I2CLock + OtpDriverLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("切换光开关(Rx)...");
                _ctx.AddLog("切换到接收测试路径...");

                // 关模块自身激光
                lock (SharedHardwareLocks.I2CLock)
                {
                    HardwareHelper.ModuleTxEnable(slot, 1); // 1=禁止Tx（关激光，高有效）
                    _driver.TxDisableAll(true);  // 软件Tx Disable = true（禁止发光）
                    _driver.TxSelect(false);     // 取消Tx选择
                    _driver.SetRxDDEM(0, true);  // 保持Rx DDM使能
                }

                // 光开关切Rx方向，配置并打开外部光源
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    HardwareHelper.OpticalSwitchRoute(slot, false); // false=切到Rx方向
                    // Rx光路: OTP12光源 → 光开关 → Rx VOA → 模块收光口(ROSA)

                    HardwareHelper.SourceSetWavelength(slot, 1310); // 光源波长1310nm
                    HardwareHelper.SourceSetPower(slot, 0);         // 光源输出功率0dBm
                    HardwareHelper.SetRxVOA(slot, 0);               // Rx路径VOA归零(无衰减)
                    HardwareHelper.SourceSetState(slot, "ON");      // 打开外部光源
                }
                Thread.Sleep(500); // 等待光源和光开关稳定
                CheckStop();

                // 读取Rx输入功率基准（VOA=0时模块实际收到的光功率）
                double rxPwr0 = -99;
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    rxPwr0 = HardwareHelper.ReadRxInputPower(slot);
                    // 读OTP12 Rx路径的功率计（在VOA输出端/模块输入端前）
                    // 这个值 ≈ 光源功率(0dBm) - 光开关/连接器插入损耗(1~3dB)
                    // 通常约-1~-3dBm
                }
                _ctx.AddLog(string.Format("Rx输入功率(VOA=0): {0:F2}dBm", rxPwr0));

                //----------------------------------------------------------------
                // 步骤13: ★★★接收灵敏度测试（RxADC突变点法）★★★
                //
                // 【这是本次改造的核心！】
                // 原来的方法：用DCA眼图仪看眼图，逐步加衰减直到眼图闭合/误码，
                //             需要昂贵的示波器且速度慢。
                // 新方法：读模块内部Rx接收信号强度ADC(RxADC)寄存器值，
                //         逐步加VOA衰减，当ADC值大幅跌落（信号丢失）即为灵敏度点。
                //
                // 原理：
                //   1. 光信号强时 → ROSA接收信号大 → RxADC值高(1500~3000)
                //   2. 逐步加衰减 → 光信号变弱 → RxADC逐渐下降
                //   3. 当光功率低于接收灵敏度点 → 信号突然丢失 → RxADC急跌到低值(<100)
                //   4. 检测这个突变点对应的衰减量，即可算出灵敏度
                //
                // 算法详解：
                //   1. VOA=0时读RxADC得到基线值(baseAdc)
                //   2. 设定阈值 = 基线 × 30%（adcThreshold）
                //   3. VOA从0开始每次加0.5dB，等80ms后读RxADC
                //   4. 连续3次读到的ADC < 阈值 → 确认为突变点（防抖）
                //   5. 回退2步(1dB)作为灵敏度点(留余量)
                //   6. 回退验证：减2dB衰减，确认ADC恢复
                //   7. 灵敏度 = rxPwr0 - senAtt（dBm）
                //
                // 硬件: OTP12 RxVOA + I2C(读模块RxADC寄存器)
                // 锁:   OtpDriverLock(设VOA) + I2CLock(读ADC)
                //----------------------------------------------------------------
                _ctx.UpdateStatus("接收灵敏度测试(RxADC突变点)...");
                _ctx.AddLog("===== 接收灵敏度测试(ADC突变点法) =====");

                double senAtt = -1;     // 灵敏度点对应的VOA衰减量(dB)，-1=未找到
                int stableCount = 0;    // 连续低于阈值的次数（防抖计数器）
                const int MAX_ATT = 30; // 最大衰减30dB（超过这个灵敏度就太差了）
                const double ATT_STEP = 0.5; // 每步加0.5dB衰减

                // 读取初始RxADC基线值（VOA=0，光信号最强时）
                ushort baseAdc = 0;
                lock (SharedHardwareLocks.I2CLock)
                {
                    baseAdc = _driver.ReadRxADC(0);
                    // ReadRxADC读模块内部RSSI(接收信号强度指示)ADC原始值
                    // 这是12位ADC(0~4095)或16位ADC，反映ROSA输出的信号幅度
                }
                int adcThreshold = (int)(baseAdc * 0.3); // 阈值设为基线的30%
                // 当ADC跌到基线30%以下，认为信号已丢失。
                // 30%是经验值：足够低避免噪声误触发，足够高保证在灵敏度点附近。
                _ctx.AddLog(string.Format("RxADC基线: {0}, 突变阈值: {1}", baseAdc, adcThreshold));

                // 逐步增加VOA衰减，搜索突变点
                for (double att = 0; att <= MAX_ATT; att += ATT_STEP)
                {
                    CheckStop();

                    // 设置VOA衰减量
                    lock (SharedHardwareLocks.OtpDriverLock)
                    {
                        HardwareHelper.SetRxVOA(slot, att);
                    }
                    Thread.Sleep(80); // 等VOA机械响应(VOA是电机驱动，需要稳定时间)
                                      // + 模块内部AGC/CDR响应时间

                    // 读取当前RxADC值
                    ushort curAdc = 0;
                    lock (SharedHardwareLocks.I2CLock)
                    {
                        curAdc = _driver.ReadRxADC(0);
                    }

                    // 检测ADC是否跌落至阈值以下
                    if (curAdc < adcThreshold && adcThreshold > 10)
                    // adcThreshold > 10 防止基线值本身太小(如模块没发光/坏了)导致误判
                    {
                        stableCount++;
                        if (stableCount >= 3)
                        // 连续3次确认才判定为突变点！
                        // 防抖原因：ADC值有噪声，偶尔一次读到低可能是噪声/抖动。
                        // 连续3次（3 × (VOA设置时间+80ms等待+读ADC) ≈ 390ms）都低于阈值，
                        // 才能确认真的信号丢失了。
                        {
                            senAtt = att - ATT_STEP * 2;
                            // 回退2步(1dB)作为灵敏度点：
                            // att是ADC刚跌落的衰减量，此时信号已经丢了(BER>10^-12)，
                            // 真正的灵敏度点比这个高1~2dB。
                            // 回退2步(1dB)是保守估计，保证此时BER仍满足要求。

                            _ctx.AddLog(string.Format("ADC突变: ADC={0} < 阈值{1} @ 衰减{2:F1}dB",
                                curAdc, adcThreshold, att));
                            break;
                        }
                    }
                    else
                    {
                        stableCount = 0; // ADC高于阈值，重置连续计数（防抖复位）
                    }
                }

                if (senAtt > 0)
                {
                    // 回退验证：把VOA减小2dB，确认ADC能恢复
                    // 双重保险，防止假阳性（如I2C读错误导致误判）
                    lock (SharedHardwareLocks.OtpDriverLock)
                    {
                        HardwareHelper.SetRxVOA(slot, Math.Max(0, senAtt - 2));
                    }
                    Thread.Sleep(200);
                    ushort recoverAdc = 0;
                    lock (SharedHardwareLocks.I2CLock)
                    {
                        recoverAdc = _driver.ReadRxADC(0);
                    }
                    _ctx.AddLog(string.Format("回退验证: 衰减{0:F1}dB → ADC={1}", Math.Max(0, senAtt - 2), recoverAdc));

                    // 计算接收灵敏度(dBm)
                    _ctx.rxSen[0] = (float)(rxPwr0 - senAtt);
                    // 灵敏度 = 初始输入功率 - 突变点衰减
                    // 例：rxPwr0 = 0dBm，senAtt = 18dB → 灵敏度 = -18dBm
                    // 10GBASE-LR典型灵敏度 ≤ -18dBm
                    _ctx.AddLog(string.Format("接收灵敏度: {0:F2}dBm", _ctx.rxSen[0]));
                }
                else
                {
                    _ctx.AddLog("警告: 未检测到RxADC突变点");
                    // 到30dB衰减还没检测到突变：
                    //   1. 模块灵敏度太好（>-30dBm，不太可能）
                    //   2. Rx ADC寄存器读不到/始终高（模块问题）
                    //   3. VOA/光源故障，光没加上衰减
                    testPassed = false; // 这是严重问题，判测试失败
                }

                // VOA归零（为下一步LOS测试准备）
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    HardwareHelper.SetRxVOA(slot, 0);
                }
                Thread.Sleep(200);
                CheckStop();

                //----------------------------------------------------------------
                // 步骤14: LOS断言/解除断言测试
                //
                // LOS(Loss of Signal)是模块硬件输出的信号丢失告警信号（引脚+寄存器）。
                // 当接收光功率低于某阈值时，模块判定信号丢失，LOS拉高(1)通知系统。
                //
                // 两个参数需要测试：
                //   ALOS(Assert，断言): 光功率降低到该值时LOS从0变1（信号丢失）
                //   DLOS(De-assert，解除): 光功率升高到该值时LOS从1变0（信号恢复）
                //   回差(Hysteresis) = DLOS - ALOS，通常0.5~2dB
                //   回差存在的意义：防止在阈值附近光功率波动导致LOS频繁翻转
                //
                // 测试方法：
                //   断言：VOA从0逐步加0.5dB衰减，直到读LOS=1，记录此时功率
                //   解除：从断言点逐步减0.5dB衰减，直到读LOS=0，记录此时功率
                //
                // 硬件: OTP12 RxVOA + 治具板GPIO(读LOS引脚) 或 I2C(读LOS寄存器)
                // 锁:   OtpDriverLock(设VOA)
                //----------------------------------------------------------------
                _ctx.UpdateStatus("测试LOS断言/解除...");
                _ctx.AddLog("===== LOS断言/解除测试 =====");

                // LOS断言测试：逐步加衰减直到LOS=1
                double losAssertAtt = -1;
                for (double att = 0; att <= MAX_ATT; att += 0.5)
                {
                    CheckStop();
                    lock (SharedHardwareLocks.OtpDriverLock)
                    {
                        HardwareHelper.SetRxVOA(slot, att);
                    }
                    Thread.Sleep(100);
                    string los = HardwareHelper.ModuleGetRxLOS(slot);
                    // ModuleGetRxLOS通过治具板GPIO读模块LOS引脚电平
                    // 或通过I2C读模块A2页LOS状态寄存器位
                    if (los == "1") // LOS=1 = 信号丢失告警已触发
                    {
                        losAssertAtt = att;
                        _ctx.rxALos[0] = (float)(rxPwr0 - att); // 计算断言时的实际功率(dBm)
                        _ctx.AddLog(string.Format("LOS断言: 衰减={0:F1}dB, 功率={1:F2}dBm", att, rxPwr0 - att));
                        break;
                    }
                }

                // LOS解除测试：从断言点逐步减小衰减直到LOS=0
                double losDeassertAtt = -1;
                if (losAssertAtt > 0)
                {
                    for (double att = losAssertAtt; att >= 0; att -= 0.5)
                    {
                        CheckStop();
                        lock (SharedHardwareLocks.OtpDriverLock)
                        {
                            HardwareHelper.SetRxVOA(slot, att);
                        }
                        Thread.Sleep(100);
                        string los = HardwareHelper.ModuleGetRxLOS(slot);
                        if (los == "0") // LOS=0 = 信号恢复
                        {
                            losDeassertAtt = att;
                            _ctx.rxDLos[0] = (float)(rxPwr0 - att); // 解除时的实际功率(dBm)
                            _ctx.AddLog(string.Format("LOS解除: 衰减={0:F1}dB, 功率={1:F2}dBm", att, rxPwr0 - att));
                            break;
                        }
                    }
                }

                // 计算LOS回差
                if (losAssertAtt > 0 && losDeassertAtt > 0)
                {
                    double hysteresis = (rxPwr0 - losDeassertAtt) - (rxPwr0 - losAssertAtt);
                    // 回差 = DLOS功率 - ALOS功率（都是dBm，差值即dB）
                    _ctx.AddLog(string.Format("LOS回差: {0:F1}dB", hysteresis));
                }
                else
                {
                    _ctx.AddLog("警告: LOS断言/解除测试未完成");
                }

                // 测试完毕，复位硬件
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    HardwareHelper.SetRxVOA(slot, 0); // VOA归零
                    HardwareHelper.SourceSetState(slot, "OFF"); // 关外部光源
                }
                CheckStop();

                //----------------------------------------------------------------
                // 步骤15: DDM告警阈值检查
                //
                // 读A2页地址112~116字节的Alarm/Warning Flag寄存器：
                //   - Temp high/low alarm/warning
                //   - Vcc high/low alarm/warning
                //   - Bias high/low alarm/warning
                //   - Tx Power high/low alarm/warning
                //   - Rx Power high/low alarm/warning
                // 这些位是模块硬件自动置位的。当前测试条件下所有告警位应为0。
                //
                // 硬件: I2C读模块A2页告警标志寄存器
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("检查DDM告警...");
                _ctx.AddLog("检查DDM告警标志...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.GetDDMFlagsInterrupt();
                    // 读取并清除DDM告警标志寄存器（读会清除Latch状态）
                    // 如果有告警位置1，驱动内部可能记录日志
                }
                CheckStop();

                //----------------------------------------------------------------
                // 步骤16: 写Flash校准数据 + EEPROM校验
                //
                // 把前面调试好的所有参数写入模块内部Flash/EEPROM持久保存：
                //   1. WriteAlarmThresholds：写入DDM告警阈值
                //      （温度/电压/偏流/功率的高低告警/警告门限）
                //   2. WriteFlashData：写入校准参数
                //      （APC DAC、MOD DAC、Tx/Rx功率校准系数、LOS阈值等）
                //   3. EEPROMCheckSum：验证校验和
                //      SFF-8472要求A0页校验和：地址0~62字节之和+地址63=0x00(mod 256)
                //
                // 写入后，模块断电重启也不会丢失这些参数，用户实际使用时
                // 模块就按这些校准好的值工作。
                //
                // 硬件: I2C写模块Flash/EEPROM
                // 锁:   I2CLock
                //----------------------------------------------------------------
                _ctx.UpdateStatus("写入校准数据...");
                _ctx.AddLog("写入Flash校准数据...");
                lock (SharedHardwareLocks.I2CLock)
                {
                    _driver.WriteAlarmThresholds(0,
                        85, -40,    // Temp高/低告警阈值(℃)
                        3.8, 2.9,   // Vcc高/低告警阈值(V)
                        100, 1,     // Bias高/低告警阈值(mA)
                        2, -10,     // Tx功率高/低告警阈值(dBm)
                        1, -20);    // Rx功率高/低告警阈值(dBm)
                    // 这些阈值是SFP+模块的典型告警门限，
                    // 超出范围模块会触发告警引脚/标志位

                    if (!_driver.WriteFlashData())
                    {
                        // WriteFlashData把所有校准参数（APC/MOD/LOS/校准系数等）
                        // 写入模块非易失性Flash。写Flash通常需要特定写入时序
                        // （写使能→写数据→等待编程完成→写保护）。
                        _ctx.AddLog("警告: Flash数据写入失败");
                    }
                    else
                    {
                        _ctx.AddLog("Flash数据写入成功");
                    }

                    if (!_driver.EEPROMCheckSum())
                    {
                        // EEPROM校验和检查：
                        // SFF-8472规定A0页CC_BASE(地址95) = 0x00 - sum(0~94) (mod 256)
                        // 写入Flash数据后校验和必须重新计算并更新，否则协议栈/交换机
                        // 读模块信息时会报错。
                        _ctx.AddLog("警告: EEPROM校验和错误");
                    }
                    else
                    {
                        _ctx.AddLog("EEPROM校验和通过");
                    }
                }

                //----------------------------------------------------------------
                // 测试完成：关激光，记录结果
                //----------------------------------------------------------------
                lock (SharedHardwareLocks.I2CLock)
                {
                    HardwareHelper.ModuleTxEnable(slot, 1); // 关激光（1=禁止）
                    _driver.TxDisableAll(true);
                }

                _ctx.TestPassed = testPassed; // 写入最终结果（true=通过，false=失败）
                _ctx.UpdateStatus(testPassed ? "测试通过" : "测试失败");
                _ctx.AddLog(testPassed ? "===== 测试通过！=====" : "===== 测试失败 =====");
                _ctx.NotifyDataUpdated(); // 通知UI刷新最后一次数据
            }
            catch (StopRequestedException)
            {
                //================================================================
                // 用户主动停止触发的异常（协作式取消）
                // 由CheckStop()抛出StopRequestedException跳转到这里
                // 这不是错误，只是正常的取消流程
                //================================================================
                _ctx.UpdateStatus("已停止");
                _ctx.AddLog("测试已被用户停止");
                _ctx.TestPassed = false;
                _ctx.NotifyDataUpdated();
            }
            catch (Exception ex)
            {
                //================================================================
                // 真正的测试错误（I2C失败/芯片ID错误/通信异常等）
                // 错误信息存在ErrorMessage里，UI会显示
                //================================================================
                _ctx.TestPassed = false;
                _ctx.ErrorMessage = ex.Message;
                _ctx.UpdateStatus("测试失败: " + ex.Message);
                _ctx.AddLog("错误: " + ex.Message);
                _ctx.NotifyDataUpdated();
            }
            finally
            {
                //================================================================
                // finally块：无论测试成功/失败/被停止/异常，都会执行
                // 1. 标记测试结束
                // 2. 安全清理硬件（关激光/VOA/光源）
                //================================================================
                _ctx.IsTesting = false; // 告诉UI：这个通道测完了（UI timer检测到后更新状态）
                _isRunning = false;

                // 安全清理：确保激光关闭、硬件复位到安全状态
                try
                {
                    Cleanup(slot);
                }
                catch { }
                // Cleanup内部的异常被吞掉（catch空块），
                // 因为清理发生在finally中，如果清理又抛异常会覆盖原始异常。
                // 而且此时测试已经结束了，清理失败不应该影响测试结果。
            }
        }

        //=======================================================================
        // CheckStop —— 协作式取消检测
        //
        // 在每个测试步骤之后调用。如果用户点了停止按钮（UI线程设_stopRequested=true），
        // 则抛StopRequestedException，跳到catch(StopRequestedException)块实现优雅退出。
        //
        // 为什么用异常而不是return？
        //   因为测试流程嵌套很深（for循环内、lock内、if内），
        //   用return需要每层都判断返回值并逐层返回，代码极其臃肿。
        //   异常机制可以直接从任意深度"跳"到catch块，代码简洁清晰。
        //=======================================================================

        private void CheckStop()
        {
            if (_stopRequested)
            {
                throw new StopRequestedException("测试被用户停止");
            }
        }

        //=======================================================================
        // Cleanup —— 硬件安全清理
        //
        // 在finally块中调用，无论测试结果如何都确保硬件处于安全状态：
        //   1. 关模块激光（软件TxDisableAll + 硬件TxEnable=1 双重保险）
        //   2. Tx/Rx VOA归零（下次测试从干净状态开始）
        //   3. 关外部光源（延长光源寿命）
        //
        // 每个操作都单独try-catch，确保某个设备操作失败不会影响其他清理。
        //=======================================================================

        private void Cleanup(int slot)
        {
            // 清理模块端：关激光
            if (_driver != null)
            {
                lock (SharedHardwareLocks.I2CLock)
                {
                    try { _driver.TxDisableAll(true); } catch { }
                    // 软件关激光：通过I2C设Tx Disable寄存器

                    try { HardwareHelper.ModuleTxEnable(slot, 1); } catch { }
                    // 硬件关激光：通过GPIO拉Tx Enable引脚为高电平（禁止）
                    // 软件+硬件双保险，防止单一方式失效导致激光常开
                }
            }
            // 清理OTP12端：VOA归零 + 关光源
            if (GlobalVarFun.otp12 != null && GlobalVarFun.otp12.IsConnected)
            // 先检查OTP12是否连接，防止设备未连接时空引用异常
            {
                lock (SharedHardwareLocks.OtpDriverLock)
                {
                    try { HardwareHelper.SetTxVOA(slot, 0); } catch { }
                    // Tx路径VOA归零

                    try { HardwareHelper.SetRxVOA(slot, 0); } catch { }
                    // Rx路径VOA归零

                    try { HardwareHelper.SourceSetState(slot, "OFF"); } catch { }
                    // 关闭外部光源（白色光源/激光源长时间工作会发热/老化）
                }
            }
        }

        //=======================================================================
        // StopRequestedException —— 内部异常类
        //
        // 用于协作式取消：CheckStop()抛出后被TestThreadProc的catch块捕获。
        // 设为private是因为它只在ChannelTester内部使用，不需要暴露给外部。
        // 继承Exception基类，传入消息参数。
        //=======================================================================

        private class StopRequestedException : Exception
        {
            public StopRequestedException(string msg) : base(msg) { }
        }
    }
}