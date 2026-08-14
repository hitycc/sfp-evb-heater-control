using FibertopTest_Common;
using SFPXFP自动测试软件多端口;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Ivi.Visa.Interop;

namespace SFPXFP自动测试软件多端口
{
    public partial class SetupFrm : Form
    {
        string gpibAddress = TestControl.gpibAddress;
        FilesINI filesini = new FilesINI("D:\\Com.ini");
        public SetupFrm()
        {
            InitializeComponent();
        }
        //////////////////////////////////////界面设置初始化///////////////////////////////////////////////////////////
        #region 设置初始化 / 设置记录
        private void SetupInit()
        {
            try
            {
                txapcMin_numericUpDown.Value = (decimal)TestSet.txapc_Min;
                txapcMax_numericUpDown.Value = (decimal)TestSet.txapc_Max;
                txmodMin_numericUpDown.Value = (decimal)TestSet.txmod_Min;
                txmodMax_numericUpDown.Value = (decimal)TestSet.txmod_Max;
                rxlosMin_numericUpDown.Value = (decimal)TestSet.rxlos_Min;
                rxlosMax_numericUpDown.Value = (decimal)TestSet.rxlos_Max;
                rxAPDmin_numericUpDown.Value = (decimal)TestSet.rxapd_min;
                rxAPDmax_numericUpDown.Value = (decimal)TestSet.rxapd_max;
                TosaTempmin_numericUpDown.Value = (decimal)TestSet.tosatemp_min;
                TosaTempmax_numericUpDown.Value = (decimal)TestSet.tosatemp_max;
                VONmin_numericUpDown.Value = (decimal)TestSet.von_min;
                VONmax_numericUpDown.Value = (decimal)TestSet.von_max;
                CrossingMin_numericUpDown.Value = (decimal)TestSet.txcpa_Min;
                CrossingMax_numericUpDown.Value = (decimal)TestSet.txcpa_Max;
                spec_numericUpDown.Value = (decimal)TestSet.spectralwidth_max;

                if (cBEMLTest.Checked)
                {
                    if (tBTargetWlength.Text != "")
                    {
                        TestSet.wLength_target = Convert.ToDouble(tBTargetWlength.Text.Trim());
                    }
                    else
                    {
                        return;
                    }
                }
                GlobalVarFun.setup.rxpwr_cal = (float)(rxCalNumericUpDown.Value);
                wLengthCalnumericUpDown.Value = (decimal) GlobalVarFun.setup.wlgth_cal;
                erCalNumericUpDown.Value = (decimal)TestSet.txer_cal;
                txCalNumericUpDown.Value = (decimal)TestSet.txpwr_cal;
                rxCalNumericUpDown.Value = (decimal)TestSet.rxpwr_cal;
                //optoErr_numericUpDown.Value  = (decimal)GlobalVarFun.setup.meter_err_dut1;
                txpe_numericUpDown.Value = TestResult.txpeVal;
                waveforms_numericUpDown.Value = TestResult.waveforms_count;
                optoErr_numericUpDown.Value = (decimal)TestSet.meter_pwr_err;
                optoErr2_numericUpDown.Value = (decimal)TestSet2.meter_pwr_err;
                optoErr3_numericUpDown.Value = (decimal)TestSet3.meter_pwr_err;
                optoErr4_numericUpDown.Value = (decimal)TestSet4.meter_pwr_err;



                if (GlobalVarFun.moduleType == "SFP+")
                {
                    if (GlobalVarFun.txpwr_debug_method == 0x11)
                    //if (checkBox_TOSA_NoMPD.Checked)
                    {
                        // 0x00:线性计算法 apc-->uw & bias   0x11: 普通二分法 apc-->dBm   22:差值二分法 apc-->uW 33:差值二分法 apc-->uW ,0.6倍bias, ER 二次调试
                        checkBox_TOSA_NoMPD.Checked = true;
                    }
                    if (GlobalVarFun.txpwr_debug_method == 0x33)
                    //if (cB_25G_Algorithm.Checked)
                    {
                        cB_25G_Algorithm.Checked = true;
                    }
                }

                //功能选择
                checkBox_rxTest.Checked = GlobalVarFun.setup.rx_test;
                checkBox_RxNoPwr.Checked = GlobalVarFun.setup.rx_nopwr_test;
                checkBox_txTest.Checked = GlobalVarFun.setup.tx_test;
                checkBox_TxNoPwr.Checked = GlobalVarFun.setup.tx_nopwr_test;
                cBHardwareTxDis.Checked = GlobalVarFun.setup.tx_hardware_disable;
                checkBox_EyeSave.Checked = GlobalVarFun.setup.image_save;
                checkBox_AlarmThresholds.Checked = GlobalVarFun.setup.threshold_check;
                checkBox_debugTest.Checked = GlobalVarFun.setup.flash_check;
                cBSenTest.Checked = GlobalVarFun.setup.rx_sen_test;
                checkBox_txJt.Checked = GlobalVarFun.setup.tx_jitter_test;
                cBEMLTest.Checked = GlobalVarFun.setup.tx_eml_test;
                cB_25G_Algorithm.Checked = GlobalVarFun.setup.algorithm_25g_lr;
                checkBox_TOSA_NoMPD.Checked = GlobalVarFun.setup.algorithm_cob_ld;
                cBHardwareLOS.Checked = GlobalVarFun.setup.rx_hardware_los;
                checkBox_APD.Checked = GlobalVarFun.setup.rx_apd_test;
                cBelec_moudle.Checked = GlobalVarFun.setup.electrical_module;
                checkBox_Init.Checked = GlobalVarFun.setup.init_module;
                checkBox_DisCDR.Checked = GlobalVarFun.setup.tx_rx_cdr_dis;
                checkBox_DisTypeCheck.Checked = GlobalVarFun.setup.scheme_check_dis;
            }
            catch
            {
               // MessageBox.Show("参数异常，请重新检查参数");
            }

        }
        #endregion
        //////////////////////////////////////设备连接////////////////////////////////////////////////////////////////

        //
        private void UncheckAllCheckBoxesInGroupBox(GroupBox groupBox)
        {
            if (groupBox == null) return;

            // 方法一：使用 foreach 和 is 操作符
            foreach (Control control in groupBox.Controls)
            {
                if (control is CheckBox checkBox)
                {
                    checkBox.Checked = false;
                }
            }
        }

        //DCA N092X
        private void cBDCANl092X_CheckedChanged(object sender, EventArgs e)
        {
            gpibAddress = "TCPIP0::localhost::hislip0,4880::INSTR";
            GlobalVarFun.setup.dca_n1092x = true;
        }
        //DCA 86100D
        private void cBDAC86100D_CheckedChanged(object sender, EventArgs e)
        {
            gpibAddress = "GPIB0::07::INSTR";
            GlobalVarFun.setup.dca_86100d = true;
        }

        #region//设备衰减/延时
        private void optoErr_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void ER_Att_NumericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void delayNumericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            GlobalVarFun.meterdealy = (int)delayNumericUpDown8.Value;
            GlobalVarFun.setup.meter_delay = GlobalVarFun.meterdealy;
        }

        private void delayNumericUpDown9_ValueChanged(object sender, EventArgs e)
        {

        }

        private void DCAoptoerr_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void spec_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void delaynumericUpDown_pss_bert_ValueChanged(object sender, EventArgs e)
        {

        }
        #endregion

