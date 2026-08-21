using SFPXFP自动测试软件多端口;
using System;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
//using WindowsFormsApp1;
namespace FibertopTest_Common
{
    public class GlobalVarFun
    {
        public static byte txpwr_debug_method = 0x00; // 0x00:线性计算法 apc-->uw & bias   0x11: 普通二分法 apc-->dBm   22:差值二分法 apc-->uW
        public static bool k_lut_flag = false; // 补偿表： true = 比例缩放K    false = 平移方法
        //
        public static string Language = "Chinese";
        public static string testType = "";
        public static string moduleType = "";
        public static string moduleLutDBFilePath = "";
        public static string meter_error_message = ""; 

        public static bool i2c_can_use  = false;
        public static bool i2c_can_use_2 = false;

        public static bool sql_connect_status = false;
        public static bool sql_record_status  = false;
        public static bool access_connect_status = false;
        public static bool access_updated_status = false;

        public static bool sql_connect_status_2 = false;
        public static bool sql_record_status_2 = false;
        public static bool access_connect_status_2 = false;
        public static bool access_updated_status_2 = false;

        public static bool sql_connect_status_3 = false;
        public static bool sql_record_status_3 = false;
        public static bool access_connect_status_3 = false;
        public static bool access_updated_status_3 = false;

        public static bool sql_connect_status_4 = false;
        public static bool sql_record_status_4 = false;
        public static bool access_connect_status_4 = false;
        public static bool access_updated_status_4 = false;

        public static I2C iic;
        public static I2C iic_2;

        public static EVB Evb;
        //public static EVB IIC2;
        //public static EVB IIC3;
        //public static EVB IIC4;

        public static SqlConnection sqlconnection;
        public static ModuleTest mTest;
        public static ModuleTest mTest2;
        public static ModuleTest mTest3;
        public static ModuleTest mTest4;
        public static bool record_need_save = false;
        public static bool record_need_save_2 = false;
        public static bool record_need_save_3 = false;
        public static bool record_need_save_4 = false;
        public static string sqlserver_ip = "null";

        public static SetupSelect setup;
        public static bool module_insert1 = false;
        public static bool module_insert2 = false;

        public static TestControl mycontrol_dut1;
        public static TestControl mycontrol_dut2;
        public static TestControl mycontrol_dut3;
        public static TestControl mycontrol_dut4;
        public static bool eyeMaskIsOpened = false;
        public static bool testDataIsOK1 = false;
        public static bool testDataIsOK2 = false;
        public static bool testDataIsOK3 = false;
        public static bool testDataIsOK4 = false;
        public static int type_index = 0;
        public static string pnselect = "";

        //COM
        public static int meter_com = 0;
        public static int doa_com = 0;
        public static int doa2_com = 0;
        public static int switch_com = 0;
        public static int bert_com = 0;
        public static int ms9710b_com = 0;
        public static int kh96120c_com = 0;
        public static int age3632a_com = 0;

        public static int meterdealy = 500;
        //信号量
        public static readonly SemaphoreSlim BusSemaphore = new SemaphoreSlim(1, 1);

        public static Dictionary<int, string> VOAtxDutToSlot = new Dictionary<int, string>()
        {
            {1, "07"},
            {2, "07"},
            {3, "08"},
            {4, "08"}
        };

        /// <summary>
        /// EML自动测试：DUT编号 → OTP板卡槽位字符串
        /// 全部4个通道统一使用OTP槽位05
        /// </summary>
        public static Dictionary<int, string> OpmDutToOtpSlot = new Dictionary<int, string>()
        {
            {1, "05"},
            {2, "05"},
            {3, "05"},
            {4, "05"}
        };

        /// DUT编号 → OTP槽位字符串
        public static Dictionary<int, string> VOArxDutToSlot = new Dictionary<int, string>()
        {
            {1, "09"},
            {2, "09"},
            {3, "10"},
            {4, "10"}
        };


        ///DUT编号 → OPM衰减通道 
        public static Dictionary<int, int> DutToOpmCh = new Dictionary<int, int>()
        {
            {1, 1},
            {2, 2},
            {3, 3},
            {4, 4}
        };

        ///DUT编号 → OTP VOA衰减通道 
        public static Dictionary<int, int> DutToVoaCh = new Dictionary<int, int>()
        {
            {1, 1},
            {2, 2},
            {3, 3},
            {4, 4}
        };

        ///DUT编号 → OTP BERT误码通道
        public static Dictionary<int, int> DutToBertCh = new Dictionary<int, int>()
        {
            {1, 1},
            {2, 2},
            {3, 3},
            {4, 4}
        };

        //public static bool apb_check = false;
        ////////////////////////////////////////////////////////////////////////////////////////////
        public static bool GetRegisterInfo()
        {
            return true;
        }

        public static bool GetRegisterInfo3()
        {
            return true;
        }

        //获取CPU信息 
        private  static  string GetCpuInfo()
        {
            return "";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
    }

    public struct ReturnReuslt
    {
        public string test_type;// 测试类型
        public float func_id;   // 方法ID
        public int channel;     // 通道号
        public string simpler;  // 简略信息
        public string message;  // 详细信息
    }

    public class ReturnTxRxResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string TestResultMessage { get; set; }
        public Color  TestResultColor { get; set; }//测试状态框  green ;  red
        public Color TestLogColor { get; set; }//信息提示框文字颜色

        public int Percentage { get; set; }//进度条
        public string StatusText { get; set; }//状态

        //Sn
        public string TestResultSn { get; set; }
        public string TestResultFibertopSn { get; set; }
        public int Testprogress { get; set; }
        public int Channel { get; set; }
        public float TxPower { get; set; }
        public float ExtinctionRatio { get; set; }
        public float Bias { get; set; }
        public float WaveLength { get; set; }


        public UInt16  apc { get; set; }
        public UInt16 mod { get; set; }
        public UInt16 cpa { get; set; }
        public UInt16 tx_pe { get; set; }
        public UInt16 tosatemp { get; set; }
        public UInt16 apd { get; set; }
        public UInt16 los { get; set; }
        public UInt16 von { get; set; }

