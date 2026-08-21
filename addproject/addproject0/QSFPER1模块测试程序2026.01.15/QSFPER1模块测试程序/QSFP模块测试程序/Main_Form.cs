using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fibertower_Common;
using System.Threading;
using System.Data.OleDb;
using System.Diagnostics;
using System.Data.SqlClient;
using System.IO.Ports;
using System.IO;
using System.Management;
using System.Globalization;

namespace XFP模块测试程序
{
    public partial class Main_Form : Form
    {
        I2C i2c;
        SqlConnection sqlconnection;
        OleDbConnection dbconnect;
        OleDbCommand dbcommand;
        OleDbDataAdapter dbadapter;
        DataSet dbset;
        SerialPort PSSOPSIX;
        private TestQSFPER1 er1Test;
        public TestQSFPER1 Er1Test { get { return er1Test; } }
        //float bias_min, bias_max;
        public static bool debugMode = false;
        public static byte[] tap_L = new byte[22];
        public static byte[] tap_H = new byte[22];
        public static byte dsp_pol = 17;
        public Main_Form()
        {
            InitializeComponent();
            if (GetRegisterInfo() == false)
            {
                //Application.Exit();
                Environment.Exit(0);
            }
        }

        private bool GetRegisterInfo()
        {
            return true;
        }

        // Call this after the four instrument ports have been configured and opened.
        // The eyeReader must call the real DCA driver and return TDECQ/Outer ER.
        public void InitializeEr1Test(SerialPort meterPort,
            OpticalPowerMeterProtocol meterProtocol, SerialPort attenuatorPort,
            SerialPort berPort, string berChannel, EyeDataReader eyeReader)
        {
            if (i2c == null)
                throw new InvalidOperationException("I2C 尚未连接。");

            er1Test = new TestQSFPER1();
            er1Test.Log = delegate(string message)
            {
                Debug.WriteLine("[QSFPER1] " + message);
            };
            er1Test.Init(i2c,
                new SerialPowerMeterAdapter(meterPort, meterProtocol, 1, 300),
                new SerialAttenuatorAdapter(attenuatorPort, 60, 0),
                new PssBerAnalyzer(berPort, berChannel, 1000, ParsePssBer),
                new DelegateEyeAnalyzer(eyeReader));
        }

        private double ParsePssBer(string status)
        {
            // The original QSFP program reads the BER at offset 22, length 11.
            // Keep the strict check so a changed instrument format cannot pass silently.
            if (status == null || status.Length < 33)
                throw new FormatException("PSS BER 返回字符串长度不足: " + status);

            string field = status.Substring(22, 11).Trim();
            double ber;
            if (!double.TryParse(field, NumberStyles.Float,
                CultureInfo.InvariantCulture, out ber))
                throw new FormatException("PSS BER 字段无法解析: " + field);
            return ber;
        }

        //在事件中接收I2C_Form发送的信息
        public void s_OnSendMsg(I2C i2c, SqlConnection sqlconnection)
        {
            this.i2c = i2c;
            this.sqlconnection = sqlconnection;

            i2c.TWI_Open();
            timer1.Start();
        }

        private void Main_Form_Load(object sender, EventArgs e)
        {
            //光开关
            string[] portnames = SerialPort.GetPortNames();
            Array.Sort(portnames); //已存在串口更新
            for (int i = 0; i < portnames.Length; i++)
            {
                OPSIX_comboBox.Items.Add(portnames[i]);
            }
            if (OPSIX_comboBox.Items.Count > 0)
            {
                OPSIX_comboBox.SelectedIndex = 0;
            }
            //初始化界面控件
            foreach (Control control in Alarm_Warning_groupBox.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox picturebox = control as PictureBox;
                    picturebox.Image = imageList1.Images["LedNone.ico"];
                }
            }
            foreach (Control control in control_groupBox.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox picturebox = control as PictureBox;
                    picturebox.Image = imageList1.Images["LedNone.ico"];
                }
            }
            foreach (Control control in groupBox2.Controls)
            {
                if (control is PictureBox)
                {
                    PictureBox picturebox = control as PictureBox;
                    picturebox.Image = imageList1.Images["LedNone.ico"];
                }
            }

            //
            Temp_value_textBox.Text = Temp_trackBar.Value.ToString();
            float vtemp = (float)(Temp_trackBar.Value * (2.5 / 4095));
            Temp_degree_textBox.Text = VoltagetoTemperature(vtemp).ToString("F2") + "℃";
            //
            TempSOA_value_textBox.Text = TempSOA_trackBar.Value.ToString();
            vtemp = (float)(TempSOA_trackBar.Value * (2.5 / 4095));
            TempSOA_degree_textBox.Text = VoltagetoTemperature(vtemp).ToString("F2") + "℃";
            //

            //
            chipSel_comboBox.Items.Clear();
            chipSel_comboBox.Items.Add("0");
            chipSel_comboBox.Items.Add("1");
            chipSel_comboBox.Items.Add("2");
            chipSel_comboBox.Items.Add("3");
            chipSel_comboBox.SelectedIndex = 0;
            //

            channelSel_comboBox.Items.Clear();
            channelSel_comboBox.Items.Add("0");
            channelSel_comboBox.Items.Add("1");
            channelSel_comboBox.Items.Add("2");
            channelSel_comboBox.Items.Add("3");
            channelSel_comboBox.SelectedIndex = 0;
            //
            
