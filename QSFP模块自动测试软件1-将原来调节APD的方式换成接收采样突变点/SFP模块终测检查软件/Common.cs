using System;
using System.IO;
using System.Management;
using System.Data.SqlClient;
using System.Data.OleDb;
using Agilent.AgilentInfiniiumDCA.Interop;
using System.IO.Ports;
using DCAX_86100;

namespace FibertopTest_Common
{
    //===========================================================================
    // GlobalVarFun — 全局"中央配电箱"
    // 所有成员都是 public static，全局唯一、随处可访问。
    // 存放：硬件对象引用、各测试项目的开关标志、状态标志、精度校准参数、设备连接状态
    // 类比：控制面板上所有开关和指示灯的总控箱
    //===========================================================================
    public class GlobalVarFun
    {
        //----- 硬件对象引用（Login_Form中创建，赋值到此处供全程序使用）-----//

        /// <summary>
        /// 加热台对象（TCP连接，端口9000）
        /// 作用：给模块供电、读取电压/电流、控制GPIO引脚、转发I2C命令
        /// </summary>
        public static SFP_EVB_Heater heater;

        /// <summary>
        /// OTP-12(T) 多合一测试仪对象（TCP连接，端口5024）
        /// 集成了 OPM(光功率计) / VOA(衰减器) / SWITCH(光开关) / LAC/LAG(光源) / BERT(误码仪)
        /// </summary>
        public static OTP12Driver otp12;

        /// <summary>
        /// 当前测试的槽位号（1~4）
        /// 加热台有4个槽位，每个槽位可以插一个模块
        /// </summary>
        public static int cutrrentSlot = 1;

        //----- 调试方法选择器 -----//

        /// <summary>
        /// 发射光功率调试方法:
        ///   0x00 = 线性计算法（apc → μW & bias_mA 线性关系）
        ///   0x11 = 普通二分法（apc → dBm，通过二分法逼近目标功率）
        ///   0x22 = 定值判断法（DC耦合TOSA / COB-LD专用）
        /// 不同模块芯片方案需要不同的调试策略
        /// </summary>
        public static byte txpwr_debug_method = 0x00;

        /// <summary>
        /// 消光比调试方法:
        ///   0x00 = 普通二分法（AutoSetTxEr_MethodA）
        ///   0x11 = 逐步逼近法（AutoSetTxEr_MethodB，COB-LD专用）
        /// </summary>
        public static byte txer_debug_method = 0x00;

        /// <summary>
        /// 温度补偿表更新策略标志:
        ///   true  = 比例缩放法（所有温度点乘同一个缩放系数K）
        ///   false = 等量平移法（所有温度点加同一个偏移量delta）
        /// 默认用平移法
        /// </summary>
        public static bool k_lut_flag = false;

        //----- 测试工序和模块类型 -----//

        /// <summary>测试工序类型："firstTest"=初测（需调试+校准）；"finalTest"=终测（只检查不修改）</summary>
        public static string testType = "";

        /// <summary>模块封装类型，当前固定为 "QSFP"（4通道并行光模块）</summary>
        public static string moduleType = "QSFP";

        /// <summary>模块型号名（如 QFP-MM85FG-S1DC），从Login界面选择</summary>
        public static string moudlefpn = "QFP-MM85FG-S1DC";

        /// <summary>模块参数数据库（Access .mdb文件）的本地存放路径</summary>
        public static string moduleLutDBFilePath = "";

        //----- 通信/连接状态标志（相当于红绿灯，false=异常/未连接）-----//

        public static bool i2c_can_use = false;          // I2C总线是否正常可用
        public static bool usb_can_use = false;          // USB转I2C(CP2112)是否正常
        public static bool usb_i2c_open = false;         // USB转I2C是否已成功打开
        public static bool sql_connect_status = false;   // SQL Server数据库连接状态
        public static bool sql_record_status = false;    // SQL写入记录是否正常（曾经写入失败会变false）
        public static bool access_connect_status = false;// Access数据库(.mdb)连接状态
        public static bool access_updated_status = false;// Access文件是否已从服务器更新到本地

        //----- 各测试项目的"开关"（控制流水线上哪些工位需要执行）-----//
        // 这些标志在 Setup_Form 中由用户勾选设置，在 FirstTestProcess / FinalTestProcess 中用于条件判断

        public static bool power_use_DAC = false;    // true=用眼图仪测光功率；false=用光功率计测
        public static bool rx_ddm_test = false;      // 是否执行接收DDM校准测试
        public static bool rx_los_test = false;      // 是否执行接收LOS告警调试
        public static bool rx_nopower_test = true;   // 是否检查接收无光状态（衰减器打到60dB读功率<-40）
        public static bool tx_test = true;           // 是否执行发射测试（测光功率和眼图参数）
        public static bool tx_nopower_test = true;   // 是否检查发射无光状态（TxDisable后功率<-40）
        public static bool sen_test = false;         // 是否执行灵敏度误码测试（需要连接误码仪）
        public static bool tx_jitter_test = true;    // 是否检查发射Jitter抖动（Total = RMS*14 + PP）
        public static int waveforms_num = 0;         // 眼图模板测试累计波形点数（≥100才开启mask test）
        public static bool cob_ld = true;            // 是否是COB-LD封装（Chip-On-Board，无独立TOSA）
        public static bool hw_txdis_test = false;    // 是否检查硬件TxDisable功能（通过加热台GPIO控制）
        public static bool hw_los_test = false;      // 是否检查硬件LOS信号（通过加热台GPIO读取）
        public static bool tx_eye_save_test = false; // 是否保存眼图截图（GIF格式存SQL）
        public static bool threshold_check = false;  // 终测是否检查告警门限值（与标准值比对）
        public static bool flash_check = false;      // 终测是否检查Flash调试数据完整性
        public static bool txrx_cdr_dis = false;     // 是否关闭TxRx的CDR（时钟数据恢复）功能
        public static bool distype_check = false;    // 是否跳过模块方案类型检查
        public static bool tx_pe_test = false;       // 是否写入TX-PE预加重参数
        public static bool testDataIsOK = false;     // 测试参数校验结果（校准点衰减值设置正确与否）
        public static bool tx_tec_test = false;      // 模块是否带TEC（热电制冷器，EML激光器需要）
        public static bool TOSATempEN = false;       // 是否启动TOSA温度调试（EML波长调试）
        public static bool VONEN = false;            // 是否启动VON负压调试（APD偏压）
        public static bool APDen = false;            // 是否启动APD偏压调试（雪崩光电二极管）
        public static bool DCA86100D_Open = false;   // 是否使用86100D型号眼图仪（新款）
        public static bool N1092x_Open = false;      // 是否使用N1092X型号眼图仪（新款光模块专用）