        public double[] RxAdcValues { get; set; } = new double[6]; // 存储 rxAdc[0], rxAdc[1], rxAdc[2], rxAdc[5]
        public float[] RxRealPowers { get; set; } = new float[6];  // 存储实际功率值 [0], [1], [2]
        public float[] RxddmPowers { get; set; } = new float[6];   // 存储实际功率值 [0], [1], [2]
        public byte NoPowerValue { get; set; } // 存储无光状态下的值
        public string RxSenBert { get; set; }  //存储接收误码率

        //结果判定
        public string TxpwrResultShow { get; set; }  //结果显示
        public string TxerResultShow { get; set; }   //结果显示
        public string TxBiasResultShow { get; set; } //结果显示
        public string TxCrResultShow { get; set; }   //结果显示
        public string TxJtResultShow { get; set; }   //结果显示
        //方案显示
        public string ModuleSchemeShow { get; set; } //方案显示
        public bool  ModuleSRShow { get; set; }     //方案显示
        public bool  ModuleChipisOK { get; set; }     //芯片状态显示


        // ... 可能还有其他需要的字段
    }

    //设置界面全局标识
    public struct SetupSelect
    {
        //Tx
        public bool tx_test;
        public bool tx_nopwr_test;
        public bool tx_eml_test;
        public bool algorithm_25g_lr;
        public bool algorithm_cob_ld;
        public bool image_save;
        public bool tx_hardware_disable;
        public bool tx_jitter_test;
        public bool tx_pe_test;
        public bool tx_use_dca_txpwr;

        //Rx
        public bool rx_test;
        public bool rx_nopwr_test;
        public bool rx_los_test;
        public bool rx_sen_test;
        public bool rx_ddm_test;
        public bool rx_apd_test;
        public bool rx_apd_cal;
        public bool rx_hardware_los;

        //综合
        public bool threshold_check;
        public bool flash_check;

        //其它
        public bool init_module;
        public bool tx_rx_cdr_dis;
        public bool scheme_check_dis;
        public bool electrical_module;
        public bool dca_86100d;
        public bool dca_n1092x;
        public bool meterType_hand;

        //设备参数设置
        public int waveforms_num;    //眼图累积点
        public int meter_delay;    //功率计延时
        public int doa_delay;       //衰减器延时
        public int bert_delay;     //误码仪延时

        //设备COM
        public string meter_com;
        public string doa_com;
        public string bert_com;
        public string dca_gpib;
        public string ms9710x_com;
        public string kt86120x_com;
        public string ag_e3632a_com;
        public string switch_com;
        //设备通道
        public int meter_ch_a;
        public int meter_ch_b;

        public int bert_ch_a;
        public int bert_ch_b;

        //设备参数偏差值
        public float meter_err_dut1;
        public float meter_err_dut2;
        public float meter_err_dut3;
        public float meter_err_dut4;
        public float dca_er_err;
        public float dac_txpwr_err;
        public float spectral_wideth;

        //设备连接状态标识
        public bool meter_connect;
        public bool doa_connect;
        public bool doa_connect2;
        public bool doa_connect3;
        public bool doa_connect4;
        public bool bert_connect;
        public bool dca_connect;
        public bool ms9710x_connect;
        public bool kt86120x_connect;
        public bool ag_e3632a_connect;
        public bool opticalswitch_connect;
        public bool otp12_connect;


        //测试参数设置
        //测试精度
        public float er_cal;
        public float txpwr_cal;
        public float rxpwr_cal;
        public float wlgth_cal;
    }

    // 数字光衰减器
    public class DOA
    {
        public static float currentAtt = 0;
        public static float rxSenAtt   = 0;
        public static float rxDLosAtt  = 0;
        public static float rxALosAtt  = 0;
        public static float rxOverLoadAtt = 0;
        public static float[] rxCheckAtt = new float[5];

        public static float[] rxCalAtt = new float[5];
    }
    public class DOA2
    {
        public static float currentAtt = 0;
        public static float rxSenAtt = 0;
        public static float rxDLosAtt = 0;
        public static float rxALosAtt = 0;
        public static float rxOverLoadAtt = 0;
        public static float[] rxCheckAtt = new float[5];

        public static float[] rxCalAtt = new float[5];
    }
    public class DOA3
    {
        public static float currentAtt = 0;
        public static float rxSenAtt = 0;
        public static float rxDLosAtt = 0;
        public static float rxALosAtt = 0;
        public static float rxOverLoadAtt = 0;
        public static float[] rxCheckAtt = new float[5];

        public static float[] rxCalAtt = new float[5];
    }
    public class DOA4
    {
        public static float currentAtt = 0;
        public static float rxSenAtt = 0;
        public static float rxDLosAtt = 0;
        public static float rxALosAtt = 0;
        public static float rxOverLoadAtt = 0;
        public static float[] rxCheckAtt = new float[5];

        public static float[] rxCalAtt = new float[5];
    }

    public class TestSet
    {
        public static string apdName = ""; //非空 表示有APD
        public static float txPwr_Min = 0;
        public static float txPwr_Max = 0;
        public static float bias_Min = 0;
        public static float bias_Max = 0;
        public static float txEr_Min = 0;
        public static float txEr_Max = 0;

        public static float txCr_Min = 0; //交叉点
        public static float txCr_Max = 0; //交叉点
        public static float txJt_Max = 0; //Jitter total = 6*Jrms + Jpp
        
        public static float txPwr_target = 0;
        public static float txBias_target = 0;
        public static float txEr_target  = 0;
        public static double wLength_target = 0; 

