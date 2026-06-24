using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace _03_TestFunction_Demo
{
    public partial class Form1 : Form
    {
        // 存储所有测试结果，用于导出CSV表格
        private List<string[]> testDataList = new List<string[]>();
        // 测试状态标记：true正在测试，false空闲，防止重复点击启动测试
        private bool isTesting = false;
        // 停止测试标记，用来跳出循环
        private bool stopTestFlag = false;

        // 光模块阈值常量 电压3.0~3.6V，温度0~70℃
        private const double VoltMin = 3.0;
        private const double VoltMax = 3.6;
        private const double TempMin = 0;
        private const double TempMax = 70;

        // 全局存储当日日志文件夹路径
        private string GlobalLogPath = "";
        public Form1()
        {
            
            InitializeComponent();

        }
        private void CreateLogFolder()
        {
            // 获取程序exe所在文件夹
            string basePath = Application.StartupPath;
            // 拼接总日志文件夹
            string logRoot = Path.Combine(basePath, "Logs");
            // 获取当天日期字符串
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            // 当日完整文件夹路径
            string todayLogPath = Path.Combine(logRoot, today);
            // 文件夹不存在则创建
            if(!Directory.Exists(todayLogPath))
            {
                Directory.CreateDirectory(todayLogPath);
            }
            GlobalLogPath = todayLogPath;
        }
        /// <summary>
        /// 定时器每秒执行，刷新状态栏时间
        /// </summary>
        private void timerStatus_Tick(object sender, EventArgs e)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            lblTime.Text = $"时间：{nowTime}";
        }

        /// <summary>
        /// 开始批量测试按钮
        /// </summary>
        private void btnStartTest_Click(object sender, EventArgs e)
        {
            if(isTesting)
            {
                MessageBox.Show("测试正在运行中，请勿重复点击", "提示");
            }
            progressTest.Value = 0;
            isTesting = true;
            testDataList.Clear();
            stopTestFlag = false;

            // 修改状态文字、切换按钮可用状态
            lblTestStatus.Text = "测试：运行中";
            lblDevStatus.Text = "已连接";
            btnStartTest.Enabled = false;
            btnStopTest.Enabled = true;
            AddLog("===== 开始批量SFP模块自动测试 =====");

            int totalModule = 10;
            // for循环遍历测试10个模块
            for(int i=1;i<=totalModule;i++)
            {
                // 检测停止标记，用户点停止就跳出循环
                if (stopTestFlag)
                {
                    AddLog("用户手动终止测试！");
                    break;
                }

                // 模拟模块编号、电压、温度
                string moduleId = $"SFP-{i:D2}";
                double voltNum = 3.2 + i * 0.02;
                double tempNum = 22 + i * 1;
                //double voltNum = 3.2;
                //double tempNum = 22;
                string volt = voltNum.ToString("0.00");
                string temp = tempNum.ToString("0.0");
                string result = "合格";

                // 阈值判断
                bool voltOK = voltNum > VoltMin && voltNum < VoltMax;
                bool tempOK = tempNum > TempMin && tempNum < TempMax;
                if(!voltOK|!tempOK)
                {
                    result = "不合格";
                    MessageBox.Show($"模块{moduleId}参数超标！\n电压{voltNum}V 温度:{tempNum}℃","温度异常");
                }

                // 给标签赋值，合格绿色、不合格红色
                string showStr = $"模块{moduleId} 电压{voltNum} 温度{tempNum}";
                lblSingleData.Text = showStr;
                if(voltOK&&tempOK)
                {
                    lblSingleData.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblSingleData.ForeColor = System.Drawing.Color.Red;
                }
                // 存入数据集合
                testDataList.Add(new string[] { moduleId, volt, temp,result });

                // 更新进度条
                progressTest.Value = i * 100 / totalModule;
                AddLog($"测试模块{moduleId} | 电压:{volt} | 温度:{temp} | {result}");

                // 延时模拟硬件响应
                System.Threading.Thread.Sleep(200);
                // 刷新界面，防止Sleep卡死窗口
                Application.DoEvents();
            }

            //测试结束恢复所有状态
            isTesting = false;
            btnStartTest.Enabled = true;
            btnStopTest.Enabled = false;
            lblTestStatus.Text = "测试：空闲";
            lblDevStatus.Text="未连接";
            AddLog("===== 批量测试流程结束 =====");

        }
        
        /// /// <summary>
        /// 通用日志打印方法，带时间戳自动滚屏
        /// </summary>
        private void AddLog(string msg)
        {
            string time = DateTime.Now.ToString("HH-mm-ss");
            string logText = $"[{time}]{msg}\r\n";
            rtbLog.AppendText(logText);
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();
        }

        /// <summary>
        /// 停止测试按钮
        /// </summary>
        private void btnStopTest_Click(object sender, EventArgs e)
        {
            if(!isTesting)
            {
                MessageBox.Show("当前没有正在运行的测试", "提示");
                return;
            }
            stopTestFlag = true;
        }
        /// <summary>
        /// 保存TXT日志
        /// </summary>
        private void btnSaveTxtLog_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss") + "_运行日志.txt";
                string fullPath = Path.Combine(GlobalLogPath, fileName);
                using (StreamWriter sw = new StreamWriter(fullPath, false, Encoding.UTF8))
                {
                    sw.Write(rtbLog.Text);
                }
                MessageBox.Show($"文件保存成功!\n路径：{fullPath}", "保存成功");
                AddLog($"已保存文件：{fileName}");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误");
            }
        }
        /// <summary>
        /// 导出CSV测试报告
        /// </summary>
        private void btnSaveCsvReport_Click(object sender, EventArgs e)
        {
            if(testDataList.Count==0)
            {
                MessageBox.Show("暂无测试数据，请先执行批量测试", "提示");
                return;
            }
            try
            {
                string filename = DateTime.Now.ToString("HH-mm-ss") + "_测试报告.csv";
                string fullPath = Path.Combine(GlobalLogPath, filename);
                using(StreamWriter sw=new StreamWriter(fullPath,false,Encoding.UTF8))
                {
                    sw.Write("模块编号,电压（V）,温度（℃）,测试结果");
                    foreach(string[] row in testDataList)
                    {
                        sw.Write($"{row[0]},{row[1]}.{row[2]},{row[3]}");
                    }
                }
                MessageBox.Show($"CSV导出完成,可用Excel打开\n路径：{fullPath}", "导出成功");
                AddLog($"导出报告：{filename}");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"导出失败:{ex.Message}", "错误");
            }
        }
    }
}
