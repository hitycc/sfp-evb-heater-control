using System;
using System.Collections.Generic;
using System.Threading;

namespace FibertopTest_Common
{
    //===========================================================================
    // TestThreadManager —— 多模块并行测试线程管理器
    //
    // 【这个类是做什么的？】
    //   这是一个通用的多线程测试调度框架，负责管理最多4个模块的并行测试线程。
    //   它本身不包含任何具体的测试逻辑（不知道什么是SFP/QSFP、不知道怎么测光功率），
    //   而是通过"委托注入"的方式，让外部把具体的测试函数传给它，它只负责：
    //     1. 创建4个TestContext（每个模块一份独立的状态/数据容器）
    //     2. 为每个模块启动一个后台线程
    //     3. 在线程中调用注入的TestFunction执行具体测试
    //     4. 通过回调委托(ProgressUpdate/ModuleTestCompleted/AllTestsCompleted)
    //        通知UI测试进度和结果
    //     5. 监控所有线程完成情况
    //
    // 【设计模式】
    //   这是典型的"策略模式(Strategy Pattern)"+ "依赖注入"：
    //   - 框架（本类）：负责线程调度、上下文管理、进度通知等"基础设施"
    //   - 策略（TestFunction委托）：由外部注入具体的测试逻辑
    //   - 好处：同一个TestThreadManager可以复用于初测/终测/SFP/QSFP等不同测试流程，
    //     只要给TestFunction赋不同的函数即可，框架代码不用改。
    //
    // 【和ChannelTester的关系与区别】
    //   ┌──────────────────┬──────────────────────────┬──────────────────────────┐
    //   │      维度        │     ChannelTester        │    TestThreadManager     │
    //   ├──────────────────┼──────────────────────────┼──────────────────────────┤
    //   │ 数据隔离方式     │ ChannelContext实例字段   │ TestContext+[ThreadStatic]│
    //   │ 测试逻辑位置     │ 写死在TestThreadProc()   │ 通过TestFunction委托注入 │
    //   │ 停止机制         │ 协作式取消(stopRequested)│ Thread.Abort()强制终止   │
    //   │ UI更新方式       │ ChannelContext事件+SyncCtx│ Action委托回调           │
    //   │ 进度报告         │ 无百分比，通过状态文本   │ ProgressUpdate(slot,%)   │
    //   │ 锁管理           │ 内部直接lock硬件锁       │ 不管理，交给TestFunction │
    //   │ 硬件清理         │ finally Cleanup()安全清理│ 不负责，TestFunction自管 │
    //   │ 架构风格         │ 面向对象(封装数据+行为)  │ 函数式(委托注入策略)     │
    //   │ 适用场景         │ SFP UX3320T终测专用      │ 通用框架(QSFP/初测等)    │
    //   └──────────────────┴──────────────────────────┴──────────────────────────┘
    //
    //   Main_Form.cs中实际使用的是ChannelTester新架构。
    //   TestThreadManager可能被TestQSFP.cs使用，或是多线程改造过程中的通用框架版本。
    //
    // 【委托类型说明】
    //   - Action<T>：无返回值的回调函数（如void Callback(T param)）
    //   - Func<T, TResult>：有返回值的函数（TResult Function(T param)）
    // 【使用示例（外部调用流程）】
    //   var mgr = new TestThreadManager();
    //   mgr.InitializeContexts(firstTest: false);           // 初始化4个上下文
    //   mgr.TestFunction = MyTestFunction;                  // 注入测试函数
    //   mgr.ProgressUpdate = (slot, prog, text) => { ... }; // 订阅进度回调(UI更新)
    //   mgr.ModuleTestCompleted = ctx => { ... };           // 订阅单模块完成回调
    //   mgr.AllTestsCompleted = ctxs => { ... };            // 订阅全部完成回调
    //   mgr.StartAllTests();                                // 启动4线程并行测试
    //===========================================================================

    public class TestThreadManager
    {
        //=======================================================================
        // 常量
        //=======================================================================

        /// <summary>
        /// 最大并行测试模块数量（4通道治具，物理上限4个槽位）
        /// </summary>
        public const int MAX_MODULES = 4;

        //=======================================================================
        // 属性/字段说明
        //=======================================================================

