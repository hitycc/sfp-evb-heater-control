using System;
using FibertopTest_Common;

namespace OTP_AllQuery_Test
{
    class Program
    {
        // 如果默认IP不对，请修改此处
        const string DeviceIP = "129.168.1.133";
        const int DevicePort = 9000;

        static void Main(string[] args)
        {
            Console.WriteLine("=== SFP EVB 加热台 IIC 读取测试 ===");
            Console.WriteLine();

            var heater = new SFP_EVB_Heater();

            try
            {
                Console.WriteLine("正在连接设备 {0}:{1} ...", DeviceIP, DevicePort);
                bool connected = heater.Open(DeviceIP, DevicePort);
                if (!connected)
                {
                    Console.WriteLine("连接失败！请检查IP地址、端口号以及设备是否开机。");
                    Console.WriteLine("当前默认IP: {0}，端口: {1}，请根据实际情况修改 Program.cs 顶部的 DeviceIP 和 DevicePort。",
                        DeviceIP, DevicePort);
                    return;
                }
                Console.WriteLine("连接成功！");
                Console.WriteLine();

                // 发送命令 IIC2:get a2,0,9
                string cmdDesc = "IIC2:get a2,0,9";
                Console.WriteLine("发送命令: " + cmdDesc);
                Console.WriteLine("--------------------------------");

                string result = heater.IIC_Get("a2", "0", "9", slot: 2);

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("返回数据为空（无响应），命令执行失败。");
                }
                else
                {
                    Console.WriteLine("返回数据: " + result);
                    Console.WriteLine("命令执行成功，已收到设备返回值。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生异常: " + ex.Message);
            }
            finally
            {
                heater.Close();
                Console.WriteLine();
                Console.WriteLine("已断开连接，按任意键退出...");
                Console.ReadKey(true);
            }
        }
    }
}