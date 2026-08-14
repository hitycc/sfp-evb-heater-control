using Agilent.AgilentInfiniiumDCA.Interop;
using DCAX_86100;
using FibertopTest_Common;
using Ivi.Visa.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SFPXFP自动测试软件多端口
{
    //public class TestControl:UserControl
    public class TestControl
    {
        #region  //设备 参数 初始化
        public Services sqlserver;
        public static AgilentInfiniiumDCA scope;// = new AgilentInfiniiumDCAClass(); //创建一个86100DCA对象;
        public static AgilentE3632A agilent_e3632a = new AgilentE3632A();
        public static DCA_86100 scope_86100d = new DCA_86100();
        public static Keysight86120C kt86120c = new Keysight86120C();
        public static MS9710B ms9710b = new MS9710B();
        public OpticalAttenuator opticaldoaatt = new OpticalAttenuator();
        public static OpticalPowerMeter opticalmeter = new OpticalPowerMeter();
        public static PssBertController pssbert = new PssBertController();
        public static Photoswitch1x4 opticalswitch = new Photoswitch1x4();
        public static OTP12Driver otp12 = new OTP12Driver();

        BackgroundWorker backgroundWorkerAutoSet;
        //设备接口
        //I2C i2c;// = new TWI() as I2C;
        EVB i2c;
        ModuleTest test = new SFPUX3320T();

        //Rx ADC
        UInt16[] rxAdc = new UInt16[6];
        float rxPwrMaxErr = 1; // 接收DDM校准检查精度
        float txPwrMaxErr = 1; // 发射DDM校准检查精度
        float erValMaxErr = 1; // 发射消光比精度
        double wLengthMaxErr = 1;
        float meter_err = 0;
        public static string gpibAddress = "TCPIP0::localhost::inst0::INSTR";//"GPIB0::07::INSTR";
        //设备锁
        private static readonly object dca_lock = new object();
        private static readonly object tx_lock = new object();
        private static readonly object tx_meter = new object();
        //public static readonly object tx_dca = new object();
        private static readonly object rx_lock = new object();
        //信号量
        private static readonly SemaphoreSlim dcaSemaphore = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim switchSemaphore = new SemaphoreSlim(1, 1);
        //异步等待延时
        private static int waittimes = 10;
        byte Dut = 1;
        int meter_ch = 0;
        //日志
        public delegate void DelegateListBoxShow(object sender, ReturnReuslt result);
        public DelegateListBoxShow ModListBoxShow;
        //错误信息
        ReturnTxRxResult retutntxrxresult = new ReturnTxRxResult();
        #endregion

        #region //AddTestLog
        private void AddTestLog(string strMessage)
        {
            retutntxrxresult.ErrorMessage = strMessage;
            ReturnReuslt result = new ReturnReuslt();
            result.message = strMessage;
            ModListBoxShow(this, result);
        }
        #endregion

        #region // 将光功率 从dBm转换为uW
        private float ConvertdBmtouW(float dBm)
        {
            return (float)(Math.Pow(10, 0.1 * dBm) * 10000.0);
        }
        #endregion

        #region  //参数变量初始化

        public TestControl(byte dut_num)
        {
            //设置初始化
            if (IsInDesignMode())
            {
                return;
            }
            GlobalVarFun.setup = new SetupSelect();
            GlobalVarFun.Language = "Chinese";
            GlobalVarFun.setup.ag_e3632a_com = "";
            GlobalVarFun.setup.ag_e3632a_connect = false;
            GlobalVarFun.setup.algorithm_25g_lr = false;
            GlobalVarFun.setup.algorithm_cob_ld = true;
            //GlobalVarFun.setup.bert_ch_a = 1;
            //GlobalVarFun.setup.bert_ch_b = 2;
            GlobalVarFun.setup.bert_com = "";
            GlobalVarFun.setup.bert_connect = false;
            GlobalVarFun.setup.bert_delay = 100;
            GlobalVarFun.setup.dac_txpwr_err = 0;
            GlobalVarFun.setup.dca_86100d = false;
            GlobalVarFun.setup.dca_connect = false;
            GlobalVarFun.setup.dca_er_err = 0.0f;
            GlobalVarFun.setup.dca_gpib = "";
            GlobalVarFun.setup.dca_n1092x = false;
            GlobalVarFun.setup.doa_com = "";

            GlobalVarFun.setup.doa_delay = 10;
            GlobalVarFun.setup.electrical_module = false;

            GlobalVarFun.setup.flash_check = true;
            GlobalVarFun.setup.image_save = false;
            GlobalVarFun.setup.init_module = false;
            GlobalVarFun.setup.kt86120x_com = "";
            GlobalVarFun.setup.kt86120x_connect = false;
            GlobalVarFun.setup.meter_ch_a = TestSet.meter_ch;
            GlobalVarFun.setup.meter_ch_b = TestSet2.meter_ch;
            GlobalVarFun.setup.meter_com = "";
            GlobalVarFun.setup.meter_connect = false;
            GlobalVarFun.setup.meter_delay = GlobalVarFun.meterdealy;//500
            GlobalVarFun.setup.meter_err_dut1 = TestSet.meter_pwr_err;
            GlobalVarFun.setup.meter_err_dut2 = TestSet2.meter_pwr_err;
            GlobalVarFun.setup.meter_err_dut3 = TestSet3.meter_pwr_err;
            GlobalVarFun.setup.meter_err_dut4 = TestSet4.meter_pwr_err;
            GlobalVarFun.setup.ms9710x_com = "";
            GlobalVarFun.setup.ms9710x_connect = false;


            GlobalVarFun.setup.rx_apd_test = false;
            GlobalVarFun.setup.rx_ddm_test = false;
            GlobalVarFun.setup.rx_hardware_los = true;
            GlobalVarFun.setup.rx_los_test = false;
            GlobalVarFun.setup.rx_nopwr_test = false;
            GlobalVarFun.setup.rx_sen_test = false;
            GlobalVarFun.setup.rx_test = true;
            GlobalVarFun.setup.rx_apd_cal = false;
            GlobalVarFun.setup.scheme_check_dis = false;
            GlobalVarFun.setup.spectral_wideth = 0;
            GlobalVarFun.setup.threshold_check = true;


            GlobalVarFun.setup.tx_eml_test = false;
            GlobalVarFun.setup.tx_hardware_disable = true;
            GlobalVarFun.setup.tx_jitter_test = false;
            GlobalVarFun.setup.tx_nopwr_test = true;
            GlobalVarFun.setup.tx_pe_test = false;
            GlobalVarFun.setup.tx_rx_cdr_dis = false;
            GlobalVarFun.setup.tx_test = true;
            GlobalVarFun.setup.tx_use_dca_txpwr = false;
            GlobalVarFun.setup.waveforms_num = 0;
            GlobalVarFun.setup.wlgth_cal = 0;
            GlobalVarFun.setup.meterType_hand = false;//默认台式功率计
            GlobalVarFun.setup.otp12_connect = false;

            scope = new AgilentInfiniiumDCAClass();
            Dut = dut_num;//端口初始化
            sqlserver = new Services();
            switch (Dut)
            {
                case 1:
                    test = GlobalVarFun.mTest;
                    meter_ch = TestSet.meter_ch;
                    meter_err = TestSet.meter_pwr_err;
                    i2c = GlobalVarFun.Evb;
                    GlobalVarFun.setup.er_cal = TestSet.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet.txpwr_cal;
                    GlobalVarFun.setup.doa_connect = false;
                    break;
                case 2:
                    test = GlobalVarFun.mTest2;
                    meter_ch = TestSet2.meter_ch;
                    meter_err = TestSet2.meter_pwr_err;
                    i2c = GlobalVarFun.Evb;
                    GlobalVarFun.setup.er_cal = TestSet2.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet2.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet2.txpwr_cal;
                    GlobalVarFun.setup.doa_connect2 = false;
                    break;
                case 3:
                    test = GlobalVarFun.mTest3;
                    meter_ch = TestSet3.meter_ch;
                    meter_err = TestSet3.meter_pwr_err;
                    i2c = GlobalVarFun.Evb;
                    GlobalVarFun.setup.er_cal = TestSet3.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet3.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet3.txpwr_cal;
                    GlobalVarFun.setup.doa_connect3 = false;
                    break;
                case 4:
                    test = GlobalVarFun.mTest4;
                    meter_ch = TestSet4.meter_ch;
                    meter_err = TestSet4.meter_pwr_err;
                    i2c = GlobalVarFun.Evb;
                    GlobalVarFun.setup.er_cal = TestSet4.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet4.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet4.txpwr_cal;
                    GlobalVarFun.setup.doa_connect4 = false;
                    break;
                default:
                    break;
            }

            //初始化后台代理
            InitializeBackgoundWorker();
            test.Init(i2c, Dut);
        }

        private bool IsInDesignMode()
        {
            return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;
        }
        #endregion

        public byte TWI_ReadByte(int deviceAddr, int regAddr, byte dut)
        {
            byte[] b = new byte[1];
            if (TWI_ReadPage(deviceAddr, regAddr, b, 1, dut) == 1) return b[0];
            return 0;
        }
        public int TWI_ReadPage(int deviceAddr, int regAddr, byte[] buf, int len, byte dut)
        {
            try
            {
                string dA = $"{(deviceAddr & 0xFF):X2}";
                string rA = $"{(regAddr & 0xFF):X2}";
                string resp = i2c.IIC_Get(dA, rA, len.ToString(), dut);
                if (string.IsNullOrEmpty(resp)) return 0;
                var matches = Regex.Matches(resp, @"(?:0x)?([0-9a-fA-F]{2})\b");
                int n = 0;
                foreach (Match m in matches)
                {
                    if (n >= len) break;
                    buf[n] = Convert.ToByte(m.Groups[1].Value, 16);
                    n++;
                }
                return n;
            }
            catch { return 0; }
        }

        public bool TWI_WriteByte(int deviceAddr, int regAddr, int val, byte dut)
        {
            byte[] b = new byte[] { (byte)val };
            return TWI_WritePage(deviceAddr, regAddr, b, 1, dut) == 1;
        }

        public int TWI_WritePage(int deviceAddr, int regAddr, byte[] buf, int len, byte dut)
        {
            try
            {
                string dA = $"{(deviceAddr & 0xFF):X2}";
                string rA = $"{(regAddr & 0xFF):X2}";
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append($"{buf[i]:X2}");
                }
                bool ok = i2c.IIC_Set(dA, rA, len.ToString(), sb.ToString(), dut);
                return ok ? len : 0;
            }
            catch { return 0; }
        }

        #region
        public bool GetTypeDebugInfoFromAccessdb()
        {
            return test.GetTypeDebugInfoFromAccessdb();
        }

        public bool GetModuleTypeFromAccessdb(ref string[] str, ref int len)
        {
            return test.GetModuleTypeFromAccessdb(ref str, ref len);
        }
        #endregion

        #region  //写码信息
        public bool Read_moduleInfo()
        {
            return test.GetFlashInfo();
        }

        public async void Read_moduleInfo_Async()
        {

            // 将耗时的硬件读取操作放到后台线程
            try
            {
                await Task.Run(() =>
                {
                    test.GetFlashInfo();
                });
            }
            catch
            { }
            finally
            {
                // 无论如何都要释放信号量
                //GlobalVarFun.BusSemaphore.Release();
            }
        }
        #endregion

        #region //告警门限
        public bool Read_AlarmWarn_Thresholds()
        {
            return test.GetDDMThresholds();
        }

        public async void Read_AlarmWarn_Thresholds_Async()
        {

            // 将耗时的硬件读取操作放到后台线程
            try
            {
                await Task.Run(() =>
                {
                    test.GetDDMThresholds();
                });
            }
            catch
            { }
            finally
            {
                // 无论如何都要释放信号量
                // GlobalVarFun.BusSemaphore.Release();
            }

        }
        #endregion

        #region //告警标志
        public bool Read_Flags_and_Interrupt()
        {
            return test.GetDDMFlagsInterrupt();
        }
        #endregion

        #region //更新监控数据
        public bool Converted_analog_values()
        {
            test.GetDDMAnalogValues();
            switch (Dut)
            {
                case 1:
                    if ((TestResult.tempDDM < 0) || TestResult.tempDDM > 40)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            test.GetDDMAnalogValues();
                            if ((TestResult.tempDDM > 0) || TestResult.tempDDM < 40)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    break;
                case 2:
                    if ((TestResult2.tempDDM < 0) || TestResult2.tempDDM > 40)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            test.GetDDMAnalogValues();
                            if ((TestResult2.tempDDM > 0) || TestResult2.tempDDM < 40)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    break;
                case 3:
                    if ((TestResult3.tempDDM < 0) || TestResult3.tempDDM > 40)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            test.GetDDMAnalogValues();
                            if ((TestResult3.tempDDM > 0) || TestResult3.tempDDM < 40)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    break;
                case 4:
                    if ((TestResult4.tempDDM < 0) || TestResult4.tempDDM > 40)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            test.GetDDMAnalogValues();
                            if ((TestResult4.tempDDM > 0) || TestResult4.tempDDM < 40)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        public async void Converted_analog_values_Async()
        {

            try
            {
                // 将耗时的硬件读取操作放到后台线程
                await Task.Run(() =>
                {
                    test.GetDDMAnalogValues();
                    //switch (Dut)
                    //{
                    //    case 1:
                    //        if ((TestResult.tempDDM < 0) || TestResult.tempDDM > 40)
                    //        {
                    //            for (int i = 0; i < 3; i++)
                    //            {
                    //               return  test.GetDDMAnalogValues();
                    //            }
                    //        }
                    //        break;
                    //    case 2:
                    //        if ((TestResult2.tempDDM < 0) || TestResult2.tempDDM > 40)
                    //        {
                    //            for (int i = 0; i < 3; i++)
                    //            {
                    //               return  test.GetDDMAnalogValues();
                    //            }
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}
                });
            }
            catch (Exception ex)
            {
                // 处理可能出现的异常
                AddTestLog("Converted_analog_values_Async" + ex.ToString());
            }
            finally
            {
                // 无论如何都要释放信号量
                // GlobalVarFun.BusSemaphore.Release();
            }
        }
        #endregion

        #region //获取模块电压
        public float GetVcc()
        {
            return test.GetVCC();
        }
        #endregion

        #region //CheckTestTypeInfo
        public bool CheckTestTypeInfo()
        {
            return test.CheckTestTypeInfo();
        }


        #endregion

        # region//CheckDebugPWD
        public byte CheckDebugPWD()
        {
            return test.CheckDebugPWD();
        }
        #endregion

        #region //更新 模块 类型/速率/版本/状态  等信息
        public bool ShowCheckModuleStatus()
        {
            bool rtn_flag = true;
            string str = "";
            if (GlobalVarFun.Language == "Chinese")
            {
                str = " 三合一方案......";
            }
            else
            {
                str = " Three-in-one chip scheme.....";
            }
            switch (Dut)
            {
                case 1:
                    if (GlobalVarFun.moduleType == "SFP-GN25L95")
                    {
                        TestResult.scheme_Ver = " SFP-GN25L95" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFP-GN25L96")
                    {
                        TestResult.scheme_Ver = " SFP-GN25L96" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFP-UX3320C")
                    {
                        TestResult.scheme_Ver = " SFP-UX3320C" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFP-UX3320T")
                    {
                        TestResult.scheme_Ver = " SFP-UX3320T" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFPP-GN1196")
                    {
                        TestResult.scheme_Ver = " SFPP-GN1196" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFPP-UX3261S")
                    {
                        TestResult.scheme_Ver = "SFPPUX3261S" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFPP-UX2270+2072")
                    {
                        TestResult.scheme_Ver = "SFPP-UX2270+2072" + str;
                        return true;
                    }
                    // MCU方案的模块支持如下判断  SFP-MCU  SFP+  XFP
                    //////////////////////////////////////////////////////////////////////////
                    //
                    // 多模850nm 模块特殊判断
                    if (((TestResult.fibertop_pn).Contains("-MM85") == TestResult.moduleIsSR)
                        || ((TestResult.fibertop_pn).Contains("-MC85") == TestResult.moduleIsSR)
                        || ((TestResult.fibertop_pn).Contains("-MS83") == TestResult.moduleIsSR)
                        || ((TestResult.fibertop_pn).Contains("-AC") == TestResult.moduleIsSR))
                    {
                        TestResult.moduleIsSR = true;
                    }
                    else
                    {
                        TestResult.moduleIsSR = false;
                        rtn_flag = false;
                    }

                    //
                    TestResult.scheme_Ver = " " + TestResult.chipType;
                    TestResult.scheme_Ver += " " + TestResult.bitRate;
                    TestResult.scheme_Ver += " " + TestResult.softType;
                    TestResult.scheme_Ver += " " + TestResult.softVer;

                    if (TestResult.wpIsEn == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult.scheme_Ver += "  写码密码:使能";
                        }
                        else
                        {
                            TestResult.scheme_Ver += "  Coding PWD:Enabled";
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult.scheme_Ver += "  写码密码:关闭";
                        }
                        else
                        {
                            TestResult.scheme_Ver += "  Coding PWD:Disenabled";
                        }
                    }

                    if (TestResult.chipIsOK == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult.scheme_Ver += "  芯片状态:正常";
                        }
                        else
                        {
                            TestResult.scheme_Ver += "  Chip status: Normal";
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult.scheme_Ver += "  芯片状态:异常";
                        }
                        else
                        {
                            TestResult.scheme_Ver += "  Chip status: Abnormal";
                        }
                        rtn_flag = false;
                    }
                    break;
                case 2:
                    if (GlobalVarFun.moduleType == "SFP-GN25L95")
                    {
                        TestResult2.scheme_Ver = " SFP-GN25L95" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFP-GN25L96")
                    {
                        TestResult2.scheme_Ver = " SFP-GN25L96" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFP-UX3320C")
                    {
                        TestResult2.scheme_Ver = " SFP-UX3320C" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFP-UX3320T")
                    {
                        TestResult2.scheme_Ver = " SFP-UX3320T" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFPP-GN1196")
                    {
                        TestResult2.scheme_Ver = " SFPP-GN1196" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFPP-UX3261S")
                    {
                        TestResult2.scheme_Ver = "SFPPUX3261S" + str;
                        return true;
                    }
                    if (GlobalVarFun.moduleType == "SFPP-UX2270+2072")
                    {
                        TestResult2.scheme_Ver = "SFPP-UX2270+2072" + str;
                        return true;
                    }
                    // MCU方案的模块支持如下判断  SFP-MCU  SFP+  XFP
                    //////////////////////////////////////////////////////////////////////////
                    //
                    // 多模850nm 模块特殊判断
                    if (((TestResult2.fibertop_pn).Contains("-MM85") == TestResult2.moduleIsSR)
                        || ((TestResult2.fibertop_pn).Contains("-MC85") == TestResult2.moduleIsSR)
                        || ((TestResult2.fibertop_pn).Contains("-MS83") == TestResult2.moduleIsSR)
                        || ((TestResult2.fibertop_pn).Contains("-AC") == TestResult2.moduleIsSR))
                    {
                        TestResult2.moduleIsSR = true;
                    }
                    else
                    {
                        TestResult2.moduleIsSR = false;
                        rtn_flag = false;
                    }

                    //
                    TestResult2.scheme_Ver = " " + TestResult2.chipType;
                    TestResult2.scheme_Ver += " " + TestResult2.bitRate;
                    TestResult2.scheme_Ver += " " + TestResult2.softType;
                    TestResult2.scheme_Ver += " " + TestResult2.softVer;

                    if (TestResult2.wpIsEn == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult2.scheme_Ver += "  写码密码:使能";
                        }
                        else
                        {
                            TestResult2.scheme_Ver += "  Coding PWD:Enabled";
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult2.scheme_Ver += "  写码密码:关闭";
                        }
                        else
                        {
                            TestResult2.scheme_Ver += "  Coding PWD:Disenabled";
                        }
                    }

                    if (TestResult2.chipIsOK == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult2.scheme_Ver += "  芯片状态:正常";
                        }
                        else
                        {
                            TestResult2.scheme_Ver += "  Chip status: Normal";
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            TestResult2.scheme_Ver += "  芯片状态:异常";
                        }
                        else
                        {
                            TestResult2.scheme_Ver += "  Chip status: Abnormal";
                        }
                        rtn_flag = false;
                    }
                    break;
                default:
                    break;
            }

            //////////////////////////////////////////////////////////////////////////

            return rtn_flag;
        }
        #endregion

        #region //裕泰微/88E1112电口测试
        public bool Elec_moudleTest()
        {
            return test.elec_moudleTest();
        }
        #endregion

        #region //接收LOS自动调试
        public bool RxLosAutoSet()
        {
            UInt16 min = TestSet.rxlos_Min;
            UInt16 max = TestSet.rxlos_Max;
            UInt16 los_val = 0;
            ReturnReuslt result = new ReturnReuslt();

            if (min > max || max > 255)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "自动调试Los范围错误！";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Auto debug Los range errors!";
                }
                result.message = "接收LOS自动调试 : " + " min:" + min.ToString() + "max" + max.ToString() + "自动调试Los范围错误！";
                ModListBoxShow(this, result);
                return false;
            }

            if (min == max) // 当min == max时，固定设置。
            {
                los_val = min;
                result.message = "los_val : " + " min:" + min.ToString();
                ModListBoxShow(this, result);
                return test.SetRxLos(los_val);
            }

            // 收光调整到去告警点 DLOS
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxDLosAtt);
                    result.message = "rxDLosAtt : " + " att:" + DOA.rxDLosAtt.ToString();
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxDLosAtt);
                    result.message = "rxDLosAtt : " + " att:" + DOA2.rxDLosAtt.ToString();
                    break;
            }

            ModListBoxShow(this, result);

            // 1. 检查最小点是否可以产生DELOS 去告警
            los_val = min;
            if (test.SetRxLos(los_val) == false) return false;

            result.message = "los_val : " + los_val.ToString();
            ModListBoxShow(this, result);

            if (test.CheckRxLOS() == true) // LOS告警
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "自动调试Los最小值去告警错误！";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Auto debug Los minimum to alarm error!";
                }
                return false;
            }

            // 收光调整到告警点 LOS
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxALosAtt);
                    result.message = "rxALosAtt : " + " att:" + DOA.rxALosAtt.ToString();
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxALosAtt);
                    result.message = "rxALosAtt : " + " att:" + DOA2.rxALosAtt.ToString();
                    break;
            }


            ModListBoxShow(this, result);

            // 2. 检查最大点是否可以产生LOS
            los_val = max;
            if (test.SetRxLos(los_val) == false) return false;

            result.message = "los_val : " + los_val.ToString();
            ModListBoxShow(this, result);

            if (test.CheckRxLOS() == false) // 不产生LOS告警
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "自动调试Los最大值告警错误！";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Automatic debugging Los maximum alarm error!";
                }
                return false;
            }

            // 3. 检查中点是否可以产生LOS
            los_val = min;
            los_val += max;
            los_val /= 2;
            if (test.SetRxLos(los_val) == false) return false;

            result.message = "los_val : " + los_val.ToString();
            ModListBoxShow(this, result);

            if (test.CheckRxLOS() == false) // 不产生LOS告警
            {
                min = los_val;
            }

            // 4. 自动调试Los
            los_val = min;
            do
            {
                if (test.SetRxLos(los_val) == false) return false;

                result.message = "los_val : " + los_val.ToString();
                ModListBoxShow(this, result);

                // 检查LOS
                if (test.CheckRxLOS() == true) // 产生告警LOS
                {
                    // DELOS  去告警
                    switch (Dut)
                    {
                        case 1:
                            opticaldoaatt.SetAttenuation(DOA.rxDLosAtt);
                            break;
                        case 2:
                            opticaldoaatt.SetAttenuation(DOA2.rxDLosAtt);
                            break;
                    }

                    // DELOS 去告警
                    if (test.CheckRxLOS() == false) // 去告警OK
                    {
                        return true; //  正确 返回
                    }
                    return false; //  调试失败 返回
                }
                //
                if ((los_val + 2) > max)
                {
                    los_val += 1; // 步径为1
                }
                else
                {
                    los_val += 2; // 步径为2
                }
                //
            } while (los_val <= max);

            //
            return false;
        }

        public async Task<bool> RxLosAutoSet_Async()
        {
            //DUT编号→OTP板卡槽位映射（如DUT1→槽位"09"）
            string slotStr = GlobalVarFun.VOArxDutToSlot[Dut];
            //DUT编号→VOA通道映射（1→1, 2→2, 3→3, 4→4）
            int VoaChannel = GlobalVarFun.DutToVoaCh[Dut];

            //打开光输出
            otp12.VOA_SetOutputState(VoaChannel, "ON");
            UInt16 min = TestSet.rxlos_Min;
            UInt16 max = TestSet.rxlos_Max;
            UInt16 los_val = 0;
            ReturnReuslt result = new ReturnReuslt();

            //模块内部通过一个寄存器（`rx_los`，0~255范围的DAC值）来设定LOS判定的阈值。
            //这个函数的目的就是自动找到一个合适的寄存器值__
            otp12.SetSlot(slotStr);
            //先关闭ALC自动功率跟踪（ALC开启时会自动调节衰减，手动设置会被覆盖）
            otp12.VOA_SetAlcState(Dut, "OFF");
            //设置工作模式为衰减模式（而非功率模式POWer）
            otp12.VOA_SetMode(Dut, "ATTenuation");
            //设置操作模式为绝对值模式ABSolute（而非参考值模式REFerence）
            otp12.VOA_SetApMode(Dut, "ABSolute");
            //打开输出光路（确保光路上有输出）

            if (min > max || max > 255)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "自动调试Los范围错误！";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Auto debug Los range errors!";
                }
                result.message = "接收LOS自动调试 : " + " min:" + min.ToString() + "max" + max.ToString() + "自动调试Los范围错误！";
                ModListBoxShow(this, result);
                return false;
            }

            if (min == max) // 当min == max时，固定设置。
            {
                los_val = min;
                result.message = "los_val : " + " min:" + min.ToString();
                ModListBoxShow(this, result);
                return test.SetRxLos(los_val);
            }
            //此时的Los的值是去告警光功率
            retutntxrxresult.los = los_val;
            //收光调整到去告警点 DLOS
            switch (Dut)
            {
                case 1:
                    //验证最小值(min)在"去告警光功率"下是否正确
                    //VOA设到 rxDLosAtt（低衰减=高光功率）
                    otp12.VOA_SetAttenuation(Dut, DOA.rxDLosAtt);
                    result.message = "rxDLosAtt : " + " att:" + DOA.rxDLosAtt.ToString();
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxDLosAtt);
                    result.message = "rxDLosAtt : " + " att:" + DOA2.rxDLosAtt.ToString();
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxDLosAtt);
                    result.message = "rxDLosAtt : " + " att:" + DOA3.rxDLosAtt.ToString();
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxDLosAtt);
                    result.message = "rxDLosAtt : " + " att:" + DOA4.rxDLosAtt.ToString();
                    break;
            }

            ModListBoxShow(this, result);
            await Task.Delay(waittimes);
            // 1. 检查最小点是否可以产生DELOS 去告警 los_val表示的这个是los的值
            los_val = min;
            if (test.SetRxLos(los_val) == false) return false;

            result.message = "los_val : " + los_val.ToString();
            ModListBoxShow(this, result);
            retutntxrxresult.los = los_val;
            if (test.CheckRxLOS() == true) // LOS告警
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "自动调试Los最小值去告警错误！";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Auto debug Los minimum to alarm error!";
                }
                return false;
            }

            // 收光调整到告警点 LOS
            switch (Dut)
            {
                //验证最大值(max)在"告警光功率"下是否正确          
                //VOA设到 rxALosAtt（高衰减 = 低光功率）  
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxALosAtt);
                    result.message = "rxALosAtt : " + " att:" + DOA.rxALosAtt.ToString();
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxALosAtt);
                    result.message = "rxALosAtt : " + " att:" + DOA2.rxALosAtt.ToString();
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxALosAtt);
                    result.message = "rxALosAtt : " + " att:" + DOA3.rxALosAtt.ToString();
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxALosAtt);
                    result.message = "rxALosAtt : " + " att:" + DOA4.rxALosAtt.ToString();
                    break;
            }

            ModListBoxShow(this, result);

            // 2. 检查最大点是否可以产生LOS
            los_val = max;
            if (test.SetRxLos(los_val) == false) return false;
            retutntxrxresult.los = los_val;
            result.message = "los_val : " + los_val.ToString();
            ModListBoxShow(this, result);

            if (test.CheckRxLOS() == false) // 不产生LOS告警
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "自动调试Los最大值告警错误！";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Automatic debugging Los maximum alarm error!";
                }
                return false;
            }

            // 3. 检查中点是否可以产生LOS
            los_val = min;
            los_val += max;
            los_val /= 2;
            if (test.SetRxLos(los_val) == false) return false;
            retutntxrxresult.los = los_val;
            result.message = "los_val : " + los_val.ToString();
            ModListBoxShow(this, result);

            if (test.CheckRxLOS() == false) // 不产生LOS告警
            {
                min = los_val;
            }

            // 4. 自动调试Los
            los_val = min;
            do
            {
                if (test.SetRxLos(los_val) == false) return false;

                result.message = "los_val : " + los_val.ToString();
                ModListBoxShow(this, result);

                // 检查LOS
                if (test.CheckRxLOS() == true) // 产生告警LOS
                {
                    // DELOS  去告警
                    switch (Dut)
                    {
                        case 1:
                            otp12.VOA_SetAttenuation(Dut, DOA.rxDLosAtt);
                            break;
                        case 2:
                            otp12.VOA_SetAttenuation(Dut, DOA2.rxDLosAtt);
                            break;
                        case 3:
                            otp12.VOA_SetAttenuation(Dut, DOA3.rxDLosAtt);
                            break;
                        case 4:
                            otp12.VOA_SetAttenuation(Dut, DOA4.rxDLosAtt);
                            break;
                    }
                    retutntxrxresult.los = los_val;
                    // DELOS 去告警
                    if (test.CheckRxLOS() == false) // 去告警OK
                    {
                        return true; //  正确 返回
                    }
                    return false; //  调试失败 返回
                }
                //
                if ((los_val + 2) > max)
                {
                    los_val += 1; // 步径为1
                }
                else
                {
                    los_val += 2; // 步径为2
                }
                await Task.Delay(waittimes);
                //
            } while (los_val <= max);
            retutntxrxresult.los = los_val;
            //
            return false;
        }
        #endregion

        #region // 计算RX CAL参数
        public bool CulRxCalPar()
        {
            double[] x = new double[5];  //ADC原始值
            double[] y = new double[5];  //校正值
            double[] a = new double[5];  //系数
            double[] dt = new double[5]; //误差  

            double rxcaldbm1 = 0;
            double rxcaldbm2 = 0;
            double rxcaldbm3 = 0;
            double rxcaldbm4 = 0;
            double rxcaldbm5 = 0;

            int i = 0;

            for (i = 0; i < 5; i++)
            {
                dt[i] = 0;
            }

            //
            switch (Dut)
            {
                case 1:
                    rxcaldbm1 = TestSet.rxPwr_Cal[0] / 10;
                    y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                    rxcaldbm2 = TestSet.rxPwr_Cal[1] / 10;
                    y[1] = Math.Pow(10, rxcaldbm2) * 10000;
                    rxcaldbm3 = TestSet.rxPwr_Cal[2] / 10;
                    if ((GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-GN1196"))
                    {
                        rxcaldbm3 = -40;
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    else
                    {
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    //
                    x[0] = rxAdc[0];
                    x[1] = rxAdc[1];
                    x[2] = rxAdc[2];
                    //
                    if (GlobalVarFun.setup.rx_apd_cal)
                    {
                        rxcaldbm4 = TestSet.rxPwr_Cal[3] / 10;
                        y[3] = Math.Pow(10, rxcaldbm4) * 10000;
                        //
                        rxcaldbm5 = TestSet.rxPwr_Cal[4] / 10;
                        y[4] = Math.Pow(10, rxcaldbm5) * 10000;
                        //
                        x[3] = rxAdc[3];
                        x[4] = rxAdc[4];
                        //
                        Bit.iapcir(x, y, 5, a, 3, dt); // APD
                    }
                    else
                    {
                        Bit.iapcir(x, y, 3, a, 2, dt);  // PIN
                    }
                    break;
                case 2:
                    rxcaldbm1 = TestSet2.rxPwr_Cal[0] / 10;
                    y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                    rxcaldbm2 = TestSet2.rxPwr_Cal[1] / 10;
                    y[1] = Math.Pow(10, rxcaldbm2) * 10000;
                    rxcaldbm3 = TestSet2.rxPwr_Cal[2] / 10;
                    if ((GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-GN1196"))
                    {
                        rxcaldbm3 = -40;
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    else
                    {
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    //
                    x[0] = rxAdc[0];
                    x[1] = rxAdc[1];
                    x[2] = rxAdc[2];
                    //
                    if (GlobalVarFun.setup.rx_apd_cal)
                    {
                        rxcaldbm4 = TestSet2.rxPwr_Cal[3] / 10;
                        y[3] = Math.Pow(10, rxcaldbm4) * 10000;
                        //
                        rxcaldbm5 = TestSet2.rxPwr_Cal[4] / 10;
                        y[4] = Math.Pow(10, rxcaldbm5) * 10000;
                        //
                        x[3] = rxAdc[3];
                        x[4] = rxAdc[4];
                        //
                        Bit.iapcir(x, y, 5, a, 3, dt); // APD
                    }
                    else
                    {
                        Bit.iapcir(x, y, 3, a, 2, dt);  // PIN
                    }
                    break;
                case 3:
                    rxcaldbm1 = TestSet3.rxPwr_Cal[0] / 10;
                    y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                    rxcaldbm2 = TestSet3.rxPwr_Cal[1] / 10;
                    y[1] = Math.Pow(10, rxcaldbm2) * 10000;
                    rxcaldbm3 = TestSet3.rxPwr_Cal[2] / 10;
                    if ((GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-GN1196"))
                    {
                        rxcaldbm3 = -40;
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    else
                    {
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    //
                    x[0] = rxAdc[0];
                    x[1] = rxAdc[1];
                    x[2] = rxAdc[2];
                    //
                    if (GlobalVarFun.setup.rx_apd_cal)
                    {
                        rxcaldbm4 = TestSet3.rxPwr_Cal[3] / 10;
                        y[3] = Math.Pow(10, rxcaldbm4) * 10000;
                        //
                        rxcaldbm5 = TestSet3.rxPwr_Cal[4] / 10;
                        y[4] = Math.Pow(10, rxcaldbm5) * 10000;
                        //
                        x[3] = rxAdc[3];
                        x[4] = rxAdc[4];
                        //
                        Bit.iapcir(x, y, 5, a, 3, dt); // APD
                    }
                    else
                    {
                        Bit.iapcir(x, y, 3, a, 2, dt);  // PIN
                    }
                    break;
                case 4:
                    rxcaldbm1 = TestSet4.rxPwr_Cal[0] / 10;
                    y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                    rxcaldbm2 = TestSet4.rxPwr_Cal[1] / 10;
                    y[1] = Math.Pow(10, rxcaldbm2) * 10000;
                    rxcaldbm3 = TestSet4.rxPwr_Cal[2] / 10;
                    if ((GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-GN1196"))
                    {
                        rxcaldbm3 = -40;
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    else
                    {
                        y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                    }
                    //
                    x[0] = rxAdc[0];
                    x[1] = rxAdc[1];
                    x[2] = rxAdc[2];
                    //
                    if (GlobalVarFun.setup.rx_apd_cal)
                    {
                        rxcaldbm4 = TestSet4.rxPwr_Cal[3] / 10;
                        y[3] = Math.Pow(10, rxcaldbm4) * 10000;
                        //
                        rxcaldbm5 = TestSet4.rxPwr_Cal[4] / 10;
                        y[4] = Math.Pow(10, rxcaldbm5) * 10000;
                        //
                        x[3] = rxAdc[3];
                        x[4] = rxAdc[4];
                        //
                        Bit.iapcir(x, y, 5, a, 3, dt); // APD
                    }
                    else
                    {
                        Bit.iapcir(x, y, 3, a, 2, dt);  // PIN
                    }
                    break;

            }

            switch (Dut)
            {
                case 1:
                    TestResult.rxPwrCal_c[0] = (float)a[0];
                    TestResult.rxPwrCal_c[1] = (float)a[1];
                    TestResult.rxPwrCal_c[2] = (float)a[2];
                    TestResult.rxPwrCal_c[3] = (float)a[3];
                    TestResult.rxPwrCal_c[4] = (float)a[4];
                    break;
                case 2:
                    TestResult2.rxPwrCal_c[0] = (float)a[0];
                    TestResult2.rxPwrCal_c[1] = (float)a[1];
                    TestResult2.rxPwrCal_c[2] = (float)a[2];
                    TestResult2.rxPwrCal_c[3] = (float)a[3];
                    TestResult2.rxPwrCal_c[4] = (float)a[4];
                    break;
                case 3:
                    TestResult3.rxPwrCal_c[0] = (float)a[0];
                    TestResult3.rxPwrCal_c[1] = (float)a[1];
                    TestResult3.rxPwrCal_c[2] = (float)a[2];
                    TestResult3.rxPwrCal_c[3] = (float)a[3];
                    TestResult3.rxPwrCal_c[4] = (float)a[4];
                    break;
                case 4:
                    TestResult4.rxPwrCal_c[0] = (float)a[0];
                    TestResult4.rxPwrCal_c[1] = (float)a[1];
                    TestResult4.rxPwrCal_c[2] = (float)a[2];
                    TestResult4.rxPwrCal_c[3] = (float)a[3];
                    TestResult4.rxPwrCal_c[4] = (float)a[4];
                    break;
                default:
                    break;
            }


            if ((Math.Abs(a[0]) > 1000) || (Math.Abs(a[1]) > 1000))
            {
                return false;
            }

            /////////////////////////////////////////////////////////////
            //2020.10.27 //2022.5.19
            if (GlobalVarFun.moduleType == "SFP-UX3320C" || GlobalVarFun.moduleType == "SFP-UX3320T" || GlobalVarFun.moduleType == "SFPP-UX3261S" || GlobalVarFun.moduleType == "SFPP-UX2270+2072")
            {
                switch (Dut)
                {
                    case 1:
                        TestResult.rxAdcCal[0] = rxAdc[0];
                        TestResult.rxAdcCal[1] = rxAdc[1];
                        TestResult.rxAdcCal[2] = rxAdc[2];
                        TestResult.rxAdcCal[3] = rxAdc[3];
                        TestResult.rxAdcCal[4] = rxAdc[4];
                        TestResult.rxAdcCal[5] = rxAdc[5];

                        rxcaldbm1 = TestSet.rxPwr_Cal[0] / 10;
                        rxcaldbm2 = TestSet.rxPwr_Cal[1] / 10;
                        rxcaldbm3 = TestSet.rxPwr_Cal[2] / 10;
                        rxcaldbm4 = TestSet.rxPwr_Cal[3] / 10;
                        rxcaldbm5 = TestSet.rxPwr_Cal[4] / 10;
                        break;
                    case 2:
                        TestResult2.rxAdcCal[0] = rxAdc[0];
                        TestResult2.rxAdcCal[1] = rxAdc[1];
                        TestResult2.rxAdcCal[2] = rxAdc[2];
                        TestResult2.rxAdcCal[3] = rxAdc[3];
                        TestResult2.rxAdcCal[4] = rxAdc[4];
                        TestResult2.rxAdcCal[5] = rxAdc[5];

                        rxcaldbm1 = TestSet2.rxPwr_Cal[0] / 10;
                        rxcaldbm2 = TestSet2.rxPwr_Cal[1] / 10;
                        rxcaldbm3 = TestSet2.rxPwr_Cal[2] / 10;
                        rxcaldbm4 = TestSet2.rxPwr_Cal[3] / 10;
                        rxcaldbm5 = TestSet2.rxPwr_Cal[4] / 10;
                        break;
                    case 3:
                        TestResult3.rxAdcCal[0] = rxAdc[0];
                        TestResult3.rxAdcCal[1] = rxAdc[1];
                        TestResult3.rxAdcCal[2] = rxAdc[2];
                        TestResult3.rxAdcCal[3] = rxAdc[3];
                        TestResult3.rxAdcCal[4] = rxAdc[4];
                        TestResult3.rxAdcCal[5] = rxAdc[5];

                        rxcaldbm1 = TestSet3.rxPwr_Cal[0] / 10;
                        rxcaldbm2 = TestSet3.rxPwr_Cal[1] / 10;
                        rxcaldbm3 = TestSet3.rxPwr_Cal[2] / 10;
                        rxcaldbm4 = TestSet3.rxPwr_Cal[3] / 10;
                        rxcaldbm5 = TestSet3.rxPwr_Cal[4] / 10;
                        break;
                    case 4:
                        TestResult4.rxAdcCal[0] = rxAdc[0];
                        TestResult4.rxAdcCal[1] = rxAdc[1];
                        TestResult4.rxAdcCal[2] = rxAdc[2];
                        TestResult4.rxAdcCal[3] = rxAdc[3];
                        TestResult4.rxAdcCal[4] = rxAdc[4];
                        TestResult4.rxAdcCal[5] = rxAdc[5];

                        rxcaldbm1 = TestSet4.rxPwr_Cal[0] / 10;
                        rxcaldbm2 = TestSet4.rxPwr_Cal[1] / 10;
                        rxcaldbm3 = TestSet4.rxPwr_Cal[2] / 10;
                        rxcaldbm4 = TestSet4.rxPwr_Cal[3] / 10;
                        rxcaldbm5 = TestSet4.rxPwr_Cal[4] / 10;
                        break;
                    default:
                        break;
                }


                //
                y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                y[1] = Math.Pow(10, rxcaldbm2) * 10000;
                x[0] = rxAdc[0];
                x[1] = rxAdc[1];
                Bit.iapcir(x, y, 2, a, 2, dt);
                switch (Dut)
                {
                    case 1:
                        TestResult.rxPwrCal_b[0] = (float)a[0];
                        TestResult.rxPwrCal_k[0] = (float)a[1];
                        break;
                    case 2:
                        TestResult2.rxPwrCal_b[0] = (float)a[0];
                        TestResult2.rxPwrCal_k[0] = (float)a[1];
                        break;
                    case 3:
                        TestResult3.rxPwrCal_b[0] = (float)a[0];
                        TestResult3.rxPwrCal_k[0] = (float)a[1];
                        break;
                    case 4:
                        TestResult4.rxPwrCal_b[0] = (float)a[0];
                        TestResult4.rxPwrCal_k[0] = (float)a[1];
                        break;
                    default:
                        break;
                }

                //
                y[0] = Math.Pow(10, rxcaldbm2) * 10000;
                y[1] = Math.Pow(10, rxcaldbm3) * 10000;
                x[0] = rxAdc[1];
                x[1] = rxAdc[2];
                Bit.iapcir(x, y, 2, a, 2, dt);
                switch (Dut)
                {
                    case 1:
                        TestResult.rxPwrCal_b[1] = (float)a[0];
                        TestResult.rxPwrCal_k[1] = (float)a[1];
                        break;
                    case 2:
                        TestResult2.rxPwrCal_b[1] = (float)a[0];
                        TestResult2.rxPwrCal_k[1] = (float)a[1];
                        break;
                    case 3:
                        TestResult3.rxPwrCal_b[1] = (float)a[0];
                        TestResult3.rxPwrCal_k[1] = (float)a[1];
                        break;
                    case 4:
                        TestResult4.rxPwrCal_b[1] = (float)a[0];
                        TestResult4.rxPwrCal_k[1] = (float)a[1];
                        break;
                    default:
                        break;
                }
                //
                y[0] = Math.Pow(10, rxcaldbm3) * 10000;
                y[1] = Math.Pow(10, -40.0 / 10) * 10000;
                //
                x[0] = rxAdc[2];
                x[1] = rxAdc[5];
                //
                Bit.iapcir(x, y, 2, a, 2, dt);
                switch (Dut)
                {
                    case 1:
                        TestResult.rxPwrCal_b[2] = (float)a[0];
                        TestResult.rxPwrCal_k[2] = (float)a[1];
                        break;
                    case 2:
                        TestResult2.rxPwrCal_b[2] = (float)a[0];
                        TestResult2.rxPwrCal_k[2] = (float)a[1];
                        break;
                    case 3:
                        TestResult3.rxPwrCal_b[2] = (float)a[0];
                        TestResult3.rxPwrCal_k[2] = (float)a[1];
                        break;
                    case 4:
                        TestResult4.rxPwrCal_b[2] = (float)a[0];
                        TestResult4.rxPwrCal_k[2] = (float)a[1];
                        break;
                    default:
                        break;
                }
            }
            /////////////////////////////////////////////////////////////

            return true;
        }
        #endregion

        #region  // 接收DDM自动校准
        private bool RxPwrDDMAutoCal()
        {
            ReturnReuslt result = new ReturnReuslt();
            //设置TXSFP光源1
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxCalAtt[0]);
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxCalAtt[0]);
                    break;
            }

            rxAdc[0] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[0] = rxAdc[0];
            result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
            ModListBoxShow(this, result);

            //设置TXSFP光源2
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxCalAtt[1]);
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxCalAtt[1]);
                    break;
            }

            rxAdc[1] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[1] = rxAdc[1];
            retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Cal[1];
            result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
            ModListBoxShow(this, result);

            //设置TXSFP光源3
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxCalAtt[2]);
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxCalAtt[2]);
                    break;
            }

            rxAdc[2] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[2] = rxAdc[2];
            result.message = "设置TXSFP光源3：Success" + " AttVal" + DOA.rxCalAtt[2].ToString() + " rxAdc:" + rxAdc[2].ToString();
            ModListBoxShow(this, result);

            if (GlobalVarFun.setup.rx_apd_cal) // APD 检查后面2个点
            {
                //设置TXSFP光源4
                switch (Dut)
                {
                    case 1:
                        opticaldoaatt.SetAttenuation(DOA.rxCalAtt[3]);
                        break;
                    case 2:
                        opticaldoaatt.SetAttenuation(DOA2.rxCalAtt[3]);
                        break;
                }

                rxAdc[3] = test.GetRxADC();
                retutntxrxresult.RxddmPowers[3] = rxAdc[3];

                //设置TXSFP光源5
                switch (Dut)
                {
                    case 1:
                        opticaldoaatt.SetAttenuation(DOA.rxCalAtt[4]);
                        break;
                    case 2:
                        opticaldoaatt.SetAttenuation(DOA2.rxCalAtt[4]);
                        break;
                }
                rxAdc[4] = test.GetRxADC();
                retutntxrxresult.RxddmPowers[4] = rxAdc[4];
            }

            //设置TXSFP光源 为无光状态
            opticaldoaatt.SetAttenuation(60);
            rxAdc[5] = test.GetRxADC();
            rxAdc[5] += 3; //加大 预防跳动问题
            retutntxrxresult.RxddmPowers[5] = rxAdc[5];
            result.message = "设置TXSFP光源 为无光状态：Success" + " AttVal " + "60" + " rxAdc:" + rxAdc[5].ToString();

            if (rxAdc[5] > 63) // 最大63
            {
                rxAdc[5] = 63;
                retutntxrxresult.ErrorMessage += "++" + "无光采样值超出最大限制";
                retutntxrxresult.RxddmPowers[0] = rxAdc[5];
                result.message = "设置TXSFP光源 为无光状态：Fail " + retutntxrxresult.ErrorMessage;
            }
            switch (Dut)
            {
                case 1:
                    TestResult.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                case 2:
                    TestResult2.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                default:
                    break;
            }
            //计算校准参数
            if (CulRxCalPar() == false)
            {
                result.message = "计算校准参数：Fail";
                ModListBoxShow(this, result);
                return false;
            }

            // 写入校准参数到模块
            if (test.WriteRxCalData() == false)
            {
                result.message = "写入校准参数到模块：Fail";
                ModListBoxShow(this, result);
                return false;
            }
            return true;
        }
        #endregion

        #region 接收光功率DDM（数字诊断监控）自动校准
        private async Task<bool> RxPwrDDMAutoCal_Async()
        {
            //创建一个结果对象，用于封装日志消息，传递给UI列表框显示
            ReturnReuslt result = new ReturnReuslt();

            //根据当前待测端口号Dut（1~4），从全局映射表中获取该端口接收方向对应的OTP-12槽位号的字符串。
            string slotStr = GlobalVarFun.VOArxDutToSlot[Dut];
            //根据当前端口号Dut，获取对应的VOA（可调光衰减器）通道号
            int VoaChannel = GlobalVarFun.DutToVoaCh[Dut];
            //让OTP-12驱动切换到当前端口对应的槽位，后续所有VOA/BERT/OPM操作都在该槽位执行
            otp12.SetSlot(slotStr);
            //先关闭ALC自动功率跟踪（ALC开启时会自动调节衰减，手动设置会被覆盖）
            otp12.VOA_SetAlcState(Dut, "OFF");
            //设置工作模式为衰减模式（而非功率模式POWer）,而非功率模式（POWer模式会自动调节到目标功率）
            otp12.VOA_SetMode(Dut, "ATTenuation");
            //设置操作模式为绝对值模式ABSolute（而非参考值模式REFerence）,而非相对于参考值的增量（REFerence模式）
            otp12.VOA_SetApMode(Dut, "ABSolute");
            //打开输出光路（确保光路上有输出）,确保光信号能通过衰减器输出到待测模块
            otp12.VOA_SetOutputState(VoaChannel, "ON");
            //设置TXSFP光源1 异步等待waittimes毫秒（默认值为2ms，但实际OTP设备操作可能需要更长时间），让硬件设置生效稳定。
            await Task.Delay(waittimes);
            switch (Dut)
            {
                case 1:
                    //如果是端口1，设置VOA衰减值为DOA.rxCalAtt[0]（端口1的第0个校准衰减值，单位dB）。DOA是端口1的衰减参数配置对象
                    otp12.VOA_SetAttenuation(Dut, DOA.rxCalAtt[0]);
                    //构建日志消息字符串，包含"设置TXSFP光源1成功"、衰减值和当前ADC值。注意：这里rxAdc[0]还没读取，显示的是旧值，这是一个小问题，后面Fix Bug1修复了
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    //记录第0个校准点实际光功率值（来自TestSet配置，单位0.1dBm，例如-50表示-5.0dBm），存入结果对象。break跳出switch。
                    retutntxrxresult.RxRealPowers[0] = TestSet.rxPwr_Cal[0];
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxCalAtt[0]);
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA2.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    retutntxrxresult.RxRealPowers[0] = TestSet2.rxPwr_Cal[0];
                    break;
                case 3:
                    //
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxCalAtt[0]);
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA3.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    retutntxrxresult.RxRealPowers[0] = TestSet3.rxPwr_Cal[0];
                    break;
                case 4:
                    //
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxCalAtt[0]);
                    result.message = "设置TXSFP光源1：Success" + " AttVal" + DOA4.rxCalAtt[0].ToString() + " rxAdc:" + rxAdc[0].ToString();
                    retutntxrxresult.RxRealPowers[0] = TestSet4.rxPwr_Cal[0];
                    break;
            }
            //通过test对象（具体模块驱动，如SFP-UX3320T）读取待测模块接收端DC原始采样值。这个值是模块内部RSSI（接收信号强度指示）电路的数字量，范围通常是0~63（6位ADC）
            rxAdc[0] = test.GetRxADC();
            //将读取到的ADC值存入结果对象的 `RxddmPowers[0]`，这是模块DDM监控上报的原始功率ADC值
            retutntxrxresult.RxddmPowers[0] = rxAdc[0];
            // Fix Bug1: 根据Dut选择正确的Att值用于日志显示
            float calAtt0 = 0;
            switch (Dut) { case 1: calAtt0 = DOA.rxCalAtt[0]; break; case 2: calAtt0 = DOA2.rxCalAtt[0]; break; case 3: calAtt0 = DOA3.rxCalAtt[0]; break; case 4: calAtt0 = DOA4.rxCalAtt[0]; break; }
            result.message = "设置TXSFP光源1：Success" + " AttVal" + calAtt0.ToString() + " rxAdc:" + rxAdc[0].ToString();
            ModListBoxShow(this, result);
            await Task.Delay(waittimes);
            //设置TXSFP光源2
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxCalAtt[1]);
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxCalAtt[1]);
                    retutntxrxresult.RxRealPowers[1] = TestSet2.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA2.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxCalAtt[1]);
                    retutntxrxresult.RxRealPowers[1] = TestSet3.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA3.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxCalAtt[1]);
                    //设置第二个校准点的衰减值 `rxCalAtt[1]`，记录期望光功率 `rxPwr_Cal[1]`，构建日志
                    retutntxrxresult.RxRealPowers[1] = TestSet4.rxPwr_Cal[1];
                    result.message = "设置TXSFP光源2：Success" + " AttVal" + DOA4.rxCalAtt[1].ToString() + " rxAdc:" + rxAdc[1].ToString();
                    break;
            }

            rxAdc[1] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[1] = rxAdc[1];
            // Fix Bug2: 删除硬编码TestSet2覆盖行，switch中已正确设置RxRealPowers[1]
            float calAtt1 = 0;
            switch (Dut) { case 1: calAtt1 = DOA.rxCalAtt[1]; break; case 2: calAtt1 = DOA2.rxCalAtt[1]; break; case 3: calAtt1 = DOA3.rxCalAtt[1]; break; case 4: calAtt1 = DOA4.rxCalAtt[1]; break; }
            result.message = "设置TXSFP光源2：Success" + " AttVal" + calAtt1.ToString() + " rxAdc:" + rxAdc[1].ToString();
            //通过委托调用UI线程，将日志消息显示到界面列表框中
            ModListBoxShow(this, result);

            //设置TXSFP光源3
            await Task.Delay(waittimes);
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxCalAtt[2]);
                    retutntxrxresult.RxRealPowers[2] = TestSet.rxPwr_Cal[2];  // Fix Bug3: [1]→[2]
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxCalAtt[2]);
                    retutntxrxresult.RxRealPowers[2] = TestSet2.rxPwr_Cal[2];  // Fix Bug3: [1]→[2]
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxCalAtt[2]);
                    retutntxrxresult.RxRealPowers[2] = TestSet3.rxPwr_Cal[2];  // Fix Bug3: [1]→[2]
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxCalAtt[2]);
                    retutntxrxresult.RxRealPowers[2] = TestSet4.rxPwr_Cal[2];  // Fix Bug3: [1]→[2]
                    break;
            }
            rxAdc[2] = test.GetRxADC();
            retutntxrxresult.RxddmPowers[2] = rxAdc[2];
            // Fix Bug4: 删除硬编码TestSet3覆盖行，switch中已正确设置
            // Fix Bug5: 修正日志消息标题、Att值和rxAdc索引
            float calAtt2 = 0;
            switch (Dut) { case 1: calAtt2 = DOA.rxCalAtt[2]; break; case 2: calAtt2 = DOA2.rxCalAtt[2]; break; case 3: calAtt2 = DOA3.rxCalAtt[2]; break; case 4: calAtt2 = DOA4.rxCalAtt[2]; break; }
            result.message = "设置TXSFP光源3：Success" + " AttVal" + calAtt2.ToString() + " rxAdc:" + rxAdc[2].ToString();
            ModListBoxShow(this, result);
            await Task.Delay(waittimes);
            //判断是否启用了APD校准模式。PIN光电二极管只需要3个校准点，
            //APD雪崩光电二极管需要5个点（因为APD在高功率端有更复杂的非线性响应）
            if (GlobalVarFun.setup.rx_apd_cal) // APD 检查后面2个点
            {
                //设置TXSFP光源4
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxCalAtt[3]);
                        retutntxrxresult.RxRealPowers[3] = TestSet.rxPwr_Cal[3];
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxCalAtt[3]);
                        retutntxrxresult.RxRealPowers[3] = TestSet2.rxPwr_Cal[3];
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxCalAtt[3]);
                        retutntxrxresult.RxRealPowers[3] = TestSet3.rxPwr_Cal[3];
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxCalAtt[3]);
                        retutntxrxresult.RxRealPowers[3] = TestSet4.rxPwr_Cal[3];
                        break;
                }
                await Task.Delay(waittimes); // Fix Bug6: APD点4等待光功率稳定
                rxAdc[3] = test.GetRxADC();
                retutntxrxresult.RxddmPowers[3] = rxAdc[3];

                //设置TXSFP光源5
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxCalAtt[4]);
                        retutntxrxresult.RxRealPowers[4] = TestSet.rxPwr_Cal[4];
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxCalAtt[4]);
                        retutntxrxresult.RxRealPowers[4] = TestSet2.rxPwr_Cal[4];
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxCalAtt[4]);
                        retutntxrxresult.RxRealPowers[4] = TestSet3.rxPwr_Cal[4];
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxCalAtt[4]);
                        retutntxrxresult.RxRealPowers[4] = TestSet4.rxPwr_Cal[4];
                        break;
                }
                await Task.Delay(waittimes); // Fix Bug6: APD点5等待光功率稳定
                rxAdc[4] = test.GetRxADC();
                retutntxrxresult.RxddmPowers[4] = rxAdc[4];
            }

            //设置TXSFP光源 为无光状态
            await Task.Delay(waittimes);
            //将VOA衰减设置到60dB，这是一个非常大的衰减值，相当于光路完全阻断，
            //待测模块接收端几乎收不到光（无光状态）。
            //读取无光状态下的ADC值，然后加3为余量。因为无光时ADC值可能跳动，加3是为了留出裕量，避免因噪声导致误判为有光。
            otp12.VOA_SetAttenuation(Dut, 60);
            rxAdc[5] = test.GetRxADC();
            rxAdc[5] += 3; //加大 预防跳动问题
            retutntxrxresult.RxddmPowers[5] = rxAdc[5];
            result.message = "设置TXSFP光源 为无光状态：Success" + " AttVal" + "60" + " rxAdc:" + rxAdc[5].ToString();
            //ADC值范围是0~63（6位ADC）。如果加3后超过63，钳位到63，并记录错误信息"无光采样值超出最大限制"。
            //Fix Bug7：原来错误地写入了 `RxddmPowers[0]`（覆盖了第1个校准点），修正为 `[5]`。
            if (rxAdc[5] > 63) // 最大63
            {
                rxAdc[5] = 63;
                retutntxrxresult.ErrorMessage += "++" + "无光采样值超出最大限制";
                retutntxrxresult.RxddmPowers[5] = rxAdc[5];  // Fix Bug7: [0]→[5]
                result.message = "设置TXSFP光源 为无光状态：Fail " + retutntxrxresult.ErrorMessage;
            }
            //将无光ADC值保存到对应端口的TestResult对象中（`rxNoPwrVal`），后续用于LOS（信号丢失）告警判定阈值。
            switch (Dut)
            {
                case 1:
                    TestResult.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                case 2:
                    TestResult2.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                case 3:
                    TestResult3.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                case 4:
                    TestResult4.rxNoPwrVal = (byte)rxAdc[5];
                    break;
                default:
                    break;
            }
            //等待异步线程稳定
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
                                        //计算校准参数
                                        //调用CulRxCalPar()方法，根据采集到的rxAdc[]（ADC原始值）和rxPwr_Cal[]（期望光功率dBm*10）进行多项式曲线拟合。
                                        //内部使用Bit.iapcir()做最小二乘拟合：
                                        //PIN管：3个点做2次多项式拟合（y = a0 + a1 * x + a2 * x²）
                                        //APD管：5个点做3次多项式拟合（y = a0 + a1 * x + a2 * x² +a3 * x³）
                                        //写入校准参数到模块 拟合得到的系数存入TestResult.rxPwrCal_c[]，用于模块内部将ADC值转换为光功率值
                                        //如果计算失败（如系数过大|a|>1000），返回false
            if (CulRxCalPar() == false)
            {
                result.message = "计算校准参数：Fail";
                ModListBoxShow(this, result);
                return false;
            }
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
            //调用模块驱动的 WriteRxCalData() 方法，将计算好的校准系数通过I2C总线写入待测模块的寄存器/Flash中。
            //模块上电后会使用这些系数将ADC原始值转换为实际的光功率dBm值，实现DDM监控功能
            if (test.WriteRxCalData() == false)
            {
                result.message = "写入校准参数到模块：Fail";
                ModListBoxShow(this, result);
                return false;
            }
            return true;
        }
        #endregion

        #region // 接收DDM校准精度检查
        private bool RxPwrErrorCheck()
        {
            float rxpow = 0;
            float temp = 0;
            ReturnReuslt result = new ReturnReuslt();
            // 收无光检测
            if (GlobalVarFun.setup.rx_nopwr_test)
            {
                switch (Dut)
                {
                    case 1:
                        if (DOA.currentAtt != 60)
                        {
                            opticaldoaatt.SetAttenuation(60);
                        }
                        break;
                    case 2:
                        if (DOA2.currentAtt != 60)
                        {
                            opticaldoaatt.SetAttenuation(60);
                        }
                        break;
                    default:
                        break;
                }
                //
                rxpow = test.GetRxPower();
                retutntxrxresult.RxddmPowers[5] = rxpow;
                if (rxpow > -40)
                {
                    result.message = "Rx无光检查：Fail" + rxpow.ToString();
                    ModListBoxShow(this, result);
                    return false;
                }
            }

            if (GlobalVarFun.setup.rx_apd_cal) // APD 检查后面2个点
            {
                //设置TXSFP光源5
                switch (Dut)
                {
                    case 1:
                        opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[4]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[4] = rxpow;
                        TestResult.rxPwrDDM[4] = rxpow;
                        break;
                    case 2:
                        opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[4]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[4] = rxpow;
                        TestResult.rxPwrDDM[4] = rxpow;
                        break;
                    default:
                        break;
                }
                result.message = "检查点5：PASS";
                ModListBoxShow(this, result);

                //设置TXSFP光源4

                switch (Dut)
                {
                    case 1:
                        opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[3]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[3] = rxpow;
                        TestResult.rxPwrDDM[3] = rxpow;
                        break;
                    case 2:
                        opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[3]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[3] = rxpow;
                        TestResult2.rxPwrDDM[3] = rxpow;
                        break;
                    default:
                        break;
                }

                result.message = "检查点4：PASS";
                ModListBoxShow(this, result);
                switch (Dut)
                {
                    case 1:
                        TestResult.rxPwrErr[3] = Convert.ToSingle(retutntxrxresult.RxddmPowers[3]) - Convert.ToSingle(TestResult.rxPwrReal[3]);
                        temp = Math.Abs(TestResult.rxPwrErr[3]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点4：Fail" + "ERR:" + TestResult.rxPwrErr[3].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[3].ToString() + "RxRealPwr" + TestResult.rxPwrReal[3].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }

                        TestResult.rxPwrErr[4] = Convert.ToSingle(retutntxrxresult.RxddmPowers[4]) - Convert.ToSingle(TestResult.rxPwrReal[4]);
                        temp = Math.Abs(TestResult.rxPwrErr[4]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点5：Fail" + "ERR:" + TestResult.rxPwrErr[4].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[4].ToString() + "RxRealPwr" + TestResult.rxPwrReal[4].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                        break;
                    case 2:
                        TestResult2.rxPwrErr[3] = Convert.ToSingle(retutntxrxresult.RxddmPowers[3]) - Convert.ToSingle(TestResult2.rxPwrReal[3]);
                        temp = Math.Abs(TestResult2.rxPwrErr[3]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点4：Fail" + "ERR:" + TestResult2.rxPwrErr[3].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[3].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[3].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }

                        TestResult2.rxPwrErr[4] = Convert.ToSingle(retutntxrxresult.RxddmPowers[4]) - Convert.ToSingle(TestResult2.rxPwrReal[4]);
                        temp = Math.Abs(TestResult2.rxPwrErr[4]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点5：Fail" + "ERR:" + TestResult2.rxPwrErr[4].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[4].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[4].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }

            //设置TXSFP光源3
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[2] = rxpow;
                    TestResult.rxPwrDDM[2] = rxpow;
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[2] = rxpow;
                    TestResult2.rxPwrDDM[2] = rxpow;
                    break;
                default:
                    break;
            }
            result.message = "检查点3：PASS";
            ModListBoxShow(this, result);

            //设置TXSFP光源2

            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[1]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[1] = rxpow;
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[1]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[1] = rxpow;
                    TestResult2.rxPwrDDM[1] = rxpow;
                    break;
                default:
                    break;
            }
            retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Real[1];

            result.message = "检查点2：PASS";
            ModListBoxShow(this, result);

            //设置TXSFP光源1

            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[0]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[0] = rxpow;
                    TestResult.rxPwrDDM[0] = rxpow;
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[0]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[0] = rxpow;
                    TestResult2.rxPwrDDM[0] = rxpow;
                    break;
                default:
                    break;
            }
            result.message = "检查点1：PASS";
            ModListBoxShow(this, result);
            switch (Dut)
            {
                case 1:
                    TestResult.rxPwrErr[0] = Convert.ToSingle(retutntxrxresult.RxddmPowers[0]) - Convert.ToSingle(TestResult.rxPwrReal[0]);
                    temp = Math.Abs(TestResult.rxPwrErr[0]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点1：Fail" + "ERR:" + TestResult.rxPwrErr[0].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[0].ToString() + "RxRealPwr" + TestResult.rxPwrReal[0].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult.rxPwrErr[1] = Convert.ToSingle(retutntxrxresult.RxddmPowers[1]) - Convert.ToSingle(TestResult.rxPwrReal[1]);
                    temp = Math.Abs(TestResult.rxPwrErr[1]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点2：Fail" + "ERR:" + TestResult.rxPwrErr[1].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[1].ToString() + "RxRealPwr" + TestResult.rxPwrReal[1].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult.rxPwrErr[2] = Convert.ToSingle(retutntxrxresult.RxddmPowers[2]) - Convert.ToSingle(TestResult.rxPwrReal[2]);
                    temp = Math.Abs(TestResult.rxPwrErr[2]);
                    if (temp > rxPwrMaxErr)
                    {
                        //return false;
                        if (GlobalVarFun.moduleType.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                        {
                            return true;
                        }
                        else
                        {
                            result.message = "检查点3：Fail" + "ERR:" + TestResult.rxPwrErr[2].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[2].ToString() + "RxRealPwr" + TestResult.rxPwrReal[2].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                    }
                    break;
                case 2:
                    TestResult2.rxPwrErr[0] = Convert.ToSingle(retutntxrxresult.RxddmPowers[0]) - Convert.ToSingle(TestResult2.rxPwrReal[0]);
                    temp = Math.Abs(TestResult2.rxPwrErr[0]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点1：Fail" + "ERR:" + TestResult2.rxPwrErr[0].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[0].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[0].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult2.rxPwrErr[1] = Convert.ToSingle(retutntxrxresult.RxddmPowers[1]) - Convert.ToSingle(TestResult2.rxPwrReal[1]);
                    temp = Math.Abs(TestResult2.rxPwrErr[1]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点2：Fail" + "ERR:" + TestResult2.rxPwrErr[1].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[1].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[1].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult2.rxPwrErr[2] = Convert.ToSingle(retutntxrxresult.RxddmPowers[2]) - Convert.ToSingle(TestResult2.rxPwrReal[2]);
                    temp = Math.Abs(TestResult2.rxPwrErr[2]);
                    if (temp > rxPwrMaxErr)
                    {
                        //return false;
                        if (GlobalVarFun.moduleType.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                        {
                            return true;
                        }
                        else
                        {
                            result.message = "检查点3：Fail" + "ERR:" + TestResult2.rxPwrErr[2].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[2].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[2].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }

            return true;
        }

        private async Task<bool> RxPwrErrorCheck_Async()
        {
            string slotStr = GlobalVarFun.VOArxDutToSlot[Dut];
            int VoaChannel = GlobalVarFun.DutToVoaCh[Dut];

            otp12.VOA_SetOutputState(VoaChannel, "ON");

            otp12.SetSlot(slotStr);
            //先关闭ALC自动功率跟踪（ALC开启时会自动调节衰减，手动设置会被覆盖）
            otp12.VOA_SetAlcState(Dut, "OFF");
            //设置工作模式为衰减模式（而非功率模式POWer）
            otp12.VOA_SetMode(Dut, "ATTenuation");
            //设置操作模式为绝对值模式ABSolute（而非参考值模式REFerence）
            otp12.VOA_SetApMode(Dut, "ABSolute");
            //打开输出光路（确保光路上有输出）

            float rxpow = 0;
            float temp = 0;
            ReturnReuslt result = new ReturnReuslt();
            // 收无光检测
            if (GlobalVarFun.setup.rx_nopwr_test)
            {
                switch (Dut)
                {
                    case 1:
                        if (DOA.currentAtt != 60)
                        {
                            otp12.VOA_SetAttenuation(Dut, 60);
                        }
                        break;
                    case 2:
                        if (DOA2.currentAtt != 60)
                        {
                            otp12.VOA_SetAttenuation(Dut, 60);
                        }
                        break;
                    case 3:
                        if (DOA3.currentAtt != 60)
                        {
                            otp12.VOA_SetAttenuation(Dut, 60);
                        }
                        break;
                    case 4:
                        if (DOA4.currentAtt != 60)
                        {
                            otp12.VOA_SetAttenuation(Dut, 60);
                        }
                        break;
                    default:
                        break;
                }
                //
                rxpow = test.GetRxPower();
                retutntxrxresult.RxddmPowers[5] = rxpow;
                if (rxpow > -40)
                {
                    result.message = "Rx无光检查：Fail" + rxpow.ToString();
                    ModListBoxShow(this, result);
                    return false;
                }
            }
            await Task.Delay(waittimes);
            if (GlobalVarFun.setup.rx_apd_cal) // APD 检查后面2个点
            {
                //设置TXSFP光源5
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxCheckAtt[4]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[4] = rxpow;
                        TestResult.rxPwrDDM[4] = rxpow;
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxCheckAtt[4]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[4] = rxpow;
                        TestResult2.rxPwrDDM[4] = rxpow;
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxCheckAtt[4]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[4] = rxpow;
                        TestResult3.rxPwrDDM[4] = rxpow;
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxCheckAtt[4]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[4] = rxpow;
                        TestResult4.rxPwrDDM[4] = rxpow;
                        break;
                    default:
                        break;
                }
                result.message = "检查点5：PASS";
                ModListBoxShow(this, result);

                //设置TXSFP光源4

                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxCheckAtt[3]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[3] = rxpow;
                        TestResult.rxPwrDDM[3] = rxpow;
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxCheckAtt[3]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[3] = rxpow;
                        TestResult2.rxPwrDDM[3] = rxpow;
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxCheckAtt[3]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[3] = rxpow;
                        TestResult3.rxPwrDDM[3] = rxpow;
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxCheckAtt[3]);
                        rxpow = test.GetRxPower();
                        retutntxrxresult.RxddmPowers[3] = rxpow;
                        TestResult4.rxPwrDDM[3] = rxpow;
                        break;
                    default:
                        break;
                }
                await Task.Delay(waittimes);
                result.message = "检查点4：PASS";
                ModListBoxShow(this, result);
                switch (Dut)
                {
                    case 1:
                        TestResult.rxPwrErr[3] = Convert.ToSingle(retutntxrxresult.RxddmPowers[3]) - Convert.ToSingle(TestResult.rxPwrReal[3]);
                        temp = Math.Abs(TestResult.rxPwrErr[3]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点4：Fail" + "ERR:" + TestResult.rxPwrErr[3].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[3].ToString() + "RxRealPwr" + TestResult.rxPwrReal[3].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }

                        TestResult.rxPwrErr[4] = Convert.ToSingle(retutntxrxresult.RxddmPowers[4]) - Convert.ToSingle(TestResult.rxPwrReal[4]);
                        temp = Math.Abs(TestResult.rxPwrErr[4]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点5：Fail" + "ERR:" + TestResult.rxPwrErr[4].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[4].ToString() + "RxRealPwr" + TestResult.rxPwrReal[4].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                        break;
                    case 2:
                        TestResult2.rxPwrErr[3] = Convert.ToSingle(retutntxrxresult.RxddmPowers[3]) - Convert.ToSingle(TestResult2.rxPwrReal[3]);
                        temp = Math.Abs(TestResult2.rxPwrErr[3]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点4：Fail" + "ERR:" + TestResult2.rxPwrErr[3].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[3].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[3].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }

                        TestResult2.rxPwrErr[4] = Convert.ToSingle(retutntxrxresult.RxddmPowers[4]) - Convert.ToSingle(TestResult2.rxPwrReal[4]);
                        temp = Math.Abs(TestResult2.rxPwrErr[4]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点5：Fail" + "ERR:" + TestResult2.rxPwrErr[4].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[4].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[4].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                        break;
                    case 3:
                        TestResult3.rxPwrErr[3] = Convert.ToSingle(retutntxrxresult.RxddmPowers[3]) - Convert.ToSingle(TestResult3.rxPwrReal[3]);
                        temp = Math.Abs(TestResult3.rxPwrErr[3]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点4：Fail" + "ERR:" + TestResult3.rxPwrErr[3].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[3].ToString() + "RxRealPwr" + TestResult3.rxPwrReal[3].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }

                        TestResult3.rxPwrErr[4] = Convert.ToSingle(retutntxrxresult.RxddmPowers[4]) - Convert.ToSingle(TestResult3.rxPwrReal[4]);
                        temp = Math.Abs(TestResult3.rxPwrErr[4]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点5：Fail" + "ERR:" + TestResult3.rxPwrErr[4].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[4].ToString() + "RxRealPwr" + TestResult3.rxPwrReal[4].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                        break;
                    case 4:
                        TestResult4.rxPwrErr[3] = Convert.ToSingle(retutntxrxresult.RxddmPowers[3]) - Convert.ToSingle(TestResult4.rxPwrReal[3]);
                        temp = Math.Abs(TestResult4.rxPwrErr[3]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点4：Fail" + "ERR:" + TestResult4.rxPwrErr[3].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[3].ToString() + "RxRealPwr" + TestResult4.rxPwrReal[3].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }

                        TestResult4.rxPwrErr[4] = Convert.ToSingle(retutntxrxresult.RxddmPowers[4]) - Convert.ToSingle(TestResult4.rxPwrReal[4]);
                        temp = Math.Abs(TestResult4.rxPwrErr[4]);
                        if (temp > rxPwrMaxErr)
                        {
                            result.message = "检查点5：Fail" + "ERR:" + TestResult4.rxPwrErr[4].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[4].ToString() + "RxRealPwr" + TestResult4.rxPwrReal[4].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }

            //设置TXSFP光源3
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[2] = rxpow;
                    TestResult.rxPwrDDM[2] = rxpow;
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[2] = rxpow;
                    TestResult2.rxPwrDDM[2] = rxpow;
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[2] = rxpow;
                    TestResult3.rxPwrDDM[2] = rxpow;
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[2] = rxpow;
                    TestResult4.rxPwrDDM[2] = rxpow;
                    break;
                default:
                    break;
            }
            result.message = "检查点3：PASS";
            ModListBoxShow(this, result);

            //设置TXSFP光源2
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[1] = rxpow;
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Real[1];
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[1] = rxpow;
                    TestResult2.rxPwrDDM[1] = rxpow;
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Real[1];
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[1] = rxpow;
                    TestResult3.rxPwrDDM[1] = rxpow;
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Real[1];
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxCheckAtt[2]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[1] = rxpow;
                    TestResult4.rxPwrDDM[1] = rxpow;
                    retutntxrxresult.RxRealPowers[1] = TestSet.rxPwr_Real[1];
                    break;
                default:
                    break;
            }


            result.message = "检查点2：PASS";
            ModListBoxShow(this, result);

            await Task.Delay(waittimes);
            //设置TXSFP光源1
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxCheckAtt[0]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[0] = rxpow;
                    TestResult.rxPwrDDM[0] = rxpow;
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxCheckAtt[0]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[0] = rxpow;
                    TestResult2.rxPwrDDM[0] = rxpow;
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxCheckAtt[0]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[0] = rxpow;
                    TestResult3.rxPwrDDM[0] = rxpow;
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxCheckAtt[0]);
                    rxpow = test.GetRxPower();
                    retutntxrxresult.RxddmPowers[0] = rxpow;
                    TestResult4.rxPwrDDM[0] = rxpow;
                    break;
                default:
                    break;
            }
            result.message = "检查点1：PASS";
            ModListBoxShow(this, result);
            switch (Dut)
            {
                case 1:
                    TestResult.rxPwrErr[0] = Convert.ToSingle(retutntxrxresult.RxddmPowers[0]) - Convert.ToSingle(TestResult.rxPwrReal[0]);
                    temp = Math.Abs(TestResult.rxPwrErr[0]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点1：Fail" + "ERR:" + TestResult.rxPwrErr[0].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[0].ToString() + "RxRealPwr" + TestResult.rxPwrReal[0].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult.rxPwrErr[1] = Convert.ToSingle(retutntxrxresult.RxddmPowers[1]) - Convert.ToSingle(TestResult.rxPwrReal[1]);
                    temp = Math.Abs(TestResult.rxPwrErr[1]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点2：Fail" + "ERR:" + TestResult.rxPwrErr[1].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[1].ToString() + "RxRealPwr" + TestResult.rxPwrReal[1].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult.rxPwrErr[2] = Convert.ToSingle(retutntxrxresult.RxddmPowers[2]) - Convert.ToSingle(TestResult.rxPwrReal[2]);
                    temp = Math.Abs(TestResult.rxPwrErr[2]);
                    if (temp > rxPwrMaxErr)
                    {
                        //return false;
                        if (GlobalVarFun.moduleType.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                        {
                            return true;
                        }
                        else
                        {
                            result.message = "检查点3：Fail" + "ERR:" + TestResult.rxPwrErr[2].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[2].ToString() + "RxRealPwr" + TestResult.rxPwrReal[2].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                    }
                    break;
                case 2:
                    TestResult2.rxPwrErr[0] = Convert.ToSingle(retutntxrxresult.RxddmPowers[0]) - Convert.ToSingle(TestResult2.rxPwrReal[0]);
                    temp = Math.Abs(TestResult2.rxPwrErr[0]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点1：Fail" + "ERR:" + TestResult2.rxPwrErr[0].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[0].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[0].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult2.rxPwrErr[1] = Convert.ToSingle(retutntxrxresult.RxddmPowers[1]) - Convert.ToSingle(TestResult2.rxPwrReal[1]);
                    temp = Math.Abs(TestResult2.rxPwrErr[1]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点2：Fail" + "ERR:" + TestResult2.rxPwrErr[1].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[1].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[1].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult2.rxPwrErr[2] = Convert.ToSingle(retutntxrxresult.RxddmPowers[2]) - Convert.ToSingle(TestResult2.rxPwrReal[2]);
                    temp = Math.Abs(TestResult2.rxPwrErr[2]);
                    if (temp > rxPwrMaxErr)
                    {
                        //return false;
                        if (GlobalVarFun.moduleType.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                        {
                            return true;
                        }
                        else
                        {
                            result.message = "检查点3：Fail" + "ERR:" + TestResult2.rxPwrErr[2].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[2].ToString() + "RxRealPwr" + TestResult2.rxPwrReal[2].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                    }
                    break;
                case 3:
                    TestResult3.rxPwrErr[0] = Convert.ToSingle(retutntxrxresult.RxddmPowers[0]) - Convert.ToSingle(TestResult3.rxPwrReal[0]);
                    temp = Math.Abs(TestResult3.rxPwrErr[0]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点1：Fail" + "ERR:" + TestResult3.rxPwrErr[0].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[0].ToString() + "RxRealPwr" + TestResult3.rxPwrReal[0].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult3.rxPwrErr[1] = Convert.ToSingle(retutntxrxresult.RxddmPowers[1]) - Convert.ToSingle(TestResult3.rxPwrReal[1]);
                    temp = Math.Abs(TestResult3.rxPwrErr[1]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点2：Fail" + "ERR:" + TestResult3.rxPwrErr[1].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[1].ToString() + "RxRealPwr" + TestResult3.rxPwrReal[1].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult3.rxPwrErr[2] = Convert.ToSingle(retutntxrxresult.RxddmPowers[2]) - Convert.ToSingle(TestResult3.rxPwrReal[2]);
                    temp = Math.Abs(TestResult3.rxPwrErr[2]);
                    if (temp > rxPwrMaxErr)
                    {
                        //return false;
                        if (GlobalVarFun.moduleType.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                        {
                            return true;
                        }
                        else
                        {
                            result.message = "检查点3：Fail" + "ERR:" + TestResult3.rxPwrErr[2].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[2].ToString() + "RxRealPwr" + TestResult3.rxPwrReal[2].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                    }
                    break;
                case 4:
                    TestResult4.rxPwrErr[0] = Convert.ToSingle(retutntxrxresult.RxddmPowers[0]) - Convert.ToSingle(TestResult4.rxPwrReal[0]);
                    temp = Math.Abs(TestResult4.rxPwrErr[0]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点1：Fail" + "ERR:" + TestResult4.rxPwrErr[0].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[0].ToString() + "RxRealPwr" + TestResult4.rxPwrReal[0].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult4.rxPwrErr[1] = Convert.ToSingle(retutntxrxresult.RxddmPowers[1]) - Convert.ToSingle(TestResult4.rxPwrReal[1]);
                    temp = Math.Abs(TestResult4.rxPwrErr[1]);
                    if (temp > rxPwrMaxErr)
                    {
                        result.message = "检查点2：Fail" + "ERR:" + TestResult4.rxPwrErr[1].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[1].ToString() + "RxRealPwr" + TestResult4.rxPwrReal[1].ToString();
                        ModListBoxShow(this, result);
                        return false;
                    }

                    TestResult4.rxPwrErr[2] = Convert.ToSingle(retutntxrxresult.RxddmPowers[2]) - Convert.ToSingle(TestResult4.rxPwrReal[2]);
                    temp = Math.Abs(TestResult4.rxPwrErr[2]);
                    if (temp > rxPwrMaxErr)
                    {
                        //return false;
                        if (GlobalVarFun.moduleType.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                        {
                            return true;
                        }
                        else
                        {
                            result.message = "检查点3：Fail" + "ERR:" + TestResult4.rxPwrErr[2].ToString() + " DDM:"
                                + retutntxrxresult.RxddmPowers[2].ToString() + "RxRealPwr" + TestResult4.rxPwrReal[2].ToString();
                            ModListBoxShow(this, result);
                            return false;
                        }
                    }
                    break;
                default:
                    break;
            }

            return true;
        }
        #endregion

        private async Task<bool> RxSenBitErrorCheck_Async()
        {
            string errmsg = "";
            string Status = ""; // BERT返回的原始状态字符串
            double berThreshold = 5e-5;
            ReturnReuslt result = new ReturnReuslt();

            // VOA初始化 选择当前Dut对应的OTP-12槽位
            string slotStr = GlobalVarFun.VOArxDutToSlot[Dut];
            otp12.SetSlot(slotStr);
            otp12.VOA_SetAlcState(Dut, "OFF"); // 关闭自动光功率控制（ALC）
            otp12.VOA_SetMode(Dut, "ATTenuation");
            otp12.VOA_SetApMode(Dut, "ABSolute");
            otp12.VOA_SetOutputState(Dut, "ON");
            // 切换光开关到接收方向
            otp12.SW_SetRouteForModule(Dut, false);
            await Task.Delay(500); // 等待500ms让硬件稳定

            // 配置BERT速率和码型（10G31 = 10.3125Gbps, PRBS31）
            // 这里要做区分,不同的模块要选择不同的速率和码型
            if (true)
            {

            }
            otp12.BERT_SetRate("10G31");
            otp12.BERT_SetPattern(0);

            // 设置到饱和点
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxOverLoadAtt); //饱和点
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxOverLoadAtt); //饱和点
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxOverLoadAtt); //饱和点
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxOverLoadAtt); //饱和点
                    break;
            }

            await Task.Delay(500);
            if (GlobalVarFun.setup.bert_connect && GlobalVarFun.setup.rx_sen_test)
            {
                try
                {
                    otp12.BERT_ClearAllErr();//// 清除BERT误码计数器
                    Status = otp12.BERT_GetErrData(Dut);// 读取误码数据，格式："errBits totalBits lockFlag"
                    //string error = "";
                    try
                    {
                        // 解析误码数据...
                        string[] parts = Status.Trim().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3)
                        {
                            long errBits = long.Parse(parts[0]);// 误码比特数
                            long totalBits = long.Parse(parts[1]);// 总传输比特数
                            int lockFlag = int.Parse(parts[2]);// 时钟锁定标志(1=锁定,0=失步)

                            if (lockFlag == 1) // 锁定
                            {
                                double ber = totalBits > 0 ? (double)errBits / totalBits : 0; //计算实际误码率
                                if (errBits == 0 || ber <= berThreshold)
                                {
                                    result.message = "饱和光功率测试PASS: " + Status + " BER=" + ber.ToString("E6");
                                    ModListBoxShow(this, result);
                                    //饱和光功率测试PASS
                                }
                                else
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        errmsg += "饱和光功率测试失败：\r\n";
                                    }
                                    else
                                    {
                                        errmsg += "Saturation optical power test failed：\r\n";
                                    }
                                    errmsg += Status + " BER=" + ber.ToString("E6") + "\r\n";
                                    result.message = "饱和光功率测试失败：误码率=" + ber.ToString("E6");
                                    ModListBoxShow(this, result);
                                }
                            }
                            else // lockFlag == 0 失步
                            {
                                if (GlobalVarFun.Language == "Chinese")
                                {
                                    errmsg += "饱和光功率测试失败(失步)：\r\n";
                                }
                                else
                                {
                                    errmsg += "Saturation optical power test failed (no lock)：\r\n";
                                }
                                errmsg += Status + "\r\n";
                                result.message = "饱和光功率测试：失步 " + Status;
                                ModListBoxShow(this, result);
                            }
                        }
                        else
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                errmsg += "饱和光功率测试失败,误码数据异常：\r\n";
                            }
                            else
                            {
                                errmsg += "Saturation light power test failed,Bit error exception：\r\n";
                            }
                            errmsg += Status + "\r\n";
                        }
                    }
                    catch
                    {
                        errmsg += "饱和光功率测试失败,误码数据解析异常：\r\n";
                        errmsg += Status + "\r\n";
                    }
                    if (errmsg == "") errmsg = "";
                    result.message = "饱和光功率测试：" + (errmsg == "" ? "PASS" : errmsg);
                    ModListBoxShow(this, result);
                }
                catch
                {
                    errmsg += "饱和光功率测试失败,误码率获取异常：\r\n";
                    result.message = "饱和光功率测试：" + errmsg;
                    ModListBoxShow(this, result);
                }
            }
            //将衰减器调到灵敏度点（衰减最大，入射光最弱），测试最小接收光功率下的误码率。
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxSenAtt);      //灵敏度点
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxSenAtt);      //灵敏度点
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxSenAtt);      //灵敏度点
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxSenAtt);      //灵敏度点
                    break;
            }

            await Task.Delay(500);

            if (GlobalVarFun.setup.bert_connect && GlobalVarFun.setup.rx_sen_test)
            {
                otp12.BERT_ClearAllErr();//清除误码率              
                errmsg += sencheck(Dut);
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxSenAtt - 2);      //灵敏度点-2dB
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxSenAtt - 2);      //灵敏度点-2dB
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxSenAtt - 2);      //灵敏度点-2dB
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxSenAtt - 2);      //灵敏度点-2dB
                        break;
                }

                otp12.BERT_ClearAllErr();//清除误码率     
                await Task.Delay(500);
                errmsg += sencheck(Dut);

                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxOverLoadAtt);  //光饱和点
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxOverLoadAtt);  //光饱和点
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxOverLoadAtt);  //光饱和点
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxOverLoadAtt);  //光饱和点
                        break;
                }

                otp12.BERT_ClearAllErr();//清除误码率     
                await Task.Delay(500);
                errmsg += sencheck(Dut);
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxOverLoadAtt + 3);  //光饱和点+3dB
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxOverLoadAtt + 3);  //光饱和点+3dB
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxOverLoadAtt + 3);  //光饱和点+3dB
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxOverLoadAtt + 3);  //光饱和点+3dB
                        break;
                }

                otp12.BERT_ClearAllErr();//清除误码率     
                await Task.Delay(500);
                errmsg += sencheck(Dut);
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxSenAtt);      //回到灵敏度点
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxSenAtt);      //回到灵敏度点
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxSenAtt);      //回到灵敏度点
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxSenAtt);      //回到灵敏度点
                        break;
                }

                otp12.BERT_ClearAllErr();//清除误码率     
                await Task.Delay(1000);
                errmsg += sencheck(Dut);
                retutntxrxresult.ErrorMessage = errmsg;

                if (errmsg != "")
                {
                    return false;
                }
            }
            return true;
        }
        //
        private string sencheck(int ch)
        {
            string error = "";
            string Status = "";
            double berThreshold = 5e-5;
            // 用于界面日志显示的结果对象
            ReturnReuslt result = new ReturnReuslt();
            try
            {
                //// ch=Dut编号(1-4)，读取该通道误码数据 `"errBits totalBits lockFlag"`
                Status = otp12.BERT_GetErrData(ch);
                string fpn = "";
                switch (Dut)
                {
                    case 1:
                        fpn = TestResult.fibertop_pn;
                        break;
                    case 2:
                        fpn = TestResult2.fibertop_pn;
                        break;
                    case 3:
                        fpn = TestResult3.fibertop_pn;
                        break;
                    case 4:
                        fpn = TestResult4.fibertop_pn;
                        break;
                    default:
                        break;
                }

                try
                {
                    string[] parts = Status.Trim().Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        long errBits = long.Parse(parts[0]);
                        long totalBits = long.Parse(parts[1]);
                        int lockFlag = int.Parse(parts[2]);

                        if (lockFlag == 1) // 同步锁定
                        {
                            double ber = totalBits > 0 ? (double)errBits / totalBits : 0;
                            result.message = "误码率 : " + Status + " BER=" + ber.ToString("E6");
                            ModListBoxShow(this, result);

                            if (errBits == 0 || ber <= berThreshold)
                            {
                                result.message = "灵敏度测试PASS: " + Status + " BER=" + ber.ToString("E6");
                                ModListBoxShow(this, result);
                                //灵敏度测试PASS
                            }
                            else
                            {
                                if (GlobalVarFun.Language == "Chinese")
                                {
                                    error += "灵敏度测试失败：\r\n";
                                }
                                else
                                {
                                    error += "Sensitivity test failure：\r\n";
                                }
                                error += Status + " BER=" + ber.ToString("E6") + "\r\n";
                            }
                        }
                        else // lockFlag == 0 失步
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                error += "灵敏度测试失败(失步)：\r\n";
                            }
                            else
                            {
                                error += "Sensitivity test failure (no lock)：\r\n";
                            }
                            error += Status + "\r\n";
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            error += "灵敏度测试失败,误码数据格式异常：\r\n";
                        }
                        else
                        {
                            error += "Sensitivity test failure,Bit error exception：\r\n";
                        }
                        error += Status + "\r\n";
                    }
                }
                catch (System.Exception parseEx)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        error += "灵敏度测试失败,误码数据解析异常：\r\n";
                    }
                    else
                    {
                        error += "Sensitivity test failure,Bit error parse exception：\r\n";
                    }
                    error += Status + " " + parseEx.Message + "\r\n";
                }
            }
            catch
            {
                error += "灵敏度测试失败,获取误码异常：\r\n";
                error += Status + "\r\n";
            }
            if (error == "") error = "PASS";
            result.message = "灵敏度测试：" + error;
            ModListBoxShow(this, result);

            return error == "PASS" ? "" : error;
        }


        #region// 接收LOS 及告警功率 功能检查
        private bool RxLosAlarmCheck()
        {
            string errmsg = "";
            float att_val = 0;
            ReturnReuslt result = new ReturnReuslt();
            result.message = "接收LOS 及告警功率 功能检查 ";
            ModListBoxShow(this, result);
            //先切换到灵敏度点
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxSenAtt);
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxSenAtt);
                    break;
            }

            //de los  去告警
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxDLosAtt);
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxDLosAtt);
                    break;
            }

            // LOS 去告警
            if (test.CheckRxLOS() == true)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "LOS 去告警异常_01 \r\n";
                }
                else
                {
                    errmsg += "LOS De-Assert Alarm Exception 01 \r\n";
                }
            }

            if (GlobalVarFun.setup.rx_hardware_los)
            {
                //if (i2c.HardWare_LOS_Get() != true)
                //{
                //    if (GlobalVarFun.Language == "Chinese")
                //    {
                //        errmsg += "硬件LOS 去告警异常 \r\n";
                //    }
                //    else
                //    {
                //        errmsg += "Hardware LOS De-Assert  \r\n";
                //    }
                //}
            }
            // 逐步+2dB逼近 设置

            switch (Dut)
            {
                case 1:
                    att_val = DOA.rxDLosAtt;
                    break;
                case 2:
                    att_val = DOA2.rxDLosAtt;
                    break;
            }
            att_val = att_val + 2.0f;
            while (att_val < DOA.rxALosAtt)
            {
                result.message = "att_val : " + att_val.ToString();
                ModListBoxShow(this, result);

                opticaldoaatt.SetAttenuation(att_val);
                att_val = att_val + 2.0f;
            }

            // los 告警
            if (GlobalVarFun.testType == "firstTest")
            {
                switch (Dut)
                {
                    case 1:
                        opticaldoaatt.SetAttenuation(DOA.rxALosAtt + 0.8f); // 后置0.8dB
                        break;
                    case 2:
                        opticaldoaatt.SetAttenuation(DOA2.rxALosAtt + 0.8f); // 后置0.8dB
                        break;
                }

            }
            else
            {
                switch (Dut)
                {
                    case 1:
                        opticaldoaatt.SetAttenuation(DOA.rxALosAtt);
                        break;
                    case 2:
                        opticaldoaatt.SetAttenuation(DOA2.rxALosAtt);
                        break;
                }

            }
            // LOS 告警
            if (test.CheckRxLOS() == false)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "LOS告警异常 \r\n";
                }
                else
                {
                    errmsg += "LOS Assert alarm Exception \r\n";
                }
                //return false;
            }

            if (GlobalVarFun.setup.rx_hardware_los)
            {
                //if (i2c.HardWare_LOS_Get() == true)
                //{
                //    if (GlobalVarFun.Language == "Chinese")
                //    {
                //        errmsg += "硬件LOS 告警异常 \r\n";
                //    }
                //    else
                //    {
                //        errmsg += "The hardware LOS Assert alarm is abnormal \r\n";
                //    }
                //}
            }
            // 逐步-1dB逼近 设置

            switch (Dut)
            {
                case 1:
                    att_val = DOA.rxALosAtt;
                    break;
                case 2:
                    att_val = DOA2.rxALosAtt;
                    break;
            }
            att_val = att_val - 1.0f;
            while (att_val > DOA.rxDLosAtt)
            {
                result.message = "att_val : " + att_val.ToString();
                ModListBoxShow(this, result);
                opticaldoaatt.SetAttenuation(att_val);
                att_val = att_val - 1.0f;
            }

            //de los  去告警
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxDLosAtt);
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxDLosAtt);
                    break;
            }

            // LOS 去告警
            if (test.CheckRxLOS() == true)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "LOS 去告警异常_02 \r\n";
                }
                else
                {
                    errmsg += "LOS De-Assert  Alarm Exception 02 \r\n";
                }
                //return false;
            }
            //
            retutntxrxresult.ErrorMessage = errmsg;
            result.message = "接收LOS 及告警功率 功能检查 : " + errmsg;
            ModListBoxShow(this, result);
            //
            if (errmsg != "")
            {
                return false;
            }

            return true;
        }

        private async Task<bool> RxLosAlarmCheck_Async()
        {
            string slotStr = GlobalVarFun.VOArxDutToSlot[Dut];
            int VoaChannel = GlobalVarFun.DutToVoaCh[Dut];

            otp12.VOA_SetOutputState(VoaChannel, "ON");

            otp12.SetSlot(slotStr);
            //先关闭ALC自动功率跟踪（ALC开启时会自动调节衰减，手动设置会被覆盖）
            otp12.VOA_SetAlcState(Dut, "OFF");
            //设置工作模式为衰减模式（而非功率模式POWer）
            otp12.VOA_SetMode(Dut, "ATTenuation");
            //设置操作模式为绝对值模式ABSolute（而非参考值模式REFerence）
            otp12.VOA_SetApMode(Dut, "ABSolute");
            //打开输出光路（确保光路上有输出）

            string errmsg = "";
            float att_val = 0;
            ReturnReuslt result = new ReturnReuslt();
            result.message = "接收LOS 及告警功率 功能检查 ";
            ModListBoxShow(this, result);
            //先切换到灵敏度点
            await Task.Delay(waittimes);
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxSenAtt);
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxSenAtt);
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxSenAtt);
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxSenAtt);
                    break;
            }

            await Task.Delay(waittimes);
            //de los  去告警
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxDLosAtt);
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxDLosAtt);
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxDLosAtt);
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxDLosAtt);
                    break;
            }

            // LOS 去告警
            await Task.Delay(waittimes);
            if (test.CheckRxLOS() == true)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "LOS 去告警异常_01 \r\n";
                }
                else
                {
                    errmsg += "LOS De-Assert Alarm Exception 01 \r\n";
                }
            }

            if (GlobalVarFun.setup.rx_hardware_los)
            {
                //if (i2c.HardWare_LOS_Get() != true)
                //{
                //    if (GlobalVarFun.Language == "Chinese")
                //    {
                //        errmsg += "硬件LOS 去告警异常 \r\n";
                //    }
                //    else
                //    {
                //        errmsg += "Hardware LOS De-Assert  \r\n";
                //    }
                //}
            }
            // 逐步+2dB逼近 设置

            switch (Dut)
            {
                case 1:
                    att_val = DOA.rxDLosAtt;
                    att_val = att_val + 2.0f;
                    while (att_val < DOA.rxALosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);

                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val + 2.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
                case 2:
                    att_val = DOA2.rxDLosAtt;
                    att_val = att_val + 2.0f;
                    while (att_val < DOA2.rxALosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);

                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val + 2.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
                case 3:
                    att_val = DOA3.rxDLosAtt;
                    att_val = att_val + 2.0f;
                    while (att_val < DOA3.rxALosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);

                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val + 2.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
                case 4:
                    att_val = DOA4.rxDLosAtt;
                    att_val = att_val + 2.0f;
                    while (att_val < DOA4.rxALosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);

                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val + 2.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
            }

            // los 告警
            await Task.Delay(waittimes);
            if (GlobalVarFun.testType == "firstTest")
            {
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxALosAtt + 0.8f);
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxALosAtt + 0.8f);
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxALosAtt + 0.8f);
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxALosAtt + 0.8f);
                        break;
                }

            }
            else
            {
                switch (Dut)
                {
                    case 1:
                        otp12.VOA_SetAttenuation(Dut, DOA.rxALosAtt);
                        break;
                    case 2:
                        otp12.VOA_SetAttenuation(Dut, DOA2.rxALosAtt);
                        break;
                    case 3:
                        otp12.VOA_SetAttenuation(Dut, DOA3.rxALosAtt);
                        break;
                    case 4:
                        otp12.VOA_SetAttenuation(Dut, DOA4.rxALosAtt);
                        break;
                }

            }
            // LOS 告警
            if (test.CheckRxLOS() == false)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "LOS告警异常 \r\n";
                }
                else
                {
                    errmsg += "LOS Assert alarm Exception \r\n";
                }
                //return false;
            }

            if (GlobalVarFun.setup.rx_hardware_los)
            {
                //if (i2c.HardWare_LOS_Get() == true)
                //{
                //    if (GlobalVarFun.Language == "Chinese")
                //    {
                //        errmsg += "硬件LOS 告警异常 \r\n";
                //    }
                //    else
                //    {
                //        errmsg += "The hardware LOS Assert alarm is abnormal \r\n";
                //    }
                //}
            }
            // 逐步-1dB逼近 设置
            switch (Dut)
            {
                case 1:
                    att_val = DOA.rxALosAtt;
                    att_val = att_val - 1.0f;
                    while (att_val > DOA.rxDLosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);
                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val - 1.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
                case 2:
                    att_val = DOA2.rxALosAtt;
                    att_val = att_val - 1.0f;
                    while (att_val > DOA2.rxDLosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);
                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val - 1.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
                case 3:
                    att_val = DOA3.rxALosAtt;
                    att_val = att_val - 1.0f;
                    while (att_val > DOA3.rxDLosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);
                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val - 1.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
                case 4:
                    att_val = DOA4.rxALosAtt;
                    att_val = att_val - 1.0f;
                    while (att_val > DOA4.rxDLosAtt)
                    {
                        result.message = "att_val : " + att_val.ToString();
                        ModListBoxShow(this, result);
                        otp12.VOA_SetAttenuation(Dut, att_val);
                        att_val = att_val - 1.0f;
                        await Task.Delay(waittimes);
                    }
                    break;
            }

            //de los  去告警
            await Task.Delay(waittimes);
            switch (Dut)
            {
                case 1:
                    otp12.VOA_SetAttenuation(Dut, DOA.rxDLosAtt);
                    break;
                case 2:
                    otp12.VOA_SetAttenuation(Dut, DOA2.rxDLosAtt);
                    break;
                case 3:
                    otp12.VOA_SetAttenuation(Dut, DOA3.rxDLosAtt);
                    break;
                case 4:
                    otp12.VOA_SetAttenuation(Dut, DOA4.rxDLosAtt);
                    break;
            }

            // LOS 去告警
            if (test.CheckRxLOS() == true)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "LOS 去告警异常_02 \r\n";
                }
                else
                {
                    errmsg += "LOS De-Assert  Alarm Exception 02 \r\n";
                }
                //return false;
            }
            //
            retutntxrxresult.ErrorMessage = errmsg;
            result.message = "接收LOS 及告警功率 功能检查 : " + errmsg;
            ModListBoxShow(this, result);
            //
            if (errmsg != "")
            {
                return false;
            }

            return true;
        }
        #endregion

        #region // 发射部分测试检查

        private bool TxFinalTestCheck(bool autoScale)
        {
            float tx_pwr = 0, tx_er = 0, bias = 0, tx_cr = 0, tx_jt = 0;
            string errmsg = "";
            txPwrMaxErr = GlobalVarFun.setup.txpwr_cal;
            rxPwrMaxErr = GlobalVarFun.setup.rxpwr_cal;
            erValMaxErr = GlobalVarFun.setup.er_cal;
            wLengthMaxErr = GlobalVarFun.setup.wlgth_cal;

            switchSemaphore.Wait();
            try
            {
                //光开关切换
                TestControl.opticalswitch.SetChannel(Dut);
                //发射光谱测试
                if (GlobalVarFun.setup.ms9710x_connect)
                {
                    AddTestLog("发射光谱测试");
                    string strval = string.Empty;
                    string[] strArray;
                    char[] charArray = new char[] { ' ' };
                    string[] strnew = new string[600];

                    try
                    {
                        //ms9710b.MS9710B_AUTO();
                        ms9710b.MS9710B_SSI();
                        Thread.Sleep(2000);
                        strval = ms9710b.MS9710B_APR();
                        strArray = strval.Split(charArray);

                        strnew = strArray[0].Split(',');
                        switch (Dut)
                        {
                            case 1:
                                if (strnew.Length > 3)
                                {
                                    if (TestResult.fibertop_pn.Contains("MM85") || !(TestResult.fibertop_pn.Contains("T")))
                                    {
                                        TestResult.wLength = Convert.ToDouble(strnew[0]);
                                        TestResult.spectralwidth = Convert.ToDouble(strnew[2]);
                                    }
                                    else
                                    {
                                        TestResult.spectralwidth = Convert.ToDouble(strnew[1]);
                                        TestResult.smsr = Convert.ToDouble(strnew[0]);
                                        TestResult.wLength = Convert.ToDouble(strnew[2]);
                                        //Leftwlength = strnew[4];
                                        if (TestResult.smsr < 29)
                                        {
                                            if (GlobalVarFun.Language == "Chinese")
                                            {
                                                retutntxrxresult.ErrorMessage = "边模参数异常 1！";
                                                retutntxrxresult.ErrorMessage += "边模：" + TestResult.smsr.ToString();
                                                retutntxrxresult.TestResultMessage = "边模参数异常, 请重试......";
                                            }
                                            else
                                            {
                                                retutntxrxresult.ErrorMessage = "SMSR exception!";
                                                retutntxrxresult.ErrorMessage += "SMSR：" + TestResult.smsr.ToString();
                                                retutntxrxresult.TestResultMessage = "The SMSR is abnormal. Please try again......";
                                            }
                                            return false;
                                        }
                                        if (TestResult.spectralwidth > TestSet.spectralwidth_max)
                                        {
                                            if (GlobalVarFun.Language == "Chinese")
                                            {
                                                retutntxrxresult.ErrorMessage = "谱宽参数异常 1！";
                                                retutntxrxresult.ErrorMessage += "谱宽：" + TestResult.spectralwidth.ToString();
                                                retutntxrxresult.TestResultMessage = "谱宽参数异常, 请重试......";
                                            }
                                            else
                                            {
                                                retutntxrxresult.ErrorMessage = "Spectral width exception!";
                                                retutntxrxresult.ErrorMessage += "Spectral width：" + TestResult.spectralwidth.ToString();
                                                retutntxrxresult.TestResultMessage = "The Spectral width is abnormal. Please try again......";
                                            }
                                            return false;
                                        }

                                    }
                                }
                                break;
                            case 2:
                                if (strnew.Length > 3)
                                {
                                    if (TestResult2.fibertop_pn.Contains("MM85") || !(TestResult2.fibertop_pn.Contains("T")))
                                    {
                                        TestResult2.wLength = Convert.ToDouble(strnew[0]);
                                        TestResult2.spectralwidth = Convert.ToDouble(strnew[2]);
                                    }
                                    else
                                    {
                                        TestResult2.spectralwidth = Convert.ToDouble(strnew[1]);
                                        TestResult2.smsr = Convert.ToDouble(strnew[0]);
                                        TestResult2.wLength = Convert.ToDouble(strnew[2]);
                                        //Leftwlength = strnew[4];
                                        if (TestResult2.smsr < 29)
                                        {
                                            if (GlobalVarFun.Language == "Chinese")
                                            {
                                                retutntxrxresult.ErrorMessage = "边模参数异常 1！";
                                                retutntxrxresult.ErrorMessage += "边模：" + TestResult2.smsr.ToString();
                                                retutntxrxresult.TestResultMessage = "边模参数异常, 请重试......";
                                            }
                                            else
                                            {
                                                retutntxrxresult.ErrorMessage = "SMSR exception!";
                                                retutntxrxresult.ErrorMessage += "SMSR：" + TestResult2.smsr.ToString();
                                                retutntxrxresult.TestResultMessage = "The SMSR is abnormal. Please try again......";
                                            }
                                            return false;
                                        }
                                        if (TestResult2.spectralwidth > TestSet2.spectralwidth_max)
                                        {
                                            if (GlobalVarFun.Language == "Chinese")
                                            {
                                                retutntxrxresult.ErrorMessage = "谱宽参数异常 1！";
                                                retutntxrxresult.ErrorMessage += "谱宽：" + TestResult2.spectralwidth.ToString();
                                                retutntxrxresult.TestResultMessage = "谱宽参数异常, 请重试......";
                                            }
                                            else
                                            {
                                                retutntxrxresult.ErrorMessage = "Spectral width exception!";
                                                retutntxrxresult.ErrorMessage += "Spectral width：" + TestResult2.spectralwidth.ToString();
                                                retutntxrxresult.TestResultMessage = "The Spectral width is abnormal. Please try again......";
                                            }
                                            return false;
                                        }

                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        float pwr_ave_err = 0;
                        switch (Dut)
                        {
                            case 1:
                                if (TestResult.wLength <= 0)
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        retutntxrxresult.ErrorMessage = "光谱参数采集异常 1！";
                                        retutntxrxresult.ErrorMessage += "光谱仪波长：" + TestResult.wLength.ToString();
                                        retutntxrxresult.TestResultMessage = "光谱参数获取失败, 请重试......";
                                    }
                                    else
                                    {
                                        retutntxrxresult.ErrorMessage = "Abnormal spectral parameter collection 1!";
                                        retutntxrxresult.ErrorMessage += "WaveLength：" + TestResult.wLength.ToString();
                                        retutntxrxresult.TestResultMessage = "Spectral parameter acquisition failed. Please try again......";
                                    }
                                    return false;
                                }
                                pwr_ave_err = 0;
                                pwr_ave_err = Math.Abs(TestResult.txPowerDCA + (float)GlobalVarFun.setup.dac_txpwr_err - TestResult.txPower);
                                if (pwr_ave_err > 1.5)
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        retutntxrxresult.ErrorMessage = "示波器采集平均光功率偏差大！";
                                        retutntxrxresult.ErrorMessage += "Power Average：" + TestResult.txPowerDCA.ToString();
                                        retutntxrxresult.TestResultMessage = "平均光功率偏差较大, 请检查后重试......";
                                    }
                                    else
                                    {
                                        retutntxrxresult.ErrorMessage = "The average optical power deviation collected by the oscilloscope is relatively large!";
                                        retutntxrxresult.ErrorMessage += "Power Average：" + TestResult.txPowerDCA.ToString();
                                        retutntxrxresult.TestResultMessage = "The average optical power deviation is relatively large. Please check and try again......";
                                    }
                                    return false;
                                }
                                break;
                            case 2:
                                if (TestResult2.wLength <= 0)
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        retutntxrxresult.ErrorMessage = "光谱参数采集异常 1！";
                                        retutntxrxresult.ErrorMessage += "光谱仪波长：" + TestResult2.wLength.ToString();
                                        retutntxrxresult.TestResultMessage = "光谱参数获取失败, 请重试......";
                                    }
                                    else
                                    {
                                        retutntxrxresult.ErrorMessage = "Abnormal spectral parameter collection 1!";
                                        retutntxrxresult.ErrorMessage += "WaveLength：" + TestResult2.wLength.ToString();
                                        retutntxrxresult.TestResultMessage = "Spectral parameter acquisition failed. Please try again......";
                                    }
                                    return false;
                                }
                                pwr_ave_err = 0;
                                pwr_ave_err = Math.Abs(TestResult2.txPowerDCA + (float)GlobalVarFun.setup.dac_txpwr_err - TestResult2.txPower);
                                if (pwr_ave_err > 1.5)
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        retutntxrxresult.ErrorMessage = "示波器采集平均光功率偏差大！";
                                        retutntxrxresult.ErrorMessage += "Power Average：" + TestResult2.txPowerDCA.ToString();
                                        retutntxrxresult.TestResultMessage = "平均光功率偏差较大, 请检查后重试......";
                                    }
                                    else
                                    {
                                        retutntxrxresult.ErrorMessage = "The average optical power deviation collected by the oscilloscope is relatively large!";
                                        retutntxrxresult.ErrorMessage += "Power Average：" + TestResult2.txPowerDCA.ToString();
                                        retutntxrxresult.TestResultMessage = "The average optical power deviation is relatively large. Please check and try again......";
                                    }
                                    return false;
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    catch
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ErrorMessage = "光谱仪异常 2！";
                            retutntxrxresult.TestResultMessage = "光谱仪参数获取异常, 请确认......";
                        }
                        else
                        {
                            retutntxrxresult.ErrorMessage = "Spectrometer anomaly 2!";
                            retutntxrxresult.TestResultMessage = "Abnormal acquisition of spectrometer parameters, please confirm......";
                        }
                        AddTestLog(retutntxrxresult.ErrorMessage);
                        return false;
                    }
                }
                //电源 E3632A 电流
                if (GlobalVarFun.setup.ag_e3632a_connect)
                {
                    AddTestLog("电源 E3632A 电流");
                    double Supply = 0;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.supply = agilent_e3632a.GetCurrent();
                            if (GlobalVarFun.module_insert1 && GlobalVarFun.module_insert2)
                            {
                                TestResult.supply *= 0.5f;//两只端口模块都在位，单只模块 电流 * 0.5     //2026.06.06
                            }
                            Supply = TestResult.supply;
                            break;
                        case 2:
                            TestResult2.supply = agilent_e3632a.GetCurrent();
                            if (GlobalVarFun.module_insert1 && GlobalVarFun.module_insert2)
                            {
                                TestResult2.supply *= 0.5f;//两只端口模块都在位，单只模块 电流 * 0.5    //2026.06.06
                            }
                            Supply = TestResult2.supply;
                            break;
                        default:
                            break;
                    }
                    if (Supply < 50)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ErrorMessage = "电源电流参数采集异常！";
                            retutntxrxresult.TestResultMessage = "电源电流参数获取失败, 请重试......";
                        }
                        else
                        {
                            retutntxrxresult.ErrorMessage = "Abnormal collection of power current parameters!";
                            retutntxrxresult.TestResultMessage = "Power current parameter acquisition failed. Please try......";
                        }
                        AddTestLog(retutntxrxresult.ErrorMessage);
                        return false;
                    }
                }

                //波长计
                if (GlobalVarFun.setup.kt86120x_connect)
                {
                    AddTestLog("波长计");
                    try
                    {
                        switch (Dut)
                        {
                            case 1:
                                TestResult.wLength = kt86120c.GetWavelength();
                                break;
                            case 2:
                                TestResult2.wLength = kt86120c.GetWavelength();
                                break;
                            default:
                                break;
                        }
                    }
                    catch
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ErrorMessage = "波长计异常！";
                            retutntxrxresult.TestResultMessage = "波长计参数获取异常, 请确认......";
                        }
                        else
                        {
                            retutntxrxresult.ErrorMessage = "The wavelength meter is abnormal!";
                            retutntxrxresult.TestResultMessage = "Abnormal wavelength meter parameter acquisition, please confirm......";
                        }
                        AddTestLog(retutntxrxresult.ErrorMessage);
                        return false;
                    }
                }
                // Tx 发射关闭  无光显示-40检查  //2024.1.11修改优化
                //////////////////////////////////////////////////////////////////////////////////////////////////////
                if (GlobalVarFun.setup.tx_nopwr_test)
                {
                    AddTestLog("tx_nopwr_test");
                    if (test.SoftTxDis(true) == false) // Tx Disable 软件关闭发射
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg = "软件关闭Tx发光失败！\r\n";
                        }
                        else
                        {
                            errmsg = "Software shut down Tx glow failure!\r\n";
                        }
                        retutntxrxresult.ErrorMessage = errmsg;
                        AddTestLog(errmsg);
                        return false;
                    }
                    Thread.Sleep(300);
                    if (test.GetTxPower() > -40)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg = "Tx发射无光显示-40检查失败！\r\n";
                        }
                        else
                        {
                            errmsg = "Tx transmit optical display -40 Check failed!\r\n";
                        }
                        AddTestLog(errmsg);
                        return false;
                    }
                    if (test.SoftTxDis(false) == false) // Tx Enable 软件开启发射
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg = "软件开启Tx发光操作失败01！\r\n";
                        }
                        else
                        {
                            errmsg = "Software to start Tx optical operation failed 01!\r\n";
                        }
                        AddTestLog(errmsg);
                        return false;
                    }
                    Thread.Sleep(300);
                    //
                    if (test.GetTxBias() < 2) //bias<2mA
                    {
                        if (test.SoftTxDis(false) == false) // Tx Enable 软件开启发射  异常后再次开启
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                errmsg = "软件开启Tx发光操作失败02！\r\n";
                            }
                            else
                            {
                                errmsg = "Software failed to turn on Tx  operation 02!\r\n";
                            }
                            AddTestLog(errmsg);
                            return false;
                        }
                        Thread.Sleep(100);
                        if (test.GetTxBias() < 2) //bias<2mA
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                errmsg = "软件开启Tx发光操作失败03！\r\n";
                            }
                            else
                            {
                                errmsg = "Software failed to turn on Tx  operation03!\r\n";
                            }
                            AddTestLog(errmsg);
                            return false;
                        }
                    }
                    Thread.Sleep(100); //延时100ms
                }

                if (GlobalVarFun.setup.tx_hardware_disable)
                {
                    //if (test.SetModuleDis(true) == false) // Tx Disable 硬件关闭发射
                    //{
                    //    if (GlobalVarFun.Language == "Chinese")
                    //    {
                    //        errmsg = "硬件关闭Tx发光失败！\r\n";
                    //    }
                    //    else
                    //    {
                    //        errmsg = "Hardware turn off Tx glow failed!\r\n";
                    //    }
                    //    AddTestLog(errmsg);
                    //    return false;
                    //}
                    //Thread.Sleep(300);
                    //if (test.GetTxPower() > -40)
                    //{
                    //    if (GlobalVarFun.Language == "Chinese")
                    //    {
                    //        errmsg = "Tx发射无光显示-40检查失败！\r\n";
                    //    }
                    //    else
                    //    {
                    //        errmsg = "Tx transmit optical display -40 Check failed!\r\n";
                    //    }
                    //    AddTestLog(errmsg);
                    //    i2c.setModuleDis(false); //失败后，硬件开启发射
                    //    return false;
                    //}
                    //if (test.SetModuleDis(false) == false) // Tx Enable 硬件开启发射
                    //{
                    //    if (GlobalVarFun.Language == "Chinese")
                    //    {
                    //        errmsg = "硬件开启Tx发光操作失败01！\r\n";
                    //    }
                    //    else
                    //    {
                    //        errmsg = "Hardware to enable Tx optical operation failed 01!\r\n";
                    //    }
                    //    AddTestLog(errmsg);
                    //    return false;
                    //}
                    //Thread.Sleep(300);
                    ////
                    //if (test.GetTxBias() < 2) //bias<2mA
                    //{
                    //    if (test.SetModuleDis(false) == false) // Tx Enable 硬件开启发射  异常后再次开启
                    //    {
                    //        if (GlobalVarFun.Language == "Chinese")
                    //        {
                    //            errmsg = "硬件开启Tx发光操作失败02！\r\n";
                    //        }
                    //        else
                    //        {
                    //            errmsg = "Hardware to enable Tx optical operation failed 02!\r\n";
                    //        }
                    //        AddTestLog(errmsg);
                    //        return false;
                    //    }
                    //    Thread.Sleep(100);
                    //    if (test.GetTxBias() < 2) //bias<2mA
                    //    {
                    //        if (GlobalVarFun.Language == "Chinese")
                    //        {
                    //            errmsg = "硬件开启Tx发光操作失败03！\r\n";
                    //        }
                    //        else
                    //        {
                    //            errmsg = "Hardware to enable Tx optical operation failed 03!\r\n";
                    //        }
                    //        AddTestLog(errmsg);
                    //        return false;
                    //    }
                    //}
                    //Thread.Sleep(100); //延时100ms               
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////

                // 读取DDM
                Converted_analog_values();

                // 选择用光功率计读取光功率
                if (GlobalVarFun.setup.tx_use_dca_txpwr == false)
                {
                    Thread.Sleep(30);
                    switch (Dut)
                    {
                        case 1:
                            meter_err = TestSet.meter_pwr_err;
                            break;
                        case 2:
                            meter_err = TestSet2.meter_pwr_err;
                            break;
                        case 3:
                            meter_err = TestSet3.meter_pwr_err;
                            break;
                        case 4:
                            meter_err = TestSet4.meter_pwr_err;
                            break;
                    }
                    tx_pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    if (tx_pwr < -30) //光太小
                    {
                        Thread.Sleep(200);
                        tx_pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    }
                    if (tx_pwr < -30) //光太小
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg = "Meter CH: " + meter_ch.ToString() + " tx_pwr: " + tx_pwr.ToString() + " 光功率计读取到的Tx发光太小！\r\n";
                        }
                        else
                        {
                            errmsg = "Meter CH: " + meter_ch.ToString() + " tx_pwr: " + tx_pwr.ToString() + "The Tx glow read by the optical power meter is too small!\r\n";
                        }
                        AddTestLog(errmsg);
                        return false;
                    }
                    AddTestLog("Meter CH: " + meter_ch.ToString() + " tx_pwr: " + tx_pwr.ToString() + " meter_err:" + meter_err.ToString());
                }

                //2023.3.1修改
                if (GlobalVarFun.setup.dca_connect == true)
                {
                    // 读取发射参数
                    if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
                    {
                        if (Get_86100D_TxEyeData_DCA(autoScale) == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                errmsg = "眼图仪86100D读取Tx参数失败！\r\n";
                            }
                            else
                            {
                                errmsg = "Oscilloscope failed to read Tx parameters!\r\n";
                            }
                            AddTestLog(errmsg);
                            return false;
                        }
                    }
                    else
                    {
                        if (Get_TxEyeData_DCA(autoScale) == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                errmsg = "眼图仪读取Tx参数失败！\r\n";
                            }
                            else
                            {
                                errmsg = "Oscilloscope failed to read Tx parameters!\r\n";
                            }
                            AddTestLog(errmsg);
                            return false;
                        }
                    }
                    switch (Dut)
                    {
                        case 1:
                            tx_er = TestResult.txEr;

                            // 选择用DCA眼图仪 读取光功率
                            if (GlobalVarFun.setup.tx_use_dca_txpwr == true)
                            {
                                tx_pwr = TestResult.txPowerDCA + (float)(GlobalVarFun.setup.dac_txpwr_err); // 加偏差
                            }
                            break;
                        case 2:
                            tx_er = TestResult2.txEr;

                            // 选择用DCA眼图仪 读取光功率
                            if (GlobalVarFun.setup.tx_use_dca_txpwr == true)
                            {
                                tx_pwr = TestResult2.txPowerDCA + (float)(GlobalVarFun.setup.dac_txpwr_err); // 加偏差
                            }
                            break;
                        default:
                            break;
                    }
                }
                //
                switch (Dut)
                {
                    case 1:
                        TestResult.txPower = tx_pwr;
                        TestResult.txPwrErr = TestResult.txPowerDDM - TestResult.txPower;
                        retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                        //
                        TestResult.txEr = tx_er;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + tx_er.ToString("F1");
                        //
                        bias = TestResult.txBiasDDM;
                        retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                        //
                        tx_cr = TestResult.txCrossing;
                        retutntxrxresult.TxCrResultShow = "---" + "/" + tx_cr.ToString("F1");
                        //
                        tx_jt = TestResult.txJiterTT;
                        retutntxrxresult.TxJtResultShow = "---" + "/" + tx_jt.ToString("F1");
                        break;
                    case 2:
                        TestResult2.txPower = tx_pwr;
                        TestResult2.txPwrErr = TestResult2.txPowerDDM - TestResult2.txPower;
                        retutntxrxresult.TxBiasResultShow = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                        //
                        TestResult2.txEr = tx_er;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + tx_er.ToString("F1");
                        //
                        bias = TestResult2.txBiasDDM;
                        retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                        //
                        tx_cr = TestResult2.txCrossing;
                        retutntxrxresult.TxCrResultShow = "---" + "/" + tx_cr.ToString("F1");
                        //
                        tx_jt = TestResult2.txJiterTT;
                        retutntxrxresult.TxJtResultShow = "---" + "/" + tx_jt.ToString("F1");
                        break;
                    default:
                        break;
                }
                // 检查
                if (GlobalVarFun.Language == "Chinese")
                {
                    switch (Dut)
                    {
                        case 1:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "Err: " + TestResult.txPwrErr.ToString() + "TxPwrddm: " +
                                    TestResult.txPowerDDM.ToString() + "TxPwrReal: " + TestResult.txPower.ToString() + "发光功率监控值与实际发光偏差超出设定范围！\r\n";
                            //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "光功率超过最大值！\r\n" + "TxPwr: " + tx_pwr.ToString() + "\r\n";
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "光功率超过最小值！\r\n" + "TxPwr: " + tx_pwr.ToString() + "\r\n";
                            //
                            if (bias > TestSet.bias_Max) errmsg += "Bias超过最大值！\r\n" + " bias:" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + "\r\n";
                            if (bias < TestSet.bias_Min) errmsg += "Bias超过最小值！\r\n" + " bias:" + bias.ToString() + " bias_Min:" + TestSet.bias_Min.ToString() + "\r\n";
                            break;
                        case 2:
                            if (Math.Abs(TestResult2.txPwrErr) > txPwrMaxErr) errmsg += "Err: " + TestResult2.txPwrErr.ToString() + "TxPwrddm: " +
                                    TestResult2.txPowerDDM.ToString() + "TxPwrReal: " + TestResult2.txPower.ToString() + "发光功率监控值与实际发光偏差超出设定范围！\r\n";
                            //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "光功率超过最大值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Max:" + TestSet.txPwr_Max.ToString() + "\r\n";
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "光功率超过最小值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Min:" + TestSet.txPwr_Min.ToString() + "\r\n";
                            //
                            if (bias > TestSet.bias_Max) errmsg += "Bias超过最大值！\r\n" + " bias:" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + "\r\n";
                            if (bias < TestSet.bias_Min) errmsg += "Bias超过最小值！\r\n" + " bias:" + bias.ToString() + " bias_Min:" + TestSet.bias_Min.ToString() + "\r\n";
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    switch (Dut)
                    {
                        case 1:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "optical power monitoring value and actual luminous deviation beyond the set range!\r\n";//发光功率监控值与实际发光偏差超出设定范围！
                                                                                                                                                                                //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "Optical power exceeds maximum!\r\n";//光功率超过最大值！
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "The optical power exceeds the minimum!\r\n";//光功率超过最小值！
                                                                                                                   //
                            if (bias > TestSet.bias_Max) errmsg += "BBias exceeds the maximum!\r\n";//Bias超过最大值！
                            if (bias < TestSet.bias_Min) errmsg += "Bias over the minimum!\r\n";//Bias超过最小值！
                            break;
                        case 2:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "optical power monitoring value and actual luminous deviation beyond the set range!\r\n";//发光功率监控值与实际发光偏差超出设定范围！
                                                                                                                                                                                //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "Optical power exceeds maximum!\r\n";//光功率超过最大值！
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "The optical power exceeds the minimum!\r\n";//光功率超过最小值！
                                                                                                                   //
                            if (bias > TestSet.bias_Max) errmsg += "BBias exceeds the maximum!\r\n";//Bias超过最大值！
                            if (bias < TestSet.bias_Min) errmsg += "Bias over the minimum!\r\n";//Bias超过最小值！
                            break;
                        default:
                            break;
                    }
                }
                //
                //2023.3.1修改
                if (GlobalVarFun.setup.dca_connect == true)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        switch (Dut)
                        {
                            case 1:
                                if (tx_er > TestSet.txEr_Max) errmsg += "消光比超过最大值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString() + "\r\n";
                                if (tx_er < TestSet.txEr_Min) errmsg += "消光比超过最小值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString() + "\r\n";
                                //
                                if (tx_cr > TestSet.txCr_Max) errmsg += "交叉点超过最大值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Max:" + TestSet.txCr_Max.ToString() + "\r\n";
                                if (tx_cr < TestSet.txCr_Min) errmsg += "交叉点超过最小值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Min:" + TestSet.txCr_Min.ToString() + "\r\n";
                                //
                                //Jitter Total 检查功能
                                if (GlobalVarFun.setup.tx_jitter_test)
                                {
                                    if (tx_jt >= TestSet.txJt_Max) errmsg += "抖动Jt超过最大值！\r\n" + "tx_jt:" + tx_jt.ToString() + " txJt_Max:" + TestSet.txJt_Max.ToString() + "\r\n";
                                }
                                break;
                            case 2:
                                if (tx_er > TestSet.txEr_Max) errmsg += "消光比超过最大值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString() + "\r\n";
                                if (tx_er < TestSet.txEr_Min) errmsg += "消光比超过最小值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString() + "\r\n";
                                //
                                if (tx_cr > TestSet.txCr_Max) errmsg += "交叉点超过最大值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Max:" + TestSet.txCr_Max.ToString() + "\r\n";
                                if (tx_cr < TestSet.txCr_Min) errmsg += "交叉点超过最小值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Min:" + TestSet.txCr_Min.ToString() + "\r\n";
                                //
                                //Jitter Total 检查功能
                                if (GlobalVarFun.setup.tx_jitter_test)
                                {
                                    if (tx_jt >= TestSet.txJt_Max) errmsg += "抖动Jt超过最大值！\r\n" + "tx_jt:" + tx_jt.ToString() + " txJt_Max:" + TestSet.txJt_Max.ToString() + "\r\n";
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (Dut)
                        {
                            case 1:
                                if (tx_er > TestSet.txEr_Max) errmsg += "Extinction ratio exceeds maximum!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString();//消光比超过最大值！
                                if (tx_er < TestSet.txEr_Min) errmsg += "Extinction ratio exceeds minimum value!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString();//消光比超过最小值！
                                                                                                                                                                                                 //
                                if (tx_cr > TestSet.txCr_Max) errmsg += "Crossing point exceeds maximum!\r\n";//交叉点超过最大值！
                                if (tx_cr < TestSet.txCr_Min) errmsg += "The crossing point exceeds the minimum!\r\n";//交叉点超过最小值！
                                                                                                                      //
                                                                                                                      //Jitter Total 检查功能
                                if (GlobalVarFun.setup.tx_jitter_test)
                                {
                                    if (tx_jt >= TestSet.txJt_Max) errmsg += "Jitter Jt exceeds the maximum!\r\n";//抖动Jt超过最大值！
                                }
                                break;
                            case 2:
                                if (tx_er > TestSet.txEr_Max) errmsg += "Extinction ratio exceeds maximum!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString();//消光比超过最大值！
                                if (tx_er < TestSet.txEr_Min) errmsg += "Extinction ratio exceeds minimum value!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString();//消光比超过最小值！
                                                                                                                                                                                                 //
                                if (tx_cr > TestSet.txCr_Max) errmsg += "Crossing point exceeds maximum!\r\n";//交叉点超过最大值！
                                if (tx_cr < TestSet.txCr_Min) errmsg += "The crossing point exceeds the minimum!\r\n";//交叉点超过最小值！
                                                                                                                      //
                                                                                                                      //Jitter Total 检查功能
                                if (GlobalVarFun.setup.tx_jitter_test)
                                {
                                    if (tx_jt >= TestSet.txJt_Max) errmsg += "Jitter Jt exceeds the maximum!\r\n";//抖动Jt超过最大值！
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                switchSemaphore.Release();
            }
            //
            //AddTestLog(errmsg);
            //
            if (errmsg != "")
            {
                AddTestLog(errmsg);
                return false;
            }
            //
            return true;
        }
        #endregion

        #region // 发射部分测试检查异步

        private async Task<bool> TxFinalTestCheck_Async(bool autoScale)
        {
            float tx_pwr = 0, tx_er = 0, bias = 0, tx_cr = 0, tx_jt = 0;
            string errmsg = "";
            txPwrMaxErr = GlobalVarFun.setup.txpwr_cal;
            rxPwrMaxErr = GlobalVarFun.setup.rxpwr_cal;
            erValMaxErr = GlobalVarFun.setup.er_cal;
            wLengthMaxErr = GlobalVarFun.setup.wlgth_cal;

            string[] strnew = new string[600];
            string strval = string.Empty;
            string[] strArray;
            char[] charArray = new char[] { ' ' };

            double Supply = 0;

            string txpower = "0";
            float pwr = 0;
            string slotStr = GlobalVarFun.OpmDutToOtpSlot[Dut];
            int opmCh = GlobalVarFun.DutToOpmCh[Dut];
            otp12.SetSlot(slotStr);

            if (GlobalVarFun.testType == "finalTest")
            {
                // 选择用光功率计读取光功率
                if (GlobalVarFun.setup.tx_use_dca_txpwr == false)
                {
                    await Task.Delay(30);
                    switch (Dut)
                    {
                        case 1:
                            meter_err = TestSet.meter_pwr_err;
                            break;
                        case 2:
                            meter_err = TestSet2.meter_pwr_err;
                            break;
                        case 3:
                            meter_err = TestSet3.meter_pwr_err;
                            break;
                        case 4:
                            meter_err = TestSet4.meter_pwr_err;
                            break;
                    }
                    txpower = otp12.OPM_ReadPower(opmCh);
                    float.TryParse(txpower, out pwr);
                    tx_pwr = pwr + meter_err;
                    if (tx_pwr < -30) //光太小
                    {
                        await Task.Delay(200);
                        //tx_pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        txpower = otp12.OPM_ReadPower(opmCh);
                        float.TryParse(txpower, out pwr);
                        tx_pwr = pwr + meter_err;
                    }
                    if (tx_pwr < -30) //光太小
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg = "Meter CH: " + meter_ch.ToString() + " tx_pwr: " + tx_pwr.ToString() + " 光功率计读取到的Tx发光太小！\r\n";
                        }
                        else
                        {
                            errmsg = "Meter CH: " + meter_ch.ToString() + " tx_pwr: " + tx_pwr.ToString() + "The Tx glow read by the optical power meter is too small!\r\n";
                        }
                        AddTestLog(errmsg);
                        return false;
                    }
                    AddTestLog("Meter CH: " + meter_ch.ToString() + " tx_pwr: " + tx_pwr.ToString() + " meter_err:" + meter_err.ToString());
                }
                //发射光谱测试
                //lock (tx_lock)
                await switchSemaphore.WaitAsync();
                try
                {
                    //示波器
                    if (GlobalVarFun.setup.otp12_connect == true)
                    {
                        otp12.SetSlot("06");
                        // 设置ERM信号速率（10G模块）
                        otp12.ERM_SetRate(Dut, "10G");
                        // 等待信号稳定
                        await Task.Delay(2000);

                        string erData = otp12.ERM_ReadERData(Dut);
                        if (!string.IsNullOrEmpty(erData))
                        {
                            // 返回格式: "power,er" 例如 "-9.001,12.001"
                            string[] parts = erData.Split(',');
                            if (parts.Length >= 2)
                            {
                                if (float.TryParse(parts[1].Trim(), out tx_er))
                                {
                                    if (tx_er > 0 && tx_er <= 50)
                                    {

                                    }
                                    else
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddTestLog("=============================设备异常=================================");
                    AddTestLog(ex.ToString());
                    AddTestLog("======================================================================");
                    return false;
                }
                finally
                {
                    switchSemaphore.Release();
                }

                //
                switch (Dut)
                {
                    case 1:
                        TestResult.txPower = tx_pwr;
                        TestResult.txPwrErr = TestResult.txPowerDDM - TestResult.txPower;
                        retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                        //
                        TestResult.txEr = tx_er;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + tx_er.ToString("F1");
                        //
                        bias = TestResult.txBiasDDM;
                        retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                        break;
                    case 2:
                        TestResult2.txPower = tx_pwr;
                        TestResult2.txPwrErr = TestResult2.txPowerDDM - TestResult2.txPower;
                        retutntxrxresult.TxBiasResultShow = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                        //
                        TestResult2.txEr = tx_er;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + tx_er.ToString("F1");
                        //
                        bias = TestResult2.txBiasDDM;
                        retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                        break;
                    case 3:
                        TestResult3.txPower = tx_pwr;
                        TestResult3.txPwrErr = TestResult3.txPowerDDM - TestResult3.txPower;
                        retutntxrxresult.TxBiasResultShow = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                        //
                        TestResult3.txEr = tx_er;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + tx_er.ToString("F1");
                        //
                        bias = TestResult3.txBiasDDM;
                        retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                        break;
                    case 4:
                        TestResult4.txPower = tx_pwr;
                        TestResult4.txPwrErr = TestResult4.txPowerDDM - TestResult4.txPower;
                        retutntxrxresult.TxBiasResultShow = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                        //
                        TestResult4.txEr = tx_er;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + tx_er.ToString("F1");
                        //
                        bias = TestResult4.txBiasDDM;
                        retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                        break;
                    default:
                        break;
                }
                // 检查
                if (GlobalVarFun.Language == "Chinese")
                {
                    switch (Dut)
                    {
                        case 1:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "Err: " + TestResult.txPwrErr.ToString() + "TxPwrddm: " +
                                    TestResult.txPowerDDM.ToString() + "TxPwrReal: " + TestResult.txPower.ToString() + "发光功率监控值与实际发光偏差超出设定范围！\r\n";
                            //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "光功率超过最大值！\r\n" + "TxPwr: " + tx_pwr.ToString() + "\r\n";
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "光功率超过最小值！\r\n" + "TxPwr: " + tx_pwr.ToString() + "\r\n";
                            //
                            if (bias > TestSet.bias_Max) errmsg += "Bias超过最大值！\r\n" + " bias:" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + "\r\n";
                            if (bias < TestSet.bias_Min) errmsg += "Bias超过最小值！\r\n" + " bias:" + bias.ToString() + " bias_Min:" + TestSet.bias_Min.ToString() + "\r\n";
                            break;
                        case 2:
                            if (Math.Abs(TestResult2.txPwrErr) > txPwrMaxErr) errmsg += "Err: " + TestResult2.txPwrErr.ToString() + "TxPwrddm: " +
                                    TestResult2.txPowerDDM.ToString() + "TxPwrReal: " + TestResult2.txPower.ToString() + "发光功率监控值与实际发光偏差超出设定范围！\r\n";
                            //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "光功率超过最大值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Max:" + TestSet.txPwr_Max.ToString() + "\r\n";
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "光功率超过最小值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Min:" + TestSet.txPwr_Min.ToString() + "\r\n";
                            //
                            if (bias > TestSet.bias_Max) errmsg += "Bias超过最大值！\r\n" + " bias:" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + "\r\n";
                            if (bias < TestSet.bias_Min) errmsg += "Bias超过最小值！\r\n" + " bias:" + bias.ToString() + " bias_Min:" + TestSet.bias_Min.ToString() + "\r\n";
                            break;
                        case 3:
                            if (Math.Abs(TestResult3.txPwrErr) > txPwrMaxErr) errmsg += "Err: " + TestResult3.txPwrErr.ToString() + "TxPwrddm: " +
                                    TestResult3.txPowerDDM.ToString() + "TxPwrReal: " + TestResult3.txPower.ToString() + "发光功率监控值与实际发光偏差超出设定范围！\r\n";
                            //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "光功率超过最大值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Max:" + TestSet.txPwr_Max.ToString() + "\r\n";
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "光功率超过最小值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Min:" + TestSet.txPwr_Min.ToString() + "\r\n";
                            //
                            if (bias > TestSet.bias_Max) errmsg += "Bias超过最大值！\r\n" + " bias:" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + "\r\n";
                            if (bias < TestSet.bias_Min) errmsg += "Bias超过最小值！\r\n" + " bias:" + bias.ToString() + " bias_Min:" + TestSet.bias_Min.ToString() + "\r\n";
                            break;
                        case 4:
                            if (Math.Abs(TestResult4.txPwrErr) > txPwrMaxErr) errmsg += "Err: " + TestResult4.txPwrErr.ToString() + "TxPwrddm: " +
                                    TestResult4.txPowerDDM.ToString() + "TxPwrReal: " + TestResult4.txPower.ToString() + "发光功率监控值与实际发光偏差超出设定范围！\r\n";
                            //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "光功率超过最大值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Max:" + TestSet.txPwr_Max.ToString() + "\r\n";
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "光功率超过最小值！\r\n" + "TxPwr: " + tx_pwr.ToString() + " txPwr_Min:" + TestSet.txPwr_Min.ToString() + "\r\n";
                            //
                            if (bias > TestSet.bias_Max) errmsg += "Bias超过最大值！\r\n" + " bias:" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + "\r\n";
                            if (bias < TestSet.bias_Min) errmsg += "Bias超过最小值！\r\n" + " bias:" + bias.ToString() + " bias_Min:" + TestSet.bias_Min.ToString() + "\r\n";
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    switch (Dut)
                    {
                        case 1:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "optical power monitoring value and actual luminous deviation beyond the set range!\r\n";//发光功率监控值与实际发光偏差超出设定范围！
                                                                                                                                                                                //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "Optical power exceeds maximum!\r\n";//光功率超过最大值！
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "The optical power exceeds the minimum!\r\n";//光功率超过最小值！
                                                                                                                   //
                            if (bias > TestSet.bias_Max) errmsg += "BBias exceeds the maximum!\r\n";//Bias超过最大值！
                            if (bias < TestSet.bias_Min) errmsg += "Bias over the minimum!\r\n";//Bias超过最小值！
                            break;
                        case 2:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "optical power monitoring value and actual luminous deviation beyond the set range!\r\n";//发光功率监控值与实际发光偏差超出设定范围！
                                                                                                                                                                                //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "Optical power exceeds maximum!\r\n";//光功率超过最大值！
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "The optical power exceeds the minimum!\r\n";//光功率超过最小值！
                                                                                                                   //
                            if (bias > TestSet.bias_Max) errmsg += "BBias exceeds the maximum!\r\n";//Bias超过最大值！
                            if (bias < TestSet.bias_Min) errmsg += "Bias over the minimum!\r\n";//Bias超过最小值！
                            break;
                        case 3:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "optical power monitoring value and actual luminous deviation beyond the set range!\r\n";//发光功率监控值与实际发光偏差超出设定范围！
                                                                                                                                                                                //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "Optical power exceeds maximum!\r\n";//光功率超过最大值！
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "The optical power exceeds the minimum!\r\n";//光功率超过最小值！
                                                                                                                   //
                            if (bias > TestSet.bias_Max) errmsg += "BBias exceeds the maximum!\r\n";//Bias超过最大值！
                            if (bias < TestSet.bias_Min) errmsg += "Bias over the minimum!\r\n";//Bias超过最小值！
                            break;
                        case 4:
                            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr) errmsg += "optical power monitoring value and actual luminous deviation beyond the set range!\r\n";//发光功率监控值与实际发光偏差超出设定范围！
                                                                                                                                                                                //
                            if (tx_pwr > TestSet.txPwr_Max) errmsg += "Optical power exceeds maximum!\r\n";//光功率超过最大值！
                            if (tx_pwr < TestSet.txPwr_Min) errmsg += "The optical power exceeds the minimum!\r\n";//光功率超过最小值！
                                                                                                                   //
                            if (bias > TestSet.bias_Max) errmsg += "BBias exceeds the maximum!\r\n";//Bias超过最大值！
                            if (bias < TestSet.bias_Min) errmsg += "Bias over the minimum!\r\n";//Bias超过最小值！
                            break;
                        default:
                            break;
                    }
                }

                if (GlobalVarFun.Language == "Chinese")
                {
                    switch (Dut)
                    {
                        case 1:
                            if (tx_er > TestSet.txEr_Max) errmsg += "消光比超过最大值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString() + "\r\n";
                            if (tx_er < TestSet.txEr_Min) errmsg += "消光比超过最小值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString() + "\r\n";
                            //
                            //if (tx_cr > TestSet.txCr_Max) errmsg += "交叉点超过最大值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Max:" + TestSet.txCr_Max.ToString() + "\r\n";
                            //if (tx_cr < TestSet.txCr_Min) errmsg += "交叉点超过最小值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Min:" + TestSet.txCr_Min.ToString() + "\r\n";
                            ////
                            ////Jitter Total 检查功能
                            //if (GlobalVarFun.setup.tx_jitter_test)
                            //{
                            //    if (tx_jt >= TestSet.txJt_Max) errmsg += "抖动Jt超过最大值！\r\n" + "tx_jt:" + tx_jt.ToString() + " txJt_Max:" + TestSet.txJt_Max.ToString() + "\r\n";
                            //}
                            break;
                        case 2:
                            if (tx_er > TestSet.txEr_Max) errmsg += "消光比超过最大值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString() + "\r\n";
                            if (tx_er < TestSet.txEr_Min) errmsg += "消光比超过最小值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString() + "\r\n";
                            //
                            //if (tx_cr > TestSet.txCr_Max) errmsg += "交叉点超过最大值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Max:" + TestSet.txCr_Max.ToString() + "\r\n";
                            //if (tx_cr < TestSet.txCr_Min) errmsg += "交叉点超过最小值！\r\n" + "tx_cr:" + tx_cr.ToString() + " txCr_Min:" + TestSet.txCr_Min.ToString() + "\r\n";
                            ////
                            ////Jitter Total 检查功能
                            //if (GlobalVarFun.setup.tx_jitter_test)
                            //{
                            //    if (tx_jt >= TestSet.txJt_Max) errmsg += "抖动Jt超过最大值！\r\n" + "tx_jt:" + tx_jt.ToString() + " txJt_Max:" + TestSet.txJt_Max.ToString() + "\r\n";
                            //}
                            break;
                        case 3:
                            if (tx_er > TestSet.txEr_Max) errmsg += "消光比超过最大值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString() + "\r\n";
                            if (tx_er < TestSet.txEr_Min) errmsg += "消光比超过最小值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString() + "\r\n";
                            break;
                        case 4:
                            if (tx_er > TestSet.txEr_Max) errmsg += "消光比超过最大值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString() + "\r\n";
                            if (tx_er < TestSet.txEr_Min) errmsg += "消光比超过最小值！\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString() + "\r\n";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (Dut)
                    {
                        case 1:
                            if (tx_er > TestSet.txEr_Max) errmsg += "Extinction ratio exceeds maximum!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString();//消光比超过最大值！
                            if (tx_er < TestSet.txEr_Min) errmsg += "Extinction ratio exceeds minimum value!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString();//消光比超过最小值！
                                                                                                                                                                                             //
                                                                                                                                                                                             //if (tx_cr > TestSet.txCr_Max) errmsg += "Crossing point exceeds maximum!\r\n";//交叉点超过最大值！
                                                                                                                                                                                             //if (tx_cr < TestSet.txCr_Min) errmsg += "The crossing point exceeds the minimum!\r\n";//交叉点超过最小值！
                                                                                                                                                                                             //                                                                                      //
                                                                                                                                                                                             //                                                                                      //Jitter Total 检查功能
                                                                                                                                                                                             //if (GlobalVarFun.setup.tx_jitter_test)
                                                                                                                                                                                             //{
                                                                                                                                                                                             //    if (tx_jt >= TestSet.txJt_Max) errmsg += "Jitter Jt exceeds the maximum!\r\n";//抖动Jt超过最大值！
                                                                                                                                                                                             //}
                            break;
                        case 2:
                            if (tx_er > TestSet.txEr_Max) errmsg += "Extinction ratio exceeds maximum!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString();//消光比超过最大值！
                            if (tx_er < TestSet.txEr_Min) errmsg += "Extinction ratio exceeds minimum value!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString();//消光比超过最小值！
                                                                                                                                                                                             //
                                                                                                                                                                                             //if (tx_cr > TestSet.txCr_Max) errmsg += "Crossing point exceeds maximum!\r\n";//交叉点超过最大值！
                                                                                                                                                                                             //if (tx_cr < TestSet.txCr_Min) errmsg += "The crossing point exceeds the minimum!\r\n";//交叉点超过最小值！
                                                                                                                                                                                             //                                                                                      //
                                                                                                                                                                                             //                                                                                      //Jitter Total 检查功能
                                                                                                                                                                                             //if (GlobalVarFun.setup.tx_jitter_test)
                                                                                                                                                                                             //{
                                                                                                                                                                                             //    if (tx_jt >= TestSet.txJt_Max) errmsg += "Jitter Jt exceeds the maximum!\r\n";//抖动Jt超过最大值！
                                                                                                                                                                                             //}
                            break;
                        case 3:
                            if (tx_er > TestSet.txEr_Max) errmsg += "Extinction ratio exceeds maximum!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString();//消光比超过最大值！
                            if (tx_er < TestSet.txEr_Min) errmsg += "Extinction ratio exceeds minimum value!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString();//消光比超过最小值！
                                                                                                                                                                                             //
                                                                                                                                                                                             //if (tx_cr > TestSet.txCr_Max) errmsg += "Crossing point exceeds maximum!\r\n";//交叉点超过最大值！
                                                                                                                                                                                             //if (tx_cr < TestSet.txCr_Min) errmsg += "The crossing point exceeds the minimum!\r\n";//交叉点超过最小值！
                                                                                                                                                                                             //                                                                                      //
                                                                                                                                                                                             //                                                                                      //Jitter Total 检查功能
                                                                                                                                                                                             //if (GlobalVarFun.setup.tx_jitter_test)
                                                                                                                                                                                             //{
                                                                                                                                                                                             //    if (tx_jt >= TestSet.txJt_Max) errmsg += "Jitter Jt exceeds the maximum!\r\n";//抖动Jt超过最大值！
                                                                                                                                                                                             //}
                            break;
                        case 4:
                            if (tx_er > TestSet.txEr_Max) errmsg += "Extinction ratio exceeds maximum!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Max:" + TestSet.txEr_Max.ToString();//消光比超过最大值！
                            if (tx_er < TestSet.txEr_Min) errmsg += "Extinction ratio exceeds minimum value!\r\n" + "tx_er:" + tx_er.ToString() + " txEr_Min:" + TestSet.txEr_Min.ToString();//消光比超过最小值！
                                                                                                                                                                                             //
                                                                                                                                                                                             //if (tx_cr > TestSet.txCr_Max) errmsg += "Crossing point exceeds maximum!\r\n";//交叉点超过最大值！
                                                                                                                                                                                             //if (tx_cr < TestSet.txCr_Min) errmsg += "The crossing point exceeds the minimum!\r\n";//交叉点超过最小值！
                                                                                                                                                                                             //                                                                                      //
                                                                                                                                                                                             //                                                                                      //Jitter Total 检查功能
                                                                                                                                                                                             //if (GlobalVarFun.setup.tx_jitter_test)
                                                                                                                                                                                             //{
                                                                                                                                                                                             //    if (tx_jt >= TestSet.txJt_Max) errmsg += "Jitter Jt exceeds the maximum!\r\n";//抖动Jt超过最大值！
                                                                                                                                                                                             //}
                            break;
                        default:
                            break;
                    }
                }


            }
            //}
            //catch
            //{
            //    return false;
            //}
            //finally
            //{
            //    switchSemaphore.Release();
            //}
            // Tx 发射关闭  无光显示-40检查  //2024.1.11修改优化
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            if (GlobalVarFun.setup.tx_nopwr_test)
            {
                AddTestLog("tx_nopwr_test");
                if (test.SoftTxDis(true) == false) // Tx Disable 软件关闭发射
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg = "软件关闭Tx发光失败！\r\n";
                    }
                    else
                    {
                        errmsg = "Software shut down Tx glow failure!\r\n";
                    }
                    retutntxrxresult.ErrorMessage = errmsg;
                    AddTestLog(errmsg);
                    return false;
                }
                await Task.Delay(300);
                if (test.GetTxPower() > -40)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg = "Tx发射无光显示-40检查失败！\r\n";
                    }
                    else
                    {
                        errmsg = "Tx transmit optical display -40 Check failed!\r\n";
                    }
                    AddTestLog(errmsg);
                    return false;
                }
                if (test.SoftTxDis(false) == false) // Tx Enable 软件开启发射
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg = "软件开启Tx发光操作失败01！\r\n";
                    }
                    else
                    {
                        errmsg = "Software to start Tx optical operation failed 01!\r\n";
                    }
                    AddTestLog(errmsg);
                    return false;
                }
                await Task.Delay(300);
                //
                if (test.GetTxBias() < 2) //bias<2mA
                {
                    if (test.SoftTxDis(false) == false) // Tx Enable 软件开启发射  异常后再次开启
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg = "软件开启Tx发光操作失败02！\r\n";
                        }
                        else
                        {
                            errmsg = "Software failed to turn on Tx  operation 02!\r\n";
                        }
                        AddTestLog(errmsg);
                        return false;
                    }
                    await Task.Delay(100);
                    if (test.GetTxBias() < 2) //bias<2mA
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            errmsg = "软件开启Tx发光操作失败03！\r\n";
                        }
                        else
                        {
                            errmsg = "Software failed to turn on Tx  operation03!\r\n";
                        }
                        AddTestLog(errmsg);
                        return false;
                    }
                }
                await Task.Delay(100); //延时100ms
            }

            if (GlobalVarFun.setup.tx_hardware_disable)
            {
                //if (test.SetModuleDis(true) == false) // Tx Disable 硬件关闭发射
                //{
                //    if (GlobalVarFun.Language == "Chinese")
                //    {
                //        errmsg = "硬件关闭Tx发光失败！\r\n";
                //    }
                //    else
                //    {
                //        errmsg = "Hardware turn off Tx glow failed!\r\n";
                //    }
                //    AddTestLog(errmsg);
                //    return false;
                //}
                //await Task.Delay(300);
                //if (test.GetTxPower() > -40)
                //{
                //    if (GlobalVarFun.Language == "Chinese")
                //    {
                //        errmsg = "Tx发射无光显示-40检查失败！\r\n";
                //    }
                //    else
                //    {
                //        errmsg = "Tx transmit optical display -40 Check failed!\r\n";
                //    }
                //    AddTestLog(errmsg);
                //    i2c.setModuleDis(false); //失败后，硬件开启发射
                //    return false;
                //}
                //if (test.SetModuleDis(false) == false) // Tx Enable 硬件开启发射
                //{
                //    if (GlobalVarFun.Language == "Chinese")
                //    {
                //        errmsg = "硬件开启Tx发光操作失败01！\r\n";
                //    }
                //    else
                //    {
                //        errmsg = "Hardware to enable Tx optical operation failed 01!\r\n";
                //    }
                //    AddTestLog(errmsg);
                //    return false;
                //}
                //await Task.Delay(300);
                ////
                //if (test.GetTxBias() < 2) //bias<2mA
                //{
                //    if (test.SetModuleDis(false) == false) // Tx Enable 硬件开启发射  异常后再次开启
                //    {
                //        if (GlobalVarFun.Language == "Chinese")
                //        {
                //            errmsg = "硬件开启Tx发光操作失败02！\r\n";
                //        }
                //        else
                //        {
                //            errmsg = "Hardware to enable Tx optical operation failed 02!\r\n";
                //        }
                //        AddTestLog(errmsg);
                //        return false;
                //    }
                //    await Task.Delay(100);
                //    if (test.GetTxBias() < 2) //bias<2mA
                //    {
                //        if (GlobalVarFun.Language == "Chinese")
                //        {
                //            errmsg = "硬件开启Tx发光操作失败03！\r\n";
                //        }
                //        else
                //        {
                //            errmsg = "Hardware to enable Tx optical operation failed 03!\r\n";
                //        }
                //        AddTestLog(errmsg);
                //        return false;
                //    }
                //}
                //await Task.Delay(100); //延时100ms               
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////

            // 读取DDM
            Converted_analog_values();
            //
            //AddTestLog(errmsg);
            //
            if (errmsg != "")
            {
                AddTestLog(errmsg);
                return false;
            }
            //
            return true;
        }
        #endregion

        #region //从示波器获取消光比
        private bool Get_ERatio_DCA(bool autoScale)
        {
            lock (dca_lock)
            {
                float tx_er = 0;
                try
                {
                    if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
                    {
                        if (autoScale == true)
                        {
                            scope_86100d.SetAutoScale(gpibAddress, 25);
                        }
                        scope_86100d.SetClearDisplay(gpibAddress, 10);
                        scope_86100d.SetRun(gpibAddress);
                        //等待刷新
                        Thread.Sleep(3500);
                        for (int i = 0; i < 5; i++)
                        {
                            tx_er = scope_86100d.GetExtRatio(gpibAddress);
                            if (tx_er != -1)
                            {
                                break;
                            }
                        }

                    }
                    else
                    {
                        scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                        scope.System.IO.WriteString(":CDISPLAY", true);
                        scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                        scope.System.IO.WriteString(":RUN", true);
                        if (autoScale == true)
                        {
                            scope.System.IO.WriteString(":AUToscale", true);
                        }
                        //scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel,CHANNEL1", true);
                        scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel", true);
                        String str = scope.System.IO.ReadString();
                        scope.System.EnableLocalControls();
                        tx_er = Convert.ToSingle(str);
                    }
                    //
                    if ((tx_er > 50) || (tx_er < 0.5)) // 异常 再测一次
                    {
                        Thread.Sleep(100);
                        //
                        if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
                        {
                            scope_86100d.SetClearDisplay(gpibAddress, 10);
                            Thread.Sleep(100);
                            tx_er = scope_86100d.GetExtRatio(gpibAddress);
                        }
                        else
                        {
                            scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel", true);
                            String str = scope.System.IO.ReadString();
                            scope.System.EnableLocalControls();
                            tx_er = Convert.ToSingle(str);
                        }
                    }
                    //
                    tx_er += (float)(GlobalVarFun.setup.dca_er_err); // 加设备偏差值
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txEr = tx_er;
                            break;
                        case 2:
                            TestResult2.txEr = tx_er;
                            break;
                        default:
                            break;
                    }
                    return true;
                }
                catch (Exception exp)
                {
                    AddTestLog("示波器86100读取错误！" + exp.Message);
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txEr = 0;
                            break;
                        case 2:
                            TestResult2.txEr = 0;
                            break;
                        default:
                            break;
                    }
                    return false;
                }
            }
        }

        private async Task<bool> Get_ERatio_DCA_Async(bool autoScale)
        {
            float tx_er = 0;
            await dcaSemaphore.WaitAsync();
            try
            {
                /*// 切换光开关到发射方向（模块Tx → ERM仪器）
                otp12.SW_SetRouteForModule(Dut, true);*/
                // 设置到ERM消光比模块槽位
                otp12.SetSlot("06");
                // 设置ERM信号速率（10G模块）
                otp12.ERM_SetRate(Dut, "1.25G");
                // 等待信号稳定
                await Task.Delay(1000);

                // 多次读取消光比，取有效值
                for (int i = 0; i < 5; i++)
                {
                    string erData = otp12.ERM_ReadERData(Dut);
                    if (!string.IsNullOrEmpty(erData))
                    {
                        // 返回格式: "power,er" 例如 "-9.001,12.001"
                        string[] parts = erData.Split(',');
                        if (parts.Length >= 2)
                        {
                            if (float.TryParse(parts[1].Trim(), out tx_er))
                            {
                                if (tx_er > 0 && tx_er <= 50)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    await Task.Delay(500);
                }

                // 异常值重试一次
                if ((tx_er > 50) || (tx_er < 0.5))
                {
                    await Task.Delay(500);
                    string erData = otp12.ERM_ReadERData(Dut);
                    if (!string.IsNullOrEmpty(erData))
                    {
                        string[] parts = erData.Split(',');
                        if (parts.Length >= 2)
                        {
                            float.TryParse(parts[1].Trim(), out tx_er);
                        }
                    }
                }

                // 加设备偏差值
                tx_er += (float)(GlobalVarFun.setup.dca_er_err);
                switch (Dut)
                {
                    case 1:
                        TestResult.txEr = tx_er;
                        break;
                    case 2:
                        TestResult2.txEr = tx_er;
                        break;
                    case 3:
                        TestResult3.txEr = tx_er;
                        break;
                    case 4:
                        TestResult4.txEr = tx_er;
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception exp)
            {
                AddTestLog("ERM消光比读取错误！" + exp.Message);
                switch (Dut)
                {
                    case 1:
                        TestResult.txEr = 0;
                        break;
                    case 2:
                        TestResult2.txEr = 0;
                        break;
                    case 3:
                        TestResult3.txEr = 0;
                        break;
                    case 4:
                        TestResult4.txEr = 0;
                        break;
                    default:
                        break;
                }
                return false;
            }
            finally
            {
                dcaSemaphore.Release();
            }
            //}
        }
        #endregion 

        #region  //86100D获取眼图参数
        private bool Get_86100D_TxEyeData_DCA(bool autoScale)
        {
            lock (dca_lock)
            {
                int intWaveForms = 0;
                int intWaveForms_old = -1;
                int intMaxWaveForms = 100;

                float tx_er = 0;
                //string str = null;

                try
                {
                    scope_86100d.SetClearDisplay(gpibAddress, 10);
                    scope_86100d.SetRun(gpibAddress);

                    if (autoScale == true)
                    {
                        //scope.System.IO.WriteString(":AUToscale", true);
                        scope_86100d.SetAutoScale(gpibAddress, 25);
                    }
                    //int delay = (int)DelaynumericUpDown10.Value;
                    //Thread.Sleep(delay+100);
                    //等待刷新
                    Thread.Sleep(3500);

                    // 终测  新增界面参数显示
                    if (GlobalVarFun.testType == "finalTest")
                    {
                        //scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO DECibel", true);
                        //scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER PP", true);
                        //scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER RMS", true);
                        ////scope.System.IO.WriteString(":MEASURE:CGRADE:ESN", true);
                        //scope.System.IO.WriteString(":MEASURE:CGRADE:CROSsing", true);              
                    }
                    //初测
                    switch (Dut)
                    {
                        case 1:
                            if ((GlobalVarFun.testType == "firstTest"))
                            {
                                TestResult.txPowerDCA = scope_86100d.GetPower(gpibAddress);
                                // ER                 
                                tx_er = scope_86100d.GetExtRatio(gpibAddress);
                                tx_er += (float)(GlobalVarFun.setup.dca_er_err); // 加偏差
                                TestResult.txEr = tx_er;
                                // Crossing               
                                TestResult.txCrossing = scope_86100d.GetCrossing(gpibAddress);
                                // Jitter RMS                   
                                TestResult.txJiterRMS = (float)(scope_86100d.GetJitterRMS(gpibAddress));// 单位 ps
                                                                                                        // Jitter PP
                                TestResult.txJiterPP = (float)(scope_86100d.GetJitterPP(gpibAddress));// 单位 ps
                                                                                                      //Jitter Total //2019.2.15 add
                                if (GlobalVarFun.moduleType == "SFP+" || GlobalVarFun.moduleType == "XFP")
                                {
                                    TestResult.txJiterTT = TestResult.txJiterRMS + TestResult.txJiterPP;
                                }
                                else
                                {
                                    TestResult.txJiterTT = (TestResult.txJiterRMS * 14) + TestResult.txJiterPP;
                                }
                                //RiseTime //2024.12.07                  
                                TestResult.TxRiseTime = (float)(scope_86100d.GetRiseTime(gpibAddress) * Math.Pow(10, 12));
                                //FallTime //2024.12.07                  
                                TestResult.TxFallTime = (float)(scope_86100d.GetFallTime(gpibAddress) * Math.Pow(10, 12));
                                // ESN             
                                TestResult.txESN = scope_86100d.GetEyeSNR(gpibAddress);
                                //Eye Amp
                                TestResult.TxEyeAmp = (float)(scope_86100d.GetAmplitude(gpibAddress)) * 1000000;//uw 2025.09.12
                            }
                            break;
                        case 2:
                            if ((GlobalVarFun.testType == "firstTest"))
                            {
                                TestResult2.txPowerDCA = scope_86100d.GetPower(gpibAddress);
                                // ER                 
                                tx_er = scope_86100d.GetExtRatio(gpibAddress);
                                tx_er += (float)(GlobalVarFun.setup.dca_er_err); // 加偏差
                                TestResult2.txEr = tx_er;
                                // Crossing               
                                TestResult2.txCrossing = scope_86100d.GetCrossing(gpibAddress);
                                // Jitter RMS                   
                                TestResult2.txJiterRMS = (float)(scope_86100d.GetJitterRMS(gpibAddress));// 单位 ps
                                                                                                         // Jitter PP
                                TestResult2.txJiterPP = (float)(scope_86100d.GetJitterPP(gpibAddress));// 单位 ps
                                                                                                       //Jitter Total //2019.2.15 add
                                if (GlobalVarFun.moduleType == "SFP+" || GlobalVarFun.moduleType == "XFP")
                                {
                                    TestResult2.txJiterTT = TestResult2.txJiterRMS + TestResult2.txJiterPP;
                                }
                                else
                                {
                                    TestResult2.txJiterTT = (TestResult2.txJiterRMS * 14) + TestResult2.txJiterPP;
                                }
                                //RiseTime //2024.12.07                  
                                TestResult2.TxRiseTime = (float)(scope_86100d.GetRiseTime(gpibAddress) * Math.Pow(10, 12));
                                //FallTime //2024.12.07                  
                                TestResult2.TxFallTime = (float)(scope_86100d.GetFallTime(gpibAddress) * Math.Pow(10, 12));
                                // ESN             
                                TestResult2.txESN = scope_86100d.GetEyeSNR(gpibAddress);
                                //Eye Amp
                                TestResult2.TxEyeAmp = (float)(scope_86100d.GetAmplitude(gpibAddress)) * 1000000;//uw 2025.09.12
                            }
                            break;
                        default:
                            break;
                    }
                    // 终测  模板测试
                    switch (Dut)
                    {
                        case 1:
                            if ((GlobalVarFun.testType == "finalTest"))
                            {

                                TestResult.txPowerDCA = scope_86100d.GetPower(gpibAddress);
                                // ER                 
                                tx_er = scope_86100d.GetExtRatio(gpibAddress);
                                tx_er += (float)(GlobalVarFun.setup.er_cal); // 加偏差
                                TestResult.txEr = tx_er;
                                // Crossing               
                                TestResult.txCrossing = scope_86100d.GetCrossing(gpibAddress);
                                // Jitter RMS                   
                                TestResult.txJiterRMS = (float)(scope_86100d.GetJitterRMS(gpibAddress));// 单位 ps
                                                                                                        // Jitter PP
                                TestResult.txJiterPP = (float)(scope_86100d.GetJitterPP(gpibAddress));// 单位 ps
                                                                                                      //Jitter Total //2019.2.15 add
                                if (GlobalVarFun.moduleType == "SFP+" || GlobalVarFun.moduleType == "XFP")
                                {
                                    TestResult.txJiterTT = TestResult.txJiterRMS + TestResult.txJiterPP;
                                }
                                else
                                {
                                    TestResult.txJiterTT = (TestResult.txJiterRMS * 14) + TestResult.txJiterPP;
                                }
                                //RiseTime //2024.12.07                  
                                TestResult.TxRiseTime = (float)(scope_86100d.GetRiseTime(gpibAddress) * Math.Pow(10, 12));
                                //FallTime //2024.12.07                  
                                TestResult.TxFallTime = (float)(scope_86100d.GetFallTime(gpibAddress) * Math.Pow(10, 12));
                                // ESN             
                                TestResult.txESN = scope_86100d.GetEyeSNR(gpibAddress);
                                //Eye Amp
                                TestResult.TxEyeAmp = (float)(scope_86100d.GetAmplitude(gpibAddress)) * 1000000;//uw 2025.09.12
                                                                                                                // 终测  从示波器86100 截取眼图gif
                                TestResult.bimage_len = 0;
                                if ((GlobalVarFun.testType == "finalTest") && (GlobalVarFun.setup.image_save))// && (TestResult.waveforms_count >= 100))
                                {
                                    //等待波形计数
                                    intMaxWaveForms = TestResult.waveforms_count;
                                    scope_86100d.SetLimitTestOFF(gpibAddress);
                                    if (intMaxWaveForms >= 100)
                                    {
                                        scope_86100d.SetWaveforms(gpibAddress, intMaxWaveForms);
                                    }

                                    while (intWaveForms < intMaxWaveForms)
                                    {
                                        Thread.Sleep(1200);
                                        intWaveForms = scope_86100d.GetWaveforms(gpibAddress);
                                        //
                                        if (intWaveForms_old < 50)
                                        {
                                            //scope.System.IO.WriteString(":MTESt:START", true);
                                        }
                                        //
                                        if (intWaveForms <= intWaveForms_old) //读取的参数异常
                                        {
                                            if (GlobalVarFun.Language == "Chinese")
                                            {
                                                AddTestLog("示波器86100眼图累计点读取错误！");
                                            }
                                            else
                                            {
                                                AddTestLog("Oscilloscope 86100 eye map accumulation point reading error!");
                                            }
                                            return false;
                                        }
                                        //
                                        intWaveForms_old = intWaveForms;
                                    }

                                    if (File.Exists(@"D:\User Files\Screen Images\Screen.gif")) //检查是否存在,如已存在,先删除.
                                    {
                                        File.Delete(@"D:\User Files\Screen Images\Screen.gif");
                                    }
                                    scope_86100d.SaveImage(gpibAddress, @"D:\User Files\Screen Images\Screen.gif");
                                    //将gif转换为byte数组
                                    byte[] byteArray = null;
                                    byteArray = File.ReadAllBytes(@"D:\User Files\Screen Images\Screen.gif");
                                    TestResult.bimage_len = byteArray.Length; //有效长度

                                    if ((TestResult.bimage_len < 1000) || (TestResult.bimage_len > 1000000)) // 1k-100k Bytes
                                    {
                                        if (GlobalVarFun.Language == "Chinese")
                                        {
                                            AddTestLog("GIF眼图 长度错误！");
                                        }
                                        else
                                        {
                                            AddTestLog("GIF eye image length error!");//GIF眼图 长度错误！
                                        }
                                        return false;
                                    }

                                    TestResult.txEye_image = new byte[TestResult.bimage_len]; // 重新定义Byte[]数组大小

                                    for (int i = 0; i < TestResult.bimage_len; i++)
                                    {
                                        TestResult.txEye_image[i] = byteArray[i];
                                    }
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());
                                    }
                                    else
                                    {
                                        AddTestLog("GIF eye view bytes=" + TestResult.bimage_len.ToString());//GIF眼图bytes
                                    }

                                }
                            }
                            break;
                        case 2:
                            if ((GlobalVarFun.testType == "finalTest"))
                            {

                                TestResult2.txPowerDCA = scope_86100d.GetPower(gpibAddress);
                                // ER                 
                                tx_er = scope_86100d.GetExtRatio(gpibAddress);
                                tx_er += (float)(GlobalVarFun.setup.er_cal); // 加偏差
                                TestResult2.txEr = tx_er;
                                // Crossing               
                                TestResult2.txCrossing = scope_86100d.GetCrossing(gpibAddress);
                                // Jitter RMS                   
                                TestResult2.txJiterRMS = (float)(scope_86100d.GetJitterRMS(gpibAddress));// 单位 ps
                                                                                                         // Jitter PP
                                TestResult2.txJiterPP = (float)(scope_86100d.GetJitterPP(gpibAddress));// 单位 ps
                                                                                                       //Jitter Total //2019.2.15 add
                                if (GlobalVarFun.moduleType == "SFP+" || GlobalVarFun.moduleType == "XFP")
                                {
                                    TestResult2.txJiterTT = TestResult.txJiterRMS + TestResult.txJiterPP;
                                }
                                else
                                {
                                    TestResult2.txJiterTT = (TestResult2.txJiterRMS * 14) + TestResult2.txJiterPP;
                                }
                                //RiseTime //2024.12.07                  
                                TestResult2.TxRiseTime = (float)(scope_86100d.GetRiseTime(gpibAddress) * Math.Pow(10, 12));
                                //FallTime //2024.12.07                  
                                TestResult2.TxFallTime = (float)(scope_86100d.GetFallTime(gpibAddress) * Math.Pow(10, 12));
                                // ESN             
                                TestResult2.txESN = scope_86100d.GetEyeSNR(gpibAddress);
                                //Eye Amp
                                TestResult2.TxEyeAmp = (float)(scope_86100d.GetAmplitude(gpibAddress)) * 1000000;//uw 2025.09.12
                                                                                                                 // 终测  从示波器86100 截取眼图gif
                                TestResult2.bimage_len = 0;
                                if ((GlobalVarFun.testType == "finalTest") && (GlobalVarFun.setup.image_save))// && (TestResult.waveforms_count >= 100))
                                {
                                    //等待波形计数
                                    intMaxWaveForms = TestResult2.waveforms_count;
                                    scope_86100d.SetLimitTestOFF(gpibAddress);
                                    if (intMaxWaveForms >= 100)
                                    {
                                        scope_86100d.SetWaveforms(gpibAddress, intMaxWaveForms);
                                    }

                                    while (intWaveForms < intMaxWaveForms)
                                    {
                                        Thread.Sleep(1200);
                                        intWaveForms = scope_86100d.GetWaveforms(gpibAddress);
                                        //
                                        if (intWaveForms_old < 50)
                                        {
                                            //scope.System.IO.WriteString(":MTESt:START", true);
                                        }
                                        //
                                        if (intWaveForms <= intWaveForms_old) //读取的参数异常
                                        {
                                            if (GlobalVarFun.Language == "Chinese")
                                            {
                                                AddTestLog("示波器86100眼图累计点读取错误！");
                                            }
                                            else
                                            {
                                                AddTestLog("Oscilloscope 86100 eye map accumulation point reading error!");
                                            }
                                            return false;
                                        }
                                        //
                                        intWaveForms_old = intWaveForms;
                                    }

                                    if (File.Exists(@"D:\User Files\Screen Images\Screen.gif")) //检查是否存在,如已存在,先删除.
                                    {
                                        File.Delete(@"D:\User Files\Screen Images\Screen.gif");
                                    }
                                    scope_86100d.SaveImage(gpibAddress, @"D:\User Files\Screen Images\Screen.gif");
                                    //将gif转换为byte数组
                                    byte[] byteArray = null;
                                    byteArray = File.ReadAllBytes(@"D:\User Files\Screen Images\Screen.gif");
                                    TestResult2.bimage_len = byteArray.Length; //有效长度

                                    if ((TestResult2.bimage_len < 1000) || (TestResult2.bimage_len > 1000000)) // 1k-100k Bytes
                                    {
                                        if (GlobalVarFun.Language == "Chinese")
                                        {
                                            AddTestLog("GIF眼图 长度错误！");
                                        }
                                        else
                                        {
                                            AddTestLog("GIF eye image length error!");//GIF眼图 长度错误！
                                        }
                                        return false;
                                    }

                                    TestResult2.txEye_image = new byte[TestResult.bimage_len]; // 重新定义Byte[]数组大小

                                    for (int i = 0; i < TestResult.bimage_len; i++)
                                    {
                                        TestResult2.txEye_image[i] = byteArray[i];
                                    }
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());
                                    }
                                    else
                                    {
                                        AddTestLog("GIF eye view bytes=" + TestResult2.bimage_len.ToString());//GIF眼图bytes
                                    }

                                }
                            }
                            break;
                        default:
                            break;
                    }
                    //
                    //scope.System.EnableLocalControls();
                    return true;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(exp.Message);
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("从示波器86100D读取ER/CP/Jitter等参数错误！" + ex.ToString());
                    }
                    else
                    {
                        AddTestLog("Error reading ER/CP/Jitter from the oscilloscope 86100D!" + ex.ToString());
                    }
                    return false;
                }
            }
        }
        #endregion

        #region  //从 86100 A/B/C 示波器获取消光比等参数

        private bool Get_TxEyeData_DCA(bool autoScale)
        {
            lock (dca_lock)
            {
                int intWaveForms = 0;
                int intWaveForms_old = -1;
                int intMaxWaveForms = 100;

                float tx_er = 0;
                string str = null;

                try
                {
                    scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    scope.System.IO.WriteString(":CDISPLAY", true);
                    scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    scope.System.IO.WriteString(":MEASURE:SEND OFF", true);
                    scope.System.IO.WriteString("*CLS", true);
                    scope.System.IO.WriteString(":RUN", true);
                    if (autoScale == true)
                    {
                        scope.System.IO.WriteString(":AUToscale", true);
                    }

                    // 终测  新增界面参数显示
                    if (GlobalVarFun.testType == "finalTest")
                    {
                        scope.System.IO.WriteString(":MEASURE:CGRADE:RISETIME", true);
                        scope.System.IO.WriteString(":MEASURE:CGRADE:FALLTIME", true);
                        scope.System.IO.WriteString(":MEASURE:CGRADE:ESN", true);
                        scope.System.IO.WriteString(":MEASURE:CGRADE:AMPLITUDE", true);

                        scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO DECibel", true);
                        scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER PP", true);
                        scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER RMS", true);
                        //scope.System.IO.WriteString(":MEASURE:CGRADE:ESN", true);
                        scope.System.IO.WriteString(":MEASURE:CGRADE:CROSsing", true);
                    }
                    // 终测  模板测试
                    int count = 0;
                    switch (Dut)
                    {
                        case 1:
                            count = TestResult.waveforms_count;
                            break;
                        case 2:
                            count = TestResult2.waveforms_count;
                            break;
                        default:
                            count = 0;
                            break;
                    }
                    if ((GlobalVarFun.testType == "finalTest") && (count >= 100) && (TestSet.eyeMaskIsOpened == true))
                    {
                        scope.System.IO.WriteString(":MTESt:TEST ON", true); //开模板mask显示
                        scope.System.IO.WriteString(":MTESt:START", true);
                        //

                        //等待波形计数
                        intMaxWaveForms = count;
                        while (intWaveForms < intMaxWaveForms)
                        {
                            Thread.Sleep(1000);
                            scope.System.IO.WriteString(":MTESt:COUNt:WAVeforms?", true);
                            str = scope.System.IO.ReadString();
                            intWaveForms = Convert.ToInt32(Convert.ToSingle(str));
                            //
                            if (intWaveForms_old < 50)
                            {
                                scope.System.IO.WriteString(":MTESt:START", true);
                            }
                            //
                            if (intWaveForms <= intWaveForms_old) //读取的参数异常
                            {
                                if (GlobalVarFun.Language == "Chinese")
                                {
                                    AddTestLog("示波器86100眼图累计点读取错误！");
                                }
                                else
                                {
                                    AddTestLog("Oscilloscope 86100 eye map accumulation point reading error!");
                                }
                                return false;
                            }
                            //
                            intWaveForms_old = intWaveForms;
                        }

                        //判断是否有fail点落在模板内
                        //scope.System.IO.WriteString(":MTESt:START", true);
                        scope.System.IO.WriteString(":MTESt:COUNt:FSAMples?", true);
                        str = scope.System.IO.ReadString();
                        intWaveForms = Convert.ToInt32(Convert.ToSingle(str));
                        if (intWaveForms > 0)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                AddTestLog("眼图模板测试时出现散点 FAIL!");
                            }
                            else
                            {
                                AddTestLog("Scatters occurred during the eye pattern template test, FAIL!");//眼图模板测试时出现散点
                            }
                            return false;
                        }
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            AddTestLog("眼图模板测试成功=" + intMaxWaveForms.ToString());
                        }
                        else
                        {
                            AddTestLog("The eye map template was successfully tested=" + intMaxWaveForms.ToString());//眼图模板测试成功
                        }
                    }


                    // ER
                    scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel", true);
                    str = scope.System.IO.ReadString();
                    tx_er = Convert.ToSingle(str);
                    tx_er += (float)(GlobalVarFun.setup.er_cal); // 加偏差
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txEr = tx_er;
                            break;
                        case 2:
                            TestResult2.txEr = tx_er;
                            break;
                        default:
                            break;
                    }
                    // Crossing
                    scope.System.IO.WriteString(":MEASURE:CGRADE:CROSsing?", true);
                    str = scope.System.IO.ReadString();
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txCrossing = Convert.ToSingle(str);
                            break;
                        case 2:
                            TestResult2.txCrossing = Convert.ToSingle(str);
                            break;
                        default:
                            break;
                    }
                    // Jitter RMS
                    scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? RMS", true);
                    str = scope.System.IO.ReadString();
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txJiterRMS = (float)(Convert.ToSingle(str) * 1e12); // 单位 ps
                            break;
                        case 2:
                            break;
                        default:
                            TestResult2.txJiterRMS = (float)(Convert.ToSingle(str) * 1e12); // 单位 ps
                            break;
                    }
                    // Jitter PP
                    scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? PP", true);
                    str = scope.System.IO.ReadString();
                    TestResult.txJiterPP = (float)(Convert.ToSingle(str) * 1e12); // 单位 ps

                    //Jitter Total //2019.2.15 add
                    if (GlobalVarFun.moduleType == "SFP+" || GlobalVarFun.moduleType == "XFP")
                    {
                        TestResult.txJiterTT = TestResult.txJiterRMS + TestResult.txJiterPP;
                    }
                    else
                    {
                        TestResult.txJiterTT = (TestResult.txJiterRMS * 14) + TestResult.txJiterPP;
                    }
                    //RiseTime //2024.12.07
                    scope.System.IO.WriteString(":MEASURE:RISETIME?", true);
                    str = scope.System.IO.ReadString();
                    switch (Dut)
                    {
                        case 1:
                            TestResult.TxRiseTime = (float)(Convert.ToSingle(str) * Math.Pow(10, 12)); // 
                            break;
                        case 2:
                            TestResult2.TxRiseTime = (float)(Convert.ToSingle(str) * Math.Pow(10, 12)); // 
                            break;
                        default:
                            break;
                    }                                                                         //FallTime //2024.12.07
                    scope.System.IO.WriteString(":MEASURE:FALLTIME?", true);
                    str = scope.System.IO.ReadString();
                    switch (Dut)
                    {
                        case 1:
                            TestResult.TxFallTime = (float)(Convert.ToSingle(str) * Math.Pow(10, 12)); // 
                            break;
                        case 2:
                            TestResult2.TxFallTime = (float)(Convert.ToSingle(str) * Math.Pow(10, 12)); // 
                            break;
                        default:
                            break;
                    }                                                                         // ESN
                    scope.System.IO.WriteString(":MEASURE:CGRADE:ESN?", true);
                    str = scope.System.IO.ReadString();
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txESN = Convert.ToSingle(str);
                            break;
                        case 2:
                            TestResult2.txESN = Convert.ToSingle(str);
                            break;
                        default:
                            break;
                    }
                    //Eye Amp 
                    str = "";
                    int times = 0;
                    while ((str == "") || times < 5) //2025.12.04
                    {
                        scope.System.IO.WriteString(":MEASURE:CGRADE:AMPLITUDE?", true);
                        str = scope.System.IO.ReadString();
                        times++;
                    }
                    double amp = Convert.ToDouble(str.Trim()) * 1000000;//uw
                    switch (Dut)
                    {
                        case 1:
                            TestResult.TxEyeAmp = Convert.ToSingle(amp.ToString());
                            break;
                        case 2:
                            TestResult2.TxEyeAmp = Convert.ToSingle(amp.ToString());
                            break;
                        default:
                            break;
                    }
                    // opto power             
                    //TestResult.txPowerDCA = Get_OptoPower_DCA();
                    scope.System.IO.WriteString(":MEASURE:APOWER? DECibel", true);
                    str = scope.System.IO.ReadString();
                    //AddTestLog(str);
                    //times = 0;
                    //while ((str.Contains("9.9999")) || times < 5) //2025.12.04
                    //{
                    //    scope.System.IO.WriteString(":MEASURE:APOWER? DECibel", true);
                    //    str = scope.System.IO.ReadString();
                    //    times++;
                    //}
                    if (str.Contains("9.9999"))
                    {
                        scope.System.IO.WriteString(":MEASURE:APOWER? DECibel", true);
                        str = scope.System.IO.ReadString();
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPowerDCA = Convert.ToSingle(str) + GlobalVarFun.setup.dac_txpwr_err;
                            if (str.Contains("9.9999"))
                            {
                                TestResult.txPowerDCA = -100;
                            }
                            TestResult.bimage_len = 0;
                            break;
                        case 2:
                            TestResult2.txPowerDCA = Convert.ToSingle(str) + GlobalVarFun.setup.dac_txpwr_err;
                            if (str.Contains("9.9999"))
                            {
                                TestResult2.txPowerDCA = -100;
                            }
                            TestResult2.bimage_len = 0;
                            break;
                        default:
                            break;
                    }

                    // 终测  从示波器86100 截取眼图gif
                    if ((GlobalVarFun.testType == "finalTest") && (GlobalVarFun.setup.image_save))// && (TestResult.waveforms_count >= 100))
                    {
                        byte[] byteArray = null;
                        //scope.System.GetScreenBitmap(ref byteArray);
                        object obj = new object();
                        scope.System.IO.WriteString(":DISPlay:DATA? GIF,SCReen,NORMal", true);
                        obj = scope.System.IO.ReadIEEEBlock(Ivi.Visa.Interop.IEEEBinaryType.BinaryType_UI1, false, true);

                        //将86100读取的二进制数据流转为Byte[]
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BinaryFormatter binFormatter = new BinaryFormatter();
                            binFormatter.Serialize(ms, obj);
                            byteArray = ms.GetBuffer();
                        }


                        switch (Dut)
                        {
                            case 1:
                                TestResult.bimage_len = (byteArray.Length / 2) - 27; //有效长度

                                if ((TestResult.bimage_len < 1000) || (TestResult.bimage_len > 100000)) // 1k-100k Bytes
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        AddTestLog("GIF眼图 长度错误！");
                                    }
                                    else
                                    {
                                        AddTestLog("GIF eye image length error!");//GIF眼图 长度错误！
                                    }
                                    return false;
                                }

                                TestResult.txEye_image = new byte[TestResult.bimage_len]; // 重新定义Byte[]数组大小

                                for (int i = 0; i < TestResult.bimage_len; i++)
                                {
                                    TestResult.txEye_image[i] = byteArray[i + 27];
                                }

                                if ((TestResult.txEye_image[0] != 0x47) || (TestResult.txEye_image[1] != 0x49) || (TestResult.txEye_image[2] != 0x46) || (TestResult.txEye_image[3] != 0x38) || (TestResult.txEye_image[TestResult.bimage_len - 1] != 0x3B)) // GIF8
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        AddTestLog("GIF眼图 头尾标识错误！");
                                    }
                                    else
                                    {
                                        AddTestLog("GIF eye image header and tail logo error!");
                                    }
                                    return false;
                                }
                                if (GlobalVarFun.Language == "Chinese")
                                {
                                    AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());
                                }
                                else
                                {
                                    AddTestLog("GIF eye view bytes=" + TestResult.bimage_len.ToString());//GIF眼图bytes
                                }
                                break;
                            case 2:
                                TestResult2.bimage_len = (byteArray.Length / 2) - 27; //有效长度

                                if ((TestResult2.bimage_len < 1000) || (TestResult2.bimage_len > 100000)) // 1k-100k Bytes
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        AddTestLog("GIF眼图 长度错误！");
                                    }
                                    else
                                    {
                                        AddTestLog("GIF eye image length error!");//GIF眼图 长度错误！
                                    }
                                    return false;
                                }

                                TestResult2.txEye_image = new byte[TestResult2.bimage_len]; // 重新定义Byte[]数组大小

                                for (int i = 0; i < TestResult.bimage_len; i++)
                                {
                                    TestResult2.txEye_image[i] = byteArray[i + 27];
                                }

                                if ((TestResult2.txEye_image[0] != 0x47) || (TestResult2.txEye_image[1] != 0x49) || (TestResult2.txEye_image[2] != 0x46) || (TestResult2.txEye_image[3] != 0x38) || (TestResult2.txEye_image[TestResult.bimage_len - 1] != 0x3B)) // GIF8
                                {
                                    if (GlobalVarFun.Language == "Chinese")
                                    {
                                        AddTestLog("GIF眼图 头尾标识错误！");
                                    }
                                    else
                                    {
                                        AddTestLog("GIF eye image header and tail logo error!");
                                    }
                                    return false;
                                }
                                if (GlobalVarFun.Language == "Chinese")
                                {
                                    AddTestLog("GIF眼图bytes=" + TestResult2.bimage_len.ToString());
                                }
                                else
                                {
                                    AddTestLog("GIF eye view bytes=" + TestResult2.bimage_len.ToString());//GIF眼图bytes
                                }
                                break;
                            default:
                                break;
                        }
                        //保存眼图 test  存放到本机 C:\
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            string strFilePath = "C:\\1.gif"; //Application.StartupPath + "\\image\\" + "1.gif";

                            if (File.Exists(strFilePath)) //检查1.gif是否存在,如已存在,先删除.
                            {
                                File.Delete(strFilePath);
                            }

                            FileStream fs = new FileStream(strFilePath, FileMode.Append, FileAccess.Write);
                            BinaryWriter bw = new BinaryWriter(fs);
                            switch (Dut)
                            {
                                case 1:
                                    bw.Write(TestResult.txEye_image, 0, TestResult.bimage_len);
                                    break;
                                case 2:
                                    bw.Write(TestResult2.txEye_image, 0, TestResult2.bimage_len);
                                    break;
                                default:
                                    break;
                            }
                            //bw.Write(byteArray, 0, byteArray.Length);
                            bw.Close();
                            fs.Close();
                        }
                    }
                    //
                    scope.System.EnableLocalControls();
                    return true;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(exp.Message);
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("从示波器86100读取ER/CP/Jitter等参数错误！" + ex.ToString());
                    }
                    else
                    {
                        AddTestLog("Error reading ER/CP/Jitter from the oscilloscope 86100!" + ex.ToString());
                    }
                    return false;
                }
            }
        }
        #endregion

        #region //更新 模块 类型/速率/版本/状态  等信息
        private bool ShowCheckModuleStatus2()
        {
            bool rtn_flag = true;
            string str = "";
            if (GlobalVarFun.Language == "Chinese")
            {
                str = " 三合一方案......";
            }
            else
            {
                str = " Three-in-one chip scheme.....";
            }
            if (GlobalVarFun.moduleType == "SFP-GN25L95")
            {
                retutntxrxresult.ModuleSchemeShow = " SFP-GN25L95" + str;
                return true;
            }
            if (GlobalVarFun.moduleType == "SFP-GN25L96")
            {
                retutntxrxresult.ModuleSchemeShow = " SFP-GN25L96" + str;
                return true;
            }
            if (GlobalVarFun.moduleType == "SFP-UX3320C")
            {
                retutntxrxresult.ModuleSchemeShow = " SFP-UX3320C" + str;
                return true;
            }
            if (GlobalVarFun.moduleType == "SFP-UX3320T")
            {
                retutntxrxresult.ModuleSchemeShow = " SFP-UX3320T" + str;
                return true;
            }
            if (GlobalVarFun.moduleType == "SFPP-GN1196")
            {
                retutntxrxresult.ModuleSchemeShow = " SFPP-GN1196" + str;
                return true;
            }
            if (GlobalVarFun.moduleType == "SFPP-UX3261S")
            {
                retutntxrxresult.ModuleSchemeShow = "SFPPUX3261S" + str;
                return true;
            }
            if (GlobalVarFun.moduleType == "SFPP-UX2270+2072")
            {
                retutntxrxresult.ModuleSchemeShow = "SFPP-UX2270+2072" + str;
                return true;
            }
            // MCU方案的模块支持如下判断  SFP-MCU  SFP+  XFP
            //////////////////////////////////////////////////////////////////////////
            //
            // 多模850nm 模块特殊判断
            switch (Dut)
            {
                case 1:
                    if (((TestResult.fibertop_pn).Contains("-MM85") == TestResult.moduleIsSR)
                    || ((TestResult.fibertop_pn).Contains("-MC85") == TestResult.moduleIsSR)
                    || ((TestResult.fibertop_pn).Contains("-MS83") == TestResult.moduleIsSR)
                    || ((TestResult.fibertop_pn).Contains("-AC") == TestResult.moduleIsSR))
                    {
                        retutntxrxresult.ModuleSRShow = false;
                    }
                    else
                    {
                        retutntxrxresult.ModuleSRShow = true;
                        rtn_flag = false;
                    }

                    //
                    retutntxrxresult.ModuleSchemeShow = " " + TestResult.chipType;
                    retutntxrxresult.ModuleSchemeShow += " " + TestResult.bitRate;
                    retutntxrxresult.ModuleSchemeShow += " " + TestResult.softType;
                    retutntxrxresult.ModuleSchemeShow += " " + TestResult.softVer;

                    if (TestResult.wpIsEn == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  写码密码:使能";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Coding PWD:Enabled";
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  写码密码:关闭";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Coding PWD:Disenabled";
                        }
                    }

                    if (TestResult.chipIsOK == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  芯片状态:正常";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Chip status: Normal";
                        }
                        retutntxrxresult.ModuleChipisOK = false;
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  芯片状态:异常";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Chip status: Abnormal";
                        }
                        retutntxrxresult.ModuleChipisOK = true;
                        rtn_flag = false;
                    }
                    break;
                case 2:
                    if (((TestResult2.fibertop_pn).Contains("-MM85") == TestResult2.moduleIsSR)
                    || ((TestResult2.fibertop_pn).Contains("-MC85") == TestResult2.moduleIsSR)
                    || ((TestResult2.fibertop_pn).Contains("-MS83") == TestResult2.moduleIsSR)
                    || ((TestResult2.fibertop_pn).Contains("-AC") == TestResult2.moduleIsSR))
                    {
                        retutntxrxresult.ModuleSRShow = false;
                    }
                    else
                    {
                        retutntxrxresult.ModuleSRShow = true;
                        rtn_flag = false;
                    }

                    //
                    retutntxrxresult.ModuleSchemeShow = " " + TestResult2.chipType;
                    retutntxrxresult.ModuleSchemeShow += " " + TestResult2.bitRate;
                    retutntxrxresult.ModuleSchemeShow += " " + TestResult2.softType;
                    retutntxrxresult.ModuleSchemeShow += " " + TestResult2.softVer;

                    if (TestResult2.wpIsEn == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  写码密码:使能";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Coding PWD:Enabled";
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  写码密码:关闭";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Coding PWD:Disenabled";
                        }
                    }

                    if (TestResult2.chipIsOK == true)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  芯片状态:正常";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Chip status: Normal";
                        }
                        retutntxrxresult.ModuleChipisOK = false;
                    }
                    else
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            retutntxrxresult.ModuleSchemeShow += "  芯片状态:异常";
                        }
                        else
                        {
                            retutntxrxresult.ModuleSchemeShow += "  Chip status: Abnormal";
                        }
                        retutntxrxresult.ModuleChipisOK = true;
                        rtn_flag = false;
                    }
                    break;
                default:
                    break;
            }
            //////////////////////////////////////////////////////////////////////////

            return rtn_flag;
        }
        #endregion

        #region //AutoTestRxAPD
        private bool AutoTestRxAPD()
        {
            double[] psspertbuf = new double[256];
            double valmin = psspertbuf[0];
            byte valminindex = 0;
            string ch = "0";
            string pssChannel = GlobalVarFun.setup.bert_ch_a.ToString();
            string status = "";
            int i = 0;
            string str = "";
            int index = 0;
            byte min = (byte)TestSet.rxapd_min;
            byte max = (byte)TestSet.rxapd_max;
            byte[] bufminindex = new byte[255];
            switch (Dut)
            {
                case 1:
                    opticaldoaatt.SetAttenuation(DOA.rxDLosAtt + 3); //切换到去告警功率
                    break;
                case 2:
                    opticaldoaatt.SetAttenuation(DOA2.rxDLosAtt + 3); //切换到去告警功率
                    break;
            }


            for (int x = 0; x <= 255; x++)
            {
                psspertbuf[x] = 255;
            }
            valmin = psspertbuf[0];

            ch = pssChannel.Trim().Substring(pssChannel.Trim().Length - 1);//截取PSS通道号
            pssbert.ClearChannelError(ch);
            for (i = min; i <= max; i = (i + 2))
            {
                if (i > max)
                {
                    i = max;
                }

                test.setAPD((byte)i);
                status = pssbert.GetChannelStatus(ch);
                index = status.Length;//status.IndexOf('-');
                psspertbuf[i] = Convert.ToDouble(status.Substring(22, index - 22 - 2 - 1).Trim());//10
                if ((psspertbuf[i] == 0) && status.Contains("Y N"))
                {
                    //Thread.Sleep(2000);//
                    status = pssbert.GetChannelStatus(ch);
                    index = status.Length;
                    str = status.Substring(22, index - 22 - 2 - 1).Trim();//11
                    if (str.Contains("Y") || str.Contains("N"))
                    {
                        psspertbuf[i] = Convert.ToDouble(status.Substring(22, index - 22 - 2 - 1).Trim());//10
                    }
                    else
                    {
                        psspertbuf[i] = Convert.ToDouble(status.Substring(22, index - 22 - 2 - 1).Trim());//11
                    }
                }
                else if ((psspertbuf[i] == 0) && !status.Contains("Y N"))//失步
                {
                    psspertbuf[i] = 100;
                }
                pssbert.ClearChannelError(ch);

                if ((status.Substring(status.Length - 3) == "Y Y") || (status.Substring(status.Length - 3) == "Y N"))//误码/同步
                {
                    if (psspertbuf[i] < valmin)
                    {
                        valmin = psspertbuf[i];
                        valminindex = (byte)i;
                    }
                }
                else
                {
                    psspertbuf[i] = 100;
                    i = (i + 5);
                    if (i > max)
                    {
                        i = max;
                    }
                    continue;
                }

                if (i == max)
                {
                    break;
                }
            }
            int y = 0;
            for (byte x = 0; x < 255; x++)
            {
                if (psspertbuf[x] == psspertbuf[valminindex])
                {
                    bufminindex[y] = x;
                    y++;
                }
            }
            if (y != 1)
            {
                y = (int)(y / 1.2);//2.5
                valminindex = bufminindex[y];
            }
            if (psspertbuf[valminindex] > 1)
            {
                return false;
            }
            if ((valminindex < min) || (valminindex > max))
            {
                return false;
            }
            test.setAPD(valminindex);
            switch (Dut)
            {
                case 1:
                    TestResult.rxapdVal = valminindex;
                    break;
                case 2:
                    TestResult2.rxapdVal = valminindex;
                    break;
                default:
                    break;
            }
            return true;
        }

        private async Task<bool> AutoTestRxAPD_Async(byte dutNo)
        {
            // // 从全局映射读取当前DUT对应的槽位、VOA通道、BERT通道
            // string targetSlot = GlobalVarFun.VOArxDutToSlot[dutNo];
            // int voaCh = GlobalVarFun.DutToVoaCh[dutNo];
            // int bertCh = GlobalVarFun.DutToBertCh[dutNo];

            // double[] psspertbuf = new double[256];
            // double valmin = double.MaxValue;
            // byte valminindex = 0;
            // string statusRaw = "";
            // int i = 0;
            // string strErr = "";
            // byte min = (byte)TestSet.rxapd_min;
            // byte max = (byte)TestSet.rxapd_max;

            // // ========== 原独立衰减器逻辑替换为OTP VOA ==========
            // double losAtt = 0;
            // switch (Dut)
            // {
            //     case 1: losAtt = DOA.rxDLosAtt + 3; break;
            //     case 2: losAtt = DOA2.rxDLosAtt + 3; break;
            //     case 3: losAtt = DOA3.rxDLosAtt + 3; break;
            //     case 4: losAtt = DOA4.rxDLosAtt + 3; break;
            // }
            // // OTP指定槽位+通道设置衰减
            //otp12.VOA_SetAttenuationToSlot(targetSlot, voaCh, losAtt);
            // await Task.Delay(300); // OTP光衰调节稳定延时

            // // 数组初始化
            // for (int x = 0; x <= 255; x++)
            //     psspertbuf[x] = 100.0;
            // valmin = 100;

            // // 清空当前BERT通道误码计数（替换原pssbert.ClearChannelError）
            // otp12.BERT_ClearAllErr();
            // await Task.Delay(100);

            // // 遍历APD调节区间 min ~ max，步长+2
            // for (i = min; i <= max; i += 2)
            // {
            //     if (i > max) i = max;
            //     await Task.Delay(waittimes);

            //     // 设置当前模块APD值（原有test对象逻辑不变）
            //     test.setAPD((byte)i);

            //     // ========== 替换原pssbert.GetChannelStatus ==========
            //     // OTP读取BERT通道误码数据：返回 误码数,总比特,锁定标记
            //     statusRaw = GlobalVarFun.OTP_12.BERT_GetErrData(bertCh);
            //     if (string.IsNullOrEmpty(statusRaw))
            //     {
            //         psspertbuf[i] = 100;
            //         continue;
            //     }
            //     // 拆分OTP返回数据
            //     string[] dataArr = statusRaw.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            //     double errBit = 0;
            //     if (double.TryParse(dataArr[0], out errBit))
            //         psspertbuf[i] = err;

            //     // 原有业务判断逻辑完全保留
            //     if ((psspertbuf[i] == 0) && statusRaw.Contains("1")) // 锁定标记=1（同步）
            //     {
            //         statusRaw = GlobalVarFun.OTP_12.BERT_GetErr(bertCh);
            //         dataArr = statusRaw.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            //         double.TryParse(dataArr[0], out psspertbuf[i]);
            //     }
            //     else if (psspertbuf[i] == 0 && !statusRaw.Contains("1"))
            //     {
            //         psspertbuf[i] = 100; // 失步赋值大值
            //     }

            //     // 清空误码计数器
            //     GlobalVarFun.OTP_12.BERT_ClearAllErr();

            //     // 判断同步标记，寻找最小误码点
            //     if (statusRaw.Contains("1"))
            //     {
            //         if (psspertbuf[i] < valmin)
            //         {
            //             valmin = psspertbuf[i];
            //             valminindex = (byte)i;
            //         }
            //     }
            //     else
            //     {
            //         psspertbuf[i] = 100;
            //         i += 5;
            //         if (i > max) i = max;
            //         continue;
            //     }

            //     if (i == max) break;
            // }

            // // 多相同最小值取中间索引逻辑不变
            // byte[] bufminindex = new byte[255];
            // int y = 0;
            // for (byte x = 0; x < 255; x++)
            // {
            //     if (Math.Abs(psspert[x] - valmin) < 0.0001)
            //     {
            //         bufminindex[y++] = x;
            //     }
            // }
            // if (y != 1)
            // {
            //     y = (int)(y / 1.2);
            //     valminindex = bufminindex[y];
            // }

            // // 校验最小误码是否合格
            // if (psspertbuf[valminindex] > 1 || valminindex < min || valminindex > max)
            //     return false;

            // // 写入最优APD值到对应DUT结果
            // test.setAPD(valminindex);
            // switch (dutNo)
            // {
            //     case 1: TestResult.rxapdVal = valminindex; break;
            //     case 2: TestResult2.rxapdVal = valminindex; break;
            //     case 3: TestResult3.rxapdVal = valminindex; break;
            //     case 4: TestResult4.rxapdVal = valminindex; break;
            // }
            return true;
        }
        #endregion

        #region// 待测模块发光功率自动调试 
        private bool TxPowerAutoSet()
        {
            UInt16 mod = TestSet.txmod_Min;
            mod += 10;

            // 消光比调到 min + 10
            if (test.SetTxModBias(mod) == false) return false;
            //

            // 选择调试方法
            if (GlobalVarFun.txpwr_debug_method == 0x00)
            {
                AddTestLog("txpwr_debug_method:0x00");
                return AutoSetTxPower_MethodA(); // 线性计算法 apc-->uw & bias
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x11)
            {
                AddTestLog("txpwr_debug_method:0x11");
                return AutoSetTxPower_MethodB(); // 普通二分法 apc-->dBm
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x22)
            {
                AddTestLog("txpwr_debug_method:0x22");
                return AutoSetTxPower_MethodC(); // 差值二分法 apc-->uW
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x33)
            {
                AddTestLog("txpwr_debug_method:0x33");
                //return AutoSetTxPower_MethodD(); // 差值二分法 apc-->uW , 0.6倍bias
                return AutoSetTxPower_MethodE(); // 普通二分法 apc-->dBm
            }
            else
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage = "发光功率自动调试方法错误，请选择正确的方法";
                }
                else
                {
                    retutntxrxresult.ErrorMessage = "Tx optical power automatic debugging method is wrong, please choose the correct method!";
                }
                return false; //未定义 错误返回
            }
        }

        private async Task<bool> TxPowerAutoSet_Async()
        {
            UInt16 mod = TestSet.txmod_Min;
            mod += 10;

            // 消光比调到 min + 10
            if (test.SetTxModBias(mod) == false) return false;
            //

            // 选择调试方法
            if (GlobalVarFun.txpwr_debug_method == 0x00)
            {
                AddTestLog("txpwr_debug_method:0x00");
                // return AutoSetTxPower_MethodA(); // 线性计算法 apc-->uw & bias
                bool res = await AutoSetTxPower_MethodA_Async();
                return res;
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x11)
            {
                AddTestLog("txpwr_debug_method:0x11");
                //return AutoSetTxPower_MethodB(); // 普通二分法 apc-->dBm
                bool res = await AutoSetTxPower_MethodB_Async();
                return res;
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x22)
            {
                AddTestLog("txpwr_debug_method:0x22");
                //return AutoSetTxPower_MethodC(); // 差值二分法 apc-->uW
                bool res = await AutoSetTxPower_MethodC_Async();
                return res;
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x33)
            {
                AddTestLog("txpwr_debug_method:0x33");
                //return AutoSetTxPower_MethodD(); // 差值二分法 apc-->uW , 0.6倍bias
                //return AutoSetTxPower_MethodE(); // 普通二分法 apc-->dBm
                bool res = await AutoSetTxPower_MethodE_Async();
                return res;
            }
            else
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage = "发光功率自动调试方法错误，请选择正确的方法";
                }
                else
                {
                    retutntxrxresult.ErrorMessage = "Tx optical power automatic debugging method is wrong, please choose the correct method!";
                }
                return false; //未定义 错误返回
            }

            //await Task.Delay(10000);
            //return true;
        }
        #endregion

        #region// 普通二分法 调试 Bias
        private UInt16 AutoSetTxBias_MethodDic(UInt16 min, UInt16 max, float bias_target, float maxErr)
        {
            float bias, result_err;
            UInt16 apc = 0;
            int looptime = 0;

            // 普通二分法查找
            do
            {
                looptime++;

                apc = (UInt16)((min + max) / 2);

                if (apc < 2) return 0; // 值太小 Error

                if (test.SetTxApcBias(apc) == false) return 0;
                Thread.Sleep(300); // 延时 保证精度
                bias = test.GetTxBias();
                if (bias <= 0) return 0;
                result_err = bias - bias_target;
                //
                if (result_err > 0)
                {
                    max = (UInt16)(apc - 1);
                }
                else
                {
                    min = (UInt16)(apc + 1);
                }
                //
                result_err /= bias_target; //转算为 百分比
            } while ((Math.Abs(result_err) > maxErr) && (max > min) && (looptime < 10));

            // 二分法查找完成   判断：bias是否在误差范围内
            if (Math.Abs(result_err) > maxErr)
            {
                return 0; // 异常
            }
            else
            {
                return apc; // 正常
            }
        }
        #endregion

        #region// 普通二分法 调试 Power
        private UInt16 AutoSetTxPWR_MethodDic(UInt16 min, UInt16 max, float pwr_target, float maxErr)
        {
            float bias, pwr = 0, result_err;
            UInt16 apc = 0;
            int looptime = 0;

            // 普通二分法查找
            do
            {
                looptime++;

                apc = (UInt16)((min + max) / 2);

                if (apc < 2) return 0; // 值太小 Error

                if (test.SetTxApcBias(apc) == false) return 0;
                Thread.Sleep(100); // 延时 保证精度
                switch (Dut)
                {
                    case 1:
                        TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        break;
                    case 2:
                        TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        break;
                    default:
                        break;
                }
                bias = test.GetTxBias();
                if ((bias <= 0) || (pwr <= -60)) return 0;
                result_err = pwr - pwr_target;
                //
                if (result_err > 0)
                {
                    max = (UInt16)(apc - 1);
                }
                else
                {
                    min = (UInt16)(apc + 1);
                }
            } while ((Math.Abs(result_err) > maxErr) && (max > min) && (looptime < 10));

            // 二分法查找完成   判断：tx_power是否在误差范围内
            if (Math.Abs(result_err) > maxErr)
            {
                return 0; // 异常
            }
            else
            {
                return apc; // 正常
            }
        }
        #endregion

        #region // 待测模块发光功率自动调试  // 方法A  apc-->uW & bias_mA 线性关系
        private bool AutoSetTxPower_MethodA()
        {
            UInt16 min = TestSet.txapc_Min;
            UInt16 max = TestSet.txapc_Max;
            UInt16 mid = (UInt16)((min + max) / 2);
            UInt16 apc, apc_uw, apc_bias;

            float pwr_min, pwr_mid, pwr_target, pwr;
            float uw_min, uw_mid, uw_target;
            float bias_min, bias_mid, bias_target, bias;

            float Bias_Min = TestSet.bias_Min;
            float Bias_Max = TestSet.bias_Max;

            Bias_Min *= 1.02f;
            Bias_Max /= 1.02f;

            float TxPwr_Min = TestSet.txPwr_Min;
            float TxPwr_Max = TestSet.txPwr_Max;

            TxPwr_Min += 0.2f;
            TxPwr_Max -= 0.2f;

            pwr_target = TestSet.txPwr_target;
            uw_target = ConvertdBmtouW(pwr_target);
            bias_target = TestSet.txBias_target;

            // bias目标值异常
            if (bias_target < 1)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "Bias目标值设置异常";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Bias Indicates that the target value is abnormal";
                }
                return false; // 异常
            }

            // min
            if (test.SetTxApcBias(min) == false) return false;
            Thread.Sleep(100); // 延时 保证精度
            pwr = pwr_min = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            uw_min = ConvertdBmtouW(pwr_min);
            bias = bias_min = test.GetTxBias();
            if ((bias <= 0) || (pwr_min <= -60)) return false;
            if (pwr_min > TestSet.txPwr_Max) goto CHECK_POS; // 跳转到结果检查

            // mid
            if (test.SetTxApcBias(mid) == false) return false;
            Thread.Sleep(100); // 延时 保证精度
            pwr = pwr_mid = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            uw_mid = ConvertdBmtouW(pwr_mid);
            bias = bias_mid = test.GetTxBias();
            if ((bias <= 0) || (pwr_mid <= -60)) return false;
            //if (pwr_mid < TestSet.txPwr_Min) goto CHECK_POS; // 跳转到结果检查

            // 线性计算  根据tx power uW
            if ((pwr_mid - pwr_min) < 0.3) goto CHECK_POS; //0.3dB 光功率可调范围太小
            apc_uw = min;
            apc_uw += (UInt16)((int)(((uw_target - uw_min) / (uw_mid - uw_min)) * (mid - min) + 0.5)); // 四舍五入
            if (apc_uw > max) apc_uw = max;

            // 线性计算  根据bias mA
            apc_bias = min;
            apc_bias += (UInt16)((int)(((bias_target - bias_min) / (bias_mid - bias_min)) * (mid - min) + 0.5)); // 四舍五入
            if (apc_bias > max) apc_bias = max;

            ///////////////////////////////////////////////////////////////////////////
            // 调试目标Power
            if (test.SetTxApcBias(apc_uw) == false) return false;
            Thread.Sleep(100); // 延时 保证精度
            switch (Dut)
            {
                case 1:
                    meter_err = TestSet.meter_pwr_err;
                    break;
                case 2:
                    meter_err = TestSet2.meter_pwr_err;
                    break;
                case 3:
                    meter_err = TestSet3.meter_pwr_err;
                    break;
                case 4:
                    meter_err = TestSet4.meter_pwr_err;
                    break;
            }
            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            if (Math.Abs(pwr - pwr_target) > 0.35f) //
            {
                apc_uw = AutoSetTxPWR_MethodDic(min, max, pwr_target, 0.35f);
                if (apc_uw == 0)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.ErrorMessage += "目标Power 调试失败";
                    }
                    else
                    {
                        retutntxrxresult.ErrorMessage += "The target Power debugging failed";
                    }
                    return false; // 异常
                }
            }
            ///////////////////////////////////////////////////////////////////////////
            // 调试目标Bias
            if (test.SetTxApcBias(apc_bias) == false) return false;
            Thread.Sleep(300); // 延时 保证精度
            bias = test.GetTxBias();
            if (Math.Abs((bias - bias_target) / bias_target) > 0.06f) //
            {
                apc_bias = AutoSetTxBias_MethodDic(min, max, bias_target, 0.06f);
                if (apc_bias == 0) // 异常
                {
                    apc_bias = apc_uw;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.ErrorMessage += "目标Bias 调试失败";
                    }
                    else
                    {
                        retutntxrxresult.ErrorMessage += "Target Bias debugging failed";
                    }
                    return false; // 异常
                }
            }
            ///////////////////////////////////////////////////////////////////////////

            // 平均 设置值
            apc = (UInt16)((apc_uw + apc_bias) / 2);
            if ((apc < min) || (apc > max))
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "发光调试计算的APC值错误" + apc_uw.ToString() + apc_bias.ToString() + apc.ToString();
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "The APC value calculated by luminescence debugging is incorrect" + apc_uw.ToString() + apc_bias.ToString() + apc.ToString();
                }
                return false; // 异常
            }

            if (test.SetTxApcBias(apc) == false) return false;
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    break;
                case 2:
                    TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    break;
                default:
                    break;
            }
            bias = test.GetTxBias();
            if ((bias <= 0) || (pwr <= -60)) return false;

            // bias电流偏大 或者 TxPwr发光偏大
            if ((bias >= Bias_Max) || (pwr >= TxPwr_Max))
            {
                do
                {
                    if (apc < 3) return false;

                    if (apc < 30) apc -= 1;
                    else apc -= 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                } while (((bias >= Bias_Max) || (pwr >= TxPwr_Max)) && (pwr > TxPwr_Min) && (apc > TestSet.txapc_Min));
                //
                goto CHECK_POS;
            }
            // bias电流偏小 或者 TxPwr发光偏小
            if ((bias <= Bias_Min) || (pwr <= TxPwr_Min))
            {
                do
                {
                    if (apc < 30) apc += 1;
                    else apc += 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                } while (((bias <= Bias_Min) || (pwr <= TxPwr_Min)) && (pwr < TxPwr_Max) && (apc < TestSet.txapc_Max));
            }

        //////
        CHECK_POS:
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if ((pwr <= TestSet.txPwr_Max) && (pwr >= TestSet.txPwr_Min) && (bias <= TestSet.bias_Max) && (bias >= TestSet.bias_Min))
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Luminous calibration ADC error";
                            }
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    retutntxrxresult.TxpwrResultShow = TestSet2.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet2.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if ((pwr <= TestSet2.txPwr_Max) && (pwr >= TestSet2.txPwr_Min) && (bias <= TestSet2.bias_Max) && (bias >= TestSet2.bias_Min))
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Luminous calibration ADC error";
                            }
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                default:
                    break;
            }

            //
            return false;
        }
        #endregion

        #region // 待测模块发光功率自动调试  // 方法A  apc-->uW & bias_mA 线性关系 异步
        private async Task<bool> AutoSetTxPower_MethodA_Async()
        {
            UInt16 min = TestSet.txapc_Min;
            UInt16 max = TestSet.txapc_Max;
            UInt16 mid = (UInt16)((min + max) / 2);
            UInt16 apc, apc_uw, apc_bias;

            float pwr_min, pwr_mid, pwr_target, pwr;
            float uw_min, uw_mid, uw_target;
            float bias_min, bias_mid, bias_target, bias;

            float Bias_Min = TestSet.bias_Min;
            float Bias_Max = TestSet.bias_Max;

            Bias_Min *= 1.02f;
            Bias_Max /= 1.02f;

            float TxPwr_Min = TestSet.txPwr_Min;
            float TxPwr_Max = TestSet.txPwr_Max;

            TxPwr_Min += 0.2f;
            TxPwr_Max -= 0.2f;

            pwr_target = TestSet.txPwr_target;
            uw_target = ConvertdBmtouW(pwr_target);
            bias_target = TestSet.txBias_target;
            // bias目标值异常
            if (bias_target < 1)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "Bias目标值设置异常";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "Bias Indicates that the target value is abnormal";
                }
                return false; // 异常
            }

            // min
            if (test.SetTxApcBias(min) == false) return false;
            Thread.Sleep(100); // 延时 保证精度
            pwr = pwr_min = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            uw_min = ConvertdBmtouW(pwr_min);
            bias = bias_min = test.GetTxBias();
            if ((bias <= 0) || (pwr_min <= -60)) return false;
            if (pwr_min > TestSet.txPwr_Max) goto CHECK_POS; // 跳转到结果检查

            // mid
            if (test.SetTxApcBias(mid) == false) return false;
            Thread.Sleep(100); // 延时 保证精度
            pwr = pwr_mid = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            uw_mid = ConvertdBmtouW(pwr_mid);
            bias = bias_mid = test.GetTxBias();
            if ((bias <= 0) || (pwr_mid <= -60)) return false;
            //if (pwr_mid < TestSet.txPwr_Min) goto CHECK_POS; // 跳转到结果检查

            // 线性计算  根据tx power uW
            if ((pwr_mid - pwr_min) < 0.3) goto CHECK_POS; //0.3dB 光功率可调范围太小
            apc_uw = min;
            apc_uw += (UInt16)((int)(((uw_target - uw_min) / (uw_mid - uw_min)) * (mid - min) + 0.5)); // 四舍五入
            if (apc_uw > max) apc_uw = max;

            // 线性计算  根据bias mA
            apc_bias = min;
            apc_bias += (UInt16)((int)(((bias_target - bias_min) / (bias_mid - bias_min)) * (mid - min) + 0.5)); // 四舍五入
            if (apc_bias > max) apc_bias = max;

            ///////////////////////////////////////////////////////////////////////////
            // 调试目标Power
            if (test.SetTxApcBias(apc_uw) == false) return false;
            Thread.Sleep(100); // 延时 保证精度
            switch (Dut)
            {
                case 1:
                    meter_err = TestSet.meter_pwr_err;
                    break;
                case 2:
                    meter_err = TestSet2.meter_pwr_err;
                    break;
            }
            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            if (Math.Abs(pwr - pwr_target) > 0.35f) //
            {
                apc_uw = AutoSetTxPWR_MethodDic(min, max, pwr_target, 0.35f);
                if (apc_uw == 0)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.ErrorMessage += "目标Power 调试失败";
                    }
                    else
                    {
                        retutntxrxresult.ErrorMessage += "The target Power debugging failed";
                    }
                    return false; // 异常
                }
            }
            ///////////////////////////////////////////////////////////////////////////
            // 调试目标Bias
            if (test.SetTxApcBias(apc_bias) == false) return false;
            Thread.Sleep(300); // 延时 保证精度
            bias = test.GetTxBias();
            if (Math.Abs((bias - bias_target) / bias_target) > 0.06f) //
            {
                apc_bias = AutoSetTxBias_MethodDic(min, max, bias_target, 0.06f);
                if (apc_bias == 0) // 异常
                {
                    apc_bias = apc_uw;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.ErrorMessage += "目标Bias 调试失败";
                    }
                    else
                    {
                        retutntxrxresult.ErrorMessage += "Target Bias debugging failed";
                    }
                    return false; // 异常
                }
            }
            ///////////////////////////////////////////////////////////////////////////

            // 平均 设置值
            apc = (UInt16)((apc_uw + apc_bias) / 2);
            if ((apc < min) || (apc > max))
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "发光调试计算的APC值错误" + apc_uw.ToString() + apc_bias.ToString() + apc.ToString();
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "The APC value calculated by luminescence debugging is incorrect" + apc_uw.ToString() + apc_bias.ToString() + apc.ToString();
                }
                return false; // 异常
            }

            if (test.SetTxApcBias(apc) == false) return false;
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    AddTestLog("txpwer:" + TestResult.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                    break;
                case 2:
                    TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    AddTestLog("txpwer2:" + TestResult2.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                    break;
                default:
                    break;
            }
            bias = test.GetTxBias();
            if ((bias <= 0) || (pwr <= -60)) return false;
            // bias电流偏大 或者 TxPwr发光偏大
            if ((bias >= Bias_Max) || (pwr >= TxPwr_Max))
            {
                AddTestLog("bias电流偏大 或者 TxPwr发光偏大");
                do
                {
                    if (apc < 3) return false;

                    if (apc < 30) apc -= 1;
                    else apc -= 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            AddTestLog("txpwer:" + TestResult.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            AddTestLog("txpwer2:" + TestResult2.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                    await Task.Delay(waittimes);
                } while (((bias >= Bias_Max) || (pwr >= TxPwr_Max)) && (pwr > TxPwr_Min) && (apc > TestSet.txapc_Min));
                //
                goto CHECK_POS;
            }
            // bias电流偏小 或者 TxPwr发光偏小
            if ((bias <= Bias_Min) || (pwr <= TxPwr_Min))
            {
                AddTestLog("bias电流偏小 或者 TxPwr发光偏小");
                do
                {
                    if (apc < 30) apc += 1;
                    else apc += 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            AddTestLog("txpwer:" + TestResult.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            AddTestLog("txpwer2:" + TestResult2.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                    await Task.Delay(waittimes);
                } while (((bias <= Bias_Min) || (pwr <= TxPwr_Min)) && (pwr < TxPwr_Max) && (apc < TestSet.txapc_Max));
            }

        //////
        CHECK_POS:
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if ((pwr <= TestSet.txPwr_Max) && (pwr >= TestSet.txPwr_Min) && (bias <= TestSet.bias_Max) && (bias >= TestSet.bias_Min))
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Luminous calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if ((pwr <= TestSet.txPwr_Max) && (pwr >= TestSet.txPwr_Min) && (bias <= TestSet.bias_Max) && (bias >= TestSet.bias_Min))
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Luminous calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                default:
                    break;
            }
            //
            AddTestLog(retutntxrxresult.TxpwrResultShow.ToString());
            if (retutntxrxresult.ErrorMessage == "") return true;
            return false;
        }
        #endregion

        #region // 待测模块发光功率自动调试  // 方法B  根据dBm  用普通二分法
        private bool AutoSetTxPower_MethodB()
        {
            UInt16 min = TestSet.txapc_Min;
            UInt16 max = TestSet.txapc_Max;
            UInt16 apc;
            float bias, pwr = 0, pwr_maxErr, result_err;
            int looptime = 0;

            float Bias_Min = TestSet.bias_Min;
            float Bias_Max = TestSet.bias_Max;

            Bias_Min *= 1.05f;
            Bias_Max /= 1.05f;

            float TxPwr_Min = TestSet.txPwr_Min;
            float TxPwr_Max = TestSet.txPwr_Max;

            TxPwr_Min += 0.2f;
            TxPwr_Max -= 0.2f;

            pwr_maxErr = 0.5f; // 0.5dB  二分法查找精度

            string slotStr = GlobalVarFun.OpmDutToOtpSlot[Dut];
            int opmCh = GlobalVarFun.DutToOpmCh[Dut];
            otp12.SetSlot(slotStr);

            string txpower;
            // 普通二分法查找
            do
            {
                looptime++;
                apc = (UInt16)((min + max) / 2);
                if (apc < 2) return false; // 值太小 Error

                if (test.SetTxApcBias(apc) == false) return false;
                switch (Dut)
                {
                    case 1:
                        txpower = otp12.OPM_ReadPower(opmCh);
                        float.TryParse(txpower, out pwr);
                        pwr += meter_err;
                        TestResult.txPower = pwr;
                        if (pwr < -40)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                txpower = otp12.OPM_ReadPower(opmCh);
                                float.TryParse(txpower, out pwr);
                                pwr += meter_err;
                                TestResult.txPower = pwr;
                            }
                        }
                        AddTestLog("txpwer:" + TestResult.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                        break;
                    case 2:
                        txpower = otp12.OPM_ReadPower(opmCh);
                        float.TryParse(txpower, out pwr);
                        pwr += meter_err;
                        TestResult2.txPower = pwr;
                        if (pwr < -40)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                txpower = otp12.OPM_ReadPower(opmCh);
                                float.TryParse(txpower, out pwr);
                                pwr += meter_err;
                                TestResult2.txPower = pwr;
                            }
                        }
                        AddTestLog("txpwer2:" + TestResult2.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                        break;
                    case 3:
                        txpower = otp12.OPM_ReadPower(opmCh);
                        float.TryParse(txpower, out pwr);
                        pwr += meter_err;
                        TestResult3.txPower = pwr;
                        if (pwr < -40)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                txpower = otp12.OPM_ReadPower(opmCh);
                                float.TryParse(txpower, out pwr);
                                pwr += meter_err;
                                TestResult3.txPower = pwr;
                            }
                        }
                        AddTestLog("txpwer3:" + TestResult3.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                        break;
                    case 4:
                        txpower = otp12.OPM_ReadPower(opmCh);
                        float.TryParse(txpower, out pwr);
                        pwr += meter_err;
                        TestResult4.txPower = pwr;
                        if (pwr < -40)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                txpower = otp12.OPM_ReadPower(opmCh);
                                float.TryParse(txpower, out pwr);
                                pwr += meter_err;
                                TestResult4.txPower = pwr;
                            }
                        }
                        AddTestLog("txpwer4:" + TestResult4.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                        break;
                    default:
                        break;
                }
                bias = test.GetTxBias();
                if ((bias <= 0) || (pwr <= -60)) return false;
                result_err = pwr - TestSet.txPwr_target;
                //
                if (result_err > 0)
                {
                    max = (UInt16)(apc - 1);
                }
                else
                {
                    min = (UInt16)(apc + 1);
                }
            } while ((Math.Abs(result_err) > pwr_maxErr) && (max > min) && (looptime < 10));
            switch (Dut)
            {
                case 1:
                    AddTestLog("txpwer:" + TestResult.txPower.ToString() + " bias:" + bias.ToString() + " APC:" + apc.ToString());
                    break;
                case 2:
                    AddTestLog("txpwer2:" + TestResult2.txPower.ToString() + " bias2:" + bias.ToString() + " APC:" + apc.ToString());
                    break;
                case 3:
                    AddTestLog("txpwer3:" + TestResult3.txPower.ToString() + " bias3:" + bias.ToString() + " APC:" + apc.ToString());
                    break;
                case 4:
                    AddTestLog("txpwer4:" + TestResult4.txPower.ToString() + " bias4:" + bias.ToString() + " APC:" + apc.ToString());
                    break;
                default:
                    break;
            }
            // 二分法查找完成   判断：光功率达到目标值 并且 bias在合理范围内
            if ((Math.Abs(result_err) <= pwr_maxErr) && (bias <= Bias_Max) && (bias >= Bias_Min))
            {
                AddTestLog("判断：光功率达到目标值 并且 bias在合理范围内");
                goto CHECK_POS;
            }

            // bias电流偏大
            if (bias >= Bias_Max)
            {
                AddTestLog("bias电流偏大,微调");
                do
                {
                    if (apc < 3) return false;

                    if (apc < 30) apc -= 1;
                    else apc -= 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult.txPower = pwr;
                                }
                            }
                            break;
                        case 2:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult2.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult2.txPower = pwr;
                                }
                            }
                            break;
                        case 3:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult3.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult3.txPower = pwr;
                                }
                            }
                            break;
                        case 4:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult4.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult4.txPower = pwr;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                } while ((bias >= Bias_Max) && (pwr > TxPwr_Min) && (apc > TestSet.txapc_Min));
                //
                goto CHECK_POS;
            }
            // bias电流偏小
            if (bias <= Bias_Min)
            {
                AddTestLog("bias电流偏小,微调");
                do
                {
                    if (apc < 30) apc += 1;
                    else apc += 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult4.txPower = pwr;
                                }
                            }
                            break;
                        case 2:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult2.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult2.txPower = pwr;
                                }
                            }
                            break;
                        case 3:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult3.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult3.txPower = pwr;
                                }
                            }
                            break;
                        case 4:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult4.txPower = pwr;
                            if (pwr < -40)
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    txpower = otp12.OPM_ReadPower(opmCh);
                                    float.TryParse(txpower, out pwr);
                                    pwr += meter_err;
                                    TestResult4.txPower = pwr;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                } while ((bias <= Bias_Min) && (pwr < TxPwr_Max) && (apc < TestSet.txapc_Max));
            }

        //////
        CHECK_POS:
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    result_err = pwr - TestSet.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    result_err = pwr - TestSet2.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 3:
                    TestResult3.txPower = pwr;
                    result_err = pwr - TestSet3.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 4:
                    TestResult4.txPower = pwr;
                    result_err = pwr - TestSet4.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                default:
                    break;
            }

            //
            AddTestLog("bias :" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + " bias_Min:" + TestSet.bias_Min.ToString());
            AddTestLog("pwr :" + pwr.ToString() + " txPwr_Max:" + TestSet.txPwr_Max.ToString() + " txPwr_Min:" + TestSet.txPwr_Min.ToString());

            AddTestLog(retutntxrxresult.TxpwrResultShow.ToString());
            if (retutntxrxresult.ErrorMessage == "") return true;
            return false;
        }

        #endregion

        #region // 待测模块发光功率自动调试  // 方法B  根据dBm  用普通二分法  异步
        private async Task<bool> AutoSetTxPower_MethodB_Async()
        {
            UInt16 min = TestSet.txapc_Min;
            UInt16 max = TestSet.txapc_Max;
            UInt16 apc;
            float bias, pwr = 0, pwr_maxErr, result_err;
            int looptime = 0;

            float Bias_Min = TestSet.bias_Min;
            float Bias_Max = TestSet.bias_Max;

            Bias_Min *= 1.05f;
            Bias_Max /= 1.05f;

            float TxPwr_Min = TestSet.txPwr_Min;
            float TxPwr_Max = TestSet.txPwr_Max;

            TxPwr_Min += 0.2f;
            TxPwr_Max -= 0.2f;

            pwr_maxErr = 0.35f; // 0.35dB  二分法查找精度

            AddTestLog("Bias_Min:" + Bias_Min.ToString() + " Bias_Max:" + Bias_Max.ToString() + " TxPwr_Min:" + TxPwr_Min.ToString() + " TxPwr_Max:" + TxPwr_Max.ToString());
            // 普通二分法查找
            do
            {
                looptime++;
                apc = (UInt16)((min + max) / 2);
                if (apc < 2) return false; // 值太小 Error

                if (test.SetTxApcBias(apc) == false) return false;
                await Task.Delay(100);
                switch (Dut)
                {
                    case 1:
                        TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        AddTestLog("txpwer:" + TestResult.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                        break;
                    case 2:
                        TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        AddTestLog("txpwer2:" + TestResult2.txPower.ToString() + " APC:" + apc.ToString() + " meter_ch:" + meter_ch.ToString() + " meter_err:" + meter_err.ToString());
                        break;
                    default:
                        break;
                }
                bias = test.GetTxBias();
                if ((bias <= 0) || (pwr <= -60)) return false;
                result_err = pwr - TestSet.txPwr_target;
                //
                if (result_err > 0)
                {
                    max = (UInt16)(apc - 1);
                }
                else
                {
                    min = (UInt16)(apc + 1);
                }
                await Task.Delay(1);
            } while ((Math.Abs(result_err) > pwr_maxErr) && (max > min) && (looptime < 10));
            switch (Dut)
            {
                case 1:
                    AddTestLog("txpwer:" + TestResult.txPower.ToString() + " bias:" + bias.ToString() + " APC:" + apc.ToString());
                    break;
                case 2:
                    AddTestLog("txpwer2:" + TestResult2.txPower.ToString() + " bias2:" + bias.ToString() + " APC:" + apc.ToString());
                    break;
                default:
                    break;
            }

            // 二分法查找完成   判断：光功率达到目标值 并且 bias在合理范围内
            if ((Math.Abs(result_err) <= pwr_maxErr) && (bias <= Bias_Max) && (bias >= Bias_Min))
            {
                AddTestLog("判断：光功率达到目标值 并且 bias在合理范围内");
                goto CHECK_POS;
            }

            // bias电流偏大
            if (bias >= Bias_Max)
            {
                AddTestLog("bias:" + bias.ToString() + " bias电流偏大,微调");
                do
                {
                    if (apc < 3) return false;

                    if (apc < 30) apc -= 1;
                    else apc -= 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    await Task.Delay(100);
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                    await Task.Delay(waittimes);
                } while ((bias >= Bias_Max) && (pwr > TxPwr_Min) && (apc > TestSet.txapc_Min));
                //
                goto CHECK_POS;
            }
            // bias电流偏小
            if (bias <= Bias_Min)
            {
                AddTestLog("bias:" + bias.ToString() + "bias电流偏小,微调");
                do
                {
                    if (apc < 30) apc += 1;
                    else apc += 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    await Task.Delay(150);
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }

                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                    await Task.Delay(waittimes);
                } while ((bias <= Bias_Min) && (pwr < TxPwr_Max) && (apc < TestSet.txapc_Max));
            }

        //////
        CHECK_POS:
            retutntxrxresult.apc = apc;
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    result_err = pwr - TestSet.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    result_err = pwr - TestSet2.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            AddTestLog(retutntxrxresult.ErrorMessage);
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                default:
                    break;
            }

            //
            AddTestLog("bias :" + bias.ToString() + " bias_Max:" + TestSet.bias_Max.ToString() + " bias_Min:" + TestSet.bias_Min.ToString());
            AddTestLog("pwr :" + pwr.ToString() + " txPwr_Max:" + TestSet.txPwr_Max.ToString() + " txPwr_Min:" + TestSet.txPwr_Min.ToString());

            AddTestLog(retutntxrxresult.TxpwrResultShow.ToString());
            if (retutntxrxresult.ErrorMessage == "") return true;
            return false;
        }

        #endregion

        #region// 待测模块发光功率自动调试  // 方法C  用差值二分法 apc-->uW
        private bool AutoSetTxPower_MethodC()
        {
            UInt16 min = TestSet.txapc_Min;
            UInt16 max = TestSet.txapc_Max;
            UInt16 apc = 0;
            float Bias_Min, Bias_Max;
            float bias = 0;
            float pwr_min, pwr_max, pwr, pwr_target;
            float uw_min, uw_max, uwpwr, uw_target;
            float pwr_maxErr, result_err;
            int looptime = 0;

            Bias_Min = TestSet.bias_Min * 1.05f;
            Bias_Max = TestSet.bias_Max / 1.05f;

            float TxPwr_Min = TestSet.txPwr_Min;
            float TxPwr_Max = TestSet.txPwr_Max;

            TxPwr_Min += 0.2f;
            TxPwr_Max -= 0.2f;

            pwr_target = TestSet.txPwr_target;
            uw_target = ConvertdBmtouW(pwr_target);

            pwr_maxErr = 0.35f; // 0.35dB  二分法查找精度

            // min
            if (test.SetTxApcBias(min) == false) return false;
            Thread.Sleep(500); // 延时 保证精度
            switch (Dut)
            {
                case 1:
                    meter_err = TestSet.meter_pwr_err;
                    break;
                case 2:
                    meter_err = TestSet2.meter_pwr_err;
                    break;
            }
            pwr = pwr_min = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            if ((pwr_min <= -60) || (pwr_min >= TestSet.txPwr_Max)) return false;
            uw_min = ConvertdBmtouW(pwr_min);

            // max
            if (test.SetTxApcBias(max) == false) return false;
            Thread.Sleep(1000); // 延时 保证精度
            pwr = pwr_max = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            if ((pwr_max <= -60) || (pwr_max <= TestSet.txPwr_Min)) return false;
            uw_max = ConvertdBmtouW(pwr_max);

            bias = test.GetTxBias();
            if (bias <= 0) return false;

            // 差值二分法查找
            do
            {
                looptime++;

                if ((pwr_max - pwr_min) < 0.1) goto CHECK_POS;
                apc = min;
                apc += (UInt16)((int)(((uw_target - uw_min) / (uw_max - uw_min)) * (max - min) + 0.5)); // 四舍五入
                //
                if ((apc > max) || (apc < 3)) return false;
                //
                if (test.SetTxApcBias(apc) == false) return false;
                switch (Dut)
                {
                    case 1:
                        TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        break;
                    case 2:
                        TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        break;
                    default:
                        break;
                }

                if (pwr <= -60) return false;
                uwpwr = ConvertdBmtouW(pwr);
                result_err = pwr - TestSet.txPwr_target;
                //
                if (result_err > 0)
                {
                    max = (UInt16)(apc - 1);
                    pwr_max = pwr;
                    uw_max = uwpwr;
                }
                else
                {
                    min = (UInt16)(apc + 1);
                    pwr_min = pwr;
                    uw_min = uwpwr;
                }
            } while ((Math.Abs(result_err) > pwr_maxErr) && (max > min) && (looptime < 10));

            bias = test.GetTxBias();
            if (bias <= 0) return false;

            // 测试OK的   光功率达到目标值  bias在合理范围内
            if (Math.Abs(result_err) <= pwr_maxErr && bias <= Bias_Max && bias >= Bias_Min)
            {
                goto CHECK_POS;
            }

            // bias电流偏大
            if (bias >= Bias_Max)
            {
                do
                {
                    if (apc < 3) return false;

                    if (apc < 30) apc -= 1;
                    else apc -= 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                } while ((bias >= Bias_Max) && (pwr > TxPwr_Min) && (apc > TestSet.txapc_Min));
                //
                goto CHECK_POS;
            }
            // bias电流偏小
            if (bias <= Bias_Min)
            {
                do
                {
                    if (apc < 30) apc += 1;
                    else apc += 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                } while ((bias <= Bias_Min) && (pwr < TxPwr_Max) && (apc < TestSet.txapc_Max));
            }

        //////
        CHECK_POS:
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    result_err = pwr - TestSet.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    result_err = pwr - TestSet.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                default:
                    break;
            }
            //
            return false;
        }
        #endregion

        #region// 待测模块发光功率自动调试   // 方法C  用差值二分法 apc-->uW  异步
        private async Task<bool> AutoSetTxPower_MethodC_Async()
        {
            UInt16 min = TestSet.txapc_Min;
            UInt16 max = TestSet.txapc_Max;
            UInt16 apc = 0;
            float Bias_Min, Bias_Max;
            float bias = 0;
            float pwr_min, pwr_max, pwr, pwr_target;
            float uw_min, uw_max, uwpwr, uw_target;
            float pwr_maxErr, result_err;
            int looptime = 0;

            Bias_Min = TestSet.bias_Min * 1.05f;
            Bias_Max = TestSet.bias_Max / 1.05f;

            float TxPwr_Min = TestSet.txPwr_Min;
            float TxPwr_Max = TestSet.txPwr_Max;

            TxPwr_Min += 0.2f;
            TxPwr_Max -= 0.2f;

            pwr_target = TestSet.txPwr_target;
            uw_target = ConvertdBmtouW(pwr_target);

            pwr_maxErr = 0.35f; // 0.35dB  二分法查找精度

            // min
            if (test.SetTxApcBias(min) == false) return false;
            Thread.Sleep(500); // 延时 保证精度
            switch (Dut)
            {
                case 1:
                    meter_err = TestSet.meter_pwr_err;
                    break;
                case 2:
                    meter_err = TestSet2.meter_pwr_err;
                    break;
            }
            pwr = pwr_min = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            if ((pwr_min <= -60) || (pwr_min >= TestSet.txPwr_Max)) return false;
            uw_min = ConvertdBmtouW(pwr_min);

            // max
            if (test.SetTxApcBias(max) == false) return false;
            Thread.Sleep(1000); // 延时 保证精度
            pwr = pwr_max = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            if ((pwr_max <= -60) || (pwr_max <= TestSet.txPwr_Min)) return false;
            uw_max = ConvertdBmtouW(pwr_max);

            bias = test.GetTxBias();
            if (bias <= 0) return false;

            // 差值二分法查找
            do
            {
                looptime++;

                if ((pwr_max - pwr_min) < 0.1) goto CHECK_POS;
                apc = min;
                apc += (UInt16)((int)(((uw_target - uw_min) / (uw_max - uw_min)) * (max - min) + 0.5)); // 四舍五入
                //
                if ((apc > max) || (apc < 3)) return false;
                //
                if (test.SetTxApcBias(apc) == false) return false;
                switch (Dut)
                {
                    case 1:
                        TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        break;
                    case 2:
                        TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                        break;
                    default:
                        break;
                }

                if (pwr <= -60) return false;
                uwpwr = ConvertdBmtouW(pwr);
                result_err = pwr - TestSet.txPwr_target;
                //
                if (result_err > 0)
                {
                    max = (UInt16)(apc - 1);
                    pwr_max = pwr;
                    uw_max = uwpwr;
                }
                else
                {
                    min = (UInt16)(apc + 1);
                    pwr_min = pwr;
                    uw_min = uwpwr;
                }
                await Task.Delay(waittimes);
            } while ((Math.Abs(result_err) > pwr_maxErr) && (max > min) && (looptime < 10));

            bias = test.GetTxBias();
            if (bias <= 0) return false;

            // 测试OK的   光功率达到目标值  bias在合理范围内
            if (Math.Abs(result_err) <= pwr_maxErr && bias <= Bias_Max && bias >= Bias_Min)
            {
                goto CHECK_POS;
            }

            // bias电流偏大
            if (bias >= Bias_Max)
            {
                do
                {
                    if (apc < 3) return false;

                    if (apc < 30) apc -= 1;
                    else apc -= 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                    await Task.Delay(waittimes);
                } while ((bias >= Bias_Max) && (pwr > TxPwr_Min) && (apc > TestSet.txapc_Min));
                //
                goto CHECK_POS;
            }
            // bias电流偏小
            if (bias <= Bias_Min)
            {
                do
                {
                    if (apc < 30) apc += 1;
                    else apc += 2;

                    if (test.SetTxApcBias(apc) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        case 2:
                            TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            break;
                        default:
                            break;
                    }
                    bias = test.GetTxBias();
                    if ((bias <= 0) || (pwr <= -60)) return false;
                    await Task.Delay(waittimes);
                } while ((bias <= Bias_Min) && (pwr < TxPwr_Max) && (apc < TestSet.txapc_Max));
            }

        //////
        CHECK_POS:
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    result_err = pwr - TestSet.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet.txPwr_Max && pwr >= TestSet.txPwr_Min && bias <= TestSet.bias_Max && bias >= TestSet.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    result_err = pwr - TestSet2.txPwr_target;
                    retutntxrxresult.TxpwrResultShow = TestSet2.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet2.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    //
                    if (pwr <= TestSet2.txPwr_Max && pwr >= TestSet2.txPwr_Min && bias <= TestSet2.bias_Max && bias >= TestSet2.bias_Min)
                    {
                        // 写入发射校准参数到模块
                        if (test.WriteTxCalData() == false)
                        {
                            if (GlobalVarFun.Language == "Chinese")
                            {
                                retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                            }
                            else
                            {
                                retutntxrxresult.ErrorMessage += "Tx calibration ADC error";
                            }
                            return false;
                        }
                        //
                        return true;
                    }
                    //
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Bias大,发光小";
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Bias小,发光大";
                    }
                    else
                    {
                        if (bias > TestSet.bias_Max || pwr < TestSet.txPwr_Min) retutntxrxresult.ErrorMessage += "Big Bias, small glow";//Bias大,发光小
                        if (bias < TestSet.bias_Min || pwr > TestSet.txPwr_Max) retutntxrxresult.ErrorMessage += "Small Bias, big glow";//Bias小,发光大
                    }
                    break;
                default:
                    break;
            }
            //
            return false;
        }
        #endregion

        #region  // 待测模块发光功率自动调试  // 方法B  根据dBm  用普通二分法
        private bool AutoSetTxPower_MethodE()
        {
            float bias = 0, pwr = 0;
            UInt16 apc = 0, mod = 0;

            switch (Dut)
            {
                case 1:
                    apc = (UInt16)((TestSet.txapc_Min + TestSet.txapc_Max) / 2);
                    if (test.SetTxApcBias(apc) == false) return false;

                    mod = (UInt16)((TestSet.txmod_Min + TestSet.txmod_Max) / 2);
                    //mod = 0;
                    if (test.SetTxModBias(mod) == false) return false;

                    TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    TestResult.txBiasDDM = bias = test.GetTxBias();
                    break;
                case 2:
                    apc = (UInt16)((TestSet2.txapc_Min + TestSet2.txapc_Max) / 2);
                    if (test.SetTxApcBias(apc) == false) return false;

                    mod = (UInt16)((TestSet2.txmod_Min + TestSet2.txmod_Max) / 2);
                    //mod = 0;
                    if (test.SetTxModBias(mod) == false) return false;

                    TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    TestResult2.txBiasDDM = bias = test.GetTxBias();
                    break;
                default:
                    break;
            }
            //调试信息界面显示
            retutntxrxresult.apc = apc;
            retutntxrxresult.mod = mod;

            if ((bias <= 1) || (pwr <= -30))
            {
                retutntxrxresult.ErrorMessage += "光功率或者Bias异常偏小: ";
                return false;
            }

            return true;
        }
        #endregion

        #region  // 待测模块发光功率自动调试  // 方法B  根据dBm  用普通二分法  异步
        private async Task<bool> AutoSetTxPower_MethodE_Async()
        {
            float bias = 0, pwr = 0;
            UInt16 apc = 0, mod = 0;

            switch (Dut)
            {
                case 1:
                    apc = (UInt16)((TestSet.txapc_Min + TestSet.txapc_Max) / 2);
                    if (test.SetTxApcBias(apc) == false) return false;

                    mod = (UInt16)((TestSet.txmod_Min + TestSet.txmod_Max) / 2);
                    //mod = 0;
                    if (test.SetTxModBias(mod) == false) return false;

                    TestResult.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    TestResult.txBiasDDM = bias = test.GetTxBias();
                    break;
                case 2:
                    apc = (UInt16)((TestSet2.txapc_Min + TestSet2.txapc_Max) / 2);
                    if (test.SetTxApcBias(apc) == false) return false;

                    mod = (UInt16)((TestSet2.txmod_Min + TestSet2.txmod_Max) / 2);
                    //mod = 0;
                    if (test.SetTxModBias(mod) == false) return false;

                    TestResult2.txPower = pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                    TestResult2.txBiasDDM = bias = test.GetTxBias();
                    break;
                default:
                    break;
            }
            //调试信息界面显示
            retutntxrxresult.apc = apc;
            retutntxrxresult.mod = mod;

            if ((bias <= 1) || (pwr <= -30))
            {
                retutntxrxresult.ErrorMessage += "光功率或者Bias异常偏小: ";
                return false;
            }

            return true;
        }
        #endregion

        #region // 待测模块消光比自动调试
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        private bool TxErAutoSet()
        {
            lock (tx_lock)
            {
                //光开关切换
                TestControl.opticalswitch.SetChannel(Dut);
                if (GlobalVarFun.txpwr_debug_method == 0x33)
                {
                    AddTestLog("txpwr_debug_method:0x33");
                    return AutoSetTxEr_MethodE(); // 普通二分法,二次调试
                }
                else
                {
                    AddTestLog("txpwr_debug_method:其他，AutoSetTxEr_MethodA");

                    return AutoSetTxEr_MethodA(); // 普通二分法
                }
            }
            //return AutoSetTxEr_MethodB(); // 差值二分法
        }

        private async Task<bool> TxErAutoSet_Async()
        {
            // lock (tx_lock)
            //{
            //光开关切换
            //TestControl.opticalswitch.SetChannel(Dut);
            if (GlobalVarFun.txpwr_debug_method == 0x33)
            {
                AddTestLog("txpwr_debug_method:0x33");
                //return AutoSetTxEr_MethodE(); // 普通二分法,二次调试
                bool res = await AutoSetTxEr_MethodE_Async();
                return res;
            }
            else
            {
                AddTestLog("txpwr_debug_method:其他，AutoSetTxEr_MethodA");
                //return AutoSetTxEr_MethodA(); // 普通二分法
                bool res = await AutoSetTxEr_MethodA_Async();
                return res;
            }
            //return AutoSetTxEr_MethodB(); // 差值二分法
            //}

            //await Task.Delay(15000);
            //return true;
        }
        #endregion

        #region  // 待测模块消光比自动调试  普通二分法
        private bool AutoSetTxEr_MethodA()
        {
            UInt16 min = (UInt16)TestSet.txmod_Min;
            UInt16 max = (UInt16)TestSet.txmod_Max;
            UInt16 mod = 0;
            float er_target, result_err = 0;
            int looptime = 0;
            er_target = TestSet.txEr_target;
            //lock (tx_lock)
            //    //光开关切换
            //    TestControl.opticalswitch.SetChannel(Dut);
            // 普通二分法查找
            do
            {
                looptime++;

                mod = (UInt16)((min + max) / 2);

                if (mod < 3) return false; // 值异常

                if (test.SetTxModBias(mod) == false) return false;
                if (Get_ERatio_DCA(false) == false) return false;

                switch (Dut)
                {
                    case 1:
                        if (TestResult.txEr > 99)
                        {
                            if (Get_ERatio_DCA(true) == false) return false;
                        }
                        result_err = TestResult.txEr - TestSet.txEr_target;
                        AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult.txEr.ToString() + " txEr_target:" + TestSet.txEr_target.ToString());
                        break;
                    case 2:
                        if (TestResult2.txEr > 99)
                        {
                            if (Get_ERatio_DCA(true) == false) return false;
                        }
                        result_err = TestResult2.txEr - TestSet.txEr_target;
                        AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult2.txEr.ToString() + " txEr_target:" + TestSet.txEr_target.ToString());
                        break;
                    default:
                        break;
                }

                //
                if (result_err > 0)
                {
                    max = (UInt16)(mod - 1);
                }
                else
                {
                    min = (UInt16)(mod + 1);
                }
            } while ((Math.Abs(result_err) > erValMaxErr) && (max > min) && (looptime < 10));

            switch (Dut)
            {
                case 1:
                    TestResult.txErErr = result_err;
                    retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txEr.ToString("F1"); // 界面显示
                    break;
                case 2:
                    TestResult2.txErErr = result_err;
                    retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txEr.ToString("F1"); // 界面显示
                    break;
                default:
                    break;
            }
            //
            if (Math.Abs(result_err) <= erValMaxErr) //(TestResult.txEr <= TestSet.txEr_Max) && (TestResult.txEr >= TestSet.txEr_Min)
            {
                return true;
            }
            else
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    if (result_err > 0) retutntxrxresult.ErrorMessage += "ER：消光比大";
                    if (result_err < 0) retutntxrxresult.ErrorMessage += "ER：消光比小";
                }
                else
                {
                    if (result_err > 0) retutntxrxresult.ErrorMessage += "ER: The extinction ratio is large";//ER：消光比大
                    if (result_err < 0) retutntxrxresult.ErrorMessage += "ER: The extinction ratio is small";//ER：消光比小
                }
                //
                return false;
            }
        }

        #endregion

        #region  // 待测模块消光比自动调试   普通二分法  异步
        private async Task<bool> AutoSetTxEr_MethodA_Async()
        {
            UInt16 min = (UInt16)TestSet.txmod_Min;
            UInt16 max = (UInt16)TestSet.txmod_Max;
            UInt16 mod = 0;
            float er_target, result_err = 0;
            int looptime = 0;
            er_target = TestSet.txEr_target;
            int millisecondsDelay = 2;

            AddTestLog("erValMaxErr:" + erValMaxErr.ToString() + " er_target:" + er_target.ToString() + " MOdmin:" + min.ToString() + " Modmax:" + max.ToString());

            await switchSemaphore.WaitAsync();
            //lock (tx_lock)
            try
            {
                // 普通二分法查找
                do
                {
                    looptime++;

                    mod = (UInt16)((min + max) / 2);

                    if (mod < 3) return false; // 值异常

                    if (test.SetTxModBias(mod) == false) return false;
                    bool res = await Get_ERatio_DCA_Async(false);
                    if (res == false) return false;
                    //if (Get_ERatio_DCA(false) == false) return false;
                    switch (Dut)
                    {
                        case 1:
                            if (TestResult.txEr > 99)
                            {
                                //if (Get_ERatio_DCA(true) == false) return false;
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult.txEr - TestSet.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult.txEr.ToString() + " txEr_target:" + TestSet.txEr_target.ToString());
                            break;
                        case 2:
                            if (TestResult2.txEr > 99)
                            {
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult2.txEr - TestSet.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult2.txEr.ToString() + " txEr_target:" + TestSet.txEr_target.ToString());
                            break;
                        case 3:
                            if (TestResult3.txEr > 99)
                            {
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult3.txEr - TestSet.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult3.txEr.ToString() + " txEr_target:" + TestSet.txEr_target.ToString());
                            break;
                        case 4:
                            if (TestResult4.txEr > 99)
                            {
                                res = await Get_ERatio_DCA_Async(false);
                                if (res == false) return false;
                            }
                            result_err = TestResult4.txEr - TestSet.txEr_target;
                            AddTestLog("mod:" + mod.ToString() + " txEr:" + TestResult4.txEr.ToString() + " txEr_target:" + TestSet.txEr_target.ToString());
                            break;
                        default:
                            break;
                    }

                    //
                    if (result_err > 0)
                    {
                        max = (UInt16)(mod - 1);
                    }
                    else
                    {
                        min = (UInt16)(mod + 1);
                    }
                    await Task.Delay(millisecondsDelay);
                } while ((Math.Abs(result_err) > erValMaxErr) && (max > min) && (looptime < 10));

                retutntxrxresult.mod = mod;
                switch (Dut)
                {
                    case 1:
                        TestResult.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txEr.ToString("F1"); // 界面显示
                        break;
                    case 2:
                        TestResult2.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult2.txEr.ToString("F1"); // 界面显示
                        break;
                    case 3:
                        TestResult3.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult3.txEr.ToString("F1"); // 界面显示
                        break;
                    case 4:
                        TestResult4.txErErr = result_err;
                        retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult4.txEr.ToString("F1"); // 界面显示
                        break;
                    default:
                        break;
                }
                //
                if (Math.Abs(result_err) <= erValMaxErr) //(TestResult.txEr <= TestSet.txEr_Max) && (TestResult.txEr >= TestSet.txEr_Min)
                {
                    return true;
                }
                else
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (result_err > 0) retutntxrxresult.ErrorMessage += "ER：消光比大";
                        if (result_err < 0) retutntxrxresult.ErrorMessage += "ER：消光比小";
                    }
                    else
                    {
                        if (result_err > 0) retutntxrxresult.ErrorMessage += "ER: The extinction ratio is large";//ER：消光比大
                        if (result_err < 0) retutntxrxresult.ErrorMessage += "ER: The extinction ratio is small";//ER：消光比小
                    }
                    //
                    return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                //释放
                switchSemaphore.Release();
            }
        }
        #endregion

        #region // 待测模块消光比自动调试  普通二分法
        private bool AutoSetTxEr_MethodE()
        {
            UInt16 apc_min = 0;
            UInt16 apc_max = 0;
            UInt16 mod_min = 0;
            UInt16 mod_max = 0;
            float er_err = 0, er_target, bias, pwr;
            int looptime = 0;
            //int ch = TestSet.ch;
            bool b_rtn = true;
            UInt16 apc_step, mod_step;
            float fk_apc = 0.6f;//0.06f;
            float fk_mod = 0.5f;//0.05
            UInt16 apc, mod;

            switch (Dut)
            {
                case 1:
                    apc_min = TestSet.txapc_Min;
                    apc_max = TestSet.txapc_Max;
                    mod_min = TestSet.txmod_Min;
                    mod_max = TestSet.txmod_Max;
                    er_target = TestSet.txEr_target;
                    break;
                case 2:
                    apc_min = TestSet2.txapc_Min;
                    apc_max = TestSet2.txapc_Max;
                    mod_min = TestSet2.txmod_Min;
                    mod_max = TestSet2.txmod_Max;
                    er_target = TestSet2.txEr_target;
                    break;
                default:
                    break;
            }


            mod = (UInt16)((mod_min + mod_max) / 2);
            if (test.SetTxModBias(mod) == false) return false;

            // 普通二分法查找
            looptime = 0;
            do
            {
                apc = (UInt16)((apc_min + apc_max) / 2);
                if (test.SetTxApcBias(apc) == false) return false;

                if ((looptime % 3) == 0)
                {
                    if (Get_ERatio_DCA(true) == false) return false; //AUTO
                }
                else
                {
                    if (Get_ERatio_DCA(false) == false) return false;
                }
                looptime++;
                switch (Dut)
                {
                    case 1:
                        er_err = TestResult.txEr - TestSet.txEr_target;
                        break;
                    case 2:
                        er_err = TestResult2.txEr - TestSet2.txEr_target;
                        break;
                    default:
                        break;
                }

                if (er_err > 0)
                {
                    apc_min = (UInt16)(apc + 2);
                }
                else
                {
                    apc_max = (UInt16)(apc - 2);
                }
            } while ((Math.Abs(er_err) > erValMaxErr) && (apc_max > apc_min) && (looptime < 8));
            switch (Dut)
            {
                case 1:
                    apc_min = TestSet.txapc_Min;
                    apc_max = TestSet.txapc_Max;
                    break;
                case 2:
                    apc_min = TestSet2.txapc_Min;
                    apc_max = TestSet2.txapc_Max;
                    break;
                default:
                    break;
            }
            switch (Dut)
            {
                case 1:
                    meter_err = TestSet.meter_pwr_err;
                    break;
                case 2:
                    meter_err = TestSet2.meter_pwr_err;
                    break;
            }
            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
            bias = test.GetTxBias();

            if (Math.Abs(er_err) > erValMaxErr)
            {
                retutntxrxresult.ErrorMessage += "Tx消光比ER调试异常: ";
                b_rtn = false;
                goto RTN_POS;
            }

            apc_step = (UInt16)((float)(apc * fk_apc) + 0.5f); //四舍五入法
            if (apc_step < 1) apc_step = 1;
            if (apc_step > 5) apc_step = 5;

            mod_step = (UInt16)((float)(mod * fk_mod) + 0.5f); //四舍五入法
            if (mod_step < 1) mod_step = 1;
            if (mod_step > 5) mod_step = 5;

            retutntxrxresult.ErrorMessage = "";

            //优化调试Tx发光功率和消光比ER
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            switch (Dut)
            {
                case 1: // 1.发光小于最小值
                    if (pwr <= TestSet.txPwr_Min)
                    {
                        if (bias > TestSet.bias_Max) //Bias大于最大值
                        {
                            retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                            b_rtn = false;
                            goto RTN_POS;
                        }

                        retutntxrxresult.ErrorMessage = " 微调 1 \r";

                        er_target = TestSet.txEr_target;
                        looptime = 0;
                        do
                        {
                            apc += apc_step;
                            if (test.SetTxApcBias(apc) == false) return false;

                            for (int k = 0; k < 4; k++)
                            {
                                mod += mod_step;
                                if (test.SetTxModBias(mod) == false) return false;
                                if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                if (TestResult.txEr >= er_target)
                                {
                                    break; //跳出循环
                                }
                            }

                            er_err = TestResult.txEr - TestSet.txEr_target;

                            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            bias = test.GetTxBias();
                            looptime++;

                        } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));

                        goto RTN_POS;
                    }
                    // 2.发光大于最大值
                    else if (pwr >= TestSet.txPwr_Max)
                    {
                        if (bias < TestSet.bias_Min) //Bias小于最小值
                        {
                            retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                            b_rtn = false;
                            goto RTN_POS;
                        }

                        retutntxrxresult.ErrorMessage = " 微调 2 \r";

                        er_target = TestSet.txEr_target;
                        looptime = 0;
                        do
                        {
                            apc -= apc_step;
                            if (test.SetTxApcBias(apc) == false) return false;

                            for (int k = 0; k < 4; k++)
                            {
                                mod -= mod_step;
                                if (test.SetTxModBias(mod) == false) return false;
                                if (Get_ERatio_DCA(false) == false) return false;
                                if (TestResult.txEr <= er_target)
                                {
                                    break; //跳出循环
                                }
                            }

                            er_err = TestResult.txEr - TestSet.txEr_target;
                            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            bias = test.GetTxBias();
                            looptime++;
                        } while ((pwr > TestSet.txPwr_target) && (bias > TestSet.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                        goto RTN_POS;
                    }
                    // 3.发光正常偏大
                    else if ((pwr > TestSet.txPwr_target) && (pwr < TestSet.txPwr_Max))
                    {
                        if (bias > (TestSet.txBias_target + TestSet.bias_Max) / 2)
                        {
                            retutntxrxresult.ErrorMessage = " 微调 30 \r";
                            er_target = (TestSet.txEr_target + TestSet.txEr_Max) / 2;
                            looptime = 0;
                            do
                            {
                                apc -= apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    if (k > 0)
                                    {
                                        mod -= mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                    }
                                    if (Get_ERatio_DCA(false) == false) return false;
                                    if (TestResult.txEr <= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult.txEr - TestSet.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;
                            } while ((pwr > TestSet.txPwr_target) && (bias > TestSet.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 3));
                        }
                        else // (bias <= TestSet.txBias_target)
                        {
                            if (bias > TestSet.bias_Min)
                            {
                                b_rtn = true;
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 31 \r";

                            er_target = (TestSet.txEr_target + TestSet.txEr_Min) / 2;
                            looptime = 0;
                            do
                            {
                                apc += apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod += mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                    if (TestResult.txEr >= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult.txEr - TestSet.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;

                            } while ((pwr < TestSet.txPwr_Max) && (bias < (TestSet.bias_Min * 1.03f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                        }
                        goto RTN_POS;
                    }
                    // 4.发光正常偏小
                    else if ((pwr <= TestSet.txPwr_target) && (pwr > TestSet.txPwr_Min))
                    {
                        if (bias > TestSet.txBias_target)
                        {
                            if ((bias < TestSet.bias_Max) && (bias > (TestSet.bias_Max + TestSet.txBias_target) / 2))
                            {
                                b_rtn = true;
                                goto RTN_POS;
                            }
                            else if (bias < (TestSet.bias_Max + TestSet.txBias_target) / 2)
                            {
                                retutntxrxresult.ErrorMessage = " 微调 40 \r";
                                er_target = TestSet.txEr_Min + 0.1f;

                                if (bias > TestSet.txBias_target)
                                {
                                    looptime = 0;
                                    do
                                    {
                                        apc += apc_step;
                                        if (test.SetTxApcBias(apc) == false) return false;
                                        for (int k = 0; k < 4; k++)
                                        {
                                            if (k > 0)
                                            {
                                                mod += mod_step;
                                                if (test.SetTxModBias(mod) == false) return false;
                                            }
                                            if (Get_ERatio_DCA((bool)(k == 3)) == false) return false;
                                            if (TestResult.txEr >= er_target)
                                            {
                                                break; //跳出循环
                                            }
                                        }
                                        looptime++;
                                        er_err = TestResult.txEr - TestSet.txEr_target;
                                        pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                        bias = test.GetTxBias();
                                    } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 3));
                                    goto RTN_POS;
                                }

                                retutntxrxresult.ErrorMessage = " 微调 41 \r";
                                looptime = 0;
                                do
                                {
                                    apc += apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        if (k > 0)
                                        {
                                            mod += mod_step;
                                            if (test.SetTxModBias(mod) == false) return false;
                                        }
                                        if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                        if (TestResult.txEr >= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult.txEr - TestSet.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                                //
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 42 \r";
                            er_target = (TestSet.txEr_target + TestSet.txEr_Max) / 2;
                            looptime = 0;
                            do
                            {
                                apc -= apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod -= mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA(false) == false) return false;
                                    if (TestResult.txEr <= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult.txEr - TestSet.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;
                            } while ((pwr > TestSet.txPwr_Min) && (bias > TestSet.bias_Max) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                        }
                        else // (bias <= TestSet.txBias_target)
                        {
                            retutntxrxresult.ErrorMessage = " 微调 43 \r";
                            er_target = (TestSet.txEr_target + TestSet.txEr_Min) / 2;
                            looptime = 0;
                            do
                            {
                                apc += apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod += mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                    if (TestResult.txEr >= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult.txEr - TestSet.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;

                            } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                        }
                        goto RTN_POS;
                    }
                    else
                    {
                        //无操作
                    }
                    break;
                case 2: // 1.发光小于最小值
                    if (pwr <= TestSet2.txPwr_Min)
                    {
                        if (bias > TestSet2.bias_Max) //Bias大于最大值
                        {
                            retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                            b_rtn = false;
                            goto RTN_POS;
                        }

                        retutntxrxresult.ErrorMessage = " 微调 1 \r";

                        er_target = TestSet2.txEr_target;
                        looptime = 0;
                        do
                        {
                            apc += apc_step;
                            if (test.SetTxApcBias(apc) == false) return false;

                            for (int k = 0; k < 4; k++)
                            {
                                mod += mod_step;
                                if (test.SetTxModBias(mod) == false) return false;
                                if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                if (TestResult2.txEr >= er_target)
                                {
                                    break; //跳出循环
                                }
                            }

                            er_err = TestResult2.txEr - TestSet2.txEr_target;

                            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            bias = test.GetTxBias();
                            looptime++;

                        } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));

                        goto RTN_POS;
                    }
                    // 2.发光大于最大值
                    else if (pwr >= TestSet2.txPwr_Max)
                    {
                        if (bias < TestSet2.bias_Min) //Bias小于最小值
                        {
                            retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                            b_rtn = false;
                            goto RTN_POS;
                        }

                        retutntxrxresult.ErrorMessage = " 微调 2 \r";

                        er_target = TestSet2.txEr_target;
                        looptime = 0;
                        do
                        {
                            apc -= apc_step;
                            if (test.SetTxApcBias(apc) == false) return false;

                            for (int k = 0; k < 4; k++)
                            {
                                mod -= mod_step;
                                if (test.SetTxModBias(mod) == false) return false;
                                if (Get_ERatio_DCA(false) == false) return false;
                                if (TestResult2.txEr <= er_target)
                                {
                                    break; //跳出循环
                                }
                            }

                            er_err = TestResult2.txEr - TestSet2.txEr_target;
                            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                            bias = test.GetTxBias();
                            looptime++;
                        } while ((pwr > TestSet2.txPwr_target) && (bias > TestSet2.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                        goto RTN_POS;
                    }
                    // 3.发光正常偏大
                    else if ((pwr > TestSet2.txPwr_target) && (pwr < TestSet2.txPwr_Max))
                    {
                        if (bias > (TestSet2.txBias_target + TestSet2.bias_Max) / 2)
                        {
                            retutntxrxresult.ErrorMessage = " 微调 30 \r";
                            er_target = (TestSet2.txEr_target + TestSet2.txEr_Max) / 2;
                            looptime = 0;
                            do
                            {
                                apc -= apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    if (k > 0)
                                    {
                                        mod -= mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                    }
                                    if (Get_ERatio_DCA(false) == false) return false;
                                    if (TestResult2.txEr <= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult2.txEr - TestSet2.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;
                            } while ((pwr > TestSet2.txPwr_target) && (bias > TestSet2.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 3));
                        }
                        else // (bias <= TestSet.txBias_target)
                        {
                            if (bias > TestSet2.bias_Min)
                            {
                                b_rtn = true;
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 31 \r";

                            er_target = (TestSet2.txEr_target + TestSet2.txEr_Min) / 2;
                            looptime = 0;
                            do
                            {
                                apc += apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod += mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                    if (TestResult2.txEr >= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult2.txEr - TestSet2.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;

                            } while ((pwr < TestSet2.txPwr_Max) && (bias < (TestSet2.bias_Min * 1.03f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                        }
                        goto RTN_POS;
                    }
                    // 4.发光正常偏小
                    else if ((pwr <= TestSet2.txPwr_target) && (pwr > TestSet2.txPwr_Min))
                    {
                        if (bias > TestSet2.txBias_target)
                        {
                            if ((bias < TestSet2.bias_Max) && (bias > (TestSet2.bias_Max + TestSet2.txBias_target) / 2))
                            {
                                b_rtn = true;
                                goto RTN_POS;
                            }
                            else if (bias < (TestSet2.bias_Max + TestSet2.txBias_target) / 2)
                            {
                                retutntxrxresult.ErrorMessage = " 微调 40 \r";
                                er_target = TestSet2.txEr_Min + 0.1f;

                                if (bias > TestSet2.txBias_target)
                                {
                                    looptime = 0;
                                    do
                                    {
                                        apc += apc_step;
                                        if (test.SetTxApcBias(apc) == false) return false;
                                        for (int k = 0; k < 4; k++)
                                        {
                                            if (k > 0)
                                            {
                                                mod += mod_step;
                                                if (test.SetTxModBias(mod) == false) return false;
                                            }
                                            if (Get_ERatio_DCA((bool)(k == 3)) == false) return false;
                                            if (TestResult2.txEr >= er_target)
                                            {
                                                break; //跳出循环
                                            }
                                        }
                                        looptime++;
                                        er_err = TestResult2.txEr - TestSet2.txEr_target;
                                        pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                        bias = test.GetTxBias();
                                    } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 3));
                                    goto RTN_POS;
                                }

                                retutntxrxresult.ErrorMessage = " 微调 41 \r";
                                looptime = 0;
                                do
                                {
                                    apc += apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        if (k > 0)
                                        {
                                            mod += mod_step;
                                            if (test.SetTxModBias(mod) == false) return false;
                                        }
                                        if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                        if (TestResult2.txEr >= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult2.txEr - TestSet2.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                                //
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 42 \r";
                            er_target = (TestSet2.txEr_target + TestSet2.txEr_Max) / 2;
                            looptime = 0;
                            do
                            {
                                apc -= apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod -= mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA(false) == false) return false;
                                    if (TestResult2.txEr <= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult2.txEr - TestSet2.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;
                            } while ((pwr > TestSet2.txPwr_Min) && (bias > TestSet2.bias_Max) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                        }
                        else // (bias <= TestSet.txBias_target)
                        {
                            retutntxrxresult.ErrorMessage = " 微调 43 \r";
                            er_target = (TestSet2.txEr_target + TestSet2.txEr_Min) / 2;
                            looptime = 0;
                            do
                            {
                                apc += apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod += mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                    if (TestResult2.txEr >= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult2.txEr - TestSet2.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;

                            } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                        }
                        goto RTN_POS;
                    }
                    else
                    {
                        //无操作
                    }
                    break;
                default:
                    break;
            }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        RTN_POS:
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    TestResult.txBiasDDM = bias;
                    TestResult.txErErr = er_err;
                    //界面显示
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txEr.ToString("F1");

                    retutntxrxresult.apc = apc;
                    retutntxrxresult.mod = mod;

                    //if (bias > TestSet.bias_Max)
                    if (bias > (TestSet.bias_Max * 1.02f))
                    {
                        retutntxrxresult.ErrorMessage += "Bias大 ";
                        b_rtn = false;
                    }
                    if (bias < TestSet.bias_Min)
                    {
                        retutntxrxresult.ErrorMessage += "Bias小 ";
                        b_rtn = false;
                    }
                    if (pwr < TestSet.txPwr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "发光小 ";
                        b_rtn = false;
                    }
                    if (pwr > TestSet.txPwr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "发光大 ";
                        b_rtn = false;
                    }
                    if (TestResult.txEr < TestSet.txEr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER小 ";
                        b_rtn = false;
                    }
                    if (TestResult.txEr > TestSet.txEr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER大 ";
                        b_rtn = false;
                    }

                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    TestResult2.txBiasDDM = bias;
                    TestResult2.txErErr = er_err;
                    //界面显示
                    retutntxrxresult.TxpwrResultShow = TestSet2.txPwr_target.ToString("F1") + "/" + TestResult2.txPower.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet2.txBias_target.ToString("F1") + "/" + TestResult2.txBiasDDM.ToString("F1");
                    retutntxrxresult.TxerResultShow = TestSet2.txEr_target.ToString("F1") + "/" + TestResult2.txEr.ToString("F1");

                    retutntxrxresult.apc = apc;
                    retutntxrxresult.mod = mod;

                    //if (bias > TestSet.bias_Max)
                    if (bias > (TestSet2.bias_Max * 1.02f))
                    {
                        retutntxrxresult.ErrorMessage += "Bias大 ";
                        b_rtn = false;
                    }
                    if (bias < TestSet2.bias_Min)
                    {
                        retutntxrxresult.ErrorMessage += "Bias小 ";
                        b_rtn = false;
                    }
                    if (pwr < TestSet2.txPwr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "发光小 ";
                        b_rtn = false;
                    }
                    if (pwr > TestSet2.txPwr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "发光大 ";
                        b_rtn = false;
                    }
                    if (TestResult2.txEr < TestSet2.txEr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER小 ";
                        b_rtn = false;
                    }
                    if (TestResult2.txEr > TestSet2.txEr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER大 ";
                        b_rtn = false;
                    }

                    break;
                default:
                    break;
            }

            return b_rtn;
        }
        #endregion

        #region //待测模块消光比自动调试  普通二分法 异步
        private async Task<bool> AutoSetTxEr_MethodE_Async()
        {
            UInt16 apc_min = 0;
            UInt16 apc_max = 0;
            UInt16 mod_min = 0;
            UInt16 mod_max = 0;
            float er_err = 0, er_target, bias, pwr;
            int looptime = 0;
            //int ch = TestSet.ch;
            bool b_rtn = true;
            UInt16 apc_step, mod_step;
            float fk_apc = 0.6f;//0.06f;
            float fk_mod = 0.5f;//0.05
            UInt16 apc, mod;

            switch (Dut)
            {
                case 1:
                    apc_min = TestSet.txapc_Min;
                    apc_max = TestSet.txapc_Max;
                    mod_min = TestSet.txmod_Min;
                    mod_max = TestSet.txmod_Max;
                    er_target = TestSet.txEr_target;
                    break;
                case 2:
                    apc_min = TestSet2.txapc_Min;
                    apc_max = TestSet2.txapc_Max;
                    mod_min = TestSet2.txmod_Min;
                    mod_max = TestSet2.txmod_Max;
                    er_target = TestSet2.txEr_target;
                    break;
                default:
                    break;
            }


            mod = (UInt16)((mod_min + mod_max) / 2);
            if (test.SetTxModBias(mod) == false) return false;

            await switchSemaphore.WaitAsync();
            try
            {
                //光开关切换
                TestControl.opticalswitch.SetChannel(Dut);
                // 普通二分法查找
                looptime = 0;
                do
                {
                    apc = (UInt16)((apc_min + apc_max) / 2);
                    if (test.SetTxApcBias(apc) == false) return false;

                    if ((looptime % 3) == 0)
                    {
                        if (Get_ERatio_DCA(true) == false) return false; //AUTO
                    }
                    else
                    {
                        if (Get_ERatio_DCA(false) == false) return false;
                    }
                    looptime++;
                    switch (Dut)
                    {
                        case 1:
                            er_err = TestResult.txEr - TestSet.txEr_target;
                            break;
                        case 2:
                            er_err = TestResult2.txEr - TestSet2.txEr_target;
                            break;
                        default:
                            break;
                    }

                    if (er_err > 0)
                    {
                        apc_min = (UInt16)(apc + 2);
                    }
                    else
                    {
                        apc_max = (UInt16)(apc - 2);
                    }
                    await Task.Delay(waittimes);
                } while ((Math.Abs(er_err) > erValMaxErr) && (apc_max > apc_min) && (looptime < 8));
                switch (Dut)
                {
                    case 1:
                        apc_min = TestSet.txapc_Min;
                        apc_max = TestSet.txapc_Max;
                        break;
                    case 2:
                        apc_min = TestSet2.txapc_Min;
                        apc_max = TestSet2.txapc_Max;
                        break;
                    default:
                        break;
                }
                switch (Dut)
                {
                    case 1:
                        meter_err = TestSet.meter_pwr_err;
                        break;
                    case 2:
                        meter_err = TestSet2.meter_pwr_err;
                        break;
                }
                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                bias = test.GetTxBias();

                if (Math.Abs(er_err) > erValMaxErr)
                {
                    retutntxrxresult.ErrorMessage += "Tx消光比ER调试异常: ";
                    b_rtn = false;
                    goto RTN_POS;
                }

                apc_step = (UInt16)((float)(apc * fk_apc) + 0.5f); //四舍五入法
                if (apc_step < 1) apc_step = 1;
                if (apc_step > 5) apc_step = 5;

                mod_step = (UInt16)((float)(mod * fk_mod) + 0.5f); //四舍五入法
                if (mod_step < 1) mod_step = 1;
                if (mod_step > 5) mod_step = 5;

                retutntxrxresult.ErrorMessage = "";

                //优化调试Tx发光功率和消光比ER
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                switch (Dut)
                {
                    case 1: // 1.发光小于最小值
                        if (pwr <= TestSet.txPwr_Min)
                        {
                            if (bias > TestSet.bias_Max) //Bias大于最大值
                            {
                                retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                                b_rtn = false;
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 1 \r";

                            er_target = TestSet.txEr_target;
                            looptime = 0;
                            do
                            {
                                apc += apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod += mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                    if (TestResult.txEr >= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult.txEr - TestSet.txEr_target;

                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;
                                await Task.Delay(waittimes);
                            } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));

                            goto RTN_POS;
                        }
                        // 2.发光大于最大值
                        else if (pwr >= TestSet.txPwr_Max)
                        {
                            if (bias < TestSet.bias_Min) //Bias小于最小值
                            {
                                retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                                b_rtn = false;
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 2 \r";

                            er_target = TestSet.txEr_target;
                            looptime = 0;
                            do
                            {
                                apc -= apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod -= mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA(false) == false) return false;
                                    if (TestResult.txEr <= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult.txEr - TestSet.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;
                                await Task.Delay(waittimes);
                            } while ((pwr > TestSet.txPwr_target) && (bias > TestSet.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                            goto RTN_POS;
                        }
                        // 3.发光正常偏大
                        else if ((pwr > TestSet.txPwr_target) && (pwr < TestSet.txPwr_Max))
                        {
                            if (bias > (TestSet.txBias_target + TestSet.bias_Max) / 2)
                            {
                                retutntxrxresult.ErrorMessage = " 微调 30 \r";
                                er_target = (TestSet.txEr_target + TestSet.txEr_Max) / 2;
                                looptime = 0;
                                do
                                {
                                    apc -= apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        if (k > 0)
                                        {
                                            mod -= mod_step;
                                            if (test.SetTxModBias(mod) == false) return false;
                                        }
                                        if (Get_ERatio_DCA(false) == false) return false;
                                        if (TestResult.txEr <= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult.txEr - TestSet.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                    await Task.Delay(waittimes);
                                } while ((pwr > TestSet.txPwr_target) && (bias > TestSet.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 3));
                            }
                            else // (bias <= TestSet.txBias_target)
                            {
                                if (bias > TestSet.bias_Min)
                                {
                                    b_rtn = true;
                                    goto RTN_POS;
                                }

                                retutntxrxresult.ErrorMessage = " 微调 31 \r";

                                er_target = (TestSet.txEr_target + TestSet.txEr_Min) / 2;
                                looptime = 0;
                                do
                                {
                                    apc += apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        mod += mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                        if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                        if (TestResult.txEr >= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult.txEr - TestSet.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                    await Task.Delay(waittimes);
                                } while ((pwr < TestSet.txPwr_Max) && (bias < (TestSet.bias_Min * 1.03f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                            }
                            goto RTN_POS;
                        }
                        // 4.发光正常偏小
                        else if ((pwr <= TestSet.txPwr_target) && (pwr > TestSet.txPwr_Min))
                        {
                            if (bias > TestSet.txBias_target)
                            {
                                if ((bias < TestSet.bias_Max) && (bias > (TestSet.bias_Max + TestSet.txBias_target) / 2))
                                {
                                    b_rtn = true;
                                    goto RTN_POS;
                                }
                                else if (bias < (TestSet.bias_Max + TestSet.txBias_target) / 2)
                                {
                                    retutntxrxresult.ErrorMessage = " 微调 40 \r";
                                    er_target = TestSet.txEr_Min + 0.1f;

                                    if (bias > TestSet.txBias_target)
                                    {
                                        looptime = 0;
                                        do
                                        {
                                            apc += apc_step;
                                            if (test.SetTxApcBias(apc) == false) return false;
                                            for (int k = 0; k < 4; k++)
                                            {
                                                if (k > 0)
                                                {
                                                    mod += mod_step;
                                                    if (test.SetTxModBias(mod) == false) return false;
                                                }
                                                if (Get_ERatio_DCA((bool)(k == 3)) == false) return false;
                                                if (TestResult.txEr >= er_target)
                                                {
                                                    break; //跳出循环
                                                }
                                            }
                                            looptime++;
                                            er_err = TestResult.txEr - TestSet.txEr_target;
                                            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                            bias = test.GetTxBias();
                                            await Task.Delay(waittimes);
                                        } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 3));
                                        goto RTN_POS;
                                    }

                                    retutntxrxresult.ErrorMessage = " 微调 41 \r";
                                    looptime = 0;
                                    do
                                    {
                                        apc += apc_step;
                                        if (test.SetTxApcBias(apc) == false) return false;

                                        for (int k = 0; k < 4; k++)
                                        {
                                            if (k > 0)
                                            {
                                                mod += mod_step;
                                                if (test.SetTxModBias(mod) == false) return false;
                                            }
                                            if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                            if (TestResult.txEr >= er_target)
                                            {
                                                break; //跳出循环
                                            }
                                        }

                                        er_err = TestResult.txEr - TestSet.txEr_target;
                                        pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                        bias = test.GetTxBias();
                                        looptime++;
                                        await Task.Delay(waittimes);
                                    } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                                    //
                                    goto RTN_POS;
                                }

                                retutntxrxresult.ErrorMessage = " 微调 42 \r";
                                er_target = (TestSet.txEr_target + TestSet.txEr_Max) / 2;
                                looptime = 0;
                                do
                                {
                                    apc -= apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        mod -= mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                        if (Get_ERatio_DCA(false) == false) return false;
                                        if (TestResult.txEr <= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult.txEr - TestSet.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                    await Task.Delay(waittimes);
                                } while ((pwr > TestSet.txPwr_Min) && (bias > TestSet.bias_Max) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                            }
                            else // (bias <= TestSet.txBias_target)
                            {
                                retutntxrxresult.ErrorMessage = " 微调 43 \r";
                                er_target = (TestSet.txEr_target + TestSet.txEr_Min) / 2;
                                looptime = 0;
                                do
                                {
                                    apc += apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        mod += mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                        if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                        if (TestResult.txEr >= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult.txEr - TestSet.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                    await Task.Delay(waittimes);
                                } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                            }
                            goto RTN_POS;
                        }
                        else
                        {
                            //无操作
                        }
                        break;
                    case 2: // 1.发光小于最小值
                        if (pwr <= TestSet2.txPwr_Min)
                        {
                            if (bias > TestSet2.bias_Max) //Bias大于最大值
                            {
                                retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                                b_rtn = false;
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 1 \r";

                            er_target = TestSet2.txEr_target;
                            looptime = 0;
                            do
                            {
                                apc += apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod += mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                    if (TestResult2.txEr >= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult2.txEr - TestSet2.txEr_target;

                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;

                            } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));

                            goto RTN_POS;
                        }
                        // 2.发光大于最大值
                        else if (pwr >= TestSet2.txPwr_Max)
                        {
                            if (bias < TestSet2.bias_Min) //Bias小于最小值
                            {
                                retutntxrxresult.ErrorMessage += "Tx调试异常: ";
                                b_rtn = false;
                                goto RTN_POS;
                            }

                            retutntxrxresult.ErrorMessage = " 微调 2 \r";

                            er_target = TestSet2.txEr_target;
                            looptime = 0;
                            do
                            {
                                apc -= apc_step;
                                if (test.SetTxApcBias(apc) == false) return false;

                                for (int k = 0; k < 4; k++)
                                {
                                    mod -= mod_step;
                                    if (test.SetTxModBias(mod) == false) return false;
                                    if (Get_ERatio_DCA(false) == false) return false;
                                    if (TestResult2.txEr <= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }

                                er_err = TestResult2.txEr - TestSet2.txEr_target;
                                pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                bias = test.GetTxBias();
                                looptime++;
                            } while ((pwr > TestSet2.txPwr_target) && (bias > TestSet2.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                            goto RTN_POS;
                        }
                        // 3.发光正常偏大
                        else if ((pwr > TestSet2.txPwr_target) && (pwr < TestSet2.txPwr_Max))
                        {
                            if (bias > (TestSet2.txBias_target + TestSet2.bias_Max) / 2)
                            {
                                retutntxrxresult.ErrorMessage = " 微调 30 \r";
                                er_target = (TestSet2.txEr_target + TestSet2.txEr_Max) / 2;
                                looptime = 0;
                                do
                                {
                                    apc -= apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        if (k > 0)
                                        {
                                            mod -= mod_step;
                                            if (test.SetTxModBias(mod) == false) return false;
                                        }
                                        if (Get_ERatio_DCA(false) == false) return false;
                                        if (TestResult2.txEr <= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult2.txEr - TestSet2.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                } while ((pwr > TestSet2.txPwr_target) && (bias > TestSet2.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 3));
                            }
                            else // (bias <= TestSet.txBias_target)
                            {
                                if (bias > TestSet2.bias_Min)
                                {
                                    b_rtn = true;
                                    goto RTN_POS;
                                }

                                retutntxrxresult.ErrorMessage = " 微调 31 \r";

                                er_target = (TestSet2.txEr_target + TestSet2.txEr_Min) / 2;
                                looptime = 0;
                                do
                                {
                                    apc += apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        mod += mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                        if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                        if (TestResult2.txEr >= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult2.txEr - TestSet2.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;

                                } while ((pwr < TestSet2.txPwr_Max) && (bias < (TestSet2.bias_Min * 1.03f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                            }
                            goto RTN_POS;
                        }
                        // 4.发光正常偏小
                        else if ((pwr <= TestSet2.txPwr_target) && (pwr > TestSet2.txPwr_Min))
                        {
                            if (bias > TestSet2.txBias_target)
                            {
                                if ((bias < TestSet2.bias_Max) && (bias > (TestSet2.bias_Max + TestSet2.txBias_target) / 2))
                                {
                                    b_rtn = true;
                                    goto RTN_POS;
                                }
                                else if (bias < (TestSet2.bias_Max + TestSet2.txBias_target) / 2)
                                {
                                    retutntxrxresult.ErrorMessage = " 微调 40 \r";
                                    er_target = TestSet2.txEr_Min + 0.1f;

                                    if (bias > TestSet2.txBias_target)
                                    {
                                        looptime = 0;
                                        do
                                        {
                                            apc += apc_step;
                                            if (test.SetTxApcBias(apc) == false) return false;
                                            for (int k = 0; k < 4; k++)
                                            {
                                                if (k > 0)
                                                {
                                                    mod += mod_step;
                                                    if (test.SetTxModBias(mod) == false) return false;
                                                }
                                                if (Get_ERatio_DCA((bool)(k == 3)) == false) return false;
                                                if (TestResult2.txEr >= er_target)
                                                {
                                                    break; //跳出循环
                                                }
                                            }
                                            looptime++;
                                            er_err = TestResult2.txEr - TestSet2.txEr_target;
                                            pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                            bias = test.GetTxBias();
                                        } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 3));
                                        goto RTN_POS;
                                    }

                                    retutntxrxresult.ErrorMessage = " 微调 41 \r";
                                    looptime = 0;
                                    do
                                    {
                                        apc += apc_step;
                                        if (test.SetTxApcBias(apc) == false) return false;

                                        for (int k = 0; k < 4; k++)
                                        {
                                            if (k > 0)
                                            {
                                                mod += mod_step;
                                                if (test.SetTxModBias(mod) == false) return false;
                                            }
                                            if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                            if (TestResult2.txEr >= er_target)
                                            {
                                                break; //跳出循环
                                            }
                                        }

                                        er_err = TestResult2.txEr - TestSet2.txEr_target;
                                        pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                        bias = test.GetTxBias();
                                        looptime++;
                                    } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                                    //
                                    goto RTN_POS;
                                }

                                retutntxrxresult.ErrorMessage = " 微调 42 \r";
                                er_target = (TestSet2.txEr_target + TestSet2.txEr_Max) / 2;
                                looptime = 0;
                                do
                                {
                                    apc -= apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        mod -= mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                        if (Get_ERatio_DCA(false) == false) return false;
                                        if (TestResult2.txEr <= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult2.txEr - TestSet2.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;
                                } while ((pwr > TestSet2.txPwr_Min) && (bias > TestSet2.bias_Max) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                            }
                            else // (bias <= TestSet.txBias_target)
                            {
                                retutntxrxresult.ErrorMessage = " 微调 43 \r";
                                er_target = (TestSet2.txEr_target + TestSet2.txEr_Min) / 2;
                                looptime = 0;
                                do
                                {
                                    apc += apc_step;
                                    if (test.SetTxApcBias(apc) == false) return false;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        mod += mod_step;
                                        if (test.SetTxModBias(mod) == false) return false;
                                        if (Get_ERatio_DCA((bool)(k == 3 || looptime == 2)) == false) return false;
                                        if (TestResult2.txEr >= er_target)
                                        {
                                            break; //跳出循环
                                        }
                                    }

                                    er_err = TestResult2.txEr - TestSet2.txEr_target;
                                    pwr = opticalmeter.ReadPower(meter_ch, GlobalVarFun.setup.meter_delay) + meter_err;
                                    bias = test.GetTxBias();
                                    looptime++;

                                } while ((pwr < TestSet2.txPwr_target) && (bias < (TestSet2.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                            }
                            goto RTN_POS;
                        }
                        else
                        {
                            //无操作
                        }
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                switchSemaphore.Release();
            }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        RTN_POS:
            switch (Dut)
            {
                case 1:
                    TestResult.txPower = pwr;
                    TestResult.txBiasDDM = bias;
                    TestResult.txErErr = er_err;
                    //界面显示
                    retutntxrxresult.TxpwrResultShow = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    retutntxrxresult.TxerResultShow = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txEr.ToString("F1");

                    retutntxrxresult.apc = apc;
                    retutntxrxresult.mod = mod;

                    //if (bias > TestSet.bias_Max)
                    if (bias > (TestSet.bias_Max * 1.02f))
                    {
                        retutntxrxresult.ErrorMessage += "Bias大 ";
                        b_rtn = false;
                    }
                    if (bias < TestSet.bias_Min)
                    {
                        retutntxrxresult.ErrorMessage += "Bias小 ";
                        b_rtn = false;
                    }
                    if (pwr < TestSet.txPwr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "发光小 ";
                        b_rtn = false;
                    }
                    if (pwr > TestSet.txPwr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "发光大 ";
                        b_rtn = false;
                    }
                    if (TestResult.txEr < TestSet.txEr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER小 ";
                        b_rtn = false;
                    }
                    if (TestResult.txEr > TestSet.txEr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER大 ";
                        b_rtn = false;
                    }

                    break;
                case 2:
                    TestResult2.txPower = pwr;
                    TestResult2.txBiasDDM = bias;
                    TestResult2.txErErr = er_err;
                    //界面显示
                    retutntxrxresult.TxpwrResultShow = TestSet2.txPwr_target.ToString("F1") + "/" + TestResult2.txPower.ToString("F1");
                    retutntxrxresult.TxBiasResultShow = TestSet2.txBias_target.ToString("F1") + "/" + TestResult2.txBiasDDM.ToString("F1");
                    retutntxrxresult.TxerResultShow = TestSet2.txEr_target.ToString("F1") + "/" + TestResult2.txEr.ToString("F1");

                    retutntxrxresult.apc = apc;
                    retutntxrxresult.mod = mod;

                    //if (bias > TestSet.bias_Max)
                    if (bias > (TestSet2.bias_Max * 1.02f))
                    {
                        retutntxrxresult.ErrorMessage += "Bias大 ";
                        b_rtn = false;
                    }
                    if (bias < TestSet2.bias_Min)
                    {
                        retutntxrxresult.ErrorMessage += "Bias小 ";
                        b_rtn = false;
                    }
                    if (pwr < TestSet2.txPwr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "发光小 ";
                        b_rtn = false;
                    }
                    if (pwr > TestSet2.txPwr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "发光大 ";
                        b_rtn = false;
                    }
                    if (TestResult2.txEr < TestSet2.txEr_Min)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER小 ";
                        b_rtn = false;
                    }
                    if (TestResult2.txEr > TestSet2.txEr_Max)
                    {
                        retutntxrxresult.ErrorMessage += "消光比ER大 ";
                        b_rtn = false;
                    }

                    break;
                default:
                    break;
            }

            return b_rtn;
        }
        #endregion 

        #region //VONAutoSet
        private bool VONAutoSet()
        {
            UInt16 vonval = 0;
            if (TestSet.von_max == TestSet.von_min)
            {
                vonval = TestSet.von_min;
            }
            else
            {
                vonval = (UInt16)((TestSet.von_max + TestSet.von_min) / 2);
            }
            if (vonval > 2400)
            {
                vonval = 2400;
            }
            if (test.setVON(vonval) == false)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "VON 负压调试失败";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "VON The negative pressure debugging failed";
                }
                return false;
            }
            return true;
        }
        #endregion

        #region //CPAAutoSet
        private bool CPAAutoSet()
        {
            int looptimes = 0;
            UInt16 min = TestSet.txcpa_Min;
            UInt16 max = TestSet.txcpa_Max;
            UInt16 val = 0;
            val = (UInt16)((min + max) / 2);
            if (test.setCPA(val) == false) return false;
            //bias
            if (test.SetTxApcBias((ushort)((TestSet.txapc_Min + TestSet.txapc_Min) / 2)) == false) return false;
            //mod
            if (test.SetTxModBias((ushort)((TestSet.txmod_Max + TestSet.txmod_Min) / 2)) == false) return false;
            //获取示波器参数
            if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
            {
                if (Get_86100D_TxEyeData_DCA(true) == false) return false;
            }
            else
            {
                if (Get_TxEyeData_DCA(true) == false) return false;
            }
            //调试CPA
            if (min == max)
            {
                if (test.setCPA(val) == false) return false;
                if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
                {
                    if (Get_86100D_TxEyeData_DCA(true) == false) return false;
                }
                else
                {
                    if (Get_TxEyeData_DCA(true) == false) return false;
                }
            }
            else
            {
                switch (Dut)
                {
                    case 1:
                        while ((TestResult.txCrossing > TestSet.txCr_Max) || (TestResult.txCrossing < TestSet.txCr_Min) && (TestSet.txcpa_Min < val) && (min < max) && (looptimes < 15))
                        {
                            val = (UInt16)((min + max) / 2);
                            if (test.setCPA(val) == false) return false;
                            if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
                            {
                                if (Get_86100D_TxEyeData_DCA(false) == false) return false;
                            }
                            else
                            {
                                if (Get_TxEyeData_DCA(false) == false) return false;
                            }
                            if (TestResult.txCrossing > TestSet.txCr_Max)
                            {
                                max = val;
                            }
                            else if (TestResult.txCrossing < TestSet.txCr_Min)
                            {
                                min = val;
                            }
                            else
                            {
                                break;
                            }
                            looptimes++;
                            if ((val > TestSet.txcpa_Max - 1) || (val < TestSet.txcpa_Min + 1)) break;
                        }
                        break;
                    case 2:
                        while ((TestResult2.txCrossing > TestSet2.txCr_Max) || (TestResult2.txCrossing < TestSet2.txCr_Min) && (TestSet2.txcpa_Min < val) && (min < max) && (looptimes < 15))
                        {
                            val = (UInt16)((min + max) / 2);
                            if (test.setCPA(val) == false) return false;
                            if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
                            {
                                if (Get_86100D_TxEyeData_DCA(false) == false) return false;
                            }
                            else
                            {
                                if (Get_TxEyeData_DCA(false) == false) return false;
                            }
                            if (TestResult2.txCrossing > TestSet2.txCr_Max)
                            {
                                max = val;
                            }
                            else if (TestResult2.txCrossing < TestSet2.txCr_Min)
                            {
                                min = val;
                            }
                            else
                            {
                                break;
                            }
                            looptimes++;
                            if ((val > TestSet2.txcpa_Max - 1) || (val < TestSet2.txcpa_Min + 1)) break;
                        }
                        break;
                    default:
                        break;
                }
            }
            switch (Dut)
            {
                case 1:
                    if ((TestResult.txCrossing > TestSet.txCr_Max) && (TestSet.txcpa_Min < val))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                //break;
                case 2:
                    if ((TestResult2.txCrossing > TestSet2.txCr_Max) && (TestSet2.txcpa_Min < val))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                // break;
                default:
                    return false;
                    // break;
            }

        }

        private async Task<bool> CPAAutoSet_Async()
        {
            //int looptimes = 0;
            //UInt16 min = TestSet.txcpa_Min;
            //UInt16 max = TestSet.txcpa_Max;
            //UInt16 val = 0;
            //val = (UInt16)((min + max) / 2);
            //if (test.setCPA(val) == false) return false;
            ////bias
            //if (test.SetTxApcBias((ushort)((TestSet.txapc_Min + TestSet.txapc_Min) / 2)) == false) return false;
            ////mod
            //if (test.SetTxModBias((ushort)((TestSet.txmod_Max + TestSet.txmod_Min) / 2)) == false) return false;
            ////获取示波器参数
            //if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
            //{
            //    if (Get_86100D_TxEyeData_DCA(true) == false) return false;
            //}
            //else
            //{
            //    if (Get_TxEyeData_DCA(true) == false) return false;
            //}
            ////调试CPA
            //if (min == max)
            //{
            //    if (test.setCPA(val) == false) return false;
            //    if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
            //    {
            //        if (Get_86100D_TxEyeData_DCA(true) == false) return false;
            //    }
            //    else
            //    {
            //        if (Get_TxEyeData_DCA(true) == false) return false;
            //    }
            //}
            //else
            //{
            //    switch (Dut)
            //    {
            //        case 1:
            //            while ((TestResult.txCrossing > TestSet.txCr_Max) || (TestResult.txCrossing < TestSet.txCr_Min) && (TestSet.txcpa_Min < val) && (min < max) && (looptimes < 15))
            //            {
            //                val = (UInt16)((min + max) / 2);
            //                if (test.setCPA(val) == false) return false;
            //                if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
            //                {
            //                    if (Get_86100D_TxEyeData_DCA(false) == false) return false;
            //                }
            //                else
            //                {
            //                    if (Get_TxEyeData_DCA(false) == false) return false;
            //                }
            //                if (TestResult.txCrossing > TestSet.txCr_Max)
            //                {
            //                    max = val;
            //                }
            //                else if (TestResult.txCrossing < TestSet.txCr_Min)
            //                {
            //                    min = val;
            //                }
            //                else
            //                {
            //                    break;
            //                }
            //                looptimes++;
            //                if ((val > TestSet.txcpa_Max - 1) || (val < TestSet.txcpa_Min + 1)) break;
            //                await Task.Delay(waittimes);
            //            }
            //            break;
            //        case 2:
            //            while ((TestResult2.txCrossing > TestSet2.txCr_Max) || (TestResult2.txCrossing < TestSet2.txCr_Min) && (TestSet2.txcpa_Min < val) && (min < max) && (looptimes < 15))
            //            {
            //                val = (UInt16)((min + max) / 2);
            //                if (test.setCPA(val) == false) return false;
            //                if (GlobalVarFun.setup.dca_86100d || GlobalVarFun.setup.dca_n1092x)
            //                {
            //                    if (Get_86100D_TxEyeData_DCA(false) == false) return false;
            //                }
            //                else
            //                {
            //                    if (Get_TxEyeData_DCA(false) == false) return false;
            //                }
            //                if (TestResult2.txCrossing > TestSet2.txCr_Max)
            //                {
            //                    max = val;
            //                }
            //                else if (TestResult2.txCrossing < TestSet2.txCr_Min)
            //                {
            //                    min = val;
            //                }
            //                else
            //                {
            //                    break;
            //                }
            //                looptimes++;
            //                if ((val > TestSet2.txcpa_Max - 1) || (val < TestSet2.txcpa_Min + 1)) break;
            //                await Task.Delay(waittimes);
            //            }
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //switch (Dut)
            //{
            //    case 1:
            //        if ((TestResult.txCrossing > TestSet.txCr_Max) && (TestSet.txcpa_Min < val))
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    //break;
            //    case 2:
            //        if ((TestResult2.txCrossing > TestSet2.txCr_Max) && (TestSet2.txcpa_Min < val))
            //        {
            //            return false;
            //        }
            //        else
            //        {
            //            return true;
            //        }
            //    // break;
            //    default:
            //        return false;
            //        // break;
            //}

            await Task.Delay(waittimes);
            return true;

        }
        #endregion

        #region //EML_AutoTest
        private bool EML_AutoTest(UInt16 emlvalmin, UInt16 emlvalmax)
        {
            int looptime = 0;           // 二分法循环计数器
            UInt16 emlval = 0;          // 当前设置的TEC温度值（DAC值）
            Double wavelenth = 0;       // 波长计读取的实际波长
            Double result_err = 0;      // 实际波长与目标波长的误差
                                        // OTP12初始化 - 切换光开关到发射方向（模块→波长计）

            string slotStr = GlobalVarFun.OpmDutToOtpSlot[Dut];
            int opmCh = GlobalVarFun.DutToOpmCh[Dut];
            otp12.SetSlot(slotStr);

            // 普通二分法查找
            //如果没有外接波长计（kt86120c），直接取 `(最小值+最大值)/2` 的中间值设置，__不做反馈调节__。
            if (GlobalVarFun.setup.otp12_connect == false)
            {
                emlval = (UInt16)((emlvalmin + emlvalmax) / 2);
                if (test.setWaveLength(emlval) == false) return false;
            }
            else
            {
                do
                {
                    looptime++;

                    emlval = (UInt16)((emlvalmin + emlvalmax) / 2); // 取中间值作为试探点

                    if (emlval < 2) return false; // 值太小 Error
                    if (emlval < 830) // 下限钳位（对应约830nm）
                    {
                        emlval = 830;
                    }
                    if (emlval > 1830) // 上限钳位（对应约1830nm）
                    {
                        emlval = 1830;
                    }
                    test.setWaveLength(emlval); // 设置TEC温度到emlval
                    try
                    {
                        // 新代码 - 先设置OPM波长（可能需要设置后读取），然后读取
                        string waveStr = otp12.OPM_GetWaveLength(opmCh);
                        // OPM返回可能是米科学计数（如 1.550000E-06），需要转换为nm
                        // 1.550000E-06 m = 1550 nm
                        wavelenth = double.Parse(waveStr) * 1e9; // 米转纳米（如果返回是米的话）
                                                                 // 或者如果返回是nm（如 1.550000E+03），则直接解析
                    }
                    catch
                    {
                        // 新代码 - 先设置OPM波长（可能需要设置后读取），然后读取
                        string waveStr = otp12.OPM_GetWaveLength(opmCh);
                        // OPM返回可能是米科学计数（如 1.550000E-06），需要转换为nm
                        // 1.550000E-06 m = 1550 nm
                        wavelenth = double.Parse(waveStr) * 1e9; // 米转纳米（如果返回是米的话）
                                                                 // 或者如果返回是nm（如 1.550000E+03），则直接解析
                    }
                    Thread.Sleep(20);// 计算误差
                    if (wavelenth <= 0) return false;
                    result_err = wavelenth - TestSet.wLength_target; //wLengthTarget目标波长
                                                                     //
                    if (result_err < 0)
                    {
                        emlvalmax = (UInt16)(emlval - 1); // 波长偏短 → 需要降低温度值，下调上界
                    }
                    else
                    {
                        emlvalmin = (UInt16)(emlval + 1); // 波长偏长 → 需要升高温度值，上调下界
                    }
                } while ((Math.Abs(result_err) > wLengthMaxErr) && (emlvalmax > emlvalmin) && (looptime < 100));
                //}
                //catch
                //{
                //    errorMessage += "波长计读取异常";
                //    return false;
                //}
                if ((Math.Abs(result_err) > wLengthMaxErr))
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.ErrorMessage += "波长调试失败，与目标波长不符";
                    }
                    else
                    {
                        retutntxrxresult.ErrorMessage += "The wavelength debugging failed and does not match the target wavelength";
                    }
                    return false;
                }
            }
            switch (Dut)
            {
                case 1:
                    TestResult.txtosaTemp = emlval;
                    break;
                case 2:
                    TestResult2.txtosaTemp = emlval;
                    break;
                case 3:
                    TestResult3.txtosaTemp = emlval;
                    break;
                case 4:
                    TestResult4.txtosaTemp = emlval;
                    break;
                default:
                    break;
            }

            return true;
        }

        private async Task<bool> EML_AutoTest_Async(UInt16 emlvalmin, UInt16 emlvalmax)
        {
            int looptime = 0;
            UInt16 emlval = 0;
            Double wavelenth = 0;
            Double result_err = 0;
            // 普通二分法查找
            //try
            //{
            if (GlobalVarFun.setup.kt86120x_connect == false)
            {
                emlval = (UInt16)((emlvalmin + emlvalmax) / 2);
                if (test.setWaveLength(emlval) == false) return false;
            }
            else
            {
                do
                {
                    looptime++;

                    emlval = (UInt16)((emlvalmin + emlvalmax) / 2);

                    if (emlval < 2) return false; // 值太小 Error
                    if (emlval < 830)
                    {
                        emlval = 830;
                    }
                    if (emlval > 1830)
                    {
                        emlval = 1830;
                    }
                    test.setWaveLength(emlval);
                    try
                    {
                        wavelenth = kt86120c.GetWavelength();
                    }
                    catch { wavelenth = kt86120c.GetWavelength(); }
                    Thread.Sleep(20);
                    if (wavelenth <= 0) return false;
                    result_err = wavelenth - TestSet.wLength_target;
                    //
                    if (result_err < 0)
                    {
                        emlvalmax = (UInt16)(emlval - 1);
                    }
                    else
                    {
                        emlvalmin = (UInt16)(emlval + 1);
                    }

                    await Task.Delay(waittimes);
                } while ((Math.Abs(result_err) > wLengthMaxErr) && (emlvalmax > emlvalmin) && (looptime < 100));
                //}
                //catch
                //{
                //    errorMessage += "波长计读取异常";
                //    return false;
                //}
                if ((Math.Abs(result_err) > wLengthMaxErr))
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.ErrorMessage += "波长调试失败，与目标波长不符";
                    }
                    else
                    {
                        retutntxrxresult.ErrorMessage += "The wavelength debugging failed and does not match the target wavelength";
                    }
                    return false;
                }
            }
            switch (Dut)
            {
                case 1:
                    TestResult.txtosaTemp = emlval;
                    break;
                case 2:
                    TestResult2.txtosaTemp = emlval;
                    break;
                default:
                    break;
            }

            return true;
        }
        #endregion

        #region //wLengthAutoCheck
        private bool wLengthAutoCheck()
        {
            string slotStr = GlobalVarFun.OpmDutToOtpSlot[Dut];
            int opmCh = GlobalVarFun.DutToOpmCh[Dut];
            otp12.SetSlot(slotStr);

            Double wavelenth = 0;
            Double result_err = 0;
            try
            {
                string waveStr = otp12.OPM_GetWaveLength(opmCh);
                wavelenth = double.Parse(waveStr) * 1e9; // 
            }
            catch
            {
                string waveStr = otp12.OPM_GetWaveLength(opmCh);
                wavelenth = double.Parse(waveStr) * 1e9; // 
            }

            switch (Dut)
            {
                case 1:
                    result_err = wavelenth - TestSet.wLength_target;
                    break;
                case 2:
                    result_err = wavelenth - TestSet2.wLength_target;
                    break;
                case 3:
                    result_err = wavelenth - TestSet3.wLength_target;
                    break;
                case 4:
                    result_err = wavelenth - TestSet4.wLength_target;
                    break;
                default:
                    break;
            }

            if (Math.Abs(result_err) > wLengthMaxErr)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "波长检查失败，与目标波长不符";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "The wavelength check failed and does not match the target wavelength";
                }
                return false;
            }
            else
            {
                switch (Dut)
                {
                    case 1:
                        TestResult.wLength = wavelenth;
                        break;
                    case 2:
                        TestResult2.wLength = wavelenth;
                        break;
                    case 3:
                        TestResult3.wLength = wavelenth;
                        break;
                    case 4:
                        TestResult4.wLength = wavelenth;
                        break;
                    default:
                        break;
                }
                return true;
            }
        }

        private async Task<bool> wLengthAutoCheck_Async()
        {
            Double wavelenth = 0;
            Double result_err = 0;
            await Task.Delay(waittimes);
            try
            {
                wavelenth = kt86120c.GetWavelength();
            }
            catch
            {
                wavelenth = kt86120c.GetWavelength();//
            }
            await Task.Delay(waittimes);
            switch (Dut)
            {
                case 1:
                    result_err = wavelenth - TestSet.wLength_target;
                    break;
                case 2:
                    result_err = wavelenth - TestSet2.wLength_target;
                    break;
                default:
                    break;
            }

            if (Math.Abs(result_err) > wLengthMaxErr)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.ErrorMessage += "波长检查失败，与目标波长不符";
                }
                else
                {
                    retutntxrxresult.ErrorMessage += "The wavelength check failed and does not match the target wavelength";
                }
                return false;
            }
            else
            {
                switch (Dut)
                {
                    case 1:
                        TestResult.wLength = wavelenth;
                        break;
                    case 2:
                        TestResult2.wLength = wavelenth;
                        break;
                    default:
                        break;
                }
                return true;
            }
        }
        #endregion

        #region //初始化后台代理
        private void InitializeBackgoundWorker()
        {
            backgroundWorkerAutoSet = new BackgroundWorker();
            backgroundWorkerAutoSet.WorkerSupportsCancellation = true;
            backgroundWorkerAutoSet.WorkerReportsProgress = true;
            backgroundWorkerAutoSet.DoWork += new DoWorkEventHandler(backgroundWorkerAutoSet_DoWork);
            //backgroundWorkerAutoSet.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerAutoSet_RunWorkerCompleted);
            //backgroundWorkerAutoSet.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerAutoSet_ProgressChanged);
        }

        // 后台进程  自动调试
        private void backgroundWorkerAutoSet_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sqlserver.SaveRecordToSQL(Dut) == false)
            {
                AddTestLog("保存到SQL数据库失败");
            }
        }
        #endregion

        #region //初测处理函数
        //private bool FirstTestProcess()
        public async Task<bool> FirstTestProcessAsync(
         IProgress<ReturnTxRxResult> progress)// 参数类型改为 IProgress<T>   
        {
            //设置测试结果颜色为白色（中性/进行中），立即向UI报告初始状态。
            retutntxrxresult.TestResultColor = Color.White;
            //根据当前端口号Dut（1~4），从对应端口的配置对象（`TestSet`/`TestSet2`/`TestSet3`/`TestSet4`）加载校准精度参数：
            progress.Report(retutntxrxresult);
            switch (Dut)
            {
                case 1:
                    GlobalVarFun.setup.er_cal = TestSet.txer_cal;//消光比允许误差（dB）
                    GlobalVarFun.setup.rxpwr_cal = TestSet.rxpwr_cal;//接收光功率允许误差（dB）
                    GlobalVarFun.setup.txpwr_cal = TestSet.txpwr_cal;//发射光功率允许误差（dB）
                    meter_ch = TestSet.meter_ch;//光功率计通道号
                    meter_err = TestSet.meter_pwr_err;//光功率计偏差修正值
                    break;
                case 2:
                    GlobalVarFun.setup.er_cal = TestSet2.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet2.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet2.txpwr_cal;
                    meter_ch = TestSet2.meter_ch;
                    meter_err = TestSet2.meter_pwr_err;
                    break;
                case 3:
                    GlobalVarFun.setup.er_cal = TestSet3.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet3.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet3.txpwr_cal;
                    meter_ch = TestSet3.meter_ch;
                    meter_err = TestSet3.meter_pwr_err;
                    break;
                case 4:
                    GlobalVarFun.setup.er_cal = TestSet4.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet4.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet4.txpwr_cal;
                    meter_ch = TestSet4.meter_ch;
                    meter_err = TestSet4.meter_pwr_err;
                    break;
            }

            //将全局设置中的误差阈值赋给类成员变量，后续检查时直接使用
            txPwrMaxErr = GlobalVarFun.setup.txpwr_cal;
            rxPwrMaxErr = GlobalVarFun.setup.rxpwr_cal;
            erValMaxErr = GlobalVarFun.setup.er_cal;
            wLengthMaxErr = GlobalVarFun.setup.wlgth_cal;

            //更新进度条5%，显示"调试中..."
            retutntxrxresult.Testprogress = 5;
            progress.Report(new ReturnTxRxResult { Percentage = 5, StatusText = "调试中..." });
            // 判断是否进行初始化
            if (GlobalVarFun.setup.init_module)
            {
                // 方案进行初始化操作
                AddTestLog("方案进行初始化操作");
                await Task.Delay(waittimes);
                //如果配置了"初始化模块"选项，则调用test.InitModule() 对模块芯片进行初始化（如写入默认寄存器值、复位等）。
                //test.InitModule()：模块驱动接口方法，对当前DUT的芯片执行初始化序列（不同芯片方案初始化内容不同）
                if (test.InitModule() == false)
                {
                    //失败时：设置结果为红色，设置 `test_status=3`（测试失败状态），返回 `false`
                    retutntxrxresult.TestResultColor = Color.Red;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = GlobalVarFun.moduleType.ToString() + "：待测模块初始化操作失败, 请插入下一只模块......";
                        AddTestLog("模块初始化失败！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = GlobalVarFun.moduleType.ToString() + "：The module under test fails to be initialized. Please insert the next module......";//待测模块初始化操作失败, 请插入下一只模块
                        AddTestLog("Description Module initialization failed！");
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                if (GlobalVarFun.Language == "Chinese")
                {
                    AddTestLog("初始化完成！");
                }
                else
                {
                    AddTestLog("Initialization complete！");
                }
            }
            await Task.Delay(waittimes);//等待，以使其异步线程进入测试
            // 0、判断是否进行关闭TxRxCDR
            //2020.4.8 CDR（Clock and Data Recovery，时钟数据恢复）某些模块在测试时需要关闭CDR以避免干扰调试。
            if (GlobalVarFun.setup.tx_rx_cdr_dis)
            {
                // TxRxCDR 控制操作
                AddTestLog("TxRxCDR 控制操作");
                await Task.Delay(waittimes);
                //test.DisTxRxCDR(true)：通过I2C写入寄存器，禁用TX/RX的CDR功能
                if (test.DisTxRxCDR(true) == false)
                {
                    retutntxrxresult.TestResultColor = Color.Red;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = GlobalVarFun.moduleType.ToString() + "：待测模块TxRxCDR操作失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "待测模块TxRxCDR操作失败, 请插入下一只模块......" });
                        AddTestLog("模块TxRxCDR操作失败！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = GlobalVarFun.moduleType.ToString() + "：The TxRxCDR module to be tested fails to operate. Please insert the next module......";//待测模块TxRxCDR操作失败, 请插入下一只模块
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "The TxRxCDR module to be tested fails to operate. Please insert the next module......" });
                        AddTestLog("Module TxRxCDR operation failed!");
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                if (GlobalVarFun.Language == "Chinese")
                {
                    AddTestLog("TxRx_CDR操作完成！");
                }
                else
                {
                    AddTestLog("TxRx CDR operation complete!");
                }
            }
            retutntxrxresult.Testprogress = 10;
            //
            //
            // * 进入接收、发射调试 
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 进入调试模式
            AddTestLog("进入调试模式");
            //向模块写入调试密码（厂商特定寄存器序列），使模块进入调试/测试模式。只有进入调试模式后，才能修改APC、MOD、LOS等校准寄存器
            if (test.SetDebugPWD() == false)
            {
                retutntxrxresult.TestResultColor = Color.Red;
                if (GlobalVarFun.Language == "Chinese")
                {
                    retutntxrxresult.TestResultMessage = "待测模块进入调试模式失败,确认模块类型是否正确, 请插入下一只模块......";
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "待测模块进入调试模式失败,确认模块类型是否正确, 请插入下一只模块......" });
                    AddTestLog("模块进入调试模式失败！");
                }
                else
                {
                    retutntxrxresult.TestResultMessage = "The module under test fails to enter the debugging mode. Check whether the module type is correct. Insert the next module......";//待测模块进入调试模式失败,确认模块类型是否正确, 请插入下一只模块
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "The module under test fails to enter the debugging mode. Check whether the module type is correct. Insert the next module......" });
                    AddTestLog("Description The module failed to enter debugging mode！");
                }
                switch (Dut)
                {
                    case 1:
                        TestResult.test_status = 3;
                        break;
                    case 2:
                        TestResult2.test_status = 3;
                        break;
                    case 3:
                        TestResult3.test_status = 3;
                        break;
                    case 4:
                        TestResult4.test_status = 3;
                        break;
                }
                return false;
            }
            await Task.Delay(waittimes);
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // 千兆电口和10G电口 特殊操作 //2021.3.13
            // 获取fibertop_pn
            string strPN = "";
            switch (Dut)
            {
                case 1:
                    strPN = TestResult.fibertop_pn;
                    break;
                case 2:
                    strPN = TestResult2.fibertop_pn;
                    break;
                case 3:
                    strPN = TestResult3.fibertop_pn;
                    break;
                case 4:
                    strPN = TestResult4.fibertop_pn;
                    break;
                default:
                    break;
            }

            //电口模块（SFP-GE=千兆电口，SFP-TG=万兆电口）的调试流程完全不同于光口模块
            if (strPN.Contains("SFP-GE") || strPN.Contains("SFP-TG"))
            {
                switch (Dut)
                {
                    // 电口模块：设置固定APC=10, MOD=30
                    // 调用test.SaveTxDataAfterDebug()保存
                    // 直接goto CHECK_POS跳转到收尾阶段
                    case 1:
                        TestResult.txapcVal = 10;
                        TestResult.txmodVal = 30;
                        break;
                    case 2:
                        TestResult2.txapcVal = 10;
                        TestResult2.txmodVal = 30;
                        break;
                    case 3:
                        TestResult3.txapcVal = 10;
                        TestResult3.txmodVal = 30;
                        break;
                    case 4:
                        TestResult4.txapcVal = 10;
                        TestResult4.txmodVal = 30;
                        break;
                    default:
                        break;
                }
                //没
                AddTestLog("SaveTxDataAfterDebug");
                if (test.SaveTxDataAfterDebug() == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("电口模块保存调试参数失败！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = "电口模块保存调试参数失败, 请插入下一只模块......";
                    }
                    //
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "电口模块保存调试参数失败, 请插入下一只模块......" });
                    retutntxrxresult.TestResultColor = Color.Red;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                        default:
                            break;
                    }
                    return false;
                }
                //设置GlobalVarFun.record_need_save = true标记需要存数据库
                GlobalVarFun.record_need_save = true;
                progress.Report(new ReturnTxRxResult { Percentage = 80, StatusText = "电口模块保存调试参数中..." });
                retutntxrxresult.Testprogress = 80;
                //使用goto CHECK_POS直接跳转到收尾阶段（跳过光口的收发调试）
                goto CHECK_POS;
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 0、写入TX-PE等调试参数   // 2017.8.21
            //写入TX-PE等默认调试参数
            //没
            if (GlobalVarFun.setup.tx_pe_test)
            {
                progress.Report(new ReturnTxRxResult { Percentage = 13, StatusText = "发射调试中..." });
                retutntxrxresult.Testprogress = 13;
                AddTestLog("写入TX-PE等调试参数");
                //test.WriteTxRxDefaultVal()：写入TX预加重/去加重（PE，Pre-Emphasis）等默认参数值到模块寄存器，为后续高速信号调试做准备
                //函数默认返回true
                if (test.WriteTxRxDefaultVal() == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("写入TX-PE等调试参数失败！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = "写入TX-PE等调试参数失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "写入TX-PE等调试参数失败, 请插入下一只模块......" });
                    }
                    //
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                        default:
                            break;
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    AddTestLog(retutntxrxresult.ErrorMessage);
                    return false;
                }
            }

            //APD接收自动调试（仅APD类型模块）
            //没
            if (GlobalVarFun.setup.rx_apd_test)
            {
                progress.Report(new ReturnTxRxResult { Percentage = 15, StatusText = "APD Rx调试中..." });
                //使能TX（通过I2C写寄存器0xA2/0x79 bit1）
                AddTestLog("rx_apd_test");
                await Task.Delay(waittimes);
                byte TX_EN_byte = TWI_ReadByte(0xa2, 0x79, Dut);
                TX_EN_byte = Bit.SetBit(TX_EN_byte, 1);
                TWI_WriteByte(0xA2, 0x79, TX_EN_byte, Dut);
                Converted_analog_values(); // 更新界面DDM信息
                //// 2. 检查误码仪是否连接
                if (GlobalVarFun.setup.bert_connect == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("未连接误码仪！");
                        retutntxrxresult.TestResultMessage = "模块接收APD自动调试失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "模块接收APD自动调试失败, 请插入下一只模块......" });
                        return false;
                    }
                    else
                    {
                        AddTestLog("The bit error rate tester is not connected!");
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "The automatic debugging of the module receiving APD failed. Please insert the next module......" });
                        retutntxrxresult.TestResultMessage = "The automatic debugging of the module receiving APD failed. Please insert the next module......";
                        return false;
                    }
                }
                //没
                AddTestLog("AutoTestRxAPD");
                Task<bool> autoapd = AutoTestRxAPD_Async(Dut);
                bool res = await autoapd;
                if (res == false)
                //   if (AutoTestRxAPD() == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("接收APD自动调试失败！" + retutntxrxresult.ErrorMessage);
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "模块接收APD自动调试失败, 请插入下一只模块......" });
                        retutntxrxresult.TestResultMessage = "模块接收APD自动调试失败, 请插入下一只模块......";
                    }
                    else
                    {
                        AddTestLog("The automatic debugging for receiving APD failed!");
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "The automatic debugging of the module receiving APD failed, please insert the next module......" });
                        retutntxrxresult.TestResultMessage = "The automatic debugging of the module receiving APD failed, please insert the next module......";
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.apd = TestResult.rxapdVal;
                        break;
                    case 2:
                        retutntxrxresult.apd = TestResult2.rxapdVal;
                        break;
                    case 3:
                        retutntxrxresult.apd = TestResult3.rxapdVal;
                        break;
                    case 4:
                        retutntxrxresult.apd = TestResult4.rxapdVal;
                        break;

                    default:
                        break;
                }

                //没
                //保存APD调试参数 本次
                AddTestLog("保存APD调试参数");
                await Task.Delay(waittimes);
                if (test.SaveRxDataAfterDebug() == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("保存Rx接收调试参数失败！");
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "保存Rx接收调试参数失败, 请插入下一只模块......" });
                        retutntxrxresult.TestResultMessage = "保存Rx接收调试参数失败, 请插入下一只模块......";
                    }
                    else
                    {
                        AddTestLog("Failed to save Rx receive debugging parameters!");
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "Failed to save Rx receive debugging parameters, please insert the next module......" });
                        retutntxrxresult.TestResultMessage = "Failed to save Rx receive debugging parameters, please insert the next module......";
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
            }
            //
            // 1、接收DDM 及 LOS 告警项目调试及功能检查 //2021.4.27调整顺序
            // 没
            if (GlobalVarFun.setup.rx_test)
            {
                progress.Report(new ReturnTxRxResult { Percentage = 20, StatusText = "接收调试中..." });
                retutntxrxresult.Testprogress = 20;
                AddTestLog("rx_test");
                await Task.Delay(waittimes);
                switch (Dut)
                {
                    case 1:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult.fibertop_pn.Contains("DIP") || TestResult.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(false);
                        }
                        break;
                    case 2:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult2.fibertop_pn.Contains("DIP") || TestResult2.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(false);
                        }
                        break;
                    case 3:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult.fibertop_pn.Contains("DIP") || TestResult.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(false);
                        }
                        break;
                    case 4:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult.fibertop_pn.Contains("DIP") || TestResult.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(false);
                        }
                        break;
                    default:
                        break;
                }
                AddTestLog("RxPwrDDMAutoCal");
                Task<bool> rxpwrcal = RxPwrDDMAutoCal_Async();
                bool res = await rxpwrcal;
                if (res == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("接收DDM自动校准失败！");
                        retutntxrxresult.TestResultMessage = "模块接收DDM 自动校准失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "模块接收DDM 自动校准失败, 请插入下一只模块......" });
                    }
                    else
                    {
                        AddTestLog("DDM auto calibration failed!");
                        retutntxrxresult.TestResultMessage = "Module failed to receive DDM automatic calibration, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "Module failed to receive DDM automatic calibration, please insert the next module......" });
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
                switch (Dut)
                {
                    case 1:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult.fibertop_pn.Contains("DIP") || TestResult.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(true);
                        }
                        break;
                    case 2:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult2.fibertop_pn.Contains("DIP") || TestResult2.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(true);
                        }
                        break;
                    case 3:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult.fibertop_pn.Contains("DIP") || TestResult.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(true);
                        }
                        break;
                    case 4:
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") && (TestResult.fibertop_pn.Contains("DIP") || TestResult.fibertop_pn.Contains("DCP")))
                        {
                            test.TxTempLookupTableCtrl(true);
                        }
                        break;
                    default:
                        break;
                }
                progress.Report(new ReturnTxRxResult { Percentage = 25, StatusText = "接收DDM检测..." });
                retutntxrxresult.Testprogress = 25;
                retutntxrxresult.StatusText = "接收DDM检测...";
                progress.Report(retutntxrxresult);
                //
                await Task.Delay(waittimes);//等待，以使其异步线程进入测试

                retutntxrxresult.Testprogress = 35;
                retutntxrxresult.TestResultColor = Color.White;//
                progress.Report(new ReturnTxRxResult { Percentage = 35, StatusText = "LOS告警功能自动调试..." });
                retutntxrxresult.StatusText = "LOS告警功能自动调试...";
                progress.Report(retutntxrxresult);
                //
                await Task.Delay(waittimes);//等待，以使其它异步线程进入测试
                AddTestLog("RxLosAutoSet");
                Task<bool> rxlosauto = RxLosAutoSet_Async();
                res = await rxlosauto;
                if (res == false)
                //if (RxLosAutoSet() == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("LOS告警功能自动调试失败！");
                        retutntxrxresult.TestResultMessage = "待测模块自动调试Los功能失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "待测模块自动调试Los功能失败, 请插入下一只模块......" });
                        AddTestLog("模块Los调试失败！");
                    }
                    else
                    {
                        AddTestLog("LOS Alarm automatic debugging failed!");
                        retutntxrxresult.TestResultMessage = "The module under test fails to automatically debug the Los function. Please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "The module under test fails to automatically debug the Los function. Please insert the next module......" });
                        AddTestLog("Module Los debug failed!");
                    }
                    switch (Dut)
                    {
                        case 1:
                            retutntxrxresult.los = TestResult.rxlosVal; // 显示los调试结果
                            break;
                        case 2:
                            retutntxrxresult.los = TestResult2.rxlosVal; // 显示los调试结果
                            break;
                        case 3:
                            retutntxrxresult.los = TestResult3.rxlosVal; // 显示los调试结果
                            break;
                        case 4:
                            retutntxrxresult.los = TestResult4.rxlosVal; // 显示los调试结果
                            break;
                        default:
                            break;
                    }

                    //
                    retutntxrxresult.TestResultColor = Color.Red;

                    return false;
                }
                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.los = TestResult.rxlosVal; // 显示los调试结果
                        break;
                    case 2:
                        retutntxrxresult.los = TestResult2.rxlosVal; // 显示los调试结果
                        break;
                    case 3:
                        retutntxrxresult.los = TestResult3.rxlosVal; // 显示los调试结果
                        break;
                    case 4:
                        retutntxrxresult.los = TestResult4.rxlosVal; // 显示los调试结果
                        break;
                    default:
                        break;
                }
                progress.Report(new ReturnTxRxResult { Percentage = 40, StatusText = "LOS功能检查..." });
                retutntxrxresult.Testprogress = 40;

                await Task.Delay(waittimes);//等待，以使其异步线程进入测试

                retutntxrxresult.Testprogress = 43;
                progress.Report(new ReturnTxRxResult { Percentage = 43, StatusText = "保存Rx接收调试参数..." });
                retutntxrxresult.StatusText = "保存Rx接收调试参数...";
                progress.Report(retutntxrxresult);
                //
                await Task.Delay(waittimes);//等待，以使其异步线程进入测试
                AddTestLog("SaveRxDataAfterDebug");
                if (test.SaveRxDataAfterDebug() == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("保存Rx接收调试参数失败！");
                        retutntxrxresult.TestResultMessage = "保存Rx接收调试参数失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "保存Rx接收调试参数失败, 请插入下一只模块......" });
                    }
                    else
                    {
                        AddTestLog("Failed to save Rx receive debugging parameters!");
                        retutntxrxresult.TestResultMessage = "Failed to save Rx receive debugging parameters, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "Failed to save Rx receive debugging parameters, please insert the next module......" });
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
                retutntxrxresult.Testprogress = 44;
                progress.Report(new ReturnTxRxResult { Percentage = 44, StatusText = "接收RxSen检测..." });
                retutntxrxresult.StatusText = "接收RxSen检测...";
                progress.Report(retutntxrxresult);
                //
                if (GlobalVarFun.setup.rx_test)
                {
                    await Task.Delay(waittimes);
                    AddTestLog("rx_sen_test");

                    AddTestLog("RxSenBitErrorCheck");
                    Task<bool> rxsencheck = RxSenBitErrorCheck_Async();
                    res = await rxsencheck;
                    if (res == false)
                    // if (RxSenBitErrorCheck() == false)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            AddTestLog("接收RxSen检测出现误码！");
                            retutntxrxresult.TestResultMessage = "模块接收灵敏度RxSen检查失败, 请插入下一只模块......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = "模块接收灵敏度RxSen检查失败, 请插入下一只模块......" });
                        }
                        else
                        {
                            AddTestLog("Received RxSen detection error!");
                            retutntxrxresult.TestResultMessage = "Module receiving sensitivity RxSen check failed, please insert the next module......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        }
                        switch (Dut)
                        {
                            case 1:
                                TestResult.test_status = 3;
                                break;
                            case 2:
                                TestResult2.test_status = 3;
                                break;
                            case 3:
                                TestResult3.test_status = 3;
                                break;
                            case 4:
                                TestResult4.test_status = 3;
                                break;
                        }
                        retutntxrxresult.TestResultColor = Color.Red;
                        return false;
                    }

                }
                //
                GlobalVarFun.record_need_save = true;
            }
            retutntxrxresult.Testprogress = 45;
            progress.Report(new ReturnTxRxResult { Percentage = 45, StatusText = "调试中..." });

            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 2、发射调试
            //
            //波长自动测试
            if (GlobalVarFun.setup.tx_eml_test)
            {
                retutntxrxresult.Testprogress = 45;
                progress.Report(new ReturnTxRxResult { Percentage = 45, StatusText = "EML 发射调试中..." });
                AddTestLog("tx_eml_test");
                await switchSemaphore.WaitAsync();
                //lock (tx_lock)
                try
                {
                    //Tx EN
                    byte TX_EN_byte = TWI_ReadByte(0xa2, 0x79, Dut);
                    TX_EN_byte = Bit.SetBit(TX_EN_byte, 1);
                    TWI_WriteByte(0xa2, 0x79, TX_EN_byte, Dut);
                    Converted_analog_values(); // 更新界面DDM信息

                    //VON
                    AddTestLog("VONAutoSet");
                    if (VONAutoSet() == false)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            AddTestLog("负压调试失败！" + retutntxrxresult.ErrorMessage);
                            retutntxrxresult.TestResultMessage = "VON负压调试失败, 请插入下一只模块......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        }
                        else
                        {
                            AddTestLog("Negative pressure debugging failed.!");
                            retutntxrxresult.TestResultMessage = "VON negative pressure debugging failed. Please insert the next module.......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        }
                        switch (Dut)
                        {
                            case 1:
                                TestResult.test_status = 3;
                                break;
                            case 2:
                                TestResult2.test_status = 3;
                                break;
                            case 3:
                                TestResult3.test_status = 3;
                                break;
                            case 4:
                                TestResult4.test_status = 3;
                                break;
                        }
                        retutntxrxresult.TestResultColor = Color.Red;
                        return false;
                    }
                    switch (Dut)
                    {
                        case 1:
                            retutntxrxresult.von = TestResult.txVON;
                            break;
                        case 2:
                            retutntxrxresult.von = TestResult2.txVON;
                            break;
                        case 3:
                            retutntxrxresult.von = TestResult3.txVON;
                            break;
                        case 4:
                            retutntxrxresult.von = TestResult4.txVON;
                            break;
                        default:
                            break;
                    }
                    //波长测试
                    AddTestLog("波长测试 EML_AutoTest");
                    if (EML_AutoTest(TestSet.tosatemp_min, TestSet.tosatemp_max) == false)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            AddTestLog("发射波长调试失败！" + retutntxrxresult.ErrorMessage);
                            retutntxrxresult.TestResultMessage = "发射波长调试失败, 请插入下一只模块......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        }
                        else
                        {
                            AddTestLog("The wavelength tuning of the emission wave has failed!");
                            retutntxrxresult.TestResultMessage = "The wavelength emission adjustment failed. Please insert the next module......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        }
                        switch (Dut)
                        {
                            case 1:
                                TestResult.test_status = 3;
                                break;
                            case 2:
                                TestResult2.test_status = 3;
                                break;
                            case 3:
                                TestResult3.test_status = 3;
                                break;
                            case 4:
                                TestResult4.test_status = 3;
                                break;
                        }
                        retutntxrxresult.TestResultColor = Color.Red;
                        return false;
                    }
                    switch (Dut)
                    {
                        case 1:
                            retutntxrxresult.tosatemp = TestResult.txtosaTemp;
                            break;
                        case 2:
                            retutntxrxresult.tosatemp = TestResult2.txtosaTemp;
                            break;
                        case 3:
                            retutntxrxresult.tosatemp = TestResult3.txtosaTemp;
                            break;
                        case 4:
                            retutntxrxresult.tosatemp = TestResult4.txtosaTemp;
                            break;
                        default:
                            break;
                    }
                    AddTestLog("CPAAutoSet");
                    Task<bool> txcpaauto = CPAAutoSet_Async();
                    bool res = await txcpaauto;
                    if (res == false)
                    //if (CPAAutoSet() == false)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            AddTestLog("发射交叉点调试失败！" + retutntxrxresult.ErrorMessage);
                            retutntxrxresult.TestResultMessage = "发射交叉点调试失败, 请插入下一只模块......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        }
                        else
                        {
                            AddTestLog("The crossing tuning of the emission wave has failed!");
                            retutntxrxresult.TestResultMessage = "The crossing emission adjustment failed. Please insert the next module......";
                            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        }
                        switch (Dut)
                        {
                            case 1:
                                TestResult.test_status = 3;
                                break;
                            case 2:
                                TestResult2.test_status = 3;
                                break;
                            case 3:
                                TestResult3.test_status = 3;
                                break;
                            case 4:
                                TestResult4.test_status = 3;
                                break;
                        }
                        retutntxrxresult.TestResultColor = Color.Red;
                        return false;
                    }
                    switch (Dut)
                    {
                        case 1:
                            retutntxrxresult.cpa = TestResult.txCPA;
                            break;
                        case 2:
                            retutntxrxresult.cpa = TestResult2.txCPA;
                            break;
                        case 3:
                            retutntxrxresult.cpa = TestResult3.txCPA;
                            break;
                        case 4:
                            retutntxrxresult.cpa = TestResult4.txCPA;
                            break;
                        default:
                            break;
                    }

                }
                catch
                {
                    retutntxrxresult.TestResultColor = Color.Red;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                finally
                {
                    switchSemaphore.Release();
                }
            }
            //发射bias,mod调试
            if (GlobalVarFun.setup.tx_test)
            {
                retutntxrxresult.Testprogress = 50;
                progress.Report(new ReturnTxRxResult { Percentage = 50, StatusText = "发射调试中..." });
                AddTestLog("tx_test");
                AddTestLog("关闭发射自动温补功能 TxTempLookupTableCtrl");
                await Task.Delay(waittimes);
                // 关闭发射自动温补功能
                if (test.TxTempLookupTableCtrl(false) == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("关闭发射温度补偿失败！");
                        retutntxrxresult.TestResultMessage = "关闭发射温度补偿失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    else
                    {
                        AddTestLog("Failed to turn off launch temperature compensation!");
                        retutntxrxresult.TestResultMessage = "Failed to turn off emission temperature compensation, please insert next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
                retutntxrxresult.Testprogress = 55;
                progress.Report(new ReturnTxRxResult { Percentage = 55, StatusText = "发射光功率调试..." });
                retutntxrxresult.StatusText = "发射光功率调试...";
                progress.Report(retutntxrxresult);

                AddTestLog("TxPowerAutoSet");

                if (TxPowerAutoSet() == false)
                {
                    AddTestLog(GlobalVarFun.meter_error_message);
                    retutntxrxresult.ErrorMessage += GlobalVarFun.meter_error_message;
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("发射光功率调试失败：" + retutntxrxresult.ErrorMessage);
                        retutntxrxresult.TestResultMessage = "发射光功率调试失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    else
                    {
                        AddTestLog("The transmitted optical power debugging fails. Procedure：" + retutntxrxresult.ErrorMessage);
                        retutntxrxresult.TestResultMessage = "Transmit optical power debugging failed, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    switch (Dut)
                    {
                        case 1:
                            retutntxrxresult.apc = TestResult.txapcVal;
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            retutntxrxresult.apc = TestResult2.txapcVal;
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            retutntxrxresult.apc = TestResult3.txapcVal;
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            retutntxrxresult.apc = TestResult4.txapcVal;
                            TestResult4.test_status = 3;
                            break;
                        default:
                            break;
                    }
                    //
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
                Converted_analog_values(); // 更新界面DDM信息

                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.apc = TestResult.txapcVal;
                        break;
                    case 2:
                        retutntxrxresult.apc = TestResult2.txapcVal;
                        break;
                    case 3:
                        retutntxrxresult.apc = TestResult3.txapcVal;
                        break;
                    case 4:
                        retutntxrxresult.apc = TestResult4.txapcVal;
                        break;
                    default:
                        break;
                }
                retutntxrxresult.Testprogress = 60;
                progress.Report(new ReturnTxRxResult { Percentage = 60, StatusText = "发射消光比调试..." });
                retutntxrxresult.StatusText = "发射消光比调试...";
                progress.Report(retutntxrxresult);
                //}
                //lock (tx_lock)
                //{ 
                AddTestLog("TxErAutoSet");
                Task<bool> testERauto = TxErAutoSet_Async();
                bool res = await testERauto;
                //res = await TxErAutoSet_Async();
                if (res == false)
                //if (TxErAutoSet() == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("发射消光比调试失败：" + retutntxrxresult.ErrorMessage);
                        retutntxrxresult.TestResultMessage = "发射消光比调试失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    else
                    {
                        AddTestLog("Emission extinction ratio debugging failed：" + retutntxrxresult.ErrorMessage);
                        retutntxrxresult.TestResultMessage = "Emission extinction ratio debugging failed, please insert next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }

                    switch (Dut)
                    {
                        case 1:
                            retutntxrxresult.mod = TestResult.txmodVal;
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            retutntxrxresult.mod = TestResult2.txmodVal;
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            retutntxrxresult.mod = TestResult3.txmodVal;
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            retutntxrxresult.mod = TestResult4.txmodVal;
                            TestResult4.test_status = 3;
                            break;
                        default:
                            break;
                    }
                    //
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
                if (GlobalVarFun.setup.algorithm_25g_lr || GlobalVarFun.setup.tx_eml_test)
                {
                    if (GlobalVarFun.setup.algorithm_25g_lr) AddTestLog("algorithm_25g_lr: true");
                    if (GlobalVarFun.setup.tx_eml_test) AddTestLog("tx_eml_test: true");
                    string txpower = "0";
                    float pwr = 0;

                    string slotStr = GlobalVarFun.OpmDutToOtpSlot[Dut];
                    int opmCh = GlobalVarFun.DutToOpmCh[Dut];
                    otp12.SetSlot(slotStr);

                    //获取 TXPower
                    switch (Dut)
                    {
                        case 1:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult.txPower = pwr;
                            break;
                        case 2:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult2.txPower = pwr;
                            break;
                        case 3:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult3.txPower = pwr;
                            break;
                        case 4:
                            txpower = otp12.OPM_ReadPower(opmCh);
                            float.TryParse(txpower, out pwr);
                            pwr += meter_err;
                            TestResult4.txPower = pwr;
                            break;
                        default:
                            break;
                    }

                    // 写入发射校准参数到模块
                    //Thread.Sleep(200);
                    AddTestLog("WriteTxCalData");
                    if (test.WriteTxCalData() == false)
                    {
                        if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                        {
                            test.EEPROMcheckSum();
                        }
                        retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                        AddTestLog(retutntxrxresult.ErrorMessage);
                        switch (Dut)
                        {
                            case 1:
                                TestResult.test_status = 3;
                                break;
                            case 2:
                                TestResult2.test_status = 3;
                                break;
                            case 3:
                                TestResult3.test_status = 3;
                                break;
                            case 4:
                                TestResult4.test_status = 3;
                                break;
                        }
                        retutntxrxresult.TestResultColor = Color.Red;
                        return false;
                    }

                }
                // 写入发射校准参数到模块
                //Thread.Sleep(200);
                AddTestLog("txPower:" + TestResult.txPower.ToString());
                AddTestLog("WriteTxCalData");
                if (test.WriteTxCalData() == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    retutntxrxresult.ErrorMessage += "发光校准ADC错误";
                    AddTestLog(retutntxrxresult.ErrorMessage);
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                AddTestLog("txPower:" + TestResult.txPower.ToString());

                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.mod = TestResult.txmodVal;
                        break;
                    case 2:
                        retutntxrxresult.mod = TestResult2.txmodVal;
                        break;
                    case 3:
                        retutntxrxresult.mod = TestResult3.txmodVal;
                        break;
                    case 4:
                        retutntxrxresult.mod = TestResult4.txmodVal;
                        break;
                    default:
                        break;
                }
                // }
                retutntxrxresult.Testprogress = 75;
                progress.Report(new ReturnTxRxResult { Percentage = 75, StatusText = "保存Tx发射调试参数..." });
                retutntxrxresult.StatusText = "保存Tx发射调试参数...";
                progress.Report(retutntxrxresult);

                AddTestLog("SaveTxDataAfterDebug");
                await Task.Delay(waittimes);
                if (test.SaveTxDataAfterDebug() == false)
                {
                    if ((GlobalVarFun.moduleType == "SFP-UX3320T") || (GlobalVarFun.moduleType == "SFPP-UX3261S") || (GlobalVarFun.moduleType == "SFPP-UX2270+2072"))
                    {
                        test.EEPROMcheckSum();
                    }
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("保存Tx发射调试参数失败！");
                        retutntxrxresult.TestResultMessage = "保存Tx发射调试参数失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    else
                    {
                        AddTestLog("Failed to save Tx launch debugging parameters!");
                        retutntxrxresult.TestResultMessage = "Failed to save Tx emission debugging parameters, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    //
                    AddTestLog(retutntxrxresult.ErrorMessage);
                    retutntxrxresult.TestResultColor = Color.Red;
                    return false;
                }
                retutntxrxresult.Testprogress = 80;
                progress.Report(new ReturnTxRxResult { Percentage = 80, StatusText = "模块发射光功率和消光比检查..." });
                //lock (tx_lock)
                //{
                //Tx发射参数检查
                AddTestLog("TxFinalTestCheck");

            }
            GlobalVarFun.record_need_save = true;
            // }
            retutntxrxresult.Testprogress = 85;
            progress.Report(new ReturnTxRxResult { Percentage = 85, StatusText = "调试中..." });
        //
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        CHECK_POS:
            //
            // 3. 读取模块flash调试信息
            AddTestLog("GetFlashInfoDebug");
            if (test.GetFlashInfoDebug() == false)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    AddTestLog("读取模块flash调试信息失败！");
                    retutntxrxresult.TestResultMessage = "读取模块flash调试信息失败, 请插入下一只模块......";
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                }
                else
                {
                    AddTestLog("Failed to read module flash debugging information!");
                    retutntxrxresult.TestResultMessage = "Failed to read the module flash debugging information. Please insert the next module......";
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                }
                //
                switch (Dut)
                {
                    case 1:
                        TestResult.test_status = 3;
                        break;
                    case 2:
                        TestResult2.test_status = 3;
                        break;
                    case 3:
                        TestResult3.test_status = 3;
                        break;
                    case 4:
                        TestResult4.test_status = 3;
                        break;
                }
                retutntxrxresult.TestResultColor = Color.Red;
                return false;
            }
            //
            switch (Dut)
            {
                case 1:
                    retutntxrxresult.TestResultSn = TestResult.fibertop_sn; //界面显示待测FSN流水号
                    break;
                case 2:
                    retutntxrxresult.TestResultSn = TestResult2.fibertop_sn; //界面显示待测FSN流水号
                    break;
                case 3:
                    retutntxrxresult.TestResultSn = TestResult3.fibertop_sn; //界面显示待测FSN流水号
                    break;
                case 4:
                    retutntxrxresult.TestResultSn = TestResult4.fibertop_sn; //界面显示待测FSN流水号
                    break;
                default:
                    break;
            }
            //
            retutntxrxresult.Testprogress = 90;
            progress.Report(new ReturnTxRxResult { Percentage = 90, StatusText = "保存参数到数据库..." });
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 4、保存参数到数据库
            if (((GlobalVarFun.sql_connect_status == true) && (GlobalVarFun.record_need_save == true)) ||
                ((GlobalVarFun.sql_connect_status_2 == true) && (GlobalVarFun.record_need_save_2 == true)) ||
                ((GlobalVarFun.sql_connect_status_3 == true) && (GlobalVarFun.record_need_save_3 == true)) ||
                ((GlobalVarFun.sql_connect_status_4 == true) && (GlobalVarFun.record_need_save_4 == true)))
            {
                AddTestLog("保存参数到数据库");
                if (GlobalVarFun.sql_connect_status)
                {
                    AddTestLog("sql_connect_status:" + "true");
                }
                else
                {
                    AddTestLog("sql_connect_status:" + "false");
                }

                if (GlobalVarFun.record_need_save)
                {
                    AddTestLog("record_need_save:" + "true");
                }
                else
                {
                    AddTestLog("record_need_save:" + "false");
                }

                if (Dut == 1) GlobalVarFun.record_need_save = false;
                if (Dut == 2) GlobalVarFun.record_need_save_2 = false;
                if (Dut == 3) GlobalVarFun.record_need_save_3 = false;
                if (Dut == 4) GlobalVarFun.record_need_save_4 = false;

                //if (sqlserver.SaveRecordToSQL(Dut) == false)
                //{
                //    AddTestLog("保存到SQL数据库失败");
                //    return false;
                //}

                if (backgroundWorkerAutoSet.IsBusy)
                {
                    retutntxrxresult.TestResultColor = Color.Yellow;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = "模块初测调试完成，未保存到SQL数据库，请检查数据库连接！！请插入下一只模块......";
                        AddTestLog("SQL数据库写入初测记录进程被占用，初测参数未保存！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = "Module initial debugging completed, not saved to the SQL database, please check the database connection!! Please insert the next module......";//模块初测调试完成，未保存到SQL数据库，请检查数据库连接！！请插入下一只模块
                        AddTestLog("SQL database write test log process is occupied, test parameters are not saved!");
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                // 启动后台进程 保存测试数据到数据库
                backgroundWorkerAutoSet.RunWorkerAsync();
                //barcode_textBox.Text = sn_textBox.Text; ///////////////////////////////test
                //AddTestLog(retutntxrxresult.ErrorMessage);
                //retutntxrxresult.ErrorMessage = "";
                AddTestLog("保存到SQL数据库成功");
            }
            else
            {
                //DUT1
                if (GlobalVarFun.sql_connect_status)
                {
                    AddTestLog("sql_connect_status : true");
                }
                else
                {
                    AddTestLog("sql_connect_status : false");
                }
                //
                if (GlobalVarFun.record_need_save)
                {
                    AddTestLog("record_need_save : true");
                }
                else
                {
                    AddTestLog("record_need_save : false");
                }

                //DUT2
                if (GlobalVarFun.sql_connect_status_2)
                {
                    AddTestLog("sql_connect_status_2 : true");
                }
                else
                {
                    AddTestLog("sql_connect_status_2 : false");
                }
                //
                if (GlobalVarFun.record_need_save_2)
                {
                    AddTestLog("record_need_save_2 : true");
                }
                else
                {
                    AddTestLog("record_need_save_2 : false");
                }

                //DUT3
                if (GlobalVarFun.sql_connect_status_3)
                {
                    AddTestLog("sql_connect_status_3 : true");
                }
                else
                {
                    AddTestLog("sql_connect_status_3 : false");
                }
                //
                if (GlobalVarFun.record_need_save_3)
                {
                    AddTestLog("record_need_save_3 : true");
                }
                else
                {
                    AddTestLog("record_need_save_3 : false");
                }

                //DUT4
                if (GlobalVarFun.sql_connect_status_4)
                {
                    AddTestLog("sql_connect_status_4 : true");
                }
                else
                {
                    AddTestLog("sql_connect_status_4 : false");
                }
                //
                if (GlobalVarFun.record_need_save_4)
                {
                    AddTestLog("record_need_save_4 : true");
                }
                else
                {
                    AddTestLog("record_need_save_4 : false");
                }

                if (GlobalVarFun.Language == "Chinese")
                {
                    AddTestLog("初测记录未保存到SQL数据库！");
                }
                else
                {
                    AddTestLog("Initial test records are not saved to SQL database!");
                }
            }
            //
            retutntxrxresult.TestLogColor = Color.Green;
            if (GlobalVarFun.Language == "Chinese")
            {
                AddTestLog("初测调试完成！");
                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.TestResultMessage = TestResult.sn.TrimEnd() + "初测完成, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult.test_status = 2;
                        break;
                    case 2:
                        retutntxrxresult.TestResultMessage = TestResult2.sn.TrimEnd() + "初测完成, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult2.test_status = 2;
                        break;
                    case 3:
                        retutntxrxresult.TestResultMessage = TestResult3.sn.TrimEnd() + "初测完成, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult3.test_status = 2;
                        break;
                    case 4:
                        retutntxrxresult.TestResultMessage = TestResult4.sn.TrimEnd() + "初测完成, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult4.test_status = 2;
                        break;
                    default:
                        break;
                }
                AddTestLog(retutntxrxresult.TestResultMessage);
            }
            else
            {
                AddTestLog("Initial test debugging completed!");//初测调试完成！
                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.TestResultMessage = TestResult.sn.TrimEnd() + "Initial debugging is complete, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult.test_status = 2;
                        break;
                    case 2:
                        retutntxrxresult.TestResultMessage = TestResult2.sn.TrimEnd() + "Initial debugging is complete, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult2.test_status = 2;
                        break;
                    case 3:
                        retutntxrxresult.TestResultMessage = TestResult3.sn.TrimEnd() + "Initial debugging is complete, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult3.test_status = 2;
                        break;
                    case 4:
                        retutntxrxresult.TestResultMessage = TestResult4.sn.TrimEnd() + "Initial debugging is complete, please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        TestResult4.test_status = 2;
                        break;
                    default:
                        break;
                }
            }

            retutntxrxresult.Testprogress = 100;
            retutntxrxresult.TestResultColor = Color.Green;
            retutntxrxresult.StatusText = retutntxrxresult.TestResultMessage.Trim();
            progress.Report(retutntxrxresult);//测试完成
                                              //progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage.Trim() });//测试完成

            return true;
        }
        #endregion

        #region // 终测处理函数
        //private bool FinalTestProcess()
        public async Task<bool> FinalTestProcessAsync(
        IProgress<ReturnTxRxResult> progress)// 参数类型改为 IProgress<T> 
        {
            string errMsg = "";
            switch (Dut)
            {
                case 1:
                    TestResult.test_status = 1;
                    GlobalVarFun.setup.er_cal = TestSet.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet.txpwr_cal;
                    meter_ch = TestSet.meter_ch;
                    meter_err = TestSet.meter_pwr_err;
                    break;
                case 2:
                    TestResult2.test_status = 1;
                    GlobalVarFun.setup.er_cal = TestSet2.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet2.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet2.txpwr_cal;
                    meter_ch = TestSet2.meter_ch;
                    meter_err = TestSet2.meter_pwr_err;
                    break;
                case 3:
                    TestResult3.test_status = 1;
                    GlobalVarFun.setup.er_cal = TestSet3.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet3.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet3.txpwr_cal;
                    meter_ch = TestSet3.meter_ch;
                    meter_err = TestSet3.meter_pwr_err;
                    break;
                case 4:
                    TestResult4.test_status = 1;
                    GlobalVarFun.setup.er_cal = TestSet4.txer_cal;
                    GlobalVarFun.setup.rxpwr_cal = TestSet4.rxpwr_cal;
                    GlobalVarFun.setup.txpwr_cal = TestSet4.txpwr_cal;
                    meter_ch = TestSet4.meter_ch;
                    meter_err = TestSet4.meter_pwr_err;
                    break;
            }
            // 0、判断是否进行关闭TxRxCDR //2020.4.8
            if (GlobalVarFun.setup.tx_rx_cdr_dis)
            {
                // TxRxCDR 控制操作
                AddTestLog("TxRxCDR 控制操作");
                if (test.DisTxRxCDR(true) == false)
                {
                    retutntxrxresult.TestResultColor = Color.Red;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = GlobalVarFun.moduleType.ToString() + "：待测模块TxRxCDR操作失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        AddTestLog("模块TxRxCDR操作失败！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = GlobalVarFun.moduleType.ToString() + "：The TxRxCDR module to be tested fails to operate. Please insert the next module......";//待测模块TxRxCDR操作失败, 请插入下一只模块
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        AddTestLog("Module TxRxCDR operation failed!");
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                if (GlobalVarFun.Language == "Chinese")
                {
                    AddTestLog("TxRx_CDR操作完成！");
                }
                else
                {
                    AddTestLog("TxRx CDR operation complete!");
                }
            }
            retutntxrxresult.Testprogress = 10;
            progress.Report(new ReturnTxRxResult { Percentage = 10, StatusText = "模块接收LOS或告警功能检查..." });
            //

            // 1、接收LOS 及 DDM 告警项目 功能检查
            if (GlobalVarFun.setup.rx_test)
            {
                await Task.Delay(waittimes);
                AddTestLog("RxLosAlarmCheck");
                Task<bool> loscheck = RxLosAlarmCheck_Async();
                bool res = await loscheck;
                if (res == false)
                // if (RxLosAlarmCheck() == false)
                {
                    retutntxrxresult.TestResultColor = Color.Red;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = "模块接收LOS或告警功能 检查失败, 请插入下一只模块......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = "The module fails to receive the LOS or alarm function check. Please insert the next module......";
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                AddTestLog("RxPwrErrorCheck");
                await Task.Delay(waittimes);
                Task<bool> rxpwrcheck = RxPwrErrorCheck_Async();
                res = await rxpwrcheck;
                if (res == false)
                // if (RxPwrErrorCheck() == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("接收DDM检测精度超出设定范围！");
                        retutntxrxresult.TestResultMessage = "模块接收校准 DDM精度检查失败, 请插入下一只模块......";
                    }
                    else
                    {
                        AddTestLog("Receive DDM detection accuracy beyond the set range!");
                        retutntxrxresult.TestResultMessage = "Module failed to receive calibration DDM accuracy check, please insert next module......";
                    }
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    //
                    retutntxrxresult.TestResultColor = Color.Red;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                if (GlobalVarFun.setup.rx_test)
                {
                    await Task.Delay(waittimes);
                    AddTestLog("rx_sen_test");
                    //lock (rx_lock)
                    //{
                    Task<bool> rxsencheck = RxSenBitErrorCheck_Async();
                    res = await rxsencheck;
                    if (res == false)
                    //if (RxSenBitErrorCheck() == false)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            AddTestLog("接收RxSen检测出现误码！");
                            retutntxrxresult.TestResultMessage = "模块接收灵敏度RxSen检查失败, 请插入下一只模块......";
                        }
                        else
                        {
                            AddTestLog("Received Rx Sen detection error!");
                            retutntxrxresult.TestResultMessage = "Module receiving sensitivity RxSen check failed, please insert the next module......";
                        }
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                        //
                        retutntxrxresult.TestResultColor = Color.Red;
                        switch (Dut)
                        {
                            case 1:
                                TestResult.test_status = 3;
                                break;
                            case 2:
                                TestResult2.test_status = 3;
                                break;
                            case 3:
                                TestResult3.test_status = 3;
                                break;
                            case 4:
                                TestResult4.test_status = 3;
                                break;

                        }
                        return false;
                    }
                    //}
                }
                //
                GlobalVarFun.record_need_save = true;
            }
            retutntxrxresult.Testprogress = 40;
            progress.Report(retutntxrxresult);
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////
            //
            //发射波长检测
            if (GlobalVarFun.setup.tx_eml_test)
            {
                AddTestLog("发射波长检测");
                await Task.Delay(waittimes);
                progress.Report(new ReturnTxRxResult { Percentage = 40, StatusText = "发射波长检测..." });
                // lock (tx_lock)
                await switchSemaphore.WaitAsync();
                try
                {
                    if (wLengthAutoCheck() == false)
                    {
                        if (GlobalVarFun.Language == "Chinese")
                        {
                            AddTestLog("模块发射波长参数异常！");
                            retutntxrxresult.TestResultMessage = "模块发射波长 检查失败, 请插入下一只模块......";
                        }
                        else
                        {
                            AddTestLog("The wavelength parameter of the module emission is abnormal!");
                            retutntxrxresult.TestResultMessage = "Module emission wavelength check failed. Please insert the next module......";
                        }
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    }
                }
                catch
                {
                    AddTestLog("发射波长检测异常");
                }
                finally
                {
                    switchSemaphore.Release();
                }
            }
            // 2、检测发光功率、消光比
            if (GlobalVarFun.setup.tx_test)
            {
                AddTestLog("2、检测发光功率、消光比");
                await Task.Delay(waittimes);

                Task<bool> finalcheck = TxFinalTestCheck_Async(true);
                bool res = await finalcheck;
                if (res == false)
                //if (TxFinalTestCheck(true) == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("模块发射光功率和消光比参数异常！");
                        retutntxrxresult.TestResultMessage = "模块发射光功率和消光比 检查失败, 请插入下一只模块......";
                    }
                    else
                    {
                        AddTestLog("Module transmit optical power and extinction ratio parameters are abnormal!");
                        retutntxrxresult.TestResultMessage = "Module transmit light power and extinction ratio check failed, please insert the next module......";
                    }
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    retutntxrxresult.TestResultColor = Color.Red;
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                GlobalVarFun.record_need_save = true;
            }
            retutntxrxresult.Testprogress = 70;
            progress.Report(retutntxrxresult);
            progress.Report(new ReturnTxRxResult { Percentage = 70, StatusText = "模块告警门限检查..." });
            ///////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 3、检测模块告警门限
            if (GlobalVarFun.setup.threshold_check)
            {
                AddTestLog("检测模块告警门限");
                await Task.Delay(waittimes);
                if (test.CheckThresholdsInfo(ref errMsg) == false)
                {
                    retutntxrxresult.TestResultColor = Color.Red;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = "待测模块告警门限检查错误, 请插入下一只模块......";
                        AddTestLog("告警门限检查错误: " + errMsg);
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = "The alarm threshold of the module under test is incorrect. Insert the next module......";//待测模块告警门限检查错误, 请插入下一只模块
                        AddTestLog("The alarm threshold check is incorrect: " + errMsg);
                    }
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                //GlobalVarFun.record_need_save = true;
            }
            retutntxrxresult.Testprogress = 80;
            progress.Report(new ReturnTxRxResult { Percentage = 80, StatusText = "进入调试模式..." });
            ///////////////////////////////////////////////////////////////////////////////////////////////
            // 4、模块调试参数 检查
            if (GlobalVarFun.setup.flash_check)
            {
                AddTestLog("进入调试模式");
                await Task.Delay(waittimes);
                // 进入调试模式
                if (test.SetDebugPWD() == false)
                {
                    retutntxrxresult.TestResultColor = Color.Red;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = "待测模块进入调试模式失败, 请插入下一只模块......";
                        AddTestLog("模块进入调试模式失败！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = "The module under test fails to enter debugging mode. Please insert the next module......";//待测模块进入调试模式失败, 请插入下一只模块
                        AddTestLog("Module failed to enter debug mode!");
                    }
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }

                if (GlobalVarFun.setup.tx_eml_test)
                {
                    AddTestLog("Tx EN");
                    //Tx EN
                    byte TX_EN_byte = TWI_ReadByte(0xa2, 0x79, Dut);
                    TX_EN_byte = Bit.SetBit(TX_EN_byte, 1);
                    TWI_WriteByte(0xa2, 0x79, TX_EN_byte, Dut);
                    //Thread.Sleep(200);
                    await Task.Delay(200);
                    Converted_analog_values(); // 更新界面DDM信息
                }
                //读取模块调试信息
                AddTestLog("读取模块调试信息 GetFlashInfoDebug");
                await Task.Delay(waittimes);
                if (test.GetFlashInfoDebug() == false)
                {
                    retutntxrxresult.TestResultColor = Color.Red;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        retutntxrxresult.TestResultMessage = "待测模块调试信息读取失败, 请插入下一只模块......";
                        AddTestLog("读取模块调试信息失败！");
                    }
                    else
                    {
                        retutntxrxresult.TestResultMessage = "The debugging information of the module under test fails to be read. Please insert the next module......";//待测模块调试信息读取失败, 请插入下一只模块
                        AddTestLog("Failed to read module debugging information!");//读取模块调试信息失败！
                    }
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.TestResultFibertopSn = TestResult.fibertop_sn; //界面显示待测FSN流水号
                        break;
                    case 2:
                        retutntxrxresult.TestResultFibertopSn = TestResult2.fibertop_sn; //界面显示待测FSN流水号
                        break;
                    case 3:
                        retutntxrxresult.TestResultFibertopSn = TestResult3.fibertop_sn; //界面显示待测FSN流水号
                        break;
                    case 4:
                        retutntxrxresult.TestResultFibertopSn = TestResult4.fibertop_sn; //界面显示待测FSN流水号
                        break;
                    default:
                        break;
                }
                //
                retutntxrxresult.Testprogress = 85;
                progress.Report(new ReturnTxRxResult { Percentage = 85, StatusText = "flash_data 参数检查..." });
                //
                AddTestLog("CheckModuleFlashInfo");
                if (test.CheckModuleFlashInfo(ref errMsg) == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("Falsh信息检查错误: " + errMsg);
                        retutntxrxresult.TestResultMessage = "模块 flash_data 参数检查失败, 请插入下一只模块......";
                    }
                    else
                    {
                        AddTestLog("Falsh information check error: " + errMsg);//Falsh信息检查错误
                        retutntxrxresult.TestResultMessage = "Module flash_data parameter check failed, please insert the next module......";
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
                //
                GlobalVarFun.record_need_save = true;
            }
            //电口测试
            if (GlobalVarFun.setup.electrical_module)
            {
                AddTestLog("电口测试");
                await Task.Delay(waittimes);
                if (Elec_moudleTest() == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        AddTestLog("电口调试信息检查错误: " + errMsg);
                        retutntxrxresult.TestResultMessage = "模块 LOS/速率 参数检查失败, 请插入下一只模块......";
                    }
                    else
                    {
                        AddTestLog("The electrical debugging information is incorrect: " + errMsg);
                        retutntxrxresult.TestResultMessage = "Module LOS/ rate parameter failed to be checked. Please insert the next module......";
                    }
                    retutntxrxresult.TestResultColor = Color.Red;
                    progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
                    switch (Dut)
                    {
                        case 1:
                            TestResult.test_status = 3;
                            break;
                        case 2:
                            TestResult2.test_status = 3;
                            break;
                        case 3:
                            TestResult3.test_status = 3;
                            break;
                        case 4:
                            TestResult4.test_status = 3;
                            break;
                    }
                    return false;
                }
            }
            retutntxrxresult.Testprogress = 90;
            progress.Report(new ReturnTxRxResult { Percentage = 90, StatusText = "保存参数到数据库..." });
            ///////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 5、保存参数到数据库
            if ((GlobalVarFun.sql_connect_status == true) && (GlobalVarFun.record_need_save == true))
            {
                AddTestLog("保存参数到数据库");
                GlobalVarFun.record_need_save = false;
                //
                if (sqlserver.SaveRecordToSQL(Dut) == false)
                {
                    AddTestLog("保存到SQL数据库失败");
                    return false;
                }
                AddTestLog("保存到SQL数据库成功");

            }
            else
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    AddTestLog("终测记录未保存到SQL数据库！");
                }
                else
                {
                    AddTestLog("Final test record not saved to SQL database!");
                }
            }
            //
            retutntxrxresult.TestResultColor = Color.Green;
            if (GlobalVarFun.Language == "Chinese")
            {
                AddTestLog("终测检查完成！");
                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.TestResultMessage = TestResult.sn.TrimEnd() + "终测检查完成, 请插入下一只模块......";
                        break;
                    case 2:
                        retutntxrxresult.TestResultMessage = TestResult2.sn.TrimEnd() + "终测检查完成, 请插入下一只模块......";
                        break;
                    case 3:
                        retutntxrxresult.TestResultMessage = TestResult3.sn.TrimEnd() + "终测检查完成, 请插入下一只模块......";
                        break;
                    case 4:
                        retutntxrxresult.TestResultMessage = TestResult4.sn.TrimEnd() + "终测检查完成, 请插入下一只模块......";
                        break;
                    default:
                        break;
                }
                progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
            }
            else
            {
                AddTestLog("Final inspection complete!");//终测检查完成！
                switch (Dut)
                {
                    case 1:
                        retutntxrxresult.TestResultMessage = TestResult.sn.TrimEnd() + "Final check complete, please insert the next module......";
                        break;
                    case 2:
                        retutntxrxresult.TestResultMessage = TestResult2.sn.TrimEnd() + "Final check complete, please insert the next module......";
                        break;
                    case 3:
                        retutntxrxresult.TestResultMessage = TestResult3.sn.TrimEnd() + "Final check complete, please insert the next module......";
                        break;
                    case 4:
                        retutntxrxresult.TestResultMessage = TestResult4.sn.TrimEnd() + "Final check complete, please insert the next module......";
                        break;
                    default:
                        break;
                }
                progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
            }
            switch (Dut)
            {
                case 1:
                    TestResult.test_status = 2;
                    break;
                case 2:
                    TestResult2.test_status = 2;
                    break;
                case 3:
                    TestResult3.test_status = 2;
                    break;
                case 4:
                    TestResult4.test_status = 2;
                    break;
            }
            retutntxrxresult.Testprogress = 100;
            retutntxrxresult.TestResultColor = Color.Green;
            progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage });
            progress.Report(retutntxrxresult);
            if (GlobalVarFun.testType == "finalTest")
            {
                switch (Dut)
                {
                    case 1:
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage = TestResult.sn.TrimEnd() + "测试完成, 请插入下一只模块......" });
                        break;
                    case 2:
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage = TestResult2.sn.TrimEnd() + "测试完成, 请插入下一只模块......" });
                        break;
                    case 3:
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage = TestResult3.sn.TrimEnd() + "测试完成, 请插入下一只模块......" });
                        break;
                    case 4:
                        progress.Report(new ReturnTxRxResult { Percentage = 100, StatusText = retutntxrxresult.TestResultMessage = TestResult4.sn.TrimEnd() + "测试完成, 请插入下一只模块......" });
                        break;
                    default:
                        break;
                }

            }

            return true;
        }
        #endregion

    }
}
