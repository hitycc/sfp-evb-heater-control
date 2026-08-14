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

namespace SFPXFP自动测试软件多端口
{
    public partial class LogFrm : Form
    {
        public LogFrm()
        {
            InitializeComponent();
        }

        private void LogFrm_Load(object sender, EventArgs e)
        {
            
        }
        // 加载现有日志文件
        private void LoadLogFile(string filePath)
        {
            try
            {
                // Clear existing content
                logRichTextBox.Clear();
                // Read the entire file content
                string logContent = File.ReadAllText(filePath, Encoding.UTF8);
                logContent = logContent.Replace("\0", "");
                // Append the content to the RichTextBox
                logRichTextBox.AppendText(logContent);
                // Scroll to the end (useful if loading an existing file)
                logRichTextBox.ScrollToCaret(); // Scrolls to the last appended text position (which is the end after Clear and AppendText)
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading log file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 追加新日志（例如，当有新的日志条目生成时）
        private void AppendLog(string logEntry)
        {
            // Use Invoke if calling from a non-UI thread
            if (logRichTextBox.InvokeRequired)
            {
                logRichTextBox.Invoke(new Action<string>(AppendLog), logEntry);
                return;
            }

            logRichTextBox.AppendText(logEntry + Environment.NewLine);
            // Optionally, scroll to the end after appending
            logRichTextBox.SelectionStart = logRichTextBox.Text.Length;
            logRichTextBox.ScrollToCaret();
        }

        private void 端口1日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
            string filename = "D:\\SFPXFPTesTLogDUT1.txt";
            try
            {
                if (File.Exists(filename))
                {
                    LoadLogFile(filename);
                }
                else
                {
                    MessageBox.Show("日志不存在");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("打开日志失败");
                return;
            }
        }

        private void 端口2日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
            string filename = "D:\\SFPXFPTesTLogDUT2.txt";
            try
            {
                if (File.Exists(filename))
                {
                    LoadLogFile(filename);
                }
                else
                {
                    MessageBox.Show("日志不存在");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("打开日志失败");
                return;
            }
        }

        private void 端口3日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
            string filename = "D:\\SFPXFPTesTLogDUT3.txt";
            try
            {
                if (File.Exists(filename))
                {
                    LoadLogFile(filename);
                }
                else
                {
                    MessageBox.Show("日志不存在");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("打开日志失败");
                return;
            }
        }

        private void 端口4日志ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logRichTextBox.Clear();
            string filename = "D:\\SFPXFPTesTLogDUT4.txt";
            try
            {
                if (File.Exists(filename))
                {
                    LoadLogFile(filename);
                }
                else
                {
                    MessageBox.Show("日志不存在");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("打开日志失败");
                return;
            }
        }
    }
}