        //----- 接收器类型 -----//

        /// <summary>
        /// 接收器类型: true=APD（雪崩光电二极管，灵敏度高、需要高偏压、5点校准）
        ///             false=PIN（普通光电二极管，简单便宜、3点校准）
        /// </summary>
        public static bool rx_is_apd = false;

        //----- 精度校准参数（Access数据库读取，用户可在Setup界面微调）-----//

        public static int type_index = 0;            // 模块型号在Access列表中的索引
        public static int tx_pe = 0;                 // 发射预加重值（0~255）

        /// <summary>消光比校准偏移值(dB)，实测ER=仪表读数+此偏移</summary>
        public static double ER_cal_num = 0.5;

        /// <summary>发射功率校准偏移值(dB)，实测TxPower=仪表读数+此偏移</summary>
        public static double tx_cal_num = 0.8;

        /// <summary>接收功率校准偏移值(dB)，实测RxPower=仪表读数+此偏移</summary>
        public static double rx_cal_num = 1.2;

        /// <summary>
        /// 光路附加损耗(dB) — 因光纤跳线、连接器等引入的额外衰减
        /// 从光功率计读数后需要加上这个偏移才是模块端口的真实值
        /// </summary>
        public static double opto_att_offset = 3.5;

        /// <summary>4个通道各自的光路附加损耗，每个通道的光路损耗可能不同</summary>
        public static double[] opto_att_offsetbuf = { 3.5, 3.5, 3.5, 3.5 };

        /// <summary>波长调试最大允许误差(nm)，超出此范围判定失败</summary>
        public static double wLengthMaxErr = 0.02;

        //----- 调试参数范围（默认值，会被Access数据库覆盖）-----//
        // apc = 自动功率控制寄存器值（控制激光器偏置电流）
        // mod = 调制电流寄存器值（控制消光比）
        // los = LOS告警门限寄存器值

        public static string apc_min = "30";
        public static string apc_max = "150";
        public static string mod_min = "30";
        public static string mod_max = "150";
        public static string los_min = "20";
        public static string los_max = "80";

        //----- 全局设备引用（核心！整个程序通过这些对象与外部世界交互）-----//

        /// <summary>I2C通信接口 — 与模块芯片对话的"电话线"（当前用I2C_Heater通过TCP转发）</summary>
        public static I2C iic;

        /// <summary>USB转I2C的备用方案（Silicon Labs CP2112芯片），目前未使用</summary>
        public static CP2112 USBtoI2C;

        /// <summary>SQL Server数据库连接 — 存储所有测试记录</summary>
        public static SqlConnection sqlconnection;

        /// <summary>模块测试操作对象 — 实现了 ModuleTest 接口（当前为 QSFP 类）</summary>
        public static ModuleTest mTest;

        /// <summary>老款 Agilent 86100 眼图仪（COM组件，通过GPIB通信）</summary>
        public static AgilentInfiniiumDCA scope;

        /// <summary>光功率计串口对象</summary>
        public static SerialPort uartMeter;

        /// <summary>光衰减器串口对象</summary>
        public static SerialPort uartAtt;

        /// <summary>误码仪（PSS BERT）串口对象</summary>
        public static SerialPort pssert;

        /// <summary>光开关串口对象</summary>
        public static SerialPort opticalSwitch;

        /// <summary>是德 86120C 波长计（通过GPIB/VISA通信）</summary>
        public static Keysight86120C kt86120c;

        /// <summary>新款 86100D / N1092X 眼图仪（通过 VISA DLL 通信）</summary>
        public static DCA_86100 scope_86100d;

        /// <summary>GPIB设备名称字符串</summary>
        public static string gpibname = "";

        //----- 设备连接状态标志 -----//

        public static bool optoMeter_connected = false;       // 光功率计是否已连接
        public static bool optoAtt_connected = false;         // 光衰减器是否已连接
        public static bool optoAtt_new_connected = false;     // 是否使用新型号衰减器(DOA16012，支持SCPI指令)
        public static bool instrument_connected = false;      // 眼图仪(86100)是否已连接
        public static bool pssbert_connected = false;         // 误码仪是否已连接
        public static bool optoSwitch_connected = false;      // 光开关是否已连接
        public static bool wlength_connected = false;         // 波长计是否已连接
        public static bool dca86100d_connected = false;       // 新款86100D是否已连接

        /// <summary>是否需要保存测试记录到SQL的标志 — 测试完成后置true，保存后置false</summary>
        public static bool record_need_save = false;

        /// <summary>测试顺序选择: true=先测发射再测接收; false=先测接收再测发射</summary>
        public static bool test_tx_select = true;

        /// <summary>误码仪指令间延时(ms)，太快可能返回错误</summary>
        public static int pss_bert_delay = 100;

        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 软件注册信息校验
        /// 注意：当前直接返回true（跳过校验），意味着任何机器都能运行。
        /// 如果需要真正的授权控制，需要在此处实现注册码验证逻辑。
        /// </summary>
        public static bool GetRegisterInfo()
        {
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
    }

    //===========================================================================
    // DOA — Digital Optical Attenuator 数字光衰减器参数
    // 衰减器像"水龙头"一样控制接收端光功率的大小。
    // 四个关键接收测试点（光强从大到小）:
    //   饱和点(Overload) → 灵敏度点(Sen) → 去告警点(DLos) → 告警点(ALos) → 无光
    //===========================================================================
    public class DOA
    {
        /// <summary>当前衰减器设定的衰减值(dB)，用于计算等待时间</summary>
        public static float currentAtt = 0;

        //----- 四个关键测试点的衰减值（单通道版本）-----//

        /// <summary>灵敏度点衰减值(dB) — 模块接收的最低可工作光功率</summary>
        public static float rxSenAtt = 0;

        /// <summary>去告警点衰减值(dB) — LOS告警消失的临界光功率（De-assert LOS）</summary>
        public static float rxDLosAtt = 0;

        /// <summary>告警点衰减值(dB) — LOS告警产生的临界光功率（Assert LOS）</summary>
        public static float rxALosAtt = 0;

        /// <summary>过载/饱和点衰减值(dB) — 接收光功率过强导致饱和的临界值</summary>
        public static float rxOverLoadAtt = 0;

        //----- 四个关键测试点的衰减值（4通道版本，QSFP有4个光通道，每个通道光路衰减不同）-----//

        /// <summary>4通道灵敏度点衰减值 [ch0, ch1, ch2, ch3]</summary>
        public static float[] rxSenAttBuf = new float[4];

        /// <summary>4通道去告警点衰减值</summary>
        public static float[] rxDLosAttBuf = new float[4];

