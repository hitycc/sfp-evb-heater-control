using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Fibertower_Common;

namespace XFP模块测试程序
{
    public partial class Cal_Form : Form
    {
        I2C i2c;
        bool debugmode = false;

        public Cal_Form()
        {
            InitializeComponent();
        }

        public Cal_Form(I2C i2c)
        {            
            InitializeComponent();          
            this.i2c = i2c;
            timer1.Start();
        }

        private void Cal_Form_Load(object sender, EventArgs e)
        {
            channelSel_comboBox.Items.Clear();
            channelSel_comboBox.Items.Add("0");
            channelSel_comboBox.Items.Add("1");
            channelSel_comboBox.Items.Add("2");
            channelSel_comboBox.Items.Add("3");
            channelSel_comboBox.SelectedIndex = 0;
        }

        private void Cal_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
            this.Close();
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            byte[] readbuffer = new byte[4];          
            //pwd[0] = 0xA9;
            //pwd[1] = 0x46;
            //pwd[2] = 0x50;
            //pwd[3] = 0x54;
            i2c.TWI_ReadPage(0xa0, 123, readbuffer, 4);
            if ((readbuffer[0] == 0xA9) && (readbuffer[1] == 0x46) && (readbuffer[2] == 0x50) && (readbuffer[3] == 0x54))
            {
                debugmode = true;
            }
            // 自动进入调试模式
            if (debug_checkBox.Checked && (debugmode == false))
            {
                WriteVenderPWD();
            }

            SelectTable(6);

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i *= 2;
            i += 0xE0;

            i2c.TWI_ReadPage(0xa0, i, readbuffer, 2);
            txadc_textbox.Text = (readbuffer[0] * 256 + readbuffer[1]).ToString();

            i += 8;
            i2c.TWI_ReadPage(0xa0, i, readbuffer, 2);
            rxadc_textbox.Text = (readbuffer[0] * 256 + readbuffer[1]).ToString();

            //更新DDM
            i = Convert.ToByte(channelSel_comboBox.Text);
            i *= 2;
            i += 0x32;
            i2c.TWI_ReadPage(0xa0, i, readbuffer, 2);
            if (readbuffer[0] == 0 && readbuffer[1] == 0)
                readbuffer[1] = 1;
            txdbm_textbox.Text = (10 * Math.Log10((readbuffer[0] * 256 + readbuffer[1]) / 10000.0)).ToString("F2");

            i = Convert.ToByte(channelSel_comboBox.Text);
            i *= 2;
            i += 0x22;
            i2c.TWI_ReadPage(0xa0, i, readbuffer, 2);
            if (readbuffer[0] == 0 && readbuffer[1] == 0)
                readbuffer[1] = 1;
            rxdbm_textbox.Text = (10 * Math.Log10((readbuffer[0] * 256 + readbuffer[1]) / 10000.0)).ToString("F2");

            if (txcaldbm_textbox.Focused)
                txcaladc_textbox.Text = txadc_textbox.Text;

            if (rxcaldbm1_textbox.Focused)
                rxcaladc1_textbox.Text = rxadc_textbox.Text;
            if (rxcaldbm2_textbox.Focused)
                rxcaladc2_textbox.Text = rxadc_textbox.Text;
            if (rxcaldbm3_textbox.Focused)
                rxcaladc3_textbox.Text = rxadc_textbox.Text;
            if (rxcaldbm4_textbox.Focused)
                rxcaladc4_textbox.Text = rxadc_textbox.Text;
            if (rxcaldbm5_textbox.Focused)
                rxcaladc5_textbox.Text = rxadc_textbox.Text;
        }


        //TX功率校正
        private void txcal_button_Click(object sender, EventArgs e)
        {
            float txpower = Convert.ToSingle(txcaldbm_textbox.Text);
            float k = 0;
            int ADC = Convert.ToInt32(txcaladc_textbox.Text);
            byte[] c0 = new byte[4];
            
            byte[] writebuffer = new byte[4];
            byte[] readbuffer = new byte[4];

            if (debugmode == false)
            {
                MessageBox.Show("模块未进入调试模式");
                return;
            }

            k = (float)Math.Pow(10, 0.1 * txpower) * 10000;
            k = k / ADC;
            txcal_k_textbox.Text = k.ToString("F2");
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
            Thread.Sleep(600);

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
        }