        public static UInt16 txapc_Min = 10;
        public static UInt16 txapc_Max = 100;
        public static UInt16 txmod_Min = 20;
        public static UInt16 txmod_Max = 100;
        public static UInt16 rxlos_Min = 20;
        public static UInt16 rxlos_Max = 100;
        public static UInt16 rxapd_min = 70;
        public static UInt16 rxapd_max = 90;
        public static UInt16 tosatemp_min = 830;
        public static UInt16 tosatemp_max = 1830;
        public static UInt16 von_min = 0;
        public static UInt16 von_max = 4095;
        public static UInt16 txcpa_Min = 10;
        public static UInt16 txcpa_Max = 25;

        public static float[] rxPwr_Real = new float[5];
        public static float rx_Sen = 0;
        public static float rx_DLos = 0;
        public static float rx_ALos = 0;
        public static float rx_OverLoad = 0;

        public static float[] rxPwr_Cal = new float[5];

        //UI def
        public static UInt16 txapc_Min_def = 10;
        public static UInt16 txapc_Max_def = 100;
        public static UInt16 txmod_Min_def = 20;
        public static UInt16 txmod_Max_def = 100;
        public static UInt16 rxlos_Min_def = 20;
        public static UInt16 rxlos_Max_def = 100;
        public static UInt16 rxapd_min_def = 70;
        public static UInt16 rxapd_max_def = 90;
        public static UInt16 tosatemp_min_def = 830;
        public static UInt16 tosatemp_max_def = 1830;
        public static UInt16 von_min_def = 0;
        public static UInt16 von_max_def = 4095;
        public static UInt16 txcpa_Min_def = 10;
        public static UInt16 txcpa_Max_def = 25;

        public static UInt16 delay_doa = 30;
        public static UInt16 delay_opm = 500;
        public static UInt16 delay_pssbert = 100;

        public static float txPwr_prec = 0.5f;
        public static float rxPwr_prec = 1.0f;
        public static float txer_prec = 0.3f;
        public static float wlgth_prec = 1550;
        public static float wlgth_err = 1;
        public static double  spectralwidth_max = 0.8f;

        //checkbox
        public static string test_tx = "";
        public static string test_txnopwr = "";
        public static string test_rx = "";
        public static string test_rxnopwr = "";
        public static string test_sen = "";
        public static string test_txdishw = "";
        public static string test_rxloshw = "";
        public static string test_25galg = "";
        public static string test_cobld = "";
        public static string test_eml = "";
        public static string test_apd = "";
        public static string test_coppersfp = "";
        public static string test_cdrdis = "";
        public static string test_schemedis = "";
        public static string test_init = "";
        public static string test_rosa_pin = "";
        public static string test_eyesave = "";

        public static bool eyeMaskIsOpened = false;
        public static byte Dut = 1;
        public static bool setupUI_ok = false;

        public static float meter_pwr_err = 3.5f;
        public static int meter_ch = 1;
        public static int meter_ch_index = 0;
        public static int bert_ch = 1;

        public static float txpwr_cal = 0.8f;
        public static float txer_cal = 0.3f;
        public static float rxpwr_cal = 1.2f;

    }
    public class TestSet2
    {
        public static string apdName = ""; //非空 表示有APD
        public static float txPwr_Min = 0;
        public static float txPwr_Max = 0;
        public static float bias_Min = 0;
        public static float bias_Max = 0;
        public static float txEr_Min = 0;
        public static float txEr_Max = 0;

        public static float txCr_Min = 0; //交叉点
        public static float txCr_Max = 0; //交叉点
        public static float txJt_Max = 0; //Jitter total = 6*Jrms + Jpp

        public static float txPwr_target = 0;
        public static float txBias_target = 0;
        public static float txEr_target = 0;
        public static double wLength_target = 0;

        public static UInt16 txapc_Min = 10;
        public static UInt16 txapc_Max = 100;
        public static UInt16 txmod_Min = 20;
        public static UInt16 txmod_Max = 100;
        public static UInt16 rxlos_Min = 20;
        public static UInt16 rxlos_Max = 100;
        public static UInt16 rxapd_min = 70;
        public static UInt16 rxapd_max = 90;
        public static UInt16 tosatemp_min = 830;
        public static UInt16 tosatemp_max = 1830;
        public static UInt16 von_min = 0;
        public static UInt16 von_max = 4095;
        public static UInt16 txcpa_Min = 10;
        public static UInt16 txcpa_Max = 25;

        public static float[] rxPwr_Real = new float[5];
        public static float rx_Sen = 0;
        public static float rx_DLos = 0;
        public static float rx_ALos = 0;
        public static float rx_OverLoad = 0;

        public static float[] rxPwr_Cal = new float[5];

        //UI def
        public static UInt16 txapc_Min_def = 10;
        public static UInt16 txapc_Max_def = 100;
        public static UInt16 txmod_Min_def = 20;
        public static UInt16 txmod_Max_def = 100;
        public static UInt16 rxlos_Min_def = 20;
        public static UInt16 rxlos_Max_def = 100;
        public static UInt16 rxapd_min_def = 70;
        public static UInt16 rxapd_max_def = 90;
        public static UInt16 tosatemp_min_def = 830;
        public static UInt16 tosatemp_max_def = 1830;
        public static UInt16 von_min_def = 0;
        public static UInt16 von_max_def = 4095;
        public static UInt16 txcpa_Min_def = 10;
        public static UInt16 txcpa_Max_def = 25;

        public static UInt16 delay_doa = 30;
        public static UInt16 delay_opm = 500;
        public static UInt16 delay_pssbert = 100;

        public static float txPwr_prec = 0.5f;
        public static float rxPwr_prec = 1.0f;
        public static float txer_prec = 0.3f;
        public static float wlgth_prec = 1550;
        public static float wlgth_err = 1;
        public static double spectralwidth_max = 0.8f;

        //checkbox
        public static string test_tx = "";
        public static string test_txnopwr = "";
        public static string test_rx = "";
        public static string test_rxnopwr = "";
        public static string test_sen = "";
        public static string test_txdishw = "";
        public static string test_rxloshw = "";
        public static string test_25galg = "";
        public static string test_cobld = "";
        public static string test_eml = "";
        public static string test_apd = "";
        public static string test_coppersfp = "";
        public static string test_cdrdis = "";
        public static string test_schemedis = "";
        public static string test_init = "";
        public static string test_rosa_pin = "";
        public static string test_eyesave = "";

