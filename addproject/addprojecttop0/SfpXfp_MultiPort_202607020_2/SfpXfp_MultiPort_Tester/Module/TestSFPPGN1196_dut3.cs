using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;

namespace FibertopTest_Common
{
    public class SFPPGN1196_Dut3 : ModuleTest
    {
        EVB i2c;
        private int _slot;
        public static byte Dut_num = 0;
        UInt16[] modlut = new UInt16[32];
        byte[] apclut = new byte[64];
        byte[] apdlut = new byte[64];

        byte[] register = new byte[128];//62
        byte[] register2 = new byte[128];//62
        byte[] threshold = new byte[40];

        byte[] ex_cal = new byte[39]
                {
                    0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                    0x3F,0x80,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x00,0x00,
                    0x01,0x00,0x00,0x00,0x01,0x00,0x00,0x00,0x01,0x00,0x00,0x00,
                    0x00,0x00,0x00
                };

        byte[] awflag_en = new byte[8] { 0xFF, 0xC0, 0x00, 0x00, 0xFF, 0xC0, 0x00, 0x00 };

        byte[] byte_debug_pwd = new byte[4];
        byte[] byte_protect_pwd = new byte[4];
        byte[] byte_def_pwd = new byte[4];

        public void Init(EVB i2c, byte Dut)
        {
            this.i2c = i2c;
            Dut_num = Dut;
            _slot = Dut;
            // 0x00:线性计算法 apc-->uw & bias   0x11: 普通二分法 apc-->dBm   22:差值二分法 apc-->uW
            GlobalVarFun.txpwr_debug_method = 0x11;

            TestResult.flash_data_len = 1024; // 256+256+128+128+128+128=1024  必须<=1024

            // 更新调试密码
            byte_debug_pwd[0] = 0xA9;//
            byte_debug_pwd[1] = 0x54;//
            byte_debug_pwd[2] = 0x50;//
            byte_debug_pwd[3] = 0x66;//

            byte_protect_pwd[0] = 0x11;
            byte_protect_pwd[0] = 0x11;
            byte_protect_pwd[0] = 0x11;
            byte_protect_pwd[0] = 0x11;

            byte_def_pwd[0] = 0xFF;
            byte_def_pwd[0] = 0xFF;
            byte_def_pwd[0] = 0xFF;
            byte_def_pwd[0] = 0xFF;
        }

        public bool CheckTestTypeInfo()
        {
            // 功能未用
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
            if (TWI_WritePage(0xa2, 0x7B, byte_debug_pwd, 4) != 4)
            {
                return false;
            }
            Thread.Sleep(300);
            //
            byte[] readbuffer = new byte[4];
            byte[] pwd_read = new byte[4];
            pwd_read[0] = 0x00;
            pwd_read[1] = 0x00;
            pwd_read[2] = 0x00;
            pwd_read[3] = 0x00;
            // 读密码
            if (TWI_ReadPage(0xa2, 0x7B, readbuffer, 4) != 4)
            {
                return false;
            }
            //
            return Bit.ByteEquals(readbuffer, pwd_read); // GN1196 密码位置读取固定为 0x00
        }

        public byte CheckDebugPWD()
        {
            byte[] readbuffer = new byte[4];

            // 读密码
            if (TWI_ReadPage(0xa2, 0x7B, readbuffer, 4) != 4)
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

            /*// 未初始化模块使用
            if ((GlobalVarFun.testType == "firstTest") && (vccDDM < 2.3f))
            {
                vccDDM = 2.3f; // 2.3V 初测用
            }*/

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

                TestResult.tempDDM = (j + readbuffer[1] * (1 / 256.0f));
                TestResult.vccDDM = ((readbuffer[2] * 256 + readbuffer[3]) / 10000.0f);
                TestResult.txBiasDDM = ((readbuffer[4] * 256 + readbuffer[5]) / 500.0f);
                //
                if (readbuffer[6] == 0x00 && readbuffer[7] == 0x00)
                    readbuffer[7] = 0x01;
                if (readbuffer[8] == 0x00 && readbuffer[9] == 0x00)
                    readbuffer[9] = 0x01;
                //
                TestResult.txPowerDDM = (float)(10 * Math.Log10((readbuffer[6] * 256 + readbuffer[7]) / 10000.0));
                TestResult.rxPowerDDM = (float)(10 * Math.Log10((readbuffer[8] * 256 + readbuffer[9]) / 10000.0));
            }
            else
            {
                TestResult.tempDDM = 0;
                TestResult.vccDDM = 0;
                TestResult.txBiasDDM = 0;
                TestResult.txPowerDDM = -100;
                TestResult.rxPowerDDM = -100;
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
            TestResult.tempHA = (float)(i + readbuffer[1] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[2]);
            TestResult.tempLA = (float)(i + readbuffer[3] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[4]);
            TestResult.tempHW = (float)(i + readbuffer[5] * 1.0 / 256.0);
            i = Convert.ToInt32((sbyte)readbuffer[6]);
            TestResult.tempLW = (float)(i + readbuffer[7] * 1.0 / 256.0);

            TestResult.vccHA = (float)((readbuffer[8] * 256 + readbuffer[9]) / 10000.0);
            TestResult.vccLA = (float)((readbuffer[10] * 256 + readbuffer[11]) / 10000.0);
            TestResult.vccHW = (float)((readbuffer[12] * 256 + readbuffer[13]) / 10000.0);
            TestResult.vccLW = (float)((readbuffer[14] * 256 + readbuffer[15]) / 10000.0);

            TestResult.txBiasHA = (float)((readbuffer[16] * 256 + readbuffer[17]) / 500.0);
            TestResult.txBiasLA = (float)((readbuffer[18] * 256 + readbuffer[19]) / 500.0);
            TestResult.txBiasHW = (float)((readbuffer[20] * 256 + readbuffer[21]) / 500.0);
            TestResult.txBiasLW = (float)((readbuffer[22] * 256 + readbuffer[23]) / 500.0);

            TestResult.txPowerHA = (float)(10 * Math.Log10((readbuffer[24] * 256 + readbuffer[25]) / 10000.0));
            TestResult.txPowerLA = (float)(10 * Math.Log10((readbuffer[26] * 256 + readbuffer[27]) / 10000.0));
            TestResult.txPowerHW = (float)(10 * Math.Log10((readbuffer[28] * 256 + readbuffer[29]) / 10000.0));
            TestResult.txPowerLW = (float)(10 * Math.Log10((readbuffer[30] * 256 + readbuffer[31]) / 10000.0));

            TestResult.rxPowerHA = (float)(10 * Math.Log10((readbuffer[32] * 256 + readbuffer[33]) / 10000.0));
            TestResult.rxPowerLA = (float)(10 * Math.Log10((readbuffer[34] * 256 + readbuffer[35]) / 10000.0));
            TestResult.rxPowerHW = (float)(10 * Math.Log10((readbuffer[36] * 256 + readbuffer[37]) / 10000.0));
            TestResult.rxPowerLW = (float)(10 * Math.Log10((readbuffer[38] * 256 + readbuffer[39]) / 10000.0));
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

            TestResult.tempHA_flag = Bit.GetBit(readbuffer[0], 7);
            TestResult.tempLA_flag = Bit.GetBit(readbuffer[0], 6);
            TestResult.vccHA_flag = Bit.GetBit(readbuffer[0], 5);
            TestResult.vccLA_flag = Bit.GetBit(readbuffer[0], 4);
            TestResult.txBiasHA_flag = Bit.GetBit(readbuffer[0], 3);
            TestResult.txBiasLA_flag = Bit.GetBit(readbuffer[0], 2);
            TestResult.txPwrHA_flag = Bit.GetBit(readbuffer[0], 1);
            TestResult.txPwrLA_flag = Bit.GetBit(readbuffer[0], 0);
            TestResult.rxPwrHA_flag = Bit.GetBit(readbuffer[1], 7);
            TestResult.rxPwrLA_flag = Bit.GetBit(readbuffer[1], 6);

            TestResult.tempHW_flag = Bit.GetBit(readbuffer[4], 7);
            TestResult.tempLW_flag = Bit.GetBit(readbuffer[4], 6);
            TestResult.vccHW_flag = Bit.GetBit(readbuffer[4], 5);
            TestResult.vccLW_flag = Bit.GetBit(readbuffer[4], 4);
            TestResult.txBiasHW_flag = Bit.GetBit(readbuffer[4], 3);
            TestResult.txBiasLW_flag = Bit.GetBit(readbuffer[4], 2);
            TestResult.txPwrHW_flag = Bit.GetBit(readbuffer[4], 1);
            TestResult.txPwrLW_flag = Bit.GetBit(readbuffer[4], 0);
            TestResult.rxPwrHW_flag = Bit.GetBit(readbuffer[5], 7);
            TestResult.rxPwrLW_flag = Bit.GetBit(readbuffer[5], 6);

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
                TestResult.flash_data[i] = readbuffer[i];
            }

