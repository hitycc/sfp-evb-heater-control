/////////////////////////////////////////////////////////////////////////////
// SLABCP2112.cs
// For SLABHIDtoSMBus.dll version 1.3
// and Silicon Labs CP2112 HID to SMBus
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// Namespaces
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


/////////////////////////////////////////////////////////////////////////////
// SLABHIDtoSMBus.dll Namespace
/////////////////////////////////////////////////////////////////////////////

namespace FibertopTest_Common
{
    /////////////////////////////////////////////////////////////////////////////
    // SLABHIDtoSMBus.dll Imports
    /////////////////////////////////////////////////////////////////////////////

    public class CP2112
    {
        /////////////////////////////////////////////////////////////////////////////
        // Return Code Definitions
        /////////////////////////////////////////////////////////////////////////////

        #region Return Code Definitions

        // HID_SMBUS_STATUS Return Codes
        private const byte HID_SMBUS_SUCCESS = 0x00;
        private const byte HID_SMBUS_DEVICE_NOT_FOUND = 0x01;
        private const byte HID_SMBUS_INVALID_HANDLE = 0x02;
        private const byte HID_SMBUS_INVALID_DEVICE_OBJECT = 0x03;
        private const byte HID_SMBUS_INVALID_PARAMETER = 0x04;
        private const byte HID_SMBUS_INVALID_REQUEST_LENGTH = 0x05;

        private const byte HID_SMBUS_READ_ERROR = 0x10;
        private const byte HID_SMBUS_WRITE_ERROR = 0x11;
        private const byte HID_SMBUS_READ_TIMED_OUT = 0x12;
        private const byte HID_SMBUS_WRITE_TIMED_OUT = 0x13;
        private const byte HID_SMBUS_DEVICE_IO_FAILED = 0x14;
        private const byte HID_SMBUS_DEVICE_ACCESS_ERROR = 0x15;
        private const byte HID_SMBUS_DEVICE_NOT_SUPPORTED = 0x16;

        private const byte HID_SMBUS_UNKNOWN_ERROR = 0xFF;

        private const byte HID_SMBUS_S0_IDLE = 0x00;
        private const byte HID_SMBUS_S0_BUSY = 0x01;
        private const byte HID_SMBUS_S0_COMPLETE = 0x02;
        private const byte HID_SMBUS_S0_ERROR = 0x03;

        // HID_SMBUS_TRANSFER_S0 = HID_SMBUS_S0_BUSY
        private const byte HID_SMBUS_S1_BUSY_ADDRESS_ACKED = 0x00;
        private const byte HID_SMBUS_S1_BUSY_ADDRESS_NACKED = 0x01;
        private const byte HID_SMBUS_S1_BUSY_READING = 0x02;
        private const byte HID_SMBUS_S1_BUSY_WRITING = 0x03;

        // HID_SMBUS_TRANSFER_S0 = HID_SMBUS_S0_ERROR
        private const byte HID_SMBUS_S1_ERROR_TIMEOUT_NACK = 0x00;
        private const byte HID_SMBUS_S1_ERROR_TIMEOUT_BUS_NOT_FREE = 0x01;
        private const byte HID_SMBUS_S1_ERROR_ARB_LOST = 0x02;
        private const byte HID_SMBUS_S1_ERROR_READ_INCOMPLETE = 0x03;
        private const byte HID_SMBUS_S1_ERROR_WRITE_INCOMPLETE = 0x04;
        private const byte HID_SMBUS_S1_ERROR_SUCCESS_AFTER_RETRY = 0x05;

        #endregion

        /////////////////////////////////////////////////////////////////////////////
        // String Definitions
        /////////////////////////////////////////////////////////////////////////////

        #region String Definitions

