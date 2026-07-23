using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SFP模块终测检查软件
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
                
                Main_Form main_Form = new Main_Form();
                Login_Form login_Form = new Login_Form();

                login_Form.ShowDialog();
                if (login_Form.DialogResult == DialogResult.OK)
                {
                    Application.Run(main_Form);
                }
            }
        }
    }
}
