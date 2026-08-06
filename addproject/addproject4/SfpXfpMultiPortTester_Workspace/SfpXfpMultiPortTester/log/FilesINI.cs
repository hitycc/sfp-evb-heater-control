using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
 
 
namespace SfpXfpMultiPortTester
{
    class FilesINI
    {
        // 声明INI文件的写操作函数 WritePrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
 
        // 声明INI文件的读操作函数 GetPrivateProfileString()
        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        public string INIPath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory) + "Config.ini";

        public FilesINI()
        { 
            
        }

        public FilesINI(string file_name)
        {
            INIPath = Convert.ToString(System.AppDomain.CurrentDomain.BaseDirectory) + file_name;
        }

 
        /// 写入INI的方法
        public void INIWrite(string section, string key, string value)
        {
            // section=配置节点名称，key=键名，value=返回键值，path=路径
            WritePrivateProfileString(section, key, value, INIPath);
        }
 
        //读取INI的方法
        public string INIRead(string section, string key)
        {
            // 每次从ini中读取多少字节
            System.Text.StringBuilder temp = new System.Text.StringBuilder(255);
 
            // section=配置节点名称，key=键名，temp=上面，path=路径
            GetPrivateProfileString(section, key, "", temp, 255, INIPath);
            return temp.ToString();
 
        }
 
        //删除INI文件
        public void INIDelete()
        {
            File.Delete(INIPath);
        }
 
    }
}
