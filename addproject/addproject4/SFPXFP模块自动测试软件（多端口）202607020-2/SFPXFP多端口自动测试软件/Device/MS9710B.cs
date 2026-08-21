using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;


namespace FibertopTest_Common
{
  public  class MS9710B
 {
        SerialPort _serialPort = new SerialPort();
        private int WaitCount = 1000; // 等待获取的次数
        private int WaitTimer = 1;    // 等待获取的时间
        string[] PeakSearchArray = {"PEAK","NEXT","LAST","LEFT","RIGHT"};
        string[] SpectrumAnalysisArray = {"2NDPEAK","LEFT","RIGHT"};
        string[] PowerMonitotArray = { "632.8", "850.0", "1300.0", "1550.0" }; 

        /****************************************>  基础配置  <*****************************************/

        
        /******************************************
         * function : 串口通讯配置
         * ***************************************/
        public void MS9710B_ComConfig(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits,int readTimeout)
        {
            _serialPort.PortName = portName; // 设置串口名称
            _serialPort.BaudRate = baudRate; // 设置波特率
            _serialPort.Parity = parity;     // 设置校验位
            _serialPort.DataBits = dataBits; // 设置数据位
            _serialPort.StopBits = stopBits; // 设置停止位
            _serialPort.ReadTimeout = 5000;  // 读取超时时间
            _serialPort.RtsEnable = true;    // 启用RTS
        }

        
        /******************************************
         * function : 打开串口
         * ***************************************/
        public void MS9710B_ComOpen()
        {
            _serialPort.Open();
        }
        
        
        /******************************************
         * function : 关闭串口
         * ***************************************/
        public void MS9710B_ComClose()
        {
            _serialPort.Close();
        }

        
        /******************************************
         * function : 字符串写入(write)接口
         * ***************************************/
        public void MS9710B_WriteString(string str)
        {
            _serialPort.WriteLine(str);
        }

        /******************************************
         * function : 字符串读取(read)接口
         * ***************************************/
        public string MS9710B_ReadString()
        {
            return _serialPort.ReadLine();            
        }
        

        /****************************************>  设备函数  <*****************************************/

        
        /******************************************
         * function : 获取设备相关信息
         * ***************************************/
        public string MS9710B_IDN()
        {
            _serialPort.WriteLine("*IDN?");
            Thread.Sleep(WaitTimer);
            return _serialPort.ReadLine();
        }

        /******************************************
         * function : 设备复位
         * ***************************************/
        public string MS9710B_RST()
        {
            _serialPort.WriteLine("*RST");
            Thread.Sleep(50000);
            _serialPort.WriteLine("ESR2?");
            Thread.Sleep(WaitTimer);
            return _serialPort.ReadLine();
        }


        /******************************************
         * function : DFB-LD Test
         * ***************************************/
        public bool MS9710B_DFB_LD(SpectrumAnalysis s, int ndB)
        {
            string Str = string.Empty;
            int count = 0;
            int ndb = 0;

            // 数据判断
            if (1 <= ndB && ndB <= 50)
            {
                ndb = ndB;
            }
            else
            {
                ndb = 20;// 默认值
            }

            // 发送命令
            _serialPort.WriteLine("AP DFB," + SpectrumAnalysisArray[(int)s] +","+ ndb.ToString());

            // 等待结束
            count = WaitCount;
            do
            {
                _serialPort.WriteLine("ESR2?");
                Thread.Sleep(WaitTimer);
                Str = _serialPort.ReadLine();
                if ((Convert.ToInt32(Str) >> 0 & 0x01) == 1) // bit0
                {
                    return false;
                }
            } while ((count--) != 0);

            return true;
        }


        /******************************************
         * function : FP-LD Test
         * ***************************************/
        public bool MS9710B_FP_LD(int ndB)
        {
            string Str = string.Empty;
            int count = 0;
            int ndb = 0;

            // 数据判断
            if (1 <= ndB && ndB <= 50)
            {
                ndb = ndB;
            }
            else
            {
                ndb = 20;// 默认值
            }

            // 发送命令
            _serialPort.WriteLine("AP FP," + ndb.ToString());

            // 等待结束
            count = WaitCount;
            do
            {
                _serialPort.WriteLine("ESR2?");
                Thread.Sleep(WaitTimer);
                Str = _serialPort.ReadLine();
                if ((Convert.ToInt32(Str) >> 0 & 0x01) == 1) // bit0
                {
                    return false;
                }
            } while ((count--) != 0);

            return true;
        }


        /******************************************
         * function : LED Test
         * ***************************************/
        public bool MS9710B_LED(int ndB,float fdB)
        {
            string Str = string.Empty;
            int count = 0;
            int ndb = 0;
            float fdb = 0;

            // 数据判断
            if (1 <= ndB && ndB <= 50)
            {
                ndb = ndB;
            }
            else
            {
                ndb = 20;// 默认值
            }
            if (-10.00f <= fdB && fdB <= 10.00f)
            {
                fdb = fdB;
            }
            else
            {
                fdb = 0.0f;
            }

            // 发送命令
            _serialPort.WriteLine("AP LED," + ndb.ToString() + "," + fdb.ToString());

            // 等待结束
            count = WaitCount;
            do
            {
                _serialPort.WriteLine("ESR2?");
                Thread.Sleep(WaitTimer);
                Str = _serialPort.ReadLine();
                if ((Convert.ToInt32(Str) >> 0 & 0x01) == 1) // bit0
                {
                    return false;
                }
            } while ((count--) != 0);

            return true;
        }


