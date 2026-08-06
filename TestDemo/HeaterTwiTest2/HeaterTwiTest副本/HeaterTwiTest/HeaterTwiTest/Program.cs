using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FibertopTest_Common;

namespace HeaterTwiTest
{
    class Program
    {
        private static SFP_EVB_Heater heater;

        static void Main(string[] args)
        {
            Console.WriteLine("====== SFP EVB Heater TWI 通信测试 ======");
            Console.WriteLine("测试内容: 1. 读取ChipID (UX3320)  2. 写/读密码验证");
            Console.WriteLine();

            // 创建设备实例并连接
            heater = new SFP_EVB_Heater();

            Console.Write($"请输入加热台IP地址 (直接回车使用默认 {heater.DefaultIP}): ");
            string ip = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(ip)) ip = heater.DefaultIP;

            Console.WriteLine($"正在连接 {ip}:{heater.DefaultPort} ...");
            if (!heater.Open(ip))
            {
                Console.WriteLine("连接失败！请检查IP地址和网络连接。");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("连接成功！");
            Console.WriteLine();

            // 选择Slot
            int slot = 1;
            Console.Write("请输入要测试的Slot号 (1-4, 默认1): ");
            string slotInput = Console.ReadLine().Trim();
            if (!string.IsNullOrEmpty(slotInput))
            {
                if (!int.TryParse(slotInput, out slot) || slot < 1 || slot > 4)
                {
                    Console.WriteLine("无效的Slot号，使用默认值1。");
                    slot = 1;
                }
            }
            Console.WriteLine($"测试槽位: Slot {slot}");
            Console.WriteLine();

            // ========== 上电 ==========
            Console.WriteLine("----- 模块上电 -----");
            Console.WriteLine($"发送: IO{slot}:setPowerEN 1");
            bool powerOk = heater.SetPowerEN(1, slot);
            Console.WriteLine($"PowerEN结果: {(powerOk ? "成功" : "未收到确认，继续...")}");
            Thread.Sleep(500); // 等待模块上电稳定

            // 查询ABS引脚确认模块是否插入
            string absStatus = heater.GetABS(slot);
            Console.WriteLine($"ABS引脚状态: {absStatus ?? "null"} (0=模块已插入, 1=模块未插入)");
            Console.WriteLine();

            int passCount = 0;
            int failCount = 0;

            // ========== 测试1: 读取ChipID ==========
            Console.WriteLine("====== 测试1: 读取ChipID (UX3320) ======");
            bool test1Pass = TestReadChipID(slot);
            if (test1Pass) passCount++; else failCount++;
            Console.WriteLine();

            // ========== 测试2: 写密码并读回验证 ==========
            Console.WriteLine("====== 测试2: 写调试密码并读回验证 ======");
            bool test2Pass = TestWriteReadPassword(slot);
            if (test2Pass) passCount++; else failCount++;
            Console.WriteLine();

            // ========== 结果汇总 ==========
            Console.WriteLine("====== 测试结果汇总 ======");
            Console.WriteLine($"通过: {passCount}  失败: {failCount}");
            Console.WriteLine();

            // 关闭连接
            heater.Close();
            Console.WriteLine("已断开连接。按任意键退出...");
            Console.ReadKey();
        }

        #region TWI 底层通信方法

        /// <summary>
        /// 从设备响应字符串中解析十六进制字节数组
        /// 支持格式: "xx xx xx", "0xxx 0xxx", "xx,xx,xx", 混合文本中的hex字节
        /// </summary>
        private static byte[] ParseHexBytes(string response)
        {
            if (string.IsNullOrEmpty(response)) return new byte[0];

            // 提取所有16进制字节对 (两个连续的hex字符，可能带0x前缀)
            MatchCollection matches = Regex.Matches(response, @"(?:0x)?([0-9a-fA-F]{2})\b");
            byte[] bytes = new byte[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                bytes[i] = Convert.ToByte(matches[i].Groups[1].Value, 16);
            }
            return bytes;
        }

