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
    public partial class Setup_Form : Form
    {
        //AgilentInfiniiumDCA scope;// = GlobalVarFun.scope;
        SqlConnection sqlconnection;// = GlobalVarFun.sqlconnection;      
        ModuleTest test;
        I2C i2c;            
        bool eyeMaskIsOpened = false;   
        //bool testDataIsOK = false;
        UInt16[] rxAdc = new UInt16[6];
        string gpibAddress = "GPIB0::07::INSTR";

        public Setup_Form()
        {      
            InitializeComponent();
        }
      
        private void Setup_Form_Load(object sender, EventArgs e)
        {
            this.i2c = GlobalVarFun.iic;
            this.sqlconnection = GlobalVarFun.sqlconnection;
            this.test = GlobalVarFun.mTest;     
            test.Init(i2c); //必须调用
                    
            if (radioButton_PIN.Checked)
            {
                rxPwrtextBox1.Text =  "-10";
                rxPwrtextBox2.Text = "-18";
                rxPwrtextBox3.Text =  "-25";
                rxPwrtextBox4.Text =  "-26";
                rxPwrtextBox5.Text =  "-30";
                GlobalVarFun.rx_is_apd = false;
            }
            //
            if (radioButton_APD.Checked)
            {
                rxPwrtextBox1.Text =  "-10";
                rxPwrtextBox2.Text =  "-18";
                rxPwrtextBox3.Text =  "-22";
                rxPwrtextBox4.Text =  "-26";
                rxPwrtextBox5.Text =  "-30";
                GlobalVarFun.rx_is_apd = true;
            }

            TestResult.waveforms_count = Convert.ToInt32(waveforms_numericUpDown.Value);

            meterType_comboBox.SelectedIndex = 1; //光功率计类型选择

            //TxRx_CDR 控制选择
            checkBox_DisCDR.Checked = false;
            //checkBox_TOSA_NoMPD.Checked = false;
            if (GlobalVarFun.moduleType == "QSFP")
            {
                checkBox_DisCDR.Enabled = true;
                checkBox_TOSA_NoMPD.Enabled = true;
            }
            else
            {
                checkBox_DisCDR.Enabled = false;
                checkBox_TOSA_NoMPD.Enabled = false;
            }
            //EML激光器波长选择
            switch (TestSet.EMLTestType)
            { 
                case 0:
                    rB40G.Checked = true;
                    break;
                case 1:
                    rBDualFibers.Checked = true;
                    break;
                case 2:
                    rBBiDi23.Checked = true;
                    break;
                case 3:
                    rBBiDi32.Checked = true;
                    break;
                default :
                    break;
            }
            //Tx/Rx测试选择
            if (GlobalVarFun.test_tx_select)
            {
                rBTestTxSelect.Checked = true;
            }
            else
            {
                rBTestRxSelect.Checked = true;
            }
            checkBox_DisTypeCheck.Enabled = true;
            checkBox_DisTypeCheck.Checked = false;
            //调试范围
            txapcMin_numericUpDown.Text = TestSet.txapc_Min_set.ToString();
            txapcMax_numericUpDown.Text = TestSet.txapc_Max_set.ToString();
            txmodMin_numericUpDown.Text = TestSet.txmod_Min_set.ToString();
            txmodMax_numericUpDown.Text = TestSet.txmod_Max_set.ToString();
            rxlosMin_numericUpDown.Text = TestSet.rxlos_Min_set.ToString();
            rxlosMax_numericUpDown.Text = TestSet.rxlos_Max_set.ToString();

            TOSATempMax_numericUpDown.Text = TestSet.tosatemp_max.ToString();
            TOSATempMin_numericUpDown.Text = TestSet.tosatemp_min.ToString();
            VONMax_numericUpDown.Text = TestSet.Tx_von.ToString();
            APDMax_numericUpDown.Text = TestSet.rx_apd.ToString();

            // 根据模块型号 更改主窗口标题
            if (GlobalVarFun.testType == "firstTest")
            {
                this.Text = "*** " + GlobalVarFun.moduleType + "  初测调试软件" + this.Text;
                //txCalNumericUpDown.Value = Convert.ToDecimal(0.5);
                //rxCalNumericUpDown.Value = Convert.ToDecimal(1.0);

               // button1_testType.Text = "初测.调试";
               // textBoxTester.Text = "FirstTest_01";

                if (GlobalVarFun.moduleType == "QSFP")
                {
                   // txpe_checkBox.Enabled = true;
                   // txpe_numericUpDown.Enabled = true;
                }

                //if (GlobalVarFun.moduleType == "SFPP-GN1196" || GlobalVarFun.moduleType == "SFP-GN25L95" || GlobalVarFun.moduleType == "SFP-GN25L96" || GlobalVarFun.moduleType == "SFP-UX3320C" || GlobalVarFun.moduleType == "SFP-UX3320T")
                //{
                //    checkBox_Init.Enabled = true;
                //}

                checkBox_debugTest.Enabled = false;
                checkBox_AlarmThresholds.Enabled = false;

                checkBox_EyeSave.Enabled = false;
                waveforms_numericUpDown.Enabled = false;

                checkBox_txJt.Checked = false;
                checkBox_txJt.Enabled = false;
            }
            else if (GlobalVarFun.testType == "finalTest")
            {
                this.Text = "*** " + GlobalVarFun.moduleType + "  终测检查软件" + this.Text;
                rxlosMin_numericUpDown.Enabled = false;
                rxlosMax_numericUpDown.Enabled = false;
                txapcMin_numericUpDown.Enabled = false;
                txapcMax_numericUpDown.Enabled = false;
                txmodMin_numericUpDown.Enabled = false;
                txmodMax_numericUpDown.Enabled = false;
                erCalNumericUpDown.Enabled = false;

                //checkBox_Init.Enabled = false;

                checkBox_EyeSave.Enabled = true;
                waveforms_numericUpDown.Enabled = true;

                checkBox_txJt.Checked = true;
                checkBox_txJt.Enabled = true;
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

            // 从Access数据库中更新模块型号列表
            string[] strType = new string[300];
            int len = 0;
            moduletype_comboBox.Items.Clear();
            //
            if (test.GetModuleTypeFromAccessdb(ref strType, ref len))
            {
                for (int i = 0; i < len; i++)
                {
                    moduletype_comboBox.Items.Add(strType[i]);
                }
            }

            if (moduletype_comboBox.Items.Count > 0)
            {
                moduletype_comboBox.SelectedIndex = GlobalVarFun.type_index;
            }
            else
            {
                MessageBox.Show("初始化模块型号列表失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            string[] portnames = SerialPort.GetPortNames();
            Array.Sort(portnames); //已存在串口更新
            meterCom_comboBox.Items.Clear();
            attCom_comboBox.Items.Clear();
            SwitchCom_comboBox.Items.Clear();
            List<string> devlist = GetGpibDevices();
            for (int i = 0; i < portnames.Length; i++)
            {
                meterCom_comboBox.Items.Add(portnames[i]);
                attCom_comboBox.Items.Add(portnames[i]);
                PSSCom_comboBox.Items.Add(portnames[i]);
                SwitchCom_comboBox.Items.Add(portnames[i]);
            }
            if (meterCom_comboBox.Items.Count > 0)
            {
                meterCom_comboBox.SelectedIndex = MEMTER.com_index;
            }
            if (attCom_comboBox.Items.Count > 0)
            {
                attCom_comboBox.SelectedIndex = DOA.com_index;
            }
            if (gpibCom_comboBox.Items.Count > 0)
            {
                gpibCom_comboBox.SelectedIndex = 0;
            }
            if (PSSCom_comboBox.Items.Count > 0)
            {
                PSSCom_comboBox.SelectedIndex = BIT_ERROR.com_index;
            }
            if (SwitchCom_comboBox.Items.Count > 0)
            {
                SwitchCom_comboBox.SelectedIndex = opcicalSwitch.com_index;
            }
            for (int i = 0; i < devlist.Count; i++)
            {
                cbBWavelength.Items.Add(devlist[i]);
            }
            statusDisplay();
        }

        private void moduletype_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {         
            GlobalVarFun.type_index = moduletype_comboBox.SelectedIndex;
            TestResult.fibertop_pn = moduletype_comboBox.Text;
           // checkBox_DisCDR.Checked = false; //2020.4.8
            //checkBox_DisTypeCheck.Checked = false; //2021.5.29
            //checkBox_TOSA_NoMPD.Checked = false; //2023.11.7
            
            if (test.GetTypeDebugInfoFromAccessdb() == false)
            {
                GlobalVarFun.access_connect_status = false;
                MessageBox.Show("模块型号列表选取型号失败，未读取到信息！\r\n", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // // 初测调试 收紧指标范围
                if (GlobalVarFun.testType == "firstTest")
                {
                    TestSet.txPwr_Min += 0.2f;
                    TestSet.txPwr_Max -= 0.2f;

                    TestSet.bias_Min *= 1.05f;
                    TestSet.bias_Max /= 1.05f;

                    TestSet.txEr_Min += 0.1f;
                    TestSet.txEr_Max -= 0.1f;

                    TestSet.rx_ALos += 1.0f;  // 告警前移1dB
                    //TestSet.rx_DLos -= 0.5f;  // 去告警后移0.5dB
                }
                //
                txpe_checkBox.Checked = false; //2017.8.21
            }
            //     
            rxPwrtextBox1.Text = TestSet.rxPwr_Real[0].ToString("F1");
            rxPwrtextBox2.Text = TestSet.rxPwr_Real[1].ToString("F1");
            rxPwrtextBox3.Text = TestSet.rxPwr_Real[2].ToString("F1");
            rxPwrtextBox4.Text = TestSet.rxPwr_Real[3].ToString("F1");
            rxPwrtextBox5.Text = TestSet.rxPwr_Real[4].ToString("F1");
            textBox_overLoad.Text = TestSet.rx_OverLoad.ToString("F1");
            textBox_Sen.Text = TestSet.rx_Sen.ToString("F1");
            textBox_DLos.Text = TestSet.rx_DLos.ToString("F1");
            textBox_ALos.Text = TestSet.rx_ALos.ToString("F1");

            if (GlobalVarFun.optoMeter_connected)
            {
                txapcMin_numericUpDown.Text = TestSet.txapc_Min_set.ToString();
                txapcMax_numericUpDown.Text = TestSet.txapc_Max_set.ToString();
                txmodMin_numericUpDown.Text = TestSet.txmod_Min_set.ToString();
                txmodMax_numericUpDown.Text = TestSet.txmod_Max_set.ToString();
                rxlosMin_numericUpDown.Text = TestSet.rxlos_Min_set.ToString();
                rxlosMax_numericUpDown.Text = TestSet.rxlos_Max_set.ToString();

                APDMin_numericUpDown.Text = TestSet.rxapd_min.ToString();
                APDMax_numericUpDown.Text = TestSet.rxapd_max.ToString();
            }
            else
            {
                // APC MOD LOS 调试范围值 //2017.11.30
                txapcMin_numericUpDown.Text = TestSet.txapc_Min.ToString();
                txapcMax_numericUpDown.Text = TestSet.txapc_Max.ToString();
                txmodMin_numericUpDown.Text = TestSet.txmod_Min.ToString();
                txmodMax_numericUpDown.Text = TestSet.txmod_Max.ToString();
                rxlosMin_numericUpDown.Text = TestSet.rxlos_Min.ToString();
                rxlosMax_numericUpDown.Text = TestSet.rxlos_Max.ToString();
                APDMin_numericUpDown.Text = TestSet.rxapd_min.ToString();
                APDMax_numericUpDown.Text = TestSet.rxapd_max.ToString();
            }

            if (TestResult.fibertop_pn.Contains("HG") && (TestResult.fibertop_pn.Contains("LW") || TestResult.fibertop_pn.Contains("CW")) && (!TestResult.fibertop_pn.Contains("BL")))
            {
                rBDualFibers.Checked = true;
            }
            if (TestResult.fibertop_pn.Contains("HG") && (TestResult.fibertop_pn.Contains("BL")))
            {
                rBBiDi23.Checked = true;
            }
            if (TestResult.fibertop_pn.Contains("FG") || (TestResult.fibertop_pn.Contains("MM")))
            {
                rB40G.Checked = true;
            }

        }

        // 自动调试可选项控件状态控制
        private void SetDebugParaCtrlStatus(bool setVal)
        {
            rxCalNumericUpDown.Enabled = setVal;
            connection_button.Enabled = setVal;
            delayNumericUpDown9.Enabled = setVal;
            conntMeter_button.Enabled = setVal;
            conntAtt_button.Enabled = setVal;
            button_overLoadTest.Enabled = setVal;
            button_SenTest.Enabled = setVal;
            button_DLosTest.Enabled = setVal;
            button_ALosTest.Enabled = setVal;

            button_calTest1.Enabled = setVal;
            button_calTest2.Enabled = setVal;
            button_calTest3.Enabled = setVal;

            testDataCheck_button.Enabled = setVal;

            checkBox_rxTest.Enabled = setVal;
            checkBox_RxNoPwr.Enabled = setVal;
            checkBox_txTest.Enabled = setVal;
            checkBox_TxNoPwr.Enabled = setVal;
            cBSenTest.Enabled = setVal;
            //checkBox_txJt.Enabled = setVal;
            //checkBox_EyeSave.Enabled = setVal;

            textBox_Att1.ReadOnly = !setVal;
            textBox_Att2.ReadOnly = !setVal;
            textBox_Att3.ReadOnly = !setVal;

            textBox_overLoadAtt.ReadOnly = !setVal;
            textBox_SenAtt.ReadOnly = !setVal;
            textBox_DLosAtt.ReadOnly = !setVal;
            textBox_ALosAtt.ReadOnly = !setVal;

            //bn_textBox.ReadOnly = !setVal;
            //tosaSn_textBox.ReadOnly = !setVal;
            //rosaSn_textBox.ReadOnly = !setVal;

            //
            delayNumericUpDown8.Enabled = setVal;
            optoErr_numericUpDown.Enabled = setVal;
            moduletype_comboBox.Enabled = setVal;

            radioButton_PIN.Enabled = setVal;
            radioButton_APD.Enabled = setVal;

            txCalNumericUpDown.Enabled = setVal;
            ER_Att_NumericUpDown.Enabled = setVal;

         
           // textBoxTester.ReadOnly = !setVal;

           // cpn_checkBox.Enabled = setVal;
           // cpn_textBox.ReadOnly = !setVal;

            if (radioButton_APD.Checked)
            {
                textBox_Att4.ReadOnly = !setVal;
                textBox_Att5.ReadOnly = !setVal;
                button_calTest4.Enabled = setVal;
                button_calTest5.Enabled = setVal;
            }

            if (GlobalVarFun.moduleType == "QSFP")
            {
                checkBox_DisCDR.Enabled = setVal;
                checkBox_TOSA_NoMPD.Enabled = setVal;
            }
            checkBox_DisTypeCheck.Enabled = setVal;

            if (GlobalVarFun.testType == "firstTest") //初测调试
            {
                if (GlobalVarFun.moduleType == "QSFP")
                {
                    txpe_checkBox.Enabled = setVal;
                    txpe_numericUpDown.Enabled = setVal;
                    cBTosaTemp.Enabled = setVal;
                    cBVon.Enabled = setVal;
                    cBAPD.Enabled = setVal;
                    cBDAC86100D.Enabled = setVal;
                }
                //
                //if (GlobalVarFun.moduleType == "SFPP-GN1196" || GlobalVarFun.moduleType == "SFP-GN25L95" || GlobalVarFun.moduleType == "SFP-GN25L96" || GlobalVarFun.moduleType == "SFP-UX3320C" || GlobalVarFun.moduleType == "SFP-UX3320T")
                //{
                //    checkBox_Init.Enabled = setVal;
                //}
                //
                rxlosMin_numericUpDown.Enabled = setVal;
                rxlosMax_numericUpDown.Enabled = setVal;
                txapcMin_numericUpDown.Enabled = setVal;
                txapcMax_numericUpDown.Enabled = setVal;
                txmodMin_numericUpDown.Enabled = setVal;
                txmodMax_numericUpDown.Enabled = setVal;
                erCalNumericUpDown.Enabled = setVal;
            }
            else // 终测检查
            {
                checkBox_debugTest.Enabled = setVal;
                checkBox_AlarmThresholds.Enabled = setVal;

                checkBox_EyeSave.Enabled = setVal;
                waveforms_numericUpDown.Enabled = setVal;

                checkBox_txJt.Enabled = setVal;
            }

            checkBox_useDCA.Enabled = setVal;

            string[] portnames = SerialPort.GetPortNames();
            Array.Sort(portnames); //已存在串口更新
            List<string> devlist = GetGpibDevices();
            meterCom_comboBox.Items.Clear();
            attCom_comboBox.Items.Clear();
            SwitchCom_comboBox.Items.Clear();
            for (int i = 0; i < portnames.Length; i++)
            {
                meterCom_comboBox.Items.Add(portnames[i]);
                attCom_comboBox.Items.Add(portnames[i]);
                PSSCom_comboBox.Items.Add(portnames[i]);
                SwitchCom_comboBox.Items.Add(portnames[i]);
            }
            if (meterCom_comboBox.Items.Count > 0)
            {
                meterCom_comboBox.SelectedIndex = 0;
            }
            if (attCom_comboBox.Items.Count > 0)
            {
                attCom_comboBox.SelectedIndex = 0;
            }
            if (gpibCom_comboBox.Items.Count > 0)
            {
                gpibCom_comboBox.SelectedIndex = 0;
            }
            if (PSSCom_comboBox.Items.Count > 0)
            {
                PSSCom_comboBox.SelectedIndex = 0;
            }
            if (SwitchCom_comboBox.Items.Count > 0)
            {
                SwitchCom_comboBox.SelectedIndex = 0;
            }
            statusDisplay();

            // 调试参数 范围设置
            ///////////////////////////////////////////////////////////////////
        }

        //连接86100DCA
        private void connection_button_Click(object sender, EventArgs e)
        {
            //更新眼图测试最大累计点
            TestResult.waveforms_count = Convert.ToInt32(GlobalVarFun.waveforms_num);

            eyeMaskIsOpened = false;

            try
            {
                if (GlobalVarFun.instrument_connected == false)
                {
                    if (GlobalVarFun.DCA86100D_Open || GlobalVarFun.N1092x_Open)
                    {
                        GlobalVarFun.scope_86100d.OpenVISA(gpibAddress);
                        GlobalVarFun.scope_86100d.SetClearDisplay(gpibAddress, 10);
                        GlobalVarFun.scope_86100d.SetRun(gpibAddress);
                        //////////////////////////////////////////
                        //float er = GlobalVarFun.scope_86100d.GetExtRatio(gpibAddress);
                        //Thread.Sleep(100);
                        //float crossing = GlobalVarFun.scope_86100d.GetCrossing(gpibAddress);
                        //Thread.Sleep(100);
                        //float jitter = GlobalVarFun.scope_86100d.GetJitterPP(gpibAddress);
                        connection_button.BackColor = System.Drawing.Color.GreenYellow;
                        GlobalVarFun.instrument_connected = true;
                    }
                    else
                    {
                        if (GlobalVarFun.scope.Initialized)
                        {
                            GlobalVarFun.scope.Close();
                        }
                        GlobalVarFun.scope.Initialize("GPIB0::07::INSTR", false, false, "");
                        //scope.System.WaitForOperationComplete(1000); // 等待完成
                        GlobalVarFun.scope.System.IO.WriteString(":CHANnel1:DISPlay ON", true); // Channel 1 On
                        GlobalVarFun.scope.System.IO.WriteString("*CLS", true);
                        //
                        if ((GlobalVarFun.testType == "finalTest") && (TestResult.waveforms_count >= 100))
                        {
                            if ((TestResult.mask_margin > 90) || (TestResult.mask_margin < 5))
                            {
                                MessageBox.Show("眼图模板Margin超出范围(5-90%)！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            if (string.IsNullOrEmpty(TestResult.mask_name))
                            {
                                MessageBox.Show("眼图模板名字为空！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            GlobalVarFun.scope.System.IO.WriteString(":MTESt:ARUN OFF", true); //关闭自动模板测试
                            GlobalVarFun.scope.System.IO.WriteString(":MTESt:LOAD '" + TestResult.mask_name + "'", true); //打开眼图模板
                            GlobalVarFun.scope.System.IO.WriteString(":MTEST:MMARgin:STATe ON", true);
                            GlobalVarFun.scope.System.IO.WriteString(":MTEST:MMARgin:PERCent " + TestResult.mask_margin.ToString(), true);
                            GlobalVarFun.scope.System.IO.WriteString(":MTESt:TEST ON", true);
                            //
                            eyeMaskIsOpened = true;
                        }
                        else
                        {
                            GlobalVarFun.scope.System.IO.WriteString(":MTESt:TEST OFF", true);
                        }
                        //
                        GlobalVarFun.scope.System.TimeoutMilliseconds = 10000; //timeout
                        GlobalVarFun.scope.System.EnableLocalControls();
                        GlobalVarFun.instrument_connected = true;
                        //connection_button.Text = "Get Connect";
                        connection_button.BackColor = System.Drawing.Color.GreenYellow;
                    }
                }
                else
                {
                    if (!GlobalVarFun.DCA86100D_Open && !GlobalVarFun.N1092x_Open)
                    {
                        GlobalVarFun.scope.Close();
                    }
                    GlobalVarFun.instrument_connected = false;
                    //connection_button.Text = "No Connect";
                    connection_button.BackColor = System.Drawing.Color.Gray;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                connection_button.BackColor = System.Drawing.Color.Yellow;
                //System.Windows.Forms.Application.Exit();
            }
        }

        // 连接光功率计
        private void conntMeter_button_Click(object sender, EventArgs e)
        {
            try
            {
                if (GlobalVarFun.optoMeter_connected == false)
                {
                    if (GlobalVarFun.uartMeter != null)
                    {
                        if (GlobalVarFun.uartMeter.IsOpen)
                        {
                            GlobalVarFun.uartMeter.Close();
                        }
                    }
                    //
                    if (meterType_comboBox.SelectedIndex == 0) //手持光功率计 光讯
                    {
                        MEMTER.type_index = 0;
                        GlobalVarFun.uartMeter.PortName = meterCom_comboBox.Text;
                        GlobalVarFun.uartMeter.BaudRate = 9600;
                        GlobalVarFun.uartMeter.ReadTimeout = 1000;
                        GlobalVarFun.uartMeter.Open();
                        byte[] WriteBuffer = new byte[7] { 0xef, 0xef, 0x04, 0x04, 0x60, 0x06, 0x4c };
                        byte[] ReadBuffer = new byte[14];
                        GlobalVarFun.uartMeter.Write(WriteBuffer, 0, 7);
                        Thread.Sleep(100);
                        GlobalVarFun.uartMeter.Read(ReadBuffer, 0, 14);
                        if ((ReadBuffer[0] == 0xed) && (ReadBuffer[1] == 0xfa))
                        {
                            GlobalVarFun.optoMeter_connected = true;
                            meterCom_comboBox.Enabled = false;
                            meterType_comboBox.Enabled = false;
                            //opto_meter_button.Text = "Get Connect";
                            conntMeter_button.BackColor = System.Drawing.Color.GreenYellow;
                            return;
                        }
                        else
                        {
                            GlobalVarFun.uartMeter.Close();
                            GlobalVarFun.optoMeter_connected = false;
                            conntMeter_button.BackColor = System.Drawing.Color.Gray;
                        }
                    }
                    else //if (meterType_comboBox.SelectedIndex == 1) //台式光功率计 普塞斯PSS
                    {
                        MEMTER.type_index = 1;
                        GlobalVarFun.uartMeter.PortName = meterCom_comboBox.Text;
                        GlobalVarFun.uartMeter.BaudRate = 115200;
                        GlobalVarFun.uartMeter.ReadTimeout = 1000;
                        GlobalVarFun.uartMeter.Open();
                        byte[] WriteBuffer = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
                        byte[] ReadBuffer = new byte[40];
                        GlobalVarFun.uartMeter.Write(WriteBuffer, 0, 7);
                        Thread.Sleep(300);
                        GlobalVarFun.uartMeter.Read(ReadBuffer, 0, 36);
                        if ((ReadBuffer[0] == 0x50) && (ReadBuffer[1] == 0x53) && (ReadBuffer[2] == 0x53) &&
                            (ReadBuffer[4] == 0x4F) && (ReadBuffer[5] == 0x50) && (ReadBuffer[6] == 0x4D))
                        {
                            GlobalVarFun.optoMeter_connected = true;
                            meterCom_comboBox.Enabled = false;
                            meterType_comboBox.Enabled = false;
                            //opto_meter_button.Text = "Get Connect";
                            conntMeter_button.BackColor = System.Drawing.Color.GreenYellow;
                            return;
                        }
                        else
                        {
                            GlobalVarFun.uartMeter.Close();
                            GlobalVarFun.optoMeter_connected = false;
                            conntMeter_button.BackColor = System.Drawing.Color.Gray;
                        }

                        
                    }
                }
                else
                {
                    GlobalVarFun.uartMeter.Close();
                    GlobalVarFun.optoMeter_connected = false;
                    meterCom_comboBox.Enabled = true;
                    meterType_comboBox.Enabled = true;
                    //opto_meter_button.Text = "No Connect";
                    conntMeter_button.BackColor = System.Drawing.Color.Gray;
                }
            }
            catch
            {
                GlobalVarFun.optoMeter_connected = false;
                //opto_meter_button.Text = "No Connect";
                conntMeter_button.BackColor = System.Drawing.Color.Yellow;
            }
        }

        // 连接光光衰减器
        private void conntAtt_button_Click(object sender, EventArgs e)
        {
            //2A 49 44 4E 3F 0D 0A
            byte[] WriteBuffer = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
            byte[] ReadBuffer = new byte[40];
            int uart_readLen_rtn;

            try
            {
                if (GlobalVarFun.optoAtt_connected == false)
                {
                    if (GlobalVarFun.uartAtt != null)
                    {
                        if (GlobalVarFun.uartAtt.IsOpen)
                        {
                            GlobalVarFun.uartAtt.Close();
                        }
                    }
                    //
                    GlobalVarFun.uartAtt.PortName = attCom_comboBox.Text;
                    GlobalVarFun.uartAtt.BaudRate = 115200;
                    GlobalVarFun.uartAtt.ReadTimeout = 1000;
                    GlobalVarFun.uartAtt.Open();
                    ReadBuffer[0] = 0xFF;
                    GlobalVarFun.uartAtt.Write(WriteBuffer, 0, 7);
                    Thread.Sleep(100);
                    uart_readLen_rtn = GlobalVarFun.uartAtt.Read(ReadBuffer, 0, 34);
                    string str = System.Text.Encoding.ASCII.GetString(ReadBuffer, 0, 20);
                    if ((ReadBuffer[0] == 0x50) && (ReadBuffer[1] == 0x53) && (ReadBuffer[2] == 0x53) && (uart_readLen_rtn == 34))
                    {
                        if (str.Contains("DOA16012"))
                        {
                            GlobalVarFun.optoAtt_new_connected = true;
                        }
                        else
                        {
                            GlobalVarFun.optoAtt_new_connected = false;
                            //opto_meter_button.Text = "Get Connect";
                        }
                        GlobalVarFun.optoAtt_connected = true;
                        attCom_comboBox.Enabled = false;
                        //opto_meter_button.Text = "Get Connect";
                        conntAtt_button.BackColor = System.Drawing.Color.GreenYellow;
                        return;
                    }
                    else
                    {
                        GlobalVarFun.uartAtt.Close();
                        GlobalVarFun.optoAtt_connected = false;
                        conntAtt_button.BackColor = System.Drawing.Color.Gray;
                    }
                }
                else
                {
                    GlobalVarFun.uartAtt.Close();
                    GlobalVarFun.optoAtt_connected = false;
                    attCom_comboBox.Enabled = true;
                    //opto_meter_button.Text = "No Connect";
                    conntAtt_button.BackColor = System.Drawing.Color.Gray;
                }
            }
            catch
            {
                GlobalVarFun.optoAtt_connected = false;
                //opto_meter_button.Text = "No Connect";
                conntAtt_button.BackColor = System.Drawing.Color.Yellow;
            }
        }

        private void radioButton_PIN_CheckedChanged(object sender, EventArgs e)
        {
            //ddm_rxpower4_textbox.ReadOnly = true;
           // ddm_rxpower5_textbox.ReadOnly = true;

            textBox_Att4.ReadOnly = true;
            textBox_Att5.ReadOnly = true;

            button_calTest4.Enabled = false;
            button_calTest5.Enabled = false;

            //
            Refresh();
        }

        private void radioButton_APD_CheckedChanged(object sender, EventArgs e)
        {
            //ddm_rxpower4_textbox.ReadOnly = false;
            //ddm_rxpower5_textbox.ReadOnly = false;

            textBox_Att4.ReadOnly = false;
            textBox_Att5.ReadOnly = false;

            button_calTest4.Enabled = true;
            button_calTest5.Enabled = true;

            //
            Refresh();
        }
          
        private void checkBox_Init_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBox_Init.Checked)
            //{
            //    if (GlobalVarFun.moduleType == "SFPP-GN1196" || GlobalVarFun.moduleType == "SFP-GN25L95" || GlobalVarFun.moduleType == "SFP-GN25L96" || GlobalVarFun.moduleType == "SFP-UX3320C" || GlobalVarFun.moduleType == "SFP-UX3320T")
            //    {
            //        // 无操作
            //    }
            //    else
            //    {
            //        checkBox_Init.Checked = false;
            //        checkBox_Init.Enabled = false;
            //        MessageBox.Show(GlobalVarFun.moduleType + "方案不支持初始化功能！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}
        }

        private void conntPSS_button_Click(object sender, EventArgs e)
        {
            //SerialPort pssert;
            string command = string.Empty;
            string readstr = string.Empty;
            //pssert = new SerialPort();
            try
            {
                if (GlobalVarFun.pssbert_connected == false)
                {
                    if (GlobalVarFun.pssert != null)
                    {
                        if (GlobalVarFun.pssert.IsOpen)
                        {
                            GlobalVarFun.pssert.Close();
                        }
                    }
                    GlobalVarFun.pssert.PortName = PSSCom_comboBox.Text;
                    GlobalVarFun.pssert.BaudRate = 115200;
                    GlobalVarFun.pssert.ReadTimeout = 1000;

                    GlobalVarFun.pssert.Open();
                    command = "*IDN?";

                    GlobalVarFun.pssert.WriteLine(command);
                    Thread.Sleep(20);
                    readstr = GlobalVarFun.pssert.ReadLine();
                    if (readstr.Substring(0, 8) == "PSS,BERT")
                    {
                        conntPSS_button.BackColor = System.Drawing.Color.Yellow;
                        PSSCom_comboBox.Enabled = false;
                        //PSSch_comboBox.Enabled = false;
                        GlobalVarFun.pssbert_connected = true;
                        return;
                    }
                    else
                    {
                        GlobalVarFun.pssert.Close();
                        GlobalVarFun.pssbert_connected = false;
                        PSSCom_comboBox.Enabled = true;
                        //PSSch_comboBox.Enabled = true;
                        conntPSS_button.BackColor = System.Drawing.Color.Gray;
                        // PSSCom_comboBox.Enabled = false;
                    }
                }
                else
                {
                    GlobalVarFun.pssert.Close();
                    GlobalVarFun.pssbert_connected = false;
                    PSSCom_comboBox.Enabled = true;
                    //PSSch_comboBox.Enabled = true;
                    conntPSS_button.BackColor = System.Drawing.Color.Gray;
                }
                //  
            }
            catch
            {
                GlobalVarFun.pssert.Close();
                conntPSS_button.BackColor = System.Drawing.Color.Yellow;
            }
            finally
            {
                //
            }
        }

        private void testDataCheck_button_Click(object sender, EventArgs e)
        {
            float err = 0;
            float range = 0.2f;
            int channel = 0;
            testDataCheck_button.BackColor = System.Drawing.Color.Gray;

            if ((GlobalVarFun.optoMeter_connected == false) || (GlobalVarFun.optoAtt_connected == false)) // 连接光功率计和光衰减器判断
            {
                MessageBox.Show("请先连接光功率计和光衰减器！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //real_rxpower1_textbox.Text = rxPwrtextBox1.Text;
            //real_rxpower2_textbox.Text = rxPwrtextBox2.Text;
            //real_rxpower3_textbox.Text = rxPwrtextBox3.Text;
            //real_rxpower4_textbox.Text = rxPwrtextBox4.Text;
            //real_rxpower5_textbox.Text = rxPwrtextBox5.Text;
            //ddm_rxpower1_textbox.Text = "";
            //ddm_rxpower2_textbox.Text = "";
            //ddm_rxpower3_textbox.Text = "";
            //ddm_rxpower4_textbox.Text = "";
            //ddm_rxpower5_textbox.Text = "";

            //real_rxpower6_textbox.Text = "-40";

            Refresh();

            TestResult.rxPwrReal[0] = Convert.ToSingle(rxPwrtextBox1.Text);
            TestResult.rxPwrReal[1] = Convert.ToSingle(rxPwrtextBox2.Text);
            TestResult.rxPwrReal[2] = Convert.ToSingle(rxPwrtextBox3.Text);
            TestResult.rxPwrReal[3] = Convert.ToSingle(rxPwrtextBox4.Text);
            TestResult.rxPwrReal[4] = Convert.ToSingle(rxPwrtextBox5.Text);

            TestResult.rxSen = Convert.ToSingle(textBox_Sen.Text);
            TestResult.rxDLos = Convert.ToSingle(textBox_DLos.Text);
            TestResult.rxALos = Convert.ToSingle(textBox_ALos.Text);
            TestResult.rxOverLoad = Convert.ToSingle(textBox_overLoad.Text);

            //
            if (moduletype_comboBox.Text.Contains("MM"))     
            {
                DOA.rxCalAtt[0] = DOA.rxCheckAtt[0];
                DOA.rxCalAtt[1] = DOA.rxCheckAtt[1];
                DOA.rxCalAtt[2] = DOA.rxCheckAtt[2];
                DOA.rxCalAtt[3] = DOA.rxCheckAtt[3];
                DOA.rxCalAtt[4] = DOA.rxCheckAtt[4];

                DOA.rxSenAttBuf[1] = DOA.rxSenAttBuf[0];
                DOA.rxSenAttBuf[2] = DOA.rxSenAttBuf[0];
                DOA.rxSenAttBuf[3] = DOA.rxSenAttBuf[0];
                channel = 0;
            }
            else
            {

                if (cBMoudleCH.Text == "ch0")
                {
                    DOA.rxCalAtt[0] = DOA.rxCheckAtt[0];
                    DOA.rxCalAtt[1] = DOA.rxCheckAtt[1];
                    DOA.rxCalAtt[2] = DOA.rxCheckAtt[2];
                    DOA.rxCalAtt[3] = DOA.rxCheckAtt[3];
                    DOA.rxCalAtt[4] = DOA.rxCheckAtt[4];                 
                    channel = 0;

                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(channel + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(channel) == false)//开启光源通道
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                    }
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    DOA.rxCalAtt[5] = DOA.rxCheckAtt[5];
                    DOA.rxCalAtt[6] = DOA.rxCheckAtt[6];
                    DOA.rxCalAtt[7] = DOA.rxCheckAtt[7];
                    DOA.rxCalAtt[8] = DOA.rxCheckAtt[8];
                    DOA.rxCalAtt[9] = DOA.rxCheckAtt[9];
                    channel = 1;

                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(channel + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(channel) == false)//开启光源通道
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                    }
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    DOA.rxCalAtt[10] = DOA.rxCheckAtt[10];
                    DOA.rxCalAtt[11] = DOA.rxCheckAtt[11];
                    DOA.rxCalAtt[12] = DOA.rxCheckAtt[12];
                    DOA.rxCalAtt[13] = DOA.rxCheckAtt[13];
                    DOA.rxCalAtt[14] = DOA.rxCheckAtt[14];
                    channel = 2;

                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(channel + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(channel) == false)//开启光源通道
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                    }
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    DOA.rxCalAtt[15] = DOA.rxCheckAtt[15];
                    DOA.rxCalAtt[16] = DOA.rxCheckAtt[16];
                    DOA.rxCalAtt[17] = DOA.rxCheckAtt[17];
                    DOA.rxCalAtt[18] = DOA.rxCheckAtt[18];
                    DOA.rxCalAtt[19] = DOA.rxCheckAtt[19];
                    channel = 3;
                   
                    if (GlobalVarFun.optoSwitch_connected)
                    {
                        opticalSwitchSet(channel + 1);//光开关切换通道
                    }
                    else
                    {
                        if (test.SourceSoftEn(channel) == false)//开启光源通道
                        {
                            GlobalVarFun.usb_can_use = false;
                        }
                    }
                }
            }
            GlobalVarFun.testDataIsOK = true;

            // RX SEN
            SetDOA_RxAttVal(DOA.rxSenAttBuf[channel]);           
            err = TestResult.rxSen - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // RX DLOS
            SetDOA_RxAttVal(DOA.rxDLosAttBuf[channel]);
            err = TestResult.rxDLos - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // RX ALOS
            SetDOA_RxAttVal(DOA.rxALosAttBuf[channel]);
            err = TestResult.rxALos - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // RX OVERLOAD
            SetDOA_RxAttVal(DOA.rxOverLoadAttBuf[channel]);
            err = TestResult.rxOverLoad - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // CHECK 
            SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 0]);
            TestSet.rxPwr_Cal[0] = Get_OptoPower_Meter();
            err = TestResult.rxPwrReal[0] - TestSet.rxPwr_Cal[0];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // CHECK 
            SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 1]);
            TestSet.rxPwr_Cal[1] = Get_OptoPower_Meter();
            err = TestResult.rxPwrReal[1] - TestSet.rxPwr_Cal[1];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // CHECK 
            SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 2]);
            TestSet.rxPwr_Cal[2] = Get_OptoPower_Meter();
            err = TestResult.rxPwrReal[2] - TestSet.rxPwr_Cal[2];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            if (radioButton_APD.Checked)
            {
                // CHECK 
                SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 3]);
                TestSet.rxPwr_Cal[3] = Get_OptoPower_Meter();
                err = TestResult.rxPwrReal[3] - TestSet.rxPwr_Cal[3];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK = false;
                }

                // CHECK 
                SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 4]);
                TestSet.rxPwr_Cal[4] = Get_OptoPower_Meter();
                err = TestResult.rxPwrReal[4] - TestSet.rxPwr_Cal[4];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK = false;
                }
            }
            //

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            //if (radioButton_PIN.Checked)
            //{
            //    if (DOA.rxCalAtt[2] > 2)
            //    {
            //        DOA.rxCalAtt[1] = DOA.rxCalAtt[2] - 1;
            //        SetDOA_RxAttVal(DOA.rxCalAtt[5 * channel + 1]);
            //        TestSet.rxPwr_Cal[1] = Get_OptoPower_Meter();
            //    }
            //    else
            //    {
            //        GlobalVarFun.testDataIsOK = false;
            //    }
            //}         

            if (GlobalVarFun.testDataIsOK == true)
            {
                testDataCheck_button.BackColor = System.Drawing.Color.GreenYellow;
            }
            else
            {
                testDataCheck_button.BackColor = System.Drawing.Color.Yellow;
                MessageBox.Show("测试参数设置异常，精度为 +-0.2dB ！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        // 计算RX CAL参数
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
            if (radioButton_APD.Checked)
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

            TestResult.rxPwrCal_c[0] = (float)a[0];
            TestResult.rxPwrCal_c[1] = (float)a[1];
            TestResult.rxPwrCal_c[2] = (float)a[2];
            TestResult.rxPwrCal_c[3] = (float)a[3];
            TestResult.rxPwrCal_c[4] = (float)a[4];

            if ((Math.Abs(a[0]) > 1000) || (Math.Abs(a[1]) > 1000))
            {
                return false;
            }

            /////////////////////////////////////////////////////////////
            //2020.10.27 //2022.5.19
            if (GlobalVarFun.moduleType == "SFP-UX3320C" || GlobalVarFun.moduleType == "SFP-UX3320T")
            {
                if (radioButton_APD.Checked) //暂不支持APD
                {
                    return false;
                }

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

                //
                y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                y[1] = Math.Pow(10, rxcaldbm2) * 10000;
                x[0] = rxAdc[0];
                x[1] = rxAdc[1];
                Bit.iapcir(x, y, 2, a, 2, dt);
                TestResult.rxPwrCal_b[0] = (float)a[0];
                TestResult.rxPwrCal_k[0] = (float)a[1];
                //

                //
                y[0] = Math.Pow(10, rxcaldbm2) * 10000;
                y[1] = Math.Pow(10, rxcaldbm3) * 10000;
                x[0] = rxAdc[1];
                x[1] = rxAdc[2];
                Bit.iapcir(x, y, 2, a, 2, dt);
                TestResult.rxPwrCal_b[1] = (float)a[0];
                TestResult.rxPwrCal_k[1] = (float)a[1];
                //

                //
                y[0] = Math.Pow(10, rxcaldbm3) * 10000;
                y[1] = Math.Pow(10, -40.0 / 10) * 10000;
                //
                x[0] = rxAdc[2];
                x[1] = rxAdc[5];
                //
                Bit.iapcir(x, y, 2, a, 2, dt);
                TestResult.rxPwrCal_b[2] = (float)a[0];
                TestResult.rxPwrCal_k[2] = (float)a[1];
                //
            }
            /////////////////////////////////////////////////////////////

            return true;
        }

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

                try
                {
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
                }
                catch
                {
                    return (char)0x03; // 操作失败
                }
                return (char)0x00; // 操作成功
            }
        }
       
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


        /// <summary>
        /// 光开关通道选择
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        private bool opticalSwitchSet(int ch)
        {
            string chnum;
            string command = "Configure:WorkChannel " + ch.ToString();
            if (GlobalVarFun.optoSwitch_connected == false)
            {

            }
            GlobalVarFun.opticalSwitch.WriteLine(command);
            chnum = GlobalVarFun.opticalSwitch.ReadLine();
            if (chnum.Contains(ch.ToString()))
            {
                return true;
            }
            return false;
        }  
        /// <summary>
        /// 接收测试选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox_rxTest_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_rxTest.Checked)
            {
                GlobalVarFun.rx_ddm_test = true;
            }
            else
            {
                GlobalVarFun.rx_ddm_test = false;
            }
        }
       /// <summary>
       /// 连接误码仪
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void conntPSS_button_Click_1(object sender, EventArgs e)
        {
            //SerialPort pssert;
            string command = string.Empty;
            string readstr = string.Empty;
            //pssert = new SerialPort();
            try
            {
                if (GlobalVarFun.pssbert_connected == false)
                {
                    if (GlobalVarFun.pssert != null)
                    {
                        if (GlobalVarFun.pssert.IsOpen)
                        {
                            GlobalVarFun.pssert.Close();
                        }
                    }
                    GlobalVarFun.pssert.PortName = PSSCom_comboBox.Text;
                    GlobalVarFun.pssert.BaudRate = 115200;
                    GlobalVarFun.pssert.ReadTimeout = 1000;

                    GlobalVarFun.pssert.Open();
                    command = "*IDN?";

                    GlobalVarFun.pssert.WriteLine(command);
                    Thread.Sleep(20);
                    readstr = GlobalVarFun.pssert.ReadLine();
                    if (readstr.Substring(0, 8) == "PSS,BERT")
                    {
                        conntPSS_button.BackColor = System.Drawing.Color.GreenYellow;
                        PSSCom_comboBox.Enabled = false;
                        //PSSch_comboBox.Enabled = false;
                        GlobalVarFun.pssbert_connected = true;                       
                       // return;
                    }
                    else
                    {
                        GlobalVarFun.pssert.Close();
                        GlobalVarFun.pssbert_connected = false;
                        PSSCom_comboBox.Enabled = true;
                       // PSSch_comboBox.Enabled = true;
                        conntPSS_button.BackColor = System.Drawing.Color.Gray;
                        // PSSCom_comboBox.Enabled = false;
                    }
                }
                else
                {
                    GlobalVarFun.pssert.Close();
                    GlobalVarFun.pssbert_connected = false;
                    PSSCom_comboBox.Enabled = true;
                   // PSSch_comboBox.Enabled = true;
                    conntPSS_button.BackColor = System.Drawing.Color.Gray;
                }
                //  
            }
            catch
            {
                GlobalVarFun.pssert.Close();
                PSSCom_comboBox.Enabled = true;
                //PSSch_comboBox.Enabled = true;
                conntPSS_button.BackColor = System.Drawing.Color.Yellow;            
            }
            finally
            {
                //
            }
        }
     
        /// <summary>
        /// 光开关connect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpticalSwitch_Click(object sender, EventArgs e)
        {
            byte[] WriteBuffer = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
            byte[] ReadBuffer = new byte[40];
           // int uart_readLen_rtn;
            string command = "*IDN?";
            string[] arry = command.Split(' ');
            byte[] b = new byte[arry.Length];
            string str;
            try
            {
                if (GlobalVarFun.optoSwitch_connected == false)
                {
                    if (GlobalVarFun.opticalSwitch != null)
                    {
                        if (GlobalVarFun.opticalSwitch.IsOpen)
                        {
                            GlobalVarFun.opticalSwitch.Close();
                        }
                    }
                    //
                    GlobalVarFun.opticalSwitch = new SerialPort();
                    GlobalVarFun.opticalSwitch.PortName = SwitchCom_comboBox.Text;
                    GlobalVarFun.opticalSwitch.BaudRate = 115200;
                    GlobalVarFun.opticalSwitch.ReadTimeout = 1000;
                    GlobalVarFun.opticalSwitch.Open();
                    ReadBuffer[0] = 0xFF;
                    //uartAtt.Write(WriteBuffer, 0, 7);                  
                    GlobalVarFun.opticalSwitch.WriteLine(command);
                    Thread.Sleep(1000);
                    str = GlobalVarFun.opticalSwitch.ReadLine();
                    //Thread.Sleep(100);
                   // uart_readLen_rtn = uartAtt.Read(ReadBuffer, 0, 34);

                    if (str.Contains("PSS"))
                    {
                        GlobalVarFun.optoSwitch_connected = true;
                        SwitchCom_comboBox.Enabled = false;
                        //opto_meter_button.Text = "Get Connect";
                        btnOpticalSwitch.BackColor = System.Drawing.Color.GreenYellow;
                        return;
                    }
                    else
                    {
                        GlobalVarFun.opticalSwitch.Close();
                        GlobalVarFun.optoSwitch_connected = false;
                        btnOpticalSwitch.BackColor = System.Drawing.Color.Gray;
                    }
                }
                else
                {
                    GlobalVarFun.opticalSwitch.Close();
                    GlobalVarFun.optoSwitch_connected = false;
                    SwitchCom_comboBox.Enabled = true;
                    //opto_meter_button.Text = "No Connect";
                    btnOpticalSwitch.BackColor = System.Drawing.Color.Gray;
                }
            }
            catch
            {
                GlobalVarFun.optoSwitch_connected = false;
                //opto_meter_button.Text = "No Connect";
                btnOpticalSwitch.BackColor = System.Drawing.Color.Yellow;
            }
        }

        private void moduletype_comboBox_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            //GlobalVarFun.moduleType = moduletype_comboBox.Text;
            GlobalVarFun.type_index = moduletype_comboBox.SelectedIndex;
            TestResult.fibertop_pn = moduletype_comboBox.Text;

        }

        private void rxCalNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.rx_cal_num = (double)rxCalNumericUpDown.Value;
        }

        private void rxlosMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            //GlobalVarFun.los_min = rxlosMin_numericUpDown.ToString();
            TestSet.rxlos_Min_set = (ushort)rxlosMin_numericUpDown.Value;
        }

        private void rxlosMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            //GlobalVarFun.los_max = rxlosMax_numericUpDown.ToString();
            TestSet.rxlos_Max_set = (ushort)rxlosMax_numericUpDown.Value;
        }
        /// <summary>
        /// check1衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_calTest1_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))     
            {
                if (GlobalVarFun.USBtoI2C == null)
                {
                    MessageBox.Show("未选择USB连接");
                    return;
                }
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                   // test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxCheckAtt[ch * 5 + 0] = Convert.ToSingle(textBox_Att1.Text);
            textBox_Att1.Text = DOA.rxCheckAtt[ch * 5 + 0].ToString("F1");
            DOA.rxCheckAtt[ch * 5 + 0] = Convert.ToSingle(textBox_Att1.Text);
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 0]);
            GlobalVarFun.testDataIsOK = false;
        }
        /// <summary>
        /// check2衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_calTest2_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))        
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    //test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxCheckAtt[ch * 5 + 1] = Convert.ToSingle(textBox_Att2.Text);
            textBox_Att2.Text = DOA.rxCheckAtt[ch * 5 + 1].ToString("F1");
            DOA.rxCheckAtt[ch * 5 + 1] = Convert.ToSingle(textBox_Att2.Text);
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 1]);
            GlobalVarFun.testDataIsOK = false;
        }
        /// <summary>
        /// check3衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_calTest3_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))           
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                   // test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxCheckAtt[ch * 5 + 2] = Convert.ToSingle(textBox_Att3.Text);
            textBox_Att3.Text = DOA.rxCheckAtt[ch * 5 + 2].ToString("F1");
            DOA.rxCheckAtt[ch * 5 + 2] = Convert.ToSingle(textBox_Att3.Text);
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 2]);
            GlobalVarFun.testDataIsOK = false;
        }
        /// <summary>
        /// check4衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_calTest4_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    //test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxCheckAtt[ch * 5 + 3] = Convert.ToSingle(textBox_Att4.Text);
            textBox_Att4.Text = DOA.rxCheckAtt[ch * 5 + 3].ToString("F1");
            DOA.rxCheckAtt[ch * 5 + 3] = Convert.ToSingle(textBox_Att4.Text);
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 3]);
            GlobalVarFun.testDataIsOK = false;
        }
        /// <summary>
        /// check5衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_calTest5_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    //test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxCheckAtt[ch * 5 + 4] = Convert.ToSingle(textBox_Att5.Text);
            textBox_Att5.Text = DOA.rxCheckAtt[ch * 5 + 4].ToString("F1");
            DOA.rxCheckAtt[ch * 5 + 4] = Convert.ToSingle(textBox_Att5.Text);
            SetDOA_RxAttVal(DOA.rxCheckAtt[ch * 5 + 4]);
            GlobalVarFun.testDataIsOK = false;
        }
        /// <summary>
        /// overLoad衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_overLoadTest_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                   // test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxOverLoadAttBuf[ch] = Convert.ToSingle(textBox_overLoadAtt.Text);
            textBox_overLoadAtt.Text = DOA.rxOverLoadAttBuf[ch].ToString("F1");
            DOA.rxOverLoadAttBuf[ch] = Convert.ToSingle(textBox_overLoadAtt.Text);
            SetDOA_RxAttVal(DOA.rxOverLoadAttBuf[ch]);
            GlobalVarFun.testDataIsOK = false;  
        }
        /// <summary>
        /// Sen衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_SenTest_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                   // test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }

            DOA.rxSenAttBuf[ch] = Convert.ToSingle(textBox_SenAtt.Text);          
            textBox_SenAtt.Text = DOA.rxSenAttBuf[ch].ToString("F1");
            DOA.rxSenAttBuf[ch] = Convert.ToSingle(textBox_SenAtt.Text);
            SetDOA_RxAttVal(DOA.rxSenAttBuf[ch]);
            GlobalVarFun.testDataIsOK = false;
        }
        /// <summary>
        /// DLos衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_DLosTest_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    //test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxDLosAttBuf[ch] = Convert.ToSingle(textBox_DLosAtt.Text);
            textBox_DLosAtt.Text = DOA.rxDLosAttBuf[ch].ToString("F1");
            DOA.rxDLosAttBuf[ch] = Convert.ToSingle(textBox_DLosAtt.Text);
            SetDOA_RxAttVal(DOA.rxDLosAttBuf[ch]);
            GlobalVarFun.testDataIsOK = false;
        }
        /// <summary>
        /// ALos衰减值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ALosTest_Click_1(object sender, EventArgs e)
        {
            int ch = 0;

            if (!moduletype_comboBox.Text.Contains("MM"))
            {
                if (cBMoudleCH.Text == "ch0")
                {
                    ch = 0;
                }
                else if (cBMoudleCH.Text == "ch1")
                {
                    ch = 1;
                }
                else if (cBMoudleCH.Text == "ch2")
                {
                    ch = 2;
                }
                else if (cBMoudleCH.Text == "ch3")
                {
                    ch = 3;
                }
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    //test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            DOA.rxALosAttBuf[ch] = Convert.ToSingle(textBox_ALosAtt.Text);
            textBox_ALosAtt.Text = DOA.rxALosAttBuf[ch].ToString("F1");
            DOA.rxALosAttBuf[ch] = Convert.ToSingle(textBox_ALosAtt.Text);
            SetDOA_RxAttVal(DOA.rxALosAttBuf[ch]);
            GlobalVarFun.testDataIsOK = false;
        }
        //PIN 3点校准
        private void radioButton_PIN_CheckedChanged_1(object sender, EventArgs e)
        {
            GlobalVarFun.rx_is_apd = false;
            button_calTest4.Enabled = false;
            button_calTest5.Enabled = false;
            textBox_Att4.ReadOnly = true;
            textBox_Att5.ReadOnly = true;
        }
        //APD 5点校准
        private void radioButton_APD_CheckedChanged_1(object sender, EventArgs e)
        {
            GlobalVarFun.rx_is_apd = true;
            button_calTest4.Enabled = true;
            button_calTest5.Enabled = true;
            textBox_Att4.ReadOnly = false;
            textBox_Att5.ReadOnly = false;
        }

        private void optoErr_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            //GlobalVarFun.opto_att_offset = (int)optoErr_numericUpDown.Value;
            GlobalVarFun.opto_att_offsetbuf[0] = (double)optoErr_numericUpDown.Value;//
        }

        private void optoErrch1_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.opto_att_offsetbuf[1] = (double)optoErrch1_numericUpDown.Value;//
        }

        private void optoErrch2_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.opto_att_offsetbuf[2] = (double)optoErrch2_numericUpDown.Value;//
        }

        private void optoErrch3_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.opto_att_offsetbuf[3] = (double)optoErrch3_numericUpDown.Value;//
        }

        private void delayNumericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            DOA.delay = (int)delayNumericUpDown9.Value;

        }

        private void delayNumericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            MEMTER.delay = (int)delayNumericUpDown8.Value;
        }

        private void Setup_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            //if (sqlconnection != null)
            //    sqlconnection.Close();

            ///*if (accessdbconnect != null)
            //    accessdbconnect.Close();*/

            //if (scope != null && GlobalVarFun.instrument_connected == true)
            //    scope.Close();

            //if (i2c != null)
            //    i2c.TWI_Close();

            //if (uartAtt != null)
            //    if (uartAtt.IsOpen)
            //        uartAtt.Close();

            //if (uartMeter != null)
            //    if (uartMeter.IsOpen)
            //        uartMeter.Close();

            //if (GlobalVarFun.opticalSwitch != null)
            //    if (opticalSwitch.IsOpen)
            //        opticalSwitch.Close();
        }

        private void statusDisplay()
        {
            int i = 0;
            if (cBMoudleCH.Text == "ch0")
            {
                i = 0;
            }
            if (cBMoudleCH.Text == "ch1")
            {
                i = 1;
            }
            if (cBMoudleCH.Text == "ch2")
            {
                i = 2;
            }
            if (cBMoudleCH.Text == "ch3")
            {
                i = 3;
            }
            //测试设备
            if (GlobalVarFun.optoMeter_connected)
            {
                meterCom_comboBox.Enabled = false;
                meterType_comboBox.Enabled = false;            
                conntMeter_button.BackColor = System.Drawing.Color.GreenYellow;
            }
            if (GlobalVarFun.optoAtt_connected)
            {
                attCom_comboBox.Enabled = false;         
                conntAtt_button.BackColor = System.Drawing.Color.GreenYellow;
            }
            if (GlobalVarFun.pssbert_connected)
            {
                PSSCom_comboBox.Enabled = false;
                conntPSS_button.BackColor = System.Drawing.Color.GreenYellow;
            }
            if (GlobalVarFun.optoSwitch_connected)
            {
                SwitchCom_comboBox.Enabled = false;       
                btnOpticalSwitch.BackColor = System.Drawing.Color.GreenYellow;
            }
            if (GlobalVarFun.instrument_connected)
            {
                gpibCom_comboBox.Enabled = false;
                connection_button.BackColor = System.Drawing.Color.GreenYellow;
            }
            if (GlobalVarFun.wlength_connected)
            {
                cbBWavelength.Enabled = false;
                btnWaveLenth.BackColor = System.Drawing.Color.GreenYellow;
            }
            //测试选项
            if (GlobalVarFun.tx_test)
            {
                checkBox_txTest.Checked = true;
            }
            else
            {
                checkBox_txTest.Checked = false;
            }
            //
            if (GlobalVarFun.tx_nopower_test)
            {
                checkBox_TxNoPwr.Checked = true;
            }
            else
            {
                checkBox_TxNoPwr.Checked = false;
            }
            //
            if (GlobalVarFun.rx_ddm_test)
            {
                checkBox_rxTest.Checked = true;
            }
            else
            {
                checkBox_rxTest.Checked = false;
            }
            //
            if (GlobalVarFun.rx_los_test)
            {
                checkBox_LosTest.Checked = true;
            }
            else
            {
                checkBox_LosTest.Checked = false;
            }
            //
            if (GlobalVarFun.rx_nopower_test)
            {
                checkBox_RxNoPwr.Checked = true;
            }
            else
            {
                checkBox_RxNoPwr.Checked = false;
            }
            //
            if (GlobalVarFun.distype_check)
            {
                checkBox_DisTypeCheck.Checked = true;
            }
            else
            {
                checkBox_DisTypeCheck.Checked = false;
            }
            //
            if (GlobalVarFun.txrx_cdr_dis)
            {
                checkBox_DisCDR.Checked = true;
            }
            else
            {
                checkBox_DisCDR.Checked = false;
            }
            //
            if (GlobalVarFun.flash_check)
            {
                checkBox_debugTest.Checked = true;
            }
            else
            {
                checkBox_debugTest.Checked = false;
            }
            //
            if (GlobalVarFun.threshold_check)
            {
                checkBox_AlarmThresholds.Checked = true;
            }
            else
            {
                checkBox_AlarmThresholds.Checked = false;
            }
            //
            if (GlobalVarFun.tx_eye_save_test)
            {
                checkBox_EyeSave.Checked = true;
            }
            else
            {
                checkBox_EyeSave.Checked = false;
            }
            //
            if (GlobalVarFun.hw_los_test)
            {
                cBHardwareLOS.Checked = true;
            }
            else
            {
                cBHardwareLOS.Checked = false;
            }
            //
            if (GlobalVarFun.hw_txdis_test)
            {
                cBHardwareTxDis.Checked = true;
            }
            else
            {
                cBHardwareTxDis.Checked = false;
            }
            //
            if (GlobalVarFun.tx_nopower_test)
            {
                checkBox_TxNoPwr.Checked = true;
            }
            else
            {
                checkBox_TxNoPwr.Checked = false;
            }
            //
            if (GlobalVarFun.sen_test)
            {
                cBSenTest.Checked = true;
            }
            else
            {
                cBSenTest.Checked = false;
            }
            //
            if (GlobalVarFun.tx_jitter_test)
            {
                checkBox_txJt.Checked = true;
            }
            else
            {
                checkBox_txJt.Checked = false;
            }
            //
            if (GlobalVarFun.cob_ld)
            {
                checkBox_TOSA_NoMPD.Checked = true;
            }
            else
            {
                checkBox_TOSA_NoMPD.Checked = false;
            }
            //
            if (GlobalVarFun.power_use_DAC)
            {
                checkBox_useDCA.Checked = true;
            }
            else
            {
                checkBox_useDCA.Checked = false;
            }
            if (GlobalVarFun.rx_is_apd)
            {
                radioButton_APD.Checked = true;
            }
            else
            {
                radioButton_PIN.Checked = true;
            }
            //带TEC方案
            if (GlobalVarFun.tx_tec_test == true)
            {
                cBTEC.Checked = true;
            }
            else
            {
                cBTEC.Checked = false;
            }
            //
            if (GlobalVarFun.TOSATempEN == true)
            {
                cBTosaTemp.Checked = true;
            }
            else
            {
                cBTosaTemp.Checked = false;
            }
            //
            if (GlobalVarFun.VONEN == true)
            {
                cBVon.Checked = true;    
            }
            else
            {
                cBVon.Checked = false;    
            }
            //
            if (GlobalVarFun.APDen == true)
            {
                cBAPD.Checked = true;
            }
            else
            {
                cBAPD.Checked = false;
            }
            if (GlobalVarFun.DCA86100D_Open == true)
            {
                cBDAC86100D.Checked = true;
            }
            else
            {
                cBDAC86100D.Checked = false;
            }
            if (GlobalVarFun.N1092x_Open)
            {
                cBDCANl092X.Checked = true;
            }
            else
            {
                cBDCANl092X.Checked = false;
            }
            //if (GlobalVarFun.moduleType == "QSFP")
            //{
            //    //GlobalVarFun.ER_cal_num = 0.4;
            //}
            //else //未用模块类型
            //{
            //    GlobalVarFun.ER_cal_num = 0.3;
            //}
            if (GlobalVarFun.wlength_connected)
            {
                btnWaveLenth.BackColor = Color.GreenYellow;
                cbBWavelength.Enabled = false;
                cbBWavelength.Text = GlobalVarFun.gpibname;
            }
            if (GlobalVarFun.test_tx_select)
            {
                rBTestTxSelect.Checked = true;
            }
            else
            {
                rBTestRxSelect.Checked = true;
            }

            textBox_Att1.Text = DOA.rxCheckAtt[0].ToString("F1");
            textBox_Att2.Text = DOA.rxCheckAtt[1].ToString("F1");
            textBox_Att3.Text = DOA.rxCheckAtt[2].ToString("F1");
            textBox_Att4.Text = DOA.rxCheckAtt[3].ToString("F1");
            textBox_Att5.Text = DOA.rxCheckAtt[4].ToString("F1");
            //
            textBox_overLoadAtt.Text = DOA.rxOverLoadAttBuf[i].ToString("F1");
            textBox_SenAtt.Text = DOA.rxSenAttBuf[i].ToString("F1");
            textBox_DLosAtt.Text = DOA.rxDLosAttBuf[i].ToString("F1");
            textBox_ALosAtt.Text = DOA.rxALosAttBuf[i].ToString("F1");
            //
            //optoErr_numericUpDown.Value = (decimal)GlobalVarFun.opto_att_offset;
            optoErr_numericUpDown.Value = (decimal)GlobalVarFun.opto_att_offsetbuf[0];
            optoErrch1_numericUpDown.Value = (decimal)GlobalVarFun.opto_att_offsetbuf[1];
            optoErrch2_numericUpDown.Value = (decimal)GlobalVarFun.opto_att_offsetbuf[2];
            optoErrch3_numericUpDown.Value = (decimal)GlobalVarFun.opto_att_offsetbuf[3];
            
            delayNumericUpDown8.Value = (decimal)MEMTER.delay;
            delayNumericUpDown9.Value = (decimal)DOA.delay;
            //
            erCalNumericUpDown.Value = (decimal)GlobalVarFun.ER_cal_num;
            txCalNumericUpDown.Value = (decimal)GlobalVarFun.tx_cal_num;
            rxCalNumericUpDown.Value = (decimal)GlobalVarFun.rx_cal_num;
            TxWLengthnumericUpDown.Value = (decimal)GlobalVarFun.wLengthMaxErr;
            tBWLength.Text = TestSet.wLength_target.ToString();
            PssBertdelayNumericUpDown.Value = GlobalVarFun.pss_bert_delay;
            TOSATempSet_numericUpDown.Value = (decimal)TestSet.tosatemp_def;
        }

        private void meterCom_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
           // MEMTER.com_portname = meterCom_comboBox.Text;
            MEMTER.com_index = meterCom_comboBox.SelectedIndex;
        }

        private void attCom_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //DOA.com_portname = attCom_comboBox.Text;
            DOA.com_index = attCom_comboBox.SelectedIndex;
        }

        private void SwitchCom_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
           // opcicalSwitch.com_portname = SwitchCom_comboBox.Text;
            opcicalSwitch.com_index = SwitchCom_comboBox.SelectedIndex;
        }

        private void PSSCom_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //BIT_ERROR.com_portname = PSSCom_comboBox.Text;
            BIT_ERROR.com_index = PSSCom_comboBox.SelectedIndex;
        }

        private void erCalNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.ER_cal_num = (double)erCalNumericUpDown.Value;
        }

        private void txCalNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.tx_cal_num = (double)txCalNumericUpDown.Value;
        }

        private void checkBox_txTest_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_txTest.Checked)
            {
                GlobalVarFun.tx_test = true;
            }
            else
            {
                GlobalVarFun.tx_test = false;
            }
        }

        private void checkBox_TxNoPwr_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_TxNoPwr.Checked)
            {
                GlobalVarFun.tx_nopower_test = true;
            }
            else
            {
                GlobalVarFun.tx_nopower_test = false;
            }
        }

        private void checkBox_LosTest_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_LosTest.Checked)
            {
                GlobalVarFun.rx_los_test = true;
            }
            else
            {
                GlobalVarFun.rx_los_test = false;
            }
        }

        private void cBSenTest_CheckedChanged(object sender, EventArgs e)
        {
            if (cBSenTest.Checked)
            {
                GlobalVarFun.sen_test = true;
            }
            else
            {
                GlobalVarFun.sen_test = false;
            }
        }

        private void checkBox_txJt_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_txJt.Checked)
            {
                GlobalVarFun.tx_jitter_test = true;
            }
            else
            {
                GlobalVarFun.tx_jitter_test = false;
            }
        }

        private void checkBox_TOSA_NoMPD_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_TOSA_NoMPD.Checked)
            {
                GlobalVarFun.cob_ld = true;
                GlobalVarFun.txpwr_debug_method = 0x11;
            }
            else
            {
                GlobalVarFun.cob_ld = false;
                GlobalVarFun.txpwr_debug_method = 0x00;
            }
        }

        private void cBHardwareTxDis_CheckedChanged(object sender, EventArgs e)
        {
            if (cBHardwareTxDis.Checked)
            {
                GlobalVarFun.hw_txdis_test = true;
            }
            else
            {
                GlobalVarFun.hw_txdis_test = false;
            }
        }

        private void cBHardwareLOS_CheckedChanged(object sender, EventArgs e)
        {
            if (cBHardwareLOS.Checked)
            {
                GlobalVarFun.hw_los_test = true;
            }
            else
            {
                GlobalVarFun.hw_los_test = true;
            }
        }

        private void checkBox_EyeSave_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_EyeSave.Checked)
            {
                GlobalVarFun.tx_eye_save_test = true;
            }
            else
            {
                GlobalVarFun.tx_eye_save_test = false;
            }
        }

        private void checkBox_AlarmThresholds_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_AlarmThresholds.Checked)
            {
                GlobalVarFun.threshold_check = true;
            }
            else
            {
                GlobalVarFun.threshold_check = false;
            }
        }

        private void checkBox_debugTest_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_debugTest.Checked)
            {
                GlobalVarFun.flash_check = true;
            }
            else
            {
                GlobalVarFun.flash_check = false;
            }
        }

        private void checkBox_DisCDR_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_DisCDR.Checked)
            {
                GlobalVarFun.txrx_cdr_dis = true;
            }
            else
            {
                GlobalVarFun.txrx_cdr_dis = false;
            }
        }

        private void checkBox_DisTypeCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_DisTypeCheck.Checked)
            {
                GlobalVarFun.distype_check = true;
            }
            else
            {
                GlobalVarFun.distype_check = false;
            }
        }

        private void txapcMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
           // GlobalVarFun.ER_cal_num = (double)erCalNumericUpDown.Value;
            TestSet.txapc_Min_set = (ushort)txapcMin_numericUpDown.Value;
        }

        private void txmodMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.txmod_Min_set = (ushort)txmodMin_numericUpDown.Value;
        }

        private void txpe_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
           
        }

        private void checkBox_RxNoPwr_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_RxNoPwr.Checked)
            {
                GlobalVarFun.rx_nopower_test = true;
            }
            else
            {
                GlobalVarFun.rx_nopower_test = false;
            }
        }

        private void checkBox_useDCA_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_useDCA.Checked)
            {
                GlobalVarFun.power_use_DAC = true;
            }
            else
            {
                GlobalVarFun.power_use_DAC = false;
            }
        }

        private void txmodMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.txmod_Max_set = (ushort)txmodMax_numericUpDown.Value;
        }

        private void txapcMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.txapc_Max_set = (ushort)txapcMax_numericUpDown.Value;
        }

        private void btnRxAutoCheck_Click(object sender, EventArgs e)
        {
            int ch = 0;
            if (!GlobalVarFun.optoMeter_connected)
            {
                MessageBox.Show("请链接光功率计！");
                return;
            }

            if (!GlobalVarFun.optoAtt_connected)
            {
                MessageBox.Show("请链接衰减器！");
                return;
            }

            if (GlobalVarFun.optoSwitch_connected == false)
            {

                if (GlobalVarFun.USBtoI2C == null)//GlobalVarFun.USBtoI2C//usb_can_use
                {
                    MessageBox.Show("未选择光源USB连接/光开关");
                    return;
                }
            }

            TestResult.rxPwrReal[0] = Convert.ToSingle(rxPwrtextBox1.Text);
            TestResult.rxPwrReal[1] = Convert.ToSingle(rxPwrtextBox2.Text);
            TestResult.rxPwrReal[2] = Convert.ToSingle(rxPwrtextBox3.Text);
            TestResult.rxPwrReal[3] = Convert.ToSingle(rxPwrtextBox4.Text);
            TestResult.rxPwrReal[4] = Convert.ToSingle(rxPwrtextBox5.Text);

            TestResult.rxSen = Convert.ToSingle(textBox_Sen.Text);
            TestResult.rxDLos = Convert.ToSingle(textBox_DLos.Text);
            TestResult.rxALos = Convert.ToSingle(textBox_ALos.Text);
            TestResult.rxOverLoad = Convert.ToSingle(textBox_overLoad.Text);

            for (int i = 0; i < 1; i++)
            {
                cBMoudleCH.SelectedIndex = i;
                ch = cBMoudleCH.SelectedIndex;
                btnRxAutoCheck.BackColor = SystemColors.Control;
                testDataCheck_button.BackColor = SystemColors.Control;
                Refresh();
                if (RxAutoCheckStup(ch) == true)
                {
                    btnRxAutoCheck.BackColor = Color.Green;
                    testDataCheck_button.BackColor = Color.GreenYellow;
                    Thread.Sleep(100);               
                    if (moduletype_comboBox.Text.Contains("MM"))
                    {
                        break;//多模只以ch0通道为模版
                    }
                }
                else
                {
                    btnRxAutoCheck.BackColor = Color.Red;
                    testDataCheck_button.BackColor = Color.Yellow;
                    break;
                }

                //if (checkstup(ch) == true)
                //{
                //    btnRxAutoCheck.BackColor = Color.Green;
                //    testDataCheck_button.BackColor = Color.GreenYellow;
                //    Thread.Sleep(100);
                //    if (moduletype_comboBox.Text.Contains("MM"))
                //    {
                //        break;//多模只以ch0通道为模版
                //    }
                //}
                //else
                //{
                //    btnRxAutoCheck.BackColor = Color.Red;
                //    testDataCheck_button.BackColor = Color.Yellow;
                //    break;
                //}
                Refresh();
            }
        }
        //接收校准点检查自动设置
        private bool RxAutoCheckStup(int ch)
        {
           // int ch = 0;
            float tarpwr = 0, rxpwr = 0,err = 0,att = 0;
            if (!moduletype_comboBox.Text.Contains("MM"))//单模
            {
                //if (GlobalVarFun.USBtoI2C == null)//GlobalVarFun.USBtoI2C//usb_can_use
                //{
                //    MessageBox.Show("未选择USB连接");
                //    return false;
                //}
                ch = cBMoudleCH.SelectedIndex;
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
                else
                {
                    // test.SourceSoftEn(ch);//开启光源通道ch
                    if (test.SourceSoftEn(ch) == false)//开启光源通道
                    {
                        GlobalVarFun.usb_can_use = false;
                    }
                }
            }
            else//多模
            {
                ch = cBMoudleCH.SelectedIndex;
                if (GlobalVarFun.optoSwitch_connected)//光开关
                {
                    opticalSwitchSet(ch + 1);//光开关切换通道
                }
            }
            //check1
            //Convert.ToSingle(rxPwrtextBox1.Text);
            tarpwr = TestResult.rxPwrReal[0];
            att = AutoSetDOA(tarpwr);
            TestSet.rxPwr_Cal[0] = Get_OptoPower_Meter();
            //rxpwr = Get_OptoPower_Meter();
            rxpwr = TestSet.rxPwr_Cal[0];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_Att1.Text = att.ToString("F1");
            DOA.rxCalAtt[ch * 5 + 0] = att;
            DOA.rxCheckAtt[ch * 5 + 0] = att;
            //check2
            //tarpwr = Convert.ToSingle(rxPwrtextBox2.Text);
            tarpwr = TestResult.rxPwrReal[1];
            att = AutoSetDOA(tarpwr);
            //rxpwr = Get_OptoPower_Meter();
            TestSet.rxPwr_Cal[1] = Get_OptoPower_Meter();
            rxpwr = TestSet.rxPwr_Cal[1];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_Att2.Text = att.ToString("F1");
            DOA.rxCalAtt[ch * 5 + 1] = att;
            DOA.rxCheckAtt[ch * 5 + 1] = att;
            //check3
            //tarpwr = Convert.ToSingle(rxPwrtextBox3.Text);
            tarpwr = TestResult.rxPwrReal[2];
            att = AutoSetDOA(tarpwr);
           // rxpwr = Get_OptoPower_Meter();
            TestSet.rxPwr_Cal[2] = Get_OptoPower_Meter();
            rxpwr = TestSet.rxPwr_Cal[2];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_Att3.Text = att.ToString("F1");
            DOA.rxCalAtt[ch * 5 + 2] = att;
            DOA.rxCheckAtt[ch * 5 + 2] = att;

            if (GlobalVarFun.rx_is_apd)
            {
                //check4
                //tarpwr = Convert.ToSingle(rxPwrtextBox4.Text);
                tarpwr = TestResult.rxPwrReal[3];
                att = AutoSetDOA(tarpwr);
                //rxpwr = Get_OptoPower_Meter();
                TestSet.rxPwr_Cal[3] = Get_OptoPower_Meter();
                rxpwr = TestSet.rxPwr_Cal[3];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox_Att4.Text = att.ToString("F1");
                DOA.rxCalAtt[ch * 5 + 3] = att;
                DOA.rxCheckAtt[ch * 5 + 3] = att;
                //check5
               // tarpwr = Convert.ToSingle(rxPwrtextBox5.Text);
                tarpwr = TestResult.rxPwrReal[4];
                att = AutoSetDOA(tarpwr);
               // rxpwr = Get_OptoPower_Meter();
                TestSet.rxPwr_Cal[4] = Get_OptoPower_Meter();
                rxpwr = TestSet.rxPwr_Cal[4];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox_Att5.Text = att.ToString("F1");
                DOA.rxCalAtt[ch * 5 + 4] = att;
                DOA.rxCheckAtt[ch * 5 + 4] = att;
            }

            //overload
            tarpwr = Convert.ToSingle(textBox_overLoad.Text);
            att = AutoSetDOA(tarpwr);
            rxpwr = Get_OptoPower_Meter();
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_overLoadAtt.Text = att.ToString("F1");
            DOA.rxOverLoadAttBuf[ch] = att;
            //sen
            tarpwr = Convert.ToSingle(textBox_Sen.Text);
            att = AutoSetDOA(tarpwr);
            rxpwr = Get_OptoPower_Meter();
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_SenAtt.Text = att.ToString("F1");
            DOA.rxSenAttBuf[ch] = att;
            //D_los
            tarpwr = Convert.ToSingle(textBox_DLos.Text);
            att = AutoSetDOA(tarpwr);
            rxpwr = Get_OptoPower_Meter();
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_DLosAtt.Text = att.ToString("F1");
            DOA.rxDLosAttBuf[ch] = att;
            //A_los
            tarpwr = Convert.ToSingle(textBox_ALos.Text);
            att = AutoSetDOA(tarpwr);
            rxpwr = Get_OptoPower_Meter();
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_ALosAtt.Text = att.ToString("F1");
            DOA.rxALosAttBuf[ch] = att;

            GlobalVarFun.testDataIsOK = true;

            return true;
        }

        //自动设置衰减值
        private float AutoSetDOA(float tarval)
        {
            float att = 10;
            float rxpwr =0;
            int looptime =0;
            float pwrerr = 0;
            string attstr = "";  
           
            do
            {
                if (Math.Abs(pwrerr) < 1)
                {
                    if (pwrerr < 0)
                    {
                        pwrerr = -0.1f;
                    }
                    else
                    {
                        pwrerr = 0.1f;
                    }
                }          
                att += pwrerr;
                if (att < 0)
                {
                    att = 1;
                }
                attstr = att.ToString("F1");
                att = Convert.ToSingle(attstr);
                SetDOA_RxAttVal(att);
                rxpwr = Get_OptoPower_Meter();
                pwrerr = rxpwr - tarval;           
                looptime++;
               
            } while ((looptime < 15) && (att >= 0) && (Math.Abs(pwrerr) > 0.1));

            return att;
        }

         private bool checkstup(int ch)
        {
             float err = 0;
            float range = 0.2f;
            int channel = ch;
           // testDataCheck_button.BackColor = System.Drawing.Color.Gray;

            if ((GlobalVarFun.optoMeter_connected == false) || (GlobalVarFun.optoAtt_connected == false)) // 连接光功率计和光衰减器判断
            {
               // MessageBox.Show("请先连接光功率计和光衰减器！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            //real_rxpower1_textbox.Text = rxPwrtextBox1.Text;
            //real_rxpower2_textbox.Text = rxPwrtextBox2.Text;
            //real_rxpower3_textbox.Text = rxPwrtextBox3.Text;
            //real_rxpower4_textbox.Text = rxPwrtextBox4.Text;
            //real_rxpower5_textbox.Text = rxPwrtextBox5.Text;
            //ddm_rxpower1_textbox.Text = "";
            //ddm_rxpower2_textbox.Text = "";
            //ddm_rxpower3_textbox.Text = "";
            //ddm_rxpower4_textbox.Text = "";
            //ddm_rxpower5_textbox.Text = "";

            //real_rxpower6_textbox.Text = "-40";

            Refresh();

            TestResult.rxPwrReal[0] = Convert.ToSingle(rxPwrtextBox1.Text);
            TestResult.rxPwrReal[1] = Convert.ToSingle(rxPwrtextBox2.Text);
            TestResult.rxPwrReal[2] = Convert.ToSingle(rxPwrtextBox3.Text);
            TestResult.rxPwrReal[3] = Convert.ToSingle(rxPwrtextBox4.Text);
            TestResult.rxPwrReal[4] = Convert.ToSingle(rxPwrtextBox5.Text);

            TestResult.rxSen = Convert.ToSingle(textBox_Sen.Text);
            TestResult.rxDLos = Convert.ToSingle(textBox_DLos.Text);
            TestResult.rxALos = Convert.ToSingle(textBox_ALos.Text);
            TestResult.rxOverLoad = Convert.ToSingle(textBox_overLoad.Text);

            //
            //if (moduletype_comboBox.Text.Contains("MM"))     
            //{
            //    DOA.rxCalAtt[0] = DOA.rxCheckAtt[0];
            //    DOA.rxCalAtt[1] = DOA.rxCheckAtt[1];
            //    DOA.rxCalAtt[2] = DOA.rxCheckAtt[2];
            //    DOA.rxCalAtt[3] = DOA.rxCheckAtt[3];
            //    DOA.rxCalAtt[4] = DOA.rxCheckAtt[4];
            //    channel = 0;
            //}
            //else
            //{

            //    if (cBMoudleCH.Text == "ch0")
            //    {
            //        DOA.rxCalAtt[0] = DOA.rxCheckAtt[0];
            //        DOA.rxCalAtt[1] = DOA.rxCheckAtt[1];
            //        DOA.rxCalAtt[2] = DOA.rxCheckAtt[2];
            //        DOA.rxCalAtt[3] = DOA.rxCheckAtt[3];
            //        DOA.rxCalAtt[4] = DOA.rxCheckAtt[4];
            //        channel = 0;                
            //    }
            //    else if (cBMoudleCH.Text == "ch1")
            //    {
            //        DOA.rxCalAtt[5] = DOA.rxCheckAtt[5];
            //        DOA.rxCalAtt[6] = DOA.rxCheckAtt[6];
            //        DOA.rxCalAtt[7] = DOA.rxCheckAtt[7];
            //        DOA.rxCalAtt[8] = DOA.rxCheckAtt[8];
            //        DOA.rxCalAtt[9] = DOA.rxCheckAtt[9];
            //        channel = 1;                                  
            //    }
            //    else if (cBMoudleCH.Text == "ch2")
            //    {
            //        DOA.rxCalAtt[10] = DOA.rxCheckAtt[10];
            //        DOA.rxCalAtt[11] = DOA.rxCheckAtt[11];
            //        DOA.rxCalAtt[12] = DOA.rxCheckAtt[12];
            //        DOA.rxCalAtt[13] = DOA.rxCheckAtt[13];
            //        DOA.rxCalAtt[14] = DOA.rxCheckAtt[14];
            //        channel = 2;                 
            //    }
            //    else if (cBMoudleCH.Text == "ch3")
            //    {
            //        DOA.rxCalAtt[15] = DOA.rxCheckAtt[15];
            //        DOA.rxCalAtt[16] = DOA.rxCheckAtt[16];
            //        DOA.rxCalAtt[17] = DOA.rxCheckAtt[17];
            //        DOA.rxCalAtt[18] = DOA.rxCheckAtt[18];
            //        DOA.rxCalAtt[19] = DOA.rxCheckAtt[19];
            //        channel = 3;                 
            //    }
            //}
            GlobalVarFun.testDataIsOK = true;

            // RX SEN
            SetDOA_RxAttVal(DOA.rxSenAttBuf[channel]);
            err = TestResult.rxSen - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // RX DLOS
            SetDOA_RxAttVal(DOA.rxDLosAttBuf[channel]);
            err = TestResult.rxDLos - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // RX ALOS
            SetDOA_RxAttVal(DOA.rxALosAttBuf[channel]);
            err = TestResult.rxALos - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // RX OVERLOAD
            SetDOA_RxAttVal(DOA.rxOverLoadAttBuf[channel]);
            err = TestResult.rxOverLoad - Get_OptoPower_Meter();
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // CHECK 1
            SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 0]);
            TestSet.rxPwr_Cal[0] = Get_OptoPower_Meter();
            err = TestResult.rxPwrReal[0] - TestSet.rxPwr_Cal[0];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // CHECK 2
            SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 1]);
            TestSet.rxPwr_Cal[1] = Get_OptoPower_Meter();
            err = TestResult.rxPwrReal[1] - TestSet.rxPwr_Cal[1];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            // CHECK 3
            SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 2]);
            TestSet.rxPwr_Cal[2] = Get_OptoPower_Meter();
            err = TestResult.rxPwrReal[2] - TestSet.rxPwr_Cal[2];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK = false;
            }

            if (radioButton_APD.Checked)
            {
                // CHECK 4
                SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 3]);
                TestSet.rxPwr_Cal[3] = Get_OptoPower_Meter();
                err = TestResult.rxPwrReal[3] - TestSet.rxPwr_Cal[3];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK = false;
                }

                // CHECK 5
                SetDOA_RxAttVal(DOA.rxCheckAtt[5 * channel + 4]);
                TestSet.rxPwr_Cal[4] = Get_OptoPower_Meter();
                err = TestResult.rxPwrReal[4] - TestSet.rxPwr_Cal[4];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK = false;
                }
            }


            //

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            //if (radioButton_PIN.Checked)
            //{
            //    if (DOA.rxCalAtt[2] > 2)
            //    {
            //        DOA.rxCalAtt[1] = DOA.rxCalAtt[2] - 1;
            //        SetDOA_RxAttVal(DOA.rxCalAtt[5 * channel + 1]);
            //        TestSet.rxPwr_Cal[1] = Get_OptoPower_Meter();
            //    }
            //    else
            //    {
            //        GlobalVarFun.testDataIsOK = false;
            //    }
            //}         

            if (GlobalVarFun.testDataIsOK == true)
            {
               // testDataCheck_button.BackColor = System.Drawing.Color.GreenYellow;
                return true;
            }
            else
            {
                //testDataCheck_button.BackColor = System.Drawing.Color.Yellow;
               // MessageBox.Show("测试参数设置异常，精度为 +-0.2dB ！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void cBTEC_CheckedChanged(object sender, EventArgs e)
        {
            if (cBTEC.Checked)
            {
                GlobalVarFun.tx_tec_test = true;
            }
            else
            {
                GlobalVarFun.tx_tec_test = false;
            }
        }

        private void cBTosaTemp_CheckedChanged(object sender, EventArgs e)
        {
            if (cBTosaTemp.Checked)
            {
                GlobalVarFun.TOSATempEN = true;
                TOSATempMax_numericUpDown.Enabled = true;
                TOSATempMin_numericUpDown.Enabled = true;
                TOSATempSet_numericUpDown.Enabled = true;
                //VONMin_numericUpDown.Enabled = true;
                //VONMax_numericUpDown.Enabled = true;
            }
            else
            {
                GlobalVarFun.TOSATempEN = false;
                TOSATempMax_numericUpDown.Enabled = false;
                TOSATempMin_numericUpDown.Enabled = false;
                TOSATempSet_numericUpDown.Enabled = false;
                //VONMin_numericUpDown.Enabled = false;
                //VONMax_numericUpDown.Enabled = false;
            }
        }

        private void cBVon_CheckedChanged(object sender, EventArgs e)
        {
            if (cBVon.Checked)
            {
                GlobalVarFun.VONEN = true;
                VONMax_numericUpDown.Enabled = true;
                VONMin_numericUpDown.Enabled = true;
            }
            else
            {
                GlobalVarFun.VONEN = false;
                VONMax_numericUpDown.Enabled = false;
                VONMin_numericUpDown.Enabled = false;
            }
        }

        private void cBAPD_CheckedChanged(object sender, EventArgs e)
        {
            if (cBAPD.Checked)
            {
                GlobalVarFun.APDen = true;
                APDMax_numericUpDown.Enabled = true;
                APDMin_numericUpDown.Enabled = true;
            }
            else
            {
                GlobalVarFun.APDen = false;
                APDMax_numericUpDown.Enabled = false;
                APDMin_numericUpDown.Enabled = false;
            }
        }

        private void TOSATemp_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.tosatemp_max = (ushort)TOSATempMax_numericUpDown.Value;          
        }

        private void VON_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.Tx_von = (byte)VONMax_numericUpDown.Value;
        }

        private void APD_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.rxapd_min = (byte)APDMax_numericUpDown.Value;
        }
        
        private void btnWaveLenth_Click(object sender, EventArgs e)
        {
            if (GlobalVarFun.wlength_connected == true)
            {
                btnWaveLenth.BackColor = System.Drawing.Color.Gray;
                GlobalVarFun.kt86120c.Disconnect();//关闭连接
                GlobalVarFun.kt86120c.Dispose();
                cbBWavelength.Enabled = true;
                GlobalVarFun.wlength_connected = false;
            }
            else
            {
                GlobalVarFun.gpibname = cbBWavelength.Text.Trim();
                GlobalVarFun.kt86120c.OpticalModuleController();
                if (GlobalVarFun.kt86120c.Connect(cbBWavelength.Text) == false)
                {
                    btnWaveLenth.BackColor = System.Drawing.Color.Yellow;
                    GlobalVarFun.wlength_connected = false;
                    cbBWavelength.Enabled = true;
                }
                else
                {
                    btnWaveLenth.BackColor = System.Drawing.Color.GreenYellow;
                    cbBWavelength.Enabled = false;
                    GlobalVarFun.wlength_connected = true;
                }
            }
        }

        public List<string> GetGpibDevices()
        {
            List<string> deviceList = new List<string>();
            ResourceManager rm = new ResourceManager();
            try
            {
                // 查找所有GPIB设备（格式为 "GPIB::*::INSTR"）
                // string[] resources = rm.FindResources("GPIB?*INSTR");
                string[] resources = rm.FindRsrc("GPIB?*INSTR");

                foreach (string resource in resources)
                {
                    if (resource.Contains("GPIB"))
                    {
                        deviceList.Add(resource);
                    }
                }
            }
            catch //(Exception ex)
            {

            }
            return deviceList;
        }

        private void TxWLengthnumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.wLengthMaxErr = (double)TxWLengthnumericUpDown.Value;
        }

        private void tBWLength_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TestSet.wLength_target = Convert.ToDouble(tBWLength.Text);
            }
            catch
            {
                //MessageBox.Show("目标波长错误！");
                //TestSet.wLength_target = 0;
            }
            
        }

        private void cBDAC86100D_CheckedChanged(object sender, EventArgs e)
        {
            if (cBDAC86100D.Checked)
            {
                GlobalVarFun.DCA86100D_Open = true;
                cBDCANl092X.Checked = false;
                gpibAddress = "GPIB0::07::INSTR";
            }
            else
            {
                GlobalVarFun.DCA86100D_Open = false;
            }
            
        }

        private void cBDCANl092X_CheckedChanged(object sender, EventArgs e)
        {
            if (cBDCANl092X.Checked)
            {
                GlobalVarFun.N1092x_Open = true;              
                cBDAC86100D.Checked = false;
                gpibAddress = "TCPIP0::localhost::inst0::INSTR";
            }
            else
            {
                GlobalVarFun.DCA86100D_Open = false;
            }
            
            //gpibAddress = "TCPIP0::localhost::hislip0,4880::INSTR";          
        }

        private void rBDualFibers_CheckedChanged(object sender, EventArgs e)
        {
            wl_ch0_min_numericUpDown.Value = (decimal)1294.53f;
            wl_ch0_max_numericUpDown.Value = (decimal)1296.59f;

            wl_ch1_min_numericUpDown.Value = (decimal)1299.02f;
            wl_ch1_max_numericUpDown.Value = (decimal)1301.09f;

            wl_ch2_min_numericUpDown.Value = (decimal)1303.54f;
            wl_ch2_max_numericUpDown.Value = (decimal)1305.63f;

            wl_ch3_min_numericUpDown.Value = (decimal)1308.09f;
            wl_ch3_max_numericUpDown.Value = (decimal)1310.19f;

            TestSet.wl_min[0] = (float)1294.53f;
            TestSet.wl_min[1] = (float)1299.02f;
            TestSet.wl_min[2] = (float)1303.54f;
            TestSet.wl_min[3] = (float)1308.09f;

            TestSet.wl_max[0] = (float)1296.59f;
            TestSet.wl_max[1] = (float)1301.09f;
            TestSet.wl_max[2] = (float)1305.63f;
            TestSet.wl_max[3] = (float)1310.19f;

            TestSet.EMLTestType = 1;
        }

        private void rBBiDi23_CheckedChanged(object sender, EventArgs e)
        {
            wl_ch0_min_numericUpDown.Value = (decimal)1272.55f;
            wl_ch0_max_numericUpDown.Value = (decimal)1274.54f;

            wl_ch1_min_numericUpDown.Value = (decimal)1276.89f;
            wl_ch1_max_numericUpDown.Value = (decimal)1278.89f;

            wl_ch2_min_numericUpDown.Value = (decimal)1281.25f;
            wl_ch2_max_numericUpDown.Value = (decimal)1283.27f;

            wl_ch3_min_numericUpDown.Value = (decimal)1285.65f;
            wl_ch3_max_numericUpDown.Value = (decimal)1287.68f;

            TestSet.wl_min[0] = (float)1272.55f;
            TestSet.wl_min[1] = (float)1276.89f;
            TestSet.wl_min[2] = (float)1281.25f;
            TestSet.wl_min[3] = (float)1285.65f;

            TestSet.wl_max[0] = (float)1274.54f;
            TestSet.wl_max[1] = (float)1278.89f;
            TestSet.wl_max[2] = (float)1283.27f;
            TestSet.wl_max[3] = (float)1287.68f;

            TestSet.EMLTestType = 2;
        }

        private void rBBiDi32_CheckedChanged(object sender, EventArgs e)
        {
            wl_ch0_min_numericUpDown.Value = (decimal)1294.53f;
            wl_ch0_max_numericUpDown.Value = (decimal)1296.59f;

            wl_ch1_min_numericUpDown.Value = (decimal)1299.02f;
            wl_ch1_max_numericUpDown.Value = (decimal)1301.09f;

            wl_ch2_min_numericUpDown.Value = (decimal)1303.54f;
            wl_ch2_max_numericUpDown.Value = (decimal)1305.54f;

            wl_ch3_min_numericUpDown.Value = (decimal)1308.09f;
            wl_ch3_max_numericUpDown.Value = (decimal)1310.09f;

            TestSet.wl_min[0] = (float)1294.53f;
            TestSet.wl_min[1] = (float)1299.02f;
            TestSet.wl_min[2] = (float)1303.54f;
            TestSet.wl_min[3] = (float)1308.09f;

            TestSet.wl_max[0] = (float)1296.59f;
            TestSet.wl_max[1] = (float)1301.09f;
            TestSet.wl_max[2] = (float)1305.54f;
            TestSet.wl_max[3] = (float)1310.09f;

            TestSet.EMLTestType = 3;
        }

        private void rB40G_CheckedChanged(object sender, EventArgs e)
        {
            wl_ch0_min_numericUpDown.Value = 0;
            wl_ch0_max_numericUpDown.Value = 0;

            wl_ch1_min_numericUpDown.Value = 0;
            wl_ch1_max_numericUpDown.Value = 0;

            wl_ch2_min_numericUpDown.Value = 0;
            wl_ch2_max_numericUpDown.Value = 0;

            wl_ch3_min_numericUpDown.Value = 0;
            wl_ch3_max_numericUpDown.Value = 0;

            tBWLength.Text = "0";

            TestSet.EMLTestType = 0;
        }

        private void TOSATempMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.tosatemp_max = (ushort)TOSATempMax_numericUpDown.Value; 
        }

        private void TOSATempMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.tosatemp_min = (ushort)TOSATempMin_numericUpDown.Value; 
        }

        private void rBTestTxSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (rBTestTxSelect.Checked)
            {
                GlobalVarFun.test_tx_select = true;
                checkBox_txTest.Checked = true;
                checkBox_TxNoPwr.Checked = true;

                checkBox_rxTest.Checked = false;
                checkBox_LosTest.Checked = false;
                checkBox_RxNoPwr.Checked = false;
                if (TestResult.fibertop_pn.Contains("-40"))
                {
                    cBTEC.Checked = true;
                    cBTosaTemp.Checked = true;
                    //cBVon.Checked = true;
                    cBSenTest.Checked = false;
                    cBAPD.Checked = false;
                }
            }
        }

        private void rBTestRxSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (rBTestRxSelect.Checked)
            {
                GlobalVarFun.test_tx_select = false;

                checkBox_txTest.Checked = false;
                checkBox_TxNoPwr.Checked = false;
                cBTEC.Checked = false;
                cBTosaTemp.Checked = false;

                checkBox_rxTest.Checked = true;
                checkBox_LosTest.Checked = true;
                checkBox_RxNoPwr.Checked = true;

                cBSenTest.Checked = true;
                //cBAPD.Checked = true;
            }           
        }

        private void TOSATempSet_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.tosatemp_def = (ushort)TOSATempSet_numericUpDown.Value; 
        }

        private void APDMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.rxapd_min = (ushort)APDMin_numericUpDown.Value; 
        }

        private void APDMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            TestSet.rxapd_max = (ushort)APDMax_numericUpDown.Value; 
        }

        private void PssBertdelayNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.pss_bert_delay = (int)PssBertdelayNumericUpDown.Value;
        }


    }
}
