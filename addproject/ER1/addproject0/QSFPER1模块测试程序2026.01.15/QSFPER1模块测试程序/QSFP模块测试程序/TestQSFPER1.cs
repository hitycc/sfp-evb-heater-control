using System;
using System.Threading;
using Fibertower_Common;

namespace XFP模块测试程序
{
    // These small interfaces keep the module test class independent from the
    // concrete QSFP instrument drivers. Existing QSFP drivers can be adapted
    // to these interfaces without changing the calibration flow.
    public interface IOpticalPowerMeter
    {
        double ReadPowerDbm();
    }

    public interface IOpticalAttenuator
    {
        bool SetAttenuation(double dbm);
    }

    public interface IBerAnalyzer
    {
        double ReadBer();
    }

    public interface IEyeAnalyzer
    {
        bool ReadEyeData(out double tdecq, out double outerEr);
    }

    public delegate void TestLogHandler(string message);

    public sealed class TestQSFPER1
    {
        private const byte I2cAddress = 0xA0;
        private const byte PageSelectAddress = 0x7F;
        private const byte DebugPage = 0x06;
        private const byte CalibrationPage = 0x07;
        private const byte TxDisableAddress = 86;
        private const int ChannelCount = 4;

        private I2C i2c;
        private IOpticalPowerMeter powerMeter;
        private IOpticalAttenuator attenuator;
        private IBerAnalyzer berAnalyzer;
        private IEyeAnalyzer eyeAnalyzer;

        public TestQSFPER1()
        {
            StabilizeMilliseconds = 500;
            FlashWaitMilliseconds = 700;
            LastError = string.Empty;
        }

        public TestQSFPER1(I2C i2c)
            : this()
        {
            Init(i2c);
        }

        public string LastError { get; private set; }
        public int StabilizeMilliseconds { get; set; }
        public int FlashWaitMilliseconds { get; set; }
        public TestLogHandler Log { get; set; }

        public void Init(I2C i2c)
        {
            this.i2c = i2c;
        }

        public void Init(I2C i2c, IOpticalPowerMeter powerMeter,
            IOpticalAttenuator attenuator, IBerAnalyzer berAnalyzer,
            IEyeAnalyzer eyeAnalyzer)
        {
            Init(i2c);
            this.powerMeter = powerMeter;
            this.attenuator = attenuator;
            this.berAnalyzer = berAnalyzer;
            this.eyeAnalyzer = eyeAnalyzer;
        }

        public void SetInstruments(IOpticalPowerMeter powerMeter,
            IOpticalAttenuator attenuator, IBerAnalyzer berAnalyzer,
            IEyeAnalyzer eyeAnalyzer)
        {
            this.powerMeter = powerMeter;
            this.attenuator = attenuator;
            this.berAnalyzer = berAnalyzer;
            this.eyeAnalyzer = eyeAnalyzer;
        }

        public bool WriteVendorPassword()
        {
            if (i2c == null)
                return Fail("I2C is not initialized.");

            byte[] password = new byte[] { 0xA9, 0x46, 0x50, 0x54 };
            if (i2c.TWI_WritePage(I2cAddress, 123, password, 4) != 4)
                return Fail("Writing vendor password failed.");
            return true;
        }

        public bool SelectTable(byte table)
        {
            if (i2c == null)
                return Fail("I2C is not initialized.");
            if (!i2c.TWI_WriteByte(I2cAddress, PageSelectAddress, table))
                return Fail("Selecting table 0x" + table.ToString("X2") + " failed.");
            return true;
        }

        public bool PrepareDebugMode()
        {
            if (!WriteVendorPassword())
                return false;
            return SelectTable(DebugPage);
        }