        /// <summary>4通道告警点衰减值</summary>
        public static float[] rxALosAttBuf = new float[4];

        /// <summary>4通道过载点衰减值</summary>
        public static float[] rxOverLoadAttBuf = new float[4];

        //----- 校准用衰减值数组 -----//
        // 长度20 = 4通道 × 5校准点/通道
        // 索引公式: ch * 5 + pointIndex
        // 例: ch1的第3个校准点 → rxCalAtt[1*5+2] = rxCalAtt[7]

        /// <summary>
        /// 校准验证点的衰减值(dB)
        /// 校准完成后，在这些衰减值下验证DDM读数精度
        /// </summary>
        public static float[] rxCheckAtt = new float[20];

        /// <summary>
        /// 校准点的衰减值(dB)
        /// 在这些衰减值下读取ADC原始值 → 用于拟合校准曲线
        /// </summary>
        public static float[] rxCalAtt = new float[20];

        //----- 串口通信配置 -----//

        /// <summary>衰减器串口号在列表中的索引</summary>
        public static int com_index = 0;

        /// <summary>衰减器串口名称（如 COM3）</summary>
        public static string com_portname = "COM1";

        /// <summary>
        /// 衰减器响应延时(ms/dB)
        /// 每次改变衰减值时，等待时间 = delay × |新值-旧值|
        /// </summary>
        public static int delay = 10;

        //----- 用户界面文本框对应的衰减值（Setup界面用）-----//

        public static string overload_att = "0.0";  // 过载点衰减值
        public static string sen_att = "0.0";       // 灵敏度点衰减值
        public static string A_los_att = "0.0";     // 告警点衰减值
        public static string D_los = "0.0";         // 去告警点衰减值

        public static string check_att1 = "0.0";    // 校准验证点1
        public static string check_att2 = "0.0";    // 校准验证点2
        public static string check_att3 = "0.0";    // 校准验证点3
        public static string check_att4 = "0.0";    // 校准验证点4（仅APD使用）
        public static string check_att5 = "0.0";    // 校准验证点5（仅APD使用）
    }

    //===========================================================================
    // BIT_ERROR — 误码仪(BERT)参数
    // 用于测试接收灵敏度——在低光功率下检查是否产生误码
    // 通信方式: 串口(RS232) + SCPI指令
    //===========================================================================
    public class BIT_ERROR
    {
        /// <summary>误码仪串口在列表中的索引</summary>
        public static int com_index = 0;

        /// <summary>当前测试的误码仪通道号（如 "CH0"）</summary>
        public static string ch = "CH0";

        /// <summary>误码仪串口名称</summary>
        public static string com_portname = "COM1";

        /// <summary>误码仪指令间延时(ms)</summary>
        public static int delay = 0;
    }

    //===========================================================================
    // MEMTER — 光功率计参数
    // 用于精确测量光功率（精确到0.01dBm）
    // 通信方式: 串口(RS232)
    //===========================================================================
    public class MEMTER
    {
        /// <summary>光功率计串口在列表中的索引</summary>
        public static int com_index = 0;

        /// <summary>
        /// 光功率计类型:
        ///   0 = 手持光功率计（光讯品牌，二进制帧协议 0xEF 0xEF开头）
        ///   1 = 台式光功率计（普塞斯PSS品牌，ASCII SCPI指令 "Read:Power Channel1"）
        /// </summary>
        public static int type_index = 1;

        /// <summary>光功率计串口名称</summary>
        public static string com_portname = "COM1";

        /// <summary>光功率计响应延时(ms)，读取前需等待此时间</summary>
        public static int delay = 500;
    }

    //===========================================================================
    // opcicalSwitch — 光开关参数
    // 光开关用于切换不同光通道，让一台测试设备分时测试多路光信号
    //===========================================================================
    public class opcicalSwitch
    {
        /// <summary>光开关串口在列表中的索引</summary>
        public static int com_index = 0;

        /// <summary>光开关串口名称</summary>
        public static string com_portname = "COM1";

        /// <summary>光开关切换延时(ms)</summary>
        public static int delay = 0;
    }

    //===========================================================================
    // Agilent86100 — 眼图仪偏差校准参数
    //===========================================================================
    public class Agilent86100
    {
        /// <summary>
        /// 消光比(ER)的设备偏移值(dB)
        /// 眼图仪测得的ER值 + 此偏移 = 真实ER值
        /// 因为不同眼图仪之间存在系统误差，需要定期用标准光源校准
        /// </summary>
        public static Double ER_offset = 0.0;
    }

    //===========================================================================
    // TestSet — 测试规格说明书
    // 存放当前型号模块的"应该达到什么指标"——目标值和合格范围
    // 数据来源：Access数据库(.mdb) 中该型号对应的记录
    // 类比：产品规格书上的技术参数表
    //===========================================================================
    public class TestSet
    {
        /// <summary>APD名称 — 非空表示该模块型号使用了APD（雪崩光电二极管）</summary>
        public static string apdName = "";

        //----- 发射参数：合格范围和目标值（从Access数据库读取）-----//
        // 目标值(target)是调试算法努力达到的"理想值"
        // 范围(Min/Max)是合格/不合格的判断边界

        /// <summary>发射光功率下限(dBm)</summary>
        public static float txPwr_Min = 0;

        /// <summary>发射光功率上限(dBm)</summary>
        public static float txPwr_Max = 0;

        /// <summary>偏置电流下限(mA)</summary>
        public static float bias_Min = 0;

        /// <summary>偏置电流上限(mA)</summary>
        public static float bias_Max = 0;

        /// <summary>消光比下限(dB) — ER = Extinction Ratio = P1/P0的比值取对数</summary>
        public static float txEr_Min = 0;

        /// <summary>消光比上限(dB)</summary>
        public static float txEr_Max = 0;

        /// <summary>眼图交叉点下限(%) — Crossing Point，越高眼图越"张开"</summary>
        public static float txCr_Min = 0;

        /// <summary>眼图交叉点上限(%)</summary>
        public static float txCr_Max = 0;

        /// <summary>
        /// Jitter抖动总上限(ps)
        /// 计算公式: Jitter_Total = 6 × Jitter_RMS + Jitter_PP
        /// </summary>
        public static float txJt_Max = 0;

        /// <summary>发射光功率目标值(dBm) — 调试时尽量往这个值靠</summary>
        public static float txPwr_target = 0;

        /// <summary>偏置电流目标值(mA) — 由 (bias_Min + bias_Max) / 2 计算</summary>
        public static float txBias_target = 0;

        /// <summary>消光比目标值(dB)</summary>
        public static float txEr_target = 0;

