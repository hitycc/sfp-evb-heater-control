using System;
using System.Text;

namespace FibertopTest_Common
{
    /// <summary>
    /// 基于SFP EVB加热台实现的I2C接口
    /// 替换原来的TWI/CP2112，通过TCP转发I2C命令
    /// </summary>
    public class I2C_Heater : I2C
    {
        private SFP_EVB_Heater _heater;
        private int _slot;  // 槽位号（1~4）

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="heater">加热台对象</param>
        /// <param name="slot">槽位号（1~4）</param>
        public I2C_Heater(SFP_EVB_Heater heater, int slot)
        {
            _heater = heater;
            _slot = slot;
        }

        /// <summary>
        /// 打开I2C连接（加热台已连接的话直接返回true）
        /// </summary>
        public bool TWI_Open()
        {
            // 加热台在外面已经连接好了，这里直接返回状态
            return _heater.IsOpen;
        }

        /// <summary>
        /// 关闭I2C连接
        /// </summary>
        public bool TWI_Close()
        {
            // 不在这里关闭加热台，由外部管理
            return true;
        }

        /// <summary>
        /// 写单字节
        /// </summary>
        /// <param name="DeviceAddress">器件地址（如0xA0）</param>
        /// <param name="WriteDataByteAddress">寄存器地址</param>
        /// <param name="WriteData">要写入的数据</param>
        public bool TWI_WriteByte(byte DeviceAddress, byte WriteDataByteAddress, byte WriteData)
        {
            string deviceAddrStr = DeviceAddress.ToString("X2");
            string regAddrStr = WriteDataByteAddress.ToString("X2");
            string dataStr = WriteData.ToString("X2");

            return _heater.IIC_Set(deviceAddrStr, regAddrStr, "1", dataStr, _slot);
        }

        /// <summary>
        /// 读单字节
        /// </summary>
        /// <param name="DeviceAddress">器件地址（如0xA0）</param>
        /// <param name="WriteDataByteAddress">寄存器地址</param>
        /// <returns>读取到的字节值</returns>
        public byte TWI_ReadByte(byte DeviceAddress, byte WriteDataByteAddress)
        {
            string deviceAddrStr = DeviceAddress.ToString("X2");
            string regAddrStr = WriteDataByteAddress.ToString("X2");

            string res = _heater.IIC_Get(deviceAddrStr, regAddrStr, "1", _slot);
            byte[] data = ParseIICGetData(res);

            if (data != null && data.Length > 0)
            {
                return data[0];
            }
            return 0;
        }

        /// <summary>
        /// 写页（多字节连续写）
        /// </summary>
        /// <param name="DeviceAddress">器件地址</param>
        /// <param name="WriteDataByteAddress">起始寄存器地址</param>
        /// <param name="WriteDataBuffer">数据缓冲区</param>
        /// <param name="num">要写入的字节数</param>
        /// <returns>实际写入的字节数</returns>
        public uint TWI_WritePage(byte DeviceAddress, byte WriteDataByteAddress, byte[] WriteDataBuffer, uint num)
        {
            string deviceAddrStr = DeviceAddress.ToString("X2");
            string regAddrStr = WriteDataByteAddress.ToString("X2");
            string lenStr = num.ToString();

            // 把字节数组转成逗号分隔的十六进制字符串
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < num; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(WriteDataBuffer[i].ToString("X2"));
            }
            string dataStr = sb.ToString();

            bool success = _heater.IIC_Set(deviceAddrStr, regAddrStr, lenStr, dataStr, _slot);

            return success ? num : 0;
        }

        /// <summary>
        /// 读页（多字节连续读）
        /// </summary>
        /// <param name="DeviceAddress">器件地址</param>
        /// <param name="ReadDataByteAddress">起始寄存器地址</param>
        /// <param name="ReadDataBuffer">接收数据的缓冲区</param>
        /// <param name="num">要读取的字节数</param>
        /// <returns>实际读取的字节数</returns>
        public uint TWI_ReadPage(byte DeviceAddress, byte ReadDataByteAddress, byte[] ReadDataBuffer, uint num)
        {
            string deviceAddrStr = DeviceAddress.ToString("X2");
            string regAddrStr = ReadDataByteAddress.ToString("X2");
            string lenStr = num.ToString();

            string res = _heater.IIC_Get(deviceAddrStr, regAddrStr, lenStr, _slot);
            byte[] data = ParseIICGetData(res);

            if (data != null && data.Length > 0)
            {
                int copyLen = Math.Min(data.Length, (int)num);
                Array.Copy(data, ReadDataBuffer, copyLen);
                return (uint)copyLen;
            }
            return 0;
        }

        /// <summary>
        /// 设置模块禁用（控制PowerEN引脚）
        /// </summary>
        /// <param name="dis">true=禁用（断电），false=使能（上电）</param>
        public bool setModuleDis(bool dis)
        {
            // dis=true 表示禁用，对应PowerEN=0
            // dis=false 表示使能，对应PowerEN=1
            int state = dis ? 0 : 1;
            return _heater.SetPowerEN(state, _slot);
        }

        /// <summary>
        /// 读取硬件LOS状态
        /// </summary>
        public bool HardWare_LOS_Get()
        {
            string res = _heater.GetRxLos(_slot);
            // 返回"1"表示有LOS（无光），"0"表示正常
            return res == "1";
        }

        /// <summary>
        /// 解析IIC_Get返回的数据，提取出字节数组
        /// </summary>
        /// <param name="response">加热台返回的原始字符串</param>
        /// <returns>解析出的字节数组，失败返回null</returns>
        private byte[] ParseIICGetData(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            try
            {
                // 返回格式示例：
                // addr:  0,data_length: 9
                // data: 18,41,0,6,ff,0,0,0,1,10

                // 查找"data:"开头的行
                string[] lines = response.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    string trimLine = line.Trim();
                    if (trimLine.StartsWith("data:"))
                    {
                        // 提取逗号分隔的十六进制数据
                        string dataPart = trimLine.Substring(5).Trim();
                        string[] hexValues = dataPart.Split(',');

                        byte[] result = new byte[hexValues.Length];
                        for (int i = 0; i < hexValues.Length; i++)
                        {
                            result[i] = Convert.ToByte(hexValues[i].Trim(), 16);
                        }
                        return result;
                    }
                }

                // 如果没找到"data:"，尝试直接解析逗号分隔的十六进制
                if (response.Contains(","))
                {
                    string[] hexValues = response.Split(',');
                    byte[] result = new byte[hexValues.Length];
                    for (int i = 0; i < hexValues.Length; i++)
                    {
                        string hex = hexValues[i].Trim();
                        // 去掉可能的前缀如"0x"
                        if (hex.StartsWith("0x") || hex.StartsWith("0X"))
                        {
                            hex = hex.Substring(2);
                        }
                        result[i] = Convert.ToByte(hex, 16);
                    }
                    return result;
                }
            }
            catch
            {
                // 解析失败返回null
            }

            return null;
        }
    }
}