        // Product String Types
        private const uint HID_SMBUS_GET_VID_STR = 0x01;
        private const uint HID_SMBUS_GET_PID_STR = 0x02;
        private const uint HID_SMBUS_GET_PATH_STR = 0x03;
        private const uint HID_SMBUS_GET_SERIAL_STR = 0x04;
        private const uint HID_SMBUS_GET_MANUFACTURER_STR = 0x05;
        private const uint HID_SMBUS_GET_PRODUCT_STR = 0x06;

        // String Lengths
        private const uint HID_SMBUS_DEVICE_STRLEN = 260;

        #endregion

        /////////////////////////////////////////////////////////////////////////////
        // SMBUS Definitions
        /////////////////////////////////////////////////////////////////////////////

        #region SMBUS Definitions

        // SMbus Configuration Limits
        private const uint HID_SMBUS_MIN_BIT_RATE = 1;
        private const ushort HID_SMBUS_MIN_TIMEOUT = 0;
        private const ushort HID_SMBUS_MAX_TIMEOUT = 1000;
        private const ushort HID_SMBUS_MAX_RETRIES = 1000;
        private const byte HID_SMBUS_MIN_ADDRESS = 0x02;
        private const byte HID_SMBUS_MAX_ADDRESS = 0xFE;

        // Read/Write Limits
        private const ushort HID_SMBUS_MIN_READ_REQUEST_SIZE = 1;
        private const ushort HID_SMBUS_MAX_READ_REQUEST_SIZE = 512;
        private const byte HID_SMBUS_MIN_TARGET_ADDRESS_SIZE = 1;
        private const byte HID_SMBUS_MAX_TARGET_ADDRESS_SIZE = 16;
        private const byte HID_SMBUS_MAX_READ_RESPONSE_SIZE = 61;
        private const byte HID_SMBUS_MIN_WRITE_REQUEST_SIZE = 1;
        private const byte HID_SMBUS_MAX_WRITE_REQUEST_SIZE = 61;

        #endregion

        /////////////////////////////////////////////////////////////////////////////
        // GPIO Definitions
        /////////////////////////////////////////////////////////////////////////////

        #region GPIO Definitions

        // GPIO Pin Direction Bit Value
        private const byte HID_SMBUS_DIRECTION_INPUT = 0;
        private const byte HID_SMBUS_DIRECTION_OUTPUT = 1;

        // GPIO Pin Mode Bit Value
        private const byte HID_SMBUS_MODE_OPEN_DRAIN = 0;
        private const byte HID_SMBUS_MODE_PUSH_PULL = 1;

        // GPIO Function Bitmask
        private const byte HID_SMBUS_MASK_FUNCTION_GPIO_7_CLK = 0x01;
        private const byte HID_SMBUS_MASK_FUNCTION_GPIO_0_TXT = 0x02;
        private const byte HID_SMBUS_MASK_FUNCTION_GPIO_1_RXT = 0x04;

        // GPIO Function Bit Value
        private const byte HID_SMBUS_GPIO_FUNCTION = 0;
        private const byte HID_SMBUS_SPECIAL_FUNCTION = 1;

        // GPIO Pin Bitmask
        private const byte HID_SMBUS_MASK_GPIO_0 = 0x01;
        private const byte HID_SMBUS_MASK_GPIO_1 = 0x02;
        private const byte HID_SMBUS_MASK_GPIO_2 = 0x04;
        private const byte HID_SMBUS_MASK_GPIO_3 = 0x08;
        private const byte HID_SMBUS_MASK_GPIO_4 = 0x10;
        private const byte HID_SMBUS_MASK_GPIO_5 = 0x20;
        private const byte HID_SMBUS_MASK_GPIO_6 = 0x40;
        private const byte HID_SMBUS_MASK_GPIO_7 = 0x80;

        #endregion

        /////////////////////////////////////////////////////////////////////////////
        // Part Number Definitions
        /////////////////////////////////////////////////////////////////////////////

        #region Part Number Definitions

        // Part Numbers
        private const byte HID_SMBUS_PART_CP2112 = 0x0C;
        private const ushort HID_SMBUS_VID_CP2112 = 0x10C4;
        private const ushort HID_SMBUS_PID_CP2112 = 0xEA90;