        public static bool eyeMaskIsOpened = false;
        public static byte Dut = 2;
        public static bool setupUI_ok = false;
        public static float meter_pwr_err = 3.5f;
        public static int meter_ch = 3;
        public static int meter_ch_index = 0;
        public static int bert_ch = 2;

        public static float txpwr_cal = 0.8f;
        public static float txer_cal = 0.3f;
        public static float rxpwr_cal = 1.2f;
    }
    public class TestSet3
    {
        public static string apdName = ""; //非空 表示有APD
        public static float txPwr_Min = 0;
        public static float txPwr_Max = 0;
        public static float bias_Min = 0;
        public static float bias_Max = 0;
        public static float txEr_Min = 0;
        public static float txEr_Max = 0;

        public static float txCr_Min = 0; //交叉点
        public static float txCr_Max = 0; //交叉点
        public static float txJt_Max = 0; //Jitter total = 6*Jrms + Jpp

        public static float txPwr_target = 0;
        public static float txBias_target = 0;
        public static float txEr_target = 0;
        public static double wLength_target = 0;

        public static UInt16 txapc_Min = 10;
        public static UInt16 txapc_Max = 100;
        public static UInt16 txmod_Min = 20;
        public static UInt16 txmod_Max = 100;
        public static UInt16 rxlos_Min = 20;
        public static UInt16 rxlos_Max = 100;
        public static UInt16 rxapd_min = 70;
        public static UInt16 rxapd_max = 90;
        public static UInt16 tosatemp_min = 830;
        public static UInt16 tosatemp_max = 1830;
        public static UInt16 von_min = 0;
        public static UInt16 von_max = 4095;
        public static UInt16 txcpa_Min = 10;
        public static UInt16 txcpa_Max = 25;

        public static float[] rxPwr_Real = new float[5];
        public static float rx_Sen = 0;
        public static float rx_DLos = 0;
        public static float rx_ALos = 0;
        public static float rx_OverLoad = 0;

        public static float[] rxPwr_Cal = new float[5];

        //UI def
        public static UInt16 txapc_Min_def = 10;
        public static UInt16 txapc_Max_def = 100;
        public static UInt16 txmod_Min_def = 20;
        public static UInt16 txmod_Max_def = 100;
        public static UInt16 rxlos_Min_def = 20;
        public static UInt16 rxlos_Max_def = 100;
        public static UInt16 rxapd_min_def = 70;
        public static UInt16 rxapd_max_def = 90;
        public static UInt16 tosatemp_min_def = 830;
        public static UInt16 tosatemp_max_def = 1830;
        public static UInt16 von_min_def = 0;
        public static UInt16 von_max_def = 4095;
        public static UInt16 txcpa_Min_def = 10;
        public static UInt16 txcpa_Max_def = 25;

        public static UInt16 delay_doa = 30;
        public static UInt16 delay_opm = 500;
        public static UInt16 delay_pssbert = 100;

        public static float txPwr_prec = 0.5f;
        public static float rxPwr_prec = 1.0f;
        public static float txer_prec = 0.3f;
        public static float wlgth_prec = 1550;
        public static float wlgth_err = 1;
        public static double spectralwidth_max = 0.8f;

        //checkbox
        public static string test_tx = "";
        public static string test_txnopwr = "";
        public static string test_rx = "";
        public static string test_rxnopwr = "";
        public static string test_sen = "";
        public static string test_txdishw = "";
        public static string test_rxloshw = "";
        public static string test_25galg = "";
        public static string test_cobld = "";
        public static string test_eml = "";
        public static string test_apd = "";
        public static string test_coppersfp = "";
        public static string test_cdrdis = "";
        public static string test_schemedis = "";
        public static string test_init = "";
        public static string test_rosa_pin = "";
        public static string test_eyesave = "";

        public static bool eyeMaskIsOpened = false;
        public static byte Dut = 3;
        public static bool setupUI_ok = false;
        public static float meter_pwr_err = 3.5f;
        public static int meter_ch = 3;
        public static int meter_ch_index = 0;
        public static int bert_ch = 2;

        public static float txpwr_cal = 0.8f;
        public static float txer_cal = 0.3f;
        public static float rxpwr_cal = 1.2f;
    }

    public class TestSet4
    {
        public static string apdName = ""; //非空 表示有APD
        public static float txPwr_Min = 0;
        public static float txPwr_Max = 0;
        public static float bias_Min = 0;
        public static float bias_Max = 0;
        public static float txEr_Min = 0;
        public static float txEr_Max = 0;

        public static float txCr_Min = 0; //交叉点
        public static float txCr_Max = 0; //交叉点
        public static float txJt_Max = 0; //Jitter total = 6*Jrms + Jpp

        public static float txPwr_target = 0;
        public static float txBias_target = 0;
        public static float txEr_target = 0;
        public static double wLength_target = 0;

        public static UInt16 txapc_Min = 10;
        public static UInt16 txapc_Max = 100;
        public static UInt16 txmod_Min = 20;
        public static UInt16 txmod_Max = 100;
        public static UInt16 rxlos_Min = 20;
        public static UInt16 rxlos_Max = 100;
        public static UInt16 rxapd_min = 70;
        public static UInt16 rxapd_max = 90;
        public static UInt16 tosatemp_min = 830;
        public static UInt16 tosatemp_max = 1830;
        public static UInt16 von_min = 0;
        public static UInt16 von_max = 4095;
        public static UInt16 txcpa_Min = 10;
        public static UInt16 txcpa_Max = 25;

        public static float[] rxPwr_Real = new float[5];
        public static float rx_Sen = 0;
        public static float rx_DLos = 0;
        public static float rx_ALos = 0;
        public static float rx_OverLoad = 0;

        public static float[] rxPwr_Cal = new float[5];