            if (SelectTable(1) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 0, readbuffer, 256) != 256)
            {
                return false;
            }
            for (i = 0; i < 256; i++)
            {
                TestResult.flash_data[i + 256] = readbuffer[i];
            }

            TestResult.sn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 68, 16);
            TestResult.pn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 40, 16);
            TestResult.vn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 20, 16);
            TestResult.date = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 84, 8);
            //TestResult.sn.TrimEnd();
            //TestResult.pn.TrimEnd();
            //TestResult.vn.TrimEnd();
            //TestResult.date.TrimEnd();

            return true;
        }

        public bool GetFlashInfoDebug()
        {
            byte[] readbuffer = new byte[256];
            int i;

            // 读取 Alarm & Warning Flags Enable  PW2
            if (SelectTable(1) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 0xF8, readbuffer, 8) != 8)
            {
                return false;
            }
            for (i = 0; i < 8; i++)
            {
                TestResult.flash_data[i + 256 + 248] = readbuffer[i]; //0xF8-FF
            }
            //

            if (SelectTable(0x80) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 512] = readbuffer[i];
            }

            if (SelectTable(0x81) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 640] = readbuffer[i];
            }

            if (SelectTable(0x82) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)//moudle_LUT
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 768] = readbuffer[i];
            }

            if (SelectTable(0x83) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 128) != 128)//APC_LUT
            {
                return false;
            }
            for (i = 0; i < 128; i++)
            {
                TestResult.flash_data[i + 896] = readbuffer[i];
            }

            if (SelectTable(0x85) == false) return false; // 表选择
            if (TWI_ReadPage(0xa2, 128, readbuffer, 5) != 5)//fibertopSN
            {
                return false;
            }
            // 更新 Fsn  飞思卓产品内部流水号
            //UInt64 iFsn = 0;
            //iFsn += TestResult.flash_data[768 + 0]; // 0x00 = 1
            //iFsn <<= 8;
            //iFsn += TestResult.flash_data[768 + 1]; // 0x01 = 2
            //iFsn <<= 8;
            //iFsn += TestResult.flash_data[768 + 2]; // 0x02 = 3
            //iFsn <<= 8;
            //iFsn += TestResult.flash_data[768 + 3]; // 0x03 = 4
            //iFsn <<= 8;
            //iFsn += TestResult.flash_data[768 + 4]; // 0x04 = 5

            UInt64 iFsn = 0;
            iFsn += readbuffer[0]; // 0x00 = 1
            iFsn <<= 8;
            iFsn += readbuffer[1]; // 0x01 = 2
            iFsn <<= 8;
            iFsn += readbuffer[2]; // 0x02 = 3
            iFsn <<= 8;
            iFsn += readbuffer[3]; // 0x03 = 4
            iFsn <<= 8;
            iFsn += readbuffer[4]; // 0x04 = 5

            if (iFsn > TestResult.max_Fsn)
            {
                iFsn = TestResult.max_Fsn;
            }
            TestResult.fibertop_sn = iFsn.ToString("D12");
            //TestResult.fibertop_sn = System.Text.Encoding.ASCII.GetString(TestResult.flash_data, 768 + 0, 10); //0x00-0A  table05
            //TestResult.fibertop_sn.TrimEnd();
            //TestResult.fibertop_sn = TestResult.sn; //test

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
                TestResult.flash_data[i + 256] = readbuffer[i];
            }

            return true;
        }

        public bool CheckThresholdsInfo(ref string errMsg)
        {
            byte[] threshold_read = new byte[40];
            byte[] ex_cal_read = new byte[39];
            byte check_sum = 0;
            int i = 0;

            errMsg = "";

            for (i = 0; i < 40; i++)
            {
                threshold_read[i] = TestResult.flash_data[i + 256];
            }

            for (i = 0; i < 39; i++)
            {
                ex_cal_read[i] = TestResult.flash_data[i + 256 + 56];
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
                check_sum += TestResult.flash_data[i + 256];
            }

            // check sum errro
            if (check_sum != TestResult.flash_data[95 + 256])
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
            UInt16[] modlut_read = new UInt16[32];
            byte[] apclut_read = new byte[32];
            byte[] apdlut_read = new byte[32];

            byte[] regcfg = new byte[128];
            byte[] regcfg_read = new byte[128];
            byte[] regcfg2 = new byte[128];
            byte[] regcfg2_read = new byte[128];

            byte[] awflag_en_read = new byte[8];

            int[] q = new int[32];

            int i = 0;

            UInt16 temp_ui = 0;

            errMsg = "";

            //CheckThresholdsInfo(ref errMsg); // 检查告警门限、内外校准参数

            for (i = 0; i < 128; i++)//
            {
                regcfg[i] = register[i];
                regcfg2[i] = register2[i];//
            }


            for (i = 0; i < 32; i++)//
            {
                modlut_read[i] = TestResult.flash_data[i * 2 + 832];
                modlut_read[i] <<= 4;//2
                temp_ui = TestResult.flash_data[i * 2 + 1 + 832];
                temp_ui >>= 4;//6
                modlut_read[i] += temp_ui;
                //
                //apclut_read[i]  = TestResult.flash_data[i + 896];
                // apdlut_read[i]  = TestResult.flash_data[i + 960];

            }

            for (i = 0; i < 8; i++)
            {
                awflag_en_read[i] = TestResult.flash_data[i + 256 + 248]; //0xF8-FF
            }

            for (i = 0; i < 128; i++)
            {
                regcfg_read[i] = TestResult.flash_data[i + 512]; //0xA0 = 160 - 0x80//
                regcfg2_read[i] = TestResult.flash_data[i + 640];//
            }

            /////////////////////////////////////////////////////////////////
            regcfg2[24] = regcfg2_read[24] = 0xFF;// // PW1
            regcfg2[25] = regcfg2_read[25] = 0xFF;//
            regcfg2[26] = regcfg2_read[26] = 0xFF;//
            regcfg2[27] = regcfg2_read[27] = 0xFF;//

            regcfg2[28] = byte_debug_pwd[0];// // PW2
            regcfg2[29] = byte_debug_pwd[1];//
            regcfg2[30] = byte_debug_pwd[2];//
            regcfg2[31] = byte_debug_pwd[3];//

            regcfg[6] = regcfg_read[6] = 0x00; //

            regcfg[22] = regcfg_read[22] = 0x00; // APC_LOCK_CTRL//
            regcfg[24] = regcfg_read[24] = 0x00; // APC SET
            regcfg[25] = regcfg_read[25] = 0x00; // APC SET

            regcfg[26] = regcfg_read[26] = 0x00; // 
            regcfg[27] = regcfg_read[27] = 0x00; // 

            regcfg[28] = regcfg_read[28] = 0x00; // BIAS SET
            regcfg[29] = regcfg_read[29] = 0x00; // BIAS SET

            regcfg[30] = regcfg_read[30] = 0x00; // MOD SET
            regcfg[31] = regcfg_read[31] = 0x00; // MOD SET

            regcfg[32] = regcfg_read[32] = 0x00; //RESERVED

            //regcfg[38] = regcfg_read[38] = 0x00; //

            regcfg[73] = regcfg_read[73] = 0x00; //IDAC

            regcfg[86] = regcfg_read[86] = 0x00; // LOS SET

            regcfg[92] = regcfg_read[92] = 0x00; //

            regcfg2[1] = regcfg2_read[1] = 0x00; //
            regcfg2[3] = regcfg2_read[3] = 0x00; //

            regcfg2[70] = regcfg2_read[70] = 0x00; //RSVD
            regcfg2[71] = regcfg2_read[71] = 0x00; //RSVD
            regcfg2[80] = regcfg2_read[80] = 0x00; //RSVD

            regcfg2[81] = regcfg2_read[81] = 0x00; //
            regcfg2[86] = regcfg2_read[86] = 0x00; //
            regcfg2[87] = regcfg2_read[87] = 0x00; //
            regcfg2[88] = regcfg2_read[88] = 0x00; ////////////////
            regcfg2[89] = regcfg2_read[89] = 0x00; ////////////////

            regcfg2[96] = regcfg2_read[96] = 0x00; //
            regcfg2[97] = regcfg2_read[97] = 0x00; //
            regcfg2[98] = regcfg2_read[98] = 0x00; //
            regcfg2[99] = regcfg2_read[99] = 0x00; //
            regcfg2[100] = regcfg2_read[100] = 0x00; //
            regcfg2[101] = regcfg2_read[101] = 0x00; //
            regcfg2[102] = regcfg2_read[102] = 0x00; //
            regcfg2[103] = regcfg2_read[103] = 0x00; //
            regcfg2[104] = regcfg2_read[104] = 0x00; //
            regcfg2[105] = regcfg2_read[105] = 0x00; //
            regcfg2[106] = regcfg2_read[106] = 0x00; //
            regcfg2[107] = regcfg2_read[107] = 0x00; //
            regcfg2[108] = regcfg2_read[108] = 0x00; //
            regcfg2[109] = regcfg2_read[109] = 0x00; //
            regcfg2[110] = regcfg2_read[110] = 0x00; //
            regcfg2[111] = regcfg2_read[111] = 0x00; //
            regcfg2[112] = regcfg2_read[112] = 0x00; //
            regcfg2[113] = regcfg2_read[113] = 0x00; //
            regcfg2[114] = regcfg2_read[114] = 0x00; //
            regcfg2[115] = regcfg2_read[115] = 0x00; //
            regcfg2[116] = regcfg2_read[116] = 0x00; //
            regcfg2[117] = regcfg2_read[117] = 0x00; //

            /////////////////////////////////////////////////////////////////

            //未用字节
            //awflag_en_read[2] = awflag_en_read[3] = awflag_en_read[6] = awflag_en_read[7] = 0x00;//
            // check Alarm & Warning Flags Enable
            if (!Bit.ByteEquals(awflag_en_read, awflag_en))
            {
                errMsg += "(A2h:0xF8-FF)AW Flags Enable设置";
            }

            // check GN1196 配置表80 0x80开始  128个字节
            if (Bit.ByteEquals(regcfg, regcfg_read) == false)
            {
                /*string ss = string.Empty;
                for (int w = 0; w < 128; w++)
                {
                    if (regcfg[w] != regcfg_read[w])
                    {
                        ss += w.ToString()+" ";
                    }
                }
                errMsg += "GN1196配置表80: "+ss;*/
                errMsg += "GN1196配置表80 ";
            }
            // check GN1196 配置表81 0x80开始  128个字节
            if (Bit.ByteEquals(regcfg2, regcfg2_read) == false)
            {
                string ss = string.Empty;
                for (int w = 0; w < 128; w++)
                {
                    if (regcfg2[w] != regcfg2_read[w])
                    {
                        ss += w.ToString() + " ";
                    }
                }
                errMsg += "GN1196配置表81: " + ss;
                // errMsg += "GN1196配置表81 ";
            }

            // check  MOD 补偿表
            UInt16[] modlut_fk = new UInt16[64];
            float fk = 0;
            fk = modlut_read[12]; // 25度 补偿点//26
            fk /= modlut[12];//26
            //
            for (i = 0; i < 32; i++)
            {
                q[i] = modlut_read[i] - modlut[i];
            }
            if (q.Max() != q.Min()) // 平移检查错误  进入比例缩放检查
            {
                for (i = 0; i < 32; i++)
                {
                    modlut_fk[i] = (UInt16)(fk * modlut[i]);
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
            string Apc_LUT_Field = "APC_LUT";
            string Mod_LUT_Field = "MODval";
            string Apd_LUT_Field = "APDval";
            string str = "";
            //
            try
            {
                dbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= " + GlobalVarFun.moduleLutDBFilePath);
                // dbconnectionstr = string.Format("select TxPowerSetPoint,TxERSetPoint,TxER_MIN,TxER_MAX,Bias_MAX,Bias_MIN,TxPower_MIN,TxPower_MAX,TxCr_MIN,TxCr_MAX,TxJt_MAX,Sensitivity,RxALos,RxDLos,RxOverLoad,"
                //  + "RxPwrCheck_1,RxPwrCheck_2,RxPwrCheck_3,RxPwrCheck_4,RxPwrCheck_5,APD_Name,APCmin,APCmax,MODmin,MODmax,LOSmin,LOSmax,MaskName,MaskMargin from ModuleType where Type = '{0}'", TestResult.fibertop_pn);
                dbconnectionstr = string.Format("select TxPowerSetPoint,TxERSetPoint,TxER_MIN,TxER_MAX,Bias_MAX,Bias_MIN,TxPower_MIN,TxPower_MAX,TxCr_MIN,TxCr_MAX,TxJt_MAX,Sensitivity,RxALos,RxDLos,RxOverLoad,"
                                            + @"RxPwrCheck_1,RxPwrCheck_2,RxPwrCheck_3,RxPwrCheck_4,RxPwrCheck_5,APD_Name,APCmin,APCmax,MODmin,MODmax,LOSmin,LOSmax,MaskName,MaskMargin,
                                              APCmin_def,APCmax_def,MODmin_def,MODmax_def,LOSmin_def,LOSmax_def,APDmin_def,APDmax_def,VONmin_def,VONmax_def,Crossingmin_def,Crossingmax_def,TosaTempmin_def,
                                              Tosatempmax_def,ER_Prec,TxPwr_Prec,RxPwr_Prec,Wlgth_Prec,Wlgth_err,delay_DOA,delay_OPM,Test_Rx,Test_RxNopwr,Test_Tx,Test_TxNopwr,Test_TxDis_HW,Test_RxLos_HW,Test_Sen,
                                              Test_25GALG,Test_COB_LD,Test_EML,Test_APD,Test_CopperSFP,Test_CDRDis,Test_SchemeDis,Test_Init,delay_PSSBERT,Rosa_PIN,Test_EyeSave
                                              from ModuleType where Type = '{0}'", TestResult.fibertop_pn);
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

                TestResult.mask_name = dbset.Tables["ModuleType"].Rows[0]["MaskName"].ToString().Trim();
                str = dbset.Tables["ModuleType"].Rows[0]["MaskMargin"].ToString().Trim();
                if (!string.IsNullOrEmpty(str))
                {
                    TestResult.mask_margin = Convert.ToUInt16(str);
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
                    dbconnectionstr = string.Format("select register,register2,{1},{2},{3} from [{0}] ORDER BY ID ASC", TestResult.fibertop_pn, AW_Threshold_Field, Apc_LUT_Field, Mod_LUT_Field);
                }
                else
                {
                    dbconnectionstr = string.Format("select register,register2,{1},{2},{3},{4} from [{0}] ORDER BY ID ASC", TestResult.fibertop_pn, AW_Threshold_Field, Apc_LUT_Field, Mod_LUT_Field, Apd_LUT_Field);
                }

                dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, TestResult.fibertop_pn);
                //
                for (i = 0; i < 40; i++)
                {
                    threshold[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i][AW_Threshold_Field]);
                }

                for (i = 0; i < 128; i++)//
                {
                    register[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i]["register"]);
                }

                for (i = 0; i < 128; i++)//
                {
                    register2[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i]["register2"]);
                }

                for (i = 0; i < 32; i++)
                {
                    apclut[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i][Apc_LUT_Field]);
                }

                for (i = 0; i < 32; i++)
                {
                    modlut[i] = Convert.ToUInt16(dbset.Tables[TestResult.fibertop_pn].Rows[i][Mod_LUT_Field]);
                }

                if (string.IsNullOrEmpty(TestSet.apdName) == false) // APD
                {
                    for (i = 0; i < 32; i++)
                    {
                        apdlut[i] = Convert.ToByte(dbset.Tables[TestResult.fibertop_pn].Rows[i][Apd_LUT_Field]);
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
            byte[] valbuff = new byte[2];

            if (setVal > 900)//
            {
                setVal = 900;//
            }
            TestResult.txapcVal = setVal;

            valbuff[0] = (byte)(setVal >> 8);
            valbuff[1] = (byte)(setVal & 0xFF);

            // 选择表 02
            if (SelectTable(0x80) == false) return false;

            if ((TWI_WritePage(0xa2, 0x98, valbuff, 2)) != 2) return false;

            Thread.Sleep(60); // 延时

            return true;
        }

        public bool SetTxModBias(UInt16 setVal)
        {
            byte[] valmodbuff = new byte[2];

            if (setVal > 1023)
            {
                setVal = 1023;
            }
            TestResult.txmodVal = setVal;

            valmodbuff[0] = (byte)(setVal >> 4);
            valmodbuff[1] = (byte)(setVal << 4);
            //valmodbuff[0] = (byte)(setVal >> 8);
            //valmodbuff[1] = (byte)(setVal & 0xFF);

            // 选择表 02
            if (SelectTable(0x80) == false) return false;

            if ((TWI_WritePage(0xa2, 0x9E, valmodbuff, 2)) != 2) return false;

            Thread.Sleep(60); // 延时

            return true;
        }

        public bool SetRxLos(UInt16 setVal)
        {
            if (setVal > 63) //最大63
            {
                setVal = 63;
            }
            TestResult.rxlosVal = setVal;

            // 选择表 02
            if (SelectTable(0x80) == false) return false;//

            byte[] readbuffer = new byte[2];
            if (TWI_ReadPage(0xA2, 0xD6, readbuffer, 1) != 1)
            {
                return false;
            }
            readbuffer[0] &= 0xC0; //bit7-6
            setVal |= readbuffer[0];

            bool rtnVal = TWI_WriteByte(0xA2, 0xD6, (byte)setVal);//
            Thread.Sleep(60); // 延时
            return rtnVal;
        }

        public bool setAPD(UInt16 setVal)
        {
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

            // 选择表 02
            if (SelectTable(0x81) == false) return 0;//2

            if (TWI_ReadPage(0xA2, 0xE8, readbuffer, 2) == 2)//0xF0
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

        public UInt16 GetTxADC()
        {
            UInt16 txadc = 0;
            byte[] readbuffer = new byte[2];

            // 选择表 02
            if (SelectTable(0x81) == false) return 0;//2

            if (TWI_ReadPage(0xA2, 0xE6, readbuffer, 2) == 2)//0xEE
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

        public bool WriteTxCalData()
        {
            byte[] writeByte = new byte[4];
            byte[] readByte = new byte[4];
            //byte[] calpwr_uw = new byte[2];
            //
            double[] x = new double[5];  //ADC原始值//
            double[] y = new double[5];  //校正值//
            double[] a = new double[5];  //系数//
            double[] dt = new double[5];   //误差//
            for (int i = 0; i < 5; i++)//
                dt[i] = 0.0;//
            byte[] c0 = new byte[2];     //C0 C1 C2 C3 C4 数组 //
            byte[] c1 = new byte[2];//
            //
            float ADC = 0;

            // 选择表 02
            if (SelectTable(0x81) == false) return false;

            // 计算校准参数
            ADC = GetTxADC();
            if (ADC <= 1)
            {
                return false;
            }

            //TestResult.txPwrCal_k = ((float)((Math.Pow(10, TestResult.txPower / 10.0) * 10000.0) / ADC));
            //TestResult.txPwrCal_b = 0;
            //float fk = TestResult.txPwrCal_k;
            //if (fk < 0)   fk = 0;
            //if (fk > 255) fk = 255;
            //writeByte[0] = (byte)fk;
            //writeByte[1] = (byte)((fk - writeByte[0]) * 256);
            //writeByte[2] = 0x00;
            //writeByte[3] = 0x00;

            y[0] = Convert.ToDouble(TestResult.txPower);//
            y[0] = (float)Math.Pow(10, 0.1 * y[0]) * 10000;//
            y[1] = 0;//

            x[0] = Convert.ToDouble(ADC);//
            x[1] = 0;//

            Bit.iapcir(x, y, 2, a, 2, dt);
            double K = a[1];
            K *= 32768;
            if (K < 0) { K = 0; }
            if (K > 65535) { K = 65535; }
            c1[0] = (byte)(((UInt16)K) >> 8);
            c1[1] = (byte)(((UInt16)K));
            c0[0] = 0;
            c0[1] = 0;

            writeByte[0] = c1[0];
            writeByte[1] = c1[1];
            writeByte[2] = c0[0];
            writeByte[3] = c0[1];

            if (TWI_WritePage(0xA2, 0xD0, writeByte, 4) != 4)//
            {
                return false;
            }

            Thread.Sleep(50); // 延时

            if (TWI_ReadPage(0xA2, 0xD0, readByte, 4) != 4)
            {
                return false;
            }

            return (Bit.ByteEquals(writeByte, readByte));
        }

        public bool WriteRxCalData()
        {
            byte[] writeByte = new byte[6];
            byte[] readByte = new byte[6];
            float k, b;
            //
            byte[] c0 = new byte[2];     //C0 C1 C2 C3 C4 数组 //
            byte[] c1 = new byte[2];//
            byte[] c2 = new byte[2];//
            try
            {
                //
                b = TestResult.rxPwrCal_c[0];
                k = TestResult.rxPwrCal_c[1];
                //
                c2[0] = 0x00;
                c2[1] = 0x00;

                writeByte[0] = c2[0];
                writeByte[1] = c2[1];

                k *= 32768;
                if (k < 0) { k = 0; }
                if (k > 65535) { k = 65535; }
                c1[0] = (byte)(((UInt16)k) >> 8);
                c1[1] = (byte)(((UInt16)k));

                writeByte[2] = c1[0];
                writeByte[3] = c1[1];

                if (b < (-65535)) { b = (-65535); }
                if (b > 65535) { b = 65535; }
                Int16 btemp = Convert.ToInt16(b);
                c0 = BitConverter.GetBytes(btemp);
                Array.Reverse(c0);

                writeByte[4] = c0[0];
                writeByte[5] = c0[1];

                b *= 4096;
                if (b < (-32768)) { b = (-32768); }
                if (b > 32767) { b = 32767; }

                //writeByte[4] = (byte)(((UInt16)b) >> 8);
                //writeByte[5] = (byte)(((UInt16)b));
                //writeByte[4] = 0x00;
                //writeByte[5] = 0x00;

                //k *= 32768;
                //if (k < 0) { k = 0; }
                //if (k > 65535) { k = 65535; }
                //writeByte[2] = (byte)(((UInt16)k) >> 8);
                //writeByte[3] = (byte)(((UInt16)k));

                ////b *= 4096;

                //if (b < (-65535)) { b = (-65535); }
                //if (b > 65535) { b = 65535; }
                //Int16 btemp = Convert.ToInt16(b);
                //byte[] c0temp = BitConverter.GetBytes(btemp);

                //writeByte[4] = c0temp[0];
                //writeByte[5] = c0temp[1];






                // 选择表 02
                if (SelectTable(0x81) == false) return false;

                //
                if (TWI_WritePage(0xA2, 0xD4, writeByte, 6) != 6)//
                {
                    return false;
                }

                if (TWI_ReadPage(0xA2, 0xD4, readByte, 6) != 6)//
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return (Bit.ByteEquals(writeByte, readByte));
        }

        public bool SaveRxDataAfterDebug()
        {
            //发送保存命令到 flash
            return WritSaveCmd();
        }

        public bool SaveTxDataAfterDebug()
        {
            byte[] apc = new byte[64];
            byte[] mod = new byte[64];//128

            byte[] apc_read = new byte[64];
            byte[] mod_read = new byte[64];//128

            byte[] A2hLow96 = new byte[96];
            byte[] A2hLow96_read = new byte[96];

            int i, delta_apc, delta_mod, apc_value, mod_value;

            float temp_value = 25;
            int temp_index = 0;

            /////////////////////
            for (i = 0; i < 40; i++)
            {
                A2hLow96[i] = threshold[i];
            }
            for (i = 0; i < 39; i++)//
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
            if (temp_value < -40) return false;

            temp_index = (int)(((float)(temp_value + 40)) / 5f);//2.5

            if (temp_index < 0) temp_index = 0;
            if (temp_index > 32) temp_index = 32;

            // 计算平移量
            delta_apc = TestResult.txapcVal - apclut[temp_index];
            delta_mod = TestResult.txmodVal - modlut[temp_index];

            if (modlut[temp_index] == 0) return false; // 消光比补偿表不能为0

            // 计算比例系数
            float fk = TestResult.txmodVal;
            fk /= modlut[temp_index];
            //

            for (i = 0; i < 32; i++)//64
            {
                apc_value = delta_apc + apclut[i];
                if (apc_value < 0) apc_value = 0;
                if (apc_value > 1023) apc_value = 1023;//
                                                       // apc[i] = Convert.ToByte(apc_value);
                apc[i * 2] = (byte)((apc_value & 0x300) >> 8);//
                apc[i * 2 + 1] = (byte)(apc_value & 0xFF);//

                if (GlobalVarFun.k_lut_flag == true)
                {
                    mod_value = (int)(fk * modlut[i]); // 比例缩放
                }
                else
                {
                    mod_value = delta_mod + modlut[i]; // 等量平移
                }
                if (mod_value < 0) mod_value = 0;
                if (mod_value > 255) mod_value = 255;
                mod[i * 2] = (byte)(mod_value >> 4);
                mod[i * 2 + 1] = (byte)(mod_value << 4);
            }

            if (TWI_WritePage(0xa2, 0x00, A2hLow96, 96) != 96) return false;
            Thread.Sleep(10);
            if (TWI_ReadPage(0xa2, 0x00, A2hLow96_read, 96) != 96) return false;

            if (SelectTable(0x83) == false) return false; //表选择//
            if (TWI_WritePage(0xa2, 0x80, apc, 64) != 64) return false;//
            Thread.Sleep(10);
            if (TWI_ReadPage(0xa2, 0x80, apc_read, 64) != 64) return false;

            if (SelectTable(0x82) == false) return false; //表选择
            if (TWI_WritePage(0xa2, 0xC0, mod, 64) != 64) return false;//128
            Thread.Sleep(10);
            if (TWI_ReadPage(0xa2, 0xC0, mod_read, 64) != 64) return false;//128

            // check sum
            A2hLow96_read[95] = 0;
            for (i = 0; i < 95; i++)
            {
                A2hLow96_read[95] += A2hLow96_read[i];
            }

            // 保存门限0-95  到测试结果
            for (i = 0; i < 96; i++)
            {
                TestResult.flash_data[i + 256] = A2hLow96_read[i]; //
            }
            //

            // 开启自动温补  按照数据库配置
            if (TxTempLookupTableCtrl(true) == false)
            {
                return false;
            }

            if ((Bit.ByteEquals(A2hLow96, A2hLow96_read) == false)
                || (Bit.ByteEquals(apc, apc_read) == false)
                || (Bit.ByteEquals(mod, mod_read) == false))
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
            byte[] regcfg = new byte[128];
            byte[] regcfg_read = new byte[128];

            byte[] regcfg2 = new byte[128];//
            byte[] regcfg2_read = new byte[128];//

            byte[] awflag_en_read = new byte[8];

            byte[] checkbyte_a0 = new byte[2];
            byte[] checkbyte_c0 = new byte[2];

            byte[] PW2 = new byte[4];//

            int i = 0;
            for (i = 0; i < 128; i++)//
            {
                regcfg2[i] = register2[i];//
                regcfg[i] = register[i];
            }

            if (SelectTable(0) == false) return false; // 选择表//

            //PW2[0] = 0xA9;//
            //PW2[1] = 0x54;//
            //PW2[2] = 0x50;//
            //PW2[3] = 0x66;//
            //TWI_WritePage(0xa2, 0x7B, PW2, 4);////载入密码，确认为二级//0x7B//

            Thread.Sleep(50);

            if (SelectTable(0x81) == false) return false; // 选择表//

            // 重要操作 2017.1.12
            if (SelectTable(0x81) == false) return false;
            if (TWI_WriteByte(0xA2, 0x8D, 0x00) == false) return false; // 配置操作EEPROM//0xC7
            Thread.Sleep(50);//
            if (TWI_WriteByte(0xA2, 0x8D, 0x00) == false) return false; // 配置操作EEPROM//0xC7,写两次
            //
            if (TWI_ReadPage(0xA2, 0x80, checkbyte_a0, 1) != 1) return false; // 未初始化为 0x00//
            if (TWI_ReadPage(0xA2, 0x82, checkbyte_c0, 1) != 1) return false; // bit0 = 1 EEPROM正常//

            // 判断模块是否初始化过
            if (checkbyte_a0[0] != 0x00)
            {
                // GN25L95已经初始化过 // 写入调试密码 A9 '0x54' '0x50' '0x66'
                if (SelectTable(0) == false) return false; // 选择表//
                if (TWI_WritePage(0xa2, 0x7B, byte_debug_pwd, 4) != 4) return false;
                if (TWI_ReadPage(0xA2, 0x82, checkbyte_c0, 1) != 1) return false; // bit0 = 1 EEPROM正常
            }

            if (checkbyte_c0[0] == 0x00) return false; // 0xC0 bit0 = 0 模块异常
            //// 开始配置GN25L95 寄存器 ////
            //
            //regcfg[28] = byte_debug_pwd[0]; // PW2
            //regcfg[29] = byte_debug_pwd[1];
            //regcfg[30] = byte_debug_pwd[2];
            //regcfg[31] = byte_debug_pwd[3];
            //
            regcfg[0xA4 - 0xA0] &= 0xFC; // 初始化： 关闭APC、MOD温度补偿//
            //

            // 重要操作
            //if (TWI_WriteByte(0xA2, 0xC7, 0x00) == false) return false; // 配置操作EEPROM//
            //
            if (SelectTable(0x81) == false) return false; // 选择表//
            if (TWI_WritePage(0xA2, 0x80, regcfg2, 128) != 128) return false;//
            Thread.Sleep(100);
            TWI_WriteByte(0xA2, 0xB7, 0);
            if (TWI_ReadPage(0xA2, 0x80, regcfg2_read, 128) != 128) return false;//

            if (SelectTable(0x80) == false) return false; // 选择表//
            if (TWI_WritePage(0xA2, 0x80, regcfg, 128) != 128) return false;//0xA0/62
            Thread.Sleep(100);
            if (TWI_WritePage(0xA2, 0x80, regcfg, 128) != 128) return false;//0xA0/62
            Thread.Sleep(100);
            if (TWI_ReadPage(0xA2, 0x80, regcfg_read, 128) != 128) return false;//0xA0/62

            /////////////////////////////////////////////////////////////////
            regcfg2[28] = byte_debug_pwd[0];// // PW2
            regcfg2[29] = byte_debug_pwd[1];//
            regcfg2[30] = byte_debug_pwd[2];//
            regcfg2[31] = byte_debug_pwd[3];//

            regcfg[6] = regcfg_read[6] = 0x00; //

            regcfg[22] = regcfg_read[22] = 0x00; //
                                                 //regcfg[24] = regcfg_read[24] = 0x00; // APC SET
                                                 //regcfg[25] = regcfg_read[25] = 0x00; // APC SET

            regcfg[26] = regcfg_read[26] = 0x00; // 
            regcfg[27] = regcfg_read[27] = 0x00; // 

            regcfg[28] = regcfg_read[28] = 0x00; // BIAS SET
            regcfg[29] = regcfg_read[29] = 0x00; // BIAS SET

            regcfg[30] = regcfg_read[30] = 0x00; // MOD SET
            regcfg[31] = regcfg_read[31] = 0x00; // MOD SET

            //regcfg[38] = regcfg_read[38] = 0x00; //

            regcfg[73] = regcfg_read[73] = 0x00; //IDAC

            regcfg[86] = regcfg_read[86] = 0x00; // LOS SET

            regcfg2[1] = regcfg2_read[1] = 0x00; //
            regcfg2[3] = regcfg2_read[3] = 0x00; //

            // regcfg2[55] = regcfg2_read[55] = 0x00; //

            regcfg2[70] = regcfg2_read[70] = 0x00; //RSVD
            regcfg2[71] = regcfg2_read[71] = 0x00; //RSVD
            regcfg2[80] = regcfg2_read[80] = 0x00; //RSVD

            regcfg2[81] = regcfg2_read[81] = 0x00; //
            regcfg2[86] = regcfg2_read[86] = 0x00; //
            regcfg2[87] = regcfg2_read[87] = 0x00; //

            regcfg2[96] = regcfg2_read[96] = 0x00; //
            regcfg2[97] = regcfg2_read[97] = 0x00; //
            regcfg2[98] = regcfg2_read[98] = 0x00; //
            regcfg2[99] = regcfg2_read[99] = 0x00; //
            regcfg2[100] = regcfg2_read[100] = 0x00; //
            regcfg2[101] = regcfg2_read[101] = 0x00; //
            regcfg2[102] = regcfg2_read[102] = 0x00; //
            regcfg2[103] = regcfg2_read[103] = 0x00; //
            regcfg2[104] = regcfg2_read[104] = 0x00; //
            regcfg2[105] = regcfg2_read[105] = 0x00; //
            regcfg2[106] = regcfg2_read[106] = 0x00; //
            regcfg2[107] = regcfg2_read[107] = 0x00; //
            regcfg2[108] = regcfg2_read[108] = 0x00; //
            regcfg2[109] = regcfg2_read[109] = 0x00; //
            regcfg2[110] = regcfg2_read[110] = 0x00; //
            regcfg2[111] = regcfg2_read[111] = 0x00; //
            regcfg2[112] = regcfg2_read[112] = 0x00; //
            regcfg2[113] = regcfg2_read[113] = 0x00; //
            regcfg2[114] = regcfg2_read[114] = 0x00; //
            regcfg2[115] = regcfg2_read[115] = 0x00; //
            regcfg2[116] = regcfg2_read[116] = 0x00; //
            regcfg2[117] = regcfg2_read[117] = 0x00; //

            /////////////////////////////////////////////////////////////////

            // 配置并检查 Alarm & Warning Flags Enable
            if (SelectTable(1) == false) return false; // 选择表
            //
            if (TWI_WritePage(0xA2, 0xF8, awflag_en, 8) != 8) return false;
            if (TWI_ReadPage(0xA2, 0xF8, awflag_en_read, 8) != 8) return false;
            //awflag_en_read[2] = awflag_en_read[3] = awflag_en_read[6] = awflag_en_read[7] = 0x00; //未用字节

            //
            if (SoftResetGN25L95() == false) return false; // 软件操作 复位GN25L95
            //

            // 校验数据是否正确
            if (Bit.ByteEquals(regcfg, regcfg_read) && Bit.ByteEquals(regcfg2, regcfg2_read) && Bit.ByteEquals(awflag_en_read, awflag_en))
            {
                return true; // 初始化成功
            }
            else
            {
                //string ss = string.Empty;
                //string tt = string.Empty;
                //for (int x = 0; x < 128; x++)
                //{
                //    if (regcfg[x] != regcfg_read[x])
                //    {
                //        ss += x.ToString() + " ";
                //    }
                //    if (regcfg2[x] != regcfg2_read[x])
                //    {
                //        tt += x.ToString() + " ";
                //    }
                //}
                return false; // 初始化失败
            }
        }

        // 模块发射温度补偿表控制   如果不需要直接返回
        public bool TxTempLookupTableCtrl(bool enable)
        {
            byte value = 0x00;
            // 选择 
            if (SelectTable(0x80) == false) return false;//
            value = TWI_ReadByte(0xa2, 0xE4);
            //
            if (enable)
            {
                if (TWI_WriteByte(0xa2, 0xE4, register[0xE4 - 0x80]) == false) return false; // 按照数据库配置 开启温补//
                //if (TWI_WriteByte(0xa2, 0xC4, register[0xC4 - 0xA0]) == false) return false; // WRITE_ACCESS Registers 写入数据库配置
            }
            else
            {
                value = (byte)(value & 0xFC);//0x07
                if (TWI_WriteByte(0xA2, 0xE4, value) == false) return false; // 关闭温补
                //if (TWI_WriteByte(0xA2, 0xC4, 19) == false) return false; // WRITE_ACCESS Registers 允许写入告警门限
            }
            //
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
            return TWI_WriteByte(0xA2, 0x7F, tbl);
        }

        // 写入软件复位密码  复位GN25L95
        private bool SoftResetGN25L95()
        {
            byte[] reset_pwd = new byte[5];
            reset_pwd[0] = 0x5D;
            reset_pwd[1] = 0x2C;
            reset_pwd[2] = 0x6A;
            reset_pwd[3] = 0xC9;
            reset_pwd[4] = 0x8B;//
            //if (TWI_WritePage(0xa2, 0x7B, reset_pwd, 5) == 5)
            //{
            //    Thread.Sleep(500); // 延时
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

            TWI_WritePage(0xA2, 0x7B, reset_pwd, 5);
            Thread.Sleep(500); // 延时
            if ((TWI_ReadPage(0xa2, 0x7F, reset_pwd, 1) != 1)) return false;

            if (reset_pwd[0] == 0x00)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool EEPROMcheckSum()
        {
            return true;
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
        /// TWI 单次页读 (private)。单次I2C事务，最多读8字节，避免len>=10时十进制字符串与十六进制混淆。
        /// </summary>
        private int TWI_ReadPageRaw(int deviceAddr, int regAddr, byte[] buf, int len)
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
        /// TWI 单次页写 (private)。单次I2C事务，最多写8字节，避免len>=10时十进制字符串与十六进制混淆。
        /// </summary>
        private int TWI_WritePageRaw(int deviceAddr, int regAddr, byte[] buf, int len)
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
        /// TWI 多字节读 (public)。自动分页，每次最多读8字节，支持任意长度。deviceAddr/regAddr 支持0xA2或162等int字面量。
        /// </summary>
        public int TWI_ReadPage(int deviceAddr, int regAddr, byte[] buf, int len)
        {
            const int pageSize = 8;
            int totalRead = 0;
            int offset = 0;
            int curReg = regAddr & 0xFF;

            while (offset < len)
            {
                int chunkLen = len - offset;
                if (chunkLen > pageSize) chunkLen = pageSize;

                byte[] chunkBuf = new byte[chunkLen];
                int n = TWI_ReadPageRaw(deviceAddr, curReg, chunkBuf, chunkLen);
                if (n <= 0) break;

                Array.Copy(chunkBuf, 0, buf, offset, n);
                totalRead += n;
                offset += n;
                curReg = (curReg + n) & 0xFF;

                if (n < chunkLen) break;
            }
            return totalRead;
        }

        /// <summary>
        /// TWI 多字节写 - byte[]版本 (public)。自动分页，每次最多写8字节，支持任意长度。数据字节用逗号分隔，不带0x前缀。
        /// </summary>
        public int TWI_WritePage(int deviceAddr, int regAddr, byte[] buf, int len)
        {
            const int pageSize = 8;
            int totalWritten = 0;
            int offset = 0;
            int curReg = regAddr & 0xFF;

            while (offset < len)
            {
                int chunkLen = len - offset;
                if (chunkLen > pageSize) chunkLen = pageSize;

                byte[] chunkBuf = new byte[chunkLen];
                Array.Copy(buf, offset, chunkBuf, 0, chunkLen);

                int n = TWI_WritePageRaw(deviceAddr, curReg, chunkBuf, chunkLen);
                if (n <= 0) break;

                totalWritten += n;
                offset += n;
                curReg = (curReg + n) & 0xFF;

                if (n < chunkLen) break;
                Thread.Sleep(5);
            }
            return totalWritten;
        }

        /// <summary>
        /// TWI 单字节写 - int值便捷重载 (public)。等效于new byte[1]{(byte)value}，len=1。
        /// </summary>
        public int TWI_WritePage(int deviceAddr, int regAddr, int value, int length)
        {
            byte[] data = new byte[1] { (byte)value };
            return TWI_WritePageRaw(deviceAddr, regAddr, data, 1);
        }

        /// <summary>
        /// TWI 读单字节 (public)
        /// </summary>
        public byte TWI_ReadByte(int deviceAddr, int regAddr)
        {
            byte[] b = new byte[1];
            if (TWI_ReadPageRaw(deviceAddr, regAddr, b, 1) == 1) return b[0];
            return 0;
        }

        /// <summary>
        /// TWI 写单字节 (public)。val支持传入0x03或3等int/byte字面量。
        /// </summary>
        public bool TWI_WriteByte(int deviceAddr, int regAddr, int val)
        {
            byte[] b = new byte[] { (byte)val };
            return TWI_WritePageRaw(deviceAddr, regAddr, b, 1) == 1;
        }
        #endregion

        /*#region TWI底层通信方法(适配SFP_EVB_Heater，支持十六进制/十进制参数，public可外部调用)

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
        #endregion*/
        //////////////////////////////////////////////////////////////////////////////////////////////////////
    }
}