        /// <summary>
        /// 4个模块的测试上下文数组，下标0~3对应槽位1~4。
        /// 每个TestContext是独立的，线程间不共享。
        /// { get; private set; } 表示外部可以读取（如UI读结果），
        /// 但只能在本类内部赋值（构造/初始化时创建）。
        /// 初始值为new TestContext[4]（4个null引用，InitializeContexts中填充）。
        /// </summary>
        public TestContext[] Contexts { get; private set; } = new TestContext[MAX_MODULES];

        /// <summary>
        /// 4个测试线程对象数组，和Contexts一一对应。
        /// _threads[i]是Contexts[i]对应模块的执行线程。
        /// private因为外部不需要直接操作线程对象（通过StartAllTests/AbortAll控制）。
        /// </summary>
        private Thread[] _threads = new Thread[MAX_MODULES];

        /// <summary>
        /// 【回调委托】所有4个模块都测试完成时触发。
        /// 参数TestContext[]是4个模块的完整结果集合，UI收到后可以汇总显示
        /// （如"4个模块全部通过"/"3通过1失败"、弹出结果对话框、保存数据库等）。
        /// 如果外部不赋值（null），则不触发（?.Invoke安全调用）。
        /// </summary>
        public Action<TestContext[]> AllTestsCompleted { get; set; }

        /// <summary>
        /// 【回调委托】单个模块测试完成时触发（每完成一个就触发一次，共4次）。
        /// 参数TestContext是刚完成的那个模块的上下文。
        /// UI可以实时更新该通道的状态（灯变绿/变红、显示结果数据），
        /// 不用等所有模块都完成才刷新。
        /// </summary>
        public Action<TestContext> ModuleTestCompleted { get; set; }

        /// <summary>
        /// 【回调委托】测试进度更新时触发（测试函数在执行过程中调用）。
        /// 参数1(int)：槽位号(1~4)，标识是哪个模块的进度
        /// 参数2(int)：进度百分比(0~100)，UI用来更新ProgressBar
        /// 参数3(string)：当前状态文本（如"正在调试光功率..."、"测试通过"），
        ///                UI用来显示在状态栏/日志中
        ///
        /// 注意：测试函数在后台线程上调用此回调，如果回调直接操作UI控件，
        /// 需要自己做Control.Invoke跨线程调度（或用SynchronizationContext）。
        /// </summary>
        public Action<int, int, string> ProgressUpdate { get; set; }

        /// <summary>
        /// 当前是否有测试正在运行。
        /// 用于防止重复启动（StartAllTests中检查IsRunning）。
        /// private set表示只有本类内部能修改状态，外部只读。
        /// 初始值false。
        /// </summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// 【核心委托，策略注入点！】
        /// 每个模块线程执行的具体测试函数。
        /// 签名：Func<TestContext, bool>
        ///   - 入参：TestContext（该模块的上下文，函数从中取配置、往里写结果）
        ///   - 返回：bool（true=测试通过，false=测试失败）
        ///
        /// 使用方式：
        ///   mgr.TestFunction = FirstTestProcess;  // 初测流程
        ///   mgr.TestFunction = FinalTestProcess;  // 终测流程
        ///   mgr.TestFunction = ctx => { ... };    // 或用Lambda表达式
        ///
        /// ★重要★：如果启动测试前不设置TestFunction（保持null），
        /// StartAllTests会抛InvalidOperationException异常。
        ///
        /// 在测试函数内部，可以通过TestContext.Current（[ThreadStatic]静态属性）
        /// 访问当前线程的TestContext，不必把ctx作为参数层层传递。
        /// </summary>
        public Func<TestContext, bool> TestFunction { get; set; }

        //=======================================================================
        // InitializeContexts —— 初始化4个模块的测试上下文
        //=======================================================================

        /// <summary>
        /// 创建4个TestContext对象，分别对应4个物理槽位(1~4)。
        /// 必须在StartAllTests()之前调用，否则Contexts全是null会NullReferenceException。
        ///
        /// 用C#对象初始化器语法（{...}）一次性设置初始属性值：
        ///   Slot        : 1~4（硬件1-based编号）
        ///   IsFirstTest : true=初测调试模式，false=终测检查模式
        ///   TestSuccess : 初始false（测试结束后由RunSingleTest设置）
        ///   StatusText  : "等待开始..."（UI初始显示文本）
        ///   Progress    : 0%（进度条初始值）
        ///   ErrorMessage: 空字符串（出错时填错误消息）
        /// </summary>
        /// <param name="firstTest">true=初测调试模式，false=终测检查模式</param>
        public void InitializeContexts(bool firstTest)
        {
            for (int i = 0; i < MAX_MODULES; i++)
            {
                Contexts[i] = new TestContext
                {
                    Slot = i + 1,           // 槽位号1~4（硬件用1-based）
                    IsFirstTest = firstTest,
                    TestSuccess = false,
                    StatusText = "等待开始...",
                    Progress = 0,
                    ErrorMessage = ""
                };
            }
        }

