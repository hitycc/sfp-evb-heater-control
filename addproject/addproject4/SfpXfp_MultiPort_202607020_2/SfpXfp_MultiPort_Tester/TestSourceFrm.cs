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
        bool moduleOnline = false;
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
        private int _ddmReading = 0; // 重入保护：0=空闲, 1=正在读取
        private async void ShowModuleDdmInfo()
        {
            // 重入保护：如果上一次DDM读取还没完成，直接跳过本次
            if (Interlocked.Exchange(ref _ddmReading, 1) == 1)
                return;

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
                Interlocked.Exchange(ref _ddmReading, 0);
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
            GlobalVarFun.mycontrol_dut1.ModListBoxShow(this, result);

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

            // 如果窗体正在关闭或已经销毁，立即返回
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
                    testcontrol.Read_moduleInfo_Async(); //读取模块基本信息（型号、厂商、序列号等，存在A0地址低128字节）
                    testcontrol.Converted_analog_values_Async(); //读取并转换模拟量（温度、电压、偏置电流、发射功率、接收功率）
                    testcontrol.Read_AlarmWarn_Thresholds_Async();//读取告警/警告阈值

                    // 使用vccDDM判断模块是否在线
                    if (TestResult.vccDDM < 2.0)
                    {
                        return; // moduleOnline默认false，让外层判断return mdgb 模块未插或未上电，电压低于2.0V认为不在线
                    }

                    //2021.5.29 增加模块方案检查选择 如果配置中没有禁用"方案检查"，就调用 `CheckTestTypeInfo()` 判断模块芯片方案是否与测试程序匹配（比如是不是正确的芯片型号）
                    if (!GlobalVarFun.setup.scheme_check_dis)
                    {
                        typeCheckOk = testcontrol.CheckTestTypeInfo(); // 模块方案类型信息判断
                    }
                    else
                    {
                        //如果用户在设置中禁用了方案检查，则直接认为芯片OK
                        TestResult.chipIsOK = true;
                    }

                    if (typeCheckOk && TestResult.fibertop_pn != "SFP-SM31TG-10DIU")
                    {
                        statusCheckOk = testcontrol.ShowCheckModuleStatus(); //显示并判断模块方案/速率/版本/工作状态等信息
                    }
                    //当自动测试已开启（`autoTestCtrl=true`）但模块尚未标记为在线（`moduleOnline=false`）时，
                    //检查模块调试密码，确认模块已正确插入
                    if (autoTestCtrl && !moduleOnline)
                    {
                        debugPwd = GlobalVarFun.mycontrol_dut1.CheckDebugPWD(); // 检查模块是否插入
                    }
                });

                // await之后已回到UI线程，更新DDM显示
                //把刚才后台读到的 DDM 数据显示到界面文本框上
                //（温度、电压、偏置电流、发光功率、收光功率），保留2位小数。
                Temp_textBox.Text = (TestResult.tempDDM).ToString("F2");
                Vcc_textBox.Text = (TestResult.vccDDM).ToString("F2");
                Bias_textBox.Text = (TestResult.txBiasDDM).ToString("F2");
                TxPWR_textBox.Text = (TestResult.txPowerDDM).ToString("F2");
                RxPWR_textBox.Text = (TestResult.rxPowerDDM).ToString("F2");

                // Task.Run返回后，在UI线程判断模块是否在线
                //电压过低说明模块离线，更新界面：I2C状态灯变红、
                //状态栏显示占位符、开始按钮变橙色（等待状态
                if (TestResult.vccDDM < 2.0)
                {
                    moduleOnline = false;
                    SetLED(i2cok_pictureBox1, true);
                    SetLED(i2cok_pictureBox2, true);
                    toolStripStatusLabel1.Text = ".......................";
                    Startautoset_button.BackColor = Color.Orange;
                    return;
                }
                // 如果窗体正在关闭或已经销毁，立即返回
                if (_formClosing || this.IsDisposed) return;

                /* if (!typeCheckOk)
                 {
                     //Startautoset_button.BackColor = Color.Red;
                     //Startautoset_button.Text = "模块芯片工作状态异常Error, 无法测试 ......";
                     return;
                 }*/

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
                //如果SQL数据库连接正常但记录保存失败，按钮变红并提示，阻止继续测试
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
                //当自动测试模式开启（`autoTestCtrl==true`），
                //且之前模块是离线状态（`moduleOnline==false`），
                //但这一次读到了有效数据（走到了这里，说明VCC>=2.0），
                //说明检测到新模块插入__，调用 `StartNewTestSequence()` 启动新一轮自动测试
                //这就是"自动批量测试"的触发机制——不需要手动点按钮，插上模块就自动开始测试
                if (moduleOnline == false) // 检测到新测试模块插入测试板
                {
                    TestResult.test_status = 1;
                    StartNewTestSequence();
                }
            }
            //捕获所有异常，只在调试输出中打印，__不让定时器崩溃__（否则整个程序就停止响应了）
            catch (Exception ex)
            {
                // 定时器异常保护，避免崩溃
                //调试日志（Debug输出）
                System.Diagnostics.Debug.WriteLine($"timer1_Tick error: {ex.Message}");
            }
            //无论成功还是异常，都把 `_tickProcessing` 复位为0，确保下次 Tick 能正常执行
            finally
            {
                Interlocked.Exchange(ref _tickProcessing, 0);
            }
        }

        //标记为异步方法，内部可以使用await
        //异步返回类型，调用方用 await 等待测试完成
        //取消令牌，用于支持中途取消测试（比如用户点了"停止"按钮）
        private async Task RunTestSequenceAsync(CancellationToken cancellationToken)
        {
            //this 指当前窗体（TestSourceFrm 本身，是一个 Form，继承自 Control）
            //保存一个在 UI 线程上创建的控件引用，目的是为了跨线程更新 UI 时做 `InvokeRequired` 判断。
            // Capture the UI control instance where the method is defined (usually the Form itself)
            Control uiControl = this; // Or any specific UI control on your form that's always created on the UI thread
            //这是 C# 中标准的异步进度报告模式
            //Progress<ReturnTxRxResult>：一个泛型进度报告类，
            //ReturnTxRxResult是自定义的数据结构，包含发射/接收光功率等实时测试数据
            //创建进度报告对象（IProgress<T> 模式）
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
                //构造函数里的 lambda result => { UpdateUIControls(result); }：
                //定义了"收到进度更新时要做什么" 调用 `UpdateUIControls`
                //更新界面上的数据显示（比如实时显示光功率值）
                //关键机制：Progress<T>在创建时自动捕获当前的同步上下文（也就是 UI 线程的上下文）。
                //所以不管后台测试线程在什么线程调用 `progress.Report(result)`，这个 lambda 都会自动回到 UI 线程执行，
                //不会出现跨线程操作UI的异常。
                UpdateUIControls(result);
                // Note: Removed Refresh() as it's usually unnecessary for Text property changes.
                // The controls should repaint automatically.
            });
            //简单理解：这个对象就是一个"快递员"，后台线程把测试数据交给它，它负责安全地送到UI线程去更新界面。
            //全局测试结果先设为 false（失败），等测试流程返回 true 时再设为 true（通过）。
            //这是一种"默认失败，成功才标记"的防御性编程，如果中途异常或没走完，Test_ok 保持 false
            TestResult.Test_ok = false;
            if (GlobalVarFun.testType == "firstTest")
            {
                //初测流程
                Task<bool> dut1first = testcontrol.FirstTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                bool firstTestResult = await dut1first;
                //bool firstTestResult = await testcontrol.FirstTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                if (firstTestResult) TestResult.Test_ok = true;
            }
            else // (GlobalVarFun.testType == "finalTest")
            {
                // 终测流程
                Task<bool> dut1final = testcontrol.FinalTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                bool finalTestResult = await dut1final;
                //bool finalTestResult = await testcontrol.FinalTestProcessAsync(uiUpdateProgressForRxPwr as IProgress<ReturnTxRxResult>);
                if (finalTestResult) TestResult.Test_ok = true;
            }
        }

        // Helper method containing the actual UI update logic
        //参数result：ReturnTxRxResult类型，包含后台测试传来的所有实时数据
        //（进度百分比、状态文字、光功率值、测试结果颜色、错误信息等）
        private void UpdateUIControls(ReturnTxRxResult result)
        {
            //this.BeginInvoke(...)：把代码异步调度到UI线程执行。这是 WinForms 中跨线程更新UI的标准做法
            this.BeginInvoke(new MethodInvoker(() =>
            {
                //调试日志（Debug输出）
                System.Diagnostics.Debug.WriteLine($"UpdateUIControls called on form: {this.GetType().Name}, Instance HashCode: {this.GetHashCode()}, Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                //Rx UI Updates

                //指示
                // 更新4个数据库状态指示灯
                //SetLED(pictureBox, false)设为绿色（正常），SetLED(pictureBox, true)
                //设为红色（异常）。所以 `!status` 的意思是：status=true（正常）→ 传false → 绿灯；status=false（异常）→ 传true → 红灯。
                SetLED(sqlrecord_pictureBox, !GlobalVarFun.sql_record_status);//SQL数据库记录保存状态
                SetLED(sqlconnt_pictureBox, !GlobalVarFun.sql_connect_status);//SQL数据库连接状态
                SetLED(accessconnt_pictureBox, !GlobalVarFun.access_connect_status);//Access数据库连接状态
                SetLED(accessupdated_pictureBox, !GlobalVarFun.access_updated_status);//Access数据库更新状态
                //这些是I2C通信指示灯、模块类型指示灯、芯片状态指示灯的更新代码，
                //全部被注释掉了，暂时不生效。可能是因为多DUT（多通道）重构后，这些灯的逻辑移到其他地方去了。
                // SetLED(i2cok_pictureBox1, false);
                // SetLED(i2cok_pictureBox2, false);

                //SetLED(typeok_pictureBox1, true);
                //SetLED(sr850_pictureBox1, TestResult.moduleIsSR);
                //SetLED(chipok_pictureBox1, TestResult.chipIsOK);
                //sr850_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                //chipok_pictureBox1.Image = imageList1.Images["LedNone.ico"];
                //SetLED(typeok_pictureBox1, false);

                //更新进度条和按钮状态
                //更新进度条的值（0-100），后台测试在每个测试步骤完成后上报当前百分比
                progressBar1.Value = result.Percentage;

                //按钮背景色跟着测试状态变色（白色=测试中、绿色=通过、红色=失败、橙色=等待、黄色=取消等）。
                //颜色由后台测试逻辑决定，通过result.TestLogColor传过来
                Startautoset_button.BackColor = (Color)result.TestLogColor;
                if (result.StatusText != null)
                {
                    //如果测试通过了，强制把按钮设为绿色（覆盖前面由 `result.TestLogColor` 设置的颜色，确保通过时一定是绿色
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
                        //接收端APD校准相关的数据显示（DDM收光功率、实际收光功率等）
                        // 全部被注释掉了，暂时不显示
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
                        //当测试失败且测试状态为3（状态码3通常表示"测试失败/不合格"）时，按钮强制变红。
                        //原本还会在testLog_textBox显示错误信息，但这行被注释了
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
                    Startautoset_button.BackColor = Color.White;// 测试中 → 白色
                }
                if (TestResult.test_status == 0)
                {
                    Startautoset_button.BackColor = Color.Orange;// 等待/空闲 → 橙色
                }
                if (TestResult.test_status == 2)
                {
                    Startautoset_button.BackColor = Color.Green;// 测试通过 → 绿色
                }

            }));
            // Optional: Call Refresh if needed for some custom drawing or layout issues not handled by Text changes
            // this.Refresh(); // Uncomment only if absolutely necessary after Text changes.
        }

        private async void StartNewTestSequence()
        {
            //isTestRunning是一个 bool 标志，防止测试过程中被重复调用（比如定时器连续触发两次）。
            //如果已经有测试在跑，直接 return 不执行。
            if (isTestRunning)
            {
                return;
            }
            //标记isTestRunning = true，告诉其他代码"测试正在进行中"
            isTestRunning = true; // Mark test as running
            //timer是一个Stopwatch（高精度计时器），用来统计本次测试耗时。
            //Reset()清零，Start()开始计时
            timer.Reset();
            timer.Start();
            //进度条归零
            progressBar1.Value = 0;
            //开始测试按钮背景色设为蜜白色（Honeydew，浅绿白色，表示正常进行中）。
            Startautoset_button.BackColor = Color.Honeydew;
            //设置全局测试状态为1（1=测试中，其他值可能表示空闲/完成/失败等）
            TestResult.test_status = 1;//开始测试
            //根据全局语言设置，在按钮上显示中文或英文提示，告诉用户"检测到模块了，不要拔插"
            if (GlobalVarFun.Language == "Chinese")
            {
                Startautoset_button.Text = "已检测到模块插入, 请不要插拔模块......";
            }
            else
            {
                Startautoset_button.Text = "Module insertion has been detected. Do not remove or insert the module......";
            }
            //删除上一次测试的日志文件
            simpleLogger.FileDelete();
            //创建一个新的日志记录器，日志文件路径为 D:\SFPXFPTesTLogDUT1.txt
            //（DUT1 = Device Under Test 1，即第1个被测通道）
            simpleLogger = new SimpleLogger("D:\\SFPXFPTesTLogDUT1.txt");
            //标记模块为在线状态（定时器中判断新模块插入时 moduleOnline 是 false，这里正式确认模块在线）
            moduleOnline = true;
            //在界面上显示模块型号（PN = Part Number，料号），如"SFP-GN25L95"等
            pnshow_textBox.Text = TestResult.fibertop_pn;
            //设置两个 I2C 状态指示灯为绿色（false=正常/绿色，true=异常/红色，
            //结合之前定时器中离线时传 true 变红色可知）。
            SetLED(i2cok_pictureBox1, false);
            SetLED(i2cok_pictureBox2, false);
            //强制刷新窗体界面，让上面的 UI 变更立即显示出来，而不是等消息循环空闲时才刷新
            Update();

            // 模块DDM温度判断 0~40
            //不是初测（终测等）→ __一定检查温度__，不管什么模块
            //是初测→ 看模块型号
            //- 只有"初测"且是这几个特定型号时，才跳过温度检查；其他情况都检查温度。
            //温度正常范围是0°C ~40°C，超出则按钮变红、提示"温度异常无法测试"，然后 return 退出
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
            //如果不是"初测"（即终测），检查模块供电电压。
            //正常范围 3.15V ~3.45V（SFP标准3.3V±5 %）。
            //超出范围说明模块供电异常或模块故障，按钮变红、提示错误、return 退出。
            //初测时不检查电压（初测可能模块还在调试阶段，电压可能不稳）。
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
            //重置"记录需要保存"标志，测试过程中如果有数据需要保存到SQL，会把这个标志设为true。
            GlobalVarFun.record_need_save = false;

            Startautoset_button.BackColor = Color.Honeydew;
            //显示"XX型号：模块正在自动测试中，请等待......"
            //按钮恢复蜜白色，显示"正在测试中"，进度条设到5%，强制刷新界面。
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
            //创建一个结果对象，用来接收测试返回的发射/接收相关数据
            //（虽然这段代码后面没直接用到它，但 `RunTestSequenceAsync` 内部会填充）
            ReturnTxRxResult returnTxRxResult = new ReturnTxRxResult();
            //testLog_textBox.ForeColor = Color.Red;
            //创建取消令牌源，用于支持"取消测试"功能
            //（比如用户点停止按钮时，可以通过它通知异步任务取消）
            testCancellationSource = new CancellationTokenSource();

            try
            {
                //调用核心测试逻辑的异步方法
                //这是核心！调用异步测试方法，传入取消令牌。
                //await 会等待整个测试序列完成（包括写寄存器、光功率测试、眼图测试、灵敏度测试等所有项目），
                //但不会阻塞UI线程。
                await RunTestSequenceAsync(testCancellationSource.Token);
                //测试完成后，刷新一次DDM信息显示
                ShowModuleDdmInfo();//DDM刷新
            }
            //如果测试过程中被用户取消（触发了 `CancellationToken`），会抛出 `OperationCanceledException`。
            catch (OperationCanceledException)
            {
                //捕获后显示"测试已被取消"，按钮变黄色。
                // 测试被取消
                Startautoset_button.Text = "测试已被取消。";
                Startautoset_button.BackColor = Color.Yellow;
            }
            finally
            {
                //停止计时，取出经过的时间字符串
                timer.Stop();
                string str = timer.Elapsed.ToString();
                //str = str.Substring(6, 5);
                //`Substring(3, 7)` 是截取时间字符串的一部分（Stopwatch 的 `Elapsed.ToString()` 默认格式是 `00:00:00.1234567`，
                //从第3位取7个字符大概取到"分:秒.毫秒"部分，
                //显示为类似 `00:12.345` 表示12.345秒）
                str = str.Substring(3, 7);
                //在界面上显示测试耗时。
                if (GlobalVarFun.Language == "Chinese")
                {
                    label_testtime.Text = "测试时间: " + str + "s";
                }
                else
                {
                    label_testtime.Text = "Test Time: " + str + "s";//测试时间
                }
                //测试计数加1（累计插拔测试了多少次模块）
                testnum++;
                //BeginInvoke异步写入文件记录测试次数（避免阻塞当前线程），写到某个配置文件中保存，下次启动程序可以读取
                this.BeginInvoke(new MethodInvoker(() =>
                {
                    writeMyFileTxt("TestNum", testnum.ToString());//模块插拔次数记录更新
                }));
                //如果累计测试超过2000次，提示更换测试座（SFP模块的金手指座子插拔次数多了会磨损，接触不良影响测试准确性，属于设备维护提醒）
                if (testnum > 2000)
                {
                    AddTestLog("测试次数超过2000，请及时更换测试板座子");
                }
                //tBNum.Text = testnum.ToString();
                Update();
                //关键：把 isTestRunning 设回 false，
                //允许下一轮新测试启动。这和开头的 isTestRunning = true 对应
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
