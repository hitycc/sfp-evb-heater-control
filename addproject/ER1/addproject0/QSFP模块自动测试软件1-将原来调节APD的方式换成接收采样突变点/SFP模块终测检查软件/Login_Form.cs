using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Threading;
using System.Diagnostics;
using System.Net.NetworkInformation;
using FibertopTest_Common;

namespace SFP模块终测检查软件
{
    public partial class Login_Form : Form
    {
        //string filePath = "X:\\Fibertop\\"; // 网络数据库镜像
        string filePath = "C:\\Fibertop\\"; // 本地数据库镜像
        string[] moduleTypeDBName = new string[20];

        public Login_Form()
        {
            InitializeComponent();
        }

        private void Login_Form_Load(object sender, EventArgs e)
        {
            sqlserver_comboBox.SelectedIndex = 0;
        }

        private void sqlserver_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SQL 数据库信息 2020.12.11修改新的用户操作数据库
            //GlobalVarFun.sqlconnection = new SqlConnection("server=" + sqlserver_comboBox.Text + ";uid=sa;pwd=fiber123;database=SFP");
            GlobalVarFun.sqlconnection = new SqlConnection("server=" + sqlserver_comboBox.Text + ";uid=tester;pwd=fibertop2020;database=SFP");
        }

        // 测试SQL数据库 连接
        private void testSQL_button_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(sqlserver_comboBox.Text) || sqlserver_comboBox.Text == "null")
            {
                MessageBox.Show("请选择SQL数据库服务器！\r\n", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 服务器IP 测试是否通畅
            if (TestServerIPonline() == false)
            {
                MessageBox.Show("服务器IP地址ping不通，网络不畅通，请检查网络连接！\r\n", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                GlobalVarFun.sqlconnection.Open();
                GlobalVarFun.sqlconnection.Close();
                MessageBox.Show("测试连接成功", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show("测试连接失败，请确认SQL数据库IP地址正确！！\r\n" + exception.Message, "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

        private void update_button_Click(object sender, EventArgs e)
        {
            int i = 0;

            if (string.IsNullOrEmpty(sqlserver_comboBox.Text) || sqlserver_comboBox.Text == "null")
            {
                GlobalVarFun.access_updated_status = false;
                MessageBox.Show("未选择服务器IP，系统将不更新本机Access文件，请确认！", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // 服务器IP 测试是否通畅
                if (TestServerIPonline() == false)
                {
                    MessageBox.Show("服务器IP地址ping不通，网络不畅通，请检查网络连接！\r\n", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    if (MessageBox.Show("操作失败：从服务器更新Access文件到本地失败，请确认是否继续？？？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return;
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
            if (i >= 4)
            {
                type_comboBox.SelectedIndex = 4;
            }
            /*// 默认选择 ER1（如果数据库中存在该型号）。不要使用固定下标，
            // 因为 TypeInfo 表的排序/记录数变化会导致下标 4 指向其他模块。
            int er1Index = type_comboBox.FindStringExact("QSFPER1");
            if (er1Index >= 0)
            {
                type_comboBox.SelectedIndex = er1Index;
            }
            else if (type_comboBox.Items.Count > 0)
            {
                type_comboBox.SelectedIndex = 0;
            }*/
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                GlobalVarFun.iic = new TWI() as I2C;
                GlobalVarFun.USBtoI2C = new CP2112();
                GlobalVarFun.usb_can_use = true;
                GlobalVarFun.usb_i2c_open = true;
            }

            if (radioButton2.Checked)
            {
                GlobalVarFun.iic = new TWI() as I2C;
            }

            if (radioButton5.Checked)
            {
                GlobalVarFun.iic = new CP2112() as I2C;
            }

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
                if (GlobalVarFun.iic.TWI_Open() == false)
                {
                    throw new Exception();
                }
                GlobalVarFun.iic.TWI_Close();
            }
            catch
            {
                MessageBox.Show("通信端口初始化失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            string selectedModuleType = type_comboBox.Text.Trim();
            if (selectedModuleType == "QSFPER1")
            {
                GlobalVarFun.mTest = new QSFPER1() as ModuleTest;
            }
            else if (selectedModuleType == "QSFP")
            {
                GlobalVarFun.mTest = new QSFP() as ModuleTest;
            }
            else
            {
                MessageBox.Show("模块类型初始化失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void type_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (type_comboBox.SelectedIndex >= 0)
            {
                GlobalVarFun.moduleType = type_comboBox.Text.Trim();
                ok_button.Enabled = true;
            }
        }

        private void readFibertopbn_button1_Click(object sender, EventArgs e)
        {
            //
        }
    }
}