        //=======================================================================
        // StartAllTests —— 启动4个模块的并行测试
        //=======================================================================

        /// <summary>
        /// 启动全部4个模块的并行测试（在UI线程调用，非阻塞）。
        ///
        /// 工作流程：
        ///   1. 检查是否已在运行（防重复启动）
        ///   2. 检查TestFunction是否已设置（防止空引用）
        ///   3. 设IsRunning=true
        ///   4. 循环创建4个线程，每个线程入口是RunSingleTest(i)
        ///   5. 启动所有4个测试线程
        ///   6. 额外启动一个"监控线程"等待所有线程完成
        ///   7. 立即返回（不阻塞调用线程/UI线程）
        ///
        /// ★闭包陷阱说明★：
        ///   for循环中的int slotIndex = i; 这行非常关键！
        ///   Lambda表达式 () => RunSingleTest(slotIndex) 捕获的是"变量"而非"值"。
        ///   如果直接写 () => RunSingleTest(i)，4个Lambda共享同一个i变量，
        ///   当线程实际执行时for循环可能已经走完，i变成4，所有线程都调用RunSingleTest(4)
        ///   →数组越界！
        ///   解决方法：在循环体内声明int slotIndex = i，每次迭代创建一个新的局部变量，
        ///   每个Lambda捕获不同的slotIndex实例，值就正确了。
        ///   这是C#多线程编程中的经典坑。
        /// </summary>
        public void StartAllTests()
        {
            if (IsRunning) return; // 已经在运行就直接返回，防重复点击"开始"按钮
            if (TestFunction == null)
                throw new InvalidOperationException("TestFunction 未设置，无法启动测试");
            // 必须先赋值TestFunction！因为线程里要调用TestFunction(ctx)，
            // 如果没设置就是null，调用null()会抛NullReferenceException。
            // 这里提前检查给出明确错误信息。

            IsRunning = true;

            // 创建并启动4个测试线程
            for (int i = 0; i < MAX_MODULES; i++)
            {
                int slotIndex = i;  // ★ 闭包陷阱！必须用局部变量捕获循环变量i。
                                    // 每个迭代创建一个新的slotIndex变量，
                                    // Lambda捕获的是这个独立变量，值不会随循环变化。

                _threads[i] = new Thread(() => RunSingleTest(slotIndex))
                // () => RunSingleTest(slotIndex) 是Lambda表达式，
                // 作为ThreadStart委托传给Thread构造函数。
                // 线程启动后会执行 RunSingleTest(slotIndex)。
                {
                    Name = "ModuleTestThread_" + (i + 1),
                    // 线程命名：ModuleTestThread_1 ~ ModuleTestThread_4
                    // 在Visual Studio的"调试→窗口→线程"窗口中可以看到线程名字，
                    // 方便调试时识别哪个线程对应哪个模块。

                    IsBackground = true
                    // 设为后台线程：当所有前台线程（主要是UI线程）结束时，
                    // 后台线程会自动被CLR终止，不会出现"关了窗口进程还在"的问题。
                    // 测试线程不需要独立于UI存活，设为Background是正确的。
                };
                _threads[i].Start(); // 启动线程！操作系统调度RunSingleTest开始执行。
            }

            // 启动独立的监控线程，等待所有4个测试线程结束后触发AllTestsCompleted回调。
            // 为什么不直接在StartAllTests里Join？
            //   因为StartAllTests在UI线程调用，Join()会阻塞UI线程导致界面卡死。
            //   所以另开一个后台线程做等待，不阻塞UI。
            Thread monitorThread = new Thread(WaitForAllComplete)
            {
                Name = "TestMonitorThread",
                IsBackground = true
            };
            monitorThread.Start();
        }

