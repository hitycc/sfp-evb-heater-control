// =============================================================================
// Login_Form.cs - 登录/初始化配置窗口
// =============================================================================
// 功能说明：
//   这是软件启动后第一个显示的窗口，负责在进入主测试界面之前完成所有初始化工作：
//     1. 配置并测试SQL数据库服务器连接
//     2. 从服务器同步Access数据库文件到本地（包含模块型号参数表）
//     3. 配置并测试OTP12温控设备连接（模块温度控制）
//     4. 配置并测试SFP_EVB加热台连接（I2C通信通过加热台转发）
//     5. 选择模块型号（QSFP等）、测试类型（初测/终测）
//     6. 输入批次号、操作员信息
//     7. 初始化I2C通信通道和模块测试对象
//   所有初始化成功后，才进入Main_Form主测试界面。
//
// 硬件连接关系：
//   PC(软件) ──TCP/IP──→ SFP_EVB_Heater(加热台) ──I2C──→ 光模块
//   PC(软件) ──TCP/IP──→ OTP12(温控仪)
//   PC(软件) ──TCP/IP──→ SQL Server数据库
// =============================================================================

using System;                          // C#基础类型（string、int、bool、Exception等）
using System.Collections.Generic;      // 泛型集合类（List<>、Dictionary<>等）
using System.ComponentModel;           // 组件模型（WinForms控件基础特性）
using System.Drawing;                  // GDI+绘图（颜色、尺寸等，用于UI）
using System.Linq;                     // LINQ查询扩展
using System.Text;                     // 文本编码处理（StringBuilder等）
using System.Windows.Forms;            // Windows Forms UI框架（Form、Button、MessageBox等所有界面控件）
using System.Data;                     // ADO.NET数据模型（DataSet、DataTable、DataRow等）
using System.Data.OleDb;               // OLE DB数据提供程序（用于连接Access数据库.mdb文件）
using System.Data.Sql;                 // SQL Server基础命名空间
using System.Data.SqlClient;           // SQL Server数据提供程序（SqlConnection、SqlCommand等）
using System.Threading;               // 线程操作（Thread.Sleep延时等待）
using System.Diagnostics;              // 诊断工具（Process类，用于启动cmd.exe执行DOS命令）
using System.Net.NetworkInformation;   // 网络诊断（Ping类，用于测试服务器IP是否可达）
using FibertopTest_Common;             // 公司公共库（GlobalVarFun、TestResult、TestSet、DOA等静态类）

namespace SFP模块终测检查软件            // 命名空间：本项目所有类都在此命名空间下
{
    /// <summary>
    /// 登录/初始化窗口类（partial表示类定义分散在多个文件中，
    /// 另一部分在Login_Form.Designer.cs中由VS设计器自动维护）
    /// </summary>
    public partial class Login_Form : Form
    {
        // filePath: Access数据库文件在本地磁盘的存放路径
        // 原始设计是从服务器共享文件夹(X:\Fibertop\)复制到本地C:\Fibertop\，
        // 避免网络抖动影响测试。数据库文件SupportedInfo.mdb中存储了支持的
        // 模块型号列表以及每个型号对应的参数表文件路径。
        //string filePath = "X:\\Fibertop\\"; // 网络数据库镜像（映射网络驱动器方式，已弃用）
        string filePath = "C:\\Fibertop\\";   // 本地数据库镜像（从服务器复制到本地C盘使用）

        // moduleTypeDBName: 存储每个模块型号对应的Access参数表文件的完整本地路径
        // 数组长度固定20，表示最多支持20种模块型号。
        // 当用户点"更新"按钮后，从SupportedInfo.mdb的TypeInfo表中读取并填充此数组。
        // 在ok_button_Click中根据type_comboBox选中的索引取出对应路径。
        string[] moduleTypeDBName = new string[20];


        /// <summary>
        /// Login_Form构造函数 - WinForms窗体的入口，在new Login_Form()时被调用
        /// </summary>
        public Login_Form()
        {
            InitializeComponent();        // 调用VS设计器自动生成的方法：根据Designer.cs中的定义
                                          // 创建并初始化所有界面控件（按钮、文本框、下拉框等）
                                          // 这行必须放在最前面，否则控件为null会报NullReferenceException

            //下面这个是新增的
            Login_Form_Load(null, null);  // 手动直接调用Load事件处理方法，传入null表示没有事件参数
                                          // 原因：正常情况下WinForms只在窗体第一次ShowDialog时触发Load事件。
                                          // 但如果在while循环中反复ShowDialog同一个Login_Form实例，
                                          // 第二次及以后不会再触发Load事件，所以这里在构造函数里手动调用一次，
                                          // 确保每次新建实例都会执行初始化（设置下拉框默认项、创建设备对象、填充默认IP等）
        }