        /// <summary>波长目标值(nm) — EML激光器的目标波长</summary>
        public static double wLength_target = 0;

        //----- APC/MOD/LOS 寄存器调试点范围 -----//
        // APC(Automatic Power Control) = 自动光功率控制寄存器，值范围 0~255
        // MOD(Modulation) = 调制电流寄存器，控制消光比，值范围 0~255
        // LOS = LOS告警门限寄存器
        //
        // Min/Max     = Access数据库读取的默认值范围
        // Min_set/Max_set = 用户在Setup界面手动调整后的实际范围

        public static UInt16 txapc_Min = 10;
        public static UInt16 txapc_Max = 100;
        public static UInt16 txmod_Min = 20;
        public static UInt16 txmod_Max = 100;
        public static UInt16 rxlos_Min = 20;
        public static UInt16 rxlos_Max = 100;

        public static UInt16 txapc_Min_set = 10;
        public static UInt16 txapc_Max_set = 100;
        public static UInt16 txmod_Min_set = 20;
        public static UInt16 txmod_Max_set = 100;
        public static UInt16 rxlos_Min_set = 20;
        public static UInt16 rxlos_Max_set = 100;

        //----- APD / TOSA温度 / VON 调试范围 -----//

        /// <summary>APD偏压最小值（DAC寄存器值 0~255）</summary>
        public static UInt16 rxapd_min = 70;

        /// <summary>APD偏压最大值</summary>
        public static UInt16 rxapd_max = 90;

        /// <summary>TOSA温度最小值 — DAC值830 ≈ 40℃（对应12位DAC: 0~4095 → 0~2.5V TEC电压）</summary>
        public static UInt16 tosatemp_min = 830;

        /// <summary>TOSA温度最大值 — DAC值1830 ≈ 70℃</summary>
        public static UInt16 tosatemp_max = 1830;

        /// <summary>TOSA温度默认值</summary>
        public static UInt16 tosatemp_def = 900;

        /// <summary>VON负压最小值（DAC寄存器值 0~4095，对应 0~-2.5V）</summary>
        public static UInt16 von_min = 0;

        /// <summary>VON负压最大值</summary>
        public static UInt16 von_max = 4095;

        //----- 接收端参数 -----//

        /// <summary>5个校准点的目标光功率(dBm) — 从Access读取的理想值</summary>
        public static float[] rxPwr_Real = new float[5];

        /// <summary>灵敏度点光功率(dBm)</summary>
        public static float rx_Sen = 0;

        /// <summary>去告警点光功率(dBm)</summary>
        public static float rx_DLos = 0;

        /// <summary>告警点光功率(dBm)</summary>
        public static float rx_ALos = 0;

        /// <summary>过载/饱和点光功率(dBm)</summary>
        public static float rx_OverLoad = 0;

        /// <summary>TOSA温度默认值（直接写到模块的值）</summary>
        public static UInt16 tosa_temp = 830;

        /// <summary>Tx负压默认值（直接写到模块的值）</summary>
        public static byte Tx_von = 0;

        /// <summary>Rx APD偏压默认值（直接写到模块的值）</summary>
        public static byte rx_apd = 255;

        /// <summary>
        /// 5个校准点的实测光功率(dBm)
        /// rxPwr_Real = 用户从Access数据库读到的目标值（"理想值"）
        /// rxPwr_Cal  = 参数校验时实际从光功率计读到的值（"实测值"）
        /// 两者的差就是系统误差，用于修正校准
        /// </summary>
        public static float[] rxPwr_Cal = new float[5];

        //----- 当前测试通道 -----//

        /// <summary>当前正在测试的通道号（0~3，对应QSFP的4个通道）</summary>
        public static int ch = 0;

        //----- 界面显示的DDM字符串（"ch0/ch1/ch2/ch3"格式，每通道用"/"分隔）-----//

        public static string bias_ddm = "0.0/0.0/0.0/0.0";
        public static string txpwr_ddm = "-40/-40/-40/-40";
        public static string rxpwr_ddm = "-40/-40/-40/-40";

        //----- 100G LR4波长调试相关 -----//

        /// <summary>4个通道各自的波长下限(nm)</summary>
        public static float[] wl_min = new float[4];

        /// <summary>4个通道各自的波长上限(nm)</summary>
        public static float[] wl_max = new float[4];

        /// <summary>4个通道各自的波长目标值(nm)</summary>
        public static float[] wl_target = new float[4];

        /// <summary>4通道各自的TOSA温度最小值（二分查找法过程中的缓存值）</summary>
        public static UInt16[] tosa_tempbufmin = new UInt16[4];

        /// <summary>4通道各自的TOSA温度最大值</summary>
        public static UInt16[] tosa_tempbufmax = new UInt16[4];

        /// <summary>4通道中最小的TOSA温度上限</summary>
        public static UInt16 tosa_tempValmin = 0;

        /// <summary>4通道中最大的TOSA温度下限</summary>
        public static UInt16 tosa_tempValmax = 0;

        /// <summary>
        /// EML激光器波长测试类型:
        ///   0 = 40G（4通道同波长，只需找单一温度点）
        ///   1 = 100G LR4 双光纤
        ///   2 = BiDi 23（双向单纤，2路上行3路下行）
        ///   3 = BiDi 32
        /// 决定波长调试使用哪种策略
        /// </summary>
        public static int EMLTestType = 0;
    }

    //===========================================================================
    // TestResult — 测试报告/测试结果存放处
    // 存放测试过程中产生的所有测量数据、调试值、告警标志、校准系数等
    // 是数据从"测量"到"显示"再到"保存SQL"的中转站
    //===========================================================================
    public class TestResult
    {
        //----- 眼图数据 -----//

        /// <summary>眼图图像二维数组（已废弃，改用4个独立的byte[]分通道存储）</summary>
        public static byte[,] txEye_imagebuf = null;

        /// <summary>通道0的眼图GIF图像数据（从86100眼图仪截取）</summary>
        public static byte[] txEye_image_ch0 = null;

        /// <summary>通道1的眼图GIF图像数据</summary>
        public static byte[] txEye_image_ch1 = null;

        /// <summary>通道2的眼图GIF图像数据</summary>
        public static byte[] txEye_image_ch2 = null;

        /// <summary>通道3的眼图GIF图像数据</summary>
        public static byte[] txEye_image_ch3 = null;

        //----- Flash数据 -----//

        /// <summary>模块Flash扇区数据的完整镜像（2048字节），包含身份信息+调试参数+补偿表</summary>
        public static byte[] flash_data = new byte[2048];

        /// <summary>实际读取的Flash有效数据长度（Init中设为768）</summary>
        public static int flash_data_len = 2048;

