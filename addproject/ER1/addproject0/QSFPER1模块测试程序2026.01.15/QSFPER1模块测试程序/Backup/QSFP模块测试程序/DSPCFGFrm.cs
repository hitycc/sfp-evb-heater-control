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

namespace XFP模块测试程序
{
    public partial class DSPCFGFrm : Form
    {
        I2C i2c;
        byte Chanel;
        public DSPCFGFrm()
        {
            InitializeComponent();
        }

        public DSPCFGFrm(I2C i2c)
        {            
            InitializeComponent();
            this.i2c = i2c;
            timer1.Start();

            channelSel_comboBox.Items.Clear();
            channelSel_comboBox.Items.Add("0");
            channelSel_comboBox.Items.Add("1");
            channelSel_comboBox.Items.Add("2");
            channelSel_comboBox.Items.Add("3");
            channelSel_comboBox.SelectedIndex = 0;
            Chanel = (byte)channelSel_comboBox.SelectedIndex;

            cbBFECMode.Items.Clear();
            cbBFECMode.Items.Add("FEC_BYPASS");
            cbBFECMode.Items.Add("FEC_REGEN");
            cbBFECMode.Items.Add("TP_GEN");
            //
            cbBDSPMode.Items.Clear();
            cbBDSPMode.Items.Add("Mission");
            cbBDSPMode.Items.Add("Hlpbk");
            cbBDSPMode.Items.Add("Llpbk");
            cbBDSPMode.Items.Add("HPRBS");
            cbBDSPMode.Items.Add("LPRBS");
            //
            cbBPrbsPatt.Items.Clear();
            cbBPrbsPatt.Items.Add("P31");
            cbBPrbsPatt.Items.Add("P23");
            cbBPrbsPatt.Items.Add("P15");
            cbBPrbsPatt.Items.Add("P13");
            cbBPrbsPatt.Items.Add("P9");
            cbBPrbsPatt.Items.Add("P7");
            cbBPrbsPatt.Items.Add("SSPRQ");
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
        #region
        /*  private void LTx_TAP0_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
           
            i = Chanel;
            i += 0xC0;
            LTx_TAP0_textBox.Text = LTx_TAP0_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP0_trackBar.Value);
            byte Btemp = 0;
            Btemp = writebuf[0];           
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            //writebuf[0] = (byte)LTx_TAP0_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP0_trackBar.Value >> 8);          
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            i2c.TWI_WritePage(0xa0, 0xB4, writebuf, 2);
            lbLTx_TAP0.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void LTx_TAP1_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_TAP1_textBox.Text = LTx_TAP1_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP1_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP1_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP1_trackBar.Value >> 8);
            SelectTable(i);
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            i2c.TWI_WritePage(0xa0, 0xB6, writebuf, 2);
        }

        private void LTx_TAP2_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_TAP2_textBox.Text = LTx_TAP2_trackBar.Value.ToString();
            writebuf[0] = (byte)LTx_TAP2_trackBar.Value;
            //writebuf = ValuetoByte(LTx_TAP2_trackBar.Value);
            //writebuf[1] = (byte)(LTx_TAP2_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xB8, writebuf, 2);
        }

        private void LTx_TAP3_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_TAP3_textBox.Text = LTx_TAP3_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP3_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xBA, writebuf, 2);
        }

        private void LTx_TAP4_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_TAP4_textBox.Text = LTx_TAP3_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP4_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xBC, writebuf, 2);
        }

        private void LTx_TAP5_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_TAP5_textBox.Text = LTx_TAP5_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP5_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xBE, writebuf, 2);
        }

        private void LTx_TAP6_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_TAP6_textBox.Text = LTx_TAP6_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP6_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC0, writebuf, 2);
        }

        private void HTx_TAP0_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP0_textBox.Text = HTx_TAP0_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP0_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP0_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP0_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC6, writebuf, 2);
            lbHTx_TAP0.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void HTx_TAP1_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP1_textBox.Text = HTx_TAP1_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP1_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP1_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP1_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC8, writebuf, 2);
            lbHTx_TAP1.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void HTx_TAP2_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP2_textBox.Text = HTx_TAP2_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP2_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP2_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP2_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xCA, writebuf, 2);
            lbHTx_TAP2.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void HTx_TAP3_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP3_textBox.Text = HTx_TAP3_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP3_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xCC, writebuf, 2);
            lbHTx_TAP3.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void LTx_EYE1_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_EYE1_textBox.Text = LTx_EYE1_trackBar.Value.ToString();
            writebuf[0] = (byte)LTx_EYE1_trackBar.Value;
            writebuf[1] = (byte)(LTx_EYE1_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC2, writebuf, 2);
            lbLTx_EYE1.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void LTx_EYE2_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_EYE2_textBox.Text = LTx_EYE2_trackBar.Value.ToString();
            writebuf[0] = (byte)LTx_EYE2_trackBar.Value;
            writebuf[1] = (byte)(LTx_EYE2_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC4, writebuf, 2);
            lbLTx_EYE2.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void HTx_EYE1_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_EYE1_textBox.Text = HTx_EYE1_trackBar.Value.ToString();
            writebuf[0] = (byte)HTx_EYE1_trackBar.Value;
            writebuf[1] = (byte)(HTx_EYE1_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xCE, writebuf, 2);
            lbHTx_EYE1.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }

        private void HTx_EYE2_trackBar_Scroll(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_EYE2_textBox.Text = HTx_EYE2_trackBar.Value.ToString();
            writebuf[0] = (byte)HTx_EYE2_trackBar.Value;
            writebuf[1] = (byte)(HTx_EYE2_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xD0, writebuf, 2);
            lbHTx_EYE2.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
        }*/
        #endregion
        private void btnRead_Click(object sender, EventArgs e)
        {
            byte[] readbuffer = new byte[40];
            byte i = 0;
            int value = 0;

            if (!WriteVenderPWD())
            {
                MessageBox.Show("写入厂商密码出错！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectTable(0xC0);
            byte val_r = i2c.TWI_ReadByte(0xA0, 0xF5);
            cbBPolarity.SelectedIndex = val_r;

            i = Chanel;
            i += 0xC0;
            SelectTable(i);
            i2c.TWI_ReadPage(0xa0, 0xB4, readbuffer, 34);           
            value = BytetoValue(readbuffer[0], readbuffer[1]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            LTx_TAP0_trackBar.Value = value;//readbuffer[0] * 256 + readbuffer[1];
            LTx_TAP0_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[2], readbuffer[3]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            LTx_TAP1_trackBar.Value = value;//readbuffer[2] * 256 + readbuffer[3];
            LTx_TAP1_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[4], readbuffer[5]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            LTx_TAP2_trackBar.Value = value;//readbuffer[4] * 256 + readbuffer[5];
            LTx_TAP2_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[6], readbuffer[7]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            LTx_TAP3_trackBar.Value = value;//readbuffer[6] * 256 + readbuffer[7];
            LTx_TAP3_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[8], readbuffer[9]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            LTx_TAP4_trackBar.Value = value;//readbuffer[6] * 256 + readbuffer[7];
            LTx_TAP4_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[10], readbuffer[11]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            LTx_TAP5_trackBar.Value = value;//readbuffer[6] * 256 + readbuffer[7];
            LTx_TAP5_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[12], readbuffer[13]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            LTx_TAP6_trackBar.Value = value;//readbuffer[6] * 256 + readbuffer[7];
            LTx_TAP6_textBox.Text = value.ToString();
            if (cBEYE3_4_EN.Checked == false)
            {
                value = BytetoValue(readbuffer[14], readbuffer[15]);
                if (value > 1500)
                {
                    value = 1500;
                }
                if (value < 500)
                {
                    value = 500;
                }
                LTx_EYE1_trackBar.Value = value;//readbuffer[14] * 256 + readbuffer[15];
                LTx_EYE1_textBox.Text = value.ToString();

                value = BytetoValue(readbuffer[16], readbuffer[17]);
                if (value > 2500)
                {
                    value = 2500;
                }
                if (value < 1500)
                {
                    value = 1500;
                }
                LTx_EYE2_trackBar.Value = value;//readbuffer[16] * 256 + readbuffer[17];
                LTx_EYE2_textBox.Text = value.ToString();
            }
            else
            {
                if (((sbyte)readbuffer[14] > -17) && ((sbyte)readbuffer[14] < 17))
                {
                    LTx_EYE1_trackBar.Value = (sbyte)readbuffer[14];
                    LTx_EYE1_textBox.Text = ((sbyte)readbuffer[14]).ToString();
                }
                else
                {
                    MessageBox.Show("LTx_EYE读取错误");
                    return;
                }
                if (((sbyte)readbuffer[15] > -17) && ((sbyte)readbuffer[15] < 17))
                {
                    LTx_EYE2_trackBar.Value = (sbyte)readbuffer[15];
                    LTx_EYE2_textBox.Text = ((sbyte)readbuffer[15]).ToString();
                }
                else
                {
                    MessageBox.Show("LTx_EYE读取错误");
                    return;
                }
                if (((sbyte)readbuffer[16] > -17) && ((sbyte)readbuffer[16] < 17))
                {
                    LTx_EYE3_trackBar.Value = (sbyte)readbuffer[16];
                    LTx_EYE3_textBox.Text = ((sbyte)readbuffer[16]).ToString();
                }
                else
                {
                    MessageBox.Show("LTx_EYE读取错误");
                    return;
                }
                if (((sbyte)readbuffer[17] > -17) && ((sbyte)readbuffer[17] < 17))
                {
                    LTx_EYE4_trackBar.Value = (sbyte)readbuffer[17];
                    LTx_EYE4_textBox.Text = ((sbyte)readbuffer[17]).ToString();
                }
                else
                {
                    MessageBox.Show("LTx_EYE读取错误");
                    return;
                }
            }
            value = BytetoValue(readbuffer[18], readbuffer[19]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            HTx_TAP0_trackBar.Value = value;//readbuffer[18] * 256 + readbuffer[19];
            HTx_TAP0_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[20], readbuffer[21]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            HTx_TAP1_trackBar.Value = value;//readbuffer[20] * 256 + readbuffer[21];
            HTx_TAP1_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[22], readbuffer[23]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            HTx_TAP2_trackBar.Value = value;//readbuffer[22] * 256 + readbuffer[23];
            HTx_TAP2_textBox.Text = value.ToString();

            value = BytetoValue(readbuffer[24], readbuffer[25]);
            if (value > 1000)
            {
                value = 1000;
            }
            if (value < -1000)
            {
                value = -1000;
            }
            HTx_TAP3_trackBar.Value = value;//readbuffer[24] * 256 + readbuffer[25];
            HTx_TAP3_textBox.Text = value.ToString();
            if (cBEYE3_4_EN.Checked == false)
            {
                value = BytetoValue(readbuffer[26], readbuffer[27]);
                if (value > 1500)
                {
                    value = 1500;
                }
                if (value < 500)
                {
                    value = 500;
                }
                HTx_EYE1_trackBar.Value = value;//readbuffer[26] * 256 + readbuffer[27];
                HTx_EYE1_textBox.Text = value.ToString();

                value = BytetoValue(readbuffer[28], readbuffer[29]);
                if (value > 2500)
                {
                    value = 2500;
                }
                if (value < 1500)
                {
                    value = 1500;
                }
                HTx_EYE2_trackBar.Value = value;//readbuffer[28] * 256 + readbuffer[29];
                HTx_EYE2_textBox.Text = value.ToString();
            }
            else
            {
                if (((sbyte)readbuffer[26] > -17) && ((sbyte)readbuffer[26] < 17))
                {
                    HTx_EYE1_trackBar.Value = (sbyte)readbuffer[26];
                    HTx_EYE1_textBox.Text = ((sbyte)readbuffer[26]).ToString();
                }
                else
                {
                    MessageBox.Show("HTx_EYE读取错误");
                    return;
                }
                if (((sbyte)readbuffer[27] > -17) && ((sbyte)readbuffer[27] < 17))
                {
                    HTx_EYE2_trackBar.Value = (sbyte)readbuffer[27];
                    HTx_EYE2_textBox.Text = ((sbyte)readbuffer[27]).ToString();
                }
                else
                {
                    MessageBox.Show("HTx_EYE读取错误");
                    return;
                }
                if (((sbyte)readbuffer[28] > -17) && ((sbyte)readbuffer[28] < 17))
                {
                    HTx_EYE3_trackBar.Value = (sbyte)readbuffer[28];
                    HTx_EYE3_textBox.Text = ((sbyte)readbuffer[28]).ToString();
                }
                else
                {
                    MessageBox.Show("HTx_EYE读取错误");
                    return;
                }
                if (((sbyte)readbuffer[29] > -17) && ((sbyte)readbuffer[29] < 17))
                {
                    HTx_EYE4_trackBar.Value = (sbyte)readbuffer[29];
                    HTx_EYE4_textBox.Text = ((sbyte)readbuffer[29]).ToString();
                }
                else
                {
                    MessageBox.Show("HTx_EYE读取错误");
                    return;
                }
            }

        }

        private int BytetoValue(byte val1, byte val2)
        {
            byte[] Dbyte = new byte[2];
            Dbyte[1] = val1;
            Dbyte[0] = val2;
            return BitConverter.ToInt16(Dbyte, 0); // 自动解析补码 
        }

        private byte[] ValuetoByte(int val)
        {
            return BitConverter.GetBytes(val); // 自动处理补码转换
        }
        // 表选择
        private bool SelectTable(byte tbl)
        {
            return i2c.TWI_WriteByte(0xA0, 127, tbl);
        }

        //private void channelSel_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    Chanel = (byte)channelSel_comboBox.SelectedIndex;
        //}

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (SelectTable(0xC0) == false)
            {
                MessageBox.Show("表选失败");
                return;
            }
            i2c.TWI_WriteByte(0xA0, 0xF5, (byte)cbBPolarity.SelectedIndex);//极性

            if (cBSaveTapDefval.Checked)
            {
                if (SelectTable(0xC0) == false)
                {
                    MessageBox.Show("保存默认Tap值表选失败");
                    return;
                }
                if (i2c.TWI_WritePage(0xa0, 0xB4, Main_Form.tap_L, 18) != 18)
                {
                    MessageBox.Show("保存默认Tap_L值失败");
                    return;
                }
                //if (i2c.TWI_WritePage(0xa0, 0xC6, Main_Form.tap_H, 22) != 22)
                //{
                //    MessageBox.Show("保存默认Tap_H值失败");
                //    return;
                //}
                if (i2c.TWI_WriteByte(0xa0, 0xF5, Main_Form.dsp_pol) == false)//极性默认值
                {
                    MessageBox.Show("保存默认TxRxPolInv 极性值失败");
                    return;
                }
            }

            // 发送保存命令
            byte[] saveByte = new byte[3];
            saveByte[0] = 0x08; // threshold  Page03   bit3  0x82地址
            saveByte[1] = 0x0D; // 00001101  bit3 bit2 bit0  0x83地址
           
            saveByte[1] |= 0x40; //bit6=1 SOA_LUT
            saveByte[1] |= 0x80; //bit7=1 DSP
            //Save data to flash
            if (!SelectTable(0x06))
            {
                MessageBox.Show("保存命令：选择表6错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (i2c.TWI_WritePage(0xa0, 0x82, saveByte, 2) != 2)
            {
                MessageBox.Show("保存命令：发送保存命令错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Thread.Sleep(1000);
            MessageBox.Show("保存成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
               
        private void LTx_TAP0_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_TAP0_textBox.Text = LTx_TAP0_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP0_trackBar.Value);
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            //writebuf[0] = (byte)LTx_TAP0_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP0_trackBar.Value >> 8);          
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            i2c.TWI_WritePage(0xa0, 0xB4, writebuf, 2);
            lbLTx_TAP0.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void LTx_TAP1_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_TAP1_textBox.Text = LTx_TAP1_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP1_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP1_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP1_trackBar.Value >> 8);
            SelectTable(i);
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            i2c.TWI_WritePage(0xa0, 0xB6, writebuf, 2);
            lbLTx_TAP1.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void LTx_TAP2_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_TAP2_textBox.Text = LTx_TAP2_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP2_trackBar.Value);
            //writebuf = ValuetoByte(LTx_TAP2_trackBar.Value);
            //writebuf[1] = (byte)(LTx_TAP2_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xB8, writebuf, 2);
            lbLTx_TAP2.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void LTx_TAP3_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_TAP3_textBox.Text = LTx_TAP3_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP3_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xBA, writebuf, 2);
            lbLTx_TAP3.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void LTx_TAP4_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_TAP4_textBox.Text = LTx_TAP4_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP4_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xBC, writebuf, 2);
            lbLTx_TAP4.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void LTx_TAP5_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_TAP5_textBox.Text = LTx_TAP5_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP5_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xBE, writebuf, 2);
            lbLTx_TAP5.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void LTx_TAP6_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_TAP6_textBox.Text = LTx_TAP6_trackBar.Value.ToString();
            writebuf = ValuetoByte(LTx_TAP6_trackBar.Value);
            //writebuf[0] = (byte)LTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(LTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC0, writebuf, 2);
            lbLTx_TAP6.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void LTx_EYE1_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            Chanel = 0;
            i = Chanel;
            i += 0xC0;
            LTx_EYE1_textBox.Text = LTx_EYE1_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == false)
            {
                writebuf[0] = (byte)LTx_EYE1_trackBar.Value;
                writebuf[1] = (byte)(LTx_EYE1_trackBar.Value >> 8);
                SelectTable(i);
               
                byte Btemp = 0;
                Btemp = writebuf[0];
                writebuf[0] = writebuf[1];
                writebuf[1] = Btemp;
                i2c.TWI_WritePage(0xa0, 0xC2, writebuf, 2);
                lbLTx_EYE1.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            }
            else
            {
                writebuf[0] = (byte)LTx_EYE1_trackBar.Value;
                SelectTable(i);              
                i2c.TWI_WritePage(0xa0, 0xC2, writebuf, 1);
                lbLTx_EYE1.Text = writebuf[0].ToString("X2");
            }
        }

        private void LTx_EYE2_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            Chanel = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_EYE2_textBox.Text = LTx_EYE2_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == false)
            {
                writebuf[0] = (byte)LTx_EYE2_trackBar.Value;
                writebuf[1] = (byte)(LTx_EYE2_trackBar.Value >> 8);
                SelectTable(i);
                //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
                //    return;
                byte Btemp = 0;
                Btemp = writebuf[0];
                writebuf[0] = writebuf[1];
                writebuf[1] = Btemp;
                i2c.TWI_WritePage(0xa0, 0xC4, writebuf, 2);
                lbLTx_EYE2.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            }
            else
            {
                writebuf[0] = (byte)(LTx_EYE2_trackBar.Value);
                SelectTable(i);                             
                i2c.TWI_WriteByte(0xa0, 0xC3, writebuf[0]);
                lbLTx_EYE2.Text = writebuf[0].ToString("X2");
            }
        }

