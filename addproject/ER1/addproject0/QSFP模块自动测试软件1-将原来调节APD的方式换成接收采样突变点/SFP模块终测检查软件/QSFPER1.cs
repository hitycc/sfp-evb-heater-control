using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Data;
using System.Data.OleDb;

namespace FibertopTest_Common
{
    /// <summary>
    /// 将手动ER1中的内容参照TestQSFP.cs中的框架
    /// </summary>
    public class QSFPER1 : ModuleTest
    {
        private I2C i2c;
        
        byte[] apclut = new byte[32]; //APC（自动功率控制）温度补偿查找表，32字节对应 -40°C 到 +115°C 每5°C一个点，用于在不同温度下调整激光器偏置电流。
        byte[] modlut = new byte[32]; //MOD（调制电流）温度补偿查找表，32字节，用于在不同温度下调整调制电流以维持消光比
        byte[] apdlut = new byte[32]; //APD（雪崩光电二极管）偏置电压温度补偿查找表，32字节，用于在不同温度下调整APD高压以维持接收灵敏度。

        byte[] threshold = new byte[72];//DDM 告警阈值数据，72字节对应温度/电压/偏置/Tx功率/Rx功率的高低告警和警告门限。存储在模块Page 03中

        byte[] ex_cal = new byte[39] //外部校准参数，初始值硬编码。大部分为0，其中有一些预设值（如 `0x3F800000` 是IEEE浮点数1.0），用于DDM校准
                {
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                    0x3F,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,
                    0x01,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x01,0x00,0x00,0x00,
                    0x00,0x00,0x00
                };

        byte[] byte_debug_pwd = new byte[4];

        public void Init(I2C i2c)
        {
            this.i2c = i2c;
          
            GlobalVarFun.txpwr_debug_method = 0x00;

            TestResult.flash_data_len = 768; // 256+256+256=768  必须<=1024

            // 更新调试密码 调试密码，4字节，写入模块特定寄存器后才能进入调试模式（允许修改LUT表等参数）。
            byte_debug_pwd[0] = 0xA9;
            byte_debug_pwd[1] = 0x46;
            byte_debug_pwd[2] = 0x50;
            byte_debug_pwd[3] = 0x54;
        }
        //检查模块方案
        public bool CheckTestTypeInfo()
        {
            byte[] readbuffer = new byte[4];
            String str, strRate;
            if (i2c == null || !SelectTable(6))
            {
                return false;
            }
            // 检查模块方案
            if (i2c.TWI_ReadPage(0xa0, 0xFC, readbuffer, 4) != 4)
            {
                return false; // Error
            }
            
            str = string.Format("设计方案{0}  ", (readbuffer[0] & 0x0F).ToString("D"));

            strRate = "000";
            //速率类型
            switch (readbuffer[0] & 0xF0)
            {
                case 0x10:
                    str += "40G";
                    strRate = "40G";
                    break;
                case 0x20:
                    str += "100G";
                    strRate = "100G";
                    break;
                case 0x30:
                    str += "100G/112G 双速率 ";
                    strRate = "100G";
                    break;
                case 0x40:
                    str += "100G PAM4 ";
                    strRate = "100G-PAM4";
                    break;
                default:
                    str += " ";
                    break;
            }
            TestResult.bitRate = str;
            //芯片方案编号，不同速率对应不同芯片组合
            //str += " ";
            str += " ";
            if (strRate == "40G")
            {
                switch (readbuffer[0] & 0x0F)
                {
                    case 0x01:
                        str += "MAX24040";
                        break;
                    case 0x02:
                        str += "4xGN1157";
                        break;
                    case 0x03:
                        str += "PHXT8104+PHXR8104";
                        break;
                    case 0x04:
                        str += "  ";
                        break;
                    case 0x05:
                        str += "37045+37044";
                        break;
                    case 0x06:
                        str += "24025+37046";
                        break;
                    case 0x07:
                        str += "24025+2110S";
                        break;
                    case 0x08:
                        str += "037057+37046";
                        break;
                    default:
                        str += "Reserved";
                        break;
                }
            }
            else if (strRate == "100G-PAM4")
            {
                switch (readbuffer[0] & 0x0F)
                {
                    case 0x01:
                        str += "IN010C25+ADuC7023";
                        break;
                    case 0x02:
                        str += "BCM87101+GD32E501";
                        break;
                    default:
                        str += "Reserved";
                        break;
                }
            }
            else if (strRate == "100G")
            {
                switch (readbuffer[0] & 0x0F)
                {
                    case 0x01:
                        str += "37049+37046+011039+002304";
                        break;
                    case 0x02:
                        str += "24028+37046";
                        break;
                    case 0x03:
                        str += "37049+37046+1185";
                        break;
                    case 0x04:
                        str += "37059+37244";
                        break;
                    case 0x05:
                        str += "37045+37044";
                        break;
                    case 0x06:
                        str += "24025+37046";
                        break;
                    case 0x07:
                        str += "24025+2110S";
                        break;
                    case 0x08:
                        str += "037057+37046";
                        break;
                    case 0x09:
                        str += "UX2291+2091";
                        break;
                    default:
                        str += "Reserved";
                        break;
                }
            }
            else
            {
                str += "未定义";
            }

            str += " ";
            if (strRate == "100G-PAM4")
            {
                // Page 06: 0xFC~0xFF holds hardware version, firmware version,
                // PAM4：SR1/FR1/LR1/ER1/ZR1（单通道，1/2/10/40/80km）
                switch (readbuffer[1] & 0xE0)
                {
                    case 0x20:
                        str += "SR1";
                        break;
                    case 0x40:
                        str += "FR1";
                        break;
                    case 0x60:
                        str += "LR1";
                        break;
                    case 0x80:
                        str += "ER1";
                        break;
                    case 0xA0:
                        str += "ZR1";
                        break;
                    default:
                        str += "未知";
                        break;
                }
            }
            //非PAM4：SR4/CW4/LR4/ER4/ZR4（4通道并行）
            else
            {
                switch (readbuffer[1] & 0xE0)
                {
                    case 0x20:
                        str += "SR4";
                        break;
                    case 0x40:
                        str += "CW4";
                        break;
                    case 0x60:
                        str += "LR4";
                        break;
                    case 0x80:
                        str += "ER4";
                        break;
                    case 0xA0:
                        str += "ZR4";
                        break;
                    default:
                        str += "未知";
                        break;
                }
            }
            // 模块芯片方案
            str += string.Format("  软件版本:V{0}  ", (readbuffer[1] & 0x0F).ToString("D"));
            TestResult.chipType = "QSFP: " + str;
          
            // 模块芯片工作状态 
            if ((readbuffer[2] & 0x0F) == 0x0F)
            {
                TestResult.chipIsOK = false;
            }
            else
            {
                TestResult.chipIsOK = true;
            }

            return true;
        }
        //地址86（0x56）是QSFP的"通道TX Disable控制寄存器"，低4位对应4个通道的TX Disable位。`|= 0x0F` 全部置1（关闭），
        //`&= 0xF0` 全部清0（开启）。修改后等待100ms让硬件稳定。
        public bool SoftTxDis(bool txDis)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 86);

            if (txDis == true)
            {
                wrtBuf |= 0x0F;//bit=1  tx_dis
            }
            else
            {
                wrtBuf &= 0xF0;//bit=0  tx_en
            }

            if (i2c.TWI_WriteByte(0xa0, 86, wrtBuf) == false)
            {
                return false;
            }
            Thread.Sleep(100); //延时100ms

