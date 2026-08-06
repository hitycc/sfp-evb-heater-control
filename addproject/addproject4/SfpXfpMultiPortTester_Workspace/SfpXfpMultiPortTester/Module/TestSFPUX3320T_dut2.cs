using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;

namespace FibertopTest_Common
{
    public class SFPUX3320T_Dut2 : ModuleTest
    {
        EVB i2c;
        private int _slot;
        public static byte Dut_num = 0;
        UInt16[] biaslut = new UInt16[84];
        UInt16[] modlut = new UInt16[84];
        UInt16[] apdlut = new UInt16[84];

        byte[] register = new byte[93];

        byte[] threshold = new byte[56];

        byte[] ex_cal = new byte[39]
                {
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                    0x3F,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,
                    0x01,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x01,0x00,0x00,0x00,
                    0x00,0x00,0x00
                };
        byte[] A2Lower60h = new byte[24]
                {
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                };

        byte[] byte_debug_pwd = new byte[4];
        byte[] byte_protect_pwd = new byte[4];
        byte[] byte_def_pwd = new byte[4];
        /*byte[] PW1 = new byte[4]
        {
            0xFF, 0xFF, 0xFF, 0xFF
        };
        byte[] PW2 = new byte[4];*/


        public void Init(EVB i2c, byte Dut)
        {
            this.i2c = i2c;
            Dut_num = Dut;
            _slot = Dut;
            // 0x00:线性计算法 apc-->uw & bias   0x11: 普通二分法 apc-->dBm   22:差值二分法 apc-->uW
            GlobalVarFun.txpwr_debug_method = 0x11;

            TestResult2.flash_data_len = 1024; // 256+256+128+128+128+128=1024  必须<=1024

            // 更新调试密码
            byte_debug_pwd[0] = 0xA9;
            byte_debug_pwd[1] = 0x54;
            byte_debug_pwd[2] = 0x50;
            byte_debug_pwd[3] = 0x46;

            byte_protect_pwd[0] = 0x11;
            byte_protect_pwd[0] = 0x11;
            byte_protect_pwd[0] = 0x11;
            byte_protect_pwd[0] = 0x11;

            byte_def_pwd[0] = 0xFF;
            byte_def_pwd[0] = 0xFF;
            byte_def_pwd[0] = 0xFF;
            byte_def_pwd[0] = 0xFF;

            /*PW2[0] = byte_debug_pwd[0];
            PW2[1] = byte_debug_pwd[1];
            PW2[2] = byte_debug_pwd[2];
            PW2[3] = byte_debug_pwd[3];
            */
        }

        public bool CheckTestTypeInfo()
        {
            //UX3320T
            byte[] ux3320cID = new byte[7]
            {
               0x55, 0x58, 0x33, 0x33, 0x32, 0x30, 0x30
            };
            byte[] readChipID = new byte[7];
            byte[] temp_val = new byte[2];

            //SelectTable(3);
            temp_val[0] = 0x03;
            if (TWI_WritePage(0xA2, 127, temp_val, 1) != 1) //写入table3
            {
                return false;
            }

            TWI_ReadPage(0xA2, 0xF5, readChipID, 7);//读取ChipID
            if (Bit.ByteEquals(readChipID, ux3320cID) == false)
            {
                return false;
            }

            //终测检查E2PROM状态指示 F0h_Table03
            if (GlobalVarFun.testType == "finalTest")
            {
                if (TWI_ReadPage(0xA2, 0xEF, temp_val, 2) != 2) return false;
                if ((temp_val[1] & 0xFC) != 0x00) return false; //bit7-2 must=0
            }

            return true;
        }

        public bool SoftTxDis(bool txDis)
        {
            byte wrtBuf = TWI_ReadByte(0xA2, 110);

            if (txDis == true)
            {
                wrtBuf |= 0x40; //bit6=1  tx_dis
            }
            else
            {
                wrtBuf &= 0xBF; //bit6=0  tx_en
            }

            Thread.Sleep(10); //延时10ms
            TWI_WriteByte(0xA2, 110, wrtBuf);
            Thread.Sleep(10); //延时10ms

            if (TWI_WriteByte(0xA2, 110, wrtBuf) == false) return false;

            Thread.Sleep(200); //延时200ms

            return true;
        }

        public bool SetDebugPWD()
        {
            byte[] readbuffer = new byte[4];
            byte[] pwd_default = new byte[4];

            pwd_default[0] = 0xFF;
            pwd_default[1] = 0xFF;
            pwd_default[2] = 0xFF;
            pwd_default[3] = 0xFF;

            TWI_WritePage(0xA2, 0x7B, byte_debug_pwd, 4); //write pwd for test
            //Thread.Sleep(100);

            if (TWI_WritePage(0xA2, 0x7B, byte_debug_pwd, 4) != 4)
            {
                return false;
            }
            Thread.Sleep(100);
            //
            SelectTable(3);
            //
            TWI_ReadPage(0xA2, 0x80, readbuffer, 2);
            readbuffer[2] = (byte)(TWI_ReadByte(0xA2, 0xEF) & 0x03);
            if ((readbuffer[0] == 0xAA) && (readbuffer[1] == 0x00) && (readbuffer[2] == 0x02))
            {
                return true;
            }

            //初测尝试写入初始化密码FFFFFFFF
            if (GlobalVarFun.testType == "firstTest")
            {
                if (TWI_WritePage(0xA2, 0x7B, pwd_default, 4) != 4)
                {
                    return false;
                }
                Thread.Sleep(100);
                //
                SelectTable(3);
                //
                TWI_ReadPage(0xA2, 0x80, readbuffer, 2);
                readbuffer[2] = (byte)(TWI_ReadByte(0xA2, 0xEF) & 0x03);
                if ((readbuffer[0] == 0xAA) && (readbuffer[1] == 0x00) && (readbuffer[2] == 0x02))
                {
                    return true;
                }
            }

            return false; //进入调试模式失败
        }

        public byte CheckDebugPWD()
        {
            byte[] readbuffer = new byte[4];

            // 读密码
            if (TWI_ReadPage(0xA2, 0x7B, readbuffer, 4) != 4)
            {
                return 0x01;
            }

            if (Bit.ByteEquals(readbuffer, byte_debug_pwd) == false)
            {
                return 0x02;
            }

            return 0x00;
        }

        public bool CheckRxLOS()
        {
            byte status = TWI_ReadByte(0xa2, 110);
            return Bit.GetBit(status, 1);
        }

        public float GetTemp()
        {
            float temp = 0;
            byte[] readbuffer = new byte[2];
            if (TWI_ReadPage(0xa2, 96, readbuffer, 2) != 2)
            {
                return -100;
            }
            //
            sbyte i = (sbyte)readbuffer[0];
            int j = Convert.ToInt32(i);
            temp = (j + readbuffer[1] * (1 / 256.0f));
            return temp;
        }

        public float GetVCC()
        {
            byte[] readbuffer = new byte[2];
            float vccDDM;

            if (TWI_ReadPage(0xa2, 98, readbuffer, 2) == 2)
            {
                vccDDM = ((readbuffer[0] * 256 + readbuffer[1]) / 10000.0f);
            }
            else
            {
                return 0; // Error
            }

            // 未初始化模块使用
            if ((GlobalVarFun.testType == "firstTest") && (vccDDM < 1.0f))
            {
                vccDDM = 2.3f; // 2.3V 初测初始化
            }

            return vccDDM;
        }

        public float GetTxBias()
        {
            float txbias = 0;
            byte[] readbuffer = new byte[2];
            if (TWI_ReadPage(0xa2, 100, readbuffer, 2) != 2)
            {
                Thread.Sleep(50);
                // 重试一次
                if (TWI_ReadPage(0xa2, 100, readbuffer, 2) != 2)
                {
                    return -1; // Error
                }
            }
            //
            txbias = ((readbuffer[0] * 256 + readbuffer[1]) / 500.0f);
            return txbias;
        }

        public float GetTxPower()
        {
            float txpow = 0;
            byte[] readbuffer = new byte[2];
            if (TWI_ReadPage(0xa2, 102, readbuffer, 2) != 2)
            {
                return -100; // Error
            }
            //
            if (readbuffer[0] == 0 && readbuffer[1] == 0)
            {
                readbuffer[1] = 1;
            }
            txpow = (float)(10 * Math.Log10((readbuffer[0] * 256 + readbuffer[1]) / 10000.0));
            return txpow;
        }

