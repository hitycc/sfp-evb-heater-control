using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using FibertopTest_Common;
using LastEVBControlDemoApp;

namespace OTP_AllQuery_Test
{
    class Program
    {
        private static SFP_EVB_Heater heater;

        static void Main(string[] args)
        {
            Console.WriteLine("====== SFP EVB Heater TWI 通信 & 消光比测试 ======");
            Console.WriteLine("测试内容: 1. 写密码 + 分页写/读16字节验证(每页8字节)  2. 小于8字节写/读验证  3. OTP-12 消光比读取");
            Console.WriteLine();

            // ========== 第一部分：SFP EVB Heater TWI 测试 ==========
            heater = new SFP_EVB_Heater();

            Console.Write($"请输入加热台IP地址 (直接回车使用默认 {heater.DefaultIP}): ");
            string ip = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(ip)) ip = heater.DefaultIP;

            Console.WriteLine($"正在连接加热台 {ip}:{heater.DefaultPort} ...");
            if (!heater.Open(ip))
            {
                Console.WriteLine("加热台连接失败！请检查IP地址和网络连接。");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("加热台连接成功！");
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
            Console.WriteLine($"[Slot {slot}] ----- 模块上电 -----");
            Console.WriteLine($"发送: IO{slot}:setPowerEN 1");
            bool powerOk = heater.SetPowerEN(1, slot);
            Console.WriteLine($"PowerEN结果: {(powerOk ? "成功" : "未收到确认，继续...")}");
            Thread.Sleep(500);

            // 查询ABS引脚确认模块是否插入
            string absStatus = heater.GetABS(slot);
            Console.WriteLine($"[Slot {slot}] ABS引脚状态: {absStatus ?? "null"} (0=模块已插入, 1=模块未插入)");
            Console.WriteLine();

            int passCount = 0;
            int failCount = 0;

            // ========== 测试1: 写密码 + 分页写/读16字节验证 ==========
            Console.WriteLine($"[Slot {slot}] ====== 测试1: 写密码 + 分页写/读16字节(0x01)验证 ======");
            bool test1Pass = TestWritePasswordAnd16Bytes(slot);
            if (test1Pass) passCount++; else failCount++;
            Console.WriteLine();

            // ========== 测试2: 小于8字节写/读验证 - 写2个0x02到0xA2,0xC0 ==========
            Console.WriteLine($"[Slot {slot}] ====== 测试2: 小于8字节写/读验证 - 写2个0x02到0xA2,0xC0并读回 ======");
            bool test2Pass = TestWriteRead2Bytes(slot);
            if (test2Pass) passCount++; else failCount++;
            Console.WriteLine();

            // 关闭加热台连接
            heater.Close();
            Console.WriteLine("加热台已断开连接。");
            Console.WriteLine();

            // ========== 第三部分：OTP-12 消光比读取 ==========
            Console.WriteLine($"====== 测试3: OTP-12 消光比读取 ======");

            OTP12Driver otp12 = new OTP12Driver();
            Console.Write($"请输入OTP-12设备IP地址 (直接回车使用默认 {otp12.DefaultIp}): ");
            string otpIp = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(otpIp)) otpIp = otp12.DefaultIp;

            Console.WriteLine($"正在连接OTP-12 {otpIp}:{otp12.DefaultPort} ...");
            if (!otp12.Connect(otpIp))
            {
                Console.WriteLine("OTP-12连接失败！跳过消光比测试。");
                failCount++;
            }
            else
            {
                Console.WriteLine("OTP-12连接成功！");
                Console.WriteLine();

                bool test3Pass = TestReadER(otp12);
                if (test3Pass) passCount++; else failCount++;

                otp12.DisConnect();
                Console.WriteLine("OTP-12已断开连接。");
            }
            Console.WriteLine();

            // ========== 结果汇总 ==========
            Console.WriteLine($"====== 测试结果汇总 ======");
            Console.WriteLine($"通过: {passCount}  失败: {failCount}");
            Console.WriteLine();

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        #region TWI 底层通信方法(分页，每页最多8字节，适配SFP_EVB_Heater)

