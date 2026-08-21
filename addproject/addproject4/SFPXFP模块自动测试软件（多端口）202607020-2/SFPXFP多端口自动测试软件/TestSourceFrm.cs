using FibertopTest_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
//using WindowsFormsApp1;
using static System.Net.Mime.MediaTypeNames;

namespace SFPXFP自动测试软件多端口
{
   
    public partial class TestSourceFrm : Form
    {
        TestControl testcontrol = new TestControl(TestSet.Dut);
        public TestSourceFrm()
        {
            InitializeComponent();
            GlobalVarFun.mycontrol_dut1 = testcontrol;
            //MessageBox.Show("创建端口1");
        }
        bool autoTestCtrl = false;
        bool moduleOnline = true;
        int testnum = 0;// 插拔次数
        Stopwatch timer;
        ReturnReuslt result = new ReturnReuslt();
        private volatile bool isTestRunning = false;
        private int _tickProcessing = 0; // 定时器重入保护
        private volatile bool _formClosing = false; // 窗体关闭标志

        SimpleLogger simpleLogger = new SimpleLogger("D:\\SFPXFPTesTLogDUT1.txt");
        private CancellationTokenSource testCancellationSource;

        public void StopTimers()
        {
            _formClosing = true;
            timer1.Stop();
        }
        #region 
        //设置图片框控件
        private void SetLED(PictureBox picbox, bool bit_value)
        {
            if (bit_value)
                picbox.Image = imageList1.Images["LedRed.ico"];
            else
                picbox.Image = imageList1.Images["LedGreen.ico"];
        }
        #endregion

        #region//ShowModuleDdmInfo DM监控
        //private int _ddmReading = 0; // 重入保护：0=空闲, 1=正在读取
        private async void ShowModuleDdmInfo()
        {
            //// 重入保护：如果上一次DDM读取还没完成，直接跳过本次
            //if (Interlocked.Exchange(ref _ddmReading, 1) == 1)
            //    return;

            try
            {
                await Task.Run(() =>
                {
                    testcontrol.Read_moduleInfo_Async();
                    testcontrol.Converted_analog_values_Async();
                    testcontrol.Read_AlarmWarn_Thresholds_Async();
                });
                // await之后已回到UI线程，直接更新UI，无需BeginInvoke
                Temp_textBox.Text = (TestResult.tempDDM).ToString("F2");
                Vcc_textBox.Text = (TestResult.vccDDM).ToString("F2");
                Bias_textBox.Text = (TestResult.txBiasDDM).ToString("F2");
                TxPWR_textBox.Text = (TestResult.txPowerDDM).ToString("F2");
                RxPWR_textBox.Text = (TestResult.rxPowerDDM).ToString("F2");
            }
            finally
            {
                //Interlocked.Exchange(ref _ddmReading, 0);
            }
        }
        #endregion