        //=======================================================================
        // RunSingleTest —— 单个模块的测试线程入口函数
        //
        // 这是每个测试线程实际执行的方法。流程：
        //   1. 获取该模块的TestContext
        //   2. Activate()设置[ThreadStatic]当前上下文（让TestContext.Current可用）
        //   3. 记录开始时间、状态
        //   4. try块：通知UI进度0% → 调用TestFunction执行具体测试 → 记录结果
        //   5. catch块：异常处理，记录错误信息
        //   6. finally块：记录结束时间，通知UI进度100%，触发单模块完成回调
        //
        // 注意：这个方法本身完全不关心TestFunction里做了什么
        // （SFP测试/QSFP测试/读寄存器/发光...），它只管通用的"前-后"处理：
        // 时间记录、状态设置、异常捕获、进度通知。这就是框架代码的特点。
        //=======================================================================

        private void RunSingleTest(int index)
        {
            TestContext ctx = Contexts[index]; // 取该模块的上下文对象引用
            ctx.Activate();
            // ★ 关键调用！设置ThreadStatic的_current字段：
            //   TestContext._current = this; // (即当前ctx)
            // _current有[ThreadStatic]特性，每个线程有独立副本。
            // 激活后，在测试函数的任何地方都可以通过TestContext.Current
            // 拿到当前线程（当前模块）的ctx，无需把ctx作为参数层层传递。
            //
            // 类比：相当于给线程发了一张"工牌"，戴上之后走到哪里都知道自己是谁。

            ctx.StartTime = DateTime.Now;      // 记录测试开始时间（用于算耗时）
            ctx.StatusText = "正在测试...";    // 更新状态文本（UI会显示）

            try
            {
                ProgressUpdate?.Invoke(ctx.Slot, 0, "开始测试...");
                // ?.Invoke是C# 6空条件运算符：
                // 如果ProgressUpdate不为null（外部订阅了回调），就调用它；
                // 如果为null（没人关心进度），什么都不做，不会抛NullReferenceException。
                // 通知UI：这个模块进度0%，显示"开始测试..."

                bool success = TestFunction(ctx);
                // ★ 调用外部注入的具体测试函数！
                // 这一行才是真正执行测试的地方。
                // 可能是SFP初测/终测、QSFP测试等，由外部设置决定。
                // TestFunction返回true=通过，false=失败。
                // 函数执行期间可以随时调用ProgressUpdate更新进度。

                ctx.TestSuccess = success; // 记录测试结果
                ctx.StatusText = success ? "测试通过" : "测试失败"; // 更新最终状态文本
            }
            catch (Exception ex)
            {
                // 测试函数抛出未处理异常 → 测试失败
                ctx.TestSuccess = false;
                ctx.ErrorMessage = ex.Message; // 保存异常消息（UI显示给用户）
                ctx.StatusText = "异常: " + ex.Message;
                // 注意：这里没有重抛异常，所以异常不会导致进程崩溃，
                // 线程会正常进入finally块完成清理。
            }
            finally
            {
                // 无论成功/失败/异常，finally块一定执行
                ctx.EndTime = DateTime.Now; // 记录结束时间（ctx.Duration可以算耗时）
                ProgressUpdate?.Invoke(ctx.Slot, 100, ctx.StatusText); // 通知UI进度100%
                ModuleTestCompleted?.Invoke(ctx); // 通知UI：这个模块测完了
            }
        }

        //=======================================================================
        // WaitForAllComplete —— 监控线程入口，等待所有线程完成
        //
        // 在独立的监控线程上执行（由StartAllTests启动），做的事很简单：
        //   1. 依次对4个测试线程调用Join()（阻塞等待每个线程结束）
        //   2. 全部结束后设IsRunning=false
        //   3. 触发AllTestsCompleted回调通知UI所有模块都完成了
        //
        // Thread.Join()：阻塞当前线程直到目标线程执行完毕。
        // 依次Join 4个线程，相当于Task.WhenAll(allThreads)的效果。
        // 即使某个线程已经结束了，Join()也会立即返回，不会阻塞。
        //
        // 这个方法在监控线程上运行，所以阻塞Join不会影响UI线程。
        //=======================================================================

        private void WaitForAllComplete()
        {
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (_threads[i] != null && _threads[i].IsAlive)
                // IsAlive检查：线程还活着（没结束）才需要Join
                // 如果线程已经完成了（IsAlive=false），Join会立即返回
                {
                    _threads[i].Join(); // 阻塞等待该线程结束
                }
            }