        public bool ReadModule(byte channel, out ModuleParameters parameters)
        {
            parameters = new ModuleParameters();
            if (!CheckChannel(channel) || !PrepareDebugMode())
                return false;

            byte rxVagc;
            byte apc;
            if (!ReadDebugByte((byte)(0x9B + channel), out rxVagc))
                return false;
            if (!ReadDebugByte((byte)(0xA0 + channel), out apc))
                return false;

            parameters.Channel = channel;
            parameters.RxVagc = rxVagc;
            parameters.Apc = apc;
            return true;
        }

        public bool SetRxVagc(byte channel, byte value)
        {
            if (!CheckChannel(channel) || !SelectTable(DebugPage))
                return false;
            if (!i2c.TWI_WriteByte(I2cAddress, (byte)(0x9B + channel), value))
                return Fail("Writing RxVagc failed.");
            return true;
        }

        public bool SetApc(byte channel, byte value)
        {
            if (!CheckChannel(channel) || !SelectTable(DebugPage))
                return false;
            if (!i2c.TWI_WriteByte(I2cAddress, (byte)(0xA0 + channel), value))
                return Fail("Writing APC failed.");
            return true;
        }

        public bool SetTxEnabled(byte channel, bool enabled)
        {
            if (!CheckChannel(channel) || i2c == null)
                return false;

            byte txDisable = i2c.TWI_ReadByte(I2cAddress, TxDisableAddress);
            if (enabled)
                txDisable = Bit.ClearBit(txDisable, channel);
            else
                txDisable = Bit.SetBit(txDisable, channel);

            if (!i2c.TWI_WriteByte(I2cAddress, TxDisableAddress, txDisable))
                return Fail("Writing TX enable state failed.");
            return true;
        }

        public bool EnableOnlyTx(byte channel)
        {
            if (!CheckChannel(channel) || i2c == null)
                return false;
            if (!i2c.TWI_WriteByte(I2cAddress, TxDisableAddress, 0x0F))
                return Fail("Disabling all TX channels failed.");
            return SetTxEnabled(channel, true);
        }

        public bool SaveCalToFlash()
        {
            if (!SelectTable(DebugPage))
                return false;
            if (!i2c.TWI_WriteByte(I2cAddress, 0x83, 0x02))
                return Fail("Writing calibration save command failed.");
            Thread.Sleep(FlashWaitMilliseconds);
            return true;
        }

        public bool ReadRxAdc(byte channel, out ushort adc)
        {
            adc = 0;
            if (!CheckChannel(channel) || !SelectTable(DebugPage))
                return false;

            byte[] buffer = new byte[2];
            byte address = (byte)(0xE8 + channel * 2);
            if (i2c.TWI_ReadPage(I2cAddress, address, buffer, 2) != 2)
                return Fail("Reading RX ADC failed.");
            adc = (ushort)((buffer[0] << 8) | buffer[1]);
            return true;
        }

        public bool ReadTxAdc(byte channel, out ushort adc)
        {
            adc = 0;
            if (!CheckChannel(channel) || !SelectTable(DebugPage))
                return false;

            byte[] buffer = new byte[2];
            byte address = (byte)(0xE0 + channel * 2);
            if (i2c.TWI_ReadPage(I2cAddress, address, buffer, 2) != 2)
                return Fail("Reading TX ADC failed.");
            adc = (ushort)((buffer[0] << 8) | buffer[1]);
            return true;
        }

        public bool WriteNoLightCalibration(byte channel, byte adc)
        {
            if (!CheckChannel(channel) || !PrepareDebugMode())
                return false;

            byte address = (byte)(0xC0 + channel);
            if (i2c.TWI_WriteByte(I2cAddress, address, adc) == false)
                return Fail("Writing no-light calibration failed.");
            if (!SaveCalToFlash())
                return false;

            byte[] readback = new byte[1];
            if (!SelectTable(DebugPage) || i2c.TWI_ReadPage(I2cAddress, address, readback, 1) != 1)
                return Fail("Reading no-light calibration failed.");
            if (readback[0] != adc)
                return Fail("No-light calibration readback mismatch.");
            return true;
        }

