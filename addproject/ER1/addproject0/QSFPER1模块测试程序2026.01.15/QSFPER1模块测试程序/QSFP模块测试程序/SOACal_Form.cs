using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using Fibertower_Common;

namespace XFP模块测试程序
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Union
    {
        [FieldOffset(0)]
        public byte uc0;
        [FieldOffset(1)]
        public byte uc1;
        [FieldOffset(2)]
        public byte uc2;
        [FieldOffset(3)]
        public byte uc3;
        [FieldOffset(0)]
        public Single f;
        [FieldOffset(0)]
        public UInt32 ui;
        
    }
    public partial class SOACal_Form : Form
    {
        I2C i2c;
        Union u = new Union();
        bool moduleOnline = true;

        public SOACal_Form()
        {
            InitializeComponent();
        }

        public SOACal_Form(I2C i2c)
        {
            InitializeComponent();
            this.i2c = i2c;
        }

        private void SOACal_Form_Load(object sender, EventArgs e)
        {
            //
        }

        private void SOACal_Form_Load(object sender, FormClosedEventArgs e)
        {
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
        private bool SaveSOACalToFlash()
        {
            if (!SelectTable(6))
            {
                return false;
            }

            return i2c.TWI_WriteByte(0xA0, 0x83, 0x40); //bit6
        }

        private void button1Debug_Click(object sender, EventArgs e)
        {
            if (!WriteVenderPWD())
            {
                MessageBox.Show("写入用户密码错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void button1Read_Click(object sender, EventArgs e)
        {
            byte[] rdBuf = new byte[12];
            int i = 0;
            //Single ftmp;

            SelectTable(0x0B);

            if (i2c.TWI_ReadPage(0xA0, 0xA7, rdBuf, 12) != 12)
            {
                MessageBox.Show("读取信息错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            textBox_tecKP.Text = rdBuf[0].ToString("X02") + rdBuf[1].ToString("X02") + rdBuf[2].ToString("X02") + rdBuf[3].ToString("X02");
            textBox_tecKI.Text = rdBuf[4].ToString("X02") + rdBuf[5].ToString("X02") + rdBuf[6].ToString("X02") + rdBuf[7].ToString("X02");
            textBox_tecKD.Text = rdBuf[8].ToString("X02") + rdBuf[9].ToString("X02") + rdBuf[10].ToString("X02") + rdBuf[11].ToString("X02");

            i = 0;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_tecfP.Text = (u.f).ToString("F4");

            i = 4;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_tecfI.Text = (u.f).ToString("F4");

            i = 8;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_tecfD.Text = (u.f).ToString("F4");
        }

        private void button1Write_Click(object sender, EventArgs e)
        {
            byte[] wrBuf = new byte[12];
            byte[] readBuf = new byte[12];
            int i = 0;

            i = 0;
            u.f = Convert.ToSingle(textBox_tecfP.Text);
            wrBuf[i + 0] = u.uc3;
            wrBuf[i + 1] = u.uc2;
            wrBuf[i + 2] = u.uc1;
            wrBuf[i + 3] = u.uc0;

            i = 4;
            u.f = Convert.ToSingle(textBox_tecfI.Text);
            wrBuf[i + 0] = u.uc3;
            wrBuf[i + 1] = u.uc2;
            wrBuf[i + 2] = u.uc1;
            wrBuf[i + 3] = u.uc0;

            i = 8;
            u.f = Convert.ToSingle(textBox_tecfD.Text);
            wrBuf[i + 0] = u.uc3;
            wrBuf[i + 1] = u.uc2;
            wrBuf[i + 2] = u.uc1;
            wrBuf[i + 3] = u.uc0;

            textBox_tecKP.Text = wrBuf[0].ToString("X02") + wrBuf[1].ToString("X02") + wrBuf[2].ToString("X02") + wrBuf[3].ToString("X02");
            textBox_tecKI.Text = wrBuf[4].ToString("X02") + wrBuf[5].ToString("X02") + wrBuf[6].ToString("X02") + wrBuf[7].ToString("X02");
            textBox_tecKD.Text = wrBuf[8].ToString("X02") + wrBuf[9].ToString("X02") + wrBuf[10].ToString("X02") + wrBuf[11].ToString("X02");

            SelectTable(0x0B);

            if (i2c.TWI_WritePage(0xA0, 0xA7, wrBuf, 12) != 12)
            {
                MessageBox.Show("写入 TOSA_TEC_PID 参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            i2c.TWI_ReadPage(0xA0, 0xA7, readBuf, 12);

            if (Bit.ByteEquals(readBuf, wrBuf))
                MessageBox.Show("写入 TOSA_TEC_PID 参数 成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("写入 TOSA_TEC_PID 参数 失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button2Read_Click(object sender, EventArgs e)
        {
            byte[] rdBuf = new byte[12];
            int i = 0;
            //Single ftmp;

            SelectTable(0x0B);

            if (i2c.TWI_ReadPage(0xA0, 0xB3, rdBuf, 12) != 12)
            {
                MessageBox.Show("读取信息错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            textBox_soaKP.Text = rdBuf[0].ToString("X02") + rdBuf[1].ToString("X02") + rdBuf[2].ToString("X02") + rdBuf[3].ToString("X02");
            textBox_soaKI.Text = rdBuf[4].ToString("X02") + rdBuf[5].ToString("X02") + rdBuf[6].ToString("X02") + rdBuf[7].ToString("X02");
            textBox_soaKD.Text = rdBuf[8].ToString("X02") + rdBuf[9].ToString("X02") + rdBuf[10].ToString("X02") + rdBuf[11].ToString("X02");

            i = 0;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_soafP.Text = (u.f).ToString("F4");

            i = 4;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_soafI.Text = (u.f).ToString("F4");

            i = 8;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_soafD.Text = (u.f).ToString("F4");

            ///////////////////////////////////////////////////////////////////////////////////////////////
            //SelectTable(0x0B);
            if (i2c.TWI_ReadPage(0xA0, 0xC5, rdBuf, 12) != 12)
            {
                MessageBox.Show("读取信息错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            textBox_soatecKP.Text = rdBuf[0].ToString("X02") + rdBuf[1].ToString("X02") + rdBuf[2].ToString("X02") + rdBuf[3].ToString("X02");
            textBox_soatecKI.Text = rdBuf[4].ToString("X02") + rdBuf[5].ToString("X02") + rdBuf[6].ToString("X02") + rdBuf[7].ToString("X02");
            textBox_soatecKD.Text = rdBuf[8].ToString("X02") + rdBuf[9].ToString("X02") + rdBuf[10].ToString("X02") + rdBuf[11].ToString("X02");

            i = 0;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_soatecfP.Text = (u.f).ToString("F4");

            i = 4;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_soatecfI.Text = (u.f).ToString("F4");

            i = 8;
            u.uc0 = rdBuf[i + 3];
            u.uc1 = rdBuf[i + 2];
            u.uc2 = rdBuf[i + 1];
            u.uc3 = rdBuf[i + 0];
            textBox_soatecfD.Text = (u.f).ToString("F4");
        }

        private void button2Write_Click(object sender, EventArgs e)
        {
            byte[] wrBuf = new byte[12];
            byte[] readBuf = new byte[12];
            byte[] wrBuf2 = new byte[12];
            byte[] readBuf2 = new byte[12];
            int i = 0;

            i = 0;
            u.f = Convert.ToSingle(textBox_soafP.Text);
            wrBuf[i + 0] = u.uc3;
            wrBuf[i + 1] = u.uc2;
            wrBuf[i + 2] = u.uc1;
            wrBuf[i + 3] = u.uc0;

            i = 4;
            u.f = Convert.ToSingle(textBox_soafI.Text);
            wrBuf[i + 0] = u.uc3;
            wrBuf[i + 1] = u.uc2;
            wrBuf[i + 2] = u.uc1;
            wrBuf[i + 3] = u.uc0;

            i = 8;
            u.f = Convert.ToSingle(textBox_soafD.Text);
            wrBuf[i + 0] = u.uc3;
            wrBuf[i + 1] = u.uc2;
            wrBuf[i + 2] = u.uc1;
            wrBuf[i + 3] = u.uc0;

            textBox_soaKP.Text = wrBuf[0].ToString("X02") + wrBuf[1].ToString("X02") + wrBuf[2].ToString("X02") + wrBuf[3].ToString("X02");
            textBox_soaKI.Text = wrBuf[4].ToString("X02") + wrBuf[5].ToString("X02") + wrBuf[6].ToString("X02") + wrBuf[7].ToString("X02");
            textBox_soaKD.Text = wrBuf[8].ToString("X02") + wrBuf[9].ToString("X02") + wrBuf[10].ToString("X02") + wrBuf[11].ToString("X02");

            SelectTable(0x0B);

            if (i2c.TWI_WritePage(0xA0, 0xB3, wrBuf, 12) != 12)
            {
                MessageBox.Show("写入 SOA_PID 参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            i2c.TWI_ReadPage(0xA0, 0xB3, readBuf, 12);

            /*if (Bit.ByteEquals(readBuf, wrBuf))
                MessageBox.Show("写入 SOA_PID 参数 成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("写入 SOA_PID 参数 失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);*/

            ///////////////////////////////////////////////////////////
            i = 0;
            u.f = Convert.ToSingle(textBox_soatecfP.Text);
            wrBuf2[i + 0] = u.uc3;
            wrBuf2[i + 1] = u.uc2;
            wrBuf2[i + 2] = u.uc1;
            wrBuf2[i + 3] = u.uc0;

            i = 4;
            u.f = Convert.ToSingle(textBox_soatecfI.Text);
            wrBuf2[i + 0] = u.uc3;
            wrBuf2[i + 1] = u.uc2;
            wrBuf2[i + 2] = u.uc1;
            wrBuf2[i + 3] = u.uc0;

            i = 8;
            u.f = Convert.ToSingle(textBox_soatecfD.Text);
            wrBuf2[i + 0] = u.uc3;
            wrBuf2[i + 1] = u.uc2;
            wrBuf2[i + 2] = u.uc1;
            wrBuf2[i + 3] = u.uc0;

            textBox_soatecKP.Text = wrBuf2[0].ToString("X02") + wrBuf2[1].ToString("X02") + wrBuf2[2].ToString("X02") + wrBuf2[3].ToString("X02");
            textBox_soatecKI.Text = wrBuf2[4].ToString("X02") + wrBuf2[5].ToString("X02") + wrBuf2[6].ToString("X02") + wrBuf2[7].ToString("X02");
            textBox_soatecKD.Text = wrBuf2[8].ToString("X02") + wrBuf2[9].ToString("X02") + wrBuf2[10].ToString("X02") + wrBuf2[11].ToString("X02");

            //SelectTable(0x0B);

            if (i2c.TWI_WritePage(0xA0, 0xC5, wrBuf2, 12) != 12)
            {
                MessageBox.Show("写入 SOA_TEC_PID 参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            i2c.TWI_ReadPage(0xA0, 0xC5, readBuf2, 12);

            if (Bit.ByteEquals(readBuf, wrBuf) && Bit.ByteEquals(readBuf2, wrBuf2))
                MessageBox.Show("写入 SOA_TEC 和 SOA_PID 参数 成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("写入 SOA_TEC 和 SOA_PID 参数 失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button3Read_Click(object sender, EventArgs e)
        {
            byte[] rdBuf = new byte[64];
            int i = 0;
            //Single ftmp;

            SelectTable(0x0C);

            if (i2c.TWI_ReadPage(0xA0, 0x80, rdBuf, 64) != 64)
            {
                MessageBox.Show("读取信息错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //CH0
            {
                i = 0;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch0_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch0_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch0_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch0_c3.Text = (u.f).ToString("E2");
            }

            //CH1
            {
                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch1_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch1_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch1_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch1_c3.Text = (u.f).ToString("E2");
            }

            //CH2
            {
                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch2_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch2_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch2_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch2_c3.Text = (u.f).ToString("E2");
            }

            //CH3
            {
                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch3_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch3_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch3_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_los_ch3_c3.Text = (u.f).ToString("E2");
            }
        }

        private void button3Write_Click(object sender, EventArgs e)
        {
            byte[] wrBuf = new byte[64];
            byte[] readBuf = new byte[64];
            int i = 0;

            //CH0
            {
                i = 0;
                u.f = Convert.ToSingle(textBox_los_ch0_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch0_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch0_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch0_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH1
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH2
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH3
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            SelectTable(0x0C);

            if (i2c.TWI_WritePage(0xA0, 0x80, wrBuf, 64) != 64)
            {
                MessageBox.Show("写入 SOA_LOS 参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            i2c.TWI_ReadPage(0xA0, 0x80, readBuf, 64);

            if (Bit.ByteEquals(readBuf, wrBuf))
                MessageBox.Show("写入 SOA_LOS 参数 成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("写入 SOA_LOS 参数 失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button4Read_Click(object sender, EventArgs e)
        {
            byte[] rdBuf = new byte[64];
            int i = 0;
            //Single ftmp;

            SelectTable(0x0C);

            if (i2c.TWI_ReadPage(0xA0, 0xC0, rdBuf, 64) != 64)
            {
                MessageBox.Show("读取信息错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //CH0
            {
                i = 0;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch0_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch0_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch0_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch0_c3.Text = (u.f).ToString("E2");
            }

            //CH1
            {
                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch1_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch1_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch1_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch1_c3.Text = (u.f).ToString("E2");
            }

            //CH2
            {
                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch2_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch2_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch2_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch2_c3.Text = (u.f).ToString("E2");
            }

            //CH3
            {
                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch3_c0.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch3_c1.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch3_c2.Text = (u.f).ToString("E2");

                i += 4;
                u.uc0 = rdBuf[i + 3];
                u.uc1 = rdBuf[i + 2];
                u.uc2 = rdBuf[i + 1];
                u.uc3 = rdBuf[i + 0];
                textBox_gain_ch3_c3.Text = (u.f).ToString("E2");
            }

            SelectTable(0x0B);
            if (i2c.TWI_ReadPage(0xA0, 0xBF, rdBuf, 4) != 4)
            {
                MessageBox.Show("读取信息错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            u.uc0 = rdBuf[3];
            u.uc1 = rdBuf[2];
            u.uc2 = rdBuf[1];
            u.uc3 = rdBuf[0];
            textBox_gain_k.Text = (u.f).ToString("F2");
        }

        private void button4Write_Click(object sender, EventArgs e)
        {
            byte[] wrBuf = new byte[64];
            byte[] readBuf = new byte[64];
            byte[] wrBuf2 = new byte[4];
            byte[] readBuf2 = new byte[4];
            int i = 0;

            //CH0
            {
                i = 0;
                u.f = Convert.ToSingle(textBox_gain_ch0_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch0_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch0_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch0_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH1
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch1_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch1_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch1_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch1_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH2
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch2_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch2_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch2_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch2_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH3
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch3_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch3_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch3_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_gain_ch3_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            SelectTable(0x0C);

            if (i2c.TWI_WritePage(0xA0, 0xC0, wrBuf, 64) != 64)
            {
                MessageBox.Show("写入 SOA_GAIN 参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            i2c.TWI_ReadPage(0xA0, 0xC0, readBuf, 64);
            
            //GAIN_K
            u.f = Convert.ToSingle(textBox_gain_k.Text);
            wrBuf2[0] = u.uc3;
            wrBuf2[1] = u.uc2;
            wrBuf2[2] = u.uc1;
            wrBuf2[3] = u.uc0;

            SelectTable(0x0B);
            if (i2c.TWI_WritePage(0xA0, 0xBF, wrBuf2, 4) != 4)
            {
                MessageBox.Show("写入 SOA_GAIN_K 参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            i2c.TWI_ReadPage(0xA0, 0xBF, readBuf2, 4);
            //

            if (Bit.ByteEquals(readBuf, wrBuf) && Bit.ByteEquals(readBuf2, wrBuf2))
                MessageBox.Show("写入 SOA_GAIN 参数 成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("写入 SOA_GAIN 参数 失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (SaveSOACalToFlash())
                MessageBox.Show("保存调试参数成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("保存调试参数失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button5Read_Click(object sender, EventArgs e)
        {
            byte[] rdBuf = new byte[39];

            SelectTable(0x0B);

            if (i2c.TWI_ReadPage(0xA0, 0x80, rdBuf, 39) != 39)
            {
                MessageBox.Show("读取信息错误001！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            textBox_iBias_ch_ref.Text = rdBuf[0].ToString();
            textBox_iBias_set.Text = (rdBuf[1] * 256 + rdBuf[2]).ToString();
            textBox_iBias_min.Text = (rdBuf[3] * 256 + rdBuf[4]).ToString();
            textBox_iBias_max.Text = (rdBuf[5] * 256 + rdBuf[6]).ToString();
            textBox_los_hyst.Text = rdBuf[7].ToString();

            textBox_ch0_RSSI_good.Text = (rdBuf[9] * 256 + rdBuf[10]).ToString();
            textBox_ch1_RSSI_good.Text = (rdBuf[11] * 256 + rdBuf[12]).ToString();
            textBox_ch2_RSSI_good.Text = (rdBuf[13] * 256 + rdBuf[14]).ToString();
            textBox_ch3_RSSI_good.Text = (rdBuf[15] * 256 + rdBuf[16]).ToString();

            textBox_ch0_rxp_adc.Text = (rdBuf[31] * 256 + rdBuf[32]).ToString();
            textBox_ch1_rxp_adc.Text = (rdBuf[33] * 256 + rdBuf[34]).ToString();
            textBox_ch2_rxp_adc.Text = (rdBuf[35] * 256 + rdBuf[36]).ToString();
            textBox_ch3_rxp_adc.Text = (rdBuf[37] * 256 + rdBuf[38]).ToString();

            i2c.TWI_ReadPage(0xA0, 0xC3, rdBuf, 2);
            textBox_iBias_los.Text = (rdBuf[0] * 256 + rdBuf[1]).ToString();

            i2c.TWI_ReadPage(0xA0, 0xD1, rdBuf, 2);
            textBox_ase_max.Text = (rdBuf[0] * 256 + rdBuf[1]).ToString();
        }

        private void button5Write_Click(object sender, EventArgs e)
        {
            byte[] wrBuf = new byte[14];
            byte[] readBuf = new byte[14];
            byte[] wrBuf2 = new byte[2];
            byte[] readBuf2 = new byte[2];
            byte[] wrBuf3 = new byte[2];
            byte[] readBuf3 = new byte[2];
            UInt16 ui = 0;

            SelectTable(0x0B);

            if (i2c.TWI_ReadPage(0xA0, 0x83, wrBuf, 14) != 14)
            {
                MessageBox.Show("读取信息错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ui = Convert.ToUInt16(textBox_iBias_min.Text);
            wrBuf[0] = (byte)(ui >> 8);
            wrBuf[1] = (byte)ui;

            ui = Convert.ToUInt16(textBox_iBias_max.Text);
            wrBuf[2] = (byte)(ui >> 8);
            wrBuf[3] = (byte)ui;

            wrBuf[4] = Convert.ToByte(textBox_los_hyst.Text);


            ui = Convert.ToUInt16(textBox_ch0_RSSI_good.Text);
            wrBuf[6] = (byte)(ui >> 8);
            wrBuf[7] = (byte)ui;

            ui = Convert.ToUInt16(textBox_ch1_RSSI_good.Text);
            wrBuf[8] = (byte)(ui >> 8);
            wrBuf[9] = (byte)ui;

            ui = Convert.ToUInt16(textBox_ch2_RSSI_good.Text);
            wrBuf[10] = (byte)(ui >> 8);
            wrBuf[11] = (byte)ui;

            ui = Convert.ToUInt16(textBox_ch3_RSSI_good.Text);
            wrBuf[12] = (byte)(ui >> 8);
            wrBuf[13] = (byte)ui;
            
            if (i2c.TWI_WritePage(0xA0, 0x83, wrBuf, 14) != 14)
            {
                MessageBox.Show("写入 SOA 调试参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            i2c.TWI_ReadPage(0xA0, 0x83, readBuf, 14);

            ui = Convert.ToUInt16(textBox_iBias_los.Text);
            wrBuf2[0] = (byte)(ui >> 8);
            wrBuf2[1] = (byte)ui;
            i2c.TWI_WritePage(0xA0, 0xC3, wrBuf2, 2);
            i2c.TWI_ReadPage(0xA0, 0xC3, readBuf2, 2);

            ui = Convert.ToUInt16(textBox_ase_max.Text);
            wrBuf3[0] = (byte)(ui >> 8);
            wrBuf3[1] = (byte)ui;
            i2c.TWI_WritePage(0xA0, 0xD1, wrBuf3, 2);
            i2c.TWI_ReadPage(0xA0, 0xD1, readBuf3, 2);

            if (Bit.ByteEquals(readBuf, wrBuf) && Bit.ByteEquals(readBuf2, wrBuf2) && Bit.ByteEquals(readBuf3, wrBuf3))
                MessageBox.Show("写入 SOA 调试参数 成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("写入 SOA 调试参数 失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void cBSOA_LOS_Auto_CheckedChanged(object sender, EventArgs e)
        {
            if (cBSOA_LOS_Auto.Checked)
            {
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (GetVCC() < 2.0) //电源电压小于2.00V 异常模块
            {
                moduleOnline = false;
                btnSOAAuto_Status.Text = "请插入模块...";
                btnSOAAuto_Status.BackColor = Color.Orange;
                return;
            }
            else
            {
                //btnSOAAuto_Status.Text = "模块已插入...";
            }
            if (moduleOnline == true)
            {
                return;
            }

            if (SOA_LOS_Test() == false)
            {
                btnSOAAuto_Status.Text = "SOA调试失败";
                btnSOAAuto_Status.BackColor = Color.Red;
                moduleOnline = true;
                return;
            }
            else
            {
                btnSOAAuto_Status.Text = "SOA调试成功";
                btnSOAAuto_Status.BackColor = Color.Green;
                moduleOnline = true;
                return;
            }
        }

        private float GetVCC()
        {
            byte[] readbuffer = new byte[2];
            float vccDDM;
            if (i2c.TWI_ReadPage(0xa0, 26, readbuffer, 2) == 2)
            {
                vccDDM = ((readbuffer[0] * 256 + readbuffer[1]) / 10000.0f);
            }
            else
            {
                Thread.Sleep(100);
                if (i2c.TWI_ReadPage(0xa0, 26, readbuffer, 2) == 2)
                {
                    vccDDM = ((readbuffer[0] * 256 + readbuffer[1]) / 10000.0f);
                }
                else
                {
                    return 0; // Error
                }
            }
            return vccDDM;
        }

        private bool SOA_LOS_Test()
        {
            byte[] rdBuf = new byte[2];
            byte[] writebuffer = new byte[2];
            byte[] pwd = new byte[4];
            double[] rssivalbuf0 = new double[46];
            double[] rssivalbuf1 = new double[46];
            double[] rssivalbuf2 = new double[46];
            double[] rssivalbuf3 = new double[46];
            double[] soavalbuf = new double[46];
            int num = 0;
            byte[] readbuffer = new byte[8];
            byte ch = 0;
            int soaval = 400;//0
            byte soaLUT = 0;
            byte temp = 0;
            string strtmp = "";
            double[] a = new double[5];  //系数
            double[] dt = new double[5];   //误差
            for (int j = 0; j < 5; j++)
                dt[j] = 0.0;
            //
            if ((pwd[0] != 0xA9) || (pwd[1] != 0x46) || (pwd[2] != 0x50) || (pwd[3] != 0x54))
            {
                if (!WriteVenderPWD())//进入调试模式
                {
                    return false;
                }
            }
            if (!SelectTable(6))
            {
                return false;
            }

            if (i2c.TWI_ReadPage(0xA0, 0xCE, rdBuf, 2) != 2)
            {
                return false;//读ROSA-SOA温度设置值错误
            }
            for (int i = 0; i < 4; i++)
            {
                if (i2c.TWI_ReadPage(0xA0, ch, rdBuf, 2) != 2)
                {
                    return false;//读SOA-iBisa设置值错误
                }
            }
            if (i2c.TWI_ReadPage(0xA0, 0xDE, rdBuf, 2) != 2)
            {
                return false;
            }
            //关闭SOA补偿
            soaLUT = i2c.TWI_ReadByte(0xA0, 0xFF);
            temp = soaLUT;
            temp &= 0x10;
            if (temp != 0x10)
            {
                soaLUT |= 0x10; //bit4=1 disable SOA soft control LUT
                if (!i2c.TWI_WriteByte(0xA0, 0xFF, soaLUT))
                {
                    return false;
                }
            }
            while (soaval <= 2300)
            {
                //set SOA_Bias
                BiasSOA_trackBar.Value = soaval;
                Refresh();
                soavalbuf[num] = soaval;
                if (Main_Form.debugMode == true)
                {
                    // SelectTable(6);
                    //2023.11.10                  
                    i2c.TWI_ReadPage(0xa0, 0xE8, readbuffer, 8);
                    strtmp = (readbuffer[0] * 256 + readbuffer[1]).ToString() + " ";
                    strtmp += (readbuffer[2] * 256 + readbuffer[3]).ToString() + "\r\n";
                    strtmp += (readbuffer[4] * 256 + readbuffer[5]).ToString() + " ";
                    strtmp += (readbuffer[6] * 256 + readbuffer[7]).ToString();
                    rssivalbuf0[num] = readbuffer[0] * 256 + readbuffer[1];
                    rssivalbuf1[num] = readbuffer[2] * 256 + readbuffer[3];
                    rssivalbuf2[num] = readbuffer[4] * 256 + readbuffer[5];
                    rssivalbuf3[num] = readbuffer[6] * 256 + readbuffer[7];
                    rxadc4ch_textbox.Text = strtmp;
                }
                soaval += 50;
                num++;
            }
            //ch0
            Bit.iapcir(soavalbuf, rssivalbuf0, 46, a, 4, dt);
            textBox_los_ch0_c0.Text = a[0].ToString("E2");
            textBox_los_ch0_c1.Text = a[1].ToString("E2");
            textBox_los_ch0_c2.Text = a[2].ToString("E2");
            textBox_los_ch0_c3.Text = a[3].ToString("E2");
            Refresh();
            //ch1
            Bit.iapcir(soavalbuf, rssivalbuf1, 46, a, 4, dt);
            textBox_los_ch1_c0.Text = a[0].ToString("E2");
            textBox_los_ch1_c1.Text = a[1].ToString("E2");
            textBox_los_ch1_c2.Text = a[2].ToString("E2");
            textBox_los_ch1_c3.Text = a[3].ToString("E2");
            Refresh();
            //ch2
            Bit.iapcir(soavalbuf, rssivalbuf2, 46, a, 4, dt);
            textBox_los_ch2_c0.Text = a[0].ToString("E2");
            textBox_los_ch2_c1.Text = a[1].ToString("E2");
            textBox_los_ch2_c2.Text = a[2].ToString("E2");
            textBox_los_ch2_c3.Text = a[3].ToString("E2");
            Refresh();
            //ch3
            Bit.iapcir(soavalbuf, rssivalbuf3, 46, a, 4, dt);
            textBox_los_ch3_c0.Text = a[0].ToString("E2");
            textBox_los_ch3_c1.Text = a[1].ToString("E2");
            textBox_los_ch3_c2.Text = a[2].ToString("E2");
            textBox_los_ch3_c3.Text = a[3].ToString("E2");
            Refresh();
            //SOA write
            if (!SOAWrite())
            {
                return false;
            }
            Thread.Sleep(20);
            //保存调试参数
            if (SaveSOACalToFlash())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool SOAWrite()
        {
            byte[] wrBuf = new byte[64];
            byte[] readBuf = new byte[64];
            byte[] wrBuf2 = new byte[4];
            byte[] readBuf2 = new byte[4];
            int i = 0;

            //CH0
            {
                i = 0;
                u.f = Convert.ToSingle(textBox_los_ch0_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch0_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch0_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch0_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH1
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch1_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH2
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch2_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            //CH3
            {
                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c0.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c1.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c2.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;

                i += 4;
                u.f = Convert.ToSingle(textBox_los_ch3_c3.Text);
                wrBuf[i + 0] = u.uc3;
                wrBuf[i + 1] = u.uc2;
                wrBuf[i + 2] = u.uc1;
                wrBuf[i + 3] = u.uc0;
            }

            SelectTable(0x0C);

            if (i2c.TWI_WritePage(0xA0, 0x80, wrBuf, 64) != 64)
            {
                //MessageBox.Show("写入 SOA_GAIN 参数 错误！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            i2c.TWI_ReadPage(0xA0, 0x80, readBuf, 64);

            if (Bit.ByteEquals(readBuf, wrBuf))
                return true;
            //MessageBox.Show("写入 SOA_GAIN 参数 成功！", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                return false;
            // MessageBox.Show("写入 SOA_GAIN 参数 失败！", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void BiasSOA_trackBar_ValueChanged(object sender, EventArgs e)
        {
            byte[] readbuffer = new byte[8];
            double[] rssivalbuf0 = new double[46];
            double[] rssivalbuf1 = new double[46];
            double[] rssivalbuf2 = new double[46];
            double[] rssivalbuf3 = new double[46];
            string strtmp = "";

            BiasSOA_value_textBox.Text = BiasSOA_trackBar.Value.ToString();

            float vsoa = (float)(BiasSOA_trackBar.Value * (2.4 / 4095));
            SOA_V_textBox.Text = vsoa.ToString("F2") + "V";

            if (i2c.TWI_ReadByte(0xA0, 127) != 0x06)
                return;

            byte[] writebuffer = BitConverter.GetBytes((UInt16)BiasSOA_trackBar.Value);
            Array.Reverse(writebuffer); //高字节在前
            i2c.TWI_WritePage(0xA0, 0xDE, writebuffer, 2);
            //read RSSI
            i2c.TWI_ReadPage(0xa0, 0xE8, readbuffer, 8);
            strtmp = (readbuffer[0] * 256 + readbuffer[1]).ToString() + " ";
            strtmp += (readbuffer[2] * 256 + readbuffer[3]).ToString() + "\r\n";
            strtmp += (readbuffer[4] * 256 + readbuffer[5]).ToString() + " ";
            strtmp += (readbuffer[6] * 256 + readbuffer[7]).ToString();
            rxadc4ch_textbox.Text = strtmp;
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


    }
}
