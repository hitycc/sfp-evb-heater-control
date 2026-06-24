using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace _03_TestFunction_Demo
{
    public partial class Form1 : Form
    {
        // 存储模拟测试数据列表，用于导出CSV报告
        // 每条数据：模块编号、电压、温度、测试结果
        private List<string[]> testDataList = new List<string[]>();
        public Form1()
        {
            InitializeComponent();
            // 初始化：自动创建今日日志文件夹
            CreateLogFolder();
            // 启动计时器，刷新状态栏时间
            timerStatus.Start();
        }
        /// <summary>
        /// 程序启动自动生成日志文件夹，格式：Logs/2026-06-23
        /// Path类：专门处理文件路径，跨系统兼容，不用手动拼接斜杠
        /// </summary>
        private void CreateLogFolder()
        {
            // 获取程序运行根目录
            string basePath = Application.StartupPath;
            // 拼接Logs总文件夹路径
            string logRoot = Path.Combine(basePath, "Logs");
            // 获取当天日期字符串 yyyy-MM-dd
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            // 拼接当日日志文件夹完整路径
            string todayLogPath = Path.Combine(logRoot, today);
            // 判断文件夹不存在则创建
            if (!Directory.Exists(todayLogPath))
            {
                Directory.CreateDirectory(todayLogPath);
            }
            // 存储全局，后续保存文件使用
            GlobalLogPath = todayLogPath;
        }
        // 全局存储当日日志路径
        private string GlobalLogPath = "";
        /// <summary>
        /// 每秒执行一次，刷新状态栏系统时间
        /// </summary>
        private void timerStatus_Tick(object sender, EventArgs e)
        {
            string nowTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            lblTime.Text = $"时间：{nowTime}";
        }

        private void btnStartTest_Click(object sender, EventArgs e)
        {
            // 重置进度条、测试数据
            progressTest.Value = 0;
            testDataList.Clear();
            lblTestStatus.Text = "测试：运行中";
            lblDevStatus.Text = "设备：已连接";
            AddLog("===== 开始批量SFP模块自动测试 =====");
            // 模拟10个光模块循环测试
            int totalModule = 10;
            for (int i = 1; i <= totalModule; i++)
            {
                // 模拟读取硬件数据
                string moduleId = $"SFP-{i:D2}";
                string volt = (3.2 + i * 0.5).ToString("0.00");
                string temp = (25 + i).ToString("0.0");
                string result = "合格";
                //存入测试数据集合，用于导出报告
                testDataList.Add(new string[] { moduleId, volt, temp, result });
                // 更新进度条
                progressTest.Value = i * 100 / totalModule;
                AddLog($"测试模块{moduleId}|电压：{volt}V|温度{temp}℃|{result}");
                // 模拟硬件读取延时，肉眼可见进度条走动
                System.Threading.Thread.Sleep(200);
                // 刷新界面，防止窗口假死
                Application.DoEvents();
            }
            lblTestStatus.Text = "测试：完成";
            AddLog("===== 全部模块测试完成 =====");
        }
        private void AddLog(string msg)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd");
            string logText = $"[{time}]{msg}\r\n";
            rtbLog.AppendText(logText);
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();
        }

        private void btnSaveTxtLog_Click(object sender, EventArgs e)
        {
            try
            {
                // 拼接日志文件名：时:分:秒.txt
                string fileName = DateTime.Now.ToString("yyyy-MM-dd")+"_运行日志.txt";
                string fullPath = Path.Combine(GlobalLogPath, fileName);
                // 创建写入流，写入日志框全部内容
                using (StreamWriter sw = new StreamWriter(fullPath, false, Encoding.UTF8))
                {
                    sw.Write(rtbLog.Text);
                }
                MessageBox.Show($"TXT日志保存成功！\n路径：{fullPath}", "保存成功");
                AddLog($"已保存成功文件{fileName}");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"保存失败{ex.Message}", "错误");
            }
        }
        private void btnSaveCsvReport_Click(object sender, EventArgs e)
        {
            if(testDataList.Count==0)
            {
                MessageBox.Show("暂无数据，请先执行批量测试");
                return;
            }
            try
            {
                string fileName = DateTime.Now.ToString("HH-mm-ss") + "_测试报告.csv";
                string fullPath = Path.Combine(GlobalLogPath, fileName);
                using (StreamWriter sw = new StreamWriter(fullPath,false,Encoding.UTF8))
                {
                    //写入CSV表头（逗号分隔）
                    sw.WriteLine("模块编号,电压（V）,温度（℃）,测试结果");
                    //循环写入每条测试数据
                    foreach(string[] dataRow in testDataList )
                    {
                        sw.WriteLine($"{dataRow[0]},{dataRow[1]},{dataRow[2]},{dataRow[3]}");
                    }
                }
                MessageBox.Show($"CSV报告导出成功！可使用Excel打开\n路径：{fullPath}", "导出完成");
                AddLog($"已导出CSV测试报告：{fileName}");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"导出失败{ex.Message}", "错误");
            }
        }
    }
}
