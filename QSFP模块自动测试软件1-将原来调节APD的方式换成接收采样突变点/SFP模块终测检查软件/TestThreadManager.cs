using System;
using System.Collections.Generic;
using System.Threading;

namespace FibertopTest_Common
{
    /// <summary>
    /// 4模块并行测试线程管理器
    /// 
    /// 【这个类做什么？】
    /// 管理4个测试线程的创建、启动、等待、状态监控。
    /// 每个线程测试一个独立的模块，4个模块同时测试。
    /// 
    /// 【工作流程】
    /// 1. 用户点击"开始测试"按钮
    /// 2. TestThreadManager 创建4个 TestContext（每个模块一个）
    /// 3. 为每个 TestContext 创建一个线程，运行测试函数
    /// 4. 4个线程同时运行，互不干扰
    /// 5. 等待所有4个线程完成后，汇总结果
    /// </summary>
    public class TestThreadManager
    {
        /// <summary>最大并行测试模块数量</summary>
        public const int MAX_MODULES = 4;

        /// <summary>4个模块的测试上下文</summary>
        public TestContext[] Contexts { get; private set; } = new TestContext[MAX_MODULES];

        /// <summary>4个测试线程</summary>
        private Thread[] _threads = new Thread[MAX_MODULES];

        /// <summary>测试完成的回调函数</summary>
        public Action<TestContext[]> AllTestsCompleted { get; set; }

        /// <summary>单个模块测试完成时的回调</summary>
        public Action<TestContext> ModuleTestCompleted { get; set; }

        /// <summary>进度更新回调（slot, progress, status）</summary>
        public Action<int, int, string> ProgressUpdate { get; set; }

        /// <summary>是否正在测试中</summary>
        public bool IsRunning { get; private set; } = false;

        /// <summary>
        /// 每个模块线程执行的测试函数委托
        /// 参数是 TestContext，返回值是是否成功
        /// </summary>
        public Func<TestContext, bool> TestFunction { get; set; }

        // ============================================================
        // 初始化
        // ============================================================

        /// <summary>
        /// 初始化4个测试上下文
        /// </summary>
        /// <param name="firstTest">true=初测调试, false=终测检查</param>
        public void InitializeContexts(bool firstTest)
        {
            for (int i = 0; i < MAX_MODULES; i++)
            {
                Contexts[i] = new TestContext
                {
                    Slot = i + 1,           // 槽位1~4
                    IsFirstTest = firstTest,
                    TestSuccess = false,
                    StatusText = "等待开始...",
                    Progress = 0,
                    ErrorMessage = ""
                };
            }
        }

        // ============================================================
        // 启动并行测试
        // ============================================================

        /// <summary>
        /// 启动4个模块的并行测试
        /// </summary>
        public void StartAllTests()
        {
            if (IsRunning) return;
            if (TestFunction == null)
                throw new InvalidOperationException("TestFunction 未设置，无法启动测试");

            IsRunning = true;

            for (int i = 0; i < MAX_MODULES; i++)
            {
                int slotIndex = i;  // 闭包捕获用

                _threads[i] = new Thread(() => RunSingleTest(slotIndex))
                {
                    Name = "ModuleTestThread_" + (i + 1),
                    IsBackground = true
                };
                _threads[i].Start();
            }

            // 启动一个监控线程，等待所有测试完成
            Thread monitorThread = new Thread(WaitForAllComplete)
            {
                Name = "TestMonitorThread",
                IsBackground = true
            };
            monitorThread.Start();
        }

        /// <summary>
        /// 单个模块的测试线程入口
        /// </summary>
        private void RunSingleTest(int index)
        {
            TestContext ctx = Contexts[index];
            ctx.Activate();  // 设置为当前线程的活动上下文
            ctx.StartTime = DateTime.Now;
            ctx.StatusText = "正在测试...";

            try
            {
                ProgressUpdate?.Invoke(ctx.Slot, 0, "开始测试...");

                // 调用实际的测试函数（FirstTestProcess 或 FinalTestProcess）
                bool success = TestFunction(ctx);

                ctx.TestSuccess = success;
                ctx.StatusText = success ? "测试通过" : "测试失败";
            }
            catch (Exception ex)
            {
                ctx.TestSuccess = false;
                ctx.ErrorMessage = ex.Message;
                ctx.StatusText = "异常: " + ex.Message;
            }
            finally
            {
                ctx.EndTime = DateTime.Now;
                ProgressUpdate?.Invoke(ctx.Slot, 100, ctx.StatusText);
                ModuleTestCompleted?.Invoke(ctx);
            }
        }

        /// <summary>
        /// 等待所有4个线程完成
        /// </summary>
        private void WaitForAllComplete()
        {
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (_threads[i] != null && _threads[i].IsAlive)
                {
                    _threads[i].Join();
                }
            }

            IsRunning = false;
            AllTestsCompleted?.Invoke(Contexts);
        }

        // ============================================================
        // 状态查询
        // ============================================================

        /// <summary>
        /// 获取所有模块的测试状态摘要
        /// </summary>
        public string GetStatusSummary()
        {
            var lines = new List<string>();
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (Contexts[i] == null) continue;
                var ctx = Contexts[i];
                lines.Add($"[模块{ctx.Slot}] {ctx.StatusText}" +
                    (ctx.Duration.TotalSeconds > 0 ? $" ({ctx.Duration.TotalSeconds:F1}s)" : ""));
            }
            return string.Join("\n", lines);
        }

        /// <summary>
        /// 检查是否所有测试都已完成
        /// </summary>
        public bool IsAllDone()
        {
            if (!IsRunning) return true;
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (_threads[i] != null && _threads[i].IsAlive)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 强制中止所有测试线程
        /// </summary>
        public void AbortAll()
        {
            for (int i = 0; i < MAX_MODULES; i++)
            {
                if (_threads[i] != null && _threads[i].IsAlive)
                {
                    _threads[i].Abort();  // 注意：Abort已过时，但这里简单处理
                }
            }
            IsRunning = false;
        }
    }
}