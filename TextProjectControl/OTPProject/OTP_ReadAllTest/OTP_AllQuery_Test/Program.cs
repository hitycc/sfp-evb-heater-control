using System;
using System.Threading;

namespace OTP_AutoRead_Test
{
    class Program
    {
        static OTP12Driver driver = new OTP12Driver();
        static readonly string ip = "192.168.100.156";
        static readonly int port = 5024;
        // 单条指令间隔毫秒
        private const int CmdDelayMs = 80;
        // 模块切换大间隔
        private const int SectionDelayMs = 200;

        static void Main(string[] args)
        {
            Console.WriteLine("================ OTP全部只读自动测试(自动切换槽+防延时版) ================");
            Console.WriteLine($"设备地址：{ip}:{port}\n");

            // 打印整机槽位硬件对照表
            PrintSlotHardwareMap();

            // 连接设备
            bool connOk = driver.Connect(ip, port);
            if (!connOk)
            {
                Console.WriteLine("❌ 设备连接失败，终止测试");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("✅ 连接成功，开始自动测试\n");
            Thread.Sleep(SectionDelayMs);

            // 系统&公共命令固定01槽BE板
            driver.SetSlot("01");
            TestSection4_System();
            Thread.Sleep(SectionDelayMs);

            TestSection5_BoardCommon();
            Thread.Sleep(SectionDelayMs);

            // OPM 切换到05槽
            driver.SetSlot("05");
            TestSection6_OPM();
            Thread.Sleep(SectionDelayMs);

            // VOA 切换到07槽
            driver.SetSlot("07");
            TestSection7_VOA();
            Thread.Sleep(SectionDelayMs);

            // 光开关SW 切换到11槽
            driver.SetSlot("11");
            TestSection8_Switch();
            Thread.Sleep(SectionDelayMs);

            // ERM 切换到06槽
            driver.SetSlot("06");
            TestSection9_ERM();
            Thread.Sleep(SectionDelayMs);

            // 下面机箱无对应硬件
            driver.SetSlot("01");
            TestSection10_LAC();
            Thread.Sleep(SectionDelayMs);
            TestSection11_LAG();
            Thread.Sleep(SectionDelayMs);
            TestSection12_BERT();
            Thread.Sleep(SectionDelayMs);
            TestSection13_PCS_PCG();
            Thread.Sleep(SectionDelayMs);
            TestSection14_OPMT();
            Thread.Sleep(SectionDelayMs);
            TestSection15_Trigger();

            //===== 【修正位置】四路测试放到断开连接之前！=====
            //四路光模块自动循环测试（发射指标：功率+消光比+衰减值）
            string[] swSlotArr = { "11", "11", "12", "12" };
            int[] chArr = { 1, 2, 1, 2 };
            string[] voaSlotArr = { "07", "07", "08", "08" };

            Console.WriteLine("\n=============== 开始四路SFP模块自动测试 ===============");

            //循环4次，依次测试1~4号模块
            for (int index = 0; index < 4; index++)
            {
                int moduleNo = index + 1;
                string swSlot = swSlotArr[index];
                int channel = chArr[index];
                string voaSlot = voaSlotArr[index];

                Console.WriteLine($"\n---------- 正在测试第{moduleNo}号加热台模块 ----------");

                //① 切换光开关槽位，切换对应通道
                driver.SetSlot(swSlot);
                string ret = driver.SW_SetChannel(channel);
                Thread.Sleep(300);
                string realCh = driver.SW_GetCurrentChannel(1);
                Console.WriteLine($"已切换到【槽位{swSlot}】光开关，目标通道{channel}，当前实际通道：{realCh}");

                //② 切换到05槽，读取OPM光功率
                driver.SetSlot("05");
                string power = driver.OPM_ReadPower(1);
                if (power.Contains("-200"))
                    Console.WriteLine($"模块{moduleNo} 发射光功率：{power} 【警告：无光输入，检查光纤】");
                else
                    Console.WriteLine($"模块{moduleNo} 发射光功率：{power}");
                Thread.Sleep(100);

                //③ 切换到06槽，读取ERM消光比
                driver.SetSlot("06");
                string erData = driver.ERM_ReadERData(1);
                Console.WriteLine($"模块{moduleNo} 消光比数据：{erData}");
                Thread.Sleep(100);

                //④ 切换到对应VOA槽位，读取当前衰减值
                driver.SetSlot(voaSlot);
                string att = driver.VOA_GetAttenuation(1);
                Console.WriteLine($"模块{moduleNo} 当前光路衰减dB：{att}");
                Thread.Sleep(100);
            }

            Console.WriteLine("\n=============== 四路模块全部测试完毕 ===============");

            // 【最后才断开TCP连接】
            driver.DisConnect();
            Console.WriteLine("\n================ 全部模块测试完成 ================");

            Console.WriteLine("按任意键关闭窗口...");
            Console.ReadKey();
        }


        #region 新增：整机槽位硬件打印函数
        /// <summary>
        /// 开机打印12槽硬件对应关系，方便新人核对
        /// </summary>
        static void PrintSlotHardwareMap()
        {
            Console.WriteLine("==================== 整机12槽硬件配置对照表 ====================");
            Console.WriteLine("SLOT-01  → BE2-03  主控板（通道1系统通信）");
            Console.WriteLine("SLOT-02  → BE2-03  主控板（通道2系统通信）");
            Console.WriteLine("SLOT-03  → BE2-03  主控板（通道3系统通信）");
            Console.WriteLine("SLOT-04  → BE2-03  主控板（通道4系统通信）");
            Console.WriteLine("SLOT-05  → OPM-04  四路共用光功率计 | 读取发射光功率");
            Console.WriteLine("SLOT-06  → ERM-04  四路共用消光比板 | 读取消光比ER");
            Console.WriteLine("SLOT-07  → VOA-02  通道1/2 发射光路可调衰减");
            Console.WriteLine("SLOT-08  → VOA-02  通道3/4 发射光路可调衰减");
            Console.WriteLine("SLOT-09  → VOA-02  通道1/2 接收光路可调衰减");
            Console.WriteLine("SLOT-10  → VOA-02 通道3/4 接收光路可调衰减");
            Console.WriteLine("SLOT-11  → SWD2-02 光开关 | 控制1、2号模块光路切换");
            Console.WriteLine("SLOT-12  → SWD2-02 光开关 | 控制3、4号模块光路切换");
            Console.WriteLine("================================================================\n");
        }
        #endregion

        #region 4 系统级命令 Slot固定01
        static void TestSection4_System()
        {
            Console.WriteLine("========【4 系统级只读命令 | 槽位01 BE2】========");
            PrintResult("401 查询设备型号", driver.QueryDeviceInfo());
            PrintResult("402 查询全部槽位单板", driver.QueryAllBoardCatalog());
            PrintResult("403 查询系统日期", driver.GetSystemDate());
            PrintResult("404 查询系统时间", driver.GetSystemTime());
            PrintResult("405 查询SCPI版本", driver.GetScpiVersion());
            PrintResult("406 查询会话超时", driver.GetSessionTimeout());
            PrintResult("407 当前连接数量", driver.GetSessionCount());
            PrintResult("408 查询子架ID", driver.GetRackId());
            PrintResult("409 查询网1IP", driver.GetEthIp(1));
            PrintResult("410 查询当前告警", driver.QueryCurrentAlarm());
            PrintResult("411 读取槽1工作日志", driver.ReadBoardLog(1, "work", 500));
            PrintResult("412 查询槽1单板类型", driver.QueryBoardInfo(1, "TYPE"));
            PrintResult("413 读取数值格式", driver.GetDataFormat());
            PrintResult("414 自动升级状态", driver.GetAutoUpgradeState());
            Console.WriteLine();
        }
        #endregion

        #region 5 单板公共命令 Slot固定01
        static void TestSection5_BoardCommon()
        {
            Console.WriteLine("========【5 单板公共只读 | 槽位01 BE2】========");
            PrintResult("501 单板序列号", driver.QueryBoardSN());
            PrintResult("502 单板运行状态", driver.QueryBoardStatus());
            Console.WriteLine();
        }
        #endregion

        #region 6 OPM模块 Slot自动05
        static void TestSection6_OPM()
        {
            Console.WriteLine("========【6 OPM光功率模块 | 槽位05 OPM-04】========");
            PrintResult("601 通道1光功率", driver.OPM_ReadPower(1));
            PrintResult("602 平均采样次数", driver.OPM_GetAverCount(1));
            PrintResult("603 参考光功率", driver.OPM_GetRefPower(1));
            PrintResult("604 参考模式开关", driver.OPM_GetRefState(1));
            PrintResult("605 当前波长", driver.OPM_GetWaveLength(1));
            PrintResult("606 功率单位", driver.OPM_GetPowerUnit(1));
            Console.WriteLine();
        }
        #endregion

        #region 7 VOA衰减模块 Slot自动07
        static void TestSection7_VOA()
        {
            Console.WriteLine("========【7 VOA衰减模块 | 槽位07 VOA-02】========");
            PrintResult("701 工作模式", driver.VOA_GetMode(1));
            PrintResult("702 支持模式列表", driver.VOA_GetModeList(1));
            PrintResult("703 衰减分辨率", driver.VOA_GetAttRes(1));
            PrintResult("704 当前衰减值", driver.VOA_GetAttenuation(1));
            PrintResult("705 输入功率", driver.VOA_GetInputPower(1));
            PrintResult("706 输出功率", driver.VOA_GetOutputPower(1));
            PrintResult("707 输出开关状态", driver.VOA_GetOutputState(1));
            PrintResult("708 ALC自动功率", driver.VOA_GetAlcState(1));
            Console.WriteLine();
        }
        #endregion

        #region 8 SW光开关 Slot自动11
        static void TestSection8_Switch()
        {
            Console.WriteLine("========【8 光开关SW | 槽位11 SWD2-02】========");
            PrintResult("801 开关类型", driver.SW_GetSwitchType());
            PrintResult("802 当前通道", driver.SW_GetCurrentChannel(1));
            PrintResult("803 总通道数", driver.SW_GetSwitchTotalCount());
            Console.WriteLine();
        }
        #endregion

        #region 9 ERM消光比 Slot自动06
        static void TestSection9_ERM()
        {
            Console.WriteLine("========【9 ERM消光比 | 槽位06 ERM-04】========");
            PrintResult("901 消光比数据", driver.ERM_ReadERData(1));
            PrintResult("902 参考值", driver.ERM_GetRef(1));
            PrintResult("903 速率", driver.ERM_GetRate(1));
            PrintResult("904 校准模式", driver.ERM_GetCalibrateModel(1));
            Console.WriteLine();
        }
        #endregion

        #region 10 LAC光源（本机无硬件）
        static void TestSection10_LAC()
        {
            Console.WriteLine("========【10 LAC固定光源（本机无板）】========");
            PrintResult("1001 光源开关", driver.LAC_GetState(1));
            PrintResult("1002 输出功率", driver.LAC_GetPower(1));
            PrintResult("1003 工作波长", driver.LAC_GetWave(1));
            Console.WriteLine();
        }
        #endregion

        #region 11 LAG可调光源（本机无硬件）
        static void TestSection11_LAG()
        {
            Console.WriteLine("========【11 LAG可调光源（本机无板）】========");
            PrintResult("1101 光源状态", driver.LAG_GetState(1));
            PrintResult("1102 输出功率", driver.LAG_GetPower(1));
            PrintResult("1103 通道号", driver.LAG_GetChannel(1));
            PrintResult("1104 调制频率", driver.LAG_GetFreq(1));
            PrintResult("1105 波长", driver.LAG_GetWave(1));
            PrintResult("1106 老化状态", driver.LAG_GetAge(1));
            PrintResult("1107 网格间隔", driver.LAG_GetGrid(1));
            PrintResult("1108 寄存器0x01", driver.LAG_GetRegData(1, "0x01"));
            Console.WriteLine();
        }
        #endregion

        #region 12 BERT误码仪（本机无硬件）
        static void TestSection12_BERT()
        {
            Console.WriteLine("========【12 BERT误码仪（本机无板）】========");
            PrintResult("1201 比特速率", driver.BERT_GetRate());
            PrintResult("1202 码型", driver.BERT_GetPattern());
            PrintResult("1203 DUT状态", driver.BERT_GetDutStatus());
            PrintResult("1204 通道1幅度", driver.BERT_GetPGAmpl(1));
            PrintResult("1205 极性", driver.BERT_GetPGPol(1));
            PrintResult("1206 误码数据", driver.BERT_GetErrData(1));
            Console.WriteLine();
        }
        #endregion

        #region 13 PCS偏振（本机无硬件）
        static void TestSection13_PCS_PCG()
        {
            Console.WriteLine("========【13 PCS/PCG偏振（本机无板）】========");
            PrintResult("1301 偏振扫描状态", driver.PCS_GetScanState());
            PrintResult("1302 偏振输出状态", driver.PCG_GetPolarState());
            Console.WriteLine();
        }
        #endregion

        #region 14 OPMT多通道（本机无硬件）
        static void TestSection14_OPMT()
        {
            Console.WriteLine("========【14 OPMT多通道（本机无板）】========");
            PrintResult("1401 通道1功率", driver.OPMT_ReadPower(1));
            PrintResult("1402 平均次数", driver.OPMT_GetAverCount(1));
            PrintResult("1403 轨迹点数", driver.OPM_GetTracePoint(1));
            PrintResult("1404 轨迹数据", driver.OPM_GetTraceResult(1));
            Console.WriteLine();
        }
        #endregion

        #region 15 TRIG触发（本机无硬件）
        static void TestSection15_Trigger()
        {
            Console.WriteLine("========【15 TRIG触发（本机无板）】========");
            PrintResult("1501 触发边沿", driver.TRIG_GetSlope());
            PrintResult("1502 触发源", driver.TRIG_GetSource());
            PrintResult("1503 波长范围", driver.TRIG_GetWaveRange());
            PrintResult("1504 触发延时", driver.TRIG_GetDelay());
            PrintResult("1505 日志参数", driver.TRIG_GetLogParam());
            PrintResult("1506 SWS触发状态", driver.SWS_GetTriggerState());
            PrintResult("1507 SWS触发结果", driver.SWS_GetTriggerResult());
            Console.WriteLine();
        }
        #endregion

        /// 统一打印 + 每条指令后自动Sleep(80)
        static void PrintResult(string funcName, string ret)
        {
            if (string.IsNullOrEmpty(ret))
                Console.WriteLine($"【{funcName}】 → 无返回/超时");
            else if (ret.Contains("ERROR"))
                Console.WriteLine($"【{funcName}】 → 报错：{ret}");
            else
                Console.WriteLine($"【{funcName}】 → 正常：{ret}");

            // 每条命令执行完休眠80ms，解决Device busy
            Thread.Sleep(CmdDelayMs);
        }
    }
}