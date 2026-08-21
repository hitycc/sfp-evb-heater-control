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
    //添加工位枚举
    public enum ER1TestStation
    {
        Tx,  // 发射工位
        Rx   // 接收工位
    }
    public class GlobalVarFun
    {
        //添加全局静态变量 代码直接硬编码设置
        public static ER1TestStation er1Station = ER1TestStation.Tx;
        public static byte txpwr_debug_method = 0x00; // 0x00:线性计算法 apc-->uw & bias   0x11: 普通二分法 apc-->dBm   22:差值二分法 apc-->uW
        public static byte txer_debug_method = 0x00; 
        public static bool k_lut_flag = false; // 补偿表： true = 比例缩放K    false = 平移方法
        //
        public static string testType = "";
        public static string moduleType = "QSFP";
        public static string moudlefpn = "QFP-MM85FG-S1DC";
        public static string moduleLutDBFilePath = "";

        public static bool i2c_can_use  = false;
        public static bool usb_can_use  = false;
        public static bool usb_i2c_open = false;
        public static bool sql_connect_status = false;
        public static bool sql_record_status  = false;
        public static bool access_connect_status = false;
        public static bool access_updated_status = false;

        public static bool power_use_DAC = false;
        public static bool rx_ddm_test = false;
        public static bool rx_los_test = false;
        public static bool rx_nopower_test = true;
        public static bool tx_test = true;
        public static bool tx_nopower_test = true;
        public static bool sen_test = false;
        public static bool tx_jitter_test = true;
        public static int waveforms_num = 0;
        public static bool cob_ld = true;
        public static bool hw_txdis_test = false;
        public static bool hw_los_test = false;
        public static bool tx_eye_save_test = false;
        public static bool threshold_check = false;
        public static bool flash_check = false;
        public static bool txrx_cdr_dis = false;
        public static bool distype_check = false;
        public static bool tx_pe_test = false;
        public static bool testDataIsOK = false;
        public static bool tx_tec_test = false;
        public static bool TOSATempEN = false;
        public static bool VONEN = false;
        public static bool APDen = false;
        public static bool DCA86100D_Open = false;
        public static bool N1092x_Open = false;

        public static bool rx_is_apd = false;

        public static int type_index = 0;
        public static int tx_pe = 0;
        public static double ER_cal_num = 0.5;
        public static double tx_cal_num = 0.8;
        public static double rx_cal_num = 1.2;
        public static double opto_att_offset = 3.5;
        public static double[] opto_att_offsetbuf = { 3.5, 3.5, 3.5, 3.5 };
        public static double  wLengthMaxErr = 0.02;

        public static string apc_min = "30";
        public static string apc_max = "150";
        public static string mod_min = "30";
        public static string mod_max = "150";
        public static string los_min = "20";
        public static string los_max = "80";

        public static I2C iic;
        public static CP2112 USBtoI2C;
        //public static CP2112 usbtoi2c;
        public static SqlConnection sqlconnection;
        public static ModuleTest mTest;
        public static  AgilentInfiniiumDCA scope;
        public static SerialPort uartMeter;
        public static SerialPort uartAtt;
        public static SerialPort pssert;//
        public static SerialPort opticalSwitch;
        public static Keysight86120C kt86120c;
        public static DCA_86100 scope_86100d;
        public static string gpibname = "";

        public static bool optoMeter_connected = false;
        public static bool optoAtt_connected = false;
        public static bool optoAtt_new_connected = false;
        public static bool instrument_connected = false;
        public static bool pssbert_connected = false;
        public static bool optoSwitch_connected = false;
        public static bool wlength_connected = false;
        public static bool dca86100d_connected = false;

        public static bool record_need_save = false;

        public static bool test_tx_select = true;
        public static int pss_bert_delay = 100;
        ////////////////////////////////////////////////////////////////////////////////////////////
        public static bool GetRegisterInfo()
        {
            return true;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////
    }

    // 数字光衰减器
    public class DOA
    {
        public static float currentAtt = 0;
        public static float rxSenAtt   = 0;
        public static float rxDLosAtt  = 0;
        public static float rxALosAtt  = 0;
        public static float rxOverLoadAtt = 0;

        public static float[] rxSenAttBuf = new float[4];
        public static float[] rxDLosAttBuf = new float[4];
        public static float[] rxALosAttBuf = new float[4];
        public static float[] rxOverLoadAttBuf = new float[4];

        public static float[] rxCheckAtt = new float[20];//5
        public static float[] rxCalAtt = new float[20];//5
        //public static float[] rxCalAtt1 = new float[5];
        //public static float[] rxCalAtt2 = new float[5];
        //public static float[] rxCalAtt3 = new float[5];
        public static  int  com_index = 0;
        public static string com_portname = "COM1";
        public static int delay = 10;

        public static string overload_att = "0.0";
        public static string sen_att = "0.0";
        public static string A_los_att = "0.0";
        public static string D_los = "0.0";

        public static string check_att1 = "0.0";
        public static string check_att2 = "0.0";
        public static string check_att3 = "0.0";
        public static string check_att4 = "0.0";
        public static string check_att5 = "0.0";
    }
    //误码仪
    public class BIT_ERROR
    {
        public static int com_index = 0;
        public static string  ch ="CH0";
        public static string com_portname = "COM1";
        public static int delay = 0;
    }
    //光功率计
    public class MEMTER
    {
        public static int com_index = 0;
        public static int type_index = 1;
        public static string com_portname = "COM1";
        public static int delay = 500;
    }
    //光开关
    public class opcicalSwitch
    {
        public static int com_index = 0;
        public static string com_portname = "COM1";
        public static int delay = 0;
    }
    //眼图仪
    public class Agilent86100
    {
        public static Double ER_offset = 0.0;
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

        public static UInt16 txapc_Min_set = 10;
        public static UInt16 txapc_Max_set = 100;
        public static UInt16 txmod_Min_set = 20;
        public static UInt16 txmod_Max_set = 100;
        public static UInt16 rxlos_Min_set = 20;
        public static UInt16 rxlos_Max_set = 100;

        public static UInt16 rxapd_min = 70;
        public static UInt16 rxapd_max = 90;
        public static UInt16 tosatemp_min = 830;
        public static UInt16 tosatemp_max = 1830;
        public static UInt16 tosatemp_def = 900;
        public static UInt16 von_min = 0;
        public static UInt16 von_max = 4095;

        public static float[] rxPwr_Real = new float[5];
        public static float rx_Sen = 0;
        public static float rx_DLos = 0;
        public static float rx_ALos = 0;
        public static float rx_OverLoad = 0;
         public static UInt16 tosa_temp = 830;
        public static byte Tx_von = 0;
        public static byte rx_apd = 255;

        public static float[] rxPwr_Cal = new float[5];

        public static int ch = 0;

        public static string bias_ddm = "0.0/0.0/0.0/0.0";
        public static string txpwr_ddm = "-40/-40/-40/-40";
        public static string rxpwr_ddm = "-40/-40/-40/-40";

        public static float[] wl_min = new float[4];
        public static float[] wl_max = new float[4];
        public static float[] wl_target = new float[4];

        public static UInt16[] tosa_tempbufmin = new UInt16[4];
        public static UInt16[] tosa_tempbufmax = new UInt16[4];

        public static UInt16 tosa_tempValmin = 0;
        public static UInt16 tosa_tempValmax = 0;

        public static int EMLTestType = 0;

       
    }

    public class TestResult
    {
        public static byte[,] txEye_imagebuf = null;
        public static byte[] txEye_image_ch0 = null;
        public static byte[] txEye_image_ch1 = null;
        public static byte[] txEye_image_ch2 = null;
        public static byte[] txEye_image_ch3 = null;
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
        public static string bias_ddm = "0.0/0.0/0.0/0.0";
        public static string txpwr_ddm = "-40/-40/-40/-40";
        public static string rxpwr_ddm = "-40/-40/-40/-40";

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

        public static float[] txBiasDDMbuf = new float[4];
        public static float[] txPowerDDMbuf = new float[4];
        public static float[] rxPowerDDMbuf = new float[4];

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

        public static float[] txPowerbuf = new float[4];
        public static float[] txErbuf = new float[4];
        public static float[] txESNbuf = new float[4];
        public static float[] txCrossingbuf = new float[4];
        public static float[] txJiterRMSbuf = new float[4];
        public static float[] txJiterPPbuf = new float[4];
        public static float[] txJiterTTbuf = new float[4]; //Jitter total
        //
        public static float txPwrErr = 0;
        public static float txErErr  = 0;
        public static float[] txPwrErrbuf = new float[4];
        public static float[] txErErrbuf = new float[4];

        public static float[] rxPwrErr  = new float[5];
        public static float[] rxPwrReal = new float[5];
        public static float[] rxPwrDDM  = new float[5];

        public static float[,] rxPwrErrbuf = new float[4, 5];
        public static float[,] rxPwrRealbuf = new float[4, 5];
        public static float[,] rxPwrDDMbuf = new float[4, 5];
        
        public static float rxSen = 0;
        public static float rxDLos = 0;
        public static float rxALos = 0;
        public static float rxOverLoad = 0;

        public static float[] rxSenbuf = { 0, 0, 0, 0 };
        public static float[] rxDLosbuf = { 0, 0, 0, 0 };
        public static float[] rxALosbuf = { 0, 0, 0, 0 };
        public static float[] rxOverLoadbuf = { 0, 0, 0, 0 };
        public static double[] wLength = { 0, 0, 0, 0 };  
        //
        public static UInt16 txapcVal = 0;
        public static UInt16 txmodVal = 0;
        public static UInt16 rxlosVal = 0;

        public static UInt16 rxapdVal = 0;
        public static UInt16 txtosaTemp = 0;
        public static UInt16 txVON = 0;
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
        public static bool chipIsOK = false;   // 模块芯片工作状态是不是OK
        public static bool wpIsEn = false;     // wp is enable?
        public static bool moduleIsSR = false; // module is sr-850nm ?

        public static int ch = 0;
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

    public interface ModuleTest
    {
        bool SetRxVagc(UInt16 setVal); //调节 RxVagc
        bool SaveER1Calibration(); //保存ER1校准数据至Flash

        void Init(I2C i2c); // 初始化测试参数,必须先调用.

        bool CheckTestTypeInfo(); // 检查待测模块类型是否正确

        bool SoftTxDis(bool txDis); //软件Tx_Dis控制
        bool SoftTxCHEn(int CH);//软件CH Tx_En控制
        bool SourceSoftEn(int CH);//光源通道单开
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
        byte GetTxApcBiasSet();
        byte GetTxModBiasSet();
        double GetTxPwr();

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
        UInt16 GetRxADC();
        UInt16 GetTxADC();
        bool WriteRxCalData();
        bool WriteTxCalData();
        bool SaveRxDataAfterDebug();
        bool SaveTxDataAfterDebug();
        bool TxTempLookupTableCtrl(bool enable);
        //
        bool WriteTxRxDefaultVal(); //2017.8.21
        bool SetTx_EN();
        bool SetAPD(UInt16 setVal);
        bool SetVON(UInt16 setVal);
        bool SetTOSATemp(UInt16 setVal);
        //////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}
