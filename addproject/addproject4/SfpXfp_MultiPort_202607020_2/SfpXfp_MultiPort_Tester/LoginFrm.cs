using FibertopTest_Common;
//using Ivi.Visa.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace SFPXFP自动测试软件多端口
{
    public partial class LoginFrm : Form
    {
        string filePath = "C:\\Fibertop\\"; // 本地数据库镜像
        string[] moduleTypeDBName = new string[20];
        //string[] usb1;
        //string[] usb2;
        //CP2112 cp2112;
        //CP2112 cp2112_2;

        SFP_EVB_Heater evb;

        public LoginFrm()
        {
            InitializeComponent();
            //cp2112 = new CP2112();
            //cp2112_2 = new CP2112();
            evb = new SFP_EVB_Heater();
            //usb1 = cp2112.GetPortString();
            //usb2 = cp2112_2.GetPortString();
        }
        // 测试服务器IP是否通畅
        private bool TestServerIPonline()
        {
            try
            {
                Ping ping = new Ping();
                PingReply pingReply = ping.Send(sqlserver_comboBox.Text);
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
        private bool CopyShareDBFileToLocal()
        {
            Process proc = new Process();
            string dosLine;
            bool Flag = false;
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
                dosLine = @"net use \\" + sqlserver_comboBox.Text + @"\Fibertop ""test2016"" /user:""fibertop""";
                proc.StandardInput.WriteLine(dosLine);

                // 延时
                Thread.Sleep(1300);

                // Copy 共享文件夹
                dosLine = @"xcopy \\" + sqlserver_comboBox.Text + @"\Fibertop C:\ /s/e/y";
                proc.StandardInput.WriteLine(dosLine);

                // 延时
                Thread.Sleep(300);

                // 断开共享文件夹
                dosLine = @"net use \\" + sqlserver_comboBox.Text + @"\飞思卓共享文件 /del";
                proc.StandardInput.WriteLine(dosLine);

                proc.StandardInput.WriteLine("exit");
                //proc.StandardInput.Close();
                proc.WaitForExit();

                string str = proc.StandardOutput.ReadToEnd();
                if (str.Contains("复制了 0 个文件")) // 复制文件失败
                {
                    Flag = false;
                }
                else
                {
                    Flag = true;
                }
                MessageBox.Show(str); // 运行信息显示
            }
            catch (Exception ex)
            {
                Flag = false;
                throw ex;
            }
            finally
            {
                proc.Close();
                proc.Dispose();
            }

            return Flag;
        }
        private void Language_Chinese()
        {
            ok_button.Text = "确定";
            cancel_button.Text = "退出";
            testSQL_button.Text = "测试连接";
            update_button.Text = "从服务器更新";
            label1.Text = "服务器IP地址";
            label8.Text = "模块类型";
            groupBox3.Text = "测试工序";
            radioButton3.Text = "初测";
            radioButton4.Text = "终测";
            //radioButton1.Text = "USB";
            //radioButton2.Text = "并口";
            groupBox2.Text = "生产单号信息";
            label2.Text = "生产单号";
            readFibertopbn_button1.Text = "读取";
            label3.Text = "Tosa批次号";
            label5.Text = "Bosa";
            label4.Text = "Rosa批次号";
            label6.Text = "PCBA批次号";
            label7.Text = "外壳批次号";
        }

        private void Language_English()
        {
            ok_button.Text = "OK";
            cancel_button.Text = "NO";
            testSQL_button.Text = "Test Connect";
            update_button.Text = "Update From Server";
            label1.Text = "Server IP ";
            label8.Text = "Module Type";
            groupBox3.Text = "Test Procedure";
            radioButton3.Text = "FirstTest";
            radioButton4.Text = "FinalTest";
            //radioButton1.Text = "USB";
            //radioButton2.Text = "Parallel Port";
            groupBox2.Text = "Production Order Number Information";
            label2.Text = "Producation Order";
            readFibertopbn_button1.Text = "Read";
            label3.Text = "Tosa Batch No";
            label5.Text = "Bosa";
            label4.Text = "Rosa Batch No";
            label6.Text = "PCBA Batch No";
            label7.Text = "Shell Batch No";
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            GlobalVarFun.Evb = evb as EVB;

            

            ///////////////////////////////////////////////////////////
            GlobalVarFun.testType = "";
            if (radioButton3.Checked) // 初测
            {
                GlobalVarFun.testType = "firstTest";
            }

            if (radioButton4.Checked) // 终测
            {
                GlobalVarFun.testType = "finalTest";
            }
            ///////////////////////////////////////////////////////////

            try
            {
                
            }
            catch(Exception ex)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show(ex.ToString()+"通信端口初始化失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (GlobalVarFun.Language == "English")
                {
                    MessageBox.Show("Failed to initialize the communication port !\r\n Program exit", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Application.Exit();
            }

            if (type_comboBox.Text == "SFP-UX3320T")
            {
                GlobalVarFun.mTest = new SFPUX3320T() as ModuleTest;
                GlobalVarFun.mTest2 = new SFPUX3320T_Dut2() as ModuleTest;
                GlobalVarFun.mTest3 = new SFPUX3320T_Dut3() as ModuleTest;
                GlobalVarFun.mTest4 = new SFPUX3320T_Dut4() as ModuleTest;
            }
            else if (type_comboBox.Text == "SFPP-GN1196")
            {
                GlobalVarFun.mTest = new SFPPGN1196() as ModuleTest;
                GlobalVarFun.mTest2 = new SFPPGN1196_Dut2() as ModuleTest;
                GlobalVarFun.mTest3 = new SFPPGN1196_Dut3() as ModuleTest;
                GlobalVarFun.mTest4 = new SFPPGN1196_Dut4() as ModuleTest;
            }
            else
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("模块类型初始化失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (GlobalVarFun.Language == "English")
                {
                    MessageBox.Show("Description Failed to initialize the module type!\r\n Program exit", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Application.Exit();
            }

            // Access 数据库存放地址
            GlobalVarFun.moduleLutDBFilePath = moduleTypeDBName[type_comboBox.SelectedIndex];
            GlobalVarFun.access_connect_status = true;

            // 判断网络数据库是否存在
            if (sqlserver_comboBox.Text.Trim() == "" || sqlserver_comboBox.Text == "null")
            {
                GlobalVarFun.sqlconnection = null;
            }

            TestResult.fibertop_bn = bn_textBox.Text;
        }

        private void testSQL_button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sqlserver_comboBox.Text) || sqlserver_comboBox.Text == "null")
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("请选择SQL数据库服务器！\r\n", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (GlobalVarFun.Language == "English")
                {
                    MessageBox.Show("Please select SQL Database server!\r\n", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//请选择SQL数据库服务器！
                }
                return;
            }

            // 服务器 IP 测试是否通畅
            if (TestServerIPonline() == false)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("服务器IP地址ping不通，网络不畅通，请检查网络连接！\r\n", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (GlobalVarFun.Language == "English")
                {
                    MessageBox.Show("The IP address of the server cannot be pinged, and the network is disconnected. Please check the network connection.\r\n", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Error);//服务器IP地址ping不通，网络不畅通，请检查网络连接！
                }
                return;
            }

            try
            {
                Services sqlserver = new Services();
                sqlserver.ServersOpen();
                sqlserver.ServersClose();
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("测试连接成功", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                if (GlobalVarFun.Language == "English")
                {
                    MessageBox.Show("Test connection successful", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Information);//测试连接成功
                }
            }
            catch (Exception exception)
            {
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("测试连接失败，请确认SQL数据库IP地址正确！！\r\n" + exception.Message, "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (GlobalVarFun.Language == "English")
                {
                    MessageBox.Show("Test connection failed, please confirm the SQL database IP address is correct!!\r\n" + exception.Message, "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);//测试连接失败，请确认SQL数据库IP地址正确！！
                }
            }
        }

        private void update_button_Click(object sender, EventArgs e)
        {
            int i = 0;

            if (string.IsNullOrEmpty(sqlserver_comboBox.Text) || sqlserver_comboBox.Text == "null")
            {
                GlobalVarFun.access_updated_status = false;
                if (GlobalVarFun.Language == "Chinese")
                {
                    MessageBox.Show("未选择服务器IP，系统将不更新本机Access文件，请确认！", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if (GlobalVarFun.Language == "English")
                {
                    MessageBox.Show("If you do not select the server IP address, the system will not update the local Access file, please confirm!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);//未选择服务器IP，系统将不更新本机Access文件，请确认！
                }

            }
            else
            {
                // 服务器IP 测试是否通畅
                if (TestServerIPonline() == false)
                {
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        MessageBox.Show("服务器IP地址ping不通，网络不畅通，请检查网络连接！\r\n", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    if (GlobalVarFun.Language == "English")
                    {
                        MessageBox.Show("The IP address of the server cannot be pinged, and the network is disconnected. Please check the network connection.\r\n", "Test Result", MessageBoxButtons.OK, MessageBoxIcon.Error);//服务器IP地址ping不通，网络不畅通，请检查网络连接！
                    }
                    return;
                }
                //
                // 更新服务器的Access数据库文件到本机
                if (CopyShareDBFileToLocal())
                {
                    GlobalVarFun.access_updated_status = true;
                }
                else
                {
                    GlobalVarFun.access_updated_status = false;
                    if (GlobalVarFun.Language == "Chinese")
                    {
                        if (MessageBox.Show("操作失败：从服务器更新Access文件到本地失败，请确认是否继续？？？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        {
                            return;
                        }
                    }
                    if (GlobalVarFun.Language == "English")
                    {
                        if (MessageBox.Show("Operation failed: Update Access file from server to local failed, please confirm whether to continue??", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)//操作失败：从服务器更新Access文件到本地失败，请确认是否继续？？？
                        {
                            return;
                        }
                    }


                }
            }

            type_comboBox.Items.Clear();

            try
            {
                OleDbConnection dbconnect;
                OleDbCommand dbcommand;
                OleDbDataAdapter dbadapter;
                DataSet dbset;

                string dbconnectionstr = "";

                dbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + filePath + "SupportedInfo.mdb");
                dbconnectionstr = string.Format("select ModuleType,AccessFilePath from [TypeInfo]");

                dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, "TypeInfo");
                //
                foreach (DataRow dataRow in dbset.Tables["TypeInfo"].Rows)
                {
                    if (dataRow["ModuleType"].ToString() != "")
                    {
                        type_comboBox.Items.Add(dataRow["ModuleType"]);
                        moduleTypeDBName[i] = filePath + Convert.ToString(dataRow["AccessFilePath"]);
                        i++;
                    }
                }

                dbconnect.Close();
                dbcommand.Dispose();
                dbadapter.Dispose();
                dbset.Dispose();
            }
            catch (Exception exp)
            {
                ok_button.Enabled = false;
                MessageBox.Show(exp.Message);
                return;
            }
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {

        }

        private void cbBLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbBLanguage.Text == "Chinese")
            {
                Language_Chinese();
            }
            if (cbBLanguage.Text == "English")
            {
                Language_English();
            }
            GlobalVarFun.Language = cbBLanguage.Text;
        }

        private void type_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (type_comboBox.SelectedIndex >= 0)
            {
                GlobalVarFun.moduleType = type_comboBox.Text;
                ok_button.Enabled = true;
            }
        }

        private void rBUSB1_CheckedChanged(object sender, EventArgs e)
        {
            //if (rBUSB1.Checked)
            //{
            //    cbBUSB1.Enabled = true;
            //}
            //else
            //{
            //    cbBUSB1.Enabled = false;
            //}
        }

        private void rBUSB2_CheckedChanged(object sender, EventArgs e)
        {
            //if (rBUSB2.Checked)
            //{
            //    cbBUSB2.Enabled = true;
            //}
            //else
            //{
            //    cbBUSB2.Enabled = false;
            //}
        }

        private void LoginFrm_Load(object sender, EventArgs e)
        {
            cbBLanguage.SelectedIndex = 0;
        }

        private void sqlserver_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            GlobalVarFun.sqlserver_ip = sqlserver_comboBox.Text.Trim();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string heaterIP = textBox5.Text.Trim();
            if (evb.Open(heaterIP))
            {
                MessageBox.Show("加热台连接成功!");
            }
            else
            {
                MessageBox.Show("加热台连接失败!");
            }
                
        }
    }
}