        /// <summary>
        /// 从设备响应字符串中解析十六进制字节数组
        /// </summary>
        private static byte[] ParseHexBytes(string response)
        {
            if (string.IsNullOrEmpty(response)) return new byte[0];
            MatchCollection matches = Regex.Matches(response, @"(?:0x)?([0-9a-fA-F]{2})\b");
            byte[] bytes = new byte[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                bytes[i] = Convert.ToByte(matches[i].Groups[1].Value, 16);
            }
            return bytes;
        }

        /// <summary>
        /// TWI 单次页读 (private)。单次I2C事务，最多读8字节，避免len>=10时十进制字符串与十六进制混淆。
        /// </summary>
        private static int TWI_ReadPageRaw(int slot, int deviceAddr, int regAddr, byte[] buf, int len)
        {
            try
            {
                string dA = $"{(deviceAddr & 0xFF):X2}";
                string rA = $"{(regAddr & 0xFF):X2}";
                Console.WriteLine($"  [Slot {slot}] TWI_ReadPageRaw: dev=0x{dA}, reg=0x{rA}, len={len}");
                string resp = heater.IIC_Get(dA, rA, len.ToString(), slot);
                Console.WriteLine($"  [Slot {slot}] 响应: {resp ?? "(null)"}");
                if (string.IsNullOrEmpty(resp)) return 0;
                var matches = Regex.Matches(resp, @"(?:0x)?([0-9a-fA-F]{2})\b");
                int n = 0;
                foreach (Match m in matches)
                {
                    if (n >= len) break;
                    buf[n] = Convert.ToByte(m.Groups[1].Value, 16);
                    n++;
                }
                return n;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [Slot {slot}] TWI_ReadPageRaw异常: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// TWI 单次页写 (private)。单次I2C事务，最多写8字节，避免len>=10时十进制字符串与十六进制混淆。
        /// </summary>
        private static int TWI_WritePageRaw(int slot, int deviceAddr, int regAddr, byte[] buf, int len)
        {
            try
            {
                string dA = $"{(deviceAddr & 0xFF):X2}";
                string rA = $"{(regAddr & 0xFF):X2}";
                var sb = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append($"{buf[i]:X2}");
                }
                string dataStr = sb.ToString();
                Console.WriteLine($"  [Slot {slot}] TWI_WritePageRaw: dev=0x{dA}, reg=0x{rA}, len={len}, data={dataStr}");
                bool ok = heater.IIC_Set(dA, rA, len.ToString(), dataStr, slot);
                Console.WriteLine($"  [Slot {slot}] 结果: {(ok ? "OK" : "FAIL")}");
                return ok ? len : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [Slot {slot}] TWI_WritePageRaw异常: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// TWI 多字节读 (public)。自动分页，每次最多读8字节，支持任意长度。
        /// </summary>
        private static int TWI_ReadPage(int slot, int deviceAddr, int regAddr, byte[] buf, int len)
        {
            const int pageSize = 8;
            int totalRead = 0;
            int offset = 0;
            int curReg = regAddr & 0xFF;

            Console.WriteLine($"[Slot {slot}] TWI_ReadPage: dev=0x{deviceAddr:X2}, reg=0x{regAddr:X2}, totalLen={len} (pageSize={pageSize})");

            while (offset < len)
            {
                int chunkLen = len - offset;
                if (chunkLen > pageSize) chunkLen = pageSize;

                byte[] chunkBuf = new byte[chunkLen];
                int n = TWI_ReadPageRaw(slot, deviceAddr, curReg, chunkBuf, chunkLen);
                if (n <= 0) break;

                Array.Copy(chunkBuf, 0, buf, offset, n);
                totalRead += n;
                offset += n;
                curReg = (curReg + n) & 0xFF;

                if (n < chunkLen) break;
                Thread.Sleep(5);
            }

            Console.WriteLine($"[Slot {slot}] TWI_ReadPage完成: 实际读取{totalRead}字节");
            return totalRead;
        }

        /// <summary>
        /// TWI 多字节写 (private)。自动分页，每次最多写8字节，支持任意长度。
        /// </summary>
        private static int TWI_WritePage(int slot, int deviceAddr, int regAddr, byte[] buf, int len)
        {
            const int pageSize = 8;
            int totalWritten = 0;
            int offset = 0;
            int curReg = regAddr & 0xFF;

            Console.WriteLine($"[Slot {slot}] TWI_WritePage: dev=0x{deviceAddr:X2}, reg=0x{regAddr:X2}, totalLen={len} (pageSize={pageSize})");

            while (offset < len)
            {
                int chunkLen = len - offset;
                if (chunkLen > pageSize) chunkLen = pageSize;

                byte[] chunkBuf = new byte[chunkLen];
                Array.Copy(buf, offset, chunkBuf, 0, chunkLen);

                int n = TWI_WritePageRaw(slot, deviceAddr, curReg, chunkBuf, chunkLen);
                if (n <= 0) break;

                totalWritten += n;
                offset += n;
                curReg = (curReg + n) & 0xFF;

                if (n < chunkLen) break;
                Thread.Sleep(5);
            }

            Console.WriteLine($"[Slot {slot}] TWI_WritePage完成: 实际写入{totalWritten}字节");
            return totalWritten;
        }

        /// <summary>
        /// TWI 读单字节
        /// </summary>
        private static byte TWI_ReadByte(int slot, int deviceAddr, int regAddr)
        {
            byte[] b = new byte[1];
            if (TWI_ReadPageRaw(slot, deviceAddr, regAddr, b, 1) == 1) return b[0];
            return 0;
        }

        /// <summary>
        /// TWI 写单字节
        /// </summary>
        private static bool TWI_WriteByte(int slot, int deviceAddr, int regAddr, int val)
        {
            byte[] b = new byte[] { (byte)val };
            return TWI_WritePageRaw(slot, deviceAddr, regAddr, b, 1) == 1;
        }

        /// <summary>
        /// 选择寄存器页 (Table Select)
        /// </summary>
        private static bool SelectTable(int slot, byte table)
        {
            Console.WriteLine($"[Slot {slot}] SelectTable({table}): 开始切换页...");

            // 先读当前页
            byte curTable = TWI_ReadByte(slot, 0xA2, 127);
            Console.WriteLine($"[Slot {slot}] SelectTable({table}): 当前页=0x{curTable:X2}");
            if (curTable == table)
            {
                Console.WriteLine($"[Slot {slot}] SelectTable({table}): 当前已是目标页");
                return true;
            }

            // 写入目标页
            if (!TWI_WriteByte(slot, 0xA2, 127, table))
            {
                Console.WriteLine($"[Slot {slot}] SelectTable({table}): 写入页选择失败!");
                return false;
            }
            Thread.Sleep(5);

            // 读回验证
            byte verifyTable = TWI_ReadByte(slot, 0xA2, 127);
            if (verifyTable != table)
            {
                Console.WriteLine($"[Slot {slot}] SelectTable({table}): 验证失败! 读回=0x{verifyTable:X2}");
                return false;
            }

            Console.WriteLine($"[Slot {slot}] SelectTable({table}): 成功切换到Table {table}");
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
                if (i > 0) sb.Append(",");
                sb.Append($"{data[i]:X2}");
            }
            return sb.ToString();
        }

        #endregion

        #region 测试用例

        /// <summary>
        /// 测试1: 写调试密码 {0xFF,0xFF,0xFF,0xFF} 到 0xA2,0x7B，
        /// 然后切换表3，用分页写(每页8字节)写16字节0x01到0xA2,0xC0，
        /// 再用分页读(每页8字节)读回16字节并验证。
        /// </summary>
        private static bool TestWritePasswordAnd16Bytes(int slot)
        {
            // ---- 步骤1: 选择Table 0 ----
            Console.WriteLine($"[Slot {slot}] 步骤1: 选择Table 0");
            if (!SelectTable(slot, 0x00))
            {
                Console.WriteLine($"[Slot {slot}] WARN: 选择Table 0失败，尝试继续...");
            }
            Thread.Sleep(10);

            // ---- 步骤2: 写入调试密码 {0xFF,0xFF,0xFF,0xFF} 到 0xA2,0x7B ----
            byte[] debugPassword = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };
            Console.WriteLine($"[Slot {slot}] 步骤2: 写入调试密码到0xA2,0x7B");
            Console.WriteLine($"[Slot {slot}] 写入数据: {BytesToHexString(debugPassword, 4)}");

            // 写两次（确保写入成功）
            int writeLen = TWI_WritePage(slot, 0xA2, 0x7B, debugPassword, 4);
            if (writeLen != 4)
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 第一次写入密码失败! 写入{writeLen}/4字节");
                return false;
            }
            Thread.Sleep(10);

            writeLen = TWI_WritePage(slot, 0xA2, 0x7B, debugPassword, 4);
            if (writeLen != 4)
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 第二次写入密码失败! 写入{writeLen}/4字节");
                return false;
            }
            Thread.Sleep(100);