        /// <summary>
        /// 将字节数组转换为空格分隔的十六进制字符串，用于IIC_Set的data参数
        /// </summary>
        private static string BytesToHexString(byte[] data, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append(data[offset + i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// TWI 读单字节
        /// </summary>
        private static bool TwiReadByte(int slot, int devAddr, int regAddr, out byte value)
        {
            value = 0;
            string regAddrStr = $"{regAddr:X2}";
            string devAddrStr = $"{devAddr:X2}";
            string cmd = $"IIC{slot}:get {devAddrStr},{regAddrStr},1";
            Console.WriteLine($"  发送: {cmd}");
            string res = heater.IIC_Get(devAddrStr, regAddrStr, "1", slot);
            Console.WriteLine($"  响应: {res ?? "(null)"}");

            if (string.IsNullOrEmpty(res)) return false;

            byte[] bytes = ParseHexBytes(res);
            if (bytes.Length < 1) return false;
            value = bytes[0];
            return true;
        }

        /// <summary>
        /// TWI 写单字节
        /// value参数支持传入int(十进制或十六进制字面量均可，如0x03或3)，内部自动转为byte
        /// </summary>
        private static bool TwiWriteByte(int slot, int devAddr, int regAddr, int value)
        {
            byte b = (byte)value;
            string regAddrStr = $"{regAddr:X2}";
            string devAddrStr = $"{devAddr:X2}";
            string dataStr = $"{b:X2}";
            Console.WriteLine($"  发送: IIC{slot}:set {devAddrStr},{regAddrStr},1,{dataStr}");
            bool ok = heater.IIC_Set(devAddrStr, regAddrStr, "1", dataStr, slot);
            Console.WriteLine($"  结果: {(ok ? "OK" : "FAIL")}");
            return ok;
        }

        /// <summary>
        /// TWI 多字节读
        /// </summary>
        private static int TwiReadPage(int slot, int devAddr, int regAddr, byte[] buffer, int length)
        {
            string regAddrStr = $"{regAddr:X2}";
            string devAddrStr = $"{devAddr:X2}";
            string cmd = $"IIC{slot}:get {devAddrStr},{regAddrStr},{length}";
            Console.WriteLine($"  发送: {cmd}");
            string res = heater.IIC_Get(devAddrStr, regAddrStr, length.ToString(), slot);
            Console.WriteLine($"  响应: {res ?? "(null)"}");

            if (string.IsNullOrEmpty(res)) return 0;

            byte[] bytes = ParseHexBytes(res);
            int readLen = Math.Min(bytes.Length, length);
            Array.Copy(bytes, 0, buffer, 0, readLen);
            return readLen;
        }

        /// <summary>
        /// TWI 多字节写 (byte[]版本)
        /// </summary>
        private static int TwiWritePage(int slot, int devAddr, int regAddr, byte[] data, int length)
        {
            string regAddrStr = $"{regAddr:X2}";
            string devAddrStr = $"{devAddr:X2}";
            // 数据字节用空格分隔，不带0x前缀
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append($"{data[i]:X2}");
            }
            string dataStr = sb.ToString();

            Console.WriteLine($"  发送: IIC{slot}:set {devAddrStr},{regAddrStr},{length},{dataStr}");
            bool ok = heater.IIC_Set(devAddrStr, regAddrStr, length.ToString(), dataStr, slot);
            Console.WriteLine($"  结果: {(ok ? "OK" : "FAIL")}");
            return ok ? length : 0;
        }

        /// <summary>
        /// TWI 单字节写 (int值便捷版，参数支持传入0x03或3等int字面量)
        /// 等效于创建byte[1]{(byte)value}后调用byte[]版本
        /// </summary>
        private static int TwiWritePage(int slot, int devAddr, int regAddr, int value, int length)
        {
            byte[] data = new byte[1] { (byte)value };
            return TwiWritePage(slot, devAddr, regAddr, data, 1);
        }

        /// <summary>
        /// 选择寄存器页 (Table Select)
        /// </summary>
        private static bool SelectTable(int slot, byte table)
        {
            // 先读当前页
            byte[] rVal = new byte[1];
            if (TwiReadPage(slot, 0xA2, 127, rVal, 1) != 1)
            {
                Console.WriteLine($"  SelectTable({table}): 读取当前页失败!");
                return false;
            }
            if (rVal[0] == table)
            {
                Console.WriteLine($"  SelectTable({table}): 当前已是目标页");
                return true;
            }

            // 写入目标页
            if (!TwiWriteByte(slot, 0xA2, 127, table))
            {
                Console.WriteLine($"  SelectTable({table}): 写入页选择失败!");
                return false;
            }
            Thread.Sleep(5);

            // 读回验证
            if (TwiReadPage(slot, 0xA2, 127, rVal, 1) != 1)
            {
                Console.WriteLine($"  SelectTable({table}): 读回验证失败!");
                return false;
            }
            if (rVal[0] != table)
            {
                Console.WriteLine($"  SelectTable({table}): 验证失败! 读回=0x{rVal[0]:X2}");
                return false;
            }

            Console.WriteLine($"  SelectTable({table}): 成功");
            return true;
        }

        /// <summary>
        /// 字节数组比较
        /// </summary>
        private static bool ByteEquals(byte[] a, byte[] b, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        private static string BytesToHexString(byte[] data, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (i > 0) sb.Append(" ");
                sb.Append($"{data[i]:X2}");
            }
            return sb.ToString();
        }

        private static string BytesToAscii(byte[] data, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                if (data[i] >= 0x20 && data[i] <= 0x7E)
                    sb.Append((char)data[i]);
                else
                    sb.Append('.');
            }
            return sb.ToString();
        }

        #endregion

        #region 测试用例

        /// <summary>
        /// 测试1: 读取UX3320 ChipID
        /// 步骤: 选表3 -> 从0xA2,0xF5读7字节 -> 应等于 "UX33200" = {0x55,0x58,0x33,0x33,0x32,0x30,0x30}
        /// </summary>
        private static bool TestReadChipID(int slot)
        {
            byte[] expectedChipID = new byte[7] { 0x55, 0x58, 0x33, 0x33, 0x32, 0x30, 0x30 }; // "UX33200"
            byte[] readChipID = new byte[7];

            // 1. 选择表3
            Console.WriteLine("步骤1: 选择Table 3");
            if (!SelectTable(slot, 0x03))
            {
                Console.WriteLine("FAIL: 无法选择Table 3!");
                return false;
            }
            Thread.Sleep(10);

            // 2. 从0xA2, 0xF5读取7字节ChipID
            Console.WriteLine("步骤2: 从0xA2,0xF5读取7字节ChipID");
            int readLen = TwiReadPage(slot, 0xA2, 0xF5, readChipID, 7);
            if (readLen != 7)
            {
                Console.WriteLine($"FAIL: 读取ChipID长度错误! 期望7字节, 实际{readLen}字节");
                return false;
            }

            // 3. 打印读取到的数据
            Console.WriteLine($"读取到的ChipID: {BytesToHexString(readChipID, 7)}");
            Console.WriteLine($"ChipID ASCII:   {BytesToAscii(readChipID, 7)}");
            Console.WriteLine($"期望ChipID:     {BytesToHexString(expectedChipID, 7)}  (\"UX33200\")");

            // 4. 比较
            if (ByteEquals(readChipID, expectedChipID, 7))
            {
                Console.WriteLine("结果: PASS ✓ - ChipID匹配UX3320!");
                return true;
            }
            else
            {
                Console.WriteLine("结果: FAIL ✗ - ChipID不匹配!");
                // 打印差异字节
                for (int i = 0; i < 7; i++)
                {
                    if (readChipID[i] != expectedChipID[i])
                    {
                        Console.WriteLine($"  字节[{i}]: 期望0x{expectedChipID[i]:X2}, 实际0x{readChipID[i]:X2}");
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 测试2: 写调试密码 {0xA9,0x54,0x50,0x46} 到 0xA2,0x7B，然后读回验证
        /// </summary>
        private static bool TestWriteReadPassword(int slot)
        {
            byte[] debugPassword = new byte[4] { 0xA9, 0x54, 0x50, 0x46 };
            byte[] readBuffer = new byte[4];

            // 1. 切回表0（密码寄存器在任何页都可以访问，但确保在默认页）
            Console.WriteLine("步骤1: 选择Table 0");
            if (!SelectTable(slot, 0x00))
            {
                Console.WriteLine("WARN: 选择Table 0失败，尝试继续...");
            }
            Thread.Sleep(10);

            // 2. 写入调试密码（写两次，和原始代码SetDebugPWD一致）
            Console.WriteLine("步骤2: 写入调试密码到0xA2,0x7B");
            Console.WriteLine($"写入数据: {BytesToHexString(debugPassword, 4)}");
            int writeLen = TwiWritePage(slot, 0xA2, 0x7B, debugPassword, 4);
            if (writeLen != 4)
            {
                Console.WriteLine("FAIL: 第一次写入密码失败!");
                return false;
            }
            Thread.Sleep(10);

            // 写第二次（确保写入成功）
            writeLen = TwiWritePage(slot, 0xA2, 0x7B, debugPassword, 4);
            if (writeLen != 4)
            {
                Console.WriteLine("FAIL: 第二次写入密码失败!");
                return false;
            }
            Thread.Sleep(100);

            // 3. 读回4字节
            Console.WriteLine("步骤3: 从0xA2,0x7B读回4字节");
            int readLen = TwiReadPage(slot, 0xA2, 0x7B, readBuffer, 4);
            if (readLen != 4)
            {
                Console.WriteLine($"FAIL: 读回密码长度错误! 期望4字节, 实际{readLen}字节");
                return false;
            }

            // 4. 打印和比较
            Console.WriteLine($"写入数据: {BytesToHexString(debugPassword, 4)}");
            Console.WriteLine($"读回数据: {BytesToHexString(readBuffer, 4)}");

            if (ByteEquals(readBuffer, debugPassword, 4))
            {
                Console.WriteLine("结果: PASS ✓ - 密码写入/读回验证成功!");
                return true;
            }
            else
            {
                Console.WriteLine("结果: FAIL ✗ - 读回数据与写入不一致!");
                for (int i = 0; i < 4; i++)
                {
                    if (readBuffer[i] != debugPassword[i])
                    {
                        Console.WriteLine($"  字节[{i}]: 期望0x{debugPassword[i]:X2}, 实际0x{readBuffer[i]:X2}");
                    }
                }
                return false;
            }
        }

        #endregion
    }
}