        //UI def
        public static UInt16 txapc_Min_def = 10;
        public static UInt16 txapc_Max_def = 100;
        public static UInt16 txmod_Min_def = 20;
        public static UInt16 txmod_Max_def = 100;
        public static UInt16 rxlos_Min_def = 20;
        public static UInt16 rxlos_Max_def = 100;
        public static UInt16 rxapd_min_def = 70;
        public static UInt16 rxapd_max_def = 90;
        public static UInt16 tosatemp_min_def = 830;
        public static UInt16 tosatemp_max_def = 1830;
        public static UInt16 von_min_def = 0;
        public static UInt16 von_max_def = 4095;
        public static UInt16 txcpa_Min_def = 10;
        public static UInt16 txcpa_Max_def = 25;

        public static UInt16 delay_doa = 30;
        public static UInt16 delay_opm = 500;
        public static UInt16 delay_pssbert = 100;

        public static float txPwr_prec = 0.5f;
        public static float rxPwr_prec = 1.0f;
        public static float txer_prec = 0.3f;
        public static float wlgth_prec = 1550;
        public static float wlgth_err = 1;
        public static double spectralwidth_max = 0.8f;

        //checkbox
        public static string test_tx = "";
        public static string test_txnopwr = "";
        public static string test_rx = "";
        public static string test_rxnopwr = "";
        public static string test_sen = "";
        public static string test_txdishw = "";
        public static string test_rxloshw = "";
        public static string test_25galg = "";
        public static string test_cobld = "";
        public static string test_eml = "";
        public static string test_apd = "";
        public static string test_coppersfp = "";
        public static string test_cdrdis = "";
        public static string test_schemedis = "";
        public static string test_init = "";
        public static string test_rosa_pin = "";
        public static string test_eyesave = "";

        public static bool eyeMaskIsOpened = false;
        public static byte Dut = 4;
        public static bool setupUI_ok = false;
        public static float meter_pwr_err = 3.5f;
        public static int meter_ch = 3;
        public static int meter_ch_index = 0;
        public static int bert_ch = 2;

        public static float txpwr_cal = 0.8f;
        public static float txer_cal = 0.3f;
        public static float rxpwr_cal = 1.2f;
    }

    public class ChannelConfig
    {
        public string VoaSlot;   // VOA模块所在板卡号（字符串，如"09"表示第9号槽位）
        public int VoaCh;        // VOA通道号（每个VOA模块有ch1和ch2两个通道）
        public string SwSlot;    // 光开关模块所在板卡号（如"11"表示第11号槽位）
        public int SwIn;         // 光开关输入端口号
        public int SwOut;        // 光开关输出端口号
    }

    public class TestResult
    {
        public static byte[] txEye_image = null;
        public static byte[] flash_data  = new byte[2048];//1024
        public static int flash_data_len = 2048;//1024
        public static int bimage_len = 0;
        public static int waveforms_count = 0;
        public static int mask_margin = 1;
        public static string mask_name = "10GbE_10_3125_May02.msk";

        public static UInt64 max_Fsn = 999999999999; // 12位

        public static string fibertop_bn = "00000"; // 生产单号
        public static string tosa_sn = "11111";
        public static string rosa_sn = "22222";

        public static string fibertop_sn = "";
        public static string fibertop_pn = "";

        public static string sn = "";
        public static string pn = "";
        public static string vn = "";
        public static string date = "";

        public static float tempDDM = 0;
        public static float vccDDM = 0;
        public static float txBiasDDM = 0;
        public static float txPowerDDM = 0;
        public static float rxPowerDDM = 0;

        public static float tempHA = 0;
        public static float tempLA = 0;
        public static float tempHW = 0;
        public static float tempLW = 0;

        public static float vccHA = 0;
        public static float vccLA = 0;
        public static float vccHW = 0;
        public static float vccLW = 0;

        public static float txBiasHA = 0;
        public static float txBiasLA = 0;
        public static float txBiasHW = 0;
        public static float txBiasLW = 0;

        public static float txPowerHA = 0;
        public static float txPowerLA = 0;
        public static float txPowerHW = 0;
        public static float txPowerLW = 0;

        public static float rxPowerHA = 0;
        public static float rxPowerLA = 0;
        public static float rxPowerHW = 0;
        public static float rxPowerLW = 0;

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
        

        // 真实测试值
        ////////////////////////////////
        public static float txPowerDCA = 0;
        public static float txPower = 0;
        public static float txEr = 0;
        public static float txESN = 0;
        public static float txCrossing = 0;
        public static float txJiterRMS = 0;
        public static float txJiterPP = 0;
        public static float txJiterTT = 0; //Jitter total
        public static float TxRiseTime = 0.0f;//2024.12.07
        public static float TxFallTime = 0.0f;//2024.12.07
        public static double wLength = 0;//波长 2025.05.21 
        public static double smsr = 0;//边模抑制比 2025.09.11
        public static double spectralwidth = 0;//谱宽 2025.09.11
        public static double supply = 0;//电流（功耗）2025.09.11
        public static float TxEyeAmp = 0;//眼图幅度 2025.09.12 
        //
        public static float txPwrErr = 0;
        public static float txErErr  = 0;

        public static float[] rxPwrErr  = new float[5];
        public static float[] rxPwrReal = new float[5];
        public static float[] rxPwrDDM  = new float[5];
        
        public static float rxSen = 0;
        public static float rxDLos = 0;
        public static float rxALos = 0;
        public static float rxOverLoad = 0;
        //
        public static UInt16 txapcVal = 0;
        public static UInt16 txmodVal = 0;
        public static UInt16 rxlosVal = 0;
        public static UInt16 rxapdVal = 0;
        public static UInt16 txtosaTemp = 0;
        public static UInt16 txVON = 0;
        public static UInt16 txCPA =0;
        //
        public static float txPwrCal_k = 0;
        public static float txPwrCal_b = 0;
        public static float[] rxPwrCal_c = new float[5];
        public static byte rxNoPwrVal = 0;
        //
        public static float[] rxPwrCal_k = new float[3]; //for UX3320C 三折线
        public static float[] rxPwrCal_b = new float[3]; //for UX3320C 三折线
        public static UInt16[] rxAdcCal = new UInt16[6]; //for UX3320C 三折线
        //
        public static string tester_no;
        //
        public static Byte txpeVal = 0; //2017.8.21

