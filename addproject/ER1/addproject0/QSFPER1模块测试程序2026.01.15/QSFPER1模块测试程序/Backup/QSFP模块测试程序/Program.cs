using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace XFP模块测试程序
{
    static class Program
    {
        private static System.Threading.Mutex mutex;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();      // 首行，强制生效 
            Application.SetCompatibleTextRenderingDefault(false); // 文本渲染优化 
            bool isAppRunning = false;
            mutex = new System.Threading.Mutex(
                true,
                System.Diagnostics.Process.GetCurrentProcess().ProcessName,
                out isAppRunning);
            if (!isAppRunning)
            {
                MessageBox.Show("已经运行了本测试程序，不能重复运行，请检查确认。");
                Application.Exit();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //新建通信选择窗口
                I2C_Form i2c_Form = new I2C_Form();
                Main_Form main_Form = new Main_Form();
                I2C_Form.c.OnSendMsg += new Communication.MyEnentHander(main_Form.s_OnSendMsg);
                //使用模式对话框方法显示i2c_Form
                i2c_Form.ShowDialog();
                //DialogResult用来判断是否登录成功
                if (i2c_Form.DialogResult == DialogResult.OK)
                {
                    Application.Run(main_Form);
                }
            }
        }
    }
}
