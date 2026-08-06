using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using System.Data.OleDb;
using System.Data.SqlClient;
using Agilent.AgilentInfiniiumDCA.Interop;
using System.Threading;
using System.IO.Ports;
using System.IO;
//using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using FibertopTest_Common;
using Ivi.Visa.Interop;
using DCAX_86100;

namespace SFP模块终测检查软件
{
    public partial class Main_Form : Form
    {
        BackgroundWorker backgroundWorkerAutoSet;
        //
        //OleDbConnection accessdbconnect;
        Stopwatch timer;
        //
        SqlConnection sqlconnection;
        //
        I2C i2c;
       // I2C Ui2c;
        ModuleTest test;
        AgilentInfiniiumDCA scope = GlobalVarFun.scope;
        DCA_86100 scope_86100d = GlobalVarFun.scope_86100d;
        Keysight86120C kt86120c = GlobalVarFun.kt86120c;
        //SerialPort uartMeter = GlobalVarFun.uartMeter;
        //SerialPort uartAtt = GlobalVarFun.uartAtt;
        //SerialPort pssert = GlobalVarFun.pssert;//
        //SerialPort opticalSwitch = GlobalVarFun.opticalSwitch;

        string errorMessage = ""; //
        string pssChannel = "";//
        float rxPwrMaxErr = 1; // 接收DDM校准检查精度
        float txPwrMaxErr = 1; // 发射DDM校准检查精度
        float erValMaxErr = 1; // 发射消光比精度

        bool optoMeter_connected = GlobalVarFun.optoMeter_connected;
        bool optoAtt_connected = GlobalVarFun.optoAtt_connected;
        bool instrument_connected = GlobalVarFun.instrument_connected;
        bool pssbert_connected = GlobalVarFun.pssbert_connected;
        bool optoSwitch_connected = GlobalVarFun.optoSwitch_connected;    

        bool eyeMaskIsOpened = false;

        bool autoTestCtrl = false;
        bool moduleOnline = true;
        string gpibAddress = "GPIB0::07::INSTR";
       // bool testDataIsOK = false;

        UInt16[] rxAdc = new UInt16[6];

        UInt16 apc = 0;
        UInt16 mod = 0;
 
        ////////////////////////////////////////////////////////////////////////////////////////////
        //
        public Main_Form()
        {
            InitializeComponent();

            // 测试软件启动判断是否注册信息正确
            if (GlobalVarFun.GetRegisterInfo() == false)
            {
                //Application.Exit();
                Environment.Exit(0);
            }
            
            //
            GlobalVarFun.i2c_can_use = false; // 等I2C消息函数OK后才能启用
            GlobalVarFun.sql_connect_status = false; // 数据库连接状态

            TestResult.txPower = -40;
            TestResult.txEr    = 0;

            TestResult.tempDDM = 0;
            TestResult.vccDDM  = 0;
            TestResult.txBiasDDM  = 0;
            TestResult.txPowerDDM = -60;
            TestResult.rxPowerDDM = -60;

            DOA.rxCheckAtt[0] = 0;
            DOA.rxCheckAtt[1] = 8;
            DOA.rxCheckAtt[2] = 12;
            DOA.rxCheckAtt[3] = 20;
            DOA.rxCheckAtt[4] = 25;

            DOA.rxOverLoadAttBuf[0] = 0;
            DOA.rxSenAttBuf[0] = 19;
            DOA.rxALosAttBuf[0] = 20;
            DOA.rxDLosAttBuf[0] = 25;

            DOA.rxOverLoadAttBuf[1] = 0;
            DOA.rxSenAttBuf[1] = 19;
            DOA.rxALosAttBuf[1] = 20;
            DOA.rxDLosAttBuf[1] = 25;

            DOA.rxOverLoadAttBuf[2] = 0;
            DOA.rxSenAttBuf[2] = 19;
            DOA.rxALosAttBuf[2] = 20;
            DOA.rxDLosAttBuf[2] = 25;

            DOA.rxOverLoadAttBuf[3] = 0;
            DOA.rxSenAttBuf[3] = 19;
            DOA.rxALosAttBuf[3] = 20;
            DOA.rxDLosAttBuf[3] = 25;
        }

        // 初始化窗体
        private void Main_Form_Load(object sender, EventArgs e)
        {
            //初始化后台代理
            InitializeBackgoundWorker();

            timer = new Stopwatch();

            this.i2c = GlobalVarFun.iic;      
            //this.usbtoi2c = GlobalVarFun.USBtoI2C;
            this.sqlconnection = GlobalVarFun.sqlconnection;
            this.test = GlobalVarFun.mTest;
            test.Init(i2c); //必须调用       
            GlobalVarFun.scope = new AgilentInfiniiumDCAClass(); //创建一个86100DCA对象
            GlobalVarFun.uartMeter = new SerialPort(); // 创建光功率计串口
            GlobalVarFun.uartAtt = new SerialPort();   // 创建光衰减器串口
            GlobalVarFun.pssert = new SerialPort();    //创建误码仪串口
            GlobalVarFun.opticalSwitch = new SerialPort(); //创建光开关串口
            GlobalVarFun.scope_86100d = new DCA_86100();//86100D,N1092X
            GlobalVarFun.kt86120c = new Keysight86120C();//光波长计
            // Access 数据库存放路径更新
            //accessdbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + GlobalVarFun.moduleLutDBFilePath);
            
            //scope = new AgilentInfiniiumDCAClass(); //创建一个86100DCA对象
            //instrument_connected = false;

            //uartMeter = new SerialPort(); // 创建光功率计串口
            //uartAtt = new SerialPort();   // 创建光衰减器串口
            //pssert = new SerialPort();    //创建误码仪串口
            //opticalSwitch = new SerialPort(); //创建光开关串口
            //
            //初始化界面控件
            ///////////////////////////////////////////////////////////////////////////////////
            if (GlobalVarFun.rx_is_apd == false)
            {
                real_rxpower1_textbox.Text = TestSet.rxPwr_Real[0].ToString("F1");// "-10";
                real_rxpower2_textbox.Text = TestSet.rxPwr_Real[1].ToString("F1");//"-18";
                real_rxpower3_textbox.Text = TestSet.rxPwr_Real[2].ToString("F1");//"-25";
                real_rxpower4_textbox.Text = TestSet.rxPwr_Real[3].ToString("F1");//"-26";
                real_rxpower5_textbox.Text = TestSet.rxPwr_Real[4].ToString("F1"); //"-30";
                ddm_rxpower4_textbox.Enabled = false;
                ddm_rxpower5_textbox.Enabled = false;
                ddm_rxpower4_textbox.ReadOnly = true;
                ddm_rxpower5_textbox.ReadOnly = true;
            }
            else
            {
                real_rxpower1_textbox.Text = TestSet.rxPwr_Real[0].ToString("F1");// "-10";
                real_rxpower2_textbox.Text = TestSet.rxPwr_Real[1].ToString("F1");// "-18";
                real_rxpower3_textbox.Text = TestSet.rxPwr_Real[2].ToString("F1");//"-22";
                real_rxpower4_textbox.Text = TestSet.rxPwr_Real[3].ToString("F1");//"-26";
                real_rxpower5_textbox.Text = TestSet.rxPwr_Real[4].ToString("F1");//"-30";
                ddm_rxpower4_textbox.Enabled = true;
                ddm_rxpower5_textbox.Enabled = true;
                ddm_rxpower4_textbox.ReadOnly = true;
                ddm_rxpower5_textbox.ReadOnly = true;
            }           

            real_rxpower6_textbox.Text = "-40";
            //
            foreach (Control control in groupBox2.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox picturebox = control as PictureBox;
                    picturebox.Image = imageList1.Images["LedNone.ico"];
                }
            }
            
            foreach (Control control in groupBox5.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox picturebox = control as PictureBox;
                    picturebox.Image = imageList1.Images["LedNone.ico"];
                }
            }

            //txapcMin_numericUpDown.Text = TestSet.txapc_Min.ToString();
            //txapcMax_numericUpDown.Text = TestSet.txapc_Max.ToString();
            //txmodMin_numericUpDown.Text = TestSet.txmod_Min.ToString();
            //txmodMax_numericUpDown.Text = TestSet.txmod_Max.ToString();
            //rxlosMin_numericUpDown.Text = TestSet.rxlos_Min.ToString();
            //rxlosMax_numericUpDown.Text = TestSet.rxlos_Max.ToString();

            //TestResult.waveforms_count = Convert.ToInt32(waveforms_numericUpDown.Value);

            //meterType_comboBox.SelectedIndex = 1; //光功率计类型选择

            ////TxRx_CDR 控制选择
            //checkBox_DisCDR.Checked = false;
            //checkBox_TOSA_NoMPD.Checked = false;
            //if (GlobalVarFun.moduleType == "SFP+")
            //{
            //    checkBox_DisCDR.Enabled = true;
            //    checkBox_TOSA_NoMPD.Enabled = true;
            //}
            //else
            //{
            //    checkBox_DisCDR.Enabled = false;
            //    checkBox_TOSA_NoMPD.Enabled = false;
            //}
            
            //checkBox_DisTypeCheck.Enabled = true;
            //checkBox_DisTypeCheck.Checked = false;

            //// 根据模块型号 更改主窗口标题
            if (GlobalVarFun.testType == "firstTest")
            {
                this.Text = "*** " + GlobalVarFun.moduleType + "  初测调试软件" + this.Text;
                //txCalNumericUpDown.Value = Convert.ToDecimal(0.5);
                //rxCalNumericUpDown.Value = Convert.ToDecimal(1.0);

                button1_testType.Text = "初测.调试";
                textBoxTester.Text = "FirstTest_01";

                //if (GlobalVarFun.moduleType == "SFP+")
                //{
                //    txpe_checkBox.Enabled = true;
                //    txpe_numericUpDown.Enabled = true;
                //}

                //if (GlobalVarFun.moduleType == "SFPP-GN1196" || GlobalVarFun.moduleType == "SFP-GN25L95" || GlobalVarFun.moduleType == "SFP-GN25L96" || GlobalVarFun.moduleType == "SFP-UX3320C" || GlobalVarFun.moduleType == "SFP-UX3320T")
                //{
                //    checkBox_Init.Enabled = true;
                //}

                //checkBox_debugTest.Enabled = false;
                //checkBox_AlarmThresholds.Enabled = false;

                //checkBox_EyeSave.Enabled = false;
                //waveforms_numericUpDown.Enabled = false;

                //checkBox_txJt.Checked = false;
                //checkBox_txJt.Enabled = false;
            }
            else if (GlobalVarFun.testType == "finalTest")
            {
                this.Text = "*** " + GlobalVarFun.moduleType + "  终测检查软件" + this.Text;
                //rxlosMin_numericUpDown.Enabled = false;
                //rxlosMax_numericUpDown.Enabled = false;
                //txapcMin_numericUpDown.Enabled = false;
                //txapcMax_numericUpDown.Enabled = false;
                //txmodMin_numericUpDown.Enabled = false;
                //txmodMax_numericUpDown.Enabled = false;
                //erCalNumericUpDown.Enabled = false;

                //checkBox_Init.Enabled = false;

                //checkBox_EyeSave.Enabled = true;
                //waveforms_numericUpDown.Enabled = true;

                //checkBox_txJt.Checked = true;
                //checkBox_txJt.Enabled = true;
            }
            else
            {
                MessageBox.Show("测试工序初始化错误！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            ////////////////////////////////////////////////////////////////////////////
            if (i2c.TWI_Open() == false)
            {
                GlobalVarFun.i2c_can_use = false; // I2C 异常
            }
            else
            {
                GlobalVarFun.i2c_can_use = true; //IIC 可以使用
            }

            // 测试SQL数据连接情况
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (sqlconnection != null)
            {
                try
                {
                    sqlconnection.Open();
                    GlobalVarFun.sql_connect_status = true; // 数据库连接正常
                }
                catch (Exception exp)
                {
                    GlobalVarFun.sql_connect_status = false; // 数据库连接失败
                    MessageBox.Show(exp.Message);
                }
                finally
                {
                    sqlconnection.Close();
                }
            }
            GlobalVarFun.sql_record_status = GlobalVarFun.sql_connect_status;//更新记录状态和SQL连接状态一致 2018.5.19
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // 从Access数据库中更新模块型号列表
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            string[] strType = new string[300];
            //int len = 0;
            //moduletype_comboBox.Items.Clear();
            ////
            //if (test.GetModuleTypeFromAccessdb(ref strType, ref len))
            //{
            //    for (int i = 0; i < len; i++)
            //    {
            //        moduletype_comboBox.Items.Add(strType[i]);
            //    }
            //}

            //if (moduletype_comboBox.Items.Count > 0)
            //{
            //    moduletype_comboBox.SelectedIndex = 0;
            //}
            //else
            //{
            //    MessageBox.Show("初始化模块型号列表失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    Application.Exit();
            //}
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string[] portnames = SerialPort.GetPortNames();
            Array.Sort(portnames); //已存在串口更新
            //meterCom_comboBox.Items.Clear();
            //attCom_comboBox.Items.Clear();
            //for (int i = 0; i < portnames.Length; i++)
            //{
            //    meterCom_comboBox.Items.Add(portnames[i]);
            //    attCom_comboBox.Items.Add(portnames[i]);
            //    PSSCom_comboBox.Items.Add(portnames[i]);
            //}
            //if (meterCom_comboBox.Items.Count > 0)
            //{
            //    meterCom_comboBox.SelectedIndex = 0;
            //}
            //if (attCom_comboBox.Items.Count > 0)
            //{
            //    attCom_comboBox.SelectedIndex = 0;
            //}
            //if (gpibCom_comboBox.Items.Count > 0)
            //{
            //    gpibCom_comboBox.SelectedIndex = 0;
            //}
            //if (PSSCom_comboBox.Items.Count > 0)
            //{
            //    PSSCom_comboBox.SelectedIndex = 0;
            //}

            //// 调试参数 范围设置 //2017.11.30
            /////////////////////////////////////////////////////////////////////
            //if (GlobalVarFun.moduleType == "XFP")
            //{
            //    erCalNumericUpDown.Value = (decimal)0.3;
            //}
            //else if (GlobalVarFun.moduleType == "SFP+")
            //{
            //    erCalNumericUpDown.Value = (decimal)0.3;
            //    //
            //    txpe_numericUpDown.Value = 72; //2017.8.21
            //}
            //else if (GlobalVarFun.moduleType == "SFP-MCU")
            //{
            //    erCalNumericUpDown.Value = (decimal)0.5;
            //}
            //else if (GlobalVarFun.moduleType == "SFP-GN25L95")
            //{
            //    rxlosMax_numericUpDown.Maximum = 127; //LOS 最大设置范围
            //    //
            //    erCalNumericUpDown.Value = (decimal)0.5;
            //}
            //else if (GlobalVarFun.moduleType == "SFP-GN25L96")
            //{
            //    rxlosMax_numericUpDown.Maximum = 127; //LOS 最大设置范围//
            //    //
            //    erCalNumericUpDown.Value = (decimal)0.5;
            //}
            //else if (GlobalVarFun.moduleType == "SFP-UX3320C")
            //{
            //    //rxlosMax_numericUpDown.Maximum = 255; //LOS 最大设置范围
            //    //
            //    erCalNumericUpDown.Value = (decimal)0.5;
            //}
            //else if (GlobalVarFun.moduleType == "SFP-UX3320T")
            //{
            //    //rxlosMax_numericUpDown.Maximum = 255; //LOS 最大设置范围
            //    //
            //    erCalNumericUpDown.Value = (decimal)0.5;
            //}
            //else if (GlobalVarFun.moduleType == "SFPP-GN1196")
            //{
            //    rxlosMax_numericUpDown.Maximum = 63; //LOS 最大设置范围//
            //    //
            //    erCalNumericUpDown.Value = (decimal)0.5;
            //}
            //else //未用模块类型
            //{
            //    erCalNumericUpDown.Value = (decimal)0.3;
            //}
            ///////////////////////////////////////////////////////////////////

            //
            testType_textBox1.Text = GlobalVarFun.moduleType;
            SetLED(sqlconnt_pictureBox, !GlobalVarFun.sql_connect_status);
            SetLED(accessconnt_pictureBox, !GlobalVarFun.access_connect_status);
            SetLED(accessupdated_pictureBox, !GlobalVarFun.access_updated_status);
            //
            
            // 开定时器
            timer1.Start();
        }

        private void Main_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sqlconnection != null)
                sqlconnection.Close();

            /*if (accessdbconnect != null)
                accessdbconnect.Close();*/

            if (GlobalVarFun.scope != null && GlobalVarFun.instrument_connected == true && GlobalVarFun.DCA86100D_Open == false && GlobalVarFun.N1092x_Open == false)
                GlobalVarFun.scope.Close();

            if (GlobalVarFun.scope_86100d != null && GlobalVarFun.instrument_connected == true)
                GlobalVarFun.instrument_connected = false;

            if (i2c != null)
                i2c.TWI_Close();

            if (GlobalVarFun.uartAtt != null)
                if (GlobalVarFun.uartAtt.IsOpen)
                    GlobalVarFun.uartAtt.Close();

            if (GlobalVarFun.uartMeter != null)
                if (GlobalVarFun.uartMeter.IsOpen)
                    GlobalVarFun.uartMeter.Close();

