using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fibertower_Common;
using System.IO;

namespace XFP模块测试程序
{
    public partial class DSPRegDebugForm : Form
    {
        I2C i2c;
        public DSPRegDebugForm()
        {
            InitializeComponent();
        }

        public DSPRegDebugForm(I2C i2c)
        {
            InitializeComponent();
            this.i2c = i2c;
        }

        byte[] readbuff = new byte[128];

        // 表选择
        private bool SelectTable(byte tbl)
        {
            return i2c.TWI_WriteByte(0xA0, 127, tbl);
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            byte[] readbuf = new byte[128];
            byte[] writebuf = new byte[128];
            byte regmsb = Convert.ToByte(tBRegMSB.Text, 16);
            byte reglsb = Convert.ToByte(tBRegLSB.Text, 16);
            byte devaddr = Convert.ToByte(tBDevAddr.Text, 16);
            byte length = Convert.ToByte(tBReg_DataLTH.Text, 16);

            writebuf[0] = 0x00;// R/W read
            writebuf[1] = devaddr;
            writebuf[2] = length;
            writebuf[3] = regmsb;
            writebuf[4] = reglsb;

            SelectTable(0xB0);//
            //if (i2c.TWI_ReadPage(0xa0, 0xEE, readbuf, 18) != 18) return;
            i2c.TWI_WritePage(0xa0, 0xEF, writebuf, 5);
            if (i2c.TWI_ReadPage(0xa0, 0xF4, readbuf, 8) != 8) return;

            listView1.Items.Clear();

            for (int i = 0; i < 8; i++)
            {
                ListViewItem myli = new ListViewItem(i.ToString("X"));

                for (int j = 0; j < 16; j++)
                {
                    myli.SubItems.Add(readbuff[i * 16 + j].ToString("X"));
                }
                listView1.Items.Add(myli);
            }
        }

        private void checkBox_debug_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_debug.Checked)
            {
                trackBar_Reg.Enabled = true;
            }
            else
            {
                trackBar_Reg.Enabled = false;
            }
        }

        private void trackBar_Reg_ValueChanged(object sender, EventArgs e)
        {
            byte regmsb = new byte();
            byte reglsb = new byte();
            byte devaddr = new byte();
            byte length = new byte();
            byte[] writebuf = new byte[18];
            byte[] readbuf = new byte[18];

            length = Convert.ToByte(tBReg_DataLTH.Text, 16);
            devaddr = Convert.ToByte(tBDevAddr.Text, 16);
            regmsb = Convert.ToByte(tBRegMSB.Text, 16);
            reglsb = Convert.ToByte(tBRegLSB.Text, 16);
            writebuf[0] = 0x00;// R/W read
            writebuf[1] = devaddr;
            writebuf[2] = length;
            writebuf[3] = regmsb;
            writebuf[4] = reglsb;
            SelectTable(0xB0);//

            textBox_trackBar_val.Text = trackBar_Reg.Value.ToString("X2");
            if (checkBox_ByteSelect.Checked)//单字节
            {
                writebuf[5] = (byte)trackBar_Reg.Value;
                writebuf[2] = 0x11;
                if (i2c.TWI_WritePage(0xa0, 0xEF, writebuf, 6) != 6)
                {
                    MessageBox.Show("写入失败");
                }
            }
            else//双字节
            {
                writebuf[2] = 0x12;
                writebuf[5] = (byte)((Convert.ToByte(trackBar_Reg.Value.ToString())) >> 8);
                writebuf[6] = (byte)(Convert.ToByte(trackBar_Reg.Value.ToString()));
                if (i2c.TWI_WritePage(0xa0, 0xEF, writebuf, 7) != 7)
                {
                    MessageBox.Show("写入失败");
                }
            }
        }

        private void checkBox_ByteSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_ByteSelect.Checked)
            {
                tBRegMSB.Enabled = false;
            }
            else
            {
                tBRegMSB.Enabled = true;
            }
        }

        private void button_RegRead_Click(object sender, EventArgs e)
        {
            byte regmsb = new byte();
            byte reglsb = new byte();
            byte devaddr = new byte();
            byte length = new byte();
            byte[] writebuf = new byte[18];
            byte[] readbuf = new byte[18];

            length = Convert.ToByte(tBReg_DataLTH.Text, 16);
            devaddr = Convert.ToByte(tBDevAddr.Text, 16);
            regmsb = Convert.ToByte(tBRegMSB.Text, 16);
            reglsb = Convert.ToByte(tBRegLSB.Text, 16);
            writebuf[0] = 0x01;// R/W read
            writebuf[1] = devaddr;
            writebuf[2] = length;
            writebuf[3] = regmsb;
            writebuf[4] = reglsb;

            if (checkBox_ByteSelect.Checked)
            {
                length = 0x11;//单字节数据
            }
            else
            {
                length = 0x12;//双字节数据
            }
            SelectTable(0xB0);//

            i2c.TWI_WritePage(0xa0, 0xEF, writebuf, 5);
            i2c.TWI_ReadPage(0xa0, 0xF4, readbuf, 8);

            if (checkBox_ByteSelect.Checked)
            {
                trackBar_Reg.Value = readbuf[0];
            }
            else
            {
                trackBar_Reg.Value = readbuf[0] * 256 + readbuf[1];
            }
        }

        private void checkBox_ByteSelect_CheckedChanged_1(object sender, EventArgs e)
        {
            if (checkBox_ByteSelect.Checked)
            {
                trackBar_Reg.Maximum = 255;
            }
            else
            {
                trackBar_Reg.Maximum = 1023;
            }
        }

        private void textBox_trackBar_val_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                trackBar_Reg.Value = Convert.ToInt16(textBox_trackBar_val.Text, 16);
            }
        }
    }
}