        //----- 眼图图像参数 -----//

        /// <summary>眼图GIF图像的有效字节长度（从VISA二进制流解析后去除协议头的长度）</summary>
        public static int bimage_len = 0;

        /// <summary>眼图模板测试的波形累计点数（≥100才开启mask test）</summary>
        public static int waveforms_count = 0;

        /// <summary>眼图模板Margin百分比(%) — 模板比标准放大多少（5~90%）</summary>
        public static int mask_margin = 1;

        /// <summary>眼图模板文件名（如 "10GbE_10_3125_May02.msk"），在86100上加载</summary>
        public static string mask_name = "10GbE_10_3125_May02.msk";

        //----- 生产管理编号 -----//

        /// <summary>FSN最大合法值（12位十进制 = 9999 9999 9999）</summary>
        public static UInt64 max_Fsn = 999999999999;

        /// <summary>飞思卓生产单号(Batch Number)</summary>
        public static string fibertop_bn = "00000";

        /// <summary>TOSA(Tx)序列号</summary>
        public static string tosa_sn = "11111";

        /// <summary>ROSA(Rx)序列号</summary>
        public static string rosa_sn = "22222";

        /// <summary>偏置电流DDM显示字符串（格式: "ch0/ch1/ch2/ch3"）</summary>
        public static string bias_ddm = "0.0/0.0/0.0/0.0";

        /// <summary>发射功率DDM显示字符串</summary>
        public static string txpwr_ddm = "-40/-40/-40/-40";

        /// <summary>接收功率DDM显示字符串</summary>
        public static string rxpwr_ddm = "-40/-40/-40/-40";

        /// <summary>飞思卓内部流水号(FSN) — 从Flash表6解析的5字节大端整数</summary>
        public static string fibertop_sn = "";

        /// <summary>模块型号名（飞思卓型号/客户定制型号）</summary>
        public static string fibertop_pn = "";

        //----- 模块自身EEPROM中的身份信息（通过I2C读取，符合QSFP规范的标准地址）-----//

        /// <summary>模块序列号(Serial Number) — 16字节ASCII，地址196</summary>
        public static string sn = "";

        /// <summary>模块型号(Part Number) — 16字节ASCII，地址168</summary>
        public static string pn = "";

        /// <summary>供应商名称/版本 — 16字节ASCII，地址148</summary>
        public static string vn = "";

        /// <summary>生产日期 — 8字节ASCII，地址212，格式YYYYMMDD</summary>
        public static string date = "";

        //----- DDM实时监控值（Digital Diagnostic Monitoring）-----//
        // DDM = 光模块内部自监测的5个标准参数（SFF-8472/QSFP规范定义）
        // 模块通过内部ADC实时测量，存放在I2C指定地址

        /// <summary>模块内部温度(℃)</summary>
        public static float tempDDM = 0;

        /// <summary>模块供电电压(V) — 标称3.3V</summary>
        public static float vccDDM = 0;

        //----- 单通道DDM值 -----//

        /// <summary>当前通道的偏置电流(mA) — 激光器工作电流</summary>
        public static float txBiasDDM = 0;

        /// <summary>当前通道的发射光功率(dBm) — 模块DDM上报值</summary>
        public static float txPowerDDM = 0;

        /// <summary>当前通道的接收光功率(dBm) — 模块DDM上报值</summary>
        public static float rxPowerDDM = 0;

        //----- 4通道DDM数组（buf后缀 = buffer，存放4个通道的完整数据）-----//

        /// <summary>4通道偏置电流(mA) — TxBiasDDMbuf[ch]</summary>
        public static float[] txBiasDDMbuf = new float[4];

        /// <summary>4通道发射功率(dBm) — TxPowerDDMbuf[ch]</summary>
        public static float[] txPowerDDMbuf = new float[4];

        /// <summary>4通道接收功率(dBm) — RxPowerDDMbuf[ch]</summary>
        public static float[] rxPowerDDMbuf = new float[4];

        //----- 告警和警告门限值 ----//
        // 命名规律: <参数名> + <门限类型>
        //   HA = High Alarm (高告警) — 最严重，超出将触发硬件动作
        //   LA = Low Alarm  (低告警)
        //   HW = High Warning (高警告) — 次严重，仅提醒
        //   LW = Low Warning  (低警告)

        public static float tempHA = 0;    // 温度高告警门限(℃)
        public static float tempLA = 0;    // 温度低告警门限(℃)
        public static float tempHW = 0;    // 温度高警告门限(℃)
        public static float tempLW = 0;    // 温度低警告门限(℃)

        public static float vccHA = 0;     // 电压高告警门限(V)
        public static float vccLA = 0;     // 电压低告警门限(V)
        public static float vccHW = 0;     // 电压高警告门限(V)
        public static float vccLW = 0;     // 电压低警告门限(V)

        public static float txBiasHA = 0;  // 偏流高告警门限(mA)
        public static float txBiasLA = 0;  // 偏流低告警门限(mA)
        public static float txBiasHW = 0;  // 偏流高警告门限(mA)
        public static float txBiasLW = 0;  // 偏流低警告门限(mA)

        public static float txPowerHA = 0; // 发射功率高告警门限(dBm)
        public static float txPowerLA = 0; // 发射功率低告警门限(dBm)
        public static float txPowerHW = 0; // 发射功率高警告门限(dBm)
        public static float txPowerLW = 0; // 发射功率低警告门限(dBm)

        public static float rxPowerHA = 0; // 接收功率高告警门限(dBm)
        public static float rxPowerLA = 0; // 接收功率低告警门限(dBm)
        public static float rxPowerHW = 0; // 接收功率高警告门限(dBm)
        public static float rxPowerLW = 0; // 接收功率低警告门限(dBm)

        //----- 告警/警告 触发标志（flags）-----//
        // true = 对应的门限已被触发（LED变红），false = 正常（LED变绿）
        // 由 GetDDMFlagsInterrupt() 从模块I2C寄存器读取并设置

        public static bool tempHA_flag = false;
        public static bool tempLA_flag = false;
        public static bool tempHW_flag = false;
        public static bool tempLW_flag = false;

        public static bool vccHA_flag = false;
        public static bool vccLA_flag = false;
        public static bool vccHW_flag = false;
        public static bool vccLW_flag = false;

        public static bool txBiasHA_flag = false;
        public static bool txBiasLA_flag = false;
        public static bool txBiasHW_flag = false;
        public static bool txBiasLW_flag = false;

        public static bool txPwrHA_flag = false;
        public static bool txPwrLA_flag = false;
        public static bool txPwrHW_flag = false;
        public static bool txPwrLW_flag = false;

