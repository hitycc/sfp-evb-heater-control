using System;
using System.Threading;

namespace LastEVBControlDemoApp
{
    class Program
    {
        // ========== 测试参数配置 ==========
        const string Slot = "09";                       // VOA槽位号（接收端衰减）
        const int Ch = 1;                               // VOA通道
        const string DeviceIp = "192.168.100.156";      // 设备IP
        const int DevicePort = 5024;                    // SCPI端口
        const int StabilizeMs = 2000;                   // 功率稳定等待时间(ms)
        static readonly double[] TargetPowers = { -8, -22, -30 };  // 目标功率列表(dBm)

        // ========== 光开关配置（加热台1号通道 → 接收路径）==========
        // 供应商说明：测接收前必须先切光开关到接收，否则模块Tx反向进光导致VOA异常
        // Rx接收方向：光源/VOA(inCh=2) → 模块Rx(outCh=1)  槽位11
        const string SwitchSlot = "11";
        const int RxSwitchInCh = 2;
        const int RxSwitchOutCh = 1;
        // 测试结束后切回Tx发射：模块Tx(inCh=1) → 仪器(outCh=2)
        const int TxSwitchInCh = 1;
        const int TxSwitchOutCh = 2;

        static void Main(string[] args)
        {
            Console.WriteLine("============================================");
            Console.WriteLine("  VOA接收端光功率/衰减值 测试程序");
            Console.WriteLine("  VOA槽位: {0}  通道: {1}", Slot, Ch);
            Console.WriteLine("  光开关槽位: {0}  接收路径: in{1}→out{2}", SwitchSlot, RxSwitchInCh, RxSwitchOutCh);
            Console.WriteLine("  目标功率: {0} dBm", string.Join(", ", TargetPowers));
            Console.WriteLine("============================================");
            Console.WriteLine();
            Console.WriteLine("[提示] 请先确认光模块已插入加热台，外部光源正常。");
            Console.WriteLine("按任意键开始测试...");
            Console.ReadKey(true);
            Console.WriteLine();

            OTP12Driver drv = new OTP12Driver();
            try
            {
                // ------ 1. 连接设备 ------
                Console.WriteLine("[1] 连接设备 {0}:{1} ...", DeviceIp, DevicePort);
                if (!drv.Connect(DeviceIp, DevicePort))
                {
                    Console.WriteLine("错误：连接失败！请检查IP/网络。");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("  -> 连接成功。");
                string idn = drv.QueryDeviceInfo();
                Console.WriteLine("  -> 设备信息: {0}", idn ?? "无响应");
                Console.WriteLine();

                // ------ 2. 先切光开关到接收路径（关键！防止模块Tx反向进光）------
                Console.WriteLine("[2] 切换光开关到接收路径（槽位{0}, in{1}→out{2}）...", SwitchSlot, RxSwitchInCh, RxSwitchOutCh);
                drv.SetSlot(SwitchSlot);
                bool swOk = drv.SW_SetChannel(RxSwitchInCh, RxSwitchOutCh);
                Console.WriteLine("  -> 光开关切换: {0}", swOk ? "OK" : "FAIL");
                // 切回VOA槽位
                drv.SetSlot(Slot);
                Thread.Sleep(500); // 等待光开关切换稳定
                Console.WriteLine();

                // ------ 3. 配置VOA通道 ------
                Console.WriteLine("[3] 配置VOA通道{0} ...", Ch);
                bool ok;
                ok = drv.VOA_SetMode(Ch, "POWer");
                Console.WriteLine("  -> 功率控制模式(POWer): {0}", ok ? "OK" : "FAIL");
               /* ok = drv.VOA_SetAlcState(Ch, "ON");
                Console.WriteLine("  -> ALC自动功率跟踪(ON): {0}", ok ? "OK" : "FAIL");
                ok = drv.VOA_SetApMode(Ch, "ABSolute");
                Console.WriteLine("  -> 绝对功率模式(ABSolute): {0}", ok ? "OK" : "FAIL");*/
                ok = drv.VOA_SetOutputState(Ch, "ON");
                Console.WriteLine("  -> 输出光路(ON): {0}", ok ? "OK" : "FAIL");
                Console.WriteLine();

                // ------ 4. 逐点测试 ------
                Console.WriteLine("[4] 开始逐点测试...");
                Console.WriteLine();
                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine("| 目标功率(dBm) | 实际功率(dBm) | 衰减值(dB)  | 输入功率(dBm) |");
                Console.WriteLine("+----------------------------------------------------------------+");

                foreach (double target in TargetPowers)
                {
                    Console.WriteLine("  设置目标功率: {0} dBm ...", target);
                    bool setOk = drv.VOA_SetOutPower(Ch, target);
                    if (!setOk)
                    {
                        Console.WriteLine("  -> 设置失败，跳过该点。");
                        Console.WriteLine("|{0,10}     |{1,12}     |{2,10}     |{3,11}     |",
                            target, "ERR", "ERR", "ERR");
                        continue;
                    }

                    Console.WriteLine("  等待 {0}ms 功率稳定...", StabilizeMs);
                    Thread.Sleep(StabilizeMs);

                    string outPwrStr = drv.VOA_GetOutputPower(Ch);   // 实际输出功率
                    string inPwrStr  = drv.VOA_GetInputPower(Ch);     // 输入功率

                    // 计算实际衰减 = 输入功率 - 输出功率（与Web面板一致）
                    // POWer模式下INPut:ATT?返回的是设定值而非ALC实时衰减，所以用功率差计算
                    string attStr = CalcAttenuation(inPwrStr, outPwrStr);

                    Console.WriteLine("|{0,10}     |{1,12}     |{2,10}     |{3,11}     |",
                        target, Fmt(outPwrStr), attStr, Fmt(inPwrStr));
                }

                Console.WriteLine("+----------------------------------------------------------------+");
                Console.WriteLine();

/*                // ------ 5. 收尾 ------
                Console.WriteLine("[5] 测试完成，关闭VOA输出...");
                drv.SetSlot(Slot);
                drv.VOA_SetOutputState(Ch, "OFF");
                Console.WriteLine("  -> VOA输出已关闭。");*/

                // 切回光开关发射路径（方便后续测发射端）
                Console.WriteLine("  切回光开关发射路径（槽位{0}, in{1}→out{2}）...", SwitchSlot, TxSwitchInCh, TxSwitchOutCh);
                drv.SetSlot(SwitchSlot);
                bool swBack = drv.SW_SetChannel(TxSwitchInCh, TxSwitchOutCh);
                Console.WriteLine("  -> 光开关切回: {0}", swBack ? "OK" : "FAIL");
            }
            catch (Exception ex)
            {
                Console.WriteLine("异常: " + ex.Message);
            }
            finally
            {
                drv.DisConnect();
                Console.WriteLine("  -> 已断开连接。");
            }

            Console.WriteLine();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// 根据输入功率和输出功率计算实际衰减值 (dB)
        /// 衰减 = 输入功率(dBm) - 输出功率(dBm)
        /// </summary>
        static string CalcAttenuation(string inPwrRaw, string outPwrRaw)
        {
            double inPwr = ParseDbm(inPwrRaw);
            double outPwr = ParseDbm(outPwrRaw);
            if (double.IsNaN(inPwr) || double.IsNaN(outPwr))
                return "N/A";
            double att = inPwr - outPwr;
            return att.ToString("F3");
        }

        /// <summary>
        /// 解析SCPI返回的dBm数值字符串（支持科学计数、带单位格式）
        /// </summary>
        static double ParseDbm(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return double.NaN;
            string s = raw.Trim();
            string[] parts = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string num = parts[0];
            if (double.TryParse(num,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double v))
            {
                return v;
            }
            return double.NaN;
        }

        /// <summary>
        /// 将SCPI返回的数值字符串（可能带科学计数、可能带单位）格式化为3位小数字符串
        /// </summary>
        static string Fmt(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "N/A";
            string s = raw.Trim();
            string[] parts = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string num = parts[0];
            if (double.TryParse(num,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out double v))
            {
                return v.ToString("F3");
            }
            return s;
        }
    }
}