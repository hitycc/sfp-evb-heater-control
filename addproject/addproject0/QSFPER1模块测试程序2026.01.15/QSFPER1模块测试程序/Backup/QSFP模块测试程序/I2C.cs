using System;

namespace Fibertower_Common
{
    public interface I2C
    {
        bool TWI_Open();
        bool TWI_WriteByte(byte DeviceAddress, byte WriteDataByteAddress, byte WriteData);
        byte TWI_ReadByte(byte DeviceAddress, byte WriteDataByteAddress);
        uint TWI_WritePage(byte DeviceAddress, byte WriteDataByteAddress, byte[] WriteDataBuffer, uint num);
        uint TWI_ReadPage(byte DeviceAddress, byte ReadDataByteAddress, byte[] ReadDataBuffer, uint num);
        bool TWI_Close();
    }  
}