        //RX功率校正
        private void rxcal_button_Click(object sender, EventArgs e)
        {
            double[] x = new double[5];  //ADC原始值
            double[] y = new double[5];  //校正值
            double[] a = new double[5];  //系数
            double[] dt = new double[5];   //误差
            for (int j = 0; j < 5; j++)
                dt[j] = 0.0;
            byte[] c0 = new byte[4];     //C0 C1 C2 数组 
            byte[] c1 = new byte[4];
            byte[] c2 = new byte[4];
            byte[] writebuffer = new byte[12];
            byte[] readbuffer = new byte[12];

            if (debugmode == false)
            {
                MessageBox.Show("模块未进入调试模式");
                return;
            }

            if (radioButton1.Checked)
            {
                double rxcaldbm1 = Convert.ToSingle(rxcaldbm1_textbox.Text) / 10;
                y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                double rxcaldbm2 = Convert.ToSingle(rxcaldbm2_textbox.Text) / 10;
                y[1] = Math.Pow(10, rxcaldbm2) * 10000;

                x[0] = Convert.ToDouble(rxcaladc1_textbox.Text);
                x[1] = Convert.ToDouble(rxcaladc2_textbox.Text);

                Bit.iapcir(x, y, 2, a, 2, dt);
                rxcal_b_textbox.Text = a[1].ToString("f5");
                rxcal_c_textbox.Text = a[0].ToString("f5");
                c2 = BitConverter.GetBytes((float)0);
                c1 = BitConverter.GetBytes((float)a[1]);
                c0 = BitConverter.GetBytes((float)a[0]);
                c0.CopyTo(writebuffer, 0);
                c1.CopyTo(writebuffer, 4);
                c2.CopyTo(writebuffer, 8);
            }
            else
            {
                double rxcaldbm1 = Convert.ToSingle(rxcaldbm1_textbox.Text) / 10;
                y[0] = Math.Pow(10, rxcaldbm1) * 10000;
                double rxcaldbm2 = Convert.ToSingle(rxcaldbm2_textbox.Text) / 10;
                y[1] = Math.Pow(10, rxcaldbm2) * 10000;
                double rxcaldbm3 = Convert.ToSingle(rxcaldbm3_textbox.Text) / 10;
                y[2] = Math.Pow(10, rxcaldbm3) * 10000;
                double rxcaldbm4 = Convert.ToSingle(rxcaldbm4_textbox.Text) / 10;
                y[3] = Math.Pow(10, rxcaldbm4) * 10000;
                double rxcaldbm5 = Convert.ToSingle(rxcaldbm5_textbox.Text) / 10;
                y[4] = Math.Pow(10, rxcaldbm5) * 10000;

                x[0] = Convert.ToDouble(rxcaladc1_textbox.Text);
                x[1] = Convert.ToDouble(rxcaladc2_textbox.Text);
                x[2] = Convert.ToDouble(rxcaladc3_textbox.Text);
                x[3] = Convert.ToDouble(rxcaladc4_textbox.Text);
                x[4] = Convert.ToDouble(rxcaladc5_textbox.Text);

                Bit.iapcir(x, y, 5, a, 3, dt);
                rxcal_a_textbox.Text = a[2].ToString("f5");
                rxcal_b_textbox.Text = a[1].ToString("f5");
                rxcal_c_textbox.Text = a[0].ToString("f5");
                c2 = BitConverter.GetBytes((float)a[2]);
                c1 = BitConverter.GetBytes((float)a[1]);
                c0 = BitConverter.GetBytes((float)a[0]);
                c0.CopyTo(writebuffer, 0);
                c1.CopyTo(writebuffer, 4);
                c2.CopyTo(writebuffer, 8);
            }

            if (!SelectTable(7))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i *= 16;
            i += 0x10;
            i += 0x80;

            if ((i2c.TWI_WritePage(0xA0, i, writebuffer, 12) != 12))
            {
                MessageBox.Show("写入失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SaveCalToFlash())
            {
                MessageBox.Show("Save命令失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Thread.Sleep(800);

            if (!SelectTable(7))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if ((i2c.TWI_ReadPage(0xA0, i, readbuffer, 12) != 12))
            {
                MessageBox.Show("读取失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Bit.ByteEquals(readbuffer, writebuffer))
                MessageBox.Show("保存成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("保存失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //select pin
            rxcaldbm1_textbox.Enabled = true;
            rxcaldbm2_textbox.Enabled = true;
            rxcaldbm3_textbox.Enabled = false;
            rxcaldbm4_textbox.Enabled = false;
            rxcaldbm5_textbox.Enabled = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //select apd
            rxcaldbm1_textbox.Enabled = true;
            rxcaldbm2_textbox.Enabled = true;
            rxcaldbm3_textbox.Enabled = true;
            rxcaldbm4_textbox.Enabled = true;
            rxcaldbm5_textbox.Enabled = true;
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

        private void nopwr_cal_button_Click(object sender, EventArgs e)
        {
            byte[] wrbuffer = new byte[2];
            byte[] rdbuffer = new byte[2];

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte i = Convert.ToByte(channelSel_comboBox.Text);
            i += 0xC0;

            //无光ADC偏差值
            wrbuffer[0] = Convert.ToByte(nopwr_adc_textBox.Text);

            if ((i2c.TWI_WritePage(0xA0, i, wrbuffer, 1) != 1))
            {
                MessageBox.Show("写入失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!SaveCalToFlash())
            {
                MessageBox.Show("Save命令失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Thread.Sleep(600);

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((i2c.TWI_ReadPage(0xA0, i, rdbuffer, 1) != 1))
            {
                MessageBox.Show("读取失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            rdbuffer[1] = 0;
            wrbuffer[1] = 0;

            if (Bit.ByteEquals(rdbuffer, wrbuffer))
                MessageBox.Show("无光较准保存成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("无光较准保存失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void channelSel_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte ch = Convert.ToByte(channelSel_comboBox.Text);
            //TxChEnable(ch);
        }
     
    }
}