            //更新模块型号列表
            try
            {
                dbconnect = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source= C:\\Fibertop\\QSFP\\QSFPER1AutoSet.mdb");
                dbcommand = new OleDbCommand("select Type from ModuleType", dbconnect);
                dbadapter = new OleDbDataAdapter(dbcommand);
                dbset = new DataSet();
                dbadapter.Fill(dbset, "ModuleType");
                foreach (DataRow dataRow in dbset.Tables["ModuleType"].Rows)
                {
                    if (dataRow["Type"].ToString() != "")
                        Module_Type_comboBox.Items.Add(dataRow["Type"]);
                }
                Module_Type_comboBox.SelectedIndex = 0;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
                Application.Exit();
            }
        }

        private void Main_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sqlconnection != null)
                sqlconnection.Close();
            if (dbconnect != null)
                dbconnect.Close();
            if (i2c != null)
                i2c.TWI_Close();
        }

        //温度计算
        private float VoltagetoTemperature(float Voltage)
        {
            if (Voltage >= 2.5)
                Voltage = 2.4999f;
            if (Voltage <= 0)
                Voltage = 0.0001f;

            float Rt = (float)(10000 * Voltage / (2.5 - Voltage));
            float temperature = (float)(1 / (Math.Log((Rt / 10000)) / 3900 + (1 / 298.15)) - 273.15);
            return temperature;
        }

        //设置图片框控件
        private void SetRedLED(PictureBox picbox, bool bit_value)
        {
            if (bit_value)
                picbox.Image = imageList1.Images["LedRed.ico"];
            else
                picbox.Image = imageList1.Images["LedGreen.ico"];
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Write_button.Enabled == false)
            {
                return;
            }

            Read_ModuleSN(); //Read SN 2024.3.27//
            //
            Read_Flags_and_Interrupt();
            Converted_analog_values();
            General_Control_Status_Bits();
            Update_TX_status();

            if (debugMode == true)
            {
                byte[] readbuffer = new byte[8];
                byte i = Convert.ToByte(channelSel_comboBox.Text);
                i *= 2;
                i += 0xE0;
                SelectTable(6);
                i2c.TWI_ReadPage(0xa0, i, readbuffer, 2);
                txadc_textbox.Text = (readbuffer[0] * 256 + readbuffer[1]).ToString();

                i += 8;
                i2c.TWI_ReadPage(0xa0, i, readbuffer, 2);
                rxadc_textbox.Text = (readbuffer[0] * 256 + readbuffer[1]).ToString();

                //2023.11.10
                string strtmp = "";
                i2c.TWI_ReadPage(0xa0, 0xE8, readbuffer, 8);
                strtmp  = (readbuffer[0] * 256 + readbuffer[1]).ToString() + " ";
                strtmp += (readbuffer[2] * 256 + readbuffer[3]).ToString() + "\r\n";
                strtmp += (readbuffer[4] * 256 + readbuffer[5]).ToString() + " ";
                strtmp += (readbuffer[6] * 256 + readbuffer[7]).ToString();
                rxadc4ch_textbox.Text = strtmp;
            }
            else
            {
                txadc_textbox.Text = "0";
                rxadc_textbox.Text = "0";
            }
        }

        //读取模块SN
        private void Read_ModuleSN()
        {
            byte[] readbuffer = new byte[16];
            UInt64 iFsn = 0;

            try
            {
                SelectTable(0);
                i2c.TWI_ReadPage(0xa0, 128 + 68, readbuffer, 16);
                string qsfp_sn = System.Text.Encoding.ASCII.GetString(readbuffer, 0, 16);
                sn_textBox.Text = qsfp_sn.TrimEnd();

                //Fsn飞思卓产品内部流水号
                SelectTable(6);
                i2c.TWI_ReadPage(0xa0, 0x92, readbuffer, 5);
                iFsn = 0;
                iFsn += readbuffer[0];
                iFsn <<= 8;
                iFsn += readbuffer[1];
                iFsn <<= 8;
                iFsn += readbuffer[2];
                iFsn <<= 8;
                iFsn += readbuffer[3];
                iFsn <<= 8;
                iFsn += readbuffer[4];
                if (iFsn > 999999999999) iFsn = 999999999999; // 10进制12位
                fsn_textBox1.Text = iFsn.ToString("D12");
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void APC_trackBar_ValueChanged(object sender, EventArgs e)
        {
            APC_value_textBox.Text = APC_trackBar.Value.ToString();
            //float vpd = (float)(APC_trackBar.Value * 0.6103515625);
            //APC_mV_textBox.Text = vpd.ToString("F2") + "mV";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xA0;

            i2c.TWI_WriteByte(0xa0, i, (byte)APC_trackBar.Value);
        }

        private void Mod_trackBar_ValueChanged(object sender, EventArgs e)
        {
            Mod_value_textBox.Text = Mod_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xA4;

            i2c.TWI_WriteByte(0xa0, i, (byte)Mod_trackBar.Value);
        }

        private void TxCPA_trackBar_ValueChanged(object sender, EventArgs e)
        {
            TxCPA_value_textBox.Text = TxCPA_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xA8;

            i2c.TWI_WriteByte(0xa0, i, (byte)TxCPA_trackBar.Value);
        }

        private void TxPE_trackBar_ValueChanged(object sender, EventArgs e)
        {
            TxPE_value_textBox.Text = TxPE_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xAC;

            i2c.TWI_WriteByte(0xa0, i, (byte)TxPE_trackBar.Value);
        }
        
        private void TxSwing_trackBar_ValueChanged(object sender, EventArgs e)
        {
            TxSwing_value_textBox.Text = TxSwing_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xC4;

            i2c.TWI_WriteByte(0xa0, i, (byte)TxSwing_trackBar.Value);
        }


        private void TxCtrl1_trackBar_ValueChanged(object sender, EventArgs e)
        {
            TxCtrl1_value_textBox.Text = TxCtrl1_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xD0;

            i2c.TWI_WriteByte(0xa0, i, (byte)TxCtrl1_trackBar.Value);
        }

        private void TxCtrl2_trackBar_ValueChanged(object sender, EventArgs e)
        {
            TxCtrl2_value_textBox.Text = TxCtrl2_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xD4;

            i2c.TWI_WriteByte(0xa0, i, (byte)TxCtrl2_trackBar.Value);
        }

        private void Temp_trackBar_ValueChanged(object sender, EventArgs e)
        {
            Temp_value_textBox.Text = Temp_trackBar.Value.ToString();

            float vtemp = (float)(Temp_trackBar.Value * (2.5 / 4095));
            Temp_degree_textBox.Text = VoltagetoTemperature(vtemp).ToString("F2") + "℃";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte[] writebuffer = BitConverter.GetBytes((UInt16)Temp_trackBar.Value);
            Array.Reverse(writebuffer); //高字节在前
            i2c.TWI_WritePage(0xA0, 0xCC, writebuffer, 2);
        }

        private void Von_trackBar_ValueChanged(object sender, EventArgs e)
        {
            Von_value_textBox.Text = Von_trackBar.Value.ToString();
            float von = -(float)(Von_trackBar.Value * (2.5 / 255));
            Von_V_textBox.Text = von.ToString("F2") + "V";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xC8;

            i2c.TWI_WriteByte(0xa0, i, (byte)Von_trackBar.Value);
        }

        private void RxSwing_trackBar_ValueChanged(object sender, EventArgs e)
        {
            RxSwing_value_textBox.Text = RxSwing_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xB0;

            i2c.TWI_WriteByte(0xa0, i, (byte)RxSwing_trackBar.Value);
        }

        private void LOS_trackBar_ValueChanged(object sender, EventArgs e)
        {
            LOS_value_textBox.Text = LOS_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xB4;

            i2c.TWI_WriteByte(0xa0, i, (byte)LOS_trackBar.Value);
        }

        private void RxPE_trackBar_ValueChanged(object sender, EventArgs e)
        {
            RxPE_value_textBox.Text = RxPE_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xB8;

            i2c.TWI_WriteByte(0xa0, i, (byte)RxPE_trackBar.Value);
        }

        private void APD_trackBar_ValueChanged(object sender, EventArgs e)
        {
            APD_value_textBox.Text = APD_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xBC;

            i2c.TWI_WriteByte(0xa0, i, (byte)APD_trackBar.Value);
        }

        private void Read_Flags_and_Interrupt()
        {
            byte[] temp = new byte[19];
            byte[] mask = new byte[19];
            byte[] mask3 = new byte[10];
            byte[] tecdac = new byte[8];
            byte[] soalos = new byte[2];

            if (i2c.TWI_ReadPage(0xa0, 0, temp, 19) != 19)
            {
                //Alarm_textBox.Text = "";
                //Masking_Alarm_textBox.Text = "";
                //Masking3_Alarm_textBox.Text = "";
                foreach (Control control in control_groupBox.Controls)
                {
                    if (control is PictureBox)
                    {
                        PictureBox picturebox = control as PictureBox;
                        picturebox.Image = imageList1.Images["LedNone.ico"];
                    }
                }
                return;
            }

            i2c.TWI_ReadPage(0xa0, 86, mask, 19);
            SelectTable(6);
            i2c.TWI_ReadPage(0xa0, 151, soalos, 1); //soa_los
            i2c.TWI_ReadPage(0xa0, 216, tecdac, 6); //tec for tosa+soa
            SelectTable(3);
            i2c.TWI_ReadPage(0xa0, 242, mask3, 10);

            soa_los_textBox.Text = string.Format("{0}", soalos[0].ToString("X02"));
            page06_tecdac_textBox.Text = string.Format("{0} {1} {2} {3} {4} {5}", tecdac[0].ToString("X02"), tecdac[1].ToString("X02"), tecdac[2].ToString("X02"), tecdac[3].ToString("X02"), tecdac[4].ToString("X02"), tecdac[5].ToString("X02"));

            Alarm_textBox.Text = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18}",
                temp[0].ToString("X02"),   temp[1].ToString("X02"), temp[2].ToString("X02"),  temp[3].ToString("X02"),  temp[4].ToString("X02"), 
                temp[5].ToString("X02"),   temp[6].ToString("X02"), temp[7].ToString("X02"),  temp[8].ToString("X02"),  temp[9].ToString("X02"), 
                temp[10].ToString("X02"), temp[11].ToString("X02"), temp[12].ToString("X02"), temp[13].ToString("X02"), temp[14].ToString("X02"),
                temp[15].ToString("X02"), temp[16].ToString("X02"), temp[17].ToString("X02"), temp[18].ToString("X02"));
            Masking_Alarm_textBox.Text = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18}",
                mask[0].ToString("X02"),  mask[1].ToString("X02"),  mask[2].ToString("X02"),  mask[3].ToString("X02"),  mask[4].ToString("X02"), 
                mask[5].ToString("X02"),  mask[6].ToString("X02"),  mask[7].ToString("X02"),  mask[8].ToString("X02"),  mask[9].ToString("X02"),
                mask[10].ToString("X02"), mask[11].ToString("X02"), mask[12].ToString("X02"), mask[13].ToString("X02"), mask[14].ToString("X02"),
                mask[15].ToString("X02"), mask[16].ToString("X02"), mask[17].ToString("X02"), mask[18].ToString("X02"));
            Masking3_Alarm_textBox.Text = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}",
                mask3[0].ToString("X02"), mask3[1].ToString("X02"), mask3[2].ToString("X02"), mask3[3].ToString("X02"), mask3[4].ToString("X02"),
                mask3[5].ToString("X02"), mask3[6].ToString("X02"), mask3[7].ToString("X02"), mask3[8].ToString("X02"), mask3[9].ToString("X02"));

            int i = Convert.ToByte(channelSel_comboBox.Text);

            SetRedLED(Temp_HA_pictureBox, Bit.GetBit(temp[6], 7));
            SetRedLED(Temp_LA_pictureBox, Bit.GetBit(temp[6], 6));
            SetRedLED(VCC_HA_pictureBox, Bit.GetBit(temp[7], 7));
            SetRedLED(VCC_LA_pictureBox, Bit.GetBit(temp[7], 6));
            //
            SetRedLED(Temp_HW_pictureBox, Bit.GetBit(temp[6], 5));
            SetRedLED(Temp_LW_pictureBox, Bit.GetBit(temp[6], 4));
            SetRedLED(VCC_HW_pictureBox, Bit.GetBit(temp[7], 5));
            SetRedLED(VCC_LW_pictureBox, Bit.GetBit(temp[7], 4));
            //
            if (i == 0)
            {
                SetRedLED(RxPWR_HA_pictureBox, Bit.GetBit(temp[9], 7));
                SetRedLED(RxPWR_LA_pictureBox, Bit.GetBit(temp[9], 6));
                SetRedLED(Bias_HA_pictureBox, Bit.GetBit(temp[11], 7));
                SetRedLED(Bias_LA_pictureBox, Bit.GetBit(temp[11], 6));
                SetRedLED(TxPWR_HA_pictureBox, Bit.GetBit(temp[13], 7));
                SetRedLED(TxPWR_LA_pictureBox, Bit.GetBit(temp[13], 6));
                //
                SetRedLED(RxPWR_HW_pictureBox, Bit.GetBit(temp[9], 5));
                SetRedLED(RxPWR_LW_pictureBox, Bit.GetBit(temp[9], 4));
                SetRedLED(Bias_HW_pictureBox, Bit.GetBit(temp[11], 5));
                SetRedLED(Bias_LW_pictureBox, Bit.GetBit(temp[11], 4));
                SetRedLED(TxPWR_HW_pictureBox, Bit.GetBit(temp[13], 5));
                SetRedLED(TxPWR_LW_pictureBox, Bit.GetBit(temp[13], 4));
            }
            else if (i == 1)
            {
                SetRedLED(RxPWR_HA_pictureBox, Bit.GetBit(temp[9], 3));
                SetRedLED(RxPWR_LA_pictureBox, Bit.GetBit(temp[9], 2));
                SetRedLED(Bias_HA_pictureBox, Bit.GetBit(temp[11], 3));
                SetRedLED(Bias_LA_pictureBox, Bit.GetBit(temp[11], 2));
                SetRedLED(TxPWR_HA_pictureBox, Bit.GetBit(temp[13], 3));
                SetRedLED(TxPWR_LA_pictureBox, Bit.GetBit(temp[13], 2));
                //
                SetRedLED(RxPWR_HW_pictureBox, Bit.GetBit(temp[9], 1));
                SetRedLED(RxPWR_LW_pictureBox, Bit.GetBit(temp[9], 0));
                SetRedLED(Bias_HW_pictureBox, Bit.GetBit(temp[11], 1));
                SetRedLED(Bias_LW_pictureBox, Bit.GetBit(temp[11], 0));
                SetRedLED(TxPWR_HW_pictureBox, Bit.GetBit(temp[13], 1));
                SetRedLED(TxPWR_LW_pictureBox, Bit.GetBit(temp[13], 0));
            }
            else if (i == 2)
            {
                SetRedLED(RxPWR_HA_pictureBox, Bit.GetBit(temp[10], 7));
                SetRedLED(RxPWR_LA_pictureBox, Bit.GetBit(temp[10], 6));
                SetRedLED(Bias_HA_pictureBox, Bit.GetBit(temp[12], 7));
                SetRedLED(Bias_LA_pictureBox, Bit.GetBit(temp[12], 6));
                SetRedLED(TxPWR_HA_pictureBox, Bit.GetBit(temp[14], 7));
                SetRedLED(TxPWR_LA_pictureBox, Bit.GetBit(temp[14], 6));
                //
                SetRedLED(RxPWR_HW_pictureBox, Bit.GetBit(temp[10], 5));
                SetRedLED(RxPWR_LW_pictureBox, Bit.GetBit(temp[10], 4));
                SetRedLED(Bias_HW_pictureBox, Bit.GetBit(temp[12], 5));
                SetRedLED(Bias_LW_pictureBox, Bit.GetBit(temp[12], 4));
                SetRedLED(TxPWR_HW_pictureBox, Bit.GetBit(temp[14], 5));
                SetRedLED(TxPWR_LW_pictureBox, Bit.GetBit(temp[14], 4));
            }
            else if (i == 3)
            {
                SetRedLED(RxPWR_HA_pictureBox, Bit.GetBit(temp[10], 3));
                SetRedLED(RxPWR_LA_pictureBox, Bit.GetBit(temp[10], 2));
                SetRedLED(Bias_HA_pictureBox, Bit.GetBit(temp[12], 3));
                SetRedLED(Bias_LA_pictureBox, Bit.GetBit(temp[12], 2));
                SetRedLED(TxPWR_HA_pictureBox, Bit.GetBit(temp[14], 3));
                SetRedLED(TxPWR_LA_pictureBox, Bit.GetBit(temp[14], 2));
                //
                SetRedLED(RxPWR_HW_pictureBox, Bit.GetBit(temp[10], 1));
                SetRedLED(RxPWR_LW_pictureBox, Bit.GetBit(temp[10], 0));
                SetRedLED(Bias_HW_pictureBox, Bit.GetBit(temp[12], 1));
                SetRedLED(Bias_LW_pictureBox, Bit.GetBit(temp[12], 0));
                SetRedLED(TxPWR_HW_pictureBox, Bit.GetBit(temp[14], 1));
                SetRedLED(TxPWR_LW_pictureBox, Bit.GetBit(temp[14], 0));
            }
            else
            {
                //
            }
        }

        //更新监控
        private void Converted_analog_values()
        {
            byte[] ReadBuffer = new byte[16];
            float ftemp = 0;

            if (i2c.TWI_ReadPage(0xa0, 22, ReadBuffer, 12) != 12)
            {
                foreach (Control control in groupBox1.Controls)
                {
                    if (control is TextBox)
                    {
                        TextBox textBox = control as TextBox;
                        textBox.Text = "0";
                    }
                }
                return;
            }

            sbyte i = (sbyte)ReadBuffer[0];
            int j = Convert.ToInt32(i);
            Temp_textBox.Text = (j + ReadBuffer[1] * 1 / 256.0).ToString("F2");
            VCC_textBox.Text = ((ReadBuffer[4] * 256 + ReadBuffer[5]) / 10000.0).ToString("F2");

            ftemp = (float)(((sbyte)ReadBuffer[8] * 256 + ReadBuffer[9]) / 256.0f);
            Vtemp_textBox.Text = ftemp.ToString("F2") + "℃";

            ftemp = (float)(((sbyte)ReadBuffer[10] * 256 + ReadBuffer[11]) / 10.0f);
            TEC_Bias_textBox.Text = ftemp.ToString() + "mA";

            //ROSA SOA TEC //2023.10.20
            //////////////////////////////////////////////////////////////////////////////////////
            ftemp = (float)(((sbyte)ReadBuffer[2] * 256 + ReadBuffer[3]) / 256.0f);
            VtempSOA_textBox.Text = ftemp.ToString("F2") + "℃";

            ftemp = (float)(((sbyte)ReadBuffer[6] * 256 + ReadBuffer[7]) / 10.0f);
            TEC_BiasSOA_textBox.Text = ftemp.ToString() + "mA";
            //////////////////////////////////////////////////////////////////////////////////////


            if (i2c.TWI_ReadPage(0xa0, 34, ReadBuffer, 16) != 16)
            {
                foreach (Control control in groupBox1.Controls)
                {
                    if (control is TextBox)
                    {
                        TextBox textBox = control as TextBox;
                        textBox.Text = "0";
                    }
                }
                return;
            }

            if (ReadBuffer[0] == 0 && ReadBuffer[1] == 0)
                ReadBuffer[1] = 1;

            if (ReadBuffer[2] == 0 && ReadBuffer[3] == 0)
                ReadBuffer[3] = 1;

            if (ReadBuffer[4] == 0 && ReadBuffer[5] == 0)
                ReadBuffer[5] = 1;

            if (ReadBuffer[6] == 0 && ReadBuffer[7] == 0)
                ReadBuffer[7] = 1;

            Bias_textBox.Text  = ((ReadBuffer[8] * 256 + ReadBuffer[9]) / 500.0).ToString("F2");
            Bias_textBox.Text += "/";
            Bias_textBox.Text += ((ReadBuffer[10] * 256 + ReadBuffer[11]) / 500.0).ToString("F2");
            Bias_textBox.Text += "/";
            Bias_textBox.Text += ((ReadBuffer[12] * 256 + ReadBuffer[13]) / 500.0).ToString("F2");
            Bias_textBox.Text += "/";
            Bias_textBox.Text += ((ReadBuffer[14] * 256 + ReadBuffer[15]) / 500.0).ToString("F2");
            //
            RxPWR_textBox.Text  = (10 * Math.Log10((ReadBuffer[0] * 256 + ReadBuffer[1]) / 10000.0)).ToString("F1");
            RxPWR_textBox.Text += "/";
            RxPWR_textBox.Text += (10 * Math.Log10((ReadBuffer[2] * 256 + ReadBuffer[3]) / 10000.0)).ToString("F1");
            RxPWR_textBox.Text += "/";
            RxPWR_textBox.Text += (10 * Math.Log10((ReadBuffer[4] * 256 + ReadBuffer[5]) / 10000.0)).ToString("F1");
            RxPWR_textBox.Text += "/";
            RxPWR_textBox.Text += (10 * Math.Log10((ReadBuffer[6] * 256 + ReadBuffer[7]) / 10000.0)).ToString("F1");

            if (i2c.TWI_ReadPage(0xa0, 50, ReadBuffer, 8) != 8)
            {
                foreach (Control control in groupBox1.Controls)
                {
                    if (control is TextBox)
                    {
                        TextBox textBox = control as TextBox;
                        textBox.Text = "0";
                    }
                }
                return;
            }

            if (ReadBuffer[0] == 0 && ReadBuffer[1] == 0)
                ReadBuffer[1] = 1;

            if (ReadBuffer[2] == 0 && ReadBuffer[3] == 0)
                ReadBuffer[3] = 1;

            if (ReadBuffer[4] == 0 && ReadBuffer[5] == 0)
                ReadBuffer[5] = 1;

            if (ReadBuffer[6] == 0 && ReadBuffer[7] == 0)
                ReadBuffer[7] = 1;

            TxPWR_textBox.Text  = (10 * Math.Log10((ReadBuffer[0] * 256 + ReadBuffer[1]) / 10000.0)).ToString("F2");
            TxPWR_textBox.Text += "/";
            TxPWR_textBox.Text += (10 * Math.Log10((ReadBuffer[2] * 256 + ReadBuffer[3]) / 10000.0)).ToString("F2");
            TxPWR_textBox.Text += "/";
            TxPWR_textBox.Text += (10 * Math.Log10((ReadBuffer[4] * 256 + ReadBuffer[5]) / 10000.0)).ToString("F2");
            TxPWR_textBox.Text += "/";
            TxPWR_textBox.Text += (10 * Math.Log10((ReadBuffer[6] * 256 + ReadBuffer[7]) / 10000.0)).ToString("F2");

            ftemp  = (float)(ReadBuffer[0] * 256 + ReadBuffer[1]);
            ftemp += (float)(ReadBuffer[2] * 256 + ReadBuffer[3]);
            ftemp += (float)(ReadBuffer[4] * 256 + ReadBuffer[5]);
            ftemp += (float)(ReadBuffer[6] * 256 + ReadBuffer[7]);
            ftemp = (float)(10 * Math.Log10(ftemp / 10000.0));
            label67_txPwr.Text = ftemp.ToString("F2");
        }

        //更新告警
        private void General_Control_Status_Bits()
        {
            byte[] status = new byte[4];
            byte[] status1 = new byte[2];

            if (i2c.TWI_ReadPage(0xa0, 2, status, 4) != 4)
            {
                foreach (Control control in Alarm_Warning_groupBox.Controls)
                {
                    if (control is PictureBox)
                    {
                        PictureBox picturebox = control as PictureBox;
                        picturebox.Image = imageList1.Images["LedNone.ico"];
                    }
                }
                return;
            }

            SetRedLED(Data_Not_Read_pictureBox, Bit.GetBit(status[0], 0));
            SetRedLED(Interrupt_pictureBox, !Bit.GetBit(status[0], 1));

            byte i = Convert.ToByte(channelSel_comboBox.Text);

            SetRedLED(RX_LOS_pictureBox, Bit.GetBit(status[1], i));
            SetRedLED(TX_LOS_pictureBox, Bit.GetBit(status[1], i+4));
            SetRedLED(TX_Fault_pictureBox, Bit.GetBit(status[2], i));
            SetRedLED(TX_CDR_LOL_pictureBox, Bit.GetBit(status[3], i+4));
            SetRedLED(RX_CDR_LOL_pictureBox, Bit.GetBit(status[3], i));

            i2c.TWI_ReadPage(0xa0, 86, status1, 1); //read tx dis
            SetRedLED(TX_Disable_pictureBox, Bit.GetBit(status1[0], i));

            i2c.TWI_ReadPage(0xa0, 93, status1, 1); //read Lp mode
            if ((status1[0] & 0x03) != 0x00)
            {
                SetRedLED(LpMode_pictureBox, true);
            }
            else
            {
                SetRedLED(LpMode_pictureBox, false);
            }

            i2c.TWI_ReadPage(0xa0, 98, status1, 1); //read tx rx cdr dis
            SetRedLED(TXCDR_Disable_pictureBox, !Bit.GetBit(status1[0], i + 4)); //tx cdr
            SetRedLED(RXCDR_Disable_pictureBox, !Bit.GetBit(status1[0], i));   //rx cdr

            /*SetRedLED(MOD_NR_pictureBox, Bit.GetBit(status[0], 5));
            SetRedLED(P_Down_pictureBox, Bit.GetBit(status[0], 4));
            SetRedLED(TX_NR_pictureBox, Bit.GetBit(status[1], 7));
            SetRedLED(RX_NR_pictureBox, Bit.GetBit(status[1], 4));*/
        }

        //更新模块状态
        private void Update_TX_status()
        {
            byte[] temp = new byte[4];

            SelectTable(0xB0);
            i2c.TWI_ReadPage(0xa0, 0xE8, temp, 1);
            SetRedLED(pictureBox_FEC_En, !Bit.GetBit(temp[0], 0));//2025.07.22

            SelectTable(6); //重要:定时器结束时需选择表06

            i2c.TWI_ReadPage(0xa0, 0xDC, temp, 2);
            textBox_tecVdac.Text = (((temp[0] * 256 + temp[1]) * 2.4) / 4095.0).ToString("F2") + "V"; //TOSA SOFT VTEC

            i2c.TWI_ReadPage(0xa0, 0xD8, temp, 2);
            textBox_soatecVdac.Text = (((temp[0] * 256 + temp[1]) * 2.4) / 4095.0).ToString("F2") + "V"; //SOA SOFT VTEC

            ////////////////////////////////////////////////////////////////////////
            i2c.TWI_ReadPage(0xa0, 0x80, temp, 1);
            SetRedLED(ERLUT_pictureBox, !Bit.GetBit(temp[0], 0));
            i2c.TWI_ReadPage(0xa0, 0x98, temp, 1);
            tempIndex_textBox.Text = string.Format("{0}", temp[0].ToString("D"));
            //
            i2c.TWI_ReadPage(0xa0, 0xFA, temp, 2);
            if (temp[0] == 0x66) { SetRedLED(TX_EN_pictureBox, false); }
            else { SetRedLED(TX_EN_pictureBox, true); }
            //
            if (temp[1] == 0x66) { SetRedLED(TEC_OPEN_pictureBox, false); }
            else { SetRedLED(TEC_OPEN_pictureBox, true); }
            //
            i2c.TWI_ReadPage(0xa0, 0x9A, temp, 1);
            SetRedLED(dualRate_pictureBox, !Bit.GetBit(temp[0], 0));//2023.11.8          
            ////////////////////////////////////////////////////////////////////////

            if (i2c.TWI_ReadPage(0xa0, 0xFC, temp, 4) != 4)
            {
                foreach (Control control in groupBox2.Controls)
                {
                    if (control is PictureBox)
                    {
                        PictureBox picturebox = control as PictureBox;
                        picturebox.Image = imageList1.Images["LedNone.ico"];
                    }
                }
                foreach (Control control in control_groupBox.Controls)
                {
                    if (control is PictureBox)
                    {
                        PictureBox picturebox = control as PictureBox;
                        picturebox.Image = imageList1.Images["LedNone.ico"];
                    }
                }
                return;
            }

            if ((temp[2] & 0x0F) == 0x0F)
            {
                SetRedLED(Chip_OK_pictureBox, false); //Chip is OK
            }
            else
            {
                SetRedLED(Chip_OK_pictureBox, true);
            }
            
            SetRedLED(WP_pictureBox, !Bit.GetBit(temp[1], 4));
            SetRedLED(Vendor_pictureBox, !Bit.GetBit(temp[2], 4));
            SetRedLED(TEC_OK_pictureBox, !Bit.GetBit(temp[2], 5));

            SetRedLED(TECSOA_OK_pictureBox, !Bit.GetBit(temp[3], 2));
            SetRedLED(TECSOA_OPEN_pictureBox, !Bit.GetBit(temp[3], 3));
            SetRedLED(SOALUT_DIS_pictureBox, Bit.GetBit(temp[3], 4));

            SetRedLED(mcu_BM_pictureBox, Bit.GetBit(temp[3], 6));  //0xFF: bit6 自定义 MCU_BM 管脚状态指示
            SetRedLED(PinLpMode_pictureBox,   Bit.GetBit(temp[3], 7));  //0xFF: bit7 自定义 LP_MODE 管脚状态指示
            SetRedLED(Chip_ReInit_pictureBox, Bit.GetBit(temp[3], 1));  //0xFF: bit1 自定义 Chip reinit 状态指示
            SetRedLED(MCU_OK_pictureBox,      Bit.GetBit(temp[3], 0));  //0xFF: bit0 自定义 MCU is ok 状态指示

            if (Bit.GetBit(temp[2], 4))
            {
                debugMode = true;
            }
            else
            {
                debugMode = false;
            }

            String str, strRate;
            byte hardwareVer = temp[0];
            byte firmwareVer = temp[1];

            str = string.Format("设计方案{0}  ", (hardwareVer&0x0F).ToString("D"));

            strRate = "000";
            switch (hardwareVer & 0xF0)
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

            str += " ";
            if (strRate == "40G")
            {
                switch (hardwareVer & 0x0F)
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
            else if (strRate == "100G")
            {
                switch (hardwareVer & 0x0F)
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
            else if (strRate == "100G-PAM4")
            {
                switch (hardwareVer & 0x0F)
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
            else
            {
                str += "未定义";
            }

            str += " ";
            if (strRate == "100G-PAM4")
            {
                switch (firmwareVer & 0xE0)
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
            else
            {
                switch (firmwareVer & 0xE0)
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
            str += string.Format("  软件版本:V{0}  ", (firmwareVer&0x0F).ToString("D"));
            
            firmware_ver_toolStripStatusLabel.Text = "QSFP: " + str;            
        }

        //写入VENDOR密码
        private bool WriteVenderPWD()
        {
            uint num = 0;

            byte[] pwd = new byte[4];
            pwd[0] = 0xA9;
            pwd[1] = 0x46;
            pwd[2] = 0x50;
            pwd[3] = 0x54;

            num = i2c.TWI_WritePage(0xA0, 123, pwd, 4);
            Thread.Sleep(100);

            return (num == 4) ? true : false;
        }

        // 表选择
        private bool SelectTable(byte tbl)
        {
            return i2c.TWI_WriteByte(0xA0, 127, tbl);
        }

        // 保存校准数据到FLASH
        private bool SaveCalToFlash()
        {
            if (!SelectTable(6))
            {
                return false;
            }

            return i2c.TWI_WriteByte(0xA0, 0x83, 0x02);
        }

        /*// 保存所有信息到FLASH
        private bool SaveAllFlash()
        {
            if (!SelectTable(6))
            {
                return false;
            }

            return i2c.TWI_WriteByte(0xA0, 0x84, 1);
        }*/

        // 保存温补数据到FLASH
        private bool SaveERLUTToFlash()
        {
            if (!SelectTable(6))
            {
                return false;
            }

            return i2c.TWI_WriteByte(0xA0, 0x82, 1);
        }

        //读取当前设置
        private void Read_button_Click(object sender, EventArgs e)
        {
            byte[] rdBuf = new byte[2];
            byte i = 0;
            byte ch;
            byte temp = 0;
            timer1.Stop();
                     
            if (!WriteVenderPWD())
            {
                MessageBox.Show("写入厂商密码出错！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }

            SelectTable(0xB0);
            temp = i2c.TWI_ReadByte(0xA0, 0xE8);
            if ((temp == 1) && cBFECEn.Checked)
            {
                cBFECEn.Checked = false;//FEC使能关闭
            }
            else if ((temp == 1) && (cBFECEn.Checked == false))
            {
                cBFECEn.Checked = true;
                Thread.Sleep(10);
                cBFECEn.Checked = false;//FEC使能关闭
            }

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }

            if (!i2c.TWI_WriteByte(0xA0, 0x80, 0x00))
            {
                MessageBox.Show("关闭LUT失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }

            temp = i2c.TWI_ReadByte(0xA0, 0x9A);
            if (Bit.GetBit(temp, 4))
            {               
                cBddm4chEn.Checked = false;
                lbchddm.Text = "1通道DDM";
            }
            else
            {
                cBddm4chEn.Checked = true;
                lbchddm.Text = "4通道DDM";               
            }

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xA0;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读APC值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            APC_trackBar.Value = rdBuf[0];

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xA4;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读MOD值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            Mod_trackBar.Value = rdBuf[0];

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xA8;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Tx_CPA值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            TxCPA_trackBar.Value = rdBuf[0];

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xAC;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Tx_PE值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            TxPE_trackBar.Value = rdBuf[0];

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xB0;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Rx_Swing值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            RxSwing_trackBar.Value = rdBuf[0];

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xB4;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Los值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            LOS_trackBar.Value = rdBuf[0];

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xB8;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Rx_PE值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            RxPE_trackBar.Value = rdBuf[0];

            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xBC;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读VAPD值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            APD_trackBar.Value = rdBuf[0];

            //
            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xC4;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Tx_Out_Swing值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            TxSwing_trackBar.Value = rdBuf[0];
            //
            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0x9B;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读RxVagc值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            trackBar_RxVagc.Value = rdBuf[0];

            //
            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xD0;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Tx_Ctrl_1值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            TxCtrl1_trackBar.Value = rdBuf[0];

            //
            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xD4;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读Tx_Ctrl_2值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            TxCtrl2_trackBar.Value = rdBuf[0];

            //
            i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xC8;
            if (i2c.TWI_ReadPage(0xA0, i, rdBuf, 1) != 1)
            {
                MessageBox.Show("读EA值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            Von_trackBar.Value = rdBuf[0];

            //
            if (i2c.TWI_ReadPage(0xA0, 0xCC, rdBuf, 2) != 2)
            {
                MessageBox.Show("读TOSA温度设置值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            //rdBuf[0] &= 0x0F;
            int tempsetvalue = (rdBuf[0] * 256 + rdBuf[1]);
            if (tempsetvalue < Temp_trackBar.Minimum)
                tempsetvalue = Temp_trackBar.Minimum;
            if (tempsetvalue > Temp_trackBar.Maximum)
                tempsetvalue = Temp_trackBar.Maximum;
            Temp_trackBar.Value = tempsetvalue;

            //
            if (i2c.TWI_ReadPage(0xA0, 0xCE, rdBuf, 2) != 2)
            {
                MessageBox.Show("读ROSA-SOA温度设置值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            //rdBuf[0] &= 0x0F;
            tempsetvalue = (rdBuf[0] * 256 + rdBuf[1]);
            if (tempsetvalue < TempSOA_trackBar.Minimum)
                tempsetvalue = TempSOA_trackBar.Minimum;
            if (tempsetvalue > TempSOA_trackBar.Maximum)
                tempsetvalue = TempSOA_trackBar.Maximum;
            TempSOA_trackBar.Value = tempsetvalue;

            /////////////////////////////////////////////////////////
            SelectTable(0x0B);
            ch = Convert.ToByte(SOA_RSSIchannel_textBox.Text);
            if (ch > 3)
            {
                ch = 3;
            }
            ch *= 2;
            ch += 0x89;
            if (i2c.TWI_ReadPage(0xA0, ch, rdBuf, 2) != 2)
            {
                SelectTable(0x06);
                MessageBox.Show("读SOA-RSSI-VALUE设置值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            int soarssivalue = (rdBuf[0] * 256 + rdBuf[1]);
            SOA_RSSIval_textBox.Text = soarssivalue.ToString();
            SelectTable(0x06);
            /////////////////////////////////////////////////////////

            //
            if (i2c.TWI_ReadPage(0xA0, 0xDE, rdBuf, 2) != 2)
            {
                MessageBox.Show("读SOA-iBisa设置值错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS_R;
            }
            //rdBuf[0] &= 0x0F;
            int soabiasvalue = (rdBuf[0] * 256 + rdBuf[1]);
            if (soabiasvalue < BiasSOA_trackBar.Minimum)
                soabiasvalue = BiasSOA_trackBar.Minimum;
            if (soabiasvalue > BiasSOA_trackBar.Maximum)
                soabiasvalue = BiasSOA_trackBar.Maximum;
            BiasSOA_trackBar.Value = soabiasvalue;
           
            /*// 更新 Fsn  飞思卓产品内部流水号
            byte[] readbuffer = new byte[6];
            SelectTable(6);
            if (i2c.TWI_ReadPage(0xa0, 0x92, readbuffer, 5) != 5)
            {
                MessageBox.Show("模块读取信息fsn错误01！\r\n请确认。", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            UInt64 iFsn = 0;
            iFsn += readbuffer[0];
            iFsn <<= 8;
            iFsn += readbuffer[1];
            iFsn <<= 8;
            iFsn += readbuffer[2];
            iFsn <<= 8;
            iFsn += readbuffer[3];
            iFsn <<= 8;
            iFsn += readbuffer[4];
            if (iFsn > 999999999999) iFsn = 999999999999; // 10进制12位
            fsn_textBox1.Text = iFsn.ToString("D12");
            //*/

        RTN_POS_R:
            timer1.Start();
        }

        //保存当前配置
        private void Write_button_Click(object sender, EventArgs e)
        {
            byte[] pwd = new byte[4];
            pwd[0] = 0x00;
            pwd[1] = 0x00;
            pwd[2] = 0x00;
            pwd[3] = 0x00;

            timer1.Stop();
            Write_button.Enabled = false;

            i2c.TWI_ReadPage(0xA0, 123, pwd, 4);
            if ((pwd[0] != 0xA9) || (pwd[1] != 0x46) || (pwd[2] != 0x50) || (pwd[3] != 0x54))
            {
                MessageBox.Show("待测QSFP模块不在调试模式下，无法保存，请确认！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }

            byte channel = Convert.ToByte(channelSel_comboBox.Text);

            if (channel > 3)
            {
                MessageBox.Show("通道号超出0-3，请确认！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }

            /////////////////////////////////
            int bias_value, mod_value, temp_value, temp_index, apd_value;
            int delt_bias=0, delt_mod=0, delt_apd=0;
            string dbconnectionstr;
            byte[] Low128 = new byte[128];
            //byte[] r_Low128 = new byte[128];
            byte[] biaslut = new byte[128];
            byte[] modlut  = new byte[128];
            byte[] apdlut = new byte[128];
            byte[] r_biaslut = new byte[128];
            byte[] r_modlut = new byte[128];
            byte[] r_apdlut = new byte[128];
            //Array.Copy(Low128, 0, threshold, 0, 60);
            byte[] threshold = new byte[72];
            byte[] r_threshold = new byte[72];

            bias_value = APC_trackBar.Value;
            mod_value = Mod_trackBar.Value;
            apd_value = APD_trackBar.Value;
            temp_value = (int)float.Parse(Temp_textBox.Text);
            if (temp_value < -40) temp_value = -40;
            if (temp_value > 115) temp_value = 115;
            temp_index = (temp_value + 40) / 5;
            if (temp_index < 0) temp_index = 0;
            if (temp_index > 31) temp_index = 31;

            //
            // 选择补偿表 Bias 08
            if (!SelectTable(8))
            {
                MessageBox.Show("选择表08 错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            if (i2c.TWI_ReadPage(0xa0, 0x80, biaslut, 128) != 128)
            {
                MessageBox.Show("读取QSFP模块内部Bias补偿表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }

            // 选择补偿表 Mod 09
            if (!SelectTable(9))
            {
                MessageBox.Show("选择表09 错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            if (i2c.TWI_ReadPage(0xa0, 0x80, modlut, 128) != 128)
            {
                MessageBox.Show("读取QSFP模块内部Mod补偿表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }

            // 选择补偿表 APD 0A
            if (!SelectTable(10))
            {
                MessageBox.Show("选择表0A 错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            if (i2c.TWI_ReadPage(0xa0, 0x80, apdlut, 128) != 128)
            {
                MessageBox.Show("读取QSFP模块内部Apd补偿表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            //

            //
            if (apd_checkBox.Checked)
            {
                dbconnectionstr = string.Format("select Low128,Page03,BiasVal,ModVal,ApdVal from [{0}]", Module_Type_comboBox.Text);
            }
            else
            {
                dbconnectionstr = string.Format("select Low128,Page03,BiasVal,ModVal from [{0}]", Module_Type_comboBox.Text);
            }

            dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
            dbadapter = new OleDbDataAdapter(dbcommand);
            dbset = new DataSet();
            dbadapter.Fill(dbset, Module_Type_comboBox.Text);
            //
            delt_bias = bias_value - Convert.ToUInt16(dbset.Tables[Module_Type_comboBox.Text].Rows[temp_index]["BiasVal"]);
            delt_mod = mod_value - Convert.ToUInt16(dbset.Tables[Module_Type_comboBox.Text].Rows[temp_index]["ModVal"]);
            if (apd_checkBox.Checked)
            {
                delt_apd = apd_value - Convert.ToUInt16(dbset.Tables[Module_Type_comboBox.Text].Rows[temp_index]["ApdVal"]);
            }
            //

            for (int i = 0; i < 128; i++)
            {
                Low128[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["Low128"]);
            }
            for (int i = 0; i < 32; i++)
            {
                int bias_lut = delt_bias + Convert.ToUInt16(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["BiasVal"]);
                if (bias_lut < 0) bias_lut = 0;
                if (bias_lut > 255) bias_lut = 255;
                biaslut[4 * i + channel] = (byte)bias_lut;

                int mod_lut = delt_mod + Convert.ToUInt16(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["ModVal"]);
                if (mod_lut < 0) mod_lut = 0;
                if (mod_lut > 255) mod_lut = 255;
                modlut[4 * i + channel] = (byte)mod_lut;

                if (apd_checkBox.Checked)
                {
                    int apd_lut = delt_apd + Convert.ToUInt16(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["ApdVal"]);
                    if (apd_lut < 0) apd_lut = 0;
                    if (apd_lut > 255) apd_lut = 255;
                    apdlut[4 * i + channel] = (byte)apd_lut;
                }
            }

            //读取数据库补偿表 Page03
            for (int i = 0; i < 72; i++)
            {
                threshold[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["Page03"]);
            }
            
            //
            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            // 更新界面信息到模块内部
            //i2c.TWI_WriteByte(0xa0, (byte)(channel + 0xA0), (byte)APC_trackBar.Value);
            if (LOS_initvalue_checkBox.Checked)
                i2c.TWI_WriteByte(0xa0, (byte)(channel + 0xB4), Convert.ToByte(LOS_initvalue_textBox.Text));
            else
                i2c.TWI_WriteByte(0xa0, (byte)(channel + 0xB4), Convert.ToByte(LOS_trackBar.Value));
            //
            //RxVagc
            if (cBRxVagcInitval.Checked)
            {
                if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                    return;

                byte i = Convert.ToByte(channelSel_comboBox.Text);
                i += 0x9B;

                i2c.TWI_WriteByte(0xa0, i, Convert.ToByte(tBRxVagcInitVal.Text));
            }
            else
            {
                if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                    return;

                byte i = Convert.ToByte(channelSel_comboBox.Text);
                i += 0x9B;
                i2c.TWI_WriteByte(0xa0, i, (byte)trackBar_RxVagc.Value);
            }
            // 选择补偿表 Bias
            if (!SelectTable(8))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            i2c.TWI_WritePage(0xa0, 0x80, biaslut, 128);
            i2c.TWI_ReadPage(0xa0, 0x80, r_biaslut, 128);
            if (!Bit.ByteEquals(biaslut, r_biaslut))
            {
               /* string str="";
                for (int i = 0; i < 128; i++)
                {
                    if (biaslut[i] != r_biaslut[i])
                    {
                        str += i.ToString() + " ";
                    }             
                }
                MessageBox.Show(str.ToString());*/
                MessageBox.Show("补偿表校验失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                goto RTN_POS;
            }

            // 选择补偿表 Mod
            if (!SelectTable(9))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            i2c.TWI_WritePage(0xa0, 0x80, modlut, 128);
            i2c.TWI_ReadPage(0xa0, 0x80, r_modlut, 128);
            if (!Bit.ByteEquals(modlut, r_modlut))
            {
                MessageBox.Show("补偿表校验失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                goto RTN_POS;
            }

            if (apd_checkBox.Checked)
            {
                // 选择补偿表 APD
                if (!SelectTable(10))
                {
                    MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    goto RTN_POS;
                }
                i2c.TWI_WritePage(0xa0, 0x80, apdlut, 128);
                i2c.TWI_ReadPage(0xa0, 0x80, r_apdlut, 128);
                if (!Bit.ByteEquals(modlut, r_modlut))
                {
                    MessageBox.Show("补偿表校验失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    goto RTN_POS;
                }
            }

            // 选择告警门限表 Page03
            if (!SelectTable(3))
            {
                MessageBox.Show("选择03表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            i2c.TWI_WritePage(0xa0, 0x80, threshold, 72);
            i2c.TWI_ReadPage(0xa0, 0x80, r_threshold, 72);
            if (!Bit.ByteEquals(threshold, r_threshold))
            {
                MessageBox.Show("保存门限03表校验失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                goto RTN_POS;
            }
            //写入Tap默认值
            if (cBSaveTapDefval.Checked)
            {
                if (SelectTable(0xC0) == false)
                {
                    MessageBox.Show("保存默认Tap值表选失败");
                    return;
                }
                if (i2c.TWI_WritePage(0xa0, 0xB4, tap_L, 18) != 18)
                {
                    MessageBox.Show("保存默认Tap_L值失败");
                    return;
                }
                //if (i2c.TWI_WritePage(0xa0, 0xC6, tap_H, 22) != 22)
                //{
                //    MessageBox.Show("保存默认Tap_H值失败");
                //    return;
                //}
                if (i2c.TWI_WriteByte(0xa0, 0xF5,dsp_pol) == false)
                {
                    MessageBox.Show("保存默认TxRxPolInv 极性值失败");
                    return;
                }
            }          

            // 发送保存命令
            byte[] saveByte = new byte[3];
            saveByte[0] = 0x08; // threshold  Page03   bit3  0x82地址
            saveByte[1] = 0x0D; // 00001101  bit3 bit2 bit0  0x83地址
            if (apd_checkBox.Checked)
            {
                saveByte[1] |= 0x10; //bit4=1 APD_LUT
            }
            saveByte[1] |= 0x40; //bit6=1 SOA_LUT
            saveByte[1] |= 0x80;//bit7 DSP
            //Save data to flash
            if (!SelectTable(6))
            {
                MessageBox.Show("保存命令：选择表6错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            if (i2c.TWI_WritePage(0xa0, 0x82, saveByte, 2) != 2)
            {
                MessageBox.Show("保存命令：发送保存命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                goto RTN_POS;
            }
            Thread.Sleep(1000);
            MessageBox.Show("保存成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //
        RTN_POS:
            timer1.Start();
            Write_button.Enabled = true;
        }

        //擦除代码
        private void Erase_Code_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("你确定要擦除 QSFP 模块的软件？\r\n注意：擦除后请开关一次电源。", "提示", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                return;
            }

            if (!WriteVenderPWD())
            {
                MessageBox.Show("写入厂商密码出错！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SelectTable(0x06))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte wrtBuf = 0x66;

            if (!i2c.TWI_WriteByte(0xA0, 0x9F, wrtBuf))
            {
                MessageBox.Show("发送擦除命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("成功擦除QSFP模块程序，请重新开关模块电源。 \n");
        }

        //更新调试参数
        private void Module_Type_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
           // string dbconnectionstr = string.Format("select Low128,Page03,BiasVal,ModVal,ApdVal,TxTap_L,TxTap_H from [{0}]", Module_Type_comboBox.Text);
            string dbconnectionstr = string.Format("select Low128,Page03,BiasVal,ModVal,ApdVal,TxTap_L,TxTap_H,TxRxPolInv from [{0}]", Module_Type_comboBox.Text);
            dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
            dbadapter = new OleDbDataAdapter(dbcommand);
            dbset = new DataSet();
            dbadapter.Fill(dbset, Module_Type_comboBox.Text);
            byte[] buffertemp = new byte[22];
            byte[] buffertemp2 = new byte[22];
            for (int i = 0; i < 18; i++)
            {
                buffertemp[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["TxTap_L"]);
                buffertemp2[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["TxTap_H"]);
            }
            dsp_pol = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[0]["TxRxPolInv"]);

            for (int i = 0; i < 18;i++ )
            {
                //tap_L[2 * i] = buffertemp[2 * i + 1];
                //tap_L[2 * i + 1] = buffertemp[2 * i];

                //tap_H[2 * i] = buffertemp2[2 * i + 1];
                //tap_H[2 * i + 1] = buffertemp2[2 * i];
                tap_L[i] = buffertemp[i];
                tap_H[i] = buffertemp2[i];
            }

            //TxERSetPoint_textBox.Text = dbset.Tables["ModuleType"].Rows[0]["TxERSetPoint"].ToString();
            //txpwr_min_textBox.Text = dbset.Tables["ModuleType"].Rows[0]["TxPower_MIN"].ToString();
            //txpwr_max_textBox.Text = dbset.Tables["ModuleType"].Rows[0]["TxPower_MAX"].ToString();
            //bias_min = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["Bias_MIN"]);
            //bias_max = Convert.ToSingle(dbset.Tables["ModuleType"].Rows[0]["Bias_MAX"]);

            //APC_trackBar.Maximum = 255;
            //Mod_trackBar.Maximum = 255;
            //LOS_trackBar.Maximum = 255;
            //APD_trackBar.Minimum = 0;
            //APD_trackBar.Maximum = 255;
            //TX_EN_pictureBox.Enabled = false;
            //TEC_OPEN_pictureBox.Enabled = false;
            //TEC_OK_pictureBox.Enabled = false;
            //TEC_OPEN_checkBox.Enabled = false;
            //TX_EN_checkBox.Enabled = false;
            //Temp_trackBar.Enabled = false;
            //Von_trackBar.Enabled = false;
            apd_checkBox.Checked = false;

            /*switch (Module_Type_comboBox.Text)
            {
                case "FXP85192-SRC":
                    {
                        TX_EN_pictureBox.Enabled = false;
                        TEC_OPEN_pictureBox.Enabled = false;
                        TEC_OK_pictureBox.Enabled = false;
                        TEC_OPEN_checkBox.Enabled = false;
                        TX_EN_checkBox.Enabled = false;
                        Temp_trackBar.Enabled = false;
                        Von_trackBar.Enabled = false;
                        //APD_trackBar.Enabled = false;
                        //APC_trackBar.Maximum = 255;
                        //Mod_trackBar.Maximum = 1023;
                        //LOS_trackBar.Maximum = 255;
                    }
                    break;
                case "FXP31192-LRC":
                    {
                        TX_EN_pictureBox.Enabled = false;
                        TEC_OPEN_pictureBox.Enabled = false;
                        TEC_OK_pictureBox.Enabled = false;
                        TEC_OPEN_checkBox.Enabled = false;
                        TX_EN_checkBox.Enabled = false;
                        Temp_trackBar.Enabled = false;
                        Von_trackBar.Enabled = false;
                        //APD_trackBar.Enabled = false;
                        //APC_trackBar.Maximum = 255;
                        //Mod_trackBar.Maximum = 1023;
                        //LOS_trackBar.Maximum = 255;
                    }
                    break;
                case "FXP55192-ERC":
                    {
                        TX_EN_pictureBox.Enabled = true;
                        TEC_OPEN_pictureBox.Enabled = true;
                        TEC_OK_pictureBox.Enabled = true;
                        TEC_OPEN_checkBox.Enabled = true;
                        TX_EN_checkBox.Enabled = true;
                        Temp_trackBar.Enabled = true;
                        Von_trackBar.Enabled = true;
                        //APD_trackBar.Enabled = false;
                        //APC_trackBar.Maximum = 1023;
                        //Mod_trackBar.Maximum = 1023;
                        //LOS_trackBar.Maximum = 255;
                        //Temp_trackBar.Minimum = 830;
                        //Temp_trackBar.Maximum = 1830;
                        Von_trackBar.Maximum = 4095;
                    }
                    break;
                case "FXP55192-ZRC":
                    {
                        TX_EN_pictureBox.Enabled = true;
                        TEC_OPEN_pictureBox.Enabled = true;
                        TEC_OK_pictureBox.Enabled = true;
                        TEC_OPEN_checkBox.Enabled = true;
                        TX_EN_checkBox.Enabled = true;
                        Temp_trackBar.Enabled = true;
                        Von_trackBar.Enabled = true;
                        //APD_trackBar.Enabled = false;
                        //APC_trackBar.Maximum = 1023;
                        //Mod_trackBar.Maximum = 1023;
                        //LOS_trackBar.Maximum = 255;
                        //Temp_trackBar.Minimum = 830;
                        //Temp_trackBar.Maximum = 1830;
                        Von_trackBar.Maximum = 4095;
                        //APD_trackBar.Minimum = 0;
                        //APD_trackBar.Maximum = 4095;
                    }
                    break;
                default: break;
            }*/
        }

        //发射开关控制
        private void TX_EN_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte[] pwd = new byte[4];
            pwd[0] = 0x00;
            pwd[1] = 0x00;
            pwd[2] = 0x00;
            pwd[3] = 0x00;
            i2c.TWI_ReadPage(0xA0, 123, pwd, 4);
            if ((pwd[0] != 0xA9) || (pwd[1] != 0x46) || (pwd[2] != 0x50) || (pwd[3] != 0x54))
            {
                MessageBox.Show("待测QSFP模块不在调试模式下，无法保存，请确认！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ////

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte wrtBuf = 0;

            if (TX_EN_checkBox.Checked)
            {
                wrtBuf = 0x66;
            }

            if (!i2c.TWI_WriteByte(0xA0, 0xFA, wrtBuf))
            {
                MessageBox.Show("发送TX EN命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //TEC开关控制
        private void TEC_OPEN_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte[] pwd = new byte[4];
            pwd[0] = 0x00;
            pwd[1] = 0x00;
            pwd[2] = 0x00;
            pwd[3] = 0x00;
            i2c.TWI_ReadPage(0xA0, 123, pwd, 4);
            if ((pwd[0] != 0xA9) || (pwd[1] != 0x46) || (pwd[2] != 0x50) || (pwd[3] != 0x54))
            {
                MessageBox.Show("待测QSFP模块不在调试模式下，无法保存，请确认！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            ////

            // 关闭TEC功能提示风险
            if (TEC_OPEN_checkBox.Checked == false)
            {
                if (MessageBox.Show("关闭TEC控制有损坏TOSA的风险，确定继续关闭TEC？\r\n注意：为了TOSA安全，请尽快打开TEC。", "提示", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }
            }
            //

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte wrtBuf = 0;

            if (TEC_OPEN_checkBox.Checked)
            {
                wrtBuf = 0x66;
            }

            if (!i2c.TWI_WriteByte(0xA0, 0xFB, wrtBuf))
            {
                MessageBox.Show("发送TEC OPEN命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        //交准设置
        private void Cal_ToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            //Cal_Form cal_form = new Cal_Form(this.i2c);
            timer1.Stop();
            using (var cal_form = new Cal_Form(this.i2c))
            {
                cal_form.ShowDialog();
            }
            timer1.Start();
        }

        //阈值设置
        private void threshold_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Ex_Form ex_form = new Ex_Form(this.i2c, this.dbconnect, false);
            timer1.Stop();
            ex_form.ShowDialog();
            timer1.Start();
        }

        //关闭写保护
        private void ClosedWP_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!WriteVenderPWD())
            {
                MessageBox.Show("写入厂商密码出错！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte wrtBuf = 0x55;

            if (!i2c.TWI_WriteByte(0xA0, 0x81, wrtBuf))
            {
                MessageBox.Show("发送关闭写保护命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            i2c.TWI_WriteByte(0xA0, 0x83, 1);
            Thread.Sleep(1000);
            MessageBox.Show("成功关闭模块写保护，请重新开关模块电源。 \n");
        }

        //打开写保护
        private void OpenWP_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!WriteVenderPWD())
            {
                MessageBox.Show("写入厂商密码出错！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte wrtBuf = 0;

            if (!i2c.TWI_WriteByte(0xA0, 0x81, wrtBuf))
            {
                MessageBox.Show("发送打开写保护命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            i2c.TWI_WriteByte(0xA0, 0x83, 1);
            Thread.Sleep(1000);
            MessageBox.Show("成功打开模块写保护，请重新开关模块电源。 \n");
        }

        private void Soft_TX_Disable_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 86);

            byte i = Convert.ToByte(channelSel_comboBox.Text);

            if (Soft_TX_Disable_checkBox.Checked)
            {
                wrtBuf = Bit.SetBit(wrtBuf, i);
            }
            else
            {
                wrtBuf = Bit.ClearBit(wrtBuf, i);
            }
            i2c.TWI_WriteByte(0xA0, 86, wrtBuf);
        }

        private void Soft_TxDisAll_checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 86);

            if (Soft_TxDisAll_checkBox1.Checked)
            {
                wrtBuf |= 0x0F;
            }
            else
            {
                wrtBuf &= 0xF0;
            }

            i2c.TWI_WriteByte(0xA0, 86, wrtBuf);
        }

        private void TxChEnable(byte ch)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 86);
           
            wrtBuf |= 0x0F;
                    
            i2c.TWI_WriteByte(0xA0, 86, wrtBuf);//dis ch0,ch1,ch2,ch3
            //
            wrtBuf = i2c.TWI_ReadByte(0xa0, 86);

            byte i = ch;//Convert.ToByte(channelSel_comboBox.Text);            
            wrtBuf = Bit.ClearBit(wrtBuf, i);
            
            i2c.TWI_WriteByte(0xA0, 86, wrtBuf);
        }

        private void Soft_P_Down_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 93);

            if (Soft_LpMode_checkBox.Checked)
            {
                wrtBuf |= 0x03;
            }
            else
            {
                wrtBuf &= 0xFC;
            }
            i2c.TWI_WriteByte(0xA0, 93, wrtBuf);
        }

        private void LOS_initvalue_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LOS_initvalue_checkBox.Checked)
            {
                LOS_initvalue_textBox.ReadOnly = false;
                LOS_trackBar.Enabled = false;
            }
            else
            {
                LOS_initvalue_textBox.ReadOnly = true;
                LOS_trackBar.Enabled = true;
            }
        }

        private void apd_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (apd_checkBox.Checked)
            {
                APD_trackBar.Enabled = true;
            }
            else
            {
                APD_trackBar.Enabled = false;
            }
        }

        private void read_reg_button_Click(object sender, EventArgs e)
        {
            byte[] writebuffer = new byte[3];
            byte temp_byte = 0x00;

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            read_reg_button.Enabled = false;

            wreg_checkBox.Checked = false;

            writebuffer[0] = Convert.ToByte(devAddr_textBox.Text); //I2C器件地址 2021.10.12
            writebuffer[1] = Convert.ToByte(chipSel_comboBox.Text); //读取和通道选择
            writebuffer[1] &= 0x0F;
            writebuffer[2] = Convert.ToByte(regAddr_textBox.Text); //寄存器地址
            
            i2c.TWI_WritePage(0xA0, 0xF6, writebuffer, 3);

            Thread.Sleep(600);
            temp_byte = i2c.TWI_ReadByte(0xa0, 0xF9); //读取寄存器内容

            regValue_textBox.Text = temp_byte.ToString("X02");

            wreg_trackBar.Value = temp_byte;

            read_reg_button.Enabled = true;
        }

        private void wreg_trackBar_ValueChanged(object sender, EventArgs e)
        {
            wreg_value_textBox.Text = wreg_trackBar.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte[] writebuffer = new byte[4];

            writebuffer[0] = Convert.ToByte(devAddr_textBox.Text); //I2C器件地址 2021.10.12
            writebuffer[1] = Convert.ToByte(chipSel_comboBox.Text); //写入和通道选择
            writebuffer[1] &= 0x0F;
            writebuffer[1] |= 0x60;
            writebuffer[2] = Convert.ToByte(regAddr_textBox.Text); //寄存器地址
            writebuffer[3] = (Byte)wreg_trackBar.Value; //写入寄存器内容
            if (wreg_checkBox.Checked)
            {
                i2c.TWI_WritePage(0xA0, 0xF6, writebuffer, 4);
            }
        }

        private void TxCPA_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TxCPA_checkBox.Checked)
            {
                TxCPA_trackBar.Enabled = true;
            }
            else
            {
                TxCPA_trackBar.Enabled = false;
            }
        }

        private void TxSwing_checkBox_CheckedChanged_1(object sender, EventArgs e)
        {
            if (TxSwing_checkBox.Checked)
            {
                TxSwing_trackBar.Enabled = true;
            }
            else
            {
                TxSwing_trackBar.Enabled = false;
            }
        }

        private void TxCtrl_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TxCtrl_checkBox.Checked)
            {
                TxCtrl1_trackBar.Enabled = true;
                TxCtrl2_trackBar.Enabled = true;
            }
            else
            {
                TxCtrl1_trackBar.Enabled = false;
                TxCtrl2_trackBar.Enabled = false;
            }
        }

        private void RxPE_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (RxPE_checkBox.Checked)
            {
                RxPE_trackBar.Enabled = true;
            }
            else
            {
                RxPE_trackBar.Enabled = false;
            }
        }

        private void RxSwing_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (RxSwing_checkBox.Checked)
            {
                RxSwing_trackBar.Enabled = true;
            }
            else
            {
                RxSwing_trackBar.Enabled = false;
            }
        }

        private void TxPE_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TxPE_checkBox.Checked)
            {
                TxPE_trackBar.Enabled = true;
            }
            else
            {
                TxPE_trackBar.Enabled = false;
            }
        }

        private void wreg_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (wreg_checkBox.Checked)
            {
                wreg_trackBar.Enabled = true;
            }
            else
            {
                wreg_trackBar.Enabled = false;
            }
        }

        private void txcal_button_Click(object sender, EventArgs e)
        {
            if (debugMode == false)
            {
                MessageBox.Show("非调试模式，TX无法校准！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            float txpower = Convert.ToSingle(txcaldbm_textbox.Text);
            float k = 0;
            int ADC = Convert.ToInt32(txadc_textbox.Text);
            byte[] c0 = new byte[4];

            byte[] writebuffer = new byte[4];
            byte[] readbuffer = new byte[4];

            k = (float)Math.Pow(10, 0.1 * txpower) * 10000;
            k = k / ADC;
            c0 = BitConverter.GetBytes(k);
            c0.CopyTo(writebuffer, 0);

            if (!SelectTable(7))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i *= 4;
            i += 0x00;
            i += 0x80;

            if ((i2c.TWI_WritePage(0xA0, i, writebuffer, 4) != 4))
            {
                MessageBox.Show("写入失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SaveCalToFlash())
            {
                MessageBox.Show("Save命令失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Thread.Sleep(700);

            if (!SelectTable(7))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if ((i2c.TWI_ReadPage(0xA0, i, readbuffer, 4) != 4))
            {
                MessageBox.Show("读取失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Bit.ByteEquals(readbuffer, writebuffer))
                MessageBox.Show("保存成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("保存失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //切换到调试参数table
            SelectTable(6);
        }

        private void Soft_TXCDR_Disable_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xA0, 98);

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 4;

            if (Soft_TXCDR_Disable_checkBox.Checked)
            {
                wrtBuf = Bit.ClearBit(wrtBuf, i);
            }
            else
            {
                wrtBuf = Bit.SetBit(wrtBuf, i);
            }
            i2c.TWI_WriteByte(0xA0, 98, wrtBuf);
        }

        private void Soft_TxCDRDisAll_checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xA0, 98);

            if (Soft_TxCDRDisAll_checkBox1.Checked)
            {
                wrtBuf &= 0x0F;
            }
            else
            {
                wrtBuf |= 0xF0;
            }
            i2c.TWI_WriteByte(0xA0, 98, wrtBuf);
        }

        private void Soft_RXCDR_Disable_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xA0, 98);

            byte i = Convert.ToByte(channelSel_comboBox.Text);

            if (Soft_RXCDR_Disable_checkBox.Checked)
            {
                wrtBuf = Bit.ClearBit(wrtBuf, i);
            }
            else
            {
                wrtBuf = Bit.SetBit(wrtBuf, i);
            }
            i2c.TWI_WriteByte(0xA0, 98, wrtBuf);
        }

        private void Soft_RxCDRDisAll_checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xA0, 98);

            if (Soft_RxCDRDisAll_checkBox1.Checked)
            {
                wrtBuf &= 0xF0;
            }
            else
            {
                wrtBuf |= 0x0F;
            }
            i2c.TWI_WriteByte(0xA0, 98, wrtBuf);
        }

        private void TempSOA_trackBar_ValueChanged(object sender, EventArgs e)
        {
            TempSOA_value_textBox.Text = TempSOA_trackBar.Value.ToString();

            float vtemp = (float)(TempSOA_trackBar.Value * (2.5 / 4095));
            TempSOA_degree_textBox.Text = VoltagetoTemperature(vtemp).ToString("F2") + "℃";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte[] writebuffer = BitConverter.GetBytes((UInt16)TempSOA_trackBar.Value);
            Array.Reverse(writebuffer); //高字节在前
            i2c.TWI_WritePage(0xA0, 0xCE, writebuffer, 2);
        }

        private void BiasSOA_trackBar_ValueChanged(object sender, EventArgs e)
        {
            BiasSOA_value_textBox.Text = BiasSOA_trackBar.Value.ToString();

            float vsoa = (float)(BiasSOA_trackBar.Value * (2.4 / 4095));
            SOA_V_textBox.Text = vsoa.ToString("F2") + "V";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            //byte i = Convert.ToByte(channelSel_comboBox.Text);
            //i += 0xBC;

            byte[] writebuffer = BitConverter.GetBytes((UInt16)BiasSOA_trackBar.Value);
            Array.Reverse(writebuffer); //高字节在前
            i2c.TWI_WritePage(0xA0, 0xDE, writebuffer, 2);
        }

        private void RxSOA_LUT_DIS_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte[] pwd = new byte[4];
            pwd[0] = 0x00;
            pwd[1] = 0x00;
            pwd[2] = 0x00;
            pwd[3] = 0x00;
            i2c.TWI_ReadPage(0xA0, 123, pwd, 4);
            if ((pwd[0] != 0xA9) || (pwd[1] != 0x46) || (pwd[2] != 0x50) || (pwd[3] != 0x54))
            {
                MessageBox.Show("待测QSFP模块不在调试模式下，无法切换，请确认！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte wrtBuf = 0;

            wrtBuf = i2c.TWI_ReadByte(0xA0, 0xFF);

            if (RxSOA_LUT_DIS_checkBox.Checked)
            {
                wrtBuf |= 0x10; //bit4=1 disable SOA soft control LUT
            }
            else
            {
                wrtBuf &= 0xEF; //bit4=0 enable SOA soft control LUT
            }

            if (!i2c.TWI_WriteByte(0xA0, 0xFF, wrtBuf))
            {
                MessageBox.Show("发送RxSOA自动补偿命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dualRate_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            byte[] pwd = new byte[4];
            pwd[0] = 0x00;
            pwd[1] = 0x00;
            pwd[2] = 0x00;
            pwd[3] = 0x00;
            i2c.TWI_ReadPage(0xA0, 123, pwd, 4);
            if ((pwd[0] != 0xA9) || (pwd[1] != 0x46) || (pwd[2] != 0x50) || (pwd[3] != 0x54))
            {
                MessageBox.Show("待测QSFP模块不在调试模式下，无法切换，请确认！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 0x9A);

            if (dualRate_checkBox.Checked)
            {
                wrtBuf |= 0x01; //bit0=1 enable dual rate 100G/112G
            }
            else
            {
                wrtBuf &= 0xFE; //bit0=0 disable dual rate 100G/112G
            }
            i2c.TWI_WriteByte(0xA0, 0x9A, wrtBuf);
        }

        private void SOA_RSSI_W_button_Click(object sender, EventArgs e)
        {
            byte[] writebuffer = new byte[3];
            UInt16 uitmp;
            byte ch;

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            SOA_RSSI_W_button.Enabled = false;
            SelectTable(0x0B);

            uitmp = Convert.ToUInt16(SOA_RSSIval_textBox.Text);
            if (uitmp > 4000)
            {
                uitmp = 4000;
            }

            ch = Convert.ToByte(SOA_RSSIchannel_textBox.Text);
            writebuffer[0] = (byte)(uitmp >> 8);
            writebuffer[1] = (byte)(uitmp & 0x00FF);

            if (ch > 3)
            {
                ch = 3;
            }

            ch *= 2;
            ch += 0x89;

            i2c.TWI_WritePage(0xA0, ch, writebuffer, 2);

            SelectTable(0x06);
            SOA_RSSI_W_button.Enabled = true;
        }

        private void SOACal_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //SOACal_Form soaCal_form = new SOACal_Form(this.i2c);           
            timer1.Stop();
            using (var soaCal_form = new SOACal_Form(this.i2c))
            {
                soaCal_form.ShowDialog();
            }          
            timer1.Start();
        }

        private void btnOPSIX_Click(object sender, EventArgs e)
        {
            string str = "";
            try
            {
                if (PSSOPSIX != null)
                {
                    if (PSSOPSIX.IsOpen)
                    {
                        PSSOPSIX.Close();
                    }
                }
                str = PSSOPSIX_Connect();
                if (str.Contains("PSS"))
                {
                    btnOPSIX.BackColor = System.Drawing.Color.Yellow;
                }
                else
                {
                    btnOPSIX.BackColor = System.Drawing.Color.Gray;
                    PSSOPSIX = null;
                }
            }
            catch
            {            
                PSSOPSIX = null;
                MessageBox.Show("光开关连接异常");
            }
        }

        private string PSSOPSIX_Connect()
        {
            byte[] WriteBuffer = new byte[7] { 0x2A, 0x49, 0x44, 0x4E, 0x3F, 0x0D, 0x0A };
            byte[] ReadBuffer = new byte[40];
            PSSOPSIX = new SerialPort();   //
            PSSOPSIX.PortName = OPSIX_comboBox.Text;
            PSSOPSIX.BaudRate = 115200;
            PSSOPSIX.ReadTimeout = 1000;
            PSSOPSIX.Open();
            ReadBuffer[0] = 0xFF;
            // uartAtt.Write(WriteBuffer, 0, 7);
            string command = "*IDN?";
            string[] arry = command.Split(' ');
            byte[] b = new byte[arry.Length];
            string str;

            PSSOPSIX.WriteLine(command);
            Thread.Sleep(1000);
            str = PSSOPSIX.ReadLine();
            // listBox1.Items.Add(str);

            command = "Configure:WorkChannel?";
            PSSOPSIX.WriteLine(command);
            str += PSSOPSIX.ReadLine();
            return str;
        }

        private void OPSIX_CH_Select(int ch)
        {
            string command = "Configure:WorkChannel " + ch.ToString();
            string chnum;
            try
            {
                PSSOPSIX.WriteLine(command);
                Thread.Sleep(1000);
                chnum = PSSOPSIX.ReadLine();
            }
            catch
            {
                MessageBox.Show("光开关通道切换异常");
            }
            //listBox1.Items.Add(chnum);
        }

        private void channelSel_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte i = Convert.ToByte(channelSel_comboBox.Text);
            if (cBOPSIX.Checked)//光开关
            {
                if (PSSOPSIX == null)
                {
                    // PSSOPSIX_Connect();
                    byte ch = Convert.ToByte(channelSel_comboBox.Text);
                    TxChEnable(ch);
                }
                else
                {
                    OPSIX_CH_Select(i + 1);//通道选择
                }
            }
        }

        private void checkBox_LpMode_en_CheckedChanged(object sender, EventArgs e)
        {
            byte wrtBuf = i2c.TWI_ReadByte(0xa0, 93);

            if (checkBox_LpMode_en.Checked)
            {
                wrtBuf |= 0x01;
            }
            else
            {
                wrtBuf &= 0xFE;
            }
            i2c.TWI_WriteByte(0xA0, 93, wrtBuf);
        }

        private void cBRxVagc_CheckedChanged(object sender, EventArgs e)
        {
            if (cBRxVagc.Checked)
            {
                trackBar_RxVagc.Enabled = true;
            }
            else
            {
                trackBar_RxVagc.Enabled = false;
            }
        }

        private void trackBar_RxVagc_ValueChanged(object sender, EventArgs e)
        {
            tBRxVagc.Text = trackBar_RxVagc.Value.ToString();

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0x9B;

            i2c.TWI_WriteByte(0xa0, i, (byte)trackBar_RxVagc.Value);
        }

        private void eEPROMMAPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //EEPROM_MAP_Form eeprom_form = new EEPROM_MAP_Form(this.i2c);
            timer1.Stop();
            using (var eeprom_form = new EEPROM_MAP_Form(this.i2c))
            {
                eeprom_form.ShowDialog();
            }
            timer1.Start();
        }

        private void dSPRegDebugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DSPRegDebugForm dspdebug_form = new DSPRegDebugForm(this.i2c);
            timer1.Stop();
            using (var dspdebug_form = new DSPRegDebugForm(this.i2c))
            {
                dspdebug_form.ShowDialog();
            }
            timer1.Start();
        }

        private void dSPcfgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DSPCFGFrm dapcfg_form = new DSPCFGFrm(this.i2c);
            timer1.Stop();
            using (var dapcfg_form = new DSPCFGFrm(this.i2c))
            {
                dapcfg_form.ShowDialog();
            }
            timer1.Start();
        }
     
        private void cBddm4chEn_CheckedChanged(object sender, EventArgs e)
        {
            byte temp = 0;
            SelectTable(6);
            i2c.TWI_ReadByte(0xA0, 0x9A);
            if (cBddm4chEn.Checked)
            {
                temp &= 0xEF;
                lbchddm.Text = "4通道DDM";
            }
            else
            {
                temp |= 0x10;
                lbchddm.Text = "1通道DDM";
            }
            i2c.TWI_WriteByte(0xA0, 0x9A, temp);
        }

        private void cBFECEn_CheckedChanged(object sender, EventArgs e)
        {
            byte temp = 1;
            byte[] pwdread = new byte[4];
            //byte[] pwd_dsp = { 0xAA, 0xA5, 0x5A, 0xAA };
            //i2c.TWI_ReadPage(0xA0, 0x7B, pwdread, 4);
            //if ((pwdread[0] == 0xA9) && (pwdread[1] == 0x46)&& (pwdread[2] == 0x50)&& (pwdread[3] == 0x54))
            //{
            //    //
            //}
            //else
            //{
            //    i2c.TWI_WritePage(0xA0, 0x7B, pwd_dsp, 4);
            //    Thread.Sleep(20);
            //}

            SelectTable(0xB0);
            if (cBFECEn.Checked)
            {
                i2c.TWI_WriteByte(0xA0, 0xE8, 0x01);
            }
            else
            {
                i2c.TWI_WriteByte(0xA0, 0xE8, 0x00);
            }
            //DSP更新
            i2c.TWI_WriteByte(0xA0, 0xE9, 0x01);
            Thread.Sleep(500);
            temp = i2c.TWI_ReadByte(0xA0, 0xE9);
            int loop = 0;
            while ((temp == 1) && (loop < 10))
            {
                temp = i2c.TWI_ReadByte(0xA0, 0xE9);
                Thread.Sleep(20);
                loop++;
            }
            temp = i2c.TWI_ReadByte(0xA0, 0xE8);
            if (temp == 0)
            {
                lbfecstate.Text = "关闭";               
            }
            if (temp == 1)
            {
                lbfecstate.Text = "开启";
            }
            SetRedLED(pictureBox_FEC_En, !Bit.GetBit(temp, 0));
        }

        private void cBRxVagcInitval_CheckedChanged(object sender, EventArgs e)
        {
            if (cBRxVagcInitval.Checked)
            {
                if (tBRxVagcInitVal.Text == "")
                {
                    MessageBox.Show("固定值不能为空");
                    cBRxVagcInitval.Checked = false;
                    return;
                }
                tBRxVagcInitVal.ReadOnly = false;
                trackBar_RxVagc.Enabled = false;
            }
            else
            {
                tBRxVagcInitVal.ReadOnly = true;
                trackBar_RxVagc.Enabled = true;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            byte val = 0;
            val = i2c.TWI_ReadByte(0xA0, 93);
            val |= 0x80;
            i2c.TWI_WriteByte(0xA0, 93, val);//复位
        }
     
        private void button1_Click(object sender, EventArgs e)
        {
            byte val = 0;
            SelectTable(0x03);
            val = i2c.TWI_ReadByte(0xa0, 230);          
            val &= 0xBF;//bit6:0 EN
            i2c.TWI_WriteByte(0xa0, 230, val);
        }

        private void btnFECDis_Click(object sender, EventArgs e)
        {
            byte val = 0;
            SelectTable(0x03);
            val = i2c.TWI_ReadByte(0xa0, 230);
            val |= 0x40;//bit6:1 Dis
            i2c.TWI_WriteByte(0xa0, 230, val);
        }

        private void trackBar_RxVagc_Scroll(object sender, EventArgs e)
        {

        }
    }
}