        public bool WriteTxCalibration(byte channel, double powerDbm, ushort adc)
        {
            if (!CheckChannel(channel) || adc == 0 || !PrepareDebugMode() || !SelectTable(CalibrationPage))
                return false;

            double coefficient = Math.Pow(10.0, powerDbm / 10.0) * 10000.0 / adc;
            byte[] data = BitConverter.GetBytes((float)coefficient);
            byte address = (byte)(0x80 + channel * 4);

            if (i2c.TWI_WritePage(I2cAddress, address, data, 4) != 4)
                return Fail("Writing TX calibration failed.");
            if (!SaveCalToFlash())
                return false;

            byte[] readback = new byte[4];
            if (!SelectTable(CalibrationPage) || i2c.TWI_ReadPage(I2cAddress, address, readback, 4) != 4)
                return Fail("Reading TX calibration failed.");
            if (!ByteEquals(data, readback))
                return Fail("TX calibration readback mismatch.");
            return true;
        }

        public bool WriteRxCalibration(byte channel, RxCalibrationPoint[] points)
        {
            if (!CheckChannel(channel) || points == null || (points.Length != 2 && points.Length != 5))
                return Fail("RX calibration requires exactly 2 or 5 points.");
            if (!PrepareDebugMode())
                return false;

            double[] x = new double[points.Length];
            double[] y = new double[points.Length];
            double[] a = new double[3];
            double[] error = new double[points.Length];
            int i;

            for (i = 0; i < points.Length; i++)
            {
                if (points[i].Adc == 0)
                    return Fail("RX calibration ADC cannot be zero.");
                x[i] = points[i].Adc;
                y[i] = Math.Pow(10.0, points[i].PowerDbm) * 10000.0;
            }

            Bit.iapcir(x, y, (short)points.Length, a, (short)(points.Length == 2 ? 2 : 3), error);

            byte[] data = new byte[12];
            BitConverter.GetBytes((float)a[0]).CopyTo(data, 0);
            BitConverter.GetBytes((float)a[1]).CopyTo(data, 4);
            BitConverter.GetBytes((float)(points.Length == 5 ? a[2] : 0.0)).CopyTo(data, 8);

            if (!SelectTable(CalibrationPage))
                return false;
            byte address = (byte)(0x90 + channel * 16);
            if (i2c.TWI_WritePage(I2cAddress, address, data, 12) != 12)
                return Fail("Writing RX calibration failed.");
            if (!SaveCalToFlash())
                return false;

            byte[] readback = new byte[12];
            if (!SelectTable(CalibrationPage) || i2c.TWI_ReadPage(I2cAddress, address, readback, 12) != 12)
                return Fail("Reading RX calibration failed.");
            if (!ByteEquals(data, readback))
                return Fail("RX calibration readback mismatch.");
            return true;
        }

        public bool AutoRxAdjust(byte channel)
        {
            return AutoRxAdjust(channel, 170, 0.0001, 8, 4, 40);
        }

        public bool AutoRxAdjust(byte channel, byte initialValue, double targetBer,
            byte coarseStep, byte fineRadius, int maxAttempts)
        {
            if (!CheckChannel(channel) || berAnalyzer == null)
                return Fail("RX auto adjustment requires a BER analyzer.");
            if (coarseStep == 0 || maxAttempts <= 0)
                return Fail("Invalid RX adjustment parameters.");
            if (!PrepareDebugMode())
                return false;

            int bestValue = initialValue;
            double bestBer = double.MaxValue;
            int attempts = 0;
            int offset;

            for (offset = 0; offset <= 64 && attempts < maxAttempts; offset += coarseStep)
            {
                int low = initialValue - offset;
                int high = initialValue + offset;
                if (offset == 0)
                {
                    if (EvaluateRx(channel, initialValue, targetBer, ref bestValue, ref bestBer, ref attempts))
                        return true;
                }
                else
                {
                    if (low >= 0 && attempts < maxAttempts && EvaluateRx(channel, (byte)low, targetBer, ref bestValue, ref bestBer, ref attempts))
                        return true;
                    if (high <= 255 && attempts < maxAttempts && EvaluateRx(channel, (byte)high, targetBer, ref bestValue, ref bestBer, ref attempts))
                        return true;
                }
            }

            for (offset = -fineRadius; offset <= fineRadius && attempts < maxAttempts; offset++)
            {
                int candidate = bestValue + offset;
                if (candidate >= 0 && candidate <= 255 &&
                    EvaluateRx(channel, (byte)candidate, targetBer, ref bestValue, ref bestBer, ref attempts))
                    return true;
            }

            return Fail("RX adjustment failed. Best BER=" + bestBer.ToString("E3") +
                ", RxVagc=" + bestValue.ToString());
        }