        private void HTx_TAP0_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP0_textBox.Text = HTx_TAP0_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP0_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP0_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP0_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC6, writebuf, 2);
            lbHTx_TAP0.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void HTx_TAP1_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP1_textBox.Text = HTx_TAP1_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP1_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP1_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP1_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xC8, writebuf, 2);
            lbHTx_TAP1.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void HTx_TAP2_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP2_textBox.Text = HTx_TAP2_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP2_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP2_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP2_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xCA, writebuf, 2);
            lbHTx_TAP2.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void HTx_TAP3_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_TAP3_textBox.Text = HTx_TAP3_trackBar.Value.ToString();
            writebuf = ValuetoByte(HTx_TAP3_trackBar.Value);
            //writebuf[0] = (byte)HTx_TAP3_trackBar.Value;
            //writebuf[1] = (byte)(HTx_TAP3_trackBar.Value >> 8);
            SelectTable(i);
            //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
            //    return;
            byte Btemp = 0;
            Btemp = writebuf[0];
            writebuf[0] = writebuf[1];
            writebuf[1] = Btemp;
            i2c.TWI_WritePage(0xa0, 0xCC, writebuf, 2);
            lbHTx_TAP3.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            TapAbsSum();
        }

        private void HTx_TAP4_trackBar_ValueChanged(object sender, EventArgs e)
        {

        }

        private void HTx_TAP5_trackBar_ValueChanged(object sender, EventArgs e)
        {

        }

        private void HTx_TAP6_trackBar_ValueChanged(object sender, EventArgs e)
        {

        }

        private void HTx_EYE1_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_EYE1_textBox.Text = HTx_EYE1_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == false)
            {
                writebuf = ValuetoByte(HTx_EYE1_trackBar.Value);
                //writebuf[0] = (byte)HTx_EYE1_trackBar.Value;
                //writebuf[1] = (byte)(HTx_EYE1_trackBar.Value >> 8);
                SelectTable(i);
                //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
                //    return;
                byte Btemp = 0;
                Btemp = writebuf[0];
                writebuf[0] = writebuf[1];
                writebuf[1] = Btemp;
                i2c.TWI_WritePage(0xa0, 0xCE, writebuf, 2);
                lbHTx_EYE1.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            }
            else
            {
                writebuf[0] = (byte)HTx_EYE1_trackBar.Value;               
                SelectTable(i);                          
                i2c.TWI_WritePage(0xa0, 0xCE, writebuf, 1);
                lbHTx_EYE1.Text = writebuf[0].ToString("X2");
            }
        }

        private void HTx_EYE2_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_EYE2_textBox.Text = HTx_EYE2_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == false)
            {
                writebuf = ValuetoByte(HTx_EYE2_trackBar.Value);
                //writebuf[0] = (byte)HTx_EYE2_trackBar.Value;
                //writebuf[1] = (byte)(HTx_EYE2_trackBar.Value >> 8);
                SelectTable(i);
                //if (i2c.TWI_ReadByte(0xA0, 127) != (byte)(i))
                //    return;
                byte Btemp = 0;
                Btemp = writebuf[0];
                writebuf[0] = writebuf[1];
                writebuf[1] = Btemp;
                i2c.TWI_WritePage(0xa0, 0xD0, writebuf, 2);
                lbHTx_EYE2.Text = writebuf[0].ToString("X2") + writebuf[1].ToString("X2");
            }
            else
            {
                writebuf[0] = (byte)HTx_EYE2_trackBar.Value;
                i2c.TWI_WritePage(0xa0, 0xCF, writebuf, 1);
                lbHTx_EYE2.Text = writebuf[0].ToString("X2");
            }
        }

        private void LTx_TAP0_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_TAP0_textBox.Text);
                LTx_TAP0_trackBar.Value = num;

            }
            catch { }
        }


        private void LTx_TAP1_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_TAP1_textBox.Text);
                LTx_TAP1_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_TAP2_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_TAP2_textBox.Text);
                LTx_TAP2_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_TAP3_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_TAP3_textBox.Text);
                LTx_TAP3_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_TAP4_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_TAP4_textBox.Text);
                LTx_TAP4_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_TAP5_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_TAP5_textBox.Text);
                LTx_TAP5_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_TAP6_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_TAP6_textBox.Text);
                LTx_TAP6_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_EYE1_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_EYE1_textBox.Text);
                LTx_EYE1_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_EYE2_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_EYE2_textBox.Text);
                LTx_EYE2_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_TAP0_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_TAP0_textBox.Text);
                HTx_TAP0_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_TAP1_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_TAP1_textBox.Text);
                HTx_TAP1_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_TAP2_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_TAP2_textBox.Text);
                HTx_TAP2_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_TAP3_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_TAP3_textBox.Text);
                HTx_TAP3_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_TAP4_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_TAP4_textBox.Text);
                HTx_TAP4_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_TAP5_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_TAP5_textBox.Text);
                HTx_TAP5_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_TAP6_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_TAP6_textBox.Text);
                HTx_TAP6_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_EYE1_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_EYE1_textBox.Text);
                HTx_EYE1_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_EYE2_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_EYE2_textBox.Text);
                HTx_EYE2_trackBar.Value = num;

            }
            catch { }
        }

        private void TapAbsSum()
        {
            byte i = 0;
            byte[] readbuf = new byte[14];
            int value = 0;
            i = Chanel;
            i += 0xC0;
            SelectTable(i);
            i2c.TWI_ReadPage(0xA0, 0xB4, readbuf, 14);
            for (int x = 0; x < 7; x++)
            {
                value += Math.Abs(BytetoValue(readbuf[x*2], readbuf[x*2+1]));
            }
            lbTapAbsL.Text = "Abs_Sum：" + value.ToString();
            //
            value = 0;
            i2c.TWI_ReadPage(0xA0, 0xC6, readbuf, 8);
            for (int x = 0; x < 4; x++)
            {
                value += Math.Abs(BytetoValue(readbuf[x * 2], readbuf[x * 2 + 1]));
            }
            lbTapAbsH.Text = "Abs_Sum：" + value.ToString();
        }

        private void cbBFECMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte temp = 0;
            SelectTable(0xC0);
            temp = (byte)cbBFECMode.SelectedIndex;//i2c.TWI_ReadByte(0xA0,0xE8);
            switch(temp)
            {
                case 0:
                    i2c.TWI_WriteByte(0xA0, 0xF4, 0x00);
                    break;
                case 1:
                    i2c.TWI_WriteByte(0xA0, 0xF4, 0x01);
                    break;
                case 2:
                    i2c.TWI_WriteByte(0xA0, 0xF4, 0x03);
                    break;               
                default:
                    break;
            }
        }

        private void cbBDSPMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            int mode = cbBDSPMode.SelectedIndex;
            byte[] pwd_dsp = { 0xAA, 0xA5, 0x5A, 0xAA };
            i2c.TWI_WritePage(0xA0, 0x7B, pwd_dsp, 4);
            Thread.Sleep(20);
            SelectTable(0xB0);
            i2c.TWI_WriteByte(0xA0, 0xE6, (byte)mode); 
        }

        private void cbBPrbsPatt_SelectedIndexChanged(object sender, EventArgs e)
        {
            int pattern = cbBDSPMode.SelectedIndex;
            byte[] pwd_dsp = { 0xAA, 0xA5, 0x5A, 0xAA };
            i2c.TWI_WritePage(0xA0, 0x7B, pwd_dsp, 4);
            Thread.Sleep(20);
            SelectTable(0xB0);
            i2c.TWI_WriteByte(0xA0, 0xE7, (byte)pattern);
        }

        private void channelSel_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DSPCFGFrm_Load(object sender, EventArgs e)
        {

        }

        private void cBEYE3_4_EN_CheckedChanged(object sender, EventArgs e)
        {
            if (cBEYE3_4_EN.Checked)
            {
                LTx_EYE3_trackBar.Enabled = true;
                LTx_EYE4_trackBar.Enabled = true;
                LTx_EYE3_textBox.Enabled = true;
                LTx_EYE4_textBox.Enabled = true;

                HTx_EYE3_trackBar.Enabled = true;
                HTx_EYE4_trackBar.Enabled = true;
                HTx_EYE3_textBox.Enabled = true;
                HTx_EYE4_textBox.Enabled = true;

                LTx_EYE1_trackBar.Maximum = 16;
                LTx_EYE1_trackBar.Minimum = -16;
                LTx_EYE2_trackBar.Maximum = 16;
                LTx_EYE2_trackBar.Minimum = -16;
                //LTx_EYE1_trackBar.Value = -16;
                //LTx_EYE2_trackBar.Value = -16;
                lbLTx_EYE1.Text = "00";
                lbLTx_EYE2.Text = "00";

                HTx_EYE1_trackBar.Maximum = 16;
                HTx_EYE1_trackBar.Minimum = -16;
                HTx_EYE2_trackBar.Maximum = 16;
                HTx_EYE2_trackBar.Minimum = -16;
                //HTx_EYE1_trackBar.Value = 0;
                //HTx_EYE2_trackBar.Value = 0;
                lbHTx_EYE1.Text = "00";
                lbHTx_EYE2.Text = "00";

            }
            else
            {
                LTx_EYE3_trackBar.Enabled = false;
                LTx_EYE4_trackBar.Enabled = false;
                LTx_EYE3_textBox.Enabled = false;
                LTx_EYE4_textBox.Enabled = false;

                HTx_EYE3_trackBar.Enabled = false;
                HTx_EYE4_trackBar.Enabled = false;
                HTx_EYE3_textBox.Enabled = false;
                HTx_EYE4_textBox.Enabled = false;

                LTx_EYE1_trackBar.Maximum = 1500;
                LTx_EYE1_trackBar.Minimum = 500;
                LTx_EYE2_trackBar.Maximum = 2500;
                LTx_EYE2_trackBar.Minimum = 1500;
                LTx_EYE1_trackBar.Value = 500;
                LTx_EYE2_trackBar.Value = 1500;
                lbLTx_EYE1.Text = "01F4";
                lbLTx_EYE2.Text = "05DC";

                HTx_EYE1_trackBar.Maximum = 1500;
                HTx_EYE1_trackBar.Minimum = 500;
                HTx_EYE2_trackBar.Maximum = 2500;
                HTx_EYE2_trackBar.Minimum = 1500;
                HTx_EYE1_trackBar.Value = 500;
                HTx_EYE2_trackBar.Value = 1500;
                lbHTx_EYE1.Text = "01F4";
                lbHTx_EYE2.Text = "05DC";

            }
        }

        private void LTx_EYE3_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            Chanel = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_EYE3_textBox.Text = LTx_EYE3_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == true)            
            {
                writebuf[1] = (byte)(LTx_EYE3_trackBar.Value);
                SelectTable(i);               
                i2c.TWI_WriteByte(0xa0, 0xC4, writebuf[1]);
                lbLTx_EYE3.Text = writebuf[1].ToString("X2");
            }
        }

        private void LTx_EYE4_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            Chanel = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            LTx_EYE4_textBox.Text = LTx_EYE4_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == true)
            {
                writebuf[1] = (byte)(LTx_EYE4_trackBar.Value);
                SelectTable(i);                           
                i2c.TWI_WriteByte(0xa0, 0xC5, writebuf[1]);
                lbLTx_EYE4.Text = writebuf[1].ToString("X2");
            }
        }

        private void HTx_EYE3_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_EYE3_textBox.Text = HTx_EYE3_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == true)         
            {
                writebuf[0] = (byte)HTx_EYE3_trackBar.Value;
                SelectTable(i);
                i2c.TWI_WritePage(0xa0, 0xD0, writebuf, 1);
                lbHTx_EYE3.Text = writebuf[0].ToString("X2");
            }
        }

        private void HTx_EYE4_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte i = 0;
            byte[] writebuf = new byte[2];
            i = Chanel;
            i += 0xC0;
            HTx_EYE4_textBox.Text = HTx_EYE4_trackBar.Value.ToString();
            if (cBEYE3_4_EN.Checked == true)
            {
                writebuf[0] = (byte)HTx_EYE4_trackBar.Value;
                SelectTable(i);
                i2c.TWI_WritePage(0xa0, 0xD1, writebuf, 1);
                lbHTx_EYE4.Text = writebuf[0].ToString("X2");
            }
        }

        private void LTx_EYE3_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_EYE3_textBox.Text);
                LTx_EYE3_trackBar.Value = num;

            }
            catch { }
        }

        private void LTx_EYE4_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(LTx_EYE4_textBox.Text);
                LTx_EYE4_trackBar.Value = num;

            }
            catch { }
        }

        private void HTx_EYE3_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_EYE3_textBox.Text);
                HTx_EYE3_trackBar.Value = num;
            }
            catch { }
        }

        private void HTx_EYE4_textBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt16(HTx_EYE4_textBox.Text);
                HTx_EYE4_trackBar.Value = num;

            }
            catch { }
        }

        private void cbBPolarity_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte writebyte = 0;
            SelectTable(0xC0);
            writebyte = (byte)cbBPolarity.SelectedIndex;
            i2c.TWI_WriteByte(0xA0, 0xF5, writebyte);
        }

        //private void cBFECEn_CheckedChanged(object sender, EventArgs e)
        //{
        //    byte temp = 1;
        //    //byte[] pwd_dsp = {0xAA,0xA5,0x5A,0xAA};
        //    //i2c.TWI_WritePage(0xA0, 0x7B, pwd_dsp, 4);
        //    //Thread.Sleep(20);

        //    SelectTable(0xB0);
        //    if (cBFECEn.Checked)
        //    {
        //        i2c.TWI_WriteByte(0xA0, 0xE8, 0x01);
        //    }
        //    else
        //    {
        //        i2c.TWI_WriteByte(0xA0, 0xE8, 0x00);
        //    }
        //    //DSP更新
        //    i2c.TWI_WriteByte(0xA0, 0xE9, 0x01);
        //    Thread.Sleep(500);
        //    temp = i2c.TWI_ReadByte(0xA0,0xE9);
        //    int loop = 0;
        //    while ((temp == 1)&&(loop < 10))
        //    {
        //        temp = i2c.TWI_ReadByte(0xA0, 0xE9);
        //        Thread.Sleep(20);
        //        loop++;
        //    }
        //    temp = i2c.TWI_ReadByte(0xA0, 0xE8);
        //    if (temp == 0)
        //    {
        //        lbfecstate.Text = "关闭";
        //    }
        //    if (temp == 0)
        //    {
        //        lbfecstate.Text = "开启";
        //    }
        //}

    }
}