        public static bool rxPwrHA_flag = false;
        public static bool rxPwrLA_flag = false;
        public static bool rxPwrHW_flag = false;
        public static bool rxPwrLW_flag = false;

        //===========================================================================
        // 以下是"实测值"——测试过程中从仪器/模块读取的真实测量数据
        //===========================================================================

        //----- 发射参数实测值（单通道版本，当前测试通道的值）-----//

        /// <summary>眼图仪(DCA)读取的光功率(dBm) — 从眼图仪获取，未经偏移修正</summary>
        public static float txPowerDCA = 0;

        /// <summary>发射光功率实测值(dBm) — 经光路损耗偏移修正后的最终值</summary>
        public static float txPower = 0;

        /// <summary>消光比(dB) — ER = Extinction Ratio = 10*log10(P1/P0)</summary>
        public static float txEr = 0;

        /// <summary>眼图信噪比(ESN = Eye Signal-to-Noise ratio)</summary>
        public static float txESN = 0;

        /// <summary>眼图交叉点(%) — 眼图上下沿交点位置，理想值为50%</summary>
        public static float txCrossing = 0;

        /// <summary>Jitter RMS值(ps) — 抖动的均方根值</summary>
        public static float txJiterRMS = 0;

        /// <summary>Jitter峰峰值(ps) — 抖动的最大峰峰值</summary>
        public static float txJiterPP = 0;

        /// <summary>
        /// Jitter Total (ps) — 总抖动
        /// 计算公式因模块类型而异:
        ///   SFP+/XFP: Total = RMS + PP
        ///   其他:     Total = 14 × RMS + PP
        /// </summary>
        public static float txJiterTT = 0;

        //----- 发射参数 4通道数组 -----//

        public static float[] txPowerbuf = new float[4];       // 4通道发射功率
        public static float[] txErbuf = new float[4];          // 4通道消光比
        public static float[] txESNbuf = new float[4];         // 4通道眼图SNR
        public static float[] txCrossingbuf = new float[4];    // 4通道交叉点
        public static float[] txJiterRMSbuf = new float[4];    // 4通道RMS抖动
        public static float[] txJiterPPbuf = new float[4];     // 4通道PP抖动
        public static float[] txJiterTTbuf = new float[4];     // 4通道Total抖动

        //----- DDM偏差（被测值与目标值/标准值的差）-----//

        /// <summary>发射功率偏差(dB) — DDM上报值 - 仪表实测值</summary>
        public static float txPwrErr = 0;

        /// <summary>消光比偏差(dB)</summary>
        public static float txErErr = 0;

        /// <summary>4通道发射功率偏差</summary>
        public static float[] txPwrErrbuf = new float[4];

        /// <summary>4通道消光比偏差</summary>
        public static float[] txErErrbuf = new float[4];

        //----- 接收参数实测值 -----//

        /// <summary>5个校准点的DDM读取功率偏差(dB)</summary>
        public static float[] rxPwrErr = new float[5];

        /// <summary>5个校准点的实际光功率(dBm) — 从光功率计读取</summary>
        public static float[] rxPwrReal = new float[5];

        /// <summary>5个校准点的DDM上报功率(dBm) — 从模块DDM寄存器读取</summary>
        public static float[] rxPwrDDM = new float[5];

        //----- 接收参数 4通道 × 5点 二维数组（索引: [ch, 校准点]）-----//

        public static float[,] rxPwrErrbuf = new float[4, 5];
        public static float[,] rxPwrRealbuf = new float[4, 5];
        public static float[,] rxPwrDDMbuf = new float[4, 5];

        //----- 接收4个关键光功率点的实测值 -----//

        /// <summary>灵敏度点光功率实测值(dBm)</summary>
        public static float rxSen = 0;

        /// <summary>去告警点光功率实测值(dBm)</summary>
        public static float rxDLos = 0;

        /// <summary>告警点光功率实测值(dBm)</summary>
        public static float rxALos = 0;

        /// <summary>过载点光功率实测值(dBm)</summary>
        public static float rxOverLoad = 0;

        //----- 接收4参数 × 4通道 数组 -----//

        public static float[] rxSenbuf = { 0, 0, 0, 0 };
        public static float[] rxDLosbuf = { 0, 0, 0, 0 };
        public static float[] rxALosbuf = { 0, 0, 0, 0 };
        public static float[] rxOverLoadbuf = { 0, 0, 0, 0 };

        /// <summary>4通道实测波长(nm) — 从86120C波长计读取</summary>
        public static double[] wLength = { 0, 0, 0, 0 };

        //----- 调试寄存器写入值 -----//

        /// <summary>APC寄存器最终写入值（控制光功率）</summary>
        public static UInt16 txapcVal = 0;

        /// <summary>MOD寄存器最终写入值（控制消光比）</summary>
        public static UInt16 txmodVal = 0;

        /// <summary>LOS寄存器最终写入值（LOS告警门限）</summary>
        public static UInt16 rxlosVal = 0;

        /// <summary>APD偏压最终写入值（APD反向偏压控制）</summary>
        public static UInt16 rxapdVal = 0;

        /// <summary>TOSA温度最终写入值（DAC值 830~1830）</summary>
        public static UInt16 txtosaTemp = 0;

        /// <summary>VON负压最终写入值</summary>
        public static UInt16 txVON = 0;

        //----- 校准系数 -----//

        /// <summary>
        /// 发射校准系数 k — 斜率
        /// 公式: 光功率(uW) = k × ADC + b
        /// k = Pow(10, txPower/10) * 10000 / ADC
        /// 其中 b 恒为0（过原点）
        /// </summary>
        public static float txPwrCal_k = 0;

        /// <summary>发射校准系数 b — 截距（恒为0）</summary>
        public static float txPwrCal_b = 0;

        /// <summary>
        /// 接收校准多项式系数 C0~C4
        /// 多项式: P = C0 + C1×(ADC-Z) + C2×(ADC-Z)² + C3×(ADC-Z)³ + C4×(ADC-Z)⁴
        /// 其中 Z 是所有采样ADC值的均值
        /// APD用5点3阶拟合 → 最多使用C0~C3
        /// PIN用3点2阶拟合 → 最多使用C0~C2
        /// 系数由 Bit.iapcir() 最小二乘拟合算法计算得到
        /// </summary>
        public static float[] rxPwrCal_c = new float[5];

        /// <summary>接收无光时的暗电流ADC值（衰减器打到最大时的ADC值）</summary>
        public static byte rxNoPwrVal = 0;

        //----- UX3320C 专用三段折线校准参数 -----//
        // UX3320C芯片使用三段分段线性拟合，而非多项式拟合
        // 每段对应不同光功率范围，各有一段独立的 k(斜率) 和 b(截距)