        //
        public static string chipType = ""; // LDD+LA+MCU
        public static string bitRate = "";  // 模块速率
        public static string softType = "";   // SR-850nm LR LRM ZR EZR DWDM
        public static string softVer = "";    // 1-15
        public static bool chipIsOK = false;   // work is ok ?
        public static bool wpIsEn = false;     // wp is enable?
        public static bool moduleIsSR = false; // module is sr-850nm ?
        public static bool moudleIsapd = false;//标识带APD模块
        public static string scheme_Ver = "";
        public static bool Test_ok = false;
        public static int test_status = 0;//0:未开始测试，橙色；1：开始测试，白色；2：测试成功 绿色；3：测试失败 红色
        public static int testnum = 0;


    }

    public class TestResult2
    {
        public static byte[] txEye_image = null;
        public static byte[] flash_data = new byte[2048];//1024
        public static int flash_data_len = 2048;//1024
        public static int bimage_len = 0;
        public static int waveforms_count = 0;
        public static int mask_margin = 1;
        public static string mask_name = "10GbE_10_3125_May02.msk";

        public static UInt64 max_Fsn = 999999999999; // 12位

        public static string fibertop_bn = "00000"; // 生产单号
        public static string tosa_sn = "11111";
        public static string rosa_sn = "22222";

        public static string fibertop_sn = "";
        public static string fibertop_pn = "";

        public static string sn = "";
        public static string pn = "";
        public static string vn = "";
        public static string date = "";

        public static float tempDDM = 0;
        public static float vccDDM = 0;
        public static float txBiasDDM = 0;
        public static float txPowerDDM = 0;
        public static float rxPowerDDM = 0;

        public static float tempHA = 0;
        public static float tempLA = 0;
        public static float tempHW = 0;
        public static float tempLW = 0;

        public static float vccHA = 0;
        public static float vccLA = 0;
        public static float vccHW = 0;
        public static float vccLW = 0;

        public static float txBiasHA = 0;
        public static float txBiasLA = 0;
        public static float txBiasHW = 0;
        public static float txBiasLW = 0;

        public static float txPowerHA = 0;
        public static float txPowerLA = 0;
        public static float txPowerHW = 0;
        public static float txPowerLW = 0;

        public static float rxPowerHA = 0;
        public static float rxPowerLA = 0;
        public static float rxPowerHW = 0;
        public static float rxPowerLW = 0;

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


        // 真实测试值
        ////////////////////////////////
        public static float txPowerDCA = 0;
        public static float txPower = 0;
        public static float txEr = 0;
        public static float txESN = 0;
        public static float txCrossing = 0;
        public static float txJiterRMS = 0;
        public static float txJiterPP = 0;
        public static float txJiterTT = 0; //Jitter total
        public static float TxRiseTime = 0.0f;//2024.12.07
        public static float TxFallTime = 0.0f;//2024.12.07
        public static double wLength = 0;//波长 2025.05.21 
        public static double smsr = 0;//边模抑制比 2025.09.11
        public static double spectralwidth = 0;//谱宽 2025.09.11
        public static double supply = 0;//电流（功耗）2025.09.11
        public static float TxEyeAmp = 0;//眼图幅度 2025.09.12 
        //
        public static float txPwrErr = 0;
        public static float txErErr = 0;

        public static float[] rxPwrErr = new float[5];
        public static float[] rxPwrReal = new float[5];
        public static float[] rxPwrDDM = new float[5];

        public static float rxSen = 0;
        public static float rxDLos = 0;
        public static float rxALos = 0;
        public static float rxOverLoad = 0;
        //
        public static UInt16 txapcVal = 0;
        public static UInt16 txmodVal = 0;
        public static UInt16 rxlosVal = 0;
        public static UInt16 rxapdVal = 0;
        public static UInt16 txtosaTemp = 0;
        public static UInt16 txVON = 0;
        public static UInt16 txCPA = 0;
        //
        public static float txPwrCal_k = 0;
        public static float txPwrCal_b = 0;
        public static float[] rxPwrCal_c = new float[5];
        public static byte rxNoPwrVal = 0;
        //
        public static float[] rxPwrCal_k = new float[3]; //for UX3320C 三折线
        public static float[] rxPwrCal_b = new float[3]; //for UX3320C 三折线
        public static UInt16[] rxAdcCal = new UInt16[6]; //for UX3320C 三折线
        //
        public static string tester_no;
        //
        public static Byte txpeVal = 0; //2017.8.21

        //
        public static string chipType = ""; // LDD+LA+MCU
        public static string bitRate = "";  // 模块速率
        public static string softType = "";   // SR-850nm LR LRM ZR EZR DWDM
        public static string softVer = "";    // 1-15
        public static bool chipIsOK = false;   // work is ok ?
        public static bool wpIsEn = false;     // wp is enable?
        public static bool moduleIsSR = false; // module is sr-850nm ?
        public static bool moudleIsapd = false;//标识带APD模块
        public static string scheme_Ver = "";
        public static bool Test_ok = false;
        public static int test_status = 0;//0:未开始测试，橙色；1：开始测试，白色；2：测试成功 绿色；3：测试失败 红色
        public static int testnum = 0;
    }
    public class TestResult3
    {
        public static byte[] txEye_image = null;
        public static byte[] flash_data = new byte[2048];//1024
        public static int flash_data_len = 2048;//1024
        public static int bimage_len = 0;
        public static int waveforms_count = 0;
        public static int mask_margin = 1;
        public static string mask_name = "10GbE_10_3125_May02.msk";

        public static UInt64 max_Fsn = 999999999999; // 12位

        public static string fibertop_bn = "00000"; // 生产单号
        public static string tosa_sn = "11111";
        public static string rosa_sn = "22222";