        public bool AutoTxAdjust(byte channel)
        {
            return AutoTxAdjust(channel, 160, 3.5, 4.0, 8, 40);
        }

        public bool AutoTxAdjust(byte channel, byte initialValue, double maxTdecq,
            double minOuterEr, byte step, int maxAttempts)
        {
            if (!CheckChannel(channel) || eyeAnalyzer == null)
                return Fail("TX auto adjustment requires an eye analyzer.");
            if (step == 0 || maxAttempts <= 0)
                return Fail("Invalid TX adjustment parameters.");
            if (!PrepareDebugMode())
                return false;
            if (!EnableOnlyTx(channel))
                return false;

            int bestValue = initialValue;
            double bestScore = double.MaxValue;
            int attempts = 0;
            int offset;

            for (offset = 0; offset <= 64 && attempts < maxAttempts; offset += step)
            {
                int low = initialValue - offset;
                int high = initialValue + offset;
                if (offset == 0)
                {
                    if (EvaluateTx(channel, initialValue, maxTdecq, minOuterEr,
                        ref bestValue, ref bestScore, ref attempts))
                        return true;
                }
                else
                {
                    if (low >= 0 && attempts < maxAttempts && EvaluateTx(channel, (byte)low, maxTdecq, minOuterEr,
                        ref bestValue, ref bestScore, ref attempts))
                        return true;
                    if (high <= 255 && attempts < maxAttempts && EvaluateTx(channel, (byte)high, maxTdecq, minOuterEr,
                        ref bestValue, ref bestScore, ref attempts))
                        return true;
                }
            }

            return Fail("TX adjustment failed. APC=" + bestValue.ToString() +
                ", best score=" + bestScore.ToString("F3"));
        }

        public bool RunRxPowerCalibration(byte channel)
        {
            if (!CheckChannel(channel) || attenuator == null || powerMeter == null)
                return Fail("RX power calibration requires an attenuator and power meter.");
            if (!PrepareDebugMode())
                return false;

            RxCalibrationPoint[] points = new RxCalibrationPoint[2];
            double[] target = new double[] { -6.0, -8.0 };
            int i;
            for (i = 0; i < target.Length; i++)
            {
                if (!attenuator.SetAttenuation(target[i]))
                    return Fail("Setting attenuator to " + target[i].ToString("F1") + " dBm failed.");
                Thread.Sleep(StabilizeMilliseconds);

                ushort adc;
                if (!ReadRxAdc(channel, out adc))
                    return false;
                double actualPower;
                try
                {
                    actualPower = powerMeter.ReadPowerDbm();
                }
                catch (Exception ex)
                {
                    return Fail("Reading optical power failed: " + ex.Message);
                }
                points[i] = new RxCalibrationPoint(actualPower, adc);
                WriteLog("RX point " + i.ToString() + ": " + actualPower.ToString("F2") + " dBm, ADC=" + adc.ToString());
            }
            return WriteRxCalibration(channel, points);
        }