        public static float[] rxPwrCal_k = new float[3];     // 三段折线的斜率
        public static float[] rxPwrCal_b = new float[3];     // 三段折线的截距
        public static UInt16[] rxAdcCal = new UInt16[6];     // 分段点的ADC值

        //----- 其他 -----//

        /// <summary>测试员工号/工位号</summary>
        public static string tester_no;

        /// <summary>
        /// 发射预加重值(TX-PE)，2017.8.21新增
        /// 预加重 = 在发射端预先强调高频成分，补偿传输线的高频衰减
        /// </summary>
        public static Byte txpeVal = 0;

        //----- 模块方案信息（从Flash寄存器解析得到，用于状态栏显示）-----//

        /// <summary>芯片方案字符串（如"37049+37046+011039+002304"）</summary>
        public static string chipType = "";

        /// <summary>模块速率字符串（"40G" / "100G"）</summary>
        public static string bitRate = "";

        /// <summary>传输距离类型（"SR4" / "CW4" / "LR4" / "ER4" / "ZR4" / "PAM4"）</summary>
        public static string softType = "";

        /// <summary>固件版本号（"1" ~ "15"）</summary>
        public static string softVer = "";

        /// <summary>芯片工作状态是否正常（true=正常）</summary>
        public static bool chipIsOK = false;

        /// <summary>WP(Write Protect)写保护是否使能</summary>
        public static bool wpIsEn = false;

        /// <summary>模块是否为SR-850nm短距多模</summary>
        public static bool moduleIsSR = false;

        /// <summary>当前通道号</summary>
        public static int ch = 0;
    }

    //===========================================================================
    // I2C 接口 — 与模块通信的"电话线协议"
    // 定义标准I2C读写操作，所有具体实现都遵循这个协议。
    //
    // 三种实现（多态）:
    //   1. TWI (I2C_TWI.cs)         — 并口LPT方案，通过 inpout32.dll 软件模拟I2C时序
    //   2. CP2112 (I2C_SLABCP2112.cs) — USB转SMBus方案，通过 CP2112 芯片
    //   3. I2C_Heater (I2C_Heater.cs) — TCP转发方案，通过加热台的TCP命令转发I2C
    //      ├── 当前正在使用
    //
    // 优势: Main_Form 和 TestQSFP 不需要关心底层用哪种物理方式
    //       ——只需要知道"这个接口能读写I2C"就够了
    //===========================================================================
    public interface I2C
    {
        /// <summary>打开I2C总线连接</summary>
        bool TWI_Open();

        /// <summary>
        /// 写单字节到I2C设备
        /// </summary>
        /// <param name="DeviceAddress">I2C设备地址（通常为 0xA0 = 模块EEPROM主地址）</param>
        /// <param name="WriteDataByteAddress">目标寄存器/存储地址</param>
        /// <param name="WriteData">要写入的1字节数据</param>
        bool TWI_WriteByte(byte DeviceAddress, byte WriteDataByteAddress, byte WriteData);

        /// <summary>
        /// 从I2C设备读单字节
        /// </summary>
        /// <returns>读取到的字节值，失败返回0</returns>
        byte TWI_ReadByte(byte DeviceAddress, byte WriteDataByteAddress);

        /// <summary>
        /// 连续写多字节到I2C设备（Write Page，用于批量写入）
        /// </summary>
        /// <returns>实际写入的字节数</returns>
        uint TWI_WritePage(byte DeviceAddress, byte WriteDataByteAddress, byte[] WriteDataBuffer, uint num);

        /// <summary>
        /// 连续从I2C设备读多字节（Read Page，用于批量读取）
        /// </summary>
        /// <returns>实际读取的字节数</returns>
        uint TWI_ReadPage(byte DeviceAddress, byte ReadDataByteAddress, byte[] ReadDataBuffer, uint num);

        /// <summary>
        /// 硬件控制模块电源开关
        /// </summary>
        /// <param name="dis">true=断电/禁用; false=上电/使能</param>
        bool setModuleDis(bool dis);

        /// <summary>
        /// 读取硬件LOS（Loss Of Signal）信号状态
        /// </summary>
        /// <returns>true=无光（LOS告警）; false=正常有光</returns>
        bool HardWare_LOS_Get();

        /// <summary>关闭I2C总线连接</summary>
        bool TWI_Close();
    }

    //===========================================================================
    // ModuleTest 接口 — 模块测试操作"说明书"
    //
    // 定义了"可以对一个光模块做的所有操作"，具体实现在 TestQSFP.cs (QSFP类)。
    // 按功能分为四大类:
    //
    //   I.   初始化与类型检测 — Init / CheckTestTypeInfo / SetDebugPWD
    //   II.  DDM读取 — GetTemp / GetVcc / GetTxBias / GetDDMAnalogValues / GetDDMFlagsInterrupt
    //   III. Flash读写 — GetFlashInfo / CheckModuleFlashInfo
    //   IV.  初测调试 — SetTxApcBias / SetTxModBias / SetRxLos / WriteRxCalData / SaveTxDataAfterDebug ...
    //
    // 每个方法内部都是: SelectTable(表号) → 计算寄存器地址 → I2C读写 → 等待 → 验证
    //===========================================================================
    public interface ModuleTest
    {
        //=======================================================================
        // I. 初始化与类型检测
        //=======================================================================

        /// <summary>
        /// 初始化测试参数（必须最先调用，传入I2C对象并设置调试方法等初始值）
        /// </summary>
        void Init(I2C i2c);

        /// <summary>
        /// 检查待测模块的方案/类型/速率是否与当前选择的型号匹配
        /// 读取模块Flash中预写的方案识别字节（地址0xFC），解析出芯片方案、速率、固件版本
        /// </summary>
        bool CheckTestTypeInfo();

        //=======================================================================
        // II. 发射/接收通道控制
        //=======================================================================

        /// <summary>软件控制全部4通道发射开关 — 写寄存器86</summary>
        bool SoftTxDis(bool txDis);

        /// <summary>软件控制指定通道发射开关 — 只开CH通道，关其他3个</summary>
        bool SoftTxCHEn(int CH);

        /// <summary>通过USB-CP2112单独开启指定光源通道（用于接收测试时切换光源）</summary>
        bool SourceSoftEn(int CH);

        /// <summary>写入4字节调试密码到地址0x7B，进入调试模式（写两次确保成功）</summary>
        bool SetDebugPWD();

        /// <summary>
        /// 检查模块是否在调试模式状态下
        /// 返回: 0x00=在调试模式; 0x01=读取失败; 0x02=密码不匹配（不在调试模式）
        /// </summary>
        byte CheckDebugPWD();

