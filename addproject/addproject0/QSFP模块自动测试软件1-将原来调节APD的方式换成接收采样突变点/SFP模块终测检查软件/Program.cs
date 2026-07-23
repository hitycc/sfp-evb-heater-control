using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SFP模块终测检查软件
{
    /// <summary>
    /// 程序入口类
    /// 整个程序从这里开始执行，相当于"大门"
    /// </summary>
    static class Program
    {
        /// <summary>
        /// 互斥量（Mutex）
        /// 作用：防止同一个程序同时运行多个实例
        /// 原理：程序启动时创建一把"锁"，如果锁已经存在，说明已经有程序在运行了
        /// </summary>
        private static System.Threading.Mutex mutex;

        /// <summary>
        /// 应用程序的主入口点
        /// 程序启动后第一个执行的函数
        /// </summary>
        [STAThread]  // 特性：表示程序使用单线程单元模型（WinForms必须加这个）
        static void Main()
        {
            // ============================================================
            // 第一步：初始化界面样式
            // ============================================================

            // 启用 Windows 视觉样式
            // 作用：让按钮、文本框、单选框等控件使用系统默认的美化样式
            // 如果不加这行，控件会是很丑的经典灰色样式
            Application.EnableVisualStyles();

            // 设置文本渲染方式
            // 参数 false：使用 GDI+ 渲染文本（效果更好，更清晰）
            // 参数 true：使用旧的 GDI 渲染文本（兼容性更好，但效果差一些）
            Application.SetCompatibleTextRenderingDefault(false);


            // ============================================================
            // 第二步：检查程序是否已经在运行（防止重复启动）
            // ============================================================

            // 定义变量：标记互斥量是不是"新创建"的
            // true = 互斥量是新创建的 → 之前没有程序在运行
            // false = 互斥量已经存在了 → 已经有程序在运行了
            bool createdNew;

            // 创建互斥量（Mutex）
            // 参数1：true → 调用者拥有互斥量的初始所有权
            // 参数2：互斥量的名字，用程序名作为名字，保证全局唯一
            // 参数3：out 输出参数，返回互斥量是不是新创建的
            mutex = new System.Threading.Mutex(
                true,
                System.Diagnostics.Process.GetCurrentProcess().ProcessName,  // 获取当前程序的进程名
                out createdNew);


            // ============================================================
            // 第三步：判断是否已经有程序在运行
            // ============================================================

            // 如果 createdNew = false → 互斥量已经存在 → 已经有程序在运行了
            // !createdNew 就是"不是新创建的" = "已经存在了"
            if (!createdNew)
            {
                // 弹出提示框，告诉用户不能重复运行
                MessageBox.Show(
                    "已经运行了本测试程序，不能重复运行，请检查确认。",  // 提示内容
                    "提示",                                                  // 标题
                    MessageBoxButtons.OK,                                    // 按钮类型：只有确定按钮
                    MessageBoxIcon.Warning);                                 // 图标：警告图标

                // 退出程序
                Application.Exit();
            }


            // ============================================================
            // 第四步：程序正常启动流程
            // ============================================================
            else
            {
                // 初始化4通道上下文管理器（多线程测试的基础）
                ChannelManager.Initialize();

                // 创建主窗体对象（但还不显示）
                Main_Form main_Form = new Main_Form();

                // 创建登录窗体对象
                Login_Form login_Form = new Login_Form();

                // 显示登录窗体（模态对话框）
                // ShowDialog() 的特点：
                //   1. 这个窗体不关掉，后面的代码就不会执行
                //   2. 用户只能操作这个窗体，不能点后面的主窗体
                //   3. 关闭后会返回一个 DialogResult，表示用户是点了确定还是取消
                login_Form.ShowDialog();


                // ========================================================
                // 第五步：判断登录是否成功
                // ========================================================

                // 如果登录窗体返回的是 OK（用户点了确定/登录按钮）
                if (login_Form.DialogResult == DialogResult.OK)
                {
                    // 运行主窗体（程序正式开始，进入主界面）
                    // Application.Run() 会启动消息循环，让窗体保持显示并响应用户操作
                    // 直到用户关闭主窗体，程序才会结束
                    Application.Run(main_Form);
                }

                // 如果用户点了取消或者直接关了登录窗口
                // 就什么也不做，程序直接结束
            }
        }
    }
}