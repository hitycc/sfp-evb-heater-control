using FibertopTest_Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFPXFP自动测试软件多端口
{
    public class Services
    {
        private SqlConnection sqlconnection = new SqlConnection("server=" + GlobalVarFun.sqlserver_ip + ";uid=tester;pwd=fibertop2020;database=SFP");

        //打开SQL数据库
        public void ServersOpen()
        {
            sqlconnection.Open();
        }
        //关闭SQL数据库
        public void ServersClose()
        {
            sqlconnection.Close();
        }

        // 测试服务器IP是否通畅
        public bool TestServerIPonline()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(GlobalVarFun.sqlserver_ip);
                ping.Dispose();
                if (pingReply.Status != IPStatus.Success)
                {
                    return false;
                }
            }
            catch //(Exception exp)
            {
                return false;
            }
            //
            return true;
        }
        //  更新服务器的Access文件到 本机
        public String CopyShareDBFileToLocal()
        {
            Process proc = new Process();
            string str = proc.StandardOutput.ReadToEnd();
            string dosLine;
           // bool Flag = false;
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();

                // 连接共享文件夹
                dosLine = @"net use \\" + GlobalVarFun.sqlserver_ip + @"\Fibertop ""test2016"" /user:""fibertop""";
                proc.StandardInput.WriteLine(dosLine);

                // 延时
                Thread.Sleep(1300);

                // Copy 共享文件夹
                dosLine = @"xcopy \\" + GlobalVarFun.sqlserver_ip + @"\Fibertop C:\ /s/e/y";
                proc.StandardInput.WriteLine(dosLine);

                // 延时
                Thread.Sleep(300);

                // 断开共享文件夹
                dosLine = @"net use \\" + GlobalVarFun.sqlserver_ip + @"\飞思卓共享文件 /del";
                proc.StandardInput.WriteLine(dosLine);

                proc.StandardInput.WriteLine("exit");
                //proc.StandardInput.Close();
                proc.WaitForExit();


                //if (str.Contains("复制了 0 个文件")) // 复制文件失败
                //{
                //    Flag = false;
                //}
                //else
                //{
                //    Flag = true;
                //}
                //MessageBox.Show(str); // 运行信息显示
            }
            catch (Exception ex)
            {
                //Flag = false;
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }

            return str;
        }

        // 保存数据到数据库
        public bool SaveRecordToSQL(byte dut)
        {
            if (dut == 1)
            {
                return SaveRecordToSQL1();
            }
            else if (dut == 2)
            {
                return SaveRecordToSQL2();
            }
            else if (dut == 3)
            {
                return SaveRecordToSQL3();
            }
            else if (dut == 4)
            {
                return SaveRecordToSQL4();
            }
            else
            {
                return false;
            }
        }
        private bool SaveRecordToSQL1()
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

            float tx_pwr_real = TestResult.txPower;
            float tx_er = TestResult.txEr;
            float tx_esn = TestResult.txESN;
            float tx_crossing = TestResult.txCrossing;
            float tx_jitterRMS = TestResult.txJiterRMS;
            float tx_jitterPP = TestResult.txJiterPP;
            float tx_risetime = TestResult.TxRiseTime;
            float tx_falltime = TestResult.TxFallTime;
            float tx_eyeamp = TestResult.TxEyeAmp;
            float tx_pwr_ave = TestResult.txPowerDCA;///+ (float)DCAoptoerr_numericUpDown.Value;

            double tx_wlgth = TestResult.wLength;
            double tx_smsr = TestResult.smsr;
            double tx_spec_width = TestResult.spectralwidth;

            double supply = TestResult.supply;

            float tx_pwrErr = TestResult.txPwrErr;

            float[] rx_PwrReal = new float[5];
            float[] rx_PwrDDM = new float[5];
            float[] rx_pwrErr = new float[5];

            float rx_sen = TestResult.rxSen;
            float rx_DLos = TestResult.rxDLos;
            float rx_ALos = TestResult.rxALos;
            float rx_overload = TestResult.rxOverLoad;

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

            for (i = 0; i < 5; i++)
            {
                rx_PwrReal[i] = TestResult.rxPwrReal[i];
                rx_PwrDDM[i] = TestResult.rxPwrDDM[i];
                rx_pwrErr[i] = TestResult.rxPwrErr[i];
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
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg += "SQL数据库无法打开连接！"; // 异常情况
                    }
                    else
                    {
                        errmsg += "SQL database cannot open connection!";
                    }
                }
                //
                //Thread.Sleep(300);
                //
                if (GlobalVarFun.testType == "firstTest")
                {
                    strName = "FirstTest_Record_Table";
                }
                else
                {
                    strName = "FinalTest_Record_Table";
                }
                //
                conString = @"insert into " + strName + @" ([FibertopBN],[FibertopSN],[FibertopPN],[TosaSN],[RosaSN],[SN],[PN],[VN],[Date],[Temp],[Vcc],[TxBias],[TxPower],[TxPowerReal],[TxPowerErr],[TxER],[TxESN],[TxCrossing],[TxJitterRMS],[TxJitterPP],[TxRiseTime],[TxFallTime],[TxEyePattern],[TxEyeMargin],[TxEyeImage],"
                          + @"[RxPwrReal_1],[RxPwrReal_2],[RxPwrReal_3],[RxPwrReal_4],[RxPwrReal_5],[RxPwrDDM_1],[RxPwrDDM_2],[RxPwrDDM_3],[RxPwrDDM_4],[RxPwrDDM_5],[RxPwrErr_1],[RxPwrErr_2],[RxPwrErr_3],[RxPwrErr_4],[RxPwrErr_5],"
                          + @"[Sensitivity],[RxALos],[RxDLos],[RxOverLoad],[FlashData],[DesignType],[TestDate],[TesterNO],[WaveLength],[SMSR],[SpectralWidth],[Supply],[TxEyeAmp],[TxPowerAverage])"
                          + @" VALUES (@FibertopBN,@FibertopSN,@FibertopPN,@TosaSN,@RosaSN,@SN,@PN,@VN,@Date,@Temp,@Vcc,@TxBias,@TxPower,@TxPowerReal,@TxPowerErr,@TxER,@TxESN,@TxCrossing,@TxJitterRMS,@TxJitterPP,@TxRiseTime,@TxFallTime,@TxEyePattern,@TxEyeMargin,@TxEyeImage,"
                          + @"@RxPwrReal_1,@RxPwrReal_2,@RxPwrReal_3,@RxPwrReal_4,@RxPwrReal_5,@RxPwrDDM_1,@RxPwrDDM_2,@RxPwrDDM_3,@RxPwrDDM_4,@RxPwrDDM_5,@RxPwrErr_1,@RxPwrErr_2,@RxPwrErr_3,@RxPwrErr_4,@RxPwrErr_5,"
                          + @"@Sensitivity,@RxALos,@RxDLos,@RxOverLoad,@FlashData,@DesignType,@TestDate,@TesterNO,@WaveLength,@SMSR,@SpectralWidth,@Supply,@TxEyeAmp,@TxPowerAverage)";
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
                    myCommand.Parameters.Add("@TxBias", SqlDbType.Float).Value = tx_bias;
                    myCommand.Parameters.Add("@TxPower", SqlDbType.Float).Value = tx_pwr;
                    myCommand.Parameters.Add("@TxPowerReal", SqlDbType.Float).Value = tx_pwr_real;
                    myCommand.Parameters.Add("@TxPowerErr", SqlDbType.Float).Value = tx_pwrErr;
                    myCommand.Parameters.Add("@TxER", SqlDbType.Float).Value = tx_er;
                    myCommand.Parameters.Add("@TxESN", SqlDbType.Float).Value = tx_esn;
                    myCommand.Parameters.Add("@TxCrossing", SqlDbType.Float).Value = tx_crossing;
                    myCommand.Parameters.Add("@TxJitterRMS", SqlDbType.Float).Value = tx_jitterRMS;
                    myCommand.Parameters.Add("@TxJitterPP", SqlDbType.Float).Value = tx_jitterPP;
                    myCommand.Parameters.Add("@TxRiseTime", SqlDbType.Float).Value = tx_risetime;
                    myCommand.Parameters.Add("@TxFallTime", SqlDbType.Float).Value = tx_falltime;
                    myCommand.Parameters.Add("@TxEyePattern", SqlDbType.NChar).Value = ""; ///////////////////////////////////////
                    myCommand.Parameters.Add("@TxEyeMargin", SqlDbType.Float).Value = TestResult.mask_margin;

                    //眼图数据
                    if ((TestResult.bimage_len == 0) || (GlobalVarFun.setup.image_save == false))
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = DBNull.Value; //null
                    }
                    else
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = TestResult.txEye_image; //GIF image
                    }
                    //

                    myCommand.Parameters.Add("@RxPwrReal_1", SqlDbType.Float).Value = rx_PwrReal[0];
                    myCommand.Parameters.Add("@RxPwrReal_2", SqlDbType.Float).Value = rx_PwrReal[1];
                    myCommand.Parameters.Add("@RxPwrReal_3", SqlDbType.Float).Value = rx_PwrReal[2];
                    myCommand.Parameters.Add("@RxPwrReal_4", SqlDbType.Float).Value = rx_PwrReal[3];
                    myCommand.Parameters.Add("@RxPwrReal_5", SqlDbType.Float).Value = rx_PwrReal[4];

                    myCommand.Parameters.Add("@RxPwrDDM_1", SqlDbType.Float).Value = rx_PwrDDM[0];
                    myCommand.Parameters.Add("@RxPwrDDM_2", SqlDbType.Float).Value = rx_PwrDDM[1];
                    myCommand.Parameters.Add("@RxPwrDDM_3", SqlDbType.Float).Value = rx_PwrDDM[2];
                    myCommand.Parameters.Add("@RxPwrDDM_4", SqlDbType.Float).Value = rx_PwrDDM[3];
                    myCommand.Parameters.Add("@RxPwrDDM_5", SqlDbType.Float).Value = rx_PwrDDM[4];

                    myCommand.Parameters.Add("@RxPwrErr_1", SqlDbType.Float).Value = rx_pwrErr[0];
                    myCommand.Parameters.Add("@RxPwrErr_2", SqlDbType.Float).Value = rx_pwrErr[1];
                    myCommand.Parameters.Add("@RxPwrErr_3", SqlDbType.Float).Value = rx_pwrErr[2];
                    myCommand.Parameters.Add("@RxPwrErr_4", SqlDbType.Float).Value = rx_pwrErr[3];
                    myCommand.Parameters.Add("@RxPwrErr_5", SqlDbType.Float).Value = rx_pwrErr[4];

                    myCommand.Parameters.Add("@Sensitivity", SqlDbType.Float).Value = rx_sen;
                    myCommand.Parameters.Add("@RxALos", SqlDbType.Float).Value = rx_ALos;
                    myCommand.Parameters.Add("@RxDLos", SqlDbType.Float).Value = rx_DLos;
                    myCommand.Parameters.Add("@RxOverLoad", SqlDbType.Float).Value = rx_overload;

                    myCommand.Parameters.Add("@FlashData", SqlDbType.Binary).Value = flash_data;

                    myCommand.Parameters.Add("@DesignType", SqlDbType.NChar).Value = design_type;

                    myCommand.Parameters.Add("@TestDate", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //2017.12.4
                    myCommand.Parameters.Add("@TesterNO", SqlDbType.NChar).Value = tester_no;

                    myCommand.Parameters.Add("@WaveLength", SqlDbType.NChar).Value = tx_wlgth;          //2025.09.11
                    myCommand.Parameters.Add("@SMSR", SqlDbType.NChar).Value = tx_smsr;                 //2025.09.11
                    myCommand.Parameters.Add("@SpectralWidth", SqlDbType.NChar).Value = tx_spec_width;  //2025.09.11
                    myCommand.Parameters.Add("@Supply", SqlDbType.NChar).Value = supply;                //2025.09.11
                    myCommand.Parameters.Add("@TxEyeAmp", SqlDbType.Float).Value = tx_eyeamp;           //2025.09.12
                    myCommand.Parameters.Add("@TxPowerAverage", SqlDbType.Float).Value = tx_pwr_ave;    //2025.09.13
                    //
                    i = myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                sqlconnection.Close();
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "测试数据保存到SQL数据库失败！";
                    throw new Exception("SQL执行异常", ex);
                }
                else
                {
                    errmsg += "Failed to save test data to SQL database!";//测试数据保存到SQL数据库失败！
                    throw new Exception("SQL执行异常", ex);
                }
            }
            finally
            {
                sqlconnection.Close();
            }
            //
            if (i <= 0)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "insert记录到SQL数据表返回异常！";
                }
                else
                {
                    errmsg += "insert record into SQL data table returns exception!";
                }
            }
            //
            //errorMessage = errmsg;
            //AddTestLog(errorMessage); // test
            //
            if (errmsg != "")
            {
                return false;
            }
            //
            return true;
        }

        // 保存数据到数据库
        private bool SaveRecordToSQL2()
        {
            string strName = "";
            string errmsg = "";
            string conString;
            string fibertop_bn = TestResult2.fibertop_bn;
            string fibertop_sn = TestResult2.fibertop_sn;
            string fibertop_pn = TestResult2.fibertop_pn;
            string tosa_sn = TestResult2.tosa_sn;
            string rosa_sn = TestResult2.rosa_sn;
            string sn = TestResult2.sn;
            string pn = TestResult2.pn;
            string vn = TestResult2.vn;
            string date = TestResult2.date;

            float temp = TestResult2.tempDDM;
            float vcc = TestResult2.vccDDM;
            float tx_bias = TestResult2.txBiasDDM;
            float tx_pwr = TestResult2.txPowerDDM;

            float tx_pwr_real = TestResult2.txPower;
            float tx_er = TestResult2.txEr;
            float tx_esn = TestResult2.txESN;
            float tx_crossing = TestResult2.txCrossing;
            float tx_jitterRMS = TestResult2.txJiterRMS;
            float tx_jitterPP = TestResult2.txJiterPP;
            float tx_risetime = TestResult2.TxRiseTime;
            float tx_falltime = TestResult2.TxFallTime;
            float tx_eyeamp = TestResult2.TxEyeAmp;
            float tx_pwr_ave = TestResult2.txPowerDCA;///+ (float)DCAoptoerr_numericUpDown.Value;

            double tx_wlgth = TestResult2.wLength;
            double tx_smsr = TestResult2.smsr;
            double tx_spec_width = TestResult2.spectralwidth;

            double supply = TestResult2.supply;

            float tx_pwrErr = TestResult2.txPwrErr;

            float[] rx_PwrReal = new float[5];
            float[] rx_PwrDDM = new float[5];
            float[] rx_pwrErr = new float[5];

            float rx_sen = TestResult2.rxSen;
            float rx_DLos = TestResult2.rxDLos;
            float rx_ALos = TestResult2.rxALos;
            float rx_overload = TestResult2.rxOverLoad;

            string design_type = GlobalVarFun.moduleType;

            string tester_no = TestResult2.tester_no;

            byte[] flash_data = new byte[TestResult2.flash_data_len];

            byte[] byte_image = new byte[256];

            int i;

            for (i = 0; i < TestResult2.flash_data_len; i++)
            {
                flash_data[i] = TestResult2.flash_data[i];
            }

            //for (i = 0; i < TestResult.txEye_image.Length; i++)
            //{
            //   byte_image[i] = TestResult.txEye_image[i];
            //}

            for (i = 0; i < 5; i++)
            {
                rx_PwrReal[i] = TestResult2.rxPwrReal[i];
                rx_PwrDDM[i] = TestResult2.rxPwrDDM[i];
                rx_pwrErr[i] = TestResult2.rxPwrErr[i];
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
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg += "SQL数据库无法打开连接！"; // 异常情况
                    }
                    else
                    {
                        errmsg += "SQL database cannot open connection!";
                    }
                }
                //
                //Thread.Sleep(300);
                //
                if (GlobalVarFun.testType == "firstTest")
                {
                    strName = "FirstTest_Record_Table";
                }
                else
                {
                    strName = "FinalTest_Record_Table";
                }
                //
                conString = @"insert into " + strName + @" ([FibertopBN],[FibertopSN],[FibertopPN],[TosaSN],[RosaSN],[SN],[PN],[VN],[Date],[Temp],[Vcc],[TxBias],[TxPower],[TxPowerReal],[TxPowerErr],[TxER],[TxESN],[TxCrossing],[TxJitterRMS],[TxJitterPP],[TxRiseTime],[TxFallTime],[TxEyePattern],[TxEyeMargin],[TxEyeImage],"
                          + @"[RxPwrReal_1],[RxPwrReal_2],[RxPwrReal_3],[RxPwrReal_4],[RxPwrReal_5],[RxPwrDDM_1],[RxPwrDDM_2],[RxPwrDDM_3],[RxPwrDDM_4],[RxPwrDDM_5],[RxPwrErr_1],[RxPwrErr_2],[RxPwrErr_3],[RxPwrErr_4],[RxPwrErr_5],"
                          + @"[Sensitivity],[RxALos],[RxDLos],[RxOverLoad],[FlashData],[DesignType],[TestDate],[TesterNO],[WaveLength],[SMSR],[SpectralWidth],[Supply],[TxEyeAmp],[TxPowerAverage])"
                          + @" VALUES (@FibertopBN,@FibertopSN,@FibertopPN,@TosaSN,@RosaSN,@SN,@PN,@VN,@Date,@Temp,@Vcc,@TxBias,@TxPower,@TxPowerReal,@TxPowerErr,@TxER,@TxESN,@TxCrossing,@TxJitterRMS,@TxJitterPP,@TxRiseTime,@TxFallTime,@TxEyePattern,@TxEyeMargin,@TxEyeImage,"
                          + @"@RxPwrReal_1,@RxPwrReal_2,@RxPwrReal_3,@RxPwrReal_4,@RxPwrReal_5,@RxPwrDDM_1,@RxPwrDDM_2,@RxPwrDDM_3,@RxPwrDDM_4,@RxPwrDDM_5,@RxPwrErr_1,@RxPwrErr_2,@RxPwrErr_3,@RxPwrErr_4,@RxPwrErr_5,"
                          + @"@Sensitivity,@RxALos,@RxDLos,@RxOverLoad,@FlashData,@DesignType,@TestDate,@TesterNO,@WaveLength,@SMSR,@SpectralWidth,@Supply,@TxEyeAmp,@TxPowerAverage)";
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
                    myCommand.Parameters.Add("@TxBias", SqlDbType.Float).Value = tx_bias;
                    myCommand.Parameters.Add("@TxPower", SqlDbType.Float).Value = tx_pwr;
                    myCommand.Parameters.Add("@TxPowerReal", SqlDbType.Float).Value = tx_pwr_real;
                    myCommand.Parameters.Add("@TxPowerErr", SqlDbType.Float).Value = tx_pwrErr;
                    myCommand.Parameters.Add("@TxER", SqlDbType.Float).Value = tx_er;
                    myCommand.Parameters.Add("@TxESN", SqlDbType.Float).Value = tx_esn;
                    myCommand.Parameters.Add("@TxCrossing", SqlDbType.Float).Value = tx_crossing;
                    myCommand.Parameters.Add("@TxJitterRMS", SqlDbType.Float).Value = tx_jitterRMS;
                    myCommand.Parameters.Add("@TxJitterPP", SqlDbType.Float).Value = tx_jitterPP;
                    myCommand.Parameters.Add("@TxRiseTime", SqlDbType.Float).Value = tx_risetime;
                    myCommand.Parameters.Add("@TxFallTime", SqlDbType.Float).Value = tx_falltime;
                    myCommand.Parameters.Add("@TxEyePattern", SqlDbType.NChar).Value = ""; ///////////////////////////////////////
                    myCommand.Parameters.Add("@TxEyeMargin", SqlDbType.Float).Value = TestResult.mask_margin;

                    //眼图数据
                    if ((TestResult.bimage_len == 0) || (GlobalVarFun.setup.image_save == false))
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = DBNull.Value; //null
                    }
                    else
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = TestResult.txEye_image; //GIF image
                    }
                    //

                    myCommand.Parameters.Add("@RxPwrReal_1", SqlDbType.Float).Value = rx_PwrReal[0];
                    myCommand.Parameters.Add("@RxPwrReal_2", SqlDbType.Float).Value = rx_PwrReal[1];
                    myCommand.Parameters.Add("@RxPwrReal_3", SqlDbType.Float).Value = rx_PwrReal[2];
                    myCommand.Parameters.Add("@RxPwrReal_4", SqlDbType.Float).Value = rx_PwrReal[3];
                    myCommand.Parameters.Add("@RxPwrReal_5", SqlDbType.Float).Value = rx_PwrReal[4];

                    myCommand.Parameters.Add("@RxPwrDDM_1", SqlDbType.Float).Value = rx_PwrDDM[0];
                    myCommand.Parameters.Add("@RxPwrDDM_2", SqlDbType.Float).Value = rx_PwrDDM[1];
                    myCommand.Parameters.Add("@RxPwrDDM_3", SqlDbType.Float).Value = rx_PwrDDM[2];
                    myCommand.Parameters.Add("@RxPwrDDM_4", SqlDbType.Float).Value = rx_PwrDDM[3];
                    myCommand.Parameters.Add("@RxPwrDDM_5", SqlDbType.Float).Value = rx_PwrDDM[4];

                    myCommand.Parameters.Add("@RxPwrErr_1", SqlDbType.Float).Value = rx_pwrErr[0];
                    myCommand.Parameters.Add("@RxPwrErr_2", SqlDbType.Float).Value = rx_pwrErr[1];
                    myCommand.Parameters.Add("@RxPwrErr_3", SqlDbType.Float).Value = rx_pwrErr[2];
                    myCommand.Parameters.Add("@RxPwrErr_4", SqlDbType.Float).Value = rx_pwrErr[3];
                    myCommand.Parameters.Add("@RxPwrErr_5", SqlDbType.Float).Value = rx_pwrErr[4];

                    myCommand.Parameters.Add("@Sensitivity", SqlDbType.Float).Value = rx_sen;
                    myCommand.Parameters.Add("@RxALos", SqlDbType.Float).Value = rx_ALos;
                    myCommand.Parameters.Add("@RxDLos", SqlDbType.Float).Value = rx_DLos;
                    myCommand.Parameters.Add("@RxOverLoad", SqlDbType.Float).Value = rx_overload;

                    myCommand.Parameters.Add("@FlashData", SqlDbType.Binary).Value = flash_data;

                    myCommand.Parameters.Add("@DesignType", SqlDbType.NChar).Value = design_type;

                    myCommand.Parameters.Add("@TestDate", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //2017.12.4
                    myCommand.Parameters.Add("@TesterNO", SqlDbType.NChar).Value = tester_no;

                    myCommand.Parameters.Add("@WaveLength", SqlDbType.NChar).Value = tx_wlgth;          //2025.09.11
                    myCommand.Parameters.Add("@SMSR", SqlDbType.NChar).Value = tx_smsr;                 //2025.09.11
                    myCommand.Parameters.Add("@SpectralWidth", SqlDbType.NChar).Value = tx_spec_width;  //2025.09.11
                    myCommand.Parameters.Add("@Supply", SqlDbType.NChar).Value = supply;                //2025.09.11
                    myCommand.Parameters.Add("@TxEyeAmp", SqlDbType.Float).Value = tx_eyeamp;           //2025.09.12
                    myCommand.Parameters.Add("@TxPowerAverage", SqlDbType.Float).Value = tx_pwr_ave;    //2025.09.13
                    //
                    i = myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                sqlconnection.Close();
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "测试数据保存到SQL数据库失败！";
                    throw new Exception("SQL执行异常", ex);
                }
                else
                {
                    errmsg += "Failed to save test data to SQL database!";//测试数据保存到SQL数据库失败！
                    throw new Exception("SQL执行异常", ex);
                }
            }
            finally
            {
                sqlconnection.Close();
            }
            //
            if (i <= 0)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "insert记录到SQL数据表返回异常！";
                }
                else
                {
                    errmsg += "insert record into SQL data table returns exception!";
                }
            }
            //
            //errorMessage = errmsg;
            //AddTestLog(errorMessage); // test
            //
            if (errmsg != "")
            {
                return false;
            }
            //
            return true;
        }
        private bool SaveRecordToSQL3()
        {
            string strName = "";
            string errmsg = "";
            string conString;
            string fibertop_bn = TestResult3.fibertop_bn;
            string fibertop_sn = TestResult3.fibertop_sn;
            string fibertop_pn = TestResult3.fibertop_pn;
            string tosa_sn = TestResult3.tosa_sn;
            string rosa_sn = TestResult3.rosa_sn;
            string sn = TestResult3.sn;
            string pn = TestResult3.pn;
            string vn = TestResult3.vn;
            string date = TestResult3.date;

            float temp = TestResult3.tempDDM;
            float vcc = TestResult3.vccDDM;
            float tx_bias = TestResult3.txBiasDDM;
            float tx_pwr = TestResult3.txPowerDDM;

            float tx_pwr_real = TestResult3.txPower;
            float tx_er = TestResult3.txEr;
            float tx_esn = TestResult3.txESN;
            float tx_crossing = TestResult3.txCrossing;
            float tx_jitterRMS = TestResult3.txJiterRMS;
            float tx_jitterPP = TestResult3.txJiterPP;
            float tx_risetime = TestResult3.TxRiseTime;
            float tx_falltime = TestResult3.TxFallTime;
            float tx_eyeamp = TestResult3.TxEyeAmp;
            float tx_pwr_ave = TestResult3.txPowerDCA;///+ (float)DCAoptoerr_numericUpDown.Value;

            double tx_wlgth = TestResult3.wLength;
            double tx_smsr = TestResult3.smsr;
            double tx_spec_width = TestResult3.spectralwidth;

            double supply = TestResult3.supply;

            float tx_pwrErr = TestResult3.txPwrErr;

            float[] rx_PwrReal = new float[5];
            float[] rx_PwrDDM = new float[5];
            float[] rx_pwrErr = new float[5];

            float rx_sen = TestResult3.rxSen;
            float rx_DLos = TestResult3.rxDLos;
            float rx_ALos = TestResult3.rxALos;
            float rx_overload = TestResult3.rxOverLoad;

            string design_type = GlobalVarFun.moduleType;

            string tester_no = TestResult3.tester_no;

            byte[] flash_data = new byte[TestResult3.flash_data_len];

            byte[] byte_image = new byte[256];

            int i;

            for (i = 0; i < TestResult3.flash_data_len; i++)
            {
                flash_data[i] = TestResult3.flash_data[i];
            }

            //for (i = 0; i < TestResult.txEye_image.Length; i++)
            //{
            //   byte_image[i] = TestResult.txEye_image[i];
            //}

            for (i = 0; i < 5; i++)
            {
                rx_PwrReal[i] = TestResult3.rxPwrReal[i];
                rx_PwrDDM[i] = TestResult3.rxPwrDDM[i];
                rx_pwrErr[i] = TestResult3.rxPwrErr[i];
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
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg += "SQL数据库无法打开连接！"; // 异常情况
                    }
                    else
                    {
                        errmsg += "SQL database cannot open connection!";
                    }
                }
                //
                //Thread.Sleep(300);
                //
                if (GlobalVarFun.testType == "firstTest")
                {
                    strName = "FirstTest_Record_Table";
                }
                else
                {
                    strName = "FinalTest_Record_Table";
                }
                //
                conString = @"insert into " + strName + @" ([FibertopBN],[FibertopSN],[FibertopPN],[TosaSN],[RosaSN],[SN],[PN],[VN],[Date],[Temp],[Vcc],[TxBias],[TxPower],[TxPowerReal],[TxPowerErr],[TxER],[TxESN],[TxCrossing],[TxJitterRMS],[TxJitterPP],[TxRiseTime],[TxFallTime],[TxEyePattern],[TxEyeMargin],[TxEyeImage],"
                          + @"[RxPwrReal_1],[RxPwrReal_2],[RxPwrReal_3],[RxPwrReal_4],[RxPwrReal_5],[RxPwrDDM_1],[RxPwrDDM_2],[RxPwrDDM_3],[RxPwrDDM_4],[RxPwrDDM_5],[RxPwrErr_1],[RxPwrErr_2],[RxPwrErr_3],[RxPwrErr_4],[RxPwrErr_5],"
                          + @"[Sensitivity],[RxALos],[RxDLos],[RxOverLoad],[FlashData],[DesignType],[TestDate],[TesterNO],[WaveLength],[SMSR],[SpectralWidth],[Supply],[TxEyeAmp],[TxPowerAverage])"
                          + @" VALUES (@FibertopBN,@FibertopSN,@FibertopPN,@TosaSN,@RosaSN,@SN,@PN,@VN,@Date,@Temp,@Vcc,@TxBias,@TxPower,@TxPowerReal,@TxPowerErr,@TxER,@TxESN,@TxCrossing,@TxJitterRMS,@TxJitterPP,@TxRiseTime,@TxFallTime,@TxEyePattern,@TxEyeMargin,@TxEyeImage,"
                          + @"@RxPwrReal_1,@RxPwrReal_2,@RxPwrReal_3,@RxPwrReal_4,@RxPwrReal_5,@RxPwrDDM_1,@RxPwrDDM_2,@RxPwrDDM_3,@RxPwrDDM_4,@RxPwrDDM_5,@RxPwrErr_1,@RxPwrErr_2,@RxPwrErr_3,@RxPwrErr_4,@RxPwrErr_5,"
                          + @"@Sensitivity,@RxALos,@RxDLos,@RxOverLoad,@FlashData,@DesignType,@TestDate,@TesterNO,@WaveLength,@SMSR,@SpectralWidth,@Supply,@TxEyeAmp,@TxPowerAverage)";
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
                    myCommand.Parameters.Add("@TxBias", SqlDbType.Float).Value = tx_bias;
                    myCommand.Parameters.Add("@TxPower", SqlDbType.Float).Value = tx_pwr;
                    myCommand.Parameters.Add("@TxPowerReal", SqlDbType.Float).Value = tx_pwr_real;
                    myCommand.Parameters.Add("@TxPowerErr", SqlDbType.Float).Value = tx_pwrErr;
                    myCommand.Parameters.Add("@TxER", SqlDbType.Float).Value = tx_er;
                    myCommand.Parameters.Add("@TxESN", SqlDbType.Float).Value = tx_esn;
                    myCommand.Parameters.Add("@TxCrossing", SqlDbType.Float).Value = tx_crossing;
                    myCommand.Parameters.Add("@TxJitterRMS", SqlDbType.Float).Value = tx_jitterRMS;
                    myCommand.Parameters.Add("@TxJitterPP", SqlDbType.Float).Value = tx_jitterPP;
                    myCommand.Parameters.Add("@TxRiseTime", SqlDbType.Float).Value = tx_risetime;
                    myCommand.Parameters.Add("@TxFallTime", SqlDbType.Float).Value = tx_falltime;
                    myCommand.Parameters.Add("@TxEyePattern", SqlDbType.NChar).Value = ""; ///////////////////////////////////////
                    myCommand.Parameters.Add("@TxEyeMargin", SqlDbType.Float).Value = TestResult.mask_margin;

                    //眼图数据
                    if ((TestResult.bimage_len == 0) || (GlobalVarFun.setup.image_save == false))
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = DBNull.Value; //null
                    }
                    else
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = TestResult.txEye_image; //GIF image
                    }
                    //

                    myCommand.Parameters.Add("@RxPwrReal_1", SqlDbType.Float).Value = rx_PwrReal[0];
                    myCommand.Parameters.Add("@RxPwrReal_2", SqlDbType.Float).Value = rx_PwrReal[1];
                    myCommand.Parameters.Add("@RxPwrReal_3", SqlDbType.Float).Value = rx_PwrReal[2];
                    myCommand.Parameters.Add("@RxPwrReal_4", SqlDbType.Float).Value = rx_PwrReal[3];
                    myCommand.Parameters.Add("@RxPwrReal_5", SqlDbType.Float).Value = rx_PwrReal[4];

                    myCommand.Parameters.Add("@RxPwrDDM_1", SqlDbType.Float).Value = rx_PwrDDM[0];
                    myCommand.Parameters.Add("@RxPwrDDM_2", SqlDbType.Float).Value = rx_PwrDDM[1];
                    myCommand.Parameters.Add("@RxPwrDDM_3", SqlDbType.Float).Value = rx_PwrDDM[2];
                    myCommand.Parameters.Add("@RxPwrDDM_4", SqlDbType.Float).Value = rx_PwrDDM[3];
                    myCommand.Parameters.Add("@RxPwrDDM_5", SqlDbType.Float).Value = rx_PwrDDM[4];

                    myCommand.Parameters.Add("@RxPwrErr_1", SqlDbType.Float).Value = rx_pwrErr[0];
                    myCommand.Parameters.Add("@RxPwrErr_2", SqlDbType.Float).Value = rx_pwrErr[1];
                    myCommand.Parameters.Add("@RxPwrErr_3", SqlDbType.Float).Value = rx_pwrErr[2];
                    myCommand.Parameters.Add("@RxPwrErr_4", SqlDbType.Float).Value = rx_pwrErr[3];
                    myCommand.Parameters.Add("@RxPwrErr_5", SqlDbType.Float).Value = rx_pwrErr[4];

                    myCommand.Parameters.Add("@Sensitivity", SqlDbType.Float).Value = rx_sen;
                    myCommand.Parameters.Add("@RxALos", SqlDbType.Float).Value = rx_ALos;
                    myCommand.Parameters.Add("@RxDLos", SqlDbType.Float).Value = rx_DLos;
                    myCommand.Parameters.Add("@RxOverLoad", SqlDbType.Float).Value = rx_overload;

                    myCommand.Parameters.Add("@FlashData", SqlDbType.Binary).Value = flash_data;

                    myCommand.Parameters.Add("@DesignType", SqlDbType.NChar).Value = design_type;

                    myCommand.Parameters.Add("@TestDate", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //2017.12.4
                    myCommand.Parameters.Add("@TesterNO", SqlDbType.NChar).Value = tester_no;

                    myCommand.Parameters.Add("@WaveLength", SqlDbType.NChar).Value = tx_wlgth;          //2025.09.11
                    myCommand.Parameters.Add("@SMSR", SqlDbType.NChar).Value = tx_smsr;                 //2025.09.11
                    myCommand.Parameters.Add("@SpectralWidth", SqlDbType.NChar).Value = tx_spec_width;  //2025.09.11
                    myCommand.Parameters.Add("@Supply", SqlDbType.NChar).Value = supply;                //2025.09.11
                    myCommand.Parameters.Add("@TxEyeAmp", SqlDbType.Float).Value = tx_eyeamp;           //2025.09.12
                    myCommand.Parameters.Add("@TxPowerAverage", SqlDbType.Float).Value = tx_pwr_ave;    //2025.09.13
                    //
                    i = myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                sqlconnection.Close();
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "测试数据保存到SQL数据库失败！";
                    throw new Exception("SQL执行异常", ex);
                }
                else
                {
                    errmsg += "Failed to save test data to SQL database!";//测试数据保存到SQL数据库失败！
                    throw new Exception("SQL执行异常", ex);
                }
            }
            finally
            {
                sqlconnection.Close();
            }
            //
            if (i <= 0)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "insert记录到SQL数据表返回异常！";
                }
                else
                {
                    errmsg += "insert record into SQL data table returns exception!";
                }
            }
            //
            //errorMessage = errmsg;
            //AddTestLog(errorMessage); // test
            //
            if (errmsg != "")
            {
                return false;
            }
            //
            return true;
        }
        private bool SaveRecordToSQL4()
        {
            string strName = "";
            string errmsg = "";
            string conString;
            string fibertop_bn = TestResult4.fibertop_bn;
            string fibertop_sn = TestResult4.fibertop_sn;
            string fibertop_pn = TestResult4.fibertop_pn;
            string tosa_sn = TestResult4.tosa_sn;
            string rosa_sn = TestResult4.rosa_sn;
            string sn = TestResult4.sn;
            string pn = TestResult4.pn;
            string vn = TestResult4.vn;
            string date = TestResult4.date;

            float temp = TestResult4.tempDDM;
            float vcc = TestResult4.vccDDM;
            float tx_bias = TestResult4.txBiasDDM;
            float tx_pwr = TestResult4.txPowerDDM;

            float tx_pwr_real = TestResult4.txPower;
            float tx_er = TestResult4.txEr;
            float tx_esn = TestResult4.txESN;
            float tx_crossing = TestResult4.txCrossing;
            float tx_jitterRMS = TestResult4.txJiterRMS;
            float tx_jitterPP = TestResult4.txJiterPP;
            float tx_risetime = TestResult4.TxRiseTime;
            float tx_falltime = TestResult4.TxFallTime;
            float tx_eyeamp = TestResult4.TxEyeAmp;
            float tx_pwr_ave = TestResult4.txPowerDCA;///+ (float)DCAoptoerr_numericUpDown.Value;

            double tx_wlgth = TestResult4.wLength;
            double tx_smsr = TestResult4.smsr;
            double tx_spec_width = TestResult4.spectralwidth;

            double supply = TestResult4.supply;

            float tx_pwrErr = TestResult4.txPwrErr;

            float[] rx_PwrReal = new float[5];
            float[] rx_PwrDDM = new float[5];
            float[] rx_pwrErr = new float[5];

            float rx_sen = TestResult4.rxSen;
            float rx_DLos = TestResult4.rxDLos;
            float rx_ALos = TestResult4.rxALos;
            float rx_overload = TestResult4.rxOverLoad;

            string design_type = GlobalVarFun.moduleType;

            string tester_no = TestResult4.tester_no;

            byte[] flash_data = new byte[TestResult4.flash_data_len];

            byte[] byte_image = new byte[256];

            int i;

            for (i = 0; i < TestResult4.flash_data_len; i++)
            {
                flash_data[i] = TestResult4.flash_data[i];
            }

            //for (i = 0; i < TestResult.txEye_image.Length; i++)
            //{
            //   byte_image[i] = TestResult.txEye_image[i];
            //}

            for (i = 0; i < 5; i++)
            {
                rx_PwrReal[i] = TestResult4.rxPwrReal[i];
                rx_PwrDDM[i] = TestResult4.rxPwrDDM[i];
                rx_pwrErr[i] = TestResult4.rxPwrErr[i];
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
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        errmsg += "SQL数据库无法打开连接！"; // 异常情况
                    }
                    else
                    {
                        errmsg += "SQL database cannot open connection!";
                    }
                }
                //
                //Thread.Sleep(300);
                //
                if (GlobalVarFun.testType == "firstTest")
                {
                    strName = "FirstTest_Record_Table";
                }
                else
                {
                    strName = "FinalTest_Record_Table";
                }
                //
                conString = @"insert into " + strName + @" ([FibertopBN],[FibertopSN],[FibertopPN],[TosaSN],[RosaSN],[SN],[PN],[VN],[Date],[Temp],[Vcc],[TxBias],[TxPower],[TxPowerReal],[TxPowerErr],[TxER],[TxESN],[TxCrossing],[TxJitterRMS],[TxJitterPP],[TxRiseTime],[TxFallTime],[TxEyePattern],[TxEyeMargin],[TxEyeImage],"
                          + @"[RxPwrReal_1],[RxPwrReal_2],[RxPwrReal_3],[RxPwrReal_4],[RxPwrReal_5],[RxPwrDDM_1],[RxPwrDDM_2],[RxPwrDDM_3],[RxPwrDDM_4],[RxPwrDDM_5],[RxPwrErr_1],[RxPwrErr_2],[RxPwrErr_3],[RxPwrErr_4],[RxPwrErr_5],"
                          + @"[Sensitivity],[RxALos],[RxDLos],[RxOverLoad],[FlashData],[DesignType],[TestDate],[TesterNO],[WaveLength],[SMSR],[SpectralWidth],[Supply],[TxEyeAmp],[TxPowerAverage])"
                          + @" VALUES (@FibertopBN,@FibertopSN,@FibertopPN,@TosaSN,@RosaSN,@SN,@PN,@VN,@Date,@Temp,@Vcc,@TxBias,@TxPower,@TxPowerReal,@TxPowerErr,@TxER,@TxESN,@TxCrossing,@TxJitterRMS,@TxJitterPP,@TxRiseTime,@TxFallTime,@TxEyePattern,@TxEyeMargin,@TxEyeImage,"
                          + @"@RxPwrReal_1,@RxPwrReal_2,@RxPwrReal_3,@RxPwrReal_4,@RxPwrReal_5,@RxPwrDDM_1,@RxPwrDDM_2,@RxPwrDDM_3,@RxPwrDDM_4,@RxPwrDDM_5,@RxPwrErr_1,@RxPwrErr_2,@RxPwrErr_3,@RxPwrErr_4,@RxPwrErr_5,"
                          + @"@Sensitivity,@RxALos,@RxDLos,@RxOverLoad,@FlashData,@DesignType,@TestDate,@TesterNO,@WaveLength,@SMSR,@SpectralWidth,@Supply,@TxEyeAmp,@TxPowerAverage)";
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
                    myCommand.Parameters.Add("@TxBias", SqlDbType.Float).Value = tx_bias;
                    myCommand.Parameters.Add("@TxPower", SqlDbType.Float).Value = tx_pwr;
                    myCommand.Parameters.Add("@TxPowerReal", SqlDbType.Float).Value = tx_pwr_real;
                    myCommand.Parameters.Add("@TxPowerErr", SqlDbType.Float).Value = tx_pwrErr;
                    myCommand.Parameters.Add("@TxER", SqlDbType.Float).Value = tx_er;
                    myCommand.Parameters.Add("@TxESN", SqlDbType.Float).Value = tx_esn;
                    myCommand.Parameters.Add("@TxCrossing", SqlDbType.Float).Value = tx_crossing;
                    myCommand.Parameters.Add("@TxJitterRMS", SqlDbType.Float).Value = tx_jitterRMS;
                    myCommand.Parameters.Add("@TxJitterPP", SqlDbType.Float).Value = tx_jitterPP;
                    myCommand.Parameters.Add("@TxRiseTime", SqlDbType.Float).Value = tx_risetime;
                    myCommand.Parameters.Add("@TxFallTime", SqlDbType.Float).Value = tx_falltime;
                    myCommand.Parameters.Add("@TxEyePattern", SqlDbType.NChar).Value = ""; ///////////////////////////////////////
                    myCommand.Parameters.Add("@TxEyeMargin", SqlDbType.Float).Value = TestResult.mask_margin;

                    //眼图数据
                    if ((TestResult.bimage_len == 0) || (GlobalVarFun.setup.image_save == false))
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = DBNull.Value; //null
                    }
                    else
                    {
                        myCommand.Parameters.Add("@TxEyeImage", SqlDbType.Image).Value = TestResult.txEye_image; //GIF image
                    }
                    //

                    myCommand.Parameters.Add("@RxPwrReal_1", SqlDbType.Float).Value = rx_PwrReal[0];
                    myCommand.Parameters.Add("@RxPwrReal_2", SqlDbType.Float).Value = rx_PwrReal[1];
                    myCommand.Parameters.Add("@RxPwrReal_3", SqlDbType.Float).Value = rx_PwrReal[2];
                    myCommand.Parameters.Add("@RxPwrReal_4", SqlDbType.Float).Value = rx_PwrReal[3];
                    myCommand.Parameters.Add("@RxPwrReal_5", SqlDbType.Float).Value = rx_PwrReal[4];

                    myCommand.Parameters.Add("@RxPwrDDM_1", SqlDbType.Float).Value = rx_PwrDDM[0];
                    myCommand.Parameters.Add("@RxPwrDDM_2", SqlDbType.Float).Value = rx_PwrDDM[1];
                    myCommand.Parameters.Add("@RxPwrDDM_3", SqlDbType.Float).Value = rx_PwrDDM[2];
                    myCommand.Parameters.Add("@RxPwrDDM_4", SqlDbType.Float).Value = rx_PwrDDM[3];
                    myCommand.Parameters.Add("@RxPwrDDM_5", SqlDbType.Float).Value = rx_PwrDDM[4];

                    myCommand.Parameters.Add("@RxPwrErr_1", SqlDbType.Float).Value = rx_pwrErr[0];
                    myCommand.Parameters.Add("@RxPwrErr_2", SqlDbType.Float).Value = rx_pwrErr[1];
                    myCommand.Parameters.Add("@RxPwrErr_3", SqlDbType.Float).Value = rx_pwrErr[2];
                    myCommand.Parameters.Add("@RxPwrErr_4", SqlDbType.Float).Value = rx_pwrErr[3];
                    myCommand.Parameters.Add("@RxPwrErr_5", SqlDbType.Float).Value = rx_pwrErr[4];

                    myCommand.Parameters.Add("@Sensitivity", SqlDbType.Float).Value = rx_sen;
                    myCommand.Parameters.Add("@RxALos", SqlDbType.Float).Value = rx_ALos;
                    myCommand.Parameters.Add("@RxDLos", SqlDbType.Float).Value = rx_DLos;
                    myCommand.Parameters.Add("@RxOverLoad", SqlDbType.Float).Value = rx_overload;

                    myCommand.Parameters.Add("@FlashData", SqlDbType.Binary).Value = flash_data;

                    myCommand.Parameters.Add("@DesignType", SqlDbType.NChar).Value = design_type;

                    myCommand.Parameters.Add("@TestDate", SqlDbType.DateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); //2017.12.4
                    myCommand.Parameters.Add("@TesterNO", SqlDbType.NChar).Value = tester_no;

                    myCommand.Parameters.Add("@WaveLength", SqlDbType.NChar).Value = tx_wlgth;          //2025.09.11
                    myCommand.Parameters.Add("@SMSR", SqlDbType.NChar).Value = tx_smsr;                 //2025.09.11
                    myCommand.Parameters.Add("@SpectralWidth", SqlDbType.NChar).Value = tx_spec_width;  //2025.09.11
                    myCommand.Parameters.Add("@Supply", SqlDbType.NChar).Value = supply;                //2025.09.11
                    myCommand.Parameters.Add("@TxEyeAmp", SqlDbType.Float).Value = tx_eyeamp;           //2025.09.12
                    myCommand.Parameters.Add("@TxPowerAverage", SqlDbType.Float).Value = tx_pwr_ave;    //2025.09.13
                    //
                    i = myCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                sqlconnection.Close();
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "测试数据保存到SQL数据库失败！";
                    throw new Exception("SQL执行异常", ex);
                }
                else
                {
                    errmsg += "Failed to save test data to SQL database!";//测试数据保存到 SQL数据库失败！
                    throw new Exception("SQL执行异常", ex);
                }
            }
            finally
            {
                sqlconnection.Close();
            }
            //
            if (i <= 0)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    errmsg += "insert记录到SQL数据表返回异常！";
                }
                else
                {
                    errmsg += "insert record into SQL data table returns exception!";
                }
            }
            //
            //errorMessage = errmsg;
            //AddTestLog(errorMessage); // test
            //
            if (errmsg != "")
            {
                return false;
            }
            //
            return true;
        }
    }
}