            IsRunning = false; // 全部完成，重置运行标志
            AllTestsCompleted?.Invoke(Contexts);
            // 通知UI所有4个模块都完成了，传入完整的结果数组。
            // UI可以在这里：弹出结果对话框、保存数据库、播放提示音、允许再次点击开始等。
        }

        //=======================================================================
        // GetStatusSummary —— 获取所有模块的测试状态摘要文本
        //=======================================================================

        /// <summary>
        /// 生成人类可读的测试状态摘要字符串，用于日志记录或MessageBox显示。
        /// 格式示例：
        ///   [模块1] 测试通过 (25.3s)
        ///   [模块2] 测试通过 (28.1s)
        ///   [模块3] 测试失败: I2C通信失败 (5.2s)
        ///   [模块4] 测试通过 (26.8s)
        ///
        /// 实现细节：
        /// - Duration.TotalSeconds > 0 才显示耗时（未开始的模块显示0秒没意义）
        /// - F1格式：保留1位小数（如25.3s）
        /// - string.Join("\n", lines)：用换行符连接每行
        /// </summary>
        public string GetStatusSummary()
        {
            var lines = new List<string>();
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (Contexts[i] == null) continue; // 跳过未初始化的槽位
                var ctx = Contexts[i];
                lines.Add($"[模块{ctx.Slot}] {ctx.StatusText}" +
                    (ctx.Duration.TotalSeconds > 0 ? $" ({ctx.Duration.TotalSeconds:F1}s)" : ""));
                // ctx.Duration是TestContext的属性：EndTime - StartTime（TimeSpan类型）
            }
            return string.Join("\n", lines.ToArray());
        }

        //=======================================================================
        // IsAllDone —— 非阻塞查询是否所有模块都已完成
        //=======================================================================

        /// <summary>
        /// 检查是否所有测试都已完成（非阻塞，即时查询）。
        ///
        /// 和WaitForAllComplete的区别：
        /// - WaitForAllComplete()：阻塞等待（Join），用于监控线程
        /// - IsAllDone()：立即返回true/false，UI可以用Timer轮询这个方法
        ///   来判断是否全部完成，不需要订阅AllTestsCompleted回调。
        ///
        /// 逻辑：
        ///   1. 如果IsRunning=false（根本没开始测试），返回true
        ///   2. 遍历4个线程，只要有一个还在IsAlive，返回false
        ///   3. 全部不存活了才返回true
        /// </summary>
        public bool IsAllDone()
        {
            if (!IsRunning) return true;
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (_threads[i] != null && _threads[i].IsAlive)
                    return false; // 还有线程在跑
            }
            return true; // 所有线程都结束了
        }

        //=======================================================================
        // AbortAll —— 强制中止所有测试线程
        //
        // ⚠️ 注意：Thread.Abort()在.NET Framework中已标记为过时(Obsolete)，
        // 在.NET Core/.NET 5+中甚至不存在此方法。
        // 原因：Abort()会在线程任意位置抛出ThreadAbortException，
        //   可能导致：资源泄漏（lock没释放、文件句柄没关）、数据不一致
        //   （写了一半的数据被中断）、死锁（线程持锁被杀死其他线程永远等不到锁）。
        //
        // 正确做法：使用协作式取消（CancellationToken或类似ChannelTester的
        // _stopRequested标志位），让线程自己检查并优雅退出。
        // 本方法保留是因为遗留代码兼容性，但不推荐使用。
        //=======================================================================

        /// <summary>
        /// 强制终止所有测试线程（⚠️ Abort已过时，可能导致资源泄漏，请谨慎使用）。
        /// 建议使用协作式取消模式替代。
        /// </summary>
        public void AbortAll()
        {
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (_threads[i] != null && _threads[i].IsAlive)
                {
                    _threads[i].Abort();
                    // Thread.Abort()在目标线程中注入ThreadAbortException，
                    // 强制线程终止。这是粗暴的方式，可能导致：
                    // - lock未释放→其他线程永远等不到锁→死锁
                    // - 文件/串口/网络连接未关闭→资源泄漏
                    // - 正在写的数据被打断→数据损坏
                    // 代码注释也标注了"注意：Abort已过时"
                }
            }
            IsRunning = false;
        }
    }
}