        #endregion

        /////////////////////////////////////////////////////////////////////////////
        // User Customization Definitions
        /////////////////////////////////////////////////////////////////////////////

        #region User Customization Definitions

        // User-Customizable Field Lock Bitmasks
        private const byte HID_SMBUS_LOCK_VID = 0x01;
        private const byte HID_SMBUS_LOCK_PID = 0x02;
        private const byte HID_SMBUS_LOCK_POWER = 0x04;
        private const byte HID_SMBUS_LOCK_POWER_MODE = 0x08;
        private const byte HID_SMBUS_LOCK_RELEASE_VERSION = 0x10;
        private const byte HID_SMBUS_LOCK_MFG_STR = 0x20;
        private const byte HID_SMBUS_LOCK_PRODUCT_STR = 0x40;
        private const byte HID_SMBUS_LOCK_SERIAL_STR = 0x80;

        // Field Lock Bit Values
        private const byte HID_SMBUS_LOCK_UNLOCKED = 1;
        private const byte HID_SMBUS_LOCK_LOCKED = 0;

        // Power Max Value (500 mA)
        private const byte HID_SMBUS_BUS_POWER_MAX = 0xFA;

        // Power Modes
        private const byte HID_SMBUS_BUS_POWER = 0x00;
        private const byte HID_SMBUS_SELF_POWER_VREG_DIS = 0x01;
        private const byte HID_SMBUS_SELF_POWER_VREG_EN = 0x02;

        // USB Config Bitmasks
        private const byte HID_SMBUS_SET_VID = 0x01;
        private const byte HID_SMBUS_SET_PID = 0x02;
        private const byte HID_SMBUS_SET_POWER = 0x04;
        private const byte HID_SMBUS_SET_POWER_MODE = 0x08;
        private const byte HID_SMBUS_SET_RELEASE_VERSION = 0x10;

        // USB Config Bit Values
        private const byte HID_SMBUS_SET_IGNORE = 0;
        private const byte HID_SMBUS_SET_PROGRAM = 1;

        // String Lengths
        private const byte HID_SMBUS_CP2112_MFG_STRLEN = 30;
        private const byte HID_SMBUS_CP2112_PRODUCT_STRLEN = 30;
        private const byte HID_SMBUS_CP2112_SERIAL_STRLEN = 30;

        #endregion

        /////////////////////////////////////////////////////////////////////////////
        // Exported Library Functions
        /////////////////////////////////////////////////////////////////////////////

        #region Exported Library Functions

        // HidSmbus_GetNumDevices
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetNumDevices(ref uint numDevices, ushort vid, ushort pid);

        // HidSmbus_GetString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetString(uint deviceNum, ushort vid, ushort pid, StringBuilder deviceString, uint options);

        // HidSmbus_GetOpenedString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetOpenedString(IntPtr device, StringBuilder deviceString, uint options);

        // HidSmbus_GetIndexedString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetIndexedString(uint deviceNum, ushort vid, ushort pid, uint stringIndex, StringBuilder deviceString);

        // HidSmbus_GetOpenedIndexedString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetOpenedIndexedString(IntPtr device, uint stringIndex, StringBuilder deviceString);

        // HidSmbus_GetAttributes
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetAttributes(uint deviceNum, ushort vid, ushort pid, ref ushort deviceVid, ref ushort devicePid, ref ushort deviceReleaseNumber);

        // HidSmbus_GetOpenedAttributes
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetOpenedAttributes(IntPtr device, ref ushort deviceVid, ref ushort devicePid, ref ushort deviceReleaseNumber);

        // HidSmbus_Open
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_Open(ref IntPtr device, uint deviceNum, ushort vid, ushort pid);

        // HidSmbus_Close
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_Close(IntPtr device);

        // HidSmbus_IsOpened
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_IsOpened(IntPtr device, ref int opened);

