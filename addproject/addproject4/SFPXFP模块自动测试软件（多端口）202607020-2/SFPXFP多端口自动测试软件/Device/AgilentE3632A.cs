using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Resources; 

namespace FibertopTest_Common
{
  public  class AgilentE3632A
  {
        SerialPort agilente3632a;
        public bool Open(string comName)
        {
            try
            {
                agilente3632a = new SerialPort();
                agilente3632a.PortName = comName;
                agilente3632a.BaudRate = 9600;
                agilente3632a.ReadTimeout = 3000;
                agilente3632a.DtrEnable = true;
                agilente3632a.Open();
                agilente3632a.WriteLine("SYSTem:REMote");//远程模式
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool Close()
        {
            try
            {
                agilente3632a.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public double  GetCurrent()
        {
            string curr = "";
            double supply_val = 0;
            try
            {             
                agilente3632a.WriteLine("MEASure:CURRent?");//电流测量
                curr = agilente3632a.ReadLine();//获取电流值
                supply_val = Convert.ToDouble(curr) * 1000;//mA
            }
            catch
            {
                curr = "-1";
            }
            return supply_val;
        }
    }
}
