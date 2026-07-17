using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace FibertopTest_Common
{
    public class TWI : I2C
    {
        private object cc = new object();
        //////端口PIn定义///
        private byte SDA_MASK = 0x10;
        private byte SDA_R_MASK = 0x10;
        private byte SCL_MASK = 0x8;

        private byte SFP_R_INSERT = 0x8; // BIT3 检测模块是否在位  插入为低电平
        private byte Module_Dis = 0x1;//输入 bit0 控制模块硬件Disable//
        private byte HARDWARE_LOS = 0x20; //BIT5 检测模块硬件LOS LOS为高电平;

        public byte IIC_ADDR = 0xA0;
        public short LPTADDR = 0x378;
        public UInt32 Frequten = 100000;
        public bool IIC_Error = false;

        private short m_PortData = 0xff;
        private const byte PAGE_SIZE = 8;

        private const int ms_Delay = 5; //ms

        //**************************************************************************
        //
        // FUNCTION IMPORTS FROM DLPortIO.DLL
        //
        //**************************************************************************
        // Built-in Windows API functions to allow us to dynamically load our own DLL.

        [DllImport("inpout32.dll")]
        private static extern UInt32 IsInpOutDriverOpen();
        [DllImport("inpout32.dll")]
        private static extern void Out32(short PortAddress, short Data);
        [DllImport("inpout32.dll")]
        private static extern char Inp32(short PortAddress);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        [DllImport("inpoutx64.dll", EntryPoint = "IsInpOutDriverOpen")]
        private static extern UInt32 IsInpOutDriverOpen_x64();
        [DllImport("inpoutx64.dll", EntryPoint = "Out32")]
        private static extern void Out32_x64(short PortAddress, short Data);
        [DllImport("inpoutx64.dll", EntryPoint = "Inp32")]
        private static extern char Inp32_x64(short PortAddress);

        bool m_bX64 = false;

        void SDA0()
        {
            m_PortData &= (byte)~SDA_MASK;
            if (m_bX64 == false)
                Out32(LPTADDR, m_PortData);
            else
                Out32_x64(LPTADDR, m_PortData);
        }

        void SDA1()
        {
            m_PortData |= (short)SDA_MASK;
            if (m_bX64 == false)
                Out32(LPTADDR, m_PortData);
            else
                Out32_x64(LPTADDR, m_PortData);
        }

        void SCL0()
        {
            m_PortData &= (byte)~SCL_MASK;
            if (m_bX64 == false)
                Out32(LPTADDR, m_PortData);
            else
                Out32_x64(LPTADDR, m_PortData);
        }

        void SCL1()
        {
            m_PortData |= (short)SCL_MASK;
            if (m_bX64 == false)
                Out32(LPTADDR, m_PortData);
            else
                Out32_x64(LPTADDR, m_PortData);
        }

        void iic_delay()
        {
            long cpufre;
            long cc0, cc1;
            QueryPerformanceFrequency(out cpufre);
            QueryPerformanceCounter(out cc0);
            while (true)
            {
                QueryPerformanceCounter(out cc1);
                double d = (double)(cc1 - cc0);
                if ((double)cpufre / d < Frequten)
                {
                    break;
                }
            }
        }

        public TWI()
        {
            try
            {
                uint nResult = 0;
                try
                {
                    nResult = IsInpOutDriverOpen();
                }
                catch (BadImageFormatException)
                {
                    nResult = IsInpOutDriverOpen_x64();
                    if (nResult != 0)
                        m_bX64 = true;
                }
            }
            catch (DllNotFoundException ex)
            {
                throw (ex);
            }

            SDA1();
            SCL1();
        }

        public TWI(uint parAddr, uint frequent)
            : this()
        {
            LPTADDR = (short)parAddr;
            Frequten = frequent;
        }

        void Start()
        {
            SDA1();
            iic_delay();
            SCL1();
            iic_delay();
            SDA0();
            iic_delay();
            SCL0();
            iic_delay();
        }

        void Stop()
        {
            SCL0();
            iic_delay();
            SDA0();
            iic_delay();
            SCL1();
            iic_delay();
            SDA1();
            iic_delay();
        }

        byte GetSDA()
        {
            byte n = 0;
            if (m_bX64 == false)
                n = (byte)Inp32((short)(LPTADDR + 1));
            else
                n = (byte)Inp32_x64((short)(LPTADDR + 1));
            //
            n &= SDA_R_MASK;
            return (byte)(n / SDA_R_MASK);
        }

        // 自动测试软件使用  手动调试软件不需要用此功能
        bool GetSFPinsert()
        {
            byte n = 0x00;
            //
            if (m_bX64 == false)
            {
                n = (byte)Inp32((short)(LPTADDR + 1));
            }
            else
            {
                n = (byte)Inp32_x64((short)(LPTADDR + 1));
            }
            n &= SFP_R_INSERT;
            //
            if (n != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //自动测试 模块硬件LOS
        public bool HardWare_LOS_Get()
        {
            byte n = 0x00;
            //
            if (m_bX64 == false)
            {
                n = (byte)Inp32((short)(LPTADDR + 1));
            }
            else
            {
                n = (byte)Inp32_x64((short)(LPTADDR + 1));
            }
            n &= HARDWARE_LOS;
            //
            if (n != 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //自动测试 模块硬件Disable
        public bool setModuleDis(bool dis)
        {
            byte n = 0x00;

            if (m_bX64 == false)
            {
                n = (byte)Inp32((short)(LPTADDR + 2));
            }
            else
            {
                n = (byte)Inp32_x64((short)(LPTADDR + 2));
            }
            n &= 0xFF;

            if (dis == false)
            {
                Module_Dis |= 0x01;//0x0F;
            }
            else
            {
                Module_Dis &= 0x02;//

            }
            //
            if (m_bX64 == false)
                Out32((short)(LPTADDR + 2), Module_Dis);
            else
                Out32_x64((short)(LPTADDR + 2), Module_Dis);
            return true;
        }

        bool WriteByte(byte data)
        {
            int i = 0;

            //SCL0();
            //iic_delay();
            
            for (i = 7; i >= 0; i--)
            {
                if (Bit.GetBit(data, i))
                {
                    SDA1();
                }
                else
                {
                    SDA0();
                }
                iic_delay();
                SCL1();
                iic_delay();
                iic_delay();
                SCL0();
                iic_delay();
            }
            
            //iic_delay();
            SDA1();
            iic_delay();
            SCL1();
            iic_delay();

            // 等待从机ACK
            i = 0;
            do
            {
                iic_delay();
                if (GetSDA() == 0)
                {
                    SCL0();
                    iic_delay();
                    return true; // ACK  从机响应 返回成功
                }
                i++;
            } while (i <= 6); // 延时等待判断6次

            return false; // NOACK  从机未响应  返回失败
        }

        byte ReadByte(bool ACK)
        {
            int i = 0;
            byte RVal = 0;

            //SCL0();
            //iic_delay();
            SDA1();
            iic_delay();

            for (i = 0; i < 8; i++)
            {
                SCL1();
                iic_delay();
                iic_delay();
                RVal <<= 1;
                if (GetSDA() == 0)
                {
                    RVal &= 0xFE;
                }
                else
                {
                    RVal |= 0x01;
                }
                SCL0();
                iic_delay();
            }

            // Send ACK or NOACK
            if (ACK)
            {
                SDA0();
            }
            else
            {
                SDA1();
            }
            iic_delay();
            iic_delay();
            SCL1();
            iic_delay();
            iic_delay();
            SCL0();
            iic_delay();
            
            return RVal; // 返回读取的字节
        }

        int WriteBuf(byte I2CADDR, byte waddr, byte[] data, uint wlen)
        {
            IIC_ADDR = I2CADDR;
            return WriteBuf(waddr, data, wlen);
        }

        int WriteBuf(byte waddr, byte[] data, uint wlen)
        {
            int i;
            byte[] dp = data;

            Start();
            if (!WriteByte(IIC_ADDR))
            {
                IIC_Error = true;
                Stop();
                return 0;
            }
            if (!WriteByte(waddr))
            {
                Stop();
                return 0;
            }
            for (i = 0; i < wlen; i++)
            {
                if (!WriteByte(dp[i]))
                {
                    Stop();
                    goto POS_STP; //return i + 1;
                }
            }
            Stop();

          POS_STP:
            //一次扇区写入后进行延时操作
            Thread.Sleep(ms_Delay);
            if (GlobalVarFun.moduleType == "SFPP-GN1196" || GlobalVarFun.moduleType == "SFP-GN25L95" || GlobalVarFun.moduleType == "SFP-GN25L96" || GlobalVarFun.moduleType == "SFP-UX3320C" || GlobalVarFun.moduleType == "SFP-UX3320T")
            {
                Thread.Sleep(6); //针对GN25L95/GN25L96/UX3320C/UX3320T写操作增加延时6ms
            }

            return i;
        }

        int ReadBuf(byte I2CADDR, byte Raddr, byte[] data, uint rlen)
        {
            IIC_ADDR = I2CADDR;
            return ReadBuf(Raddr, rlen, data);
        }

        int ReadBuf(byte Raddr, uint rlen, byte[] data)
        {
            int i;
            byte[] rp = data;

            Start();
            if (!WriteByte(IIC_ADDR))
            {
                IIC_Error = true;
                Stop();
                return 0;
            }
            if (!WriteByte(Raddr))
            {
                Stop();
                return 0;
            }
            //Stop();
            Start(); // ReStart
            if (!WriteByte(Bit.SetBit(IIC_ADDR, 0)))
            {
                Stop();
                return 0;
            }
            for (i = 0; i < rlen - 1; i++)
            {
                rp[i] = ReadByte(true);
            }
            rp[rlen - 1] = ReadByte(false);
            Stop();

            return i + 1;
        }

        public bool TWI_WriteByte(byte i2cadd, byte eepromadd, byte data)
        {
            lock (cc)
            {
                // 判断SFP模块是否在位
                if (GetSFPinsert() == false) return false;
                byte[] temp = new byte[1] { data };
                return (1 == WriteBuf(i2cadd, eepromadd, temp, 1));
            }
        }

        public byte TWI_ReadByte(byte i2cadd, byte eepromadd)
        {
            lock (cc)
            {
                // 判断SFP模块是否在位
                if (GetSFPinsert() == false) return 0;
                byte[] temp = new byte[1];
                ReadBuf(i2cadd, eepromadd, temp, 1);
                return temp[0];
            }
        }

        public uint TWI_WritePage(byte i2cadd, byte eepromadd, byte[] writedata, uint num)
        {
            lock (cc)
            {
                // 判断SFP模块是否在位
                if (GetSFPinsert() == false) return 0;

                uint totalnum = num;
                uint a = (uint)eepromadd / PAGE_SIZE;

                totalnum += eepromadd;
                if (totalnum > 256) return 0; // Error

                totalnum = num;

                if (a != 0)
                {
                    a = PAGE_SIZE * (a + 1) - eepromadd;
                }
                else
                {
                    a = (uint)(PAGE_SIZE - eepromadd);
                }
                if (a >= num)
                {
                    if (WriteBuf(i2cadd, eepromadd, writedata, num) != num) return 0;
                    //Thread.Sleep(ms_Delay);
                }
                else
                {
                    if (WriteBuf(i2cadd, eepromadd, writedata, a) != a) return 0;
                    //Thread.Sleep(ms_Delay);
                    num -= a;
                    eepromadd += (byte)a;
                    while (num >= PAGE_SIZE)
                    {
                        byte[] ExWriteData = new byte[num];
                        for (int i = 0; i < num; i++)
                        {
                            ExWriteData[i] = writedata[i + totalnum - num];
                        }
                        if (WriteBuf(i2cadd, eepromadd, ExWriteData, PAGE_SIZE) != PAGE_SIZE) return 0;
                        //Thread.Sleep(ms_Delay);
                        num -= PAGE_SIZE;
                        eepromadd += PAGE_SIZE;
                    }
                    if (num != 0)
                    {
                        byte[] ExWriteData = new byte[num];
                        for (int i = 0; i < num; i++)
                        {
                            ExWriteData[i] = writedata[i + totalnum - num];
                        }
                        if (WriteBuf(i2cadd, eepromadd, ExWriteData, num) != num) return 0;
                        //Thread.Sleep(ms_Delay);
                    }
                }
                return totalnum;
            }
        }

        public uint TWI_ReadPage(byte i2cadd, byte eepromadd, byte[] data, uint num)
        {
            lock (cc)
            {
                // 判断SFP模块是否在位
                if (GetSFPinsert() == false) return 0;

                uint totalnum = num;
                totalnum += eepromadd;
                if (totalnum > 256) return 0; // Error

                return (uint)ReadBuf(i2cadd, eepromadd, data, num);
            }

        }

        public bool TWI_Open()
        {
            return true;
        }

        public bool TWI_Close()
        {
            return true;
        }
    }
}