        // HidSmbus_ReadRequest
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_ReadRequest(IntPtr device, byte slaveAddress, ushort numBytesToRead);

        // HidSmbus_AddressReadRequest
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_AddressReadRequest(IntPtr device, byte slaveAddress, ushort numBytesToRead, byte targetAddressSize, byte[] targetAddress);

        // HidSmbus_ForceReadResponse
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_ForceReadResponse(IntPtr device, ushort numBytesToRead);

        // HidSmbus_ForceReadResponse
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetReadResponse(IntPtr device, ref byte status, byte[] buffer, byte bufferSize, ref byte numBytesRead);

        // HidSmbus_WriteRequest
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_WriteRequest(IntPtr device, byte slaveAddress, byte[] buffer, byte numBytesToWrite);

        // HidSmbus_TransferStatusRequest
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_TransferStatusRequest(IntPtr device);

        // HidSmbus_GetTransferStatusResponse
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetTransferStatusResponse(IntPtr device, ref byte status, ref byte detailedStatus, ref ushort numRetries, ref ushort bytesRead);

        // HidSmbus_CancelTransfer
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_CancelTransfer(IntPtr device);

        // HidSmbus_CancelIo
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_CancelIo(IntPtr device);

        // HidSmbus_SetTimeouts
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetTimeouts(IntPtr device, uint responseTimeout);

        // HidSmbus_GetTimeouts
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetTimeouts(IntPtr device, ref uint responseTimeout);

        // HidSmbus_SetSmbusConfig
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetSmbusConfig(IntPtr device, uint bitRate, byte address, int autoReadRespond, ushort writeTimeout, ushort readTimeout, int sclLowTimeout, ushort transferRetries);

        // HidSmbus_GetSmbusConfig
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetSmbusConfig(IntPtr device, ref uint bitRate, ref byte address, ref int autoReadRespond, ref ushort writeTimeout, ref ushort readTimeout, ref int sclLowtimeout, ref ushort transferRetries);

        // HidSmbus_Reset
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_Reset(IntPtr device);

        // HidSmbus_SetGpioConfig
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetGpioConfig(IntPtr device, byte direction, byte mode, byte function, byte clkDiv);

        // HidSmbus_GetGpioConfig
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetGpioConfig(IntPtr device, ref byte direction, ref byte mode, ref byte function, ref byte clkDiv);

        // HidSmbus_ReadLatch
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_ReadLatch(IntPtr device, ref byte latchValue);

        // HidSmbus_WriteLatch
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_WriteLatch(IntPtr device, byte latchValue, byte latchMask);

        // HidSmbus_GetPartNumber
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetPartNumber(IntPtr device, ref byte partNumber, ref byte version);

        // HidSmbus_GetLibraryVersion
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetLibraryVersion(ref byte major, ref byte minor, ref int release);

        // HidSmbus_GetHidLibraryVersion
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetHidLibraryVersion(ref byte major, ref byte minor, ref int release);

        // HidSmbus_GetHidGuid
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetHidGuid(ref Guid guid);

        #endregion

        /////////////////////////////////////////////////////////////////////////////
        // Exported Library Functions - Device Customization
        /////////////////////////////////////////////////////////////////////////////

        #region Exported Library Functions - Device Customization

        // HidSmbus_SetLock
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetLock(IntPtr device, byte lockValue);

        // HidSmbus_GetLock
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetLock(IntPtr device, ref byte lockValue);

        // HidSmbus_SetUsbConfig
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetUsbConfig(IntPtr device, ushort vid, ushort pid, byte power, byte powerMode, ushort releaseVersion, byte mask);

        // HidSmbus_GetUsbConfig
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetUsbConfig(IntPtr device, ref ushort vid, ref ushort pid, ref byte power, ref byte powerMode, ref ushort releaseVersion);

        // HidSmbus_SetManufacturingString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetManufacturingString(IntPtr device, byte[] manufacturingString, byte strlen);

        // HidSmbus_GetManufacturingString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetManufacturingString(IntPtr device, StringBuilder manufacturingString, ref byte strlen);