        #region//功能选择
        private void checkBox_rxTest_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_RxNoPwr_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cBSenTest_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_txTest_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_TxNoPwr_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cBHardwareTxDis_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_txJt_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cBEMLTest_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cB_25G_Algorithm_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_TOSA_NoMPD_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cBHardwareLOS_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_EyeSave_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_AlarmThresholds_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_debugTest_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_APD_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void waveforms_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }
        private void cBelec_moudle_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_Init_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_DisCDR_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox_DisTypeCheck_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rBTxTestSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (rBTxTestSelect.Checked)
            {
                //所有checkbox  checked = false
                UncheckAllCheckBoxesInGroupBox(groupBox2);
                checkBox_txTest.Checked = true;
                checkBox_TxNoPwr.Checked = true;
                cBHardwareTxDis.Checked = true;
                checkBox_AlarmThresholds.Checked = true;
                checkBox_debugTest.Checked = true;
            }
        }

        private void rBRxTestSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (rBRxTestSelect.Checked)
            {
                //所有checkbox  checked = false
                UncheckAllCheckBoxesInGroupBox(groupBox2);
                checkBox_rxTest.Checked = true;
                checkBox_RxNoPwr.Checked = true;
                cBHardwareLOS.Checked = true;
                cBSenTest.Checked = true;
                checkBox_AlarmThresholds.Checked = true;
                checkBox_debugTest.Checked = true;
            }
        }

        #endregion

        #region//参数选择
        private void txpe_checkBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txpe_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txapcMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txapcMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txmodMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txmodMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void CrossingMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void CrossingMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void TosaTempmin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void TosaTempmax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void tBTargetWlength_TextChanged(object sender, EventArgs e)
        {

        }

        private void rxlosMin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void rxlosMax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void rxAPDmin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void rxAPDmax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void VONmin_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void VONmax_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }
        #endregion

        #region//模块型号选择
        private void moduletype_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            TestResult.fibertop_pn = moduletype_comboBox.Text;
            TestResult2.fibertop_pn = moduletype_comboBox.Text;
            TestResult3.fibertop_pn = moduletype_comboBox.Text;
            TestResult4.fibertop_pn = moduletype_comboBox.Text;

            checkBox_DisCDR.Checked = false; //2020.4.8
            checkBox_DisTypeCheck.Checked = false; //2021.5.29
            checkBox_TOSA_NoMPD.Checked = false; //2023.11.7
            cB_25G_Algorithm.Checked = false;


            if ((moduletype_comboBox.SelectedIndex != GlobalVarFun.type_index) || (TestSet.setupUI_ok == false))
            {
                GlobalVarFun.mycontrol_dut4.GetTypeDebugInfoFromAccessdb();
                GlobalVarFun.mycontrol_dut3.GetTypeDebugInfoFromAccessdb();
                GlobalVarFun.mycontrol_dut2.GetTypeDebugInfoFromAccessdb();
                if ((GlobalVarFun.mycontrol_dut1.GetTypeDebugInfoFromAccessdb() == false))
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
               
            }
            //端口1
            rxPwrtextBox1.Text = TestSet.rxPwr_Real[0].ToString("F1");
            rxPwrtextBox2.Text = TestSet.rxPwr_Real[1].ToString("F1");
            rxPwrtextBox3.Text = TestSet.rxPwr_Real[2].ToString("F1");
            rxPwrtextBox4.Text = TestSet.rxPwr_Real[3].ToString("F1");
            rxPwrtextBox5.Text = TestSet.rxPwr_Real[4].ToString("F1");
            textBox_overLoad.Text = TestSet.rx_OverLoad.ToString("F1");
            textBox_Sen.Text = TestSet.rx_Sen.ToString("F1");
            textBox_DLos.Text = TestSet.rx_DLos.ToString("F1");
            textBox_ALos.Text = TestSet.rx_ALos.ToString("F1");

            // APC MOD LOS 调试范围值 //2017.11.30
            txapcMin_numericUpDown.Text = TestSet.txapc_Min.ToString();
            txapcMax_numericUpDown.Text = TestSet.txapc_Max.ToString();
            txmodMin_numericUpDown.Text = TestSet.txmod_Min.ToString();
            txmodMax_numericUpDown.Text = TestSet.txmod_Max.ToString();
            rxlosMin_numericUpDown.Text = TestSet.rxlos_Min.ToString();
            rxlosMax_numericUpDown.Text = TestSet.rxlos_Max.ToString();
            //端口2
            rxPwr2textBox1.Text = TestSet.rxPwr_Real[0].ToString("F1");
            rxPwr2textBox2.Text = TestSet.rxPwr_Real[1].ToString("F1");
            rxPwr2textBox3.Text = TestSet.rxPwr_Real[2].ToString("F1");
            rxPwr2textBox4.Text = TestSet.rxPwr_Real[3].ToString("F1");
            rxPwr2textBox5.Text = TestSet.rxPwr_Real[4].ToString("F1");
            textBox2_overLoad.Text = TestSet.rx_OverLoad.ToString("F1");
            textBox2_Sen.Text = TestSet.rx_Sen.ToString("F1");
            textBox2_DLos.Text = TestSet.rx_DLos.ToString("F1");
            textBox2_ALos.Text = TestSet.rx_ALos.ToString("F1");

            //端口3
            rxPwr3textBox1.Text = TestSet.rxPwr_Real[0].ToString("F1");
            rxPwr3textBox2.Text = TestSet.rxPwr_Real[1].ToString("F1");
            rxPwr3textBox3.Text = TestSet.rxPwr_Real[2].ToString("F1");
            rxPwr3textBox4.Text = TestSet.rxPwr_Real[3].ToString("F1");
            rxPwr3textBox5.Text = TestSet.rxPwr_Real[4].ToString("F1");
            textBox3_overLoad.Text = TestSet.rx_OverLoad.ToString("F1");
            textBox3_Sen.Text = TestSet.rx_Sen.ToString("F1");
            textBox3_DLos.Text = TestSet.rx_DLos.ToString("F1");
            textBox3_ALos.Text = TestSet.rx_ALos.ToString("F1");

            //端口4
            rxPwr4textBox1.Text = TestSet.rxPwr_Real[0].ToString("F1");
            rxPwr4textBox2.Text = TestSet.rxPwr_Real[1].ToString("F1");
            rxPwr4textBox3.Text = TestSet.rxPwr_Real[2].ToString("F1");
            rxPwr4textBox4.Text = TestSet.rxPwr_Real[3].ToString("F1");

            rxPwr4textBox5.Text = TestSet.rxPwr_Real[4].ToString("F1");
            textBox4_overLoad.Text = TestSet.rx_OverLoad.ToString("F1");
            textBox4_Sen.Text = TestSet.rx_Sen.ToString("F1");
            textBox4_DLos.Text = TestSet.rx_DLos.ToString("F1");
            textBox4_ALos.Text = TestSet.rx_ALos.ToString("F1");


            TestSet.setupUI_ok = true;
            TestSet2.setupUI_ok = true;
            TestSet3.setupUI_ok = true;
            TestSet4.setupUI_ok = true;
            GlobalVarFun.type_index = moduletype_comboBox.SelectedIndex;
            //UI默认设置
            UIValDefSet();
        }
        private void UIValDefSet()
        {
            if (TestSet.test_sen != "")
            {
                // APC MOD LOS 调试范围默认值 //2025.11.25
                txapcMin_numericUpDown.Text = TestSet.txapc_Min_def.ToString();
                txapcMax_numericUpDown.Text = TestSet.txapc_Max_def.ToString();
                txmodMin_numericUpDown.Text = TestSet.txmod_Min_def.ToString();
                txmodMax_numericUpDown.Text = TestSet.txmod_Max_def.ToString();
                rxlosMin_numericUpDown.Text = TestSet.rxlos_Min_def.ToString();
                rxlosMax_numericUpDown.Text = TestSet.rxlos_Max_def.ToString();
                //APD VON CPA Tosatemp 调试范围默认值 
                rxAPDmin_numericUpDown.Text = TestSet.rxapd_min_def.ToString();
                rxAPDmax_numericUpDown.Text = TestSet.rxapd_max_def.ToString();
                VONmin_numericUpDown.Text = TestSet.von_min_def.ToString();
                VONmax_numericUpDown.Text = TestSet.von_max_def.ToString();
                CrossingMin_numericUpDown.Text = TestSet.txcpa_Min_def.ToString();
                CrossingMax_numericUpDown.Text = TestSet.txcpa_Max_def.ToString();
                TosaTempmin_numericUpDown.Text = TestSet.tosatemp_min_def.ToString();
                TosaTempmax_numericUpDown.Text = TestSet.tosatemp_max_def.ToString();
                //TxER TxPwr Rxpwr 精度默认值
                erCalNumericUpDown.Text = TestSet.txer_prec.ToString();
                txCalNumericUpDown.Text = TestSet.txPwr_prec.ToString();
                rxCalNumericUpDown.Text = TestSet.rxPwr_prec.ToString();
                //波长精度，波长默认值
                wLengthCalnumericUpDown.Text = TestSet.wlgth_err.ToString();
                tBTargetWlength.Text = TestSet.wlgth_prec.ToString();
                //设备操作延时
                delayNumericUpDown9.Text = TestSet.delay_doa.ToString();
                delayNumericUpDown8.Text = TestSet.delay_opm.ToString();
                delaynumericUpDown_pss_bert.Text = TestSet.delay_pssbert.ToString();
                //界面checkbox 默认勾选
                cBEMLTest.Checked = (TestSet.test_eml != "") ? (TestSet.test_eml == "1") : (TestSet.test_eml == "0");
                cB_25G_Algorithm.Checked = (TestSet.test_25galg != "") ? (TestSet.test_25galg == "1") : (TestSet.test_25galg == "0");
                checkBox_TOSA_NoMPD.Checked = (TestSet.test_cobld != "") ? (TestSet.test_cobld == "1") : (TestSet.test_cobld == "0");
                checkBox_APD.Checked = (TestSet.test_apd != "") ? (TestSet.test_apd == "1") : (TestSet.test_apd == "0");
                cBelec_moudle.Checked = (TestSet.test_coppersfp != "") ? (TestSet.test_coppersfp == "1") : (TestSet.test_coppersfp == "0");
                checkBox_Init.Checked = (TestSet.test_init != "") ? (TestSet.test_init == "1") : (TestSet.test_init == "0");
                checkBox_DisCDR.Checked = (TestSet.test_cdrdis != "") ? (TestSet.test_cdrdis == "1") : (TestSet.test_cdrdis == "0");
                checkBox_DisTypeCheck.Checked = (TestSet.test_schemedis != "") ? (TestSet.test_schemedis == "1") : (TestSet.test_schemedis == "0");
                radioButton_PIN.Checked = (TestSet.test_rosa_pin != "") ? (TestSet.test_rosa_pin == "1") : (TestSet.test_rosa_pin == "0");
                checkBox_EyeSave.Checked = (TestSet.test_eyesave != "") ? (TestSet.test_eyesave == "1") : (TestSet.test_eyesave == "0");

            }
        }
        #endregion

            #region//接收检测功能设置
        private void button_calTest1_Click(object sender, EventArgs e)
        {
            DOA.rxCheckAtt[0] = Convert.ToSingle(textBox_Att1.Text);
            textBox_Att1.Text = DOA.rxCheckAtt[0].ToString("F1");
            DOA.rxCheckAtt[0] = Convert.ToSingle(textBox_Att1.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[0]);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_calTest2_Click(object sender, EventArgs e)
        {
            DOA.rxCheckAtt[1] = Convert.ToSingle(textBox_Att2.Text);
            textBox_Att2.Text = DOA.rxCheckAtt[1].ToString("F1");
            DOA.rxCheckAtt[1] = Convert.ToSingle(textBox_Att2.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[1]);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_calTest3_Click(object sender, EventArgs e)
        {
            DOA.rxCheckAtt[2] = Convert.ToSingle(textBox_Att3.Text);
            textBox_Att3.Text = DOA.rxCheckAtt[2].ToString("F1");
            DOA.rxCheckAtt[2] = Convert.ToSingle(textBox_Att3.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[2]);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_calTest4_Click(object sender, EventArgs e)
        {
            DOA.rxCheckAtt[3] = Convert.ToSingle(textBox_Att4.Text);
            textBox_Att4.Text = DOA.rxCheckAtt[3].ToString("F1");
            DOA.rxCheckAtt[3] = Convert.ToSingle(textBox_Att4.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[3]);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_calTest5_Click(object sender, EventArgs e)
        {
            DOA.rxCheckAtt[4] = Convert.ToSingle(textBox_Att5.Text);
            textBox_Att5.Text = DOA.rxCheckAtt[4].ToString("F1");
            DOA.rxCheckAtt[4] = Convert.ToSingle(textBox_Att5.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[4]);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_overLoadTest_Click(object sender, EventArgs e)
        {
            DOA.rxOverLoadAtt = Convert.ToSingle(textBox_overLoadAtt.Text);
            textBox_overLoadAtt.Text = DOA.rxOverLoadAtt.ToString("F1");
            DOA.rxOverLoadAtt = Convert.ToSingle(textBox_overLoadAtt.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxOverLoadAtt);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_SenTest_Click(object sender, EventArgs e)
        {
            DOA.rxSenAtt = Convert.ToSingle(textBox_SenAtt.Text);
            textBox_SenAtt.Text = DOA.rxSenAtt.ToString("F1");
            DOA.rxSenAtt = Convert.ToSingle(textBox_SenAtt.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxSenAtt);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_DLosTest_Click(object sender, EventArgs e)
        {
            DOA.rxDLosAtt = Convert.ToSingle(textBox_DLosAtt.Text);
            textBox_DLosAtt.Text = DOA.rxDLosAtt.ToString("F1");
            DOA.rxDLosAtt = Convert.ToSingle(textBox_DLosAtt.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxDLosAtt);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void button_ALosTest_Click(object sender, EventArgs e)
        {
            DOA.rxALosAtt = Convert.ToSingle(textBox_ALosAtt.Text);
            textBox_ALosAtt.Text = DOA.rxALosAtt.ToString("F1");
            DOA.rxALosAtt = Convert.ToSingle(textBox_ALosAtt.Text);

            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxALosAtt);

            GlobalVarFun.testDataIsOK1 = false;
        }

        private void radioButton_PIN_CheckedChanged(object sender, EventArgs e)
        {
            textBox_Att4.ReadOnly = true;
            textBox_Att5.ReadOnly = true;

            button_calTest4.Enabled = false;
            button_calTest5.Enabled = false;

            textBox2_Att4.ReadOnly = true;
            textBox2_Att5.ReadOnly = true;

            button2_calTest4.Enabled = false;
            button2_calTest5.Enabled = false;

            //GlobalVarFun.apb_check = false;//2025.05.21
            //
            Refresh();
        }

        private void radioButton_APD_CheckedChanged(object sender, EventArgs e)
        {
            textBox_Att4.ReadOnly = false;
            textBox_Att5.ReadOnly = false;

            button_calTest4.Enabled = true;
            button_calTest5.Enabled = true;

            textBox2_Att4.ReadOnly = false;
            textBox2_Att5.ReadOnly = false;

            button2_calTest4.Enabled = true;
            button2_calTest5.Enabled = true;
            //GlobalVarFun.apb_check = true;//2025.05.21
            //
            Refresh();
        }

        private void testDataCheck_button_Click(object sender, EventArgs e)
        {
            float err = 0;
            float range = 0.2f;

            testDataCheck_button.BackColor = System.Drawing.Color.Gray;

            if ((GlobalVarFun.setup.meter_connect == false) || (GlobalVarFun.setup.doa_connect == false)) // 连接光功率计和光衰减器判断
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("请先连接光功率计和光衰减器！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Please connect the optical power meter and attenuator first! Please confirm!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
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

            //
            DOA.rxCalAtt[0] = DOA.rxCheckAtt[0];
            DOA.rxCalAtt[1] = DOA.rxCheckAtt[1];
            DOA.rxCalAtt[2] = DOA.rxCheckAtt[2];
            DOA.rxCalAtt[3] = DOA.rxCheckAtt[3];
            DOA.rxCalAtt[4] = DOA.rxCheckAtt[4];

            GlobalVarFun.testDataIsOK1 = true;

            // RX SEN
            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxSenAtt);
            err = TestResult.rxSen - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK1 = false;
            }

            // RX DLOS
            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxDLosAtt);
            err = TestResult.rxDLos - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK1 = false;
            }

            // RX ALOS
            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxALosAtt);
            err = TestResult.rxALos - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK1 = false;
            }

            // RX OVERLOAD
            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxOverLoadAtt);
            err = TestResult.rxOverLoad - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK1 = false;
            }

            // CHECK 
            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[0]);
            TestSet.rxPwr_Cal[0] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            err = TestResult.rxPwrReal[0] - TestSet.rxPwr_Cal[0];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK1 = false;
            }

            // CHECK 
            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[1]);
            TestSet.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            err = TestResult.rxPwrReal[1] - TestSet.rxPwr_Cal[1];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK1 = false;
            }

            // CHECK 
            GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[2]);
            TestSet.rxPwr_Cal[2] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            err = TestResult.rxPwrReal[2] - TestSet.rxPwr_Cal[2];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK1 = false;
            }

            if (radioButton_APD.Checked)
            {
                // CHECK 
                GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[3]);
                TestSet.rxPwr_Cal[3] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
                err = TestResult.rxPwrReal[3] - TestSet.rxPwr_Cal[3];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK1 = false;
                }

                // CHECK 
                GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCheckAtt[4]);
                TestSet.rxPwr_Cal[4] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
                err = TestResult.rxPwrReal[4] - TestSet.rxPwr_Cal[4];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK1 = false;
                }
            }
            //

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            if (radioButton_PIN.Checked)
            {
                if (DOA.rxCalAtt[2] > 2)
                {
                    DOA.rxCalAtt[1] = DOA.rxCalAtt[2] - 1;
                    GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCalAtt[1]);
                    TestSet.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
                }
                else
                {
                    GlobalVarFun.testDataIsOK1 = false;
                }
            }

            if (GlobalVarFun.testDataIsOK1 == true)
            {
                testDataCheck_button.BackColor = System.Drawing.Color.GreenYellow;
            }
            else
            {
                testDataCheck_button.BackColor = System.Drawing.Color.Yellow;
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("测试参数设置异常，精度为 +-0.2dB ！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Test parameter Settings are abnormal, accuracy is +-0.2dB!", "errror", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
        }

        /// <summary>
        /// 切换光开关到指定通道对应的光路上（Tx/Rx同时打通）
        /// </summary>
        public static void SwitchToChannel(OTP12Driver drv, int channel)
        {
            var cfg = GetChannelConfig(channel);
            drv.SetSlot(cfg.SwSlot);
            drv.SW_SetChannel(cfg.SwIn, cfg.SwOut);
            Thread.Sleep(300);
            drv.SetSlot(cfg.VoaSlot);
        }

        /// <summary>
        /// 获取加热台通道(1~4)对应的VOA+光开关配置
        /// 光开关(2x2互锁): 切in1→out2自动in2→out1，Tx/Rx同时通
        /// 模块1: VOA=09ch1, SW=11 in1→out2
        /// 模块2: VOA=09ch2, SW=11 in3→out4
        /// 模块3: VOA=10ch1, SW=12 in1→out2
        /// 模块4: VOA=10ch2, SW=12 in3→out4
        /// </summary>
        public static ChannelConfig GetChannelConfig(int channel)
        {
            switch (channel)
            {
                case 1: return new ChannelConfig { VoaSlot = "09", VoaCh = 1, SwSlot = "11", SwIn = 1, SwOut = 2 };
                case 2: return new ChannelConfig { VoaSlot = "09", VoaCh = 2, SwSlot = "11", SwIn = 1, SwOut = 2 };
                case 3: return new ChannelConfig { VoaSlot = "10", VoaCh = 1, SwSlot = "12", SwIn = 1, SwOut = 2 };
                case 4: return new ChannelConfig { VoaSlot = "10", VoaCh = 2, SwSlot = "12", SwIn = 1, SwOut = 2 };
                default: throw new ArgumentException("无效通道: " + channel);
            }
        }
        #region 

        /// <summary>
        /// 解析SCPI返回的dBm数值（支持科学计数、带单位格式）
        /// </summary>
        static float ParseDbm(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return float.NaN;
            string[] parts = raw.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (float.TryParse(parts[0],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float v))
                return v;
            return float.NaN;
        }

        /// <summary>
        /// 测量指定通道的接收端实际衰减
        /// 流程: 切光开关 → 配VOA(POWer/ON/设功率) → 等稳定 → 读功率 → 计算衰减
        /// 返回 (OutPower, Attenuation, InPower)
        /// </summary>
        public static (float OutPower, float Attenuation, float InPower) MeasureAttenuation(
            OTP12Driver drv, int channel, double targetPowerDbm)
        {
            var cfg = GetChannelConfig(channel);

            // 切光开关
            SwitchToChannel(drv, channel);

            // 配置VOA: POWer模式, 开输出, 设目标功率
            drv.VOA_SetMode(cfg.VoaCh, "POWer");
            drv.VOA_SetOutputState(cfg.VoaCh, "ON");
            drv.VOA_SetOutPower(cfg.VoaCh, targetPowerDbm);

            // 等待功率稳定
            Thread.Sleep(2000);

            // 读取实际功率
            string outStr = drv.VOA_GetOutputPower(cfg.VoaCh);
            string inStr = drv.VOA_GetInputPower(cfg.VoaCh);

            float outPwr = ParseDbm(outStr);
            float inPwr = ParseDbm(inStr);
            float att = float.IsNaN(inPwr) || float.IsNaN(outPwr) ? float.NaN : inPwr - outPwr;

            return (outPwr, att, inPwr);
        }
        //RxAutoCheckStup 接收设置自动检查 端口1
        private bool RxAutoCheckStup()
        {
            float tarpwr = 0, rxpwr = 0, err = 0;
            float att = 0;
            //double TargetPower = 0;

            //check1
            tarpwr = TestResult.rxPwrReal[0];
            var result = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
            att = result.Attenuation;
            TestSet.rxPwr_Cal[0] = result.OutPower;
            rxpwr = TestSet.rxPwr_Cal[0];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_Att1.Text = att.ToString("F1");
            DOA.rxCalAtt[0] = att;
            DOA.rxCheckAtt[0] = att;

            //check2
            tarpwr = TestResult.rxPwrReal[1];
            var result2 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
            att = result2.Attenuation;
            TestSet.rxPwr_Cal[1] = result2.OutPower;
            rxpwr = TestSet.rxPwr_Cal[1];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_Att2.Text = att.ToString("F1");
            DOA.rxCalAtt[1] = att;
            DOA.rxCheckAtt[1] = att;

            //check3
            tarpwr = TestResult.rxPwrReal[2];
            var result3 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
            att = result3.Attenuation;
            TestSet.rxPwr_Cal[2] = result3.OutPower;
            rxpwr = TestSet.rxPwr_Cal[2];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_Att3.Text = att.ToString("F1");
            DOA.rxCalAtt[2] = att;
            DOA.rxCheckAtt[2] = att;

            if (radioButton_APD.Checked)
            {
                //check4
                tarpwr = TestResult.rxPwrReal[3];
                var result4 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
                att = result4.Attenuation;
                TestSet.rxPwr_Cal[3] = result4.OutPower;
                rxpwr = TestSet.rxPwr_Cal[3];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox_Att4.Text = att.ToString("F1");
                DOA.rxCalAtt[3] = att;
                DOA.rxCheckAtt[3] = att;

                //check5
                tarpwr = TestResult.rxPwrReal[4];
                var result5 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
                att = result5.Attenuation;
                TestSet.rxPwr_Cal[4] = result5.OutPower;
                rxpwr = TestSet.rxPwr_Cal[4];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox_Att5.Text = att.ToString("F1");
                DOA.rxCalAtt[5] = att;
                DOA.rxCheckAtt[5] = att;
            }

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            //if (radioButton_PIN.Checked)
            //{
            //    if (DOA.rxCalAtt[2] > 2)
            //    {
            //        DOA.rxCalAtt[1] = DOA.rxCalAtt[2] - 1;
            //        GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA.rxCalAtt[1]);
            //        TestSet.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}


            //overload
            tarpwr = TestResult.rxOverLoad;
            var result6 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
            att = result6.Attenuation;
            rxpwr =  result6.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_overLoadAtt.Text = att.ToString("F1");
            DOA.rxOverLoadAtt = att;

            //sen
            tarpwr = TestResult.rxSen;
            var result7 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
            att = result7.Attenuation;
            rxpwr = result7.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_SenAtt.Text = att.ToString("F1");
            DOA.rxSenAtt = att;

            //D_los
            tarpwr = TestResult.rxDLos;
            var result8 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
            att = result8.Attenuation;
            rxpwr = result8.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_DLosAtt.Text = att.ToString("F1");
            DOA.rxDLosAtt = att;

            //A_los
            tarpwr = TestResult.rxALos;
            var result9 = MeasureAttenuation(TestControl.otp12, 1, tarpwr);
            att = result9.Attenuation;
            rxpwr = result9.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox_ALosAtt.Text = att.ToString("F1");
            DOA.rxALosAtt = att;

            GlobalVarFun.testDataIsOK1 = true;

            return true;
        }

        #region //RxAutoCheckStup 接收设置自动检查 端口2
        private bool RxAutoCheckStup2()
        {
            float tarpwr = 0, rxpwr = 0, err = 0;
            float att = 0;
            //double TargetPower = 0;

            //check1
            tarpwr = TestResult2.rxPwrReal[0];
            var result = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
            att = result.Attenuation;
            TestSet2.rxPwr_Cal[0] = result.OutPower;
            rxpwr = TestSet2.rxPwr_Cal[0];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox2_Att1.Text = att.ToString("F1");
            DOA2.rxCalAtt[0] = att;
            DOA2.rxCheckAtt[0] = att;

            //check2
            tarpwr = TestResult2.rxPwrReal[1];
            var result2 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
            att = result2.Attenuation;
            TestSet2.rxPwr_Cal[1] = result2.OutPower;
            rxpwr = TestSet2.rxPwr_Cal[1];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox2_Att2.Text = att.ToString("F1");
            DOA2.rxCalAtt[1] = att;
            DOA2.rxCheckAtt[1] = att;

            //check3
            tarpwr = TestResult2.rxPwrReal[2];
            var result3 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
            att = result3.Attenuation;
            TestSet2.rxPwr_Cal[2] = result3.OutPower;
            rxpwr = TestSet2.rxPwr_Cal[2];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox2_Att3.Text = att.ToString("F1");
            DOA2.rxCalAtt[2] = att;
            DOA2.rxCheckAtt[2] = att;

            if (radioButton_APD.Checked)
            {
                //check4
                tarpwr = TestResult2.rxPwrReal[3];
                var result4 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
                att = result4.Attenuation;
                TestSet2.rxPwr_Cal[3] = result4.OutPower;
                rxpwr = TestSet2.rxPwr_Cal[3];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox2_Att4.Text = att.ToString("F1");
                DOA2.rxCalAtt[3] = att;
                DOA2.rxCheckAtt[3] = att;

                //check5
                tarpwr = TestResult2.rxPwrReal[4];
                var result5 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
                att = result5.Attenuation;
                TestSet2.rxPwr_Cal[4] = result5.OutPower;
                rxpwr = TestSet2.rxPwr_Cal[4];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox2_Att5.Text = att.ToString("F1");
                DOA2.rxCalAtt[5] = att;
                DOA2.rxCheckAtt[5] = att;
            }

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            //if (radioButton_PIN.Checked)
            //{
            //    if (DOA2.rxCalAtt[2] > 2)
            //    {
            //        DOA2.rxCalAtt[1] = DOA2.rxCalAtt[2] - 1;
            //        GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA2.rxCalAtt[1]);
            //        TestSet2.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}


            //overload
            tarpwr = TestResult2.rxOverLoad;
            var result6 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
            att = result6.Attenuation;
            rxpwr = result6.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox2_overLoadAtt.Text = att.ToString("F1");
            DOA2.rxOverLoadAtt = att;

            //sen
            tarpwr = TestResult2.rxSen;
            var result7 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
            att = result7.Attenuation;
            rxpwr = result7.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox2_SenAtt.Text = att.ToString("F1");
            DOA2.rxSenAtt = att;

            //D_los
            tarpwr = TestResult2.rxDLos;
            var result8 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
            att = result8.Attenuation;
            rxpwr = result8.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox2_DLosAtt.Text = att.ToString("F1");
            DOA2.rxDLosAtt = att;

            //A_los
            tarpwr = TestResult2.rxALos;
            var result9 = MeasureAttenuation(TestControl.otp12, 2, tarpwr);
            att = result9.Attenuation;
            rxpwr = result9.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox2_ALosAtt.Text = att.ToString("F1");
            DOA2.rxALosAtt = att;

            GlobalVarFun.testDataIsOK2 = true;

            return true;
        }
        private bool RxAutoCheckStup3()
        {
            float tarpwr = 0, rxpwr = 0, err = 0;
            float att = 0;
            //double TargetPower = 0;

            //check1
            tarpwr = TestResult3.rxPwrReal[0];
            var result = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
            att = result.Attenuation;
            TestSet3.rxPwr_Cal[0] = result.OutPower;
            rxpwr = TestSet3.rxPwr_Cal[0];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox3_Att1.Text = att.ToString("F1");
            DOA3.rxCalAtt[0] = att;
            DOA3.rxCheckAtt[0] = att;

            //check2
            tarpwr = TestResult3.rxPwrReal[1];
            var result2 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
            att = result2.Attenuation;
            TestSet3.rxPwr_Cal[1] = result2.OutPower;
            rxpwr = TestSet3.rxPwr_Cal[1];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox3_Att2.Text = att.ToString("F1");
            DOA3.rxCalAtt[1] = att;
            DOA3.rxCheckAtt[1] = att;

            //check3
            tarpwr = TestResult3.rxPwrReal[2];
            var result3 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
            att = result3.Attenuation;
            TestSet3.rxPwr_Cal[2] = result3.OutPower;
            rxpwr = TestSet3.rxPwr_Cal[2];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox3_Att3.Text = att.ToString("F1");
            DOA3.rxCalAtt[2] = att;
            DOA3.rxCheckAtt[2] = att;

            if (radioButton_APD.Checked)
            {
                //check4
                tarpwr = TestResult3.rxPwrReal[3];
                var result4 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
                att = result4.Attenuation;
                TestSet3.rxPwr_Cal[3] = result4.OutPower;
                rxpwr = TestSet3.rxPwr_Cal[3];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox3_Att4.Text = att.ToString("F1");
                DOA3.rxCalAtt[3] = att;
                DOA3.rxCheckAtt[3] = att;

                //check5
                tarpwr = TestResult3.rxPwrReal[4];
                var result5 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
                att = result5.Attenuation;
                TestSet3.rxPwr_Cal[4] = result5.OutPower;
                rxpwr = TestSet3.rxPwr_Cal[4];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox3_Att5.Text = att.ToString("F1");
                DOA3.rxCalAtt[5] = att;
                DOA3.rxCheckAtt[5] = att;
            }

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            //if (radioButton_PIN.Checked)
            //{
            //    if (DOA3.rxCalAtt[2] > 2)
            //    {
            //        DOA3.rxCalAtt[1] = DOA3.rxCalAtt[2] - 1;
            //        GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA3.rxCalAtt[1]);
            //        TestSet3.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}


            //overload
            tarpwr = TestResult3.rxOverLoad;
            var result6 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
            att = result6.Attenuation;
            rxpwr = result6.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox3_overLoadAtt.Text = att.ToString("F1");
            DOA3.rxOverLoadAtt = att;

            //sen
            tarpwr = TestResult3.rxSen;
            var result7 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
            att = result7.Attenuation;
            rxpwr = result7.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox3_SenAtt.Text = att.ToString("F1");
            DOA3.rxSenAtt = att;

            //D_los
            tarpwr = TestResult3.rxDLos;
            var result8 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
            att = result8.Attenuation;
            rxpwr = result8.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox3_DLosAtt.Text = att.ToString("F1");
            DOA3.rxDLosAtt = att;

            //A_los
            tarpwr = TestResult3.rxALos;
            var result9 = MeasureAttenuation(TestControl.otp12, 3, tarpwr);
            att = result9.Attenuation;
            rxpwr = result9.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox3_ALosAtt.Text = att.ToString("F1");
            DOA3.rxALosAtt = att;

            GlobalVarFun.testDataIsOK3 = true;

            return true;
        }

        private bool RxAutoCheckStup4()
        {
            float tarpwr = 0, rxpwr = 0, err = 0;
            float att = 0;
            //double TargetPower = 0;

            //check1
            tarpwr = TestResult4.rxPwrReal[0];
            var result = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
            att = result.Attenuation;
            TestSet4.rxPwr_Cal[0] = result.OutPower;
            rxpwr = TestSet4.rxPwr_Cal[0];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox4_Att1.Text = att.ToString("F1");
            DOA4.rxCalAtt[0] = att;
            DOA4.rxCheckAtt[0] = att;

            //check2
            tarpwr = TestResult4.rxPwrReal[1];
            var result2 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
            att = result2.Attenuation;
            TestSet4.rxPwr_Cal[1] = result2.OutPower;
            rxpwr = TestSet4.rxPwr_Cal[1];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox4_Att2.Text = att.ToString("F1");
            DOA4.rxCalAtt[1] = att;
            DOA4.rxCheckAtt[1] = att;

            //check3
            tarpwr = TestResult4.rxPwrReal[2];
            var result3 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
            att = result3.Attenuation;
            TestSet4.rxPwr_Cal[2] = result3.OutPower;
            rxpwr = TestSet4.rxPwr_Cal[2];
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox4_Att3.Text = att.ToString("F1");
            DOA4.rxCalAtt[2] = att;
            DOA4.rxCheckAtt[2] = att;

            if (radioButton_APD.Checked)
            {
                //check4
                tarpwr = TestResult4.rxPwrReal[3];
                var result4 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
                att = result4.Attenuation;
                TestSet4.rxPwr_Cal[3] = result4.OutPower;
                rxpwr = TestSet4.rxPwr_Cal[3];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox4_Att4.Text = att.ToString("F1");
                DOA4.rxCalAtt[3] = att;
                DOA4.rxCheckAtt[3] = att;

                //check5
                tarpwr = TestResult4.rxPwrReal[4];
                var result5 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
                att = result5.Attenuation;
                TestSet4.rxPwr_Cal[4] = result5.OutPower;
                rxpwr = TestSet4.rxPwr_Cal[4];
                err = tarpwr - rxpwr;
                if (Math.Abs(err) > 0.2) return false;
                textBox4_Att5.Text = att.ToString("F1");
                DOA4.rxCalAtt[5] = att;
                DOA4.rxCheckAtt[5] = att;
            }

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            //if (radioButton_PIN.Checked)
            //{
            //    if (DOA4.rxCalAtt[2] > 2)
            //    {
            //        DOA4.rxCalAtt[1] = DOA4.rxCalAtt[2] - 1;
            //        GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(DOA4.rxCalAtt[1]);
            //        TestSet4.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}


            //overload
            tarpwr = TestResult4.rxOverLoad;
            var result6 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
            att = result6.Attenuation;
            rxpwr = result6.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox4_overLoadAtt.Text = att.ToString("F1");
            DOA4.rxOverLoadAtt = att;

            //sen
            tarpwr = TestResult4.rxSen;
            var result7 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
            att = result7.Attenuation;
            rxpwr = result7.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox4_SenAtt.Text = att.ToString("F1");
            DOA4.rxSenAtt = att;

            //D_los
            tarpwr = TestResult4.rxDLos;
            var result8 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
            att = result8.Attenuation;
            rxpwr = result8.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox4_DLosAtt.Text = att.ToString("F1");
            DOA4.rxDLosAtt = att;

            //A_los
            tarpwr = TestResult4.rxALos;
            var result9 = MeasureAttenuation(TestControl.otp12, 4, tarpwr);
            att = result9.Attenuation;
            rxpwr = result9.OutPower;
            err = tarpwr - rxpwr;
            if (Math.Abs(err) > 0.2) return false;
            textBox4_ALosAtt.Text = att.ToString("F1");
            DOA4.rxALosAtt = att;

            GlobalVarFun.testDataIsOK4 = true;

            return true;
        }
        #endregion  
        //自动设置衰减值
        private float AutoSetDOA(float tarval)
        {
            float att = 10;
            float rxpwr = 0;
            int looptime = 0;
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
                GlobalVarFun.mycontrol_dut1.opticaldoaatt.SetAttenuation(att);
                rxpwr = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_a,GlobalVarFun.setup.meter_delay);
                pwrerr = rxpwr - tarval;
                looptime++;

            } while ((looptime < 15) && (att >= 0) && (Math.Abs(pwrerr) > 0.1));

            return att;
        }

        private float AutoSetDOA2(float tarval)
        {
            float att = 10;
            float rxpwr = 0;
            int looptime = 0;
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
                
                GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(att);
                rxpwr = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
                pwrerr = rxpwr - tarval;
                looptime++;

            } while ((looptime < 15) && (att >= 0) && (Math.Abs(pwrerr) > 0.1));
            
            return att;
        }
        #endregion

        private void btnAutoCheck_Click(object sender, EventArgs e)
        {
            if (TestResult.fibertop_pn == "")
            {
                MessageBox.Show("请选择型号");
                return;
            }
            btnAutoCheck.BackColor = SystemColors.Control;
            testDataCheck_button.BackColor = SystemColors.Control;
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
            //自动RxCheck设置
            if (RxAutoCheckStup() == true)
            {
                btnAutoCheck.BackColor = Color.Green;
                testDataCheck_button.BackColor = Color.GreenYellow;
                GlobalVarFun.testDataIsOK1 = true;
                Thread.Sleep(100);
            }
            else
            {
                btnAutoCheck.BackColor = Color.Red;
                testDataCheck_button.BackColor = Color.Yellow;
                GlobalVarFun.testDataIsOK1 = false; ;
            }
        }
        #endregion

        #region//测试精度
        private void erCalNumericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void txCalNumericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void rxCalNumericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void wLengthCalnumericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        #endregion

        private async void SetupFrm_Load(object sender, EventArgs e)
        {
            string[] strType = new string[300];
            int len = 0;
            moduletype_comboBox.Items.Clear();
            meterType_comboBox.SelectedIndex = 1;
            //设计模式 return
            if (this.DesignMode)
            {
                return;
            }

            if (GlobalVarFun.mycontrol_dut1.GetModuleTypeFromAccessdb(ref strType, ref len))
            {
                if (GlobalVarFun.pnselect != "")
                {
                    for (int i = 0; i < len; i++)
                    {
                        if(strType[i].ToString().Contains(GlobalVarFun.pnselect))
                        moduletype_comboBox.Items.Add(strType[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < len; i++)
                    {
                        moduletype_comboBox.Items.Add(strType[i]);
                    }
                }
            }

            if (moduletype_comboBox.Items.Count > 0)
            {
                if (TestSet.setupUI_ok == false)
                {
                    moduletype_comboBox.SelectedIndex = -1;
                }
                else
                {
                    moduletype_comboBox.SelectedIndex = GlobalVarFun.type_index;
                }
            }
            else
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("初始化模块型号列表失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Failed to initialize the module model list. Procedure!\r\n Program exit", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            try
            {
                string[] portnames = SerialPort.GetPortNames();
                Array.Sort(portnames); //已存在串口更新
                
                

                // 调试参数 范围设置 //2017.11.30
                ///////////////////////////////////////////////////////////////////
                if (GlobalVarFun.moduleType == "XFP")
                {
                    erCalNumericUpDown.Value = (decimal)0.3;
                }
                else if (GlobalVarFun.moduleType == "SFP+")
                {
                    erCalNumericUpDown.Value = (decimal)0.3;
                    //
                    txpe_numericUpDown.Value = 72; //2017.8.21
                }
                else if (GlobalVarFun.moduleType == "SFP-MCU")
                {
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else if (GlobalVarFun.moduleType == "SFP-GN25L95")
                {
                    rxlosMax_numericUpDown.Maximum = 127; //LOS 最大设置范围
                                                          //
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else if (GlobalVarFun.moduleType == "SFP-GN25L96")
                {
                    rxlosMax_numericUpDown.Maximum = 127; //LOS 最大设置范围//
                                                          //
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else if (GlobalVarFun.moduleType == "SFP-UX3320C")
                {
                    //rxlosMax_numericUpDown.Maximum = 255; //LOS 最大设置范围
                    //
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else if (GlobalVarFun.moduleType == "SFP-UX3320T")
                {
                    //rxlosMax_numericUpDown.Maximum = 255; //LOS 最大设置范围
                    //
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else if (GlobalVarFun.moduleType == "SFPP-GN1196")
                {
                    rxlosMax_numericUpDown.Maximum = 63; //LOS 最大设置范围//
                                                         //
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else if (GlobalVarFun.moduleType == "SFPP-UX3261S")
                {
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else if (GlobalVarFun.moduleType == "SFPP-UX2270+2072")
                {
                    erCalNumericUpDown.Value = (decimal)0.5;
                }
                else //未用模块类型
                {
                    erCalNumericUpDown.Value = (decimal)0.3;
                }

                if (GlobalVarFun.moduleType == "XFP")
                {
                    rxAPDmin_numericUpDown.Minimum = 0;
                    rxAPDmin_numericUpDown.Maximum = 4095;
                    rxAPDmax_numericUpDown.Minimum = 0;
                    rxAPDmax_numericUpDown.Maximum = 4095;
                }
               
                ///////////////////////////////////////////////////////////////////
            }
            catch
            { }

            
            string str = "";
            str = filesini.INIRead("Photoswit", "COM");
            if (str != "")
            {
               // SwitchCom_comboBox.Text = str;
            }
            str = filesini.INIRead("OpticalAttenuator", "COM");
            if (str != "")
            {
                //attCom_comboBox.Text = str;
            }
            //str = filesini.INIRead("OpticalAttenuator2", "COM");
            //if (str != "")
            //{
            //    attCom_comboBox2.Text = str;
            //}
            str = filesini.INIRead("DAC", "COM");
            if (str != "")
            {
                // SwitchCom_comboBox.Text = str;
            }
            str = filesini.INIRead("OpticalPowerMeter", "COM");
            if (str != "")
            {
                //meterCom_comboBox.Text = str;
            }
            str = filesini.INIRead("MS9710B", "COM");
            if (str != "")
            {
                 //cbBMS9710B.Text = str;
            }
            str = filesini.INIRead("GE0164BCDR", "COM");
            if (str != "")
            {
                //cbBE3632A.Text = str;
            }
            str = filesini.INIRead("Keysight86120C", "COM");
            if (str != "")
            {
                //cbBGPIBWLength.Text = str;
            }
            str = filesini.INIRead("PSSBERT", "COM");
            if (str != "")
            {
                //PSSCom_comboBox.Text = str;
            }

            SetupInit();

            // List<string> devlist = new List<string>(portnames.Length);
            // 异步获取设备列表
            var timeoutDuration = TimeSpan.FromSeconds(5); // 设置 5 秒超时
           
        }

        #region//获取GPIB
        private List<string> GetGpibDevices()
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return deviceList;
        }

        // 定义一个异步方法来获取 GPIB 设备
        private async Task<List<string>> GetGpibDevicesAsync(TimeSpan timeout)
        {
            List<string> deviceList = new List<string>();

            // 创建一个取消令牌源，用于控制超时
            using (var cts = new CancellationTokenSource(timeout))
            {
                try
                {
                    // 将耗时的 FindRsrc 操作放到后台线程执行
                    string[] resources = await Task.Run(() =>
                    {
                        ResourceManager rm = null;
                        try
                        {
                            rm = new ResourceManager();
                            // 执行可能阻塞的操作
                            return rm.FindRsrc("GPIB?*INSTR");
                        }
                        finally
                        {
                            // 确保 ResourceManager 在操作完成后被释放
                            // 如果 ResourceManager 实现了 IDisposable，请调用它
                            // if (rm != null) rm.Dispose();
                        }
                    }, cts.Token); // 将取消令牌传递给 Task.Run

                    // 如果操作成功完成，处理结果
                    if (resources != null) // FindRsrc 返回 null 在某些情况下是可能的
                    {
                        foreach (string resource in resources)
                        {
                            if (resource.Contains("GPIB") && resource.EndsWith("INSTR"))
                            {
                                deviceList.Add(resource);
                            }
                        }
                       // MessageBox.Show($"Found {deviceList.Count} GPIB devices.");
                    }
                }
                catch (OperationCanceledException)
                {
                    // 当超时发生时，会抛出 OperationCanceledException
                   // MessageBox.Show($"Timeout ({timeout.TotalSeconds}s) reached while trying to find GPIB devices. The operation was cancelled.");
                    // 可以在这里添加更多逻辑，比如通知用户或重试策略
                }
                catch (AggregateException ex) when (ex.InnerExceptions.Any(e => e is OperationCanceledException))
                {
                    // 如果 Task.Run 内部抛出异常并包装在 AggregateException 中
                   // MessageBox.Show($"Task was cancelled due to timeout.");
                }
                catch //(Exception ex)
                {
                    // 捕获 FindRsrc 本身或其他地方可能发生的非超时异常
                   // MessageBox.Show($"An error occurred while trying to find GPIB devices: {ex.GetType().Name}: {ex.Message}");
                   // MessageBox.Show($"Stack Trace: {ex.StackTrace}");
                }
            }

            return deviceList;
        }

        //
        private List<string> GetGpibDevicesSync(TimeSpan timeout)
        {
            // 使用 .Result 会阻塞当前线程，直到任务完成或超时
            // 这仍然比直接调用 FindRsrc 更好，因为它有一个上限
            return GetGpibDevicesAsync(timeout).Result;
        }
        #endregion

        #region//关闭设置界面，界面数据加载记录
        private void SetupFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                TestSet.txapc_Min = Convert.ToUInt16(txapcMin_numericUpDown.Text);
                TestSet.txapc_Max = Convert.ToUInt16(txapcMax_numericUpDown.Text);
                TestSet.txmod_Min = Convert.ToUInt16(txmodMin_numericUpDown.Text);
                TestSet.txmod_Max = Convert.ToUInt16(txmodMax_numericUpDown.Text);
                TestSet.rxlos_Min = Convert.ToUInt16(rxlosMin_numericUpDown.Text);
                TestSet.rxlos_Max = Convert.ToUInt16(rxlosMax_numericUpDown.Text);
                TestSet.rxapd_min = Convert.ToUInt16(rxAPDmin_numericUpDown.Text);
                TestSet.rxapd_max = Convert.ToUInt16(rxAPDmax_numericUpDown.Text);
                TestSet.tosatemp_min = Convert.ToUInt16(TosaTempmin_numericUpDown.Text);
                TestSet.tosatemp_max = Convert.ToUInt16(TosaTempmax_numericUpDown.Text);
                TestSet.von_min = Convert.ToUInt16(VONmin_numericUpDown.Text);
                TestSet.von_max = Convert.ToUInt16(VONmax_numericUpDown.Text);
                TestSet.txcpa_Min = Convert.ToUInt16(CrossingMin_numericUpDown.Text);
                TestSet.txcpa_Max = Convert.ToUInt16(CrossingMax_numericUpDown.Text);
                TestSet.spectralwidth_max = Convert.ToDouble(spec_numericUpDown.Text);

                TestSet2.txapc_Min = Convert.ToUInt16(txapcMin_numericUpDown.Text);
                TestSet2.txapc_Max = Convert.ToUInt16(txapcMax_numericUpDown.Text);
                TestSet2.txmod_Min = Convert.ToUInt16(txmodMin_numericUpDown.Text);
                TestSet2.txmod_Max = Convert.ToUInt16(txmodMax_numericUpDown.Text);
                TestSet2.rxlos_Min = Convert.ToUInt16(rxlosMin_numericUpDown.Text);
                TestSet2.rxlos_Max = Convert.ToUInt16(rxlosMax_numericUpDown.Text);
                TestSet2.rxapd_min = Convert.ToUInt16(rxAPDmin_numericUpDown.Text);
                TestSet2.rxapd_max = Convert.ToUInt16(rxAPDmax_numericUpDown.Text);
                TestSet2.tosatemp_min = Convert.ToUInt16(TosaTempmin_numericUpDown.Text);
                TestSet2.tosatemp_max = Convert.ToUInt16(TosaTempmax_numericUpDown.Text);
                TestSet2.von_min = Convert.ToUInt16(VONmin_numericUpDown.Text);
                TestSet2.von_max = Convert.ToUInt16(VONmax_numericUpDown.Text);
                TestSet2.txcpa_Min = Convert.ToUInt16(CrossingMin_numericUpDown.Text);
                TestSet2.txcpa_Max = Convert.ToUInt16(CrossingMax_numericUpDown.Text);
                TestSet2.spectralwidth_max = Convert.ToDouble(spec_numericUpDown.Text);
             
                TestSet2.rxpwr_cal = (float)(rxCalNumericUpDown.Value);
                TestSet2.txpwr_cal = (float)(txCalNumericUpDown.Value);
                TestSet2.txer_cal = (float)(erCalNumericUpDown.Value);
                GlobalVarFun.setup.wlgth_cal = (float)(wLengthCalnumericUpDown.Value);

                if (cBEMLTest.Checked)
                {
                    if (tBTargetWlength.Text != "")
                    {
                        TestSet.wLength_target = Convert.ToDouble(tBTargetWlength.Text.Trim());
                        TestSet2.wLength_target = Convert.ToDouble(tBTargetWlength.Text.Trim());
                    }
                    else
                    {
                        return;
                    }
                }
                TestSet.rxpwr_cal = (float)(rxCalNumericUpDown.Value);
                TestSet.txpwr_cal = (float)(txCalNumericUpDown.Value);
                TestSet.txer_cal = (float)(erCalNumericUpDown.Value);

                TestSet2.rxpwr_cal = (float)(rxCalNumericUpDown.Value);
                TestSet2.txpwr_cal = (float)(txCalNumericUpDown.Value);
                TestSet2.txer_cal = (float)(erCalNumericUpDown.Value);

                GlobalVarFun.setup.wlgth_cal = (float)(wLengthCalnumericUpDown.Value);

                TestSet.meter_pwr_err = (float)(optoErr_numericUpDown.Value);
                TestSet2.meter_pwr_err = (float)(optoErr2_numericUpDown.Value);
                TestSet3.meter_pwr_err = (float)(optoErr3_numericUpDown.Value);
                TestSet4.meter_pwr_err = (float)(optoErr4_numericUpDown.Value);

                TestResult.txpeVal = (Byte)(txpe_numericUpDown.Value); //2017.8.21
                TestResult2.txpeVal = TestResult.txpeVal;

                TestResult.waveforms_count = Convert.ToInt32(waveforms_numericUpDown.Value);
                TestResult2.waveforms_count = Convert.ToInt32(waveforms_numericUpDown.Value);

                if (GlobalVarFun.moduleType == "SFP+")
                {
                    GlobalVarFun.txpwr_debug_method = 0x11;
                    if (checkBox_TOSA_NoMPD.Checked)
                    {
                        // 0x00:线性计算法 apc-->uw & bias   0x11: 普通二分法 apc-->dBm   22:差值二分法 apc-->uW 33:差值二分法 apc-->uW ,0.6倍bias, ER 二次调试
                        GlobalVarFun.txpwr_debug_method = 0x11;
                    }
                    if (cB_25G_Algorithm.Checked)
                    {
                        GlobalVarFun.txpwr_debug_method = 0x33;
                    }
                }

                GlobalVarFun.setup.rx_test = checkBox_rxTest.Checked;
                GlobalVarFun.setup.rx_nopwr_test = checkBox_RxNoPwr.Checked;
                GlobalVarFun.setup.tx_test = checkBox_txTest.Checked;
                GlobalVarFun.setup.tx_nopwr_test = checkBox_TxNoPwr.Checked;
                GlobalVarFun.setup.tx_hardware_disable = cBHardwareTxDis.Checked;
                GlobalVarFun.setup.image_save = checkBox_EyeSave.Checked;
                GlobalVarFun.setup.threshold_check = checkBox_AlarmThresholds.Checked;
                GlobalVarFun.setup.flash_check = checkBox_debugTest.Checked;
                GlobalVarFun.setup.rx_sen_test = cBSenTest.Checked;

                GlobalVarFun.setup.tx_jitter_test = checkBox_txJt.Checked;
                GlobalVarFun.setup.tx_eml_test = cBEMLTest.Checked;
                GlobalVarFun.setup.algorithm_25g_lr = cB_25G_Algorithm.Checked;

                GlobalVarFun.setup.algorithm_cob_ld = checkBox_TOSA_NoMPD.Checked;
                GlobalVarFun.setup.rx_hardware_los = cBHardwareLOS.Checked;
                GlobalVarFun.setup.rx_apd_test = checkBox_APD.Checked;
                GlobalVarFun.setup.electrical_module = cBelec_moudle.Checked;
                GlobalVarFun.setup.init_module = checkBox_Init.Checked;
                GlobalVarFun.setup.tx_rx_cdr_dis = checkBox_DisCDR.Checked;
                GlobalVarFun.setup.scheme_check_dis = checkBox_DisTypeCheck.Checked;

                
            }
            catch
            {
                MessageBox.Show("参数异常，请重新检查参数");
            }
        }

        #endregion

       

        #region//模块插拔次数重置
        private bool writeMyFileTxt(string filename, string mess)
        {
            string filepath = "C:\\" + filename + ".txt";
            try
            {
                File.WriteAllText(filepath, string.Empty);

                using (StreamWriter writer = new StreamWriter(filepath, true, Encoding.UTF8))
                {
                    //writer.WriteLine(mess);
                    writer.Write(mess);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void btnNumReset_Click(object sender, EventArgs e)
        {
            if (GlobalVarFun.Language == "Chinese")
            {
                if (MessageBox.Show("你确定要重置插拔次数？\r\n注意：需确认更换测试板座子后执行此重置操作。", "提示", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }
            }
            else
            {
                if (MessageBox.Show("Are you sure you want to reset the number of insertions and removals? \r\n Note: This reset operation must be performed after confirming the replacement of the test board socket.", "Hint", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }
            }
            writeMyFileTxt("TestNum", "0");
            TestResult.testnum = 0;
        }
        #endregion

        
        

        private void button1_Click(object sender, EventArgs e)
        {
           MessageBox.Show(TestControl.opticalmeter.ReadPower(1,100).ToString());

        }

        

        private void testDataCheck2_button_Click(object sender, EventArgs e)
        {
            float err = 0;
            float range = 0.2f;

            testDataCheck2_button.BackColor = System.Drawing.Color.Gray;

            if ((GlobalVarFun.setup.meter_connect == false) || (GlobalVarFun.setup.doa_connect2 == false)) // 连接光功率计和光衰减器判断
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("请先连接光功率计和光衰减器！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Please connect the optical power meter and attenuator first! Please confirm!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            TestResult2.rxPwrReal[0] = Convert.ToSingle(rxPwr2textBox1.Text);
            TestResult2.rxPwrReal[1] = Convert.ToSingle(rxPwr2textBox2.Text);
            TestResult2.rxPwrReal[2] = Convert.ToSingle(rxPwr2textBox3.Text);
            TestResult2.rxPwrReal[3] = Convert.ToSingle(rxPwr2textBox4.Text);
            TestResult2.rxPwrReal[4] = Convert.ToSingle(rxPwr2textBox5.Text);

            TestResult2.rxSen = Convert.ToSingle(textBox2_Sen.Text);
            TestResult2.rxDLos = Convert.ToSingle(textBox2_DLos.Text);
            TestResult2.rxALos = Convert.ToSingle(textBox2_ALos.Text);
            TestResult2.rxOverLoad = Convert.ToSingle(textBox2_overLoad.Text);

            //
            DOA2.rxCalAtt[0] = DOA2.rxCheckAtt[0];
            DOA2.rxCalAtt[1] = DOA2.rxCheckAtt[1];
            DOA2.rxCalAtt[2] = DOA2.rxCheckAtt[2];
            DOA2.rxCalAtt[3] = DOA2.rxCheckAtt[3];
            DOA2.rxCalAtt[4] = DOA2.rxCheckAtt[4];

            GlobalVarFun.testDataIsOK2 = true;

            // RX SEN
            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxSenAtt);
            err = TestResult2.rxSen - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK2 = false;
            }

            // RX DLOS
            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxDLosAtt);
            err = TestResult2.rxDLos - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK2 = false;
            }

            // RX ALOS
            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxALosAtt);
            err = TestResult2.rxALos - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK2 = false;
            }

            // RX OVERLOAD
            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxOverLoadAtt);
            err = TestResult2.rxOverLoad - TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK2 = false;
            }

            // CHECK 
            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[0]);
            TestSet2.rxPwr_Cal[0] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
            err = TestResult.rxPwrReal[0] - TestSet2.rxPwr_Cal[0];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK2 = false;
            }

            // CHECK 
            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[1]);
            TestSet2.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
            err = TestResult.rxPwrReal[1] - TestSet2.rxPwr_Cal[1];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK2 = false;
            }

            // CHECK 
            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[2]);
            TestSet2.rxPwr_Cal[2] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
            err = TestResult.rxPwrReal[2] - TestSet2.rxPwr_Cal[2];
            if (Math.Abs(err) > range)
            {
                GlobalVarFun.testDataIsOK2 = false;
            }

            if (radioButton_APD.Checked)
            {
                // CHECK 
                GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[3]);
                TestSet2.rxPwr_Cal[3] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
                err = TestResult.rxPwrReal[3] - TestSet2.rxPwr_Cal[3];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK2 = false;
                }

                // CHECK 
                GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[4]);
                TestSet2.rxPwr_Cal[4] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
                err = TestResult.rxPwrReal[4] - TestSet2.rxPwr_Cal[4];
                if (Math.Abs(err) > range)
                {
                    GlobalVarFun.testDataIsOK2 = false;
                }
            }
            //

            // 接收DDM 校准时使用，把[1]改成[2]小 1dB
            if (radioButton_PIN.Checked)
            {
                if (DOA2.rxCalAtt[2] > 2)
                {
                    DOA2.rxCalAtt[1] = DOA2.rxCalAtt[2] - 1;
                    GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCalAtt[1]);
                    TestSet2.rxPwr_Cal[1] = TestControl.opticalmeter.ReadPower(GlobalVarFun.setup.meter_ch_b, GlobalVarFun.setup.meter_delay);
                }
                else
                {
                    GlobalVarFun.testDataIsOK2 = false;
                }
            }

            if (GlobalVarFun.testDataIsOK2 == true)
            {
                testDataCheck2_button.BackColor = System.Drawing.Color.GreenYellow;
            }
            else
            {
                testDataCheck2_button.BackColor = System.Drawing.Color.Yellow;
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("测试参数设置异常，精度为 +-0.2dB ！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Test parameter Settings are abnormal, accuracy is +-0.2dB!", "errror", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
        }

        private void btnAutoCheck2_Click(object sender, EventArgs e)
        {
            if (TestResult2.fibertop_pn == "")
            {
                MessageBox.Show("请选择型号");
                return;
            }
            btnAutoCheck2.BackColor = SystemColors.Control;
            testDataCheck2_button.BackColor = SystemColors.Control;
            Refresh();
            TestResult2.rxPwrReal[0] = Convert.ToSingle(rxPwr2textBox1.Text);
            TestResult2.rxPwrReal[1] = Convert.ToSingle(rxPwr2textBox2.Text);
            TestResult2.rxPwrReal[2] = Convert.ToSingle(rxPwr2textBox3.Text);
            TestResult2.rxPwrReal[3] = Convert.ToSingle(rxPwr2textBox4.Text);
            TestResult2.rxPwrReal[4] = Convert.ToSingle(rxPwr2textBox5.Text);

            TestResult2.rxSen = Convert.ToSingle(textBox2_Sen.Text);
            TestResult2.rxDLos = Convert.ToSingle(textBox2_DLos.Text);
            TestResult2.rxALos = Convert.ToSingle(textBox2_ALos.Text);
            TestResult2.rxOverLoad = Convert.ToSingle(textBox2_overLoad.Text);
            //自动RxCheck设置
            if (RxAutoCheckStup2() == true)
            {
                btnAutoCheck2.BackColor = Color.Green;
                testDataCheck2_button.BackColor = Color.GreenYellow;
                GlobalVarFun.testDataIsOK2 = true;
                Thread.Sleep(100);
            }
            else
            {
                btnAutoCheck2.BackColor = Color.Red;
                testDataCheck2_button.BackColor = Color.Yellow;
                GlobalVarFun.testDataIsOK2 = false;
            }
        }

        private void button2_calTest1_Click(object sender, EventArgs e)
        {
            DOA2.rxCheckAtt[0] = Convert.ToSingle(textBox2_Att1.Text);
            textBox2_Att1.Text = DOA2.rxCheckAtt[0].ToString("F1");
            DOA2.rxCheckAtt[0] = Convert.ToSingle(textBox2_Att1.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[0]);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_calTest2_Click(object sender, EventArgs e)
        {
            DOA2.rxCheckAtt[1] = Convert.ToSingle(textBox2_Att2.Text);
            textBox2_Att2.Text = DOA2.rxCheckAtt[1].ToString("F1");
            DOA2.rxCheckAtt[1] = Convert.ToSingle(textBox2_Att2.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[1]);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_calTest3_Click(object sender, EventArgs e)
        {
            DOA2.rxCheckAtt[2] = Convert.ToSingle(textBox2_Att3.Text);
            textBox2_Att3.Text = DOA2.rxCheckAtt[2].ToString("F1");
            DOA2.rxCheckAtt[2] = Convert.ToSingle(textBox2_Att3.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[2]);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_calTest4_Click(object sender, EventArgs e)
        {
            DOA2.rxCheckAtt[3] = Convert.ToSingle(textBox2_Att4.Text);
            textBox2_Att4.Text = DOA2.rxCheckAtt[3].ToString("F1");
            DOA2.rxCheckAtt[3] = Convert.ToSingle(textBox2_Att4.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[3]);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_calTest5_Click(object sender, EventArgs e)
        {
            DOA2.rxCheckAtt[4] = Convert.ToSingle(textBox2_Att5.Text);
            textBox2_Att5.Text = DOA2.rxCheckAtt[4].ToString("F1");
            DOA2.rxCheckAtt[4] = Convert.ToSingle(textBox2_Att5.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxCheckAtt[4]);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_overLoadTest_Click(object sender, EventArgs e)
        {
            DOA2.rxOverLoadAtt = Convert.ToSingle(textBox2_overLoadAtt.Text);
            textBox2_overLoadAtt.Text = DOA2.rxOverLoadAtt.ToString("F1");
            DOA2.rxOverLoadAtt = Convert.ToSingle(textBox2_overLoadAtt.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxOverLoadAtt);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_SenTest_Click(object sender, EventArgs e)
        {
            DOA2.rxSenAtt = Convert.ToSingle(textBox2_SenAtt.Text);
            textBox2_SenAtt.Text = DOA2.rxSenAtt.ToString("F1");
            DOA2.rxSenAtt = Convert.ToSingle(textBox2_SenAtt.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxSenAtt);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_DLosTest_Click(object sender, EventArgs e)
        {
            DOA2.rxDLosAtt = Convert.ToSingle(textBox2_DLosAtt.Text);
            textBox2_DLosAtt.Text = DOA2.rxDLosAtt.ToString("F1");
            DOA2.rxDLosAtt = Convert.ToSingle(textBox2_DLosAtt.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxDLosAtt);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void button2_ALosTest_Click(object sender, EventArgs e)
        {
            DOA2.rxALosAtt = Convert.ToSingle(textBox2_ALosAtt.Text);
            textBox2_ALosAtt.Text = DOA2.rxALosAtt.ToString("F1");
            DOA2.rxALosAtt = Convert.ToSingle(textBox2_ALosAtt.Text);

            GlobalVarFun.mycontrol_dut2.opticaldoaatt.SetAttenuation(DOA2.rxALosAtt);

            GlobalVarFun.testDataIsOK2 = false;
        }

        private void btnPNSelect_Click(object sender, EventArgs e)
        {
            string[] strType = new string[300];
            int len = 0;
            moduletype_comboBox.Items.Clear();
            if (GlobalVarFun.mycontrol_dut1.GetModuleTypeFromAccessdb(ref strType, ref len))
            {
                for (int i = 0; i < len; i++)
                {
                    if (strType[i].ToString().Contains(tBPnOpticType.Text.Trim()) && strType[i].ToString().Contains(tBPnwLength.Text.Trim())
                        && strType[i].ToString().Contains(tBPnRate.Text.Trim()))
                    {
                        moduletype_comboBox.Items.Add(strType[i]);
                    }
                }
            }
            GlobalVarFun.pnselect = tBPnOpticType.Text.Trim() + tBPnwLength.Text.Trim() + tBPnRate.Text.Trim();
        }

        private void btnPwrTest_Click(object sender, EventArgs e)
        {
           float pwe1 = TestControl.opticalmeter.ReadPower(1, GlobalVarFun.setup.meter_delay);
           float pwe2 = TestControl.opticalmeter.ReadPower(2, GlobalVarFun.setup.meter_delay);
           float pwe3 = TestControl.opticalmeter.ReadPower(3, GlobalVarFun.setup.meter_delay);
           float pwe4 = TestControl.opticalmeter.ReadPower(4, GlobalVarFun.setup.meter_delay);

           MessageBox.Show("延时："+ GlobalVarFun.setup.meter_delay.ToString()+" CH1:" +pwe1.ToString() + " CH2:" 
               + pwe2.ToString()+ " CH3:" + pwe3.ToString()+ " CH4:" + pwe4.ToString());
        }

        private void optoErr2_numericUpDown_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (TestResult3.fibertop_pn == "")
            {
                MessageBox.Show("请选择型号");
                return;
            }
            btnAutoCheck3.BackColor = SystemColors.Control;
            testDataCheck3_button.BackColor = SystemColors.Control;
            Refresh();
            TestResult3.rxPwrReal[0] = Convert.ToSingle(rxPwr3textBox1.Text);
            TestResult3.rxPwrReal[1] = Convert.ToSingle(rxPwr3textBox2.Text);
            TestResult3.rxPwrReal[2] = Convert.ToSingle(rxPwr3textBox3.Text);
            TestResult3.rxPwrReal[3] = Convert.ToSingle(rxPwr3textBox4.Text);
            TestResult3.rxPwrReal[4] = Convert.ToSingle(rxPwr3textBox5.Text);

            TestResult3.rxSen = Convert.ToSingle(textBox3_Sen.Text);
            TestResult3.rxDLos = Convert.ToSingle(textBox3_DLos.Text);
            TestResult3.rxALos = Convert.ToSingle(textBox3_ALos.Text);
            TestResult3.rxOverLoad = Convert.ToSingle(textBox3_overLoad.Text);
            //自动RxCheck设置
            if (RxAutoCheckStup3() == true)
            {
                btnAutoCheck3.BackColor = Color.Green;
                testDataCheck3_button.BackColor = Color.GreenYellow;
                GlobalVarFun.testDataIsOK3 = true;
                Thread.Sleep(100);
            }
            else
            {
                btnAutoCheck3.BackColor = Color.Red;
                testDataCheck3_button.BackColor = Color.Yellow;
                GlobalVarFun.testDataIsOK3 = false;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (TestResult4.fibertop_pn == "")
            {
                MessageBox.Show("请选择型号");
                return;
            }
            btnAutoCheck4.BackColor = SystemColors.Control;
            testDataCheck4_button.BackColor = SystemColors.Control;
            Refresh();
            TestResult4.rxPwrReal[0] = Convert.ToSingle(rxPwr4textBox1.Text);
            TestResult4.rxPwrReal[1] = Convert.ToSingle(rxPwr4textBox2.Text);
            TestResult4.rxPwrReal[2] = Convert.ToSingle(rxPwr4textBox3.Text);
            TestResult4.rxPwrReal[3] = Convert.ToSingle(rxPwr4textBox4.Text);
            TestResult4.rxPwrReal[4] = Convert.ToSingle(rxPwr4textBox5.Text);

            TestResult4.rxSen = Convert.ToSingle(textBox4_Sen.Text);
            TestResult4.rxDLos = Convert.ToSingle(textBox4_DLos.Text);
            TestResult4.rxALos = Convert.ToSingle(textBox4_ALos.Text);
            TestResult4.rxOverLoad = Convert.ToSingle(textBox4_overLoad.Text);
            //自动RxCheck设置
            if (RxAutoCheckStup4() == true)
            {
                btnAutoCheck4.BackColor = Color.Green;
                testDataCheck4_button.BackColor = Color.GreenYellow;
                GlobalVarFun.testDataIsOK4 = true;
                Thread.Sleep(100);
            }
            else
            {
                btnAutoCheck4.BackColor = Color.Red;
                testDataCheck4_button.BackColor = Color.Yellow;
                GlobalVarFun.testDataIsOK4 = false;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (GlobalVarFun.setup.otp12_connect == false)
                {
                    if (TestControl.otp12.Connect(textBox_otp12Ip.Text))
                    {
                        button1.BackColor = System.Drawing.Color.GreenYellow;

                        GlobalVarFun.setup.otp12_connect = true;
                        return;
                    }
                    else
                    {
                        TestControl.otp12.DisConnect();
                        GlobalVarFun.setup.otp12_connect = false;
                        button1.BackColor = System.Drawing.Color.Gray;
                    }
                }
                else
                {
                    TestControl.otp12.DisConnect();
                    GlobalVarFun.setup.otp12_connect = false;
                    button1.BackColor = System.Drawing.Color.Gray;
                }
            }
            catch
            {
                TestControl.otp12.DisConnect();
                button1.BackColor = System.Drawing.Color.Yellow;
            }
        }
    }
}