        public static string fibertop_sn = "";
        public static string fibertop_pn = "";

        public static string sn = "";
        public static string pn = "";
        public static string vn = "";
        public static string date = "";

        public static float tempDDM = 0;
        public static float vccDDM = 0;
        public static float txBiasDDM = 0;
        public static float txPowerDDM = 0;
        public static float rxPowerDDM = 0;

        public static float tempHA = 0;
        public static float tempLA = 0;
        public static float tempHW = 0;
        public static float tempLW = 0;

        public static float vccHA = 0;
        public static float vccLA = 0;
        public static float vccHW = 0;
        public static float vccLW = 0;

        public static float txBiasHA = 0;
        public static float txBiasLA = 0;
        public static float txBiasHW = 0;
        public static float txBiasLW = 0;

        public static float txPowerHA = 0;
        public static float txPowerLA = 0;
        public static float txPowerHW = 0;
        public static float txPowerLW = 0;

        public static float rxPowerHA = 0;
        public static float rxPowerLA = 0;
        public static float rxPowerHW = 0;
        public static float rxPowerLW = 0;

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


        // 真实测试值
        ////////////////////////////////
        public static float txPowerDCA = 0;
        public static float txPower = 0;
        public static float txEr = 0;
        public static float txESN = 0;
        public static float txCrossing = 0;
        public static float txJiterRMS = 0;
        public static float txJiterPP = 0;
        public static float txJiterTT = 0; //Jitter total
        public static float TxRiseTime = 0.0f;//2024.12.07
        public static float TxFallTime = 0.0f;//2024.12.07
        public static double wLength = 0;//波长 2025.05.21 
        public static double smsr = 0;//边模抑制比 2025.09.11
        public static double spectralwidth = 0;//谱宽 2025.09.11
        public static double supply = 0;//电流（功耗）2025.09.11
        public static float TxEyeAmp = 0;//眼图幅度 2025.09.12 
        //
        public static float txPwrErr = 0;
        public static float txErErr = 0;

        public static float[] rxPwrErr = new float[5];
        public static float[] rxPwrReal = new float[5];
        public static float[] rxPwrDDM = new float[5];

        public static float rxSen = 0;
        public static float rxDLos = 0;
        public static float rxALos = 0;
        public static float rxOverLoad = 0;
        //
        public static UInt16 txapcVal = 0;
        public static UInt16 txmodVal = 0;
        public static UInt16 rxlosVal = 0;
        public static UInt16 rxapdVal = 0;
        public static UInt16 txtosaTemp = 0;
        public static UInt16 txVON = 0;
        public static UInt16 txCPA = 0;
        //
        public static float txPwrCal_k = 0;
        public static float txPwrCal_b = 0;
        public static float[] rxPwrCal_c = new float[5];
        public static byte rxNoPwrVal = 0;
        //
        public static float[] rxPwrCal_k = new float[3]; //for UX3320C 三折线
        public static float[] rxPwrCal_b = new float[3]; //for UX3320C 三折线
        public static UInt16[] rxAdcCal = new UInt16[6]; //for UX3320C 三折线
        //
        public static string tester_no;
        //
        public static Byte txpeVal = 0; //2017.8.21

        //
        public static string chipType = ""; // LDD+LA+MCU
        public static string bitRate = "";  // 模块速率
        public static string softType = "";   // SR-850nm LR LRM ZR EZR DWDM
        public static string softVer = "";    // 1-15
        public static bool chipIsOK = false;   // work is ok ?
        public static bool wpIsEn = false;     // wp is enable?
        public static bool moduleIsSR = false; // module is sr-850nm ?
        public static bool moudleIsapd = false;//标识带APD模块
        public static string scheme_Ver = "";
        public static bool Test_ok = false;
        public static int test_status = 0;//0:未开始测试，橙色；1：开始测试，白色；2：测试成功 绿色；3：测试失败 红色
        public static int testnum = 0;
    }

    public class TestResult4
    {
        public static byte[] txEye_image = null;
        public static byte[] flash_data = new byte[2048];//1024
        public static int flash_data_len = 2048;//1024
        public static int bimage_len = 0;
        public static int waveforms_count = 0;
        public static int mask_margin = 1;
        public static string mask_name = "10GbE_10_3125_May02.msk";

        public static UInt64 max_Fsn = 999999999999; // 12位

        public static string fibertop_bn = "00000"; // 生产单号
        public static string tosa_sn = "11111";
        public static string rosa_sn = "22222";

        public static string fibertop_sn = "";
        public static string fibertop_pn = "";

        public static string sn = "";
        public static string pn = "";
        public static string vn = "";
        public static string date = "";

        public static float tempDDM = 0;
        public static float vccDDM = 0;
        public static float txBiasDDM = 0;
        public static float txPowerDDM = 0;
        public static float rxPowerDDM = 0;

        public static float tempHA = 0;
        public static float tempLA = 0;
        public static float tempHW = 0;
        public static float tempLW = 0;

        public static float vccHA = 0;
        public static float vccLA = 0;
        public static float vccHW = 0;
        public static float vccLW = 0;

        public static float txBiasHA = 0;
        public static float txBiasLA = 0;
        public static float txBiasHW = 0;
        public static float txBiasLW = 0;

        public static float txPowerHA = 0;
        public static float txPowerLA = 0;
        public static float txPowerHW = 0;
        public static float txPowerLW = 0;

        public static float rxPowerHA = 0;
        public static float rxPowerLA = 0;
        public static float rxPowerHW = 0;
        public static float rxPowerLW = 0;

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


        // 真实测试值
        ////////////////////////////////
        public static float txPowerDCA = 0;
        public static float txPower = 0;
        public static float txEr = 0;
        public static float txESN = 0;
        public static float txCrossing = 0;
        public static float txJiterRMS = 0;
        public static float txJiterPP = 0;
        public static float txJiterTT = 0; //Jitter total
        public static float TxRiseTime = 0.0f;//2024.12.07
        public static float TxFallTime = 0.0f;//2024.12.07
        public static double wLength = 0;//波长 2025.05.21 
        public static double smsr = 0;//边模抑制比 2025.09.11
        public static double spectralwidth = 0;//谱宽 2025.09.11
        public static double supply = 0;//电流（功耗）2025.09.11
        public static float TxEyeAmp = 0;//眼图幅度 2025.09.12 
        //
        public static float txPwrErr = 0;
        public static float txErErr = 0;