        // HidSmbus_SetProductString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetProductString(IntPtr device, byte[] productString, byte strlen);

        // HidSmbus_GetProductString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetProductString(IntPtr device, StringBuilder productString, ref byte strlen);

        // HidSmbus_SetSerialString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_SetSerialString(IntPtr device, byte[] serialString, byte strlen);

        // HidSmbus_GetSerialString
        [DllImport("SLABHIDtoSMBus.dll")]
        private static extern int HidSmbus_GetSerialString(IntPtr device, StringBuilder serialString, ref byte strlen);

        #endregion

        private object ccc = new object();

        private IntPtr device = IntPtr.Zero;

        public bool SetGPIO_0
        {
            set
            {
                byte latchValue = 0x00;
                if (value)
                    latchValue |= HID_SMBUS_MASK_GPIO_0;
                CP2112.HidSmbus_WriteLatch(device, latchValue, 0x01);
            }
        }

        public bool SetGPIO_1
        {
            set
            {
                byte latchValue = 0x00;
                if (value)
                    latchValue |= HID_SMBUS_MASK_GPIO_1;
                CP2112.HidSmbus_WriteLatch(device, latchValue, 0x02);
            }
        }

        public bool SetGPIO_2
        {
            set
            {
                byte latchValue = 0x00;
                if (value)
                    latchValue |= HID_SMBUS_MASK_GPIO_2;
                CP2112.HidSmbus_WriteLatch(device, latchValue, 0x04);
            }
        }

        private int GetTransferStatus(IntPtr device, ref byte status_i2c_s0, ref byte status_i2c_s1, ref ushort retry, ref ushort readnum)
        {
            int status = 0;
            if (device == IntPtr.Zero)
                return CP2112.HID_SMBUS_S0_ERROR;
            else
            {
                status = CP2112.HidSmbus_TransferStatusRequest(device);
                System.Threading.Thread.Sleep(5);
                status = CP2112.HidSmbus_GetTransferStatusResponse(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref readnum);
                return status;
            }
        }

