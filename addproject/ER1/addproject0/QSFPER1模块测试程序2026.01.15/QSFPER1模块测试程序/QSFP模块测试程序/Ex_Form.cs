using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Threading;
using Fibertower_Common;

namespace XFP模块测试程序
{
    public partial class Ex_Form : Form
    {
        I2C i2c;
        OleDbConnection dbconnect;
        OleDbCommand dbcommand;
        OleDbDataAdapter dbadapter;
        DataSet dbset;
        bool apdcheck;

        public Ex_Form()
        {
            InitializeComponent();
        }

        public Ex_Form(I2C i2c, OleDbConnection dbconnect, bool apdcheck)
        {            
            InitializeComponent();
            this.i2c = i2c;
            this.dbconnect = dbconnect;
            this.apdcheck = apdcheck;
            //更新模块型号列表
            try
            {
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

        // 保存所有信息到FLASH
        /*private bool SaveAllFlash()
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

        private void Converted_analog_values(byte[] readbuffer)
        {
            sbyte i;
            int j;

            i = (sbyte)readbuffer[0];
            j = Convert.ToInt32(i);
            temp_HA_textbox.Text = (j + readbuffer[1] * 1 / 256.0).ToString("F2");
            i = (sbyte)readbuffer[2];
            j = Convert.ToInt32(i);
            temp_LA_textbox.Text = (j + readbuffer[3] * 1 / 256.0).ToString("F2");
            i = (sbyte)readbuffer[4];
            j = Convert.ToInt32(i);
            temp_HW_textbox.Text = (j + readbuffer[5] * 1 / 256.0).ToString("F2");
            i = (sbyte)readbuffer[6];
            j = Convert.ToInt32(i);
            temp_LW_textbox.Text = (j + readbuffer[7] * 1 / 256.0).ToString("F2");

            vcc_HA_textbox.Text = ((readbuffer[16] * 256 + readbuffer[17]) / 10000.0).ToString("F2");
            vcc_LA_textbox.Text = ((readbuffer[18] * 256 + readbuffer[19]) / 10000.0).ToString("F2");
            vcc_HW_textbox.Text = ((readbuffer[20] * 256 + readbuffer[21]) / 10000.0).ToString("F2");
            vcc_LW_textbox.Text = ((readbuffer[22] * 256 + readbuffer[23]) / 10000.0).ToString("F2");

            rxpwr_HA_textbox.Text = (10 * Math.Log10((readbuffer[48] * 256 + readbuffer[49]) / 10000.0)).ToString("F2");
            rxpwr_LA_textbox.Text = (10 * Math.Log10((readbuffer[50] * 256 + readbuffer[51]) / 10000.0)).ToString("F2");
            rxpwr_HW_textbox.Text = (10 * Math.Log10((readbuffer[52] * 256 + readbuffer[53]) / 10000.0)).ToString("F2");
            rxpwr_LW_textbox.Text = (10 * Math.Log10((readbuffer[54] * 256 + readbuffer[55]) / 10000.0)).ToString("F2");

            bias_HA_textbox.Text = ((readbuffer[56] * 256 + readbuffer[57]) / 500.0).ToString("F2");
            bias_LA_textbox.Text = ((readbuffer[58] * 256 + readbuffer[59]) / 500.0).ToString("F2");
            bias_HW_textbox.Text = ((readbuffer[60] * 256 + readbuffer[61]) / 500.0).ToString("F2");
            bias_LW_textbox.Text = ((readbuffer[62] * 256 + readbuffer[63]) / 500.0).ToString("F2");

            txpwr_HA_textbox.Text = (10 * Math.Log10((readbuffer[64] * 256 + readbuffer[65]) / 10000.0)).ToString("F2");
            txpwr_LA_textbox.Text = (10 * Math.Log10((readbuffer[66] * 256 + readbuffer[67]) / 10000.0)).ToString("F2");
            txpwr_HW_textbox.Text = (10 * Math.Log10((readbuffer[68] * 256 + readbuffer[69]) / 10000.0)).ToString("F2");
            txpwr_LW_textbox.Text = (10 * Math.Log10((readbuffer[70] * 256 + readbuffer[71]) / 10000.0)).ToString("F2");
            

            /*ITEC_HA_textbox.Text = (((sbyte)readbuffer[48]) * 256 + readbuffer[49]).ToString();
            ITEC_LA_textbox.Text = (((sbyte)readbuffer[50]) * 256 + readbuffer[51]).ToString();
            ITEC_HW_textbox.Text = (((sbyte)readbuffer[52]) * 256 + readbuffer[53]).ToString();
            ITEC_LW_textbox.Text = (((sbyte)readbuffer[54]) * 256 + readbuffer[55]).ToString();*/
        }

        private byte[] Converted_analog_values()
        {
            byte[] writebuffer = new byte[72];
            float a;
            UInt16 b;

            // temp
            a = Convert.ToSingle(temp_HA_textbox.Text);
            b = (UInt16)((Int16)(a * 256));
            writebuffer[0] = (byte)(b / 256);
            writebuffer[1] = (byte)(b % 256);
            a = Convert.ToSingle(temp_LA_textbox.Text);
            b = (UInt16)((Int16)(a * 256));
            writebuffer[2] = (byte)(b / 256);
            writebuffer[3] = (byte)(b % 256);
            a = Convert.ToSingle(temp_HW_textbox.Text);
            b = (UInt16)((Int16)(a * 256));
            writebuffer[4] = (byte)(b / 256);
            writebuffer[5] = (byte)(b % 256);
            a = Convert.ToSingle(temp_LW_textbox.Text);
            b = (UInt16)((Int16)(a * 256));
            writebuffer[6] = (byte)(b / 256);
            writebuffer[7] = (byte)(b % 256);

            //vcc
            b = (UInt16)(10000 * Convert.ToSingle(vcc_HA_textbox.Text));
            writebuffer[16] = (byte)(b / 256);
            writebuffer[17] = (byte)(b % 256);
            b = (UInt16)(10000 * Convert.ToSingle(vcc_LA_textbox.Text));
            writebuffer[18] = (byte)(b / 256);
            writebuffer[19] = (byte)(b % 256);
            b = (UInt16)(10000 * Convert.ToSingle(vcc_HW_textbox.Text));
            writebuffer[20] = (byte)(b / 256);
            writebuffer[21] = (byte)(b % 256);
            b = (UInt16)(10000 * Convert.ToSingle(vcc_LW_textbox.Text));
            writebuffer[22] = (byte)(b / 256);
            writebuffer[23] = (byte)(b % 256);

            //rx_pwr
            a = Convert.ToSingle(rxpwr_HA_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[48] = (byte)(b / 256);
            writebuffer[49] = (byte)(b % 256);
            a = Convert.ToSingle(rxpwr_LA_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[50] = (byte)(b / 256);
            writebuffer[51] = (byte)(b % 256);
            a = Convert.ToSingle(rxpwr_HW_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[52] = (byte)(b / 256);
            writebuffer[53] = (byte)(b % 256);
            a = Convert.ToSingle(rxpwr_LW_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[54] = (byte)(b / 256);
            writebuffer[55] = (byte)(b % 256);

            //bias
            b = (UInt16)(500 * Convert.ToSingle(bias_HA_textbox.Text));
            writebuffer[56] = (byte)(b / 256);
            writebuffer[57] = (byte)(b % 256);
            b = (UInt16)(500 * Convert.ToSingle(bias_LA_textbox.Text));
            writebuffer[58] = (byte)(b / 256);
            writebuffer[59] = (byte)(b % 256);
            b = (UInt16)(500 * Convert.ToSingle(bias_HW_textbox.Text));
            writebuffer[60] = (byte)(b / 256);
            writebuffer[61] = (byte)(b % 256);
            b = (UInt16)(500 * Convert.ToSingle(bias_LW_textbox.Text));
            writebuffer[62] = (byte)(b / 256);
            writebuffer[63] = (byte)(b % 256);

            //tx_pwr
            a = Convert.ToSingle(txpwr_HA_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[64] = (byte)(b / 256);
            writebuffer[65] = (byte)(b % 256);
            a = Convert.ToSingle(txpwr_LA_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[66] = (byte)(b / 256);
            writebuffer[67] = (byte)(b % 256);
            a = Convert.ToSingle(txpwr_HW_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[68] = (byte)(b / 256);
            writebuffer[69] = (byte)(b % 256);
            a = Convert.ToSingle(txpwr_LW_textbox.Text);
            b = (UInt16)(Math.Pow(10, a / 10) * 10000);
            writebuffer[70] = (byte)(b / 256);
            writebuffer[71] = (byte)(b % 256);

            /*b = (UInt16)(Convert.ToSingle(ITEC_HA_textbox.Text));
            writebuffer[48] = (byte)(b / 256);
            writebuffer[49] = (byte)(b % 256);
            b = (UInt16)(Convert.ToSingle(ITEC_LA_textbox.Text));
            writebuffer[50] = (byte)(b / 256);
            writebuffer[51] = (byte)(b % 256);
            b = (UInt16)(Convert.ToSingle(ITEC_HW_textbox.Text));
            writebuffer[52] = (byte)(b / 256);
            writebuffer[53] = (byte)(b % 256);
            b = (UInt16)(Convert.ToSingle(ITEC_LW_textbox.Text));
            writebuffer[54] = (byte)(b / 256);
            writebuffer[55] = (byte)(b % 256);*/

            return writebuffer;
        }

        private void read_module_button_Click(object sender, EventArgs e)
        {
            byte[] readbuffer = new byte[72];

            SelectTable(3);

            if (i2c.TWI_ReadPage(0xa0, 0x80, readbuffer, 72) != 72)
            {
                MessageBox.Show("读取失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Converted_analog_values(readbuffer);
        }

        private void write_module_button_Click(object sender, EventArgs e)
        {
            byte[] writebuffer = Converted_analog_values();

            if (!SelectTable(3))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (i2c.TWI_WritePage(0xa0, 0x80, writebuffer, 72) != 72)
            {
                MessageBox.Show("写入失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!i2c.TWI_WriteByte(0xA0, 0x82, 0x08)) //0x82 bit3=1
            {
                MessageBox.Show("保存失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Thread.Sleep(600);
            MessageBox.Show("保存成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void read_sqlDB_button_Click(object sender, EventArgs e)
        {
            byte[] readbuffer = new byte[72];

            string dbconnectionstr = string.Format("select Page03 from [{0}]", Module_Type_comboBox.Text);

            dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
            dbadapter = new OleDbDataAdapter(dbcommand);
            dbset = new DataSet();
            dbadapter.Fill(dbset, Module_Type_comboBox.Text);

            for (int i = 0; i < 72; i++)
            {
                readbuffer[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["Page03"]);
            }

            Converted_analog_values(readbuffer);
        }

        private void write_sqlDB_button_Click(object sender, EventArgs e)
        {
            byte[] writebuffer = Converted_analog_values();

            string dbconnectionstr = string.Format("select Page03 from [{0}]", Module_Type_comboBox.Text);

            dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
            dbadapter = new OleDbDataAdapter(dbcommand);
            dbset = new DataSet();
            dbadapter.Fill(dbset, Module_Type_comboBox.Text);
            dbconnect.Open();

            for (int i = 0; i < 72; i++)
            {
                string UpdateCommandstr = string.Format("update [{0}] set Page03={1} where ID={2}", Module_Type_comboBox.Text, writebuffer[i], i + 1);
                dbcommand = new OleDbCommand(UpdateCommandstr, dbconnect);
                dbcommand.ExecuteNonQuery();
            }
            dbconnect.Close();
        }

        private void readlut_button_Click(object sender, EventArgs e)
        {
            byte[] lut = new byte[128];
            byte[] biaslut = new byte[32];
            byte[] modlut = new byte[32];
            byte[] apdlut = new byte[32];

            byte channel = Convert.ToByte(channelSel_comboBox.Text);

            if (!SelectTable(8))
            {
                MessageBox.Show("写入表错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (i2c.TWI_ReadPage(0xA0, 0x80, lut, 128) != 128)
            {
                MessageBox.Show("读取错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //bias
            for (int i = 0; i < 32; i++)
            {
                biaslut[i] = lut[4 * i + channel];
            }

            if (!SelectTable(9))
            {
                MessageBox.Show("写入表错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (i2c.TWI_ReadPage(0xA0, 0x80, lut, 128) != 128)
            {
                MessageBox.Show("读取错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //mod
            for (int i = 0; i < 32; i++)
            {
                modlut[i] = lut[4 * i + channel];
            }

            if (!SelectTable(10))
            {
                MessageBox.Show("写入表错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (i2c.TWI_ReadPage(0xA0, 0x80, lut, 128) != 128)
            {
                MessageBox.Show("读取错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //apd
            for (int i = 0; i < 32; i++)
            {
                apdlut[i] = lut[4 * i + channel];
            }

            listView1.Items.Clear();
            for (int i = 0; i < 32; i++)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = i.ToString();
                li.SubItems.Add((i * 5 - 40).ToString() + "℃");
                li.SubItems.Add(biaslut[i].ToString());
                li.SubItems.Add(modlut[i].ToString());
                li.SubItems.Add(apdlut[i].ToString());
                listView1.Items.Add(li);
            }
        }

        private void writelut_button_Click(object sender, EventArgs e)
        {
            byte[] lut = new byte[128];
            byte[] biaslut = new byte[32];
            byte[] modlut = new byte[32];
            byte[] apdlut = new byte[32];

            byte channel = Convert.ToByte(channelSel_comboBox.Text);

            for (int i = 0; i < 32; i++)
            {
                biaslut[i] = Convert.ToByte(listView1.Items[i].SubItems[2].Text);
                modlut[i] = Convert.ToByte(listView1.Items[i].SubItems[3].Text);
                apdlut[i] = Convert.ToByte(listView1.Items[i].SubItems[4].Text);
            }

            //bias
            if (!SelectTable(8))
            {
                MessageBox.Show("写入表错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (i2c.TWI_ReadPage(0xa0, 0x80, lut, 128) != 128)
            {
                MessageBox.Show("读取表失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int i = 0; i < 32; i++)
            {
                lut[4 * i + channel] = biaslut[i];
            }
            if (i2c.TWI_WritePage(0xa0, 0x80, lut, 128) != 128)
            {
                MessageBox.Show("写入失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //mod
            if (!SelectTable(9))
            {
                MessageBox.Show("写入表错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (i2c.TWI_ReadPage(0xa0, 0x80, lut, 128) != 128)
            {
                MessageBox.Show("读取表失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            for (int i = 0; i < 32; i++)
            {
                lut[4 * i + channel] = modlut[i];
            }
            if (i2c.TWI_WritePage(0xa0, 0x80, lut, 128) != 128)
            {
                MessageBox.Show("写入失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (apd_checkBox.Checked)
            {
                //apd
                if (!SelectTable(10))
                {
                    MessageBox.Show("写入表错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (i2c.TWI_ReadPage(0xa0, 0x80, lut, 128) != 128)
                {
                    MessageBox.Show("读取表失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                for (int i = 0; i < 32; i++)
                {
                    lut[4 * i + channel] = apdlut[i];
                }
                if (i2c.TWI_WritePage(0xa0, 0x80, lut, 128) != 128)
                {
                    MessageBox.Show("写入失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // 发送保存命令
            byte saveByte = 0x0C; // 00001100  bit3 bit2  0x83地址
            if (apd_checkBox.Checked)
            {
                saveByte |= 0x10; //bit4
            }

            if (!SelectTable(6))
            {
                MessageBox.Show("选择表错误！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!i2c.TWI_WriteByte(0xA0, 0x83, saveByte))
            {
                MessageBox.Show("保存失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Thread.Sleep(600);
            MessageBox.Show("保存成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void readluttoDB_button_Click(object sender, EventArgs e)
        {
            byte[] biaslut = new byte[32];
            byte[] modlut = new byte[32];
            byte[] apdlut = new byte[32];

            string dbconnectionstr;

            if (apd_checkBox.Checked)
                dbconnectionstr = string.Format("select Low128,BiasVal,ModVal,ApdVal from [{0}]", Module_Type_comboBox.Text);
            else
                dbconnectionstr = string.Format("select Low128,BiasVal,ModVal from [{0}]", Module_Type_comboBox.Text);

            dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
            dbadapter = new OleDbDataAdapter(dbcommand);
            dbset = new DataSet();
            dbadapter.Fill(dbset, Module_Type_comboBox.Text);
            for (int i = 0; i < 32; i++)
            {
                biaslut[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["BiasVal"]);
                modlut[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["ModVal"]);
            }

            //if (Module_Type_comboBox.Text == "FXP55192-ZRC")
            if (apd_checkBox.Checked)
            {
                for (int i = 0; i < 32; i++)
                {
                    apdlut[i] = Convert.ToByte(dbset.Tables[Module_Type_comboBox.Text].Rows[i]["ApdVal"]);
                }
            }

            listView1.Items.Clear();
            for (int i = 0; i < 32; i++)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = i.ToString();
                li.SubItems.Add((i * 5 - 40).ToString() + "℃");
                li.SubItems.Add(biaslut[i].ToString());
                li.SubItems.Add(modlut[i].ToString());
                
                if (apd_checkBox.Checked)
                {
                    li.SubItems.Add(apdlut[i].ToString());
                }
                else
                {
                    li.SubItems.Add("0");
                }
                listView1.Items.Add(li);
            }
        }

        private void writeluttoDB_button_Click(object sender, EventArgs e)
        {
            byte[] biaslut = new byte[32];
            byte[] modlut = new byte[32];
            byte[] apdlut = new byte[32];

            for (int i = 0; i < 32; i++)
            {
                biaslut[i] = Convert.ToByte(listView1.Items[i].SubItems[2].Text);
                modlut[i] = Convert.ToByte(listView1.Items[i].SubItems[3].Text);
                apdlut[i] = Convert.ToByte(listView1.Items[i].SubItems[4].Text);
            }

            string dbconnectionstr;
            
            if (apd_checkBox.Checked)
                dbconnectionstr = string.Format("select BiasVal,ModVal,ApdVal from [{0}]", Module_Type_comboBox.Text);
            else
                dbconnectionstr = string.Format("select BiasVal,ModVal from [{0}]", Module_Type_comboBox.Text);

            dbcommand = new OleDbCommand(dbconnectionstr, dbconnect);
            dbadapter = new OleDbDataAdapter(dbcommand);
            dbset = new DataSet();
            dbadapter.Fill(dbset, Module_Type_comboBox.Text);
            dbconnect.Open();

            for (int i = 0; i < 32; i++)
            {
                string UpdateCommandstr;
                
                if (apd_checkBox.Checked)
                    UpdateCommandstr = string.Format("update [{0}] set BiasVal={1},ModVal={2},ApdVal={3} where ID={4}", Module_Type_comboBox.Text, biaslut[i], modlut[i], apdlut[i], i + 1);
                else
                    UpdateCommandstr = string.Format("update [{0}] set BiasVal={1},ModVal={2} where ID={3}", Module_Type_comboBox.Text, biaslut[i], modlut[i], i + 1);

                dbcommand = new OleDbCommand(UpdateCommandstr, dbconnect);
                dbcommand.ExecuteNonQuery();
            }
            dbconnect.Close();

            MessageBox.Show("保存数据库成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Ex_Form_Load(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            for (int i = 0; i < 32; i++)
            {
                ListViewItem li = new ListViewItem();
                li.SubItems.Clear();
                li.SubItems[0].Text = i.ToString();
                li.SubItems.Add((i * 5 - 40).ToString() + "℃");
                li.SubItems.Add("0");
                li.SubItems.Add("0");
                li.SubItems.Add("0");
                listView1.Items.Add(li);
            }

            channelSel_comboBox.Items.Clear();
            channelSel_comboBox.Items.Add("0");
            channelSel_comboBox.Items.Add("1");
            channelSel_comboBox.Items.Add("2");
            channelSel_comboBox.Items.Add("3");
            channelSel_comboBox.SelectedIndex = 0;
        }

        private void button1Debug_Click(object sender, EventArgs e)
        {
            if (!WriteVenderPWD())
            {
                MessageBox.Show("写入用户密码错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Ex_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Close();
        }
    }
}