        /// <summary>检查当前通道的LOS告警状态 — 读寄存器3的对应bit</summary>
        bool CheckRxLOS();

        //=======================================================================
        // III. DDM单值读取（读QSFP规范标准寄存器，2字节大端序）
        //=======================================================================

        /// <summary>读模块内部温度(℃) — 地址22</summary>
        float GetTemp();

        /// <summary>读模块供电电压(V) — 地址26，公式: 原始值/10000</summary>
        float GetVCC();

        /// <summary>读当前通道偏置电流(mA) — 地址42+ch*2，公式: 原始值/500</summary>
        float GetTxBias();

        /// <summary>读当前通道发射功率(dBm) — 地址50+ch*2，公式: 10*log10(原始值/10000)</summary>
        float GetTxPower();

        /// <summary>读当前通道接收功率(dBm) — 地址34+ch*2</summary>
        float GetRxPower();

        //=======================================================================
        // IV. DDM 批量读取
        //=======================================================================

        /// <summary>一次性读取36字节DDM数据（温度+电压+4通道Bias+4通道Tx/Rx功率）</summary>
        bool GetDDMAnalogValues();

        /// <summary>从表3读取72字节告警/警告门限值（温度/电压/Bias/TxPwr/RxPwr各4个门限）</summary>
        bool GetDDMThresholds();

        /// <summary>读取告警/警告标志位（从寄存器6~14的19个字节中提取各位）</summary>
        bool GetDDMFlagsInterrupt();

        //=======================================================================
        // V. Flash数据读写
        //=======================================================================

        /// <summary>读表0~3的基本Flash信息（SN/PN/VN/Date等身份信息）</summary>
        bool GetFlashInfo();

        /// <summary>读表6/8/9/10的调试参数区（含FSN流水号、APC/MOD/APD补偿表）</summary>
        bool GetFlashInfoDebug();

        /// <summary>读当前通道的APC偏置寄存器值（地址 0xA0+ch）</summary>
        byte GetTxApcBiasSet();

        /// <summary>读当前通道的MOD调制寄存器值（地址 0xA4+ch）</summary>
        byte GetTxModBiasSet();

        /// <summary>读当前通道的DDM发射功率(dBm)</summary>
        double GetTxPwr();

        //=======================================================================
        // VI. Flash数据校验（终测用）
        //=======================================================================

        /// <summary>校验告警门限值是否与Access数据库中的标准值一致</summary>
        bool CheckThresholdsInfo(ref string errMsg);

        /// <summary>校验APC/MOD/APD补偿表的完整性（平移/比例缩放检查）</summary>
        bool CheckModuleFlashInfo(ref string errMsg);

        //=======================================================================
        // VII. Access数据库读取
        //=======================================================================

        /// <summary>从Access数据库读取所有可测试的模块型号列表（填充下拉框）</summary>
        bool GetModuleTypeFromAccessdb(ref string[] str, ref int len);

        /// <summary>从Access数据库读取当前选中型号的完整规格参数和补偿表标准值</summary>
        bool GetTypeDebugInfoFromAccessdb();

        //=======================================================================
        // VIII. 初测调试功能（仅在 "firstTest" 模式下调用）
        //      每个函数对应一个I2C寄存器写入操作
        //=======================================================================

        /// <summary>控制TxRx CDR（时钟数据恢复）的开关</summary>
        bool DisTxRxCDR(bool disVal);

        /// <summary>模块初始化（当前为空实现）</summary>
        bool InitModule();

        /// <summary>写APC寄存器值（地址 0xA0+ch）— 控制激光器偏置电流，从而调节光功率</summary>
        bool SetTxApcBias(UInt16 setVal);

        /// <summary>写MOD寄存器值（地址 0xA4+ch）— 控制调制电流，从而调节消光比</summary>
        bool SetTxModBias(UInt16 setVal);

        /// <summary>写LOS告警门限寄存器（地址 0xB4+ch）— 设置LOS告警触发阈值</summary>
        bool SetRxLos(UInt16 setVal);

        /// <summary>读接收ADC原始值（地址 0xE8+ch*2）— 用于DDM校准计算</summary>
        UInt16 GetRxADC();

        /// <summary>读发射ADC原始值（地址 0xE0+ch*2）— 用于发射校准计算</summary>
        UInt16 GetTxADC();

        /// <summary>将接收校准多项式系数写入模块Flash表7（地址 0x80+ch*16）</summary>
        bool WriteRxCalData();

        /// <summary>将发射校准系数 k 写入模块Flash表7（公式: 光功率uW = k × ADC）</summary>
        bool WriteTxCalData();

        /// <summary>
        /// 保存接收调试结果到Flash:
        ///   APD方案: 更新APD补偿表（平移偏移量），写入表10
        ///   PIN方案: 仅发送保存命令
        /// </summary>
        bool SaveRxDataAfterDebug();

        /// <summary>
        /// 保存发射调试结果到Flash（最完整的保存流程）:
        ///   1. 写入告警门限到表3
        ///   2. 更新APC补偿表到表8（平移偏移量 × 32个温度点）
        ///   3. 更新MOD补偿表到表9（平移或比例缩放 × 32个温度点）
        ///   4. 发送Flash保存命令（写0x82/0x83）
        ///   5. 读回验证所有写入内容
        ///   6. 重新开启温度补偿功能
        /// </summary>
        bool SaveTxDataAfterDebug();

        /// <summary>
        /// 发射温度补偿表控制:
        ///   enable=true:  写0x01到寄存器0x80 → 开启自动温度补偿（正常工况）
        ///   enable=false: 写0x00到寄存器0x80 → 关闭自动温度补偿（调试时需要固定温度）
        /// </summary>
        bool TxTempLookupTableCtrl(bool enable);

        /// <summary>写入TX-PE（预加重）等默认调试参数到模块</summary>
        bool WriteTxRxDefaultVal();

        /// <summary>使能发射（带TEC方案的模块需要延时等待TEC稳定）</summary>
        bool SetTx_EN();

        /// <summary>设置APD偏压寄存器值（地址 0xBC+ch）— 控制雪崩光电二极管的偏置电压</summary>
        bool SetAPD(UInt16 setVal);

        /// <summary>设置VON负压寄存器值（地址 0xC8+ch）— 控制PIN/APD的负偏压</summary>
        bool SetVON(UInt16 setVal);

        /// <summary>
        /// 设置TOSA温度（地址 0xCC，2字节大端）— 通过TEC热电制冷器控制EML激光器温度
        /// 值范围 830~1830 对应12位DAC输出 0~2.5V
        /// </summary>
        bool SetTOSATemp(UInt16 setVal);
    }
}