        public float GetRxPower()
        {
            float rxpow = 0;
            byte[] readbuffer = new byte[2];
            if (TWI_ReadPage(0xa2, 104, readbuffer, 2) != 2)
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

        public bool GetDDMAnalogValues()
        {
            byte[] readbuffer = new byte[10];

            if (TWI_ReadPage(0xa2, 96, readbuffer, 10) == 10)
            {
                if (readbuffer[6] == 0 && readbuffer[7] == 0)
                {
                    readbuffer[7] = 1;
                }

                if (readbuffer[8] == 0 && readbuffer[9] == 0)
                {
                    readbuffer[9] = 1;
                }

                sbyte i = (sbyte)readbuffer[0];
                int j = Convert.ToInt32(i);

                TestResult2.tempDDM = (j + readbuffer[1] * (1 / 256.0f));
                TestResult2.vccDDM = ((readbuffer[2] * 256 + readbuffer[3]) / 10000.0f);
                // 未初始化模块使用
                if ((GlobalVarFun.testType == "firstTest") && (TestResult2.vccDDM < 1.0f))
                {
                    TestResult2.vccDDM = 2.3f; // 2.3V 初测初始化
                }
                TestResult2.txBiasDDM = ((readbuffer[4] * 256 + readbuffer[5]) / 500.0f);
                //
                if (readbuffer[6] == 0x00 && readbuffer[7] == 0x00)
                    readbuffer[7] = 0x01;
                if (readbuffer[8] == 0x00 && readbuffer[9] == 0x00)
                    readbuffer[9] = 0x01;
                //
                TestResult2.txPowerDDM = (float)(10 * Math.Log10((readbuffer[6] * 256 + readbuffer[7]) / 10000.0));
                TestResult2.rxPowerDDM = (float)(10 * Math.Log10((readbuffer[8] * 256 + readbuffer[9]) / 10000.0));
            }
            else
            {
                TestResult2.tempDDM = 0;
                TestResult2.vccDDM = 0;
                TestResult2.txBiasDDM = 0;
                TestResult2.txPowerDDM = -100;
                TestResult2.rxPowerDDM = -100;
                return false;
            }

            return true;
        }

        public bool GetDDMThresholds()
        {
            byte[] readbuffer = new byte[40];
            int i = 0;

            if (TWI_ReadPage(0xa2, 0, readbuffer, 40) != 40)
            {
                return false;
            }

            //告警阈值
            i = Convert.ToInt32((sbyte)readbuffer[0]);
            TestResult2.tempHA = (float)(i + readbuffer[1] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[2]);
            TestResult2.tempLA = (float)(i + readbuffer[3] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[4]);
            TestResult2.tempHW = (float)(i + readbuffer[5] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[6]);
            TestResult2.tempLW = (float)(i + readbuffer[7] * 1.0 / 256.0);

            TestResult2.vccHA = (float)((readbuffer[8] * 256 + readbuffer[9]) / 10000.0);
            TestResult2.vccLA = (float)((readbuffer[10] * 256 + readbuffer[11]) / 10000.0);
            TestResult2.vccHW = (float)((readbuffer[12] * 256 + readbuffer[13]) / 10000.0);
            TestResult2.vccLW = (float)((readbuffer[14] * 256 + readbuffer[15]) / 10000.0);

            TestResult2.txBiasHA = (float)((readbuffer[16] * 256 + readbuffer[17]) / 500.0);
            TestResult2.txBiasLA = (float)((readbuffer[18] * 256 + readbuffer[19]) / 500.0);
            TestResult2.txBiasHW = (float)((readbuffer[20] * 256 + readbuffer[21]) / 500.0);
            TestResult2.txBiasLW = (float)((readbuffer[22] * 256 + readbuffer[23]) / 500.0);

            TestResult2.txPowerHA = (float)(10 * Math.Log10((readbuffer[24] * 256 + readbuffer[25]) / 10000.0));
            TestResult2.txPowerLA = (float)(10 * Math.Log10((readbuffer[26] * 256 + readbuffer[27]) / 10000.0));
            TestResult2.txPowerHW = (float)(10 * Math.Log10((readbuffer[28] * 256 + readbuffer[29]) / 10000.0));
            TestResult2.txPowerLW = (float)(10 * Math.Log10((readbuffer[30] * 256 + readbuffer[31]) / 10000.0));

            TestResult2.rxPowerHA = (float)(10 * Math.Log10((readbuffer[32] * 256 + readbuffer[33]) / 10000.0));
            TestResult2.rxPowerLA = (float)(10 * Math.Log10((readbuffer[34] * 256 + readbuffer[35]) / 10000.0));
            TestResult2.rxPowerHW = (float)(10 * Math.Log10((readbuffer[36] * 256 + readbuffer[37]) / 10000.0));
            TestResult2.rxPowerLW = (float)(10 * Math.Log10((readbuffer[38] * 256 + readbuffer[39]) / 10000.0));
            //
            return true;
        }

        public bool GetDDMFlagsInterrupt()
        {
            byte[] readbuffer = new byte[6];
            if (TWI_ReadPage(0xa2, 112, readbuffer, 6) != 6)
            {
                return false;
            }

            TestResult2.tempHA_flag = Bit.GetBit(readbuffer[0], 7);
            TestResult2.tempLA_flag = Bit.GetBit(readbuffer[0], 6);
            TestResult2.vccHA_flag = Bit.GetBit(readbuffer[0], 5);
            TestResult2.vccLA_flag = Bit.GetBit(readbuffer[0], 4);
            TestResult2.txBiasHA_flag = Bit.GetBit(readbuffer[0], 3);
            TestResult2.txBiasLA_flag = Bit.GetBit(readbuffer[0], 2);
            TestResult2.txPwrHA_flag = Bit.GetBit(readbuffer[0], 1);
            TestResult2.txPwrLA_flag = Bit.GetBit(readbuffer[0], 0);
            TestResult2.rxPwrHA_flag = Bit.GetBit(readbuffer[1], 7);
            TestResult2.rxPwrLA_flag = Bit.GetBit(readbuffer[1], 6);

            TestResult2.tempHW_flag = Bit.GetBit(readbuffer[4], 7);
            TestResult2.tempLW_flag = Bit.GetBit(readbuffer[4], 6);
            TestResult2.vccHW_flag = Bit.GetBit(readbuffer[4], 5);
            TestResult2.vccLW_flag = Bit.GetBit(readbuffer[4], 4);
            TestResult2.txBiasHW_flag = Bit.GetBit(readbuffer[4], 3);
            TestResult2.txBiasLW_flag = Bit.GetBit(readbuffer[4], 2);
            TestResult2.txPwrHW_flag = Bit.GetBit(readbuffer[4], 1);
            TestResult2.txPwrLW_flag = Bit.GetBit(readbuffer[4], 0);
            TestResult2.rxPwrHW_flag = Bit.GetBit(readbuffer[5], 7);
            TestResult2.rxPwrLW_flag = Bit.GetBit(readbuffer[5], 6);

            return true;
        }


        public bool GetFlashInfo()
        {
            byte[] readbuffer = new byte[256];
            int i;

            if (TWI_ReadPage(0xa0, 0, readbuffer, 256) != 256)
            {
                return false;
            }
            for (i = 0; i < 256; i++)
            {
                TestResult2.flash_data[i] = readbuffer[i];
            }

            if (SelectTable(0) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 0, readbuffer, 256) != 256)
            {
                return false;
            }
            for (i = 0; i < 256; i++)
            {
                TestResult2.flash_data[i + 256] = readbuffer[i];
            }

            TestResult2.sn = System.Text.Encoding.ASCII.GetString(TestResult2.flash_data, 68, 16);
            TestResult2.pn = System.Text.Encoding.ASCII.GetString(TestResult2.flash_data, 40, 16);
            TestResult2.vn = System.Text.Encoding.ASCII.GetString(TestResult2.flash_data, 20, 16);
            TestResult2.date = System.Text.Encoding.ASCII.GetString(TestResult2.flash_data, 84, 8);
            //TestResult2.sn.TrimEnd();
            //TestResult2.pn.TrimEnd();
            //TestResult2.vn.TrimEnd();
            //TestResult2.date.TrimEnd();

            return true;
        }