        public bool RunTxPowerCalibration(byte channel)
        {
            if (!CheckChannel(channel) || powerMeter == null)
                return Fail("TX power calibration requires a power meter.");
            if (!PrepareDebugMode())
                return false;

            ushort adc;
            if (!ReadTxAdc(channel, out adc))
                return false;
            double power;
            try
            {
                power = powerMeter.ReadPowerDbm();
            }
            catch (Exception ex)
            {
                return Fail("Reading TX optical power failed: " + ex.Message);
            }
            return WriteTxCalibration(channel, power, adc);
        }

        private bool EvaluateRx(byte channel, byte value, double targetBer,
            ref int bestValue, ref double bestBer, ref int attempts)
        {
            if (!SetRxVagc(channel, value))
                return false;
            Thread.Sleep(StabilizeMilliseconds);

            double ber;
            try
            {
                ber = berAnalyzer.ReadBer();
            }
            catch (Exception ex)
            {
                Fail("Reading BER failed: " + ex.Message);
                return false;
            }
            attempts++;
            if (double.IsNaN(ber) || double.IsInfinity(ber))
                return false;
            WriteLog("RxVagc=" + value.ToString() + ", BER=" + ber.ToString("E3"));
            if (ber < bestBer)
            {
                bestBer = ber;
                bestValue = value;
            }
            return ber < targetBer;
        }

        private bool EvaluateTx(byte channel, byte value, double maxTdecq, double minOuterEr,
            ref int bestValue, ref double bestScore, ref int attempts)
        {
            if (!SetApc(channel, value))
                return false;
            Thread.Sleep(StabilizeMilliseconds);

            double tdecq;
            double outerEr;
            try
            {
                if (!eyeAnalyzer.ReadEyeData(out tdecq, out outerEr))
                    return false;
            }
            catch (Exception ex)
            {
                Fail("Reading eye data failed: " + ex.Message);
                return false;
            }
            attempts++;
            if (double.IsNaN(tdecq) || double.IsNaN(outerEr) ||
                double.IsInfinity(tdecq) || double.IsInfinity(outerEr))
                return false;

            double score = Math.Max(0.0, tdecq - maxTdecq) + Math.Max(0.0, minOuterEr - outerEr);
            if (score < bestScore)
            {
                bestScore = score;
                bestValue = value;
            }
            WriteLog("APC=" + value.ToString() + ", TDECQ=" + tdecq.ToString("F3") +
                ", Outer ER=" + outerEr.ToString("F3"));
            return tdecq < maxTdecq && outerEr > minOuterEr;
        }

        private bool ReadDebugByte(byte address, out byte value)
        {
            value = 0;
            byte[] buffer = new byte[1];
            if (i2c.TWI_ReadPage(I2cAddress, address, buffer, 1) != 1)
                return Fail("Reading register 0x" + address.ToString("X2") + " failed.");
            value = buffer[0];
            return true;
        }

        private bool CheckChannel(byte channel)
        {
            if (channel >= ChannelCount)
                return Fail("Channel must be between 0 and 3.");
            if (i2c == null)
                return Fail("I2C is not initialized.");
            return true;
        }

        private bool Fail(string message)
        {
            LastError = message;
            WriteLog("ERROR: " + message);
            return false;
        }

        private void WriteLog(string message)
        {
            if (Log != null)
                Log(message);
        }

        private static bool ByteEquals(byte[] first, byte[] second)
        {
            if (first == null || second == null || first.Length != second.Length)
                return false;
            int i;
            for (i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                    return false;
            }
            return true;
        }
    }

    public sealed class ModuleParameters
    {
        public byte Channel;
        public byte RxVagc;
        public byte Apc;
    }

    public struct RxCalibrationPoint
    {
        public double PowerDbm;
        public ushort Adc;

        public RxCalibrationPoint(double powerDbm, ushort adc)
        {
            PowerDbm = powerDbm;
            Adc = adc;
        }
    }
}