        #region //模块插拔次数记录txt
        private string writeMyFileTxt(string filename, string mess)
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
                if (GlobalVarFun.Language == "Chinese")
                {
                    AddTestLog("写入TestNum失败");
                }
                else
                {
                    AddTestLog("Failed to write to TestNum");
                }
                return "写入txt失败";
            }
            return "写入txt成功";
        }
        #endregion

        #region//AddTestLog
        private void AddTestLog(string strMessage)
        {
            if (strMessage.Trim() == "")
            {
                return;
            }
            result.message = strMessage;
            GlobalVarFun.mycontrol_dut1.ModListBoxShow(this,result);
            
            /*
            if (testLog_textBox.Lines.Length > 7)
            {
                testLog_textBox.ScrollBars = ScrollBars.Vertical;
            }*/
        }
        #endregion

        #region //ClearTestLog
        private void ClearTestLog()
        {
            
        }
        #endregion

        private void start_button_Click(object sender, EventArgs e)
        {
            if (TestResult.fibertop_pn == "")
            {
                MessageBox.Show("请选择型号");
                return;
            }

            if (GlobalVarFun.setup.tx_eml_test)
            {
                if (TestSet.wLength_target == 0)
                {
                    return;
                }
               
            }

            // 判断调试参数范围设置是否正确
            if ((TestSet.txapc_Max < TestSet.txapc_Min) || (TestSet.txmod_Max < TestSet.txmod_Min) || (TestSet.rxlos_Max < TestSet.rxlos_Min))
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("APC/MOD/LOS调试范围设置错误(max<min)！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("The debugging range of APC/MOD/LOS is incorrectly set (max<min)! Please confirm!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            // 判断 Access 数据库 是否连接OK
            if (GlobalVarFun.access_connect_status == false)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("Access数据库连接失败！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Access database connection failed! Please confirm!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }
            //

            if (autoTestCtrl == false)
            {
                if ((GlobalVarFun.setup.otp12_connect == false))
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        MessageBox.Show("请先连接OTP12设备！ 请确认！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("The test parameter Settings are abnormal, the batch test cannot be started! Please check the parameter Settings first!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }
                //TestDataCheck_button_Click(sender, e);
                if (GlobalVarFun.testDataIsOK1 == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        MessageBox.Show("测试参数设置异常，无法启动批量测试！ 请先进行 参数设置校验 ！！！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("To use the sensitivity test, please connect the error meter first! Please confirm!!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }

                GlobalVarFun.sql_record_status = GlobalVarFun.sql_connect_status;//更新记录状态和SQL连接状态一致 2018.5.19

                autoTestCtrl = true;
                start_button.BackColor = Color.GreenYellow;
                if (GlobalVarFun.Language == "Chinese")
                {
                    start_button.Text = "停止批量调试";
                    Startautoset_button.Text = "自动批量测试启动，请插入模块......";
                }
                else
                {
                    start_button.Text = "Stop batch debugging";//停止批量调试
                    Startautoset_button.Text = "Automatic batch test start, please insert module......";//自动批量测试启动，请插入模块......
                }
                //
                Startautoset_button.BackColor = Color.Orange;
                //
                //SetDebugParaCtrlStatus(false);
            }
            else
            {
                autoTestCtrl = false;
                start_button.BackColor = Color.Gray;
                if (GlobalVarFun.Language == "Chinese")
                {
                    start_button.Text = "开始批量调试";
                    Startautoset_button.Text = "自动批量测试已停止......";
                }
                else
                {
                    start_button.Text = "Start batch debugging";//开始批量调试
                    Startautoset_button.Text = "Automatic batch testing has been stopped......";//自动批量测试已停止......
                }
                //
                Startautoset_button.BackColor = Color.OrangeRed;
                //
               // SetDebugParaCtrlStatus(true);
            }
        }

        protected virtual void btnSetup_Click(object sender, EventArgs e)
        {
            SetupFrm dut1setup_form = new SetupFrm();
            dut1setup_form.Text = "设置";
            timer1.Stop();
            try
            {
                dut1setup_form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (!_formClosing && !this.IsDisposed)
            {
                timer1.Start();
            }
        }

        private async void timer1_Tick(object sender, EventArgs e)
        {
            // 重入保护：如果上一次Tick还在处理，跳过本次
            if (Interlocked.Exchange(ref _tickProcessing, 1) == 1)
                return;

            // 窗体关闭中，不处理
            if (_formClosing || this.IsDisposed)
            {
                Interlocked.Exchange(ref _tickProcessing, 0);
                return;
            }

            try
            {
                // 第一步：异步读取DDM数据（I2C通信在后台线程）
                ShowModuleDdmInfo();//DDM刷新（内部已有重入保护和Task.Run）

                // 使用vccDDM判断模块是否在线（基于上次DDM读取结果）
                // 注意：这里不做阻塞等待，使用已有数据判断
                if (TestResult.vccDDM < 2.0)
                {
                    moduleOnline = false;
                    //SetLED(i2cok_pictureBox1, true);
                    //SetLED(i2cok_pictureBox2, true);
                    toolStripStatusLabel1.Text = ".......................";
                    Startautoset_button.BackColor = Color.Orange;
                    return;
                }

                // 第二步：将所有I2C检查操作放到后台线程执行，避免阻塞UI
                bool typeCheckOk = true;
                bool statusCheckOk = true;
                byte debugPwd = 0x00;

                await Task.Run(() =>
                {
                    //2021.5.29 增加模块方案检查选择
                    if (!GlobalVarFun.setup.scheme_check_dis)
                    {
                        typeCheckOk = testcontrol.CheckTestTypeInfo(); // 模块方案类型信息判断
                    }
                    else
                    {
                        TestResult.chipIsOK = true;
                    }

                    if (typeCheckOk && TestResult.fibertop_pn != "SFP-SM31TG-10DIU")
                    {
                        statusCheckOk = testcontrol.ShowCheckModuleStatus(); //显示并判断模块方案/速率/版本/工作状态等信息
                    }

                    if (autoTestCtrl && !moduleOnline)
                    {
                        debugPwd = GlobalVarFun.mycontrol_dut1.CheckDebugPWD(); // 检查模块是否插入
                    }
                });

                if (_formClosing || this.IsDisposed) return;

                //if (!typeCheckOk)
                //{
                //    //Startautoset_button.BackColor = Color.Red;
                //    //Startautoset_button.Text = "模块芯片工作状态异常Error, 无法测试 ......";
                //    return;
                //}

                if (!statusCheckOk)
                {
                    return;
                }

            // 判断自动批量调试是否启动
            if (autoTestCtrl == false)
            {
                return;
            }

                //2018.5.19  SQL数据连接并且测试记录保存出错进入异常处理
                if ((GlobalVarFun.sql_connect_status == true) && (GlobalVarFun.sql_record_status == false))
                {
                    Startautoset_button.BackColor = Color.OrangeRed;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        Startautoset_button.Text = "已测试模块参数记录保存SQL数据库异常, 请停止自动测试并检查连接 ......";
                    }
                    else
                    {
                        Startautoset_button.Text = "The SQL database fails to save parameter records of the tested module. Stop the automatic test and check the connection ......";
                    }
                    return;
                }

                if ((moduleOnline == false)) // 检测到新测试模块插入测试板
                {
                    StartNewTestSequence();
                }
            }
            catch (Exception)
            {
                // 定时器异常保护，避免崩溃
            }
            finally
            {
                Interlocked.Exchange(ref _tickProcessing, 0);
            }
        }

        private async Task RunTestSequenceAsync(CancellationToken cancellationToken)
        {
            // Capture the UI control instance where the method is defined (usually the Form itself)
            Control uiControl = this; // Or any specific UI control on your form that's always created on the UI thread

            var uiUpdateProgressForRxPwr = new Progress<ReturnTxRxResult>(result =>
            {
                // Check if we're on the UI thread before updating controls
                //if (uiControl.InvokeRequired) // This is the crucial check
                //{
                //    // If we're NOT on the UI thread, use BeginInvoke to marshal the call back to the UI thread
                //    // Using BeginInvoke for non-blocking invoke
                //    uiControl.BeginInvoke(new Action(() =>
                //    {
                //        UpdateUIControls(result);
                //    }));
                //    // Return early from this lambda, as the actual update will happen asynchronously on the UI thread
                //    return;
                //}

                // If we ARE on the UI thread, update directly
                UpdateUIControls(result);

                // Note: Removed Refresh() as it's usually unnecessary for Text property changes.
                // The controls should repaint automatically.
            });

            TestResult.Test_ok = false;
            if (GlobalVarFun.testType == "firstTest")
            {
                Task<bool> dut1first = testcontrol.FirstTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                bool firstTestResult = await dut1first;
                //bool firstTestResult = await testcontrol.FirstTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                if (firstTestResult) TestResult.Test_ok = true;
            }
            else // (GlobalVarFun.testType == "finalTest")
            {
                Task<bool> dut1final = testcontrol.FinalTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                bool finalTestResult = await dut1final;
                //bool finalTestResult = await testcontrol.FinalTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                if (finalTestResult) TestResult.Test_ok = true;
            }
        }

        // Helper method containing the actual UI update logic
        private void UpdateUIControls(ReturnTxRxResult result)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUIControls called on form: {this.GetType().Name}, Instance HashCode: {this.GetHashCode()}, Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                //Rx UI Updates
               
                //指示灯
                // 实时更新数据库连接状态
                //SetLED(sqlrecord_pictureBox, !GlobalVarFun.sql_record_status);
                //SetLED(sqlconnt_pictureBox, !GlobalVarFun.sql_connect_status);
                //SetLED(accessconnt_pictureBox, !GlobalVarFun.access_connect_status);
                //SetLED(accessupdated_pictureBox, !GlobalVarFun.access_updated_status);

               // SetLED(i2cok_pictureBox1, false);
               // SetLED(i2cok_pictureBox2, false);

                //SetLED(typeok_pictureBox1, true);
                //SetLED(sr850_pictureBox1, TestResult.moduleIsSR);
                //SetLED(chipok_pictureBox1, TestResult.chipIsOK);
                //sr850_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                //chipok_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                //SetLED(typeok_pictureBox1, false);

                //Tx UI Updates
                progressBar1.Value = result.Percentage;

                Startautoset_button.BackColor = (Color)result.TestLogColor;
                if (result.StatusText != null)
                {
                    Startautoset_button.Text = result.StatusText.ToString();
                }
               // testLog_textBox.Text = result.ErrorMessage;
                if (TestResult.Test_ok)
                {
                    Startautoset_button.BackColor = Color.Green;
                    //ddm_rxpower1_textbox.Text = result.RxddmPowers[0].ToString("F2");
                    //real_rxpower1_textbox.Text = result.RxRealPowers[0].ToString("F2");
                    //ddm_rxpower2_textbox.Text = result.RxddmPowers[1].ToString("F2");
                    //real_rxpower2_textbox.Text = result.RxRealPowers[1].ToString("F2");
                    //ddm_rxpower3_textbox.Text = result.RxddmPowers[2].ToString("F2");
                    //real_rxpower3_textbox.Text = result.RxRealPowers[2].ToString("F2");

                    if (GlobalVarFun.setup.rx_apd_cal)
                    {
                        //ddm_rxpower4_textbox.Text = "";
                        //real_rxpower4_textbox.Text = "";
                        //ddm_rxpower5_textbox.Text = "";
                        //real_rxpower5_textbox.Text = "";

                        //ddm_rxpower4_textbox.Text = result.RxddmPowers[3].ToString("F2"); // Ensure index 4 exists
                        //real_rxpower4_textbox.Text = result.RxRealPowers[3].ToString("F2"); // Ensure index 4 exists
                        //ddm_rxpower5_textbox.Text = result.RxddmPowers[4].ToString("F2"); // Ensure index 5 exists
                        //real_rxpower5_textbox.Text = result.RxRealPowers[4].ToString("F2"); // Ensure index 5 exists
                    }
                    if (GlobalVarFun.setup.tx_test)
                    {
                        //最大最小范围
                        //txpwr_min_textBox.Text = TestSet.txPwr_Min.ToString();
                        //txpwr_max_textBox.Text = TestSet.txPwr_Max.ToString();

                        //bias_min_textBox.Text = TestSet.bias_Min.ToString();
                        //bias_max_textBox.Text = TestSet.bias_Max.ToString();

                        //er_min_textBox.Text = TestSet.txEr_Min.ToString();
                        //er_max_textBox.Text = TestSet.txEr_Max.ToString();

                        //txCr_min_textBox.Text = TestSet.txCr_Min.ToString();
                        //txCr_max_textBox.Text = TestSet.txCr_Max.ToString();

                        //txJt_min_textBox.Text = TestSet.txJt_Max.ToString();
                        //txJt_max_textBox.Text = "--";

                        //txpower_textBox.Text = "";
                        //Bias_textBox_cal.Text = "";
                        //er_textBox.Text = "";
                        //txCr_textBox.Text = "";
                        //txJt_textBox.Text = "";

                        //lbApc.Text = "val";
                        //lbMod.Text = "val";

                        //txpower_textBox.Text = result.TxpwrResultShow;
                        //Bias_textBox_cal.Text = result.TxBiasResultShow;
                        //er_textBox.Text = result.TxerResultShow;
                        //txCr_textBox.Text = result.TxCrResultShow;
                        //txJt_textBox.Text = result.TxJtResultShow;

                        //lbApc.Text = result.apc.ToString();
                        //lbMod.Text = result.mod.ToString();

                    }
                }
                if (TestResult.Test_ok == false)
                {
                    if (TestResult.test_status == 3)
                    {
                        Startautoset_button.BackColor = Color.Red;
                        //testLog_textBox.Text = result.ErrorMessage;

                        //ddm_rxpower1_textbox.Text = result.RxddmPowers[0].ToString("F2");
                        //real_rxpower1_textbox.Text = result.RxRealPowers[0].ToString("F2");
                        //ddm_rxpower2_textbox.Text = result.RxddmPowers[1].ToString("F2");
                        //real_rxpower2_textbox.Text = result.RxRealPowers[1].ToString("F2");
                        //ddm_rxpower3_textbox.Text = result.RxddmPowers[2].ToString("F2");
                        //real_rxpower3_textbox.Text = result.RxRealPowers[2].ToString("F2");

                        if (GlobalVarFun.setup.rx_apd_cal)
                        {
                            //ddm_rxpower4_textbox.Text = "";
                            //real_rxpower4_textbox.Text = "";
                            //ddm_rxpower5_textbox.Text = "";
                            //real_rxpower5_textbox.Text = "";

                            //ddm_rxpower4_textbox.Text = result.RxddmPowers[3].ToString("F2"); // Ensure index 4 exists
                            //real_rxpower4_textbox.Text = result.RxRealPowers[3].ToString("F2"); // Ensure index 4 exists
                            //ddm_rxpower5_textbox.Text = result.RxddmPowers[4].ToString("F2"); // Ensure index 5 exists
                            //real_rxpower5_textbox.Text = result.RxRealPowers[4].ToString("F2"); // Ensure index 5 exists

                            //lbLos.Text = result.los.ToString();
                        }
                        if (GlobalVarFun.setup.tx_test)
                        {
                            //最大最小范围
                            //txpwr_min_textBox.Text = TestSet.txPwr_Min.ToString();
                            //txpwr_max_textBox.Text = TestSet.txPwr_Max.ToString();

                            //bias_min_textBox.Text = TestSet.bias_Min.ToString();
                            //bias_max_textBox.Text = TestSet.bias_Max.ToString();

                            //er_min_textBox.Text = TestSet.txEr_Min.ToString();
                            //er_max_textBox.Text = TestSet.txEr_Max.ToString();

                            //txCr_min_textBox.Text = TestSet.txCr_Min.ToString();
                            //txCr_max_textBox.Text = TestSet.txCr_Max.ToString();

                            //txJt_min_textBox.Text = TestSet.txJt_Max.ToString();
                            //txJt_max_textBox.Text = "--";

                            //txpower_textBox.Text = "";
                            //Bias_textBox_cal.Text = "";
                            //er_textBox.Text = "";
                            //txCr_textBox.Text = "";
                            //txJt_textBox.Text = "";

                            //lbApc.Text = "val";
                            //lbMod.Text = "val";

                            //txpower_textBox.Text = result.TxpwrResultShow;
                            //Bias_textBox_cal.Text = result.TxBiasResultShow;
                            //er_textBox.Text = result.TxerResultShow;
                            //txCr_textBox.Text = result.TxCrResultShow;
                            //txJt_textBox.Text = result.TxJtResultShow;

                            //lbApc.Text = result.apc.ToString();
                            //lbMod.Text = result.mod.ToString();

                        }
                    }
                }
                if (TestResult.test_status == 1)
                {
                    Startautoset_button.BackColor = Color.White;
                }
                if (TestResult.test_status == 0)
                {
                    Startautoset_button.BackColor = Color.Orange;
                }
                if (TestResult.test_status == 2)
                {
                    Startautoset_button.BackColor = Color.Green;
                }

            }));
            // Optional: Call Refresh if needed for some custom drawing or layout issues not handled by Text changes
            // this.Refresh(); // Uncomment only if absolutely necessary after Text changes.
        }

        private async void StartNewTestSequence()
        {
            if (isTestRunning)
            {
                return;
            }

            isTestRunning = true; // Mark test as running

            timer.Reset();
            timer.Start();
            //
            //
            progressBar1.Value = 0;
            Startautoset_button.BackColor = Color.Honeydew;
            TestResult.test_status = 1;//开始测试
            if (GlobalVarFun.Language == "Chinese")
            {
                Startautoset_button.Text = "已检测到模块插入, 请不要插拔模块......";
            }
            else
            {
                Startautoset_button.Text = "Module insertion has been detected. Do not remove or insert the module......";
            }
            simpleLogger.FileDelete();
            simpleLogger = new SimpleLogger("D:\\SFPXFPTesTLogDUT1.txt");
            moduleOnline = true;

            pnshow_textBox.Text = TestResult.fibertop_pn;
            //SetLED(i2cok_pictureBox1, false);
            //SetLED(i2cok_pictureBox2, false);

            Update();

            // 模块DDM温度判断 0~40
            if ((GlobalVarFun.testType != "firstTest") || ((GlobalVarFun.moduleType != "SFPP-GN1196") && (GlobalVarFun.moduleType != "SFP-GN25L95") && (GlobalVarFun.moduleType != "SFP-GN25L96")
                && (GlobalVarFun.moduleType != "SFP-UX3320C") && (GlobalVarFun.moduleType != "SFP-UX3320T") && (GlobalVarFun.moduleType != "SFPP-UX3261S") && (GlobalVarFun.moduleType != "SFPP-UX2270+2072")))
            {
                if ((TestResult.tempDDM > 40) || (TestResult.tempDDM < 0))
                {
                    Startautoset_button.BackColor = Color.OrangeRed;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        Startautoset_button.Text = "待测模块检测DDM温度: 大于40度或者小于0度, 温度异常, 无法测试 ......";
                    }
                    else
                    {
                        Startautoset_button.Text = "The DDM temperature detected by the module to be tested: If the temperature is greater than 40 ° C or less than 0 ° C, the DDM temperature is abnormal and cannot be tested ......";
                    }
                    return;
                }
            }
            // 模块DDM电压判断 3.15~3.45V  //终测
            if (GlobalVarFun.testType != "firstTest")
            {
                if ((TestResult.vccDDM > 3.45) || (TestResult.vccDDM < 3.15))
                {
                    Startautoset_button.BackColor = Color.OrangeRed;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        Startautoset_button.Text = "待测模块检测DDM电压: 大于3.45V 或者 小于3.15V, 电压异常, 无法测试 ......";
                    }
                    else
                    {
                        Startautoset_button.Text = "The module to be tested detects DDM voltage: If the voltage is greater than 3.45V or less than 3.15V, the DDM voltage is abnormal and cannot be tested ......";
                    }
                    return;
                }
            }
            
            GlobalVarFun.record_need_save = false;
            
            Startautoset_button.BackColor = Color.Honeydew;
            if (GlobalVarFun.Language == "Chinese")
            {
                Startautoset_button.Text = GlobalVarFun.moduleType.ToString() + "：模块正在自动测试中，请等待......";
            }
            else
            {
                Startautoset_button.Text = GlobalVarFun.moduleType.ToString() + "：The module is being tested automatically, please wait......";
            }
            progressBar1.Value = 5;
            Update();
            //
            ///////////////////////////////////////////////////////////////////////////////////////////////
            ReturnTxRxResult returnTxRxResult = new ReturnTxRxResult();
            //testLog_textBox.ForeColor = Color.Red;
            testCancellationSource = new CancellationTokenSource();

            try
            {
                //调用核心测试逻辑的异步方法
                await RunTestSequenceAsync(testCancellationSource.Token);
                ShowModuleDdmInfo();//DDM刷新
            }
            catch (OperationCanceledException)
            {
                // 测试被取消
                Startautoset_button.Text = "测试已被取消。";
                Startautoset_button.BackColor = Color.Yellow;
            }
            finally
            {
                timer.Stop();
                string str = timer.Elapsed.ToString();
                //str = str.Substring(6, 5);
                str = str.Substring(3, 7);
                if (GlobalVarFun.Language == "Chinese")
                {
                    label_testtime.Text = "测试时间: " + str + "s";
                }
                else
                {
                    label_testtime.Text = "Test Time: " + str + "s";//测试时间
                }

                //
                testnum++;
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    writeMyFileTxt("TestNum", testnum.ToString());//模块插拔次数记录更新
                }));
                if (testnum > 2000)
                {
                    AddTestLog("测试次数超过2000，请及时更换测试板座子");
                }
                //tBNum.Text = testnum.ToString();
                Update();
                isTestRunning = false; // Mark test as finished
            }
        }
        private void TestSourceFrm_Load(object sender, EventArgs e)
        {
            GlobalVarFun.mycontrol_dut1.ModListBoxShow = EventListBoxShow;
            // 开定时器
            timer1.Start();
            timer = new Stopwatch();
            readMyFileTxt("TestNum2");//读取插拔模块次数记录
            //tBNum.Text = testnum.ToString();
            if (GlobalVarFun.testType == "firstTest")
            {
                button1_testType.Text = "初测.调试";
            }
            else
            {
                button1_testType.Text = "终测.检查";
            }
            // 测试SQL数据连接情况
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (testcontrol.sqlserver != null && !GlobalVarFun.sqlserver_ip.Contains("null"))
            {
                try
                {
                    //sqlconnection.Open();
                    testcontrol.sqlserver.ServersOpen();
                    GlobalVarFun.sql_connect_status = true; // 数据库连接正常
                }
                catch (Exception exp)
                {
                    GlobalVarFun.sql_connect_status = false; // 数据库连接失败
                    MessageBox.Show(exp.Message);
                }
                finally
                {
                    testcontrol.sqlserver.ServersClose();
                }
            }
            //TestResult.tester_no = textBoxTester.Text;
            GlobalVarFun.sql_record_status_2 = GlobalVarFun.sql_connect_status_2;//更新记录状态和SQL连接状态一致 2018.5.19
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
        public void readMyFileTxt(string filename)
        {
            string filepath = "C:\\" + filename + ".txt";
            if (File.Exists(filepath))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(filepath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            testnum = Convert.ToInt16(line);
                        }
                    }
                }
                catch
                {
                    //
                }
            }
            else
            {
                testnum = 0;
            }
        }
        private void EventListBoxShow(object sender, ReturnReuslt reuslt)
        {
            if (simpleLogger != null)
            {
                simpleLogger.LogInfo(reuslt.message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            testcontrol.sqlserver.SaveRecordToSQL(TestSet.Dut);
        }

        private void textBoxTester_TextChanged(object sender, EventArgs e)
        {
            //TestResult2.tester_no = textBoxTester.Text;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            float pwe1 = TestControl.opticalmeter.ReadPower(1, GlobalVarFun.setup.meter_delay);
            float pwe2 = TestControl.opticalmeter.ReadPower(2, GlobalVarFun.setup.meter_delay);
            float pwe3 = TestControl.opticalmeter.ReadPower(3, GlobalVarFun.setup.meter_delay);
            float pwe4 = TestControl.opticalmeter.ReadPower(4, GlobalVarFun.setup.meter_delay);

            MessageBox.Show("延时：" + GlobalVarFun.setup.meter_delay.ToString() + " CH1:" + pwe1.ToString() + " CH2:"
                + pwe2.ToString() + " CH3:" + pwe3.ToString() + " CH4:" + pwe4.ToString());
        }
    }
}
