using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace FibertopTest_Common
{
    public class SimpleLogger
    {
        private string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MyLogs");
        private string logNamePattern = "mylog_mod_1.txt";//string.Format("mylog_{0:yyyy-MM-dd_HH-mm-ss}.txt", DateTime.Now);
        private object _lock = new object();

        public SimpleLogger(string log_name)
        {
            // 确保日志目录存在
            try
            {
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                logNamePattern = log_name;
            }
            catch
            {
                // 如果无法创建目录，日志仍然尝试写在当前工作目录
            }
        }

        public void FileDelete()
        {
            //File.Delete(LogPath);
            File.WriteAllBytes(LogPath, new byte[] { });
        }

        private string LogPath
        {
            get
            {
                // 当日日志文件
                return Path.Combine(logDirectory, logNamePattern);
            }
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        private void WriteLog(string level, string message)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
            try
            {
                // 线程安全写入
                lock (_lock)
                {
                    File.AppendAllText(LogPath, line + Environment.NewLine);
                }
            }
            catch
            {
                // 简单兜底：若写日志失败，不影响主流程
            }
        }
    }
}