        /// <summary>
        /// Login_Form_Load - 窗体加载事件处理
        /// 完成界面控件初始状态设置和设备对象创建
        /// </summary>
        private void Login_Form_Load(object sender, EventArgs e)
        {
            sqlserver_comboBox.SelectedIndex = 0;  // SQL服务器下拉框默认选中第一项（第一个预设IP地址）

            // 初始化两个设备对象（注意：这里只是new创建C#对象，还没有真正建立TCP连接）
            // 真正的连接是在ok_button_Click中调用Connect/Open时才建立的
            GlobalVarFun.otp12 = new OTP12Driver();    // OTP12：高精度温控仪，TCP/IP连接，
                                                       // 用于控制模块温度（高温/低温/常温测试）
            GlobalVarFun.heater = new SFP_EVB_Heater();// SFP_EVB_Heater：SFP评估板加热台，TCP/IP连接，
                                                       // 它不仅控制加热，还充当PC与光模块之间的I2C通信桥接器：
                                                       // PC通过TCP发命令给Heater，Heater再通过硬件I2C读写模块寄存器

            // 设置设备默认IP地址（用户可在界面文本框中修改，这些是车间工位预设值）
            textBox_otp12Ip.Text = "192.168.100.156";  // OTP12温控仪的默认IP地址
            textBox_heaterIp.Text = "129.168.1.133";   // SFP_EVB加热台的默认IP地址
        }


        /// <summary>
        /// sqlserver_comboBox_SelectedIndexChanged - SQL服务器下拉框选项变化事件
        /// 当用户切换下拉框中的服务器IP时触发，更新全局SQL连接对象。
        /// 注意：这里只是创建SqlConnection对象并设置连接字符串，并没有真正Open()
        /// </summary>
        private void sqlserver_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // SQL 数据库信息 2020.12.11修改新的用户操作数据库
            // 连接字符串格式：server=服务器IP; uid=用户名; pwd=密码; database=数据库名
            //GlobalVarFun.sqlconnection = new SqlConnection("server=" + sqlserver_comboBox.Text + ";uid=sa;pwd=fiber123;database=SFP");
            GlobalVarFun.sqlconnection = new SqlConnection(
                "server=" + sqlserver_comboBox.Text +   // 从下拉框获取服务器IP地址
                ";uid=tester;" +                        // 数据库登录用户名（从原sa改为tester）
                "pwd=fibertop2020;" +                   // 数据库登录密码
                "database=SFP");                        // 默认连接SFP数据库（存储测试记录）
        }