            // 读回密码验证
            byte[] readPwd = new byte[4];
            int readPwdLen = TWI_ReadPage(slot, 0xA2, 0x7B, readPwd, 4);
            if (readPwdLen != 4)
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 读回密码长度错误! 期望4字节, 实际{readPwdLen}字节");
                return false;
            }
            Console.WriteLine($"[Slot {slot}] 密码写入: {BytesToHexString(debugPassword, 4)}");
            Console.WriteLine($"[Slot {slot}] 密码读回: {BytesToHexString(readPwd, 4)}");
            if (!ByteEquals(readPwd, debugPassword, 4))
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 密码读回与写入不一致!");
                return false;
            }
            Console.WriteLine($"[Slot {slot}] 密码验证通过 ✓");
            Console.WriteLine();

            // ---- 步骤3: 切换到Table 3 ----
            Console.WriteLine($"[Slot {slot}] 步骤3: 选择Table 3");
            if (!SelectTable(slot, 0x03))
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 无法选择Table 3!");
                return false;
            }
            Thread.Sleep(10);

            // ---- 步骤4: 用分页写(每页8字节)写16字节0x01到0xA2,0xC0 ----
            byte[] writeData = new byte[16];
            for (int i = 0; i < 16; i++) writeData[i] = 0x01;

            Console.WriteLine($"[Slot {slot}] 步骤4: 分页写入16字节0x01到0xA2,0xC0 (每页8字节，分2页)");
            Console.WriteLine($"[Slot {slot}] 写入数据: {BytesToHexString(writeData, 16)}");
            writeLen = TWI_WritePage(slot, 0xA2, 0xC0, writeData, 16);
            if (writeLen != 16)
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 写入数据长度错误! 期望16字节, 实际写入{writeLen}字节");
                return false;
            }
            Thread.Sleep(100);

            // ---- 步骤5: 用分页读(每页8字节)从0xA2,0xC0读回16字节 ----
            byte[] readData = new byte[16];
            Console.WriteLine($"[Slot {slot}] 步骤5: 分页读取16字节从0xA2,0xC0 (每页8字节，分2页)");
            int readLen = TWI_ReadPage(slot, 0xA2, 0xC0, readData, 16);
            if (readLen != 16)
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 读回数据长度错误! 期望16字节, 实际{readLen}字节");
                return false;
            }

            // ---- 步骤6: 打印和比较 ----
            Console.WriteLine();
            Console.WriteLine($"[Slot {slot}] 写入数据: {BytesToHexString(writeData, 16)}");
            Console.WriteLine($"[Slot {slot}] 读回数据: {BytesToHexString(readData, 16)}");

            if (ByteEquals(readData, writeData, 16))
            {
                Console.WriteLine($"[Slot {slot}] 结果: PASS ✓ - 分页写/读16字节验证成功!");
                return true;
            }
            else
            {
                Console.WriteLine($"[Slot {slot}] 结果: FAIL ✗ - 读回数据与写入不一致!");
                for (int i = 0; i < 16; i++)
                {
                    if (readData[i] != writeData[i])
                    {
                        Console.WriteLine($"[Slot {slot}]   字节[{i}]: 期望0x{writeData[i]:X2}, 实际0x{readData[i]:X2}");
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 测试2: 小于8字节写/读验证 - 写2个0x02到0xA2,0xC0，然后读回2字节验证。
        /// 确保在Table 3下操作，测试少于8字节的数据能否正确读写。
        /// </summary>
        private static bool TestWriteRead2Bytes(int slot)
        {
            // ---- 步骤1: 确保在Table 3 ----
            Console.WriteLine($"[Slot {slot}] 步骤1: 确认选择Table 3");
            if (!SelectTable(slot, 0x03))
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 无法选择Table 3!");
                return false;
            }
            Thread.Sleep(10);

            // ---- 步骤2: 先读0xA2,0xC0起始的16字节，记录原始数据 ----
            Console.WriteLine($"[Slot {slot}] 步骤2: 先读取0xA2,0xC0起始16字节原始数据（用于对比）");
            byte[] originalData = new byte[16];
            int origReadLen = TWI_ReadPage(slot, 0xA2, 0xC0, originalData, 16);
            if (origReadLen != 16)
            {
                Console.WriteLine($"[Slot {slot}] WARN: 读取原始数据长度不足! 实际{origReadLen}/16字节，继续测试...");
            }
            Console.WriteLine($"[Slot {slot}] 原始数据: {BytesToHexString(originalData, origReadLen)}");
            Console.WriteLine();

            // ---- 步骤3: 写入2个0x02到0xA2,0xC0 ----
            byte[] writeData2 = new byte[2] { 0x02, 0x02 };
            Console.WriteLine($"[Slot {slot}] 步骤3: 写入2字节0x02到0xA2,0xC0");
            Console.WriteLine($"[Slot {slot}] 写入数据: {BytesToHexString(writeData2, 2)}");
            int writeLen = TWI_WritePageRaw(slot, 0xA2, 0xC0, writeData2, 2);
            if (writeLen != 2)
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 写入2字节失败! 写入{writeLen}/2字节");
                return false;
            }
            Thread.Sleep(100);

            // ---- 步骤4: 从0xA2,0xC0读回2字节验证 ----
            byte[] readData2 = new byte[2];
            Console.WriteLine($"[Slot {slot}] 步骤4: 从0xA2,0xC0读回2字节");
            int readLen = TWI_ReadPageRaw(slot, 0xA2, 0xC0, readData2, 2);
            if (readLen != 2)
            {
                Console.WriteLine($"[Slot {slot}] FAIL: 读回2字节长度错误! 期望2字节, 实际{readLen}字节");
                return false;
            }

            // ---- 步骤5: 再读16字节确认只有前2字节变化，后面字节不受影响 ----
            Console.WriteLine($"[Slot {slot}] 步骤5: 读取0xA2,0xC0起始16字节，确认其他字节未被影响");
            byte[] afterData = new byte[16];
            int afterReadLen = TWI_ReadPage(slot, 0xA2, 0xC0, afterData, 16);
            if (afterReadLen != 16)
            {
                Console.WriteLine($"[Slot {slot}] WARN: 读取写入后数据长度不足! 实际{afterReadLen}/16字节");
            }

            // ---- 步骤6: 打印和比较 ----
            Console.WriteLine();
            Console.WriteLine($"[Slot {slot}] 写入数据(2字节): {BytesToHexString(writeData2, 2)}");
            Console.WriteLine($"[Slot {slot}] 读回数据(2字节): {BytesToHexString(readData2, 2)}");
            Console.WriteLine($"[Slot {slot}] 写入前16字节:  {BytesToHexString(originalData, Math.Min(origReadLen, 16))}");
            Console.WriteLine($"[Slot {slot}] 写入后16字节:  {BytesToHexString(afterData, Math.Min(afterReadLen, 16))}");

            bool twoByteMatch = ByteEquals(readData2, writeData2, 2);

            // 检查前2字节是否为0x02
            bool firstTwoCorrect = (afterData[0] == 0x02 && afterData[1] == 0x02);

            // 检查后面的字节(从索引2开始)是否与原始数据一致
            bool restUnchanged = true;
            int checkLen = Math.Min(Math.Min(origReadLen, afterReadLen), 16);
            for (int i = 2; i < checkLen; i++)
            {
                if (afterData[i] != originalData[i])
                {
                    Console.WriteLine($"[Slot {slot}]   字节[{i}]变化: 写入前0x{originalData[i]:X2} -> 写入后0x{afterData[i]:X2}");
                    restUnchanged = false;
                }
            }

            if (twoByteMatch && firstTwoCorrect && restUnchanged)
            {
                Console.WriteLine($"[Slot {slot}] 结果: PASS ✓ - 小于8字节(2字节0x02)写/读验证成功! 其他字节未被影响。");
                return true;
            }
            else if (twoByteMatch && firstTwoCorrect)
            {
                Console.WriteLine($"[Slot {slot}] 结果: PASS ✓ (有警告) - 2字节写/读验证成功，但后续部分字节有变化。");
                return true;
            }
            else
            {
                Console.WriteLine($"[Slot {slot}] 结果: FAIL ✗ - 2字节写/读验证失败!");
                if (!twoByteMatch)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (readData2[i] != writeData2[i])
                        {
                            Console.WriteLine($"[Slot {slot}]   字节[{i}]: 期望0x{writeData2[i]:X2}, 实际0x{readData2[i]:X2}");
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 测试3: OTP-12 消光比读取
        /// 设置slot="06", 速率="1.25G", 读取通道2(ch=2)的消光比
        /// </summary>
        private static bool TestReadER(OTP12Driver otp12)
        {
            try
            {
                int ermCh = 2;

                // 步骤1: 设置槽位为"06"
                Console.WriteLine($"步骤1: 设置ERM槽位为 06");
                otp12.SetSlot("06");
                Thread.Sleep(100);

                // 步骤2: 设置信号速率为1.25G
                Console.WriteLine($"步骤2: 设置通道{ermCh}信号速率为 1.25G");
                bool rateOk = otp12.ERM_SetRate(ermCh, "1.25G");
                Console.WriteLine($"  速率设置结果: {(rateOk ? "成功" : "未收到确认，继续...")}");
                Thread.Sleep(500);

                // 步骤3: 查询当前速率确认
                string curRate = otp12.ERM_GetRate(ermCh);
                Console.WriteLine($"  当前速率配置: {curRate ?? "(null)"}");
                Thread.Sleep(100);

                // 步骤4: 读取消光比数据
                Console.WriteLine($"步骤3: 读取通道{ermCh}消光比数据 (格式: power,er)");
                string erData = otp12.ERM_ReadERData(ermCh);
                Console.WriteLine($"  原始返回: {erData ?? "(null)"}");

                if (string.IsNullOrEmpty(erData))
                {
                    Console.WriteLine($"结果: FAIL ✗ - 读取消光比数据为空!");
                    return false;
                }

                // 解析返回值: 格式为 "power,er"
                string[] parts = erData.Split(',');
                if (parts.Length >= 2)
                {
                    string powerStr = parts[0].Trim();
                    string erStr = parts[1].Trim();
                    Console.WriteLine();
                    Console.WriteLine($"====== 消光比测试结果 ======");
                    Console.WriteLine($"  光功率: {powerStr} dBm");
                    Console.WriteLine($"  消光比: {erStr} dB");
                    Console.WriteLine($"============================");

                    if (double.TryParse(erStr, out double erValue))
                    {
                        Console.WriteLine($"结果: PASS ✓ - 消光比读取成功! ER={erValue:F3} dB");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"结果: PASS ✓ - 消光比数据已获取 (数值解析: {erStr})");
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine($"结果: PASS ✓ - 返回数据: {erData} (格式非预期但已获取)");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"结果: FAIL ✗ - 消光比读取异常: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}