        public static float[] rxPwrErr = new float[5];
        public static float[] rxPwrReal = new float[5];
        public static float[] rxPwrDDM = new float[5];

        public static float rxSen = 0;
        public static float rxDLos = 0;
        public static float rxALos = 0;
        public static float rxOverLoad = 0;
        //
        public static UInt16 txapcVal = 0;
        public static UInt16 txmodVal = 0;
        public static UInt16 rxlosVal = 0;
        public static UInt16 rxapdVal = 0;
        public static UInt16 txtosaTemp = 0;
        public static UInt16 txVON = 0;
        public static UInt16 txCPA = 0;
        //
        public static float txPwrCal_k = 0;
        public static float txPwrCal_b = 0;
        public static float[] rxPwrCal_c = new float[5];
        public static byte rxNoPwrVal = 0;
        //
        public static float[] rxPwrCal_k = new float[3]; //for UX3320C 三折线
        public static float[] rxPwrCal_b = new float[3]; //for UX3320C 三折线
        public static UInt16[] rxAdcCal = new UInt16[6]; //for UX3320C 三折线
        //
        public static string tester_no;
        //
        public static Byte txpeVal = 0; //2017.8.21

        //
        public static string chipType = ""; // LDD+LA+MCU
        public static string bitRate = "";  // 模块速率
        public static string softType = "";   // SR-850nm LR LRM ZR EZR DWDM
        public static string softVer = "";    // 1-15
        public static bool chipIsOK = false;   // work is ok ?
        public static bool wpIsEn = false;     // wp is enable?
        public static bool moduleIsSR = false; // module is sr-850nm ?
        public static bool moudleIsapd = false;//标识带APD模块
        public static string scheme_Ver = "";
        public static bool Test_ok = false;
        public static int test_status = 0;//0:未开始测试，橙色；1：开始测试，白色；2：测试成功 绿色；3：测试失败 红色
        public static int testnum = 0;
    }
    public interface I2C
    {
        bool TWI_Open();
        bool TWI_WriteByte(byte DeviceAddress, byte WriteDataByteAddress, byte WriteData);
        byte TWI_ReadByte(byte DeviceAddress, byte WriteDataByteAddress);
        uint TWI_WritePage(byte DeviceAddress, byte WriteDataByteAddress, byte[] WriteDataBuffer, uint num);
        uint TWI_ReadPage(byte DeviceAddress, byte ReadDataByteAddress, byte[] ReadDataBuffer, uint num);
        bool setModuleDis(bool dis);    
        bool HardWare_LOS_Get();
        bool TWI_Close();
    }

    public interface EVB
    {
        bool Open(string ipAddress, int port, int timeOut);
        void Close();
        string GetPower(int slot = 1);
        string GetCurrent(int slot = 1);
        string GetVoltage(int slot = 1);
        bool SetVoltage(double voltage, int slot = 1);
        bool IIC_Set(string deviceAddr, string regAddr, string dataLength, string data, int slot = 1);
        string IIC_Get(string deviceAddr, string regAddr, string dataLength, int slot = 1);
        bool SetDeviceIP(string newIP);
    }

    public interface ModuleTest
    {
        void Init(EVB i2c, byte Dut); // 初始化测试参数,必须先调用.

        bool CheckTestTypeInfo(); // 检查待测模块类型是否正确

        bool SoftTxDis(bool txDis); //软件Tx_Dis控制

        bool SetDebugPWD();   // 写入调试密码,进入调试模式.
        byte CheckDebugPWD(); // 检查模块是否在调试模式状态下.
        bool CheckRxLOS();    // 检查LOS状态

        float GetTemp();      // 获取模块DDM的Temp值
        float GetVCC();       // 获取模块DDM的VCC值
        float GetTxBias();    // 获取模块DDM的Tx Bias值
        float GetTxPower();   // 获取模块DDM的Tx Power值
        float GetRxPower();   // 获取模块DDM的Rx Power值

        bool GetDDMAnalogValues(); // 获取模块DDM的5个变量信息 Temp/Vcc/Bias/Tx_Power/Rx_Power
        bool GetDDMThresholds();   // 获取模块告警门限信息
        bool GetDDMFlagsInterrupt(); // 获取模块告警警告位标识

        bool GetFlashInfo();
        bool GetFlashInfoDebug();

        bool CheckThresholdsInfo(ref string errMsg);
        bool CheckModuleFlashInfo(ref string errMsg);

        bool GetModuleTypeFromAccessdb(ref string[] str, ref int len);
        bool GetTypeDebugInfoFromAccessdb();

        // 初测调试功能函数
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        bool DisTxRxCDR(bool disVal);
        bool InitModule();
        bool SetTxApcBias(UInt16 setVal);
        bool SetTxModBias(UInt16 setVal);
        bool SetRxLos(UInt16 setVal);
        bool setAPD(UInt16 setVal);
        bool setWaveLength(UInt16 setval);
        bool setVON(UInt16 setval);
        bool setCPA(UInt16 setVal);
        UInt16 GetRxADC();
        UInt16 GetTxADC();
        bool WriteRxCalData();
        bool WriteTxCalData();
        bool SaveRxDataAfterDebug();
        bool SaveTxDataAfterDebug();
        bool TxTempLookupTableCtrl(bool enable);
        //
        bool WriteTxRxDefaultVal(); //2017.8.21
        bool EEPROMcheckSum();//2026.05.06
        bool elec_moudleTest();//2026.06.03
        bool Get_HardWare_LOS();//2026.06.06
        bool SetModuleDis(bool dis);//2026.06.06

        //////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
