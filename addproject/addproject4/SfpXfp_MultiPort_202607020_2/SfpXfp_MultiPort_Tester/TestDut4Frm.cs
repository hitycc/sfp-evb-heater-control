using FibertopTest_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SFPXFP自动测试软件多端口
{
    public partial class TestDut4Frm : Form
    {
        TestControl testcontrol4 = new TestControl(TestSet4.Dut);
        public TestDut4Frm()
        {
            InitializeComponent();
            GlobalVarFun.mycontrol_dut4 = testcontrol4;
            //MessageBox.Show("创建端口4");
            GlobalVarFun.mycontrol_dut4.ModListBoxShow = EventListBoxShow;
        }

        bool autoTestCtrl = false;
        bool moduleOnline = false;
        //int testnum = 0;// 插拔次数
        Stopwatch timer;
        ReturnReuslt result = new ReturnReuslt();
        private volatile bool isTestRunning = false;
        private int _tickProcessing = 0; // 定时器重入保护
        private volatile bool _formClosing = false; // 窗体关闭标志

        SimpleLogger simpleLogger4 = new SimpleLogger("D:\\SFPXFPTesTLogDUT4.txt");
        private CancellationTokenSource testCancellationSource;

        public void StopTimers()
        {
            _formClosing = true;
            timer1.Stop();
        }

        private void SetLED(PictureBox picbox, bool bit_value)
        {
            if (bit_value)
                picbox.Image = imageList1.Images["LedRed.ico"];
            else
                picbox.Image = imageList1.Images["LedGreen.ico"];
        }

        private void EventListBoxShow(object sender, ReturnReuslt reuslt)
        {
            if (simpleLogger4 != null)
            {
                simpleLogger4.LogInfo(reuslt.message);
            }
        }

        private int _ddmReading4 = 0; // 重入保护
        private async void ShowModuleDdmInfo()
        {
            // 重入保护：如果上一次DDM读取还没完成，直接跳过本次，防止请求堆积
            if (Interlocked.Exchange(ref _ddmReading4, 1) == 1)
                return;

            try
            {
                // 将阻塞性I2C硬件读取移到后台线程执行，不阻塞UI线程
                await Task.Run(() =>
                {
                    testcontrol4.Read_moduleInfo_Async();
                    testcontrol4.Converted_analog_values_Async();
                    testcontrol4.Read_AlarmWarn_Thresholds_Async();
                });
                // await之后自动回到UI线程，直接更新UI，无需BeginInvoke
                Temp_textBox.Text = (TestResult4.tempDDM).ToString("F2");
                Vcc_textBox.Text = (TestResult4.vccDDM).ToString("F2");
                Bias_textBox.Text = (TestResult4.txBiasDDM).ToString("F2");
                TxPWR_textBox.Text = (TestResult4.txPowerDDM).ToString("F2");
                RxPWR_textBox.Text = (TestResult4.rxPowerDDM).ToString("F2");
            }
            finally
            {
                Interlocked.Exchange(ref _ddmReading4, 0);
            }
        }

        private void TestDut4Frm_Load(object sender, EventArgs e)
        {
            // GlobalVarFun.mycontrol_dut2.ModListBoxShow = EventListBoxShow;
            // 开定时器
            timer1.Start();
            timer = new Stopwatch();

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
            if (testcontrol4.sqlserver != null && !GlobalVarFun.sqlserver_ip.Contains("null"))
            {
                try
                {
                    //sqlconnection.Open();
                    testcontrol4.sqlserver.ServersOpen();
                    GlobalVarFun.sql_connect_status_4 = true; // 数据库连接正常
                }
                catch (Exception exp)
                {
                    GlobalVarFun.sql_connect_status_4 = false; // 数据库连接失败
                    MessageBox.Show(exp.Message);
                }
                finally
                {
                    testcontrol4.sqlserver.ServersClose();
                }
            }
            GlobalVarFun.sql_record_status_4 = GlobalVarFun.sql_connect_status_4;//更新记录状态和SQL连接状态一致 2018.5.19
            //TestResult4.tester_no = textBoxTester.Text;
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            if (TestResult4.fibertop_pn == "")
            {
                MessageBox.Show("请选择型号");
                return;
            }

            if (GlobalVarFun.setup.tx_eml_test)
            {
                if (TestSet4.wLength_target == 0)
                {
                    return;
                }
            }
            TestResult4.fibertop_pn = TestResult.fibertop_pn;

            // 判断调试参数范围设置是否正确
            if ((TestSet4.txapc_Max < TestSet4.txapc_Min) || (TestSet4.txmod_Max < TestSet4.txmod_Min) || (TestSet4.rxlos_Max < TestSet4.rxlos_Min))
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
                if (GlobalVarFun.testDataIsOK4 == false)
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

                GlobalVarFun.sql_record_status_4 = GlobalVarFun.sql_connect_status_4;//更新记录状态和SQL连接状态一致 2018.5.19
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

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref _tickProcessing, 1) == 1)
                return;

            if (_formClosing || this.IsDisposed)
            {
                Interlocked.Exchange(ref _tickProcessing, 0);
                return;
            }

            try
            {
                // 将所有I2C操作（DDM读取 + 检查）合并到一个Task.Run中，避免竞态
                bool typeCheckOk = true;
                bool statusCheckOk = true;
                byte debugPwd = 0x00;

                await Task.Run(() =>
                {
                    // DDM读取
                    testcontrol4.Read_moduleInfo_Async();
                    testcontrol4.Converted_analog_values_Async();
                    testcontrol4.Read_AlarmWarn_Thresholds_Async();

                    // 使用vccDDM判断模块是否在线
                    if (TestResult4.vccDDM < 2.0)
                    {
                        return; // moduleOnline默认false，让外层判断return
                    }

                    if (!GlobalVarFun.setup.scheme_check_dis)
                    {
                        typeCheckOk = testcontrol4.CheckTestTypeInfo();
                    }
                    else
                    {
                        TestResult4.chipIsOK = true;
                    }

                    if (typeCheckOk && TestResult4.fibertop_pn != "SFP-SM31TG-10DIU")
                    {
                        statusCheckOk = testcontrol4.ShowCheckModuleStatus();
                    }

                    if (autoTestCtrl && !moduleOnline)
                    {
                        debugPwd = GlobalVarFun.mycontrol_dut4.CheckDebugPWD();
                    }
                });

                // await之后已回到UI线程，更新DDM显示
                Temp_textBox.Text = (TestResult4.tempDDM).ToString("F2");
                Vcc_textBox.Text = (TestResult4.vccDDM).ToString("F2");
                Bias_textBox.Text = (TestResult4.txBiasDDM).ToString("F2");
                TxPWR_textBox.Text = (TestResult4.txPowerDDM).ToString("F2");
                RxPWR_textBox.Text = (TestResult4.rxPowerDDM).ToString("F2");

                // Task.Run返回后，在UI线程判断模块是否在线
                if (TestResult4.vccDDM < 2.0)
                {
                    moduleOnline = false;
                    SetLED(i2cok_pictureBox1, true);
                    SetLED(i2cok_pictureBox2, true);
                    toolStripStatusLabel1.Text = "用做测试，端口4";
                    Startautoset_button.BackColor = Color.Orange;
                    return;
                }

                if (_formClosing || this.IsDisposed) return;
                //if (!typeCheckOk) return;
                if (!statusCheckOk) return;
                if (autoTestCtrl == false) return;

                if ((GlobalVarFun.sql_connect_status_4 == true) && (GlobalVarFun.sql_record_status_4 == false))
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

                if (moduleOnline == false)
                {
                    TestResult4.test_status = 1;
                    StartNewTestSequence();
                }
            }
            catch (Exception ex)
            {
                // 定时器异常保护，避免崩溃
                System.Diagnostics.Debug.WriteLine($"timer1_Tick DUT4 error: {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange(ref _tickProcessing, 0);
            }
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
            //ClearTestLog();
            //
            progressBar1.Value = 0;
            Startautoset_button.BackColor = Color.Honeydew;
            if (GlobalVarFun.Language == "Chinese")
            {
                Startautoset_button.Text = "已检测到模块插入, 请不要插拔模块......";
            }
            else
            {
                Startautoset_button.Text = "Module insertion has been detected. Do not remove or insert the module......";
            }
            TestResult4.test_status = 1;//开始测试
            //pnshow_textBox.Text = TestResult2.fibertop_pn;
            SetLED(i2cok_pictureBox1, false);
            SetLED(i2cok_pictureBox2, false);

            simpleLogger4.FileDelete();
            simpleLogger4 = new SimpleLogger("D:\\SFPXFPTesTLogDUT4.txt");
            moduleOnline = true;
            Update();
            // 模块DDM温度判断 0~40
            if ((GlobalVarFun.testType != "firstTest") || ((GlobalVarFun.moduleType != "SFPP-GN1196") && (GlobalVarFun.moduleType != "SFP-GN25L95") && (GlobalVarFun.moduleType != "SFP-GN25L96")
                && (GlobalVarFun.moduleType != "SFP-UX3320C") && (GlobalVarFun.moduleType != "SFP-UX3320T") && (GlobalVarFun.moduleType != "SFPP-UX3261S") && (GlobalVarFun.moduleType != "SFPP-UX2270+2072")))
            {
                if ((TestResult4.tempDDM > 40) || (TestResult4.tempDDM < 0))
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
                if ((TestResult4.vccDDM > 3.45) || (TestResult4.vccDDM < 3.15))
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

                //testnum++;
                //this.BeginInvoke(new MethodInvoker(() =>
                //{
                //    writeMyFileTxt("TestNum2", testnum.ToString());//模块插拔次数记录更新
                //}));
                //tBNum.Text = testnum.ToString();
                //if (testnum > 2000)
                //{
                //    AddTestLog("测试次数超过2000，请及时更换测试板座子");
                //}
                Update();
                isTestRunning = false; // Mark test as finished
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

            TestResult4.Test_ok = false;
            TestResult4.test_status = 1;
            if (GlobalVarFun.testType == "firstTest")
            {
                Task<bool> dut4first = testcontrol4.FirstTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                bool firstTestResult = await dut4first;
                //bool firstTestResult = await  testcontrol2.FirstTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                if (firstTestResult)
                {
                    TestResult4.Test_ok = true;
                    TestResult4.test_status = 2;
                }
                else
                {
                    TestResult4.test_status = 3;
                }
            }
            else // (GlobalVarFun.testType == "finalTest")
            {
                Task<bool> dut4final = testcontrol4.FinalTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                bool finalTestResult = await dut4final;
                //bool finalTestResult = await testcontrol2.FinalTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                if (finalTestResult)
                {
                    TestResult4.Test_ok = true;
                    TestResult4.test_status = 2;
                }
                else
                {
                    TestResult4.test_status = 3;
                }
            }
        }

        // Helper method containing the actual UI update logic
        private void UpdateUIControls(ReturnTxRxResult result)
        {
            //AddTestLog("界面更新 ：UpdateUIControls");
            this.BeginInvoke(new MethodInvoker(() =>
            {
                System.Diagnostics.Debug.WriteLine($"UpdateUIControls called on form: {this.GetType().Name}, Instance HashCode: {this.GetHashCode()}, Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                //Rx UI Updates
                //指示灯
                // 实时更新数据库连接状态
                SetLED(sqlrecord_pictureBox, !GlobalVarFun.sql_record_status_4);
                SetLED(sqlconnt_pictureBox, !GlobalVarFun.sql_connect_status_4);
                SetLED(accessconnt_pictureBox, !GlobalVarFun.access_connect_status_4);
                SetLED(accessupdated_pictureBox, !GlobalVarFun.access_updated_status_4);

                SetLED(i2cok_pictureBox1, false);
                SetLED(i2cok_pictureBox2, false);

                //SetLED(typeok_pictureBox1, true);

                //SetLED(sr850_pictureBox1, TestResult2.moduleIsSR);
                //SetLED(chipok_pictureBox1, TestResult2.chipIsOK);
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
                if (TestResult4.Test_ok)
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
                        // Note: Original code accessed index [4] then [5], but initial updates were [0], [1], [2].
                        // Assuming [3] was skipped intentionally or is an error in original indexing here too.
                        // If [3] exists in result.RxddmPowers and result.RxRealPowers, it should be updated here or earlier.
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

                        //txpower_textBox.Text = result.TxpwrResultShow;
                        //Bias_textBox_cal.Text = result.TxBiasResultShow;
                        //er_textBox.Text = result.TxerResultShow;
                        //txCr_textBox.Text = result.TxCrResultShow;
                        //txJt_textBox.Text = result.TxJtResultShow;

                        //lbApc.Text = result.apc.ToString();
                        //lbMod.Text = result.mod.ToString();
                    }
                }
                if (TestResult4.Test_ok == false)
                {
                    if (TestResult4.test_status == 3)
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
                            // Note: Original code accessed index [4] then [5], but initial updates were [0], [1], [2].
                            // Assuming [3] was skipped intentionally or is an error in original indexing here too.
                            // If [3] exists in result.RxddmPowers and result.RxRealPowers, it should be updated here or earlier.
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
                if (TestResult4.test_status == 1)
                {
                    Startautoset_button.BackColor = Color.White;
                }
                if (TestResult4.test_status == 2)
                {
                    Startautoset_button.BackColor = Color.Green;
                }
                if (TestResult4.test_status == 0)
                {
                    Startautoset_button.BackColor = Color.Orange;
                }
            }));
            // Optional: Call Refresh if needed for some custom drawing or layout issues not handled by Text changes
            // this.Refresh(); // Uncomment only if absolutely necessary after Text changes.
        }

        
    }
}