        public bool GetFlashInfoDebug()
        {
            byte[] readbuffer = new byte[256];
            int i;

            if (SelectTable(3) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult2.flash_data[i + 512] = readbuffer[i];
            }

            if (SelectTable(4) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult2.flash_data[i + 640] = readbuffer[i];
            }

            if (SelectTable(5) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult2.flash_data[i + 768] = readbuffer[i];
            }

            if (SelectTable(6) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult2.flash_data[i + 896] = readbuffer[i];
            }

            // 更新 Fsn  飞思卓产品内部流水号  0xF0-F4 table04 0xF0-0x80=112
            UInt64 iFsn = 0;
            iFsn += TestResult2.flash_data[640 + 112 + 0]; // 0x00 = 1
            iFsn <<= 8;
            iFsn += TestResult2.flash_data[640 + 112 + 1]; // 0x01 = 2
            iFsn <<= 8;
            iFsn += TestResult2.flash_data[640 + 112 + 2]; // 0x02 = 3
            iFsn <<= 8;
            iFsn += TestResult2.flash_data[640 + 112 + 3]; // 0x03 = 4
            iFsn <<= 8;
            iFsn += TestResult2.flash_data[640 + 112 + 4]; // 0x04 = 5

            if (iFsn > TestResult2.max_Fsn)
            {
                iFsn = TestResult2.max_Fsn;
            }
            TestResult2.fibertop_sn = iFsn.ToString("D12");
            //TestResult2.fibertop_sn = System.Text.Encoding.ASCII.GetString(TestResult2.flash_data, 640 + 0, 10); //0xF0-FA  table04
            //TestResult2.fibertop_sn.TrimEnd();
            //TestResult2.fibertop_sn = TestResult2.sn; //test

            if (SelectTable(0) == false) return false; // 表选择
            if (TWI_WritePage(0xa2, 0x7b, byte_protect_pwd, 4) != 4) //写无效密码保护A2
            {
                return false;
            }

            // 再次读取A2: 0-95
            if (TWI_ReadPage(0xa2, 0, readbuffer, 96) != 96)
            {
                return false;
            }
            for (i = 0; i < 96; i++)
            {
                TestResult2.flash_data[i + 256] = readbuffer[i];
            }

            return true;
        }