            return true;
        }
        //先关闭所有4个通道，再只清除（使能）指定通道CH的disable位，
        //实现"只开一个通道"的效果。用于测试时单独测试每个通道。
        public bool SoftTxCHEn(int CH)
        {     
            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 86);
            wrtBuf |= 0x0F;// 先全部关闭
            wrtBuf = Bit.ClearBit(wrtBuf, CH);// 再单独打开指定通道CH
            if (i2c.TWI_WriteByte(0xA0, 86, wrtBuf) == false)
            {
                return false;
            }
            Thread.Sleep(100); //延时100ms

            return true;
        }
        //与 `SoftTxCHEn` 功能相同，但使用 `GlobalVarFun.USBtoI2C`（CP2112 USB转I2C芯片）直接操作，而不是类中保存的 `i2c` 对象。
        //会先Open再Close设备。用于光源板（参考光源）的通道控制。
        public bool SourceSoftEn(int CH)
        {
            if (GlobalVarFun.USBtoI2C.TWI_Open())//open
            {
                GlobalVarFun.usb_can_use = true;
            }
            else
            {
                GlobalVarFun.usb_can_use = false;
            }
            byte wrtBuf = GlobalVarFun.USBtoI2C.TWI_ReadByte(0xa0, 86);
            wrtBuf |= 0x0F;//4 ch softDis
            wrtBuf = Bit.ClearBit(wrtBuf, CH);//CH En
            if (GlobalVarFun.USBtoI2C.TWI_WriteByte(0xA0, 86, wrtBuf) == false) //
            {
                GlobalVarFun.USBtoI2C.TWI_Close();//close  
                return false;
            }
            else
            {
                GlobalVarFun.USBtoI2C.TWI_Close();//close     
                return true;
            }
        }
        /// <summary>
        ///SetDebugPWD - 进入调试模式
        ///SFF-8436协议规定，向A0地址0x7B~0x7E写入厂商密码后可以进入调试模式。
        ///然后切到Page 6，向0x80地址写入0x00来禁用温度补偿查找表(LUT)。这样寄存器设置的值就是直接生效的DAC值，
        ///不会被温度补偿逻辑覆盖。这在调试/校准阶段是必须的。
        /// </summary>
        /// <returns></returns>
        public bool SetDebugPWD()
        {
            if (i2c == null)
                return false;

            // ER1 manual program: vendor password -> Page 06 -> disable LUT.
            if (!SelectTable(0))// 切到Page 0
                return false;

            if (i2c.TWI_WritePage(0xa0, 0x7B, byte_debug_pwd, 4) != 4)// 写密码到0x7B-0x7E
            {
                return false;
            }
            Thread.Sleep(100);

            if (CheckDebugPWD() != 0x00) return false; // 检查密码是否写入

            if (!SelectTable(0x06))// 切到Page 6
                return false;

            if (!i2c.TWI_WriteByte(0xA0, 0x80, 0x00))// 关闭LUT（查找表）使能
                return false;

            Thread.Sleep(30);
            return true;
        }
        /// <summary>
        /// CheckDebugPWD - 验证调试密码
        /// </summary>
        /// <returns></returns>
        public byte CheckDebugPWD()
        {
            byte[] readbuffer = new byte[4];

            // 读密码
            if (i2c.TWI_ReadPage(0xa0, 0x7B, readbuffer, 4) != 4)
            {
                return 0x01;
            }

            if (Bit.ByteEquals(readbuffer, byte_debug_pwd) == false)
            {
                return 0x02;
            }

            return 0x00;
        }
        /// <summary>
        /// CheckRxLOS - 检查接收信号丢失
        /// </summary>
        /// <returns></returns>
        public bool CheckRxLOS()
        {
            int ch = 0;  
            ch = TestSet.ch;           
            byte status = i2c.TWI_ReadByte(0xa0, 3);// 地址3：LOS状态寄存器
            return Bit.GetBit(status, ch); //第ch位 = 1表示该通道LOS
        }
        /// <summary>
        /// GetTemp - 读取模块温度
        /// 地址22-23存储16位有符号温度值，格式为：高字节=整数部分(有符号sbyte)，低字节=小数部分(1/256°C)。这是SFF-8436标准格式。
        /// </summary>
        /// <returns></returns>
        public float GetTemp()
        {
            float temp = 0;
            byte[] readbuffer = new byte[2];
            if (i2c.TWI_ReadPage(0xa0, 22, readbuffer, 2) != 2)
            {
                return -100;
            }
            //
            sbyte i = (sbyte)readbuffer[0];// 高字节是有符号整数部分
            int j = Convert.ToInt32(i);
            temp = (j + readbuffer[1] * (1 / 256.0f));// 低字节是小数部分(1/256度)
            return temp;
        }
        /// <summary>
        /// GetVCC - 读取模块供电电压
        /// 地址26-27是16位无符号电压值，单位0.0001V，除以10000得到伏特。量程0~6.5535V
        /// </summary>
        /// <returns></returns>
        public float GetVCC()
        {
            byte[] readbuffer = new byte[2];
            float vccDDM;

            if (i2c.TWI_ReadPage(0xa0, 26, readbuffer, 2) == 2)
            {
                vccDDM = ((readbuffer[0] * 256 + readbuffer[1]) / 10000.0f);
            }
            else
            {
                return 0; // Error
            }

            return vccDDM;
        }
        /// <summary>
        /// 读取激光器偏置电流
        /// 地址42开始每通道2字节，共4通道。单位0.002mA，除以500得到mA。有重试机制（读失败等30ms再试一次）
        /// </summary>
        /// <returns></returns>
        public float GetTxBias()
        {
            float txbias = 0;
            byte[] readbuffer = new byte[8];
            int ch = 0;
            ch = TestSet.ch;   
            if (i2c.TWI_ReadPage(0xa0, 42, readbuffer, 8) != 8)
            {
                Thread.Sleep(30);
                // 重试一次
                if (i2c.TWI_ReadPage(0xa0, 42, readbuffer, 8) != 8)
                {
                    return -1; // Error
                }
            }
            //
            txbias = ((readbuffer[0 + ch * 2] * 256 + readbuffer[1 + ch * 2]) / 500.0f);
            return txbias;
        }
        /// <summary>
        /// GetTxPower - 读取发射光功率（当前通道）
        /// 内部ADC值单位是0.1μW，除以10000得到mW，再用 `10*log10(P_mW)` 转换为dBm。
        /// 这里有个小bug-计算了地址偏移量 i，但实际读的时候还是从地址50读取（没有用`i`），所以总是读通道0。
        /// </summary>
        /// <returns></returns>
        public float GetTxPower()
        {
            float txpow = 0;
            byte[] readbuffer = new byte[2];
            byte i = Convert.ToByte(TestSet.ch);
            i *= 2;
            i += 0x32;// 通道ch的起始地址 = 50 + ch*2
            if (i2c.TWI_ReadPage(0xa0, 50, readbuffer, 2) != 2)// 注意：这里实际只读了ch0
            {
                return -100; // Error
            }
            //
            if (readbuffer[0] == 0 && readbuffer[1] == 0)
            {
                readbuffer[1] = 1;// 避免log(0)
            }
            txpow = (float)(10 * Math.Log10((readbuffer[0] * 256 + readbuffer[1]) / 10000.0));
            return txpow;
        }
        /// <summary>
        /// GetRxPower - 读取接收光功率
        /// </summary>
        /// <returns></returns>
        public float GetRxPower()
        {
            float rxpow = 0;
            byte[] readbuffer = new byte[2];

            //SelectTable(6);
            byte i = Convert.ToByte(TestSet.ch);
            i *= 2;
            i += 0x22;
            if (i2c.TWI_ReadPage(0xa0, i, readbuffer, 2) != 2)
            {
                return -100; // Error
            }
            //
            if (readbuffer[0] == 0 && readbuffer[1] == 0)
            {
                readbuffer[1] = 1;
            }
            rxpow = (float)(10 * Math.Log10((readbuffer[0] * 256 + readbuffer[1]) / 10000.0));
            return rxpow;
        }
        /// <summary>
        /// GetDDMAnalogValues - 批量读取所有DDM模拟量 (Lines 336-445)
        /// 一次性读取并解析所有4个通道的5个DDM参数。分三次读取
        /// </summary>
        /// <returns></returns>
        public bool GetDDMAnalogValues()
        {
            byte[] readbuffer = new byte[36];

            if (i2c.TWI_ReadPage(0xa0, 22, readbuffer, 36) == 36)
            {        
                sbyte i = (sbyte)readbuffer[0];
                int j = Convert.ToInt32(i);

                TestResult.tempDDM = (j + readbuffer[1] * (1 / 256.0f));
                TestResult.vccDDM = ((readbuffer[4] * 256 + readbuffer[5]) / 10000.0f);   
                //bias
                if (readbuffer[20] == 0 && readbuffer[21] == 0)
                    readbuffer[21] = 1;

                if (readbuffer[22] == 0 && readbuffer[23] == 0)
                    readbuffer[23] = 1;

                if (readbuffer[24] == 0 && readbuffer[25] == 0)
                    readbuffer[25] = 1;

                if (readbuffer[26] == 0 && readbuffer[27] == 0)
                    readbuffer[27] = 1;

                TestResult.txBiasDDMbuf[0] = (float)((readbuffer[20] * 256 + readbuffer[21]) / 500.0);
                TestResult.txBiasDDMbuf[1] = (float)((readbuffer[22] * 256 + readbuffer[23]) / 500.0);
                TestResult.txBiasDDMbuf[2] = (float)((readbuffer[24] * 256 + readbuffer[25]) / 500.0);
                TestResult.txBiasDDMbuf[3] = (float)((readbuffer[26] * 256 + readbuffer[27]) / 500.0);

                //Rx
                if (readbuffer[12] == 0 && readbuffer[13] == 0)
                {
                    readbuffer[13] = 1;
                }

                if (readbuffer[14] == 0 && readbuffer[15] == 0)
                {
                    readbuffer[15] = 1;
                }

                if (readbuffer[16] == 0 && readbuffer[17] == 0)
                {
                    readbuffer[17] = 1;
                }

                if (readbuffer[18] == 0 && readbuffer[19] == 0)
                {
                    readbuffer[15] = 1;
                }
                TestResult.rxPowerDDMbuf[0] = (float)(10 * Math.Log10((readbuffer[12] * 256 + readbuffer[13]) / 10000.0));
                TestResult.rxPowerDDMbuf[1] = (float)(10 * Math.Log10((readbuffer[14] * 256 + readbuffer[15]) / 10000.0));
                TestResult.rxPowerDDMbuf[2] = (float)(10 * Math.Log10((readbuffer[16] * 256 + readbuffer[17]) / 10000.0));
                TestResult.rxPowerDDMbuf[3] = (float)(10 * Math.Log10((readbuffer[18] * 256 + readbuffer[19]) / 10000.0));
                //Tx
                if (readbuffer[28] == 0 && readbuffer[29] == 0)
                {
                    readbuffer[29] = 1;
                }

                if (readbuffer[30] == 0 && readbuffer[31] == 0)
                {
                    readbuffer[31] = 1;
                }

                if (readbuffer[32] == 0 && readbuffer[33] == 0)
                {
                    readbuffer[33] = 1;
                }

                if (readbuffer[34] == 0 && readbuffer[35] == 0)
                {
                    readbuffer[35] = 1;
                }
                TestResult.txPowerDDMbuf[0] = (float)(10 * Math.Log10((readbuffer[28] * 256 + readbuffer[29]) / 10000.0));
                TestResult.txPowerDDMbuf[1] = (float)(10 * Math.Log10((readbuffer[30] * 256 + readbuffer[31]) / 10000.0));
                TestResult.txPowerDDMbuf[2] = (float)(10 * Math.Log10((readbuffer[32] * 256 + readbuffer[33]) / 10000.0));
                TestResult.txPowerDDMbuf[3] = (float)(10 * Math.Log10((readbuffer[34] * 256 + readbuffer[35]) / 10000.0));
            }
            else
            {
                TestResult.tempDDM = 0;
                TestResult.vccDDM = 0;
                TestResult.txBiasDDM = 0;
                TestResult.txPowerDDM = -100;
                TestResult.rxPowerDDM = -100;

                TestResult.txBiasDDMbuf[0] = 0;
                TestResult.txBiasDDMbuf[1] = 0;
                TestResult.txBiasDDMbuf[2] = 0;
                TestResult.txBiasDDMbuf[3] = 0;

                TestResult.rxPowerDDMbuf[0] = 0;
                TestResult.rxPowerDDMbuf[1] = 0;
                TestResult.rxPowerDDMbuf[2] = 0;
                TestResult.rxPowerDDMbuf[3] = 0;

                TestResult.txPowerDDMbuf[0] = 0;
                TestResult.txPowerDDMbuf[1] = 0;
                TestResult.txPowerDDMbuf[2] = 0;
                TestResult.txPowerDDMbuf[3] = 0;
                return false;
            }
            if (i2c.TWI_ReadPage(0xa0, 34, readbuffer, 16) == 16)
            {
                if (readbuffer[0] == 0 && readbuffer[1] == 0)
                    readbuffer[1] = 1;

                if (readbuffer[2] == 0 && readbuffer[3] == 0)
                    readbuffer[3] = 1;

                if (readbuffer[4] == 0 && readbuffer[5] == 0)
                    readbuffer[5] = 1;

                if (readbuffer[6] == 0 && readbuffer[7] == 0)
                    readbuffer[7] = 1;

                TestResult.bias_ddm = ((readbuffer[8] * 256 + readbuffer[9]) / 500.0).ToString("F2");
                TestResult.bias_ddm += "/";
                TestResult.bias_ddm += ((readbuffer[10] * 256 + readbuffer[11]) / 500.0).ToString("F2");
                TestResult.bias_ddm += "/";
                TestResult.bias_ddm += ((readbuffer[12] * 256 + readbuffer[13]) / 500.0).ToString("F2");
                TestResult.bias_ddm += "/";
                TestResult.bias_ddm += ((readbuffer[14] * 256 + readbuffer[15]) / 500.0).ToString("F2");
                //
                TestResult.rxpwr_ddm = (10 * Math.Log10((readbuffer[0] * 256 + readbuffer[1]) / 10000.0)).ToString("F1");
                TestResult.rxpwr_ddm += "/";
                TestResult.rxpwr_ddm += (10 * Math.Log10((readbuffer[2] * 256 + readbuffer[3]) / 10000.0)).ToString("F1");
                TestResult.rxpwr_ddm += "/";
                TestResult.rxpwr_ddm += (10 * Math.Log10((readbuffer[4] * 256 + readbuffer[5]) / 10000.0)).ToString("F1");
                TestResult.rxpwr_ddm += "/";
                TestResult.rxpwr_ddm += (10 * Math.Log10((readbuffer[6] * 256 + readbuffer[7]) / 10000.0)).ToString("F1");
            }
            else
            {
                TestResult.bias_ddm = "0.0/0.0/0.0/0.0";
                TestResult.rxpwr_ddm = "-40/-40/-40/-40/-40";
            }

            if (i2c.TWI_ReadPage(0xa0, 50, readbuffer, 8) == 8)
            {
                if (readbuffer[0] == 0 && readbuffer[1] == 0)
                    readbuffer[1] = 1;

                if (readbuffer[2] == 0 && readbuffer[3] == 0)
                    readbuffer[3] = 1;

                if (readbuffer[4] == 0 && readbuffer[5] == 0)
                    readbuffer[5] = 1;

                if (readbuffer[6] == 0 && readbuffer[7] == 0)
                    readbuffer[7] = 1;

                TestResult.txpwr_ddm = (10 * Math.Log10((readbuffer[0] * 256 + readbuffer[1]) / 10000.0)).ToString("F1");
                TestResult.txpwr_ddm += "/";
                TestResult.txpwr_ddm += (10 * Math.Log10((readbuffer[2] * 256 + readbuffer[3]) / 10000.0)).ToString("F1");
                TestResult.txpwr_ddm += "/";
                TestResult.txpwr_ddm += (10 * Math.Log10((readbuffer[4] * 256 + readbuffer[5]) / 10000.0)).ToString("F1");
                TestResult.txpwr_ddm += "/";
                TestResult.txpwr_ddm += (10 * Math.Log10((readbuffer[6] * 256 + readbuffer[7]) / 10000.0)).ToString("F1");
            }
            else
            {
                TestResult.txpwr_ddm = "-40.0/-40.0/-40.0/-40.0";
            }               
            return true;
        }
        /// <summary>
        /// GetDDMThresholds - 读取告警阈值 (Lines 447-488)
        /// </summary>
        /// <returns></returns>
        public bool GetDDMThresholds()
        {
            byte[] readbuffer = new byte[72];//40
            int i = 0;
            SelectTable(3);//切换到Page 3（地址128(0x80)开始），读取72字节的DDM告警/警告阈值数据。每个参数有4个阈值

            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 72) != 72)
            {
                return false;
            }

            //告警阈值
            i = Convert.ToInt32((sbyte)readbuffer[0]);
            //高告警（严重超标）
            TestResult.tempHA = (float)(i + readbuffer[1] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[2]);
            //低告警
            TestResult.tempLA = (float)(i + readbuffer[3] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[4]);
            //高警告（接近超标）
            TestResult.tempHW = (float)(i + readbuffer[5] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[6]);
            //低警告
            TestResult.tempLW = (float)(i + readbuffer[7] * 1.0 / 256.0);

            TestResult.vccHA = (float)((readbuffer[16] * 256 + readbuffer[17]) / 10000.0);
            TestResult.vccLA = (float)((readbuffer[18] * 256 + readbuffer[19]) / 10000.0);
            TestResult.vccHW = (float)((readbuffer[20] * 256 + readbuffer[21]) / 10000.0);
            TestResult.vccLW = (float)((readbuffer[22] * 256 + readbuffer[23]) / 10000.0);

            TestResult.txBiasHA = (float)((readbuffer[56] * 256 + readbuffer[57]) / 500.0);
            TestResult.txBiasLA = (float)((readbuffer[58] * 256 + readbuffer[59]) / 500.0);
            TestResult.txBiasHW = (float)((readbuffer[60] * 256 + readbuffer[61]) / 500.0);
            TestResult.txBiasLW = (float)((readbuffer[62] * 256 + readbuffer[63]) / 500.0);

            TestResult.txPowerHA = (float)(10 * Math.Log10((readbuffer[64] * 256 + readbuffer[65]) / 10000.0));
            TestResult.txPowerLA = (float)(10 * Math.Log10((readbuffer[66] * 256 + readbuffer[67]) / 10000.0));
            TestResult.txPowerHW = (float)(10 * Math.Log10((readbuffer[68] * 256 + readbuffer[69]) / 10000.0));
            TestResult.txPowerLW = (float)(10 * Math.Log10((readbuffer[70] * 256 + readbuffer[71]) / 10000.0));

            TestResult.rxPowerHA = (float)(10 * Math.Log10((readbuffer[48] * 256 + readbuffer[49]) / 10000.0));
            TestResult.rxPowerLA = (float)(10 * Math.Log10((readbuffer[50] * 256 + readbuffer[51]) / 10000.0));
            TestResult.rxPowerHW = (float)(10 * Math.Log10((readbuffer[52] * 256 + readbuffer[53]) / 10000.0));
            TestResult.rxPowerLW = (float)(10 * Math.Log10((readbuffer[54] * 256 + readbuffer[55]) / 10000.0));
            //
            return true;
        }
        /// <summary>
        /// GetDDMFlagsInterrupt 读取告警标志位 (Lines 491-575)
        /// </summary>
        /// <returns></returns>
        public bool GetDDMFlagsInterrupt()
        {
            byte[] readbuffer = new byte[19];
            if (i2c.TWI_ReadPage(0xa0, 0, readbuffer, 19) != 19)
            {
                return false;
            }

            TestResult.tempHA_flag = Bit.GetBit(readbuffer[6], 7);
            TestResult.tempLA_flag = Bit.GetBit(readbuffer[6], 6);
            TestResult.vccHA_flag = Bit.GetBit(readbuffer[7], 7);
            TestResult.vccLA_flag = Bit.GetBit(readbuffer[7], 6);

            TestResult.tempHW_flag = Bit.GetBit(readbuffer[6], 5);
            TestResult.tempLW_flag = Bit.GetBit(readbuffer[6], 4);
            TestResult.vccHW_flag = Bit.GetBit(readbuffer[7], 5);
            TestResult.vccLW_flag = Bit.GetBit(readbuffer[7], 4);

            int i = TestSet.ch;
            if (i == 0)
            {
                TestResult.rxPwrHA_flag = Bit.GetBit(readbuffer[9], 7);
                TestResult.rxPwrLA_flag = Bit.GetBit(readbuffer[9], 6);
                TestResult.txBiasHA_flag = Bit.GetBit(readbuffer[11], 7);
                TestResult.txBiasLA_flag = Bit.GetBit(readbuffer[11], 6);
                TestResult.txPwrHA_flag = Bit.GetBit(readbuffer[13], 7);
                TestResult.txPwrLA_flag = Bit.GetBit(readbuffer[13], 6);

                TestResult.rxPwrHW_flag = Bit.GetBit(readbuffer[9], 5);
                TestResult.rxPwrLW_flag = Bit.GetBit(readbuffer[9], 4);
                TestResult.txBiasHW_flag = Bit.GetBit(readbuffer[11], 5);
                TestResult.txBiasLW_flag = Bit.GetBit(readbuffer[11], 4);
                TestResult.txPwrHW_flag = Bit.GetBit(readbuffer[13], 5);
                TestResult.txPwrLW_flag = Bit.GetBit(readbuffer[13], 4);        
            }
            else if (i == 1)
            {
                TestResult.rxPwrHA_flag = Bit.GetBit(readbuffer[9], 3);
                TestResult.rxPwrLA_flag = Bit.GetBit(readbuffer[9], 2);
                TestResult.txBiasHA_flag = Bit.GetBit(readbuffer[11], 3);
                TestResult.txBiasLA_flag = Bit.GetBit(readbuffer[11], 2);
                TestResult.txPwrHA_flag = Bit.GetBit(readbuffer[13], 3);
                TestResult.txPwrLA_flag = Bit.GetBit(readbuffer[13], 2);

                TestResult.rxPwrHW_flag = Bit.GetBit(readbuffer[9], 1);
                TestResult.rxPwrLW_flag = Bit.GetBit(readbuffer[9], 0);
                TestResult.txBiasHW_flag = Bit.GetBit(readbuffer[11], 1);
                TestResult.txBiasLW_flag = Bit.GetBit(readbuffer[11], 0);
                TestResult.txPwrHW_flag = Bit.GetBit(readbuffer[13], 1);
                TestResult.txPwrLW_flag = Bit.GetBit(readbuffer[13], 0);    
            }
            else if (i == 2)
            {
                TestResult.rxPwrHA_flag = Bit.GetBit(readbuffer[10], 7);
                TestResult.rxPwrLA_flag = Bit.GetBit(readbuffer[10], 6);
                TestResult.txBiasHA_flag = Bit.GetBit(readbuffer[12], 7);
                TestResult.txBiasLA_flag = Bit.GetBit(readbuffer[12], 6);
                TestResult.txPwrHA_flag = Bit.GetBit(readbuffer[14], 7);
                TestResult.txPwrLA_flag = Bit.GetBit(readbuffer[14], 6);

                TestResult.rxPwrHW_flag = Bit.GetBit(readbuffer[10], 5);
                TestResult.rxPwrLW_flag = Bit.GetBit(readbuffer[10], 4);
                TestResult.txBiasHW_flag = Bit.GetBit(readbuffer[12], 5);
                TestResult.txBiasLW_flag = Bit.GetBit(readbuffer[12], 4);
                TestResult.txPwrHW_flag = Bit.GetBit(readbuffer[14], 5);
                TestResult.txPwrLW_flag = Bit.GetBit(readbuffer[14], 4);
            }
            else if (i == 3)
            {
                TestResult.rxPwrHA_flag = Bit.GetBit(readbuffer[10], 3);
                TestResult.rxPwrLA_flag = Bit.GetBit(readbuffer[10], 2);
                TestResult.txBiasHA_flag = Bit.GetBit(readbuffer[12], 3);
                TestResult.txBiasLA_flag = Bit.GetBit(readbuffer[12], 2);
                TestResult.txPwrHA_flag = Bit.GetBit(readbuffer[14], 3);
                TestResult.txPwrLA_flag = Bit.GetBit(readbuffer[14], 2);

                TestResult.rxPwrHW_flag = Bit.GetBit(readbuffer[10], 1);
                TestResult.rxPwrLW_flag = Bit.GetBit(readbuffer[10], 0);
                TestResult.txBiasHW_flag = Bit.GetBit(readbuffer[12], 1);
                TestResult.txBiasLW_flag = Bit.GetBit(readbuffer[12], 0);
                TestResult.txPwrHW_flag = Bit.GetBit(readbuffer[14], 1);
                TestResult.txPwrLW_flag = Bit.GetBit(readbuffer[14], 0);
            }
            return true;
        }

        /// <summary>
        /// GetFlashInfo - 读取基础Flash信息
        /// </summary>
        /// <returns></returns>
        public bool GetFlashInfo()
        {
            byte[] readbuffer = new byte[256];
            int i;

            SelectTable(0);
            if (i2c.TWI_ReadPage(0xa0, 0, readbuffer, 256) != 256) // Page 0的0-255 复制到 flash_data[0-255]
            {
                return false;
            }

            for (i = 0; i < 256; i++)
            {
                TestResult.flash_data[i] = readbuffer[i];
            }
            SelectTable(1);
            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 128) != 128) // Page 1的128-255 复制到 flash_data[256-383]
            {
                return false;
            }

            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 256] = readbuffer[i];
            }
            SelectTable(2); // 同样处理 → flash_data[384-511]
            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 128) != 128)
            {
                return false;
            }

            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 384] = readbuffer[i];
            }
            SelectTable(3); // 阈值页 → flash_data[512-639]
            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 128) != 128)
            {
                return false;
            }

            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 512] = readbuffer[i];
            }
            // 从固定位置提取ASCII字符串信息
            TestResult.sn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 196, 16); // 序列号
            TestResult.pn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 168, 16); // 部件号
            TestResult.vn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 148, 16); // 厂商名
            TestResult.date = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 212, 8); // 生产日期
            //TestResult.sn.TrimEnd();
            //TestResult.pn.TrimEnd();
            //TestResult.vn.TrimEnd();
            //TestResult.date.TrimEnd();

            return true;
        }
        /// <summary>
        /// GetFlashInfoDebug - 读取调试页Flash信息
        /// 读取厂商自定义页面的调试数据和三个补偿表（LUT）。同时解析飞思卓内部12位流水号
        /// </summary>
        /// <returns></returns>
        public bool GetFlashInfoDebug()
        {
            byte[] readbuffer = new byte[256];
            int i;

            SelectTable(6); // → flash_data[640-767]  调试寄存器页
            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 128) != 128)
            {
                return false;
            }

            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 640] = readbuffer[i];
            }
            //
            SelectTable(8); // → flash_data[640-767]  调试寄存器页
            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 768] = readbuffer[i];
            }
            //
            SelectTable(9); // → flash_data[896-1023] MOD LUT页
            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 896] = readbuffer[i];
            }
            //
            SelectTable(10); // → flash_data[1024-1151] APD LUT页
            if (i2c.TWI_ReadPage(0xa0, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 1024] = readbuffer[i];
            }


            // 更新 Fsn  飞思卓产品内部流水号
            UInt64 iFsn = 0;
            iFsn += TestResult.flash_data[640 + 92]; // 0xF7 = 247
            iFsn <<= 8;
            iFsn += TestResult.flash_data[640 + 93]; // 0xF8 = 248
            iFsn <<= 8;
            iFsn += TestResult.flash_data[640 + 94]; // 0xF9 = 249
            iFsn <<= 8;
            iFsn += TestResult.flash_data[640 + 95]; // 0xFA = 250
            iFsn <<= 8;
            iFsn += TestResult.flash_data[640 + 96]; // 0xFB = 251

            if (iFsn > TestResult.max_Fsn)
            {
                iFsn = TestResult.max_Fsn;
            }
            TestResult.fibertop_sn = iFsn.ToString("D12");
            //TestResult.fibertop_sn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 512+243, 10); //0xF3 = 245
            //TestResult.fibertop_sn.TrimEnd();
            //TestResult.fibertop_sn = TestResult.sn; //test

            return true;
        }
        /// <summary>
        /// CheckThresholdsInfo - 校验告警阈值
        /// 将模块中实际烧录的阈值与数据库加载的标准值进行比对。
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool CheckThresholdsInfo(ref string errMsg)
        {
            byte[] threshold_read = new byte[72];
            byte[] ex_cal_read = new byte[39];
            //byte check_sum = 0;
            int i = 0;

            errMsg = "";

            for (i = 0; i < 72; i++)
            {
                threshold_read[i] = TestResult.flash_data[i + 512];
            }

            //for (i = 0; i < 39; i++)
            //{
            //    //ex_cal_read[i] = TestResult.flash_data[i + 256 + 56];
            //}

            // check 告警门限
            if (!Bit.ByteEquals(threshold_read, threshold))
            {
                errMsg += "A2h告警门限 ";
            }

            // check  外校准系数
            //if (!Bit.ByteEquals(ex_cal_read, ex_cal))
            //{
            //    errMsg += "A2h校准参数 ";
            //}

            //check_sum = 0x00;
            //for (i = 0; i < 95; i++)
            //{
            //    check_sum += TestResult.flash_data[i + 256];
            //}

            //// check sum errro
            //if (check_sum != TestResult.flash_data[95 + 256])
            //{
            //    errMsg += "A2h的0-94字节的 check sum 错误";
            //}

            if (string.IsNullOrEmpty(errMsg))
            {
                return true; // OK
            }
            else
            {
                return false; // Error
            }
        }
        /// <summary>
        /// CheckModuleFlashInfo - 校验补偿表(LUT)
        /// APC补偿表校验：检查每个通道的4个温度点是否与标准表一致（通过检查差值数组q的Max和Min是否相等，
        /// 即所有点差值相同=整体偏移了固定值，但代码直接比较是否完全一致）
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public bool CheckModuleFlashInfo(ref string errMsg)
        {
            byte[] apclut_read = new byte[32];
            byte[] modlut_read = new byte[32];
            byte[] apdlut_read = new byte[32];

            int[] q = new int[32];

            int i = 0;

            errMsg = "";

            for (i = 0; i < 32; i++)
            {
                apclut_read[i] = TestResult.flash_data[i + 768];
                modlut_read[i] = TestResult.flash_data[i + 896];
                apdlut_read[i] = TestResult.flash_data[i + 1024];
            }

            // check  APC 补偿表
            for (i = 0; i < 28; i++)
            {
                q[i] = apclut_read[i] - apclut[i];
            }
            if (q.Max() != q.Min())
            {
                errMsg += "APC补偿表 ";
            }

            // check  MOD 补偿表
            byte[] modlut_fk = new byte[32];
            float fk = 0;
            fk = modlut_read[13]; // 25度 补偿点
            fk /= modlut[13];
            //
            for (i = 0; i < 32; i++)
            {
                q[i] = modlut_read[i] - modlut[i];
            }
            if (q.Max() != q.Min()) // 平移检查错误  进入比例缩放检查
            {
                for (i = 0; i < 32; i++)
                {
                    modlut_fk[i] = (byte)(fk * modlut[i]);
                    q[i] = modlut_read[i] - modlut_fk[i];
                }
                if (Math.Abs(q.Max() - q.Min()) > 2)
                {
                    errMsg += "MOD补偿表 "; // 两种方式检查都有问题  报错
                }
            }

            // check  APD 补偿表
            for (i = 0; i < 32; i++)
            {
                q[i] = apdlut_read[i] - apdlut[i];
            }
            if ((string.IsNullOrEmpty(TestSet.apdName) == false)) // APD
            {
                if (q.Max() != q.Min())
                {
                    errMsg += "APD补偿表 ";
                }
            }

            if (string.IsNullOrEmpty(errMsg))
            {
                return true; // OK
            }
            else
            {
                return false; // Error
            }
        }
        /// <summary>
        /// Access数据库操作
        /// GetModuleTypeFromAccessdb - 获取模块型号列表
        /// </summary>
        /// <param name="str"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        // 从Access数据库的 `ModuleType` 表读取所有支持的模块型号名称列表，用于UI下拉框选择
        public bool GetModuleTypeFromAccessdb(ref string[] str, ref int len)
        {
            OleDbConnection dbconnect;
            OleDbCommand dbcommand;
            OleDbDataAdapter dbadapter;
            DataSet dbset;

            len = 0;

            try
            {
                // 连接Access数据库
                dbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + GlobalVarFun.moduleLutDBFilePath);
                dbcommand = new OleDbCommand("select Type from ModuleType", dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, "ModuleType");

                len = dbset.Tables["ModuleType"].Rows.Count;
                if (len <= 0 || len > str.Length)
                {
                    dbconnect.Close();
                    dbcommand.Dispose();
                    dbadapter.Dispose();
                    dbset.Dispose();
                    GlobalVarFun.access_connect_status = false;
                    return false;
                }
                //
                len = 0;
                foreach (DataRow dataRow in dbset.Tables["ModuleType"].Rows)
                {
                    if (dataRow["Type"].ToString() != "")
                    {
                        str[len] = Convert.ToString(dataRow["Type"]);
                        len++;
                    }
                }
                //
                dbconnect.Close();
                dbcommand.Dispose();
                dbadapter.Dispose();
                dbset.Dispose();
                //
                GlobalVarFun.access_connect_status = true;
            }
            catch //(Exception exp)
            {
                GlobalVarFun.access_connect_status = false;
                return false;
            }

            return true;
        }
        /// <summary>
        /// 这是关键的参数加载方法，根据当前模块型号 `TestResult.fibertop_pn` 从数据库加载所有测试参数：
        /// </summary>
        /// <returns></returns>
        public bool GetTypeDebugInfoFromAccessdb()
        {
            OleDbConnection dbconnect;
            OleDbCommand dbcommand;
            OleDbDataAdapter dbadapter;
            DataSet dbset;
            string dbconnectionstr = "";
            int i = 0;

            string Apc_LUT_Field = "BiasVal";
            string Mod_LUT_Field = "ModVal";
            string Apd_LUT_Field = "ApdVal";
            string str = "";
            
            try
            {
                dbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + GlobalVarFun.moduleLutDBFilePath);
                dbconnectionstr = string.Format("select TxPowerSetPoint,TxERSetPoint,TxER_MIN,TxER_MAX,Bias_MAX,Bias_MIN,TxPower_MIN,TxPower_MAX,TxCr_MIN,TxCr_MAX,TxJt_MAX,Sensitivity,RxALos,RxDLos,RxOverLoad,"
                                             + "RxPwrCheck_1,RxPwrCheck_2,RxPwrCheck_3,RxPwrCheck_4,RxPwrCheck_5,APD_Name,APCmin,APCmax,MODmin,MODmax,LOSmin,LOSmax,Remark from ModuleType where Type = '{0}'", TestResult.fibertop_pn);
                
                //
                dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, "ModuleType");

                // APD
                TestSet.apdName = dbset.Tables["ModuleType"].Rows[0]["APD_Name"].ToString();
                TestSet.apdName.Trim();

                TestSet.txPwr_target = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxPowerSetPoint"].ToString());
                TestSet.txEr_target = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxERSetPoint"].ToString());

                TestSet.txPwr_Min = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxPower_MIN"].ToString());
                TestSet.txPwr_Max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxPower_MAX"].ToString());
                TestSet.bias_Min = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["Bias_MIN"].ToString());
                TestSet.bias_Max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["Bias_MAX"].ToString());
                TestSet.txEr_Min = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxER_MIN"].ToString());
                TestSet.txEr_Max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxER_MAX"].ToString());
               // TestSet.txCr_Min = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxCr_MIN"].ToString());
               // TestSet.txCr_Max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxCr_MAX"].ToString());
               // TestSet.txJt_Max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxJt_MAX"].ToString());

                TestSet.txBias_target = (TestSet.bias_Min + TestSet.bias_Max) / 2;

                TestSet.rx_Sen = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["Sensitivity"].ToString());
                TestSet.rx_ALos = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxALos"].ToString());
                TestSet.rx_DLos = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxDLos"].ToString());
                TestSet.rx_OverLoad = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxOverLoad"].ToString());
                TestSet.rxPwr_Real[0] = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxPwrCheck_1"].ToString());
                TestSet.rxPwr_Real[1] = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxPwrCheck_2"].ToString());
                TestSet.rxPwr_Real[2] = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxPwrCheck_3"].ToString());
                TestSet.rxPwr_Real[3] = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxPwrCheck_4"].ToString());
                TestSet.rxPwr_Real[4] = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["RxPwrCheck_5"].ToString());

                //TestResult.mask_name = dbset.Tables["ModuleType"].Rows[0]["MaskName"].ToString().Trim();
                //str = dbset.Tables["ModuleType"].Rows[0]["MaskMargin"].ToString().Trim();
                //if (!string.IsNullOrEmpty(str))
                //{
                //    TestResult.mask_margin = Convert.ToUInt16(str);
                //}

                /////////////////////////////////////////////////////////////////////////////////
                TestSet.txapc_Min = 30;
                TestSet.txapc_Max = 100;
                TestSet.txmod_Min = 30;
                TestSet.txmod_Max = 90;
                TestSet.rxlos_Min = 20;
                TestSet.rxlos_Max = 100;
                //
                str = dbset.Tables["ModuleType"].Rows[0]["APCmin"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txapc_Min = Convert.ToUInt16(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["APCmax"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txapc_Max = Convert.ToUInt16(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["MODmin"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txmod_Min = Convert.ToUInt16(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["MODmax"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txmod_Max = Convert.ToUInt16(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["LOSmin"].ToString().Trim();
                if (str != "")
                {
                    TestSet.rxlos_Min = Convert.ToUInt16(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["LOSmax"].ToString().Trim();
                if (str != "")
                {
                    TestSet.rxlos_Max = Convert.ToUInt16(str);
                }
                /////////////////////////////////////////////////////////////////////////////////

                //
                dbconnectionstr = string.Format("select Page03,{1},{2},{3} from [{0}]", TestResult.fibertop_pn, Apc_LUT_Field, Mod_LUT_Field, Apd_LUT_Field);
                
                dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, TestResult.fibertop_pn);
                //
                for (i = 0; i < 72; i++)
                {
                    threshold[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i]["Page03"]);
                }
                
                for (i = 0; i < 32; i++)
                {
                    apclut[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i][Apc_LUT_Field]);
                }

                for (i = 0; i < 32; i++) 
                {
                    modlut[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i][Mod_LUT_Field]);
                }

                for (i = 0; i < 32; i++)
                {
                    apdlut[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i][Apd_LUT_Field]);
                }

                //
                dbconnect.Close();
                dbcommand.Dispose();
                dbadapter.Dispose();
                dbset.Dispose();

                GlobalVarFun.access_connect_status = true;
            }
            catch //(Exception exp)
            {
                GlobalVarFun.access_connect_status = false;
                return false;
            }
            return true;
        }
        /// <summary>
        /// 读APC偏置设置值(0xA0+ch)
        /// </summary>
        /// <returns></returns>
        public byte GetTxApcBiasSet()
        {
            byte i = (byte)TestSet.ch;
            i += 0xA0;

            byte rtnVal = i2c.TWI_ReadByte(0xa0, i);
            return rtnVal;
        }
        /// <summary>
        /// 读MOD调制设置值(0xA4+ch)
        /// </summary>
        /// <returns></returns>
        public byte GetTxModBiasSet()
        {
            byte i = (byte)TestSet.ch;
            i += 0xA4;

            byte rtnVal = i2c.TWI_ReadByte(0xa0, i);
            return rtnVal;
        }
        /// <summary>
        /// 读当前通道Tx功率(dBm)
        /// </summary>
        /// <returns></returns>
        public double GetTxPwr()
        {
            byte[] buffer = new byte[2];
            double txpwr = 0.0f;
            byte i = (byte)TestSet.ch;
            i2c.TWI_ReadPage(0xa0, (byte)(50 + 2 * i),buffer, 2);
            txpwr = (10 * Math.Log10((buffer[0] * 256 + buffer[1]) / 10000.0));
            return txpwr;
        }

        // 初测调试功能函数
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 设置RX VAGC（可变增益控制）
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public bool SetRxVagc(UInt16 setVal)
        {
            if (i2c == null || TestSet.ch < 0 || TestSet.ch > 3)
                return false;

            if (!SelectTable(0x06))
                return false;

            if (setVal > 255)
                setVal = 255;

            byte address = (byte)(0x9B + TestSet.ch);
            bool result = i2c.TWI_WriteByte(0xA0, address, (byte)setVal);
            Thread.Sleep(30);
            return result;
        }

        public bool SaveER1Calibration()
        {
            if (i2c == null || !SelectTable(0x06))
                return false;

            bool result = i2c.TWI_WriteByte(0xA0, 0x83, 0x02);
            if (result)
                Thread.Sleep(700);
            return result;
        }
        /// <summary>
        /// 这些方法用于校准过程中设置各种DAC值，必须先切换到Page 6（调试页）
        /// 设置APC（自动功率控制）DAC值，0~255，控制激光器偏置电流
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public bool SetTxApcBias(UInt16 setVal)
        {
            byte i = (byte)TestSet.ch;
            i += 0xA0;
            if (!SelectTable(6)) return false;
            if (setVal > 255)
            {
                setVal = 255;
            }
            TestResult.txapcVal = setVal;
            bool rtnVal = i2c.TWI_WriteByte(0xa0, i, (byte)setVal);
            Thread.Sleep(30);
            return rtnVal;
        }
        /// <summary>
        /// 设置MOD（调制电流）DAC值，0~255，控制消光比
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public bool SetTxModBias(UInt16 setVal)
        {
            byte i = (byte)TestSet.ch;
            i += 0xA4;
            if (!SelectTable(6)) return false;
            if (setVal > 255)
            {
                setVal = 255;
            }
            TestResult.txmodVal = setVal;
            bool rtnVal = i2c.TWI_WriteByte(0xa0, i, (byte)setVal);
            Thread.Sleep(60);
            return rtnVal;
        }
        /// <summary>
        /// 设置LOS阈值DAC值
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public bool SetRxLos(UInt16 setVal)
        {
            int ch = TestSet.ch;
            byte i = (byte)ch;

            if (!SelectTable(6)) return false;
            if (setVal > 255)
            {
                setVal = 255;
            }
            i += 0xB4;      
            TestResult.rxlosVal = setVal;
            bool rtnVal = i2c.TWI_WriteByte(0xa0, i, (byte)setVal);
            Thread.Sleep(60); // 延时
            return rtnVal;
        }
        /// <summary>
        /// 设置TOSA温度（TEC控制），830~1830对应DAC值，转换为电压2.5V/4095*val
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public bool SetTOSATemp(UInt16 setVal)
        {
            if (setVal > 1830)
            {
                setVal = 1830;
            }
            if (setVal < 830)
            {
                setVal = 830;
            }
            float vtemp = (float)(setVal * (2.5 / 4095));
            //Temp_degree_textBox.Text = VoltagetoTemperature(vtemp).ToString("F2") + "℃";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return false;

            byte[] writebuffer = BitConverter.GetBytes((UInt16)setVal);
            Array.Reverse(writebuffer); //高字节在前
            if (i2c.TWI_WritePage(0xA0, 0xCC, writebuffer, 2) != 2) return false;
            return true;
        }
        /// <summary>
        /// 设置EML激光器的VON负压（偏置电压）
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public bool SetVON(UInt16 setVal)
        {
            if (setVal > 255)
            {
                setVal = 255;
            }
            float von = -(float)(setVal * (2.5 / 255));
            //Von_V_textBox.Text = von.ToString("F2") + "V";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return false;
            byte i = (byte)TestSet.ch;
            i += 0xC8;
            return i2c.TWI_WriteByte(0xa0, i, (byte)setVal);
        }
        /// <summary>
        /// 设置APD偏置电压DAC值
        /// </summary>
        /// <param name="setVal"></param>
        /// <returns></returns>
        public bool SetAPD(UInt16 setVal)
        {
            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return false;
            byte i = (byte)TestSet.ch;
            i += 0xBC;          
            return i2c.TWI_WriteByte(0xa0, i, (byte)setVal);
        }
        /// <summary>
        /// 发送TX使能命令，tx_tec_test时写0x66
        /// </summary>
        /// <returns></returns>
        public bool SetTx_EN()
        {         
            //byte[] pwd = new byte[4];
            //pwd[0] = 0x00;
            //pwd[1] = 0x00;
            //pwd[2] = 0x00;
            //pwd[3] = 0x00;
            //i2c.TWI_ReadPage(0xA0, 123, pwd, 4);
            //if ((pwd[0] != 0xA9) || (pwd[1] != 0x46) || (pwd[2] != 0x50) || (pwd[3] != 0x54))
            //{
            //    //待测QSFP模块不在调试模式下，无法保存，请确认
            //    return false;
            //}
            ////
            if (!SelectTable(6))
            {
                //选择表错误
                return false;
            }

            byte wrtBuf = 0;

            if (GlobalVarFun.tx_tec_test)
            {
                wrtBuf = 0x66;         
            }

            if (!i2c.TWI_WriteByte(0xA0, 0xFA, wrtBuf))
            {
                //发送TX EN命令错误
                return false;
            }
            return true;
        }
        /// <summary>
        ///0xE8+ch*2: 当前通道的RX功率ADC原始值
        /// </summary>
        /// <returns></returns>
        public UInt16 GetRxADC()
        {
            UInt16 rxadc = 0;
            byte[] readbuffer = new byte[2];

            SelectTable(6);
            byte i = Convert.ToByte(TestSet.ch);
            i *= 2;
            i += 0xE0;
            i += 8;
            if (i2c.TWI_ReadPage(0xa0, i, readbuffer, 2) == 2)
            {
                rxadc = readbuffer[0];
                rxadc <<= 8;
                rxadc += readbuffer[1];
            }
            else
            {
                rxadc = 0;
            }
            //
            return rxadc;
        }
        /// <summary>
        /// 读取模块内部12位ADC的原始采样值（0~4095或0~65535）。
        /// TX ADC用于监测发射光功率（背光二极管MPD电流），RX ADC用于监测接收光功率
        /// 0xE0+ch*2: 当前通道的TX功率监视ADC原始值 
        /// </summary>
        /// <returns></returns>
        public UInt16 GetTxADC()
        {
            UInt16 txadc = 0;
            byte[] readbuffer = new byte[2];
            SelectTable(6);

            byte i = (byte)TestSet.ch;
            i *= 2;
            i += 0xE0;

            if (i2c.TWI_ReadPage(0xa0, i, readbuffer, 2) == 2)
            {
                txadc = readbuffer[0];
                txadc <<= 8;
                txadc += readbuffer[1];
            }
            else
            {
                txadc = 0;
            }
            //
            return txadc;
        }
        /// <summary>
        /// 校准数据写入方法 (Lines 1054-1150)
        /// WriteTxCalData - 写发射功率校准参数
        /// 发射功率校准用线性比例法：光功率(uW) = k × ADC值。在校准点上，已知实际光功率（外部光功率计测得，存于TestResult.txPower，单位dBm），
        /// 和对应的ADC读值，计算出系数k写入模块，模块固件运行时用这个k把ADC值换算成光功率。
        /// </summary>
        /// <returns></returns>
        public bool WriteTxCalData()
        {
            byte[] writeByte = new byte[4];
            byte[] readByte  = new byte[4];
            byte[] c0 = new byte[4];
            //byte[] calpwr_uw = new byte[2];
            float ADC = 0;

            //读取当前TX ADC值
            ADC = GetTxADC();
            if (ADC <= 1)
            {
                return false;
            }
            // 2. 计算校准斜率k：光功率(uW) / ADC值 将k转为4字节float写入Page 7的校准区(0x80+ch*4)
            TestResult.txPwrCal_k = ((float)((Math.Pow(10, TestResult.txPower / 10.0) * 10000.0) / ADC));
            TestResult.txPwrCal_b = 0;
            
            c0 = BitConverter.GetBytes(TestResult.txPwrCal_k);
            c0.CopyTo(writeByte,0);
            //Array.Reverse(writeByte); //反转，高字节在前

            if (!SelectTable(7))
            {
                return false;
            }

             byte i = (byte)TestSet.ch;
            i *= 4;
            i += 0x00;
            i += 0x80;
            if (i2c.TWI_WritePage(0xa0, i, writeByte, 4) != 4)
            {
                return false;
            }

            if (i2c.TWI_ReadPage(0xa0, i, readByte, 4) != 4)
            {
                return false;
            }

            //Thread.Sleep(30);
            // 发送保存命令到 flash
            //if (WritSaveCmd() == false) return false;
            //Thread.Sleep(200);

            return (Bit.ByteEquals(writeByte, readByte));
        }
        /// <summary>
        /// 写接收功率校准参数
        /// 接收功率校准二次多项式：`P_dBm = c0 + c1*ADC + c2*ADC²`（或类似公式），
        /// 三个float系数共12字节。写完后调用 `WritSaveCmd()` 保存到Flash。
        /// </summary>
        /// <returns></returns>
        public bool WriteRxCalData()
        {
            byte[] writeByte = new byte[20];
            byte[] readByte  = new byte[20];
            byte[] nowritebuf = new byte[2];
            byte[] noreadbuf = new byte[2];
            byte[] c0 = new byte[4];     //C0 C1 C2 C3 C4 数组 
            byte[] c1 = new byte[4];
            byte[] c2 = new byte[4];
            byte[] c3 = new byte[4];
            byte[] c4 = new byte[4];
            // 将c0, c1, c2三个float系数(12字节)写入Page 7
            c0 = BitConverter.GetBytes(TestResult.rxPwrCal_c[0]);
            c1 = BitConverter.GetBytes(TestResult.rxPwrCal_c[1]);
            c2 = BitConverter.GetBytes(TestResult.rxPwrCal_c[2]);
            //c3 = BitConverter.GetBytes(TestResult.rxPwrCal_c[3]);
           // c4 = BitConverter.GetBytes(TestResult.rxPwrCal_c[4]);
            //
            //Array.Reverse(c0); //反转，高字节在前
            //Array.Reverse(c1);
            //Array.Reverse(c2);
           // Array.Reverse(c3);
            //Array.Reverse(c4);
            //
           // c4.CopyTo(writeByte, 0);
          //  c3.CopyTo(writeByte, 4);
            c2.CopyTo(writeByte, 8);
            c1.CopyTo(writeByte, 4);
            c0.CopyTo(writeByte, 0);
            //
            if (!SelectTable(7))
            {
                return false;
            }
            // 地址: 0x90 + ch*16
            byte i;
            i = Convert.ToByte(TestSet.ch);
            i *= 16;
            i += 0x10;
            i += 0x80;

            //writeByte[0] = 119;
            //writeByte[1] = 165;
            //writeByte[2] = 240;
            //writeByte[3] = 192;
            //writeByte[4] = 135;
            //writeByte[5] = 62;
            //writeByte[6] = 28;
            //writeByte[7] = 65;

            if (i2c.TWI_WritePage(0xa0, i, writeByte, 12) != 12)
            {
                return false;
            }
            
            if (i2c.TWI_ReadPage(0xa0, i, readByte, 12) != 12)
            {
                return false;
            }

            //Thread.Sleep(30);

            // 发送保存命令到 flash
            // 然后发送保存命令 WritSaveCmd()（写0x02到0x83）
            if (WritSaveCmd() == false) return false;
            Thread.Sleep(200);

            return (Bit.ByteEquals(writeByte, readByte));
        }
        /// <summary>
        /// 调试后保存数据方法
        /// SaveRxDataAfterDebug - 保存接收端调试数据
        /// APD接收端校准后，在当前温度点测得最佳APD DAC值，
        /// 与标准表对比得到偏移量`delta_apd`，然后把整个表的该通道全部平移这个量（限幅0~255），实现温度补偿表的更新
        /// </summary>
        /// <returns></returns>
        public bool SaveRxDataAfterDebug()
        {
            //发送保存命令到 flash
            byte[] apd = new byte[128];
            byte[] r_apd = new byte[128];
            int  temp_value, temp_index;
            int delta_apd = 0;
            if (GlobalVarFun.rx_is_apd) // 如果是APD接收器
            {
                // 1. 获取当前温度，计算温度索引(每5°C一个点)
                temp_value = (int)GetTemp();
                if (temp_value < -40) temp_value = -40;
                if (temp_value > 115) temp_value = 115;
                temp_index = (temp_value + 40) / 5; // -40°C→0, 25°C→13, 85°C→25
                if (temp_index < 0) temp_index = 0;
                if (temp_index > 31) temp_index = 31;

                // 2. 计算当前温度点APD值与标准表的差值（平移量）
                delta_apd = TestResult.rxapdVal - apdlut[temp_index];
                // 3. 读取APD LUT页(Table 10)
                if (!SelectTable(10)) return false; 
                if (i2c.TWI_ReadPage(0xa0, 0x80, apd, 128) != 128) return false;
                // 4. 对32个温度点，每个点4个通道，将当前通道加上平移量
                for (int i = 0; i < 32; i++)
                {
                    int apd_lut = delta_apd + apdlut[i];
                    if (apd_lut < 0) apd_lut = 0;
                    if (apd_lut > 255) apd_lut = 255;
                    apd[4 * i + TestSet.ch] = (byte)apd_lut;
                }

                // 选择补偿表 APD 0A
                if (!SelectTable(10)) return false;
                //5.写回APD LUT
                if (i2c.TWI_WritePage(0xa0, 0x80, apd, 128) != 128) return false;
                WriteRxSaveCmd();
                if (!SelectTable(10)) return false;   
                if (i2c.TWI_ReadPage(0xa0, 0x80, r_apd, 128) != 128) return false;
                if (!Bit.ByteEquals(apd, r_apd))
                {
                    return false;
                }
                return true;
            }
            else// PIN接收器，直接保存
            {
                return WriteRxSaveCmd();
            }
        }
        // 内部调用函数
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool WriteRxSaveCmd()
        {
            if (!SelectTable(6))
            {
                return false;
            }
            // 发送保存命令
            //byte[] saveByte = new byte[3];
            //saveByte[0] = 0x08; // threshold  Page03   bit3  0x82地址
            //saveByte[1] = 0x0D; // 00001101  bit3 bit2 bit0  0x83地址
            //if (GlobalVarFun.rx_is_apd)
            //{
            //    saveByte[1] |= 0x10; //bit4=1 APD_LUT
            //}
            //saveByte[1] |= 0x40; //bit6=1 SOA_LUT
            //if (i2c.TWI_WritePage(0xa0, 0x82, saveByte, 2) != 2)
            //{
            //    return false;
            //}
            //return true;
            return i2c.TWI_WriteByte(0xA0, 0x83, 0x11);//保存Table 6,Table 10(APD补偿)
        }
        /// <summary>
        /// SaveTxDataAfterDebug - 保存发射端调试数据
        /// </summary>
        /// <returns></returns>
        public bool SaveTxDataAfterDebug()
        {
            byte[] apc = new byte[128];
            byte[] mod = new byte[128];

            byte[] apc_read = new byte[128];
            byte[] mod_read = new byte[128];

            byte[] Page03_write = new byte[72];
            byte[] Page03_read = new byte[72];
            int i, delta_apc, delta_mod, apc_value, mod_value;

            float temp_value = 25;
            int temp_index = 0;
            int ch = 0;
            ch = TestSet.ch;
            if (!SelectTable(8)) return false;
            if (i2c.TWI_ReadPage(0xa0, 0x80, apc, 128) != 128) return false;
            if (!SelectTable(9)) return false;
            if (i2c.TWI_ReadPage(0xa0, 0x80, mod, 128) != 128) return false;
            /////////////////////
            for (i = 0; i < 72; i++)
            {
                Page03_write[i] = threshold[i];
            }           
            /////////////////////
            temp_value = GetTemp();
            if (temp_value < -60) return false;
            
            temp_index = (int)((temp_value + 40) / 5);

            if (temp_index <  0)  temp_index = 0;
            if (temp_index > 27)  temp_index = 27;

            // 计算平移量
            delta_apc = TestResult.txapcVal - apclut[temp_index];
            delta_mod = TestResult.txmodVal - modlut[temp_index];     
            if (modlut[temp_index] == 0) return false; // 消光比补偿表不能为0

            // 计算比例系数
            float fk = TestResult.txmodVal;
            fk /= modlut[temp_index];
            //

            for (i = 0; i < 32; i++)
            {
                apc_value = delta_apc + apclut[i];
                if (apc_value <   0) apc_value = 0;
                if (apc_value > 230) apc_value = 230;
                apc[4*i+ch] = Convert.ToByte(apc_value);
                
                if (GlobalVarFun.k_lut_flag == true)
                {
                    mod_value = (int)(fk * modlut[i]); // 比例缩放
                }
                else
                {
                    mod_value = delta_mod + modlut[i]; // 等量平移
                }
                if (mod_value <   0) mod_value = 0;
                if (mod_value > 255) mod_value = 255;
                mod[4*i+ch] = Convert.ToByte(mod_value);
            }
            if (!SelectTable(3)) return false;
            if (i2c.TWI_WritePage(0xa0, 0x80, Page03_write, 72) != 72) return false;
            if (!SelectTable(8)) return false;
            if (i2c.TWI_WritePage(0xa0, 0x80, apc, 128) != 128)        return false;
            if (!SelectTable(9)) return false;
            if (i2c.TWI_WritePage(0xa0, 0x80, mod, 128) != 128)        return false;

            if (!SelectTable(3)) return false;
            if (i2c.TWI_ReadPage(0xa0, 0x80, Page03_read, 72) != 72)   return false;
            if (!SelectTable(8)) return false;
            if (i2c.TWI_ReadPage(0xa0, 0x80, apc_read, 128) != 128)    return false;
            if (!SelectTable(9)) return false;
            if (i2c.TWI_ReadPage(0xa0, 0x80, mod_read, 128) != 128)    return false;
            //开启MOD温补
            TxTempLookupTableCtrl(true);
            //
            if ((Bit.ByteEquals(Page03_write, Page03_read) == false)
                || (Bit.ByteEquals(apc, apc_read) == false)
                || (Bit.ByteEquals(mod, mod_read) == false))
            {
                return false;
            }
            else
            {           
                // 发送保存命令
                byte[] saveByte = new byte[3];
                saveByte[0] = 0x08; // threshold  Page03   bit3       0x82地址
                //saveByte[1] = 0x0D; // 00001111  bit3 bit2 bit0       0x83地址
                saveByte[1] = 0x0F; // 00001111  bit3 bit2 bit1 bit0  0x83地址
                if (!SelectTable(6)) return false;
                if (i2c.TWI_WritePage(0xa0, 0x82, saveByte, 2) != 2) return false;
                Thread.Sleep(600);
                return true;
            }
        }

        // TxRxCDR控制  如果不需要直接返回
        //使能/禁用CDR（时钟数据恢复），通过写地址98控制。
        ////0x00=禁用, 0xFF=使能
        public bool DisTxRxCDR(bool disVal)
        {
            byte[] buf98 = new byte[2];
            //byte[] buf118 = new byte[2];
            //byte i = (byte)TestSet.ch;
            //read
            if (i2c.TWI_ReadPage(0xa0, 98, buf98, 1) != 1) return false;
            //if (i2c.TWI_ReadPage(0xa0, 98, buf118, 1) != 1) return false;

            if (disVal == true)
            {
               // buf110[0] = Bit.ClearBit(buf110[0], i+4);
                //buf110[0] = Bit.ClearBit(buf118[0], i);
                buf98[0] &= 0x00;
            }
            else
            {
               // buf110[0] = Bit.SetBit(buf110[0], i+4);
               // buf110[0] = Bit.SetBit(buf118[0], i);
                buf98[0] |= 0xFF;
            }

            //write
            if (i2c.TWI_WriteByte(0xa0, 98, buf98[0]) == false) return false;
            //if (i2c.TWI_WriteByte(0xa2, 118, buf118[0]) == false) return false;

            Thread.Sleep(200); //等待切换完成

            return true;
        }

        // 初始化模块   如果不需要直接返回
        //模块初始化（ER1无需特殊初始化，直接返回true）
        public bool InitModule()
        {
            return true;
        }

        // 模块发射温度补偿表控制   如果不需要直接返回
        //控制温度补偿LUT开关。写0x80地址：0x01=使能温补, 0x00=禁用
        public bool TxTempLookupTableCtrl(bool enable)
        {
            if (!SelectTable(6)) return false;
            if (enable)
            {
                if (!i2c.TWI_WriteByte(0xA0, 0x80, 0x01)) return false;  
            }
            else
            {          
                if (!i2c.TWI_WriteByte(0xA0, 0x80, 0x00)) return false;  
            }
            return true;
        }

        // 模块默认调试参数  TX-PE  RX-PE  TX-CPA等 如果不需要直接返回
        //写默认的TX-PE（预加重）值到0xED地址，并读回验证
        public bool WriteTxRxDefaultVal()
        {
            byte[] ReadBuffer = new byte[2];

            if (i2c.TWI_ReadByte(0xa0, 127) != 0x06)
                return false;
            byte i = (byte)TestSet.ch;
            i += 0xAC;
            // 写入
            if (i2c.TWI_WriteByte(0xa0, 0xED, TestResult.txpeVal) == false) return false;

            // 读出
            if (i2c.TWI_ReadPage(0xa0, 0xED, ReadBuffer, 1) != 1) return false;

            // 比较 check
            if (ReadBuffer[0] != TestResult.txpeVal)  return false; // error

            //
            return true;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        // 内部调用函数
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // 保存校准表到Flash
        private bool WritSaveCmd()
        {
            if (!SelectTable(6))
            {
                return false;
            }

            return i2c.TWI_WriteByte(0xA0, 0x83, 0x02); //save cal table
        }

        // 表选择
        protected bool SelectTable(byte tbl)
        {
            return i2c.TWI_WriteByte(0xA0, 127, tbl);
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}