        public byte TWI_ReadByte(byte DeviceAddress, byte WriteDataByteAddress)
        {
            lock (ccc)
            {
                int opened = 0;
                int status = 0;
                byte status_i2c_s0 = 0;
                byte status_i2c_s1 = 0;
                ushort retry = 0;
                ushort readnum = 0;
                byte numBytesRead = 0;
                byte[] readbuf = new byte[61];
                byte[] targetaddress = new byte[1];
                targetaddress[0] = WriteDataByteAddress;

                if (CP2112.HidSmbus_IsOpened(device, ref opened) == CP2112.HID_SMBUS_SUCCESS && opened == 1)
                {
                    do
                    {
                        status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                    }
                    while (status_i2c_s0 != CP2112.HID_SMBUS_S0_IDLE);
                    status = CP2112.HidSmbus_AddressReadRequest(device, DeviceAddress, 1, 1, targetaddress);
                    do
                    {
                        status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                    }
                    while (status_i2c_s0 != CP2112.HID_SMBUS_S0_COMPLETE && status_i2c_s0 != CP2112.HID_SMBUS_S0_IDLE);

                    if (status == CP2112.HID_SMBUS_SUCCESS && readnum > 0)
                    {
                        status = CP2112.HidSmbus_ForceReadResponse(device, readnum);
                        status = CP2112.HidSmbus_GetReadResponse(device, ref status_i2c_s0, readbuf, 61, ref numBytesRead);
                        if (status == CP2112.HID_SMBUS_SUCCESS && numBytesRead == readnum)
                            return readbuf[0];
                        else
                            return 0;
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        private uint TWI_ReadPage_FC(byte DeviceAddress, byte ReadDataByteAddress, byte[] ReadDataBuffer, uint num)
        {
            int opened = 0;
            int status = 0;
            byte status_i2c_s0 = 0;
            byte status_i2c_s1 = 0;
            ushort retry = 0;
            ushort readnum = 0;
            byte numBytesRead = 0;
            byte[] readbuf = new byte[61];
            byte[] targetaddress = new byte[1];
            targetaddress[0] = ReadDataByteAddress;

            if (num < 1)
                return 0;

            if (CP2112.HidSmbus_IsOpened(device, ref opened) == CP2112.HID_SMBUS_SUCCESS && opened == 1)
            {
                do
                {
                    status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                }
                while (status_i2c_s0 != CP2112.HID_SMBUS_S0_IDLE);
                status = CP2112.HidSmbus_AddressReadRequest(device, DeviceAddress, (ushort)num, 1, targetaddress);
                do
                {
                    status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                }
                while (status_i2c_s0 != CP2112.HID_SMBUS_S0_COMPLETE && status_i2c_s0 != CP2112.HID_SMBUS_S0_IDLE);

                if (status == CP2112.HID_SMBUS_SUCCESS && readnum > 0)
                {
                    status = CP2112.HidSmbus_ForceReadResponse(device, readnum);
                    status = CP2112.HidSmbus_GetReadResponse(device, ref status_i2c_s0, readbuf, 61, ref numBytesRead);
                    if (status == CP2112.HID_SMBUS_SUCCESS && numBytesRead == readnum)
                    {
                        for (int i = 0; i < readnum; i++)
                        {
                            ReadDataBuffer[i] = readbuf[i];
                        }
                        return readnum;
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        public uint TWI_ReadPage(byte DeviceAddress, byte ReadDataByteAddress, byte[] ReadDataBuffer, uint num)
        {
            lock (ccc)
            {
                const ushort readpage = 48;
                ushort readtime = (ushort)(num / readpage);
                uint readnum = 0;

                if (num < 1)
                    return 0;
                else
                {
                    if (readtime == 0)
                    {
                        return TWI_ReadPage_FC(DeviceAddress, ReadDataByteAddress, ReadDataBuffer, num);
                    }
                    else
                    {
                        byte[] ExReadData = new byte[readpage];
                        for (int i = readtime; i > 0; i--)
                        {
                            if (TWI_ReadPage_FC(DeviceAddress, ReadDataByteAddress, ExReadData, readpage) == 0)
                            {
                                return 0;
                            }
                            else
                            {
                                ExReadData.CopyTo(ReadDataBuffer, readnum);
                                readnum += readpage;
                                ReadDataByteAddress += (byte)readpage;
                                num -= readpage;
                            }
                        }
                        //// 2017.1.8
                        if (num < 1) return readnum;
                        ////
                        if (TWI_ReadPage_FC(DeviceAddress, ReadDataByteAddress, ExReadData, num) == 0)
                        {
                            return 0;
                        }
                        else
                        {
                            for (int i = 0; i < num; i++)
                            {
                                ReadDataBuffer[readtime * readpage + i] = ExReadData[i];
                            }
                            readnum += num;
                            return readnum;
                        }
                    }
                }
            }
        }

        public bool TWI_WriteByte(byte DeviceAddress, byte WriteDataByteAddress, byte WriteData)
        {
            lock (ccc)
            {
                int opened = 0;
                int status = 0;
                byte status_i2c_s0 = 0;
                byte status_i2c_s1 = 0;
                ushort retry = 0;
                ushort readnum = 0;
                ushort delay = 10;
                byte[] writebuf = new byte[2];
                writebuf[0] = WriteDataByteAddress;
                writebuf[1] = WriteData;

                if (CP2112.HidSmbus_IsOpened(device, ref opened) == CP2112.HID_SMBUS_SUCCESS && opened == 1)
                {
                    do
                    {
                        status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                    }
                    while (status_i2c_s0 != CP2112.HID_SMBUS_S0_IDLE);
                    status = CP2112.HidSmbus_WriteRequest(device, DeviceAddress, writebuf, 2);
                    System.Threading.Thread.Sleep(delay);
                    status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                    if (status == CP2112.HID_SMBUS_SUCCESS && status_i2c_s0 == CP2112.HID_SMBUS_S0_COMPLETE)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        private uint TWI_WritePage_FC(byte DeviceAddress, byte WriteDataByteAddress, byte[] WriteDataBuffer, byte num)
        {
            int opened = 0;
            int status = 0;
            byte status_i2c_s0 = 0;
            byte status_i2c_s1 = 0;
            ushort retry = 0;
            ushort readnum = 0;
            ushort delay = 10;
            byte[] writebuf = new byte[61];
            writebuf[0] = WriteDataByteAddress;
            for (int i = 0; i < num; i++)
            {
                writebuf[1 + i] = WriteDataBuffer[i];
            }

            if (num < 1)
                return 0;

            if (CP2112.HidSmbus_IsOpened(device, ref opened) == CP2112.HID_SMBUS_SUCCESS && opened == 1)
            {
                do
                {
                    status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                }
                while (status_i2c_s0 != CP2112.HID_SMBUS_S0_IDLE);
                num += 1;
                status = CP2112.HidSmbus_WriteRequest(device, DeviceAddress, writebuf, (byte)num);
                num -= 1;
                System.Threading.Thread.Sleep(delay);
                status = GetTransferStatus(device, ref status_i2c_s0, ref status_i2c_s1, ref retry, ref  readnum);
                if (status == CP2112.HID_SMBUS_SUCCESS && status_i2c_s0 == CP2112.HID_SMBUS_S0_COMPLETE)
                    return num;
                else
                    return 0;
            }
            else
                return 0;
        }

        public uint TWI_WritePage(byte DeviceAddress, byte WriteDataByteAddress, byte[] WriteDataBuffer, uint num)
        {
            lock (ccc)
            {
                int opened = 0;
                ushort page_size = 8;

                if (num < 1)
                    return 0;

                if (CP2112.HidSmbus_IsOpened(device, ref opened) == CP2112.HID_SMBUS_SUCCESS && opened == 1)
                {
                    uint totalnum = num;
                    uint a = (uint)WriteDataByteAddress / page_size;
                    if (a != 0)
                    {
                        a = page_size * (a + 1) - WriteDataByteAddress;
                    }
                    else
                    {
                        a = (uint)(page_size - WriteDataByteAddress);
                    }
                    if (a >= num)
                    {
                        return TWI_WritePage_FC(DeviceAddress, WriteDataByteAddress, WriteDataBuffer, (byte)num);
                    }
                    else
                    {
                        if (TWI_WritePage_FC(DeviceAddress, WriteDataByteAddress, WriteDataBuffer, (byte)a) == 0)
                            return 0;
                        num -= a;
                        WriteDataByteAddress += (byte)a;
                        while (num >= page_size)
                        {
                            byte[] ExWriteData = new byte[num];
                            for (int i = 0; i < num; i++)
                            {
                                ExWriteData[i] = WriteDataBuffer[i + totalnum - num];
                            }
                            if (TWI_WritePage_FC(DeviceAddress, WriteDataByteAddress, ExWriteData, (byte)page_size) == 0)
                                return 0;
                            num -= page_size;
                            WriteDataByteAddress += (byte)page_size;
                        }
                        if (num != 0)
                        {
                            byte[] ExWriteData = new byte[num];
                            for (int i = 0; i < num; i++)
                            {
                                ExWriteData[i] = WriteDataBuffer[i + totalnum - num];
                            }
                            if (TWI_WritePage_FC(DeviceAddress, WriteDataByteAddress, ExWriteData, (byte)num) == 0)
                                return 0;
                        }
                    }
                    return totalnum;
                }
                else
                    return 0;
            }
        }

        public bool TWI_Open()
        {
            int status = 0;
            int opened = 0;
            uint numDevices = 0;

            status = CP2112.HidSmbus_GetNumDevices(ref numDevices, CP2112.HID_SMBUS_VID_CP2112, HID_SMBUS_PID_CP2112);
            if (status == CP2112.HID_SMBUS_SUCCESS)
            {
                status = CP2112.HidSmbus_Open(ref device, 0, CP2112.HID_SMBUS_VID_CP2112, CP2112.HID_SMBUS_PID_CP2112);
                if (status == CP2112.HID_SMBUS_SUCCESS)
                {
                    System.Threading.Thread.Sleep(10);
                    if (CP2112.HidSmbus_IsOpened(device, ref opened) == CP2112.HID_SMBUS_SUCCESS)
                    {
                        if (opened == 1)
                        {
                            status = CP2112.HidSmbus_SetSmbusConfig(device, 100000, 0x02, 0, 10, 10, 10, 1);
                            if (status == CP2112.HID_SMBUS_SUCCESS)
                            {
                                status = CP2112.HidSmbus_SetGpioConfig(device, 0xFF, 0x00, 0x00, 0x00);

                                if (status == CP2112.HID_SMBUS_SUCCESS)
                                    return true;
                                else
                                    return false;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public bool TWI_Close()
        {
            int status = 0;

            if (device != IntPtr.Zero)
            {
                status = CP2112.HidSmbus_Close(device);
                if (status == CP2112.HID_SMBUS_SUCCESS)
                {
                    device = IntPtr.Zero;
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        string[] usb_devices;
        public string[] GetPortString()
        {
            int status;
            uint numDevices = 0;
            string[] devices = new string[0];
            StringBuilder deviceString = new StringBuilder(1024);

            status = CP2112.HidSmbus_GetNumDevices(ref numDevices, CP2112.HID_SMBUS_VID_CP2112, HID_SMBUS_PID_CP2112);
            if (status == CP2112.HID_SMBUS_SUCCESS)
            {
                devices = new string[numDevices];
                //     ÿ       VID/PID    HID  豸
                for (uint i = 0; i < numDevices; i++)
                {
                    status = HidSmbus_GetString(i, CP2112.HID_SMBUS_VID_CP2112, HID_SMBUS_PID_CP2112, deviceString, HID_SMBUS_GET_SERIAL_STR);
                    if (status == CP2112.HID_SMBUS_SUCCESS)
                    {
                        devices[i] = deviceString.ToString();
                    }
                    else
                    {
                        devices[i] = "Port Error";
                    }
                }
            }
            usb_devices = devices;
            return devices;
        }

        public bool SetPortOpen(string devicePort)
        {
            int status = 0;
            int opened = 0;
            uint numDevices = 0;
            StringBuilder deviceString = new StringBuilder(1024);

            status = CP2112.HidSmbus_GetNumDevices(ref numDevices, CP2112.HID_SMBUS_VID_CP2112, HID_SMBUS_PID_CP2112);
            if (status == CP2112.HID_SMBUS_SUCCESS)
            {
                int index = Array.FindIndex(usb_devices, u => u == devicePort);
                if (index == -1) return false;
                status = CP2112.HidSmbus_Open(ref device, (uint)index, CP2112.HID_SMBUS_VID_CP2112, CP2112.HID_SMBUS_PID_CP2112);
                if (status == CP2112.HID_SMBUS_SUCCESS)
                {
                    System.Threading.Thread.Sleep(10);
                    if (CP2112.HidSmbus_IsOpened(device, ref opened) == CP2112.HID_SMBUS_SUCCESS)
                    {
                        if (opened == 1)
                        {
                            status = CP2112.HidSmbus_SetSmbusConfig(device, 100000, 0x02, 0, 10, 10, 10, 1);
                            if (status == CP2112.HID_SMBUS_SUCCESS)
                            {
                                status = CP2112.HidSmbus_SetGpioConfig(device, 0xFF, 0x00, 0x00, 0x00);

                                if (status == CP2112.HID_SMBUS_SUCCESS)
                                    return true;
                                else
                                    return false;
                            }
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;

        }
    }
}