        public bool CheckThresholdsInfo(ref string errMsg)
        {
            byte[] threshold_read = new byte[56];
            byte[] ex_cal_read = new byte[39];
            byte check_sum = 0;
            int i = 0;

            errMsg = "";

            for (i = 0; i < 56; i++)
            {
                threshold_read[i] = TestResult2.flash_data[i + 256];
            }

            for (i = 0; i < 39; i++)
            {
                ex_cal_read[i] = TestResult2.flash_data[i + 256 + 56];
            }

            // check 告警门限
            if (!Bit.ByteEquals(threshold_read, threshold))
            {
                errMsg += "A2h告警门限 ";
            }

            // check  外校准系数
            if (!Bit.ByteEquals(ex_cal_read, ex_cal))
            {
                errMsg += "A2h校准参数 ";
            }

            check_sum = 0x00;
            for (i = 0; i < 95; i++)
            {
                check_sum += TestResult2.flash_data[i + 256];
            }

            // check sum errro
            if (check_sum != TestResult2.flash_data[95 + 256])
            {
                errMsg += "A2h的0-94字节的 check sum 错误";
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

        public bool CheckModuleFlashInfo(ref string errMsg)
        {
            UInt16[] modLUTData = new UInt16[84];
            UInt16[] biasLUTData = new UInt16[84];
            UInt16[] apdLUTData = new UInt16[84];

            byte[] r_biasLUT = new byte[105];
            byte[] r_modLUT = new byte[105];
            byte[] r_apdLUT = new byte[105];

            byte[] regcfgT3 = new byte[93];
            byte[] r_regcfgT3 = new byte[93];

            int i = 0;

            errMsg = "";

            //CheckThresholdsInfo(ref errMsg); // 检查告警门限、内外校准参数

            for (i = 0; i < 93; i++)
            {
                regcfgT3[i] = register[i];
                r_regcfgT3[i] = TestResult2.flash_data[i + 512];
            }
            //regcfgT3[0xA2 - 0x80] = register[0xA2 - 0x80];
            //r_regcfgT3[0xA2 - 0x80] = TestResult2.flash_data[0xA2 - 0x80 + 512];
            for (i = 0; i < 105; i++)
            {
                r_biasLUT[i] = TestResult2.flash_data[i + 640];
                r_modLUT[i] = TestResult2.flash_data[i + 768];
                r_apdLUT[i] = TestResult2.flash_data[i + 896];
            }

            i = 0;
            for (int k = 0; k < 84; k = k + 4)
            {
                biasLUTData[k + 0] = (UInt16)(r_biasLUT[i + 0] * 4 + r_biasLUT[i + 1] / 64);
                biasLUTData[k + 1] = (UInt16)((r_biasLUT[i + 1] & 0x3F) * 16 + r_biasLUT[i + 2] / 16);
                biasLUTData[k + 2] = (UInt16)((r_biasLUT[i + 2] & 0x0F) * 64 + r_biasLUT[i + 3] / 4);
                biasLUTData[k + 3] = (UInt16)((r_biasLUT[i + 3] & 0x03) * 256 + r_biasLUT[i + 4]);

                modLUTData[k + 0] = (UInt16)(r_modLUT[i + 0] * 4 + r_modLUT[i + 1] / 64);
                modLUTData[k + 1] = (UInt16)((r_modLUT[i + 1] & 0x3F) * 16 + r_modLUT[i + 2] / 16);
                modLUTData[k + 2] = (UInt16)((r_modLUT[i + 2] & 0x0F) * 64 + r_modLUT[i + 3] / 4);
                modLUTData[k + 3] = (UInt16)((r_modLUT[i + 3] & 0x03) * 256 + r_modLUT[i + 4]);

                apdLUTData[k + 0] = (UInt16)(r_apdLUT[i + 0] * 4 + r_apdLUT[i + 1] / 64);
                apdLUTData[k + 1] = (UInt16)((r_apdLUT[i + 1] & 0x3F) * 16 + r_apdLUT[i + 2] / 16);
                apdLUTData[k + 2] = (UInt16)((r_apdLUT[i + 2] & 0x0F) * 64 + r_apdLUT[i + 3] / 4);
                apdLUTData[k + 3] = (UInt16)((r_apdLUT[i + 3] & 0x03) * 256 + r_apdLUT[i + 4]);

                i = i + 5;
            }
            //

            //EEPROM Checksum 检查
            byte checksum = 0;
            for (i = 0; i < 105; i++)
            {
                checksum += r_biasLUT[i];
                checksum += r_modLUT[i];
                checksum += r_apdLUT[i];
            }
            for (i = 0x81; i <= 0xA0; i++)
            {
                checksum += r_regcfgT3[i - 0x80];
            }
            for (i = 0xA2; i <= 0xD2; i++)
            {
                checksum += r_regcfgT3[i - 0x80];
            }
            if (r_regcfgT3[0xDB - 0x80] != checksum)
            {
                errMsg += "UX3320T_E2PROMchksum ";
            }
            //

            regcfgT3[0x8C - 0x80] = r_regcfgT3[0x8C - 0x80]; //APC
            //regcfgT3[0x8D - 0x80] = r_regcfgT3[0x8D - 0x80];
            regcfgT3[0x8D - 0x80] &= 0xE0; //bit7-5
            r_regcfgT3[0x8D - 0x80] &= 0xE0;

            regcfgT3[0x87 - 0x80] = r_regcfgT3[0x87 - 0x80]; //MOD
            regcfgT3[0x88 - 0x80] = r_regcfgT3[0x88 - 0x80];

            regcfgT3[0x99 - 0x80] = r_regcfgT3[0x99 - 0x80]; //APD
            regcfgT3[0x9A - 0x80] = r_regcfgT3[0x9A - 0x80];

            regcfgT3[0x9D - 0x80] = r_regcfgT3[0x9D - 0x80]; //LOS

            regcfgT3[0xDB - 0x80] = r_regcfgT3[0xDB - 0x80]; //EEPROM CHECK SUM

            //regcfgT3[0x97 - 0x80] = r_regcfgT3[0x97 - 0x80]; //

            r_regcfgT3[0xD3 - 0x80] = regcfgT3[0xD3 - 0x80]; //PW1
            r_regcfgT3[0xD4 - 0x80] = regcfgT3[0xD4 - 0x80];
            r_regcfgT3[0xD5 - 0x80] = regcfgT3[0xD5 - 0x80];
            r_regcfgT3[0xD6 - 0x80] = regcfgT3[0xD6 - 0x80];
            r_regcfgT3[0xD7 - 0x80] = regcfgT3[0xD7 - 0x80]; //PW2
            r_regcfgT3[0xD8 - 0x80] = regcfgT3[0xD8 - 0x80];
            r_regcfgT3[0xD9 - 0x80] = regcfgT3[0xD9 - 0x80];
            r_regcfgT3[0xDA - 0x80] = regcfgT3[0xDA - 0x80];

            for (i = 0xBC; i <= 0xCF; i++)//TX RX POWER 较准参数
            {
                regcfgT3[i - 0x80] = r_regcfgT3[i - 0x80];
            }

            // check UX3320T 配置表  93个字节
            if (Bit.ByteEquals(regcfgT3, r_regcfgT3) == false)
            {
                string str = "";
                for (i = 0; i < 93; i++)
                {
                    if (regcfgT3[i] != r_regcfgT3[i])
                    {
                        str += i.ToString() + " ";
                    }
                }
                errMsg += "UX3320T配置表 " + str;
            }

            /////////////////////////////////////////////////////////////////////////
            // Check BIAS/MOD/APD 补偿表
            //
            UInt16[] lut_fk = new UInt16[84];
            int[] q = new int[84];
            float fk = 0;

            // check  MOD 补偿表
            fk = modLUTData[33]; // 26度 补偿点
            fk /= modlut[33];
            for (i = 0; i < 84; i++)
            {
                q[i] = modLUTData[i] - modlut[i];
            }
            if (q.Max() != q.Min()) // 平移检查错误  进入比例缩放检查
            {
                for (i = 0; i < 84; i++)
                {
                    lut_fk[i] = (UInt16)(fk * modlut[i]);
                    q[i] = modLUTData[i] - lut_fk[i];
                }
                if (Math.Abs(q.Max() - q.Min()) > 2)
                {
                    errMsg += "MOD补偿表 "; // 两种方式检查都有问题  报错
                }
            }

            // check  BIAS 补偿表
            for (i = 0; i < 84; i++)
            {
                q[i] = biasLUTData[i] - biaslut[i];
            }
            if (q.Max() != q.Min()) // 平移检查错误  进入比例缩放检查
            {
                if (biaslut[33] == 0)
                {
                    errMsg += "BIAS数据库 "; // APD数据库补偿表有错误  报错
                    fk = 1;
                }
                else
                {
                    fk = biasLUTData[33]; // 26度 补偿点
                    fk /= biaslut[33];
                }

                for (i = 0; i < 84; i++)
                {
                    lut_fk[i] = (UInt16)(fk * biaslut[i]);
                    q[i] = biasLUTData[i] - lut_fk[i];
                }
                if (Math.Abs(q.Max() - q.Min()) > 2)
                {
                    errMsg += "BIAS补偿表 "; // 两种方式检查都有问题  报错
                }
            }

            // check  APD 补偿表
            for (i = 0; i < 84; i++)
            {
                q[i] = apdLUTData[i] - apdlut[i];
            }
            if ((string.IsNullOrEmpty(TestSet.apdName) == false)) //判断是否需要检查APD补偿表
            {
                if (q.Max() != q.Min()) // 平移检查错误  进入比例缩放检查
                {
                    if (apdlut[33] == 0)
                    {
                        errMsg += "APD数据库 "; // APD数据库补偿表有错误  报错
                        fk = 1;
                    }
                    else
                    {
                        fk = apdLUTData[33]; // 26度 补偿点
                        fk /= apdlut[33];
                    }

                    for (i = 0; i < 84; i++)
                    {
                        lut_fk[i] = (UInt16)(fk * apdlut[i]);
                        q[i] = apdLUTData[i] - lut_fk[i];
                    }
                    if (Math.Abs(q.Max() - q.Min()) > 2)
                    {
                        errMsg += "APD补偿表 "; // 两种方式检查都有问题  报错
                    }
                }
            }
            /////////////////////////////////////////////////////////////////////////

            // errMsg错误信息判断
            if (string.IsNullOrEmpty(errMsg))
            {
                return true; // OK
            }
            else
            {
                return false; // Error
            }
        }

        // 通过Access数据库读取模块型号列表
        public bool GetModuleTypeFromAccessdb(ref string[] str, ref int len)
        {
            OleDbConnection dbconnect;
            OleDbCommand dbcommand;
            OleDbDataAdapter dbadapter;
            DataSet dbset;

            len = 0;

            try
            {
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

        public bool GetTypeDebugInfoFromAccessdb()
        {
            OleDbConnection dbconnect;
            OleDbCommand dbcommand;
            OleDbDataAdapter dbadapter;
            DataSet dbset;
            string dbconnectionstr = "";
            int i = 0;

            string AW_Threshold_Field = "AW";
            string Apc_LUT_Field = "BIASval";
            string Mod_LUT_Field = "MODval";
            string Apd_LUT_Field = "APDval";
            string str = "";
            //
            try
            {
                dbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + GlobalVarFun.moduleLutDBFilePath);
                dbconnectionstr = string.Format("select TxPowerSetPoint,TxERSetPoint,TxER_MIN,TxER_MAX,Bias_MAX,Bias_MIN,TxPower_MIN,TxPower_MAX,TxCr_MIN,TxCr_MAX,TxJt_MAX,Sensitivity,RxALos,RxDLos,RxOverLoad,"
                                             + @"RxPwrCheck_1,RxPwrCheck_2,RxPwrCheck_3,RxPwrCheck_4,RxPwrCheck_5,APD_Name,APCmin,APCmax,MODmin,MODmax,LOSmin,LOSmax,MaskName,MaskMargin,
                                                APCmin_def,APCmax_def,MODmin_def,MODmax_def,LOSmin_def,LOSmax_def,APDmin_def,APDmax_def,VONmin_def,VONmax_def,Crossingmin_def,Crossingmax_def,TosaTempmin_def,
                                              Tosatempmax_def,ER_Prec,TxPwr_Prec,RxPwr_Prec,Wlgth_Prec,Wlgth_err,delay_DOA,delay_OPM,Test_Rx,Test_RxNopwr,Test_Tx,Test_TxNopwr,Test_TxDis_HW,Test_RxLos_HW,Test_Sen,
                                              Test_25GALG,Test_COB_LD,Test_EML,Test_APD,Test_CopperSFP,Test_CDRDis,Test_SchemeDis,Test_Init,delay_PSSBERT,Rosa_PIN,Test_EyeSave
                                                from ModuleType where Type = '{0}'", TestResult2.fibertop_pn);

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
                TestSet.txCr_Min = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxCr_MIN"].ToString());
                TestSet.txCr_Max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxCr_MAX"].ToString());
                TestSet.txJt_Max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["TxJt_MAX"].ToString());

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

                TestResult2.mask_name = dbset.Tables["ModuleType"].Rows[0]["MaskName"].ToString().Trim();
                str = dbset.Tables["ModuleType"].Rows[0]["MaskMargin"].ToString().Trim();
                if (!string.IsNullOrEmpty(str))
                {
                    TestResult2.mask_margin = Convert.ToUInt16(str);
                }

                /////////////////////////////////////////////////////////////////////////////////
                TestSet.txapc_Min = 60;
                TestSet.txapc_Max = 190;
                TestSet.txmod_Min = 130;
                TestSet.txmod_Max = 320;
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
                //UI def
                str = dbset.Tables["ModuleType"].Rows[0]["APCmin_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txapc_Min_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["APCmax_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txapc_Max_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["MODmin_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txmod_Min_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["MODmax_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txmod_Max_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["LOSmin_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.rxlos_Min_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["LOSmax_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.rxlos_Max_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["APDmin_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.rxapd_min_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["APDmax_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.rxapd_max_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["VONmin_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.von_min_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["VONmax_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.von_max_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["Crossingmin_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txcpa_Min_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["Crossingmax_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txcpa_Max_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["TosaTempmin_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.tosatemp_min_def = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["TosaTempmax_def"].ToString().Trim();
                if (str != "")
                {
                    TestSet.tosatemp_max_def = Convert.ToUInt16(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["ER_Prec"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txer_prec = (float)Convert.ToDouble(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["TxPwr_Prec"].ToString().Trim();
                if (str != "")
                {
                    TestSet.txPwr_prec = (float)Convert.ToDouble(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["RxPwr_Prec"].ToString().Trim();
                if (str != "")
                {
                    TestSet.rxPwr_prec = (float)Convert.ToDouble(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["Wlgth_Prec"].ToString().Trim();
                if (str != "")
                {
                    TestSet.wlgth_prec = (float)Convert.ToDouble(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["Wlgth_err"].ToString().Trim();
                if (str != "")
                {
                    TestSet.wlgth_err = (float)Convert.ToDouble(str);
                }
                //
                str = dbset.Tables["ModuleType"].Rows[0]["delay_DOA"].ToString().Trim();
                if (str != "")
                {
                    TestSet.delay_doa = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["delay_OPM"].ToString().Trim();
                if (str != "")
                {
                    TestSet.delay_opm = Convert.ToUInt16(str);
                }
                str = dbset.Tables["ModuleType"].Rows[0]["delay_PSSBERT"].ToString().Trim();
                if (str != "")
                {
                    TestSet.delay_pssbert = Convert.ToUInt16(str);
                }
                //
                TestSet.test_rx = dbset.Tables["ModuleType"].Rows[0]["Test_Rx"].ToString();
                TestSet.test_rxnopwr = dbset.Tables["ModuleType"].Rows[0]["Test_RxNopwr"].ToString();
                TestSet.test_tx = dbset.Tables["ModuleType"].Rows[0]["Test_Tx"].ToString();
                TestSet.test_txnopwr = dbset.Tables["ModuleType"].Rows[0]["Test_TxNopwr"].ToString();
                TestSet.test_txdishw = dbset.Tables["ModuleType"].Rows[0]["Test_TxDis_HW"].ToString();
                TestSet.test_rxloshw = dbset.Tables["ModuleType"].Rows[0]["Test_RxLos_HW"].ToString();

                TestSet.test_sen = dbset.Tables["ModuleType"].Rows[0]["Test_Sen"].ToString();
                TestSet.test_25galg = dbset.Tables["ModuleType"].Rows[0]["Test_25GALG"].ToString();
                TestSet.test_cobld = dbset.Tables["ModuleType"].Rows[0]["Test_COB_LD"].ToString();
                TestSet.test_eml = dbset.Tables["ModuleType"].Rows[0]["Test_EML"].ToString();
                TestSet.test_apd = dbset.Tables["ModuleType"].Rows[0]["Test_APD"].ToString();
                TestSet.test_coppersfp = dbset.Tables["ModuleType"].Rows[0]["Test_CopperSFP"].ToString();
                TestSet.test_cdrdis = dbset.Tables["ModuleType"].Rows[0]["Test_CDRDis"].ToString();
                TestSet.test_schemedis = dbset.Tables["ModuleType"].Rows[0]["Test_SchemeDis"].ToString();
                TestSet.test_init = dbset.Tables["ModuleType"].Rows[0]["Test_Init"].ToString();
                TestSet.test_rosa_pin = dbset.Tables["ModuleType"].Rows[0]["Rosa_PIN"].ToString();
                TestSet.test_rosa_pin = dbset.Tables["ModuleType"].Rows[0]["Test_EyeSave"].ToString();
                //
                if (string.IsNullOrEmpty(TestSet.apdName)) // No APD
                {
                    dbconnectionstr = string.Format("select register,{1},{2},{3} from [{0}]", TestResult2.fibertop_pn, AW_Threshold_Field, Apc_LUT_Field, Mod_LUT_Field);
                }
                else
                {
                    dbconnectionstr = string.Format("select register,{1},{2},{3},{4} from [{0}]", TestResult2.fibertop_pn, AW_Threshold_Field, Apc_LUT_Field, Mod_LUT_Field, Apd_LUT_Field);
                }

                dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, TestResult2.fibertop_pn);
                //
                for (i = 0; i < 40; i++)
                {
                    threshold[i] = Convert.ToByte(dbset.Tables[TestResult2.fibertop_pn].Rows[i][AW_Threshold_Field]);
                }

                for (i = 0; i < 93; i++)
                {
                    register[i] = Convert.ToByte(dbset.Tables[TestResult2.fibertop_pn].Rows[i]["register"]);
                }

                for (i = 0; i < 84; i++)
                {
                    biaslut[i] = Convert.ToByte(dbset.Tables[TestResult2.fibertop_pn].Rows[i][Apc_LUT_Field]);
                }

                for (i = 0; i < 84; i++)
                {
                    modlut[i] = Convert.ToUInt16(dbset.Tables[TestResult2.fibertop_pn].Rows[i][Mod_LUT_Field]);
                }

                if (string.IsNullOrEmpty(TestSet.apdName) == false) // APD
                {
                    for (i = 0; i < 84; i++)
                    {
                        apdlut[i] = Convert.ToByte(dbset.Tables[TestResult2.fibertop_pn].Rows[i][Apd_LUT_Field]);
                    }
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

        // 初测调试功能函数
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        public bool SetTxApcBias(UInt16 setVal)
        {
            byte[] w_val = new byte[2];
            byte[] r_val = new byte[2];

            if (setVal > 255)
            {
                setVal = 255;
            }
            TestResult2.txapcVal = setVal;

            // 选择表 03
            if (SelectTable(3) == false) return false;
            Thread.Sleep(5); //延时

            w_val[1] = register[0x8D - 0x80]; //TWI_ReadByte(0xA2, 0x8D);
            w_val[0] = (byte)setVal;

            if (TWI_WritePage(0xA2, 0x8C, w_val, 2) != 2) return false;
            Thread.Sleep(400); //延时
            if (TWI_ReadPage(0xA2, 0x8C, r_val, 2) != 2) return false;
            if ((r_val[0] != w_val[0]) || (r_val[1] != w_val[1]))
            {
                return false;
            }

            return true;
        }

        public bool SetTxModBias(UInt16 setVal)
        {
            byte[] w_val = new byte[2];
            byte[] r_val = new byte[2];

            if (setVal > 1023)
            {
                setVal = 1023;
            }
            TestResult2.txmodVal = setVal;

            // 选择表 03
            if (SelectTable(3) == false) return false;
            Thread.Sleep(5); //延时

            w_val[1] = register[0x88 - 0x80]; //TWI_ReadByte(0xA2, 0x88);
            w_val[1] &= 0xFC;

            w_val[0] = (byte)((setVal >> 2) & 0xFF);
            w_val[1] |= (byte)(setVal & 0x03);

            if (TWI_WritePage(0xA2, 0x87, w_val, 2) != 2) return false;
            Thread.Sleep(100); //延时
            if (TWI_ReadPage(0xA2, 0x87, r_val, 2) != 2) return false;
            if (r_val[0] != w_val[0])
            {
                return false;
            }

            return true;
        }

        public bool SetRxLos(UInt16 setVal)
        {
            if (setVal > 255) //最大255
            {
                setVal = 255;
            }
            TestResult2.rxlosVal = setVal;

            // 选择表 03
            if (SelectTable(3) == false) return false;

            bool rtnVal = TWI_WriteByte(0xA2, 0x9D, (byte)setVal);
            Thread.Sleep(60); // 延时
            return rtnVal;
        }

        public bool setAPD(UInt16 setVal)
        {
            UInt16 ui = 0;
            byte[] w_val = new byte[2];

            ui = (UInt16)setVal;
            if (ui > 1023) ui = 1023;
            w_val[1] = TWI_ReadByte(0xa2, 0x9A);

            w_val[0] = (byte)((ui >> 2) & 0xFF);
            w_val[1] |= (byte)(ui & 0x03);

            if (TWI_WritePage(0xa2, 0x99, w_val, 2) != 2)
            {
                return false;
            }
            return true;
        }

        public bool setWaveLength(UInt16 setval)
        {
            return true;
        }

        public bool setVON(UInt16 setval)
        {
            return true;
        }

        public bool setCPA(UInt16 setVal)
        {
            return true;
        }

        public UInt16 GetRxADC()
        {
            UInt16 rxadc = 0;
            byte[] readbuffer = new byte[2];

            // 选择表 03
            if (SelectTable(3) == false) return 0;

            if (TWI_WriteByte(0xA2, 0xD1, 0x00) == false) //外较准模式
            {
                TWI_WriteByte(0xA2, 0xD1, 0x00); //write again
            }
            Thread.Sleep(300); //延时
            if (TWI_ReadPage(0xA2, 104, readbuffer, 2) == 2)
            {
                rxadc = readbuffer[0];
                rxadc <<= 8;
                rxadc += readbuffer[1];
            }
            else
            {
                rxadc = 0;
            }
            if (TWI_WriteByte(0xA2, 0xD1, 0x40) == false) //内较准模式
            {
                TWI_WriteByte(0xA2, 0xD1, 0x40); //write again
            }

            return rxadc;
        }

        public UInt16 GetTxADC()
        {
            UInt16 txadc = 0;
            byte[] readbuffer = new byte[2];

            // 选择表 03
            if (SelectTable(3) == false) return 0;

            if (TWI_WriteByte(0xA2, 0xD1, 0x00) == false) //外较准模式
            {
                TWI_WriteByte(0xA2, 0xD1, 0x00); //write again
            }
            Thread.Sleep(300); //延时
            if (TWI_ReadPage(0xA2, 102, readbuffer, 2) == 2)
            {
                txadc = readbuffer[0];
                txadc <<= 8;
                txadc += readbuffer[1];
            }
            else
            {
                txadc = 0;
            }
            if (TWI_WriteByte(0xA2, 0xD1, 0x40) == false) //内较准模式
            {
                TWI_WriteByte(0xA2, 0xD1, 0x40); //write again
            }

            return txadc;
        }

        public bool WriteTxCalData()
        {
            byte[] writeByte = new byte[4];
            byte[] readByte = new byte[4];
            //byte[] calpwr_uw = new byte[2];
            float ADC = 0;

            // 选择表 03
            if (SelectTable(3) == false) return false;

            // 计算校准参数
            ADC = GetTxADC();
            if (ADC <= 1)
            {
                return false;
            }
            TestResult2.txPwrCal_k = ((float)((Math.Pow(10, TestResult2.txPower / 10.0) * 10000.0) / ADC));
            TestResult2.txPwrCal_b = 0;

            float fk = TestResult2.txPwrCal_k;
            if (fk < 0) fk = 0;
            if (fk > 255) fk = 255;
            writeByte[0] = (byte)fk;
            writeByte[1] = (byte)((fk - writeByte[0]) * 256);
            writeByte[2] = 0x00;
            writeByte[3] = 0x00;

            if (TWI_WritePage(0xA2, 0xBC, writeByte, 4) != 4)
            {
                return false;
            }
            Thread.Sleep(50); // 延时
            if (TWI_ReadPage(0xA2, 0xBC, readByte, 4) != 4)
            {
                return false;
            }

            return (Bit.ByteEquals(writeByte, readByte));
        }

        public bool WriteRxCalData()
        {
            byte[] writeByte = new byte[16];
            byte[] readByte = new byte[16];
            byte[] bb = new byte[2];
            byte[] kk = new byte[2];
            float k, b;
            try
            {
                //
                b = TestResult2.rxPwrCal_b[0];
                k = TestResult2.rxPwrCal_k[0];
                //

                if (k < 0) { k = 0; }
                if (k > 255) { k = 255; }
                kk[0] = (byte)k;
                kk[1] = (byte)((k - kk[0]) * 256);

                if (b < (-32768)) { b = (-32768); }
                if (b > 32767) { b = 32767; }
                bb = BitConverter.GetBytes(Convert.ToInt16(b));
                Array.Reverse(bb);

                writeByte[0] = kk[0];
                writeByte[1] = kk[1];
                writeByte[2] = bb[0];
                writeByte[3] = bb[1];

                //
                b = TestResult2.rxPwrCal_b[1];
                k = TestResult2.rxPwrCal_k[1];

                if (k < 0) { k = 0; }
                if (k > 255) { k = 255; }
                kk[0] = (byte)k;
                kk[1] = (byte)((k - kk[0]) * 256);

                if (b < (-32768)) { b = (-32768); }
                if (b > 32767) { b = 32767; }
                bb = BitConverter.GetBytes(Convert.ToInt16(b));
                Array.Reverse(bb);

                writeByte[4] = kk[0];
                writeByte[5] = kk[1];
                writeByte[6] = bb[0];
                writeByte[7] = bb[1];

                //
                b = TestResult2.rxPwrCal_b[2];
                k = TestResult2.rxPwrCal_k[2];

                if (k < 0) { k = 0; }
                if (k > 255) { k = 255; }
                kk[0] = (byte)k;
                kk[1] = (byte)((k - kk[0]) * 256);

                if (b < (-32768)) { b = (-32768); }
                if (b > 32767) { b = 32767; }
                bb = BitConverter.GetBytes(Convert.ToInt16(b));
                Array.Reverse(bb);

                writeByte[8] = kk[0];
                writeByte[9] = kk[1];
                writeByte[10] = bb[0];
                writeByte[11] = bb[1];

                writeByte[12] = (byte)(TestResult2.rxAdcCal[1] >> 8);
                writeByte[13] = (byte)(TestResult2.rxAdcCal[1] & 0xFF);
                writeByte[14] = (byte)(TestResult2.rxAdcCal[2] >> 8);
                writeByte[15] = (byte)(TestResult2.rxAdcCal[2] & 0xFF);

                // 选择表 03
                if (SelectTable(3) == false) return false;

                //
                if (TWI_WritePage(0xA2, 0xC0, writeByte, 16) != 16)
                {
                    return false;
                }
                Thread.Sleep(50); // 延时
                if (TWI_ReadPage(0xA2, 0xC0, readByte, 16) != 16)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            /*//Write EEPROM checksum  0xDB_Table03
            if (GetEEPROMcheckSumDB() == false)
            {
                return false;
            }*/

            return (Bit.ByteEquals(writeByte, readByte));
        }

        public bool SaveRxDataAfterDebug()
        {
            //Write EEPROM checksum  0xDB_Table03
            if (GetEEPROMcheckSumDB() == false)
            {
                return false;
            }

            //发送保存命令到 flash
            return WritSaveCmd();
        }

        public bool SaveTxDataAfterDebug()
        {
            UInt16[] bias = new UInt16[84];
            UInt16[] mod = new UInt16[84];

            byte[] w_biasLUT = new byte[105];
            byte[] w_modLUT = new byte[105];
            byte[] w_apdLUT = new byte[105];

            byte[] r_biasLUT = new byte[105];
            byte[] r_modLUT = new byte[105];
            byte[] r_apdLUT = new byte[105];

            byte[] r_regTbl3 = new byte[93];

            byte[] A2hLow96 = new byte[96];
            byte[] r_A2hLow96 = new byte[96];

            int i, delta_bias, delta_mod, bias_value, mod_value;

            float temp_value = 26;
            int temp_index = 0;

            /////////////////////
            for (i = 0; i < 40; i++)
            {
                A2hLow96[i] = threshold[i];
            }
            for (i = 0; i < 36; i++)
            {
                A2hLow96[i + 56] = ex_cal[i];
            }

            // check sum
            A2hLow96[95] = 0;
            for (i = 0; i < 95; i++)
            {
                A2hLow96[95] += A2hLow96[i];
            }
            /////////////////////

            temp_value = GetTemp();
            if (temp_value < -60) return false;

            temp_index = (int)(((float)(temp_value + 40)) / 2.0f);

            if (temp_index < 0) temp_index = 0;
            if (temp_index > 83) temp_index = 83;

            //2021.4.25-5.9
            float f_bias = TestResult2.txBiasDDM; //1mA
            if (f_bias < 3) f_bias = 3;
            if (f_bias > 60) f_bias = 60;
            f_bias *= 9.0f; //BiasLUT 0.1mA
            //

            // 计算平移量
            delta_bias = Convert.ToInt32(f_bias) - biaslut[temp_index];
            //delta_bias = 0; //TestResult2.txapcVal - biaslut[temp_index];
            delta_mod = TestResult2.txmodVal - modlut[temp_index];

            if (modlut[temp_index] == 0) return false; // 消光比补偿表不能为0

            // 计算比例系数
            float fk = TestResult2.txmodVal;
            fk /= modlut[temp_index];
            //

            for (i = 0; i < 84; i++)
            {
                bias_value = delta_bias + biaslut[i];
                if (bias_value < 0) bias_value = 0;
                if (bias_value > 1023) bias_value = 1023;
                bias[i] = (UInt16)bias_value;

                if (GlobalVarFun.k_lut_flag == true)
                {
                    mod_value = (int)(fk * modlut[i]); // 比例缩放
                }
                else
                {
                    mod_value = delta_mod + modlut[i]; // 等量平移
                }
                if (mod_value < 0) mod_value = 0;
                if (mod_value > 1023) mod_value = 1023;
                mod[i] = (UInt16)mod_value;
            }

            i = 0;
            for (int k = 0; k < 105; k = k + 5)
            {
                w_biasLUT[k + 0] = (byte)(bias[i + 0] / 4);
                w_biasLUT[k + 1] = (byte)((bias[i + 0] & 0x03) * 64 + bias[i + 1] / 16);
                w_biasLUT[k + 2] = (byte)((bias[i + 1] & 0x0F) * 16 + bias[i + 2] / 64);
                w_biasLUT[k + 3] = (byte)((bias[i + 2] & 0x3F) * 4 + bias[i + 3] / 256);
                w_biasLUT[k + 4] = (byte)(bias[i + 3] & 0xFF);

                w_modLUT[k + 0] = (byte)(mod[i + 0] / 4);
                w_modLUT[k + 1] = (byte)((mod[i + 0] & 0x03) * 64 + mod[i + 1] / 16);
                w_modLUT[k + 2] = (byte)((mod[i + 1] & 0x0F) * 16 + mod[i + 2] / 64);
                w_modLUT[k + 3] = (byte)((mod[i + 2] & 0x3F) * 4 + mod[i + 3] / 256);
                w_modLUT[k + 4] = (byte)(mod[i + 3] & 0xFF);

                i = i + 4;
            }
            //

            //写入A2Lower 0x60-0x77 保留字节 默认值00
            TWI_WritePage(0xa2, 0x60, A2Lower60h, 24);
            Thread.Sleep(10);

            if (TWI_WritePage(0xa2, 0x00, A2hLow96, 96) != 96) return false;
            Thread.Sleep(10);
            if (TWI_ReadPage(0xa2, 0x00, r_A2hLow96, 96) != 96) return false;

            if (SelectTable(4) == false) return false; //表选择
            if (TWI_WritePage(0xa2, 0x80, w_biasLUT, 105) != 105) return false;
            Thread.Sleep(10);
            if (TWI_ReadPage(0xa2, 0x80, r_biasLUT, 105) != 105) return false;

            if (SelectTable(5) == false) return false; //表选择
            if (TWI_WritePage(0xa2, 0x80, w_modLUT, 105) != 105) return false;
            Thread.Sleep(10);
            if (TWI_ReadPage(0xa2, 0x80, r_modLUT, 105) != 105) return false;

            if (SelectTable(6) == false) return false; //表选择
            if (TWI_ReadPage(0xa2, 0x80, r_apdLUT, 105) != 105) return false;
            for (i = 0; i < 105; i++)
            {
                w_apdLUT[i] += r_apdLUT[i];
            }

            // check sum
            r_A2hLow96[95] = 0;
            for (i = 0; i < 95; i++)
            {
                r_A2hLow96[95] += r_A2hLow96[i];
            }

            // 保存门限0-95  到测试结果
            for (i = 0; i < 96; i++)
            {
                TestResult2.flash_data[i + 256] = r_A2hLow96[i]; //
            }
            //

            // 开启自动温补  按照数据库配置
            if (TxTempLookupTableCtrl(true) == false)
            {
                return false;
            }

            //if (SelectTable(3) == false) return false;
            if (TWI_ReadPage(0xa2, 0x80, r_regTbl3, 93) != 93)
            {
                return false;
            }

            // Get E2PROM checksum  0xDB_Table03
            byte sum = 0;
            for (i = 0; i < 105; i++)
            {
                sum += w_biasLUT[i];
                sum += w_modLUT[i];
                sum += w_apdLUT[i];
            }
            for (i = 0x81; i <= 0xA0; i++)
            {
                sum += r_regTbl3[i - 0x80];
            }
            for (i = 0xA2; i <= 0xD2; i++)
            {
                sum += r_regTbl3[i - 0x80];
            }

            //Write E2PROM checksum
            if (TWI_WriteByte(0xA2, 0xDB, sum) == false)
            {
                return false;
            }
            r_regTbl3[0xDB - 0x80] = TWI_ReadByte(0xA2, 0xDB);
            if (sum != r_regTbl3[0xDB - 0x80])
            {
                return false;
            }

            //写入PW2 //2022.5.20
            if (TWI_WritePage(0xA2, 0xD7, byte_debug_pwd, 4) != 4)
            {
                return false;
            }
            //

            if ((Bit.ByteEquals(A2hLow96, r_A2hLow96) == false)
                || (Bit.ByteEquals(w_biasLUT, r_biasLUT) == false)
                || (Bit.ByteEquals(w_modLUT, r_modLUT) == false))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // TxRxCDR控制  如果不需要直接返回
        public bool DisTxRxCDR(bool disVal)
        {
            return true;
        }

        // 初始化模块   如果不需要直接返回
        public bool InitModule()
        {
            byte[] w_biasLUT = new byte[105];
            byte[] w_modLUT = new byte[105];
            byte[] w_apdLUT = new byte[105];

            byte[] r_biasLUT = new byte[105];
            byte[] r_modLUT = new byte[105];
            byte[] r_apdLUT = new byte[105];

            byte[] w_regTbl3 = new byte[93];
            byte[] r_regTbl3 = new byte[93];

            byte[] checkbyte = new byte[2];

            int i = 0;

            //SetDebugPWD(); //test

            if (SelectTable(3) == false) return false; // 选择表
            if (TWI_ReadPage(0xA2, 0x80, checkbyte, 2) != 2) return false;

            // 判断模块是否初始化过
            if ((checkbyte[0] == 0xAA) && (checkbyte[1] == 0x00))
            {
                return false; //初始化过的模块
            }

            // 重要操作
            //if (SelectTable(3) == false) return false;
            /*TWI_WriteByte(0xa2, 0x86, 0x04);
            Thread.Sleep(10);
            TWI_WriteByte(0xa2, 0x97, 0x20); //需要写2次
            TWI_WriteByte(0xa2, 0x97, 0x20); //需要写2次
            Thread.Sleep(10);
            TWI_WriteByte(0xa2, 0xA1, 0x02);
            Thread.Sleep(10);*/
            TWI_WriteByte(0xa2, 0xA1, 0x12);  // Table3.161的 bit7 和 bit0 设置为 0 bit1 设 置为 1
            TWI_WriteByte(0xa2, 0x86, 0x00);  // Table3.134的 bit6 和 bit7 设置为 0
            TWI_WriteByte(0xa2, 0x96, 0x02);  // Table3.151寄存器值设置
            TWI_WriteByte(0xa2, 0x97, 0x00);  // Table3.151寄存器值设置为 0
            TWI_WriteByte(0xa2, 0x9B, 0x01);  // Table3.155的 bit2 设置为 0
            TWI_WriteByte(0xa2, 0xD1, 0x40);  // Table3.209的 bit7 设置为 0
            Thread.Sleep(10);
            //
            if (TWI_ReadPage(0xa2, 0xA1, checkbyte, 1) != 1) return false;
            if (checkbyte[0] != 0x12) return false;
            //

            for (i = 0; i < 93; i++)
            {
                w_regTbl3[i] = register[i];
            }

            int n = 0;
            for (int k = 0; k < 105; k = k + 5)
            {
                w_biasLUT[k + 0] = (byte)(biaslut[n + 0] / 4);
                w_biasLUT[k + 1] = (byte)((biaslut[n + 0] & 0x03) * 64 + biaslut[n + 1] / 16);
                w_biasLUT[k + 2] = (byte)((biaslut[n + 1] & 0x0F) * 16 + biaslut[n + 2] / 64);
                w_biasLUT[k + 3] = (byte)((biaslut[n + 2] & 0x3F) * 4 + biaslut[n + 3] / 256);
                w_biasLUT[k + 4] = (byte)(biaslut[n + 3] & 0xFF);

                w_modLUT[k + 0] = (byte)(modlut[n + 0] / 4);
                w_modLUT[k + 1] = (byte)((modlut[n + 0] & 0x03) * 64 + modlut[n + 1] / 16);
                w_modLUT[k + 2] = (byte)((modlut[n + 1] & 0x0F) * 16 + modlut[n + 2] / 64);
                w_modLUT[k + 3] = (byte)((modlut[n + 2] & 0x3F) * 4 + modlut[n + 3] / 256);
                w_modLUT[k + 4] = (byte)(modlut[n + 3] & 0xFF);

                w_apdLUT[k + 0] = (byte)(apdlut[n + 0] / 4);
                w_apdLUT[k + 1] = (byte)((apdlut[n + 0] & 0x03) * 64 + apdlut[n + 1] / 16);
                w_apdLUT[k + 2] = (byte)((apdlut[n + 1] & 0x0F) * 16 + apdlut[n + 2] / 64);
                w_apdLUT[k + 3] = (byte)((apdlut[n + 2] & 0x3F) * 4 + apdlut[n + 3] / 256);
                w_apdLUT[k + 4] = (byte)(apdlut[n + 3] & 0xFF);

                n = n + 4;
            }
            //

            //
            //// 开始配置UX3320T 寄存器 ////
            //
            SelectTable(4);
            TWI_WritePage(0xa2, 0x80, w_biasLUT, 105);
            Thread.Sleep(50);
            TWI_ReadPage(0xa2, 0x80, r_biasLUT, 105);
            if (!Bit.ByteEquals(w_biasLUT, r_biasLUT))
            {
                return false;
            }

            SelectTable(5);
            TWI_WritePage(0xa2, 0x80, w_modLUT, 105);
            Thread.Sleep(50);
            TWI_ReadPage(0xa2, 0x80, r_modLUT, 105);
            if (!Bit.ByteEquals(w_modLUT, r_modLUT))
            {
                return false;
            }

            SelectTable(6);
            TWI_WritePage(0xa2, 0x80, w_apdLUT, 105);
            Thread.Sleep(50);
            TWI_ReadPage(0xa2, 0x80, r_apdLUT, 105);
            if (!Bit.ByteEquals(w_apdLUT, r_apdLUT))
            {
                return false;
            }

            // Get EEPROM checksum
            byte sum = 0;
            for (i = 0; i < 105; i++)
            {
                sum += w_biasLUT[i];
                sum += w_modLUT[i];
                sum += w_apdLUT[i];
            }
            for (i = 0x81; i <= 0xA0; i++)
            {
                sum += w_regTbl3[i - 0x80];
            }
            for (i = 0xA1; i <= 0xD2; i++)
            {
                sum += w_regTbl3[i - 0x80];
            }
            w_regTbl3[0xDB - 0x80] = (byte)sum;
            //

            SelectTable(3);
            TWI_WritePage(0xa2, 0x80, w_regTbl3, 93);
            Thread.Sleep(50);
            TWI_ReadPage(0xa2, 0x80, r_regTbl3, 93);

            //w_regTbl3[0x97 - 0x80] = r_regTbl3[0x97 - 0x80];

            r_regTbl3[0xD3 - 0x80] = w_regTbl3[0xD3 - 0x80]; //PW1
            r_regTbl3[0xD4 - 0x80] = w_regTbl3[0xD4 - 0x80];
            r_regTbl3[0xD5 - 0x80] = w_regTbl3[0xD5 - 0x80];
            r_regTbl3[0xD6 - 0x80] = w_regTbl3[0xD6 - 0x80];
            r_regTbl3[0xD7 - 0x80] = w_regTbl3[0xD7 - 0x80]; //PW2
            r_regTbl3[0xD8 - 0x80] = w_regTbl3[0xD8 - 0x80];
            r_regTbl3[0xD9 - 0x80] = w_regTbl3[0xD9 - 0x80];
            r_regTbl3[0xDA - 0x80] = w_regTbl3[0xDA - 0x80];
            //

            //
            if (SoftResetUX3320T() == false) return false; // 软件操作 复位GN25L95
            //

            // 校验数据是否正确
            if (Bit.ByteEquals(w_regTbl3, r_regTbl3))
            {
                return true; // 初始化成功
            }
            else
            {
                return false; // 初始化失败
            }
        }

        // 模块发射温度补偿表控制   如果不需要直接返回
        public bool TxTempLookupTableCtrl(bool enable)
        {
            byte value = 0x00;

            // 选择 03
            if (SelectTable(3) == false) return false;

            if (enable)
            {
                value = register[0x82 - 0x80];//modluten biaslut使能BIAS/MOD自动查表
                //value |= 0xC0;
                TWI_WriteByte(0xA2, 0x82, value);

                if (TestResult2.fibertop_pn.Contains("DCP") || TestResult2.fibertop_pn.Contains("DIP"))//APDLUT
                {
                    //byte value = 0x00;
                    //value = TWI_ReadByte(0xa2, 0x98);
                    value = register[0x98 - 0x80];//
                    value = (byte)(value & 0xF7);  // BIT3=0 enable APDLUT
                    TWI_WriteByte(0xa2, 0x98, value);
                }
            }
            else
            {
                value = register[0x82 - 0x80];//modluten biaslut关闭BIAS/MOD自动查表
                value = (byte)(value & 0x3F);
                TWI_WriteByte(0xA2, 0x82, value);

                if (TestResult2.fibertop_pn.Contains("DCP") || TestResult2.fibertop_pn.Contains("DIP"))//APDLUT
                {
                    //byte value = 0x00;
                    //value = TWI_ReadByte(0xa2, 0x98);
                    value = register[0x98 - 0x80];//
                    value = (byte)(value | 0x08);  // BIT3=1 dis APDLUT               
                    TWI_WriteByte(0xa2, 0x98, value);
                }
            }




            Thread.Sleep(600); //延时
            return true;
        }

        // 模块默认调试参数  TX-PE  RX-PE  TX-CPA等 如果不需要直接返回
        public bool WriteTxRxDefaultVal()
        {
            return true;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        // 内部调用函数
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        private bool WritSaveCmd()
        {
            return true;
        }

        public bool SelectTable(int tbl)
        {
            byte[] r_val = new byte[1];

            r_val[0] = 0xFF;
            if (TWI_ReadPage(0xA2, 0x7F, r_val, 1) != 1) return false;
            if (r_val[0] == (byte)tbl)
            {
                return true;
            }

            if (TWI_WriteByte(0xA2, 0x7F, tbl) == false) return false;
            Thread.Sleep(5);
            if (TWI_ReadPage(0xA2, 0x7F, r_val, 1) != 1) return false;
            if (r_val[0] != (byte)tbl)
            {
                return false;
            }
            return true;
        }

        // 写入软件复位密码  复位UX3320T
        private bool SoftResetUX3320T()
        {
            byte[] reset_pwd = new byte[5];

            reset_pwd[0] = 0x4A;
            reset_pwd[1] = 0x36;
            reset_pwd[2] = 0x58;
            reset_pwd[3] = 0x6E;
            reset_pwd[4] = 0x7B;

            if (TWI_WritePage(0xA2, 0x7B, reset_pwd, 5) == 5)
            {
                Thread.Sleep(1000); //延时
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool GetEEPROMcheckSumDB()
        {
            // EEPROMCHECKSUM
            byte[] r_biasLUT = new byte[105];
            byte[] r_modLUT = new byte[105];
            byte[] r_apdLUT = new byte[105];
            byte[] r_regTbl3 = new byte[93];
            byte[] r_val = new byte[2];

            SelectTable(4);
            TWI_ReadPage(0xA2, 0x80, r_biasLUT, 105);

            SelectTable(5);
            TWI_ReadPage(0xA2, 0x80, r_modLUT, 105);

            SelectTable(6);
            TWI_ReadPage(0xA2, 0x80, r_apdLUT, 105);

            SelectTable(3);
            TWI_ReadPage(0xa2, 0x80, r_regTbl3, 93);

            byte i = 0;
            byte sum = 0;
            for (i = 0; i < 105; i++)
            {
                sum += r_biasLUT[i];
                sum += r_modLUT[i];
                sum += r_apdLUT[i];
            }
            for (i = 0x81; i <= 0xA0; i++)
            {
                sum += r_regTbl3[i - 0x80];
            }
            for (i = 0xA2; i <= 0xD2; i++)
            {
                sum += r_regTbl3[i - 0x80];
            }

            SelectTable(3);
            if (TWI_WriteByte(0xA2, 0xDB, sum) == false)
            {
                TWI_WriteByte(0xA2, 0xDB, sum); //write again
            }
            //check
            if (TWI_ReadPage(0xA2, 0xDB, r_val, 1) != 1) return false;
            if (r_val[0] != sum)
            {
                return false;
            }

            return true;
        }
        public bool EEPROMcheckSum()
        {
            return GetEEPROMcheckSumDB();
        }
        public bool elec_moudleTest()
        {
            return true;
        }
        public bool Get_HardWare_LOS()
        {
            //return i2c.GetRxLos(_slot) == "1";
            return true;
        }
        public bool SetModuleDis(bool dis)
        {
            //return i2c.SetPowerEN(dis ? 0 : 1, _slot);
            return true;
        }

        #region TWI底层通信方法(适配SFP_EVB_Heater，支持十六进制/十进制参数，public可外部调用)

        /// <summary>
        /// TWI 多字节读 (public)。deviceAddr/regAddr 支持0xA2或162等int字面量。
        /// </summary>
        public int TWI_ReadPage(int deviceAddr, int regAddr, byte[] buf, int len)
        {
            try
            {
                string dA = $"{(deviceAddr & 0xFF):X2}";
                string rA = $"{(regAddr & 0xFF):X2}";
                string resp = i2c.IIC_Get(dA, rA, len.ToString(), _slot);
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
            catch { return 0; }
        }

        /// <summary>
        /// TWI 多字节写 - byte[]版本 (public)。数据字节用空格分隔，不带0x前缀。
        /// </summary>
        public int TWI_WritePage(int deviceAddr, int regAddr, byte[] buf, int len)
        {
            try
            {
                string dA = $"{(deviceAddr & 0xFF):X2}";
                string rA = $"{(regAddr & 0xFF):X2}";
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append($"{buf[i]:X2}");
                }
                bool ok = i2c.IIC_Set(dA, rA, len.ToString(), sb.ToString(), _slot);
                return ok ? len : 0;
            }
            catch { return 0; }
        }

        /// <summary>
        /// TWI 单字节写 - int值便捷重载 (public)。等效于new byte[1]{(byte)value}，len=1。
        /// </summary>
        public int TWI_WritePage(int deviceAddr, int regAddr, int value, int length)
        {
            byte[] data = new byte[1] { (byte)value };
            return TWI_WritePage(deviceAddr, regAddr, data, 1);
        }

        /// <summary>
        /// TWI 读单字节 (public)
        /// </summary>
        public byte TWI_ReadByte(int deviceAddr, int regAddr)
        {
            byte[] b = new byte[1];
            if (TWI_ReadPage(deviceAddr, regAddr, b, 1) == 1) return b[0];
            return 0;
        }

        /// <summary>
        /// TWI 写单字节 (public)。val支持传入0x03或3等int/byte字面量。
        /// </summary>
        public bool TWI_WriteByte(int deviceAddr, int regAddr, int val)
        {
            byte[] b = new byte[] { (byte)val };
            return TWI_WritePage(deviceAddr, regAddr, b, 1) == 1;
        }
        #endregion
        //////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}