        /******************************************
         * function : Auto Measure
         * ***************************************/
        public bool MS9710B_AUTO()
        {
            string Str = string.Empty;
            int count = 0;

            // 发送命令
            _serialPort.WriteLine("AUT");

            // 等待结束
            count = WaitCount;
            do
            {
                _serialPort.WriteLine("ESR2?");
                Thread.Sleep(WaitTimer);
                Str = _serialPort.ReadLine();
                if ((Convert.ToInt32(Str) >> 0 & 0x01) == 1) // bit0
                {
                    return false;
                }
            } while ((count--) != 0);

            return true;
        }


        /******************************************
         * function : Application Result
         * ***************************************/
        public string MS9710B_APR()
        {
            string Str = string.Empty;

            // 发送命令
           // _serialPort.WriteLine("ESR2?");
           // Thread.Sleep(WaitTimer);
            _serialPort.WriteLine("APR?");
            Thread.Sleep(WaitTimer);

            // 读取数据
            Str = _serialPort.ReadLine();

            return Str;
        }
        /******************************************
        * function : 波长，边模，谱宽
        * ***************************************/
        /// <summary>
        /// 获取 波长，边模，谱宽
        /// </summary>
        /// <param name="wleth"></波长>
        /// <param name="smsr"></边模>
        /// <param name="spec_width"></谱宽>
        /// <returns></returns>
        public bool MS9710B_Get_spc_data(MS9710B ms9710b,double wleth, double smsr, double spec_width)
        {
            string strval = string.Empty;
            string[] strArray;
            char[] charArray = new char[] { ' ' };
            string[] strnew = new string[600];
            try
            {
                ms9710b.MS9710B_SSI();
                strval = ms9710b.MS9710B_APR();
                strArray = strval.Split(charArray);

                strnew = strArray[0].Split(',');
                if (strnew.Length > 3)
                {
                    spec_width = Convert.ToDouble(strnew[1]);
                    smsr = Convert.ToDouble(strnew[0]);
                    wleth = Convert.ToDouble(strnew[2]);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        /******************************************
         * function : Single Sweep
         * ***************************************/
        public bool MS9710B_SSI()
        {
            string Str = string.Empty;
            int count = 0;

            // 发送命令
            _serialPort.WriteLine("SSI");

            // 等待结束
            count = WaitCount;
            do
            {
                _serialPort.WriteLine("ESR2?");
                Thread.Sleep(WaitTimer);
                Str = _serialPort.ReadLine();
                if ((Convert.ToInt32(Str) >> 1 & 0x01) == 1) // bit1
                {
                    return false;
                }
            } while ((count--) != 0);

            return true;
        }


        /******************************************
         * function : Repeat Sweep
         * ***************************************/
        public bool MS9710B_SRT()
        {
            // 发送命令
            _serialPort.WriteLine("SRT");
            Thread.Sleep(WaitTimer);

            return true;
        }


        /******************************************
         * function : Sweep Stop
         * ***************************************/
        public bool MS9710B_SST()
        {
            // 发送命令
            _serialPort.WriteLine("SST");
            Thread.Sleep(WaitTimer);

            return true;
        }

        public string Ma9710B_SaveImage()
        {
            string Str = string.Empty;
            int count = 0;
            try
            {
                _serialPort.WriteLine("CPY");
                Thread.Sleep(WaitTimer);
                _serialPort.WriteLine("ESR2?");
                Thread.Sleep(WaitTimer);
                _serialPort.WriteLine("DMA?");
                Thread.Sleep(WaitTimer);
                // 等待结束
                count = 1000;
                do
                {
                    Str += (_serialPort.ReadLine()).Trim()+" ";
                    //Thread.Sleep(10);
                    //if ((Convert.ToInt32(Str.Trim()) >> 1 & 0x01) == 1) // bit1
                    //{
                    //    return string.Empty;
                    //}

                } while ((count--) != 0);
            }
            catch
            { }
                       
            return Str;
        }
    }

    // 数据读取结构体
    public struct DFBReadDataStruct
    {
        public string OriginalData; // 读取原数据
        public float SMSR;          // 边模抑制比
        public float Width;         // 谱宽
        public float Peak_nm;       // 峰值波长
        public float Peak_dBm;      // 
        public float LeftPeak_nm;   // 
        public float LeftPeak_dBm;  // 
        public float ModeOffest;    // 
        public float StopBand;      // 
        public float CenterOffset;  // 
    }


    // 峰值波长和波峰
    public enum PeakSearch
    {
        PEAK,   // 检测电平最高的主峰，并将跟踪标记移动到该位置
        NEXT,   // 检测电平仅次于当前峰值的次高峰，并移动标记
        LAST,   // 检测电平仅高于当前峰值的次低峰，并移动标记
        LEFT,   // 检测波长仅次于当前峰值的左侧邻近峰，并移动标记
        RIGHT   // 检测波长仅长于当前峰值的右侧邻近峰，并移动标记
    }
    

    // 边模抑制比
    public enum SpectrumAnalysis
    {
        NDPEAK,  // 以电平次高的边模为基准进行分析
        LEFT,    // 以峰值波长左侧（短波长侧）的边模为基准进行分析
        RIGHT    // 以峰值波长右侧（长波长侧）的边模为基准进行分析
    }
}