        /// <summary>
        /// button_testOtp12_Click - "测试OTP12"按钮点击事件
        /// 用户输入OTP12的IP后，点击此按钮测试能否成功连接。
        /// 这是预检功能，让用户在点"确定"前就知道OTP12是否连通
        /// </summary>
        private void button_testOtp12_Click(object sender, EventArgs e)
        {
            string ip = textBox_otp12Ip.Text.Trim();  // 获取IP文本框内容，Trim去掉前后空格
            if (string.IsNullOrEmpty(ip))              // IP为空则提示用户输入
            {
                MessageBox.Show("请输入OTP12的IP地址！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 调用OTP12Driver.Connect尝试TCP连接，返回bool：true=成功，false=失败
                if (GlobalVarFun.otp12.Connect(ip))
                {
                    MessageBox.Show("OTP12连接成功！", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("OTP12连接失败，请检查IP地址和网络！", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)  // 捕获连接过程中抛出的异常（网络不通、超时、拒绝连接等）
            {
                MessageBox.Show("OTP12连接出错：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// button_testHeater_Click - "测试加热台"按钮点击事件
        /// 测试SFP_EVB_Heater加热台的TCP连接是否正常。
        /// 加热台是最关键的设备，因为I2C通信也要通过它转发
        /// </summary>
        private void button_testHeater_Click(object sender, EventArgs e)
        {
            string ip = textBox_heaterIp.Text.Trim();  // 获取加热台IP
            if (string.IsNullOrEmpty(ip))               // IP为空则提示
            {
                MessageBox.Show("请输入加热台的IP地址！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 调用SFP_EVB_Heater.Open方法尝试TCP连接（注意Heater用Open不是Connect）
                if (GlobalVarFun.heater.Open(ip))
                {
                    MessageBox.Show("加热台连接成功！", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("加热台连接失败，请检查IP地址和网络！", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加热台连接出错：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// testSQL_button_Click - "测试SQL"按钮点击事件
        /// 测试SQL Server数据库是否可连接。
        /// 步骤：1.检查是否选了服务器 → 2.Ping测试IP → 3.尝试Open/Close数据库连接
        /// </summary>
        private void testSQL_button_Click(object sender, EventArgs e)
        {
            // 第一步：检查是否选择了SQL服务器
            if (string.IsNullOrEmpty(sqlserver_comboBox.Text) || sqlserver_comboBox.Text == "null")
            {
                MessageBox.Show("请选择SQL数据库服务器！\r\n", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 第二步：Ping服务器IP看网络是否通畅（比直接连数据库更快定位问题）
            if (TestServerIPonline() == false)
            {
                MessageBox.Show("服务器IP地址ping不通，网络不畅通，请检查网络连接！\r\n", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 第三步：真正尝试打开数据库连接
            try
            {
                GlobalVarFun.sqlconnection.Open();   // 打开SQL连接
                GlobalVarFun.sqlconnection.Close();  // 立即关闭（只是测试连通性）
                MessageBox.Show("测试连接成功", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show("测试连接失败，请确认SQL数据库IP地址正确！！\r\n" + exception.Message, "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        /// <summary>
        /// TestServerIPonline - 测试服务器IP是否能Ping通
        /// 使用ICMP协议发送Ping包检测网络可达性
        /// </summary>
        /// <returns>true=Ping成功(IP可达)，false=Ping失败</returns>
        private bool TestServerIPonline()
        {
            try
            {
                Ping ping = new Ping();  // 创建Ping对象
                // Send发送ICMP Echo请求到服务器IP，返回PingReply
                PingReply pingReply = ping.Send(sqlserver_comboBox.Text);
                ping.Dispose();  // 释放Ping对象资源
                if (pingReply.Status != IPStatus.Success)
                {
                    return false;  // Ping失败（超时、目标不可达等）
                }
            }
            catch // Ping可能抛异常（网络未连接、DNS失败等），统一视为不可达
            {
                return false;
            }
            return true;  // Ping成功
        }


        /// <summary>
        /// CopyShareDBFileToLocal - 从服务器共享文件夹复制Access数据库文件到本机C盘
        /// 
        /// 为什么要复制到本地？
        ///   Access数据库(.mdb)通过网络共享直接访问不稳定，网络闪断可能导致数据损坏
        ///   或测试程序崩溃。所以先复制到本地C:\Fibertop\，测试读取本地文件更稳定。
        ///   
        /// 实现方式：启动cmd.exe执行DOS命令：
        ///   1. net use 连接共享文件夹（带用户名密码认证）
        ///   2. xcopy 复制整个Fibertop目录到C盘（/s/e/y：含子目录、含空目录、覆盖不提示）
        ///   3. net use /del 断开共享连接
        /// </summary>
        /// <returns>true=复制成功，false=复制失败</returns>
        private bool CopyShareDBFileToLocal()
        {
            Process proc = new Process();  // Process用于启动外部进程（cmd.exe）
            string dosLine;                // 暂存DOS命令字符串
            bool Flag = false;             // 返回值标志，默认失败
            try
            {
                // 配置Process启动信息
                proc.StartInfo.FileName = "cmd.exe";              // 启动cmd.exe
                proc.StartInfo.UseShellExecute = false;            // 必须false才能重定向输入输出
                proc.StartInfo.RedirectStandardInput = true;       // 允许向cmd写入命令
                proc.StartInfo.RedirectStandardOutput = true;      // 允许读取cmd输出
                proc.StartInfo.RedirectStandardError = true;       // 允许读取错误输出
                proc.StartInfo.CreateNoWindow = true;              // 后台执行，不显示黑屏窗口
                proc.Start();                                      // 启动cmd进程

                // 命令1：net use连接服务器共享文件夹
                // 格式：net use \\IP\共享名 "密码" /user:"用户名"
                dosLine = @"net use \\" + sqlserver_comboBox.Text + @"\Fibertop ""test2016"" /user:""fibertop""";
                proc.StandardInput.WriteLine(dosLine);
                Thread.Sleep(1300);  // 等待网络连接认证完成

                // 命令2：xcopy复制共享文件夹到本地C盘
                // /s=含子目录 /e=含空目录 /y=覆盖不提示
                dosLine = @"xcopy \\" + sqlserver_comboBox.Text + @"\Fibertop C:\ /s/e/y";
                proc.StandardInput.WriteLine(dosLine);
                Thread.Sleep(300);  // 等待复制完成

                // 命令3：断开网络共享连接
                dosLine = @"net use \\" + sqlserver_comboBox.Text + @"\飞思卓共享文件 /del";
                proc.StandardInput.WriteLine(dosLine);

                // 命令4：exit退出cmd
                proc.StandardInput.WriteLine("exit");
                proc.WaitForExit();  // 等待cmd进程退出

                // 读取输出判断复制结果
                string str = proc.StandardOutput.ReadToEnd();
                if (str.Contains("复制了 0 个文件"))  // xcopy中文输出，0个文件说明失败
                {
                    Flag = false;
                }
                else
                {
                    Flag = true;
                }
                MessageBox.Show(str); // 显示cmd输出给用户确认
            }
            catch (Exception ex)
            {
                Flag = false;
                throw ex;
            }
            finally  // 无论成功失败都清理资源
            {
                proc.Close();
                proc.Dispose();
            }

            return Flag;
        }


        /// <summary>
        /// update_button_Click - "更新"按钮点击事件
        /// 非常重要的一步！用户必须先点"更新"才能选择模块型号。
        /// 功能：
        ///   1.（可选）从服务器更新Access数据库文件到本地
        ///   2. 读取本地SupportedInfo.mdb中的TypeInfo表
        ///   3. 将模块型号列表填充到"模块类型"下拉框
        ///   4. 记录每个型号的参数文件路径到moduleTypeDBName数组
        /// </summary>
        private void update_button_Click(object sender, EventArgs e)
        {
            int i = 0;  // 计数器：记录加载了多少个模块型号，同时作为数组索引

            // 检查是否选择了SQL服务器IP
            if (string.IsNullOrEmpty(sqlserver_comboBox.Text) || sqlserver_comboBox.Text == "null")
            {
                GlobalVarFun.access_updated_status = false;
                MessageBox.Show("未选择服务器IP，系统将不更新本机Access文件，请确认！", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // 选了服务器：先Ping测试网络
                if (TestServerIPonline() == false)
                {
                    MessageBox.Show("服务器IP地址ping不通，网络不畅通，请检查网络连接！\r\n", "测试结果", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // 从服务器复制Access数据库到本地
                if (CopyShareDBFileToLocal())
                {
                    GlobalVarFun.access_updated_status = true;
                }
                else
                {
                    GlobalVarFun.access_updated_status = false;
                    // 复制失败时询问用户是否继续（可能本地有旧文件可用）
                    if (MessageBox.Show("操作失败：从服务器更新Access文件到本地失败，请确认是否继续？？？", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        return;
                    }
                }
            }

            type_comboBox.Items.Clear();  // 清空之前的型号列表（防止重复添加）

            // 读取本地Access数据库(SupportedInfo.mdb)中的模块型号列表
            try
            {
                OleDbConnection dbconnect;     // 数据库连接对象
                OleDbCommand dbcommand;         // SQL命令对象
                OleDbDataAdapter dbadapter;     // 数据适配器（用于填充DataSet）
                DataSet dbset;                  // 数据集（内存中的数据缓存）

                string dbconnectionstr = "";

                // 连接到本地的SupportedInfo.mdb（Jet OLEDB 4.0是Access mdb的驱动）
                dbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + filePath + "SupportedInfo.mdb");
                // SQL查询：从TypeInfo表查ModuleType(型号名)和AccessFilePath(参数文件路径)
                dbconnectionstr = string.Format("select ModuleType,AccessFilePath from [TypeInfo]");

                dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, "TypeInfo");  // 执行查询填充DataSet（Fill自动Open/Close连接）

                // 遍历TypeInfo表的每一行
                foreach (DataRow dataRow in dbset.Tables["TypeInfo"].Rows)
                {
                    if (dataRow["ModuleType"].ToString() != "")  // 跳过空行
                    {
                        type_comboBox.Items.Add(dataRow["ModuleType"]); // 型号名加入下拉框
                        moduleTypeDBName[i] = filePath + Convert.ToString(dataRow["AccessFilePath"]); // 拼接完整路径
                        i++;
                    }
                }
                
                // 释放数据库资源
                dbconnect.Close();
                dbcommand.Dispose();
                dbadapter.Dispose();
                dbset.Dispose();                
            }
            catch (Exception exp)
            {
                ok_button.Enabled = false;  // 数据库读取失败时禁用"确定"按钮
                MessageBox.Show(exp.Message);
                return;
            }

            // 如果加载了5个以上型号，默认选中第5个（索引4），
            // 可能因为最常用的型号在第5个位置
            if (i >= 4)
            {
                type_comboBox.SelectedIndex = 4;
            }
        }


        /// <summary>
        /// ok_button_Click - "确定"按钮点击事件（最核心的方法！）
        /// 
        /// 进入主测试界面前的最后一步，完成所有关键初始化：
        ///   1. 连接OTP12温控仪
        ///   2. 连接SFP_EVB加热台
        ///   3. 通过加热台创建I2C通信通道(I2C_Heater)
        ///   4. 确定测试类型（初测firstTest/终测finalTest）
        ///   5. 测试I2C通信（Open/Close一次验证通道可用）
        ///   6. 根据模块型号创建测试对象(QSFP)
        ///   7. 设置Access参数文件路径、SQL连接状态、批次号
        /// </summary>
        private void ok_button_Click(object sender, EventArgs e)
        {
            // ====== 第1步：连接OTP12温控仪 ======
            string otp12Ip = textBox_otp12Ip.Text.Trim();
            if (!GlobalVarFun.otp12.Connect(otp12Ip))
            {
                MessageBox.Show("OTP12连接失败！\r\n请检查IP地址和网络连接", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ====== 第2步：连接SFP_EVB加热台 ======
            string heaterIp = textBox_heaterIp.Text.Trim();
            if (!GlobalVarFun.heater.Open(heaterIp))
            {
                MessageBox.Show("加热台连接失败！\r\n请检查IP地址和网络连接", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ====== 第3步：初始化I2C通信通道 ======
            // I2C_Heater是通过加热台转发I2C命令的实现类
            // cutrrentSlot：当前槽位号（加热台可能有多个插槽）
            GlobalVarFun.iic = new I2C_Heater(GlobalVarFun.heater, GlobalVarFun.cutrrentSlot);

            // ====== 第4步：确定测试类型（初测/终测） ======
            GlobalVarFun.testType = "";
            if (radioButton3.Checked) // 初测单选按钮
            {
                GlobalVarFun.testType = "firstTest";  // 初测调试模式（半成品板调试）
            }
            if (radioButton4.Checked) // 终测单选按钮
            {
                GlobalVarFun.testType = "finalTest";  // 终测检查模式（成品Pass/Fail判定）
            }

            // ====== 第5步：测试I2C通信是否正常 ======
            try
            {
                if (GlobalVarFun.iic.TWI_Open() == false)  // 尝试打开I2C通道
                {
                    throw new Exception();  // Open失败抛出异常
                }
                GlobalVarFun.iic.TWI_Close();  // 打开成功后立即关闭（仅做连通性测试）
            }
            catch
            {
                MessageBox.Show("I2C通信初始化失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();  // I2C完全不可用，无法与模块通信，直接退出程序
            }

            // ====== 第6步：根据模块型号创建测试对象 ======
            if (type_comboBox.Text == "QSFP")
            {
                // 创建QSFP测试对象并转型为ModuleTest基类
                // ModuleTest是所有测试类的基类，QSFP是具体实现
                GlobalVarFun.mTest = new QSFP() as ModuleTest;
            }        
            else
            {
                MessageBox.Show("模块类型初始化失败！\r\n程序退出", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            // ====== 第7步：设置Access数据库路径和SQL连接状态 ======
            GlobalVarFun.moduleLutDBFilePath = moduleTypeDBName[type_comboBox.SelectedIndex];
            GlobalVarFun.access_connect_status = true;
            
            if (sqlserver_comboBox.Text.Trim() == "" || sqlserver_comboBox.Text == "null")
            {
                GlobalVarFun.sqlconnection = null;  // 没选服务器则置空（Main_Form会检查）
            }

            // ====== 第8步：记录批次号 ======
            TestResult.fibertop_bn = bn_textBox.Text;  // 操作员输入的批次号，供测试记录使用
        }


        /// <summary>
        /// type_comboBox_SelectedIndexChanged - "模块类型"下拉框选项变化事件
        /// 用户选中型号后启用"确定"按钮（没选型号前按钮是禁用的，防止误操作）
        /// </summary>
        private void type_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (type_comboBox.SelectedIndex >= 0)  // SelectedIndex >= 0表示有选中项（-1=未选中）
            {
                GlobalVarFun.moduleType = type_comboBox.Text;  // 记录选中的型号到全局变量
                ok_button.Enabled = true;                       // 启用"确定"按钮
            }
        }


        /// <summary>
        /// readFibertopbn_button1_Click - "读取批次号"按钮点击事件
        /// 空方法，预留功能。可能原计划通过扫码枪自动读取批次号，目前未实现。
        /// </summary>
        private void readFibertopbn_button1_Click(object sender, EventArgs e)
        {
            // 空实现：预留接口
        }
    }
}