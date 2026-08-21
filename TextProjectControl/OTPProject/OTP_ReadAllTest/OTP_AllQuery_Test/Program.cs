using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using LastEVBControlDemoApp;

namespace LastEVBControlDemoApp
{
    class Program
    {
        private static OTP12Driver _otp12;
        private static readonly object _consoleLock = new object();

        // 4个模块的TX路由配置: (slot板号, inCh, outCh, 模块名)
        // 来自 SW_SetRouteForModule: 模块1/3→IN1→OUT2, 模块2/4→IN3→OUT4
        private static readonly (string slot, int inCh, int outCh, string name)[] TxRoutes = new[]
        {
            ("11", 1, 2, "模块1 TX (SLOT11: IN1→OUT2)"),
            ("11", 3, 4, "模块2 TX (SLOT11: IN3→OUT4)"),
            ("12", 1, 2, "模块3 TX (SLOT12: IN1→OUT2)"),
            ("12", 3, 4, "模块4 TX (SLOT12: IN3→OUT4)"),
        };

        // RX路由配置(用于"关闭"TX): 模块1/3→IN2→OUT1, 模块2/4→IN4→OUT3
        private static readonly (string slot, int inCh, int outCh, string name)[] RxRoutes = new[]
        {
            ("11", 2, 1, "模块1 RX (SLOT11: IN2→OUT1)"),
            ("11", 4, 3, "模块2 RX (SLOT11: IN4→OUT3)"),
            ("12", 2, 1, "模块3 RX (SLOT12: IN2→OUT1)"),
            ("12", 4, 3, "模块4 RX (SLOT12: IN4→OUT3)"),
        };

        static void Main(string[] args)
        {
            Console.Title = "OTP-12 光开关并发切换测试";
            Console.WriteLine("====== OTP-12 光开关并发打开测试 ======");
            Console.WriteLine("测试目的: 验证同时打开4个发射(TX)光开关时，供应商网页是否显示同步切换");
            Console.WriteLine("TX路由: 模块1/3 IN1→OUT2, 模块2/4 IN3→OUT4");
            Console.WriteLine("RX路由: 模块1/3 IN2→OUT1, 模块2/4 IN4→OUT3");
            Console.WriteLine();

            _otp12 = new OTP12Driver();

            Console.Write($"请输入OTP-12设备IP地址 (直接回车使用默认 {_otp12.DefaultIp}): ");
            string ip = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(ip)) ip = _otp12.DefaultIp;

