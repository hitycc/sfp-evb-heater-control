/*using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace I2CLogAnalyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 【选择日志文件】按钮点击事件
        private void btnSelectFile_Click(object sender, EventArgs e)
        {

        }

        // 【解析并导出结果】按钮点击事件
        private void btnParseExport_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 解析单段Start~Stop完整I2C帧，兼容标准复合读时序、单写时序
        /// 规则：
        /// 1. 纯写：Address write + Data write(寄存器) + Data write(写入值)
        /// 2. 复合读：写从机AC+寄存器 → Repeat Start读AD+Data read(读出值)，输出从机统一用AC
        /// 3. 末尾单字节NACK属于正常读时序，不丢弃记录；无有效读写数据才返回空
        /// </summary>
        private string ParseI2CFrame(List<string> frameCmds)
        {
            string writeSlaveAddr = null;   // 写阶段从机（最终输出使用，读/写统一）
            string readSlaveAddr = null;    // 读阶段从机（仅识别，不输出）
            string regAddr = null;          // 寄存器地址（第一段Data write）
            string writeData = null;        // 写入数据（第二段Data write）
            string readData = null;         // 读出数据（Data read）
            bool hasReadFlow = false;       // 是否存在重复起始读流程

            foreach (string cmd in frameCmds)
            {
                // 记录写从机
                if (cmd.StartsWith("Address write:"))
                {
                    writeSlaveAddr = cmd.Split(':')[1].Trim();
                }
                // 记录读从机、标记读流程开启
                else if (cmd.StartsWith("Address read:"))
                {
                    readSlaveAddr = cmd.Split(':')[1].Trim();
                    hasReadFlow = true;
                }
                // Data write 分两段：第一段=寄存器，第二段=写入值
                else if (cmd.StartsWith("Data write:"))
                {
                    string val = cmd.Split(':')[1].Trim();
                    if (regAddr == null)
                        regAddr = val;
                    else
                        writeData = val;
                }
                // Data read 存储读出数值
                else if (cmd.StartsWith("Data read:"))
                {
                    readData = cmd.Split(':')[1].Trim();
                }
            }

            // 校验基础必备参数：写从机 + 寄存器必须存在
            if (writeSlaveAddr == null || regAddr == null)
                return null;

            // 分支1：完整读时序（有Repeat Start读、读到数据）
            if (hasReadFlow && readData != null)
            {
                return $"【读】从机0x{writeSlaveAddr} 寄存器0x{regAddr} 读出:0x{readData}";
            }
            // 分支2：纯写时序（无读流程、存在写入数据）
            else if (!hasReadFlow && writeData != null)
            {
                return $"【写】从机0x{writeSlaveAddr} 寄存器0x{regAddr} 写入:0x{writeData}";
            }

            // 不满足完整读写条件，丢弃本条
            return null;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace I2CLogAnalyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 【选择日志文件】按钮点击事件
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "文本文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            openDialog.Title = "选择I2C时序日志txt文件";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openDialog.FileName;
                rtbResult.Clear();
            }
        }

        // 【解析并导出结果】按钮点击事件
        private void btnParseExport_Click(object sender, EventArgs e)
        {
            string sourceTxtPath = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(sourceTxtPath) || !File.Exists(sourceTxtPath))
            {
                MessageBox.Show("请先选择有效的I2C日志txt文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] allLines = File.ReadAllLines(sourceTxtPath);
            List<string> singleFrameBuffer = new List<string>();
            List<string> outputList = new List<string>();

            foreach (string line in allLines)
            {
                string trimLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimLine))
                    continue;

                string[] splitData = trimLine.Split(',');
                if (splitData.Length < 3)
                    continue;
                string cmdText = splitData[2].Trim();

                singleFrameBuffer.Add(cmdText);

                if (cmdText == "Stop")
                {
                    string parseResult = ParseI2CFrame(singleFrameBuffer);
                    if (!string.IsNullOrEmpty(parseResult))
                    {
                        outputList.Add(parseResult);
                    }
                    singleFrameBuffer.Clear();
                }
            }

            // 预览输出
            rtbResult.Clear();
            foreach (var item in outputList)
            {
                rtbResult.AppendText(item + Environment.NewLine);
            }

            // 创建输出目录
            string sourceDir = Path.GetDirectoryName(sourceTxtPath);
            string saveFolder = Path.Combine(sourceDir, "I2C_Analysis_Output");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            string sourceFileName = Path.GetFileNameWithoutExtension(sourceTxtPath);
            string exportFileName = $"{sourceFileName}_analysis_result.txt";
            string fullExportPath = Path.Combine(saveFolder, exportFileName);
            File.WriteAllLines(fullExportPath, outputList);

            MessageBox.Show($"解析完成！\n文件已导出至：\n{fullExportPath}\n内容使用制表符分隔，复制粘贴Excel自动分成4列", "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 解析单段Start~Stop完整I2C帧，兼容标准复合读时序、单写时序
        /// 规则：
        /// 1. 纯写：Address write + Data write(寄存器) + Data write(写入值)
        /// 2. 复合读：写从机AC+寄存器 → Repeat Start读AD+Data read(读出值)，输出从机统一用AC
        /// 3. 末尾单字节NACK属于正常读时序，不丢弃记录；无有效读写数据才返回空
        /// 输出格式：读	0xAC	0x74	0x43  制表符分隔四列
        /// </summary>
        private string ParseI2CFrame(List<string> frameCmds)
        {
            string writeSlaveAddr = null;   // 写阶段从机（最终输出使用，读/写统一）
            string readSlaveAddr = null;    // 读阶段从机（仅识别，不输出）
            string regAddr = null;          // 寄存器地址（第一段Data write）
            string writeData = null;        // 写入数据（第二段Data write）
            string readData = null;         // 读出数据（Data read）
            bool hasReadFlow = false;       // 是否存在重复起始读流程

            foreach (string cmd in frameCmds)
            {
                // 记录写从机
                if (cmd.StartsWith("Address write:"))
                {
                    writeSlaveAddr = cmd.Split(':')[1].Trim();
                }
                // 记录读从机、标记读流程开启
                else if (cmd.StartsWith("Address read:"))
                {
                    readSlaveAddr = cmd.Split(':')[1].Trim();
                    hasReadFlow = true;
                }
                // Data write 分两段：第一段=寄存器，第二段=写入值
                else if (cmd.StartsWith("Data write:"))
                {
                    string val = cmd.Split(':')[1].Trim();
                    if (regAddr == null)
                        regAddr = val;
                    else
                        writeData = val;
                }
                // Data read 存储读出数值
                else if (cmd.StartsWith("Data read:"))
                {
                    readData = cmd.Split(':')[1].Trim();
                }
            }

            // 校验基础必备参数：写从机 + 寄存器必须存在
            if (writeSlaveAddr == null || regAddr == null)
                return null;

            // 分支1：完整读（仅修改输出拼接字符串，解析逻辑完全不变）
            if (hasReadFlow && readData != null)
            {
                return $"读\t0x{writeSlaveAddr}\t0x{regAddr}\t0x{readData}";
            }
            // 分支2：纯写（仅修改输出拼接字符串，解析逻辑完全不变）
            else if (!hasReadFlow && writeData != null)
            {
                return $"写\t0x{writeSlaveAddr}\t0x{regAddr}\t0x{writeData}";
            }

            // 不满足完整读写条件，丢弃本条
            return null;
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}