            if (GlobalVarFun.opticalSwitch != null)
                if (GlobalVarFun.opticalSwitch.IsOpen)
                    GlobalVarFun.opticalSwitch.Close();
        }

        // 将光功率 从dBm转换为uW
        private float ConvertdBmtouW(float dBm)
        {
            return (float)(Math.Pow(10, 0.1 * dBm) * 10000.0);
        }
        //
        private double ConvertdBmtouW(double dBm)
        {
            return (double)(Math.Pow(10, 0.1 * dBm) * 10000.0);
        }

        // 显示 调试 LOG 信息 到界面
        //////////////////////////////////////////////////////////////////////////
        //
        private void ClearTestLog()
        {
            testLog_textBox.Text = "";
        }

        private void AddTestLog(string strMessage)
        {
            if (strMessage.Trim() == "")
            {
                return;
            }

            testLog_textBox.Text = testLog_textBox.Text + strMessage + "\r\n";
            /*
            if (testLog_textBox.Lines.Length > 7)
            {
                testLog_textBox.ScrollBars = ScrollBars.Vertical;
            }*/
        }
        //
        //////////////////////////////////////////////////////////////////////////


        // 设置DOA 数字光衰减器的ATT值
        private char SetDOA_RxAttVal(float attVal)
        {
            int i = 0;
            //
            if (GlobalVarFun.optoAtt_new_connected)
            {
                string strcmd = "Configure:Atten channel1 -";
                if (GlobalVarFun.optoAtt_connected == false)
                {
                    return (char)0x01; // 设备未连接
                }

                if ((attVal > 60) || (attVal < 0))
                {
                    return (char)0x02; // 参数错误
                }

                if (attVal > 40)
                {
                    attVal = 40;
                }
                strcmd += attVal.ToString();
                GlobalVarFun.uartAtt.WriteLine(strcmd);

                // 单模: 每dB延时 60ms     //单模: 每dB延时5-10ms
                i = (int)((float)DOA.delay * Math.Abs(DOA.currentAtt - attVal));
                Thread.Sleep(i + 200);
                DOA.currentAtt = attVal;

                return (char)0x00; // 操作成功
            }
            else
            {
                byte[] WriteBuffer = new byte[23] { 0x43, 0x6F, 0x6E, 0x66, 0x69, 0x67, 0x75, 0x72, 0x65, 0x3A, 0x41, 0x74, 0x74, 0x65, 0x6E, 0x20, 0x2D, 0x32, 0x30, 0x2E, 0x30, 0x0D, 0x0A };
                byte[] ReadBuffer = new byte[2];
                ReadBuffer[0] = 0xFF;

                if (GlobalVarFun.optoAtt_connected == false)
                {
                    return (char)0x01; // 设备未连接
                }

                if ((attVal > 60) || (attVal < 0))
                {
                    return (char)0x02; // 参数错误
                }

                String str = attVal.ToString("F1");
                attVal = Convert.ToSingle(str);

                byte[] byteArray = System.Text.Encoding.ASCII.GetBytes(str);
                int len = byteArray.GetLength(0);

                if (len > 4)
                {
                    return (char)0x02; // 参数错误
                }

                for (i = 0; i < len; i++)
                {
                    WriteBuffer[17 + i] = byteArray[i];
                }
                WriteBuffer[17 + len] = 0x0D;
                WriteBuffer[18 + len] = 0x0A;

                GlobalVarFun.uartAtt.Write(WriteBuffer, 0, 19 + len);

                // 单模: 每dB延时 60ms     //单模: 每dB延时5-10ms
                i = (int)((float)DOA.delay * Math.Abs(DOA.currentAtt - attVal));
                Thread.Sleep(i + 200);

                DOA.currentAtt = attVal;
                /*
                uart_readLen_rtn = uart.Read(ReadBuffer, 0, 1);
                if ((ReadBuffer[0] == 0x00) && (uart_readLen_rtn == 1))
                {
                    return (char)0x00; // 操作成功
                }
                else
                {
                    return (char)0x03; // 操作失败
                }*/

                return (char)0x00; // 操作成功
            }
        }

        // 发射测试用 读取光功率
        private float Get_TxOptoPower()
        {
            float val;
            int ch = TestSet.ch;
            if (GlobalVarFun.power_use_DAC == true) // 选择用DCA眼图仪 读取光功率
            {
                val = Get_OptoPower_DCA(); // 从眼图仪读取光功率值
            }
            else
            {
                val = Get_OptoPower_Meter(); // 从光功率计读取光功率值
                if (val <= -40)
                {
                    val = Get_OptoPower_Meter(); // Read again
                }
            }

            // 是否异常
            if (val > -100) // 无异常
            {
               // val += (float)(GlobalVarFun.opto_att_offset); // 加偏差
                val += (float)(GlobalVarFun.opto_att_offsetbuf[ch]); // 加偏差
            }

            return val;
        }

        // 读取光功率—从功率计
        private float Get_OptoPower_Meter()
        {
            float pwrValue = 0;
            float dispdata = 0;
            int k = 0;

            if (MEMTER.type_index == 0) //手持光功率计 光讯
            {
                byte[] WriteBuffer = new byte[7] { 0xef, 0xef, 0x04, 0x04, 0x60, 0x06, 0x4c };
                byte[] ReadBuffer = new byte[14];
                //
                Thread.Sleep((int)MEMTER.delay); // 延时
                //
                try
                {
                    GlobalVarFun.uartMeter.Write(WriteBuffer, 0, 7);
                    Thread.Sleep(150);
                    GlobalVarFun.uartMeter.Read(ReadBuffer, 0, 14);

                    if ((ReadBuffer[0] == 0xed) && (ReadBuffer[1] == 0xfa))
                    {
                        dispdata = (ReadBuffer[7] * 256) + ReadBuffer[8];
                        switch (ReadBuffer[9] & 0x30)//判断单位1-mw 2-uw 3-nw 4-dBm或dB
                        {
                            case 0x30: k = 1000; break;
                            case 0x20: k = 100; break;
                            case 0x10: k = 10; break;
                        }
                        switch (ReadBuffer[9] & 0x07)
                        {
                            case 1: pwrValue = Convert.ToSingle((dispdata) / k); pwrValue = Convert.ToSingle(10 * Math.Log10(pwrValue + 1E-6)); break;//mw->dBm  
                            case 2: pwrValue = Convert.ToSingle((dispdata) / k); pwrValue = Convert.ToSingle(10 * Math.Log10(pwrValue / 1000 + 1E-6)); break;//uw->dBm 
                            case 3: pwrValue = Convert.ToSingle((dispdata) / k); pwrValue = Convert.ToSingle(10 * Math.Log10(pwrValue / 1000000 + 1E-6)); break;//nw->dBm 
                            case 4: pwrValue = Convert.ToSingle((dispdata - 9000) / 100.0); break;//dBm                                                       
                            default: break;
                        }
                        return pwrValue;
                    }
                    else
                    {
                        return -100;
                    }
                }
                catch // (TimeoutException ex)
                {
                    //MessageBox.Show("光功率计读取错误！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -100;
                }
            }
            else //if (meterType_comboBox.SelectedIndex == 1) //台式光功率计 普塞斯PSS
            {
                byte[] WriteBuffer = new byte[21] { 0x52, 0x65, 0x61, 0x64, 0x3A, 0x50, 0x6F, 0x77, 0x65, 0x72, 0x20, 0x43, 0x68, 0x61, 0x6E, 0x6E, 0x65, 0x6C, 0x31, 0x0D, 0x0A };
                byte[] ReadBuffer = new byte[20];
                int readLen_rtn;
                string str;
                //
                Thread.Sleep((int)MEMTER.delay); // 延时
                //
                try
                {
                    GlobalVarFun.uartMeter.Write(WriteBuffer, 0, 21);
                    Thread.Sleep(300);
                    readLen_rtn = GlobalVarFun.uartMeter.Read(ReadBuffer, 0, 9);
                    if ((readLen_rtn < 7) || (readLen_rtn > 10)) //长度判断
                    {
                        return -100;
                    }

                    if (ReadBuffer[readLen_rtn - 1] == 0x0A)
                    {
                        str = System.Text.Encoding.ASCII.GetString(ReadBuffer);
                        str = str.Substring(0, readLen_rtn - 1);
                        pwrValue = Convert.ToSingle(str);
                        return pwrValue;
                    }
                    else
                    {
                        //return -100;
                        str = System.Text.Encoding.ASCII.GetString(ReadBuffer);//解决4通道功率计无法获取功率值问题 2025.05.09
                        if (str.Contains("annel"))
                        {
                            str = GlobalVarFun.uartMeter.ReadLine();
                            pwrValue = Convert.ToSingle(str);
                            return pwrValue;
                        }
                        else
                        {
                            return -100;
                        }
                    }
                }
                catch // (TimeoutException ex)
                {
                    //MessageBox.Show("光功率计读取错误！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return -100;
                }
            }
            //return -100;
        }

        //从示波器获取光功率
        private float Get_OptoPower_DCA()
        {
            Thread.Sleep(100); // 延时100ms
            //
            try
            {
                if (GlobalVarFun.DCA86100D_Open || GlobalVarFun.N1092x_Open)
                {
                    float apower = GlobalVarFun.scope_86100d.GetPower(gpibAddress);
                    return apower;
                }
                else
                {
                    GlobalVarFun.scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeOscilloscope;
                    GlobalVarFun.scope.System.IO.WriteString(":CDISPLAY", true);
                    GlobalVarFun.scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    //scope.System.IO.WriteString(":MEASURE:APOWER? DECibel,CHANNEL1", true);
                    GlobalVarFun.scope.System.IO.WriteString(":MEASURE:APOWER? DECibel", true);
                    GlobalVarFun.scope.System.IO.WriteString(":MEASURE:APOWER? DECibel", true);
                    String str = GlobalVarFun.scope.System.IO.ReadString();
                    GlobalVarFun.scope.System.EnableLocalControls();
                    return Convert.ToSingle(str);
                }
            }
            catch //(Exception exp)
            {
                //MessageBox.Show(exp.Message);
                //MessageBox.Show("眼图仪读取错误！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -100; // Error
            }
        }

        //从示波器获取消光比
        private bool Get_ERatio_DCA(bool autoScale)
        {
            float tx_er = 0;
            int ch = TestSet.ch;           
            try
            {
                if (GlobalVarFun.DCA86100D_Open || GlobalVarFun.N1092x_Open)
                {
                    //float er = 0;
                    if (autoScale == true)
                    {
                        GlobalVarFun.scope_86100d.SetAutoScale(gpibAddress, 25);
                    }
                    GlobalVarFun.scope_86100d.SetClearDisplay(gpibAddress, 10);
                    GlobalVarFun.scope_86100d.SetRun(gpibAddress);
                    //int delay = (int)DelaynumericUpDown10.Value;
                    //Thread.Sleep(delay+200);
                    //等待刷新
                    Thread.Sleep(3500);

                    tx_er = GlobalVarFun.scope_86100d.GetExtRatio(gpibAddress);
                }
                else
                {
                    GlobalVarFun.scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                    GlobalVarFun.scope.System.IO.WriteString(":CDISPLAY", true);
                    GlobalVarFun.scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                    GlobalVarFun.scope.System.IO.WriteString(":RUN", true);
                    if (autoScale == true)
                    {
                        GlobalVarFun.scope.System.IO.WriteString(":AUToscale", true);
                    }
                    //scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel,CHANNEL1", true);
                    GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel", true);
                    String str = GlobalVarFun.scope.System.IO.ReadString();
                    GlobalVarFun.scope.System.EnableLocalControls();
                    tx_er = Convert.ToSingle(str);
                }
                //
                if ((tx_er > 50) || (tx_er < 0.5)) // 异常 再测一次
                {
                    Thread.Sleep(100);
                    //
                    if (GlobalVarFun.DCA86100D_Open || GlobalVarFun.N1092x_Open)
                    {
                        GlobalVarFun.scope_86100d.SetClearDisplay(gpibAddress, 10);
                        Thread.Sleep(100);
                        tx_er = GlobalVarFun.scope_86100d.GetExtRatio(gpibAddress);
                    }
                    else
                    {
                        GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel", true);
                        String str = GlobalVarFun.scope.System.IO.ReadString();
                        GlobalVarFun.scope.System.EnableLocalControls();
                        tx_er = Convert.ToSingle(str);
                    }
                }
                //
                //tx_er += (float)(GlobalVarFun.ER_cal_num); // 加设备偏差值
                if ((tx_er > 50) || (tx_er < 0.5)) // 异常
                {
                    TestResult.txErbuf[ch] = 0;
                    return false;
                }
                TestResult.txErbuf[ch] = tx_er;
                return true;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                TestResult.txErbuf[ch] = 0;
                return false;
            }
        }

        //从示波器获取消光比等参数
        private bool Get_TxEyeData_DCA(bool autoScale)
        {
            int intWaveForms = 0;
            int intWaveForms_old = -1;
            int intMaxWaveForms = 100;
            int ch = 0;
            ch = TestSet.ch;
            float tx_er = 0;
            string str = null;

            try
            {
                GlobalVarFun.scope.System.Mode = AgilentInfiniiumDCAModeEnum.AgilentInfiniiumDCAModeEye;
                GlobalVarFun.scope.System.IO.WriteString(":CDISPLAY", true);
                GlobalVarFun.scope.System.IO.WriteString(":SYSTEM:HEADER OFF", true);
                GlobalVarFun.scope.System.IO.WriteString(":MEASURE:SEND OFF", true);
                GlobalVarFun.scope.System.IO.WriteString("*CLS", true);
                GlobalVarFun.scope.System.IO.WriteString(":RUN", true);
                if (autoScale == true)
                {
                    GlobalVarFun.scope.System.IO.WriteString(":AUToscale", true);
                }
                //scope.System.WaitForOperationComplete(5000); // 等待完成
                //scope.System.IO.WriteString(":*OPC?", true);
                //scope.System.IO.WriteString(":MTESt:TEST ON", true);
                //Thread.Sleep(5000);
                //scope.System.IO.WriteString(":MTESt:COUNt:WAVeforms?", true);
                //str = scope.System.IO.ReadString();
                //scope.System.IO.WriteString(":MTESt:TEST OFF", true);

                Thread.Sleep(100);

                // 终测  新增界面参数显示
                if (GlobalVarFun.testType == "finalTest")
                {
                    GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO DECibel", true);
                    GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER PP", true);
                    GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:ESN", true);
                    GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:CROSsing", true);
                }
                // 终测  模板测试
                if ((GlobalVarFun.testType == "finalTest") && (TestResult.waveforms_count >= 100) && (eyeMaskIsOpened == true))
                {
                    GlobalVarFun.scope.System.IO.WriteString(":MTESt:TEST ON", true); //开模板mask显示
                    GlobalVarFun.scope.System.IO.WriteString(":MTESt:START", true);
                    //

                    //等待波形计数
                    intMaxWaveForms = TestResult.waveforms_count;
                    while (intWaveForms < intMaxWaveForms)
                    {
                        Thread.Sleep(1000);
                        GlobalVarFun.scope.System.IO.WriteString(":MTESt:COUNt:WAVeforms?", true);
                        str = GlobalVarFun.scope.System.IO.ReadString();
                        intWaveForms = Convert.ToInt32(Convert.ToSingle(str));
                        //
                        if (intWaveForms_old < 50)
                        {
                            GlobalVarFun.scope.System.IO.WriteString(":MTESt:START", true);
                        }
                        //
                        if (intWaveForms <= intWaveForms_old) //读取的参数异常
                        {
                            AddTestLog("示波器86100眼图累计点读取错误！");
                            return false;
                        }
                        //
                        intWaveForms_old = intWaveForms;
                    }

                    //判断是否有fail点落在模板内
                    //scope.System.IO.WriteString(":MTESt:START", true);
                    GlobalVarFun.scope.System.IO.WriteString(":MTESt:COUNt:FSAMples?", true);
                    str = GlobalVarFun.scope.System.IO.ReadString();
                    intWaveForms = Convert.ToInt32(Convert.ToSingle(str));
                    if (intWaveForms > 0)
                    {
                        AddTestLog("眼图模板测试时出现散点 FAIL!");
                        return false;
                    }

                    AddTestLog("眼图模板测试成功=" + intMaxWaveForms.ToString());
                }

                // opto power
                GlobalVarFun.scope.System.IO.WriteString(":MEASURE:APOWER? DECibel", true);
                str = GlobalVarFun.scope.System.IO.ReadString();
                TestResult.txPowerDCA = Convert.ToSingle(str);

                // ER
                GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:ERATIO? DECibel", true);
                str = GlobalVarFun.scope.System.IO.ReadString();
                tx_er = Convert.ToSingle(str);
                tx_er += (float)(Agilent86100.ER_offset); // 加偏差
                TestResult.txErbuf[ch] = tx_er;

                // Crossing
                GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:CROSsing?", true);
                str = GlobalVarFun.scope.System.IO.ReadString();
                TestResult.txCrossingbuf[ch] = Convert.ToSingle(str);

                // Jitter RMS
                GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? RMS", true);
                str = GlobalVarFun.scope.System.IO.ReadString();
                TestResult.txJiterRMSbuf[ch] = (float)(Convert.ToSingle(str) * 1e12); // 单位 ps

                // Jitter PP
                GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:JITTER? PP", true);
                str = GlobalVarFun.scope.System.IO.ReadString();
                TestResult.txJiterPPbuf[ch] = (float)(Convert.ToSingle(str) * 1e12); // 单位 ps

                //Jitter Total //2019.2.15 add
                if (GlobalVarFun.moduleType == "SFP+" || GlobalVarFun.moduleType == "XFP")
                {
                    TestResult.txJiterTTbuf[ch] = TestResult.txJiterRMS + TestResult.txJiterPP;
                }
                else
                {
                    TestResult.txJiterTTbuf[ch] = (TestResult.txJiterRMS * 14) + TestResult.txJiterPP;
                }

                // ESN
                GlobalVarFun.scope.System.IO.WriteString(":MEASURE:CGRADE:ESN?", true);
                str = GlobalVarFun.scope.System.IO.ReadString();
                TestResult.txESNbuf[ch] = Convert.ToSingle(str);

                // 终测  从示波器86100 截取眼图gif
                TestResult.bimage_len = 0;
                if ((GlobalVarFun.testType == "finalTest") && (GlobalVarFun.tx_eye_save_test))// && (TestResult.waveforms_count >= 100))
                {
                    byte[] byteArray = null;
                    //scope.System.GetScreenBitmap(ref byteArray);
                    object obj = new object();
                    GlobalVarFun.scope.System.IO.WriteString(":DISPlay:DATA? GIF,SCReen,NORMal", true);
                    obj = GlobalVarFun.scope.System.IO.ReadIEEEBlock(Ivi.Visa.Interop.IEEEBinaryType.BinaryType_UI1, false, true);
                    
                    //将86100读取的二进制数据流转为Byte[]
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter binFormatter = new BinaryFormatter();
                        binFormatter.Serialize(ms, obj);
                        byteArray = ms.GetBuffer();
                    }

                    TestResult.bimage_len = (byteArray.Length / 2) - 27; //有效长度

                    if ((TestResult.bimage_len < 1000) || (TestResult.bimage_len > 100000)) // 1k-100k Bytes
                    {
                        AddTestLog("GIF眼图 长度错误！");
                        return false;
                    }
                    //TestResult.txEye_imagebuf = new byte[4, TestResult.bimage_len];// 重新定义Byte[]数组大小
                    if (ch == 0)
                    {
                        TestResult.txEye_image_ch0 = new byte[TestResult.bimage_len]; // 重新定义Byte[]数组大小
                        for (int i = 0; i < TestResult.bimage_len; i++)
                        {
                            TestResult.txEye_image_ch0[i] = byteArray[i + 27];                    
                        }
                        if ((TestResult.txEye_image_ch0[0] != 0x47) || (TestResult.txEye_image_ch0[1] != 0x49) || (TestResult.txEye_image_ch0[2] != 0x46) || (TestResult.txEye_image_ch0[3] != 0x38) || (TestResult.txEye_image_ch0[TestResult.bimage_len - 1] != 0x3B)) // GIF8
                        {
                            AddTestLog("GIF眼图 头尾标识错误！");
                            return false;
                        }
                        AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());
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
                            bw.Write(TestResult.txEye_image_ch0, 0, TestResult.bimage_len);
                            //bw.Write(byteArray, 0, byteArray.Length);
                            bw.Close();
                            fs.Close();
                        }
                    }
                    else if (ch == 1)
                    {
                        TestResult.txEye_image_ch1 = new byte[TestResult.bimage_len]; // 重新定义Byte[]数组大小
                        for (int i = 0; i < TestResult.bimage_len; i++)
                        {                       
                            TestResult.txEye_image_ch1[i] = byteArray[i + 27];                           
                        }
                        if ((TestResult.txEye_image_ch1[0] != 0x47) || (TestResult.txEye_image_ch1[1] != 0x49) || (TestResult.txEye_image_ch1[2] != 0x46) || (TestResult.txEye_image_ch1[3] != 0x38) || (TestResult.txEye_image_ch1[TestResult.bimage_len - 1] != 0x3B)) // GIF8
                        {
                            AddTestLog("GIF眼图 头尾标识错误！");
                            return false;
                        }
                        AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());
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
                            bw.Write(TestResult.txEye_image_ch1, 0, TestResult.bimage_len);
                            //bw.Write(byteArray, 0, byteArray.Length);
                            bw.Close();
                            fs.Close();
                        }
                    }
                    else if (ch == 2)
                    {
                        TestResult.txEye_image_ch2 = new byte[TestResult.bimage_len]; // 重新定义Byte[]数组大小
                        for (int i = 0; i < TestResult.bimage_len; i++)
                        {                          
                            TestResult.txEye_image_ch2[i] = byteArray[i + 27];                      
                        }
                        if ((TestResult.txEye_image_ch2[0] != 0x47) || (TestResult.txEye_image_ch2[1] != 0x49) || (TestResult.txEye_image_ch2[2] != 0x46) || (TestResult.txEye_image_ch2[3] != 0x38) || (TestResult.txEye_image_ch2[TestResult.bimage_len - 1] != 0x3B)) // GIF8
                        {
                            AddTestLog("GIF眼图 头尾标识错误！");
                            return false;
                        }
                        AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());
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
                            bw.Write(TestResult.txEye_image_ch2, 0, TestResult.bimage_len);
                            //bw.Write(byteArray, 0, byteArray.Length);
                            bw.Close();
                            fs.Close();
                        }
                    }
                    else if (ch == 3)
                    {
                        TestResult.txEye_image_ch3 = new byte[TestResult.bimage_len]; // 重新定义Byte[]数组大小
                        for (int i = 0; i < TestResult.bimage_len; i++)
                        {
                            TestResult.txEye_image_ch3[i] = byteArray[i + 27];
                        }
                        if ((TestResult.txEye_image_ch3[0] != 0x47) || (TestResult.txEye_image_ch3[1] != 0x49) || (TestResult.txEye_image_ch3[2] != 0x46) || (TestResult.txEye_image_ch3[3] != 0x38) || (TestResult.txEye_image_ch3[TestResult.bimage_len - 1] != 0x3B)) // GIF8
                        {
                            AddTestLog("GIF眼图 头尾标识错误！");
                            return false;
                        }
                        AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());
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
                            bw.Write(TestResult.txEye_image_ch3, 0, TestResult.bimage_len);
                            //bw.Write(byteArray, 0, byteArray.Length);
                            bw.Close();
                            fs.Close();
                        }
                    }
                    else
                    {
                        return false;
                    }

                    //for (int i = 0; i < TestResult.bimage_len; i++)
                    //{
                    //    //TestResult.txEye_imagebuf[ch,i] = byteArray[i + 27];
                    //    TestResult.txEye_image_ch0[i] = byteArray[i + 27];
                    //    //TestResult.txEye_image_ch1[i] = byteArray[i + 27];
                    //    //TestResult.txEye_image_ch2[i] = byteArray[i + 27];
                    //    //TestResult.txEye_image_ch3[i] = byteArray[i + 27];
                    //}

                    //if ((TestResult.txEye_imagebuf[ch, 0] != 0x47) || (TestResult.txEye_imagebuf[ch, 1] != 0x49) || (TestResult.txEye_imagebuf[ch, 2] != 0x46) || (TestResult.txEye_imagebuf[ch, 3] != 0x38) || (TestResult.txEye_imagebuf[ch,TestResult.bimage_len - 1] != 0x3B)) // GIF8
                    //{
                    //    AddTestLog("GIF眼图 头尾标识错误！");
                    //    return false;
                    //}

                    //AddTestLog("GIF眼图bytes=" + TestResult.bimage_len.ToString());

                    //for (int i = 0; i < TestResult.bimage_len; i++)
                    //{
                    //    TestResult.txEye_image_ch0[i] = TestResult.txEye_imagebuf[ch, i];
                    //}
                    //保存眼图 test  存放到本机 C:\
                    //using (MemoryStream memoryStream = new MemoryStream())
                    //{
                    //    string strFilePath = "C:\\1.gif"; //Application.StartupPath + "\\image\\" + "1.gif";

                    //    if (File.Exists(strFilePath)) //检查1.gif是否存在,如已存在,先删除.
                    //    {
                    //        File.Delete(strFilePath);
                    //    }

                    //    FileStream fs = new FileStream(strFilePath, FileMode.Append, FileAccess.Write);
                    //    BinaryWriter bw = new BinaryWriter(fs);                                     
                    //    bw.Write(TestResult.txEye_image_ch0, 0, TestResult.bimage_len);                                     
                    //    //bw.Write(byteArray, 0, byteArray.Length);
                    //    bw.Close();
                    //    fs.Close();
                    //}
                }

                //
                GlobalVarFun.scope.System.EnableLocalControls();
                return true;
            }
            catch //(Exception exp)
            {
                //MessageBox.Show(exp.Message);
                AddTestLog("从示波器86100读取ER/CP/Jitter等参数错误！");
                return false;
            }
        }

        //86100D获取眼图参数
        private bool Get_86100D_TxEyeData_DCA(bool autoScale)
        {           
            float tx_er = 0;
            int ch = 0;
            ch = TestSet.ch;
            try
            {
                GlobalVarFun.scope_86100d.SetClearDisplay(gpibAddress, 10);
                GlobalVarFun.scope_86100d.SetRun(gpibAddress);

                if (autoScale == true)
                {
                    GlobalVarFun.scope_86100d.SetAutoScale(gpibAddress, 25);
                }             
                //等待刷新
                Thread.Sleep(3500);              
                //初测
                if ((GlobalVarFun.testType == "firstTest"))
                {
                    TestResult.txPowerDCA = GlobalVarFun.scope_86100d.GetPower(gpibAddress);
                    // ER                 
                    tx_er = GlobalVarFun.scope_86100d.GetExtRatio(gpibAddress);
                    tx_er += (float)(GlobalVarFun.ER_cal_num); // 加偏差
                    TestResult.txEr = tx_er;
                    // Crossing               
                    TestResult.txCrossing = GlobalVarFun.scope_86100d.GetCrossing(gpibAddress);
                    // Jitter RMS                   
                    TestResult.txJiterRMS = (float)(GlobalVarFun.scope_86100d.GetJitterRMS(gpibAddress));// 单位 ps
                    // Jitter PP
                    TestResult.txJiterPP = (float)(GlobalVarFun.scope_86100d.GetJitterPP(gpibAddress));// 单位 ps
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
                    //TestResult.TxRiseTime = (float)(GlobalVarFun.scope_86100d.GetRiseTime(gpibAddress) * Math.Pow(10, 12));
                    //FallTime //2024.12.07                  
                    //TestResult.TxFallTime = (float)(GlobalVarFun.scope_86100d.GetFallTime(gpibAddress) * Math.Pow(10, 12));
                    // ESN             
                    TestResult.txESN = GlobalVarFun.scope_86100d.GetEyeSNR(gpibAddress);
                }

                // 终测  模板测试
                if ((GlobalVarFun.testType == "finalTest"))
                {

                    TestResult.txPowerDCA = GlobalVarFun.scope_86100d.GetPower(gpibAddress);
                    // ER                 
                    tx_er = GlobalVarFun.scope_86100d.GetExtRatio(gpibAddress);
                    tx_er += (float)(GlobalVarFun.ER_cal_num); // 加偏差
                    //TestResult.txEr = tx_er;
                    TestResult.txErbuf[ch] = tx_er;
                    // Crossing               
                    TestResult.txCrossingbuf[ch] = GlobalVarFun.scope_86100d.GetCrossing(gpibAddress);
                    // Jitter RMS                   
                    TestResult.txJiterRMSbuf[ch] = (float)(GlobalVarFun.scope_86100d.GetJitterRMS(gpibAddress));// 单位 ps
                    // Jitter PP
                    TestResult.txJiterPPbuf[ch] = (float)(GlobalVarFun.scope_86100d.GetJitterPP(gpibAddress));// 单位 ps
                    //Jitter Total //2019.2.15 add
                    if (GlobalVarFun.moduleType == "SFP+" || GlobalVarFun.moduleType == "XFP")
                    {
                        TestResult.txJiterTTbuf[ch] = TestResult.txJiterRMS + TestResult.txJiterPP;
                    }
                    else
                    {
                        TestResult.txJiterTTbuf[ch] = (TestResult.txJiterRMS * 14) + TestResult.txJiterPP;
                    }
                    //RiseTime //2024.12.07                  
                    //TestResult.TxRiseTime = (float)(GlobalVarFun.scope_86100d.GetRiseTime(gpibAddress) * Math.Pow(10, 12));
                    //FallTime //2024.12.07                  
                    //TestResult.TxFallTime = (float)(GlobalVarFun.scope_86100d.GetFallTime(gpibAddress) * Math.Pow(10, 12));
                    // ESN             
                    TestResult.txESNbuf[ch] = GlobalVarFun.scope_86100d.GetEyeSNR(gpibAddress);
                    // 终测  从示波器86100 截取眼图gif
                    TestResult.bimage_len = 0;
                    if ((GlobalVarFun.testType == "finalTest") && (GlobalVarFun.tx_eye_save_test))// && (TestResult.waveforms_count >= 100))
                    {
                        //
                    }
                }            
                return true;
            }
            catch //(Exception exp)
            {
              AddTestLog("从示波器86100读取ER/CP/Jitter等参数错误！");              
              return false;
            }

        }
        ////连接86100DCA
        //private void connection_button_Click(object sender, EventArgs e)
        //{
        //    //更新眼图测试最大累计点
        //    TestResult.waveforms_count = Convert.ToInt32(GlobalVarFun.waveforms_num);

        //    eyeMaskIsOpened = false;

        //    try
        //    {
        //        if (instrument_connected == false)
        //        {
        //            if (scope.Initialized)
        //            {
        //                scope.Close();
        //            }
        //            scope.Initialize("GPIB0::07::INSTR", false, false, "");
        //            //scope.System.WaitForOperationComplete(1000); // 等待完成
        //            scope.System.IO.WriteString(":CHANnel1:DISPlay ON", true); // Channel 1 On
        //            scope.System.IO.WriteString("*CLS", true);
        //            //
        //            if ((GlobalVarFun.testType == "finalTest") && (TestResult.waveforms_count >= 100))
        //            {
        //                if ((TestResult.mask_margin > 90) || (TestResult.mask_margin < 5))
        //                {
        //                    MessageBox.Show("眼图模板Margin超出范围(5-90%)！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                    return;
        //                }
        //                if (string.IsNullOrEmpty(TestResult.mask_name))
        //                {
        //                    MessageBox.Show("眼图模板名字为空！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //                    return;
        //                }
        //                scope.System.IO.WriteString(":MTESt:ARUN OFF", true); //关闭自动模板测试
        //                scope.System.IO.WriteString(":MTESt:LOAD '" + TestResult.mask_name + "'", true); //打开眼图模板
        //                scope.System.IO.WriteString(":MTEST:MMARgin:STATe ON", true);
        //                scope.System.IO.WriteString(":MTEST:MMARgin:PERCent " + TestResult.mask_margin.ToString(), true);
        //                scope.System.IO.WriteString(":MTESt:TEST ON", true);
        //                //
        //                eyeMaskIsOpened = true;
        //            }
        //            else
        //            {
        //                scope.System.IO.WriteString(":MTESt:TEST OFF", true);
        //            }
        //            //
        //            scope.System.TimeoutMilliseconds = 10000; //timeout
        //            scope.System.EnableLocalControls();
        //            instrument_connected = true;
        //            //connection_button.Text = "Get Connect";
        //            connection_button.BackColor = System.Drawing.Color.GreenYellow;
        //        }
        //        else
        //        {
        //            scope.Close();
        //            instrument_connected = false;
        //            //connection_button.Text = "No Connect";
        //            connection_button.BackColor = System.Drawing.Color.Gray;
        //        }
        //    }
        //    catch (Exception exp)
        //    {
        //        MessageBox.Show(exp.Message);
        //        connection_button.BackColor = System.Drawing.Color.Yellow;
        //        //System.Windows.Forms.Application.Exit();
        //    }
        //}

        //// 连接光功率计
        //private void conntMeter_button_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        if (optoMeter_connected == false)
        //        {
        //            if (uartMeter != null)
        //            {
        //                if (uartMeter.IsOpen)
        //                {
        //                    uartMeter.Close();
        //                }
        //            }
        //            //
        //            if (meterType_comboBox.SelectedIndex == 0) //手持光功率计 光讯
        //            {
        //                uartMeter.PortName = meterCom_comboBox.Text;
        //                uartMeter.BaudRate = 9600;
        //                uartMeter.ReadTimeout = 1000;
        //                uartMeter.Open();
        //                byte[] WriteBuffer = new byte[7] { 0xef, 0xef, 0x04, 0x04, 0x60, 0x06, 0x4c };
        //                byte[] ReadBuffer = new byte[14];
        //                uartMeter.Write(WriteBuffer, 0, 7);
        //                Thread.Sleep(100);
        //                uartMeter.Read(ReadBuffer, 0, 14);
        //                if ((ReadBuffer[0] == 0xed) && (ReadBuffer[1] == 0xfa))
        //                {
        //                    optoMeter_connected = true;
        //                    meterCom_comboBox.Enabled = false;
        //                    meterType_comboBox.Enabled = false;
        //                    //opto_meter_button.Text = "Get Connect";
        //                    conntMeter_button.BackColor = System.Drawing.Color.GreenYellow;
        //                    return;
        //                }
        //                else
        //                {
        //                    uartMeter.Close();
        //                    optoMeter_connected = false;
        //                    conntMeter_button.BackColor = System.Drawing.Color.Gray;
        //                }
        //            }
        //            else //if (meterType_comboBox.SelectedIndex == 1) //台式光功率计 普塞斯PSS
        //            {
        //                uartMeter.PortName = meterCom_comboBox.Text;
        //                uartMeter.BaudRate = 115200;
        //                uartMeter.ReadTimeout = 1000;
        //                uartMeter.Open();
        //                byte[] WriteBuffer = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
        //                byte[] ReadBuffer = new byte[40];
        //                uartMeter.Write(WriteBuffer, 0, 7);
        //                Thread.Sleep(300);
        //                uartMeter.Read(ReadBuffer, 0, 36);
        //                if ((ReadBuffer[0] == 0x50) && (ReadBuffer[1] == 0x53) && (ReadBuffer[2] == 0x53) && 
        //                    (ReadBuffer[4] == 0x4F) && (ReadBuffer[5] == 0x50) && (ReadBuffer[6] == 0x4D))
        //                {
        //                    optoMeter_connected = true;
        //                    meterCom_comboBox.Enabled = false;
        //                    meterType_comboBox.Enabled = false;
        //                    //opto_meter_button.Text = "Get Connect";
        //                    conntMeter_button.BackColor = System.Drawing.Color.GreenYellow;
        //                    return;
        //                }
        //                else
        //                {
        //                    uartMeter.Close();
        //                    optoMeter_connected = false;
        //                    conntMeter_button.BackColor = System.Drawing.Color.Gray;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            uartMeter.Close();
        //            optoMeter_connected = false;
        //            meterCom_comboBox.Enabled = true;
        //            meterType_comboBox.Enabled = true;
        //            //opto_meter_button.Text = "No Connect";
        //            conntMeter_button.BackColor = System.Drawing.Color.Gray;
        //        }
        //    }
        //    catch
        //    {
        //        optoMeter_connected = false;
        //        //opto_meter_button.Text = "No Connect";
        //        conntMeter_button.BackColor = System.Drawing.Color.Yellow;
        //    }
        //}

        //// 连接光光衰减器
        //private void conntAtt_button_Click(object sender, EventArgs e)
        //{
        //    //2A 49 44 4E 3F 0D 0A
        //    byte[] WriteBuffer = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
        //    byte[] ReadBuffer = new byte[40];
        //    int uart_readLen_rtn;

        //    try
        //    {
        //        if (optoAtt_connected == false)
        //        {
        //            if (uartAtt != null)
        //            {
        //                if (uartAtt.IsOpen)
        //                {
        //                    uartAtt.Close();
        //                }
        //            }
        //            //
        //            uartAtt.PortName = attCom_comboBox.Text;
        //            uartAtt.BaudRate = 115200;
        //            uartAtt.ReadTimeout = 1000;
        //            uartAtt.Open();
        //            ReadBuffer[0] = 0xFF;
        //            uartAtt.Write(WriteBuffer, 0, 7);
        //            Thread.Sleep(100);
        //            uart_readLen_rtn = uartAtt.Read(ReadBuffer, 0, 34);
        //            if ((ReadBuffer[0] == 0x50) && (ReadBuffer[1] == 0x53) && (ReadBuffer[2] == 0x53) && (uart_readLen_rtn == 34))
        //            {
        //                optoAtt_connected = true;
        //                attCom_comboBox.Enabled = false;
        //                //opto_meter_button.Text = "Get Connect";
        //                conntAtt_button.BackColor = System.Drawing.Color.GreenYellow;
        //                return;
        //            }
        //            else
        //            {
        //                uartAtt.Close();
        //                optoAtt_connected = false;
        //                conntAtt_button.BackColor = System.Drawing.Color.Gray;
        //            }
        //        }
        //        else
        //        {
        //            uartAtt.Close();
        //            optoAtt_connected = false;
        //            attCom_comboBox.Enabled = true;
        //            //opto_meter_button.Text = "No Connect";
        //            conntAtt_button.BackColor = System.Drawing.Color.Gray;
        //        }
        //    }
        //    catch
        //    {
        //        optoAtt_connected = false;
        //        //opto_meter_button.Text = "No Connect";
        //        conntAtt_button.BackColor = System.Drawing.Color.Yellow;
        //    }
        //}
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //

        //设置图片框控件
        private void SetLED(PictureBox picbox, bool bit_value)
        {
            if (bit_value)
                picbox.Image = imageList1.Images["LedRed.ico"];
            else
                picbox.Image = imageList1.Images["LedGreen.ico"];
        }

        //读取模块信息
        private void Read_moduleInfo()
        {
            if (test.GetFlashInfo() == false)
            {
                return;
            }

            //写码信息
            sn_textBox.Text = TestResult.sn;
            pn_textBox.Text = TestResult.pn;
            vn_textBox.Text = TestResult.vn;
            date_textBox.Text = TestResult.date;
           // Refresh();
        }
      
        //更新告警信息
        private void Read_Flags_and_Interrupt()
        {
            if (test.GetDDMFlagsInterrupt() == false)
            {
                return;
            }
            //
            SetLED(temp_ha_pictureBox, TestResult.tempHA_flag);
            SetLED(temp_la_pictureBox, TestResult.tempLA_flag);
            SetLED(vcc_ha_pictureBox, TestResult.vccHA_flag);
            SetLED(vcc_la_pictureBox, TestResult.vccLA_flag);

            SetLED(temp_hw_pictureBox, TestResult.tempHW_flag);
            SetLED(temp_lw_pictureBox, TestResult.tempLW_flag);
            SetLED(vcc_hw_pictureBox, TestResult.vccHW_flag);
            SetLED(vcc_lw_pictureBox, TestResult.vccLW_flag);

            SetLED(bias_ha_pictureBox, TestResult.txBiasHA_flag);
            SetLED(bias_la_pictureBox, TestResult.txBiasLA_flag);
            SetLED(txpwr_ha_pictureBox, TestResult.txPwrHA_flag);
            SetLED(txpwr_la_pictureBox, TestResult.txPwrLA_flag);
            SetLED(rxpwr_ha_pictureBox, TestResult.rxPwrHA_flag);
            SetLED(rxpwr_la_pictureBox, TestResult.rxPwrLA_flag);

            SetLED(bias_hw_pictureBox, TestResult.txBiasHW_flag);
            SetLED(bias_lw_pictureBox, TestResult.txBiasLW_flag);
            SetLED(txpwr_hw_pictureBox, TestResult.txPwrHW_flag);
            SetLED(txpwr_lw_pictureBox, TestResult.txPwrLW_flag);
            SetLED(rxpwr_hw_pictureBox, TestResult.rxPwrHW_flag);
            SetLED(rxpwr_lw_pictureBox, TestResult.rxPwrLW_flag);
            //
            //Refresh();
        }

        //更新监控数据
        private void Converted_analog_values()
        {
            if (test.GetDDMAnalogValues() == false)
            {
                return;
            }
            Temp_textBox.Text = (TestResult.tempDDM).ToString("F2");
            Vcc_textBox.Text = (TestResult.vccDDM).ToString("F2");
            Bias_textBox.Text = (TestResult.bias_ddm).ToString();
            TxPWR_textBox.Text = (TestResult.txpwr_ddm).ToString();
            RxPWR_textBox.Text = (TestResult.rxpwr_ddm).ToString();

           // Refresh();
        }

        //更新 模块 类型/速率/版本/状态  等信息
        private bool ShowCheckModuleStatus()
        {     
            byte[] temp = new byte[4];
            SelectTable(6); //重要:定时器结束时需选择表06
            if (i2c.TWI_ReadPage(0xa0, 0xFC, temp, 4) != 4) return false;
  
            String str, strRate;
            byte hardwareVer = temp[0];
            byte firmwareVer = temp[1];

            str = string.Format("设计方案{0}  ", (hardwareVer & 0x0F).ToString("D"));

            strRate = "000";
            switch (hardwareVer & 0xF0)
            {
                case 0x10:
                    str += "40G";
                    strRate = "40G";
                    break;
                case 0x20:
                    str += "100G";
                    strRate = "100G";
                    break;
                case 0x30:
                    str += "100G/112G 双速率 ";
                    strRate = "100G";
                    break;
                default:
                    str += " ";
                    break;
            }

            str += " ";
            if (strRate == "40G")
            {
                switch (hardwareVer & 0x0F)
                {
                    case 0x01:
                        str += "MAX24040";
                        break;
                    case 0x02:
                        str += "4xGN1157";
                        break;
                    case 0x03:
                        str += "PHXT8104+PHXR8104";
                        break;
                    case 0x04:
                        str += "  ";
                        break;
                    case 0x05:
                        str += "37045+37044";
                        break;
                    case 0x06:
                        str += "24025+37046";
                        break;
                    case 0x07:
                        str += "24025+2110S";
                        break;
                    case 0x08:
                        str += "037057+37046";
                        break;
                    default:
                        str += "Reserved";
                        break;
                }
            }
            else if (strRate == "100G")
            {
                switch (hardwareVer & 0x0F)
                {
                    case 0x01:
                        str += "37049+37046+011039+002304";
                        break;
                    case 0x02:
                        str += "24028+37046";
                        break;
                    case 0x03:
                        str += "37049+37046+1185";
                        break;
                    case 0x04:
                        str += "37059+37244";
                        break;
                    case 0x05:
                        str += "37045+37044";
                        break;
                    case 0x06:
                        str += "24025+37046";
                        break;
                    case 0x07:
                        str += "24025+2110S";
                        break;
                    case 0x08:
                        str += "037057+37046";
                        break;
                    case 0x09:
                        str += "UX2291+2091";
                        break;
                    default:
                        str += "Reserved";
                        break;
                }
            }
            else
            {
                str += "未定义";
            }

            str += " ";
            switch (firmwareVer & 0xE0)
            {
                case 0x20:
                    str += "SR4";
                    break;
                case 0x40:
                    str += "CW4";
                    break;
                case 0x60:
                    str += "LR4";
                    break;
                case 0x80:
                    str += "ER4";
                    break;
                case 0xA0:
                    str += "ZR4";
                    break;
                case 0xC0:
                    str += "PAM4";
                    break;
                default:
                    str += "未知";
                    break;
            }
            str += string.Format("  软件版本:V{0}  ", (firmwareVer & 0x0F).ToString("D"));

            toolStripStatusLabel1.Text = "QSFP: " + str;

            return true;
        }

        // 表选择
        private bool SelectTable(byte tbl)
        {
            return i2c.TWI_WriteByte(0xA0, 127, tbl);
        }

        // 读取待测模块的DDM信息
        private void ShowModuleDdmInfo()
        {
            Converted_analog_values();
            Read_Flags_and_Interrupt();
           // Read_AlarmWarn_Thresholds();
            //General_Control_Status_Bits();
            Refresh();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //初始化后台代理
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
            if (SaveRecordToSQL() == false) // 保存测试记录到SQL数据库
            {
                GlobalVarFun.sql_record_status = false; // SQL 写入记录 操作 Error(需要再次开始自动测试才能消除此error)
                MessageBox.Show("SQL数据库保存测试记录异常！请停止测试并检查连接！！\r\n ", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
            }
            /*else
            {
                GlobalVarFun.sql_record_status = true; // SQL 写入记录 操作 OK //2018.5.19 无操作
            }*/
        }
        //
        /*//后台进程, 自动调试完成
        private void backgroundWorkerAutoSet_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //
        }*/
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // 保存测试记录到SQL
        private bool SaveRecordToSQL()
        {
            string strName = "";
            string errmsg = "";
            string conString;
            string fibertop_bn = TestResult.fibertop_bn;
            string fibertop_sn = TestResult.fibertop_sn;
            string fibertop_pn = TestResult.fibertop_pn;
            string tosa_sn = TestResult.tosa_sn;
            string rosa_sn = TestResult.rosa_sn;
            string sn = TestResult.sn;
            string pn = TestResult.pn;
            string vn = TestResult.vn;
            string date = TestResult.date;

            float temp = TestResult.tempDDM;
            float vcc = TestResult.vccDDM;

            float tx_bias = TestResult.txBiasDDM;
            float tx_pwr = TestResult.txPowerDDM;
            float[] tx_biasbuf = new float[4];//TestResult.txBiasDDMbuf;//
            float[] tx_pwrbuf = new float[4];//TestResult.txPowerDDMbuf;//

            float tx_pwr_real = TestResult.txPower;
            float tx_er = TestResult.txEr;
            float tx_esn = TestResult.txESN;
            float tx_crossing = TestResult.txCrossing;
            float tx_jitterRMS = TestResult.txJiterRMS;
            float tx_jitterPP  = TestResult.txJiterPP;

            float tx_pwrErr = TestResult.txPwrErr;

            float[] tx_pwr_realbuf = new float[4];//TestResult.txPower;
            float[] tx_erbuf = new float[4];// TestResult.txEr;
            float[] tx_esnbuf = new float[4];//TestResult.txESN;
            float[] tx_crossingbuf = new float[4];// TestResult.txCrossing;
            float[] tx_jitterRMSbuf = new float[4];//TestResult.txJiterRMS;
            float[] tx_jitterPPbuf = new float[4];//TestResult.txJiterPP;

            float[] tx_pwrErrbuf = new float[4];//TestResult.txPwrErr;

            float[] rx_PwrReal = new float[5];
            float[] rx_PwrDDM  = new float[5];
            float[] rx_pwrErr = new float[5];

            float[,] rx_PwrRealbuf = new float[4, 5];
            float[,] rx_PwrDDMbuf = new float[4, 5];
            float[,] rx_pwrErrbuf = new float[4, 5];

            float rx_sen  = TestResult.rxSen;
            float rx_DLos = TestResult.rxDLos;
            float rx_ALos = TestResult.rxALos;
            float rx_overload = TestResult.rxOverLoad;

            float [] rx_senbuf = new float[4];//TestResult.rxSenbuf;
            float[]  rx_DLosbuf = new float[4];//TestResult.rxDLosbuf;
            float [] rx_ALosbuf = new float[4];//TestResult.rxALosbuf;
            float [] rx_overloadbuf = new float[4];//TestResult.rxOverLoadbuf;

            string design_type = GlobalVarFun.moduleType;

            string tester_no = TestResult.tester_no;

            byte[] flash_data = new byte[TestResult.flash_data_len];

            byte[] byte_image = new byte[256];

            int i;

            for (i = 0; i < TestResult.flash_data_len; i++)
            {
                flash_data[i] = TestResult.flash_data[i];
            }
            
            //for (i = 0; i < TestResult.txEye_image.Length; i++)
            //{
            //   byte_image[i] = TestResult.txEye_image[i];
            //}
            for (int j = 0; j < 4; j++)
            {
                tx_biasbuf[j] = TestResult.txBiasDDMbuf[j];
                tx_pwrbuf[j] = TestResult.txPowerDDMbuf[j];

                tx_pwr_realbuf[j] = TestResult.txPowerbuf[j];
                tx_erbuf[j] = TestResult.txPowerbuf[j];
                tx_esnbuf[j] = TestResult.txPowerbuf[j];
                tx_crossingbuf[j] = TestResult.txPowerbuf[j];
                tx_jitterRMSbuf[j] = TestResult.txPowerbuf[j];
                tx_jitterPPbuf[j] = TestResult.txPowerbuf[j];
                tx_pwrErrbuf[j] = TestResult.txPwrErrbuf[j];

                rx_senbuf[j] = TestResult.rxSenbuf[j];
                rx_DLosbuf[j] = TestResult.rxDLosbuf[j];
                rx_ALosbuf[j] = TestResult.rxALosbuf[j];
                rx_overloadbuf[j] = TestResult.rxOverLoadbuf[j];

                for (i = 0; i < 5; i++)
                {                    
                   rx_PwrRealbuf[j,i] = TestResult.rxPwrRealbuf[j, i];
                   rx_PwrDDMbuf[j,i] = TestResult.rxPwrDDMbuf[j,i];
                   rx_pwrErrbuf[j,i] = TestResult.rxPwrErrbuf[j,i];                  
                }
            }

            // SQL 连接异常
            if (GlobalVarFun.sql_connect_status == false)
            {
                return false;
            }

            //
            i = 10;
            try
            {
                // 打开SQL数据连接
                if (sqlconnection.State == ConnectionState.Closed)
                {
                    sqlconnection.Open();
                }
                else if (sqlconnection.State == ConnectionState.Broken)
                {
                    sqlconnection.Close();
                    sqlconnection.Open();
                }
                else
                {
                    errmsg += "SQL数据库无法打开连接！"; // 异常情况
                }
                //
                //Thread.Sleep(300);
                //
                if (GlobalVarFun.testType == "firstTest")
                {
                    strName = "QSFP_FirstTest_Record_Table";
                }
                else
                {
                    strName = "QSFP_FinalTest_Record_Table";
                }
                //
                conString = @"insert into " + strName + @" ([FibertopBN],[FibertopSN],[FibertopPN],[TosaSN],[RosaSN],[SN],[PN],[VN],[Date],[Temp],[Vcc],[TxBias_ch0],[TxBias_ch1],[TxBias_ch2],[TxBias_ch3],[TxPower_ch0],[TxPower_ch1],[TxPower_ch2],[TxPower_ch3],
                            [TxPowerReal_ch0],[TxPowerReal_ch1],[TxPowerReal_ch2],[TxPowerReal_ch3],[TxPowerErr_ch0],[TxPowerErr_ch1],[TxPowerErr_ch2],[TxPowerErr_ch3],[TxER_ch0],[TxER_ch1],[TxER_ch2],[TxER_ch3],[TxESN_ch0],[TxESN_ch1],[TxESN_ch2],[TxESN_ch3],
                            [TxCrossing_ch0],[TxCrossing_ch1],[TxCrossing_ch2],[TxCrossing_ch3],[TxJitterRMS_ch0],[TxJitterRMS_ch1],[TxJitterRMS_ch2],[TxJitterRMS_ch3],[TxJitterPP_ch0],[TxJitterPP_ch1],[TxJitterPP_ch2],[TxJitterPP_ch3],
                            [TxEyePattern_ch0],[TxEyePattern_ch1],[TxEyePattern_ch2],[TxEyePattern_ch3],[TxEyeMargin_ch0],[TxEyeMargin_ch1],[TxEyeMargin_ch2],[TxEyeMargin_ch3],[TxEyeImage_ch0],[TxEyeImage_ch1],[TxEyeImage_ch2],[TxEyeImage_ch3],"
                          + @"[RxPwrReal_1_ch0],[RxPwrReal_2_ch0],[RxPwrReal_3_ch0],[RxPwrReal_4_ch0],[RxPwrReal_5_ch0],[RxPwrDDM_1_ch0],[RxPwrDDM_2_ch0],[RxPwrDDM_3_ch0],[RxPwrDDM_4_ch0],[RxPwrDDM_5_ch0],[RxPwrErr_1_ch0],[RxPwrErr_2_ch0],[RxPwrErr_3_ch0],[RxPwrErr_4_ch0],[RxPwrErr_5_ch0],"
                          + @"[RxPwrReal_1_ch1],[RxPwrReal_2_ch1],[RxPwrReal_3_ch1],[RxPwrReal_4_ch1],[RxPwrReal_5_ch1],[RxPwrDDM_1_ch1],[RxPwrDDM_2_ch1],[RxPwrDDM_3_ch1],[RxPwrDDM_4_ch1],[RxPwrDDM_5_ch1],[RxPwrErr_1_ch1],[RxPwrErr_2_ch1],[RxPwrErr_3_ch1],[RxPwrErr_4_ch1],[RxPwrErr_5_ch1],"
                          + @"[RxPwrReal_1_ch2],[RxPwrReal_2_ch2],[RxPwrReal_3_ch2],[RxPwrReal_4_ch2],[RxPwrReal_5_ch2],[RxPwrDDM_1_ch2],[RxPwrDDM_2_ch2],[RxPwrDDM_3_ch2],[RxPwrDDM_4_ch2],[RxPwrDDM_5_ch2],[RxPwrErr_1_ch2],[RxPwrErr_2_ch2],[RxPwrErr_3_ch2],[RxPwrErr_4_ch2],[RxPwrErr_5_ch2],"
                          + @"[RxPwrReal_1_ch3],[RxPwrReal_2_ch3],[RxPwrReal_3_ch3],[RxPwrReal_4_ch3],[RxPwrReal_5_ch3],[RxPwrDDM_1_ch3],[RxPwrDDM_2_ch3],[RxPwrDDM_3_ch3],[RxPwrDDM_4_ch3],[RxPwrDDM_5_ch3],[RxPwrErr_1_ch3],[RxPwrErr_2_ch3],[RxPwrErr_3_ch3],[RxPwrErr_4_ch3],[RxPwrErr_5_ch3],"
                          + @"[Sensitivity_ch0],[RxALos_ch0],[RxDLos_ch0],[RxOverLoad_ch0],[Sensitivity_ch1],[RxALos_ch1],[RxDLos_ch1],[RxOverLoad_ch1],[Sensitivity_ch2],[RxALos_ch2],[RxDLos_ch2],[RxOverLoad_ch2],[Sensitivity_ch3],[RxALos_ch3],[RxDLos_ch3],[RxOverLoad_ch3],[FlashData],[DesignType],[TestDate],[TesterNO])"
                          + @" VALUES (@FibertopBN,@FibertopSN,@FibertopPN,@TosaSN,@RosaSN,@SN,@PN,@VN,@Date,@Temp,@Vcc,@TxBias_ch0,@TxBias_ch1,@TxBias_ch2,@TxBias_ch3,@TxPower_ch0,@TxPower_ch1,@TxPower_ch2,@TxPower_ch3,@TxPowerReal_ch0,@TxPowerReal_ch1,@TxPowerReal_ch2,@TxPowerReal_ch3,"
                          + @"@TxPowerErr_ch0,@TxPowerErr_ch1,@TxPowerErr_ch2,@TxPowerErr_ch3,@TxER_ch0,@TxER_ch1,@TxER_ch2,@TxER_ch3,@TxESN_ch0,@TxESN_ch1,@TxESN_ch2,@TxESN_ch3,@TxCrossing_ch0,@TxCrossing_ch1,@TxCrossing_ch2,@TxCrossing_ch3,@TxJitterRMS_ch0,@TxJitterRMS_ch1,@TxJitterRMS_ch2,@TxJitterRMS_ch3,"
                          + @"@TxJitterPP_ch0,@TxJitterPP_ch1,@TxJitterPP_ch2,@TxJitterPP_ch3,@TxEyePattern_ch0,@TxEyePattern_ch1,@TxEyePattern_ch2,@TxEyePattern_ch3,@TxEyeMargin_ch0,@TxEyeMargin_ch1,@TxEyeMargin_ch2,@TxEyeMargin_ch3,@TxEyeImage_ch0,@TxEyeImage_ch1,@TxEyeImage_ch2,@TxEyeImage_ch3,"
                          + @"@RxPwrReal_1_ch0,@RxPwrReal_2_ch0,@RxPwrReal_3_ch0,@RxPwrReal_4_ch0,@RxPwrReal_5_ch0,@RxPwrDDM_1_ch0,@RxPwrDDM_2_ch0,@RxPwrDDM_3_ch0,@RxPwrDDM_4_ch0,@RxPwrDDM_5_ch0,@RxPwrErr_1_ch0,@RxPwrErr_2_ch0,@RxPwrErr_3_ch0,@RxPwrErr_4_ch0,@RxPwrErr_5_ch0,"
                          + @"@RxPwrReal_1_ch1,@RxPwrReal_2_ch1,@RxPwrReal_3_ch1,@RxPwrReal_4_ch1,@RxPwrReal_5_ch1,@RxPwrDDM_1_ch1,@RxPwrDDM_2_ch1,@RxPwrDDM_3_ch1,@RxPwrDDM_4_ch1,@RxPwrDDM_5_ch1,@RxPwrErr_1_ch1,@RxPwrErr_2_ch1,@RxPwrErr_3_ch1,@RxPwrErr_4_ch1,@RxPwrErr_5_ch1,"
                          + @"@RxPwrReal_1_ch2,@RxPwrReal_2_ch2,@RxPwrReal_3_ch2,@RxPwrReal_4_ch2,@RxPwrReal_5_ch2,@RxPwrDDM_1_ch2,@RxPwrDDM_2_ch2,@RxPwrDDM_3_ch2,@RxPwrDDM_4_ch2,@RxPwrDDM_5_ch2,@RxPwrErr_1_ch2,@RxPwrErr_2_ch2,@RxPwrErr_3_ch2,@RxPwrErr_4_ch2,@RxPwrErr_5_ch2,"
                          + @"@RxPwrReal_1_ch3,@RxPwrReal_2_ch3,@RxPwrReal_3_ch3,@RxPwrReal_4_ch3,@RxPwrReal_5_ch3,@RxPwrDDM_1_ch3,@RxPwrDDM_2_ch3,@RxPwrDDM_3_ch3,@RxPwrDDM_4_ch3,@RxPwrDDM_5_ch3,@RxPwrErr_1_ch3,@RxPwrErr_2_ch3,@RxPwrErr_3_ch3,@RxPwrErr_4_ch3,@RxPwrErr_5_ch3,"
                          + @"@Sensitivity_ch0,@RxALos_ch0,@RxDLos_ch0,@RxOverLoad_ch0,@Sensitivity_ch1,@RxALos_ch1,@RxDLos_ch1,@RxOverLoad_ch1,@Sensitivity_ch2,@RxALos_ch2,@RxDLos_ch2,@RxOverLoad_ch2,@Sensitivity_ch3,@RxALos_ch3,@RxDLos_ch3,@RxOverLoad_ch3,@FlashData,@DesignType,@TestDate,@TesterNO)";
                //
                using (SqlCommand myCommand = new SqlCommand(conString, sqlconnection))
                {
                    myCommand.CommandTimeout = 16; // 16s 命令执行超时设置
                    myCommand.CommandType = CommandType.Text;
                    //myCommand.CommandType = CommandType.StoredProcedure; // 执行存储过程
                    //
                    myCommand.Parameters.Add("@FibertopBN", SqlDbType.NChar).Value = fibertop_bn;
                    myCommand.Parameters.Add("@FibertopSN", SqlDbType.NChar).Value = fibertop_sn;
                    myCommand.Parameters.Add("@FibertopPN", SqlDbType.NChar).Value = fibertop_pn;
                    myCommand.Parameters.Add("@TosaSN", SqlDbType.NChar).Value = tosa_sn;
                    myCommand.Parameters.Add("@RosaSN", SqlDbType.NChar).Value = rosa_sn;
                    myCommand.Parameters.Add("@SN", SqlDbType.NChar).Value = sn;
                    myCommand.Parameters.Add("@PN", SqlDbType.NChar).Value = pn;
                    myCommand.Parameters.Add("@VN", SqlDbType.NChar).Value = vn;
                    myCommand.Parameters.Add("@Date", SqlDbType.NChar).Value = date;
                    myCommand.Parameters.Add("@Temp", SqlDbType.Float).Value = temp;
                    myCommand.Parameters.Add("@Vcc", SqlDbType.Float).Value = vcc;

                    myCommand.Parameters.Add("@TxBias_ch0", SqlDbType.Float).Value = tx_biasbuf[0];
                    myCommand.Parameters.Add("@TxBias_ch1", SqlDbType.Float).Value = tx_biasbuf[1];
                    myCommand.Parameters.Add("@TxBias_ch2", SqlDbType.Float).Value = tx_biasbuf[2];
                    myCommand.Parameters.Add("@TxBias_ch3", SqlDbType.Float).Value = tx_biasbuf[3];

                    myCommand.Parameters.Add("@TxPower_ch0", SqlDbType.Float).Value = tx_pwrbuf[0];
                    myCommand.Parameters.Add("@TxPower_ch1", SqlDbType.Float).Value = tx_pwrbuf[1];
                    myCommand.Parameters.Add("@TxPower_ch2", SqlDbType.Float).Value = tx_pwrbuf[2];
                    myCommand.Parameters.Add("@TxPower_ch3", SqlDbType.Float).Value = tx_pwrbuf[3];

                    myCommand.Parameters.Add("@TxPowerReal_ch0", SqlDbType.Float).Value = tx_pwr_realbuf[0];
                    myCommand.Parameters.Add("@TxPowerReal_ch1", SqlDbType.Float).Value = tx_pwr_realbuf[1];
                    myCommand.Parameters.Add("@TxPowerReal_ch2", SqlDbType.Float).Value = tx_pwr_realbuf[2];
                    myCommand.Parameters.Add("@TxPowerReal_ch3", SqlDbType.Float).Value = tx_pwr_realbuf[3];

                    myCommand.Parameters.Add("@TxPowerErr_ch0", SqlDbType.Float).Value = tx_pwrErrbuf[0];
                    myCommand.Parameters.Add("@TxPowerErr_ch1", SqlDbType.Float).Value = tx_pwrErrbuf[1];
                    myCommand.Parameters.Add("@TxPowerErr_ch2", SqlDbType.Float).Value = tx_pwrErrbuf[2];
                    myCommand.Parameters.Add("@TxPowerErr_ch3", SqlDbType.Float).Value = tx_pwrErrbuf[3];

                    myCommand.Parameters.Add("@TxER_ch0", SqlDbType.Float).Value = tx_erbuf[0];
                    myCommand.Parameters.Add("@TxER_ch1", SqlDbType.Float).Value = tx_erbuf[1];
                    myCommand.Parameters.Add("@TxER_ch2", SqlDbType.Float).Value = tx_erbuf[2];
                    myCommand.Parameters.Add("@TxER_ch3", SqlDbType.Float).Value = tx_erbuf[3];

                    myCommand.Parameters.Add("@TxESN_ch0", SqlDbType.Float).Value = tx_esnbuf[0];
                    myCommand.Parameters.Add("@TxESN_ch1", SqlDbType.Float).Value = tx_esnbuf[1];
                    myCommand.Parameters.Add("@TxESN_ch2", SqlDbType.Float).Value = tx_esnbuf[2];
                    myCommand.Parameters.Add("@TxESN_ch3", SqlDbType.Float).Value = tx_esnbuf[3];

                    myCommand.Parameters.Add("@TxCrossing_ch0", SqlDbType.Float).Value = tx_crossingbuf[0];
                    myCommand.Parameters.Add("@TxCrossing_ch1", SqlDbType.Float).Value = tx_crossingbuf[1];
                    myCommand.Parameters.Add("@TxCrossing_ch2", SqlDbType.Float).Value = tx_crossingbuf[2];
                    myCommand.Parameters.Add("@TxCrossing_ch3", SqlDbType.Float).Value = tx_crossingbuf[3];

                    myCommand.Parameters.Add("@TxJitterRMS_ch0", SqlDbType.Float).Value = tx_jitterRMSbuf[0];
                    myCommand.Parameters.Add("@TxJitterRMS_ch1", SqlDbType.Float).Value = tx_jitterRMSbuf[1];
                    myCommand.Parameters.Add("@TxJitterRMS_ch2", SqlDbType.Float).Value = tx_jitterRMSbuf[2];
                    myCommand.Parameters.Add("@TxJitterRMS_ch3", SqlDbType.Float).Value = tx_jitterRMSbuf[3];

                    myCommand.Parameters.Add("@TxJitterPP_ch0", SqlDbType.Float).Value = tx_jitterPPbuf[0];
                    myCommand.Parameters.Add("@TxJitterPP_ch1", SqlDbType.Float).Value = tx_jitterPPbuf[1];
                    myCommand.Parameters.Add("@TxJitterPP_ch2", SqlDbType.Float).Value = tx_jitterPPbuf[2];
                    myCommand.Parameters.Add("@TxJitterPP_ch3", SqlDbType.Float).Value = tx_jitterPPbuf[3];

                    myCommand.Parameters.Add("@TxEyePattern_ch0", SqlDbType.NChar).Value = "";
                    myCommand.Parameters.Add("@TxEyePattern_ch1", SqlDbType.NChar).Value = "";
                    myCommand.Parameters.Add("@TxEyePattern_ch2", SqlDbType.NChar).Value = "";
                    myCommand.Parameters.Add("@TxEyePattern_ch3", SqlDbType.NChar).Value = ""; 

                    myCommand.Parameters.Add("@TxEyeMargin_ch0", SqlDbType.Float).Value = TestResult.mask_margin;
                    myCommand.Parameters.Add("@TxEyeMargin_ch1", SqlDbType.Float).Value = TestResult.mask_margin;
                    myCommand.Parameters.Add("@TxEyeMargin_ch2", SqlDbType.Float).Value = TestResult.mask_margin;
                    myCommand.Parameters.Add("@TxEyeMargin_ch3", SqlDbType.Float).Value = TestResult.mask_margin;
                    //眼图数据
                    if ((TestResult.bimage_len == 0) || (GlobalVarFun.tx_eye_save_test == false))
                    {
                        myCommand.Parameters.Add("@TxEyeImage_ch0", SqlDbType.Image).Value = DBNull.Value; //null
                        myCommand.Parameters.Add("@TxEyeImage_ch1", SqlDbType.Image).Value = DBNull.Value;
                        myCommand.Parameters.Add("@TxEyeImage_ch2", SqlDbType.Image).Value = DBNull.Value;
                        myCommand.Parameters.Add("@TxEyeImage_ch3", SqlDbType.Image).Value = DBNull.Value; 
                    }
                    else
                    {
                        myCommand.Parameters.Add("@TxEyeImage_ch0", SqlDbType.Image).Value = TestResult.txEye_image_ch0; //GIF image//
                        myCommand.Parameters.Add("@TxEyeImage_ch1", SqlDbType.Image).Value = TestResult.txEye_image_ch1; //GIF image//
                        myCommand.Parameters.Add("@TxEyeImage_ch2", SqlDbType.Image).Value = TestResult.txEye_image_ch2; //GIF image//
                        myCommand.Parameters.Add("@TxEyeImage_ch3", SqlDbType.Image).Value = TestResult.txEye_image_ch3; //GIF image//
                    }
                    //ch0
                    myCommand.Parameters.Add("@RxPwrReal_1_ch0", SqlDbType.Float).Value = rx_PwrRealbuf[0,0];
                    myCommand.Parameters.Add("@RxPwrReal_2_ch0", SqlDbType.Float).Value = rx_PwrRealbuf[0,1];
                    myCommand.Parameters.Add("@RxPwrReal_3_ch0", SqlDbType.Float).Value = rx_PwrRealbuf[0,2];
                    myCommand.Parameters.Add("@RxPwrReal_4_ch0", SqlDbType.Float).Value = rx_PwrRealbuf[0,3];
                    myCommand.Parameters.Add("@RxPwrReal_5_ch0", SqlDbType.Float).Value = rx_PwrRealbuf[0,4];

                    myCommand.Parameters.Add("@RxPwrDDM_1_ch0", SqlDbType.Float).Value = rx_PwrDDMbuf[0,0];
                    myCommand.Parameters.Add("@RxPwrDDM_2_ch0", SqlDbType.Float).Value = rx_PwrDDMbuf[0,1];
                    myCommand.Parameters.Add("@RxPwrDDM_3_ch0", SqlDbType.Float).Value = rx_PwrDDMbuf[0,2];
                    myCommand.Parameters.Add("@RxPwrDDM_4_ch0", SqlDbType.Float).Value = rx_PwrDDMbuf[0,3];
                    myCommand.Parameters.Add("@RxPwrDDM_5_ch0", SqlDbType.Float).Value = rx_PwrDDMbuf[0,4];

                    myCommand.Parameters.Add("@RxPwrErr_1_ch0", SqlDbType.Float).Value = rx_pwrErrbuf[0,0];
                    myCommand.Parameters.Add("@RxPwrErr_2_ch0", SqlDbType.Float).Value = rx_pwrErrbuf[0,1];
                    myCommand.Parameters.Add("@RxPwrErr_3_ch0", SqlDbType.Float).Value = rx_pwrErrbuf[0,2];
                    myCommand.Parameters.Add("@RxPwrErr_4_ch0", SqlDbType.Float).Value = rx_pwrErrbuf[0,3];
                    myCommand.Parameters.Add("@RxPwrErr_5_ch0", SqlDbType.Float).Value = rx_pwrErrbuf[0,4];
                    //ch1
                    myCommand.Parameters.Add("@RxPwrReal_1_ch1", SqlDbType.Float).Value = rx_PwrRealbuf[1, 0];
                    myCommand.Parameters.Add("@RxPwrReal_2_ch1", SqlDbType.Float).Value = rx_PwrRealbuf[1, 1];
                    myCommand.Parameters.Add("@RxPwrReal_3_ch1", SqlDbType.Float).Value = rx_PwrRealbuf[1, 2];
                    myCommand.Parameters.Add("@RxPwrReal_4_ch1", SqlDbType.Float).Value = rx_PwrRealbuf[1, 3];
                    myCommand.Parameters.Add("@RxPwrReal_5_ch1", SqlDbType.Float).Value = rx_PwrRealbuf[1, 4];

                    myCommand.Parameters.Add("@RxPwrDDM_1_ch1", SqlDbType.Float).Value = rx_PwrDDMbuf[1, 0];
                    myCommand.Parameters.Add("@RxPwrDDM_2_ch1", SqlDbType.Float).Value = rx_PwrDDMbuf[1, 1];
                    myCommand.Parameters.Add("@RxPwrDDM_3_ch1", SqlDbType.Float).Value = rx_PwrDDMbuf[1, 2];
                    myCommand.Parameters.Add("@RxPwrDDM_4_ch1", SqlDbType.Float).Value = rx_PwrDDMbuf[1, 3];
                    myCommand.Parameters.Add("@RxPwrDDM_5_ch1", SqlDbType.Float).Value = rx_PwrDDMbuf[1, 4];

                    myCommand.Parameters.Add("@RxPwrErr_1_ch1", SqlDbType.Float).Value = rx_pwrErrbuf[1, 0];
                    myCommand.Parameters.Add("@RxPwrErr_2_ch1", SqlDbType.Float).Value = rx_pwrErrbuf[1, 1];
                    myCommand.Parameters.Add("@RxPwrErr_3_ch1", SqlDbType.Float).Value = rx_pwrErrbuf[1, 2];
                    myCommand.Parameters.Add("@RxPwrErr_4_ch1", SqlDbType.Float).Value = rx_pwrErrbuf[1, 3];
                    myCommand.Parameters.Add("@RxPwrErr_5_ch1", SqlDbType.Float).Value = rx_pwrErrbuf[1, 4];
                    //ch2
                    myCommand.Parameters.Add("@RxPwrReal_1_ch2", SqlDbType.Float).Value = rx_PwrRealbuf[2, 0];
                    myCommand.Parameters.Add("@RxPwrReal_2_ch2", SqlDbType.Float).Value = rx_PwrRealbuf[2, 1];
                    myCommand.Parameters.Add("@RxPwrReal_3_ch2", SqlDbType.Float).Value = rx_PwrRealbuf[2, 2];
                    myCommand.Parameters.Add("@RxPwrReal_4_ch2", SqlDbType.Float).Value = rx_PwrRealbuf[2, 3];
                    myCommand.Parameters.Add("@RxPwrReal_5_ch2", SqlDbType.Float).Value = rx_PwrRealbuf[2, 4];

                    myCommand.Parameters.Add("@RxPwrDDM_1_ch2", SqlDbType.Float).Value = rx_PwrDDMbuf[2, 0];
                    myCommand.Parameters.Add("@RxPwrDDM_2_ch2", SqlDbType.Float).Value = rx_PwrDDMbuf[2, 1];
                    myCommand.Parameters.Add("@RxPwrDDM_3_ch2", SqlDbType.Float).Value = rx_PwrDDMbuf[2, 2];
                    myCommand.Parameters.Add("@RxPwrDDM_4_ch2", SqlDbType.Float).Value = rx_PwrDDMbuf[2, 3];
                    myCommand.Parameters.Add("@RxPwrDDM_5_ch2", SqlDbType.Float).Value = rx_PwrDDMbuf[2, 4];

                    myCommand.Parameters.Add("@RxPwrErr_1_ch2", SqlDbType.Float).Value = rx_pwrErrbuf[2, 0];
                    myCommand.Parameters.Add("@RxPwrErr_2_ch2", SqlDbType.Float).Value = rx_pwrErrbuf[2, 1];
                    myCommand.Parameters.Add("@RxPwrErr_3_ch2", SqlDbType.Float).Value = rx_pwrErrbuf[2, 2];
                    myCommand.Parameters.Add("@RxPwrErr_4_ch2", SqlDbType.Float).Value = rx_pwrErrbuf[2, 3];
                    myCommand.Parameters.Add("@RxPwrErr_5_ch2", SqlDbType.Float).Value = rx_pwrErrbuf[2, 4];
                    //ch3
                    myCommand.Parameters.Add("@RxPwrReal_1_ch3", SqlDbType.Float).Value = rx_PwrRealbuf[3, 0];
                    myCommand.Parameters.Add("@RxPwrReal_2_ch3", SqlDbType.Float).Value = rx_PwrRealbuf[3, 1];
                    myCommand.Parameters.Add("@RxPwrReal_3_ch3", SqlDbType.Float).Value = rx_PwrRealbuf[3, 2];
                    myCommand.Parameters.Add("@RxPwrReal_4_ch3", SqlDbType.Float).Value = rx_PwrRealbuf[3, 3];
                    myCommand.Parameters.Add("@RxPwrReal_5_ch3", SqlDbType.Float).Value = rx_PwrRealbuf[3, 4];

                    myCommand.Parameters.Add("@RxPwrDDM_1_ch3", SqlDbType.Float).Value = rx_PwrDDMbuf[3, 0];
                    myCommand.Parameters.Add("@RxPwrDDM_2_ch3", SqlDbType.Float).Value = rx_PwrDDMbuf[3, 1];
                    myCommand.Parameters.Add("@RxPwrDDM_3_ch3", SqlDbType.Float).Value = rx_PwrDDMbuf[3, 2];
                    myCommand.Parameters.Add("@RxPwrDDM_4_ch3", SqlDbType.Float).Value = rx_PwrDDMbuf[3, 3];
                    myCommand.Parameters.Add("@RxPwrDDM_5_ch3", SqlDbType.Float).Value = rx_PwrDDMbuf[3, 4];

                    myCommand.Parameters.Add("@RxPwrErr_1_ch3", SqlDbType.Float).Value = rx_pwrErrbuf[3, 0];
                    myCommand.Parameters.Add("@RxPwrErr_2_ch3", SqlDbType.Float).Value = rx_pwrErrbuf[3, 1];
                    myCommand.Parameters.Add("@RxPwrErr_3_ch3", SqlDbType.Float).Value = rx_pwrErrbuf[3, 2];
                    myCommand.Parameters.Add("@RxPwrErr_4_ch3", SqlDbType.Float).Value = rx_pwrErrbuf[3, 3];
                    myCommand.Parameters.Add("@RxPwrErr_5_ch3", SqlDbType.Float).Value = rx_pwrErrbuf[3, 4];
                    //ch0
                    myCommand.Parameters.Add("@Sensitivity_ch0", SqlDbType.Float).Value = rx_senbuf[0];
                    myCommand.Parameters.Add("@RxALos_ch0", SqlDbType.Float).Value = rx_ALosbuf[0];
                    myCommand.Parameters.Add("@RxDLos_ch0", SqlDbType.Float).Value = rx_DLosbuf[0];
                    myCommand.Parameters.Add("@RxOverLoad_ch0", SqlDbType.Float).Value = rx_overloadbuf[0];
                    //ch1
                    myCommand.Parameters.Add("@Sensitivity_ch1", SqlDbType.Float).Value = rx_senbuf[1];
                    myCommand.Parameters.Add("@RxALos_ch1", SqlDbType.Float).Value = rx_ALosbuf[1];
                    myCommand.Parameters.Add("@RxDLos_ch1", SqlDbType.Float).Value = rx_DLosbuf[1];
                    myCommand.Parameters.Add("@RxOverLoad_ch1", SqlDbType.Float).Value = rx_overloadbuf[1];
                    //ch2
                    myCommand.Parameters.Add("@Sensitivity_ch2", SqlDbType.Float).Value = rx_senbuf[2];
                    myCommand.Parameters.Add("@RxALos_ch2", SqlDbType.Float).Value = rx_ALosbuf[2];
                    myCommand.Parameters.Add("@RxDLos_ch2", SqlDbType.Float).Value = rx_DLosbuf[2];
                    myCommand.Parameters.Add("@RxOverLoad_ch2", SqlDbType.Float).Value = rx_overloadbuf[2];
                    //ch3
                    myCommand.Parameters.Add("@Sensitivity_ch3", SqlDbType.Float).Value = rx_senbuf[3];
                    myCommand.Parameters.Add("@RxALos_ch3", SqlDbType.Float).Value = rx_ALosbuf[3];
                    myCommand.Parameters.Add("@RxDLos_ch3", SqlDbType.Float).Value = rx_DLosbuf[3];
                    myCommand.Parameters.Add("@RxOverLoad_ch3", SqlDbType.Float).Value = rx_overloadbuf[3];
                    //
                    myCommand.Parameters.Add("@FlashData", SqlDbType.Binary).Value = flash_data;

                    myCommand.Parameters.Add("@DesignType", SqlDbType.NChar).Value = design_type;

                    myCommand.Parameters.Add("@TestDate", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //2017.12.4
                    myCommand.Parameters.Add("@TesterNO", SqlDbType.NChar).Value = tester_no;
                    //
                    i = myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                sqlconnection.Close();
                errmsg += "测试数据保存到SQL数据库失败！";
                throw new Exception("SQL执行异常", ex);
            }
            finally
            {
                sqlconnection.Close();
            }
            //
            if (i <= 0)
            {
                errmsg += "insert记录到SQL数据表返回异常！";
            }
            //
            errorMessage = errmsg;
            AddTestLog(errorMessage); // test
            //
            if (errmsg != "")
            {
                return false;
            }
            //
            return true;
        }

        // 自动批量测试启动
        private void start_button_Click(object sender, EventArgs e)
        {
            //TestSet.txapc_Min = Convert.ToUInt16(GlobalVarFun.apc_min.ToString().Trim());
            //TestSet.txapc_Max = Convert.ToUInt16(GlobalVarFun.apc_max.ToString().Trim());
            //TestSet.txmod_Min = Convert.ToUInt16(GlobalVarFun.mod_min.ToString().Trim());
            //TestSet.txmod_Max = Convert.ToUInt16(GlobalVarFun.mod_max.ToString().Trim());

            txpwr_min_textBox.Text = TestSet.txPwr_Min.ToString("F1");
            txpwr_max_textBox.Text = TestSet.txPwr_Max.ToString("F1");
            bias_min_textBox.Text = TestSet.bias_Min.ToString("F1");
            bias_max_textBox.Text = TestSet.bias_Max.ToString("F1");
            er_min_textBox.Text = TestSet.txEr_Min.ToString("F1");
            er_max_textBox.Text = TestSet.txEr_Max.ToString("F1");

            //TestSet.rxlos_Min = Convert.ToUInt16(GlobalVarFun.los_min.ToString().Trim());
            //TestSet.rxlos_Max = Convert.ToUInt16(GlobalVarFun.los_max.ToString().Trim());

            rxPwrMaxErr = (float)(GlobalVarFun.rx_cal_num);
            txPwrMaxErr = (float)(GlobalVarFun.tx_cal_num);
            erValMaxErr = (float)(GlobalVarFun.ER_cal_num);

            TestResult.txpeVal = (Byte)(GlobalVarFun.tx_pe); //2017.8.21

            TestResult.waveforms_count = Convert.ToInt32(GlobalVarFun.waveforms_num);

            if (GlobalVarFun.moduleType == "QSFP")
            {
                GlobalVarFun.txpwr_debug_method = 0x00;
                if (GlobalVarFun.cob_ld)
                {
                    // 0x00:线性计算法 apc-->uw & bias   0x11: 普通二分法 apc-->dBm   22: 定值法 COB-LD
                    GlobalVarFun.txpwr_debug_method = 0x11;

                    // 0x00:普通二分法   0x11: 逐步逼近法 for COB-LD
                    GlobalVarFun.txer_debug_method = 0x00;
                }
            }

            // 终测模式  判断眼图模板累计点测试配置是否正确
            if (GlobalVarFun.testType == "finalTest")
            {
                if ((TestResult.waveforms_count >= 100) && (eyeMaskIsOpened == false))
                {
                    MessageBox.Show("眼图模板未打开，不能进行眼图累计点测试，请重新连接86100！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // 判断调试参数范围设置是否正确
            if ((TestSet.txapc_Max < TestSet.txapc_Min) || (TestSet.txmod_Max < TestSet.txmod_Min) || (TestSet.rxlos_Max < TestSet.rxlos_Min))
            {
                MessageBox.Show("APC/MOD/LOS调试范围设置错误(max<min)！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 判断 Access 数据库 是否连接OK
            if (GlobalVarFun.access_connect_status == false)
            {
                MessageBox.Show("Access数据库连接失败！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //

            if (autoTestCtrl == false)
            {
                if (GlobalVarFun.rx_ddm_test)
                {
                    if ((GlobalVarFun.optoAtt_connected == false) || (GlobalVarFun.optoMeter_connected == false)) // 光衰减器 和 光功率计
                    {
                        MessageBox.Show("请先连接光衰减器和光功率计！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //TestDataCheck_button_Click(sender, e);
                    if (GlobalVarFun.testDataIsOK == false)
                    {
                        MessageBox.Show("测试参数设置异常，无法启动批量测试！ 请先进行 参数设置校验 ！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //2024.05.29
                    if (GlobalVarFun.sen_test)
                    {
                        if (GlobalVarFun.pssbert_connected == false)
                        {
                            MessageBox.Show("使用灵敏度测试，请先连接误码仪！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }
                //
                if (GlobalVarFun.tx_test)
                {
                    if (GlobalVarFun.instrument_connected == false) // 连接眼图仪判断
                    {
                        //2023.3.1修改
                        if (GlobalVarFun.testType == "firstTest")
                        {
                            MessageBox.Show("请先连接眼图仪！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else
                        {
                            MessageBox.Show("终测：发射只测试发光功率，不进行发射眼图参数测试！！！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                           // connection_button.BackColor = System.Drawing.Color.Red;
                        }
                    }
                    if (GlobalVarFun.optoMeter_connected == false && GlobalVarFun.power_use_DAC == false) // 不使用眼图仪测试发光功率时，连接光功率计判断
                    {
                        MessageBox.Show("请先连接光功率计！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //2023.3.1修改
                    if (GlobalVarFun.power_use_DAC == true && GlobalVarFun.instrument_connected == false)
                    {
                        MessageBox.Show("使用眼图仪测试发光功率，请先连接眼图仪！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }            
                }
                //
                //
                GlobalVarFun.sql_record_status = GlobalVarFun.sql_connect_status;//更新记录状态和SQL连接状态一致 2018.5.19
                autoTestCtrl = true;
                btnSetup.Enabled = false;
                start_button.BackColor = Color.GreenYellow;
                start_button.Text = "停止批量调试";
                //
                Startautoset_button.BackColor = Color.Orange;
                Startautoset_button.Text = "自动批量测试启动，请插入模块......";
                //
                //SetDebugParaCtrlStatus(false);
                txMustDebug_checkBox.Enabled = false;
            }
            else
            {
                autoTestCtrl = false;
                btnSetup.Enabled = true;
                start_button.BackColor = Color.Gray;
                start_button.Text = "开始批量调试";
                //
                Startautoset_button.BackColor = Color.OrangeRed;
                Startautoset_button.Text = "自动批量测试已停止......";
                //
                //SetDebugParaCtrlStatus(true);
                txMustDebug_checkBox.Enabled = true;
            }
        }

        //===========================================================================================================================//
        // 定时器自动判断模块并进行自动调试
        private void timer1_Tick(object sender, EventArgs e)
        {
            string str;
            // 实时更新数据库连接状态
            SetLED(sqlrecord_pictureBox, !GlobalVarFun.sql_record_status);
            SetLED(sqlconnt_pictureBox, !GlobalVarFun.sql_connect_status);
            SetLED(accessconnt_pictureBox, !GlobalVarFun.access_connect_status);
            SetLED(accessupdated_pictureBox, !GlobalVarFun.access_updated_status);
            //
            ShowModuleDdmInfo();
            
            //更新眼图测试最大累计点
            TestResult.waveforms_count = Convert.ToInt32(GlobalVarFun.waveforms_num);

            pnshow_textBox.Text = TestResult.fibertop_pn;//moduletype_comboBox.Text;

            TestResult.tester_no = textBoxTester.Text;
            bn_textBox.Text = TestResult.fibertop_bn;
            tosaSn_textBox.Text = TestResult.tosa_sn;
            rosaSn_textBox.Text = TestResult.rosa_sn;
            Refresh();
            //
  
            if (GlobalVarFun.i2c_can_use == false)
            {       
                return;
            }
            if (GlobalVarFun.usb_i2c_open == true)
            {
                if (GlobalVarFun.usb_can_use == false)
                {
                    SetLED(usbok_pictureBox, true);
                    //Startautoset_button.BackColor = Color.Red;
                    //Startautoset_button.Text = "USB连接状态异常, 无法测试 ......";
                    //return;
                }
                else
                {
                    SetLED(usbok_pictureBox, false);
                }
            }
            //
            if (test.GetVCC() < 2.0) //电源电压小于2.00V 异常模块
            {
                moduleOnline = false;
                SetLED(i2cok_pictureBox1, true);
                SetLED(i2cok_pictureBox2, true);
                SetLED(usbok_pictureBox, true);
                toolStripStatusLabel1.Text = ".......................";
                Startautoset_button.BackColor = Color.Orange;
                return;
            }
            //
            SetLED(i2cok_pictureBox1, false);
            SetLED(i2cok_pictureBox2, false);
            //
            //2021.5.29 增加模块方案检查选择
            if (GlobalVarFun.distype_check)
            {
                TestResult.chipIsOK = true;
                typeok_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                sr850_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                chipok_pictureBox1.Image = imageList1.Images["LedNone.ico"];
            }
            else
            {
                if (test.CheckTestTypeInfo() == false) // 模块方案类型信息判断
                {
                    SetLED(typeok_pictureBox1, true);
                    sr850_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                    chipok_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                    return;
                }
                SetLED(typeok_pictureBox1, false);
            }
            //       
            ShowModuleDdmInfo();// 显示DDM信息
            //
            if (ShowCheckModuleStatus() == false) //显示并判断模块方案/速率/版本/工作状态等信息
            {
                Startautoset_button.BackColor = Color.Red;
                Startautoset_button.Text = "模块芯片工作状态异常Error, 无法测试 ......";
                return;
            }           
            //
            // 判断自动批量调试是否启动
            if (autoTestCtrl == false)
            {
                //AddTestLog(errorMessage); //
                return;
            }
            //
            //2018.5.19  SQL数据连接并且测试记录保存出错进入异常处理
            if ((GlobalVarFun.sql_connect_status == true) && (GlobalVarFun.sql_record_status == false))
            {
                //Startautoset_button.BackColor = Color.OrangeRed;
                //Startautoset_button.Text = "已测试模块参数记录保存SQL数据库异常, 请停止自动测试并检查连接 ......";
                //return;
            }
            //
            if ((moduleOnline == false) && (test.CheckDebugPWD() == 0x02)) // 检测到 新测试模块插入测试板
            {
                timer.Reset();
                timer.Start();
                //
                errorMessage = "";
                ClearTestLog();
                ClearTextVal();
                //
                progressBar1.Value = 0;
                Startautoset_button.BackColor = Color.Honeydew;
                Startautoset_button.Text = "已检测到模块插入, 请不要插拔模块......";
                moduleOnline = true;
                Refresh();
            }
            else
            {
                return;
            }         
            //
            Read_moduleInfo();
            //
            // 模块DDM温度判断 0~40
            //if ((GlobalVarFun.testType != "firstTest") || ((GlobalVarFun.moduleType != "SFPP-GN1196") && (GlobalVarFun.moduleType != "SFP-GN25L95") && (GlobalVarFun.moduleType != "SFP-GN25L96") && (GlobalVarFun.moduleType != "SFP-UX3320C") && (GlobalVarFun.moduleType != "SFP-UX3320T")))
            //{
            //    if ((TestResult.tempDDM > 40) || (TestResult.tempDDM < 0))
            //    {
            //        Startautoset_button.BackColor = Color.OrangeRed;
            //        Startautoset_button.Text = "待测模块检测DDM温度: 大于40度或者小于0度, 温度异常, 无法测试 ......";
            //        goto RTN_POS;  //return;
            //    }
            //}
            // 模块DDM电压判断 3.15~3.45V  //终测
            if (GlobalVarFun.testType != "firstTest")
            {
                if ((TestResult.vccDDM > 3.45) || (TestResult.vccDDM < 3.15))
                {
                    Startautoset_button.BackColor = Color.OrangeRed;
                    Startautoset_button.Text = "待测模块检测DDM电压: 大于3.45V 或者 小于3.15V, 电压异常, 无法测试 ......";
                    goto RTN_POS;  //return;
                }
            }
            //
            // 模块型号比较
            if (cpn_checkBox.Checked) // 客户定制型号
            {
                if ((pn_textBox.Text).Trim() != (cpn_textBox.Text).Trim())
                {
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "待测模块型号 与 客户定制型号不一致, 无法测试 ......";
                    goto RTN_POS; //return;
                }
                Refresh();
            }
            else // 飞思卓型号
            {
                if ((pn_textBox.Text).Trim() != (GlobalVarFun.moduleType).Trim())
                {
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "待测模块型号 与 选择型号不一致, 无法测试 ......";
                    goto RTN_POS;  //return;
                }
            }
            //
            GlobalVarFun.record_need_save = false;
            //
            ddm_rxpower1_textbox.Text = "";
            ddm_rxpower2_textbox.Text = "";
            ddm_rxpower3_textbox.Text = "";
            ddm_rxpower4_textbox.Text = "";
            ddm_rxpower5_textbox.Text = "";
            //label_apcval.Text = "test";
            //label_modval.Text = "test";
            // label_losval.Text = "test";
            //
            Startautoset_button.BackColor = Color.Honeydew;
            Startautoset_button.Text = GlobalVarFun.moduleType.ToString() + "：模块正在自动测试中，请等待......";
            progressBar1.Value = 5;
            //Refresh();
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////
            errorMessage = "";
            testLog_textBox.ForeColor = Color.Red;
            //
            if (GlobalVarFun.testType == "firstTest")
            {
                FirstTestProcess(); // 初测调试处理
            }
            else // (GlobalVarFun.testType == "finalTest")
            {
                FinalTestProcess(); // 终测检查处理
            }            
            ///////////////////////////////////////////////////////////////////////////////////////////////

            RTN_POS:
            timer.Stop();
            str = timer.Elapsed.ToString();
            //str = str.Substring(6, 5);
            str = str.Substring(3, 7);
            label_testtime.Text = "测试时间: " + str + "s";
            Refresh();
        }

        // 初测调试函数
        //==============================================================================================================================================//
        private bool FirstTestProcess()
        {
            progressBar1.Value = 10;
            Refresh();       
            
            // * 进入接收、发射调试 *//
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 进入调试模式
            if (test.SetDebugPWD() == false)
            {
                Startautoset_button.BackColor = Color.Red;
                Startautoset_button.Text = "待测模块进入调试模式失败,确认模块类型是否正确, 请插入下一只模块......";
                AddTestLog("模块进入调试模式失败！");
                return false;
            }

            //带TEC方案
            if (GlobalVarFun.tx_tec_test)
            {
                Thread.Sleep(1000);//
                if (test.SetTx_EN() != true)
                {
                    AddTestLog("模块Tx使能操作失败！");
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "模块Tx使能失败......";
                    return false;
                }
                else
                {
                    AddTestLog("模块Tx使能操作成功！");
                }
                Thread.Sleep(3000);//等待TEC启动
            }

            // 0、写入TX-PE等调试参数 
            if (GlobalVarFun.tx_pe_test)
            {
                if (test.WriteTxRxDefaultVal() == false)
                {
                    AddTestLog("写入TX-PE等调试参数失败！");
                    //
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "写入TX-PE等调试参数失败, 请插入下一只模块......";
                    return false;
                }
            }

            if (GlobalVarFun.txrx_cdr_dis)
            {
                // TxRxCDR 控制操作
                if (test.DisTxRxCDR(true) == false)
                {
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = GlobalVarFun.moduleType.ToString() + "：待测模块TxRxCDR操作失败, 请插入下一只模块......";
                    AddTestLog("模块TxRxCDR操作失败！");
                    return false;
                }
                //
                AddTestLog("TxRx_CDR操作完成！");
            }
            
            if (GlobalVarFun.rx_ddm_test)
            {
                for (int i = 0; i < 4; i++)
                {
                    TestSet.ch = i;
                    btnCHState.Text = "Rx通道" + TestSet.ch.ToString();//显示当前测试通道 2024.12.04
                    Refresh();
                    if (TestResult.fibertop_pn.Contains("MM"))
                    {
                        try
                        {
                            opticalSwitchSet(i + 1);//光开关切换通道
                        }
                        catch
                        {
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = GlobalVarFun.moduleType.ToString() + "：待测模块调试失败,光开关异常......";
                            AddTestLog("光开关异常！");
                            return false;
                        }
                    }
                    else
                    {
                        if (GlobalVarFun.optoSwitch_connected)
                        {
                            opticalSwitchSet(i + 1);//光开关切换通道
                        }
                        else
                        {
                            if (test.SourceSoftEn(i) == false)//开启光源通道i
                            {
                                GlobalVarFun.usb_can_use = false;
                            }
                        }
                    }
                    //APD调试
                    if (GlobalVarFun.APDen)
                    {
                        // 关闭自动温补功能
                        if (test.TxTempLookupTableCtrl(false) == false)
                        {
                            AddTestLog("CH" + i.ToString() + "关闭温度补偿失败！");
                            //
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = "CH" + i.ToString() + "关闭温度补偿失败, 请插入下一只模块......";
                            return false;
                        }

                        SetDOA_RxAttVal(DOA.rxDLosAttBuf[i] - 6);  //灵敏度点-3，测误码率
                        //AddTestLog("SetDOA_RxAttVal :" + (DOA.rxDLosAttBuf[i] - 4).ToString());
                        //SetDOA_RxAttVal(17);  //灵敏度点-3，测误码率
                        //AddTestLog("SetDOA_RxAttVal(20);");
                        //误码率点检查
                        PSSSenseClear(i.ToString());
                        string  status = GetPSSStatus(i.ToString());
                        if (status.Contains("Y N"))
                        { 
                            int loop = 10;
                            float val = DOA.rxDLosAttBuf[i] - 6;
                            do
                            {
                                val--;
                                if (val < 8) break;//APD 收光小于-7dbm
                                SetDOA_RxAttVal(DOA.rxDLosAttBuf[i] - 6);
                                PSSSenseClear(i.ToString());
                                status = GetPSSStatus(i.ToString());
                                loop--;
                            }
                            while ((loop < 0) || (!status.Contains("Y N")));
                        }
                        if (AutoTestRxAPD() == false)
                        {
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = GlobalVarFun.moduleType.ToString() + "：待测模块APD调试失败, 请插入下一只模块......";
                            AddTestLog("模块APD调试失败！");
                            return false;
                        }
                        test.SaveRxDataAfterDebug();//
                    }      

                    if (RxPwrDDMAutoCal() == false)
                    {
                        AddTestLog("CH" + i.ToString() + "接收DDM自动校准失败！");
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH" + i.ToString() + "模块接收DDM 自动校准失败, 请插入下一只模块......";
                        return false;
                    }
                    //
                    if (RxPwrErrorCheck() == false)
                    {
                        AddTestLog("CH" + i.ToString() + "接收DDM检测精度超出设定范围！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH" + i.ToString() + "模块接收校准 DDM精度检查失败, 请插入下一只模块......";
                        return false;
                    }
                    
                    if (GlobalVarFun.rx_los_test)
                    {
                        if (RxLosAutoSet() == false)
                        {
                            AddTestLog("CH" + i.ToString() + "LOS告警功能自动调试失败！");
                            //label_losval.Text = TestResult.rxlosVal.ToString(); // 显示los调试结果
                            //
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = "CH" + i.ToString() + "待测模块自动调试Los功能失败, 请插入下一只模块......";
                            AddTestLog("CH" + i.ToString() + "模块Los调试失败！");
                            return false;
                        }
                        if (RxLosAlarmCheck() == false)
                        {
                            AddTestLog("CH" + i.ToString() + "LOS功能检查失败！");
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = "CH" + i.ToString() + "模块接收LOS或告警功能 检查失败, 请插入下一只模块......";
                            return false;
                        }
                    }
                    //
                    if (test.SaveRxDataAfterDebug() == false)
                    {
                        AddTestLog("CH" + i.ToString() + "保存Rx接收调试参数失败！");
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH" + i.ToString() + "保存Rx接收调试参数失败, 请插入下一只模块......";
                        return false;
                    }
                    progressBar1.Value = progressBar1.Value + i * 10;
                    Refresh();
                    //
                    if (RxSenBitErrorCheck() == false)
                    {
                        AddTestLog("CH" + i.ToString() + "接收RxSen检测出现误码！");
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH" + i.ToString() + "模块接收灵敏度RxSen检查失败, 请插入下一只模块......";
                        return false;
                    }
                    //
                    GlobalVarFun.record_need_save = true;
                }
            }
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 2、发射调试
            //
            if (GlobalVarFun.tx_test)
            {
                //波长调试
                for (int i = 0; i < 4; i++)
                {
                    TestSet.ch = i;
                    btnCHState.Text = "Tx通道" + TestSet.ch.ToString();//显示当前测试通道 2024.12.04
                    Refresh();
                    if (TestResult.fibertop_pn.Contains("MM"))
                    {
                        opticalSwitchSet(i + 1);//光开关切换通道
                    }
                    else
                    {
                        if (GlobalVarFun.optoSwitch_connected)
                        {
                            opticalSwitchSet(i + 1);//光开关切换通道
                        }
                        else
                        {
                            //test.SourceSoftEn(i);//开启光源通道i
                            test.SoftTxCHEn(i);
                        }
                    }
                    // 关闭发射自动温补功能
                    if (test.TxTempLookupTableCtrl(false) == false)
                    {
                        AddTestLog("CH" + i.ToString() + "关闭发射温度补偿失败！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH" + i.ToString() + "关闭发射温度补偿失败, 请插入下一只模块......";
                        return false;
                    }

                    //TOSA温度调试
                    if (GlobalVarFun.TOSATempEN)
                    {
                        if (TestSet.EMLTestType == 0)
                        {
                            //40G
                            if (AutoTestEML(TestSet.tosatemp_min, TestSet.tosatemp_max) == false)
                            {
                                AddTestLog("TxTemp调试失败！");
                                Startautoset_button.BackColor = Color.Red;
                                Startautoset_button.Text = "TxTemp调试失败, 请插入下一只模块......";
                                return false;
                            }
                        }
                        else
                        {
                            //100G
                            if (AutoTestEML_100GLR() == false)
                            {
                                AddTestLog("TxTemp调试失败！");
                                Startautoset_button.BackColor = Color.Red;
                                Startautoset_button.Text = "TxTemp调试失败, 请插入下一只模块......";
                                return false;
                            }
                        }

                    }
                
                }
                //
                for (int i = 0; i < 4; i++)
                {
                    TestSet.ch = i;                
                    btnCHState.Text = "Tx通道" + TestSet.ch.ToString();//显示当前测试通道 2024.12.04
                    Refresh();
                    if (TestResult.fibertop_pn.Contains("MM"))
                    {
                        opticalSwitchSet(i + 1);//光开关切换通道
                    }
                    else
                    {
                        if (GlobalVarFun.optoSwitch_connected)
                        {
                            opticalSwitchSet(i + 1);//光开关切换通道
                        }
                        else
                        {
                            //test.SourceSoftEn(i);//开启光源通道i
                            test.SoftTxCHEn(i);
                        }
                    }
                    // 关闭发射自动温补功能
                    if (test.TxTempLookupTableCtrl(false) == false)
                    {
                        AddTestLog("CH"+i.ToString()+"关闭发射温度补偿失败！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH"+i.ToString()+"关闭发射温度补偿失败, 请插入下一只模块......";
                        return false;
                    }
                    ////TOSA温度调试
                    //if (GlobalVarFun.TOSATempEN)
                    //{
                    //    if (TestSet.EMLTestType == 0)
                    //    {
                    //        //40G
                    //        if (AutoTestEML(TestSet.tosatemp_min, TestSet.tosatemp_max) == false)
                    //        {
                    //            AddTestLog("CH" + i.ToString() + "TxTemp调试失败！");
                    //            Startautoset_button.BackColor = Color.Red;
                    //            Startautoset_button.Text = "CH" + i.ToString() + "TxTemp调试失败, 请插入下一只模块......";
                    //            return false;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //100G
                    //        if (AutoTestEML_100GLR() == false)
                    //        {
                    //            AddTestLog("CH" + i.ToString() + "TxTemp调试失败！");
                    //            Startautoset_button.BackColor = Color.Red;
                    //            Startautoset_button.Text = "CH" + i.ToString() + "TxTemp调试失败, 请插入下一只模块......";
                    //            return false;
                    //        }
                    //    }
                        
                    //}
                    //if (TestResult.fibertop_pn.Contains("MM"))
                    //{
                    //    opticalSwitchSet(i + 1);//光开关切换通道
                    //}
                    //else
                    //{
                    //    if (GlobalVarFun.optoSwitch_connected)
                    //    {
                    //        opticalSwitchSet(i + 1);//光开关切换通道
                    //    }
                    //    else
                    //    {
                    //        //test.SourceSoftEn(i);//开启光源通道i
                    //        test.SoftTxCHEn(i);
                    //    }
                    //}
                    //TOSA VON 调试
                    if (GlobalVarFun.VONEN)
                    {
                        if (AutoSetVON() == false)
                        {
                            AddTestLog("CH" + i.ToString() + "VON调试失败！");
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = "CH" + i.ToString() + "VON调试失败, 请插入下一只模块......";
                            return false;
                        }
                    }
                    //
                    //判断此通道的发射是否已经调试过
                    if (TxDebugIsOKCheck() == false)
                    {
                        if (TxPowerAutoSet() == false)
                        {
                            AddTestLog("CH" + i.ToString() + "发射光功率调试失败：" + errorMessage);
                            //label_apcval.Text = TestResult.txapcVal.ToString();
                            //
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = "CH" + i.ToString() + "发射光功率调试失败, 请插入下一只模块......";
                            return false;
                        }
                        //
                        //Converted_analog_values(); // 更新界面DDM信息
                        if (TxErAutoSet() == false)
                        {
                            AddTestLog("CH" + i.ToString() + "发射消光比调试失败：" + errorMessage);
                            //label_modval.Text = TestResult.txmodVal.ToString();
                            //
                            Startautoset_button.BackColor = Color.Red;
                            Startautoset_button.Text = "CH" + i.ToString() + "发射消光比调试失败, 请插入下一只模块......";
                            return false;
                        }
                        if (errorMessage != "")
                        {
                            AddTestLog("CH" + i.ToString() + errorMessage);
                        }
                    }
                    //
                    //发射光功率较准
                    //TestResult.txPower = Get_TxOptoPower();
                    TestResult.txPowerbuf[i] = TestResult.txPower;
                    if (test.WriteTxCalData() == false) //写入发射校准参数
                    {
                        errorMessage += "Tx发光校准出现错误";
                        AddTestLog(errorMessage);
                        return false;
                    }
                    //
                    //Refresh();
                    if (test.SaveTxDataAfterDebug() == false)
                    {
                        AddTestLog("CH" + i.ToString() + "保存Tx发射调试参数失败！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH" + i.ToString() + "保存Tx发射调试参数失败, 请插入下一只模块......";
                        return false;
                    }
                    Converted_analog_values(); //更新界面DDM信息

                    //获取 DDM TXPower
                    TestResult.txPowerDDM = (float)test.GetTxPwr();
                    TestResult.txPowerDDMbuf[i] = TestResult.txPowerDDM;
                    TestResult.txPwrErrbuf[i] = TestResult.txPwrErr = TestResult.txPowerDDM - TestResult.txPower;
                    if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr)
                    {
                        AddTestLog("CH" + i.ToString() + "DDM发射光功率偏差超出范围：" + TestResult.txPwrErr.ToString("0.00"));
                        return false;
                    }
                    /*if ((TestResult.txPowerDDM > TestSet.txPwr_Max) || (TestResult.txPowerDDM < TestSet.txPwr_Min))
                    {
                        AddTestLog("CH" + i.ToString() + "DDM发射光功率超出设定范围：" + TestResult.txPowerDDM.ToString("0.00"));
                        return false;
                    }*/

                    //Tx发射参数检查
                    /*if (TxFinalTestCheck(false) == false)
                    {
                        AddTestLog("CH" + i.ToString() + "模块发射光功率和消光比参数异常！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "CH" + i.ToString() + "模块发射光功率和消光比 检查失败, 请插入下一只模块......";
                        return false;
                    }*/
                    //
                    GlobalVarFun.record_need_save = true;
                    progressBar1.Value = progressBar1.Value + i * 10;
                    Refresh();
                }
            }
            progressBar1.Value = 90;
            Refresh();
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
       //CHECK_POS:
            //
            // 3. 读取模块flash调试信息
            if (test.GetFlashInfoDebug() == false)
            {
                AddTestLog("读取模块flash调试信息失败！");
                //
                Startautoset_button.BackColor = Color.Red;
                Startautoset_button.Text = "读取模块flash调试信息失败, 请插入下一只模块......";
                return false;
            }
            //
            fsn_textBox.Text = TestResult.fibertop_sn; //界面显示待测FSN流水号
            //
            progressBar1.Value = 95;
            Refresh();
            //
            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 4、保存参数到数据库
            if ((GlobalVarFun.sql_connect_status == true) && (GlobalVarFun.record_need_save == true))
            {
                GlobalVarFun.record_need_save = false;
                //
                if (backgroundWorkerAutoSet.IsBusy)
                {
                    Startautoset_button.BackColor = Color.Yellow;
                    Startautoset_button.Text = "模块初测调试完成，未保存到SQL数据库，请检查数据库连接！！请插入下一只模块......";
                    AddTestLog("SQL数据库写入初测记录进程被占用，初测参数未保存！");
                    return false;
                }
                // 启动后台进程 保存测试数据到数据库
                backgroundWorkerAutoSet.RunWorkerAsync();
                //barcode_textBox.Text = sn_textBox.Text; ///////////////////////////////test
                AddTestLog(errorMessage);
                errorMessage = "";
            }
            else
            {
                AddTestLog("初测记录未保存到SQL数据库！");
            }
            //
            //开启Tx全通道
            //test.SoftTxDis(false);//enable,Tx初测完成，显示4个通道bias,TxPower            
            //
            testLog_textBox.ForeColor = Color.Green;
            AddTestLog("初测调试完成！");
            progressBar1.Value = 100;
            Startautoset_button.BackColor = Color.Green;
            Startautoset_button.Text = TestResult.sn.TrimEnd() + "初测调试完成, 请插入下一只模块......";
            Refresh();
            return true;
        }
        //==============================================================================================================================================//

        // 终测处理函数
        //==============================================================================================================================================//
        private bool FinalTestProcess()
        {
            string errMsg = "";

            //带TEC方案
            if (GlobalVarFun.tx_tec_test)
            {
                Thread.Sleep(2000);//
                //if (test.SetTx_EN() != true)
                //{
                //    AddTestLog("模块Tx使能操作失败！");
                //    Startautoset_button.BackColor = Color.Red;
                //    Startautoset_button.Text = "模块Tx使能失败......";
                //    return false;
                //}
                //else
                //{
                //    AddTestLog("模块Tx使能操作成功！");
                //}
                Thread.Sleep(3000);//等待TEC启动
            }
            for (int i = 0; i < 4; i++)
            {
                TestSet.ch = i;

                if (TestResult.fibertop_pn.Contains("MM"))
                {
                    opticalSwitchSet(i + 1);//光开关切换通道
                }
                else
                {
                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(i + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(i) == false)//开启光源通道i
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                        test.SoftTxCHEn(i);//开启待测模块通道i
                    }
                }
                // 0、判断是否进行关闭TxRxCDR //2020.4.8
                if (GlobalVarFun.txrx_cdr_dis)
                {
                    // TxRxCDR 控制操作
                    if (test.DisTxRxCDR(true) == false)
                    {
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = GlobalVarFun.moduleType.ToString() + "：待测模块TxRxCDR操作失败, 请插入下一只模块......";
                        AddTestLog("模块TxRxCDR操作失败！");
                        return false;
                    }
                    //
                    AddTestLog("TxRx_CDR操作完成！");
                }
                progressBar1.Value = 10;
                Refresh();
                //

                // 1、接收LOS 及 DDM 告警项目 功能检查
                if (GlobalVarFun.rx_ddm_test)
                {
                    if (RxLosAlarmCheck() == false)
                    {
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "模块接收LOS或告警功能 检查失败, 请插入下一只模块......";
                        return false;
                    }
                    //
                    if (RxPwrErrorCheck() == false)
                    {
                        AddTestLog("接收DDM检测精度超出设定范围！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "模块接收校准 DDM精度检查失败, 请插入下一只模块......";
                        return false;
                    }
                    //
                    if (RxSenBitErrorCheck() == false)
                    {
                        AddTestLog("接收RxSen检测出现误码！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "模块接收灵敏度RxSen检查失败, 请插入下一只模块......";
                        return false;
                    }
                    //
                    GlobalVarFun.record_need_save = true;
                }
                progressBar1.Value = 40;
                Refresh();
                //
                ///////////////////////////////////////////////////////////////////////////////////////////////
                //
                // 2、检测发光功率、消光比
                if (GlobalVarFun.tx_test)
                {
                    if (TxFinalTestCheck(true) == false)
                    {
                        AddTestLog("模块发射光功率和消光比参数异常！");
                        //
                        Startautoset_button.BackColor = Color.Red;
                        Startautoset_button.Text = "模块发射光功率和消光比 检查失败, 请插入下一只模块......";
                        return false;
                    }
                    //
                    GlobalVarFun.record_need_save = true;
                }
                progressBar1.Value = 70;
                Refresh();
            }

            ///////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 3、检测模块告警门限
            if (GlobalVarFun.threshold_check)
            {
                if (test.CheckThresholdsInfo(ref errMsg) == false)
                {
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "待测模块告警门限检查错误, 请插入下一只模块......";
                    AddTestLog("告警门限检查错误: " + errMsg);
                    return false;
                }
                //
                //GlobalVarFun.record_need_save = true;
            }
            progressBar1.Value = 80;
            Refresh();
            ///////////////////////////////////////////////////////////////////////////////////////////////
            // 4、模块调试参数 检查
            if (GlobalVarFun.flash_check)
            {
                // 进入调试模式
                if (test.SetDebugPWD() == false)
                {
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "待测模块进入调试模式失败, 请插入下一只模块......";
                    AddTestLog("模块进入调试模式失败！");
                    return false;
                }
                //读取模块调试信息
                if (test.GetFlashInfoDebug() == false)
                {
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "待测模块调试信息读取失败, 请插入下一只模块......";
                    AddTestLog("读取模块调试信息失败！");
                    return false;
                }
                //
                fsn_textBox.Text = TestResult.fibertop_sn; //界面显示待测FSN流水号
                //
                progressBar1.Value = 80;
                Refresh();
                //
                if (test.CheckModuleFlashInfo(ref errMsg) == false)
                {
                    AddTestLog("Falsh信息检查错误: " + errMsg);
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "模块 flash_data 参数检查失败, 请插入下一只模块......";
                    return false;
                }
                //
                GlobalVarFun.record_need_save = true;
            }
            progressBar1.Value = 90;
            Refresh();
            ///////////////////////////////////////////////////////////////////////////////////////////////
            //
            // 5、保存参数到数据库
            if ((GlobalVarFun.sql_connect_status == true) && (GlobalVarFun.record_need_save == true))
            {
                GlobalVarFun.record_need_save = false;
                //
                if (backgroundWorkerAutoSet.IsBusy)
                {
                    Startautoset_button.BackColor = Color.Yellow;
                    Startautoset_button.Text = "模块终测参数OK，未保存到SQL数据库，请检查数据库连接！！请插入下一只模块......";
                    AddTestLog("SQL数据库写入终测记录进程被占用，终测参数未保存！");
                    return false;
                }
                // 启动后台进程 保存测试数据到数据库
                backgroundWorkerAutoSet.RunWorkerAsync();
                barcode_textBox.Text = sn_textBox.Text; ///////////////////////////////test
                AddTestLog(errorMessage);
                errorMessage = "";
            }
            else
            {
                AddTestLog("终测记录未保存到SQL数据库！");
            }
            //
            testLog_textBox.ForeColor = Color.Green;
            AddTestLog("终测检查完成！");
            progressBar1.Value = 100;
            Startautoset_button.BackColor = Color.Green;
            Startautoset_button.Text = TestResult.sn.TrimEnd() + "终测检查完成, 请插入下一只模块......";
            Refresh();
            return true;
        }
        //==============================================================================================================================================//

        // 接收LOS自动调试
        private bool RxLosAutoSet()
        {
            UInt16 min = TestSet.rxlos_Min_set;//TestSet.rxlos_Min;
            UInt16 max = TestSet.rxlos_Max_set;//TestSet.rxlos_Max;
            UInt16 los_val = 0;
            int ch = 0;      
            ch = TestSet.ch;
            
            if (min > max || max > 255)
            {
                AddTestLog("自动调试Los范围错误！");
                return false;
            }
            
            if (min == max) // 当min == max时，固定设置。
            {
                los_val = min;
                return test.SetRxLos(los_val);
            }

            // 收光调整到去告警点 DLOS
            SetDOA_RxAttVal(DOA.rxDLosAttBuf[ch]);

            // 1. 检查最小点是否可以产生DELOS 去告警
            los_val = min;
            if (test.SetRxLos(los_val) == false)  return false;
            if (test.CheckRxLOS() == true) // LOS告警
            {
                AddTestLog("自动调试Los最小值去告警错误！");
                return false;
            }

            // 收光调整到告警点 LOS
            SetDOA_RxAttVal(DOA.rxALosAttBuf[ch]);

            // 2. 检查最大点是否可以产生LOS
            los_val = max;
            if (test.SetRxLos(los_val) == false) return false;
            if (test.CheckRxLOS() == false) // 不产生LOS告警
            {
                AddTestLog("自动调试Los最大值告警错误！");
                return false;
            }

            // 3. 检查中点是否可以产生LOS
            los_val  = min;
            los_val += max;
            los_val /= 2;
            if (test.SetRxLos(los_val) == false) return false;
            if (test.CheckRxLOS() == false) // 不产生LOS告警
            {
                min = los_val;
            }

            // 4. 自动调试Los
            los_val = min;
            do
            {
                if (test.SetRxLos(los_val) == false) return false;
                // 检查LOS
                if (test.CheckRxLOS() == true) // 产生告警LOS
                {
                    // DELOS  去告警
                    SetDOA_RxAttVal(DOA.rxDLosAttBuf[ch]);
                    // DELOS 去告警
                    if (test.CheckRxLOS() == false) // 去告警OK
                    {
                        switch(ch)
                        {
                            case 0: lbLOSValCh0.Text = los_val.ToString();
                                break;
                            case 1: lbLOSValCh1.Text = los_val.ToString();
                                break;
                            case 2: lbLOSValCh2.Text = los_val.ToString();
                                break;
                            case 3: lbLOSValCh3.Text = los_val.ToString();
                                break;
                            default:
                                break;
                        }
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
            switch (ch)
            {
                case 0: lbLOSValCh0.Text = los_val.ToString();
                    break;
                case 1: lbLOSValCh1.Text = los_val.ToString();
                    break;
                case 2: lbLOSValCh2.Text = los_val.ToString();
                    break;
                case 3: lbLOSValCh3.Text = los_val.ToString();
                    break;
                default:
                    break;
            }
            //
            return false;
        }

        
        // 接收光点设置校准功能
       // private void TestDataCheck_button_Click(object sender, EventArgs e)
       // {
        //    float err = 0;
        //    float range = 0.2f;

        //    testDataCheck_button.BackColor = System.Drawing.Color.Gray;

        //    if ((optoMeter_connected == false) || (optoAtt_connected == false)) // 连接光功率计和光衰减器判断
        //    {
        //        MessageBox.Show("请先连接光功率计和光衰减器！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    real_rxpower1_textbox.Text = rxPwrtextBox1.Text;
        //    real_rxpower2_textbox.Text = rxPwrtextBox2.Text;
        //    real_rxpower3_textbox.Text = rxPwrtextBox3.Text;
        //    real_rxpower4_textbox.Text = rxPwrtextBox4.Text;
        //    real_rxpower5_textbox.Text = rxPwrtextBox5.Text;
        //    ddm_rxpower1_textbox.Text = "";
        //    ddm_rxpower2_textbox.Text = "";
        //    ddm_rxpower3_textbox.Text = "";
        //    ddm_rxpower4_textbox.Text = "";
        //    ddm_rxpower5_textbox.Text = "";

        //    real_rxpower6_textbox.Text = "-40";

        //    Refresh();

        //    TestResult.rxPwrReal[0] = Convert.ToSingle(rxPwrtextBox1.Text);
        //    TestResult.rxPwrReal[1] = Convert.ToSingle(rxPwrtextBox2.Text);
        //    TestResult.rxPwrReal[2] = Convert.ToSingle(rxPwrtextBox3.Text);
        //    TestResult.rxPwrReal[3] = Convert.ToSingle(rxPwrtextBox4.Text);
        //    TestResult.rxPwrReal[4] = Convert.ToSingle(rxPwrtextBox5.Text);

        //    TestResult.rxSen = Convert.ToSingle(textBox_Sen.Text);
        //    TestResult.rxDLos = Convert.ToSingle(textBox_DLos.Text);
        //    TestResult.rxALos = Convert.ToSingle(textBox_ALos.Text);
        //    TestResult.rxOverLoad = Convert.ToSingle(textBox_overLoad.Text);

        //    //
        //    DOA.rxCalAtt[0] = DOA.rxCheckAtt[0];
        //    DOA.rxCalAtt[1] = DOA.rxCheckAtt[1];
        //    DOA.rxCalAtt[2] = DOA.rxCheckAtt[2];
        //    DOA.rxCalAtt[3] = DOA.rxCheckAtt[3];
        //    DOA.rxCalAtt[4] = DOA.rxCheckAtt[4];

        //    testDataIsOK = true;

        //    // RX SEN
        //    SetDOA_RxAttVal(DOA.rxSenAtt);
        //    err = TestResult.rxSen - Get_OptoPower_Meter();
        //    if (Math.Abs(err) > range)
        //    {
        //        testDataIsOK = false;
        //    }

        //    // RX DLOS
        //    SetDOA_RxAttVal(DOA.rxDLosAtt);
        //    err = TestResult.rxDLos - Get_OptoPower_Meter();
        //    if (Math.Abs(err) > range)
        //    {
        //        testDataIsOK = false;
        //    }

        //    // RX ALOS
        //    SetDOA_RxAttVal(DOA.rxALosAtt);
        //    err = TestResult.rxALos - Get_OptoPower_Meter();
        //    if (Math.Abs(err) > range)
        //    {
        //        testDataIsOK = false;
        //    }

        //    // RX OVERLOAD
        //    SetDOA_RxAttVal(DOA.rxOverLoadAtt);
        //    err = TestResult.rxOverLoad - Get_OptoPower_Meter();
        //    if (Math.Abs(err) > range)
        //    {
        //        testDataIsOK = false;
        //    }

        //    // CHECK 
        //    SetDOA_RxAttVal(DOA.rxCheckAtt[0]);
        //    TestSet.rxPwr_Cal[0] = Get_OptoPower_Meter();
        //    err = TestResult.rxPwrReal[0] - TestSet.rxPwr_Cal[0];
        //    if (Math.Abs(err) > range)
        //    {
        //        testDataIsOK = false;
        //    }

        //    // CHECK 
        //    SetDOA_RxAttVal(DOA.rxCheckAtt[1]);
        //    TestSet.rxPwr_Cal[1] = Get_OptoPower_Meter();
        //    err = TestResult.rxPwrReal[1] - TestSet.rxPwr_Cal[1];
        //    if (Math.Abs(err) > range)
        //    {
        //        testDataIsOK = false;
        //    }

        //    // CHECK 
        //    SetDOA_RxAttVal(DOA.rxCheckAtt[2]);
        //    TestSet.rxPwr_Cal[2] = Get_OptoPower_Meter();
        //    err = TestResult.rxPwrReal[2] - TestSet.rxPwr_Cal[2];
        //    if (Math.Abs(err) > range)
        //    {
        //        testDataIsOK = false;
        //    }

        //    if (radioButton_APD.Checked)
        //    {
        //        // CHECK 
        //        SetDOA_RxAttVal(DOA.rxCheckAtt[3]);
        //        TestSet.rxPwr_Cal[3] = Get_OptoPower_Meter();
        //        err = TestResult.rxPwrReal[3] - TestSet.rxPwr_Cal[3];
        //        if (Math.Abs(err) > range)
        //        {
        //            testDataIsOK = false;
        //        }

        //        // CHECK 
        //        SetDOA_RxAttVal(DOA.rxCheckAtt[4]);
        //        TestSet.rxPwr_Cal[4] = Get_OptoPower_Meter();
        //        err = TestResult.rxPwrReal[4] - TestSet.rxPwr_Cal[4];
        //        if (Math.Abs(err) > range)
        //        {
        //            testDataIsOK = false;
        //        }
        //    }
        //    //

        //    // 接收DDM 校准时使用，把[1]改成[2]小 1dB
        //    if (radioButton_PIN.Checked)
        //    { 
        //           if (DOA.rxCalAtt[2] > 2)
        //            {
        //                DOA.rxCalAtt[1] = DOA.rxCalAtt[2] - 1;
        //                SetDOA_RxAttVal(DOA.rxCalAtt[1]);
        //                TestSet.rxPwr_Cal[1] = Get_OptoPower_Meter();
        //            }
        //            else
        //            {
        //                testDataIsOK = false;
        //            }
        //    }

        //    if (testDataIsOK == true)
        //    {
        //        testDataCheck_button.BackColor = System.Drawing.Color.GreenYellow;
        //    }
        //    else
        //    {
        //        testDataCheck_button.BackColor = System.Drawing.Color.Yellow;
        //        MessageBox.Show("测试参数设置异常，精度为 +-0.2dB ！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return;
        //    }
        //}

        //// 计算RX CAL参数
        private bool CulRxCalPar()
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
            rxcaldbm1 = TestSet.rxPwr_Cal[0] / 10;
            y[0] = Math.Pow(10, rxcaldbm1) * 10000;
            rxcaldbm2 = TestSet.rxPwr_Cal[1] / 10;
            y[1] = Math.Pow(10, rxcaldbm2) * 10000;
            rxcaldbm3 = TestSet.rxPwr_Cal[2] / 10;
            y[2] = Math.Pow(10, rxcaldbm3) * 10000;
            //
            x[0] = rxAdc[0];
            x[1] = rxAdc[1];
            x[2] = rxAdc[2];
            //
            if (GlobalVarFun.rx_is_apd)
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
                Bit.iapcir(x, y, 3, a, 2, dt);  // PIN//3,2
            }

            TestResult.rxPwrCal_c[0] = (float)a[0];
            TestResult.rxPwrCal_c[1] = (float)a[1];
            TestResult.rxPwrCal_c[2] = (float)a[2];
            TestResult.rxPwrCal_c[3] = (float)a[3];
            TestResult.rxPwrCal_c[4] = (float)a[4];

            if ((Math.Abs(a[0]) > 1000) || (Math.Abs(a[1]) > 1000))
            {
                return false;
            }
        
            return true;
        }

        // 接收DDM自动校准
        private bool RxPwrDDMAutoCal()
        {
            int ch = 0;
            if (!TestResult.fibertop_pn.Contains("MM"))//LR /ZR
            {
                ch = TestSet.ch;
            }
            //ch = TestSet.ch;
            //设置TXSFP光源1
            SetDOA_RxAttVal(DOA.rxCalAtt[ch * 5 + 0]);
            rxAdc[0] = test.GetRxADC();
            ddm_rxpower1_textbox.Text = rxAdc[0].ToString();
            real_rxpower1_textbox.Text = (TestSet.rxPwr_Real[0]).ToString("F2");

            //设置TXSFP光源2
            SetDOA_RxAttVal(DOA.rxCalAtt[ch * 5 + 1]);
            rxAdc[1] = test.GetRxADC();
            ddm_rxpower2_textbox.Text = rxAdc[1].ToString();
            //real_rxpower2_textbox.Text = (TestSet.rxPwr_Cal[1]).ToString("F2");
            real_rxpower2_textbox.Text = (TestSet.rxPwr_Real[1]).ToString("F2");

            //设置TXSFP光源3
            SetDOA_RxAttVal(DOA.rxCalAtt[ch * 5 + 2]);
            rxAdc[2] = test.GetRxADC();
            ddm_rxpower3_textbox.Text = rxAdc[2].ToString();
            real_rxpower3_textbox.Text = (TestSet.rxPwr_Real[2]).ToString("F2");

            if (GlobalVarFun.rx_is_apd) // APD 检查后面2个点
            {
                //设置TXSFP光源4
                SetDOA_RxAttVal(DOA.rxCalAtt[ch * 5 + 3]);
                rxAdc[3] = test.GetRxADC();
                ddm_rxpower4_textbox.Text = rxAdc[3].ToString();
                real_rxpower4_textbox.Text = (TestSet.rxPwr_Real[3]).ToString("F2");

                //设置TXSFP光源5
                SetDOA_RxAttVal(DOA.rxCalAtt[ch * 5 + 4]);
                rxAdc[4] = test.GetRxADC();
                ddm_rxpower5_textbox.Text = rxAdc[4].ToString();
                real_rxpower5_textbox.Text = (TestSet.rxPwr_Real[4]).ToString("F2");
            }

            //设置TXSFP光源 为无光状态
            SetDOA_RxAttVal(60);
            rxAdc[5] = test.GetRxADC();
            rxAdc[5] += 3; //加大 预防跳动问题
            ddm_rxpower6_textbox.Text = rxAdc[5].ToString();
            if (rxAdc[5] > 63) // 最大63
            {
                rxAdc[5] = 63;
                ddm_rxpower1_textbox.Text += "++";
            }
            TestResult.rxNoPwrVal = (byte)rxAdc[5];

            //计算校准参数
            if (CulRxCalPar() == false)
            {
                return false;
            }

            // 写入校准参数到模块
            if (test.WriteRxCalData() == false)
            {
                return false;
            }

            return true;
        }

        // 接收DDM校准精度检查
        private bool RxPwrErrorCheck()
        {
            float rxpow = 0;
            float temp = 0;
            int ch = 0;
            if (!TestResult.fibertop_pn.Contains("MM"))
            {
                ch = TestSet.ch;
                // test.SourceSoftEn(ch);
            }
            //ch = TestSet.ch;
            // 收无光检测
            if (GlobalVarFun.rx_nopower_test)
            {
                if (DOA.currentAtt != 60)
                {
                    SetDOA_RxAttVal(60);
                }
                //
                rxpow = test.GetRxPower();
                ddm_rxpower6_textbox.Text = rxpow.ToString("F2");
                if (rxpow > -40)
                {
                    return false;
                    //if (testType_textBox1.Text != "SFPP-GN1196")//
                    //{
                    //    return false;
                    //}
                }
            }

            if (GlobalVarFun.rx_is_apd == true) // APD 检查后面2个点
            {
                //设置TXSFP光源5
                SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 4]);
                rxpow = test.GetRxPower();
                ddm_rxpower5_textbox.Text = rxpow.ToString("F2");
                TestResult.rxPwrDDM[4] = rxpow;

                //设置TXSFP光源4
                SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 3]);
                rxpow = test.GetRxPower();
                ddm_rxpower4_textbox.Text = rxpow.ToString("F2");
                TestResult.rxPwrDDM[3] = rxpow;

                TestResult.rxPwrErr[3] = Convert.ToSingle(ddm_rxpower4_textbox.Text) - Convert.ToSingle(real_rxpower4_textbox.Text);
                temp = Math.Abs(TestResult.rxPwrErr[3]);
                if (temp > rxPwrMaxErr)
                {
                    return false;
                }

                TestResult.rxPwrErr[4] = Convert.ToSingle(ddm_rxpower5_textbox.Text) - Convert.ToSingle(real_rxpower5_textbox.Text);
                temp = Math.Abs(TestResult.rxPwrErr[4]);
                if (temp > rxPwrMaxErr)
                {
                    return false;
                }
            }

            //设置TXSFP光源3
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 2]);
            rxpow = test.GetRxPower();
            ddm_rxpower3_textbox.Text = rxpow.ToString("F2");
            TestResult.rxPwrDDM[2] = rxpow;

            //设置TXSFP光源2
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 1]);
            rxpow = test.GetRxPower();
            ddm_rxpower2_textbox.Text = rxpow.ToString("F2");
            TestResult.rxPwrDDM[1] = rxpow;
            real_rxpower2_textbox.Text = (TestSet.rxPwr_Real[1]).ToString("F2");


            //设置TXSFP光源1
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 +0]);
            rxpow = test.GetRxPower();
            ddm_rxpower1_textbox.Text = rxpow.ToString("F2");
            TestResult.rxPwrDDM[0] = rxpow;

            TestResult.rxPwrErr[0] = Convert.ToSingle(ddm_rxpower1_textbox.Text) - Convert.ToSingle(real_rxpower1_textbox.Text);
            temp = Math.Abs(TestResult.rxPwrErr[0]);
            if (temp > rxPwrMaxErr)
            {
                return false;
            }

            TestResult.rxPwrErr[1] = Convert.ToSingle(ddm_rxpower2_textbox.Text) - Convert.ToSingle(real_rxpower2_textbox.Text);
            temp = Math.Abs(TestResult.rxPwrErr[1]);
            if (temp > rxPwrMaxErr)
            {
                return false;
                //if (testType_textBox1.Text.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }

            TestResult.rxPwrErr[2] = Convert.ToSingle(ddm_rxpower3_textbox.Text) - Convert.ToSingle(real_rxpower3_textbox.Text);
            temp = Math.Abs(TestResult.rxPwrErr[2]);
            if (temp > rxPwrMaxErr)
            {
                //return false;
                if (testType_textBox1.Text.Trim() == "SFPP-GN1196")//GN1196跳过检查点3
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        // RxSen 灵敏度检测
        private bool RxSenBitErrorCheck()
        {
            //切换到灵敏度光点 观测误码情况
            //
            //SetDOA_RxAttVal(DOA.rxALosAtt);
            //
            //SetDOA_RxAttVal(DOA.rxOverLoadAtt); //饱和点//
            //Thread.Sleep(500);//
            //SetDOA_RxAttVal(DOA.rxSenAtt);      //灵敏度点//

            string errmsg = "";
            string Status = "";
            string channel = "";
            int ch = 0;
            if (!TestResult.fibertop_pn.Contains("MM"))
            {
                ch = TestSet.ch;
                // test.SourceSoftEn(ch);
                pssChannel = ch.ToString();//
            }
            else
            {
                pssChannel = TestSet.ch.ToString();//
            }
            channel = pssChannel;    
      
            SetDOA_RxAttVal(DOA.rxOverLoadAttBuf[ch]); //饱和点
            Thread.Sleep(500);

            if (GlobalVarFun.sen_test == true)
            {
                PSSSenseClear(channel);//清除误码率  
                Thread.Sleep(1000);

                errmsg += sencheck(channel);

                Status = GetPSSStatus(channel);

                if (Status.Substring(Status.Length - 3) == "Y Y")//误码
                {

                    if (TestResult.fibertop_pn.Contains("HG-"))
                    {
                        errmsg += sencheck(channel);
                    }
                    else
                    {
                        if (Status.Substring(22, 10).Trim() == "0.00000e0")
                        {
                            //饱和光功率测试PASS
                        }
                        else
                        {
                            errmsg += "饱和光功率测试失败：\r\n";//误码
                            errmsg += Status + "\r\n";//误码率
                        }
                    }
                }
                else if (Status.Substring(Status.Length - 3) == "Y N")//同步
                {
                    if (TestResult.fibertop_pn.Contains("HG-"))
                    {
                        errmsg += sencheck(channel);
                    }
                    else
                    {
                        if (Status.Substring(22, 10).Trim() == "0.00000e0")
                        {
                            //饱和光功率测试PASS
                        }
                        else
                        {
                            errmsg += "饱和光功率测试失败：\r\n";//误码
                            errmsg += Status + "\r\n";//误码率
                        }
                    }
                    errmsg += sencheck(channel);
                }
                else if (Status.Substring(Status.Length - 3) == "N Y")//失步
                {
                    errmsg += "饱和光功率测试失败，失步：\r\n";//误码
                    errmsg += Status + "\r\n";//误码率
                }
                else
                {
                    errmsg += "饱和光功率测试失败,误码异常1：\r\n";//误码
                    errmsg += Status + "\r\n";//误码率
                }
            }

            SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
            Thread.Sleep(500);//

            if (GlobalVarFun.sen_test == true)
            {            
                PSSSenseClear(channel);//清除误码率              
                errmsg += sencheck(channel);

                SetDOA_RxAttVal(DOA.rxSenAttBuf[ch] - 2);      //灵敏度点 
                Thread.Sleep(200);//
                errmsg += sencheck(channel);

                SetDOA_RxAttVal(DOA.rxSenAttBuf[ch] - 4);      //灵敏度点
                Thread.Sleep(200);//
                errmsg += sencheck(channel);

                //SetDOA_RxAttVal(DOA.rxSenAtt - 3);      //灵敏度点
                //Thread.Sleep(500);//
                //errmsg += sencheck(channel);

                //Thread.Sleep(500);//
                SetDOA_RxAttVal(DOA.rxOverLoadAttBuf[ch] + 1);  //光饱和点
                Thread.Sleep(500);//
                errmsg += sencheck(channel);

                //SetDOA_RxAttVal(DOA.rxOverLoadAtt + 2);  //光饱和点
                //Thread.Sleep(500);//
                //errmsg +=  sencheck(channel);

                SetDOA_RxAttVal(DOA.rxOverLoadAttBuf[ch] + 3);  //光饱和点
                Thread.Sleep(500);//
                errmsg += sencheck(channel);

                //Thread.Sleep(500);//
                SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
                Thread.Sleep(200);//
                errmsg += sencheck(channel);
                AddTestLog(errmsg);
                //Thread.Sleep(500);//
                //
                if (errmsg != "")
                {
                    return false;
                }
            }
            return true;
        }
      
        private string GetPSSStatus(string channel)
        {
            string status = string.Empty;
            string ins = string.Empty;
            string acc = string.Empty;
            string command = string.Empty;
            //PSSSenseClear(channel);//

            command = "Status:Result? " + channel;
            //command = "Status" + channel +":Result?";
            GlobalVarFun.pssert.WriteLine(command);
            Thread.Sleep(GlobalVarFun.pss_bert_delay);//1000
            status = GlobalVarFun.pssert.ReadLine();        
            return status;
        }
        /// <summary>
        /// 清除PSS误码
        /// </summary>
        /// <param name="channel"></param>
        private void PSSSenseClear(string channel)
        {
            string command = string.Empty;
            command = "Sense:Clear " + channel;
            GlobalVarFun.pssert.WriteLine(command);
            Thread.Sleep(100);//100
        }

        private string sencheck(string pssch)
        {
            string error = "";
            string Status = "";
            string str25G = "5.000000E-005";
            double num25G = 0;
            double numnew = 0;
            int index = 0;
            try
            {
                Status = GetPSSStatus(pssch);
               
                if (Status.Contains("N Y"))
                {
                    for (int i = 3; i > 0; i--)
                    {
                        PSSSenseClear(pssch);
                        Status = GetPSSStatus(pssch);
                        if (!Status.Contains("N Y")) break;                      
                    }
                }
                num25G = Convert.ToDouble(str25G);

                if (Status.Substring(Status.Length - 3) == "Y Y")//误码
                {
                    if (TestResult.fibertop_pn.Contains("HG-"))
                    {
                        index = Status.Length;
                        //numnew = Convert.ToDouble(Status.Substring(22, index-22-2-1).Trim());//10
                        numnew = Convert.ToDouble(Status.Substring(22 , 11).Trim());//10
                        if (numnew <= num25G)
                        {
                            //灵敏度测试PASS
                        }
                        else
                        {
                            error += "灵敏度测试失败：\r\n";//误码
                            error += Status + "\r\n";//误码率
                            error += "numnew" + numnew .ToString()+ "\r\n";//
                            error += "num25G" + num25G.ToString() + "\r\n";//
                        }
                    }
                    else
                    {
                        error += "灵敏度测试失败：\r\n";//误码
                        error += Status + "\r\n";//误码率
                    }
                }
                else if (Status.Substring(Status.Length - 3) == "Y N")//同步
                {
                    if (TestResult.fibertop_pn.Contains("HG-"))
                    {
                        //numnew = Convert.ToDouble(Status.Substring(22, index - 22 - 2 - 1).Trim());//10
                        numnew = Convert.ToDouble(Status.Substring(22, 10).Trim());//10
                        if (numnew > 0)
                        {
                            numnew = Convert.ToDouble(Status.Substring(22, 11).Trim());//10
                        }
                        if (numnew <= num25G)
                        {
                            //灵敏度测试PASS
                        }
                        else
                        {
                            error += "灵敏度测试失败：\r\n";//误码
                            error += Status + "\r\n";//误码率
                            error += "numnew" + numnew.ToString() + "\r\n";//
                            error += "num25G" + num25G.ToString() + "\r\n";//
                        }
                    }
                    else
                    {
                        if (Status.Substring(21, 10).Trim() == "0.00000e0")//22
                        {
                            //灵敏度测试PASS
                        }
                        else
                        {
                            error += "灵敏度测试失败：\r\n";//误码
                            error += Status + "\r\n";//误码率
                        }
                    }
                }
                else if (Status.Substring(Status.Length - 3) == "N Y")//失步
                {
                    error += "灵敏度测试失败：\r\n";//误码
                    error += Status + "\r\n";//误码率
                }
                else
                {
                    error += "灵敏度测试失败,误码异常2：\r\n";//误码
                    error += Status + "\r\n";//误码率
                }
            }
            catch
            {
                error += "灵敏度测试失败,误码异常3：\r\n";//误码
                error += Status + "\r\n";//误码率
            }

            return error;
        }

        // 接收LOS 及告警功率 功能检查
        private bool RxLosAlarmCheck()
        {
            string errmsg = "";
            float att_val = 0;
            int ch = 0;
            if (!TestResult.fibertop_pn.Contains("MM"))
            {
                ch = TestSet.ch;
                // test.SourceSoftEn(ch);
            }
           // ch = TestSet.ch;
            //先切换到灵敏度点
            SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);

            //de los  去告警
            SetDOA_RxAttVal(DOA.rxDLosAttBuf[ch]);
            // LOS 去告警
            if (test.CheckRxLOS() == true)
            {
                errmsg += "LOS 去告警异常_01 \r\n";
                //return false;
            }

            if (GlobalVarFun.hw_los_test)
            {
                if (i2c.HardWare_LOS_Get() == true)
                {
                    errmsg += "硬件LOS 去告警异常 \r\n";
                }
            }
            // 逐步+2dB逼近 设置
            att_val = DOA.rxDLosAttBuf[ch];
            att_val = att_val + 2.0f;
            while (att_val < DOA.rxALosAttBuf[ch])
            {
                SetDOA_RxAttVal(att_val);
                att_val = att_val + 2.0f;
            }

            // los 告警
            if (GlobalVarFun.testType == "firstTest")
            {
                SetDOA_RxAttVal(DOA.rxALosAttBuf[ch] + 0.8f); // 后置0.8dB
            }
            else
            {
                SetDOA_RxAttVal(DOA.rxALosAttBuf[ch]);
            }
            // LOS 告警
            if (test.CheckRxLOS() == false)
            {
                errmsg += "LOS告警异常 \r\n";
                //return false;
            }

            if (GlobalVarFun.hw_los_test)
            {
                if (i2c.HardWare_LOS_Get() == false)
                {
                    errmsg += "硬件LOS 告警异常 \r\n";
                }
            }
            // 逐步-1dB逼近 设置
            att_val = DOA.rxALosAttBuf[ch];
            att_val = att_val - 1.0f;
            while (att_val > DOA.rxDLosAttBuf[ch])
            {
                SetDOA_RxAttVal(att_val);
                att_val = att_val - 1.0f;
            }

            //de los  去告警
            SetDOA_RxAttVal(DOA.rxDLosAttBuf[ch]);
            // LOS 去告警
            if (test.CheckRxLOS() == true)
            {
                errmsg += "LOS 去告警异常_02 \r\n";
                //return false;
            }
            //
            AddTestLog(errmsg);
            //
            if (errmsg != "")
            {
                return false;
            }

            return true;
        }

        // 发射部分测试检查
        private bool TxFinalTestCheck(bool autoScale)
        {
            float tx_pwr = 0, tx_er = 0, bias = 0, tx_cr = 0, tx_jt = 0;
            double wlgth = 0;
            string errmsg = "";
            int ch = TestSet.ch;
            // Tx 发射关闭  无光显示-40检查
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            if (GlobalVarFun.tx_nopower_test)
            {
                if (test.SoftTxDis(true) == false) // Tx Disable 软件关闭发射
                {
                    errmsg = "软件关闭Tx发光失败！\r\n";
                    AddTestLog(errmsg);
                    return false;
                }
                Thread.Sleep(100);
                if (test.GetTxPower() > -40)
                {
                    errmsg = "Tx发射无光显示-40检查失败！\r\n";
                    AddTestLog(errmsg);
                    return false;
                }
                if (test.SoftTxCHEn(ch) == false) // Tx Enable 软件开启发射
                {
                    errmsg = "软件开启Tx发光操作失败01！\r\n";
                    AddTestLog(errmsg);
                    return false;
                }
                Thread.Sleep(100);
                //
                if (test.GetTxBias() < 2) //bias<2mA
                {
                    if (test.SoftTxCHEn(ch) == false) // Tx Enable 软件开启发射  异常后再次开启
                    {
                        errmsg = "软件开启Tx发光操作失败02！\r\n";
                        AddTestLog(errmsg);
                        return false;
                    }
                    Thread.Sleep(100);
                    if (test.GetTxBias() < 2) //bias<2mA
                    {
                        errmsg = "软件开启Tx发光操作失败03！\r\n";
                        AddTestLog(errmsg);
                        return false;
                    }
                }
            }

            if (GlobalVarFun.hw_txdis_test)
            {
                if (i2c.setModuleDis(true) == false) // Tx Disable 硬件关闭发射
                {
                    errmsg = "硬件关闭Tx发光失败！\r\n";
                    AddTestLog(errmsg);
                    return false;
                }
                Thread.Sleep(100);
                if (test.GetTxPower() > -40)
                {
                    errmsg = "Tx发射无光显示-40检查失败！\r\n";
                    AddTestLog(errmsg);
                    return false;
                }
                if (i2c.setModuleDis(false) == false) // Tx Enable 硬件开启发射
                {
                    errmsg = "硬件开启Tx发光操作失败01！\r\n";
                    AddTestLog(errmsg);
                    return false;
                }
                Thread.Sleep(100);
                //
                if (test.GetTxBias() < 2) //bias<2mA
                {
                    if (test.SoftTxDis(false) == false) // Tx Enable 硬件开启发射  异常后再次开启
                    {
                        errmsg = "硬件开启Tx发光操作失败02！\r\n";
                        AddTestLog(errmsg);
                        return false;
                    }
                    Thread.Sleep(100);
                    if (test.GetTxBias() < 2) //bias<2mA
                    {
                        errmsg = "硬件开启Tx发光操作失败03！\r\n";
                        AddTestLog(errmsg);
                        return false;
                    }
                }
            }
            //////////////////////////////////////////////////////////////////////////////////////////////////////

            // 读取DDM
            Converted_analog_values();

            // 选择用光功率计读取光功率
            if (GlobalVarFun.power_use_DAC == false)
            {
                tx_pwr = Get_TxOptoPower();
                if (tx_pwr < -30) //光太小
                {
                    errmsg = "光功率计读取到的Tx发光太小！\r\n";
                    AddTestLog(errmsg);
                    return false;
                }
            }

            //2023.3.1修改
            if (instrument_connected == true)
            {
                // 读取发射参数
                if (GlobalVarFun.DCA86100D_Open || GlobalVarFun.N1092x_Open)
                {
                    if (Get_86100D_TxEyeData_DCA(autoScale) == false)
                    {
                        errmsg = "眼图仪读取Tx参数失败！\r\n";
                        AddTestLog(errmsg);
                        return false;
                    }
                }
                else
                {
                    if (Get_TxEyeData_DCA(autoScale) == false)
                    {
                        errmsg = "眼图仪读取Tx参数失败！\r\n";
                        AddTestLog(errmsg);
                        return false;
                    }
                }
                tx_er = TestResult.txErbuf[ch];

                // 选择用DCA眼图仪 读取光功率
                if (GlobalVarFun.power_use_DAC == true)
                {
                   // tx_pwr = TestResult.txPowerDCA + (float)(GlobalVarFun.opto_att_offset); // 加偏差
                    tx_pwr = TestResult.txPowerDCA + (float)(GlobalVarFun.opto_att_offsetbuf[ch]); // 加偏差
                }
            }
            //
            TestResult.txPowerbuf[ch] = tx_pwr;
            TestResult.txPwrErr = TestResult.txPowerDDM - TestResult.txPower;
            bias = TestResult.txBiasDDMbuf[ch];
            tx_cr = TestResult.txCrossingbuf[ch];
            tx_jt = TestResult.txJiterTTbuf[ch];
            wlgth = TestResult.wLength[ch];
            //
            switch (ch)
            { 
                case 0:
                    txpower_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                    er_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    Bias_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    txWlch0_textBox.Text = "---" + "/" + wlgth.ToString("F2");
                    txJt_textBox.Text = "---" + "/" + tx_jt.ToString("F1");
                    break;
                case 1:
                    txpowerch1_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                    erch1_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    Biasch1_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    txWlch1_textBox.Text = "---" + "/" + wlgth.ToString("F2");
                    txJtch1_textBox.Text = "---" + "/" + tx_jt.ToString("F1");
                    break;
                case 2:
                    txpowerch2_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                    erch2_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    Biasch2_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    txWlch2_textBox.Text = "---" + "/" + wlgth.ToString("F2");
                    txJtch2_textBox.Text = "---" + "/" + tx_jt.ToString("F1");
                    break;
                case 3:
                    txpowerch3_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + tx_pwr.ToString("F1");
                    erch3_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    Biasch3_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    txWlch3_textBox.Text = "---" + "/" + wlgth.ToString("F2");
                    txJtch3_textBox.Text = "---" + "/" + tx_jt.ToString("F1");
                    break;
                default:
                    break;
            }
            //
            // 检查
            if (Math.Abs(TestResult.txPwrErrbuf[ch]) > txPwrMaxErr)  errmsg += "发光功率监控值与实际发光偏差超出设定范围！\r\n";
            //
            if (tx_pwr > TestSet.txPwr_Max)  errmsg += "光功率超过最大值！\r\n";
            if (tx_pwr < TestSet.txPwr_Min)  errmsg += "光功率超过最小值！\r\n";
            //
            if (bias > TestSet.bias_Max)     errmsg += "Bias超过最大值！\r\n";
            if (bias < TestSet.bias_Min)     errmsg += "Bias超过最小值！\r\n";
            //
            //2023.3.1修改
            if (instrument_connected == true)
            {
                if (tx_er > TestSet.txEr_Max) errmsg += "消光比超过最大值！\r\n";
                if (tx_er < TestSet.txEr_Min) errmsg += "消光比超过最小值！\r\n";
                //
                if (tx_cr > TestSet.txCr_Max) errmsg += "交叉点超过最大值！\r\n";
                if (tx_cr < TestSet.txCr_Min) errmsg += "交叉点超过最小值！\r\n";
                //
                //Jitter Total 检查功能
                if (GlobalVarFun.tx_jitter_test)
                {
                    if (tx_jt >= TestSet.txJt_Max) errmsg += "抖动Jt超过最大值！\r\n";
                }
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

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // 待测模块发光功率自动调试 
        private bool TxDebugIsOKCheck()
        {
            float bias, pwr;
            int ch = TestSet.ch;

            //发射强制重新初调参数
            if (txMustDebug_checkBox.Checked)
            {
                return false;
            }

            TestResult.txPowerbuf[ch] = TestResult.txPower = pwr = Get_TxOptoPower();
            TestResult.txBiasDDM = bias = test.GetTxBias();
            if ((pwr < TestSet.txPwr_Min) || (pwr > TestSet.txPwr_Max) || (bias < TestSet.bias_Min) || (bias > TestSet.bias_Max))
            {
                return false;
            }

            TestResult.txPowerDDM = (float)test.GetTxPwr();
            TestResult.txPwrErrbuf[ch] = TestResult.txPwrErr = TestResult.txPowerDDM - TestResult.txPower;
            if (Math.Abs(TestResult.txPwrErr) > txPwrMaxErr)
            {
                return false;
            }

            TestResult.txapcVal = apc = test.GetTxApcBiasSet();
            TestResult.txmodVal = mod = test.GetTxModBiasSet();
            if ((apc < TestSet.txapc_Min_set) || (apc > TestSet.txapc_Max_set) || (mod < TestSet.txmod_Min_set) || (mod > TestSet.txmod_Max_set))
            {
                return false;
            }

            if (Get_ERatio_DCA(true) == false) return false; //AUTO
            TestResult.txErErrbuf[ch] = TestResult.txErbuf[ch] - TestSet.txEr_target;
            if ((TestResult.txErbuf[ch] < TestSet.txEr_Min) || (TestResult.txErbuf[ch] > TestSet.txEr_Max))
            {
                return false;
            }

            //界面显示
            switch (ch)
            {
                case 0:
                    txpower_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Bias_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    er_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                case 1:
                    txpowerch1_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Biasch1_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    erch1_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                case 2:
                    txpowerch2_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Biasch2_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    erch2_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                case 3:
                    txpowerch3_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Biasch3_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    erch3_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                default:
                    break;
            }
            
            switch (ch) //APC MOD调试参数显示
            {
                case 0: lbAPCValCh0.Text = apc.ToString(); lbMODValCh0.Text = mod.ToString();
                    break;
                case 1: lbAPCValCh1.Text = apc.ToString(); lbMODValCh1.Text = mod.ToString();
                    break;
                case 2: lbAPCValCh2.Text = apc.ToString(); lbMODValCh2.Text = mod.ToString();
                    break;
                case 3: lbAPCValCh3.Text = apc.ToString(); lbMODValCh3.Text = mod.ToString();
                    break;
                default:
                    break;
            }

            return true;
        }

        // 待测模块发光功率自动调试 
        private bool TxPowerAutoSet()
        {
            // 选择调试方法
            if (GlobalVarFun.txpwr_debug_method == 0x00)
            {
                return AutoSetTxPower_MethodA(); // 线性计算法 apc-->uw & bias
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x11)
            {
                return AutoSetTxPower_MethodB(); // 普通二分法 apc-->dBm
            }
            else if (GlobalVarFun.txpwr_debug_method == 0x22)
            {
                return AutoSetTxPower_MethodC(); // 定值判断法 for DC耦合TOSA COB-LD
            }
            else
            {
                MessageBox.Show("发光功率自动调试方法错误，请选择正确的方法！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; //未定义 错误返回
            }
        }
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //
        // 自动调试发光功率 TX POWER 函数
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 待测模块发光功率自动调试  // 方法A  apc-->uW & bias_mA 线性关系
        private bool AutoSetTxPower_MethodA()
        {
            apc = (UInt16)((TestSet.txapc_Min_set + TestSet.txapc_Max_set) / 2);
            if (test.SetTxApcBias(apc) == false) return false;
            return true;
        }

        // 待测模块发光功率自动调试  // 方法B  根据dBm  用普通二分法
        private bool AutoSetTxPower_MethodB()
        {
            float bias, pwr;
            int ch = TestSet.ch;

            apc = (UInt16)((TestSet.txapc_Min_set + TestSet.txapc_Max_set) / 2);
            if (test.SetTxApcBias(apc) == false) return false;

            //mod = (UInt16)((TestSet.txmod_Min_set + TestSet.txmod_Max_set) / 2);
            mod = 0;
            if (test.SetTxModBias(mod) == false) return false;

            TestResult.txPower = pwr = Get_TxOptoPower();
            TestResult.txBiasDDM = bias = test.GetTxBias();

            //调试信息界面显示
            switch (ch)
            {
                case 0: lbAPCValCh0.Text = apc.ToString();
                    lbMODValCh0.Text = mod.ToString();
                    txpower_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    Bias_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    break;
                case 1: lbAPCValCh1.Text = apc.ToString();
                    lbMODValCh1.Text = mod.ToString();
                    txpowerch1_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    Biasch1_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    break;
                case 2: lbAPCValCh2.Text = apc.ToString();
                    lbMODValCh2.Text = mod.ToString();
                    txpowerch2_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    Biasch2_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    break;
                case 3: lbAPCValCh3.Text = apc.ToString();
                    lbMODValCh3.Text = mod.ToString();
                    txpowerch3_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + pwr.ToString("F1");
                    Biasch3_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + bias.ToString("F1");
                    break;
                default:
                    break;
            }

            if ((bias <= 1) || (pwr <= -30))
            {
                errorMessage += "光功率或者Bias异常偏小: ";
                return false;
            }

            return true;
        }

        // 待测模块发光功率自动调试  // 方法C  用差值二分法 apc-->uW
        private bool AutoSetTxPower_MethodC()
        {
            return false;
        }        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //
        // 待测模块消光比自动调试
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////        
        private bool TxErAutoSet()
        {
            if (GlobalVarFun.txer_debug_method == 00)
            {
                return AutoSetTxEr_MethodA(); // 普通二分法
            }
            else if (GlobalVarFun.txer_debug_method == 11)
            {
                return AutoSetTxEr_MethodB();
            }
            else
            {
                return false;
            }
        }

        // 待测模块消光比自动调试  普通二分法
        private bool AutoSetTxEr_MethodA()
        {
            UInt16 apc_min = TestSet.txapc_Min_set;
            UInt16 apc_max = TestSet.txapc_Max_set;
            UInt16 mod_min = TestSet.txmod_Min_set;
            UInt16 mod_max = TestSet.txmod_Max_set;
            float er_err, er_target, bias, pwr;
            int looptime = 0;
            int ch = TestSet.ch;
            bool b_rtn = true;
            UInt16 apc_step, mod_step;
            float fk_apc = 0.06f;
            float fk_mod = 0.05f;

            er_target = TestSet.txEr_target;

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
                
                er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;

                if (er_err > 0)
                {
                    apc_min = (UInt16)(apc + 1);
                }
                else
                {
                    apc_max = (UInt16)(apc - 1);
                }
            } while ((Math.Abs(er_err) > erValMaxErr) && (apc_max > apc_min) && (looptime < 8));

            apc_min = TestSet.txapc_Min_set;
            apc_max = TestSet.txapc_Max_set;

            pwr = Get_TxOptoPower();
            bias = test.GetTxBias();

            if (Math.Abs(er_err) > erValMaxErr)
            {
                errorMessage += "Tx消光比ER调试异常: ";
                b_rtn = false;
                goto RTN_POS;
            }

            apc_step = (UInt16)((float)(apc * fk_apc) + 4.0f); //四舍五入法 //0.5
            if (apc_step < 1) apc_step = 1;
            if (apc_step > 5) apc_step = 5;

            mod_step = (UInt16)((float)(mod * fk_mod) + 4.0f); //四舍五入法 //0.5
            if (mod_step < 1) mod_step = 1;
            if (mod_step > 5) mod_step = 5;

            errorMessage = "";
            //优化调试Tx发光功率和消光比ER
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // 1.发光小于最小值
            if (pwr <= TestSet.txPwr_Min)
            {
                if (bias > TestSet.bias_Max) //Bias大于最大值
                {
                    errorMessage += "Tx调试异常: ";
                    b_rtn = false;
                    goto RTN_POS;
                }

                errorMessage = " 微调 1 \r";

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
                        if (TestResult.txErbuf[ch] >= er_target)
                        {
                            break; //跳出循环
                        }
                    }

                    er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                    pwr = Get_TxOptoPower();
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
                    errorMessage += "Tx调试异常: ";
                    b_rtn = false;
                    goto RTN_POS;
                }

                errorMessage = " 微调 2 \r";

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
                        if (TestResult.txErbuf[ch] <= er_target)
                        {
                            break; //跳出循环
                        }
                    }

                    er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                    pwr = Get_TxOptoPower();
                    bias = test.GetTxBias();
                    looptime++;
                } while ((pwr > TestSet.txPwr_target) && (bias > TestSet.txBias_target) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                goto RTN_POS;
            }
            // 3.发光正常偏大
            else if ((pwr > TestSet.txPwr_target) && (pwr < TestSet.txPwr_Max))
            {
                if (bias > (TestSet.txBias_target+TestSet.bias_Max) / 2)
                {
                    errorMessage = " 微调 30 \r";
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
                            if (TestResult.txErbuf[ch] <= er_target)
                            {
                                break; //跳出循环
                            }
                        }

                        er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                        pwr = Get_TxOptoPower();
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

                    errorMessage = " 微调 31 \r";

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
                            if (TestResult.txErbuf[ch] >= er_target)
                            {
                                break; //跳出循环
                            }
                        }

                        er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                        pwr = Get_TxOptoPower();
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
                        errorMessage = " 微调 40 \r";
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
                                    if (TestResult.txErbuf[ch] >= er_target)
                                    {
                                        break; //跳出循环
                                    }
                                }
                                looptime++;
                                er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                                pwr = Get_TxOptoPower();
                                bias = test.GetTxBias();
                            } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 3));
                            goto RTN_POS;
                        }

                        errorMessage = " 微调 41 \r";
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
                                if (TestResult.txErbuf[ch] >= er_target)
                                {
                                    break; //跳出循环
                                }
                            }

                            er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                            pwr = Get_TxOptoPower();
                            bias = test.GetTxBias();
                            looptime++;
                        } while ((pwr < TestSet.txPwr_target) && (bias < (TestSet.bias_Max * 0.93f)) && (apc < apc_max) && (mod < mod_max) && (looptime < 6));
                        //
                        goto RTN_POS;
                    }

                    errorMessage = " 微调 42 \r";
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
                            if (TestResult.txErbuf[ch] <= er_target)
                            {
                                break; //跳出循环
                            }
                        }

                        er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                        pwr = Get_TxOptoPower();
                        bias = test.GetTxBias();
                        looptime++;
                    } while ((pwr > TestSet.txPwr_Min) && (bias > TestSet.bias_Max) && (apc > apc_min) && (mod > mod_min) && (looptime < 6));
                }
                else // (bias <= TestSet.txBias_target)
                {
                    errorMessage = " 微调 43 \r";
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
                            if (TestResult.txErbuf[ch] >= er_target)
                            {
                                break; //跳出循环
                            }
                        }

                        er_err = TestResult.txErbuf[ch] - TestSet.txEr_target;
                        pwr = Get_TxOptoPower();
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
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        RTN_POS:
            TestResult.txPower = pwr;
            TestResult.txBiasDDM = bias;
            TestResult.txErErrbuf[ch] = er_err;

            //界面显示
            switch (ch)
            {
                case 0:
                    txpower_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Bias_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    er_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                case 1:
                    txpowerch1_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Biasch1_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    erch1_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                case 2:
                    txpowerch2_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Biasch2_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    erch2_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                case 3:
                    txpowerch3_textBox.Text = TestSet.txPwr_target.ToString("F1") + "/" + TestResult.txPower.ToString("F1");
                    Biasch3_textBox_cal.Text = TestSet.txBias_target.ToString("F1") + "/" + TestResult.txBiasDDM.ToString("F1");
                    erch3_textBox.Text = TestSet.txEr_target.ToString("F1") + "/" + TestResult.txErbuf[ch].ToString("F1");
                    break;
                default:
                    break;
            }
            
            switch (ch) //APC MOD调试参数显示
            {
                case 0: lbAPCValCh0.Text = apc.ToString(); lbMODValCh0.Text = mod.ToString();
                    break;
                case 1: lbAPCValCh1.Text = apc.ToString(); lbMODValCh1.Text = mod.ToString();
                    break;
                case 2: lbAPCValCh2.Text = apc.ToString(); lbMODValCh2.Text = mod.ToString();
                    break;
                case 3: lbAPCValCh3.Text = apc.ToString(); lbMODValCh3.Text = mod.ToString();
                    break;
                default:
                    break;
            }

            //if (bias > TestSet.bias_Max)
            if (bias > (TestSet.bias_Max * 1.02f))
            {
                errorMessage += "Bias大 ";
                b_rtn = false;
            }
            if (bias < TestSet.bias_Min)
            {
                errorMessage += "Bias小 ";
                b_rtn = false;
            }
            if (pwr < TestSet.txPwr_Min)
            {
                errorMessage += "发光小 ";
                b_rtn = false;
            }
            if (pwr > TestSet.txPwr_Max)
            {
                errorMessage += "发光大 ";
                b_rtn = false;
            }
            if (TestResult.txErbuf[ch] < TestSet.txEr_Min)
            {
                errorMessage += "消光比ER小 ";
                b_rtn = false;
            }
            if (TestResult.txErbuf[ch] > TestSet.txEr_Max)
            {
                errorMessage += "消光比ER大 ";
                b_rtn = false;
            }

            return b_rtn;
        }
        // 待测模块消光比自动调试  普通二分法
        private bool AutoSetTxEr_MethodB()
        {
            return false;
        }
     
        private void btnSetup_Click(object sender, EventArgs e)
        {
            Setup_Form setup_form = new Setup_Form();
            timer1.Stop();
            setup_form.ShowDialog();
            timer1.Start();
        }

        private bool opticalSwitchSet(int ch)
        {
            string chnum;
            string command = "Configure:WorkChannel "+ch.ToString();
            //if (optoSwitch_connected == false)
            //{
            //}
            GlobalVarFun.opticalSwitch.WriteLine(command);
            chnum = GlobalVarFun.opticalSwitch.ReadLine();
            if (chnum.Contains(ch.ToString()))
            {
                return true;
            }
           return false;
        }
        
        //
        private void ClearTextVal()
        {
            lbAPCValCh0.Text = "val";
            lbAPCValCh1.Text = "val";
            lbAPCValCh2.Text = "val";
            lbAPCValCh3.Text = "val";
            lbMODValCh0.Text = "val";
            lbMODValCh1.Text = "val";
            lbMODValCh2.Text = "val";
            lbMODValCh3.Text = "val";
            lbLOSValCh0.Text = "val";
            lbLOSValCh1.Text = "val";
            lbLOSValCh2.Text = "val";
            lbLOSValCh3.Text = "val";

            lbTempValCh0.Text = "val";
            lbTempValCh1.Text = "val";
            lbTempValCh2.Text = "val";
            lbTempValCh3.Text = "val";
            lbVONValCh0.Text = "val";
            lbVONValCh1.Text = "val";
            lbVONValCh2.Text = "val";
            lbVONValCh3.Text = "val";
            lbAPDValCh0.Text = "val";
            lbAPDValCh1.Text = "val";
            lbAPDValCh2.Text = "val";
            lbAPDValCh3.Text = "val";

            txpower_textBox.Text = "";
            txpowerch1_textBox.Text = "";
            txpowerch2_textBox.Text = "";
            txpowerch3_textBox.Text = "";

            Bias_textBox_cal.Text = "";
            Biasch1_textBox_cal.Text = "";
            Biasch2_textBox_cal.Text = "";
            Biasch3_textBox_cal.Text = "";

            er_textBox.Text = "";
            erch1_textBox.Text = "";
            erch2_textBox.Text = "";
            erch3_textBox.Text = "";

            txWlch0_textBox.Text = "";
            txWlch1_textBox.Text = "";
            txWlch2_textBox.Text = "";
            txWlch3_textBox.Text = "";

            txJt_textBox.Text = "";
            txJtch1_textBox.Text = "";
            txJtch2_textBox.Text = "";
            txJtch3_textBox.Text = "";
        }

        private void rBSench0_CheckedChanged(object sender, EventArgs e)
        {
            int ch = 0;
            if (GlobalVarFun.optoAtt_connected == false)
            {
                MessageBox.Show("请连接衰减器！");
                return;
            }
            if (rBSench0.Checked)
            {
                if (TestResult.fibertop_pn.Contains("MM"))
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(ch + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(ch) == false)//开启光源通道i
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                    }
                }
                if (pnshow_textBox.Text.Contains("MM"))
                {
                    SetDOA_RxAttVal(DOA.rxSenAttBuf[0]);      //灵敏度点
                }
                else
                {
                    SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
                }              
                Thread.Sleep(500);//
            }
        }

        private void rBSench1_CheckedChanged(object sender, EventArgs e)
        {
            int ch = 1;
            if (GlobalVarFun.optoAtt_connected == false)
            {
                MessageBox.Show("请连接衰减器！");
                return;
            }
            if (rBSench1.Checked)
            {
                if (TestResult.fibertop_pn.Contains("MM"))
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(ch + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(ch) == false)//开启光源通道i
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                    }
                }
                //SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
                if (pnshow_textBox.Text.Contains("MM"))
                {
                    SetDOA_RxAttVal(DOA.rxSenAttBuf[0]);      //灵敏度点
                }
                else
                {
                    SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
                }   
                Thread.Sleep(500);//
            }
        }

        private void rBSench2_CheckedChanged(object sender, EventArgs e)
        {
            int ch = 2;
            if (GlobalVarFun.optoAtt_connected == false)
            {
                MessageBox.Show("请连接衰减器！");
                return;
            }
            if(rBSench2.Checked)
            if (TestResult.fibertop_pn.Contains("MM"))
            {
                opticalSwitchSet(ch + 1);//光开关切换通道
            }
            else
            {
                if (GlobalVarFun.optoSwitch_connected)
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    if (test.SourceSoftEn(ch) == false)//开启光源通道i
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            //SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
            if (pnshow_textBox.Text.Contains("MM"))
            {
                SetDOA_RxAttVal(DOA.rxSenAttBuf[0]);      //灵敏度点
            }
            else
            {
                SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
            }   
            Thread.Sleep(500);//
        }

        private void rBSench3_CheckedChanged(object sender, EventArgs e)
        {
            int ch = 3;
            if (GlobalVarFun.optoAtt_connected == false)
            {
                MessageBox.Show("请连接衰减器！");
                return;
            }
            if (rBSench3.Checked)
            {
                if (TestResult.fibertop_pn.Contains("MM"))
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(ch + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(ch) == false)//开启光源通道i
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                    }
                }
                //SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
                if (pnshow_textBox.Text.Contains("MM"))
                {
                    SetDOA_RxAttVal(DOA.rxSenAttBuf[0]);      //灵敏度点
                }
                else
                {
                    SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);      //灵敏度点
                }   
                Thread.Sleep(500);//
            }
        }
      
        //EML Auto Test
        private bool AutoTestEML(UInt16 emlvalmin, UInt16 emlvalmax)
        {
            int looptime = 0;
            UInt16 emlval = 0;
            Double wavelenth = 0;
            Double result_err = 0;
            // 普通二分法查找
            //try
            //{
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
                test.SetTOSATemp(emlval);
                try
                {
                    wavelenth = GlobalVarFun.kt86120c.GetWavelength();
                }
                catch { wavelenth = GlobalVarFun.kt86120c.GetWavelength(); }
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
            } while ((Math.Abs(result_err) > GlobalVarFun.wLengthMaxErr) && (emlvalmax > emlvalmin) && (looptime < 30));
            //}
            //catch
            //{
            //    errorMessage += "波长计读取异常";
            //    return false;
            //}        
            if ((Math.Abs(result_err) > GlobalVarFun.wLengthMaxErr))
            {
                errorMessage += "波长调试失败，与目标波长不符";
                return false;
            }
            TestResult.txtosaTemp = emlval;

            return true;
        }

        private bool AutoTestEML100G(UInt16 emlvalmin, UInt16 emlvalmax)
        {
            int looptime = 0;
            UInt16 emlval = 0;
            Double wavelenth = 0;
            Double result_err = 0;
            // 普通二分法查找
            //try
            //{
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
                test.SetTOSATemp(emlval);
                try
                {
                    wavelenth = GlobalVarFun.kt86120c.GetWavelength();
                }
                catch { wavelenth = GlobalVarFun.kt86120c.GetWavelength(); }
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
            } while ((Math.Abs(result_err) > GlobalVarFun.wLengthMaxErr) && (emlvalmax > emlvalmin) && (looptime < 30));

            TestResult.txtosaTemp = emlval;

            //if ((Math.Abs(result_err) > GlobalVarFun.wLengthMaxErr))
            //{
            //    errorMessage += "波长调试失败，与目标波长不符";
            //    return false;
            //}
            if ((Math.Abs(result_err) > GlobalVarFun.wLengthMaxErr))
            {
                if ((wavelenth < TestSet.wl_min[TestSet.ch]) || (wavelenth > TestSet.wl_max[TestSet.ch]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool AutoTestEML_100GLR()
        {
            UInt16 emlval = 0;

            //emlval = (UInt16)((TestSet.tosa_tempValmin + TestSet.tosa_tempValmax) / 2);
            if (TestSet.tosatemp_def > 1830) TestSet.tosatemp_def = 1830;
            if (TestSet.tosatemp_def < 830) TestSet.tosatemp_def = 830;
            test.SetTOSATemp(TestSet.tosatemp_def);
            TestResult.txtosaTemp = TestSet.tosatemp_def;
            //检测通道波长
            for (int ch = 0; ch < 4; ch++)
            {
                TestSet.ch = ch;
                if (Auto100GWLengthCheck() == false) return false;
                if (ch == 3) return true;
            }
            //min
            TestSet.wLength_target = TestSet.wl_min[TestSet.ch];
            if (AutoTestEML100G(830, 1830) == false) return false;
            TestSet.tosa_tempbufmin[TestSet.ch] = TestResult.txtosaTemp;
                        
            //max
            TestSet.wLength_target = TestSet.wl_max[TestSet.ch];
            if (AutoTestEML100G(830, 1830) == false) return false;
            TestSet.tosa_tempbufmax[TestSet.ch] = TestResult.txtosaTemp;
            //
            if (TestSet.ch == 3)
            {
                TestSet.tosa_tempValmin = TestSet.tosa_tempbufmin.Min();//最小值中的最大值
                TestSet.tosa_tempValmax = TestSet.tosa_tempbufmax.Max();//最大值中的最小值

                if (TestSet.tosa_tempValmin < TestSet.tosa_tempValmax) return false;

                emlval = (UInt16)((TestSet.tosa_tempValmin + TestSet.tosa_tempValmax) / 2);
                TestResult.txtosaTemp = emlval;
                test.SetTOSATemp(emlval);  
                //检测通道波长 波长微调
                AddTestLog("波长检查");
                if (WlgthFineTuning() == false) return false;
                //for (int ch = 0; ch < 4; ch++)
                //{
                //    TestSet.ch = ch;
                //    if (Auto100GWLengthCheck() == false)
                //    {
                //        if (WlgthFineTuning() == false) return false;
                //    }
                //}
            }         
            return true;
        }

        //波长微调
        private bool WlgthFineTuning()
        { 
            int looptimes = 10;
            UInt16 step = 150;//step 150 = 0.5nm
            Double wavelenth = 0;
            do
            {                          
                for (int ch = 0; ch < 4; ch++)
                {
                    TestSet.ch = ch;
                    if (Auto100GWLengthCheck() == false)
                    {
                        try
                        {
                            wavelenth = GlobalVarFun.kt86120c.GetWavelength();
                        }
                        catch
                        {
                            wavelenth = GlobalVarFun.kt86120c.GetWavelength();//
                        }
                        if (TestSet.wl_min[TestSet.ch] > wavelenth)
                        {
                            TestResult.txtosaTemp -= step;
                            break;
                        }
                        if (wavelenth > TestSet.wl_max[TestSet.ch])
                        {
                            TestResult.txtosaTemp += step;
                            break;
                        }
                    }
                    else
                    {
                        break;//检测ok
                    }
                }
                looptimes--;
            }
            while (looptimes > 0);

            AddTestLog("波长微调");

            if(looptimes < 0) return false;
            return true;
        }
        //WaveLength Auto Check
        private bool AutoCheckwLength()
        {
            Double wavelenth = 0;
            Double result_err = 0;
            int ch = TestSet.ch;
            //try
            //{
            try
            {
                wavelenth = GlobalVarFun.kt86120c.GetWavelength();
            }
            catch
            {
                wavelenth = GlobalVarFun.kt86120c.GetWavelength();//
            }
            result_err = wavelenth - TestSet.wLength_target;
            if (Math.Abs(result_err) > GlobalVarFun.wLengthMaxErr)
            {
                errorMessage += "波长检查失败，与目标波长不符";
                return false;
            }
            else
            {
                TestResult.wLength[ch] = wavelenth;
                switch (TestSet.ch)
                {
                    case 0: lbTempValCh0.Text = TestResult.txtosaTemp.ToString();
                        txWlch0_textBox.Text = wavelenth.ToString("F2");
                        break;
                    case 1: lbTempValCh1.Text = TestResult.txtosaTemp.ToString();
                        txWlch1_textBox.Text = wavelenth.ToString("F2");
                        break;
                    case 2: lbTempValCh2.Text = TestResult.txtosaTemp.ToString();
                        txWlch2_textBox.Text = wavelenth.ToString("F2");
                        break;
                    case 3: lbTempValCh3.Text = TestResult.txtosaTemp.ToString();
                        txWlch3_textBox.Text = wavelenth.ToString("F2");
                        break;
                    default:
                        break;
                }
                return true;
            }
            //}
            //catch
            //{
            //    return false;
            //}
        }

        private bool Auto100GWLengthCheck()
        {
            Double wavelenth = 0;
            //Double result_err = 0;
            int ch = TestSet.ch;          
            test.SoftTxCHEn(TestSet.ch);

            try
            {
                wavelenth = GlobalVarFun.kt86120c.GetWavelength();
            }
            catch
            {
                wavelenth = GlobalVarFun.kt86120c.GetWavelength();//
            }
            //result_err = wavelenth - TestSet.wLength_target;
            //if (Math.Abs(result_err) > GlobalVarFun.wLengthMaxErr)
            if ((TestSet.wl_min[TestSet.ch] > wavelenth) ||  (wavelenth > TestSet.wl_max[TestSet.ch]))
            {
                errorMessage += "波长检查失败，与目标波长不符";
                return false;
            }
            else
            {
                TestResult.wLength[ch] = wavelenth;
                switch (TestSet.ch)
                {
                    case 0: lbTempValCh0.Text = TestResult.txtosaTemp.ToString();
                        txWlch0_textBox.Text = wavelenth.ToString("F2");
                        break;
                    case 1: lbTempValCh1.Text = TestResult.txtosaTemp.ToString();
                        txWlch1_textBox.Text = wavelenth.ToString("F2");
                        break;
                    case 2: lbTempValCh2.Text = TestResult.txtosaTemp.ToString();
                        txWlch2_textBox.Text = wavelenth.ToString("F2");
                        break;
                    case 3: lbTempValCh3.Text = TestResult.txtosaTemp.ToString();
                        txWlch3_textBox.Text = wavelenth.ToString("F2");
                        break;
                    default:
                        break;
                }
                return true;
            }
        }
        
        //VON Auto Test
        private bool AutoSetVON()
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
            if (test.SetVON(vonval) == false)
            {
                errorMessage += "VON 负压调试失败";
                return false;
            }
            switch (TestSet.ch)
            {
                case 0: lbVONValCh0.Text = vonval.ToString();
                    break;
                case 1: lbVONValCh1.Text = vonval.ToString();
                    break;
                case 2: lbVONValCh2.Text = vonval.ToString();
                    break;
                case 3: lbVONValCh3.Text = vonval.ToString();
                    break;
                default:
                    break;
            }
            return true;
        }     
        //APD Auto Test
        private bool AutoTestRxAPD()
        {
            double[] psspertbuf = new double[256];
            double valmin = psspertbuf[0];
            byte valminindex = 0;
            string ch = "0";
            //string pssChannel = TestSet.ch.ToString();
            string status = "";
            int i = 0;
            string str = "";
            int index = 0;
            byte min = (byte)TestSet.rxapd_min;
            byte max = (byte)TestSet.rxapd_max;
            //AddTestLog("min:"+TestSet.rxapd_min.ToString() + "max:" + TestSet.rxapd_max.ToString());///////////////////
            for (int x = 0; x <= 255; x++)
            {
                psspertbuf[x] = 255;
            }
            valmin = psspertbuf[0];
            if (min == max)
            {
                test.SetAPD((byte)min);
                TestResult.rxapdVal = min;
                return true;
            }
            ch = TestSet.ch.ToString();//pssChannel.Trim().Substring(pssChannel.Trim().Length - 1);//截取PSS通道号
            PSSSenseClear(ch);
            for (i = min; i <= max; i = (i + 2))
            {
                if (i > max)
                {
                    i = max;
                }
                AddTestLog("APDval:" + i.ToString());///////////////////
                test.SetAPD((byte)i);
                Refresh();
                status = GetPSSStatus(ch);
                AddTestLog("误码率:" + status);///////////////////
                index = status.Length;//status.IndexOf('-');
                psspertbuf[i] = Convert.ToDouble(status.Substring(22, index - 22 - 2 - 1).Trim());//10
                if ((psspertbuf[i] == 0) && status.Contains("Y N"))
                {
                    //Thread.Sleep(2000);//
                    status = GetPSSStatus(ch);
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
                PSSSenseClear(ch);

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
            AddTestLog("误码率值:" + psspertbuf[i].ToString());
            //AddTestLog("最终APDval:" + valminindex.ToString());///////////////////
            //valminindex -= 10;//最佳点调整 回退10
            test.SetAPD(valminindex);
            TestResult.rxapdVal = valminindex;
            switch (TestSet.ch)
            {
                case 0: lbAPDValCh0.Text = valminindex.ToString();
                    break;
                case 1: lbAPDValCh1.Text = valminindex.ToString();
                    break;
                case 2: lbAPDValCh2.Text = valminindex.ToString();
                    break;
                case 3: lbAPDValCh3.Text = valminindex.ToString();
                    break;
                default:
                    break;
            }
            // Refresh();
           // if (i == max)
            if (i <= max)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (test.SetDebugPWD() == false)
            {
                AddTestLog("模块PWD操作失败！");
                return;
            }

            //带TEC方案
            if (GlobalVarFun.tx_tec_test)
            {
                Thread.Sleep(1000);//
                if (test.SetTx_EN() != true)
                {
                    AddTestLog("模块Tx使能操作失败！");
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "模块Tx使能失败......";
                    return;
                }
                else
                {
                    AddTestLog("模块Tx使能操作成功！");
                }
                Thread.Sleep(3000);//等待TEC启动
            }
            for (int i = 0; i < 4; i++)
            {
                test.SoftTxCHEn(i);
                TestSet.ch = i;
                if (AutoTestEML_100GLR() == false)
                {
                    AddTestLog("CH" + TestSet.ch.ToString() + "TxTemp调试失败！");
                    Startautoset_button.BackColor = Color.Red;
                    Startautoset_button.Text = "CH" + TestSet.ch.ToString() + "TxTemp调试失败, 请插入下一只模块......";
                    return;
                }
                else
                {
                    Startautoset_button.BackColor = Color.Green;
                    Startautoset_button.Text = "CH" + TestSet.ch.ToString() + "TxTemp调试成功, 请插入下一只模块......";
                }
                lbTempValCh0.Text = TestSet.tosa_temp.ToString();
            }
        }          
        //
        //************************************************************************************************************************//
    }
}

