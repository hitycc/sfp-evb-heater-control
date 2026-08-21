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
    public partial class EEPROM_MAP_Form : Form
    {
        I2C i2c;
        //public EEPROM_MAP_Form()
        //{
        //    InitializeComponent();
        //}
        byte[] readbuff = new byte[128];
        public EEPROM_MAP_Form(I2C i2c)
        {
            InitializeComponent();
            this.i2c = i2c;
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            // byte[] readbuff = new byte[128];
            byte page = new byte();

            listView1.Items.Clear();
            //Lower Page
            if (rBLowerPage.Checked)
            {
                SelectTable(0);
                if (i2c.TWI_ReadPage(0xA0, 0, readbuff, 128) != 128)
                {
                    MessageBox.Show("读取失败");
                    return;
                }
            }
            else//
            {
                try
                {
                    page = Convert.ToByte(tBPage.Text, 16);
                    SelectTable(page);
                    if (i2c.TWI_ReadPage(0xA0, 0x80, readbuff, 128) != 128)
                    {
                        MessageBox.Show("读取失败");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Page选择错误");
                    return;
                }

            }
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);         
            //listView1.BeginUpdate();
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

        // 表选择
        private bool SelectTable(byte tbl)
        {
            return i2c.TWI_WriteByte(0xA0, 127, tbl);
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            saveDialog.FilterIndex = 1;
            saveDialog.RestoreDirectory = true;

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                //int[] dataArray = { 0x11, 0x22, 0x33, 0x44 }; //示例数组 
                //byte[] byteArray = new byte[dataArray.Length * sizeof(int)];

                // Buffer.BlockCopy(dataArray, 0, byteArray, 0, byteArray.Length);

                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    writer.Write(readbuff);
                }
                MessageBox.Show("保存成功！");
            }
        }

        private void button_PWD_Click(object sender, EventArgs e)
        {
            byte[] PWD = new byte[4];
            // byte page = new byte();
            if ((textBox_PW00.Text == "") || (textBox_PW01.Text == "") || (textBox_PW02.Text == "") || (textBox_PW03.Text == ""))
            {
                return;
            }
            else
            {
                PWD[0] = Convert.ToByte(textBox_PW00.Text, 16);
                PWD[1] = Convert.ToByte(textBox_PW01.Text, 16);
                PWD[2] = Convert.ToByte(textBox_PW02.Text, 16);
                PWD[3] = Convert.ToByte(textBox_PW03.Text, 16);
            }

            if (i2c.TWI_WritePage(0xa0, 0x7B, PWD, 4) != 4)
            {
                MessageBox.Show("密码写入失败！");
                return;
            }
            // SelectTable(0);//
            //if (checkBox_ER1.Checked)
            //{
            //    if (i2c.TWI_WritePage(0xa0, 0x7B, PWD, 4) != 4)
            //    {
            //        MessageBox.Show("密码写入失败！");
            //        return;
            //    }
            //}
            //else
            //{
            //    if (i2c.TWI_WritePage(0xa0, 0x7A, PWD, 4) != 4)
            //    {
            //        MessageBox.Show("密码写入失败！");
            //        return;
            //    }
            //}

            // page = Convert.ToByte(textBox_Page.Text, 16);
            // SelectTable(page);//
        }

        private int BytetoValue(byte val1, byte val2)
        {
            byte[] Dbyte = new byte[2];
            Dbyte[0] = val1;
            Dbyte[1] = val2;
            return BitConverter.ToInt16(Dbyte, 0); // 自动解析补码 
        }

        private byte[] ValuetoByte(int val)
        {
            return BitConverter.GetBytes(val); // 自动处理补码转换
        }

        private void button_RegRead_Click(object sender, EventArgs e)
        {
            //timer1.Stop();
            byte page = new byte();
            byte reg = new byte();
            byte[] regbuf = new byte[2];
            byte[] readbuf = new byte[2];


            page = Convert.ToByte(textBox_Page.Text, 16);
            reg = Convert.ToByte(textBox_RegVal1.Text, 16);
            checkBox_debug.Checked = false;
            SelectTable(page);//
            //i2c.TWI_WriteByte(0xA0, 127, page);

            if (checkBox_ByteSelect.Checked)//单字节
            {
                trackBar_Reg.Maximum = 255;
                readbuf[0] = i2c.TWI_ReadByte(0xa0, reg);
                trackBar_Reg.Value = readbuf[0];
            }
            else//双字节
            {
                trackBar_Reg.Maximum = 65535;
                // regbuf[0] =  Convert.ToByte(textBox_RegVal0.Text, 16);             
                reg = Convert.ToByte(textBox_RegVal1.Text, 16);
                i2c.TWI_ReadPage(0xa0, reg, readbuf, 2);
                trackBar_Reg.Value = readbuf[0] * 256 + readbuf[1];
            }
        }

        private void trackBar_Reg_ValueChanged(object sender, EventArgs e)
        {
            byte page = new byte();
            byte[] writebuf = new byte[2];
            byte addr = new byte();
            byte[] addrbuf = new byte[2];
            byte Btemp = new byte();

            page = Convert.ToByte(textBox_Page.Text, 16);
            addr = Convert.ToByte(textBox_RegVal1.Text, 16);
            SelectTable(page);//
            textBox_trackBar_val.Text = trackBar_Reg.Value.ToString("X2");
            if (checkBox_ByteSelect.Checked)//单字节
            {
                trackBar_Reg.Maximum = 255;
                if (checkBox_debug.Checked)
                {
                    writebuf[0] = (byte)trackBar_Reg.Value;

                    if (i2c.TWI_WritePage(0xa0, addr, writebuf, 1) != 1)
                    {
                        MessageBox.Show("写入失败");
                    }
                    //i2c.TWI_WriteByte(0xa0, addr, (byte)trackBar_Reg.Value);
                }
            }
            else//双字节
            {
                if (checkBox_debug.Checked)
                {
                    writebuf = ValuetoByte(trackBar_Reg.Value);
                    Btemp = writebuf[0];
                    writebuf[0] = writebuf[1];
                    writebuf[1] = Btemp;
                    //writebuf[0] = (byte)((Convert.ToByte(trackBar_Reg.Value.ToString())) >> 8);
                    //writebuf[1] = (byte)(Convert.ToByte(trackBar_Reg.Value.ToString()));
                    i2c.TWI_WritePage(0xa0, addr, writebuf, 2);
                }
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

        private void checkBox_debug_CheckedChanged_1(object sender, EventArgs e)
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

        private void textBox_trackBar_val_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                trackBar_Reg.Value = Convert.ToInt16(textBox_trackBar_val.Text, 16);
            }
        }


    }
}