            Console.WriteLine($"正在连接OTP-12 {ip}:{_otp12.DefaultPort} ...");
            if (!_otp12.Connect(ip))
            {
                Console.WriteLine("OTP-12连接失败！请检查IP地址和网络连接。");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("OTP-12连接成功！");
            Console.WriteLine();

            try
            {
                while (true)
                {
                    Console.WriteLine("====== 测试菜单 ======");
                    Console.WriteLine("1. 查询所有光开关当前状态");
                    Console.WriteLine("2. 先关闭所有TX(切RX) → 并发打开4个TX (推荐测试)");
                    Console.WriteLine("3. 直接并发打开4个TX开关");
                    Console.WriteLine("4. 顺序打开4个TX开关(对比基准)");
                    Console.WriteLine("5. 关闭所有TX(切到RX方向)");
                    Console.WriteLine("0. 退出");
                    Console.Write("请选择操作: ");

                    string choice = Console.ReadLine()?.Trim();
                    Console.WriteLine();

                    switch (choice)
                    {
                        case "1":
                            QueryAllSwitchStates();
                            break;
                        case "2":
                            TestConcurrentOpenWithReset();
                            break;
                        case "3":
                            TestConcurrentOpen();
                            break;
                        case "4":
                            TestSequentialOpen();
                            break;
                        case "5":
                            CloseAllTxSwitches();
                            break;
                        case "0":
                            _otp12.DisConnect();
                            Console.WriteLine("已断开连接。");
                            return;
                        default:
                            Console.WriteLine("无效选择，请重新输入。");
                            break;
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"异常: {ex.Message}");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                _otp12?.DisConnect();
            }
        }

        #region 底层操作

        /// <summary>
        /// 查询指定slot板卡上指定输入通道的当前连接输出通道
        /// </summary>
        private static string QuerySwitchChannel(string slot, int inCh)
        {
            string resp = _otp12.SendScpiToSlot(slot, $":ROUTe{inCh}:SCAN?");
            return resp?.Trim();
        }

        /// <summary>
        /// 设置单路开关路由 (SCPI: :ROUTe{inCh}:SCAN {outCh})
        /// </summary>
        private static bool SetSwitchRoute(string slot, int inCh, int outCh)
        {
            string res = _otp12.SendScpiToSlot(slot, $":ROUTe{inCh}:SCAN {outCh}");
            return res != null && res.Contains("Command execute successfully");
        }

        #endregion

        #region 状态查询

        /// <summary>
        /// 查询并打印SLOT11/SLOT12上所有通道的开关状态
        /// </summary>
        private static void QueryAllSwitchStates()
        {
            Console.WriteLine("====== 当前光开关状态 ======");

            var channelLabels = new Dictionary<int, string>
            {
                {1, "模块1 TX"},
                {2, "模块1 RX"},
                {3, "模块2 TX"},
                {4, "模块2 RX"},
            };

            // SLOT11: 模块1和模块2
            foreach (int ch in new[] { 1, 2, 3, 4 })
            {
                string state = QuerySwitchChannel("11", ch);
                string label = channelLabels[ch];
                bool isTx = (ch == 1 || ch == 3);
                int expectedOut = isTx ? (ch == 1 ? 2 : 4) : (ch == 2 ? 1 : 3);
                string mark = (state == expectedOut.ToString()) ? "✓" : " ";
                Console.WriteLine($"  {mark} SLOT11 IN{ch} → OUT{state ?? "null"}  ({label})");
                Thread.Sleep(30);
            }

            var channelLabels12 = new Dictionary<int, string>
            {
                {1, "模块3 TX"},
                {2, "模块3 RX"},
                {3, "模块4 TX"},
                {4, "模块4 RX"},
            };

            // SLOT12: 模块3和模块4
            foreach (int ch in new[] { 1, 2, 3, 4 })
            {
                string state = QuerySwitchChannel("12", ch);
                string label = channelLabels12[ch];
                bool isTx = (ch == 1 || ch == 3);
                int expectedOut = isTx ? (ch == 1 ? 2 : 4) : (ch == 2 ? 1 : 3);
                string mark = (state == expectedOut.ToString()) ? "✓" : " ";
                Console.WriteLine($"  {mark} SLOT12 IN{ch} → OUT{state ?? "null"}  ({label})");
                Thread.Sleep(30);
            }

            Console.WriteLine();

            // TX总状态
            int txOpenCount = 0;
            Console.WriteLine("  TX开关状态汇总:");
            foreach (var route in TxRoutes)
            {
                string state = QuerySwitchChannel(route.slot, route.inCh);
                bool isOpen = (state == route.outCh.ToString());
                if (isOpen) txOpenCount++;
                Console.WriteLine($"    {(isOpen ? "✓" : "✗")} {route.name}: OUT{state ?? "null"}");
                Thread.Sleep(30);
            }
            Console.WriteLine($"  TX打开: {txOpenCount}/4");
        }

        #endregion

        #region 开关操作

        /// <summary>
        /// 关闭所有TX开关(切到RX方向)
        /// </summary>
        private static void CloseAllTxSwitches()
        {
            Console.WriteLine("====== 关闭所有TX开关(切换到RX方向) ======");

            foreach (var route in RxRoutes)
            {
                Console.Write($"  设置 {route.name} ... ");
                bool ok = SetSwitchRoute(route.slot, route.inCh, route.outCh);
                Console.WriteLine(ok ? "OK" : "FAIL");
                Thread.Sleep(100);
            }

            Console.WriteLine("RX方向切换完成。");
            Thread.Sleep(200);
            QueryAllSwitchStates();
        }

        /// <summary>
        /// 使用4线程并发打开4个TX开关，返回精确计时结果
        /// 注意: SendScpiToSlot内部有lock，多线程并发调用时命令会串行发送，
        /// 但4个线程几乎同时进入lock等待队列，命令发送间隔最小化。
        /// </summary>
        private static List<(string name, double startMs, double endMs, bool success)> ConcurrentOpenTxSwitches()
        {
            var results = new List<(string name, double startMs, double endMs, bool success)>();
            var resultsLock = new object();
            var countdown = new CountdownEvent(TxRoutes.Length);
            var sw = Stopwatch.StartNew();

            var threads = new Thread[TxRoutes.Length];
            for (int i = 0; i < TxRoutes.Length; i++)
            {
                var route = TxRoutes[i];
                int moduleNum = i + 1; // 1,2,3,4

                threads[i] = new Thread(() =>
                {
                    double startMs = sw.Elapsed.TotalMilliseconds;
                    bool ok = false;
                    try
                    {
                        ok = _otp12.SW_SetRouteForModule(moduleNum, isTxTest: true);
                    }
                    catch (Exception ex)
                    {
                        lock (_consoleLock)
                        {
                            Console.WriteLine($"  [异常] {route.name}: {ex.Message}");
                        }
                    }
                    double endMs = sw.Elapsed.TotalMilliseconds;

                    lock (resultsLock)
                    {
                        results.Add((route.name, startMs, endMs, ok));
                    }
                    countdown.Signal();
                });
            }

            // 几乎同时启动所有4个线程
            foreach (var t in threads) t.Start();

            // 给线程一点点时间全部启动并进入lock等待
            Thread.Sleep(5);
            double dispatchMs = sw.Elapsed.TotalMilliseconds;

            countdown.Wait();
            sw.Stop();

            // ========== 输出时间分析 ==========
            Console.WriteLine();
            Console.WriteLine($"====== 并发切换时间分析 =====");
            Console.WriteLine($"  线程全部启动时刻: {dispatchMs:F3} ms");
            Console.WriteLine();

            var sorted = results.OrderBy(r => r.startMs).ToList();
            foreach (var r in sorted)
            {
                Console.WriteLine($"  {r.name,-35} 开始={r.startMs,8:F3}ms  完成={r.endMs,8:F3}ms  耗时={r.endMs - r.startMs,6:F1}ms  {(r.success ? "OK" : "FAIL")}");
            }

            if (sorted.Count > 0)
            {
                double firstStart = sorted.Min(r => r.startMs);
                double lastEnd = sorted.Max(r => r.endMs);
                double firstEnd = sorted.Min(r => r.endMs);

                Console.WriteLine();
                Console.WriteLine($"  ★ 第一条命令开始到最后一条完成 总耗时: {(lastEnd - firstStart):F1} ms");
                Console.WriteLine($"  ★ 第一条完成到最后一条完成 时间差:   {(lastEnd - firstEnd):F1} ms");
                Console.WriteLine($"    (此时间差即为供应商网页上4个开关状态变化的最大间隔)");
                Console.WriteLine($"    (如果此值<100ms，人眼基本感知不到差异，看起来是同时的)");
            }

            return results;
        }

        #endregion

        #region 测试用例

        /// <summary>
        /// 测试2: 先关闭所有TX(切RX) → 并发打开4个TX
        /// </summary>
        private static void TestConcurrentOpenWithReset()
        {
            Console.WriteLine("====== 测试: 先关闭所有TX(切RX) → 并发打开4个TX ======");
            Console.WriteLine();

            // 1. 先切到RX
            CloseAllTxSwitches();
            Console.WriteLine();

            Console.WriteLine(">>> 请在供应商网页上观察光开关状态，准备好后按回车开始并发切换 <<<");
            Console.WriteLine("    (重点观察: 4个TX开关是否同时从关(RX侧)变开(TX侧))");
            Console.ReadLine();

            // 2. 并发打开
            var swTotal = Stopwatch.StartNew();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ▶ 开始并发打开4个TX开关!");
            Console.WriteLine();

            ConcurrentOpenTxSwitches();

            swTotal.Stop();
            Console.WriteLine();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ■ 所有切换命令已完成，总耗时: {swTotal.ElapsedMilliseconds} ms");
            Console.WriteLine();

            // 3. 查询最终状态
            Thread.Sleep(500);
            Console.WriteLine("====== 切换后光开关状态 ======");
            QueryAllSwitchStates();
        }

        /// <summary>
        /// 测试3: 直接并发打开4个TX(不先关闭)
        /// </summary>
        private static void TestConcurrentOpen()
        {
            Console.WriteLine("====== 测试: 直接并发打开4个TX开关 ======");
            Console.WriteLine(">>> 请在供应商网页上观察，按回车开始 <<<");
            Console.ReadLine();

            var swTotal = Stopwatch.StartNew();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ▶ 开始并发打开4个TX开关!");

            ConcurrentOpenTxSwitches();

            swTotal.Stop();
            Console.WriteLine();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ■ 所有切换命令已完成，总耗时: {swTotal.ElapsedMilliseconds} ms");

            Thread.Sleep(500);
            QueryAllSwitchStates();
        }

        /// <summary>
        /// 测试4: 顺序打开4个TX(对比基准)
        /// </summary>
        private static void TestSequentialOpen()
        {
            Console.WriteLine("====== 测试: 顺序打开4个TX开关(对比基准) ======");
            Console.WriteLine(">>> 请在供应商网页上观察，按回车开始 <<<");
            Console.ReadLine();

            var sw = Stopwatch.StartNew();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ▶ 开始顺序打开4个TX开关...");
            Console.WriteLine();

            for (int module = 1; module <= 4; module++)
            {
                double t1 = sw.Elapsed.TotalMilliseconds;
                Console.Write($"  [{t1,8:F1}ms] 模块{module} TX ... ");
                bool ok = _otp12.SW_SetRouteForModule(module, isTxTest: true);
                double t2 = sw.Elapsed.TotalMilliseconds;
                Console.WriteLine($"{(ok ? "OK" : "FAIL")} (耗时{t2 - t1:F1}ms)");
                Thread.Sleep(50);
            }

            sw.Stop();
            Console.WriteLine();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] ■ 顺序切换完成，总耗时: {sw.ElapsedMilliseconds} ms");
            Console.WriteLine("  (对比: 并发模式的总耗时应接近但略大于单步耗时，4个开关几乎同时动作)");

            Thread.Sleep(500);
            QueryAllSwitchStates();
        }

        #endregion
    }
}