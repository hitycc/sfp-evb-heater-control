using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFPXFP自动测试软件多端口
{
    internal static class Program
    {
        private static System.Threading.Mutex mutex;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
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

                FourDutFrm fourdutfrm = new FourDutFrm();

                LoginFrm loginfrm = new LoginFrm();

                loginfrm.ShowDialog();
                if (loginfrm.DialogResult == DialogResult.OK)
                {
                    //Application.EnableVisualStyles();
                    //Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(fourdutfrm);
                }
            }
        }
    }
}
